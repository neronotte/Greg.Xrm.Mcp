using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Greg.Xrm.Mcp.Server.Tools.Views
{
	[McpServerToolType]
	public class UpdateViewDefinition(
		ILogger<UpdateViewDefinition> logger,
		IDataverseClientProvider clientProvider)
	{
		[McpServerTool(
			Name = "dataverse_view_update_structure", 
			Destructive = true,
			Idempotent = true,
			ReadOnly = false),
		Description(
@"Given the unique identifier (guid) of a specific system view, updates the definition of the columns using a layoutXML string. 
Be sure to validate the LayoutXML against it's schema definition BEFORE running this command.
The specifications for LayoutXML are available at schema://layoutxml.
Be sure to validate the FetchXML against it's schema definition BEFORE running this command.
The specifications for FetchXML are available at schema://fetchxml.
You MUST be sure that the attribute names used in both FetchXML and LayoutXML are actual column names. Don't invent column names. If you're not sure, use the metadata tools to check against the actual table metadata.
If you want to reference columns from related tables, use the syntax entityalias.columnname in the LayoutXML; entityalias MUST match the name placed in the ""alias"" attribute of the link-entity in the fetchxml, while ""columnname"" must be the actual name of the column.
If the FetchXML has an <order> node, the attribute specified in the node MUST be returned by the FetchXML and MUST be present in the LayoutXML; if the caller asks to sort for one or more columns not present in the LayoutXML, add them as last columns with a width of 150px."
)]
		public async Task<string> Execute(
			IMcpServer mcpServer,
			IPublishXmlBuilder publishXmlBuilder,
			[Description("The unique identifier of the form to retrieve")] Guid viewId,
			[Description("The new layoutXML describing the structure of the columns of the view")] string newLayoutXml,

			[Description(
@"Optionally, you can also provide a new version of the fetchxml. 
Columns specified in the view must also be added in the FetchXml as <attribute> nodes. 
If user asks to add columns from a related entity, a <link-entity> node with link-type='outer' and an unique 'alias' must be specified in the fetchxml;
attributes from the related entity must be add with an <attribute> node inside the <link-entity> node.
")] string? newFetchXml)
		{
			logger.LogTrace("{ToolName} called with parameters: ViewId={ViewId}, NewLayoutXml={NewLayoutXml}, NewFetchXml={NewFetchXml}",
				   nameof(UpdateViewDefinition),
				   viewId,
				   newLayoutXml,
				   newFetchXml);

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
				sb.AppendLine("Old LayoutXML:").AppendLine(view.LayoutXml).AppendLine();
				sb.AppendLine("New LayoutXML:").AppendLine(newLayoutXml).AppendLine();
				sb.AppendLine("Old FetchXML:").AppendLine(view.FetchXml).AppendLine();
				sb.AppendLine("New FetchXML:").AppendLine(newFetchXml).AppendLine();


				var clone = new Entity("savedquery", view.Id);
				clone["layoutxml"] = newLayoutXml;
				if (!string.IsNullOrWhiteSpace(newFetchXml))
				{
					clone["fetchxml"] = newFetchXml;
				}
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
