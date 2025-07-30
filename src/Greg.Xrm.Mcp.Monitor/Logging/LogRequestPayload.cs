using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.Monitor.Logging
{
	public class LogRequestPayload
	{
		public DateTime? Timestamp { get; set; }
		public string? Category { get; set; }
		public LogLevel Level { get; set; }
		public int EventId { get; set; }
		public string? EventName { get; set; }

		public object? State { get; set; }
		public Exception? Exception { get; set; }
		public string? Message { get; set; }
	}
}
