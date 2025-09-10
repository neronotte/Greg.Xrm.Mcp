using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Server.Tools;
using Greg.Xrm.Mcp.Server.Tools.Forms;
using Microsoft.Extensions.Hosting;

namespace Greg.Xrm.Mcp.FormEngineer
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			try
			{
				var builder = Host.CreateApplicationBuilder(args);

				builder.Services.InitializeServices();
				builder.InitializeFramework();
				builder.Services.InitializeStdioMcpServer(typeof(DataverseMetadataTools).Assembly);



				var host = builder.Build();

				await host.RunAsync();

#if DEBUG
				Console.WriteLine();
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey(true);
#endif
			}
			catch (Exception ex)
			{
				await Console.Error.WriteLineAsync($"Fatal error: {ex.Message}");
				await Console.Error.WriteLineAsync(ex.ToString());
			}
		}
	}
}
