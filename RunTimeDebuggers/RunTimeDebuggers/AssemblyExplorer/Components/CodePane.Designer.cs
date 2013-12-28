namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class CodePane
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlContainer = new System.Windows.Forms.Panel();
            this.tabsCode = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtInfo = new RunTimeDebuggers.AssemblyExplorer.HighlightRTB();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.diagram = new RunTimeDebuggers.AssemblyExplorer.ILNodeDiagram();
            this.resourcePane = new RunTimeDebuggers.AssemblyExplorer.Components.ResourcePane();
            this.pnl = new System.Windows.Forms.Panel();
            this.txtInfoTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlContainer.SuspendLayout();
            this.tabsCode.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pnlContainer, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnl, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(815, 407);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pnlContainer
            // 
            this.pnlContainer.Controls.Add(this.txtInfo);
            this.pnlContainer.Controls.Add(this.resourcePane);
            this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContainer.Location = new System.Drawing.Point(20, 0);
            this.pnlContainer.Margin = new System.Windows.Forms.Padding(0);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(795, 407);
            this.pnlContainer.TabIndex = 0;
            // 
            // tabsCode
            // 
            this.tabsCode.Controls.Add(this.tabPage1);
            this.tabsCode.Controls.Add(this.tabPage2);
            this.tabsCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabsCode.Location = new System.Drawing.Point(0, 0);
            this.tabsCode.Name = "tabsCode";
            this.tabsCode.SelectedIndex = 0;
            this.tabsCode.Size = new System.Drawing.Size(829, 439);
            this.tabsCode.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(821, 413);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "IL";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtInfo
            // 
            this.txtInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtInfo.DetectUrls = false;
            this.txtInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInfo.Location = new System.Drawing.Point(0, 0);
            this.txtInfo.Margin = new System.Windows.Forms.Padding(0);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(795, 407);
            this.txtInfo.TabIndex = 1;
            this.txtInfo.Text = "";
            this.txtInfo.WordWrap = false;
            this.txtInfo.VScroll += new System.EventHandler(this.txtInfo_VScroll);
            this.txtInfo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txtInfo_MouseMove);
            this.txtInfo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtInfo_MouseUp);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.diagram);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(821, 413);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "IL visualisation";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // diagram
            // 
            this.diagram.AutoScroll = true;
            this.diagram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diagram.LineType = NodeControl.LineTypeEnum.Bezier;
            this.diagram.Location = new System.Drawing.Point(3, 3);
            this.diagram.Name = "diagram";
            this.diagram.NodeSize = new System.Drawing.Size(100, 50);
            this.diagram.Size = new System.Drawing.Size(815, 407);
            this.diagram.TabIndex = 0;
            // 
            // resourcePane
            // 
            this.resourcePane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourcePane.Location = new System.Drawing.Point(0, 0);
            this.resourcePane.Name = "resourcePane";
            this.resourcePane.Size = new System.Drawing.Size(795, 407);
            this.resourcePane.TabIndex = 2;
            // 
            // pnl
            // 
            this.pnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl.Location = new System.Drawing.Point(0, 0);
            this.pnl.Margin = new System.Windows.Forms.Padding(0);
            this.pnl.Name = "pnl";
            this.pnl.Size = new System.Drawing.Size(20, 407);
            this.pnl.TabIndex = 2;
            this.pnl.Paint += new System.Windows.Forms.PaintEventHandler(this.pnl_Paint);
            this.pnl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnl_MouseDown);
            // 
            // CodePane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabsCode);
            this.Name = "CodePane";
            this.Size = new System.Drawing.Size(829, 439);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlContainer.ResumeLayout(false);
            this.tabsCode.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private HighlightRTB txtInfo;
        private System.Windows.Forms.ToolTip txtInfoTooltip;
        private System.Windows.Forms.Panel pnl;
        private System.Windows.Forms.Panel pnlContainer;
        private Components.ResourcePane resourcePane;
        private System.Windows.Forms.TabControl tabsCode;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private ILNodeDiagram diagram;
    }
}
