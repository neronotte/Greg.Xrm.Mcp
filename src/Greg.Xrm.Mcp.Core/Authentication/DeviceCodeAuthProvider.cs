using Microsoft.Identity.Client;

namespace Greg.Xrm.Mcp.Core.Authentication;

/// <summary>
/// Authenticates against Dataverse using MSAL device-code flow.
/// Surfaces the user code + verification URL to stderr immediately,
/// then polls silently until the user completes MFA.
/// </summary>
public static class DeviceCodeAuthProvider
{
	// Well-known Power Platform / Azure PowerShell public client AppId
	private const string DefaultClientId = "51f81489-12ee-4a9e-aaae-a2591f45987d";

	/// <summary>
	/// Starts device-code flow for the given Dataverse URL.
	/// Writes the device code prompt to stderr in a structured block,
	/// then polls until the user completes auth or the code expires.
	/// Returns the access token string on success.
	/// </summary>
	public static async Task<string> AcquireTokenAsync(string dataverseUrl, CancellationToken cancellationToken = default)
	{
		var clientId = Environment.GetEnvironmentVariable("DATAVERSE_CLIENT_ID") ?? DefaultClientId;
		var tenantId = Environment.GetEnvironmentVariable("DATAVERSE_TENANT_ID") ?? "organizations";
		var authority = $"https://login.microsoftonline.com/{tenantId}";

		var app = PublicClientApplicationBuilder
			.Create(clientId)
			.WithAuthority(authority)
			.Build();

		var scopes = new[] { $"{dataverseUrl.TrimEnd('/')}/.default" };

		var result = await app.AcquireTokenWithDeviceCode(scopes, callback =>
		{
			// Write structured device-code block to stderr — visible in VS Code MCP Output
			Console.Error.WriteLine();
			Console.Error.WriteLine("========================================");
			Console.Error.WriteLine($"Device code active: {callback.UserCode}");
			Console.Error.WriteLine($"Waiting at: {callback.VerificationUrl}");
			Console.Error.WriteLine("Status: Pending your MFA completion");
			Console.Error.WriteLine("Once you enter that code and sign in, the MCP server will complete authentication automatically.");
			Console.Error.WriteLine("========================================");
			Console.Error.WriteLine();
			return Task.CompletedTask;
		}).ExecuteAsync(cancellationToken);

		return result.AccessToken;
	}
}
