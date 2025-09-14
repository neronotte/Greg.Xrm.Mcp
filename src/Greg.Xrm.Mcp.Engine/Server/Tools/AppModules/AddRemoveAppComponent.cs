using Greg.Xrm.Mcp.Core.Authentication;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.AppModules
{
	[McpServerToolType]
	public class AddRemoveAppComponent(
		ILogger<AddRemoveAppComponent> logger,
		IDataverseClientProvider clientProvider
	)
	{
		[McpServerTool(Name = "dataverse_appmodule_addremovecomponent",
			Destructive = false,
			Idempotent = false,
			ReadOnly = false),
		Description(@"Adds or removes a component (like an entity, dashboard, systemform or savedquery) to/from a model-driven app.
Components MUST be specified one at a time. Components MUST be explictly added to the app to make them available in the app.
If the user asks to add a table to the sitemap via dataverse_sitemap_update tool, you MUST also add the table as a component to the app using this tool.
If you add an entity to the app, you do NOT need to also add its associated views and forms; they will be included automatically.
If you want to exclude specific views or forms, and you have only added  you can do that by adding explicitly all the other views or forms belonging to the same entity.
Only views of type MainApplicationView (QueryType=0) can be explicitly removed from the app.
When you remove an entity from the app, all its associated views and forms are also removed automatically.
When you remove an entity from the app, remember to also remove it from the sitemap using the dataverse_sitemap_update tool.
")]
		public async Task<string> Execute(
			[Description("MANDATORY: The operation to perform. Must be 'add' or 'remove'.")]
			string operation,
		
			[Description("MANDATORY: The GUID of the app to which components will be added.")]
			string appId,
		
			[Description("MANDATORY: The name of the component to add (e.g., 'entity', 'dashboard', 'savedquery', 'systemform').")]
			string componentName,

			[Description("MANDATORY: The GUID of the component to add.")]
			string componentId
		)
		{
			logger.LogTrace("{ToolName} called.",
					 nameof(AddRemoveAppComponent));

			if (operation != "add" && operation != "remove")
			{
				return "❌ Error: Invalid operation. Must be 'add' or 'remove'.";
			}

			if (!Guid.TryParse(appId, out var appGuid))
			{
				return "❌ Error: Invalid App ID. It must be a valid GUID.";
			}

			if (string.IsNullOrWhiteSpace(componentName))
			{
				return "❌ Error: Component Name is required.";
			}

			if (!Guid.TryParse(componentId, out var componentGuid))
			{
				return "❌ Error: Invalid Component ID. It must be a valid GUID.";
			}

			var client = await clientProvider.GetDataverseClientAsync();

			try
			{
				OrganizationRequest request;
				if (operation == "add")
				{
					request = new AddAppComponentsRequest
					{
						AppId = appGuid,
						Components = [
							new EntityReference(componentName, componentGuid)
						]
					};
				}
				else
				{
					request = new RemoveAppComponentsRequest
					{
						AppId = appGuid,
						Components = [
										new EntityReference(componentName, componentGuid)
									]
					};
				}

				await client.ExecuteAsync(request);

				return $"✅ Success: operation completed!";
			}
			catch
			(Exception ex)
			{
				logger.LogError(ex, "Error in {ToolName}: {ErrorMessage}",
					nameof(AddRemoveAppComponent), ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
