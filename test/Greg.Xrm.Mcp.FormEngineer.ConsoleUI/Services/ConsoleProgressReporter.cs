using Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Models;
using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Services
{
    /// <summary>
    /// Service for reporting validation progress to the console with visual progress bars
    /// </summary>
    public class ConsoleProgressReporter
    {
        private readonly ILogger<ConsoleProgressReporter> _logger;
        private int _lastProgressLine = -1;

        public ConsoleProgressReporter(ILogger<ConsoleProgressReporter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Reports progress for table processing with visual progress bar
        /// </summary>
        /// <param name="current">Current table index</param>
        /// <param name="total">Total number of tables</param>
        /// <param name="tableName">Current table name</param>
        /// <param name="formCount">Number of forms in this table</param>
        public void ReportTableProgress(int current, int total, string tableName, int formCount)
        {
            Console.WriteLine($"\nValidating forms...");
            
            // Display the overall table progress bar
            DisplayProgressBar("Tables", current, total, tableName);
            
            Console.WriteLine($"[Table {current}/{total}] {tableName} - {formCount} forms");
            _logger.LogInformation("Processing table {Current}/{Total}: {TableName} ({FormCount} forms)", 
                current, total, tableName, formCount);
        }

        /// <summary>
        /// Reports progress for individual form validation with form-level progress bar
        /// </summary>
        /// <param name="current">Current form index</param>
        /// <param name="total">Total forms in current table</param>
        /// <param name="formName">Form name</param>
        /// <param name="validationResult">Validation result summary</param>
        public void ReportFormProgress(int current, int total, string formName, string validationResult)
        {
            var icon = validationResult switch
            {
                var r when r.Contains("error", StringComparison.OrdinalIgnoreCase) => "[ERROR]",
                var r when r.Contains("warning", StringComparison.OrdinalIgnoreCase) => "[WARN]",
                _ => "[OK]"
            };

            // Update the form progress bar on the same line
            var truncatedFormName = formName.Length > 30 ? formName.Substring(0, 27) + "..." : formName;
            DisplayProgressBar("Forms", current, total, $"{truncatedFormName} {icon}", updateInPlace: true);

            _logger.LogDebug("Form {Current}/{Total}: {FormName} - {Result}", 
                current, total, formName, validationResult);
        }

        /// <summary>
        /// Reports overall validation progress across all tables and forms
        /// </summary>
        /// <param name="tablesProcessed">Number of tables processed</param>
        /// <param name="totalTables">Total number of tables</param>
        /// <param name="formsProcessed">Number of forms processed</param>
        /// <param name="totalForms">Total estimated forms</param>
        /// <param name="currentTable">Current table name</param>
        public void ReportOverallProgress(int tablesProcessed, int totalTables, int formsProcessed, int totalForms, string currentTable)
        {
            Console.WriteLine(); // Move to new line after form progress
            
            // Overall progress calculation
            var overallPercentage = totalTables > 0 ? (double)tablesProcessed / totalTables * 100 : 0;
            
            Console.WriteLine($"Overall Progress: {tablesProcessed}/{totalTables} tables | {formsProcessed} forms processed");
            DisplayProgressBar("Overall", (int)overallPercentage, 100, $"{overallPercentage:F1}% - {currentTable}");
            Console.WriteLine();
        }

        /// <summary>
        /// Displays a visual progress bar
        /// </summary>
        /// <param name="label">Label for the progress bar</param>
        /// <param name="current">Current progress value</param>
        /// <param name="total">Total value</param>
        /// <param name="description">Additional description</param>
        /// <param name="updateInPlace">Whether to update the progress bar in place</param>
        private void DisplayProgressBar(string label, int current, int total, string description, bool updateInPlace = false)
        {
            if (total <= 0) return;

            const int barWidth = 40;
            var percentage = Math.Min(100, (double)current / total * 100);
            var filledWidth = (int)(percentage / 100 * barWidth);
            var emptyWidth = barWidth - filledWidth;

            // Create the progress bar with ASCII characters
            var progressBar = "[" + 
                              new string('=', filledWidth) + 
                              new string(' ', emptyWidth) + 
                              "]";

            // Truncate description if too long
            var maxDescLength = Math.Max(20, Console.WindowWidth - 80);
            if (description.Length > maxDescLength)
            {
                description = description.Substring(0, maxDescLength - 3) + "...";
            }

            var progressText = $"{label,-8} {progressBar} {percentage,6:F1}% ({current,3}/{total,-3}) {description}";

            if (updateInPlace && _lastProgressLine >= 0)
            {
                // Move cursor to the last progress line and overwrite it
                var currentLine = Console.CursorTop;
                Console.SetCursorPosition(0, _lastProgressLine);
                Console.Write(new string(' ', Console.WindowWidth - 1)); // Clear the line
                Console.SetCursorPosition(0, _lastProgressLine);
                Console.WriteLine(progressText);
                
                // Restore cursor position
                if (currentLine > _lastProgressLine)
                {
                    Console.SetCursorPosition(0, currentLine);
                }
            }
            else
            {
                Console.WriteLine(progressText);
                _lastProgressLine = Console.CursorTop - 1;
            }
        }

        /// <summary>
        /// Shows a spinning animation while processing
        /// </summary>
        /// <param name="message">Message to display with spinner</param>
        /// <param name="cancellationToken">Cancellation token to stop the spinner</param>
        /// <returns>Task representing the spinner animation</returns>
        public async Task ShowSpinnerAsync(string message, CancellationToken cancellationToken = default)
        {
            var spinnerChars = new[] { '|', '/', '-', '\\' };
            var index = 0;

            Console.Write($"{message} ");
            var messageLength = message.Length + 1;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.Write(spinnerChars[index]);
                    await Task.Delay(150, cancellationToken);
                    Console.Write('\b'); // Backspace to overwrite the spinner character
                    index = (index + 1) % spinnerChars.Length;
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when cancellation is requested
            }
            finally
            {
                // Clear the spinner
                Console.Write('\b');
                Console.Write(' ');
                Console.Write('\b');
            }
        }

        /// <summary>
        /// Shows progress for Excel report generation
        /// </summary>
        /// <param name="step">Current step</param>
        /// <param name="totalSteps">Total steps</param>
        /// <param name="stepDescription">Description of current step</param>
        public void ReportExcelProgress(int step, int totalSteps, string stepDescription)
        {
            DisplayProgressBar("Excel", step, totalSteps, stepDescription);
        }

        /// <summary>
        /// Reports overall validation summary
        /// </summary>
        /// <param name="summary">Validation summary statistics</param>
        public void ReportValidationSummary(ValidationSummary summary)
        {
            Console.WriteLine("\nVALIDATION SUMMARY REPORT");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Total tables processed: {summary.TotalTables}");
            Console.WriteLine($"Total forms validated: {summary.TotalForms}");
            Console.WriteLine($"Forms with validation errors: {summary.FormsWithErrors}");
            Console.WriteLine($"Forms with warnings only: {summary.FormsWithWarnings}");
            Console.WriteLine($"Forms that passed validation: {summary.TotalForms - summary.FormsWithErrors}");
            Console.WriteLine($"Total validation errors: {summary.TotalErrors}");
            Console.WriteLine($"Total validation warnings: {summary.TotalWarnings}");
            Console.WriteLine($"Validation duration: {summary.Duration:hh\\:mm\\:ss}");

            if (summary.TotalForms > 0)
            {
                var successRate = (double)(summary.TotalForms - summary.FormsWithErrors) / summary.TotalForms * 100;
                Console.WriteLine($"Success rate: {successRate:F1}%");
                
                // Show success rate as a progress bar
                DisplayProgressBar("Success", (int)successRate, 100, $"{successRate:F1}% of forms passed validation");
            }

            if (summary.SkippedTables.Count > 0)
            {
                Console.WriteLine($"\nSkipped tables ({summary.SkippedTables.Count}):");
                foreach (var skippedTable in summary.SkippedTables)
                {
                    Console.WriteLine($"   - {skippedTable}");
                }
            }

            // Performance metrics
            if (summary.TotalForms > 0 && summary.Duration.TotalMilliseconds > 0)
            {
                var avgTimePerForm = summary.Duration.TotalMilliseconds / summary.TotalForms;
                Console.WriteLine($"\nPerformance:");
                Console.WriteLine($"   Average time per form: {avgTimePerForm:F0}ms");
                Console.WriteLine($"   Forms per second: {1000 / avgTimePerForm:F1}");
            }

            _logger.LogInformation("Validation completed - {TotalForms} forms, {TotalErrors} errors, {TotalWarnings} warnings", 
                summary.TotalForms, summary.TotalErrors, summary.TotalWarnings);
        }

        /// <summary>
        /// Reports detailed error analysis
        /// </summary>
        /// <param name="entries">All validation report entries</param>
        public void ReportDetailedErrorAnalysis(List<ValidationReportEntry> entries)
        {
            var errors = entries.Where(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)).ToList();
            var warnings = entries.Where(e => e.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase)).ToList();

            if (errors.Count > 0 || warnings.Count > 0)
            {
                Console.WriteLine("\nDETAILED VALIDATION ANALYSIS");
                Console.WriteLine(new string('-', 30));

                // Group by error type
                var errorGroups = entries
                    .GroupBy(e => ExtractErrorCategory(e.ErrorMessage))
                    .OrderByDescending(g => g.Count())
                    .ToList();

                foreach (var group in errorGroups)
                {
                    var errorCount = group.Count(g => g.Level.Equals("Error", StringComparison.OrdinalIgnoreCase));
                    var warningCount = group.Count(g => g.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase));

                    Console.WriteLine($"\n{group.Key} ({group.Count()} total: {errorCount} errors, {warningCount} warnings):");

                    var sampleEntries = group.Take(3);
                    foreach (var entry in sampleEntries)
                    {
                        var levelIcon = entry.Level.Equals("Error", StringComparison.OrdinalIgnoreCase) ? "[ERROR]" : "[WARN]";
                        Console.WriteLine($"   {levelIcon} {entry.TableName} -> {entry.FormName}:");
                        Console.WriteLine($"      {entry.ErrorMessage}");
                    }

                    if (group.Count() > 3)
                    {
                        Console.WriteLine($"   ... and {group.Count() - 3} more similar issues");
                    }
                }
            }
        }

        /// <summary>
        /// Reports error counts by table
        /// </summary>
        /// <param name="entries">All validation report entries</param>
        public void ReportErrorsByTable(List<ValidationReportEntry> entries)
        {
            var tableStats = entries
                .GroupBy(e => e.TableName)
                .Select(g => new
                {
                    TableName = g.Key,
                    ErrorCount = g.Count(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)),
                    WarningCount = g.Count(e => e.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase)),
                    TotalCount = g.Count()
                })
                .Where(s => s.TotalCount > 0)
                .OrderByDescending(s => s.ErrorCount)
                .ThenByDescending(s => s.WarningCount)
                .ToList();

            if (tableStats.Count > 0)
            {
                Console.WriteLine("\nISSUES BY TABLE");
                Console.WriteLine(new string('-', 20));

                foreach (var stat in tableStats.Take(10)) // Show top 10 tables with issues
                {
                    if (stat.ErrorCount > 0)
                    {
                        Console.WriteLine($"[ERROR] {stat.TableName}: {stat.ErrorCount} errors, {stat.WarningCount} warnings");
                    }
                    else if (stat.WarningCount > 0)
                    {
                        Console.WriteLine($"[WARN]  {stat.TableName}: {stat.WarningCount} warnings");
                    }
                }

                if (tableStats.Count > 10)
                {
                    Console.WriteLine($"   ... and {tableStats.Count - 10} more tables with issues");
                }
            }
        }

        /// <summary>
        /// Reports table-level validation statistics
        /// </summary>
        /// <param name="tableFormCounts">Dictionary of table form counts</param>
        /// <param name="validationEntries">All validation entries</param>
        public void ReportTableStatistics(Dictionary<string, (int totalForms, List<string> formNames)> tableFormCounts, List<ValidationReportEntry> validationEntries)
        {
            if (tableFormCounts.Count == 0) return;

            Console.WriteLine("\nTABLE VALIDATION STATISTICS");
            Console.WriteLine(new string('-', 40));

            // Calculate top 5 tables with issues for quick overview
            var tableIssues = tableFormCounts.Select(kvp =>
            {
                var tableName = kvp.Key;
                var totalForms = kvp.Value.totalForms;
                var tableEntries = validationEntries.Where(e => e.TableName == tableName).ToList();
                var formsWithIssues = tableEntries.GroupBy(e => e.FormName).Count();
                var formsWithErrors = tableEntries.GroupBy(e => e.FormName)
                    .Count(g => g.Any(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)));

                return new
                {
                    TableName = tableName,
                    TotalForms = totalForms,
                    FormsWithIssues = formsWithIssues,
                    FormsWithErrors = formsWithErrors,
                    CleanForms = totalForms - formsWithIssues,
                    ErrorRate = totalForms > 0 ? (double)formsWithErrors / totalForms * 100 : 0
                };
            })
            .OrderByDescending(t => t.ErrorRate)
            .ThenByDescending(t => t.FormsWithErrors)
            .Take(10)
            .ToList();

            Console.WriteLine($"Top tables requiring attention:");
            foreach (var table in tableIssues.Where(t => t.FormsWithErrors > 0))
            {
                var errorIcon = table.ErrorRate > 10 ? "[HIGH]" : table.ErrorRate > 0 ? "[MED]" : "[LOW]";
                Console.WriteLine($"  {errorIcon} {table.TableName}:");
                Console.WriteLine($"    - {table.TotalForms} total forms | {table.FormsWithErrors} with errors | {table.CleanForms} clean");
                Console.WriteLine($"    - Error rate: {table.ErrorRate:F1}%");
            }

            var totalCleanTables = tableFormCounts.Count(kvp =>
            {
                var tableName = kvp.Key;
                return !validationEntries.Any(e => e.TableName == tableName && e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase));
            });

            Console.WriteLine($"\nClean tables (no errors): {totalCleanTables}/{tableFormCounts.Count}");
            
            if (totalCleanTables < tableFormCounts.Count)
            {
                Console.WriteLine($"Tables needing attention: {tableFormCounts.Count - totalCleanTables}");
            }
        }

        /// <summary>
        /// Extracts error category from error message for grouping
        /// </summary>
        /// <param name="errorMessage">Error message text</param>
        /// <returns>Error category string</returns>
        private string ExtractErrorCategory(string errorMessage)
        {
            var message = errorMessage.ToLowerInvariant();

            if (message.Contains("incomplete grid coverage"))
                return "Grid Coverage Issues";
            if (message.Contains("overlap"))
                return "Cell Overlap Issues";
            if (message.Contains("no cells defined"))
                return "Missing Cells (Warnings)";
            if (message.Contains("no rows defined"))
                return "Missing Rows";
            if (message.Contains("xml"))
                return "XML Structure Issues";
            if (message.Contains("schema"))
                return "Schema Validation Issues";
            if (message.Contains("validation failed"))
                return "Validation Process Errors";
            if (message.Contains("element") || message.Contains("attribute"))
                return "Element/Attribute Issues";

            return "Other Validation Issues";
        }

        /// <summary>
        /// Clears the current console line
        /// </summary>
        public void ClearCurrentLine()
        {
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
        }
    }
}