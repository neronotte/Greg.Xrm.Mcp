using System.Collections;
using System.Text;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	public class FormXmlValidationResult : IReadOnlyList<FormXmlValidationMessage>
	{
		private readonly List<FormXmlValidationMessage> messages = new List<FormXmlValidationMessage>();

		public FormXmlValidationMessage this[int index] => messages[index];

		public int Count => messages.Count;

		public IEnumerator<FormXmlValidationMessage> GetEnumerator()
		{
		return messages.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public FormXmlValidationResult AddError(string message)
		{
			this.messages.Add(new FormXmlValidationMessage(FormXmlValidationLevel.Error, message));
			return this;
		}

		public FormXmlValidationResult AddError(FormXmlValidationMessage message)
		{
			this.messages.Add(message);
			return this;
		}

		public FormXmlValidationResult AddWarning(string message)
		{
			this.messages.Add(new FormXmlValidationMessage(FormXmlValidationLevel.Warning, message));
			return this;
		}

		public FormXmlValidationResult AddWarning(FormXmlValidationMessage message)
		{
			this.messages.Add(message);
			return this;
		}

		public bool IsValid => !messages.Any(m => m.Level == FormXmlValidationLevel.Error);

		public bool HasWarnings => messages.Any(m => m.Level == FormXmlValidationLevel.Warning);

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (IsValid)
			{
				sb.AppendLine("Form XML is valid.");
			}
			else
			{
				sb.AppendLine("Form XML has validation issues:");
				foreach (var message in messages)
				{
					sb.AppendLine($"- {message.Level}: {message.Message}");
				}
			}

			return sb.ToString();
		}
	}
}
