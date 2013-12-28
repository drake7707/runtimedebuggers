namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class Analysis
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
            this.imgs = new System.Windows.Forms.ImageList(this.components);
            this.tvNodes = new RunTimeDebuggers.Controls.NavigatableTreeView();
            this.SuspendLayout();
            // 
            // imgs
            // 
            this.imgs.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgs.ImageSize = new System.Drawing.Size(16, 16);
            this.imgs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // tvNodes
            // 
            this.tvNodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvNodes.HideSelection = false;
            this.tvNodes.ImageIndex = 0;
            this.tvNodes.ImageList = this.imgs;
            this.tvNodes.Location = new System.Drawing.Point(0, 0);
            this.tvNodes.Name = "tvNodes";
            this.tvNodes.SelectedImageIndex = 0;
            this.tvNodes.ShowNodeToolTips = true;
            this.tvNodes.Size = new System.Drawing.Size(624, 266);
            this.tvNodes.TabIndex = 1;
            // 
            // Analysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvNodes);
            this.Name = "Analysis";
            this.Size = new System.Drawing.Size(624, 266);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imgs;
        protected Controls.NavigatableTreeView tvNodes;
    }
}
