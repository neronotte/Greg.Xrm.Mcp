namespace Greg.Xrm.Mcp.Core.Logging
{
	public static class Tracer
	{
		private static readonly string fileName = Path.Combine(
			Path.GetDirectoryName(typeof(Tracer).Assembly.Location) ?? string.Empty,
			"mcpserver-logs.txt"
		);

		public static void Trace(string text)
		{
			File.AppendAllTextAsync(fileName, text + Environment.NewLine);
		}
	}
}
