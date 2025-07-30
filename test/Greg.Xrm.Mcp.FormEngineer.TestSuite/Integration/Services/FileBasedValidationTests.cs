using Greg.Xrm.Mcp.FormEngineer.Services;
using Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers;
using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.Integration.Services
{
	/// <summary>
	/// Test di integrazione che utilizzano file XML reali dal filesystem
	/// </summary>
	[TestFixture]
	public class FileBasedValidationTests : IntegrationTestBase
	{
		private FormXmlValidator _validator;
		private ILogger<FormXmlValidator> _logger;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			TestFileLoader.EnsureTestDataDirectoryExists();
		}

		[SetUp]
		public void Setup()
		{
			_logger = CreateMockLogger<FormXmlValidator>();
			_validator = new FormXmlValidator(_logger);
		}

		[Test]
		public void ValidFormXmlFromFile_ValidatesSuccessfully()
		{
			// Arrange
			var formXml = TestFileLoader.LoadValidFormXml("AccountMainForm.xml");

			// Act
			var result = _validator.TryValidateFormXmlAgainstSchema(formXml);

			// Assert
			Assert.That(result.IsValid, Is.True);
			Assert.That(result.Where(r => r.Level == FormXmlValidationLevel.Error), Is.Empty);
		}

		[Test]
		public void InvalidFormXmlFromFile_ReturnsValidationErrors()
		{
			// Arrange
			var formXml = TestFileLoader.LoadInvalidFormXml("MalformedForm.xml");

			// Act
			var result = _validator.TryValidateFormXmlAgainstSchema(formXml);

			// Assert
			Assert.That(result.IsValid, Is.False);
			Assert.That(result.Any(r => r.Level == FormXmlValidationLevel.Error), Is.True);
		}

		[Test]
		[TestCaseSource(nameof(GetAllValidFormXmlFiles))]
		public void AllValidFormXmlFiles_ValidateSuccessfully((string fileName, string content) testCase)
		{
			// Act
			var result = _validator.TryValidateFormXmlAgainstSchema(testCase.content);

			// Assert
			Assert.That(result.IsValid, Is.True, $"File {testCase.fileName} should validate successfully");
		}

		[Test]
		[TestCaseSource(nameof(GetAllInvalidFormXmlFiles))]
		public void AllInvalidFormXmlFiles_ReturnValidationErrors((string fileName, string content) testCase)
		{
			// Act
			var result = _validator.TryValidateFormXmlAgainstSchema(testCase.content);

			// Assert
			Assert.That(result.IsValid, Is.False, $"File {testCase.fileName} should fail validation");
		}

		private static (string fileName, string content)[] GetAllValidFormXmlFiles()
		{
			return TestFileLoader.LoadAllValidFormXml();
		}

		private static (string fileName, string content)[] GetAllInvalidFormXmlFiles()
		{
			return TestFileLoader.LoadAllInvalidFormXml();
		}
	}
}