using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.Json;
using System.Xml.Linq;

namespace Greg.Xrm.Mcp.FormEngineer.Server.Tools
{
	/// <summary>
	/// MCP Tools for Dataverse form operations
	/// </summary>
	[McpServerToolType]
	public class DataverseFormTools
	{
		protected DataverseFormTools() { }





		[McpServerTool, Description("Updates a Dataverse form definition with new XML. Returns the ID of the updated or created form.")]
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
					return JsonConvert.SerializeObject(forms.ToList(), Formatting.Indented);
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

			return JsonConvert.SerializeObject(result, Formatting.Indented);
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
				var jsonString = JsonConvert.SerializeObject(dynamicObj, Formatting.Indented);
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
				var errorJson = JsonConvert.SerializeObject(errorObj, Formatting.Indented);
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
