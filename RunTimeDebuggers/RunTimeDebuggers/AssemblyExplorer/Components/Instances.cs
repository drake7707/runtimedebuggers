using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class Instances : TreeNodeControl
    {

        private Type type;

        public Instances(IAssemblyBrowser browser, Type t)
            : base(browser)
        {
            this.type = t;
            InitializeComponent();

            Initialize(tvSearchResults);

            Fill();
        }


        private InstanceFinderManager findManager;



        public void Fill()
        {
            findManager = new InstanceFinderManager(type);
            pbar.Style = ProgressBarStyle.Marquee;
            pbar.Visible = true;
            tmrUpdateStatus.Enabled = true;
            lblStatus.Visible = true;
            var ui = WindowsFormsSynchronizationContext.Current;

            findManager.InstanceFound += result =>
            {
                ui.Send(o =>
                {
                    if (!((Instances)o).IsDisposed)
                        tvSearchResults.Nodes.Add(new InstanceNode(result));
                }, this);
            };

            TaskFactory f = new TaskFactory(1);
            f.StartTask(() => findManager.Find(10), () =>
            {
                ui.Send(o =>
                {
                    if (!((Instances)o).IsDisposed)
                    {
                        ((Instances)o).btnCancel.Visible = false;
                        ((Instances)o).pbar.Visible = false;
                        ((Instances)o).tmrUpdateStatus.Enabled = false;
                        ((Instances)o).lblStatus.Visible = false;
                    }
                }, this);
            });
        }


        private void AddNode(object obj, string alias)
        {
            if (obj is Type)
            {
                var tn = new TypeNode((Type)obj);
                tn.Nodes.Clear();
                tvSearchResults.Nodes.Add(tn);
            }
            else if (obj is MemberInfo)
                tvSearchResults.Nodes.Add(MemberNode.GetNodeOfMember((MemberInfo)obj, true));
        }

        protected override void AliasManager_AliasChanged(object obj, string alias)
        {

            bool hasNode = false;
            foreach (TreeNode n in tvSearchResults.Nodes)
            {
                if ((obj is Type && n is TypeNode && ((Type)obj).GUID == ((InstanceNode)n).InstanceResult.Origin.DeclaringType.GUID) ||
                    (obj is MemberInfo && n is MemberNode && ((MemberInfo)obj).IsEqual(((InstanceNode)n).InstanceResult.Origin)))
                {
                    hasNode = true;
                    if (string.IsNullOrEmpty(alias))
                        n.Remove();
                    else
                        ((AbstractAssemblyNode)n).OnAliasChanged(obj, alias);
                }
            }

            if (!hasNode)
                AddNode(obj, alias);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            findManager.Cancel();
            tmrUpdateStatus.Stop();
        }

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

            if (disposing && findManager != null)
            {
                findManager.Cancel();
                tmrUpdateStatus.Stop();
            }
            base.Dispose(disposing);
        }

        private void tmrUpdateStatus_Tick(object sender, EventArgs e)
        {
            lblStatus.Text = findManager.Status;
        }


    }
}
