using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Crm.Sdk;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.Views
{
	[McpServerToolType]
	public class CreateNewView(
		ILogger<CreateNewView> logger,
		IDataverseClientProvider clientProvider)
	{
		[McpServerTool(
			Name = "dataverse_view_create",
			Destructive = false,
			Idempotent = false,
			ReadOnly = false),
		Description(
@"Creates a new view (a record on the savedquery table) for a given dataverse table.
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
			[Description("The schema name of the table where the view must be created. Unless explicitly stated by the caller, format the name provided using \"A Capital Letter For Each Word\"")] string tableName,
			[Description("The name of the view to create.")] string viewName,
			[Description("The layoutXML describing the structure of the columns of the view")] string newLayoutXml,

			[Description(
@"You must also provide the fetchxml that specifies the filter criteria of the data to be presented on the view. 
Columns specified in the view must also be added in the FetchXml as <attribute> nodes. 
If user asks to add columns from a related entity, a <link-entity> node with link-type='outer' and an unique 'alias' must be specified in the fetchxml;
attributes from the related entity must be add with an <attribute> node inside the <link-entity> node.
")] string? newFetchXml)
		{
			logger.LogTrace("{ToolName} called with parameters: TableName={TableName}, LayoutXml={NewLayoutXml}, FetchXml={NewFetchXml}",
				   nameof(CreateNewView),
				   tableName,
				   newLayoutXml,
				   newFetchXml);

			try
			{
				if (string.IsNullOrWhiteSpace(tableName))
				{
					return "❌ Table name must be provided.";
				}

				if (string.IsNullOrWhiteSpace(viewName))
				{
					return "❌ View name must be provided.";
				}
				if (string.IsNullOrWhiteSpace(newLayoutXml))
				{
					return "❌ LayoutXML must be provided.";
				}
				if (string.IsNullOrWhiteSpace(newFetchXml))
				{
					return "❌ FetchXML must be provided.";
				}


				var client = await clientProvider.GetDataverseClientAsync();
				var context = new DataverseContext(client);

				var existing = context.SavedQuerySet
					.Where(sq => sq.Name == viewName && sq.ReturnedTypeCode == tableName)
					.Select(sq => sq.Id)
					.FirstOrDefault();

				if (existing != Guid.Empty)
				{
					return $"❌ Error: A view with the same name already exists (viewid: {existing}) on the same table.";
				}




				var token = await mcpServer.NotifyProgressAsync(nameof(CreateNewView), "Creating view...");

				var view = new SavedQuery();
				view.Name = viewName;
				view.FetchXml = newFetchXml;//?? $"<fetch version=\"1.0\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"false\"><entity name=\"{tableName}\"><all-attributes /></entity></fetch>";
				view.LayoutXml = newLayoutXml;
				view.ReturnedTypeCode = tableName;
				view.QueryType = SavedQueryQueryType.MainApplicationView;
				view.Description = "View created using MCP Server";
				view.Id = await client.CreateAsync(view);

				await mcpServer.NotifyProgressAsync(token, "View created, now publishing table...");


				logger.LogTrace("Publishing table...");
				publishXmlBuilder.AddTable(tableName);
				var request = publishXmlBuilder.Build();

				await client.ExecuteAsync(request);
				logger.LogTrace("Publishing table...COMPLETED");


				await mcpServer.NotifyProgressAsync(token, "Completed!", 100);

				return $"✅ View '{viewName}' created for table '{tableName}: viewid: {view.Id}'";
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while creating view: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
