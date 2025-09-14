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
	/// Test unitari per FormService
	/// Testa la logica di business senza connessioni reali a Dataverse
	/// </summary>
	[TestFixture]
	public class FormServiceUnitTests : UnitTestBase
	{
		private FormService _formService;
		private ILogger<FormService> _logger;
		private IOrganizationServiceAsync2 _mockClient;

		[SetUp]
		public void Setup()
		{
			_logger = CreateMockLogger<FormService>();
			_formService = new FormService(_logger);
			_mockClient = Substitute.For<IOrganizationServiceAsync2>();
		}

		[TestFixture]
		public class GetFormsAsync : FormServiceUnitTests
		{
			[Test]
			public void WhenEntityDoesNotExist_ThrowsArgumentException()
			{
				// Arrange
				const string nonExistentEntity = "nonexistent";
				
				// Configura il mock per simulare che l'entità non esista
				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns(new EntityCollection()); // Nessun risultato = entità non esiste

				// Act & Assert
				var exception = Assert.ThrowsAsync<McpException>(async () =>
					await _formService.GetFormsAsync(_mockClient, nonExistentEntity));

				Assert.That(exception.Message, Does.Contain($"La tabella '{nonExistentEntity}' non esiste"));
			}

			[Test]
			public async Task WhenEntityExists_ReturnsFormsList()
			{
				// Arrange
				const string existingEntity = "account";
				var testForms = SystemFormBuilder.CreateList(2);
				
				SetupMockClientForExistingEntity(existingEntity, testForms);

				// Act
				var result = await _formService.GetFormsAsync(_mockClient, existingEntity);

				// Assert
				Assert.That(result, Has.Count.EqualTo(2));
				Assert.That(result[0].Name, Is.EqualTo(testForms[0].Name));
				Assert.That(result[1].Name, Is.EqualTo(testForms[1].Name));
			}

			[Test]
			public void WhenCancellationRequested_ThrowsOperationCanceledException()
			{
				// Arrange
				using var cts = new CancellationTokenSource();
				cts.Cancel();

				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns(Task.FromCanceled<EntityCollection>(cts.Token));

				// Act & Assert
				Assert.ThrowsAsync<McpException>(async () =>
					await _formService.GetFormsAsync(_mockClient, "account", cancellationToken: cts.Token));
			}

			private void SetupMockClientForExistingEntity(string entityName, List<SystemForm> forms)
			{
				// Prima chiamata per ValidateEntityExistsAsync - ritorna che l'entità esiste
				_mockClient.RetrieveMultipleAsync(Arg.Is<QueryExpression>(q => 
						q.EntityName == "entity" || q.EntityName == "metadata"))
					.Returns(new EntityCollection(new[] { new Entity() }));

				// Seconda chiamata per GetFormsAsync - ritorna le form
				var entityCollection = new EntityCollection();
				foreach (var form in forms)
				{
					var entity = new Entity("systemform", form.Id)
					{
						["formid"] = form.Id,
						["name"] = form.Name,
						["objecttypecode"] = form.ObjectTypeCode,
						["type"] = new OptionSetValue((int)form.Type.GetValueOrDefault()),
						["description"] = form.Description,
						["isdefault"] = form.IsDefault,
						["formxml"] = form.FormXml
					};
					entityCollection.Entities.Add(entity);
				}

				_mockClient.RetrieveMultipleAsync(Arg.Is<QueryExpression>(q => 
						q.EntityName == "systemform"))
					.Returns(entityCollection);
			}
		}

		[TestFixture]
		public class GetFormByIdAsync : FormServiceUnitTests
		{
			[Test]
			public async Task WhenFormExists_ReturnsForm()
			{
				// Arrange
				var testForm = new SystemFormBuilder()
					.WithId(TestDataProvider.TestGuids.FormId1)
					.WithName("Test Form")
					.Build();

				var entity = CreateEntityFromSystemForm(testForm);
				_mockClient.RetrieveAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ColumnSet>())
					.Returns(entity);

				// Act
				var result = await _formService.GetFormByIdAsync(_mockClient, testForm.Id);

				// Assert
				Assert.That(result, Is.Not.Null);
				Assert.That(result!.Id, Is.EqualTo(testForm.Id));
				Assert.That(result.Name, Is.EqualTo(testForm.Name));
			}

			[Test]
			public async Task WhenFormDoesNotExist_ReturnsNull()
			{
				// Arrange
				_mockClient.RetrieveAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ColumnSet>())
					.Returns(Task.FromResult<Entity>(null!));

				// Act
				var result = await _formService.GetFormByIdAsync(_mockClient, TestDataProvider.TestGuids.FormId1);

				// Assert
				Assert.That(result, Is.Null);
			}

			private static Entity CreateEntityFromSystemForm(SystemForm form)
			{
				return new Entity("systemform", form.Id)
				{
					["formid"] = form.Id,
					["name"] = form.Name,
					["objecttypecode"] = form.ObjectTypeCode,
					["type"] = new OptionSetValue((int)form.Type!),
					["description"] = form.Description,
					["isdefault"] = form.IsDefault,
					["formxml"] = form.FormXml
				};
			}
		}

		[TestFixture]
		public class ValidateEntityExistsAsync : FormServiceUnitTests
		{
			[Test]
			public async Task WhenEntityExists_ReturnsTrue()
			{
				// Arrange
				const string existingEntity = "account";
				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns(new EntityCollection(new[] { new Entity() }));

				// Act
				var result = await _formService.ValidateEntityExistsAsync(_mockClient, existingEntity);

				// Assert
				Assert.That(result, Is.True);
			}

			[Test]
			public async Task WhenEntityDoesNotExist_ReturnsFalse()
			{
				// Arrange
				const string nonExistentEntity = "nonexistent";
				_mockClient.RetrieveMultipleAsync(Arg.Any<QueryExpression>())
					.Returns(new EntityCollection()); // Collezione vuota = entità non esiste

				// Act
				var result = await _formService.ValidateEntityExistsAsync(_mockClient, nonExistentEntity);

				// Assert
				Assert.That(result, Is.False);
			}
		}
	}
}