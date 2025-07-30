using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	public class FormXmlValidator(ILogger<FormXmlValidator> logger) : IFormXmlValidator
	{
		private XmlSchemaSet? schemaSet;


		public XmlSchemaSet GetSchemaSet()
		{
			if (this.schemaSet != null)
			{
				return schemaSet;
			}

			var assembly = GetType().Assembly;

			string[] resourceNames = ["FormXml.xsd", "RibbonCore.xsd", "RibbonTypes.xsd", "RibbonWSS.xsd"];

			var schemas = new XmlSchemaSet();
			foreach (var resource in resourceNames)
			{
				var stream = assembly.GetManifestResourceStream($"Greg.Xrm.Mcp.FormEngineer.Resources.{resource}");
				if (stream == null)
				{
					logger.LogError("Resource {Resource} not found in assembly", resource);
					throw new InvalidOperationException($"Resource {resource} not found in assembly");
				}

				var schema = XmlSchema.Read(stream, (sender, e) =>
				{
					logger.LogError("Error reading schema {Resource}: {Message}", resource, e.Message);
				});
				if (schema == null)
				{
					logger.LogError("Failed to read schema from resource {Resource}", resource);
					throw new InvalidOperationException($"Failed to read schema from resource {resource}");
				}

				schemas.Add(schema);
			}

			schemas.Compile();

			this.schemaSet = schemas;
			return this.schemaSet;
		}


		public FormXmlValidationResult TryValidateFormXmlAgainstSchema(string formXml)
		{
			var result = new FormXmlValidationResult();

			try
			{
				if (string.IsNullOrEmpty(formXml))
				{
					result.AddError("Form XML is empty or null");
					return result;
				}


				logger.LogDebug("🔍 Validating form XML against schema");

				
				// Create XML reader settings with validation
				var settings = new XmlReaderSettings
				{
					ValidationType = ValidationType.Schema,
					Schemas = GetSchemaSet()
				};

				// Add validation event handler to collect errors and warnings
				settings.ValidationEventHandler += (sender, e) =>
				{
					// Use the enhanced FormXmlValidationMessage.FromMessage to parse and separate message parts
					var validationMessage = FormXmlValidationMessage.FromMessage(
						e.Severity == XmlSeverityType.Error ? FormXmlValidationLevel.Error : FormXmlValidationLevel.Warning,
						e.Message
					);

					// Update the message with line/position info if available from the exception
					if (e.Exception?.LineNumber > 0 || e.Exception?.LinePosition > 0)
					{
						validationMessage = validationMessage with
						{
							Row = e.Exception?.LineNumber,
							Column = e.Exception?.LinePosition
						};
					}

					if (e.Severity == XmlSeverityType.Error)
					{
						result.AddError(validationMessage);
						logger.LogWarning("Validation Error: {Message}", validationMessage.Message);
					}
					else if (e.Severity == XmlSeverityType.Warning)
					{
						result.AddWarning(validationMessage);
						logger.LogWarning("Validation Warning: {Message}", validationMessage.Message);
					}
				};

				// Validate the XML
				using var formXmlReader = new StringReader(formXml);
				using var xmlReader = XmlReader.Create(formXmlReader, settings);
				try
				{
					// Read through the entire document to trigger validation
#pragma warning disable S108 // Nested blocks of code should not be left empty
					while (xmlReader.Read()) { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
				}
				catch (XmlException xmlEx)
				{
					var message = $"XML Parsing Error: {xmlEx.Message}";
					var validationMessage = FormXmlValidationMessage.Create(
						FormXmlValidationLevel.Error,
						"XML Parsing Error",
						xmlEx.Message,
						xmlEx.LineNumber,
						xmlEx.LinePosition
					);
					result.AddError(validationMessage);
					logger.LogError(xmlEx, "XML parsing error during validation");
				}

				// Step 2: Perform custom validation rules including grid layout validation
				//TryValidateWithCustomRules(formXml, result);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error during form validation");
				var validationMessage = FormXmlValidationMessage.Create(
					FormXmlValidationLevel.Error,
					"Unhandled validation exception",
					$"{ex.GetType().Name}: {ex.Message}"
				);
				result.AddError(validationMessage);
			}

			return result;
		}


		/// <summary>
		/// Validates the form XML against custom business rules, including section grid layout validation
		/// </summary>
		/// <param name="formXml">The form XML to validate</param>
		/// <param name="result">The validation result to add errors and warnings to</param>
		private void TryValidateWithCustomRules(string formXml, FormXmlValidationResult result)
		{
			try
			{
				var serializer = new XmlSerializer(typeof(FormType));

				var form = (FormType?)serializer.Deserialize(new StringReader(formXml));
				if (form == null)
				{
					var validationMessage = FormXmlValidationMessage.Create(
						FormXmlValidationLevel.Error,
						"Form XML deserialization failed",
						"Unable to deserialize form XML to FormType object"
					);
					result.AddError(validationMessage);
					return;
				}

				// Validate each section in the form for proper grid layout
				foreach (var tab in form.tabs?.tab ?? [])
				{
					foreach (var column in tab.columns)
					{
						foreach (var section in column.sections?.section ?? [])
						{
							// Use the new comprehensive grid validation instead of simple column validation
							ValidateSection(section, result);
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error during custom validation rules");
				var validationMessage = FormXmlValidationMessage.Create(
					FormXmlValidationLevel.Error,
					"Custom validation error",
					ex.Message
				);
				result.AddError(validationMessage);
			}
		}

		/// <summary>
		/// Validates a single section using the comprehensive grid validation logic
		/// This method now delegates to SectionGridValidator for complete grid layout validation
		/// </summary>
		/// <param name="section">The section to validate</param>
		/// <param name="result">The validation result to add errors to</param>
		private static void ValidateSection(FormTypeTabsTabColumnSectionsSection section, FormXmlValidationResult result)
		{
			// Delegate to the comprehensive grid validator which handles:
			// 1. Basic validation (rows and cells existence)
			// 2. Grid dimension calculation considering row and column spans
			// 3. Complete coverage validation (no gaps)
			// 4. Overlap detection
			// 5. Row consistency validation
			SectionGridValidator.ValidateSection(section, result);
		}
	}
}
