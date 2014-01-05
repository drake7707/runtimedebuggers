using RunTimeDebuggers.Controls;
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
            this.txtNewInfo = new RunTimeDebuggers.Controls.CodeTextBox();
            this.resourcePane = new RunTimeDebuggers.AssemblyExplorer.Components.ResourcePane();
            this.tabsCode = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.diagram = new RunTimeDebuggers.AssemblyExplorer.ILNodeDiagram();
            this.txtInfoTooltip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.txtNewInfo)).BeginInit();
            this.tabsCode.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtNewInfo
            // 
            this.txtNewInfo.AllowSeveralTextStyleDrawing = true;
            this.txtNewInfo.AutoScrollMinSize = new System.Drawing.Size(42, 14);
            this.txtNewInfo.BackBrush = null;
            this.txtNewInfo.CharHeight = 14;
            this.txtNewInfo.CharWidth = 8;
            this.txtNewInfo.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtNewInfo.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtNewInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNewInfo.DoLineSelectWhenClickedInMargin = false;
            this.txtNewInfo.IsReplaceMode = false;
            this.txtNewInfo.LeftBracket = '(';
            this.txtNewInfo.LeftPadding = 15;
            this.txtNewInfo.Location = new System.Drawing.Point(3, 3);
            this.txtNewInfo.Name = "txtNewInfo";
            this.txtNewInfo.Paddings = new System.Windows.Forms.Padding(0);
            this.txtNewInfo.ReadOnly = true;
            this.txtNewInfo.RightBracket = ')';
            this.txtNewInfo.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtNewInfo.Size = new System.Drawing.Size(815, 407);
            this.txtNewInfo.TabIndex = 2;
            this.txtNewInfo.Zoom = 100;
            this.txtNewInfo.ToolTipNeeded += new System.EventHandler<FastColoredTextBoxNS.ToolTipNeededEventArgs>(this.txtNewInfo_ToolTipNeeded);
            this.txtNewInfo.PaintLine += new System.EventHandler<FastColoredTextBoxNS.PaintLineEventArgs>(this.txtNewInfo_PaintLine);
            this.txtNewInfo.PrepareForPaint += new System.EventHandler(this.txtNewInfo_PrepareForPaint);
            this.txtNewInfo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtNewInfo_MouseDown);
            this.txtNewInfo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtNewInfo_MouseUp);
            // 
            // resourcePane
            // 
            this.resourcePane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourcePane.Location = new System.Drawing.Point(0, 0);
            this.resourcePane.Name = "resourcePane";
            this.resourcePane.Size = new System.Drawing.Size(829, 439);
            this.resourcePane.TabIndex = 2;
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
            this.tabPage1.Controls.Add(this.txtNewInfo);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(821, 413);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "IL";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            this.diagram.AutoScrollMinSize = new System.Drawing.Size(-2147483648, -2147483648);
            this.diagram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diagram.GridSize = new System.Drawing.Size(8, 8);
            this.diagram.LineType = NodeControl.LineTypeEnum.Bezier;
            this.diagram.Location = new System.Drawing.Point(3, 3);
            this.diagram.Name = "diagram";
            this.diagram.NodeSize = new System.Drawing.Size(100, 50);
            this.diagram.ShowGrid = false;
            this.diagram.Size = new System.Drawing.Size(815, 407);
            this.diagram.TabIndex = 0;
            this.diagram.Zoom = 1F;
            // 
            // CodePane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabsCode);
            this.Controls.Add(this.resourcePane);
            this.Name = "CodePane";
            this.Size = new System.Drawing.Size(829, 439);
            ((System.ComponentModel.ISupportInitialize)(this.txtNewInfo)).EndInit();
            this.tabsCode.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip txtInfoTooltip;
        private Components.ResourcePane resourcePane;
        private System.Windows.Forms.TabControl tabsCode;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private ILNodeDiagram diagram;
        private CodeTextBox txtNewInfo;
    }
}
