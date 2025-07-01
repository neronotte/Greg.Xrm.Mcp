using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Utils;
using ModelContextProtocol;

namespace Greg.Xrm.Mcp.Core.Authentication
{
	public class DataverseClientProvider(ILogger<DataverseClientProvider> log, Arguments arguments) : IDataverseClientProvider
	{
		public async Task<IOrganizationServiceAsync2> GetDataverseClientAsync()
		{
			var dataverseUrl = arguments.DataverseUrl?.TrimEnd(" /".ToCharArray());
			if (string.IsNullOrEmpty(dataverseUrl))
			{
				throw new ArgumentException("Dataverse URL is not provided or is invalid.");
			}

			ServiceClient crm;

			try
			{
				var accessToken = await TokenCache.TryGetAccessTokenAsync(dataverseUrl);
				if (accessToken == null)
				{
					crm = await CreateServiceClientFromConnectionString(dataverseUrl);
				}
				else
				{
					crm = new ServiceClient(accessToken.ServiceUri, uri => Task.FromResult(accessToken.AccessToken));
					if (!crm.IsReady)
					{
						crm = await CreateServiceClientFromConnectionString(dataverseUrl);
					}

				}

				log.LogInformation("Connection to Dataverse established successfully: {DataverseUrl}", dataverseUrl);


				try
				{
					var response = (WhoAmIResponse)await crm.ExecuteAsync(new WhoAmIRequest());
					log.LogInformation("User authenticated successfully with Dataverse: {UserId}", response.UserId);
				}
				catch (DataverseConnectionException)
				{
					return await CreateServiceClientFromConnectionString(dataverseUrl);
				}
				catch (System.ServiceModel.Security.MessageSecurityException)
				{
					return await CreateServiceClientFromConnectionString(dataverseUrl);
				}

				return crm;
			}
			catch (McpException ex)
			{
				log.LogError(ex, "Error creating Dataverse client: {Message}", ex.Message);
				throw;
			}
			catch (Exception ex)
			{
				log.LogError(ex, "Error creating Dataverse client: {Message}", ex.Message);
				throw new McpException($"Error creating Dataverse client: {ex.Message}", ex);
			}
		}

		private static async Task<ServiceClient> CreateServiceClientFromConnectionString(string dataverseUrl)
		{
			var connectionString = $"AuthType=OAuth;Url={dataverseUrl};RedirectUri=http://localhost;LoginPrompt=Auto";
			var crm = new ServiceClient(connectionString);
			if (!crm.IsReady)
			{
				await TokenCache.ClearAccessTokenAsync(dataverseUrl);
				throw new McpException($"Failed to connect to Dataverse at {dataverseUrl}. Error: {crm.LastError}");
			}

			await TokenCache.SaveAccessTokenAsync(dataverseUrl, crm.ConnectedOrgUriActual, crm.CurrentAccessToken);
			return crm;
		}
	}
}
