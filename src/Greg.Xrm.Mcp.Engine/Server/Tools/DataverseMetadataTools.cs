using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Messages;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools
{
	[McpServerToolType]
	public class DataverseMetadataTools
	{
		protected DataverseMetadataTools()
		{
			// This constructor is intentionally empty.
			// It is used to ensure that the class can be instantiated without any parameters.
		}


		[McpServerTool(
			Name = "dataverse_metadata_list_tables",
			Idempotent = true,
			Destructive = false,
			ReadOnly = true
		),
		Description(
"Retrieves the list of tables from the Dataverse environment. For each table, returns schema name and display name"),]
		public static async Task<string> RetrieveDataverseTables(
			ILogger<DataverseMetadataTools> logger,
			IDataverseClientProvider clientProvider)
		{
			logger.LogTrace("{ToolName} called.",
				   nameof(RetrieveDataverseTables));

			try
			{
				var client = await clientProvider.GetDataverseClientAsync();


				var tables = (RetrieveAllEntitiesResponse)await client.ExecuteAsync(new RetrieveAllEntitiesRequest { EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity });

				var result = tables.EntityMetadata
				.Where(e => !e.IsIntersect.GetValueOrDefault())
				.Select(e => new
				{
					Id = e.MetadataId,
					e.SchemaName,
					e.LogicalName,
					DisplayName = e.DisplayName?.UserLocalizedLabel?.Label ?? "No Display Name"
				})
				.OrderBy(e => e.SchemaName)
				.ToList();

				logger.LogTrace("Retrieved {Count} tables from Dataverse.", result.Count);

				var tablesText = JsonConvert.SerializeObject(result, Formatting.Indented);

				logger.LogTrace("{Result}", tablesText);

				return tablesText;
			}
			catch(Exception ex)
			{
				logger.LogError(ex, "An error occurred while retrieving Dataverse tables.");
				return $"Error: {ex.Message}";
			}
		}


		[McpServerTool(
			Name = "dataverse_metadata_list_table_columns",
			ReadOnly =true,
			Idempotent = true,
			Destructive = false
		),
		Description(
"Retrieves the list of columns of a given Dataverse table. For each columns, returns schema name, display name and type."),]
		public static async Task<string> RetrieveDataverseTableColumns(
			ILogger<DataverseMetadataTools> logger,
			IDataverseClientProvider clientProvider,
			[Description("The schema name of the table")] string tableName)
		{
			logger.LogTrace("{ToolName} called with parameters: TableName={TableName}",
				   nameof(RetrieveDataverseTableColumns),
				   tableName);


			if (string.IsNullOrWhiteSpace(tableName))
			{
				logger.LogError("Table name cannot be null or empty.");
				return "Error: Table name cannot be null or empty.";
			}


			logger.LogTrace("Starting to retrieve Dataverse columns for table {TableName}...", tableName);

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

				logger.LogTrace("Retrieved {Count} columns from Dataverse.", result.Count);

				var tablesText = JsonConvert.SerializeObject(result, Formatting.Indented);

				logger.LogTrace("{Result}", tablesText);
				return tablesText;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred while retrieving Dataverse tables.");
				return $"Error: {ex.Message}";
			}
		}




		[McpServerTool(
			Name = "dataverse_metadata_list_webresources_html",
			ReadOnly = true,
			Idempotent = true,
			Destructive = false
		),
		Description(
"Retrieves the list of HTML Web Resources available in Dataverse. HTML Web Resources are the only type of webresource that we can put in the sitemap as SubArea."),]
		public static async Task<string> RetrieveDataverseHtmlWebResources(
			ILogger<DataverseMetadataTools> logger,
			IDataverseClientProvider clientProvider)
		{
			logger.LogTrace("{ToolName} called.",
				   nameof(RetrieveDataverseHtmlWebResources));



			try
			{
				using var context = await clientProvider.GetDataverseContextAsync();

				var items = context.WebResourceSet
					.Where(wr => wr.WebResourceType == webresource_webresourcetype.Webpage_HTML) // 1 = HTML
					.Select(wr => new
					{
						wr.Name,
						wr.DisplayName,
						wr.WebResourceId
					})
					.OrderBy(wr => wr.Name)
					.ToList();

				var tablesText = JsonConvert.SerializeObject(items, Formatting.Indented);
				return tablesText;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred while retrieving Dataverse webresources.");
				return $"Error: {ex.Message}";
			}
		}
	}
}
