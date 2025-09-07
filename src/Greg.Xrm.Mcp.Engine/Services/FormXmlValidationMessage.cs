namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	public record FormXmlValidationMessage(FormXmlValidationLevel Level, string Message)
	{
		/// <summary>
		/// Row number where the error occurred (if applicable)
		/// </summary>
		public int? Row { get; init; }

		/// <summary>
		/// Column number where the error occurred (if applicable)
		/// </summary>
		public int? Column { get; init; }

		/// <summary>
		/// Fixed part of the error message (the template/category)
		/// </summary>
		public string? FixedPart { get; init; }

		/// <summary>
		/// Variable part of the error message (the specific details)
		/// </summary>
		public string? VariablePart { get; init; }

		/// <summary>
		/// Creates a new FormXmlValidationMessage with separated fixed and variable parts
		/// </summary>
		/// <param name="level">Validation level</param>
		/// <param name="fixedPart">Fixed part of the message (template/category)</param>
		/// <param name="variablePart">Variable part of the message (specific details)</param>
		/// <param name="row">Row number where error occurred</param>
		/// <param name="column">Column number where error occurred</param>
		/// <returns>New FormXmlValidationMessage instance</returns>
		public static FormXmlValidationMessage Create(FormXmlValidationLevel level, string fixedPart, string? variablePart = null, int? row = null, int? column = null)
		{
			var message = string.IsNullOrEmpty(variablePart) ? fixedPart : $"{fixedPart}: {variablePart}";
			
			return new FormXmlValidationMessage(level, message)
			{
				Row = row,
				Column = column,
				FixedPart = fixedPart,
				VariablePart = variablePart
			};
		}

		/// <summary>
		/// Creates a new FormXmlValidationMessage from an existing message by parsing line/position information
		/// </summary>
		/// <param name="level">Validation level</param>
		/// <param name="message">Complete error message</param>
		/// <returns>New FormXmlValidationMessage instance with parsed location info</returns>
		public static FormXmlValidationMessage FromMessage(FormXmlValidationLevel level, string message)
		{
			int? row = null;
			int? column = null;
			string? fixedPart = null;
			string? variablePart = null;

			// Try to parse "Line X, Position Y: " pattern
			var linePositionPattern = @"Line (\d+), Position (\d+): (.*)";
			var match = System.Text.RegularExpressions.Regex.Match(message, linePositionPattern);
			
			if (match.Success)
			{
				row = int.Parse(match.Groups[1].Value);
				column = int.Parse(match.Groups[2].Value);
				var remainingMessage = match.Groups[3].Value;
				
				// Try to separate fixed and variable parts from the remaining message
				var separatedParts = SeparateMessageParts(remainingMessage);
				fixedPart = separatedParts.fixedPart;
				variablePart = separatedParts.variablePart;
			}
			else
			{
				// No line/position info, just try to separate the message parts
				var separatedParts = SeparateMessageParts(message);
				fixedPart = separatedParts.fixedPart;
				variablePart = separatedParts.variablePart;
			}

			return new FormXmlValidationMessage(level, message)
			{
				Row = row,
				Column = column,
				FixedPart = fixedPart,
				VariablePart = variablePart
			};
		}

		/// <summary>
		/// Attempts to separate a message into fixed and variable parts
		/// </summary>
		/// <param name="message">Complete message to separate</param>
		/// <returns>Tuple with separated fixed and variable parts</returns>
		private static (string fixedPart, string? variablePart) SeparateMessageParts(string message)
		{
			// Common patterns for separating fixed from variable parts
			var patterns = new[]
			{
				// "The element 'X' is invalid" -> fixed: "The element is invalid", variable: "X"
				(@"The element '([^']+)' is invalid", "The element is invalid", "$1"),
				
				// "The attribute 'X' is invalid" -> fixed: "The attribute is invalid", variable: "X"
				(@"The attribute '([^']+)' is invalid", "The attribute is invalid", "$1"),
				
				// "Required attribute 'X' is missing" -> fixed: "Required attribute is missing", variable: "X"
				(@"Required attribute '([^']+)' is missing", "Required attribute is missing", "$1"),
				
				// "Invalid value 'X' for attribute 'Y'" -> fixed: "Invalid value for attribute", variable: "X for Y"
				(@"Invalid value '([^']+)' for attribute '([^']+)'", "Invalid value for attribute", "$1 for $2"),
				
				// "Element 'X' cannot contain element 'Y'" -> fixed: "Element cannot contain element", variable: "X cannot contain Y"
				(@"Element '([^']+)' cannot contain element '([^']+)'", "Element cannot contain element", "$1 cannot contain $2"),
				
				// Generic pattern: "Something 'X' something" -> try to extract the quoted part
				(@"([^']*)'([^']+)'(.*)", null, "$2") // Will use the middle part as variable, construct fixed part
			};

			foreach (var (pattern, fixedTemplate, variableTemplate) in patterns)
			{
				var match = System.Text.RegularExpressions.Regex.Match(message, pattern);
				if (match.Success)
				{
					var variablePart = match.Result(variableTemplate);
					var fixedPart = fixedTemplate ?? 
						System.Text.RegularExpressions.Regex.Replace(message, pattern, match.Groups[1].Value + match.Groups[3].Value).Trim();
					
					return (fixedPart, variablePart);
				}
			}

			// If no pattern matches, return the whole message as fixed part
			return (message, null);
		}
	}
}
