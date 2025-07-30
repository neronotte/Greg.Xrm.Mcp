using Greg.Xrm.Mcp.Core.Authentication;
using Greg.Xrm.Mcp.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers
{
	/// <summary>
	/// Classe base per test di integrazione che necessitano di mock più complessi
	/// </summary>
	public abstract class IntegrationTestBase : UnitTestBase
	{
		protected IDataverseClientProvider CreateMockDataverseClientProvider()
		{
			var provider = Substitute.For<IDataverseClientProvider>();
			var client = Substitute.For<IOrganizationServiceAsync2>();
			
			provider.GetDataverseClientAsync()
				.Returns(Task.FromResult(client));

			return provider;
		}

		protected IPublishXmlBuilder CreateMockPublishXmlBuilder()
		{
			var builder = Substitute.For<IPublishXmlBuilder>();
			
			// Configura il mock per essere fluent
			builder.AddTable(Arg.Any<string>());
			builder.Build().Returns(new Microsoft.Xrm.Sdk.OrganizationRequest());

			return builder;
		}

		/// <summary>
		/// Helper per configurare un mock client Dataverse con risposte predefinite
		/// </summary>
		protected IOrganizationServiceAsync2 CreateConfiguredDataverseClient()
		{
			var client = Substitute.For<IOrganizationServiceAsync2>();
			
			// Configura comportamenti comuni per tutti i test
			client.RetrieveMultipleAsync(Arg.Any<Microsoft.Xrm.Sdk.Query.QueryBase>())
				.Returns(new Microsoft.Xrm.Sdk.EntityCollection());

			return client;
		}
	}
}