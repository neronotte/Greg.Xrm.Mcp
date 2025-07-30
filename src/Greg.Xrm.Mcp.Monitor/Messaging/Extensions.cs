namespace Greg.Xrm.Mcp.Monitor.Messaging
{
	public static class Extensions
	{
		public static void Send<T>(this IMessenger messenger)
			where T : notnull, new()
		{
			if (messenger == null) return;

			messenger.Send(new T());
		}
	}
}
