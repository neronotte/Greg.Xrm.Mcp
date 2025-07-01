using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace Greg.Xrm.Mcp.FormEngineer.Server.Resources
{
	[McpServerResourceType]
	public class Schemas
	{
		protected Schemas() { }

		[McpServerResource(MimeType = "application/xml", Name = "FormXml Schema", Title = "Dataverse FormXML Schema", UriTemplate = "schema://formxml"),
		Description("Returns the FormXML schema used in Dataverse forms. This schema defines the structure and elements used in the FormXML format."),
		]
		public static string FormXmlSchema() => 
			Encoding.UTF8.GetString(Properties.Resources.formxml);
	}
}
