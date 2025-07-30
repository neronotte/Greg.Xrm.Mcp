namespace Greg.Xrm.Mcp.Monitor.Views
{
	partial class LogViewer
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogViewer));
			images = new ImageList(components);
			tools = new ToolStrip();
			grid = new BrightIdeasSoftware.ObjectListView();
			toolStripButton1 = new ToolStripButton();
			toolStripButton2 = new ToolStripButton();
			toolStripButton3 = new ToolStripButton();
			toolStripButton4 = new ToolStripButton();
			toolStripButton5 = new ToolStripButton();
			toolStripButton6 = new ToolStripButton();
			tools.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)grid).BeginInit();
			SuspendLayout();
			// 
			// images
			// 
			images.ColorDepth = ColorDepth.Depth32Bit;
			images.ImageStream = (ImageListStreamer)resources.GetObject("images.ImageStream");
			images.TransparentColor = Color.Transparent;
			images.Images.SetKeyName(0, "trace");
			images.Images.SetKeyName(1, "debug");
			images.Images.SetKeyName(2, "information");
			images.Images.SetKeyName(3, "warning");
			images.Images.SetKeyName(4, "error");
			images.Images.SetKeyName(5, "critical");
			images.Images.SetKeyName(6, "critical2");
			images.Images.SetKeyName(7, "accept.png");
			images.Images.SetKeyName(8, "debug1");
			// 
			// tools
			// 
			tools.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3, toolStripButton4, toolStripButton5, toolStripButton6 });
			tools.Location = new Point(0, 0);
			tools.Name = "tools";
			tools.Size = new Size(772, 25);
			tools.TabIndex = 1;
			tools.Text = "toolStrip1";
			// 
			// grid
			// 
			grid.Dock = DockStyle.Fill;
			grid.FullRowSelect = true;
			grid.Location = new Point(0, 25);
			grid.Name = "grid";
			grid.Size = new Size(772, 434);
			grid.TabIndex = 2;
			grid.View = View.Details;
			// 
			// toolStripButton1
			// 
			toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
			toolStripButton1.ImageTransparentColor = Color.Magenta;
			toolStripButton1.Name = "toolStripButton1";
			toolStripButton1.Size = new Size(23, 22);
			toolStripButton1.Text = "toolStripButton1";
			// 
			// toolStripButton2
			// 
			toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
			toolStripButton2.ImageTransparentColor = Color.Magenta;
			toolStripButton2.Name = "toolStripButton2";
			toolStripButton2.Size = new Size(23, 22);
			toolStripButton2.Text = "toolStripButton2";
			// 
			// toolStripButton3
			// 
			toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
			toolStripButton3.ImageTransparentColor = Color.Magenta;
			toolStripButton3.Name = "toolStripButton3";
			toolStripButton3.Size = new Size(23, 22);
			toolStripButton3.Text = "toolStripButton3";
			// 
			// toolStripButton4
			// 
			toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
			toolStripButton4.ImageTransparentColor = Color.Magenta;
			toolStripButton4.Name = "toolStripButton4";
			toolStripButton4.Size = new Size(23, 22);
			toolStripButton4.Text = "toolStripButton4";
			// 
			// toolStripButton5
			// 
			toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolStripButton5.Image = (Image)resources.GetObject("toolStripButton5.Image");
			toolStripButton5.ImageTransparentColor = Color.Magenta;
			toolStripButton5.Name = "toolStripButton5";
			toolStripButton5.Size = new Size(23, 22);
			toolStripButton5.Text = "toolStripButton5";
			// 
			// toolStripButton6
			// 
			toolStripButton6.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolStripButton6.Image = (Image)resources.GetObject("toolStripButton6.Image");
			toolStripButton6.ImageTransparentColor = Color.Magenta;
			toolStripButton6.Name = "toolStripButton6";
			toolStripButton6.Size = new Size(23, 22);
			toolStripButton6.Text = "toolStripButton6";
			// 
			// LogViewer
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(772, 459);
			CloseButton = false;
			CloseButtonVisible = false;
			Controls.Add(grid);
			Controls.Add(tools);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Name = "LogViewer";
			tools.ResumeLayout(false);
			tools.PerformLayout();
			((System.ComponentModel.ISupportInitialize)grid).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private ImageList images;
		private ToolStrip tools;
		private BrightIdeasSoftware.ObjectListView grid;
		private ToolStripButton toolStripButton1;
		private ToolStripButton toolStripButton2;
		private ToolStripButton toolStripButton3;
		private ToolStripButton toolStripButton4;
		private ToolStripButton toolStripButton5;
		private ToolStripButton toolStripButton6;
	}
}
