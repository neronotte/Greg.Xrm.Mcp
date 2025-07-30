using Greg.Xrm.Mcp.Monitor.Logging;
using Greg.Xrm.Mcp.Monitor.Messaging;
using Greg.Xrm.Mcp.Monitor.Views;
using WeifenLuo.WinFormsUI.Docking;

namespace Greg.Xrm.Mcp.Monitor
{
	public partial class MainForm : Form
	{
		private IMessenger? messenger;
		private readonly LogViewer logViewer = new();

		public MainForm()
		{
			InitializeComponent();

			this.logViewer.Show(this.dockPanel, DockState.Document);
		}

		public void SetMessenger(IMessenger messenger)
		{
			this.messenger = messenger;
			this.messenger.Register<LogRequestPayload>(this.logViewer.AddLog);
		}
	}
}
