namespace Greg.Xrm.Mcp.Monitor.Messaging
{
	public class MessageHandlerDelegate<T> : IMessageHandler<T>
	{
		private readonly Action<T> callback;

		public MessageHandlerDelegate(Action<T> callback)
		{
			this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
		}

		public void Handle(T message)
		{
			callback(message);
		}
	}
}
