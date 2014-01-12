namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class DebuggerVariables
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tvNodes = new RunTimeDebuggers.Controls.NavigatableTreeView();
            this.SuspendLayout();
            // 
            // tvNodes
            // 
            this.tvNodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvNodes.HideSelection = false;
            this.tvNodes.Location = new System.Drawing.Point(0, 0);
            this.tvNodes.Name = "tvNodes";
            this.tvNodes.ShowNodeToolTips = true;
            this.tvNodes.Size = new System.Drawing.Size(573, 221);
            this.tvNodes.TabIndex = 0;
              // 
            // CallStack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvNodes);
            this.Name = "CallStack";
            this.Size = new System.Drawing.Size(573, 221);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.NavigatableTreeView tvNodes;

    }
}
