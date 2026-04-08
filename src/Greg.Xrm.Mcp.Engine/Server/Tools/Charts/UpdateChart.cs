using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.Charts
{
	[McpServerToolType]
	public class UpdateChart(
		ILogger<UpdateChart> logger,
		IDataverseClientProvider clientProvider,
		IChartService chartService,
		IPublishXmlBuilder publishXmlBuilder)
	{
		[McpServerTool(
			Name = "dataverse_chart_update",
			Destructive = true,
			ReadOnly = false,
			Idempotent = true),
		Description(
@"Updates an existing Dataverse chart definition with new DataDescription XML and/or PresentationDescription XML, then publishes the changes.
DataDescription XML defines the data query (FetchXML with aggregations).
PresentationDescription XML defines the visual appearance (chart type, colors, layout, etc.).")]
		public async Task<string> Execute(
			[Description("The unique identifier (guid) of the chart to update")] Guid chartId,
			[Description("The new DataDescription XML (data query definition)")] string? dataDescriptionXml = null,
			[Description("The new PresentationDescription XML (visual presentation definition)")] string? presentationDescriptionXml = null,
			[Description("The new name for the chart")] string? chartName = null,
			[Description("The new description for the chart")] string? description = null)
		{
			logger.LogTrace("{ToolName} called with parameters: ChartId={ChartId}",
				nameof(UpdateChart),
				chartId);

			try
			{
				if (string.IsNullOrWhiteSpace(dataDescriptionXml) &&
					string.IsNullOrWhiteSpace(presentationDescriptionXml) &&
					string.IsNullOrWhiteSpace(chartName) &&
					string.IsNullOrWhiteSpace(description))
				{
					return "❌ At least one field must be provided to update (dataDescriptionXml, presentationDescriptionXml, chartName, or description).";
				}

				logger.LogTrace("🔐 Authenticating to Dataverse");
				var client = await clientProvider.GetDataverseClientAsync();

				var chart = await chartService.GetChartByIdAsync(client, chartId);
				if (chart == null)
				{
					logger.LogError("❌ No chart found with ID: {ChartId}", chartId);
					return "❌ No chart found with ID: " + chartId;
				}

				var tableName = chart.PrimaryEntityTypeCode;

				// Update fields if provided
				if (!string.IsNullOrWhiteSpace(dataDescriptionXml))
				{
					chart.DataDescription = dataDescriptionXml.RemoveXmlDeclaration();
				}

				if (!string.IsNullOrWhiteSpace(presentationDescriptionXml))
				{
					chart.PresentationDescription = presentationDescriptionXml.RemoveXmlDeclaration();
				}

				if (!string.IsNullOrWhiteSpace(chartName))
				{
					chart.Name = chartName;
				}

				if (!string.IsNullOrWhiteSpace(description))
				{
					chart.Description = description;
				}

				logger.LogTrace("🔄 Updating chart with ID: {ChartId}", chartId);
				await client.UpdateAsync(chart);

				logger.LogTrace("Publishing table...");
				publishXmlBuilder.AddTable(tableName);
				var request = publishXmlBuilder.Build();

				await client.ExecuteAsync(request);
				logger.LogTrace("Publishing table...COMPLETED");

				logger.LogTrace("✅ Chart updated successfully: {ChartId}", chartId);
				return $"✅ Chart updated successfully: {chartId}";
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error during chart update: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
