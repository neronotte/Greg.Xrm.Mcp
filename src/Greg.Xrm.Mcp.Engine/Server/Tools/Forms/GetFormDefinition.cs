using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.Forms
{
	[McpServerToolType]
	public class GetFormDefinition(
			ILogger<GetFormDefinition> logger,
			IDataverseClientProvider clientProvider,
			IFormService formService)
	{
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
		public async Task<string> Execute(
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
	}
}
