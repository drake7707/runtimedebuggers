using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class ReferencesNode : AbstractAssemblyNode
    {
        private bool populated;

        private Assembly ass;

        public ReferencesNode(Assembly ass)
        {
            this.ass = ass;


            UpdateText(false);
            int iconIdx = (int)IconEnum.ReferenceFolderClosed;
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;

            Nodes.Add("");
        }

        public override void Populate(string filterstring)
        {
            if (populated)
                return;

            Nodes.Clear();

            foreach (var reference in ass.GetReferencedAssemblies())
            {
                Nodes.Add(new ReferenceNode(reference));
            }

            populated = true;
        }

        

   
        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);
            this.Text = "References";
        }

        public override List<RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock> Visualization
        {
            get
            {
                return new List<VisualizerHelper.CodeBlock>();
            }
        }
    

    }
}
