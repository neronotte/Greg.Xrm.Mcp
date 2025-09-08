using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.FormEngineer.Model;

namespace Greg.Xrm.Mcp
{
	public static class Extensions
	{
		public static async Task<DataverseContext> GetDataverseContextAsync(this IDataverseClientProvider clientProvider)
		{
			var client = await clientProvider.GetDataverseClientAsync();
			return new DataverseContext(client);
		}
	}
}
