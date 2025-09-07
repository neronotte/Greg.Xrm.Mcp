using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using ModelContextProtocol;
using System.Text.Json;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	/// <summary>
	/// Implementazione del servizio per gestire operazioni su form Dataverse
	/// </summary>
	public class FormService(ILogger<FormService> logger) : IFormService
	{
		/// <inheritdoc />
		public async Task<List<SystemForm>> GetFormsAsync(
			IOrganizationServiceAsync2 client,
			string entityLogicalName,
			string? formName = null,
			systemform_type? formType = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				logger.LogDebug("Recupero form per tabella: {EntityName}", entityLogicalName);

				// Valida che la tabella esista
				if (!await ValidateEntityExistsAsync(client, entityLogicalName, cancellationToken))
				{
					throw new ArgumentException($"La tabella '{entityLogicalName}' non esiste nell'ambiente Dataverse");
				}

				// Costruisci query per recuperare le form
				var query = new QueryExpression("systemform")
				{
					ColumnSet = new ColumnSet(
						"formid", "name", "objecttypecode", "type", "description",
						"isdefault", "formxml"
					),
					Criteria = new FilterExpression(LogicalOperator.And)
				};

				// Filtro per tabella
				query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, entityLogicalName);

				// Filtro per nome form se specificato
				if (!string.IsNullOrEmpty(formName))
				{
					query.Criteria.AddCondition("name", ConditionOperator.Like, $"%{formName}%");
				}

				// Filtro per tipo form se specificato
				if (formType.HasValue)
				{
					query.Criteria.AddCondition("type", ConditionOperator.Equal, (int)formType.Value);
				}

				// Ordina per tipo e ordine form
				query.AddOrder("type", OrderType.Ascending);

				logger.LogDebug("Esecuzione query per recupero form: {Query}", JsonSerializer.Serialize(query));


				var result = await client.RetrieveMultipleAsync(query);

				logger.LogDebug("Trovate {Count} form per {EntityName}", result.Entities.Count, entityLogicalName);

				var forms = new List<SystemForm>();

				foreach (var entity in result.Entities)
				{
					try
					{
						var form = entity.ToEntity<SystemForm>();
						forms.Add(form);
					}
					catch (Exception ex)
					{
						logger.LogWarning(ex, "Errore durante l'analisi della form {FormId}", entity.Id);
						// Continua con le altre form anche se una non può essere analizzata
					}
				}

				return forms;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Errore durante il recupero delle form per {EntityName}", entityLogicalName);
				throw new McpException(ex.Message, ex);
			}
		}




		/// <inheritdoc />
		public async Task<SystemForm?> GetFormByIdAsync(
			IOrganizationServiceAsync2 client,
			Guid formId,
			CancellationToken cancellationToken = default)
		{
			try
			{
				logger.LogDebug("Recupero form con ID: {FormId}", formId);

				var entity = await client.RetrieveAsync("systemform", formId, new ColumnSet(
					"formid", "name", "objecttypecode", "type", "description",
					"isdefault", "formxml"
				));

				if (entity == null)
				{
					logger.LogWarning("Form non trovata: {FormId}", formId);
					return null;
				}

				var form = entity.ToEntity<SystemForm>();

				logger.LogDebug("Form recuperata: {FormName} ({FormType})", form.Name, form.Type);
				return form;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Errore durante il recupero della form {FormId}", formId);
				throw new McpException($"Errore durante il recupero della form: {ex.Message}", ex);
			}
		}



		/// <inheritdoc />
		public async Task<bool> ValidateEntityExistsAsync(
			IOrganizationServiceAsync2 client,
			string entityLogicalName,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var query = new QueryExpression("entity")
				{
					ColumnSet = new ColumnSet("logicalname"),
					Criteria = new FilterExpression()
				};
				query.Criteria.AddCondition("logicalname", ConditionOperator.Equal, entityLogicalName);
				query.TopCount = 1;

				var result = await client.RetrieveMultipleAsync(query);
				return result.Entities.Count > 0;
			}
			catch
			{
				// Se la query fallisce, prova un approccio alternativo
				try
				{
					// Prova a recuperare i metadati della tabella
					var request = new Microsoft.Xrm.Sdk.Messages.RetrieveEntityRequest
					{
						LogicalName = entityLogicalName,
						EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity
					};

					await client.ExecuteAsync(request);
					return true;
				}
				catch
				{
					return false;
				}
			}
		}
	}
}
