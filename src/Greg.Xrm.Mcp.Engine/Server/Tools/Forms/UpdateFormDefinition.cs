using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.Forms
{
	[McpServerToolType]
	public class UpdateFormDefinition(
			ILogger<UpdateFormDefinition> logger,
			IFormXmlValidator validator,
			IDataverseClientProvider clientProvider,
			IFormService formService,
			IPublishXmlBuilder publishXmlBuilder
			)
	{

		/// <summary>
		/// Updates a Dataverse form definition with new XML and publishes the changes to the environment.
		/// This method retrieves an existing form by ID, updates its XML definition, and publishes the changes
		/// to make them available to users.
		/// </summary>
		/// <param name="logger">Logger instance for tracking operations and debugging</param>
		/// <param name="validator">Service for validating FormXML against the Dataverse schema before updates</param>
		/// <param name="clientProvider">Service provider for authenticating and connecting to Dataverse</param>
		/// <param name="formService">Service for performing form-related operations</param>
		/// <param name="publishXmlBuilder">Builder service for creating publish XML requests</param>
		/// <param name="formId">The unique identifier (GUID) of the form to update</param>
		/// <param name="formXml">The new XML definition for the form, must be valid FormXML</param>
		/// <returns>
		/// A task that represents the asynchronous operation. The task result contains:
		/// - Success message with form ID if the update and publish succeed
		/// - Error message with details if validation, update, or publish fail
		/// </returns>
		/// <exception cref="ArgumentException">Thrown when formId is not a valid GUID format</exception>
		/// <exception cref="InvalidOperationException">Thrown when the form with specified ID is not found</exception>
		/// <remarks>
		/// This method performs the following operations:
		/// 1. Validates the formXml parameter against the FormXML schema using the validator service
		/// 2. Authenticates to the Dataverse environment using the client provider
		/// 3. Retrieves the existing form by ID to ensure it exists and get table information
		/// 4. Updates the form's XML definition with the provided FormXML
		/// 5. Publishes the changes to the specific table to make them available to users
		/// 
		/// **Important Notes:**
		/// - The method includes built-in FormXML schema validation to prevent invalid updates
		/// - Publishing is automatically performed for the affected table after the update
		/// - The operation is marked as destructive and idempotent in the MCP server configuration
		/// - All operations are logged with appropriate trace, error, and success messages
		/// 
		/// **Validation Process:**
		/// The FormXML validation uses the embedded Dataverse FormXML schema definition to ensure
		/// that the provided XML will be accepted by the Dataverse platform. If validation fails,
		/// the method returns detailed error information without attempting the update.
		/// 
		/// **Publishing Behavior:**
		/// After a successful form update, the method automatically publishes changes for the
		/// associated table to ensure the updated form is immediately available to users.
		/// 
		/// The method supports all form types including Main, QuickCreate, QuickView, Card, and others
		/// as defined in the systemform_type enumeration.
		/// </remarks>
		/// <example>
		/// <code>
		/// var result = await UpdateFormDefinition(
		///     logger,
		///     validator,
		///     clientProvider,
		///     formService,
		///     publishXmlBuilder,
		///     Guid.Parse("12345678-1234-1234-1234-123456789012"),
		///     "&lt;form&gt;...&lt;/form&gt;"
		/// );
		/// 
		/// if (result.StartsWith("✅"))
		/// {
		///     // Update successful
		///     Console.WriteLine("Form updated successfully!");
		/// }
		/// else
		/// {
		///     // Handle error
		///     Console.WriteLine($"Update failed: {result}");
		/// }
		/// </code>
		/// </example>
		/// <seealso cref="ValidateFormXmlAgainstSchema"/>
		/// <seealso cref="GetFormDefinition"/>
		[McpServerTool(
			Name = "dataverse_form_update_formxml",
			Destructive = true,
			ReadOnly = false,
			Idempotent = true
		),
		Description(
@"Updates a Dataverse form definition with new XML and returns the ID of the updated or created form. 
Be sure to validate the formXml against it's schema definition before executing the update.
")]
		public async Task<string> Execute(
			[Description("Form ID (required)")] Guid formId,
			[Description("Form XML")] string formXml
			)
		{
			logger.LogTrace("{ToolName} called with parameters: FormId={FormId}, FormXml={FormXml}",
				   nameof(UpdateFormDefinition),
				   formId,
				   formXml);

			try
			{
				var validationResult = validator.TryValidateFormXmlAgainstSchema(formXml);
				if (!validationResult.IsValid)
				{
					logger.LogError("❌ Form XML validation failed with {ErrorCount} errors and {WarningCount} warnings. Run the ValidateFormXmlAgainstSchema command to get more details about the validation errors",
						validationResult.Count(x => x.Level == FormXmlValidationLevel.Error),
						validationResult.Count(x => x.Level == FormXmlValidationLevel.Warning));
					return "❌ Form XML validation failed: " + string.Join(", ", validationResult);
				}


				logger.LogTrace("🔐 Authenticating to Dataverse");
				var client = await clientProvider.GetDataverseClientAsync();

				var form = await formService.GetFormByIdAsync(client, formId);
				if (form == null)
				{
					logger.LogError("❌ No form found with ID: {FormId}", formId);
					return "❌ No form found with ID: " + formId;
				}

				var tableName = form.ObjectTypeCode;

				form.FormXml = formXml;

				logger.LogTrace("🔄 Updating form with ID: {FormId}", formId);
				await client.UpdateAsync(form);


				logger.LogTrace("Publish all");

				publishXmlBuilder.AddTable(tableName);
				var request = publishXmlBuilder.Build();

				await client.ExecuteAsync(request);

				logger.LogTrace("✅ Form updated successfully: {FormId}", formId);
				return $"✅ Form updated successfully: {formId}";
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error during form retrieval");
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
