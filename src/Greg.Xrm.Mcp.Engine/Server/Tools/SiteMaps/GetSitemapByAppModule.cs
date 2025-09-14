using Greg.Xrm.Mcp.Core.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Xml.Linq;

namespace Greg.Xrm.Mcp.Server.Tools.SiteMaps
{
	[McpServerToolType]
	public class GetSitemapByAppModule(
		ILogger<GetSitemapByAppModule> logger,
		IDataverseClientProvider clientProvider
	)
	{
		[McpServerTool(
			Name = "dataverse_sitemap_getbyappmodule",
			Destructive = false,
			Idempotent = true,
			ReadOnly = true),
		Description(@"Given the unique name of a given appmodule, 
returns the XML definition of the sitemap associated with the app.
The XML definition is compliant to the sitemap schema available in the resource schema://sitemapxml.")]
		public async Task<string> Execute(
			[Description("MANDATORY: The unique name of the app module to retrieve the sitemap for.")]
			string appModuleName)
		{
			logger.LogTrace("{ToolName} called with parameters: appModuleName={AppModuleName}",
				   nameof(GetSitemapByAppModule),
				   appModuleName);



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

				var sitemapId = record.GetAttributeValue<AliasedValue>("sm.sitemapid")?.Value;
				var sitemapXml = record.GetAttributeValue<AliasedValue>("sm.sitemapxml")?.Value as string;
				if (string.IsNullOrEmpty(sitemapXml))
				{
					return $"❌ Error: No sitemap found for app module '{appModuleName}'.";
				}

				// Pretty print the XML
				var formattedXml = XDocument.Parse(sitemapXml).ToString();
				return $"✅ Sitemap (ID: {sitemapId}) for app module '{appModuleName}':\n{formattedXml}";
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
