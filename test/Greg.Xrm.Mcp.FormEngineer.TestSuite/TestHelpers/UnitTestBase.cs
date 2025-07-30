using Microsoft.Extensions.Logging;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers
{
	/// <summary>
	/// Classe base per tutti i test unitari
	/// Fornisce configurazione comune e helper methods
	/// </summary>
	public abstract class UnitTestBase
	{
		protected ILogger<T> CreateMockLogger<T>()
		{
			return Substitute.For<ILogger<T>>();
		}

		protected void VerifyLoggerWasCalled<T>(ILogger<T> logger, LogLevel level, string expectedMessage)
		{
			logger.Received().Log(
				level,
				Arg.Any<EventId>(),
				Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
				Arg.Any<Exception>(),
				Arg.Any<Func<object, Exception?, string>>());
		}

		/// <summary>
		/// Helper per creare GUIDs deterministici nei test
		/// </summary>
		protected Guid CreateTestGuid(int seed = 1)
		{
			var bytes = new byte[16];
			BitConverter.GetBytes(seed).CopyTo(bytes, 0);
			return new Guid(bytes);
		}
	}
}