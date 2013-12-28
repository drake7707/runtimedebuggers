namespace RunTimeDebuggers.ConsoleDebugger
{
    partial class ConsoleWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConsoleWindow));
            this.tmr = new System.Windows.Forms.Timer(this.components);
            this.customTableLayout1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnPause = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnShowPrefix = new System.Windows.Forms.ToolStripButton();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tpConsole = new System.Windows.Forms.TabPage();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.tpTrace = new System.Windows.Forms.TabPage();
            this.txtTrace = new System.Windows.Forms.TextBox();
            this.customTableLayout1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tpConsole.SuspendLayout();
            this.tpTrace.SuspendLayout();
            this.SuspendLayout();
            // 
            // tmr
            // 
            this.tmr.Enabled = true;
            this.tmr.Tick += new System.EventHandler(this.tmr_Tick);
            // 
            // customTableLayout1
            // 
            this.customTableLayout1.ColumnCount = 1;
            this.customTableLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.customTableLayout1.Controls.Add(this.toolStrip1, 0, 0);
            this.customTableLayout1.Controls.Add(this.tabs, 0, 1);
            this.customTableLayout1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customTableLayout1.Location = new System.Drawing.Point(0, 0);
            this.customTableLayout1.Name = "customTableLayout1";
            this.customTableLayout1.RowCount = 2;
            this.customTableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.customTableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.customTableLayout1.Size = new System.Drawing.Size(671, 336);
            this.customTableLayout1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnPause,
            this.toolStripSeparator,
            this.btnCopy,
            this.toolStripSeparator1,
            this.btnShowPrefix});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(671, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnPause
            // 
            this.btnPause.CheckOnClick = true;
            this.btnPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPause.Image = ((System.Drawing.Image)(resources.GetObject("btnPause.Image")));
            this.btnPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(23, 22);
            this.btnPause.Text = "Pause the console";
            this.btnPause.ToolTipText = "Pause the console";
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // btnCopy
            // 
            this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopy.Image = ((System.Drawing.Image)(resources.GetObject("btnCopy.Image")));
            this.btnCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(23, 22);
            this.btnCopy.Text = "&Copy";
            this.btnCopy.ToolTipText = "Copy console output to clipboard";
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnShowPrefix
            // 
            this.btnShowPrefix.Checked = true;
            this.btnShowPrefix.CheckOnClick = true;
            this.btnShowPrefix.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnShowPrefix.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowPrefix.Image = ((System.Drawing.Image)(resources.GetObject("btnShowPrefix.Image")));
            this.btnShowPrefix.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowPrefix.Name = "btnShowPrefix";
            this.btnShowPrefix.Size = new System.Drawing.Size(23, 22);
            this.btnShowPrefix.Text = "Show time and place prefix";
            this.btnShowPrefix.Click += new System.EventHandler(this.btnShowPrefix_Click);
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tpConsole);
            this.tabs.Controls.Add(this.tpTrace);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 25);
            this.tabs.Margin = new System.Windows.Forms.Padding(0, 0, 0, 7);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(671, 304);
            this.tabs.TabIndex = 3;
            // 
            // tpConsole
            // 
            this.tpConsole.Controls.Add(this.txtConsole);
            this.tpConsole.Location = new System.Drawing.Point(4, 22);
            this.tpConsole.Name = "tpConsole";
            this.tpConsole.Size = new System.Drawing.Size(663, 278);
            this.tpConsole.TabIndex = 0;
            this.tpConsole.Text = "Console";
            this.tpConsole.UseVisualStyleBackColor = true;
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.Color.Black;
            this.txtConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConsole.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConsole.ForeColor = System.Drawing.Color.Silver;
            this.txtConsole.Location = new System.Drawing.Point(0, 0);
            this.txtConsole.Margin = new System.Windows.Forms.Padding(0);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConsole.Size = new System.Drawing.Size(663, 278);
            this.txtConsole.TabIndex = 2;
            // 
            // tpTrace
            // 
            this.tpTrace.Controls.Add(this.txtTrace);
            this.tpTrace.Location = new System.Drawing.Point(4, 22);
            this.tpTrace.Margin = new System.Windows.Forms.Padding(0);
            this.tpTrace.Name = "tpTrace";
            this.tpTrace.Size = new System.Drawing.Size(663, 278);
            this.tpTrace.TabIndex = 1;
            this.tpTrace.Text = "Trace";
            this.tpTrace.UseVisualStyleBackColor = true;
            // 
            // txtTrace
            // 
            this.txtTrace.BackColor = System.Drawing.Color.White;
            this.txtTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTrace.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTrace.ForeColor = System.Drawing.Color.Black;
            this.txtTrace.Location = new System.Drawing.Point(0, 0);
            this.txtTrace.Margin = new System.Windows.Forms.Padding(0);
            this.txtTrace.Multiline = true;
            this.txtTrace.Name = "txtTrace";
            this.txtTrace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTrace.Size = new System.Drawing.Size(663, 278);
            this.txtTrace.TabIndex = 3;
            // 
            // ConsoleWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 336);
            this.Controls.Add(this.customTableLayout1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConsoleWindow";
            this.Text = "Console";
            this.customTableLayout1.ResumeLayout(false);
            this.customTableLayout1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabs.ResumeLayout(false);
            this.tpConsole.ResumeLayout(false);
            this.tpConsole.PerformLayout();
            this.tpTrace.ResumeLayout(false);
            this.tpTrace.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel customTableLayout1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TextBox txtConsole;
        private System.Windows.Forms.ToolStripButton btnPause;
        private System.Windows.Forms.Timer tmr;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnShowPrefix;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tpConsole;
        private System.Windows.Forms.TabPage tpTrace;
        private System.Windows.Forms.TextBox txtTrace;
    }
}