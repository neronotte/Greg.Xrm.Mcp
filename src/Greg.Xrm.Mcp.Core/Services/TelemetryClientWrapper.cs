using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace Greg.Xrm.Mcp.Core.Services
{
	public class TelemetryClientWrapper(TelemetryClient innerClient) : ITelemetryClient
	{
		private readonly TelemetryClient _innerClient = innerClient;

		public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
		{
			_innerClient.TrackEvent(eventName, properties, metrics);
		}

		public void TrackEvent(EventTelemetry telemetry)
		{
			_innerClient.TrackEvent(telemetry);
		}

		public void TrackTrace(string message)
		{
			_innerClient.TrackTrace(message);
		}

		public void TrackTrace(string message, SeverityLevel severityLevel)
		{
			_innerClient.TrackTrace(message, severityLevel);
		}

		public void TrackTrace(string message, IDictionary<string, string> properties)
		{
			_innerClient.TrackTrace(message, properties);
		}

		public void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties)
		{
			_innerClient.TrackTrace(message, severityLevel, properties);
		}

		public void TrackTrace(TraceTelemetry telemetry)
		{
			_innerClient.TrackTrace(telemetry);
		}

		public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
		{
			_innerClient.TrackMetric(name, value, properties);
		}

		public void TrackMetric(MetricTelemetry telemetry)
		{
			_innerClient.TrackMetric(telemetry);
		}

		public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
		{
			_innerClient.TrackException(exception, properties, metrics);
		}

		public void TrackException(ExceptionTelemetry telemetry)
		{
			_innerClient.TrackException(telemetry);
		}

		public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
		{
			_innerClient.TrackDependency(dependencyTypeName, dependencyName, data, startTime, duration, success);
		}

		public void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
		{
			_innerClient.TrackDependency(dependencyTypeName, target, dependencyName, data, startTime, duration, resultCode, success);
		}

		public void TrackDependency(DependencyTelemetry telemetry)
		{
			_innerClient.TrackDependency(telemetry);
		}

		public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string? message = null, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
		{
			_innerClient.TrackAvailability(name, timeStamp, duration, runLocation, success, message, properties, metrics);
		}

		public void TrackAvailability(AvailabilityTelemetry telemetry)
		{
			_innerClient.TrackAvailability(telemetry);
		}

		public void Track(ITelemetry telemetry)
		{
			_innerClient.Track(telemetry);
		}

		public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
		{
			_innerClient.TrackRequest(name, startTime, duration, responseCode, success);
		}

		public void TrackRequest(RequestTelemetry request)
		{
			_innerClient.TrackRequest(request);
		}

		public void Flush()
		{
			_innerClient.Flush();
		}

		public async Task<bool> FlushAsync(CancellationToken cancellationToken)
		{
			return await _innerClient.FlushAsync(cancellationToken);
		}

		public IOperationHolder<T> StartOperation<T>(string operationName) where T : OperationTelemetry, new()
		{
			return _innerClient.StartOperation<T>(operationName);
		}
	}
}
