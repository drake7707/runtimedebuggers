using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class ReferenceNode : AbstractAssemblyNode
    {
        private AssemblyName ass;

        public ReferenceNode(AssemblyName ass)
        {
            this.ass = ass;


            UpdateText(false);
            int iconIdx = (int)IconEnum.Assembly;
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;
        }

        public override void Populate(string filterstring)
        {
        }

        public override void UpdateText(bool recursive)
        {
            Text = ass.Name;
            this.StatusText = ass.Name + " - Alias: " + AliasManager.Instance.GetAlias(ass);

        }

        public override List<RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock> Visualization
        {
            get
            {
                return new List<VisualizerHelper.CodeBlock>()
                {
                    new RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock(ass.FullName)
                };
            }
        }

    }
}
