
using RunTimeDebuggers.Properties;
using RunTimeDebuggers.Controls;
namespace RunTimeDebuggers.LocalsDebugger
{
    partial class LocalsWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

     
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocalsWindow));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnShowControls = new System.Windows.Forms.ToolStripButton();
            this.btnShowFields = new System.Windows.Forms.ToolStripButton();
            this.btnShowProperties = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnEvaluate = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTogglePropertyGrid = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitVert = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tree = new RunTimeDebuggers.Controls.TreeListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tvControlTree = new RunTimeDebuggers.Controls.NavigatableTreeView();
            this.mnuLookupInLocals = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnl = new System.Windows.Forms.Panel();
            this.propGrid = new System.Windows.Forms.PropertyGrid();
            this.tmr = new System.Windows.Forms.Timer(this.components);
            this.imgs = new System.Windows.Forms.ImageList(this.components);
            this.methodsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.mnuLookupInAssemblyExplorer = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openValueInNewLocalsWatchesWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.openMemberInAssemblyExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openReturnTypeInAssemblyExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openValueAsMemberInAssemblyExploreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.changeAliasOfMemberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tmrMouseOver = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            
            this.splitVert.Panel1.SuspendLayout();
            this.splitVert.Panel2.SuspendLayout();
            this.splitVert.SuspendLayout();
            
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            
            this.tabPage2.SuspendLayout();
            this.mnuLookupInLocals.SuspendLayout();
            this.mnuLookupInAssemblyExplorer.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnShowControls,
            this.btnShowFields,
            this.btnShowProperties,
            this.toolStripSeparator1,
            this.btnEvaluate,
            this.toolStripSeparator2,
            this.btnTogglePropertyGrid});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(710, 25);
            this.toolStrip1.TabIndex = 1;
            // 
            // btnShowControls
            // 
            this.btnShowControls.Checked = true;
            this.btnShowControls.CheckOnClick = true;
            this.btnShowControls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnShowControls.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowControls.Image = global::RunTimeDebuggers.Properties.Resources.toolbox;
            this.btnShowControls.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowControls.Name = "btnShowControls";
            this.btnShowControls.Size = new System.Drawing.Size(23, 22);
            this.btnShowControls.Text = "Show controls";
            this.btnShowControls.Click += new System.EventHandler(this.btnShowControls_Click);
            // 
            // btnShowFields
            // 
            this.btnShowFields.Checked = true;
            this.btnShowFields.CheckOnClick = true;
            this.btnShowFields.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnShowFields.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowFields.Image = global::RunTimeDebuggers.Properties.Resources.Field;
            this.btnShowFields.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowFields.Name = "btnShowFields";
            this.btnShowFields.Size = new System.Drawing.Size(23, 22);
            this.btnShowFields.Text = "Show fields";
            this.btnShowFields.Click += new System.EventHandler(this.btnShowFields_Click);
            // 
            // btnShowProperties
            // 
            this.btnShowProperties.Checked = true;
            this.btnShowProperties.CheckOnClick = true;
            this.btnShowProperties.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnShowProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowProperties.Image = global::RunTimeDebuggers.Properties.Resources.Property;
            this.btnShowProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowProperties.Name = "btnShowProperties";
            this.btnShowProperties.Size = new System.Drawing.Size(23, 22);
            this.btnShowProperties.Text = "Show properties";
            this.btnShowProperties.Click += new System.EventHandler(this.btnShowProperties_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnEvaluate
            // 
            this.btnEvaluate.Checked = true;
            this.btnEvaluate.CheckOnClick = true;
            this.btnEvaluate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnEvaluate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEvaluate.Image = ((System.Drawing.Image)(resources.GetObject("btnEvaluate.Image")));
            this.btnEvaluate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEvaluate.Name = "btnEvaluate";
            this.btnEvaluate.Size = new System.Drawing.Size(23, 22);
            this.btnEvaluate.Text = "Evaluate";
            this.btnEvaluate.ToolTipText = "Evaluate each second";
            this.btnEvaluate.Click += new System.EventHandler(this.btnEvaluate_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnTogglePropertyGrid
            // 
            this.btnTogglePropertyGrid.CheckOnClick = true;
            this.btnTogglePropertyGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnTogglePropertyGrid.Image = global::RunTimeDebuggers.Properties.Resources.propgrid;
            this.btnTogglePropertyGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTogglePropertyGrid.Name = "btnTogglePropertyGrid";
            this.btnTogglePropertyGrid.Size = new System.Drawing.Size(23, 22);
            this.btnTogglePropertyGrid.Text = "Toggle property grid";
            this.btnTogglePropertyGrid.Click += new System.EventHandler(this.btnTogglePropertyGrid_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.splitVert, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(710, 290);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // splitVert
            // 
            this.splitVert.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitVert.Location = new System.Drawing.Point(3, 28);
            this.splitVert.Name = "splitVert";
            // 
            // splitVert.Panel1
            // 
            this.splitVert.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitVert.Panel2
            // 
            this.splitVert.Panel2.Controls.Add(this.propGrid);
            this.splitVert.Panel2Collapsed = true;
            this.splitVert.Size = new System.Drawing.Size(704, 259);
            this.splitVert.SplitterDistance = 444;
            this.splitVert.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pnl);
            this.splitContainer1.Size = new System.Drawing.Size(704, 259);
            this.splitContainer1.SplitterDistance = 204;
            this.splitContainer1.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(704, 204);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tree);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(696, 178);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Locals";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tree
            // 
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.Images = null;
            this.tree.Location = new System.Drawing.Point(0, 0);
            this.tree.Name = "tree";
            this.tree.Size = new System.Drawing.Size(696, 178);
            this.tree.TabIndex = 0;
            this.tree.Text = "treeListView1";
            this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_AfterSelect);
            this.tree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tree_KeyDown);
            this.tree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tree_MouseUp);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tvControlTree);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(696, 178);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Controls";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tvControlTree
            // 
            this.tvControlTree.ContextMenuStrip = this.mnuLookupInLocals;
            this.tvControlTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvControlTree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.tvControlTree.HideSelection = false;
            this.tvControlTree.Location = new System.Drawing.Point(0, 0);
            this.tvControlTree.Name = "tvControlTree";
            this.tvControlTree.ShowNodeToolTips = true;
            this.tvControlTree.Size = new System.Drawing.Size(696, 178);
            this.tvControlTree.TabIndex = 0;
            this.tvControlTree.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.tvControlTree_DrawNode);
            this.tvControlTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvControlTree_AfterSelect);
            this.tvControlTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvControlTree_MouseDown);
            // 
            // mnuLookupInLocals
            // 
            this.mnuLookupInLocals.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem});
            this.mnuLookupInLocals.Name = "mnuLookupInLocals";
            this.mnuLookupInLocals.Size = new System.Drawing.Size(310, 26);
            // 
            // openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem
            // 
            this.openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem.Name = "openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem";
            this.openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem.Size = new System.Drawing.Size(309, 22);
            this.openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem.Text = "Open value in new locals && watches window";
            this.openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem.Click += new System.EventHandler(this.openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem_Click);
            // 
            // pnl
            // 
            this.pnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl.Location = new System.Drawing.Point(0, 0);
            this.pnl.Margin = new System.Windows.Forms.Padding(0);
            this.pnl.Name = "pnl";
            this.pnl.Size = new System.Drawing.Size(704, 51);
            this.pnl.TabIndex = 0;
            // 
            // propGrid
            // 
            this.propGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propGrid.Location = new System.Drawing.Point(0, 0);
            this.propGrid.Name = "propGrid";
            this.propGrid.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propGrid.Size = new System.Drawing.Size(96, 100);
            this.propGrid.TabIndex = 0;
            this.propGrid.SelectedObjectsChanged += new System.EventHandler(this.propGrid_SelectedObjectsChanged);
            // 
            // tmr
            // 
            this.tmr.Enabled = true;
            this.tmr.Interval = 1000;
            this.tmr.Tick += new System.EventHandler(this.tmr_Tick);
            // 
            // imgs
            // 
            this.imgs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imgs.ImageSize = new System.Drawing.Size(16, 16);
            this.imgs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // mnuLookupInAssemblyExplorer
            // 
            this.mnuLookupInAssemblyExplorer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openValueInNewLocalsWatchesWindowToolStripMenuItem,
            this.toolStripMenuItem2,
            this.openMemberInAssemblyExplorerToolStripMenuItem,
            this.openReturnTypeInAssemblyExplorerToolStripMenuItem,
            this.openValueAsMemberInAssemblyExploreToolStripMenuItem,
            this.toolStripMenuItem1,
            this.changeAliasOfMemberToolStripMenuItem});
            this.mnuLookupInAssemblyExplorer.Name = "mnuLookupInAssemblyExplorer";
            this.mnuLookupInAssemblyExplorer.Size = new System.Drawing.Size(310, 126);
            // 
            // openValueInNewLocalsWatchesWindowToolStripMenuItem
            // 
            this.openValueInNewLocalsWatchesWindowToolStripMenuItem.Name = "openValueInNewLocalsWatchesWindowToolStripMenuItem";
            this.openValueInNewLocalsWatchesWindowToolStripMenuItem.Size = new System.Drawing.Size(309, 22);
            this.openValueInNewLocalsWatchesWindowToolStripMenuItem.Text = "Open value in new locals && watches window";
            this.openValueInNewLocalsWatchesWindowToolStripMenuItem.Click += new System.EventHandler(this.openValueInNewLocalsWatchesWindowToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(306, 6);
            // 
            // openMemberInAssemblyExplorerToolStripMenuItem
            // 
            this.openMemberInAssemblyExplorerToolStripMenuItem.Name = "openMemberInAssemblyExplorerToolStripMenuItem";
            this.openMemberInAssemblyExplorerToolStripMenuItem.Size = new System.Drawing.Size(309, 22);
            this.openMemberInAssemblyExplorerToolStripMenuItem.Text = "Open member in assembly explorer";
            this.openMemberInAssemblyExplorerToolStripMenuItem.Click += new System.EventHandler(this.openMemberInAssemblyExplorerToolStripMenuItem_Click);
            // 
            // openReturnTypeInAssemblyExplorerToolStripMenuItem
            // 
            this.openReturnTypeInAssemblyExplorerToolStripMenuItem.Name = "openReturnTypeInAssemblyExplorerToolStripMenuItem";
            this.openReturnTypeInAssemblyExplorerToolStripMenuItem.Size = new System.Drawing.Size(309, 22);
            this.openReturnTypeInAssemblyExplorerToolStripMenuItem.Text = "Open return type in assembly explorer";
            this.openReturnTypeInAssemblyExplorerToolStripMenuItem.Click += new System.EventHandler(this.openReturnTypeInAssemblyExplorerToolStripMenuItem_Click);
            // 
            // openValueAsMemberInAssemblyExploreToolStripMenuItem
            // 
            this.openValueAsMemberInAssemblyExploreToolStripMenuItem.Name = "openValueAsMemberInAssemblyExploreToolStripMenuItem";
            this.openValueAsMemberInAssemblyExploreToolStripMenuItem.Size = new System.Drawing.Size(309, 22);
            this.openValueAsMemberInAssemblyExploreToolStripMenuItem.Text = "Open value as member in assembly explorer";
            this.openValueAsMemberInAssemblyExploreToolStripMenuItem.Click += new System.EventHandler(this.openValueAsMemberInAssemblyExploreToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(306, 6);
            // 
            // changeAliasOfMemberToolStripMenuItem
            // 
            this.changeAliasOfMemberToolStripMenuItem.Name = "changeAliasOfMemberToolStripMenuItem";
            this.changeAliasOfMemberToolStripMenuItem.Size = new System.Drawing.Size(309, 22);
            this.changeAliasOfMemberToolStripMenuItem.Text = "Change alias of member";
            this.changeAliasOfMemberToolStripMenuItem.Click += new System.EventHandler(this.changeAliasOfMemberToolStripMenuItem_Click);
            // 
            // tmrMouseOver
            // 
            this.tmrMouseOver.Enabled = true;
            this.tmrMouseOver.Tick += new System.EventHandler(this.tmrMouseOver_Tick);
            // 
            // LocalsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(710, 290);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LocalsWindow";
            this.Text = "Locals & Watches";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitVert.Panel1.ResumeLayout(false);
            this.splitVert.Panel2.ResumeLayout(false);
            
            this.splitVert.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            
            this.tabPage2.ResumeLayout(false);
            this.mnuLookupInLocals.ResumeLayout(false);
            this.mnuLookupInAssemblyExplorer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tree;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnShowControls;
        private System.Windows.Forms.ToolStripButton btnShowFields;
        private System.Windows.Forms.ToolStripButton btnShowProperties;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer tmr;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ImageList imgs;
        private System.Windows.Forms.ToolTip methodsToolTip;
        private System.Windows.Forms.ContextMenuStrip mnuLookupInAssemblyExplorer;
        private System.Windows.Forms.ToolStripMenuItem openMemberInAssemblyExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openReturnTypeInAssemblyExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openValueAsMemberInAssemblyExploreToolStripMenuItem;
        private System.Windows.Forms.Panel pnl;
        private System.Windows.Forms.ToolStripButton btnEvaluate;
        private System.Windows.Forms.ToolStripMenuItem openValueInNewLocalsWatchesWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem changeAliasOfMemberToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.SplitContainer splitVert;
        private System.Windows.Forms.PropertyGrid propGrid;
        private System.Windows.Forms.ToolStripButton btnTogglePropertyGrid;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private NavigatableTreeView tvControlTree;
        private System.Windows.Forms.ContextMenuStrip mnuLookupInLocals;
        private System.Windows.Forms.ToolStripMenuItem openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem;
        private System.Windows.Forms.Timer tmrMouseOver;
    }
}