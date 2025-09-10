using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

namespace Greg.Xrm.Mcp.Server.Tools.AppModules
{
	[McpServerToolType]
	public class ValidateApp(
		ILogger<ValidateApp> logger,
		IDataverseClientProvider clientProvider
	)
	{
		[McpServerTool(
			Name = "dataverse_appmodule_validate",
			Destructive = false,
			Idempotent = true,
			ReadOnly = true),
			Description(@"Given the unique name of a given appmodule, performs a validation of the app module using the Dataverse ValidateAppRequest API.
It is strongly recommended to run the current tool after any change on the appmodule definition or to the app sitemap.")]
		public async Task<string> Execute(
			[Description("MANDATORY: The unique name of the app module to validate.")]
			string appModuleName
		)
		{
			logger.LogTrace("{ToolName} called with parameters: appModuleName={AppModuleName}",
				   nameof(ValidateApp),
				   appModuleName);

			if (string.IsNullOrWhiteSpace(appModuleName))
			{
			return "❌ Error: appModuleName is mandatory.";
			}

			try
			{
				var client = await clientProvider.GetDataverseClientAsync();
				var context = new DataverseContext(client);

				var appModule = context.AppModuleSet
					.Where(am => am.UniqueName == appModuleName)
					.Select(am => am.AppModuleId)
					.FirstOrDefault();

				if (appModule == null || appModule == Guid.Empty)
				{
					return $"❌ Error: App module with unique name '{appModuleName}' not found.";
				}

				var request = new ValidateAppRequest { AppModuleId = appModule.Value };

				var response = (ValidateAppResponse)await client.ExecuteAsync(request);

				var responseFormatted = JsonConvert.SerializeObject(response.AppValidationResponse, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});


				logger.LogInformation("✅ {ToolName} completed successfully: {Response}",
					nameof(ValidateApp), responseFormatted);

				var sb = new StringBuilder();
				if (response.AppValidationResponse.ValidationSuccess)
				{
					sb.AppendLine("✅ Validation succeeded.");
				}
				else
				{
					sb.AppendLine("❌ Validation failed.");
				}

				sb.AppendLine("Detailed response: ").AppendLine(responseFormatted);

				return sb.ToString();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error in {ToolName}: {ErrorMessage}",
				 nameof(CreateNewApp),
				 ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
