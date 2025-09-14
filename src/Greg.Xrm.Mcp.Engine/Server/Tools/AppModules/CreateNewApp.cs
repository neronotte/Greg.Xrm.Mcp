using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

namespace Greg.Xrm.Mcp.Server.Tools.AppModules
{
	/// <summary>
	/// The logic and apis to use are documented here:
	/// https://learn.microsoft.com/en-us/power-apps/developer/model-driven-apps/create-manage-model-driven-apps-using-code
	/// 
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="clientProvider"></param>
	[McpServerToolType]
	public class CreateNewApp(
		ILogger<CreateNewApp> logger,
		IDataverseClientProvider clientProvider)
	{
		[McpServerTool(
			Name = "dataverse_appmodule_create",
			Destructive = false,
			Idempotent = false,
			ReadOnly = false),
		Description(@"Creates a new Dataverse Model Driven App, initializes it's components and sitemap with a list of tables provided as input, and adds the app to a specific Dataverse solution.
You MUST NOT guess or invent which app to add: you MUST explicitly ASK the user for the tables to add.
User CAN indicate the uniquename of the Dataverse solution that will contain the app once created.")]
		public async Task<string> Execute(
			IPublishXmlBuilder publishXmlBuilder,
			IMcpServer server,
			[Description("MANDATORY: The display name of the app (the unique name will be inferred automatically)")] 
			string appName,
			[Description("An optional description for the app. The description will be visualized in the Dataverse App Picker form.")]
			string? appDescription = null,
			[Description("OPTIONAL: Comma (,) separated list of the logical names of the dataverse tables to include in the app. You MUST specify at least one table.")]
			string? tablesToInclude = null,
			[Description("OPTIONAL: The solution to which the app should be added.")]
			string? solutionName = null
		)
		{
			logger.LogTrace("{ToolName} called.",
				   nameof(CreateNewApp));


			if (string.IsNullOrWhiteSpace(appName))
			{
				return "❌ Error: App name is required.";
			}
			var appUniqueName = appName.OnlyLettersNumbersOrUnderscore();
			if (string.IsNullOrWhiteSpace(appUniqueName))
			{
				return "❌ Error: App name is invalid. It must include at least one letter.";
			}
			if (string.IsNullOrWhiteSpace(tablesToInclude))
			{
				var (result, answer) = await server.ElicitTextAsync("You must specify at least one table to include in the app. Please provide a comma (,) separated list of the logical names of the tables to include.");
				if (!result)
				{
					return "❌ Error: You must specify at least one table to include in the app.";
				}

				tablesToInclude = answer;
			}
			var tableNames = tablesToInclude.Split(',').Select(t => t.Trim())
				.Where(t => !string.IsNullOrWhiteSpace(t))
				.ToList();
			if (tableNames.Count == 0)
			{
				return "❌ Error: You must specify at least one table to include in the app.";
			}


			if (string.IsNullOrWhiteSpace(solutionName))
			{
				var (result, answer) = await server.ElicitTextAsync("Please provide the unique name of the solution where the app should be placed.");
				if (!result)
				{
					return "❌ Error: You must specify at least one table to include in the app.";
				}

				solutionName = answer;
			}




			var client = await clientProvider.GetDataverseClientAsync();

			var lcid = await client.GetDefaultLanguageCodeAsync();

			string? publisherPrefix;
			try
			{
				logger.LogDebug("Checking solution existence and retrieving publisher prefix");

				var query = new QueryExpression("solution");
				query.ColumnSet.AddColumns("ismanaged");
				query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solutionName);
				var link = query.AddLink("publisher", "publisherid", "publisherid");
				link.Columns.AddColumns("customizationprefix");
				link.EntityAlias = "publisher";
				query.NoLock = true;
				query.TopCount = 1;


				var solutionList = (await client.RetrieveMultipleAsync(query)).Entities;
				if (solutionList.Count == 0)
				{
					return "❌ Invalid solution name: " + solutionName;
				}

				var managed = solutionList[0].GetAttributeValue<bool>("ismanaged");
				if (managed)
				{
					return "❌ The provided solution is managed. You must specify an unmanaged solution.";
				}

				publisherPrefix = solutionList[0].GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value as string;
				if (string.IsNullOrWhiteSpace(publisherPrefix))
				{
					return "❌ Unable to retrieve the publisher prefix.";
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error in {ToolName} while retrieving solution details: {ErrorMessage}",
				 nameof(CreateNewApp),
				 ex.Message);
				return $"❌ Error: {ex.Message}";
			}


			appUniqueName = $"{publisherPrefix}_{appUniqueName}";


			var entityRefCollection = new EntityReferenceCollection();
			try
			{
				logger.LogDebug("Retrieving table metadata for tables: {TableNames}",
					string.Join(", ", tableNames));

				(bool flowControl, string? value) = await RetrieveTableList(tableNames, client, entityRefCollection);
				if (!flowControl)
				{
					return value!;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error in {ToolName} while retrieving table list: {ErrorMessage}",
				 nameof(CreateNewApp),
				 ex.Message);
				return $"❌ Error: {ex.Message}";
			}



			var app = new AppModule();
			try
			{
				logger.LogDebug("Creating app module: {AppName}",appName);
				app.Name = appName;
				app.UniqueName = appUniqueName;
				app.Description = appDescription ?? $"Created by MCP on {DateTime.UtcNow}";
				app.ClientType = 4; // Unified Interface
				app.FormFactor = 1;
				app.IsFeatured = false;
				app.NavigationType = appmodule_navigationtype.Singlesession;
				app.WebResourceId = Guid.Parse("953b9fac-1e5e-e611-80d6-00155ded156f");

				app.Id = await client.CreateAsync(app);

				var request = new AddSolutionComponentRequest
				{
					AddRequiredComponents = true,
					ComponentId = app.Id,
					ComponentType = (int)ComponentType.AppModule,
					SolutionUniqueName = solutionName
				};
				await client.ExecuteAsync(request);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error in {ToolName} while creating appmodule: {ErrorMessage}",
				 nameof(CreateNewApp),
				 ex.Message);
				return $"❌ Error: {ex.Message}";
			}


			var sitemap = new SiteMap();
			try
			{

				logger.LogDebug("Creating sitemap for: {AppName}", appName);

				const string subareaNode = @"<SubArea Id=""subarea_{entityname}""
Icon=""/_imgs/imagestrips/transparent_spacer.gif""
Entity=""{entityname}""
Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web""
AvailableOffline=""true""
PassParams=""false""
Sku=""All,OnPremise,Live,SPLA""/>";


				var subAreas = tableNames
					.Select(x => subareaNode.Replace("{entityname}", x))
					.Join(Environment.NewLine);

				sitemap.SiteMapName = appUniqueName;
				sitemap.SiteMapNameUnique = appUniqueName;
				sitemap.SiteMapXml = $@"<SiteMap IntroducedVersion=""7.0.0.0"">
	<Area Id=""area_general""
	      ResourceId=""SitemapDesigner.NewTitle""
	      DescriptionResourceId=""SitemapDesigner.NewTitle""
	      ShowGroups=""true""
	      IntroducedVersion=""7.0.0.0"">
		<Titles>
			<Title LCID=""{lcid}""
			       Title=""General""/>
		</Titles>
		<Group Id=""group_general""
		       ResourceId=""SitemapDesigner.NewGroup""
		       DescriptionResourceId=""SitemapDesigner.NewGroup""
		       IntroducedVersion=""7.0.0.0""
		       IsProfile=""false""
		       ToolTipResourseId=""SitemapDesigner.Unknown"">
			<Titles>
				<Title LCID=""{lcid}""
				       Title=""General""/>
			</Titles>
{subAreas}
		</Group>
	</Area>
</SiteMap>";
				sitemap.Id = await client.CreateAsync(sitemap);


				var request = new AddSolutionComponentRequest
				{
					AddRequiredComponents = true,
					ComponentId = sitemap.Id,
					ComponentType = (int)ComponentType.SiteMap,
					SolutionUniqueName = solutionName
				};
				await client.ExecuteAsync(request);

			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error in {ToolName} while creating sitemap: {ErrorMessage}",
				 nameof(CreateNewApp),
				 ex.Message);
				return $"❌ Error: {ex.Message}";
			}


			try
			{
				logger.LogDebug("Adding components to the app: {AppName}", appName);

				entityRefCollection.Add(sitemap.ToEntityReference());

				var addRrequest = new AddAppComponentsRequest
				{
					AppId = app.AppModuleId!.Value,
					Components = entityRefCollection
				};
				await client.ExecuteAsync(addRrequest);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error in {ToolName} while adding components to the app: {ErrorMessage}",
				 nameof(CreateNewApp),
				 ex.Message);
				return $"❌ Error: {ex.Message}";
			}


