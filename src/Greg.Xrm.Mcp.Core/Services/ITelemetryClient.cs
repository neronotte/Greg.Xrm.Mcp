using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace Greg.Xrm.Mcp.Core.Services
{
	/// <summary>
	/// Represents a telemetry client for tracking application telemetry data.
	/// </summary>
	public interface ITelemetryClient
	{
		/// <summary>
		/// Tracks a custom event with the given name.
		/// </summary>
		/// <param name="eventName">The name of the event to track.</param>
		/// <param name="properties">Named string values you can use to search and classify events.</param>
		/// <param name="metrics">Measurements associated with this event.</param>
		void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);
		
		/// <summary>
		/// Tracks a custom event telemetry.
		/// </summary>
		/// <param name="telemetry">The event telemetry to track.</param>
		void TrackEvent(EventTelemetry telemetry);
		
		/// <summary>
		/// Tracks a trace message.
		/// </summary>
		/// <param name="message">The trace message to track.</param>
		void TrackTrace(string message);
		
		/// <summary>
		/// Tracks a trace message with the specified severity level.
		/// </summary>
		/// <param name="message">The trace message to track.</param>
		/// <param name="severityLevel">The severity level of the trace message.</param>
		void TrackTrace(string message, SeverityLevel severityLevel);
		
		/// <summary>
		/// Tracks a trace message with custom properties.
		/// </summary>
		/// <param name="message">The trace message to track.</param>
		/// <param name="properties">Named string values you can use to search and classify traces.</param>
		void TrackTrace(string message, IDictionary<string, string> properties);
		
		/// <summary>
		/// Tracks a trace message with the specified severity level and custom properties.
		/// </summary>
		/// <param name="message">The trace message to track.</param>
		/// <param name="severityLevel">The severity level of the trace message.</param>
		/// <param name="properties">Named string values you can use to search and classify traces.</param>
		void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties);
		
		/// <summary>
		/// Tracks a trace telemetry.
		/// </summary>
		/// <param name="telemetry">The trace telemetry to track.</param>
		void TrackTrace(TraceTelemetry telemetry);
		
		/// <summary>
		/// Tracks a metric with the given name and value.
		/// </summary>
		/// <param name="name">The name of the metric to track.</param>
		/// <param name="value">The value of the metric.</param>
		/// <param name="properties">Named string values you can use to search and classify metrics.</param>
		void TrackMetric(string name, double value, IDictionary<string, string>? properties = null);
		
		/// <summary>
		/// Tracks a metric telemetry.
		/// </summary>
		/// <param name="telemetry">The metric telemetry to track.</param>
		void TrackMetric(MetricTelemetry telemetry);
		
		/// <summary>
		/// Tracks an exception that occurred in your application.
		/// </summary>
		/// <param name="exception">The exception to track.</param>
		/// <param name="properties">Named string values you can use to search and classify exceptions.</param>
		/// <param name="metrics">Measurements associated with this exception.</param>
		void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

		/// <summary>
		/// Tracks an exception telemetry.
		/// </summary>
		/// <param name="telemetry">The exception telemetry to track.</param>
		void TrackException(ExceptionTelemetry telemetry);
		
		/// <summary>
		/// Tracks a dependency (calls to external services).
		/// </summary>
		/// <param name="dependencyTypeName">The name of the dependency type.</param>
		/// <param name="dependencyName">The name of the dependency.</param>
		/// <param name="data">The data associated with the dependency call.</param>
		/// <param name="startTime">The time when the dependency call started.</param>
		/// <param name="duration">The duration of the dependency call.</param>
		/// <param name="success">True if the dependency call was successful; otherwise, false.</param>
		void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success);
		
		/// <summary>
		/// Tracks a dependency (calls to external services) with additional target and result code information.
		/// </summary>
		/// <param name="dependencyTypeName">The name of the dependency type.</param>
		/// <param name="target">The target of the dependency call.</param>
		/// <param name="dependencyName">The name of the dependency.</param>
		/// <param name="data">The data associated with the dependency call.</param>
		/// <param name="startTime">The time when the dependency call started.</param>
		/// <param name="duration">The duration of the dependency call.</param>
		/// <param name="resultCode">The result code of the dependency call.</param>
		/// <param name="success">True if the dependency call was successful; otherwise, false.</param>
		void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success);
		
		/// <summary>
		/// Tracks a dependency telemetry.
		/// </summary>
		/// <param name="telemetry">The dependency telemetry to track.</param>
		void TrackDependency(DependencyTelemetry telemetry);
		
		/// <summary>
		/// Tracks availability test results.
		/// </summary>
		/// <param name="name">The name of the availability test.</param>
		/// <param name="timeStamp">The time when the availability test was executed.</param>
		/// <param name="duration">The duration of the availability test.</param>
		/// <param name="runLocation">The location where the test was run.</param>
		/// <param name="success">True if the availability test passed; otherwise, false.</param>
		/// <param name="message">Optional message associated with the availability test.</param>
		/// <param name="properties">Named string values you can use to search and classify availability tests.</param>
		/// <param name="metrics">Measurements associated with this availability test.</param>
		void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string? message = null, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);
		
		/// <summary>
		/// Tracks an availability telemetry.
		/// </summary>
		/// <param name="telemetry">The availability telemetry to track.</param>
		void TrackAvailability(AvailabilityTelemetry telemetry);
		
		/// <summary>
		/// Tracks a generic telemetry item.
		/// </summary>
		/// <param name="telemetry">The telemetry item to track.</param>
		void Track(ITelemetry telemetry);
		
		/// <summary>
		/// Tracks an incoming request.
		/// </summary>
		/// <param name="name">The name of the request.</param>
		/// <param name="startTime">The time when the request started.</param>
		/// <param name="duration">The duration of the request.</param>
		/// <param name="responseCode">The response code of the request.</param>
		/// <param name="success">True if the request was successful; otherwise, false.</param>
		void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success);
		
		/// <summary>
		/// Tracks a request telemetry.
		/// </summary>
		/// <param name="request">The request telemetry to track.</param>
		void TrackRequest(RequestTelemetry request);
		
		/// <summary>
		/// Flushes the in-memory buffer and any metrics being pre-aggregated.
		/// </summary>
		void Flush();
		
		/// <summary>
		/// Flushes the in-memory buffer and any metrics being pre-aggregated asynchronously.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to cancel the flush operation.</param>
		/// <returns>A task that represents the asynchronous flush operation. The task result contains a boolean indicating whether the flush was successful.</returns>
		Task<bool> FlushAsync(CancellationToken cancellationToken);


		IOperationHolder<T> StartOperation<T>(string operationName)
			where T : OperationTelemetry, new();

	}
}
