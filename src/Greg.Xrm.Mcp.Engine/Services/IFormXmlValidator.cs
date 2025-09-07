using System.Xml.Schema;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	public interface IFormXmlValidator
	{
		XmlSchemaSet GetSchemaSet();
		FormXmlValidationResult TryValidateFormXmlAgainstSchema(string formXml);
	}
}