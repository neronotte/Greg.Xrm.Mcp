using Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Models;
using Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Services;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Greg.Xrm.Mcp.FormEngineer.ConsoleUI
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("Dataverse Form Validation Console Application");
			Console.WriteLine("=" + new string('=', 49));
			Console.WriteLine();

			try
			{
				// Set up dependency injection and logging
				using var host = CreateHost(args);
				
				// Run the validation process
				await RunValidationAsync(host.Services);
				
				Console.WriteLine("\nValidation process completed successfully!");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n[ERROR] Application failed: {ex.Message}");
				Environment.ExitCode = 1;
				
				#if DEBUG
				Console.WriteLine($"\nFull exception details:\n{ex}");
				#endif
			}
			finally
			{
				#if DEBUG
				Console.WriteLine("\nPress any key to exit...");
				Console.ReadKey(true);
				#endif
			}
		}

		/// <summary>
		/// Creates and configures the dependency injection host
		/// </summary>
		private static IHost CreateHost(string[] args)
		{
			var builder = Host.CreateApplicationBuilder(args);
			
			// Configure logging
			builder.Logging.ClearProviders();
			builder.Logging.AddConsole();
			
			#if DEBUG
			builder.Logging.SetMinimumLevel(LogLevel.Debug);
			#else
			builder.Logging.SetMinimumLevel(LogLevel.Information);
			#endif

			// Register services
			builder.Services.AddSingleton<DataverseConnectionManager>();
			builder.Services.AddSingleton<DataverseMetadataService>();
			builder.Services.AddSingleton<FormValidationService>();
			builder.Services.AddSingleton<ExcelReportGenerator>();
			builder.Services.AddSingleton<ConsoleProgressReporter>();
			builder.Services.AddSingleton<FormXmlValidator>();

			return builder.Build();
		}

		/// <summary>
		/// Main validation orchestration logic with enhanced progress bars
		/// </summary>
		private static async Task RunValidationAsync(IServiceProvider services)
		{
			var logger = services.GetRequiredService<ILogger<Program>>();
			var connectionManager = services.GetRequiredService<DataverseConnectionManager>();
			var metadataService = services.GetRequiredService<DataverseMetadataService>();
			var validationService = services.GetRequiredService<FormValidationService>();
			var excelGenerator = services.GetRequiredService<ExcelReportGenerator>();
			var progressReporter = services.GetRequiredService<ConsoleProgressReporter>();

			var stopwatch = Stopwatch.StartNew();
			var validationEntries = new List<ValidationReportEntry>();
			var summary = new ValidationSummary();
			var tableFormCounts = new Dictionary<string, (int totalForms, List<string> formNames)>(); // Track all forms per table

			try
			{
				// Step 1: Connect to Dataverse with spinner
				using var serviceClient = await connectionManager.EstablishConnectionAsync();

				// Step 2: Retrieve all tables with forms
				var tables = await metadataService.RetrieveAllTablesAsync(serviceClient);
				summary.TotalTables = tables.Count;

				if (tables.Count == 0)
				{
					Console.WriteLine("WARNING: No tables with forms found in this environment.");
					return;
				}

				// Calculate estimated total forms for overall progress
				Console.WriteLine("Calculating form counts for progress tracking...");
				var estimatedTotalForms = 0;
				for (int i = 0; i < Math.Min(tables.Count, 10); i++) // Sample first 10 tables for estimation
				{
					try
					{
						var sampleForms = await metadataService.RetrieveFormsForTableAsync(serviceClient, tables[i]);
						estimatedTotalForms += sampleForms.Count;
					}
					catch
					{
						// Skip problematic tables in estimation
					}
				}
				var avgFormsPerTable = tables.Count > 0 ? (double)estimatedTotalForms / Math.Min(10, tables.Count) : 0;
				estimatedTotalForms = (int)(avgFormsPerTable * tables.Count);

				Console.WriteLine($"Estimated total forms to validate: ~{estimatedTotalForms}");
				Console.WriteLine();

				// Step 3: Process each table and validate its forms with enhanced progress
				var processedForms = 0;
				for (int tableIndex = 0; tableIndex < tables.Count; tableIndex++)
				{
					var table = tables[tableIndex];
					
					try
					{
						// Get forms for this table
						var forms = await metadataService.RetrieveFormsForTableAsync(serviceClient, table);
						
						if (forms.Count == 0)
						{
							summary.SkippedTables.Add($"{table.DisplayName} (no forms)");
							
							// Update overall progress even for skipped tables
							progressReporter.ReportOverallProgress(
								tableIndex + 1, 
								tables.Count, 
								processedForms, 
								estimatedTotalForms, 
								$"Skipped: {table.DisplayName}"
							);
							continue;
						}

						// Report table progress with visual progress bar
						progressReporter.ReportTableProgress(tableIndex + 1, tables.Count, 
							$"{table.DisplayName} ({table.LogicalName})", forms.Count);

						// Track all forms for this table (including those without issues)
						var tableKey = $"{table.DisplayName} ({table.LogicalName})";
						tableFormCounts[tableKey] = (forms.Count, forms.Select(f => f.Name).ToList());

						// Validate each form in this table with form-level progress
						for (int formIndex = 0; formIndex < forms.Count; formIndex++)
						{
							var form = forms[formIndex];
							summary.TotalForms++;
							processedForms++;

							try
							{
								// Validate the form
								var formEntries = await validationService.ValidateFormAsync(form, table);
								validationEntries.AddRange(formEntries);

								// Update statistics
								var hasErrors = formEntries.Any(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase));
								var hasWarnings = formEntries.Any(e => e.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase));

								if (hasErrors)
								{
									summary.FormsWithErrors++;
									summary.TotalErrors += formEntries.Count(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase));
								}
								
								if (hasWarnings)
								{
									summary.FormsWithWarnings++;
									summary.TotalWarnings += formEntries.Count(e => e.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase));
								}

								// Report progress for this form with visual indicator
								var resultSummary = hasErrors 
									? $"{formEntries.Count(e => e.Level.Equals("Error", StringComparison.OrdinalIgnoreCase))} errors"
									: hasWarnings 
										? $"{formEntries.Count(e => e.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase))} warnings"
										: "Valid";

								progressReporter.ReportFormProgress(formIndex + 1, forms.Count, form.Name, resultSummary);
								
								// Update overall progress periodically (every 10 forms or last form in table)
								if (formIndex % 10 == 0 || formIndex == forms.Count - 1)
								{
									progressReporter.ReportOverallProgress(
										tableIndex + 1, 
										tables.Count, 
										processedForms, 
										Math.Max(estimatedTotalForms, processedForms), // Adjust estimate if needed
										table.DisplayName
									);
								}
							}
							catch (Exception ex)
							{
								logger.LogError(ex, "Error validating form {FormName} in table {TableName}", form.Name, table.LogicalName);
								
								// Add an error entry for the failed validation
								validationEntries.Add(new ValidationReportEntry
								{
									TableName = $"{table.DisplayName} ({table.LogicalName})",
									FormName = form.Name,
									FormType = form.FormType,
									Level = "Error",
									ErrorMessage = $"Validation process failed: {ex.Message}",
									ErrorLine = "N/A - Process error"
								});

								summary.FormsWithErrors++;
								summary.TotalErrors++;

								progressReporter.ReportFormProgress(formIndex + 1, forms.Count, form.Name, "Error (validation failed)");
							}
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Error processing table {TableName}", table.LogicalName);
						summary.SkippedTables.Add($"{table.DisplayName} (error: {ex.Message})");
						
						// Update overall progress for skipped table
						progressReporter.ReportOverallProgress(
							tableIndex + 1, 
							tables.Count, 
							processedForms, 
							estimatedTotalForms, 
							$"Error: {table.DisplayName}"
						);
					}
				}

				stopwatch.Stop();
				summary.Duration = stopwatch.Elapsed;

				// Step 4: Generate and display summary with final progress
				Console.WriteLine("\n" + new string('=', 60));
				Console.WriteLine("VALIDATION COMPLETED");
				Console.WriteLine(new string('=', 60));
				progressReporter.ReportValidationSummary(summary);

				// Step 5: Show detailed error analysis if there are issues
				if (validationEntries.Count > 0)
				{
					progressReporter.ReportDetailedErrorAnalysis(validationEntries);
					progressReporter.ReportErrorsByTable(validationEntries);
				}

				// Step 5.5: Show table-level statistics
				if (tableFormCounts.Count > 0)
				{
					progressReporter.ReportTableStatistics(tableFormCounts, validationEntries);
				}

				// Step 6: Generate Excel report with progress indicators
				Console.WriteLine("\nGenerating Excel report...");
				
				// Show Excel generation progress
				progressReporter.ReportExcelProgress(1, 5, "Creating table summary sheet...");
				await Task.Delay(500); // Small delay to show progress
				
				progressReporter.ReportExcelProgress(2, 5, "Creating validation results sheet...");
				await Task.Delay(300);
				
				progressReporter.ReportExcelProgress(3, 5, "Creating overall summary sheet...");
				var excelBytes = await excelGenerator.CreateValidationReportAsync(validationEntries, summary, tableFormCounts);
				
				progressReporter.ReportExcelProgress(4, 5, "Applying formatting...");
				await Task.Delay(300);
				
				progressReporter.ReportExcelProgress(5, 5, "Saving file...");
				var reportPath = await excelGenerator.SaveReportToFileAsync(excelBytes);

				// Step 7: Auto-launch the Excel file
				Console.WriteLine("Launching Excel file...");
				excelGenerator.OpenExcelFile(reportPath);

				// Step 8: Final summary with completion status
				Console.WriteLine("\n" + new string('=', 50));
				if (summary.TotalErrors == 0)
				{
					Console.WriteLine("All forms passed validation successfully!");
				}
				else
				{
					Console.WriteLine($"Validation completed with {summary.TotalErrors} errors and {summary.TotalWarnings} warnings.");
					Console.WriteLine("Review the Excel report for detailed information about the issues found.");
				}
				
				Console.WriteLine($"Report available at: {reportPath}");
				
				// Show final completion progress bar
				Console.WriteLine("\nProcess Completion:");
				progressReporter.ReportOverallProgress(tables.Count, tables.Count, processedForms, processedForms, "Complete!");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Critical error during validation process");
				throw;
			}
		}
	}
}
