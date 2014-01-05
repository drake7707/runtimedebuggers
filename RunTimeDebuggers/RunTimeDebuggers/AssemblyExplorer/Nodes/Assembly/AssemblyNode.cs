using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class AssemblyNode : AbstractAssemblyNode
    {
        private bool populated;

        private Assembly ass;

        public Assembly Assembly
        {
            get { return ass; }
            set { ass = value; }
        }
        public AssemblyNode(Assembly assembly)
        {
            this.ass = assembly;

            this.ImageIndex = (int)IconEnum.Assembly;
            this.SelectedImageIndex = (int)IconEnum.Assembly;
            UpdateText(false);
            this.Nodes.Add("dummy");
        }

        public override void Populate(string filterstring)
        {
            if (populated)
                return;

            Nodes.Clear();
            try
            {
                Nodes.Add(new ReferencesNode(ass));

                Nodes.Add(new ResourcesNode(ass));


                Dictionary<string, List<Type>> typesByNamespace = ass.GetTypesSafe().GroupBy(t => t.Namespace)
                    .ToDictionary(g => string.IsNullOrEmpty(g.Key) ? "{empty}" : g.Key, g => g.ToList());

                foreach (var pair in typesByNamespace.OrderBy(p => p.Key))
                {
                    Nodes.Add(new NamespaceNode(pair.Value));
                }

            }
            catch (Exception ex)
            {
                Nodes.Add("Error: " + ex.Message);
            }

            populated = true;
        }

        protected override void RemoveNodeIfEmpty(string filterstring)
        {
            if (this.Assembly != null && this.Assembly.FullName.ToLower().Contains(filterstring))
            {
                // don't remove, namespace name contains filterstring
            }
            else
                base.RemoveNodeIfEmpty(filterstring);
        }

        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);


            this.Text = AliasManager.Instance.GetFullNameWithAlias(ass, ass.GetName().Name);

            this.StatusText = ass.GetName().Name + " - Alias: " + AliasManager.Instance.GetAlias(ass);

        }

        public override List<VisualizerHelper.CodeBlock> Visualization
        {
            get
            {
                return Assembly.GetAssemblyVisualization();
            }
        }
    }
}
