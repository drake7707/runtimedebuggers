using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class NamespaceNode : AbstractAssemblyNode
    {
        private bool populated;
        private List<Type> types;

        public string Namespace
        {
            get
            {
                if (types.Count > 0)
                    return types[0].Namespace;
                return "";
            }
        }
        public NamespaceNode(List<Type> types)
        {
            this.types = types;

            UpdateText(false);
            this.ImageIndex = (int)IconEnum.Namespace;
            this.SelectedImageIndex = (int)IconEnum.Namespace;

            Nodes.Add("");
        }

        public override void Populate(string filterstring)
        {
            if (populated)
                return;

            Nodes.Clear();

            foreach (var t in types.OrderBy(t => t.Name))
            {
                if(!t.IsNested)
                    Nodes.Add(new TypeNode(t));
            }

            populated = true;
        }

        protected override void RemoveNodeIfEmpty(string filterstring)
        {
            if (this.Namespace != null && this.Namespace.ToLower().Contains(filterstring))
            {
                // don't remove, namespace name contains filterstring
            }
            else
                base.RemoveNodeIfEmpty(filterstring);
        }

        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);
            this.Text = Namespace;

            this.StatusText = Namespace;
        }

        public override string Visualization
        {
            get { return this.Text; }
        }
    }
}
