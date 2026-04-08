using Greg.Xrm.Mcp.FormEngineer.Model;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers
{
	/// <summary>
	/// Classe statica con dati di test comunemente utilizzati
	/// </summary>
	public static class TestDataProvider
	{
		/// <summary>
		/// XML di form valido per test di validazione
		/// </summary>
		public static string ValidFormXml => @"<?xml version=""1.0"" encoding=""utf-8""?>
<form>
	<tabs>
		<tab name=""general"" expanded=""true"">
			<labels>
				<label description=""General"" languagecode=""1033"" />
			</labels>
			<columns>
				<column width=""100%"">
					<sections>
						<section name=""section_1"" showlabel=""true"" showbar=""true"">
							<labels>
								<label description=""Section 1"" languagecode=""1033"" />
							</labels>
							<rows>
								<row>
									<cell id=""{4273EDBD-AC1D-40d3-9FB2-095C621B552D}"">
										<labels>
											<label description=""Account Name"" languagecode=""1033"" />
										</labels>
										<control id=""{4273EDBD-AC1D-40d3-9FB2-095C621B552D}"" classid=""{4273EDBD-AC1D-40d3-9FB2-095C621B552D}"">
											<parameters>
												<DefaultValue></DefaultValue>
											</parameters>
										</control>
									</cell>
								</row>
							</rows>
						</section>
					</sections>
				</column>
			</columns>
		</tab>
	</tabs>
</form>";

		/// <summary>
		/// XML di form malformato per test di gestione errori
		/// </summary>
		public static string MalformedFormXml => "<form><tab><unclosed>";

		/// <summary>
		/// XML di form vuoto per test edge case
		/// </summary>
		public static string EmptyFormXml => "<form></form>";

		/// <summary>
		/// Lista di entit� di test comuni
		/// </summary>
		public static readonly string[] CommonTestEntities =
		{
			"account",
			"contact",
			"lead",
			"opportunity",
			"incident",
			"task"
		};

		/// <summary>
		/// Lista di tipi di form per test
		/// </summary>
		public static readonly systemform_type[] CommonFormTypes =
		{
			systemform_type.Main,
			systemform_type.QuickCreate,
			systemform_type.QuickViewForm,
			systemform_type.Card
		};

		/// <summary>
		/// GUIDs predefiniti per test deterministici
		/// </summary>
		public static class TestGuids
		{
			public static readonly Guid FormId1 = new("11111111-1111-1111-1111-111111111111");
			public static readonly Guid FormId2 = new("22222222-2222-2222-2222-222222222222");
			public static readonly Guid FormId3 = new("33333333-3333-3333-3333-333333333333");

			public static readonly Guid ChartId1 = new("44444444-4444-4444-4444-444444444444");
			public static readonly Guid ChartId2 = new("55555555-5555-5555-5555-555555555555");
			public static readonly Guid ChartId3 = new("66666666-6666-6666-6666-666666666666");
		}

		/// <summary>
		/// Messaggi di errore comuni per test di validazione
		/// </summary>
		public static class ErrorMessages
		{
			public const string InvalidGuid = "Invalid formId";
			public const string FormNotFound = "No form found with ID";
			public const string ChartNotFound = "No chart found with ID";
			public const string EmptyXml = "empty or null";
			public const string RootMissing = "Root element is missing";
			public const string ValidationFailed = "Form XML validation failed";
		}

		/// <summary>
		/// Valid chart DataDescription XML for tests
		/// </summary>
		public static string ValidChartDataDescription => @"<datadefinition>
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

		/// <summary>
		/// Valid chart PresentationDescription XML for tests
		/// </summary>
		public static string ValidChartPresentationDescription => @"<Chart>
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
}
