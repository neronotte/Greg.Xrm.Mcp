using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Logging;
using Greg.Xrm.Mcp.Core.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Serilog;
using System.Reflection;

namespace Greg.Xrm.Mcp.Core
{
	public static class ApplicationBuilderExtensions
	{

		public static IHostApplicationBuilder InitializeFramework(this IHostApplicationBuilder builder)
		{
			var settingsDict = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			var settings = settingsDict.Get<Settings>() 
				?? throw new InvalidOperationException("Settings could not be loaded from appsettings.json");
			
			builder.Services.AddSingleton(settings);
			builder.Services.AddSingleton(sp =>
			{
				var config = sp.GetRequiredService<IConfiguration>();
				var options = config.Get<Arguments>() ?? new Arguments();
				return options;
			});

			if (!string.IsNullOrWhiteSpace(settings.ApplicationInsightsApiKey))
			{
				builder.Services.AddApplicationInsightsTelemetryWorkerService(config =>
				{
					config.ConnectionString = $"InstrumentationKey={settings.ApplicationInsightsApiKey}";
				});
				builder.Services.AddTransient<ITelemetryClient>(sp => new TelemetryClientWrapper(sp.GetRequiredService<TelemetryClient>()));
			}
			else
			{
				builder.Services.AddTransient<ITelemetryClient>(sp => new TelemetryClientToLogger(sp.GetRequiredService<ILogger<TelemetryClientToLogger>>()));
			}


			builder.Services.AddSingleton<IDataverseClientProvider, DataverseClientProvider>();
			builder.Services.AddTransient<IPublishXmlBuilder, PublishXmlBuilder>();
			
			builder.Services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.ClearProviders();
				loggingBuilder.SetMinimumLevel(LogLevel.Trace);


				if (!string.IsNullOrWhiteSpace(settings.ApplicationInsightsApiKey))
				{
					loggingBuilder.AddApplicationInsights(
						configureTelemetryConfiguration: (config) =>
						{
							config.ConnectionString = $"InstrumentationKey={settings.ApplicationInsightsApiKey}";
						},
						configureApplicationInsightsLoggerOptions: (loggerOptions) => {
							loggerOptions.FlushOnDispose = true;
							loggerOptions.TrackExceptionsAsExceptionTelemetry = true;
						}
					);
				}


				var logger = new LoggerConfiguration()
					.MinimumLevel.Debug()
					.Enrich.FromLogContext()
					.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
					.CreateLogger();

				loggingBuilder.AddSerilog(logger, dispose: true);

				#if DEBUG

				loggingBuilder.AddProvider(new MonitorLoggerProvider(settings.LogUrl, settings.LogFile));

				#endif
			});

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
