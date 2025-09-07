using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Greg.Xrm.Mcp.Server.Tools.Forms
{
	[McpServerToolType]
	public class ValidateFormDefinition(
			ILogger<ValidateFormDefinition> logger,
			IFormXmlValidator validator)
	{


		/// <summary>
		/// Validates form XML against the Dataverse FormXML schema to ensure compliance and correctness.
		/// This method performs comprehensive schema validation using the embedded XSD schema and reports
		/// all validation errors, warnings, and provides helpful guidance for fixing issues.
		/// </summary>
		/// <param name="logger">Logger instance for tracking validation operations and results</param>
		/// <param name="validator">Service that performs the actual FormXML validation against the schema</param>
		/// <param name="formXml">The form XML content to validate against the FormXML schema</param>
		/// <returns>
		/// A validation result string containing:
		/// - Success message if validation passes without errors or warnings
		/// - Detailed list of validation errors with line numbers and positions
		/// - List of validation warnings if any are found
		/// - Helpful tips for resolving common validation issues
		/// - Error message if an exception occurs during validation
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when formXml parameter is null or empty</exception>
		/// <exception cref="XmlException">Thrown when the XML is malformed or contains syntax errors</exception>
		/// <exception cref="XmlSchemaException">Thrown when schema validation encounters critical errors</exception>
		/// <remarks>
		/// This method serves as a comprehensive FormXML validation tool that should be used before
		/// attempting any form updates to ensure the XML meets Dataverse requirements.
		/// 
		/// **Validation Process:**
		/// 1. Uses the IFormXmlValidator service to validate against the embedded FormXML XSD schema
		/// 2. Categorizes validation results into errors and warnings
		/// 3. Formats results with helpful guidance and troubleshooting tips
		/// 4. Provides detailed error information including line numbers when available
		/// 
		/// **Validation Categories:**
		/// - **Errors**: Critical issues that will prevent the form from being saved in Dataverse
		/// - **Warnings**: Non-critical issues or deprecated elements that may still work but should be addressed
		/// - **Success**: Clean validation with no issues found
		/// 
		/// **Output Format:**
		/// The method returns user-friendly formatted text with:
		/// - Visual indicators (✅ for success, ❌ for errors, ⚠️ for warnings)
		/// - Numbered lists of issues for easy reference
		/// - Helpful tips section with common resolution strategies
		/// - Summary statistics for quick assessment
		/// 
		/// **Best Practices:**
		/// - Always validate FormXML before calling UpdateFormDefinition
		/// - Address all errors before attempting form updates
		/// - Consider addressing warnings for best practices compliance
		/// - Use this tool during development cycles for early issue detection
		/// 
		/// The validation uses the official Dataverse FormXML schema definition to ensure
		/// that the provided XML will be accepted by the Dataverse platform.
		/// 
		/// This method should be called before attempting to update a form using
		/// <see cref="UpdateFormDefinition"/> to ensure the XML is valid and will not
		/// cause errors during the form update process.
		/// </remarks>
		/// <example>
		/// <code>
		/// string formXmlContent = LoadFormXmlFromFile("account_form.xml");
		/// var validationResult = ValidateFormXmlAgainstSchema(logger, validator, formXmlContent);
		/// 
		/// if (validationResult.Contains("✅"))
		/// {
		///     // XML is valid, safe to proceed with update
		///     Console.WriteLine("FormXML is valid - ready for deployment");
		///     await UpdateFormDefinition(logger, validator, clientProvider, formService, 
		///         publishXmlBuilder, formId, formXmlContent);
		/// }
		/// else
		/// {
		///     // Handle validation errors
		///     Console.WriteLine("Validation issues found:");
		///     Console.WriteLine(validationResult);
		/// }
		/// </code>
		/// </example>
		/// <seealso cref="UpdateFormDefinition"/>
		/// <seealso cref="IFormXmlValidator"/>
		[McpServerTool(
			Name = "dataverse_form_validate_formxml",
			Destructive = false,
			ReadOnly = true,
			Idempotent = true
		),
		Description("Validates form XML against the Dataverse FormXML schema. Returns validation results with any errors found or a success message.")]
		public string ValidateFormXmlAgainstSchema(
			[Description("Form XML to validate")] string formXml)
		{
			logger.LogTrace("{ToolName} called with parameters: FormXml={FormXml}",
				   nameof(ValidateFormXmlAgainstSchema),
				   formXml);


			try
			{
				var validationResult = validator.TryValidateFormXmlAgainstSchema(formXml);

				// Format and return results
				var result = new StringBuilder();

				if (validationResult.IsValid && !validationResult.HasWarnings)
				{
					result.AppendLine("✅ Form XML validation successful!");
					result.AppendLine("📋 The form XML is valid according to the Dataverse FormXML schema.");
					logger.LogTrace("✅ Form XML validation successful");
					return result.ToString();
				}




				result.AppendLine("❌ Form XML validation failed!");
				result.AppendLine();

				var validationErrors = validationResult.Where(x => x.Level == FormXmlValidationLevel.Error).ToList();
				if (validationErrors.Count > 0)
				{
					result.AppendLine($"🚨 **{validationErrors.Count} Validation Error(s):**");
					for (int i = 0; i < validationErrors.Count; i++)
					{
						result.AppendLine($"   {i + 1}. {validationErrors[i]}");
					}
					result.AppendLine();
				}

				var validationWarnings = validationResult.Where(x => x.Level == FormXmlValidationLevel.Error).ToList();
				if (validationWarnings.Count > 0)
				{
					result.AppendLine($"⚠️ **{validationWarnings.Count} Validation Warning(s):**");
					for (int i = 0; i < validationWarnings.Count; i++)
					{
						result.AppendLine($"   {i + 1}. {validationWarnings[i]}");
					}
					result.AppendLine();
				}

				result.AppendLine("💡 **Tips:**");
				result.AppendLine("   • Check that all required attributes are present");
				result.AppendLine("   • Verify that element names and structure match the schema");
				result.AppendLine("   • Ensure that attribute values are within allowed ranges");
				result.AppendLine("   • Make sure nested elements follow the correct hierarchy");

				logger.LogWarning("Form XML validation failed with {ErrorCount} errors and {WarningCount} warnings",
					validationErrors.Count, validationWarnings.Count);


				return result.ToString();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error during form validation");
				return $"❌ Error during validation: {ex.Message}";
			}
		}
	}
}
