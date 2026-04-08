using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.Charts
{
	[McpServerToolType]
	public class CreateChart(
		ILogger<CreateChart> logger,
		IDataverseClientProvider clientProvider)
	{
		[McpServerTool(
			Name = "dataverse_chart_create",
			Destructive = false,
			Idempotent = false,
			ReadOnly = false),
		Description(
@"Creates a new chart (visualization) for a given Dataverse table.
The chart definition requires two XML components:
1. DataDescription XML: Defines the data query and aggregations using FetchXML syntax
2. PresentationDescription XML: Defines the visual presentation (chart type, series, categories, etc.)
You can refer to Microsoft documentation for chart XML structure: https://learn.microsoft.com/en-us/dynamics365/customerengagement/on-premises/developer/customize-dev/view-data-with-visualizations-charts")]
		public async Task<string> Execute(
			IMcpServer mcpServer,
			IPublishXmlBuilder publishXmlBuilder,
			[Description("The schema name of the table for which the chart is created. Format using 'A Capital Letter For Each Word' unless explicitly stated otherwise")] string tableName,
			[Description("The name of the chart to create")] string chartName,
			[Description("The DataDescription XML that defines the data query using FetchXML with aggregations")] string dataDescriptionXml,
			[Description("The PresentationDescription XML that defines the visual appearance (chart type, colors, layout, etc.)")] string presentationDescriptionXml,
			[Description("Optional description of the chart")] string? description = null,
			[Description("Whether this should be the default chart for the table (default: false)")] bool isDefault = false)
		{
			logger.LogTrace("{ToolName} called with parameters: TableName={TableName}, ChartName={ChartName}",
				nameof(CreateChart),
				tableName,
				chartName);

			try
			{
				if (string.IsNullOrWhiteSpace(tableName))
				{
					return "❌ Table name must be provided.";
				}

				if (string.IsNullOrWhiteSpace(chartName))
				{
					return "❌ Chart name must be provided.";
				}

				if (string.IsNullOrWhiteSpace(dataDescriptionXml))
				{
					return "❌ DataDescription XML must be provided.";
				}

				if (string.IsNullOrWhiteSpace(presentationDescriptionXml))
				{
					return "❌ PresentationDescription XML must be provided.";
				}

				logger.LogTrace("🔐 Authenticating to Dataverse");
				var client = await clientProvider.GetDataverseClientAsync();

				var context = new DataverseContext(client);

				// Check if a chart with the same name already exists
				var existing = context.SavedQueryVisualizationSet
					.Where(c => c.Name == chartName && c.PrimaryEntityTypeCode == tableName)
					.Select(c => c.Id)
					.FirstOrDefault();

				if (existing != Guid.Empty)
				{
					return $"❌ Error: A chart with the same name already exists (chartid: {existing}) on the same table.";
				}

				var token = await mcpServer.NotifyProgressAsync(nameof(CreateChart), "Creating chart...");

				dataDescriptionXml = dataDescriptionXml.RemoveXmlDeclaration()!;
				presentationDescriptionXml = presentationDescriptionXml.RemoveXmlDeclaration()!;

				var chart = new SavedQueryVisualization
				{
					Name = chartName,
					PrimaryEntityTypeCode = tableName,
					DataDescription = dataDescriptionXml,
					PresentationDescription = presentationDescriptionXml,
					Description = description ?? $"Chart created using MCP Server",
					IsDefault = isDefault
				};

				chart.Id = await client.CreateAsync(chart);

				await mcpServer.NotifyProgressAsync(token, "Chart created, now publishing table...");

				logger.LogTrace("Publishing table...");
				publishXmlBuilder.AddTable(tableName);
				var request = publishXmlBuilder.Build();

				await client.ExecuteAsync(request);
				logger.LogTrace("Publishing table...COMPLETED");

				await mcpServer.NotifyProgressAsync(token, "Completed!", 100);

				logger.LogTrace("✅ Chart created successfully: {ChartId}", chart.Id);
				return $"✅ Chart '{chartName}' created for table '{tableName}': chartid: {chart.Id}";
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while creating chart: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
