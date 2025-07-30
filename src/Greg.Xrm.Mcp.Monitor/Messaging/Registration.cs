namespace Greg.Xrm.Mcp.Monitor.Messaging
{
	public class Registration<T> : IRegistration
	{
		private readonly IMessageHandler<T> handler;

		public Registration(IMessageHandler<T> handler)
		{
			Id = Guid.NewGuid();
			this.handler = handler;
		}

		public Guid Id { get; }

		public void Execute(object message)
		{
			handler.Handle((T)message);
		}
	}
}
