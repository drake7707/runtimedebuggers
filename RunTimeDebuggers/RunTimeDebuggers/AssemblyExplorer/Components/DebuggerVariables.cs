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
    partial class DebuggerVariables : TreeNodeControl
    {
        public DebuggerVariables(IAssemblyBrowser browser)
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

            while (debugger != null && debugger.MethodDebugger != null)
                debugger = debugger.MethodDebugger; // find top of the stack

            if (debugger != null)
            {
                var debugNode = MemberNode.GetNodeOfMember(debugger.Method, true);

                TreeNode localsNode = new TreeNode("Locals");

                for (int i = 0; i < debugger.Locals.Length; i++)
                {
                    var itm = debugger.Locals[i].Value;

                    string prefix = "[" + debugger.Locals[i].Info.LocalIndex + "] ";
                    string typeSuffix = " (" + (debugger.Locals[i].Info.LocalType == null ? "" : debugger.Locals[i].Info.LocalType.ToSignatureString()) + ")";
                    try
                    {
                        if (itm == null)
                            localsNode.Nodes.Add(prefix + "null" + typeSuffix);
                        else
                            localsNode.Nodes.Add(prefix + itm + typeSuffix);
                    }
                    catch (Exception)
                    {
                        localsNode.Nodes.Add(prefix + "" + typeSuffix);
                    }
                }

                debugNode.Nodes.Add(localsNode);

                TreeNode argumentsNode = new TreeNode("Arguments");

                for (int i = 0; i < debugger.Parameters.Length; i++)
                {
                    var itm = debugger.Parameters[i];

                    string typeSuffix = " (" + (itm.Info.ParameterType == null ? "" : itm.Info.ParameterType.ToSignatureString()) + ")";
                    try
                    {
                        if (itm.Value == null)
                            argumentsNode.Nodes.Add(itm.Info.Name + "= " + "null" + typeSuffix);
                        else
                            argumentsNode.Nodes.Add(itm.Info.Name + "= " + itm.Value + typeSuffix);
                    }
                    catch (Exception)
                    {
                        argumentsNode.Nodes.Add(typeSuffix);
                    }
                }

                debugNode.Nodes.Add(argumentsNode);

                tvNodes.Nodes.Add(debugNode);
            }

            tvNodes.EndUpdate();
            tvNodes.ExpandAll();
            tvNodes.Refresh();
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
