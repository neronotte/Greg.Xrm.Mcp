using Greg.Xrm.Mcp.Core.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Messages;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.FormEngineer.Server.Tools
{
	[McpServerToolType]
	public class DataverseMetadataTools
	{
		protected DataverseMetadataTools() { }

		[McpServerTool,
		Description("Retrieves the list of tables from the Dataverse environment. For each table, returns schema name and display name"),]
		public static async Task<string> RetrieveDataverseTables(
			ILogger<DataverseMetadataTools> logger,
			IDataverseClientProvider clientProvider)
		{
			logger.LogInformation("Starting to retrieve Dataverse tables...");

			try
			{
				var client = await clientProvider.GetDataverseClientAsync();


				var tables = (RetrieveAllEntitiesResponse)await client.ExecuteAsync(new RetrieveAllEntitiesRequest { EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity });

				var result = tables.EntityMetadata
				.Where(e => !e.IsIntersect.GetValueOrDefault())
				.Select(e => new
				{
					e.SchemaName,
					DisplayName = e.DisplayName?.UserLocalizedLabel?.Label ?? "No Display Name"
				})
				.OrderBy(e => e.SchemaName)
				.ToList();

				logger.LogInformation("Retrieved {Count} tables from Dataverse.", result.Count);

				var tablesText = JsonConvert.SerializeObject(result, Formatting.Indented);
				return tablesText;
			}
			catch(Exception ex)
			{
				logger.LogError(ex, "An error occurred while retrieving Dataverse tables.");
				return $"Error: {ex.Message}";
			}
		}


		[McpServerTool,
		Description("Retrieves the list of columns of a given Dataverse table. For each columns, returns schema name, display name and type."),]
		public static async Task<string> RetrieveDataverseTableColumns(
			ILogger<DataverseMetadataTools> logger,
			IDataverseClientProvider clientProvider,
			[Description("The schema name of the table")] string tableName)
		{
			if (string.IsNullOrWhiteSpace(tableName))
			{
				logger.LogError("Table name cannot be null or empty.");
				return "Error: Table name cannot be null or empty.";
			}


			logger.LogInformation("Starting to retrieve Dataverse columns for table {TableName}...", tableName);

			try
			{
				var client = await clientProvider.GetDataverseClientAsync();


				var tables = (RetrieveEntityResponse)await client.ExecuteAsync(new RetrieveEntityRequest { 
					EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Attributes, 
					LogicalName = tableName 
				});

				var result = tables.EntityMetadata.Attributes
				.Where(e => 
					e.AttributeType != Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Virtual 
					&& e.AttributeType != Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.Uniqueidentifier
					&& e.AttributeType != Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.ManagedProperty
					&& e.AttributeType != Microsoft.Xrm.Sdk.Metadata.AttributeTypeCode.CalendarRules
					&& e.AttributeOf == null)
				.Select(e => new
				{
					e.SchemaName,
					DisplayName = e.DisplayName?.UserLocalizedLabel?.Label ?? "No Display Name",
					Type = e.AttributeType.ToString()
				})
				.OrderBy(e => e.SchemaName)
				.ToList();

				logger.LogInformation("Retrieved {Count} columns from Dataverse.", result.Count);

				var tablesText = JsonConvert.SerializeObject(result, Formatting.Indented);
				return tablesText;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred while retrieving Dataverse tables.");
				return $"Error: {ex.Message}";
			}
		}
	}
}
