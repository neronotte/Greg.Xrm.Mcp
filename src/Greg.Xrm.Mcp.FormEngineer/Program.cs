using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Reflection;

namespace Greg.Xrm.Mcp.FormEngineer
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			var builder = Host.CreateApplicationBuilder(args);

			builder.Logging.ClearProviders();
			builder.Logging.AddNLog(Path.Combine(Assembly.GetExecutingAssembly().Location, "NLog.config"));

			/****************************************************************************
			 * Registering services
			 ****************************************************************************/
			builder.Services.AddTransient<IFormService, FormService>();


			builder.InitializeArguments()
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
	}
}
