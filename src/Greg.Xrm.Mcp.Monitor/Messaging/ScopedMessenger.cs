namespace Greg.Xrm.Mcp.Monitor.Messaging
{
	public class ScopedMessenger(IMessenger messenger) : IScopedMessenger
	{
		private readonly List<Guid> registrations = [];
		private bool disposedValue;


		public Guid Register<T>(Action<T> callback)
		{
			ObjectDisposedException.ThrowIf(disposedValue, nameof(ScopedMessenger));

			var id = messenger.Register(callback);
			registrations.Add(id);
			return id;
		}

		public Guid Register<T>(IMessageHandler<T> callback)
		{
			ObjectDisposedException.ThrowIf(disposedValue, nameof(ScopedMessenger));

			var id = messenger.Register(callback);
			registrations.Add(id);
			return id;
		}

		public void Send<T>(T message)
			where T : notnull
		{
			ObjectDisposedException.ThrowIf(disposedValue, nameof(ScopedMessenger));

			messenger.Send(message);
		}

		public void Send<T>() where T : notnull, new()
		{
			ObjectDisposedException.ThrowIf(disposedValue, nameof(ScopedMessenger));

			messenger.Send<T>();
		}

		public void Unregister(Guid registrationId)
		{
			ObjectDisposedException.ThrowIf(disposedValue, nameof(ScopedMessenger));

			registrations.Remove(registrationId);
			messenger.Unregister(registrationId);
		}

		public IScopedMessenger CreateScope()
		{
			return new ScopedMessenger(messenger);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var id in registrations)
					{
						messenger.Unregister(id);
					}
					registrations.Clear();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
