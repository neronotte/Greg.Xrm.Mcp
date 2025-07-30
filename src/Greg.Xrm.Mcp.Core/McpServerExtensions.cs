using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using ModelContextProtocol;

namespace Greg.Xrm.Mcp.Core
{
	public static class McpServerExtensions
	{
		public static async Task<ProgressToken> NotifyProgressAsync(this IMcpServer server, string token, string message, float progress = 0, float total = 100, CancellationToken cancellationToken = default)
		{
			var progressToken = new ProgressToken(token);
			return await server.NotifyProgressAsync(progressToken, message, progress, total, cancellationToken);
		}


		public static async Task<ProgressToken> NotifyProgressAsync(this IMcpServer server, ProgressToken progressToken, string message, float progress = 0, float total = 100, CancellationToken cancellationToken = default)
		{
			var value = new ProgressNotificationValue
			{
				Message = message,
				Progress = progress,
				Total = total,
			};

			await server.NotifyProgressAsync(progressToken, value, cancellationToken);
			return progressToken;
		}
	}
}
