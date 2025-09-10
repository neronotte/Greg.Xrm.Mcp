using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.AppModules
{
	/// <summary>
	/// The logic and apis to use are documented here:
	/// https://learn.microsoft.com/en-us/power-apps/developer/model-driven-apps/create-manage-model-driven-apps-using-code
	/// 
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="clientProvider"></param>
	//[McpServerToolType]
	public class CreateNewApp(
		ILogger<CreateNewApp> logger,
		IDataverseClientProvider clientProvider)
	{
		[McpServerTool(
			Name = "dataverse_appmodule_create",
			Destructive = false,
			Idempotent = false,
			ReadOnly = false),
		Description(@"")]
		public async Task<string> Execute(
			IPublishXmlBuilder publishXmlBuilder,
			[Description("MANDATORY: The display name of the app (the unique name will be inferred automatically)")] string appName,
			[Description("MANDATORY: Comma (,) separated list of the schema names of the dataverse tables to include in the app. You MUST specify at least one table.")]string tablesToInclude
		)
		{
			logger.LogTrace("{ToolName} called.",
				   nameof(CreateNewApp));


			if (string.IsNullOrWhiteSpace(appName))
			{
				return "❌ Error: App name is required.";
			}
			if (string.IsNullOrWhiteSpace(tablesToInclude))
			{
				return "❌ Error: You must specify at least one table to include in the app.";
			}
			var tableNames = tablesToInclude.Split(',').Select(t => t.Trim())
				.Where(t => !string.IsNullOrWhiteSpace(t))
				.ToList();
			if (tableNames.Count == 0)
			{
				return "❌ Error: You must specify at least one table to include in the app.";
			}








			try
			{
				var app = new AppModule();
				app.Name = appName;
				app.UniqueName = appName.Replace(" ", "_");
				app.Description = $"Created by MCP on {DateTime.UtcNow}";
				app.ClientType = 4; // Unified Interface
				app.FormFactor = 1;
				app.IsFeatured = false;
				app.NavigationType = appmodule_navigationtype.Singlesession;
				app.WebResourceId = Guid.Parse("953b9fac-1e5e-e611-80d6-00155ded156f");


				var sitemap = new SiteMap();
				sitemap.SiteMapName = "";



				var addRrequest = new AddAppComponentsRequest
				{
					AppId = app.AppModuleId!.Value,
					Components = new Microsoft.Xrm.Sdk.EntityReferenceCollection{
						sitemap.ToEntityReference()
					}

				};


				var validateRequest = new ValidateAppRequest
				{
					AppModuleId = app.AppModuleId!.Value
				};


				publishXmlBuilder.AddAppModule(app.AppModuleId!.Value);


				return null;
			}
			catch(Exception ex)
			{
   				logger.LogError(ex, "❌ Error in {ToolName}: {ErrorMessage}",
					nameof(CreateNewApp),
					ex.Message);
				return $"❌ Error: {ex.Message}";
			}

		}




		// app.Descriptor = @"
		//    ""appInfo"": {
		//        ""AppId"": ""7aeb80ab-248e-f011-b4cc-6045bda046d5"",
		//        ""Title"": ""Test"",
		//        ""UniqueName"": ""ava_Test"",
		//        ""Description"": """",
		//        ""IsDefault"": false,
		//        ""Status"": 0,
		//        ""PublishedOn"": ""9/10/2025"",
		//        ""ClientType"": 4,
		//        ""webResourceId"": ""953b9fac-1e5e-e611-80d6-00155ded156f"",
		//        ""welcomePageId"": ""00000000-0000-0000-0000-000000000000"",
		//        ""Components"": [{
		//                ""Id"": ""70816501-edb9-4740-a16c-6a5efbc05d84"",
		//                ""Type"": 1
		//            }, {
		//                ""Id"": ""561d5caa-248e-f011-b4cc-6045bd99f6bd"",
		//                ""Type"": 62
		//            }
		//        ],
		//        ""AppElements"": [],
		//        ""NavigationType"": 0,
		//        ""IsFeatured"": 0,
		//        ""SolutionId"": ""fd140aae-4df4-11dd-bd17-0019b9312238"",
		//        ""IsManaged"": false,
		//        ""AppComponents"": {
		//            ""Entities"": [{
		//                    ""Id"": ""70816501-edb9-4740-a16c-6a5efbc05d84"",
		//                    ""LogicalName"": ""account""
		//                }
		//            ]
		//        }
		//    },
		//    ""webResourceInfo"": {
		//        ""Name"": ""msdyn_/Images/AppModule_Default_Icon.png"",
		//        ""DisplayName"": ""AppModule_Default_Icon.png"",
		//        ""WebResourceType"": 5,
		//        ""Guid"": ""953b9fac-1e5e-e611-80d6-00155ded156f""
		//    },
		//    ""welcomePageInfo"": {
		//        ""WebResourceType"": 0
		//    },
		//    ""publisherInfo"": {
		//        ""Id"": ""d21aab71-79e7-11dd-8874-00188b01e34f""
		//    },
		//    ""eventHandlers"": []
		//}";

	}
}
