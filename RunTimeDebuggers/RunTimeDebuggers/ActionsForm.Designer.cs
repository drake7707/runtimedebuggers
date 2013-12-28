namespace RunTimeDebuggers
{
    partial class ActionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActionsForm));
            this.btnOpenAssemblyExplorer = new System.Windows.Forms.Button();
            this.lstOpenForms = new System.Windows.Forms.ListBox();
            this.lblExecutionPlace = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOpenLocals = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnConsole = new System.Windows.Forms.Button();
            this.btnOpenEmptyWatchWindow = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpenAssemblyExplorer
            // 
            this.btnOpenAssemblyExplorer.Location = new System.Drawing.Point(3, 3);
            this.btnOpenAssemblyExplorer.Name = "btnOpenAssemblyExplorer";
            this.btnOpenAssemblyExplorer.Size = new System.Drawing.Size(140, 23);
            this.btnOpenAssemblyExplorer.TabIndex = 3;
            this.btnOpenAssemblyExplorer.Text = "Open assembly explorer";
            this.btnOpenAssemblyExplorer.UseVisualStyleBackColor = true;
            this.btnOpenAssemblyExplorer.Click += new System.EventHandler(this.btnOpenAssemblyExplorer_Click);
            // 
            // lstOpenForms
            // 
            this.lstOpenForms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstOpenForms.FormattingEnabled = true;
            this.lstOpenForms.Location = new System.Drawing.Point(3, 16);
            this.lstOpenForms.Name = "lstOpenForms";
            this.lstOpenForms.Size = new System.Drawing.Size(593, 98);
            this.lstOpenForms.TabIndex = 4;
            // 
            // lblExecutionPlace
            // 
            this.lblExecutionPlace.AutoSize = true;
            this.lblExecutionPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExecutionPlace.Location = new System.Drawing.Point(3, 0);
            this.lblExecutionPlace.Name = "lblExecutionPlace";
            this.lblExecutionPlace.Size = new System.Drawing.Size(599, 20);
            this.lblExecutionPlace.TabIndex = 5;
            this.lblExecutionPlace.Text = "label1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstOpenForms);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 52);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(599, 117);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Open forms";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblExecutionPlace, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(605, 201);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnOpenLocals);
            this.flowLayoutPanel1.Controls.Add(this.btnRefresh);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 172);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(605, 29);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // btnOpenLocals
            // 
            this.btnOpenLocals.Location = new System.Drawing.Point(3, 3);
            this.btnOpenLocals.Name = "btnOpenLocals";
            this.btnOpenLocals.Size = new System.Drawing.Size(202, 23);
            this.btnOpenLocals.TabIndex = 3;
            this.btnOpenLocals.Text = "Open locals debugger on selected form";
            this.btnOpenLocals.UseVisualStyleBackColor = true;
            this.btnOpenLocals.Click += new System.EventHandler(this.btnOpenLocals_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(211, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(125, 23);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh open forms";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.btnOpenAssemblyExplorer);
            this.flowLayoutPanel2.Controls.Add(this.btnConsole);
            this.flowLayoutPanel2.Controls.Add(this.btnOpenEmptyWatchWindow);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 20);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(605, 29);
            this.flowLayoutPanel2.TabIndex = 8;
            // 
            // btnConsole
            // 
            this.btnConsole.Location = new System.Drawing.Point(149, 3);
            this.btnConsole.Name = "btnConsole";
            this.btnConsole.Size = new System.Drawing.Size(140, 23);
            this.btnConsole.TabIndex = 4;
            this.btnConsole.Text = "Show console window";
            this.btnConsole.UseVisualStyleBackColor = true;
            this.btnConsole.Click += new System.EventHandler(this.btnConsole_Click);
            // 
            // btnOpenEmptyWatchWindow
            // 
            this.btnOpenEmptyWatchWindow.Location = new System.Drawing.Point(295, 3);
            this.btnOpenEmptyWatchWindow.Name = "btnOpenEmptyWatchWindow";
            this.btnOpenEmptyWatchWindow.Size = new System.Drawing.Size(162, 23);
            this.btnOpenEmptyWatchWindow.TabIndex = 5;
            this.btnOpenEmptyWatchWindow.Text = "Open empty watch window";
            this.btnOpenEmptyWatchWindow.UseVisualStyleBackColor = true;
            this.btnOpenEmptyWatchWindow.Click += new System.EventHandler(this.btnOpenEmptyWatchWindow_Click);
            // 
            // ActionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 201);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ActionsForm";
            this.Text = "Runtime Debugger Actions  - By drake7707";
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpenAssemblyExplorer;
        private System.Windows.Forms.ListBox lstOpenForms;
        private System.Windows.Forms.Label lblExecutionPlace;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnOpenLocals;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button btnConsole;
        private System.Windows.Forms.Button btnOpenEmptyWatchWindow;
    }
}