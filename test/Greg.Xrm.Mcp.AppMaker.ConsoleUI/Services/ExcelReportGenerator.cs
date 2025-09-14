using ClosedXML.Excel;
using Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Services
{
    /// <summary>
    /// Service for generating Excel reports of validation results with progress reporting
    /// </summary>
    public class ExcelReportGenerator
    {
        private readonly ILogger<ExcelReportGenerator> _logger;

        public ExcelReportGenerator(ILogger<ExcelReportGenerator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates an Excel validation report with progress callbacks
        /// </summary>
        /// <param name="entries">Validation report entries</param>
        /// <param name="summary">Validation summary statistics</param>
        /// <param name="tableFormCounts">Dictionary of table names to total form counts and form names</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Excel workbook as byte array</returns>
        public Task<byte[]> CreateValidationReportAsync(List<ValidationReportEntry> entries, ValidationSummary summary, Dictionary<string, (int totalForms, List<string> formNames)>? tableFormCounts = null, Action<int, String>? progressCallback = null)
        {
            _logger.LogInformation("Generating Excel report with {Count} entries", entries.Count);

            using var workbook = new XLWorkbook();
            
            progressCallback?.Invoke(1, "Creating table summary sheet...");
            // Create the table summary sheet first (most useful overview)
            CreateTableSummarySheet(workbook, entries, tableFormCounts);
            
            progressCallback?.Invoke(2, "Creating validation results sheet...");
            // Create the main validation results sheet
            CreateValidationResultsSheet(workbook, entries);
            
            progressCallback?.Invoke(3, "Creating form analysis summary sheet...");
            // Create the form analysis summary sheet
            if (tableFormCounts != null)
            {
                CreateFormAnalysisSummarySheet(workbook, entries, tableFormCounts);
            }

            progressCallback?.Invoke(4, "Creating overall summary sheet...");
            // Create the overall summary sheet
            CreateSummarySheet(workbook, summary);

            progressCallback?.Invoke(5, "Applying formatting and finalizing...");
            // Save to memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var reportBytes = stream.ToArray();

            progressCallback?.Invoke(6, "Excel report completed");
            _logger.LogInformation("Excel report generated successfully ({Size} bytes)", reportBytes.Length);
            return Task.FromResult(reportBytes);
        }

        /// <summary>
        /// Saves the Excel report to a file
        /// </summary>
        /// <param name="reportBytes">Excel report as byte array</param>
        /// <param name="outputDirectory">Directory to save the file (optional)</param>
        /// <returns>Full path to the saved file</returns>
        public async Task<string> SaveReportToFileAsync(byte[] reportBytes, string? outputDirectory = null)
        {
            try
            {
                // Determine output directory
                var directory = !string.IsNullOrEmpty(outputDirectory) 
                    ? outputDirectory 
                    : Environment.CurrentDirectory;

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Generate filename with timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"DataverseFormValidation_{timestamp}.xlsx";
                var filePath = Path.Combine(directory, fileName);

                // Write file
                await File.WriteAllBytesAsync(filePath, reportBytes);

                _logger.LogInformation("Report saved to: {FilePath}", filePath);
                Console.WriteLine($"Report saved to: {filePath}");

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving report to file");
                throw new InvalidOperationException($"Failed to save report to file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Automatically opens the Excel file using the default application
        /// </summary>
        /// <param name="filePath">Path to the Excel file</param>
        public void OpenExcelFile(string filePath)
        {
            try
            {
                _logger.LogInformation("Opening Excel file: {FilePath}", filePath);
                Console.WriteLine($"Opening Excel file: {filePath}");

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // This will use the default application associated with .xlsx files
                };

                Process.Start(processStartInfo);
                _logger.LogInformation("Excel file opened successfully");
                Console.WriteLine("Excel file opened successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to automatically open Excel file: {FilePath}", filePath);
                Console.WriteLine($"Warning: Could not automatically open Excel file. Please open it manually: {filePath}");
            }
        }

        /// <summary>
        /// Creates the main validation results sheet
        /// </summary>
        private void CreateValidationResultsSheet(XLWorkbook workbook, List<ValidationReportEntry> entries)
        {
            var worksheet = workbook.Worksheets.Add("Validation Results");

            // Define column headers - Added new columns for enhanced error clustering
            var headers = new[]
            {
                "Table Name",
                "Form Name", 
                "Form Type",
                "Level",
                "Error Message",
                "Error Line",
                "Row",
                "Column",
                "Fixed Part",
                "Variable Part"
            };

            // Set headers
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            // Add data rows
            for (int row = 0; row < entries.Count; row++)
            {
                var entry = entries[row];
                var dataRow = row + 2; // Start from row 2 (after header)

                worksheet.Cell(dataRow, 1).Value = TruncateText(entry.TableName, 32767);
                worksheet.Cell(dataRow, 2).Value = TruncateText(entry.FormName, 32767);
                worksheet.Cell(dataRow, 3).Value = TruncateText(entry.FormType, 32767);
                worksheet.Cell(dataRow, 4).Value = TruncateText(entry.Level, 32767);
                worksheet.Cell(dataRow, 5).Value = TruncateText(entry.ErrorMessage, 32000); // Leave some margin
                worksheet.Cell(dataRow, 6).Value = TruncateText(entry.ErrorLine, 200); // Limit to 200 chars as requested
                worksheet.Cell(dataRow, 7).Value = entry.Row?.ToString() ?? "";
                worksheet.Cell(dataRow, 8).Value = entry.Column?.ToString() ?? "";
                worksheet.Cell(dataRow, 9).Value = TruncateText(entry.FixedPart, 32000);
                worksheet.Cell(dataRow, 10).Value = TruncateText(entry.VariablePart, 32000);
            }

            // Create Excel table
            if (entries.Count > 0)
            {
                var tableRange = worksheet.Range(1, 1, entries.Count + 1, headers.Length);
                var table = tableRange.CreateTable("ValidationResults");
                
                // Apply table formatting
                table.Theme = XLTableTheme.TableStyleMedium9;
            }

            // Apply formatting
            ApplyTableFormatting(worksheet, headers.Length, entries.Count);
            
            // Apply conditional formatting for levels
            ApplyConditionalFormatting(worksheet, entries.Count);

            _logger.LogDebug("Created validation results sheet with {Count} entries", entries.Count);
        }

        /// <summary>
        /// Truncates text to fit within Excel cell limits
        /// </summary>
        /// <param name="text">Text to truncate</param>
        /// <param name="maxLength">Maximum allowed length</param>
        /// <returns>Truncated text with indicator if truncated</returns>
        private string TruncateText(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Length <= maxLength)
                return text;

            // Truncate and add indicator
            var truncated = text.Substring(0, maxLength - 20); // Leave room for truncation message
            return truncated + "... [TRUNCATED]";
        }

        /// <summary>
        /// Creates a summary statistics sheet with enhanced formatting
        /// </summary>
        private void CreateSummarySheet(XLWorkbook workbook, ValidationSummary summary)
        {
            var worksheet = workbook.Worksheets.Add("Summary");

            // Add summary statistics with enhanced formatting
            var row = 1;
            
            // Title section
            worksheet.Cell(row, 1).Value = "DATAVERSE FORM VALIDATION SUMMARY";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 16;
            worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.DarkBlue;
            row++;
            
            worksheet.Cell(row, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
            row += 2;

            // Metrics header
            worksheet.Cell(row, 1).Value = "Metric";
            worksheet.Cell(row, 2).Value = "Value";
            worksheet.Cell(row, 3).Value = "Percentage";
            
            var headerRange = worksheet.Range(row, 1, row, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;

            // Add metrics with visual indicators
            AddMetricRow(worksheet, row++, "Total Tables Processed", summary.TotalTables, null);
            AddMetricRow(worksheet, row++, "Total Forms Validated", summary.TotalForms, null);
            
            var errorPercentage = summary.TotalForms > 0 ? (double)summary.FormsWithErrors / summary.TotalForms * 100 : 0;
            AddMetricRow(worksheet, row++, "Forms with Errors", summary.FormsWithErrors, $"{errorPercentage:F1}%");
            
            var warningPercentage = summary.TotalForms > 0 ? (double)summary.FormsWithWarnings / summary.TotalForms * 100 : 0;
            AddMetricRow(worksheet, row++, "Forms with Warnings", summary.FormsWithWarnings, $"{warningPercentage:F1}%");
            
            AddMetricRow(worksheet, row++, "Total Errors", summary.TotalErrors, null);
            AddMetricRow(worksheet, row++, "Total Warnings", summary.TotalWarnings, null);
            AddMetricRow(worksheet, row++, "Validation Duration", summary.Duration.ToString(@"hh\:mm\:ss"), null);

            var successRate = summary.TotalForms > 0 
                ? (double)(summary.TotalForms - summary.FormsWithErrors) / summary.TotalForms * 100 
                : 100;
            AddMetricRow(worksheet, row++, "Success Rate", $"{successRate:F1}%", null);

            // Color code the success rate
            if (successRate >= 95)
                worksheet.Cell(row - 1, 2).Style.Font.FontColor = XLColor.Green;
            else if (successRate >= 80)
                worksheet.Cell(row - 1, 2).Style.Font.FontColor = XLColor.Orange;
            else
                worksheet.Cell(row - 1, 2).Style.Font.FontColor = XLColor.Red;

            // Add skipped tables if any
            if (summary.SkippedTables.Count > 0)
            {
                row++;
                worksheet.Cell(row++, 1).Value = "Skipped Tables:";
                worksheet.Cell(row - 1, 1).Style.Font.Bold = true;
                worksheet.Cell(row - 1, 1).Style.Font.FontColor = XLColor.Orange;
                
                foreach (var skippedTable in summary.SkippedTables)
                {
                    worksheet.Cell(row++, 1).Value = $"  - {skippedTable}";
                    worksheet.Cell(row - 1, 1).Style.Font.FontColor = XLColor.Gray;
                }
            }

            // Format the summary sheet
            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 15;

            _logger.LogDebug("Created summary sheet with validation statistics");
        }

        /// <summary>
        /// Adds a metric row with proper formatting
        /// </summary>
        private void AddMetricRow(IXLWorksheet worksheet, int row, string metric, object value, string? percentage)
        {
            worksheet.Cell(row, 1).Value = metric;
            worksheet.Cell(row, 2).Value = value?.ToString() ?? "";
            if (!string.IsNullOrEmpty(percentage))
            {
                worksheet.Cell(row, 3).Value = percentage;
            }

            // Add borders
            var range = worksheet.Range(row, 1, row, 3);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        /// <summary>
        /// Applies general table formatting
        /// </summary>
        private void ApplyTableFormatting(IXLWorksheet worksheet, int columnCount, int rowCount)
        {
            // Auto-fit columns
            worksheet.ColumnsUsed().AdjustToContents();

            // Set minimum and maximum column widths for each column
            worksheet.Column(1).Width = Math.Max(25, Math.Min(50, worksheet.Column(1).Width)); // Table Name
            worksheet.Column(2).Width = Math.Max(25, Math.Min(50, worksheet.Column(2).Width)); // Form Name
            worksheet.Column(3).Width = Math.Max(15, Math.Min(25, worksheet.Column(3).Width)); // Form Type
            worksheet.Column(4).Width = Math.Max(10, Math.Min(15, worksheet.Column(4).Width)); // Level
            worksheet.Column(5).Width = Math.Max(50, Math.Min(80, worksheet.Column(5).Width)); // Error Message
            worksheet.Column(6).Width = Math.Max(30, Math.Min(50, worksheet.Column(6).Width)); // Error Line (truncated to 200 chars)
            worksheet.Column(7).Width = Math.Max(8, Math.Min(15, worksheet.Column(7).Width));  // Row
            worksheet.Column(8).Width = Math.Max(8, Math.Min(15, worksheet.Column(8).Width));  // Column
            worksheet.Column(9).Width = Math.Max(30, Math.Min(60, worksheet.Column(9).Width)); // Fixed Part
            worksheet.Column(10).Width = Math.Max(30, Math.Min(60, worksheet.Column(10).Width)); // Variable Part

            // Enable text wrapping for message columns
            if (rowCount > 0)
            {
                worksheet.Column(5).Style.Alignment.WrapText = true;  // Error Message
                worksheet.Column(6).Style.Alignment.WrapText = true;  // Error Line
                worksheet.Column(9).Style.Alignment.WrapText = true;  // Fixed Part
                worksheet.Column(10).Style.Alignment.WrapText = true; // Variable Part
            }

            // Center align numeric columns (Row and Column)
            if (rowCount > 0)
            {
                worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Row
                worksheet.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Column
            }

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, columnCount);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Add borders
            if (rowCount > 0)
            {
                var dataRange = worksheet.Range(1, 1, rowCount + 1, columnCount);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        /// <summary>
        /// Applies conditional formatting based on validation levels
        /// </summary>
        private void ApplyConditionalFormatting(IXLWorksheet worksheet, int rowCount)
        {
            if (rowCount == 0) return;

            // Level column conditional formatting
            var levelRange = worksheet.Range(2, 4, rowCount + 1, 4); // Level column

            // Red background for Errors
            levelRange.AddConditionalFormat()
                .WhenEquals("Error")
                .Fill.SetBackgroundColor(XLColor.LightPink);

            // Yellow background for Warnings  
            levelRange.AddConditionalFormat()
                .WhenEquals("Warning")
                .Fill.SetBackgroundColor(XLColor.LightYellow);

            // Green background for valid entries (if any)
            levelRange.AddConditionalFormat()
                .WhenEquals("Info")
                .Fill.SetBackgroundColor(XLColor.LightGreen);
        }

        /// <summary>
        /// Creates a table summary sheet showing validation statistics per table
        /// </summary>
        /// <param name="workbook">Excel workbook</param>
        /// <param name="entries">All validation report entries</param>
        /// <param name="tableFormCounts">Dictionary of table names to total form counts and form names</param>
        private void CreateTableSummarySheet(XLWorkbook workbook, List<ValidationReportEntry> entries, Dictionary<string, (int totalForms, List<string> formNames)>? tableFormCounts = null)
        {
            var worksheet = workbook.Worksheets.Add("Table Summary");

            // Calculate table statistics from validation entries
            var tableStats = CalculateTableStatistics(entries, tableFormCounts);

            // Define column headers
            var headers = new[]
            {
                "Table Name",
                "Total Forms",
                "Forms with Errors",
                "Forms with Warnings Only",
                "Forms without Issues",
                "Error Rate %",
                "Warning Rate %",
                "Clean Rate %"
            };

            // Set headers with enhanced formatting
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Add data rows
            var currentRow = 2;
            foreach (var tableStat in tableStats)
            {
                worksheet.Cell(currentRow, 1).Value = tableStat.TableName;
                worksheet.Cell(currentRow, 2).Value = tableStat.TotalForms;
                worksheet.Cell(currentRow, 3).Value = tableStat.FormsWithErrors;
                worksheet.Cell(currentRow, 4).Value = tableStat.FormsWithWarningsOnly;
                worksheet.Cell(currentRow, 5).Value = tableStat.FormsWithoutIssues;
                worksheet.Cell(currentRow, 6).Value = tableStat.ErrorPercentage;
                worksheet.Cell(currentRow, 6).Style.NumberFormat.SetFormat("0.00%");
                worksheet.Cell(currentRow, 7).Value = tableStat.WarningOnlyPercentage;
				worksheet.Cell(currentRow, 7).Style.NumberFormat.SetFormat("0.00%");
				worksheet.Cell(currentRow, 8).Value = tableStat.CleanPercentage;
				worksheet.Cell(currentRow, 8).Style.NumberFormat.SetFormat("0.00%");

				// Apply conditional formatting for percentages
				ApplyTableSummaryConditionalFormatting(worksheet, currentRow, tableStat);

                currentRow++;
            }

            // Create Excel table if we have data
            if (tableStats.Count > 0)
            {
                var tableRange = worksheet.Range(1, 1, tableStats.Count + 1, headers.Length);
                var table = tableRange.CreateTable("TableSummary");
                table.Theme = XLTableTheme.TableStyleMedium2;
            }

            // Apply formatting
            ApplyTableSummaryFormatting(worksheet, headers.Length, tableStats.Count);

            _logger.LogDebug("Created table summary sheet with {Count} tables", tableStats.Count);
        }

        /// <summary>
        /// Calculates validation statistics for each table
        /// </summary>
        /// <param name="entries">All validation report entries</param>
        /// <param name="tableFormCounts">Dictionary of table names to total form counts and form names</param>
        /// <returns>List of table validation summaries</returns>
        private List<TableValidationSummary> CalculateTableStatistics(List<ValidationReportEntry> entries, Dictionary<string, (int totalForms, List<string> formNames)>? tableFormCounts = null)
        {
            var tableStats = new List<TableValidationSummary>();

            // If we have comprehensive form counts, use that as the source of truth
            if (tableFormCounts != null && tableFormCounts.Count > 0)
            {
                foreach (var tableEntry in tableFormCounts)
                {
                    var tableName = tableEntry.Key;
                    var (totalForms, formNames) = tableEntry.Value;

                    // Find validation issues for this table
                    var tableEntries = entries.Where(e => e.TableName == tableName).ToList();
                    
                    // Group by form to get unique forms with issues
                    var formsWithIssues = tableEntries
                        .GroupBy(e => e.FormName)
                        .Select(g => new
                        {
                            FormName = g.Key,
                            HasErrors = g.Any(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)),
                            HasWarnings = g.Any(e => e.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                        })
                        .ToList();

                    var formsWithErrors = formsWithIssues.Count(f => f.HasErrors);
                    var formsWithWarningsOnly = formsWithIssues.Count(f => !f.HasErrors && f.HasWarnings);
                    var formsWithoutIssues = totalForms - formsWithIssues.Count;

                    var stat = new TableValidationSummary
                    {
                        TableName = tableName,
                        TotalForms = totalForms,
                        FormsWithErrors = formsWithErrors,
                        FormsWithWarningsOnly = formsWithWarningsOnly,
                        FormsWithoutIssues = Math.Max(0, formsWithoutIssues) // Ensure non-negative
                    };

                    tableStats.Add(stat);
                }
            }
            else
            {
                // Fallback to the original method if comprehensive counts aren't available
                var allFormsWithIssues = entries
                    .GroupBy(e => new { e.TableName, e.FormName })
                    .Select(g => new 
                    { 
                        g.Key.TableName, 
                        g.Key.FormName,
                        HasErrors = g.Any(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)),
                        HasWarnings = g.Any(e => e.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                    })
                    .ToList();

                var tableGroups = allFormsWithIssues.GroupBy(f => f.TableName);

                foreach (var tableGroup in tableGroups)
                {
                    var tableName = tableGroup.Key;
                    var formsInTable = tableGroup.ToList();

                    var stat = new TableValidationSummary
                    {
                        TableName = tableName,
                        TotalForms = formsInTable.Count, // This will be inaccurate - only forms with issues
                        FormsWithErrors = formsInTable.Count(f => f.HasErrors),
                        FormsWithWarningsOnly = formsInTable.Count(f => !f.HasErrors && f.HasWarnings),
                        FormsWithoutIssues = 0 // Can't determine without comprehensive counts
                    };

                    tableStats.Add(stat);
                }
            }

            // Sort by error rate (descending), then by total forms (descending)
            return tableStats
                .OrderByDescending(t => t.ErrorPercentage)
                .ThenByDescending(t => t.TotalForms)
                .ToList();
        }

        /// <summary>
        /// Applies conditional formatting to table summary rows
        /// </summary>
        /// <param name="worksheet">Excel worksheet</param>
        /// <param name="row">Current row number</param>
        /// <param name="tableStat">Table statistics</param>
        private void ApplyTableSummaryConditionalFormatting(IXLWorksheet worksheet, int row, TableValidationSummary tableStat)
        {
            // Color code error rate column (column 6)
            var errorRateCell = worksheet.Cell(row, 6);
            if (tableStat.ErrorPercentage > 10)
                errorRateCell.Style.Fill.BackgroundColor = XLColor.LightPink;
            else if (tableStat.ErrorPercentage > 5)
                errorRateCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
            else if (tableStat.ErrorPercentage == 0)
                errorRateCell.Style.Fill.BackgroundColor = XLColor.LightGreen;

            // Color code clean rate column (column 8)
            var cleanRateCell = worksheet.Cell(row, 8);
            if (tableStat.CleanPercentage >= 90)
                cleanRateCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
            else if (tableStat.CleanPercentage >= 70)
                cleanRateCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
            else
                cleanRateCell.Style.Fill.BackgroundColor = XLColor.LightPink;

            // Highlight tables with high form counts
            if (tableStat.TotalForms >= 10)
            {
                worksheet.Cell(row, 2).Style.Font.Bold = true; // Total Forms column
            }
        }

        /// <summary>
        /// Applies general formatting to the table summary sheet
        /// </summary>
        /// <param name="worksheet">Excel worksheet</param>
        /// <param name="columnCount">Number of columns</param>
        /// <param name="rowCount">Number of data rows</param>
        private void ApplyTableSummaryFormatting(IXLWorksheet worksheet, int columnCount, int rowCount)
        {
            // Auto-fit columns
            worksheet.ColumnsUsed().AdjustToContents();

            // Set minimum and maximum column widths
            worksheet.Column(1).Width = Math.Max(30, Math.Min(50, worksheet.Column(1).Width)); // Table Name
            
            for (int i = 2; i <= columnCount; i++)
            {
                worksheet.Column(i).Width = Math.Max(12, Math.Min(20, worksheet.Column(i).Width));
            }

            // Center align numeric columns
            for (int col = 2; col <= columnCount; col++)
            {
                var columnRange = worksheet.Range(2, col, rowCount + 1, col);
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Add borders to all data
            if (rowCount > 0)
            {
                var dataRange = worksheet.Range(1, 1, rowCount + 1, columnCount);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Freeze the header row
            worksheet.SheetView.FreezeRows(1);
        }

        /// <summary>
        /// Creates a form analysis summary sheet
        /// </summary>
        private void CreateFormAnalysisSummarySheet(XLWorkbook workbook, List<ValidationReportEntry> entries, Dictionary<string, (int totalForms, List<string> formNames)> tableFormCounts)
        {
            var worksheet = workbook.Worksheets.Add("Form Analysis Summary");

            // Define column headers
            var headers = new[]
            {
                "Table Schema Name",
                "Table Display Name",
                "Form Name",
                "Form ID",
                "Form Type",
                "Errors",
                "Warnings",
                "Clean",
                "Messages"
            };

            // Set headers
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            int row = 2; // Start from the second row (after header)

            // Iterate through all tables and their forms
            foreach (var tableEntry in tableFormCounts)
            {
                var tableSchemaName = tableEntry.Key.Split('(').Last().Trim(')');
                var tableDisplayName = tableEntry.Key.Split('(').First().Trim();

                foreach (var formName in tableEntry.Value.formNames)
                {
                    // Find validation entries for this form
                    var formEntries = entries.Where(e => e.TableName == tableEntry.Key && e.FormName == formName).ToList();

                    // Determine error, warning, and clean flags
                    var hasErrors = formEntries.Any(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase));
                    var hasWarnings = !hasErrors && formEntries.Any(e => e.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase));
                    var isClean = !hasErrors && !hasWarnings;


                    // Concatenate messages
                    var messages = string.Join("; ", formEntries.Select(e => e.ErrorMessage).Distinct());

                    var formType = formEntries.FirstOrDefault()?.FormType ?? "N/A"; // Use first entry's type or placeholder

					// Populate row data
					worksheet.Cell(row, 1).Value = tableSchemaName; // Table schema name
                    worksheet.Cell(row, 2).Value = tableDisplayName; // Table display name
                    worksheet.Cell(row, 3).Value = formName;         // Form name
                    worksheet.Cell(row, 4).Value = "N/A";           // Form ID (placeholder, as ID is not in ValidationReportEntry)
                    worksheet.Cell(row, 5).Value = formType;           // Form type (placeholder, as type is not in ValidationReportEntry)
                    worksheet.Cell(row, 6).Value = hasErrors ? 1 : 0; // Errors
                    worksheet.Cell(row, 7).Value = hasWarnings ? 1 : 0; // Warnings
                    worksheet.Cell(row, 8).Value = isClean ? 1 : 0;   // Clean
                    worksheet.Cell(row, 9).Value = messages;          // Messages

                    row++;
                }
            }

            // Apply formatting
            var tableRange = worksheet.Range(1, 1, row - 1, headers.Length);
            var table = tableRange.CreateTable("FormAnalysisSummary");
            table.Theme = XLTableTheme.TableStyleMedium9;

            worksheet.ColumnsUsed().AdjustToContents();
        }
    }
}