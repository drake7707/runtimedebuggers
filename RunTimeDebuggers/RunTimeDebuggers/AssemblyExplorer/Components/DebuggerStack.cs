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
    partial class DebuggerStack : TreeNodeControl
    {
        public DebuggerStack(IAssemblyBrowser browser)
            : base(browser)
        {
            InitializeComponent();

            Initialize(tvNodes);

            Fill();

            ILDebugManager.Instance.Stepped += new EventHandler(Instance_Stepped);
        }

        void Instance_Stepped(object sender, EventArgs e)
        {
            Fill();
        }

        public void Fill()
        {

            tvNodes.BeginUpdate();
            tvNodes.Nodes.Clear();
            ILDebugger debugger = ILDebugManager.Instance.Debugger;
            while (debugger != null)
            {
                AddNode(debugger);
                debugger = debugger.MethodDebugger;
            }
            tvNodes.EndUpdate();
            tvNodes.ExpandAll();
            tvNodes.Refresh();
        }

        private void AddNode(ILDebugger debugger)
        {
            var debugNode = MemberNode.GetNodeOfMember(debugger.Method, true);
            tvNodes.Nodes.Insert(0, debugNode);

            foreach (var itm in debugger.Stack)
            {
                try
                {
                    if (itm.Value == null)
                        debugNode.Nodes.Add("null (" + (itm.Type == null ? "" : itm.Type.ToSignatureString()) + ")");
                    else
                        debugNode.Nodes.Add(itm.Value + " (" + (itm.Type == null ? "" : itm.Type.ToSignatureString()) + ")");

                }
                catch (Exception)
                {
                    debugNode.Nodes.Add("" + " (" + (itm.Type == null ? "" : itm.Type.ToSignatureString()) + ")");
                }
            }
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
            if (disposing)
            {
                ILDebugManager.Instance.Stepped -= new EventHandler(Instance_Stepped);
            }
            base.Dispose(disposing);
        }

    }
}
