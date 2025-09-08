using Greg.Xrm.Mcp.Core.Authentication;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Dynamic;

namespace Greg.Xrm.Mcp.Server.Tools.AppModules
{
	/// <summary>
	/// Provides functionality to retrieve a comprehensive list of all model-driven applications (app modules) 
	/// in a Dataverse environment with detailed metadata and security role associations.
	/// This tool enables administrators and developers to inventory, audit, and analyze Power Platform applications
	/// across environments for governance, deployment, and security management purposes.
	/// </summary>
	/// <param name="logger">Logger instance for tracking application retrieval operations and debugging</param>
	/// <param name="clientProvider">Service provider for authenticating and connecting to Dataverse</param>
	/// <remarks>
	/// Model-driven apps in Dataverse are represented by AppModule entities that define the structure,
	/// navigation, and security context for user applications. This tool provides complete visibility
	/// into the application landscape including managed/unmanaged status, versioning, and role assignments.
	/// 
	/// **Key Features:**
	/// - Retrieves all app modules in the environment regardless of publisher or solution
	/// - Includes comprehensive metadata such as version, management status, and configuration
	/// - Automatically resolves and includes associated security role assignments
	/// - Returns structured JSON data suitable for programmatic processing and reporting
	/// - Supports application inventory and compliance auditing scenarios
	/// 
	/// **Use Cases:**
	/// - Environment auditing and application inventory management
	/// - Security role assignment analysis and reporting
	/// - Application lifecycle management and deployment tracking
	/// - Compliance reporting for governance frameworks
	/// - Development workflow automation and CI/CD pipeline integration
	/// </remarks>
	[McpServerToolType]
	public class GetAppList(
		ILogger<GetAppList> logger,
		IDataverseClientProvider clientProvider)
	{
		/// <summary>
		/// Retrieves a comprehensive list of all model-driven applications in the Dataverse environment
		/// with detailed metadata including security role associations and configuration information.
		/// This method provides complete visibility into the application landscape for administrative,
		/// governance, and development purposes.
		/// </summary>
		/// <returns>
		/// A task that represents the asynchronous operation. The task result contains:
		/// - **JSON Array**: Structured collection of application objects with complete metadata
		/// - **Application Properties**: ID, name, unique name, description, version, and management status
		/// - **Security Roles**: Associated role IDs for each application for security analysis
		/// - **Configuration Data**: Descriptor for advanced application structure analysis
		/// - **Error Message**: Detailed error information if retrieval fails or access is denied
		/// </returns>
		/// <remarks>
		/// This method performs the following operations:
		/// 1. Establishes authenticated connection to the Dataverse environment
		/// 2. Creates a DataverseContext for querying app module and role association entities
		/// 3. Retrieves all app modules with comprehensive metadata using optimized LINQ queries
		/// 4. Performs secondary query to gather security role associations for each application
		/// 5. Merges role data with application metadata to provide complete security context
		/// 6. Serializes results to structured JSON format for consumption by calling applications
		/// 
		/// **Returned Application Information:**
		/// - **AppModuleId**: Unique identifier (GUID) for the application
		/// - **Name**: Display name of the application as shown to users
		/// - **UniqueName**: System unique name used for programmatic references
		/// - **Description**: Application description and purpose documentation
		/// - **IsManaged**: Boolean indicating if the app is part of a managed solution
		/// - **IsDefault**: Boolean indicating if this is a default application for the environment
		/// - **AppModuleVersion**: Version string for tracking application updates and releases
		/// - **ConfigXML**: Complete XML configuration defining app structure, navigation, and behavior
		/// - **Roles**: Array of security role IDs that have access to this application
		/// 
		/// **Security and Role Analysis:**
		/// The method automatically resolves security role associations through the AppModuleRoles
		/// relationship, providing immediate visibility into which roles can access each application.
		/// This information is critical for:
		/// - Security auditing and compliance reporting
		/// - Role-based access control analysis
		/// - Application permission troubleshooting
		/// - Governance and risk management workflows
		/// 
		/// **Performance Considerations:**
		/// - Uses efficient LINQ projections to minimize data transfer
		/// - Implements optimized queries to reduce roundtrips to Dataverse
		/// - Leverages using statement for proper context disposal
		/// - Processes role associations in memory to minimize database load
		/// 
		/// **Integration Scenarios:**
		/// This method is particularly valuable for:
		/// - Automated environment documentation and inventory systems
		/// - DevOps pipelines requiring application metadata for deployment decisions
		/// - Governance tools needing comprehensive application and security reporting
		/// - Migration planning requiring complete application catalog analysis
		/// - Compliance auditing workflows for regulatory requirements
		/// </remarks>
		[McpServerTool(
			Name ="dataverse_appmodule_retrieve_list", 
			Destructive =false, 
			Idempotent = true, 
			ReadOnly =true),
		Description(@"Retrieves a comprehensive list of all model-driven applications in the Dataverse environment
with detailed metadata including security role associations and configuration information.
This method provides complete visibility into the application landscape for administrative,
governance, and development purposes.")]
		public async Task<string> Execute()
		{
			logger.LogTrace("{ToolName} called.",
				   nameof(GetAppList));

			try
			{
				using var context = await clientProvider.GetDataverseContextAsync();
#pragma warning disable S2971 // LINQ expressions should be simplified
				var apps = context.AppModuleSet.Select(a => new
				{
					a.AppModuleId,
					a.Name,
					a.UniqueName,
					a.Description,
					a.IsManaged,
					a.IsDefault,
					a.AppModuleVersion,
					a.Descriptor,
				}).ToList().Select(a =>
				{
					dynamic x = new ExpandoObject();
					x.AppModuleId = a.AppModuleId;
					x.Name = a.Name;
					x.UniqueName = a.UniqueName;
					x.Description = a.Description;
					x.IsManaged = a.IsManaged;
					x.IsDefault = a.IsDefault;
					x.AppModuleVersion = a.AppModuleVersion;
					x.Descriptor = a.Descriptor != null ? JsonConvert.DeserializeObject(a.Descriptor) : null;
					return x;

				}).ToList();
#pragma warning restore S2971 // LINQ expressions should be simplified

				var appRoles = context.AppModuleRolesSet.Select(ar => new
				{
					ar.AppModuleRoleId,
					ar.AppModuleId,
					ar.RoleId,
				}).ToList();


				foreach (var app in apps)
				{
					var roles = appRoles.Where(r => r.AppModuleId.Id == app.AppModuleId)
						.Select(r => r.RoleId)
						.ToList();

					app.Roles = roles;
				}

				var result = JsonConvert.SerializeObject(apps.ToList(), Formatting.Indented);
				return result;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
