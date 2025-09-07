using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.FormEngineer.Model;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.Server.Tools.Forms
{
	[McpServerToolType]
	public class GetFormListByTableSchemaName(
			ILogger<GetFormListByTableSchemaName> logger,
			IDataverseClientProvider clientProvider)
	{
		/// <summary>
		/// Retrieves a comprehensive list of all forms for a specified Dataverse table with detailed metadata.
		/// This method provides an overview of all available forms for a table, including their properties,
		/// types, and status information in either human-readable or JSON format.
		/// </summary>
		/// <param name="logger">Logger instance for tracking list retrieval operations and debugging</param>
		/// <param name="clientProvider">Service provider for authenticating and connecting to Dataverse</param>
		/// <param name="formService">Service for performing form-related operations (used for consistency in method signatures)</param>
		/// <param name="publishXmlBuilder">Builder service for publish operations (used for consistency in method signatures)</param>
		/// <param name="entityLogicalName">The logical name of the Dataverse table to list forms for (e.g., 'account', 'contact')</param>
		/// <param name="outputType">
		/// Output format type: "Formatted" for human-readable text or "Json" for structured JSON data.
		/// Case-insensitive comparison is used.
		/// </param>
		/// <returns>
		/// A task that represents the asynchronous operation. The task result contains:
		/// - **Formatted output**: Human-readable list with form details including name, ID, type, default status, and description
		/// - **JSON output**: Structured array of form objects with all metadata properties for programmatic processing
		/// - Error message if the table is not found or if an exception occurs
		/// </returns>
		/// <exception cref="ArgumentException">Thrown when entityLogicalName is null or empty</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown when authentication to Dataverse fails</exception>
		/// <exception cref="InvalidOperationException">Thrown when the specified table does not exist in the environment</exception>
		/// <remarks>
		/// This method performs the following operations:
		/// 1. Authenticates to the Dataverse environment using the provided client provider
		/// 2. Creates a DataverseContext for querying system forms
		/// 3. Retrieves all forms for the specified table using LINQ queries
		/// 4. Orders results by form type, then by name for consistent output
		/// 5. Formats the results according to the requested output type
		/// 
		/// **Returned Information:**
		/// - Form ID (unique identifier)
		/// - Form Name (display name)
		/// - Form Type (Main, QuickCreate, QuickView, etc. from <see cref="systemform_type"/> enumeration)
		/// - Default Status (whether the form is the default for its type)
		/// - Description (if available)
		/// 
		/// **Output Formats:**
		/// - **Formatted**: User-friendly text format with emojis and clear sections for manual review
		/// - **JSON**: Machine-readable format suitable for programmatic processing and integration
		/// 
		/// This method is particularly useful for:
		/// - Form inventory and auditing across environments
		/// - Identifying available forms before modification or customization
		/// - Understanding form hierarchy and default assignments
		/// - Bulk form analysis and reporting scenarios
		/// - Development workflows requiring form metadata
		/// </remarks>
		/// <example>
		/// <code>
		/// // Get formatted list of account forms for manual review
		/// var formattedList = await GetFormListByTableSchemaName(
		///     logger, clientProvider, formService, publishXmlBuilder, "account", "Formatted");
		/// 
		/// // Get JSON list for programmatic processing
		/// var jsonList = await GetFormListByTableSchemaName(
		///     logger, clientProvider, formService, publishXmlBuilder, "contact", "Json");
		/// 
		/// // Parse JSON result for further processing
		/// var formsData = JsonConvert.DeserializeObject(jsonList);
		/// </code>
		/// </example>
		/// <seealso cref="GetFormDefinition"/>
		/// <seealso cref="UpdateFormDefinition"/>
		/// <seealso cref="systemform_type"/>
		[McpServerTool(
			Name = "dataverse_form_retrieve_list_by_table",
			Destructive = false,
			ReadOnly = true,
			Idempotent = true
		), 
		Description("Retrieves the list of forms for a given Dataverse table, and returns the details such as Id, Name, Type, Description, ad if it's default or not")]
		public async Task<string> Execute(
			[Description("The logical name of the Dataverse table")] string entityLogicalName,
			[Description("How do you want the output to be returned (Formatted or Json)")] string outputType)
		{
			logger.LogTrace("{ToolName} called with parameters: entityLogicalName={EntityLogicalName}, outputType={OutputType}",
				   nameof(GetFormListByTableSchemaName),
				   entityLogicalName,
				   outputType);
			try
			{
				var client = await clientProvider.GetDataverseClientAsync();

				logger.LogTrace("🔍 Retrieving forms for table: {EntityName}", entityLogicalName);

				var context = new DataverseContext(client);

				var forms = context.SystemFormSet.Where(x => x.ObjectTypeCode == entityLogicalName)
					.OrderBy(x => x.Type)
					.ThenBy(x => x.Name)
					.Select(x => new
					{
						x.Id,
						x.Name,
						x.Type,
						x.Description,
						x.IsDefault,
					}).ToList();

				if ("json".Equals(outputType, StringComparison.OrdinalIgnoreCase))
				{
					return JsonConvert.SerializeObject(forms.ToList(), Newtonsoft.Json.Formatting.Indented);
				}
				else
				{
					return $"✅ Found {forms.Count()} forms for table '{entityLogicalName}':\n\n" +
						string.Join(Environment.NewLine, forms.Select(f =>
							$"📋 Form: {f.Name}{Environment.NewLine}" +
							$"   ID: {f.Id}{Environment.NewLine}" +
							$"   Type: {f.Type}{Environment.NewLine}" +
							$"   Default: {(f.IsDefault.GetValueOrDefault() ? "✅ Yes" : "❌ No")}{Environment.NewLine}" +
							$"   Description: {f.Description ?? "N/A"}{Environment.NewLine}"));
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "❌ Error while retrieving forms: {Message}", ex.Message);
				return $"❌ Error: {ex.Message}";
			}
		}
	}
}
