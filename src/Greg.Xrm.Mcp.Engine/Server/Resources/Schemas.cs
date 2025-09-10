using Greg.Xrm.Mcp.Core.Services;
using Greg.Xrm.Mcp.FormEngineer.Services;
using Microsoft.ApplicationInsights.DataContracts;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;


namespace Greg.Xrm.Mcp.Server.Resources
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
			return await ReadResourceAsync($"Greg.Xrm.Mcp.Resources.LayoutXml.xsd") ?? "Not found";
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
			return await ReadResourceAsync($"Greg.Xrm.Mcp.Resources.Fetch.xsd") ?? "Not found";
		}




		[McpServerResource(
			MimeType = "text/markdown",
			Name = "sitemapxml_schema",
			Title = "Dataverse SitemapXml Schemas and Instructions",
			UriTemplate = "schema://sitemapxml"),
		Description(@"Returns a set of Xml schemas defining the structure of Dataverse sitemap (navigation bar), and instructions on how to properly generate a SiteMap XML document."),
		]
		public async Task<string> SiteMapXmlSchema()
		{
			using var operation = telemetryClient.StartOperation<RequestTelemetry>("Schemas.SiteMapXmlSchema");

			
			var xmlOutput = new StringBuilder();

			xmlOutput.AppendLine("# Dataverse SiteMap definition");

			xmlOutput.AppendLine();
			xmlOutput.AppendLine("## General rules");

			xmlOutput.AppendLine("Sitemap XML is defined by two interconnected schemas:");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("1. **SiteMap.xsd**: Defines the overall structure of the sitemap, including Areas, Groups, and SubAreas.");
			xmlOutput.AppendLine("2. **SitemapType.xsd**: Provides detailed definitions for the types used within the sitemap, such as Area, Group, and SubArea elements.");
			xmlOutput.AppendLine();

			xmlOutput.AppendLine("These schemas work together to ensure that the sitemap XML adheres to the required format and structure for Dataverse applications.");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("Each Area, Group, SubArea **MUST** have an **unique** Id, with the following rules:");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("- Area Ids must start with 'area_'");
			xmlOutput.AppendLine("- Group Ids must start with 'group_'");
			xmlOutput.AppendLine("- SubArea Ids must start with 'sa_'");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("Sitemap `Titles` node must contain a subnode for each language installed on the system.");

			xmlOutput.AppendLine();
			xmlOutput.AppendLine("## Schemas");

			xmlOutput.AppendLine("### Schema 1 - SiteMap.xsd:");
			var schema1 = await ReadResourceAsync($"Greg.Xrm.Mcp.Resources.SiteMap.xsd");
			xmlOutput.AppendLine(schema1).AppendLine();

			xmlOutput.AppendLine("### Schema 2 - SitemapType.xsd:");
			var schema2 = await ReadResourceAsync($"Greg.Xrm.Mcp.Resources.SiteMapType.xsd");
			xmlOutput.AppendLine(schema2).AppendLine();


			xmlOutput.AppendLine("## SubArea types");
			xmlOutput.AppendLine("There are 5 types of subareas:");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("1. **Entity**: Links to a specific Dataverse table (entity).");
			xmlOutput.AppendLine("2. **WebResource**: Links to a web resource, such as an HTML page or JavaScript file.");
			xmlOutput.AppendLine("3. **Url**: Links to an external URL.");
			xmlOutput.AppendLine("4. **Dashboard**: Links to a specific dashboard within Dataverse.");
			xmlOutput.AppendLine("5. **Page**: Links to Dataverse **custom page**.");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("Here some examples of each type of subarea");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("### 1. Entity");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine(@"```xml
<SubArea Id=""subarea_e2c31883""
	Icon=""/_imgs/imagestrips/transparent_spacer.gif""
	Entity=""account""
	Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web""
	AvailableOffline=""true""
	PassParams=""false""
	Sku=""All,OnPremise,Live,SPLA""/>
```");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("### 2. WebResource");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine(@"```xml
<SubArea Id=""subarea_482f0bd3""
	ResourceId=""SitemapDesigner.NewSubArea""
	Icon=""/_imgs/imagestrips/transparent_spacer.gif""
	Url=""$webresource:mspp_metadata/attribute_selector.html""
	Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web""
	AvailableOffline=""true""
	PassParams=""false""
	Sku=""All,OnPremise,Live,SPLA"">
	<Titles>
		<Title LCID=""1033""
				Title=""Web Resource""/>
	</Titles>
</SubArea>
```");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("### 3. Url");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine(@"```xml
<SubArea Id=""subarea_7fd16b09""
	ResourceId=""SitemapDesigner.NewSubArea""
	Icon=""/_imgs/imagestrips/transparent_spacer.gif""
	Url=""https://www.google.it""
	Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web""
	AvailableOffline=""true""
	PassParams=""false""
	Sku=""All,OnPremise,Live,SPLA"">
	<Titles>
		<Title LCID=""1033""
				Title=""Navigation Link""/>
	</Titles>
</SubArea>
```");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("### 4. Dashboard");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine(@"```xml
<SubArea Id=""subarea_03fb38e8""
	DescriptionResourceId=""Dashboards_Description""
	Icon=""/_imgs/imagestrips/transparent_spacer.gif""
	Url=""/workplace/home_dashboards.aspx""
	DefaultDashboard=""5d431800-1270-40e9-a90a-c970556cf0fe""
	Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web""
	AvailableOffline=""true""
	PassParams=""false""
	Sku=""All,OnPremise,Live,SPLA""
	ToolTipResourseId=""DashboardTooltip"">
	<Titles>
		<Title LCID=""1033""
				Title=""Microsoft Dynamics 365 Social Overview""/>
	</Titles>
</SubArea>
```");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine("### 5. Page");
			xmlOutput.AppendLine();
			xmlOutput.AppendLine(@"```xml
<SubArea Id=""subarea_d4b8cdbb""
	Icon=""/_imgs/imagestrips/transparent_spacer.gif""
	Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web""
	AvailableOffline=""true""
	PassParams=""false""
	Sku=""All,OnPremise,Live,SPLA""
	Page=""ava_page1_414ff"">
	<Titles>
		<Title LCID=""1033""
				Title=""Page1""/>
	</Titles>
</SubArea>
```");
			xmlOutput.AppendLine();

			return xmlOutput.ToString();
		}


		private static async Task<string?> ReadResourceAsync(string resourceName)
		{
			var assembly = typeof(Schemas).Assembly;

			var stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				return null;
			}

			using var reader = new StreamReader(stream);
			var schema = await reader.ReadToEndAsync();
			return schema;
		}
	}
}
