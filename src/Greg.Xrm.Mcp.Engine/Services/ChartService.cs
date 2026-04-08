using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using ModelContextProtocol;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	/// <summary>
	/// Implementation of chart service for managing Dataverse chart operations
	/// </summary>
	public class ChartService(ILogger<ChartService> logger) : IChartService
	{
		/// <inheritdoc />
		public async Task<List<SavedQueryVisualization>> GetChartsAsync(
			IOrganizationServiceAsync2 client,
			string entityLogicalName,
			string? chartName = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				logger.LogDebug("Retrieving charts for table: {EntityName}", entityLogicalName);

				// Build query to retrieve charts
				var query = new QueryExpression("savedqueryvisualization")
				{
					ColumnSet = new ColumnSet(
						"savedqueryvisualizationid", "name", "primaryentitytypecode",
						"description", "isdefault", "charttype",
						"datadescription", "presentationdescription"
					),
					Criteria = new FilterExpression(LogicalOperator.And)
				};

				// Filter by table
				query.Criteria.AddCondition("primaryentitytypecode", ConditionOperator.Equal, entityLogicalName);

				// Filter by chart name if specified
				if (!string.IsNullOrEmpty(chartName))
				{
					query.Criteria.AddCondition("name", ConditionOperator.Like, $"%{chartName}%");
				}

				// Order by name
				query.AddOrder("name", OrderType.Ascending);

				logger.LogDebug("Executing query to retrieve charts");

				var result = await client.RetrieveMultipleAsync(query);

				logger.LogDebug("Found {Count} charts for {EntityName}", result.Entities.Count, entityLogicalName);

				var charts = new List<SavedQueryVisualization>();

				foreach (var entity in result.Entities)
				{
					try
					{
						var chart = entity.ToEntity<SavedQueryVisualization>();
						charts.Add(chart);
					}
					catch (Exception ex)
					{
						logger.LogWarning(ex, "Error parsing chart {ChartId}", entity.Id);
						// Continue with other charts even if one can't be parsed
					}
				}

				return charts;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error retrieving charts for {EntityName}", entityLogicalName);
				throw new McpException(ex.Message, ex);
			}
		}

		/// <inheritdoc />
		public async Task<SavedQueryVisualization?> GetChartByIdAsync(
			IOrganizationServiceAsync2 client,
			Guid chartId,
			CancellationToken cancellationToken = default)
		{
			try
			{
				logger.LogDebug("Retrieving chart with ID: {ChartId}", chartId);

				var entity = await client.RetrieveAsync("savedqueryvisualization", chartId, new ColumnSet(
					"savedqueryvisualizationid", "name", "primaryentitytypecode",
					"description", "isdefault", "charttype",
					"datadescription", "presentationdescription"
				));

				if (entity == null)
				{
					logger.LogWarning("Chart not found: {ChartId}", chartId);
					return null;
				}

				var chart = entity.ToEntity<SavedQueryVisualization>();

				logger.LogDebug("Chart retrieved: {ChartName}", chart.Name);
				return chart;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error retrieving chart {ChartId}", chartId);
				throw new McpException($"Error retrieving chart: {ex.Message}", ex);
			}
		}
	}
}
