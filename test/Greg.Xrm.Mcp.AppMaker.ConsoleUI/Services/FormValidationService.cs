using Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Models;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Services
{
    /// <summary>
    /// Service for validating form XML and extracting error context
    /// </summary>
    public class FormValidationService
    {
        private readonly ILogger<FormValidationService> _logger;
        private readonly FormXmlValidator _validator;
        private readonly XmlSchemaSet _schemaSet;

        public FormValidationService(ILogger<FormValidationService> logger, FormXmlValidator validator)
        {
            _logger = logger;
            _validator = validator;
            _schemaSet = LoadFormXmlSchemas();
        }

        /// <summary>
        /// Validates a form and returns validation report entries
        /// </summary>
        /// <param name="form">Form to validate</param>
        /// <param name="table">Table the form belongs to</param>
        /// <returns>List of validation report entries</returns>
        public async Task<List<ValidationReportEntry>> ValidateFormAsync(FormInfo form, TableInfo table)
        {
            var entries = new List<ValidationReportEntry>();

            try
            {
                _logger.LogDebug("Validating form {FormName} for table {TableName}", form.Name, table.LogicalName);

                // Validate using the enhanced FormXmlValidator
                var validationResult = _validator.TryValidateFormXmlAgainstSchema(form.FormXml);

                // Convert each validation message to a report entry
                foreach (var message in validationResult)
                {
                    var errorLine = await ExtractErrorLineFromXmlAsync(form.FormXml, message.Message);

                    if (message.Column.HasValue)
                    {
						var startIndex = form.FormXml[..message.Column.Value].LastIndexOf("<", StringComparison.OrdinalIgnoreCase);

                        var endIndex = form.FormXml.IndexOf(">", startIndex, StringComparison.OrdinalIgnoreCase);

                        if (startIndex >= 0 && endIndex > startIndex)
                        {
                            errorLine = form.FormXml.Substring(startIndex, endIndex - startIndex + 1);
                        }
					}
                    

					entries.Add(new ValidationReportEntry
                    {
                        TableName = $"{table.DisplayName} ({table.LogicalName})",
                        FormName = form.Name,
                        FormType = form.FormType,
                        Level = message.Level.ToString(),
                        ErrorMessage = message.Message,
                        ErrorLine = errorLine,
                        Row = message.Row,
                        Column = message.Column,
                        FixedPart = message.FixedPart,
                        VariablePart = message.VariablePart
                    });
                }

                _logger.LogDebug("Form {FormName} validation completed with {Count} issues", form.Name, entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating form {FormName}", form.Name);
                
                // Add an error entry for the validation failure itself
                entries.Add(new ValidationReportEntry
                {
                    TableName = $"{table.DisplayName} ({table.LogicalName})",
                    FormName = form.Name,
                    FormType = form.FormType,
                    Level = "Error",
                    ErrorMessage = $"Validation failed: {ex.Message}",
                    ErrorLine = "N/A - Validation process error",
                    FixedPart = "Validation failed",
                    VariablePart = ex.Message
                });
            }

            return entries;
        }

        /// <summary>
        /// Extracts the XML line and context that generated the error
        /// </summary>
        /// <param name="formXml">Form XML content</param>
        /// <param name="errorMessage">Error message from validation</param>
        /// <returns>XML snippet showing the error context</returns>
        private async Task<string> ExtractErrorLineFromXmlAsync(string formXml, string errorMessage)
        {
            try
            {
                // Try to parse line number from error message
                var lineNumber = ExtractLineNumberFromError(errorMessage);
                if (lineNumber.HasValue)
                {
                    return ExtractXmlLinesByNumber(formXml, lineNumber.Value);
                }

                // If no line number, try to find relevant XML based on error content
                return await ExtractXmlByErrorContentAsync(formXml, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not extract error line context");
                return "Unable to extract XML context";
            }
        }

        /// <summary>
        /// Extracts line number from error message
        /// </summary>
        private int? ExtractLineNumberFromError(string errorMessage)
        {
            var patterns = new[]
            {
                @"Line (\d+)",
                @"line (\d+)",
                @"at line (\d+)",
                @"Line Number: (\d+)"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(errorMessage, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var lineNum))
                {
                    return lineNum;
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts XML lines by line number
        /// </summary>
        private string ExtractXmlLinesByNumber(string formXml, int lineNumber)
        {
            var lines = formXml.Split('\n');
            if (lineNumber <= 0 || lineNumber > lines.Length)
            {
                return "Line number out of range";
            }

            var startLine = Math.Max(0, lineNumber - 2); // Include 1 line before
            var endLine = Math.Min(lines.Length - 1, lineNumber + 1); // Include 2 lines after
            
            var result = new StringBuilder();
            for (int i = startLine; i <= endLine; i++)
            {
                var prefix = i == lineNumber - 1 ? ">>> " : "    ";
                result.AppendLine($"{prefix}Line {i + 1}: {lines[i].Trim()}");
            }

            return result.ToString().TrimEnd();
        }

        /// <summary>
        /// Extracts XML context based on error content
        /// </summary>
        private Task<string> ExtractXmlByErrorContentAsync(string formXml, string errorMessage)
        {
            try
            {
                var doc = XDocument.Parse(formXml);
                
                // Look for specific elements mentioned in error messages
                if (errorMessage.Contains("section", StringComparison.OrdinalIgnoreCase))
                {
                    var sections = doc.Descendants("section").Take(3);
                    return Task.FromResult(FormatXmlElements(sections, "Sections"));
                }
                
                if (errorMessage.Contains("row", StringComparison.OrdinalIgnoreCase))
                {
                    var rows = doc.Descendants("row").Take(3);
                    return Task.FromResult(FormatXmlElements(rows, "Rows"));
                }
                
                if (errorMessage.Contains("cell", StringComparison.OrdinalIgnoreCase))
                {
                    var cells = doc.Descendants("cell").Take(3);
                    return Task.FromResult(FormatXmlElements(cells, "Cells"));
                }
                
                if (errorMessage.Contains("tab", StringComparison.OrdinalIgnoreCase))
                {
                    var tabs = doc.Descendants("tab").Take(3);
                    return Task.FromResult(FormatXmlElements(tabs, "Tabs"));
                }

                // Default: show the root form element
                return Task.FromResult(FormatXmlElements(new[] { doc.Root }, "Root Element"));
            }
            catch
            {
                return Task.FromResult("Unable to parse XML for context extraction");
            }
        }

        /// <summary>
        /// Formats XML elements for display
        /// </summary>
        private string FormatXmlElements(IEnumerable<XElement?> elements, string elementType)
        {
            var result = new StringBuilder();
            result.AppendLine($"=== {elementType} ===");
            
            foreach (var element in elements.Where(e => e != null))
            {
                // Create a simplified view of the element
                var simplified = new XElement(element!.Name);
                
                // Copy important attributes
                foreach (var attr in element.Attributes().Take(5)) // Limit attributes
                {
                    simplified.SetAttributeValue(attr.Name, attr.Value);
                }
                
                // Add indication if element has children
                if (element.HasElements)
                {
                    simplified.Value = $"... ({element.Elements().Count()} child elements)";
                }
                else if (!string.IsNullOrWhiteSpace(element.Value))
                {
                    simplified.Value = element.Value.Length > 50 
                        ? element.Value.Substring(0, 50) + "..." 
                        : element.Value;
                }

                result.AppendLine(simplified.ToString());
                result.AppendLine();
            }

            return result.ToString().TrimEnd();
        }

        /// <summary>
        /// Loads the FormXML schemas from the XSD files
        /// </summary>
        private XmlSchemaSet LoadFormXmlSchemas()
        {
            var schemaSet = new XmlSchemaSet();
            
            try
            {
                var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var schemaFiles = new[]
                {
                    "FormXml.xsd",
                    "RibbonCore.xsd",
                    "RibbonTypes.xsd",
                    "RibbonWSS.xsd"
                };

                foreach (var schemaFile in schemaFiles)
                {
                    var schemaPath = Path.Combine(appDirectory ?? "", schemaFile);
                    if (File.Exists(schemaPath))
                    {
                        try
                        {
                            schemaSet.Add(null, schemaPath);
                            _logger.LogDebug("Loaded schema: {SchemaFile}", schemaFile);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load schema {SchemaFile}", schemaFile);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Schema file not found: {SchemaPath}", schemaPath);
                    }
                }

                schemaSet.Compile();
                _logger.LogInformation("Loaded {Count} schemas for validation", schemaSet.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FormXML schemas");
            }

            return schemaSet;
        }
    }
}