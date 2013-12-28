namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class CallGraph
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CallGraph));
            this.graph = new RunTimeDebuggers.Controls.GraphExplorer();
            this.imgs = new System.Windows.Forms.ImageList(this.components);
            this.chkOnlySameAssembly = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // graph
            // 
            this.graph.BackColor = System.Drawing.Color.White;
            this.graph.DefaultAssociationStyle = RunTimeDebuggers.Controls.GraphExplorer.DefaultAssociationStyleEnum.HVH;
            this.graph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graph.FocusOnSelectedNode = true;
            this.graph.Location = new System.Drawing.Point(0, 0);
            this.graph.Name = "graph";
            this.graph.NodeAssociationPaintHandler = null;
            this.graph.NodePadding = 20;
            this.graph.NodePaintHandler = null;
            this.graph.NodeSize = new System.Drawing.Size(200, 50);
            this.graph.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.graph.RootNode = null;
            this.graph.SelectedNode = null;
            this.graph.Size = new System.Drawing.Size(776, 432);
            this.graph.TabIndex = 0;
            this.graph.SelectionChanged += new System.EventHandler(this.graph_SelectionChanged);
            this.graph.NodeDoubleClick += new RunTimeDebuggers.Controls.GraphExplorer.NodeDoubleClickHandler(this.graph_NodeDoubleClick);
            // 
            // imgs
            // 
            this.imgs.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgs.ImageSize = new System.Drawing.Size(16, 16);
            this.imgs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // chkOnlySameAssembly
            // 
            this.chkOnlySameAssembly.AutoSize = true;
            this.chkOnlySameAssembly.BackColor = System.Drawing.Color.White;
            this.chkOnlySameAssembly.Location = new System.Drawing.Point(0, 2);
            this.chkOnlySameAssembly.Name = "chkOnlySameAssembly";
            this.chkOnlySameAssembly.Size = new System.Drawing.Size(262, 17);
            this.chkOnlySameAssembly.TabIndex = 1;
            this.chkOnlySameAssembly.Text = "Show only with nodes same assembly as rootnode";
            this.chkOnlySameAssembly.UseVisualStyleBackColor = false;
            this.chkOnlySameAssembly.CheckedChanged += new System.EventHandler(this.chkOnlySameAssembly_CheckedChanged);
            // 
            // CallGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 432);
            this.Controls.Add(this.chkOnlySameAssembly);
            this.Controls.Add(this.graph);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CallGraph";
            this.Text = "Call Graph";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.GraphExplorer graph;
        private System.Windows.Forms.ImageList imgs;
        private System.Windows.Forms.CheckBox chkOnlySameAssembly;
    }
}