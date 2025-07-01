using Microsoft.Crm.Sdk.Messages;

namespace Greg.Xrm.Mcp.Core.Services
{
	public interface IPublishXmlBuilder
	{
		void Clear();

		void AddWebResource(Guid id);
		void AddTable(string tableName);

		PublishXmlRequest? Build();
	}
}
