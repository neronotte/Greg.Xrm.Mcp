using BrightIdeasSoftware;
using Greg.Xrm.Mcp.Monitor.Logging;
using Microsoft.Extensions.Logging;
using WeifenLuo.WinFormsUI.Docking;

namespace Greg.Xrm.Mcp.Monitor.Views
{
	public partial class LogViewer : DockContent
	{
		public LogViewer()
		{
			InitializeComponent();
			this.Text = this.TabText = "Log Viewer";

			this.grid.Columns.Add(new OLVColumn
			{
				Text = "Level",
				Sortable = false,
				ImageGetter = x => GetImage((LogRequestPayload)x),
				CellVerticalAlignment = StringAlignment.Center,
				TextAlign = HorizontalAlignment.Center,
				Width = 60
			});
			this.grid.Columns.Add(new OLVColumn
			{
				Text = "Timestamp",
				Sortable = true,
				AspectName = nameof(LogRequestPayload.Timestamp),
				AspectToStringConverter = x => ((DateTime)x).ToString("dd/MM/yyyy HH:mm:ss.fff"),
				Width = 200,
			});
			this.grid.Columns.Add(new OLVColumn
			{
				Text = "Category",
				Sortable = false,
				AspectName = nameof(LogRequestPayload.Category),
				Width = 300,
			});
			this.grid.Columns.Add(new OLVColumn
			{
				Text = "Message",
				Sortable = false,
				AspectName = nameof(LogRequestPayload.Message),
				Width = 800,
				FillsFreeSpace = true,
			});

			this.grid.RowFormatter = item =>
			{
				if (item.RowObject is not LogRequestPayload log) return;

				item.ForeColor = log.Level switch
				{
					LogLevel.Trace => Color.FromArgb(166, 166, 166),
					LogLevel.Debug => Color.FromArgb(78, 167, 46),
					LogLevel.Information => Color.FromArgb(77, 147, 217),
					LogLevel.Warning => Color.FromArgb(233, 113, 50),
					LogLevel.Error => Color.FromArgb(255, 0, 0),
					LogLevel.Critical => Color.FromArgb(192, 0, 0),
					_ => Color.Black
				};
			};
		}

		private Image GetImage(LogRequestPayload x) => images.Images[(int)x.Level];

		public void AddLog(LogRequestPayload message)
		{
			this.grid.AddObject(message);
		}
	}
}
