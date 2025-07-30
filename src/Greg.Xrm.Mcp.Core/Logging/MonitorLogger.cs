using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace Greg.Xrm.Mcp.Core.Logging
{
	public class MonitorLogger : ILogger, IDisposable
	{
		private bool disposedValue;
		private readonly string? baseUrl;
		private readonly string logFileName;
		private readonly string categoryName;
		private readonly HttpClient client;
		private readonly Dictionary<object, object> scopes = [];

		//https://learn.microsoft.com/en-us/dotnet/api/system.threading.lock?view=net-9.0
		private static readonly Lock syncRoot = new();


		public MonitorLogger(string baseUrl, string logFileName, string categoryName)
		{
			this.baseUrl = baseUrl;
			this.logFileName = logFileName;
			this.categoryName = categoryName;

			this.client = new HttpClient();
			if (!string.IsNullOrWhiteSpace(baseUrl))
			{
				this.client.BaseAddress = new Uri(baseUrl);
			}
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return new Scope(this, state);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}


		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (string.IsNullOrWhiteSpace(this.baseUrl))
				return;

			lock (syncRoot)
			{
				var requestBody = new
				{
					Timestamp = DateTime.Now,
					Category = this.categoryName,
					Level = logLevel,
					EventId = eventId.Id,
					EventName = eventId.Name,
					State = state,
					Exception = exception,
					Message = formatter(state, exception),
				};

				LogToFile(requestBody);

				try
				{
					this.client.PostAsJsonAsync("log", requestBody);
				}
				catch(AggregateException ex)
				{
					var sb = new StringBuilder();
					sb.AppendLine("#### Error while trying to invoke the monitor: ");
					sb.AppendLine(ex.Message);
					sb.AppendLine("----------------------");

					if (ex.InnerExceptions.Count > 0)
					{
						sb.AppendLine("Inner Exceptions:");
						foreach (var inner in ex.InnerExceptions)
						{
							sb.AppendLine(inner.Message);
							sb.AppendLine("----------------------");
						}
					}
					

					Tracer.Trace(sb.ToString());
				}
				catch(Exception ex)
				{
					var sb = new StringBuilder();
					sb.AppendLine("#### Error while trying to invoke the monitor: ");
					sb.AppendLine(ex.Message);
					sb.AppendLine("----------------------");

					if (ex.InnerException != null)
					{
						sb.AppendLine("Inner Exception:");
						sb.AppendLine(ex.InnerException.Message);
						sb.AppendLine("----------------------");
					}


					Tracer.Trace(sb.ToString());
				}

			}

		}

		private void LogToFile(object obj)
		{
			if (string.IsNullOrWhiteSpace(this.logFileName))
				return;

			try
			{
				lock (syncRoot)
				{
					using var writer = new StreamWriter(this.logFileName, true);

					writer.WriteLine("--- " + DateTime.Now.ToString() + " --------------------------------------");
					writer.WriteLine(JsonConvert.SerializeObject(obj));
					writer.WriteLine();
				}
			}
			catch (Exception ex)
			{
				var sb = new StringBuilder();
				sb.AppendLine("#### Error while trying to log to file from the monitor: ");
				sb.AppendLine(ex.Message);
				sb.AppendLine("----------------------");

				if (ex.InnerException != null)
				{
					sb.AppendLine("Inner Exception:");
					sb.AppendLine(ex.InnerException.Message);
					sb.AppendLine("----------------------");
				}

				Tracer.Trace(sb.ToString());
			}
		}



		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					this.client.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		// Replace the Scope class with a proper dispose pattern implementation
		class Scope : IDisposable
		{
			private readonly MonitorLogger logger;
			private readonly object state;
			private bool disposedValue;

			public Scope(MonitorLogger logger, object state)
			{
				this.logger = logger;
				this.state = state;
				lock (syncRoot)
				{
					logger.scopes.Add(state, state);
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						lock (syncRoot)
						{
							logger.scopes.Remove(state);
						}
					}
					disposedValue = true;
				}
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}
		}
	}
}
