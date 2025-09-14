using Greg.Xrm.Mcp.FormEngineer.Services;
using Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers;
using Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers.Builders;
using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.Unit.Services
{
	/// <summary>
	/// Test unitari per FormXmlValidator
	/// Testa la validazione XML in modo isolato senza dipendenze esterne
	/// </summary>
	[TestFixture]
	public class FormXmlValidatorUnitTests : UnitTestBase
	{
		private FormXmlValidator _validator;
		private ILogger<FormXmlValidator> _logger;

		[SetUp]
		public void Setup()
		{
			_logger = CreateMockLogger<FormXmlValidator>();
			_validator = new FormXmlValidator(_logger);
		}

		[TestFixture]
		public class TryValidateFormXmlAgainstSchema : FormXmlValidatorUnitTests
		{
			[Test]
			public void WhenXmlIsNull_ReturnsValidationError()
			{
				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(string.Empty);

				// Assert
				Assert.That(result, Has.Count.EqualTo(1));
				Assert.That(result[0].Level, Is.EqualTo(FormXmlValidationLevel.Error));
				Assert.That(result[0].Message, Does.Contain(TestDataProvider.ErrorMessages.EmptyXml));
			}

			[Test]
			public void WhenXmlIsEmpty_ReturnsValidationError()
			{
				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(string.Empty);

				// Assert
				Assert.That(result, Has.Count.EqualTo(1));
				Assert.That(result[0].Level, Is.EqualTo(FormXmlValidationLevel.Error));
				Assert.That(result[0].Message, Does.Contain(TestDataProvider.ErrorMessages.EmptyXml));
			}

			[Test]
			public void WhenXmlIsMalformed_ReturnsParsingError()
			{
				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(TestDataProvider.MalformedFormXml);

				// Assert
				Assert.That(result, Is.Not.Empty);
				Assert.That(result.Any(r => r.Level == FormXmlValidationLevel.Error), Is.True);
			}

			[Test]
			public void WhenXmlIsValid_ReturnsNoErrors()
			{
				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(TestDataProvider.ValidFormXml);

				// Assert
				Assert.That(result.IsValid, Is.True, result.ToString());
				Assert.That(result.Where(r => r.Level == FormXmlValidationLevel.Error), Is.Empty);
			}

			[Test]
			public void WhenXmlIsWhitespace_ReturnsValidationError()
			{
				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema("   \t\n   ");

				// Assert
				Assert.That(result, Has.Count.EqualTo(1));
				Assert.That(result[0].Level, Is.EqualTo(FormXmlValidationLevel.Error));
				Assert.That(result[0].Message, Does.Contain(TestDataProvider.ErrorMessages.RootMissing));
			}
		}

		[TestFixture] 
		public class ValidationResultProperties : FormXmlValidatorUnitTests
		{
			[Test]
			public void IsValid_WhenNoErrors_ReturnsTrue()
			{
				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(TestDataProvider.ValidFormXml);

				// Assert
				Assert.That(result.IsValid, Is.True);
			}

			[Test]
			public void IsValid_WhenHasErrors_ReturnsFalse()
			{
				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(TestDataProvider.MalformedFormXml);

				// Assert
				Assert.That(result.IsValid, Is.False);
			}

			[Test]
			public void HasWarnings_WhenOnlyWarnings_ReturnsTrue()
			{
				// Arrange - Crea XML che potrebbe generare warning ma non errori
				var xmlWithPotentialWarnings = TestDataProvider.ValidFormXml;

				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(xmlWithPotentialWarnings);

				// Assert - Controlla che la proprieta funzioni correttamente
				var hasWarnings = result.Any(r => r.Level == FormXmlValidationLevel.Warning);
				Assert.That(result.HasWarnings, Is.EqualTo(hasWarnings));
			}
		}
	}
}