using Greg.Xrm.Mcp.Core.Authentication;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.AppModules
{
	[McpServerToolType]
	public class GetAppComponentList(
		ILogger<GetAppComponentList> logger,
		IDataverseClientProvider clientProvider
	)
	{
		[McpServerTool(Name = "dataverse_appmodule_getcomponentlist",
			Destructive = false,
			Idempotent = true,
			ReadOnly = true),
		Description(@"Returns the list of components associated with a given appmodule.")]
		public async Task<string> Execute(

			[Description("MANDATORY: The GUID of the app.")]
			string appId
		)
		{
			logger.LogTrace("{ToolName} called with parameters: appId={AppId}",
				   nameof(ValidateApp),
				   appId);



			if (!Guid.TryParse(appId, out var appGuid))
			{
				return "❌ Error: Invalid App ID. It must be a valid GUID.";
			}

			var client = await clientProvider.GetDataverseClientAsync();

			try
			{
				var request = new RetrieveAppComponentsRequest
				{
					AppModuleId= appGuid
				};


				var response = (RetrieveAppComponentsResponse)await client.ExecuteAsync(request);

				var responseText = JsonConvert.SerializeObject(response.AppComponents, Formatting.Indented, new JsonSerializerSettings { 
					NullValueHandling = NullValueHandling.Ignore 
				});

				return $"✅ Retrieved data: " + Environment.NewLine + responseText;
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
