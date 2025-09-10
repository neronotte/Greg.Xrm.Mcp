using Microsoft.Crm.Sdk.Messages;
using System.Xml.Linq;

namespace Greg.Xrm.Mcp.Core.Services
{
	public class PublishXmlBuilder : IPublishXmlBuilder
	{
		private readonly List<string> tableList = [];

		private readonly XDocument xml;
		private readonly XElement sitemaps;
		private readonly XElement appmodules;
		private readonly XElement webresources;
		private readonly XElement tables;
		private readonly XElement[] elements = [];


		public PublishXmlBuilder()
		{
			this.xml = new XDocument(
				new XElement("importexportxml")
			);
			this.sitemaps = new XElement("sitemaps");
			this.appmodules = new XElement("appmodules");
			this.webresources = new XElement("webresources");
			this.tables = new XElement("tables");
			this.elements = [this.sitemaps, this.appmodules, this.webresources, this.tables];

			Clear();
		}


		public void Clear()
		{
			foreach (var element in this.elements)
			{
				element.RemoveNodes();
			}
		}

		public void AddSiteMap(Guid id)
		{
			this.sitemaps.Add(new XElement("sitemap", id));
		}

		public void AddAppModule(Guid id)
		{
			this.appmodules.Add(new XElement("appmodule", id));
		}

		public void AddWebResource(Guid id)
		{
			this.webresources.Add(new XElement("webresource", id));
		}


		public void AddTable(string tableName)
		{
			if (!this.tableList.Contains(tableName))
			{
				this.tableList.Add(tableName);
				this.tables.Add(new XElement("entity", tableName));
			}
		}


		public PublishXmlRequest? Build()
		{
			foreach (var element in this.elements)
			{
				if (element.HasElements)
				{
					xml.Root?.Add(element);
				}
			}

			var request = new PublishXmlRequest
			{
				ParameterXml = this.xml.ToString()
			};

			this.xml.Root?.RemoveNodes();
			return request;
		}
	}
}
