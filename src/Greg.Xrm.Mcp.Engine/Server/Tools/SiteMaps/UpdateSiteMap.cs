using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.SiteMaps
{
	[McpServerToolType]
	public class UpdateSiteMap(
		ILogger<UpdateSiteMap> logger,
		IDataverseClientProvider clientProvider
	)
	{
		[McpServerTool(
			Name = "dataverse_sitemap_update",
			Destructive = true,
			Idempotent = false,
			ReadOnly = false),
		Description(@"Given the unique name of a given appmodule, performs an update of the sitemap associated with the app.
Before applying any update, BE SURE to retrieve the latest version of the sitemap via `dataverse_sitemap_getbyappmodule` tool.
If the user asks to add an entity to the sitemap, BE SURE that the entity exists using `dataverse_metadata_list_tables` tool.
If the user asks to add a webresource to the sitemap, BE SURE that the webresource exists and is a valid html webresource using `dataverse_metadata_list_webresources_html` tool.")]
		public async Task<string> Execute(
			IPublishXmlBuilder publishXmlBuilder,

			[Description("MANDATORY: The unique name of the app module referencing the sitemap to update.")] 
			string appModuleName,
			
			[Description("MANDATORY: The new XML definition of the sitemap to apply. The XML definition must be compliant to the sitemap schema available in the resource schema://sitemapxml.")]
			string siteMapXml)
		{
			logger.LogTrace("{ToolName} called with parameters: appModuleName={AppModuleName}",
				   nameof(UpdateSiteMap),
				   appModuleName);


			if (string.IsNullOrWhiteSpace(appModuleName))
			{
				return "❌ Error: appModuleName is mandatory.";
			}

			if(string.IsNullOrWhiteSpace(siteMapXml))
			{
				return "❌ Error: siteMapXml is mandatory.";
			}

			siteMapXml = siteMapXml.RemoveXmlDeclaration();

			try
			{
				var client = await clientProvider.GetDataverseClientAsync();

				var query = new QueryExpression("appmodule");
				query.ColumnSet.AddColumns("appmoduleid", "name", "uniquename");
				query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, appModuleName);
				var amc = query.AddLink("appmodulecomponent", "appmoduleidunique", "appmoduleidunique", JoinOperator.LeftOuter);
				amc.EntityAlias = "amc";
				amc.LinkCriteria.AddCondition("componenttype", ConditionOperator.Equal, 62); // 62 = Sitemap
				var sm = amc.AddLink("sitemap", "objectid", "sitemapid", JoinOperator.LeftOuter);
				sm.EntityAlias = "sm";
				sm.Columns.AddColumns("sitemapid", "sitemapxml", "sitemapname");
				query.TopCount = 1;


				var result = await client.RetrieveMultipleAsync(query);
				var record = result.Entities.FirstOrDefault();
				if (record == null)
				{
					return $"❌ Error: App module with unique name '{appModuleName}' not found.";
				}

				var sitemapId = record.GetAttributeValue<AliasedValue>("sm.sitemapid")?.Value as Guid?;
				if (sitemapId == null || sitemapId == Guid.Empty)
				{
					return $"❌ Error: Sitemap for app module with unique name '{appModuleName}' not found.";
				}


				var sitemap = new Entity("sitemap", sitemapId.Value);
				sitemap["sitemapxml"] = siteMapXml;
				await client.UpdateAsync(sitemap);

				publishXmlBuilder.AddSiteMap(sitemapId.Value);

				var publishRequest = publishXmlBuilder.Build();
				await client.ExecuteAsync(publishRequest);

				return $"✅ Success: Sitemap for app module '{appModuleName}' updated successfully.";
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}

		}
	}
}
