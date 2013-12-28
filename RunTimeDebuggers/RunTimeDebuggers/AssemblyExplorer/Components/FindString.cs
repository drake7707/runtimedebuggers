using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;
using System.Reflection;

namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class FindString : TreeNodeControl
    {
        public FindString(IAssemblyBrowser browser)
            : base(browser)
        {
            this.browser = browser;
     
            InitializeComponent();

            Initialize(tvSearchResults);
        }

   

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            tmrFilter.Enabled = false;
            tmrFilter.Enabled = true;
        }

        private void Filter()
        {
            tvSearchResults.Nodes.Clear();

            if (string.IsNullOrEmpty(txtFilter.Text))
                return;

            // check for too many results (which would induce serious gui lag)
            if (txtFilter.Text.Length < 3)
            {
                tvSearchResults.Nodes.Add("Filter too short");
                return;
            }

            pbar.Visible = true;

            var ui = WindowsFormsSynchronizationContext.Current;

            string filterstring = txtFilter.Text.ToLower();
            TaskFactory f = new TaskFactory(1);
            f.StartTask(() => BuildNodes(ui, filterstring), () =>
            {
                ui.Send(o =>
                   {
                       ((FindString)o).pbar.Visible = false;
                       ((FindString)o).tvSearchResults.ExpandAll();
                   }, this);
            });

        }

        private void BuildNodes(System.Threading.SynchronizationContext ui, string filterstring)
        {
            foreach (var mc in AnalysisManager.Instance.MemberCaches.Where(mc => mc is MethodBaseCache))
            {

                MemberNode n = null;
                foreach (string s in ((MethodBaseCache)mc).UsedStrings)
                {

                    if (s != null && s.ToLower().Contains(filterstring))
                    {
                        if (n == null)
                            n = MemberNode.GetNodeOfMember(mc.Member, true);

                        n.Nodes.Add(s);
                    }

                }

                if (n != null)
                {
                    ui.Send(o =>
                    {
                        ((FindString)o).tvSearchResults.Nodes.Add(n);
                    }, this);
                }
            }
        }

        private void tmrFilter_Tick(object sender, EventArgs e)
        {
            tmrFilter.Enabled = false;
            Filter();
        }
    }
}
