using Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Services
{
    /// <summary>
    /// Service for retrieving metadata from Dataverse
    /// </summary>
    public class DataverseMetadataService
    {
        private readonly ILogger<DataverseMetadataService> _logger;

        public DataverseMetadataService(ILogger<DataverseMetadataService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all tables that have forms
        /// </summary>
        /// <param name="serviceClient">Connected Dataverse client</param>
        /// <returns>List of table information</returns>
        public async Task<List<TableInfo>> RetrieveAllTablesAsync(ServiceClient serviceClient)
        {
            _logger.LogInformation("Retrieving tables with forms...");
            
            try
            {
                var retrieveAllEntitiesRequest = new RetrieveAllEntitiesRequest()
                {
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = false
                };

                var response = (RetrieveAllEntitiesResponse)await serviceClient.ExecuteAsync(retrieveAllEntitiesRequest);
                
                var tablesWithForms = response.EntityMetadata
                    .Where(entity => !string.IsNullOrEmpty(entity.LogicalName) && 
                                   !string.IsNullOrEmpty(entity.DisplayName?.UserLocalizedLabel?.Label) &&
                                   entity.ObjectTypeCode.HasValue && 
                                   !entity.IsIntersect.GetValueOrDefault() &&
                                   (entity.IsCustomizable?.Value ?? false) && 
                                   !entity.IsLogicalEntity.GetValueOrDefault() && 
                                   !entity.IsPrivate.GetValueOrDefault())
                    .Select(entity => new TableInfo
                    {
                        LogicalName = entity.LogicalName,
                        DisplayName = entity.DisplayName?.UserLocalizedLabel?.Label ?? entity.LogicalName,
                        ObjectTypeCode = entity.ObjectTypeCode!.Value
                    })
                    .OrderBy(t => t.DisplayName)
                    .ToList();

                _logger.LogInformation("Found {Count} tables with forms", tablesWithForms.Count);
                Console.WriteLine($"Found {tablesWithForms.Count} tables with forms");
                
                return tablesWithForms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tables");
                throw new InvalidOperationException($"Failed to retrieve tables from Dataverse: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves all forms for a specific table
        /// </summary>
        /// <param name="serviceClient">Connected Dataverse client</param>
        /// <param name="tableInfo">Table information</param>
        /// <returns>List of form information</returns>
        public async Task<List<FormInfo>> RetrieveFormsForTableAsync(ServiceClient serviceClient, TableInfo tableInfo)
        {
            try
            {
                var query = new QueryExpression("systemform")
                {
                    ColumnSet = new ColumnSet("formid", "name", "type", "formxml", "description"),
                    Criteria = new FilterExpression(LogicalOperator.And)
                };
                
                // Filter for the specific table
                query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, tableInfo.ObjectTypeCode);
                
                // Filter for active forms only
                query.Criteria.AddCondition("formactivationstate", ConditionOperator.Equal, 1);
                
                // Order by form type and name
                query.AddOrder("type", OrderType.Ascending);
                query.AddOrder("name", OrderType.Ascending);

                var result = await serviceClient.RetrieveMultipleAsync(query);
                
                var forms = result.Entities.Select(entity => new FormInfo
                {
                    FormId = entity.Id,
                    Name = entity.GetAttributeValue<string>("name") ?? "Unknown",
                    FormTypeCode = entity.GetAttributeValue<OptionSetValue>("type")?.Value ?? 0,
                    FormType = entity.FormattedValues["type"].ToString(),
                    FormXml = entity.GetAttributeValue<string>("formxml") ?? string.Empty
                })
                .Where(f => !string.IsNullOrEmpty(f.FormXml)) // Skip forms without XML
                .ToList();

                _logger.LogDebug("Retrieved {Count} forms for table {TableName}", forms.Count, tableInfo.LogicalName);
                
                return forms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forms for table {TableName}", tableInfo.LogicalName);
                throw new InvalidOperationException($"Failed to retrieve forms for table {tableInfo.LogicalName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts form type code to display name
        /// </summary>
        /// <param name="formTypeValue">Form type code from Dataverse</param>
        /// <returns>Human-readable form type name</returns>
        public static string GetFormTypeDisplayName(int formTypeValue)
        {
            return formTypeValue switch
            {
                1 => "Create",
                2 => "Update",
                6 => "Quick View",
                7 => "Quick Create",
                8 => "Dialog",
                9 => "Task Flow",
                10 => "Interactive experience",
                11 => "Card",
                12 => "Main - Interactive experience",
                _ => $"Unknown ({formTypeValue})"
            };
        }
    }
}