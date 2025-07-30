using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Greg.Xrm.Mcp.FormEngineer.Server.Tools
{
	/// <summary>
	/// Model Context Protocol (MCP) server tools for comprehensive Dataverse form operations.
	/// This class provides a suite of tools for retrieving, validating, updating, and managing 
	/// Dataverse system forms through MCP server integration.
	/// </summary>
	/// <remarks>
	/// The DataverseFormTools class is designed to work as part of an MCP server ecosystem,
	/// providing AI-powered assistance for Dataverse form engineering tasks. All methods are
	/// decorated with McpServerTool attributes to enable automatic discovery and execution
	/// by MCP-compatible clients.
	/// 
	/// Key capabilities include:
	/// - Form retrieval with intelligent form selection
	/// - FormXML schema validation against Dataverse standards
	/// - Form updates with automatic publishing
	/// - Comprehensive form inventory and reporting
	/// 
	/// This tool suite is particularly valuable for:
	/// - Power Platform developers working with form customizations
	/// - System administrators managing form compliance
	/// - Solution architects designing form structures
	/// - DevOps teams automating form deployment processes
	/// </remarks>
	[McpServerToolType]
	public class DataverseFormTools
	{
		/// <summary>
		/// Initializes a new instance of the DataverseFormTools class.
		/// This constructor is protected to enforce the static nature of the MCP server tools.
		/// </summary>
		protected DataverseFormTools() { }

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
		public static async Task<string> UpdateFormDefinition(
			ILogger<DataverseFormTools> logger,
			IFormXmlValidator validator,
			IDataverseClientProvider clientProvider,
			IFormService formService,
			IPublishXmlBuilder publishXmlBuilder,
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
		public static string ValidateFormXmlAgainstSchema(
			ILogger<DataverseFormTools> logger,
			IFormXmlValidator validator,
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

		/// <summary>
		/// Retrieves the definition of a Dataverse form in XML or JSON format with intelligent form selection capabilities.
		/// This method provides flexible form retrieval with smart defaults, automatic Main form detection, and comprehensive
		/// output formatting options for both human-readable and programmatic use cases.
		/// </summary>
		/// <param name="logger">Logger instance for tracking form retrieval operations and debugging information</param>
		/// <param name="clientProvider">Service provider for authenticating and establishing secure connections to Dataverse</param>
		/// <param name="formService">Service for performing form-related operations including form queries and metadata retrieval</param>
		/// <param name="entityLogicalName">
		/// The logical name of the Dataverse table (e.g., 'account', 'contact', 'opportunity').
		/// This parameter is required and case-sensitive.
		/// </param>
		/// <param name="formName">
		/// Optional specific form name to retrieve. If not provided, the method uses intelligent form selection.
		/// When combined with formType, provides precise form targeting.
		/// </param>
		/// <param name="formType">
		/// Optional form type filter (e.g., 'Main', 'QuickCreate', 'QuickView', 'Card').
		/// Must match values from the systemform_type enumeration. Case-insensitive matching is supported.
		/// </param>
		/// <param name="outputFormat">
		/// Output format specification: 'xml' for human-readable XML format, 'json' for structured JSON format.
		/// Defaults to 'xml'. Case-insensitive matching is used.
		/// </param>
		/// <returns>
		/// A task that represents the asynchronous operation. The task result contains:
		/// - **XML Format**: Human-readable form definition with metadata and complete FormXML
		/// - **JSON Format**: Structured JSON with form metadata and dynamically converted XML-to-JSON structure
		/// - **Multiple Forms Found**: Interactive selection list when smart search finds multiple Main forms
		/// - **Error Message**: Detailed error information if retrieval fails or no forms are found
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when entityLogicalName is null, empty, or when formType contains an invalid value
		/// that doesn't match the systemform_type enumeration
		/// </exception>
		/// <exception cref="UnauthorizedAccessException">Thrown when authentication to Dataverse fails</exception>
		/// <exception cref="InvalidOperationException">Thrown when the specified table does not exist in the environment</exception>
		/// <remarks>
		/// This method implements intelligent form selection logic to provide the best user experience:
		/// 
		/// **Smart Main Form Detection:**
		/// When neither formName nor formType is specified, the method automatically searches for Main forms
		/// for the specified table. This "smart behavior" covers the most common use case.
		/// 
		/// **Multiple Form Handling:**
		/// If the smart search finds multiple Main forms, the method returns a formatted selection list
		/// with form details and usage examples, allowing users to make informed choices.
		/// 
		/// **Precise Form Targeting:**
		/// Users can specify formName and/or formType for exact form retrieval when they know
		/// the specific form they need.
		/// 
		/// **Output Format Options:**
		/// 
		/// **XML Format Features:**
		/// - User-friendly display with emoji indicators
		/// - Complete form metadata (ID, type, default status, description)
		/// - Full FormXML definition in markdown code blocks
		/// - Visual separators for multiple forms
		/// - Optimized for console output and documentation
		/// 
		/// **JSON Format Features:**
		/// - Structured data suitable for programmatic processing
		/// - Dynamic XML-to-JSON conversion of FormXML structure
		/// - Preserved original XML for reference
		/// - Entity metadata and form count information
		/// - API-friendly format for integration scenarios
		/// 
		/// **Error Handling:**
		/// The method provides comprehensive error handling with informative messages:
		/// - Clear indication when no forms are found with applied filters
		/// - Validation of form type parameters against valid enumeration values
		/// - Detailed exception information for troubleshooting
		/// 
		/// **Performance Considerations:**
		/// - Single authentication per method call
		/// - Efficient form queries with appropriate filtering
		/// - Lazy evaluation of output formatting
		/// - Optimized for both single and batch form operations
		/// 
		/// **Logging Integration:**
		/// All operations are comprehensively logged with different levels:
		/// - Trace: Parameter values and operation flow
		/// - Information: Successful operations and form discovery
		/// - Warning: Unusual conditions or multiple form scenarios
		/// - Error: Failures and exception details
		/// </remarks>
		/// <example>
		/// <code>
		/// // Get the Main form for Account table (smart detection)
		/// var accountMainForm = await GetFormDefinition(
		///     logger, clientProvider, formService, "account");
		/// 
		/// // Get a specific form by name
		/// var specificForm = await GetFormDefinition(
		///     logger, clientProvider, formService, "contact", "Contact Information");
		/// 
		/// // Get QuickCreate forms in JSON format
		/// var quickCreateForms = await GetFormDefinition(
		///     logger, clientProvider, formService, "opportunity", 
		///     formType: "QuickCreate", outputFormat: "json");
		/// 
		/// // Parse JSON result for programmatic use
		/// if (quickCreateForms.StartsWith("{"))
		/// {
		///     var formData = JsonConvert.DeserializeObject(quickCreateForms);
		///     // Process structured form data
		/// }
		/// </code>
		/// </example>
		/// <seealso cref="UpdateFormDefinition"/>
		/// <seealso cref="GetFormListByTableSchemaName"/>
		/// <seealso cref="systemform_type"/>
		/// <seealso cref="SystemFormExtensions.FormatJsonOutput"/>
		/// <seealso cref="SystemFormExtensions.FormatXmlOutput"/>
		[McpServerTool(
			Name = "dataverse_form_retrieve_formxml",
			Destructive = false,
			ReadOnly = true,
			Idempotent = true
		), 
		Description("Retrieves the XML or JSON definition of a Dataverse form. If you only specify the table name, it automatically searches for the Main form. If it finds multiple Main forms, it returns the list to allow selection. You can specify formName and/or formType to be more precise.")]
		public static async Task<string> GetFormDefinition(
			ILogger<DataverseFormTools> logger,
			IDataverseClientProvider clientProvider,
			IFormService formService,
			[Description("Logical name of the Dataverse table")] string entityLogicalName,
			[Description("Specific form name (optional)")] string? formName = null,
			[Description("Form type: Main, QuickCreate, QuickView, Card, etc. (optional)")] string? formType = null,
			[Description("Output format: 'xml' or 'json' (default: xml)")] string outputFormat = "xml")
		{
			logger.LogTrace("{ToolName} called with parameters: entityLogicalName={EntityLogicalName}, formName={FormName}, formType={FormType}, outputFormat={OutputFormat}",
				   nameof(GetFormDefinition),
				   entityLogicalName,
				   formName,
				   formType,
				   outputFormat);
			try
			{
				logger.LogTrace("🔍 Retrieving forms for table: {EntityName}", entityLogicalName);

				if (string.IsNullOrEmpty(entityLogicalName))
				{
					throw new ArgumentException("entity_logical_name is required");
				}

				// Authentication
				logger.LogTrace("🔐 Authenticating to Dataverse");
				var client = await clientProvider.GetDataverseClientAsync();

				// Parse form type if specified
				systemform_type? parsedFormType = null;
				if (!string.IsNullOrEmpty(formType))
				{
					if (Enum.TryParse<systemform_type>(formType, true, out var type))
					{
						parsedFormType = type;
					}
					else
					{
						throw new ArgumentException($"Invalid form type: {formType}. Supported values: {string.Join(", ", Enum.GetNames<systemform_type>())}");
					}
				}

				// If the user hasn't specified either formName or formType, 
				// we assume they want the Main form (smart behavior)
				bool isSmartMainFormSearch = string.IsNullOrEmpty(formName) && !parsedFormType.HasValue;
				if (isSmartMainFormSearch)
				{
					parsedFormType = systemform_type.Main;
					logger.LogTrace("🎯 No form specified, automatic search for Main form for '{EntityName}'", entityLogicalName);
				}

				// Retrieve forms
				logger.LogTrace("📋 Retrieving forms for table: {EntityName}", entityLogicalName);
				var forms = await formService.GetFormsAsync(
					client,
					entityLogicalName,
					formName,
					parsedFormType);

				if (forms.Count == 0)
				{
					var filters = new List<string>();
					if (!string.IsNullOrEmpty(formName)) filters.Add($"name: '{formName}'");
					if (parsedFormType.HasValue) filters.Add($"type: '{parsedFormType}'");

					var filterText = filters.Count > 0 ? $" with filters: {string.Join(", ", filters)}" : "";

					return $"❌ No forms found for table '{entityLogicalName}'{filterText}";
				}

				// If it's a smart search and we found multiple Main forms, 
				// return the list to allow selection
				if (isSmartMainFormSearch && forms.Count > 1)
				{
					return forms.FormatMultipleFormsSelectionOutput(entityLogicalName);
				}

				// If it's a smart search and we found exactly one Main form,
				// add an informative message
				if (isSmartMainFormSearch && forms.Count == 1)
				{
					logger.LogTrace("✅ Found unique Main form for '{EntityName}': {FormName}", entityLogicalName, forms[0].Name);
				}

				// Format output
				var format = outputFormat?.ToLower() ?? "xml";

				if (format == "json")
				{
					return forms.FormatJsonOutput(entityLogicalName);
				}
				else
				{
					return forms.FormatXmlOutput(entityLogicalName);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error during form retrieval");
				return $"❌ Error: {ex.Message}";
			}
		}

		/// <summary>
		/// Retrieves a comprehensive list of all forms for a specified Dataverse table with detailed metadata.
		/// This method provides an overview of all available forms for a table, including their properties,
		/// types, and status information in either human-readable or JSON format.
		/// </summary>
		/// <param name="logger">Logger instance for tracking list retrieval operations and debugging</param>
		/// <param name="clientProvider">Service provider for authenticating and connecting to Dataverse</param>
		/// <param name="formService">Service for performing form-related operations (used for consistency in method signatures)</param>
		/// <param name="publishXmlBuilder">Builder service for publish operations (used for consistency in method signatures)</param>
		/// <param name="entityLogicalName">The logical name of the Dataverse table to list forms for (e.g., 'account', 'contact')</param>
		/// <param name="outputType">
		/// Output format type: "Formatted" for human-readable text or "Json" for structured JSON data.
		/// Case-insensitive comparison is used.
		/// </param>
		/// <returns>
		/// A task that represents the asynchronous operation. The task result contains:
		/// - **Formatted output**: Human-readable list with form details including name, ID, type, default status, and description
		/// - **JSON output**: Structured array of form objects with all metadata properties for programmatic processing
		/// - Error message if the table is not found or if an exception occurs
		/// </returns>
		/// <exception cref="ArgumentException">Thrown when entityLogicalName is null or empty</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown when authentication to Dataverse fails</exception>
		/// <exception cref="InvalidOperationException">Thrown when the specified table does not exist in the environment</exception>
		/// <remarks>
		/// This method performs the following operations:
		/// 1. Authenticates to the Dataverse environment using the provided client provider
		/// 2. Creates a DataverseContext for querying system forms
		/// 3. Retrieves all forms for the specified table using LINQ queries
		/// 4. Orders results by form type, then by name for consistent output
		/// 5. Formats the results according to the requested output type
		/// 
		/// **Returned Information:**
		/// - Form ID (unique identifier)
		/// - Form Name (display name)
		/// - Form Type (Main, QuickCreate, QuickView, etc. from <see cref="systemform_type"/> enumeration)
		/// - Default Status (whether the form is the default for its type)
		/// - Description (if available)
		/// 
		/// **Output Formats:**
		/// - **Formatted**: User-friendly text format with emojis and clear sections for manual review
		/// - **JSON**: Machine-readable format suitable for programmatic processing and integration
		/// 
		/// This method is particularly useful for:
		/// - Form inventory and auditing across environments
		/// - Identifying available forms before modification or customization
		/// - Understanding form hierarchy and default assignments
		/// - Bulk form analysis and reporting scenarios
		/// - Development workflows requiring form metadata
		/// </remarks>
		/// <example>
		/// <code>
		/// // Get formatted list of account forms for manual review
		/// var formattedList = await GetFormListByTableSchemaName(
		///     logger, clientProvider, formService, publishXmlBuilder, "account", "Formatted");
		/// 
		/// // Get JSON list for programmatic processing
		/// var jsonList = await GetFormListByTableSchemaName(
		///     logger, clientProvider, formService, publishXmlBuilder, "contact", "Json");
		/// 
		/// // Parse JSON result for further processing
		/// var formsData = JsonConvert.DeserializeObject(jsonList);
		/// </code>
		/// </example>
		/// <seealso cref="GetFormDefinition"/>
		/// <seealso cref="UpdateFormDefinition"/>
		/// <seealso cref="systemform_type"/>
		[McpServerTool(
			Name = "dataverse_form_retrieve_list_by_table",
			Destructive = false,
			ReadOnly = true,
			Idempotent = true
		), 
		Description("Retrieves the list of forms for a given Dataverse table, and returns the details such as Id, Name, Type, Description, ad if it's default or not")]
		public static async Task<string> GetFormListByTableSchemaName(
			ILogger<DataverseFormTools> logger,
			IDataverseClientProvider clientProvider,
			IFormService formService,
			IPublishXmlBuilder publishXmlBuilder,
			[Description("The logical name of the Dataverse table")] string entityLogicalName,
			[Description("How do you want the output to be returned (Formatted or Json)")] string outputType)
		{
			logger.LogTrace("{ToolName} called with parameters: entityLogicalName={EntityLogicalName}, outputType={OutputType}",
				   nameof(GetFormListByTableSchemaName),
				   entityLogicalName,
				   outputType);
			try
			{
				var client = await clientProvider.GetDataverseClientAsync();

				logger.LogTrace("🔍 Retrieving forms for table: {EntityName}", entityLogicalName);

				var context = new DataverseContext(client);

				var forms = context.SystemFormSet.Where(x => x.ObjectTypeCode == entityLogicalName)
					.OrderBy(x => x.Type)
					.ThenBy(x => x.Name)
					.Select(x => new
					{
						x.Id,
						x.Name,
						x.Type,
						x.Description,
						x.IsDefault,
					}).ToList();

				if ("json".Equals(outputType, StringComparison.OrdinalIgnoreCase))
				{
					return JsonConvert.SerializeObject(forms.ToList(), Newtonsoft.Json.Formatting.Indented);
				}
				else
				{
					return $"✅ Found {forms.Count()} forms for table '{entityLogicalName}':\n\n" +
						string.Join(Environment.NewLine, forms.Select(f =>
							$"📋 Form: {f.Name}{Environment.NewLine}" +
							$"   ID: {f.Id}{Environment.NewLine}" +
							$"   Type: {f.Type}{Environment.NewLine}" +
							$"   Default: {(f.IsDefault.GetValueOrDefault() ? "✅ Yes" : "❌ No")}{Environment.NewLine}" +
							$"   Description: {f.Description ?? "N/A"}{Environment.NewLine}"));
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while retrieving forms: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}

		/// <summary>
		/// Formats a collection of SystemForm objects into structured JSON output using extension methods.
		/// This is a helper method that delegates to the SystemFormExtensions.FormatJsonOutput method.
		/// </summary>
		/// <param name="forms">The collection of SystemForm objects to format</param>
		/// <param name="entityLogicalName">The logical name of the entity these forms belong to</param>
		/// <returns>Formatted JSON string with form metadata and structure</returns>
		/// <seealso cref="SystemFormExtensions.FormatJsonOutput"/>
		private static string FormatJsonOutput(List<SystemForm> forms, string entityLogicalName)
		{
			return forms.FormatJsonOutput(entityLogicalName);
		}

		/// <summary>
		/// Formats a collection of SystemForm objects into human-readable XML output using extension methods.
		/// This is a helper method that delegates to the SystemFormExtensions.FormatXmlOutput method.
		/// </summary>
		/// <param name="forms">The collection of SystemForm objects to format</param>
		/// <param name="entityLogicalName">The logical name of the entity these forms belong to</param>
		/// <returns>Formatted string with form details and XML definitions</returns>
		/// <seealso cref="SystemFormExtensions.FormatXmlOutput"/>
		private static string FormatXmlOutput(List<SystemForm> forms, string entityLogicalName)
		{
			return forms.FormatXmlOutput(entityLogicalName);
		}
	}
}
