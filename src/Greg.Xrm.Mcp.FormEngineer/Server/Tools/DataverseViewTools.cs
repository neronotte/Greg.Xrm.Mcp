using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Crm.Sdk;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

namespace Greg.Xrm.Mcp.FormEngineer.Server.Tools
{
	[McpServerToolType]
	public class DataverseViewTools
	{
		protected DataverseViewTools()
		{
			// This constructor is intentionally empty.
			// It is used to ensure the class can be instantiated without any parameters.
		}

		[McpServerTool(
			Name = "dataverse_view_get_list_by_table_schema_name",
			Idempotent = true,
			Destructive = false,
			ReadOnly = true
		),
		Description(
"Retrieves the list of forms for a given Dataverse table, and returns the details such as Id, Name, Type, Description, ad if it's default or not")]
		public static async Task<string> GetViewListByTableSchemaName(
			ILogger<DataverseViewTools> logger,
			IDataverseClientProvider clientProvider,
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

		[McpServerTool(
			Name ="dataverse_view_get_definition",
			Idempotent = true,
			Destructive = false,
			ReadOnly = true),
		Description(
@"Given the unique identifier (guid) of a specific system view, returns the XML definition of the view column layout (Layout XML) and the XML definition of the filters applied to the view (Filter XML). 
The specifications for LayoutXML are available at schema://layoutxml. 
The specifications for FilterXML are availabe at schema://filterxml.")]
		public static async Task<string> GetViewDefinitionAsync(
			ILogger<DataverseViewTools> logger,
			IDataverseClientProvider clientProvider,
			[Description("The unique identifier of the form to retrieve")] Guid viewId,
			[Description("How do you want the output to be returned (Formatted or Json)")] string outputType) 
		{

			logger.LogTrace("{ToolName} called with parameters: ViewId={ViewId}, OutputType={OutputType}",
				   nameof(GetViewDefinitionAsync),
				   viewId,
				   outputType);

			try
			{
				var client = await clientProvider.GetDataverseClientAsync();

				logger.LogTrace("🔍 Retrieving view with id: {ViewId}", viewId);

				var context = new DataverseContext(client);

				var view = context.SavedQuerySet.Where(x => x.Id == viewId)
					.Select(x => new
					{
						x.Id,
						x.Name,
						x.LayoutXml,
						x.FetchXml,
					}).FirstOrDefault();


				if (view == null)
				{
					logger.LogTrace("View with ID '{ViewId}' not found.", viewId);
					return $"❌ View with ID '{viewId}' not found.";
				}

				if ("json".Equals(outputType, StringComparison.OrdinalIgnoreCase))
				{
					var result = JsonConvert.SerializeObject(view, Formatting.Indented);
					logger.LogTrace("{Result}", result);
					return result;
				}
				else
				{
					var sb = new StringBuilder();
					sb.Append("View Details:").AppendLine();
					sb.Append("- ID: ").Append(viewId).AppendLine();
					sb.Append("- Name: ").Append(view.Name).AppendLine();	
					sb.Append("- Layout XML: ").AppendLine().Append(view.LayoutXml).AppendLine().AppendLine();

					if (!string.IsNullOrWhiteSpace(view.FetchXml))
					{
	 					sb.Append("- Filter XML: ").AppendLine().Append(view.FetchXml).AppendLine();
					}

					logger.LogTrace("{Result}", sb.ToString());

					return sb.ToString();
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while retrieving forms: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}


		[McpServerTool(
			Name = "dataverse_view_update_layout", 
			Destructive = true,
			Idempotent = true,
			ReadOnly = false),
		Description(
@"Given the unique identifier (guid) of a specific system view, updates the definition of the columns using a layoutXML string. 
Be sure to validate the LayoutXML against it's schema definition before executing the update.
The specifications for LayoutXML are available at schema://layoutxml.")]
		public static async Task<string> UpdateViewLayoutAsync(
			ILogger<DataverseViewTools> logger,
			IMcpServer mcpServer,
			IPublishXmlBuilder publishXmlBuilder,
			IDataverseClientProvider clientProvider,
			[Description("The unique identifier of the form to retrieve")] Guid viewId,
			[Description("The new layoutXML describing the structure of the columns of the view")] string newLayoutXml)
		{
			logger.LogTrace("{ToolName} called with parameters: ViewId={ViewId}, NewLayoutXml={NewLayoutXml}",
				   nameof(UpdateViewLayoutAsync),
				   viewId,
				   newLayoutXml);

			try
			{
				var client = await clientProvider.GetDataverseClientAsync();

				var token = await mcpServer.NotifyProgressAsync(nameof(UpdateViewLayoutAsync), "Retrieving view...");

				logger.LogTrace("🔍 Retrieving view with id: {ViewId}", viewId);
				var context = new DataverseContext(client);

				var view = context.SavedQuerySet.Where(x => x.Id == viewId)
					.FirstOrDefault();


				if (view == null)
				{
					logger.LogTrace("View with ID '{ViewId}' not found.", viewId);
					return $"❌ View with ID '{viewId}' not found.";
				}

				await mcpServer.NotifyProgressAsync(token, "Updating view layout...", 30);

				var sb = new StringBuilder();
				sb.AppendLine("View: " + viewId);
				sb.AppendLine("Old LayoutXML:").AppendLine(view.LayoutXml).AppendLine();
				sb.AppendLine("New LayoutXML:").AppendLine(newLayoutXml).AppendLine();


				var clone = new Entity("savedquery", view.Id);
				clone["layoutxml"] = newLayoutXml;
				await client.UpdateAsync(clone);

				await mcpServer.NotifyProgressAsync(token, "Publishing...", 70);

				logger.LogTrace("{Result}", sb.ToString());

				logger.LogTrace("Publishing table...");
				var tableName = view.ReturnedTypeCode;
				publishXmlBuilder.AddTable(tableName);
				var request = publishXmlBuilder.Build();

				await client.ExecuteAsync(request);
				logger.LogTrace("Publishing table...COMPLETED");


				await mcpServer.NotifyProgressAsync(token, "Completed!", 100);

				return sb.ToString();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while retrieving forms: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
