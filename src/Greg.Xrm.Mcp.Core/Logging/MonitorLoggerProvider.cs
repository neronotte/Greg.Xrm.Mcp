using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.Core.Logging
{
	public class MonitorLoggerProvider(string baseUrl, string fileName) : ILoggerProvider
	{
		private bool disposedValue;
		private readonly Dictionary<string, IDisposable> loggers = [];


		public ILogger CreateLogger(string categoryName)
		{
			if (this.loggers.TryGetValue(categoryName, out var existingLogger))
			{
				return (ILogger)existingLogger;
			}
			var logger = new MonitorLogger(baseUrl, fileName, categoryName);
			this.loggers.Add(categoryName, logger);
			return logger;
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing && loggers.Count > 0)
				{
					foreach (var logger in loggers.Values)
					{
						logger.Dispose();
					}
					
					loggers.Clear();
				}

				disposedValue = true;
			}
		}
	}
}
