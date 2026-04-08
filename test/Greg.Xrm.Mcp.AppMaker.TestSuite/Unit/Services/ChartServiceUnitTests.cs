using Greg.Xrm.Mcp.FormEngineer.Model;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers;
using Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using ModelContextProtocol;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.Unit.Services
{
	/// <summary>
	/// Unit tests for ChartService
	/// Tests business logic without real Dataverse connections
	/// </summary>
	[TestFixture]
	public class ChartServiceUnitTests : UnitTestBase
	{
		private ChartService _chartService;
		private ILogger<ChartService> _logger;
		private IOrganizationServiceAsync2 _mockClient;

		[SetUp]
		public void Setup()
		{
			_logger = CreateMockLogger<ChartService>();
			_chartService = new ChartService(_logger);
			_mockClient = Substitute.For<IOrganizationServiceAsync2>();
		}

		[TestFixture]
		public class GetChartsAsync : ChartServiceUnitTests
		{
			[Test]
			public async Task WhenEntityExists_ReturnsChartsList()
			{
				// Arrange
				const string existingEntity = "account";
				var testCharts = SavedQueryVisualizationBuilder.CreateList(2);

				SetupMockClientForCharts(existingEntity, testCharts);

				// Act
				var result = await _chartService.GetChartsAsync(_mockClient, existingEntity);

				// Assert
				Assert.That(result, Has.Count.EqualTo(2));
				Assert.That(result[0].Name, Is.EqualTo(testCharts[0].Name));
				Assert.That(result[1].Name, Is.EqualTo(testCharts[1].Name));
			}

			[Test]
			public async Task WhenNoChartsExist_ReturnsEmptyList()
			{
				// Arrange
				const string entityWithNoCharts = "customentity";

				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns(new EntityCollection()); // Empty collection

				// Act
				var result = await _chartService.GetChartsAsync(_mockClient, entityWithNoCharts);

				// Assert
				Assert.That(result, Is.Empty);
			}

			[Test]
			public async Task WhenChartNameFilterProvided_ReturnsFilteredCharts()
			{
				// Arrange
				const string entityName = "account";
				const string chartNameFilter = "Revenue";
				var testCharts = new List<SavedQueryVisualization>
				{
					new SavedQueryVisualizationBuilder()
						.WithName("Revenue Analysis")
						.WithPrimaryEntityTypeCode(entityName)
						.Build(),
					new SavedQueryVisualizationBuilder()
						.WithName("Revenue by Region")
						.WithPrimaryEntityTypeCode(entityName)
						.Build()
				};

				SetupMockClientForCharts(entityName, testCharts);

				// Act
				var result = await _chartService.GetChartsAsync(_mockClient, entityName, chartNameFilter);

				// Assert
				Assert.That(result, Has.Count.EqualTo(2));
				Assert.That(result.All(c => c.Name.Contains("Revenue")), Is.True);
			}

			[Test]
			public void WhenCancellationRequested_ThrowsMcpException()
			{
				// Arrange
				using var cts = new CancellationTokenSource();
				cts.Cancel();

				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns(Task.FromCanceled<EntityCollection>(cts.Token));

				// Act & Assert
				Assert.ThrowsAsync<McpException>(async () =>
					await _chartService.GetChartsAsync(_mockClient, "account", cancellationToken: cts.Token));
			}

			[Test]
			public async Task WhenMultipleChartsWithDifferentTypes_ReturnsAllCharts()
			{
				// Arrange
				const string entityName = "account";
				var testCharts = new List<SavedQueryVisualization>
				{
					new SavedQueryVisualizationBuilder()
						.WithChartType(savedqueryvisualization_charttype.ASPNETChart)
						.WithPrimaryEntityTypeCode(entityName)
						.Build(),
					new SavedQueryVisualizationBuilder()
						.WithChartType(savedqueryvisualization_charttype.PowerBIEmbedded)
						.WithPrimaryEntityTypeCode(entityName)
						.Build()
				};

				SetupMockClientForCharts(entityName, testCharts);

				// Act
				var result = await _chartService.GetChartsAsync(_mockClient, entityName);

				// Assert
				Assert.That(result, Has.Count.EqualTo(2));
				Assert.That(result.Any(c => c.ChartType == savedqueryvisualization_charttype.ASPNETChart), Is.True);
				Assert.That(result.Any(c => c.ChartType == savedqueryvisualization_charttype.PowerBIEmbedded), Is.True);
			}

			private void SetupMockClientForCharts(string entityName, List<SavedQueryVisualization> charts)
			{
				var entityCollection = new EntityCollection();
				foreach (var chart in charts)
				{
					var entity = new Entity("savedqueryvisualization", chart.Id)
					{
						["savedqueryvisualizationid"] = chart.Id,
						["name"] = chart.Name,
						["primaryentitytypecode"] = chart.PrimaryEntityTypeCode,
						["description"] = chart.Description,
						["isdefault"] = chart.IsDefault,
						["charttype"] = new OptionSetValue((int)chart.ChartType.GetValueOrDefault()),
						["datadescription"] = chart.DataDescription,
						["presentationdescription"] = chart.PresentationDescription
					};
					entityCollection.Entities.Add(entity);
				}

				_mockClient.RetrieveMultipleAsync(Arg.Is<QueryExpression>(q =>
						q.EntityName == "savedqueryvisualization"))
					.Returns(entityCollection);
			}
		}

		[TestFixture]
		public class GetChartByIdAsync : ChartServiceUnitTests
		{
			[Test]
			public async Task WhenChartExists_ReturnsChart()
			{
				// Arrange
				var testChart = new SavedQueryVisualizationBuilder()
					.WithId(TestDataProvider.TestGuids.ChartId1)
					.WithName("Test Chart")
					.Build();

				var entity = CreateEntityFromChart(testChart);
				_mockClient.RetrieveAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ColumnSet>())
					.Returns(entity);

				// Act
				var result = await _chartService.GetChartByIdAsync(_mockClient, testChart.Id);

				// Assert
				Assert.That(result, Is.Not.Null);
				Assert.That(result!.Id, Is.EqualTo(testChart.Id));
				Assert.That(result.Name, Is.EqualTo(testChart.Name));
			}

			[Test]
			public async Task WhenChartDoesNotExist_ReturnsNull()
			{
				// Arrange
				_mockClient.RetrieveAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ColumnSet>())
					.Returns(Task.FromResult<Entity>(null!));

				// Act
				var result = await _chartService.GetChartByIdAsync(_mockClient, TestDataProvider.TestGuids.ChartId1);

				// Assert
				Assert.That(result, Is.Null);
			}

			[Test]
			public async Task WhenChartExists_ReturnsCompleteChartData()
			{
				// Arrange
				var testChart = new SavedQueryVisualizationBuilder()
					.WithId(TestDataProvider.TestGuids.ChartId1)
					.WithName("Revenue Chart")
					.WithPrimaryEntityTypeCode("account")
					.WithDataDescription(TestDataProvider.ValidChartDataDescription)
					.WithPresentationDescription(TestDataProvider.ValidChartPresentationDescription)
					.AsDefault()
					.Build();

				var entity = CreateEntityFromChart(testChart);
				_mockClient.RetrieveAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ColumnSet>())
					.Returns(entity);

				// Act
				var result = await _chartService.GetChartByIdAsync(_mockClient, testChart.Id);

				// Assert
				Assert.That(result, Is.Not.Null);
				Assert.That(result!.Id, Is.EqualTo(testChart.Id));
				Assert.That(result.Name, Is.EqualTo("Revenue Chart"));
				Assert.That(result.PrimaryEntityTypeCode, Is.EqualTo("account"));
				Assert.That(result.DataDescription, Is.Not.Null.And.Not.Empty);
				Assert.That(result.PresentationDescription, Is.Not.Null.And.Not.Empty);
				Assert.That(result.IsDefault, Is.True);
			}

			[Test]
			public void WhenExceptionOccurs_ThrowsMcpException()
			{
				// Arrange
				_mockClient.RetrieveAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ColumnSet>())
					.Returns<Entity>(x => throw new Exception("Database connection failed"));

				// Act & Assert
				var exception = Assert.ThrowsAsync<McpException>(async () =>
					await _chartService.GetChartByIdAsync(_mockClient, TestDataProvider.TestGuids.ChartId1));

				Assert.That(exception.Message, Does.Contain("Error retrieving chart"));
			}

			private static Entity CreateEntityFromChart(SavedQueryVisualization chart)
			{
				return new Entity("savedqueryvisualization", chart.Id)
				{
					["savedqueryvisualizationid"] = chart.Id,
					["name"] = chart.Name,
					["primaryentitytypecode"] = chart.PrimaryEntityTypeCode,
					["description"] = chart.Description,
					["isdefault"] = chart.IsDefault,
					["charttype"] = new OptionSetValue((int)chart.ChartType!),
					["datadescription"] = chart.DataDescription,
					["presentationdescription"] = chart.PresentationDescription
				};
			}
		}

		[TestFixture]
		public class ErrorHandling : ChartServiceUnitTests
		{
			[Test]
			public void GetChartsAsync_WhenServiceThrowsException_ThrowsMcpException()
			{
				// Arrange
				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns<EntityCollection>(x => throw new Exception("Service error"));

				// Act & Assert
				var exception = Assert.ThrowsAsync<McpException>(async () =>
					await _chartService.GetChartsAsync(_mockClient, "account"));

				Assert.That(exception.Message, Does.Contain("Service error"));
			}

			[Test]
			public async Task GetChartsAsync_WhenChartDataIsPartiallyMissing_StillReturnsCharts()
			{
				// Arrange
				const string entityName = "account";
				var entityCollection = new EntityCollection();

				// Create chart entity with missing optional fields
				var chartEntity = new Entity("savedqueryvisualization", Guid.NewGuid())
				{
					["savedqueryvisualizationid"] = Guid.NewGuid(),
					["name"] = "Incomplete Chart",
					["primaryentitytypecode"] = entityName
					// Missing: datadescription, presentationdescription, description
				};
				entityCollection.Entities.Add(chartEntity);

				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns(entityCollection);

				// Act
				var result = await _chartService.GetChartsAsync(_mockClient, entityName);

				// Assert
				Assert.That(result, Has.Count.EqualTo(1));
				Assert.That(result[0].Name, Is.EqualTo("Incomplete Chart"));
			}
		}

		[TestFixture]
		public class PerformanceTests : ChartServiceUnitTests
		{
			[Test]
			public async Task GetChartsAsync_WithLargeResultSet_CompletesInReasonableTime()
			{
				// Arrange
				const string entityName = "account";
				var largeChartList = SavedQueryVisualizationBuilder.CreateList(100);

				var entityCollection = new EntityCollection();
				foreach (var chart in largeChartList)
				{
					var entity = new Entity("savedqueryvisualization", chart.Id)
					{
						["savedqueryvisualizationid"] = chart.Id,
						["name"] = chart.Name,
						["primaryentitytypecode"] = entityName,
						["charttype"] = new OptionSetValue(0)
					};
					entityCollection.Entities.Add(entity);
				}

				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns(entityCollection);

				var stopwatch = System.Diagnostics.Stopwatch.StartNew();

				// Act
				var result = await _chartService.GetChartsAsync(_mockClient, entityName);

				// Assert
				stopwatch.Stop();
				Assert.That(result, Has.Count.EqualTo(100));
				Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000)); // Should complete in less than 1 second
			}
		}
	}
}
