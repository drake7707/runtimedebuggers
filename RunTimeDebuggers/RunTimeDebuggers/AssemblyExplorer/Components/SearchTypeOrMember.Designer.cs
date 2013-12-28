namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class SearchTypeOrMember
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
            this.tvSearchResults = new System.Windows.Forms.TreeView();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.pbar = new System.Windows.Forms.ProgressBar();
            this.tmrFilter = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tvSearchResults, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtFilter, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pbar, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(534, 335);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tvSearchResults
            // 
            this.tvSearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvSearchResults.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvSearchResults.Location = new System.Drawing.Point(3, 42);
            this.tvSearchResults.Name = "tvSearchResults";
            this.tvSearchResults.Size = new System.Drawing.Size(528, 290);
            this.tvSearchResults.TabIndex = 2;
            // 
            // txtFilter
            // 
            this.txtFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFilter.Location = new System.Drawing.Point(3, 3);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(528, 20);
            this.txtFilter.TabIndex = 1;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            // 
            // pbar
            // 
            this.pbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbar.Location = new System.Drawing.Point(3, 26);
            this.pbar.Name = "pbar";
            this.pbar.Size = new System.Drawing.Size(528, 10);
            this.pbar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pbar.TabIndex = 3;
            this.pbar.Visible = false;
            // 
            // tmrFilter
            // 
            this.tmrFilter.Interval = 500;
            this.tmrFilter.Tick += new System.EventHandler(this.tmrFilter_Tick);
            // 
            // SearchTypeOrMember
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SearchTypeOrMember";
            this.Size = new System.Drawing.Size(534, 335);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TreeView tvSearchResults;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Timer tmrFilter;
        private System.Windows.Forms.ProgressBar pbar;
    }
}
