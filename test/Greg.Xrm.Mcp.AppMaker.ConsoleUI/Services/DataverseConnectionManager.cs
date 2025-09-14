using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Services
{
    /// <summary>
    /// Manages connection to Dataverse using user credentials
    /// </summary>
    public class DataverseConnectionManager
    {
        private readonly ILogger<DataverseConnectionManager> _logger;
        private const string CONNECTION_STRING_TEMPLATE = "AuthType=OAuth;Url={0};RedirectUri=http://localhost;LoginPrompt=Auto";

        public DataverseConnectionManager(ILogger<DataverseConnectionManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Establishes a connection to Dataverse
        /// </summary>
        /// <returns>Connected ServiceClient instance</returns>
        public async Task<ServiceClient> EstablishConnectionAsync()
        {
            var dataverseUrl = GetDataverseUrlFromUserOrEnvironment();
            
            _logger.LogInformation("?? Connecting to Dataverse: {DataverseUrl}", dataverseUrl);
            Console.WriteLine($"?? You will be prompted for authentication credentials...");

            try
            {
                var connectionString = string.Format(CONNECTION_STRING_TEMPLATE, dataverseUrl);
                var serviceClient = new ServiceClient(connectionString);

                if (!serviceClient.IsReady)
                {
                    throw new InvalidOperationException($"Failed to connect to Dataverse: {serviceClient.LastError}");
                }

                await ValidateConnectionAsync(serviceClient);
                
                _logger.LogInformation("? Successfully connected to Dataverse");
                Console.WriteLine($"? Connected successfully as user: {serviceClient.OAuthUserId}");
                Console.WriteLine($"?? Organization: {serviceClient.ConnectedOrgFriendlyName}");
                
                return serviceClient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Failed to connect to Dataverse");
                Console.WriteLine($"? Connection failed: {ex.Message}");
                Console.WriteLine("?? Please ensure:");
                Console.WriteLine("   - You have valid credentials for the Dataverse environment");
                Console.WriteLine("   - The Dataverse URL is correct and accessible");
                Console.WriteLine("   - Your user account has appropriate permissions");
                throw;
            }
        }

        /// <summary>
        /// Validates the connection by performing a WhoAmI request
        /// </summary>
        private async Task ValidateConnectionAsync(ServiceClient serviceClient)
        {
            var whoAmI = new Microsoft.Crm.Sdk.Messages.WhoAmIRequest();
            var response = (Microsoft.Crm.Sdk.Messages.WhoAmIResponse)await serviceClient.ExecuteAsync(whoAmI);
            _logger.LogDebug("User ID: {UserId}", response.UserId);
        }

        /// <summary>
        /// Gets the Dataverse URL from environment variable or prompts user
        /// </summary>
        private string GetDataverseUrlFromUserOrEnvironment()
        {
            // Try to get from environment variable first
            var envUrl = Environment.GetEnvironmentVariable("DATAVERSE_URL");
            if (!string.IsNullOrWhiteSpace(envUrl))
            {
                Console.WriteLine($"?? Using Dataverse URL from environment variable: {envUrl}");
                return envUrl.TrimEnd('/');
            }

            // Prompt user for URL
            Console.WriteLine("?? Enter your Dataverse environment URL:");
            Console.WriteLine("   Example: https://yourorg.crm.dynamics.com");
            Console.WriteLine("   (You can also set the DATAVERSE_URL environment variable)");
            Console.Write("URL: ");
            
            var userUrl = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(userUrl))
            {
                throw new ArgumentException("Dataverse URL is required. Please provide a valid URL or set the DATAVERSE_URL environment variable.");
            }

            return userUrl.TrimEnd('/');
        }
    }
}