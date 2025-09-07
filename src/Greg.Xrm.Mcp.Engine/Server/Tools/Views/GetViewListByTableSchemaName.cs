using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Crm.Sdk;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.Views
{
	[McpServerToolType]
	public class GetViewListByTableSchemaName(
		ILogger<GetViewListByTableSchemaName> logger,
		IDataverseClientProvider clientProvider)
	{

		[McpServerTool(
			Name = "dataverse_view_get_list_by_table_schema_name",
			Idempotent = true,
			Destructive = false,
			ReadOnly = true
		),
		Description(
"Retrieves the list of forms for a given Dataverse table, and returns the details such as Id, Name, Type, Description, ad if it's default or not")]
		public async Task<string> Execute(
			[Description("The logical name of the Dataverse table")] string entityLogicalName,
			[Description("How do you want the output to be returned (Formatted or Json)")] string outputType)
		{
			logger.LogTrace("{ToolName} called with parameters: EntityLogicalName={EntityLogicalName}, OutputType={OutputType}",
				   nameof(GetViewListByTableSchemaName),
				   entityLogicalName,
				   outputType);

			try
			{

				var client = await clientProvider.GetDataverseClientAsync();

				logger.LogTrace("🔍 Retrieving views for table: {EntityName}", entityLogicalName);

				var context = new DataverseContext(client);

				var views = context.SavedQuerySet
					.Where(x => x.ReturnedTypeCode == entityLogicalName && (
						x.QueryType == SavedQueryQueryType.MainApplicationView ||
						x.QueryType == SavedQueryQueryType.LookupView ||
						x.QueryType == SavedQueryQueryType.QuickFindSearch ||
						x.QueryType == SavedQueryQueryType.AdvancedSearch ||
						x.QueryType == SavedQueryQueryType.SubGrid)
					)
					.OrderBy(x => x.QueryType)
					.ThenBy(x => x.Name)
					.Select(x => new
					{
						x.Id,
						x.Name,
						x.QueryType,
						x.Description,
						x.IsDefault,
					}).ToList();

				if ("json".Equals(outputType, StringComparison.OrdinalIgnoreCase))
				{
					var result = JsonConvert.SerializeObject(views.ToList(), Formatting.Indented);
					logger.LogTrace("{Result}", result);
					return result;

				}
				else
				{
					var result = $"✅ Found {views.Count} views for table '{entityLogicalName}':\n\n" +
						string.Join(Environment.NewLine, views.Select(f =>
							$"📋 Form: {f.Name}{Environment.NewLine}" +
							$"   ID: {f.Id}{Environment.NewLine}" +
							$"   QueryType: {f.QueryType}{Environment.NewLine}" +
							$"   Default: {(f.IsDefault.GetValueOrDefault() ? "✅ Yes" : "❌ No")}{Environment.NewLine}" +
							$"   Description: {f.Description ?? "N/A"}{Environment.NewLine}"));
					logger.LogTrace("{Result}", result);
					return result;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while retrieving forms: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
