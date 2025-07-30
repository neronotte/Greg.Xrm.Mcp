using WeifenLuo.WinFormsUI.Docking;

namespace Greg.Xrm.Mcp.Monitor
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			toolStrip1 = new ToolStrip();
			statusStrip1 = new StatusStrip();
			dockPanel = new DockPanel();
			SuspendLayout();
			// 
			// toolStrip1
			// 
			toolStrip1.Location = new Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new Size(1483, 25);
			toolStrip1.TabIndex = 0;
			toolStrip1.Text = "toolStrip1";
			// 
			// statusStrip1
			// 
			statusStrip1.Location = new Point(0, 875);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(1483, 22);
			statusStrip1.TabIndex = 1;
			statusStrip1.Text = "statusStrip1";
			// 
			// dockPanel
			// 
			dockPanel.Dock = DockStyle.Fill;
			dockPanel.Name = "dockPanel";
			dockPanel.Theme = new WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme();
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1483, 897);
			Controls.Add(dockPanel);
			Controls.Add(statusStrip1);
			Controls.Add(toolStrip1);
			Name = "MainForm";
			Text = "Form1";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DockPanel dockPanel;
		private ToolStrip toolStrip1;
		private StatusStrip statusStrip1;
	}
}
