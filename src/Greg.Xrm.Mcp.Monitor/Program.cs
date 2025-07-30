using Greg.Xrm.Mcp.Monitor.Logging;
using Greg.Xrm.Mcp.Monitor.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Greg.Xrm.Mcp.Monitor
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			ApplicationConfiguration.Initialize();

			var form = new MainForm();
			var messenger = new Messenger(form);
			form.SetMessenger(messenger);

			Task.Run(() => StartWebApi(config, messenger));

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			Application.Run(form);
		}


		static void StartWebApi(IConfiguration config, Messenger messenger)
		{
			var builder = WebApplication.CreateBuilder();

			var baseUrl = config.GetValue<string>("BaseUrl");
			if (string.IsNullOrEmpty(baseUrl))
			{
				throw new InvalidOperationException("BaseUrl is not configured in appsettings.json");
			}

			builder.WebHost.UseUrls(baseUrl);


			var app = builder.Build();

			app.MapGet("/", () => {
				
			});

			app.MapPost("/log", async context =>
			{
				try
				{
					var requestBody = await context.Request.ReadFromJsonAsync<LogRequestPayload>();
					if (requestBody == null)
					{
						context.Response.StatusCode = 400; // Bad Request
						await context.Response.WriteAsync("Invalid request body");
						return;
					}

					messenger.Send(requestBody);

					// Here you would typically log the requestBody to your logging system
					Console.WriteLine($"Received log: {requestBody}");
					context.Response.StatusCode = 200; // OK
				}
				catch(Exception ex)
				{
					messenger.Send(new LogRequestPayload
					{
						Message = $"Error processing log request: {ex.Message}",
						Level = Microsoft.Extensions.Logging.LogLevel.Critical
					});
				}
			});


			app.Run();
		}
	}
}