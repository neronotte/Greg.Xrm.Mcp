using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Greg.Xrm.Mcp.Core
{
	public static class ApplicationBuilderExtensions
	{
		public static IHostApplicationBuilder InitializeArguments(this IHostApplicationBuilder builder)
		{
			builder.Services.AddSingleton(sp =>
			{
				var config = sp.GetRequiredService<IConfiguration>();
				var options = config.Get<Arguments>() ?? new Arguments();
				return options;
			});

			return builder;
		}
		public static IHostApplicationBuilder InitializeFramework(this IHostApplicationBuilder builder)
		{
			builder.Services.AddSingleton<IDataverseClientProvider, DataverseClientProvider>();
			builder.Services.AddTransient<IPublishXmlBuilder, PublishXmlBuilder>();

			return builder;
		}

		public static IMcpServerBuilder InitializeStdioMcpServer(this IHostApplicationBuilder builder, params Assembly[] assemblies)
		{
			var mcpServerBuilder = builder.Services.AddMcpServer()
				.WithStdioServerTransport();

			foreach (var assembly in assemblies)
			{
				mcpServerBuilder.WithToolsFromAssembly(assembly)
					.WithPromptsFromAssembly(assembly)
					.WithResourcesFromAssembly(assembly);
			}

			return mcpServerBuilder;
		}

	}
}
