using Greg.Xrm.Mcp.FormEngineer.Model;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers.Builders
{
	/// <summary>
	/// Builder per creare oggetti SystemForm per i test
	/// Utilizza il pattern Builder per rendere i test più leggibili
	/// </summary>
	public class SystemFormBuilder
	{
		private readonly SystemForm _systemForm;
		private static int _idCounter = 1;

		public SystemFormBuilder()
		{
			_systemForm = new SystemForm
			{
				Id = CreateUniqueGuid(),
				Name = $"Test Form {_idCounter}",
				ObjectTypeCode = "account",
				Type = systemform_type.Main,
				FormXml = CreateValidFormXml(),
				IsDefault = false,
				Description = $"Test form description {_idCounter}"
			};
			_idCounter++;
		}

		public SystemFormBuilder WithId(Guid id)
		{
			_systemForm.Id = id;
			return this;
		}

		public SystemFormBuilder WithName(string name)
		{
			_systemForm.Name = name;
			return this;
		}

		public SystemFormBuilder WithObjectTypeCode(string objectTypeCode)
		{
			_systemForm.ObjectTypeCode = objectTypeCode;
			return this;
		}

		public SystemFormBuilder WithFormType(systemform_type formType)
		{
			_systemForm.Type = formType;
			return this;
		}

		public SystemFormBuilder WithFormXml(string formXml)
		{
			_systemForm.FormXml = formXml;
			return this;
		}

		public SystemFormBuilder AsDefault()
		{
			_systemForm.IsDefault = true;
			return this;
		}

		public SystemFormBuilder WithDescription(string description)
		{
			_systemForm.Description = description;
			return this;
		}

		public SystemForm Build()
		{
			return _systemForm;
		}

		/// <summary>
		/// Crea un GUID unico ma deterministico per i test
		/// </summary>
		private static Guid CreateUniqueGuid()
		{
			var bytes = new byte[16];
			BitConverter.GetBytes(_idCounter).CopyTo(bytes, 0);
			BitConverter.GetBytes(DateTime.UtcNow.Ticks).CopyTo(bytes, 8);
			return new Guid(bytes);
		}

		/// <summary>
		/// Crea XML di form valido per i test
		/// </summary>
		private static string CreateValidFormXml()
		{
			return @"<form>
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
												<cell id=""account_name"">
													<labels>
														<label description=""Account Name"" languagecode=""1033"" />
													</labels>
													<control id=""name"" classid=""{4273EDBD-AC1D-40d3-9FB2-095C621B552D}"">
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
		}

		/// <summary>
		/// Crea XML di form non valido per test di validazione
		/// </summary>
		public SystemFormBuilder WithInvalidFormXml()
		{
			_systemForm.FormXml = "<form><tab><unclosed>";
			return this;
		}

		/// <summary>
		/// Crea una lista di SystemForm per test multipli
		/// </summary>
		/// <param name="count">Numero di form da creare</param>
		/// <returns>Lista di SystemForm con dati di test</returns>
		public static List<SystemForm> CreateList(int count = 3)
		{
			var forms = new List<SystemForm>();
			for (int i = 0; i < count; i++)
			{
				forms.Add(new SystemFormBuilder()
					.WithName($"Test Form {i + 1}")
					.WithDescription($"Test form description {i + 1}")
					.Build());
			}
			return forms;
		}

		/// <summary>
		/// Crea una lista di SystemForm con ID predefiniti per test deterministici
		/// </summary>
		/// <param name="count">Numero di form da creare</param>
		/// <returns>Lista di SystemForm con ID deterministici</returns>
		public static List<SystemForm> CreateListWithPredefinedIds(int count = 3)
		{
			var forms = new List<SystemForm>();
			var predefinedIds = new[]
			{
				TestDataProvider.TestGuids.FormId1,
				TestDataProvider.TestGuids.FormId2,
				TestDataProvider.TestGuids.FormId3
			};

			for (int i = 0; i < count && i < predefinedIds.Length; i++)
			{
				forms.Add(new SystemFormBuilder()
					.WithId(predefinedIds[i])
					.WithName($"Test Form {i + 1}")
					.WithDescription($"Test form description {i + 1}")
					.Build());
			}
			return forms;
		}

		/// <summary>
		/// Crea SystemForm per entità specifiche con dati appropriati
		/// </summary>
		/// <param name="entityLogicalName">Nome logico dell'entità</param>
		/// <param name="formType">Tipo di form</param>
		/// <returns>SystemForm configurato per l'entità</returns>
		public static SystemForm CreateForEntity(string entityLogicalName, systemform_type formType = systemform_type.Main)
		{
			return new SystemFormBuilder()
				.WithObjectTypeCode(entityLogicalName)
				.WithFormType(formType)
				.WithName($"{entityLogicalName} {formType} Form")
				.WithDescription($"Test {formType} form for {entityLogicalName}")
				.Build();
		}
	}
}