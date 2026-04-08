using Greg.Xrm.Mcp.FormEngineer.Model;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers.Builders
{
	/// <summary>
	/// Builder for creating SavedQueryVisualization objects for tests
	/// Uses Builder pattern to make tests more readable
	/// </summary>
	public class SavedQueryVisualizationBuilder
	{
		private readonly SavedQueryVisualization _chart;
		private static int _idCounter = 1;

		public SavedQueryVisualizationBuilder()
		{
			_chart = new SavedQueryVisualization
			{
				Id = CreateUniqueGuid(),
				Name = $"Test Chart {_idCounter}",
				PrimaryEntityTypeCode = "account",
				ChartType = savedqueryvisualization_charttype.ASPNETChart,
				DataDescription = CreateValidDataDescriptionXml(),
				PresentationDescription = CreateValidPresentationDescriptionXml(),
				IsDefault = false,
				Description = $"Test chart description {_idCounter}"
			};
			_idCounter++;
		}

		public SavedQueryVisualizationBuilder WithId(Guid id)
		{
			_chart.Id = id;
			return this;
		}

		public SavedQueryVisualizationBuilder WithName(string name)
		{
			_chart.Name = name;
			return this;
		}

		public SavedQueryVisualizationBuilder WithPrimaryEntityTypeCode(string entityTypeCode)
		{
			_chart.PrimaryEntityTypeCode = entityTypeCode;
			return this;
		}

		public SavedQueryVisualizationBuilder WithChartType(savedqueryvisualization_charttype chartType)
		{
			_chart.ChartType = chartType;
			return this;
		}

		public SavedQueryVisualizationBuilder WithDataDescription(string dataDescription)
		{
			_chart.DataDescription = dataDescription;
			return this;
		}

		public SavedQueryVisualizationBuilder WithPresentationDescription(string presentationDescription)
		{
			_chart.PresentationDescription = presentationDescription;
			return this;
		}

		public SavedQueryVisualizationBuilder AsDefault()
		{
			_chart.IsDefault = true;
			return this;
		}

		public SavedQueryVisualizationBuilder WithDescription(string description)
		{
			_chart.Description = description;
			return this;
		}

		public SavedQueryVisualization Build()
		{
			return _chart;
		}

		/// <summary>
		/// Creates a unique but deterministic GUID for tests
		/// </summary>
		private static Guid CreateUniqueGuid()
		{
			var bytes = new byte[16];
			BitConverter.GetBytes(_idCounter).CopyTo(bytes, 0);
			BitConverter.GetBytes(DateTime.UtcNow.Ticks).CopyTo(bytes, 8);
			return new Guid(bytes);
		}

		/// <summary>
		/// Creates valid DataDescription XML for tests (FetchXML with aggregation)
		/// </summary>
		private static string CreateValidDataDescriptionXml()
		{
			return @"<datadefinition>
				<fetchcollection>
					<fetch mapping=""logical"" aggregate=""true"">
						<entity name=""account"">
							<attribute name=""accountid"" alias=""accountid"" groupby=""true"" />
							<attribute name=""name"" alias=""name"" groupby=""true"" />
							<attribute name=""revenue"" alias=""sum_revenue"" aggregate=""sum"" />
						</entity>
					</fetch>
				</fetchcollection>
				<categorycollection>
					<category>
						<measurecollection>
							<measure alias=""sum_revenue"" />
						</measurecollection>
					</category>
				</categorycollection>
			</datadefinition>";
		}

		/// <summary>
		/// Creates valid PresentationDescription XML for tests
		/// </summary>
		private static string CreateValidPresentationDescriptionXml()
		{
			return @"<Chart>
				<Series>
					<Series ChartType=""Column"" IsValueShownAsLabel=""True"">
						<Points />
					</Series>
				</Series>
				<ChartAreas>
					<ChartArea>
						<AxisY LabelAutoFitMaxFontSize=""8"">
							<MajorGrid LineColor=""LightGray"" />
						</AxisY>
						<AxisX LabelAutoFitMaxFontSize=""8"">
							<MajorGrid Enabled=""False"" />
						</AxisX>
					</ChartArea>
				</ChartAreas>
				<Titles>
					<Title Name=""Title1"" Alignment=""TopLeft"" />
				</Titles>
			</Chart>";
		}

		/// <summary>
		/// Creates invalid DataDescription XML for validation tests
		/// </summary>
		public SavedQueryVisualizationBuilder WithInvalidDataDescription()
		{
			_chart.DataDescription = "<datadefinition><fetch><unclosed>";
			return this;
		}

		/// <summary>
		/// Creates a list of SavedQueryVisualization for multiple tests
		/// </summary>
		/// <param name="count">Number of charts to create</param>
		/// <returns>List of SavedQueryVisualization with test data</returns>
		public static List<SavedQueryVisualization> CreateList(int count = 3)
		{
			var charts = new List<SavedQueryVisualization>();
			for (int i = 0; i < count; i++)
			{
				charts.Add(new SavedQueryVisualizationBuilder()
					.WithName($"Test Chart {i + 1}")
					.WithDescription($"Test chart description {i + 1}")
					.Build());
			}
			return charts;
		}

		/// <summary>
		/// Creates a list of SavedQueryVisualization with predefined IDs for deterministic tests
		/// </summary>
		/// <param name="count">Number of charts to create</param>
		/// <returns>List of SavedQueryVisualization with deterministic IDs</returns>
		public static List<SavedQueryVisualization> CreateListWithPredefinedIds(int count = 3)
		{
			var charts = new List<SavedQueryVisualization>();
			var predefinedIds = new[]
			{
				TestDataProvider.TestGuids.ChartId1,
				TestDataProvider.TestGuids.ChartId2,
				TestDataProvider.TestGuids.ChartId3
			};

			for (int i = 0; i < count && i < predefinedIds.Length; i++)
			{
				charts.Add(new SavedQueryVisualizationBuilder()
					.WithId(predefinedIds[i])
					.WithName($"Test Chart {i + 1}")
					.WithDescription($"Test chart description {i + 1}")
					.Build());
			}
			return charts;
		}

		/// <summary>
		/// Creates SavedQueryVisualization for specific entities with appropriate data
		/// </summary>
		/// <param name="entityLogicalName">Logical name of the entity</param>
		/// <param name="chartType">Chart type</param>
		/// <returns>SavedQueryVisualization configured for the entity</returns>
		public static SavedQueryVisualization CreateForEntity(
			string entityLogicalName,
			savedqueryvisualization_charttype chartType = savedqueryvisualization_charttype.ASPNETChart)
		{
			return new SavedQueryVisualizationBuilder()
				.WithPrimaryEntityTypeCode(entityLogicalName)
				.WithChartType(chartType)
				.WithName($"{entityLogicalName} Chart")
				.WithDescription($"Test chart for {entityLogicalName}")
				.Build();
		}
	}
}
