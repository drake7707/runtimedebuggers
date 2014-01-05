using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    abstract class AbstractAssemblyNode : TreeNode
    {
        public AbstractAssemblyNode()
        {
        }

        public virtual void PopulateAll(string filterstring)
        {
            Populate(filterstring);

            foreach (TreeNode n in GetNodes().ToArray())
            {
                if (n is AbstractAssemblyNode)
                {
                    ((AbstractAssemblyNode)n).PopulateAll(filterstring);
                }
            }

            RemoveNodeIfEmpty(filterstring);
        }

        protected virtual void RemoveNodeIfEmpty(string filterstring)
        {
            if (Nodes.Count <= 0)
                this.Remove();
        }

        public abstract void Populate(string filterstring);

        public abstract List<VisualizerHelper.CodeBlock> Visualization
        {
            get;
        }

        public virtual void UpdateText(bool recursive)
        {
            if (recursive)
            {
                foreach (var n in Nodes)
                {
                    if (n is AbstractAssemblyNode)
                        ((AbstractAssemblyNode)n).UpdateText(recursive);
                }
            }
        }

        internal virtual void OnAliasChanged(object obj, string alias)
        {
            foreach (var n in Nodes)
            {
                if (n is AbstractAssemblyNode)
                    ((AbstractAssemblyNode)n).OnAliasChanged(obj, alias);
            }
        }


        internal IEnumerable<AbstractAssemblyNode> GetNodes()
        {
            foreach (TreeNode n in Nodes)
            {
                if (n is AbstractAssemblyNode)
                    yield return (AbstractAssemblyNode)n;
            }
        }

        public string StatusText { get; set; }
    }
}
