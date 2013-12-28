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
    partial class SearchTypeOrMember : TreeNodeControl
    {
        public SearchTypeOrMember(IAssemblyBrowser browser)
            : base(browser)
        {
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

            TaskFactory f = new TaskFactory(1);
            f.StartTask(() => BuildNodes(ui), () => 
            {
                 ui.Send(o =>
                    {
                        ((SearchTypeOrMember)o).pbar.Visible = false;
                    }, this);
            });

        }

        private void BuildNodes(System.Threading.SynchronizationContext ui)
        {
            TaskFactory factory = new TaskFactory(4);
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies().Where(a => !(a is System.Reflection.Emit.AssemblyBuilder)))
            {
                factory.StartTask<Assembly, AssemblyNode>(ass =>
                {
                    AssemblyNode an = new AssemblyNode(ass);
                    an.PopulateAll(txtFilter.Text.ToLower());
                    an.ExpandAll();
                    if (an.Nodes.Count > 0)
                        return an;
                    else
                        return null;
                }, a, an =>
                {
                    if (an != null)
                    {
                        ui.Send(o =>
                        {
                            ((SearchTypeOrMember)o).tvSearchResults.Nodes.Add(an);
                        }, this);
                    }
                });
            }
            factory.WaitAll();
        }

        private void tmrFilter_Tick(object sender, EventArgs e)
        {
            tmrFilter.Enabled = false;
            Filter();
        }

    }
}
