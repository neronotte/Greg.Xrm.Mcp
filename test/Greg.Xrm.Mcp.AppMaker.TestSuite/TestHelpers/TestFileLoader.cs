using System.Reflection;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers
{
	/// <summary>
	/// Helper per caricare file di test embedded o da filesystem
	/// </summary>
	public static class TestFileLoader
	{
		private static readonly string TestDataDirectory = Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
			"TestData");

		/// <summary>
		/// Carica il contenuto di un file XML di form valido per i test
		/// </summary>
		/// <param name="fileName">Nome del file (senza path)</param>
		/// <returns>Contenuto del file XML</returns>
		public static string LoadValidFormXml(string fileName)
		{
			var filePath = Path.Combine(TestDataDirectory, "FormXml", "Valid", fileName);
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException($"Test file not found: {filePath}");
			}
			return File.ReadAllText(filePath);
		}

		/// <summary>
		/// Carica il contenuto di un file XML di form non valido per i test
		/// </summary>
		/// <param name="fileName">Nome del file (senza path)</param>
		/// <returns>Contenuto del file XML</returns>
		public static string LoadInvalidFormXml(string fileName)
		{
			var filePath = Path.Combine(TestDataDirectory, "FormXml", "Invalid", fileName);
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException($"Test file not found: {filePath}");
			}
			return File.ReadAllText(filePath);
		}

		/// <summary>
		/// Carica tutti i file XML validi disponibili per test parametrizzati
		/// </summary>
		/// <returns>Array di tuple (nome file, contenuto XML)</returns>
		public static (string fileName, string content)[] LoadAllValidFormXml()
		{
			var validDir = Path.Combine(TestDataDirectory, "FormXml", "Valid");
			if (!Directory.Exists(validDir))
			{
				return Array.Empty<(string, string)>();
			}

			return Directory.GetFiles(validDir, "*.xml")
				.Select(filePath => (
					fileName: Path.GetFileName(filePath),
					content: File.ReadAllText(filePath)
				))
				.ToArray();
		}

		/// <summary>
		/// Carica tutti i file XML non validi disponibili per test parametrizzati
		/// </summary>
		/// <returns>Array di tuple (nome file, contenuto XML)</returns>
		public static (string fileName, string content)[] LoadAllInvalidFormXml()
		{
			var invalidDir = Path.Combine(TestDataDirectory, "FormXml", "Invalid");
			if (!Directory.Exists(invalidDir))
			{
				return Array.Empty<(string, string)>();
			}

			return Directory.GetFiles(invalidDir, "*.xml")
				.Select(filePath => (
					fileName: Path.GetFileName(filePath),
					content: File.ReadAllText(filePath)
				))
				.ToArray();
		}

		/// <summary>
		/// Verifica che la directory dei test data esista e sia accessibile
		/// </summary>
		/// <returns>True se la directory esiste</returns>
		public static bool IsTestDataDirectoryAvailable()
		{
			return Directory.Exists(TestDataDirectory);
		}

		/// <summary>
		/// Crea la struttura di directory per i test data se non esiste
		/// </summary>
		public static void EnsureTestDataDirectoryExists()
		{
			var validDir = Path.Combine(TestDataDirectory, "FormXml", "Valid");
			var invalidDir = Path.Combine(TestDataDirectory, "FormXml", "Invalid");
			var schemasDir = Path.Combine(TestDataDirectory, "FormXml", "Schemas");
			var fixturesDir = Path.Combine(TestDataDirectory, "Fixtures");

			Directory.CreateDirectory(validDir);
			Directory.CreateDirectory(invalidDir);
			Directory.CreateDirectory(schemasDir);
			Directory.CreateDirectory(fixturesDir);
		}
	}
}