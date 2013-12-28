using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RunTimeDebuggers.Controls
{
    public partial class RemovableTabControl : UserControl
    {
        private TabControl tabs;
        private Button btnRemove;

        public RemovableTabControl()
        {
            InitializeComponent();
            tabs.Multiline = true;
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public event EventHandler TabRemoved;
        public event EventHandler TabAdded;

        private void InitializeComponent()
        {
            this.tabs = new System.Windows.Forms.TabControl();
            this.btnRemove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(452, 213);
            this.tabs.TabIndex = 0;
            this.tabs.SelectedIndexChanged += new System.EventHandler(this.tabs_SelectedIndexChanged);
            this.tabs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabs_MouseUp);
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.Image = global::RunTimeDebuggers.Properties.Resources.deletesmall;
            this.btnRemove.Location = new System.Drawing.Point(428, 1);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(22, 17);
            this.btnRemove.TabIndex = 1;
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // RemovableTabControl
            // 
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.tabs);
            this.Name = "RemovableTabControl";
            this.Size = new System.Drawing.Size(452, 213);
            this.ResumeLayout(false);

        }

        private void tabs_MouseUp(object sender, MouseEventArgs e)
        {
            // check if the right mouse button was pressed
            if (e.Button == MouseButtons.Right)
            {
                // iterate through all the tab pages
                for (int i = 0; i < tabs.TabCount; i++)
                {
                    // get their rectangle area and check if it contains the mouse cursor
                    Rectangle r = tabs.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        RemoveTab(i);
                        return;
                    }
                }
            }

        }


        private void RemoveTab(int i)
        {
            tabs.TabPages.RemoveAt(i);

            EventHandler temp = TabRemoved;
            if (temp != null)
                temp(this, EventArgs.Empty);

        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (tabs.TabCount > 0)
            {
                TabPage p = tabs.TabPages[tabs.SelectedIndex];
                RemoveTab(tabs.SelectedIndex);
                p.Dispose();
            }
        }

        public int TabCount { get { return tabs.TabCount; } }

        public void AddTab(Control c, string tabText)
        {
            TabPage p = new TabPage(tabText);
            c.Dock = DockStyle.Fill;
            p.Controls.Add(c);
            tabs.TabPages.Add(p);

            tabs.SelectedTab = p;

            EventHandler temp = TabAdded;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        private void tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabs.SelectedIndex >= 0 && tabs.TabPages[tabs.SelectedIndex].Controls.Count > 0)
                tabs.TabPages[tabs.SelectedIndex].Controls[0].Focus();
        }
    }
}
