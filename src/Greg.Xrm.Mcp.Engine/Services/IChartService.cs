using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	/// <summary>
	/// Service for managing Dataverse chart operations
	/// </summary>
	public interface IChartService
	{
		/// <summary>
		/// Retrieves charts for a specific table
		/// </summary>
		/// <param name="client">Authenticated Dataverse client</param>
		/// <param name="entityLogicalName">Logical name of the table</param>
		/// <param name="chartName">Chart name (optional)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>List of charts found</returns>
		Task<List<SavedQueryVisualization>> GetChartsAsync(
			IOrganizationServiceAsync2 client,
			string entityLogicalName,
			string? chartName = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Retrieves a specific chart by ID
		/// </summary>
		/// <param name="client">Authenticated Dataverse client</param>
		/// <param name="chartId">Chart ID</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Chart definition</returns>
		Task<SavedQueryVisualization?> GetChartByIdAsync(
			IOrganizationServiceAsync2 client,
			Guid chartId,
			CancellationToken cancellationToken = default);
	}
}
