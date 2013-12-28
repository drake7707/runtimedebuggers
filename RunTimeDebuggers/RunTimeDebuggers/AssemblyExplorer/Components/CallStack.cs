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
    partial class CallStack : TreeNodeControl
    {
        private StackTrace stackTrace;
        public CallStack(IAssemblyBrowser browser, StackTrace stackTrace)
            : base(browser)
        {
            this.stackTrace = stackTrace;
            InitializeComponent();

            Initialize(tvNodes);

            Fill();
        }
        
        public void Fill()
        {

            tvNodes.BeginUpdate();
            foreach (var frame in this.stackTrace.GetFrames())
            {
                tvNodes.Nodes.Add(new StackFrameNode(frame, true));
            }

            tvNodes.EndUpdate();
        }


        
    }
}
