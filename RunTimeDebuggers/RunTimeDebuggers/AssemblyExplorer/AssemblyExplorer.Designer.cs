namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class AssemblyExplorer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssemblyExplorer));
            this.imgs = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnBack = new System.Windows.Forms.ToolStripButton();
            this.btnForward = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.loadAssemblyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpResourcesOfSelectedAssemblyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpAssemblyWIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.loadAliasesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAliasesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.showCallStackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.findStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDecryptStrings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.aliasesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.debuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debuggerStackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.showOnlyAliasesWhenPresentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.showFormsFlowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuRunDebugger = new System.Windows.Forms.ToolStripButton();
            this.mnuStepInto = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.lblLoadingCache = new System.Windows.Forms.ToolStripLabel();
            this.pbar = new System.Windows.Forms.ToolStripProgressBar();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tvNodes = new RunTimeDebuggers.Controls.NavigatableTreeView();
            this.tpSearch = new System.Windows.Forms.TabPage();
            this.rightSplit = new System.Windows.Forms.SplitContainer();
            this.rightTabs = new RunTimeDebuggers.Controls.RemovableTabControl();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.rightSplit.Panel2.SuspendLayout();
            this.rightSplit.SuspendLayout();
            this.SuspendLayout();
            // 
            // imgs
            // 
            this.imgs.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgs.ImageSize = new System.Drawing.Size(16, 16);
            this.imgs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnBack,
            this.btnForward,
            this.toolStripSeparator1,
            this.toolStripDropDownButton2,
            this.toolStripDropDownButton1,
            this.toolStripSeparator2,
            this.mnuRunDebugger,
            this.mnuStepInto,
            this.toolStripSeparator3,
            this.lblLoadingCache,
            this.pbar});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(917, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnBack
            // 
            this.btnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBack.Image = global::RunTimeDebuggers.Properties.Resources.back;
            this.btnBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(23, 22);
            this.btnBack.Text = "Back";
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnForward
            // 
            this.btnForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnForward.Image = global::RunTimeDebuggers.Properties.Resources.forward;
            this.btnForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(23, 22);
            this.btnForward.Text = "Forward";
            this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadAssemblyToolStripMenuItem,
            this.dumpResourcesOfSelectedAssemblyToolStripMenuItem,
            this.dumpAssemblyWIPToolStripMenuItem,
            this.toolStripMenuItem3,
            this.loadAliasesToolStripMenuItem,
            this.saveAliasesToolStripMenuItem,
            this.autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem});
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(38, 22);
            this.toolStripDropDownButton2.Text = "&File";
            // 
            // loadAssemblyToolStripMenuItem
            // 
            this.loadAssemblyToolStripMenuItem.Name = "loadAssemblyToolStripMenuItem";
            this.loadAssemblyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.loadAssemblyToolStripMenuItem.Size = new System.Drawing.Size(317, 22);
            this.loadAssemblyToolStripMenuItem.Text = "Load assembly...";
            this.loadAssemblyToolStripMenuItem.Click += new System.EventHandler(this.loadAssemblyToolStripMenuItem_Click);
            // 
            // dumpResourcesOfSelectedAssemblyToolStripMenuItem
            // 
            this.dumpResourcesOfSelectedAssemblyToolStripMenuItem.Name = "dumpResourcesOfSelectedAssemblyToolStripMenuItem";
            this.dumpResourcesOfSelectedAssemblyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.dumpResourcesOfSelectedAssemblyToolStripMenuItem.Size = new System.Drawing.Size(317, 22);
            this.dumpResourcesOfSelectedAssemblyToolStripMenuItem.Text = "Dump resources of current assembly...";
            this.dumpResourcesOfSelectedAssemblyToolStripMenuItem.Click += new System.EventHandler(this.dumpResourcesOfSelectedAssemblyToolStripMenuItem_Click);
            // 
            // dumpAssemblyWIPToolStripMenuItem
            // 
            this.dumpAssemblyWIPToolStripMenuItem.Name = "dumpAssemblyWIPToolStripMenuItem";
            this.dumpAssemblyWIPToolStripMenuItem.Size = new System.Drawing.Size(317, 22);
            this.dumpAssemblyWIPToolStripMenuItem.Text = "Dump assembly (WIP)...";
            this.dumpAssemblyWIPToolStripMenuItem.Click += new System.EventHandler(this.dumpAssemblyWIPToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(314, 6);
            // 
            // loadAliasesToolStripMenuItem
            // 
            this.loadAliasesToolStripMenuItem.Name = "loadAliasesToolStripMenuItem";
            this.loadAliasesToolStripMenuItem.Size = new System.Drawing.Size(317, 22);
            this.loadAliasesToolStripMenuItem.Text = "Load aliases...";
            this.loadAliasesToolStripMenuItem.Click += new System.EventHandler(this.loadAliasesToolStripMenuItem_Click);
            // 
            // saveAliasesToolStripMenuItem
            // 
            this.saveAliasesToolStripMenuItem.Name = "saveAliasesToolStripMenuItem";
            this.saveAliasesToolStripMenuItem.Size = new System.Drawing.Size(317, 22);
            this.saveAliasesToolStripMenuItem.Text = "Save aliases...";
            this.saveAliasesToolStripMenuItem.Click += new System.EventHandler(this.saveAliasesToolStripMenuItem_Click);
            // 
            // autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem
            // 
            this.autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem.Name = "autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem";
            this.autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem.Size = new System.Drawing.Size(317, 22);
            this.autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem.Text = "Auto alias all members of current assembly";
            this.autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem.Click += new System.EventHandler(this.autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showCallStackToolStripMenuItem,
            this.toolStripMenuItem4,
            this.findStringToolStripMenuItem,
            this.mnuDecryptStrings,
            this.toolStripMenuItem2,
            this.aliasesToolStripMenuItem,
            this.bookmarksToolStripMenuItem,
            this.toolStripMenuItem1,
            this.debuggerToolStripMenuItem,
            this.toolStripMenuItem5,
            this.showOnlyAliasesWhenPresentToolStripMenuItem,
            this.toolStripMenuItem6,
            this.showFormsFlowToolStripMenuItem});
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButton1.Text = "View";
            // 
            // showCallStackToolStripMenuItem
            // 
            this.showCallStackToolStripMenuItem.Name = "showCallStackToolStripMenuItem";
            this.showCallStackToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.showCallStackToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.showCallStackToolStripMenuItem.Text = "Show call stack";
            this.showCallStackToolStripMenuItem.Click += new System.EventHandler(this.showCallStackToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(237, 6);
            // 
            // findStringToolStripMenuItem
            // 
            this.findStringToolStripMenuItem.Name = "findStringToolStripMenuItem";
            this.findStringToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.findStringToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.findStringToolStripMenuItem.Text = "Find string";
            this.findStringToolStripMenuItem.Click += new System.EventHandler(this.findStringToolStripMenuItem_Click);
            // 
            // mnuDecryptStrings
            // 
            this.mnuDecryptStrings.Name = "mnuDecryptStrings";
            this.mnuDecryptStrings.Size = new System.Drawing.Size(240, 22);
            this.mnuDecryptStrings.Text = "Decrypt strings";
            this.mnuDecryptStrings.Click += new System.EventHandler(this.mnuDecryptStrings_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(237, 6);
            // 
            // aliasesToolStripMenuItem
            // 
            this.aliasesToolStripMenuItem.Name = "aliasesToolStripMenuItem";
            this.aliasesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.aliasesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.aliasesToolStripMenuItem.Text = "Aliases";
            this.aliasesToolStripMenuItem.Click += new System.EventHandler(this.aliasesToolStripMenuItem_Click);
            // 
            // bookmarksToolStripMenuItem
            // 
            this.bookmarksToolStripMenuItem.Name = "bookmarksToolStripMenuItem";
            this.bookmarksToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.bookmarksToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.bookmarksToolStripMenuItem.Text = "Bookmarks";
            this.bookmarksToolStripMenuItem.Click += new System.EventHandler(this.bookmarksToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(237, 6);
            // 
            // debuggerToolStripMenuItem
            // 
            this.debuggerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debuggerStackToolStripMenuItem});
            this.debuggerToolStripMenuItem.Name = "debuggerToolStripMenuItem";
            this.debuggerToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.debuggerToolStripMenuItem.Text = "Debugger";
            // 
            // debuggerStackToolStripMenuItem
            // 
            this.debuggerStackToolStripMenuItem.Name = "debuggerStackToolStripMenuItem";
            this.debuggerStackToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.debuggerStackToolStripMenuItem.Text = "Debugger stack";
            this.debuggerStackToolStripMenuItem.Click += new System.EventHandler(this.debuggerStackToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(237, 6);
            // 
            // showOnlyAliasesWhenPresentToolStripMenuItem
            // 
            this.showOnlyAliasesWhenPresentToolStripMenuItem.Checked = true;
            this.showOnlyAliasesWhenPresentToolStripMenuItem.CheckOnClick = true;
            this.showOnlyAliasesWhenPresentToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showOnlyAliasesWhenPresentToolStripMenuItem.Name = "showOnlyAliasesWhenPresentToolStripMenuItem";
            this.showOnlyAliasesWhenPresentToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.showOnlyAliasesWhenPresentToolStripMenuItem.Text = "Show only aliases when present";
            this.showOnlyAliasesWhenPresentToolStripMenuItem.Click += new System.EventHandler(this.showOnlyAliasesWhenPresentToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(237, 6);
            // 
            // showFormsFlowToolStripMenuItem
            // 
            this.showFormsFlowToolStripMenuItem.Name = "showFormsFlowToolStripMenuItem";
            this.showFormsFlowToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.showFormsFlowToolStripMenuItem.Text = "Show forms flow";
            this.showFormsFlowToolStripMenuItem.Click += new System.EventHandler(this.showFormsFlowToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // mnuRunDebugger
            // 
            this.mnuRunDebugger.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnuRunDebugger.ImageTransparentColor = System.Drawing.Color.White;
            this.mnuRunDebugger.Name = "mnuRunDebugger";
            this.mnuRunDebugger.Image = global::RunTimeDebuggers.Properties.Resources.debugger;
            this.mnuRunDebugger.Size = new System.Drawing.Size(23, 22);
            this.mnuRunDebugger.Text = "Run interpreter (WIP)";
            this.mnuRunDebugger.Click += new System.EventHandler(this.mnuRunDebugger_Click);
            // 
            // mnuStepInto
            // 
            this.mnuStepInto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnuStepInto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuStepInto.Name = "mnuStepInto";
            this.mnuStepInto.Image = global::RunTimeDebuggers.Properties.Resources.stepInto;
            this.mnuStepInto.Size = new System.Drawing.Size(23, 22);
            this.mnuStepInto.Text = "Step into next statement (WIP)";
            this.mnuStepInto.Click += new System.EventHandler(this.mnuStepInto_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // lblLoadingCache
            // 
            this.lblLoadingCache.Name = "lblLoadingCache";
            this.lblLoadingCache.Size = new System.Drawing.Size(129, 22);
            this.lblLoadingCache.Text = "Building analysis cache";
            // 
            // pbar
            // 
            this.pbar.Name = "pbar";
            this.pbar.Size = new System.Drawing.Size(100, 22);
            this.pbar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.statusStrip1, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.toolStrip1);
            this.tableLayoutPanel2.Controls.Add(this.splitContainer1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(917, 422);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 400);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(917, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 28);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabs);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rightSplit);
            this.splitContainer1.Size = new System.Drawing.Size(911, 369);
            this.splitContainer1.SplitterDistance = 301;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabPage1);
            this.tabs.Controls.Add(this.tpSearch);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(301, 369);
            this.tabs.TabIndex = 0;
            this.tabs.SelectedIndexChanged += new System.EventHandler(this.tabs_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tvNodes);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(293, 343);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "All";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tvNodes
            // 
            this.tvNodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvNodes.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvNodes.HideSelection = false;
            this.tvNodes.ImageIndex = 0;
            this.tvNodes.ImageList = this.imgs;
            this.tvNodes.Location = new System.Drawing.Point(3, 3);
            this.tvNodes.Name = "tvNodes";
            this.tvNodes.SelectedImageIndex = 0;
            this.tvNodes.ShowNodeToolTips = true;
            this.tvNodes.Size = new System.Drawing.Size(287, 337);
            this.tvNodes.TabIndex = 0;
            this.tvNodes.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvNodes_BeforeExpand);
            this.tvNodes.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvNodes_BeforeSelect);
            this.tvNodes.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvNodes_AfterSelect);
            this.tvNodes.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.Treeview_NodeMouseClick);
            this.tvNodes.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tvNodes_MouseMove);
            // 
            // tpSearch
            // 
            this.tpSearch.Location = new System.Drawing.Point(4, 22);
            this.tpSearch.Name = "tpSearch";
            this.tpSearch.Padding = new System.Windows.Forms.Padding(3);
            this.tpSearch.Size = new System.Drawing.Size(293, 343);
            this.tpSearch.TabIndex = 1;
            this.tpSearch.Text = "Search";
            this.tpSearch.UseVisualStyleBackColor = true;
            // 
            // rightSplit
            // 
            this.rightSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightSplit.Location = new System.Drawing.Point(0, 0);
            this.rightSplit.Name = "rightSplit";
            this.rightSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // rightSplit.Panel2
            // 
            this.rightSplit.Panel2.Controls.Add(this.rightTabs);
            this.rightSplit.Panel2Collapsed = true;
            this.rightSplit.Size = new System.Drawing.Size(606, 369);
            this.rightSplit.SplitterDistance = 217;
            this.rightSplit.TabIndex = 1;
            // 
            // rightTabs
            // 
            this.rightTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightTabs.Location = new System.Drawing.Point(0, 0);
            this.rightTabs.Name = "rightTabs";
            this.rightTabs.Size = new System.Drawing.Size(150, 46);
            this.rightTabs.TabIndex = 0;
            this.rightTabs.TabRemoved += new System.EventHandler(this.rightTabs_TabRemoved);
            this.rightTabs.TabAdded += new System.EventHandler(this.rightTabs_TabAdded);
            // 
            // AssemblyExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 422);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AssemblyExplorer";
            this.Text = "Assembly Explorer";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tabs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.rightSplit.Panel2.ResumeLayout(false);
            this.rightSplit.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private RunTimeDebuggers.Controls.NavigatableTreeView tvNodes;
        private System.Windows.Forms.ImageList imgs;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnBack;
        private System.Windows.Forms.ToolStripButton btnForward;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tpSearch;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.SplitContainer rightSplit;
        private RunTimeDebuggers.Controls.RemovableTabControl rightTabs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel lblLoadingCache;
        private System.Windows.Forms.ToolStripProgressBar pbar;
        private System.Windows.Forms.ToolStripMenuItem findStringToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem aliasesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bookmarksToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripMenuItem loadAssemblyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpResourcesOfSelectedAssemblyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpAssemblyWIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuDecryptStrings;
        private System.Windows.Forms.ToolStripButton mnuRunDebugger;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton mnuStepInto;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem debuggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debuggerStackToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem loadAliasesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAliasesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCallStackToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem showOnlyAliasesWhenPresentToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem showFormsFlowToolStripMenuItem;
    }
}