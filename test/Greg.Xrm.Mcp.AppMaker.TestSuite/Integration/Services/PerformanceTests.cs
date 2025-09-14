namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.Integration.Services
{
public partial class FormXmlValidatorIntegrationTests
	{
		[TestFixture]
		public class PerformanceTests : FormXmlValidatorIntegrationTests
		{
			[Test]
			public void ValidatingLargeFormXml_CompletesInReasonableTime()
			{
				// Arrange - Crea un XML molto grande
				var largeXml = CreateLargeFormXml(50); // 50 tab con molti controlli

				var stopwatch = System.Diagnostics.Stopwatch.StartNew();

				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(largeXml);

				// Assert
				stopwatch.Stop();
				Assert.That(result, Is.Not.Null);
				Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000)); // Meno di 5 secondi
			}

			private static string CreateLargeFormXml(int tabCount)
			{
				var xml = new System.Text.StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?><form><tabs>");

				for (int i = 0; i < tabCount; i++)
				{
					xml.Append($@"
					<tab name=""tab_{i}"">
						<labels><label description=""Tab {i}"" languagecode=""1033"" /></labels>
						<columns>
							<column width=""100%"">
								<sections>
									<section name=""section_{i}"">
										<labels><label description=""Section {i}"" languagecode=""1033"" /></labels>
										<rows>");

					// Aggiungi molti controlli per tab
					for (int j = 0; j < 10; j++)
					{
						xml.Append($@"
							<row>
								<cell id=""control_{i}_{j}"">
									<labels><label description=""Control {i}_{j}"" languagecode=""1033"" /></labels>
									<control id=""control_{i}_{j}"" classid=""{{4273EDBD-AC1D-40d3-9FB2-095C621B552D}}"" />
								</cell>
							</row>");
					}

					xml.Append(@"
										</rows>
									</section>
								</sections>
							</column>
						</columns>
					</tab>");
				}

				xml.Append("</tabs></form>");
				return xml.ToString();
			}
		}
	}
}