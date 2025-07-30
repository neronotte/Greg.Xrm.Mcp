using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Greg.Xrm.Mcp.FormEngineer
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			try
			{
				var builder = Host.CreateApplicationBuilder(args);

				/****************************************************************************
				 * Registering services
				 ****************************************************************************/
				builder.Services.AddTransient<IFormService, FormService>();
				builder.Services.AddTransient<IFormXmlValidator, FormXmlValidator>();


				builder
					.InitializeFramework()
					.InitializeStdioMcpServer(Assembly.GetExecutingAssembly());



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
