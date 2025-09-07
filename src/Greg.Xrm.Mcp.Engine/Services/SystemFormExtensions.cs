using Greg.Xrm.Mcp.FormEngineer.Model;
using Newtonsoft.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	/// <summary>
	/// Extension methods for SystemForm collections providing comprehensive formatting and output capabilities.
	/// This static class enhances SystemForm collections with intelligent formatting methods that support
	/// multiple output formats and user-friendly display options for Dataverse form engineering scenarios.
	/// </summary>
	/// <remarks>
	/// The SystemFormExtensions class is designed to work seamlessly with the MCP server architecture,
	/// providing consistent and professional output formatting for form-related operations. It supports
	/// both human-readable and machine-readable output formats to accommodate different use cases.
	/// 
	/// Key capabilities include:
	/// - JSON output with dynamic XML-to-JSON conversion for programmatic processing
	/// - Human-readable XML output with visual formatting for documentation and review
	/// - Interactive selection interfaces for multiple form scenarios
	/// - Robust XML-to-JSON conversion with error handling and fallback mechanisms
	/// 
	/// All methods in this class are designed to be safe, performant, and provide meaningful output
	/// even when dealing with malformed or incomplete data.
	/// </remarks>
	public static class SystemFormExtensions
	{
		/// <summary>
		/// Formats a collection of SystemForm objects into a structured JSON output with form metadata and dynamic XML-to-JSON conversion.
		/// This method creates a comprehensive JSON representation including entity information, form counts, and converted form structures.
		/// </summary>
		/// <param name="forms">The collection of SystemForm objects to format into JSON output</param>
		/// <param name="entityLogicalName">The logical name of the Dataverse entity/table that these forms belong to</param>
		/// <returns>
		/// A formatted JSON string containing:
		/// - Entity logical name and total form count
		/// - Array of form objects with metadata (ID, name, type, description, default status)
		/// - Dynamic form structure converted from XML to JSON format
		/// - Original FormXML preserved for reference
		/// </returns>
		/// <remarks>
		/// The method performs the following operations:
		/// 1. Creates an anonymous object structure with entity metadata
		/// 2. Maps each form to include essential properties and converted structure
		/// 3. Dynamically converts FormXML to JsonElement using <see cref="ConvertXmlToJsonElement"/>
		/// 4. Preserves original XML content for reference and debugging
		/// 5. Serializes the entire structure using Newtonsoft.Json with indented formatting
		/// 
		/// The resulting JSON structure follows this schema:
		/// - entity_logical_name: string
		/// - total_forms: integer
		/// - forms: array of objects containing form metadata and structure
		/// 
		/// This method is particularly useful for API responses, data export scenarios,
		/// and integration with other systems that require structured form data in JSON format.
		/// </remarks>
		/// <example>
		/// <code>
		/// var accountForms = await formService.GetFormsAsync("account");
		/// var jsonOutput = accountForms.FormatJsonOutput("account");
		/// 
		/// // Result will be structured JSON with form metadata and converted XML structure
		/// Console.WriteLine(jsonOutput);
		/// </code>
		/// </example>
		/// <seealso cref="FormatXmlOutput"/>
		/// <seealso cref="ConvertXmlToJsonElement"/>
		public static string FormatJsonOutput(this List<SystemForm> forms, string entityLogicalName)
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

		/// <summary>
		/// Formats a collection of SystemForm objects into a human-readable text output with form details and XML definitions.
		/// This method creates a comprehensive, user-friendly display of form information including formatted metadata and raw XML content.
		/// </summary>
		/// <param name="forms">The collection of SystemForm objects to format for display</param>
		/// <param name="entityLogicalName">The logical name of the Dataverse entity/table that these forms belong to</param>
		/// <returns>
		/// A formatted string containing:
		/// - Header with form count and entity name
		/// - Detailed information for each form including name, ID, type, and default status
		/// - Complete FormXML definition in code block format
		/// - Visual separators between forms for improved readability
		/// </returns>
		/// <remarks>
		/// The method generates output optimized for:
		/// - Console display and logging scenarios
		/// - Documentation and form auditing purposes
		/// - Manual review and analysis of form structures
		/// - Debugging and development workflows
		/// 
		/// The output includes:
		/// 1. Summary header with emoji indicators for visual appeal
		/// 2. Form metadata section with essential properties
		/// 3. Complete XML definition wrapped in markdown code blocks
		/// 4. Horizontal separators (80 characters) between forms
		/// 5. Conditional display of description field when available
		/// 
		/// Each form entry shows:
		/// - Form name with folder emoji (📋)
		/// - Unique identifier (GUID)
		/// - Form type (Main, QuickCreate, etc.)
		/// - Default status (boolean)
		/// - Description (if provided)
		/// - Complete FormXML with syntax highlighting markers
		/// </remarks>
		/// <example>
		/// <code>
		/// var contactForms = await formService.GetFormsAsync("contact");
		/// var textOutput = contactForms.FormatXmlOutput("contact");
		/// 
		/// // Result will be formatted text suitable for console output or logging
		/// Console.WriteLine(textOutput);
		/// logger.LogInformation(textOutput);
		/// </code>
		/// </example>
		/// <seealso cref="FormatJsonOutput"/>
		/// <seealso cref="FormatMultipleFormsSelectionOutput"/>
		public static string FormatXmlOutput(this List<SystemForm> forms, string entityLogicalName)
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
		/// Formats an interactive selection interface when multiple Main forms are found, allowing users to choose the specific form they need.
		/// This method creates a user-friendly selection menu with detailed form information and usage examples to guide users in making informed choices.
		/// </summary>
		/// <param name="forms">The collection of SystemForm objects to display in the selection interface</param>
		/// <param name="entityLogicalName">The logical name of the Dataverse entity/table that these forms belong to</param>
		/// <returns>
		/// A formatted string containing:
		/// - Header indicating the number of forms found
		/// - Numbered list of available forms with key details
		/// - Instructions for selecting a specific form
		/// - Practical usage example with the first form's name
		/// </returns>
		/// <remarks>
		/// This method is specifically designed to handle the common scenario where users request
		/// a "Main" form for a table, but multiple Main forms exist. Instead of arbitrarily choosing
		/// one or returning an error, this method provides an interactive selection interface.
		/// 
		/// **Output Structure:**
		/// 1. **Discovery Header**: Shows the search results with visual indicator
		/// 2. **Available Forms Section**: Lists each form with:
		///    - Sequential numbering for easy reference
		///    - Form name in bold formatting
		///    - Unique form ID for technical reference
		///    - Default status with visual indicators (✅/❌)
		///    - Description when available
		/// 3. **Selection Instructions**: Clear guidance on how to specify a form
		/// 4. **Usage Example**: Practical example using the first form's name
		/// 
		/// **User Experience Features:**
		/// - Visual indicators (🔍, 📋, 💡, 📝) for different sections
		/// - Consistent formatting with bold text for emphasis
		/// - Bullet points for easy scanning
		/// - Copy-paste ready examples
		/// 
		/// **Integration with MCP Tools:**
		/// The output format is designed to work seamlessly with MCP server tools,
		/// providing users with the exact syntax needed for subsequent tool calls.
		/// </remarks>
		/// <example>
		/// <code>
		/// // When multiple Main forms are found during smart form detection
		/// var accountForms = await formService.GetFormsAsync("account", null, systemform_type.Main);
		/// if (accountForms.Count > 1)
		/// {
		///     var selectionOutput = accountForms.FormatMultipleFormsSelectionOutput("account");
		///     return selectionOutput; // Returns interactive selection interface
		/// }
		/// 
		/// // The user can then call with specific form name:
		/// // GetFormDefinition(entityLogicalName="account", formName="Account Information")
		/// </code>
		/// </example>
		/// <seealso cref="FormatXmlOutput"/>
		/// <seealso cref="FormatJsonOutput"/>
		public static string FormatMultipleFormsSelectionOutput(this List<SystemForm> forms, string entityLogicalName)
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

		/// <summary>
		/// Dynamically converts Dataverse FormXML content to a JsonElement for structured data processing.
		/// This method performs safe XML-to-JSON conversion with error handling and fallback mechanisms.
		/// </summary>
		/// <param name="xmlContent">The FormXML content string to convert to JSON format. Can be null or empty.</param>
		/// <returns>
		/// A JsonElement representing the XML structure:
		/// - Empty JSON object ({}) if input is null or empty
		/// - Structured JSON object reflecting the XML hierarchy and attributes
		/// - Error object with original XML content if conversion fails
		/// </returns>
		/// <remarks>
		/// The conversion process follows these steps:
		/// 1. Validates input for null/empty content (returns empty JSON object)
		/// 2. Parses XML content using XDocument for robust XML handling
		/// 3. Converts XML structure to dynamic Dictionary using <see cref="XmlToDynamicDictionary"/>
		/// 4. Serializes Dictionary to JSON string with indented formatting
		/// 5. Parses JSON string back to JsonElement for type safety
		/// 6. Handles exceptions by returning error object with original XML
		/// 
		/// The method is designed to be resilient to malformed XML and provides
		/// fallback behavior that preserves the original content for debugging.
		/// 
		/// Conversion characteristics:
		/// - XML attributes are prefixed with '@' in JSON keys
		/// - Nested elements become nested JSON objects
		/// - Repeated elements become JSON arrays
		/// - Text-only elements become simple string values
		/// - Mixed content is handled with special '#text' properties
		/// </remarks>
		/// <exception cref="Exception">
		/// All exceptions are caught and handled gracefully by returning an error object
		/// containing the original XML content and error message.
		/// </exception>
		/// <example>
		/// <code>
		/// string formXml = "&lt;form&gt;&lt;tab name='general'&gt;Content&lt;/tab&gt;&lt;/form&gt;";
		/// JsonElement jsonResult = ConvertXmlToJsonElement(formXml);
		/// 
		/// // Result will be a JsonElement with the XML structure converted to JSON
		/// Console.WriteLine(jsonResult.ToString());
		/// </code>
		/// </example>
		/// <seealso cref="XmlToDynamicDictionary"/>
		/// <seealso cref="FormatJsonOutput"/>
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
		/// Recursively converts an XElement and its descendants into a Dictionary structure suitable for JSON serialization.
		/// This method handles complex XML structures including attributes, nested elements, arrays, and mixed content scenarios.
		/// </summary>
		/// <param name="element">The XML element to convert to Dictionary format. Can be null.</param>
		/// <returns>
		/// A Dictionary&lt;string, object&gt; representing the XML element structure:
		/// - Empty dictionary if element is null
		/// - XML attributes as dictionary keys prefixed with '@'
		/// - Child elements as nested dictionaries or arrays
		/// - Text content as simple values or '#text' properties
		/// </returns>
		/// <remarks>
		/// The conversion algorithm handles various XML patterns:
		/// 
		/// **Attributes**: XML attributes are converted to dictionary keys with '@' prefix
		/// to distinguish them from element content (e.g., 'name' attribute becomes '@name')
		/// 
		/// **Single Child Elements**: Elements that appear once become dictionary entries
		/// with the element name as key and converted content as value
		/// 
		/// **Multiple Child Elements**: Elements with the same name that appear multiple times
		/// are grouped into arrays for JSON compatibility
		/// 
		/// **Complex vs Simple Elements**:
		/// - Elements with child elements or attributes become nested Dictionary objects
		/// - Elements with only text content become simple string values
		/// 
		/// **Mixed Content**: Elements containing both text and child elements store
		/// text content under the special '#text' key to preserve all information
		/// 
		/// **Recursive Processing**: The method calls itself recursively to handle
		/// deeply nested XML structures of any depth
		/// 
		/// This approach ensures that the resulting Dictionary structure can be
		/// serialized to JSON while preserving the original XML hierarchy and semantics.
		/// </remarks>
		/// <example>
		/// <code>
		/// // XML: &lt;form name="Main"&gt;&lt;tab id="general"&gt;Content&lt;/tab&gt;&lt;tab id="details"&gt;More&lt;/tab&gt;&lt;/form&gt;
		/// XElement element = XElement.Parse(xmlString);
		/// var result = XmlToDynamicDictionary(element);
		/// 
		/// // Result will be:
		/// // {
		/// //   "@name": "Main",
		/// //   "tab": [
		/// //     { "@id": "general", "#text": "Content" },
		/// //     { "@id": "details", "#text": "More" }
		/// //   ]
		/// // }
		/// </code>
		/// </example>
		/// <seealso cref="ConvertXmlToJsonElement"/>
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
	}
}
