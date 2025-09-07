using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.ApplicationInsights.DataContracts;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;


namespace Greg.Xrm.Mcp.FormEngineer.Server.Resources
{
	[McpServerResourceType]
	public class Schemas(ITelemetryClient telemetryClient, IFormXmlValidator validator)
	{

		[McpServerResource(
			MimeType = "application/xml", 
			Name = "formxml_schema", 
			Title = "Dataverse FormXML Schema Set", 
			UriTemplate = "schema://formxml"),
		Description(
@"Returns a set of Xml schemas defining the structure of Dataverse forms. 
This schema defines the structure and elements used in the FormXML format. 
FormType is the root object of the schema."),
		]
		public string FormXmlSchema()
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



		[McpServerResource(
			MimeType = "application/xml", 
			Name = "layoutxml_schema", 
			Title = "Dataverse LayoutXML Schema", 
			UriTemplate = "schema://layoutxml"),
		Description(
			"Returns the XML schema describing the structure of Dataverse views in terms of columns."),
		]
		public async Task<string> LayoutXmlSchema()
		{
			using var operation = telemetryClient.StartOperation<RequestTelemetry>("Schemas.LayoutXmlSchema");

			var assembly = typeof(Schemas).Assembly;

			var stream = assembly.GetManifestResourceStream($"Greg.Xrm.Mcp.Resources.LayoutXml.xsd");
			if (stream == null)
			{
				return "No LayoutXml found!";
			}

			using var reader = new StreamReader(stream);

			var schema = await reader.ReadToEndAsync();
			return schema;
		}





		[McpServerResource(
			MimeType = "application/xml", 
			Name = "fetchxml_schema", 
			Title = "Dataverse FetchXml Schema", 
			UriTemplate = "schema://fetchxml"),
		Description(
			"Returns the XML schema of the query that runs Dataverse."),
		]
		public async Task<string> FetchXmlSchema()
		{
			using var operation = telemetryClient.StartOperation<RequestTelemetry>("Schemas.FetchXmlSchema");

			var assembly = typeof(Schemas).Assembly;

			var stream = assembly.GetManifestResourceStream($"Greg.Xrm.Mcp.Resources.Fetch.xsd");
			if (stream == null)
			{
				return "No Fetch found!";
			}

			using var reader = new StreamReader(stream);
			var schema = await reader.ReadToEndAsync();
			return schema;
		}
	}
}
