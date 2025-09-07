using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

namespace Greg.Xrm.Mcp.Server.Tools.Views
{
	[McpServerToolType]
	public class GetViewDefinition(
		ILogger<GetViewDefinition> logger,
		IDataverseClientProvider clientProvider)
	{
		[McpServerTool(
				   Name = "dataverse_view_get_definition",
				   Idempotent = true,
				   Destructive = false,
				   ReadOnly = true),
			   Description(
	   @"Given the unique identifier (guid) of a specific system view, returns the XML definition of the view column layout (Layout XML) and the XML definition of the filters applied to the view (Filter XML). 
The specifications for LayoutXML are available at schema://layoutxml. 
The specifications for FilterXML are availabe at schema://filterxml.")]
		public async Task<string> Execute(
				   [Description("The unique identifier of the form to retrieve")] Guid viewId,
				   [Description("How do you want the output to be returned (Formatted or Json)")] string outputType)
		{

			logger.LogTrace("{ToolName} called with parameters: ViewId={ViewId}, OutputType={OutputType}",
				   nameof(GetViewDefinition),
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
						sb.Append("- Fetch XML: ").AppendLine().Append(view.FetchXml).AppendLine();
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
	}
}
