using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using System.Text;

namespace Greg.Xrm.Mcp.Core
{
	public static class MiscellaneousExtensions
	{

		public static string Join(this IEnumerable<string> items, string separator)
		{
			if (items == null) return string.Empty;
			return string.Join(separator, items);
		}

		public static string OnlyLettersNumbersOrUnderscore(this string? text)
		{
			if (string.IsNullOrWhiteSpace(text)) return string.Empty;

			var sb = new StringBuilder();
			foreach (var c in text.Where(c => char.IsLetterOrDigit(c) || c == '_'))
			{
				sb.Append(c.ToString().ToLowerInvariant());
			}

			return sb.ToString();
		}


		public static async Task<int> GetDefaultLanguageCodeAsync(this IOrganizationServiceAsync2 crm, CancellationToken? cancellationToken = null)
		{
			cancellationToken ??= CancellationToken.None;

			var query = new QueryExpression("organization")
			{
				ColumnSet = new ColumnSet("languagecode"),
				TopCount = 1,
				NoLock = true
			};

			var result = await crm.RetrieveMultipleAsync(query, cancellationToken.Value);
			if (result.Entities.Count == 0)
			{
				throw new InvalidOperationException("Unable to retrieve the default language code. No organization found!");
			}

			var languageCode = result.Entities[0].GetAttributeValue<int>("languagecode");
			return languageCode;
		}
	}
}
