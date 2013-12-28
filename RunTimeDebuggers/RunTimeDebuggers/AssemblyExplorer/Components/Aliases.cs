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
    partial class Aliases : TreeNodeControl
    {
        public Aliases(IAssemblyBrowser browser)
            : base(browser)
        {
            InitializeComponent();

            Initialize(tvNodes);

            Fill();
        }

        public void Fill()
        {

            tvNodes.BeginUpdate();
            foreach (var alias in AliasManager.Instance.Aliases)
            {
                AddNode(alias.Key, alias.Value);
            }

            tvNodes.EndUpdate();
        }

        private void AddNode(object obj, string alias)
        {
            if (obj is Type)
            {
                var tn = new TypeNode((Type)obj);
                tn.Nodes.Clear();
                tvNodes.Nodes.Add(tn);
            }
            else if (obj is MemberInfo)
                tvNodes.Nodes.Add(MemberNode.GetNodeOfMember((MemberInfo)obj, true));
        }

        protected override void AliasManager_AliasChanged(object obj, string alias)
        {

            bool hasNode = false;
            foreach (TreeNode n in tvNodes.Nodes)
            {
                if ((obj is Type && n is TypeNode && ((Type)obj).GUID == ((TypeNode)n).Type.GUID) || 
                    (obj is MemberInfo && n is MemberNode && ((MemberInfo)obj).IsEqual(((MemberNode)n).Member)))
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


    }
}
