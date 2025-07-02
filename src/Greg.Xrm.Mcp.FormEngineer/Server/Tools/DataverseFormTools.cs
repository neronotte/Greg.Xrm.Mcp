using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Greg.Xrm.Mcp.FormEngineer.Server.Tools
{
	/// <summary>
	/// MCP Tools for Dataverse form operations
	/// </summary>
	[McpServerToolType]
	public class DataverseFormTools
	{
		protected DataverseFormTools() { }




		/// <summary>
		/// Updates a Dataverse form definition with new XML and publishes the changes to the environment.
		/// This method retrieves an existing form by ID, updates its XML definition, and publishes the changes
		/// to make them available to users.
		/// </summary>
		/// <param name="logger">Logger instance for tracking operations and debugging</param>
		/// <param name="clientProvider">Service provider for authenticating and connecting to Dataverse</param>
		/// <param name="formService">Service for performing form-related operations</param>
		/// <param name="publishXmlBuilder">Builder service for creating publish XML requests</param>
		/// <param name="entityLogicalName">The logical name of the Dataverse table (e.g., 'account', 'contact')</param>
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
		/// 1. Validates the formId parameter as a valid GUID
		/// 2. Authenticates to the Dataverse environment
		/// 3. Retrieves the existing form by ID
		/// 4. Updates the form's XML definition
		/// 5. Publishes the changes to make them available to users
		/// 
		/// Important: Always validate the formXml against the FormXML schema before calling this method
		/// to ensure the XML is well-formed and compliant with Dataverse requirements.
		/// 
		/// The method supports all form types including Main, QuickCreate, QuickView, Card, and others
		/// as defined in the systemform_type enumeration.
		/// </remarks>
		/// <example>
		/// <code>
		/// var result = await UpdateFormDefinition(
		///     logger,
		///     clientProvider,
		///     formService,
		///     publishXmlBuilder,
		///     "account",
		///     "12345678-1234-1234-1234-123456789012",
		///     "&lt;form&gt;...&lt;/form&gt;"
		/// );
		/// </code>
		/// </example>
		[McpServerTool, 
		Description(@"Updates a Dataverse form definition with new XML and returns the ID of the updated or created form. 
Be sure to validate the formXml against it's schema definition before executing the update.
")]
		public static async Task<string> UpdateFormDefinition(
			ILogger<DataverseFormTools> logger,
			IDataverseClientProvider clientProvider,
			IFormService formService,
			IPublishXmlBuilder publishXmlBuilder,
			[Description("Logical name of the Dataverse table")] string entityLogicalName,
			[Description("Form ID (required)")] string formId,
			[Description("Form XML")] string formXml
			)
		{
			try
			{
				if (!Guid.TryParse(formId, out var parsedFormId))
				{
					logger.LogError("Invalid formId: {FormId}", formId);
					return "❌ Invalid formId: " + formId;
				}


				logger.LogInformation("🔐 Authenticating to Dataverse");
				var client = await clientProvider.GetDataverseClientAsync();

				var form = await formService.GetFormByIdAsync(client, parsedFormId);
				if (form == null)
				{
					logger.LogError("❌ No form found with ID: {FormId}", formId);
					return "❌ No form found with ID: " + formId;
				}

				var tableName = form.ObjectTypeCode;

				form.FormXml = formXml;

				logger.LogInformation("🔄 Updating form with ID: {FormId}", formId);
				await client.UpdateAsync(form);


				logger.LogInformation("Publish all");

				publishXmlBuilder.AddTable(tableName);
				var request = publishXmlBuilder.Build();

				await client.ExecuteAsync(request);

				logger.LogInformation("✅ Form updated successfully: {FormId}", formId);
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
		/// This method performs the following validation steps:
		/// 1. Loads the embedded FormXML XSD schema from resources
		/// 2. Creates an XML schema set for validation
		/// 3. Configures XML reader settings for schema validation
		/// 4. Reads through the entire XML document to trigger validation
		/// 5. Collects and categorizes all validation events (errors/warnings)
		/// 6. Formats results with line numbers and helpful guidance
		/// 
		/// The validation uses the official Dataverse FormXML schema definition to ensure
		/// that the provided XML will be accepted by the Dataverse platform.
		/// 
		/// Validation errors indicate issues that will prevent the form from being saved,
		/// while warnings indicate potential issues or deprecated elements that may still work
		/// but should be addressed for best practices.
		/// 
		/// This method should be called before attempting to update a form using
		/// <see cref="UpdateFormDefinition"/> to ensure the XML is valid and will not
		/// cause errors during the form update process.
		/// </remarks>
		/// <example>
		/// <code>
		/// var validationResult = ValidateFormXmlAgainstSchema(logger, formXmlContent);
		/// if (validationResult.Contains("✅"))
		/// {
		///     // XML is valid, safe to proceed with update
		///     await UpdateFormDefinition(logger, clientProvider, formService, 
		///         publishXmlBuilder, "account", formId, formXmlContent);
		/// }
		/// else
		/// {
		///     // Handle validation errors
		///     Console.WriteLine(validationResult);
		/// }
		/// </code>
		/// </example>
		/// <seealso cref="UpdateFormDefinition"/>
		[McpServerTool, 
		Description("Validates form XML against the Dataverse FormXML schema. Returns validation results with any errors found or a success message.")]
		public static string ValidateFormXmlAgainstSchema(
			ILogger<DataverseFormTools> logger,
			[Description("Form XML to validate")] string formXml)
		{
			try
			{
				var schema = Encoding.UTF8.GetString(Properties.Resources.formxml);
				if (string.IsNullOrEmpty(formXml))
				{
					logger.LogError("❌ Form XML is empty or null");
					return "❌ Form XML is empty or null";
				}

				if (string.IsNullOrEmpty(schema))
				{
					logger.LogError("❌ Schema is empty or null");
					return "❌ Schema is empty or null";
				}

				logger.LogInformation("🔍 Validating form XML against schema");

				var validationErrors = new List<string>();
				var validationWarnings = new List<string>();

				// Create XML schema set
				var schemaSet = new XmlSchemaSet();
				
				// Add the schema from resources
				using (var schemaReader = new StringReader(schema))
				using (var xmlSchemaReader = XmlReader.Create(schemaReader))
				{
					schemaSet.Add(null, xmlSchemaReader);
				}

				// Create XML reader settings with validation
				var settings = new XmlReaderSettings
				{
					ValidationType = ValidationType.Schema,
					Schemas = schemaSet
				};

				// Add validation event handler to collect errors and warnings
				settings.ValidationEventHandler += (sender, e) =>
				{
					var message = $"Line {e.Exception?.LineNumber}, Position {e.Exception?.LinePosition}: {e.Message}";
					
					if (e.Severity == XmlSeverityType.Error)
					{
						validationErrors.Add(message);
						logger.LogWarning("Validation Error: {Message}", message);
					}
					else if (e.Severity == XmlSeverityType.Warning)
					{
						validationWarnings.Add(message);
						logger.LogWarning("Validation Warning: {Message}", message);
					}
				};

				// Validate the XML
				using (var formXmlReader = new StringReader(formXml))
				using (var xmlReader = XmlReader.Create(formXmlReader, settings))
				{
					try
					{
						// Read through the entire document to trigger validation
						while (xmlReader.Read()) { }
					}
					catch (XmlException xmlEx)
					{
						var message = $"XML Parsing Error at Line {xmlEx.LineNumber}, Position {xmlEx.LinePosition}: {xmlEx.Message}";
						validationErrors.Add(message);
						logger.LogError(xmlEx, "XML parsing error during validation");
					}
				}

				// Format and return results
				var result = new StringBuilder();
				
				if (validationErrors.Count == 0 && validationWarnings.Count == 0)
				{
					result.AppendLine("✅ Form XML validation successful!");
					result.AppendLine("📋 The form XML is valid according to the Dataverse FormXML schema.");
					logger.LogInformation("✅ Form XML validation successful");
				}
				else
				{
					result.AppendLine("❌ Form XML validation failed!");
					result.AppendLine();
					
					if (validationErrors.Count > 0)
					{
						result.AppendLine($"🚨 **{validationErrors.Count} Validation Error(s):**");
						for (int i = 0; i < validationErrors.Count; i++)
						{
							result.AppendLine($"   {i + 1}. {validationErrors[i]}");
						}
						result.AppendLine();
					}

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
				}

				return result.ToString();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error during form validation");
				return $"❌ Error during validation: {ex.Message}";
			}
		}







		/// <summary>
		/// Retrieves the definition of a Dataverse form in XML or JSON format
		/// </summary>
		/// <param name="clientProvider">Dataverse authentication service</param>
		/// <param name="formService">Service for form operations</param>
		/// <param name="logger">Logger for diagnostics</param>
		/// <param name="dataverseUrl">Dataverse environment URL (e.g.: https://yourorg.crm.dynamics.com)</param>
		/// <param name="entityLogicalName">Logical name of the Dataverse table</param>
		/// <param name="formName">Specific form name (optional)</param>
		/// <param name="formType">Form type: Main, QuickCreate, QuickView, Card, etc. (optional)</param>
		/// <param name="outputFormat">Output format: 'xml' or 'json' (default: xml)</param>
		/// <param name="includeInactive">Include inactive forms (default: false)</param>
		/// <returns>Form definition in the requested format</returns>
		[McpServerTool, Description("Retrieves the XML or JSON definition of a Dataverse form. If you only specify the table name, it automatically searches for the Main form. If it finds multiple Main forms, it returns the list to allow selection. You can specify formName and/or formType to be more precise.")]
		public static async Task<string> GetFormDefinition(
			ILogger<DataverseFormTools> logger,
			IDataverseClientProvider clientProvider,
			IFormService formService,
			[Description("Logical name of the Dataverse table")] string entityLogicalName,
			[Description("Specific form name (optional)")] string? formName = null,
			[Description("Form type: Main, QuickCreate, QuickView, Card, etc. (optional)")] string? formType = null,
			[Description("Output format: 'xml' or 'json' (default: xml)")] string outputFormat = "xml")
		{
			try
			{
				logger.LogInformation("🔍 Retrieving forms for table: {EntityName}", entityLogicalName);

				if (string.IsNullOrEmpty(entityLogicalName))
				{
					throw new ArgumentException("entity_logical_name is required");
				}

				// Authentication
				logger.LogInformation("🔐 Authenticating to Dataverse");
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
					logger.LogInformation("🎯 No form specified, automatic search for Main form for '{EntityName}'", entityLogicalName);
				}

				// Retrieve forms
				logger.LogInformation("📋 Retrieving forms for table: {EntityName}", entityLogicalName);
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
					return FormatMultipleFormsSelectionOutput(forms, entityLogicalName);
				}

				// If it's a smart search and we found exactly one Main form,
				// add an informative message
				if (isSmartMainFormSearch && forms.Count == 1)
				{
					logger.LogInformation("✅ Found unique Main form for '{EntityName}': {FormName}", entityLogicalName, forms[0].Name);
				}

				// Format output
				var format = outputFormat?.ToLower() ?? "xml";

				if (format == "json")
				{
					return FormatJsonOutput(forms, entityLogicalName);
				}
				else
				{
					return FormatXmlOutput(forms, entityLogicalName);
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
		[McpServerTool, 
		Description("Retrieves the list of forms for a given Dataverse table, and returns the details such as Id, Name, Type, Description, ad if it's default or not")]
		public static async Task<string> GetFormListByTableSchemaName(
			ILogger<DataverseFormTools> logger,
			IDataverseClientProvider clientProvider,
			IFormService formService,
			IPublishXmlBuilder publishXmlBuilder,
			[Description("The logical name of the Dataverse table")] string entityLogicalName,
			[Description("How do you want the output to be returned (Formatted or Json)")] string outputType)
		{
			try
			{
				var client = await clientProvider.GetDataverseClientAsync();

				logger.LogInformation("🔍 Retrieving forms for table: {EntityName}", entityLogicalName);

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




		private static string FormatJsonOutput(List<SystemForm> forms, string entityLogicalName)
		{
			var result = new
			{
				entity_logical_name = entityLogicalName,
				total_forms = forms.Count,
				forms = forms.Select(form => new
				{
					form_id = form.Id,
					name = form.Name,
					type = form.Type.ToString(),
					description = form.Description,
					is_default = form.IsDefault,

					// Dynamic conversion of form XML to JSON
					form_structure_dynamic = ConvertXmlToJsonElement(form.FormXml),

					// We also keep the original XML for reference
					form_xml = !string.IsNullOrEmpty(form.FormXml) ? form.FormXml : null
				})
			};

			return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
		}

		private static string FormatXmlOutput(List<SystemForm> forms, string entityLogicalName)
		{
			var output = new System.Text.StringBuilder();
			output.AppendLine($"✅ Found {forms.Count} forms for table '{entityLogicalName}' (XML format):");
			output.AppendLine();

			foreach (var form in forms)
			{
				output.AppendLine($"📋 Form: {form.Name}");
				output.AppendLine($"   ID: {form.Id}");
				output.AppendLine($"   Type: {form.Type}");
				output.AppendLine($"   Default: {form.IsDefault}");

				if (!string.IsNullOrEmpty(form.Description))
				{
					output.AppendLine($"   Description: {form.Description}");
				}

				output.AppendLine();
				output.AppendLine("📄 XML Definition:");
				output.AppendLine("```xml");
				output.AppendLine(form.FormXml);
				output.AppendLine("```");
				output.AppendLine();

				output.AppendLine("".PadLeft(80, '-'));
				output.AppendLine();
			}

			return output.ToString();
		}






		/// <summary>
		/// Dynamically converts form XML to a JsonElement
		/// </summary>
		/// <param name="xmlContent">XML content to convert</param>
		/// <returns>JsonElement representing the XML structure</returns>
		private static JsonElement ConvertXmlToJsonElement(string? xmlContent)
		{
			if (string.IsNullOrEmpty(xmlContent))
				return JsonDocument.Parse("{}").RootElement;

			try
			{
				// Parse XML
				var xmlDoc = XDocument.Parse(xmlContent);

				// Convert to dynamic Dictionary
				var dynamicObj = XmlToDynamicDictionary(xmlDoc.Root);

				// Serialize to JSON and then parse as JsonElement
				var jsonString = JsonConvert.SerializeObject(dynamicObj, Newtonsoft.Json.Formatting.Indented);
				return JsonDocument.Parse(jsonString).RootElement;
			}
			catch (Exception)
			{
				// In case of error, return an empty object with the original XML
				var errorObj = new Dictionary<string, object>
				{
					["error"] = "Error during XML conversion",
					["original_xml"] = xmlContent ?? ""
				};
				var errorJson = JsonConvert.SerializeObject(errorObj, Newtonsoft.Json.Formatting.Indented);
				return JsonDocument.Parse(errorJson).RootElement;
			}
		}

		/// <summary>
		/// Recursively converts an XElement to a Dictionary for JSON serialization
		/// </summary>
		/// <param name="element">XML element to convert</param>
		/// <returns>Dictionary representing the XML element</returns>
		private static Dictionary<string, object> XmlToDynamicDictionary(XElement? element)
		{
			var result = new Dictionary<string, object>();

			if (element == null) return result;

			// Handle XML attributes (@ prefix to distinguish them from elements)
			foreach (var attr in element.Attributes())
			{
				result[$"@{attr.Name.LocalName}"] = attr.Value;
			}

			// Handle child elements
			var childGroups = element.Elements().GroupBy(e => e.Name.LocalName);

			foreach (var group in childGroups)
			{
				if (group.Count() == 1)
				{
					// Single element
					var single = group.First();
					if (single.HasElements || single.Attributes().Any())
					{
						// Has children or attributes -> complex object
						result[group.Key] = XmlToDynamicDictionary(single);
					}
					else
					{
						// Text only -> simple value
						result[group.Key] = single.Value;
					}
				}
				else
				{
					// Multiple elements -> array
					result[group.Key] = group.Select(e =>
						e.HasElements || e.Attributes().Any()
							? (object)XmlToDynamicDictionary(e)
							: e.Value
					).ToArray();
				}
			}

			// If the element has only text and no attributes/children, 
			// add text as a special property
			if (!result.Any() && !string.IsNullOrEmpty(element.Value))
			{
				result["#text"] = element.Value;
			}

			return result;
		}

		/// <summary>
		/// Formats the output when multiple Main forms are found, allowing the user to choose
		/// </summary>
		/// <param name="forms">List of found forms</param>
		/// <param name="entityLogicalName">Logical name of the table</param>
		/// <returns>Formatted message with the list of available forms</returns>
		private static string FormatMultipleFormsSelectionOutput(List<SystemForm> forms, string entityLogicalName)
		{
			var output = new System.Text.StringBuilder();
			output.AppendLine($"🔍 Found {forms.Count} forms for table '{entityLogicalName}':");
			output.AppendLine();
			output.AppendLine("📋 **AVAILABLE FORMS:**");

			for (int i = 0; i < forms.Count; i++)
			{
				var form = forms[i];
				output.AppendLine($"   {i + 1}. **{form.Name}**");
				output.AppendLine($"      • ID: {form.Id}");
				output.AppendLine($"      • Default: {(form.IsDefault.GetValueOrDefault() ? "✅ Yes" : "❌ No")}");

				if (!string.IsNullOrEmpty(form.Description))
				{
					output.AppendLine($"      • Description: {form.Description}");
				}
				output.AppendLine();
			}

			output.AppendLine("💡 **TO SELECT A SPECIFIC FORM:**");
			output.AppendLine($"   Call again the tool specifying argument 'formName' with one of the following values:");

			foreach (var form in forms)
			{
				output.AppendLine($"   • formName: \"{form.Name}\"");
			}

			output.AppendLine();
			output.AppendLine("📝 **EXAMPLE:**");
			output.AppendLine($"   GetFormDefinition(entityLogicalName=\"{entityLogicalName}\", formName=\"{forms[0].Name}\")");

			return output.ToString();
		}
	}

}
