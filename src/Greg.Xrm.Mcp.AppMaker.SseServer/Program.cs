using Greg.Xrm.Mcp.Core;
using Greg.Xrm.Mcp.Server.Tools;

namespace Greg.Xrm.Mcp.FormEngineer.SseServer
{
	public static class Program
	{
		static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.InitializeServices();
			builder.InitializeFramework();

			// Register HttpContextAccessor to enable access to HttpContext in tools
			builder.Services.AddHttpContextAccessor();

			builder.Services.InitializeSseMcpServer(typeof(DataverseMetadataTools).Assembly);

			var app = builder.Build();

			app.MapMcp();
			app.Run();
		}
	}
}
