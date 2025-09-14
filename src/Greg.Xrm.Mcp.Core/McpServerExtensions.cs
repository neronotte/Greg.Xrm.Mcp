using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using ModelContextProtocol;
using static ModelContextProtocol.Protocol.ElicitRequestParams;

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


		public static async Task<(bool, string)> ElicitTextAsync(this IMcpServer server, string question)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNullOrWhiteSpace(question);

			var elicitResponse = await server.ElicitAsync(new ElicitRequestParams
			{
				Message = question,
				RequestedSchema = new RequestSchema {
					Properties = new Dictionary<string, PrimitiveSchemaDefinition>
					{
	 	               ["Answer"] = new StringSchema()
					}
				}
			});

			if (elicitResponse.Action != "accept" )
			{
				return (false, string.Empty);
			}

			var answer = elicitResponse.Content?["Answer"].GetString();

			if (string.IsNullOrWhiteSpace(answer))
			{
				return (false, string.Empty);
			}

			return (true, answer);
		}
	}
}
