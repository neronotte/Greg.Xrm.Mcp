using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.Core.Services
{
	public class TelemetryClientToLogger(ILogger<TelemetryClientToLogger> logger) : ITelemetryClient
	{
		private readonly ILogger<TelemetryClientToLogger> _logger = logger;

		public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
		{
			var logData = new
			{
				Type = "Event",
				Name = eventName,
				Properties = properties ?? new Dictionary<string, string>(),
				Metrics = metrics ?? new Dictionary<string, double>(),
				Timestamp = DateTimeOffset.UtcNow
			};

			_logger.LogInformation("TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackEvent(EventTelemetry telemetry)
		{
			var logData = new
			{
				Type = "Event",
				Name = telemetry.Name,
				Properties = telemetry.Properties,
				Metrics = telemetry.Metrics,
				Timestamp = telemetry.Timestamp
			};

			_logger.LogInformation("TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackTrace(string message)
		{
			var logData = new
			{
				Type = "Trace",
				Message = message,
				SeverityLevel = "Information",
				Timestamp = DateTimeOffset.UtcNow
			};

			_logger.LogInformation("TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackTrace(string message, SeverityLevel severityLevel)
		{
			var logLevel = ConvertSeverityToLogLevel(severityLevel);
			var logData = new
			{
				Type = "Trace",
				Message = message,
				SeverityLevel = severityLevel.ToString(),
				Timestamp = DateTimeOffset.UtcNow
			};

			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackTrace(string message, IDictionary<string, string> properties)
		{
			var logData = new
			{
				Type = "Trace",
				Message = message,
				SeverityLevel = "Information",
				Properties = properties ?? new Dictionary<string, string>(),
				Timestamp = DateTimeOffset.UtcNow
			};

			_logger.LogInformation("TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties)
		{
			var logLevel = ConvertSeverityToLogLevel(severityLevel);
			var logData = new
			{
				Type = "Trace",
				Message = message,
				SeverityLevel = severityLevel.ToString(),
				Properties = properties ?? new Dictionary<string, string>(),
				Timestamp = DateTimeOffset.UtcNow
			};

			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackTrace(TraceTelemetry telemetry)
		{
			var logLevel = ConvertSeverityToLogLevel(telemetry.SeverityLevel);
			var logData = new
			{
				Type = "Trace",
				Message = telemetry.Message,
				SeverityLevel = telemetry.SeverityLevel?.ToString() ?? "Information",
				Properties = telemetry.Properties,
				Timestamp = telemetry.Timestamp
			};

			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
		{
			var logData = new
			{
				Type = "Metric",
				Name = name,
				Value = value,
				Properties = properties ?? new Dictionary<string, string>(),
				Timestamp = DateTimeOffset.UtcNow
			};

			_logger.LogInformation("TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackMetric(MetricTelemetry telemetry)
		{
			var logData = new
			{
				Type = "Metric",
				Name = telemetry.Name,
				Value = telemetry.Sum,
				Properties = telemetry.Properties,
				Timestamp = telemetry.Timestamp
			};

			_logger.LogInformation("TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
		{
			var logData = new
			{
				Type = "Exception",
				Exception = exception.ToString(),
				Message = exception.Message,
				Properties = properties ?? new Dictionary<string, string>(),
				Metrics = metrics ?? new Dictionary<string, double>(),
				Timestamp = DateTimeOffset.UtcNow
			};

			_logger.LogError(exception, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackException(ExceptionTelemetry telemetry)
		{
			var logData = new
			{
				Type = "Exception",
				Exception = telemetry.Exception?.ToString(),
				Message = telemetry.Message,
				Properties = telemetry.Properties,
				Metrics = telemetry.Metrics,
				Timestamp = telemetry.Timestamp
			};

			_logger.LogError(telemetry.Exception, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
		{
			var logData = new
			{
				Type = "Dependency",
				DependencyTypeName = dependencyTypeName,
				DependencyName = dependencyName,
				Data = data,
				StartTime = startTime,
				Duration = duration,
				Success = success,
				Timestamp = DateTimeOffset.UtcNow
			};

			var logLevel = success ? LogLevel.Information : LogLevel.Warning;
			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
		{
			var logData = new
			{
				Type = "Dependency",
				DependencyTypeName = dependencyTypeName,
				Target = target,
				DependencyName = dependencyName,
				Data = data,
				StartTime = startTime,
				Duration = duration,
				ResultCode = resultCode,
				Success = success,
				Timestamp = DateTimeOffset.UtcNow
			};

			var logLevel = success ? LogLevel.Information : LogLevel.Warning;
			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackDependency(DependencyTelemetry telemetry)
		{
			var logData = new
			{
				Type = "Dependency",
				DependencyTypeName = telemetry.Type,
				Target = telemetry.Target,
				DependencyName = telemetry.Name,
				Data = telemetry.Data,
				StartTime = telemetry.Timestamp,
				Duration = telemetry.Duration,
				ResultCode = telemetry.ResultCode,
				Success = telemetry.Success,
				Properties = telemetry.Properties,
				Metrics = telemetry.Metrics,
				Timestamp = telemetry.Timestamp
			};

			var logLevel = telemetry.Success ?? true ? LogLevel.Information : LogLevel.Warning;
			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string? message = null, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
		{
			var logData = new
			{
				Type = "Availability",
				Name = name,
				TimeStamp = timeStamp,
				Duration = duration,
				RunLocation = runLocation,
				Success = success,
				Message = message,
				Properties = properties ?? new Dictionary<string, string>(),
				Metrics = metrics ?? new Dictionary<string, double>(),
				Timestamp = DateTimeOffset.UtcNow
			};

			var logLevel = success ? LogLevel.Information : LogLevel.Warning;
			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackAvailability(AvailabilityTelemetry telemetry)
		{
			var logData = new
			{
				Type = "Availability",
				Name = telemetry.Name,
				TimeStamp = telemetry.Timestamp,
				Duration = telemetry.Duration,
				RunLocation = telemetry.RunLocation,
				Success = telemetry.Success,
				Message = telemetry.Message,
				Properties = telemetry.Properties,
				Metrics = telemetry.Metrics,
				Timestamp = telemetry.Timestamp
			};

			var logLevel = telemetry.Success ? LogLevel.Information : LogLevel.Warning;
			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void Track(ITelemetry telemetry)
		{
			var logData = new
			{
				Type = "Generic",
				TelemetryType = telemetry.GetType().Name,
				Context = telemetry.Context,
				Timestamp = telemetry.Timestamp
			};

			_logger.LogInformation("TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
		{
			var logData = new
			{
				Type = "Request",
				Name = name,
				StartTime = startTime,
				Duration = duration,
				ResponseCode = responseCode,
				Success = success,
				Timestamp = DateTimeOffset.UtcNow
			};

			var logLevel = success ? LogLevel.Information : LogLevel.Warning;
			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void TrackRequest(RequestTelemetry request)
		{
			var logData = new
			{
				Type = "Request",
				Name = request.Name,
				StartTime = request.Timestamp,
				Duration = request.Duration,
				ResponseCode = request.ResponseCode,
				Success = request.Success,
				Url = request.Url,
				Properties = request.Properties,
				Metrics = request.Metrics,
				Timestamp = request.Timestamp
			};

			var logLevel = request.Success ?? true ? LogLevel.Information : LogLevel.Warning;
			_logger.Log(logLevel, "TELEMETRY: {TelemetryData}", System.Text.Json.JsonSerializer.Serialize(logData));
		}

		public void Flush()
		{
			_logger.LogDebug("TELEMETRY: Flush requested");
		}

		public async Task<bool> FlushAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("TELEMETRY: FlushAsync requested");
			await Task.CompletedTask;
			return true;
		}

		private static LogLevel ConvertSeverityToLogLevel(SeverityLevel? severityLevel)
		{
			return severityLevel switch
			{
				SeverityLevel.Verbose => LogLevel.Trace,
				SeverityLevel.Information => LogLevel.Information,
				SeverityLevel.Warning => LogLevel.Warning,
				SeverityLevel.Error => LogLevel.Error,
				SeverityLevel.Critical => LogLevel.Critical,
				_ => LogLevel.Information
			};
		}

		public IOperationHolder<T> StartOperation<T>(string operationName) where T : OperationTelemetry, new()
		{
			return new OperationHolderFake<T>(_logger, new T());
		}

		private sealed class OperationHolderFake<T> : IOperationHolder<T> where T : OperationTelemetry, new()
		{
			private ILogger? _logger;
			private IDisposable? _scope;
			private bool _disposed;

			public OperationHolderFake(ILogger logger, T operationTelemetry)
			{
				_logger = logger ?? throw new ArgumentNullException(nameof(logger));
				Telemetry = operationTelemetry;
				_scope = _logger.BeginScope("TELEMETRY: Operation '{OperationName}' started", Telemetry.Name);
			}

			public T Telemetry { get; }

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (disposing)
				{
					// Dispose managed resources
					_scope?.Dispose();
					_scope = null;
					_logger = null;
				}

				_disposed = true;
			}
		}
	}
}
