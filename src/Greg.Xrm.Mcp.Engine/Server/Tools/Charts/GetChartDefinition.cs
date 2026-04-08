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
	public class GetChartDefinition(
		ILogger<GetChartDefinition> logger,
		IDataverseClientProvider clientProvider,
		IChartService chartService)
	{
		[McpServerTool(
			Name = "dataverse_chart_read",
			Destructive = false,
			ReadOnly = true,
			Idempotent = true),
		Description("Retrieves the complete definition of a Dataverse chart including DataDescription XML (defines the data query) and PresentationDescription XML (defines the visual appearance).")]
		public async Task<string> Execute(
			[Description("The unique identifier (guid) of the chart to retrieve")] Guid chartId,
			[Description("Output format: 'xml' for human-readable format, 'json' for structured format (default: xml)")] string outputFormat = "xml")
		{
			logger.LogTrace("{ToolName} called with parameters: ChartId={ChartId}, OutputFormat={OutputFormat}",
				nameof(GetChartDefinition),
				chartId,
				outputFormat);

			try
			{
				logger.LogTrace("🔐 Authenticating to Dataverse");
				var client = await clientProvider.GetDataverseClientAsync();

				logger.LogTrace("🔍 Retrieving chart with ID: {ChartId}", chartId);
				var chart = await chartService.GetChartByIdAsync(client, chartId);

				if (chart == null)
				{
					logger.LogTrace("Chart with ID '{ChartId}' not found.", chartId);
					return $"❌ Chart with ID '{chartId}' not found.";
				}

				if ("json".Equals(outputFormat, StringComparison.OrdinalIgnoreCase))
				{
					return FormatJsonOutput(chart);
				}
				else
				{
					return FormatXmlOutput(chart);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while retrieving chart: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}

		private static string FormatJsonOutput(SavedQueryVisualization chart)
		{
			var result = new
			{
				ChartId = chart.Id,
				Name = chart.Name,
				Description = chart.Description,
				PrimaryEntityTypeCode = chart.PrimaryEntityTypeCode,
				IsDefault = chart.IsDefault,
				ChartType = chart.ChartType?.ToString(),
				DataDescription = chart.DataDescription,
				PresentationDescription = chart.PresentationDescription
			};

			return JsonConvert.SerializeObject(result, Formatting.Indented);
		}

		private static string FormatXmlOutput(SavedQueryVisualization chart)
		{
			var sb = new StringBuilder();
			sb.Append("📊 Chart Details:").AppendLine();
			sb.Append("- ID: ").Append(chart.Id).AppendLine();
			sb.Append("- Name: ").Append(chart.Name).AppendLine();
			sb.Append("- Table: ").Append(chart.PrimaryEntityTypeCode).AppendLine();
			sb.Append("- Type: ").Append(chart.ChartType?.ToString() ?? "N/A").AppendLine();
			sb.Append("- Is Default: ").Append(chart.IsDefault).AppendLine();

			if (!string.IsNullOrWhiteSpace(chart.Description))
			{
				sb.Append("- Description: ").Append(chart.Description).AppendLine();
			}

			sb.AppendLine();
			sb.Append("📈 Data Description XML (defines the data query):").AppendLine();
			sb.Append("```xml").AppendLine();
			sb.Append(chart.DataDescription ?? "(empty)").AppendLine();
			sb.Append("```").AppendLine();
			sb.AppendLine();

			sb.Append("🎨 Presentation Description XML (defines the visual appearance):").AppendLine();
			sb.Append("```xml").AppendLine();
			sb.Append(chart.PresentationDescription ?? "(empty)").AppendLine();
			sb.Append("```").AppendLine();

			return sb.ToString();
		}
	}
}
