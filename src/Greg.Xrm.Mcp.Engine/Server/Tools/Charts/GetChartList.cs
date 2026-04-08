using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

namespace Greg.Xrm.Mcp.Server.Tools.Charts
{
	[McpServerToolType]
	public class GetChartList(
		ILogger<GetChartList> logger,
		IDataverseClientProvider clientProvider,
		IChartService chartService)
	{
		[McpServerTool(
			Name = "dataverse_chart_list",
			Destructive = false,
			ReadOnly = true,
			Idempotent = true),
		Description("Lists all charts (visualizations) available for a specific Dataverse table. Returns chart names, IDs, types, and default status.")]
		public async Task<string> Execute(
			[Description("Logical name of the Dataverse table")] string entityLogicalName,
			[Description("Optional chart name filter")] string? chartName = null,
			[Description("Output format: 'formatted' or 'json' (default: formatted)")] string outputFormat = "formatted")
		{
			logger.LogTrace("{ToolName} called with parameters: entityLogicalName={EntityLogicalName}, chartName={ChartName}, outputFormat={OutputFormat}",
				nameof(GetChartList),
				entityLogicalName,
				chartName,
				outputFormat);

			try
			{
				if (string.IsNullOrWhiteSpace(entityLogicalName))
				{
					return "❌ Table name is required.";
				}

				logger.LogTrace("🔐 Authenticating to Dataverse");
				var client = await clientProvider.GetDataverseClientAsync();

				logger.LogTrace("📋 Retrieving charts for table: {EntityName}", entityLogicalName);
				var charts = await chartService.GetChartsAsync(client, entityLogicalName, chartName);

				if (charts.Count == 0)
				{
					var filter = string.IsNullOrEmpty(chartName) ? "" : $" matching '{chartName}'";
					return $"❌ No charts found for table '{entityLogicalName}'{filter}";
				}

				if ("json".Equals(outputFormat, StringComparison.OrdinalIgnoreCase))
				{
					return FormatJsonOutput(charts, entityLogicalName);
				}
				else
				{
					return FormatTextOutput(charts, entityLogicalName);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while retrieving charts: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}

		private static string FormatJsonOutput(List<SavedQueryVisualization> charts, string tableName)
		{
			var result = new
			{
				TableName = tableName,
				ChartCount = charts.Count,
				Charts = charts.Select(c => new
				{
					ChartId = c.Id,
					Name = c.Name,
					Description = c.Description,
					IsDefault = c.IsDefault,
					ChartType = c.ChartType?.ToString(),
					PrimaryEntityTypeCode = c.PrimaryEntityTypeCode
				}).ToList()
			};

			return JsonConvert.SerializeObject(result, Formatting.Indented);
		}

		private static string FormatTextOutput(List<SavedQueryVisualization> charts, string tableName)
		{
			var sb = new StringBuilder();
			sb.Append($"📊 Charts for table '{tableName}' ({charts.Count} found)").AppendLine();
			sb.AppendLine();

			foreach (var chart in charts)
			{
				sb.Append("📈 ").Append(chart.Name);
				if (chart.IsDefault == true)
				{
					sb.Append(" ⭐ (Default)");
				}
				sb.AppendLine();

				sb.Append("   ID: ").Append(chart.Id).AppendLine();
				sb.Append("   Type: ").Append(chart.ChartType?.ToString() ?? "N/A").AppendLine();

				if (!string.IsNullOrWhiteSpace(chart.Description))
				{
					sb.Append("   Description: ").Append(chart.Description).AppendLine();
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
