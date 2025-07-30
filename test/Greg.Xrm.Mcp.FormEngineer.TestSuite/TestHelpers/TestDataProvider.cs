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
		/// Lista di entità di test comuni
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
		}

		/// <summary>
		/// Messaggi di errore comuni per test di validazione
		/// </summary>
		public static class ErrorMessages
		{
			public const string InvalidGuid = "Invalid formId";
			public const string FormNotFound = "No form found with ID";
			public const string EmptyXml = "empty or null";
			public const string ValidationFailed = "Form XML validation failed";
		}
	}
}