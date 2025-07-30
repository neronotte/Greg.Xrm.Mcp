using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;


namespace Greg.Xrm.Mcp.FormEngineer.Server.Resources
{
	[McpServerResourceType]
	public class Schemas
	{
		protected Schemas() { }

		[McpServerResource(MimeType = "application/xml", Name = "formxml_schema", Title = "Dataverse FormXML Schema Set", UriTemplate = "schema://formxml"),
		Description("Returns a set of Xml schemas defining the structure of Dataverse forms. This schema defines the structure and elements used in the FormXML format. FormType is the root object of the schema."),
		]
		public static string FormXmlSchema(
			ITelemetryClient telemetryClient,
			IFormXmlValidator validator)
		{
			using var operation = telemetryClient.StartOperation<RequestTelemetry>("Schemas.FormXmlSchema");

			var schemaSet = validator.GetSchemaSet();

			int schemaIndex = 1;
			var xmlOutput = new StringBuilder();


			foreach (XmlSchema schema in schemaSet.Schemas())
			{
				xmlOutput.AppendLine($"Schema {schemaIndex}:");
				xmlOutput.AppendLine($"Target Namespace: {schema.TargetNamespace ?? "(null)"}");
				xmlOutput.AppendLine();

				// Create XML writer settings for pretty printing
				XmlWriterSettings settings = new XmlWriterSettings
				{
					Indent = true,
					IndentChars = "  ",
					NewLineChars = Environment.NewLine,
					NewLineHandling = NewLineHandling.Replace,
					Encoding = Encoding.UTF8
				};

				// Use StringBuilder to capture the XML output
				using (XmlWriter writer = XmlWriter.Create(xmlOutput, settings))
				{
					schema.Write(writer);
				}

				xmlOutput.AppendLine().AppendLine(new string('-', 40)).AppendLine();
				schemaIndex++;
			}



			return xmlOutput.ToString();
		}



		[McpServerResource(MimeType = "application/xml", Name = "layoutxml_schema", Title = "Dataverse LayoutXML Schema", UriTemplate = "schema://layoutxml"),
		Description("Returns the XML schema describing the structure of Dataverse views in terms of columns."),
		]
		public static async Task<string> LayoutXmlSchema(
			ITelemetryClient telemetryClient,
			IFormXmlValidator validator)
		{
			using var operation = telemetryClient.StartOperation<RequestTelemetry>("Schemas.LayoutXmlSchema");

			var assembly = typeof(Schemas).Assembly;

			var stream = assembly.GetManifestResourceStream($"Greg.Xrm.Mcp.FormEngineer.Resources.LayoutXml.xsd");
			if (stream == null)
			{
				return "No LayoutXml found!";
			}

			using var reader = new StreamReader(stream);

			var schema = await reader.ReadToEndAsync();
			return schema;
		}



		[McpServerResource(MimeType = "application/xml", Name = "filterxml_schema", Title = "Dataverse FilterXml Schema", UriTemplate = "schema://filterxml"),
		Description("Returns the XML schema describing the structure of Dataverse views in terms search criteria (filters applied on the data)."),
		]
		public static async Task<string> FilterXmlSchema(
			ITelemetryClient telemetryClient,
			IFormXmlValidator validator)
		{
			using var operation = telemetryClient.StartOperation<RequestTelemetry>("Schemas.LayoutXmlSchema");

			var assembly = typeof(Schemas).Assembly;

			var stream = assembly.GetManifestResourceStream($"Greg.Xrm.Mcp.FormEngineer.Resources.FilterXml.xsd");
			if (stream == null)
			{
				return "No LayoutXml found!";
			}

			using var reader = new StreamReader(stream);

			var schema = await reader.ReadToEndAsync();
			return schema;
		}
	}
}
