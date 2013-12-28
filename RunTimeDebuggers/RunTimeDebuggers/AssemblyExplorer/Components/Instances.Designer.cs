namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class Instances
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tvSearchResults = new System.Windows.Forms.TreeView();
            this.pbar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tmrUpdateStatus = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tvSearchResults, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.pbar, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblStatus, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(573, 221);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Image = global::RunTimeDebuggers.Properties.Resources.deletesmall;
            this.btnCancel.Location = new System.Drawing.Point(551, 0);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(22, 17);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tvSearchResults
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tvSearchResults, 2);
            this.tvSearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvSearchResults.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvSearchResults.Location = new System.Drawing.Point(3, 20);
            this.tvSearchResults.Name = "tvSearchResults";
            this.tvSearchResults.Size = new System.Drawing.Size(567, 178);
            this.tvSearchResults.TabIndex = 2;
            // 
            // pbar
            // 
            this.pbar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pbar.Location = new System.Drawing.Point(3, 3);
            this.pbar.Name = "pbar";
            this.pbar.Size = new System.Drawing.Size(545, 10);
            this.pbar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pbar.TabIndex = 3;
            this.pbar.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lblStatus, 2);
            this.lblStatus.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(3, 202);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.lblStatus.Size = new System.Drawing.Size(567, 18);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Scanning in ....";
            // 
            // tmrUpdateStatus
            // 
            this.tmrUpdateStatus.Tick += new System.EventHandler(this.tmrUpdateStatus_Tick);
            // 
            // Instances
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Instances";
            this.Size = new System.Drawing.Size(573, 221);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TreeView tvSearchResults;
        private System.Windows.Forms.ProgressBar pbar;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer tmrUpdateStatus;


    }
}
