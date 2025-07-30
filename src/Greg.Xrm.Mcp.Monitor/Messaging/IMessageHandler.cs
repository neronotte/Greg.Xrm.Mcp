namespace Greg.Xrm.Mcp.Monitor.Messaging
{
	public interface IMessageHandler<T>
	{
		void Handle(T message);
	}
}
