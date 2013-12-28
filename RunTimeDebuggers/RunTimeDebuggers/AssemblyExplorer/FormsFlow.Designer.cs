namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class FormsFlow
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
            this.pnlLoading = new System.Windows.Forms.Panel();
            this.pb = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlLoading.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLoading
            // 
            this.pnlLoading.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlLoading.Controls.Add(this.lblStatus);
            this.pnlLoading.Controls.Add(this.pb);
            this.pnlLoading.Location = new System.Drawing.Point(145, 192);
            this.pnlLoading.Name = "pnlLoading";
            this.pnlLoading.Size = new System.Drawing.Size(393, 123);
            this.pnlLoading.TabIndex = 0;
            // 
            // pb
            // 
            this.pb.Location = new System.Drawing.Point(17, 15);
            this.pb.Name = "pb";
            this.pb.Size = new System.Drawing.Size(362, 16);
            this.pb.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pb.TabIndex = 0;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(17, 38);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(362, 72);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "label1";
            // 
            // FormsFlow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 542);
            this.Controls.Add(this.pnlLoading);
            this.Name = "FormsFlow";
            this.Text = "Forms Flow";
            this.pnlLoading.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLoading;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar pb;
    }
}