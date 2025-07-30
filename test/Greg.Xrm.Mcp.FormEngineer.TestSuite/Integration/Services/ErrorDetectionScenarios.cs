using Greg.Xrm.Mcp.FormEngineer.Services;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.Integration.Services
{
public partial class FormXmlValidatorIntegrationTests
	{
		[TestFixture]
		public class ErrorDetectionScenarios : FormXmlValidatorIntegrationTests
		{
			[Test]
			public void FormXmlWithMissingRequiredAttributes_ReturnsValidationErrors()
			{
				// Arrange - XML con attributi mancanti
				var invalidXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<form>
	<tabs>
		<tab>
			<columns>
				<column>
					<sections>
						<section>
							<rows>
								<row>
									<cell>
										<control classid=""{4273EDBD-AC1D-40d3-9FB2-095C621B552D}"" />
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

				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(invalidXml);

				// Assert
				Assert.That(result.IsValid, Is.False);
				Assert.That(result.Count(r => r.Level == FormXmlValidationLevel.Error), Is.GreaterThan(0));
			}

			[Test]
			public void FormXmlWithInvalidStructure_ReturnsStructuralErrors()
			{
				// Arrange - Struttura XML non valida per form
				var invalidStructureXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<form>
	<invalidElement>
		<anotherInvalid />
	</invalidElement>
</form>";

				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(invalidStructureXml);

				// Assert
				Assert.That(result.IsValid, Is.False);
				Assert.That(result.Count(r => r.Level == FormXmlValidationLevel.Error), Is.GreaterThan(0));
			}
		}
	}
}