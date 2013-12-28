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

    abstract partial class Analysis : TreeNodeControl
    {
        protected AnalysisManager analysisManager;

        public Analysis(IAssemblyBrowser browser, AnalysisManager analysisManager)
            : base(browser)
        {
            this.analysisManager = analysisManager;
            InitializeComponent();

            Initialize(tvNodes);
        }


        public abstract void Fill();

        protected override void AliasManager_AliasChanged(object obj, string alias)
        {
            // iterate over all subnodes instead of nodes
            foreach (TreeNode n in tvNodes.Nodes)
            {
                foreach (var analyzeNode in n.Nodes)
                {
                    if (analyzeNode != null)
                        ((AbstractAssemblyNode)analyzeNode).OnAliasChanged(obj, alias);
                }
            }
        }
      

       

     

    }
}
