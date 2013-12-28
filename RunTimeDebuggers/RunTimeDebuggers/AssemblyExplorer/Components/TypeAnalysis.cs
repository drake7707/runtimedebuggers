using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class TypeAnalysis : Analysis
    {
        private Type type;


        public TypeAnalysis(IAssemblyBrowser browser, AnalysisManager analysisManager, Type type)
            : base(browser, analysisManager)
        {
            this.type = type;

            Fill();
        }



        public override void Fill()
        {
            tvNodes.BeginUpdate();

            var cache = analysisManager.GetTypeCache(type);

            var usedByNode = new TreeNode("Used by", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
            foreach (MemberInfo m in cache.UsedBy)
            {
                var n = MemberNode.GetNodeOfMember(m, true);
                if (n != null)
                    usedByNode.Nodes.Add(n);
            }

            TreeNode createdByNode = new TreeNode("Created by", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
            foreach (MemberInfo m in cache.CreatedBy)
            {
                var n = MemberNode.GetNodeOfMember(m, true);
                if (n != null)
                    createdByNode.Nodes.Add(n);
            }

            TreeNode derivedByNode = new TreeNode("Derived by", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
            foreach (Type m in cache.DerivedBy)
            {
                var n = new TypeNode(m);
                n.Nodes.Clear();
                derivedByNode.Nodes.Add(n);
            }

            TreeNode implementedByNode = new TreeNode("Implemented by", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
            foreach (Type m in cache.ImplementedBy)
            {
                var n = new TypeNode(m);
                n.Nodes.Clear();
                implementedByNode.Nodes.Add(n);
            }

            tvNodes.Nodes.Add(usedByNode);
            tvNodes.Nodes.Add(createdByNode);
            tvNodes.Nodes.Add(derivedByNode);
            tvNodes.Nodes.Add(implementedByNode);

            tvNodes.ExpandAll();
            tvNodes.EndUpdate();
        }
    }
}
