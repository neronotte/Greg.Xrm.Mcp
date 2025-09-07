using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Greg.Xrm.Mcp
{
	public static class DependencyInjection
	{
		public static IServiceCollection InitializeServices(this IServiceCollection services)
		{
			/****************************************************************************
			* Registering services
			****************************************************************************/
			services.AddTransient<IFormService, FormService>();
			services.AddTransient<IFormXmlValidator, FormXmlValidator>();

			// Add any common services here that should be available in all MCP hosts
			return services;
		}
	}
}
