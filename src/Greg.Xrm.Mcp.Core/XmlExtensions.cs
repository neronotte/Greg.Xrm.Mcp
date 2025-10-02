using System.Text;

namespace Greg.Xrm.Mcp.Core
{
	public static class XmlExtensions
	{
		/// <summary>
		/// Removes XML declarations from the input string.
		/// This method removes anything between &lt;? and ?&gt; (e.g., &lt;?xml version="1.0" encoding="utf-8"?&gt;).
		/// </summary>
		/// <param name="xmlString">The XML string that may contain XML declarations.</param>
		/// <returns>The XML string with all XML declarations removed.</returns>
		public static string? RemoveXmlDeclaration(this string? xmlString)
		{
			if (string.IsNullOrEmpty(xmlString))
				return xmlString;

			xmlString = xmlString.Trim();

			var result = new StringBuilder(xmlString.Length);
			int i = 0;
			
			while (i < xmlString.Length)
			{
				// Check if we found the start of a processing instruction
				if (i < xmlString.Length - 1 && xmlString[i] == '<' && xmlString[i + 1] == '?')
				{
					// Find the end of the processing instruction
					int endIndex = xmlString.IndexOf("?>", i + 2);
					if (endIndex != -1)
					{
						// Skip past the entire processing instruction
						i = endIndex + 2;
						continue;
					}
				}
				
				// Add the current character and move to the next
				result.Append(xmlString[i]);
				i++;
			}
			
			return result.ToString();
		}
	}
}
