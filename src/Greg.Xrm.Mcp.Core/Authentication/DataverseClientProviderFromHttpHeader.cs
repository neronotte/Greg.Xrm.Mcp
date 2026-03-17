using Microsoft.AspNetCore.Http;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Utils;
using ModelContextProtocol;

namespace Greg.Xrm.Mcp.Core.Authentication
{
	public class DataverseClientProviderFromHttpHeader(ILogger<DataverseClientProviderFromArguments> log, IHttpContextAccessor httpContextAccessor) : IDataverseClientProvider
	{
		public async Task<IOrganizationServiceAsync2> GetDataverseClientAsync()
		{
			var context = httpContextAccessor.HttpContext;
			if (context == null)
			{
				throw new InvalidOperationException("HTTP context is not available.");
			}

			if (!context.Request.Headers.TryGetValue("X-Dataverse-Url", out var dataverseUrlHeader))
			{
				throw new ArgumentException("X-Dataverse-Url header not specified.");
			}
			if (dataverseUrlHeader.Count == 0)
			{
				throw new ArgumentException("X-Dataverse-Url header value is not provided.");
			}
			var dataverseUrl = dataverseUrlHeader[0]?.TrimEnd(" /".ToCharArray());
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
					crm = await CreateServiceClientAsync(dataverseUrl);
				}
				else
				{
					crm = new ServiceClient(accessToken.ServiceUri, uri => Task.FromResult(accessToken.AccessToken));
					if (!crm.IsReady)
					{
						crm = await CreateServiceClientAsync(dataverseUrl);
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
					return await CreateServiceClientAsync(dataverseUrl);
				}
				catch (System.ServiceModel.Security.MessageSecurityException)
				{
					return await CreateServiceClientAsync(dataverseUrl);
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

		private static async Task<ServiceClient> CreateServiceClientAsync(string dataverseUrl)
		{
			// Try device-code flow first to get a fresh access token
			string accessToken;
			try
			{
				accessToken = await DeviceCodeAuthProvider.AcquireTokenAsync(dataverseUrl);
			}
			catch (Exception ex)
			{
				// Fallback: try OAuth loopback (works if a browser is available)
				Console.Error.WriteLine($"[Greg.Xrm.Mcp] device-code failed ({ex.Message}), falling back to OAuth loopback...");
				return await CreateServiceClientFromConnectionStringFallback(dataverseUrl);
			}

			var serviceUri = new Uri(dataverseUrl.TrimEnd('/'));
			var crm = new ServiceClient(serviceUri, uri => Task.FromResult(accessToken));
			if (!crm.IsReady)
			{
				await TokenCache.ClearAccessTokenAsync(dataverseUrl);
				throw new McpException($"Failed to connect to Dataverse at {dataverseUrl} after device-code auth. Error: {crm.LastError}");
			}

			await TokenCache.SaveAccessTokenAsync(dataverseUrl, crm.ConnectedOrgUriActual, accessToken);
			return crm;
		}

		private static async Task<ServiceClient> CreateServiceClientFromConnectionStringFallback(string dataverseUrl)
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
