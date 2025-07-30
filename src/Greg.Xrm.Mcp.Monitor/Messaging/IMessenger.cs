using System;

namespace Greg.Xrm.Mcp.Monitor.Messaging
{
	public interface IMessenger
	{
		void Send<T>(T message)
			where T : notnull;

		void Send<T>() 
			where T : notnull, new();

		Guid Register<T>(Action<T> callback);

		Guid Register<T>(IMessageHandler<T> callback);

		void Unregister(Guid registrationId);


		IScopedMessenger CreateScope();
	}
}