			try
			{
				logger.LogDebug("Publishing app: {AppName}", appName);

				publishXmlBuilder.AddAppModule(app.AppModuleId!.Value);
				publishXmlBuilder.AddSiteMap(sitemap.Id);
				var publishRequest = publishXmlBuilder.Build();
				if (publishRequest != null)
				{
					await client.ExecuteAsync(publishRequest);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error in {ToolName} while validating the app: {ErrorMessage}",
				 nameof(CreateNewApp),
				 ex.Message);
				return $"❌ Error: {ex.Message}";
			}


			try
			{
				logger.LogDebug("Validating app: {AppName}", appName);

				var validateRequest = new ValidateAppRequest
				{
					AppModuleId = app.AppModuleId!.Value
				};
				var validateResponse = (ValidateAppResponse)await client.ExecuteAsync(validateRequest);
				var responseFormatted = JsonConvert.SerializeObject(validateResponse.AppValidationResponse, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });


				var sb = new StringBuilder();
				sb.Append("App creation completed. Validation result: ");
				if (validateResponse.AppValidationResponse.ValidationSuccess)
				{
					sb.AppendLine("✅ Validation succeeded.");
				}
				else
				{
					sb.AppendLine("❌ Validation failed.");
				}

				sb.AppendLine("Detailed response: ").AppendLine(responseFormatted);

				return sb.ToString();
			}
			catch (Exception ex)
			{
   				logger.LogError(ex, "❌ Error in {ToolName}: {ErrorMessage}",
					nameof(CreateNewApp),
					ex.Message);
				return $"❌ Error: {ex.Message}";
			}

		}

		private static async Task<(bool flowControl, string? value)> RetrieveTableList(List<string> tableNames, Microsoft.PowerPlatform.Dataverse.Client.IOrganizationServiceAsync2 client, EntityReferenceCollection entityRefCollection)
		{
			var request = new RetrieveAllEntitiesRequest
			{
				EntityFilters = EntityFilters.Entity
			};

			var response = (RetrieveAllEntitiesResponse)await client.ExecuteAsync(request);
			var allEntities = response.EntityMetadata;

			foreach (var tableName in tableNames)
			{
				var entity = allEntities.FirstOrDefault(e => e.LogicalName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
				if (entity == null)
				{
					return (flowControl: false, value: $"❌ Error: Table '{tableName}' does not exist.");
				}

				entityRefCollection.Add(new EntityReference("entity", entity.MetadataId!.Value));
			}

			return (flowControl: true, value: null);
		}
	}
}
