namespace RunTimeDebuggers.LocalsDebugger
{
    partial class ExpressionBox
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
            this.txtEvaluate = new System.Windows.Forms.RichTextBox();
            this.methodsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // txtEvaluate
            // 
            this.txtEvaluate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEvaluate.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEvaluate.Location = new System.Drawing.Point(0, 0);
            this.txtEvaluate.Multiline = false;
            this.txtEvaluate.Name = "txtEvaluate";
            this.txtEvaluate.Size = new System.Drawing.Size(576, 130);
            this.txtEvaluate.TabIndex = 1;
            this.txtEvaluate.Text = "";
            this.txtEvaluate.TextChanged += new System.EventHandler(this.txtEvaluate_TextChanged);
            this.txtEvaluate.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtEvaluate_KeyDown);
            this.txtEvaluate.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtEvaluate_KeyPress);
            // 
            // ExpressionBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtEvaluate);
            this.Name = "ExpressionBox";
            this.Size = new System.Drawing.Size(576, 130);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtEvaluate;
        private System.Windows.Forms.ToolTip methodsToolTip;
    }
}
