using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using System.Text.Json;
using System.Xml.Linq;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	/// <summary>
	/// Servizio per gestire operazioni su form Dataverse
	/// </summary>
	public interface IFormService
	{
		/// <summary>
		/// Recupera la definizione di una form da Dataverse
		/// </summary>
		/// <param name="client">Client autenticato a Dataverse</param>
		/// <param name="entityLogicalName">Nome logico della tabella</param>
		/// <param name="formName">Nome della form (opzionale)</param>
		/// <param name="formType">Tipo di form (opzionale)</param>
		/// <param name="cancellationToken">Token di cancellazione</param>
		/// <returns>Lista delle form trovate</returns>
		Task<List<SystemForm>> GetFormsAsync(
			IOrganizationServiceAsync2 client,
			string entityLogicalName,
			string? formName = null,
			systemform_type? formType = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Recupera una form specifica per ID
		/// </summary>
		/// <param name="client">Client autenticato a Dataverse</param>
		/// <param name="formId">ID della form</param>
		/// <param name="cancellationToken">Token di cancellazione</param>
		/// <returns>Definizione della form</returns>
		Task<SystemForm?> GetFormByIdAsync(
			IOrganizationServiceAsync2 client,
			Guid formId,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Valida che una tabella esista nell'ambiente Dataverse
		/// </summary>
		/// <param name="client">Client autenticato a Dataverse</param>
		/// <param name="entityLogicalName">Nome logico della tabella da validare</param>
		/// <param name="cancellationToken">Token di cancellazione</param>
		/// <returns>True se la tabella esiste</returns>
		Task<bool> ValidateEntityExistsAsync(
			IOrganizationServiceAsync2 client,
			string entityLogicalName,
			CancellationToken cancellationToken = default);
	}
}
