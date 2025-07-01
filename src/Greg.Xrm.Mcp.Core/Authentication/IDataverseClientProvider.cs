using Microsoft.PowerPlatform.Dataverse.Client;

namespace Greg.Xrm.Mcp.Core.Authentication
{
	public interface IDataverseClientProvider
	{
		Task<IOrganizationServiceAsync2> GetDataverseClientAsync();
	}
}
