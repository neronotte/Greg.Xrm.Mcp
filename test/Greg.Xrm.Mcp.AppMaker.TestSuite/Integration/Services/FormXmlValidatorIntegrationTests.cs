using Greg.Xrm.Mcp.FormEngineer.Services;
using Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers;
using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.Integration.Services
{
	/// <summary>
	/// Test di integrazione per FormXmlValidator
	/// Testa il comportamento end-to-end della validazione XML
	/// </summary>
	[TestFixture]
	public partial class FormXmlValidatorIntegrationTests : IntegrationTestBase
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
		public class RealWorldFormXmlValidation : FormXmlValidatorIntegrationTests
		{
			[Test]
			public void ComplexValidFormXml_ValidatesSuccessfully()
			{
				// Arrange - XML complesso ma valido
				var complexFormXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
    <form headerdensity=""HighWithControls"" shownavigationbar=""false"">
      <tabs>
        <tab verticallayout=""true"" id=""{ee70bf5e-c18b-4baf-a5b7-886968dc7c8d}"" IsUserDefined=""1"" name=""Information"">
          <labels>
            <label description=""General"" languagecode=""1033"" />
            <label description=""Generale"" languagecode=""1040"" />
          </labels>
          <columns>
            <column width=""100%"">
              <sections>
                <section showlabel=""false"" showbar=""false"" IsUserDefined=""0"" id=""{14dcc9a6-bc1e-48ef-80c1-e51c2bbbf9e2}"" columns=""11"">
                  <labels>
                    <label description=""General"" languagecode=""1033"" />
                    <label description=""Generale"" languagecode=""1040"" />
                  </labels>
                  <rows>
                    <row>
                      <cell id=""{51f5e9d2-4cae-47c3-b187-e113e66cd97d}"">
                        <labels>
                          <label description=""Id autorizzazione"" languagecode=""1033"" />
                          <label description=""Id autorizzazione"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_sapid"" classid=""{4273EDBD-AC1D-40d3-9FB2-095C621B552D}"" datafieldname=""nn_sapid"" disabled=""false"" />
                      </cell>
                      <cell id=""{12bfcaa8-4fa4-48fe-9eb9-25b5a187c77b}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Data inizio"" languagecode=""1033"" />
                          <label description=""Data inizio"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_datainizio"" classid=""{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}"" datafieldname=""nn_datainizio"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{be5a4a27-47f2-410d-8755-dffa5640d390}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""nn_tipoautorizzazionecode"" classid=""{3EF39988-22BB-4F0B-BBBE-64B5A3748AEE}"" datafieldname=""nn_tipoautorizzazionecode"" disabled=""false"" />
                      </cell>
                      <cell id=""{96b62f6b-a612-4165-bee5-04e27bc1bbb4}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Data fine"" languagecode=""1033"" />
                          <label description=""Data fine"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_datafine"" classid=""{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}"" datafieldname=""nn_datafine"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{2377a565-22b6-440e-839b-a98703a002c9}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""nn_descrizioneautorizzazione"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""nn_descrizioneautorizzazione"" disabled=""false"" />
                      </cell>
                      <cell id=""{2488b48b-50df-4a07-9355-742a4bf582df}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""nn_flag_iscrizione"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_flag_iscrizione"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{a2056fbc-6352-4ecc-89de-bc0d98bb54e2}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""nn_sedeoperativaid"" classid=""{270BD3DB-D9AF-4782-9025-509E298DEC0A}"" datafieldname=""nn_sedeoperativaid"" disabled=""false"" uniqueid=""{d608b376-64b1-a203-19ce-82ed261beba1}"">
                          <parameters>
                            <AutoResolve>true</AutoResolve>
                            <DisableMru>false</DisableMru>
                            <DisableQuickFind>false</DisableQuickFind>
                            <DisableViewPicker>false</DisableViewPicker>
                            <DefaultViewId>{EA134780-3EA9-4417-9A84-7C75B328B286}</DefaultViewId>
                            <AllowFilterOff>false</AllowFilterOff>
                          </parameters>
                        </control>
                      </cell>
                      <cell id=""{4a1939de-ee29-49e5-91e1-3ebb2ac662a5}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Categoria"" languagecode=""1033"" />
                          <label description=""Categoria"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_categoriaautorizzativa"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""nn_categoriaautorizzativa"" disabled=""false"" uniqueid=""{5749627a-a860-e11a-8285-1039abadb63b}"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{bece331f-f947-48c0-a426-5a21a3fe816a}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Numero autorizzazione"" languagecode=""1033"" />
                          <label description=""Numero autorizzazione"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_numeroautorizzazione"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""nn_numeroautorizzazione"" disabled=""false"" />
                      </cell>
                      <cell id=""{0574fcb0-50ce-4c71-a941-68ab3bb513f3}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Data rilascio"" languagecode=""1033"" />
                          <label description=""Data rilascio"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_datarilascio"" classid=""{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}"" datafieldname=""nn_datarilascio"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{8680e955-5478-4151-90af-eac82a743fc5}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Validità"" languagecode=""1033"" />
                          <label description=""Validità"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_validita"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""nn_validita"" disabled=""false"" />
                      </cell>
                      <cell id=""{c6147556-9667-463c-8665-fbe8c4e8f334}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Ente rilasciante"" languagecode=""1033"" />
                          <label description=""Ente rilasciante"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_enterilasciante"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""nn_enterilasciante"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{90c399fa-5079-4429-b0dc-a7e97f53ffe1}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""nn_tipologiaautorizzazioneid"" classid=""{270BD3DB-D9AF-4782-9025-509E298DEC0A}"" datafieldname=""nn_tipologiaautorizzazioneid"" disabled=""false"" />
                      </cell>
                      <cell id=""{3fb834d2-7122-4d55-8b04-6ff3130aaa4c}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Tipo ente rilascio"" languagecode=""1033"" />
                          <label description=""Tipo ente rilascio"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_tipoenterilascio"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""nn_tipoenterilascio"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{64f2a03b-b9b3-4894-adf2-58de7886e364}"" locklevel=""0"" colspan=""2"" rowspan=""1"">
                        <labels>
                          <label description=""Note interne"" languagecode=""1033"" />
                          <label description=""Note interne"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_noteinterne"" classid=""{E0DECE4B-6FC8-4A8F-A065-082708572369}"" datafieldname=""nn_noteinterne"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell locklevel=""0"" id=""{749b92fc-49ec-4a0f-a728-2d53ec8f0f5f}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell id=""{db5d8ee0-e7a4-454f-960b-33e8d6bad179}"" visible=""false"">
                        <labels>
                          <label description=""Owner"" languagecode=""1033"" />
                          <label description=""Proprietario"" languagecode=""1040"" />
                        </labels>
                        <control id=""ownerid"" classid=""{270BD3DB-D9AF-4782-9025-509E298DEC0A}"" datafieldname=""ownerid"" />
                      </cell>
                    </row>
                  </rows>
                </section>
              </sections>
            </column>
          </columns>
        </tab>
        <tab name=""tab_2"" id=""8407bb2c-558b-4db4-b1e1-5da9fed74ac6"" IsUserDefined=""0"" locklevel=""0"" showlabel=""true"">
          <labels>
            <label description=""Operazioni"" languagecode=""1033"" />
            <label description=""Operazioni"" languagecode=""1040"" />
          </labels>
          <columns>
            <column width=""100%"">
              <sections>
                <section name=""tab_2_section_4"" id=""d929b0b4-4bbd-451a-bba8-e027971de608"" IsUserDefined=""0"" locklevel=""0"" showlabel=""false"" showbar=""false"" layout=""varwidth"" celllabelalignment=""Left"" celllabelposition=""Left"" columns=""11"" labelwidth=""115"">
                  <labels>
                    <label description=""New Section"" languagecode=""1033"" />
                    <label description=""Nuova sezione"" languagecode=""1040"" />
                  </labels>
                  <rows>
                    <row>
                      <cell locklevel=""0"" id=""{e7cd806e-e349-4874-9237-305dfa587135}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell locklevel=""0"" id=""{c336734c-672d-403a-b1b0-f2e58b6c41a9}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell id=""{b2e76105-f1a6-4346-acaf-328ffd2f17df}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""Entrambi"" languagecode=""1033"" />
                          <label description=""Entrambi"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_hasentrambi"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_hasentrambi"" disabled=""false"" />
                      </cell>
                      <cell id=""{75ef281f-c7f5-4230-ad8b-caa97ced8d86}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""CR"" languagecode=""1033"" />
                          <label description=""CR"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_iscr"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_iscr"" disabled=""false"" />
                      </cell>
                    </row>
                  </rows>
                </section>
                <section name=""tab_2_section_1"" id=""ed39fa1c-4e58-43fb-8b4c-98ff1e5d65e7"" IsUserDefined=""0"" locklevel=""0"" showlabel=""true"" showbar=""false"" layout=""varwidth"" celllabelalignment=""Left"" celllabelposition=""Left"" columns=""111"" labelwidth=""115"">
                  <labels>
                    <label description=""D"" languagecode=""1033"" />
                    <label description=""D"" languagecode=""1040"" />
                  </labels>
                  <rows>
                    <row>
                      <cell locklevel=""0"" id=""{67a2a2f0-c375-4053-a1f9-2429484067c4}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell locklevel=""0"" id=""{136f4572-be44-4115-b0a6-9fb5ff145b04}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell locklevel=""0"" id=""{e75db4f8-3adb-47e0-be94-aa2e3c3d0f70}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell id=""{65da9d74-ea38-4d14-83ea-8e8a7c2af491}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D1"" languagecode=""1033"" />
                          <label description=""D1"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd1"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd1"" disabled=""false"" />
                      </cell>
                      <cell id=""{b35984a5-1662-4469-b842-138bfe5ef4ff}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell id=""{cb994212-ce38-4b45-bbf2-7d60714389dc}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell id=""{64d0b532-f6df-4dff-a4e3-3aa5d84ec3fb}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""D2"" languagecode=""1033"" />
                          <label description=""D2"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd2"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd2"" disabled=""false"" uniqueid=""{62ba9549-a75b-5e0d-76a0-6a57d34bc9d3}"" />
                      </cell>
                      <cell locklevel=""0"" id=""{66c51828-f648-4ce4-b281-0d10933b4edb}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell id=""{e82c4dbe-702c-4d2f-9e2b-07f6c1f39b0e}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell id=""{fd77ba0e-8c81-4dd9-8918-b6f332b10cc6}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""D3"" languagecode=""1033"" />
                          <label description=""D3"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd3"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd3"" disabled=""false"" uniqueid=""{e92425b1-d945-906b-6ead-b881606c948b}"" />
                      </cell>
                      <cell locklevel=""0"" id=""{1f508560-38d1-434b-afaa-6c387df2fcc9}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell id=""{66b74f9a-454f-477c-9f65-d7673883514f}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D11"" languagecode=""1033"" />
                          <label description=""D11"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd11"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd11"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{3e0df825-d98e-4211-b560-c18ff6cdb78f}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""D4"" languagecode=""1033"" />
                          <label description=""D4"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd4"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd4"" disabled=""false"" uniqueid=""{02a3a041-ec1d-3b17-4828-458a17c00682}"" />
                      </cell>
                      <cell id=""{b028974f-104b-441a-b588-2298b2ed78a3}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""D6"" languagecode=""1033"" />
                          <label description=""D6"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd6"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd6"" disabled=""false"" uniqueid=""{1abbd05e-903e-147a-0374-ddda013118e0}"" />
                      </cell>
                      <cell locklevel=""0"" id=""{87bd9bd7-925a-4ec7-933c-5aa8f2881ab0}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell id=""{9cd74bb2-0c95-4cdc-834b-b2db967f9833}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""D5"" languagecode=""1033"" />
                          <label description=""D5"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd5"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd5"" disabled=""false"" uniqueid=""{b751b7e6-1a6a-eb87-d5b3-45e96a9d36b3}"" />
                      </cell>
                      <cell id=""{d194b1dd-1c01-4d55-b9ba-860d37eb35c9}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D7"" languagecode=""1033"" />
                          <label description=""D7"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd7"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd7"" disabled=""false"" />
                      </cell>
                      <cell locklevel=""0"" id=""{5a7781aa-55db-4712-bc26-38502c40e5b9}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell locklevel=""0"" id=""{7c79f0c3-f26f-4d2a-9603-8a8f7d493431}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell id=""{8930a387-d8cb-4ad7-b5a2-2dd01d9934c3}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D8"" languagecode=""1033"" />
                          <label description=""D8"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd8"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd8"" disabled=""false"" uniqueid=""{024243d7-a099-461c-47e6-d79210e069a3}"" />
                      </cell>
                      <cell id=""{f7c65bb7-ac46-4dab-95be-f6906364ab82}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D12"" languagecode=""1033"" />
                          <label description=""D12"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd12"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd12"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell locklevel=""0"" id=""{d8cf392d-4288-4336-af9d-52c9277fa84f}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell id=""{3f0a53ff-4919-482f-9c3e-7452229cccf4}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""D9"" languagecode=""1033"" />
                          <label description=""D9"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd9"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd9"" disabled=""false"" uniqueid=""{5ae4a8cc-8726-41a6-99b1-2eab137e5295}"" />
                      </cell>
                      <cell id=""{e3125e30-9afe-4fbc-8748-284eeed50835}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D13"" languagecode=""1033"" />
                          <label description=""D13"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd13"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd13"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell locklevel=""0"" id=""{08bc418a-d137-4b9e-b3c6-fe8cb37ee5a8}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell id=""{12999044-9139-44fe-80cf-41d35a1d3b53}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D10"" languagecode=""1033"" />
                          <label description=""D10"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd10"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd10"" disabled=""false"" />
                      </cell>
                      <cell id=""{3e70523e-9597-4747-84cb-ecc10b0c5ae9}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D14"" languagecode=""1033"" />
                          <label description=""D14"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd14"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd14"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell locklevel=""0"" id=""{6a5567be-8ea9-4c73-8b0f-e1effc65c5c9}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell locklevel=""0"" id=""{9ccb62a5-d524-4a68-953c-4468c679e27d}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell id=""{b6888b34-dde7-4eab-9946-53b6885c4c97}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""D15"" languagecode=""1033"" />
                          <label description=""D15"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isd15"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isd15"" disabled=""false"" />
                      </cell>
                    </row>
                  </rows>
                </section>
                <section name=""tab_2_section_3"" id=""f7c00244-12d2-490c-b7c2-0cdda45f49b0"" IsUserDefined=""0"" locklevel=""0"" showlabel=""true"" showbar=""false"" layout=""varwidth"" celllabelalignment=""Left"" celllabelposition=""Left"" columns=""111"" labelwidth=""115"">
                  <labels>
                    <label description=""R"" languagecode=""1033"" />
                    <label description=""R"" languagecode=""1040"" />
                  </labels>
                  <rows>
                    <row>
                      <cell locklevel=""0"" id=""{ba63a476-3b0c-4872-95f0-a45b337701af}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell locklevel=""0"" id=""{b5c220df-0370-47a0-8533-61a3936b447e}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell locklevel=""0"" id=""{764c0e51-4070-49a7-aeb7-9345e76912b1}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell id=""{6c6419ed-43ef-4c05-82d9-cd8a690a98f6}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""R1"" languagecode=""1033"" />
                          <label description=""R1"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr1"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr1"" disabled=""false"" uniqueid=""{ab352269-4172-e93a-5e89-d3ea650a8ce9}"" />
                      </cell>
                      <cell id=""{6e4d9367-c436-4d81-9f77-ad2b47cc6da6}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""R6"" languagecode=""1033"" />
                          <label description=""R6"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr6"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr6"" disabled=""false"" uniqueid=""{95aead78-44a6-d43d-d723-6ecb6cb3ec83}"" />
                      </cell>
                      <cell id=""{eb4f9b1a-bf83-407b-b9c9-259b25538ea7}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""R10"" languagecode=""1033"" />
                          <label description=""R10"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr10"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr10"" disabled=""false"" uniqueid=""{6a650930-deba-0815-62d8-35a5edc6ebce}"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{0eb0a9a0-b46c-4405-8644-fa65d7bbce3d}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""R2"" languagecode=""1033"" />
                          <label description=""R2"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr2"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr2"" disabled=""false"" uniqueid=""{9eb71a5d-0aad-7393-2608-48b8a671287b}"" />
                      </cell>
                      <cell id=""{2ce62f7d-6c6c-41b3-8eb3-708bd621add8}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""R7"" languagecode=""1033"" />
                          <label description=""R7"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr7"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr7"" disabled=""false"" />
                      </cell>
                      <cell id=""{506838e1-c9c2-495e-81b5-197b90036fe0}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""R11"" languagecode=""1033"" />
                          <label description=""R11"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr11"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr11"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{653481af-5fde-4bc4-9713-411840b309d2}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""R3"" languagecode=""1033"" />
                          <label description=""R3"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr3"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr3"" disabled=""false"" uniqueid=""{c8d65b89-6660-95dc-7c6c-8ac82ddeaa3e}"" />
                      </cell>
                      <cell id=""{a75418b8-34a2-4198-911b-5378c8d6f7c4}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""R8"" languagecode=""1033"" />
                          <label description=""R8"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr8"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr8"" disabled=""false"" uniqueid=""{670d78aa-8b99-a029-57c7-a18368ce833c}"" />
                      </cell>
                      <cell id=""{60801c49-933d-4205-9bd2-fb370a017713}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""R12"" languagecode=""1033"" />
                          <label description=""R12"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr12"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr12"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{82dd37ec-c447-457a-bd55-1ab935ec2c03}"" locklevel=""0"" colspan=""1"" rowspan=""1"" showlabel=""true"">
                        <labels>
                          <label description=""R4"" languagecode=""1033"" />
                          <label description=""R4"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr4"" classid=""{67FAC785-CD58-4f9f-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr4"" disabled=""false"" uniqueid=""{49c92065-9428-b68e-3ece-b8b9a806435c}"" />
                      </cell>
                      <cell id=""{4bd89b2f-b1e6-4548-bef9-d20d4858ec9e}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""R9"" languagecode=""1033"" />
                          <label description=""R9"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr9"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr9"" disabled=""false"" />
                      </cell>
                      <cell id=""{33aba020-24fc-4a06-9f26-e3dbe08d8534}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""R13"" languagecode=""1033"" />
                          <label description=""R13"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr13"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr13"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{a23a7cdf-58cc-4ea1-9503-e9cb97afb3ac}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description=""R5"" languagecode=""1033"" />
                          <label description=""R5"" languagecode=""1040"" />
                        </labels>
                        <control id=""nn_isr5"" classid=""{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"" datafieldname=""nn_isr5"" disabled=""false"" />
                      </cell>
                      <cell locklevel=""0"" id=""{996e7bb5-7e3b-46b9-aa29-1d0a7afc6313}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell locklevel=""0"" id=""{13d7199c-a83c-4cb4-8470-879803101fe4}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                  </rows>
                </section>
              </sections>
            </column>
          </columns>
        </tab>
        <tab name=""tab_3"" id=""f611f659-9202-4dea-990c-b90f1519f500"" IsUserDefined=""0"" locklevel=""0"" showlabel=""true"">
          <labels>
            <label description=""Posizioni Autorizzazione"" languagecode=""1033"" />
            <label description=""Posizioni Autorizzazione"" languagecode=""1040"" />
          </labels>
          <columns>
            <column width=""100%"">
              <sections>
                <section name=""tab_3_section_1"" id=""b8b03851-4eb2-4158-9ca5-42a8cbc91868"" IsUserDefined=""0"" locklevel=""0"" showlabel=""false"" showbar=""false"" layout=""varwidth"" celllabelalignment=""Left"" celllabelposition=""Left"" columns=""1"" labelwidth=""115"">
                  <labels>
                    <label description=""Posizioni"" languagecode=""1033"" />
                    <label description=""Posizioni"" languagecode=""1040"" />
                  </labels>
                  <rows>
                    <row>
                      <cell id=""{c281f7a0-8a2b-468a-a6fb-1a98dc331f20}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell locklevel=""0"" id=""{07a1375c-70b9-4011-a4c4-3834033c961b}"" rowspan=""4"" colspan=""1"" auto=""false"" showlabel=""false"">
                        <labels>
                          <label description=""New SG control 1742918177364"" languagecode=""1033"" />
                          <label description=""Autorizzazione Posizioni"" languagecode=""1040"" />
                        </labels>
                        <control indicationOfSubgrid=""true"" id=""Subgrid_new_3"" classid=""{E7A81278-8635-4D9E-8D4D-59480B391C5B}"">
                          <parameters>
                            <RecordsPerPage>4</RecordsPerPage>
                            <AutoExpand>Fixed</AutoExpand>
                            <EnableQuickFind>false</EnableQuickFind>
                            <EnableViewPicker>false</EnableViewPicker>
                            <EnableChartPicker>true</EnableChartPicker>
                            <ChartGridMode>All</ChartGridMode>
                            <RelationshipName>nn_autorizzazioneposizione_autorizzazione</RelationshipName>
                            <TargetEntityType>nn_autorizzazioneposizione</TargetEntityType>
                            <ViewId>{9FCD63CC-E919-4E19-A0E6-8ED43532E9CE}</ViewId>
                            <ViewIds>{9FCD63CC-E919-4E19-A0E6-8ED43532E9CE}</ViewIds>
                          </parameters>
                        </control>
                      </cell>
                    </row>
                    <row />
                    <row />
                    <row />
                  </rows>
                </section>
              </sections>
            </column>
          </columns>
        </tab>
        <tab name=""tab_4"" id=""2ba89397-dd16-490c-86fc-9a66c712cb0a"" IsUserDefined=""0"" locklevel=""0"" showlabel=""true"">
          <labels>
            <label description=""Autorizzazioni Trasporto"" languagecode=""1033"" />
            <label description=""Autorizzazioni Trasporto"" languagecode=""1040"" />
          </labels>
          <columns>
            <column width=""100%"">
              <sections>
                <section name=""tab_4_section_3"" id=""68d5ad5b-5f4a-49da-8e36-f04917cb0b6e"" IsUserDefined=""0"" locklevel=""0"" showlabel=""false"" showbar=""false"" layout=""varwidth"" celllabelalignment=""Left"" celllabelposition=""Left"" columns=""1"" labelwidth=""115"">
                  <labels>
                    <label description=""New Section"" languagecode=""1033"" />
                    <label description=""Nuova sezione"" languagecode=""1040"" />
                  </labels>
                  <rows>
                    <row>
                      <cell id=""{97963d45-2182-489a-a05a-eba74f40d209}"" showlabel=""false"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell locklevel=""0"" id=""{666ac16c-4f87-4332-8e11-323f635701b8}"" rowspan=""4"" colspan=""1"" auto=""false"" showlabel=""false"">
                        <labels>
                          <label description=""New SG control 1742919221402"" languagecode=""1033"" />
                          <label description=""Autorizzazioni Mezzi"" languagecode=""1040"" />
                        </labels>
                        <control indicationOfSubgrid=""true"" id=""Subgrid_new_4"" classid=""{E7A81278-8635-4D9E-8D4D-59480B391C5B}"">
                          <parameters>
                            <RecordsPerPage>4</RecordsPerPage>
                            <AutoExpand>Fixed</AutoExpand>
                            <EnableQuickFind>false</EnableQuickFind>
                            <EnableViewPicker>false</EnableViewPicker>
                            <EnableChartPicker>true</EnableChartPicker>
                            <ChartGridMode>All</ChartGridMode>
                            <RelationshipName>nn_autorizzazionemezzo_autorizzazione</RelationshipName>
                            <TargetEntityType>nn_autorizzazionemezzo</TargetEntityType>
                            <ViewId>{ce182fe4-9309-f011-bae3-7c1e527387f7}</ViewId>
                            <ViewIds>{ce182fe4-9309-f011-bae3-7c1e527387f7}</ViewIds>
                          </parameters>
                        </control>
                      </cell>
                    </row>
                    <row />
                    <row />
                    <row />
                  </rows>
                </section>
              </sections>
            </column>
          </columns>
        </tab>
        <tab name=""tab_administration"" id=""369e6a4a-0f91-4bf1-828b-20530b24b483"" IsUserDefined=""0"" locklevel=""0"" showlabel=""true"" visible=""false"" availableforphone=""false"">
          <labels>
            <label description=""⚙️ Administration"" languagecode=""1033"" />
            <label description=""⚙️ Amministrazione"" languagecode=""1040"" />
          </labels>
          <columns>
            <column width=""100%"">
              <sections>
                <section name=""tab_5_section_1"" id=""4bd1983a-a601-4f4d-bec7-b3b16fe7326d"" IsUserDefined=""0"" locklevel=""0"" showlabel=""false"" showbar=""false"" layout=""varwidth"" celllabelalignment=""Left"" celllabelposition=""Left"" columns=""11"" labelwidth=""115"">
                  <labels>
                    <label description=""New Section"" languagecode=""1033"" />
                    <label description=""Nuova sezione"" languagecode=""1040"" />
                  </labels>
                  <rows>
                    <row>
                      <cell locklevel=""0"" id=""{dceddba7-59f9-4717-80fc-21251f3ee3c2}"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                      <cell locklevel=""0"" id=""{c95ff5b9-20b3-4727-b759-3627e7d1e3a2}"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                      </cell>
                    </row>
                    <row>
                      <cell id=""{5b00ee5d-8c50-4696-b4b6-f4610c3227fe}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""createdby"" classid=""{270BD3DB-D9AF-4782-9025-509E298DEC0A}"" datafieldname=""createdby"" disabled=""false"" />
                      </cell>
                      <cell id=""{22c6c914-a244-45f9-a590-1882c5204a7f}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""createdon"" classid=""{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}"" datafieldname=""createdon"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{1ff15d8c-9406-445e-805f-61d3f066d36f}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""modifiedby"" classid=""{270BD3DB-D9AF-4782-9025-509E298DEC0A}"" datafieldname=""modifiedby"" disabled=""false"" />
                      </cell>
                      <cell id=""{12bab782-fb9c-4741-ab96-0321a887bb63}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""modifiedon"" classid=""{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}"" datafieldname=""modifiedon"" disabled=""false"" />
                      </cell>
                    </row>
                    <row>
                      <cell id=""{5a40811c-5f5f-4324-96ce-434a5cf5cfb6}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""ownerid"" classid=""{270BD3DB-D9AF-4782-9025-509E298DEC0A}"" datafieldname=""ownerid"" disabled=""false"" />
                      </cell>
                      <cell id=""{a45418f4-a936-4d2f-a960-77e886ac0a98}"" locklevel=""0"" colspan=""1"" rowspan=""1"">
                        <labels>
                          <label description="""" languagecode=""1033"" />
                        </labels>
                        <control id=""nn_sapid"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""nn_sapid"" disabled=""false"" />
                      </cell>
                    </row>
                  </rows>
                </section>
              </sections>
            </column>
          </columns>
        </tab>
      </tabs>
      <header id=""{ffdc9983-effc-4181-bf3c-ca0089025bff}"" celllabelposition=""Top"" columns=""111"" labelwidth=""115"" celllabelalignment=""Left"">
        <rows>
          <row>
            <cell id=""{9f722642-a9d7-4391-873b-64f007bcd9a8}"" showlabel=""false"">
              <labels>
                <label description="""" languagecode=""1033"" />
              </labels>
            </cell>
            <cell id=""{cca2ae7b-a3ac-45db-b54b-1b70fd4800bd}"" showlabel=""false"">
              <labels>
                <label description="""" languagecode=""1033"" />
              </labels>
            </cell>
            <cell id=""{0d5f5ea7-d39e-45c2-a337-9843c43e58b2}"" showlabel=""false"">
              <labels>
                <label description="""" languagecode=""1033"" />
              </labels>
            </cell>
          </row>
        </rows>
      </header>
      <footer id=""{c5269383-171c-46c8-ba9f-3f8693ee6b55}"" celllabelposition=""Top"" columns=""111"" labelwidth=""115"" celllabelalignment=""Left"">
        <rows>
          <row>
            <cell id=""{3a5434a5-65d2-4a4f-9b3e-f1559357453a}"" showlabel=""false"">
              <labels>
                <label description="""" languagecode=""1033"" />
              </labels>
            </cell>
            <cell id=""{23e0aede-7153-4a87-a075-28be6defb13e}"" showlabel=""false"">
              <labels>
                <label description="""" languagecode=""1033"" />
              </labels>
            </cell>
            <cell id=""{a1ddbf5f-6379-4827-9e3e-4ed29af4088d}"" showlabel=""false"">
              <labels>
                <label description="""" languagecode=""1033"" />
              </labels>
            </cell>
          </row>
        </rows>
      </footer>
      <Navigation>
        <NavBar>
          <NavBarByRelationshipItem Area=""ProcessCenter"" Show=""false"" RelationshipName=""hardcoded_{B7196B13-6B9D-42CD-BD58-B19A3953126F}navAsyncOperations"" Id=""navAsyncOperations"" Sequence=""9500"">
            <Titles>
              <Title Text=""System Jobs"" LCID=""1033"" />
              <Title LCID=""1040"" Text=""Processi in background"" />
            </Titles>
          </NavBarByRelationshipItem>
        </NavBar>
        <NavBarAreas>
          <NavBarArea Id=""Info"">
            <Titles>
              <Title LCID=""1033"" Text=""Common"" />
              <Title LCID=""1040"" Text=""Elementi comuni"" />
            </Titles>
          </NavBarArea>
          <NavBarArea Id=""Sales"">
            <Titles>
              <Title LCID=""1033"" Text=""Sales"" />
              <Title LCID=""1040"" Text=""Vendite"" />
            </Titles>
          </NavBarArea>
          <NavBarArea Id=""Service"">
            <Titles>
              <Title LCID=""1033"" Text=""Service"" />
              <Title LCID=""1040"" Text=""Servizio"" />
            </Titles>
          </NavBarArea>
          <NavBarArea Id=""Marketing"">
            <Titles>
              <Title LCID=""1033"" Text=""Marketing"" />
              <Title LCID=""1040"" Text=""Marketing"" />
            </Titles>
          </NavBarArea>
          <NavBarArea Id=""ProcessCenter"">
            <Titles>
              <Title LCID=""1033"" Text=""Process Sessions"" />
              <Title LCID=""1040"" Text=""Sessione di processi"" />
            </Titles>
          </NavBarArea>
        </NavBarAreas>
      </Navigation>
      <controlDescriptions />
      <DisplayConditions Order=""0"" FallbackForm=""true"">
        <Everyone />
      </DisplayConditions>
    </form>";

				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(complexFormXml);

				// Assert
				Assert.That(result.IsValid, Is.True);
				Assert.That(result.Count(r => r.Level == FormXmlValidationLevel.Error), Is.EqualTo(0));
			}

			[Test]
			public void FormXmlWithSubgrid_ValidatesCorrectly()
			{
				// Arrange - Form con subgrid (scenario comune)
				var formWithSubgrid = @"<?xml version=""1.0"" encoding=""utf-8""?>
<form>
	<tabs>
		<tab name=""general"">
			<labels>
				<label description=""General"" languagecode=""1033"" />
			</labels>
			<columns>
				<column width=""100%"">
					<sections>
						<section name=""subgrid_section"" showlabel=""true"" showbar=""true"">
							<labels>
								<label description=""Related Records"" languagecode=""1033"" />
							</labels>
							<rows>
								<row>
									<cell id=""{3a5434a5-65d2-4a4f-9b3e-f1559357453a}"" showlabel=""false"">
										<control id=""{3a5434a5-65d2-4a4f-9b3e-f1559357453a}"" classid=""{E7A81278-8635-4d9e-8D4D-59480B391C5B}"">
											<parameters>
												<ViewId>{00000000-0000-0000-00AA-000010001004}</ViewId>
												<TargetEntityType>contact</TargetEntityType>
												<RelationshipName>account_primary_contact</RelationshipName>
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

				// Act
				var result = _validator.TryValidateFormXmlAgainstSchema(formWithSubgrid);

				// Assert
				Assert.That(result.IsValid, Is.True, result.ToString());
			}
		}
	}
}