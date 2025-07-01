using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Greg.Xrm.Mcp.FormEngineer.Server.Prompts
{
	[McpServerPromptType]
	public static class Instructions
	{
		[McpServerPrompt(Name = "Instruction to use the current MCP Server"),
		Description("Instructions that can be used to effectively leverage the current MCP server capabilities."),
		]
		public static ChatMessage MainPrompt() =>
			new(ChatRole.User, Properties.Resources.prompt_main);


		[McpServerPrompt,
		Description("Clean the form of a custom dataverse table from all the issues that are generated automatically on creation.")]
		public static ChatMessage CleanForm(
		[Description("The schema name of the dataverse table to clean")] string tableName
		) =>
			new(ChatRole.User, Properties.Resources.prompt_clean.Replace("{tablename}", tableName));
	}
}
