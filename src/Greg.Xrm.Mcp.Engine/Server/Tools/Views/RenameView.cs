using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Greg.Xrm.Mcp.Server.Tools.Views
{
	[McpServerToolType]
	public class RenameView(
		ILogger<RenameView> logger,
		IDataverseClientProvider clientProvider)
	{
		[McpServerTool(
			Name = "dataverse_view_rename", 
			Destructive = true,
			Idempotent = true,
			ReadOnly = false),
		Description(
@"Given the unique identifier (guid) of a specific system view, changes its display name."
)]
		public async Task<string> Execute(
			IMcpServer mcpServer,
			IPublishXmlBuilder publishXmlBuilder,
			[Description("The unique identifier of the view to rename")] Guid viewId,
			[Description("The new layoutXML describing the structure of the columns of the view")] string newName
)
		{
			logger.LogTrace("{ToolName} called with parameters: ViewId={ViewId}, NewName={NewName}",
				   nameof(RenameView),
				   viewId,
				   newName);

			try
			{
				var client = await clientProvider.GetDataverseClientAsync();

				var token = await mcpServer.NotifyProgressAsync(nameof(UpdateViewDefinition), "Retrieving view...");

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
				sb.AppendLine("Old Name:").AppendLine(view.Name).AppendLine();
				sb.AppendLine("New Name:").AppendLine(newName).AppendLine();

				view.Name = newName;

				context.UpdateObject(view);
				context.SaveChanges();

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
				logger.LogError(ex, "❌ Error: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
