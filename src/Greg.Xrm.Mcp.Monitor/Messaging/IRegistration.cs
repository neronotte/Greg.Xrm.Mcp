namespace Greg.Xrm.Mcp.Monitor.Messaging
{
	public interface IRegistration
	{
		Guid Id { get; }
		void Execute(object message);
	}
}
