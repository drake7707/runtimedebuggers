using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class ResourcesNode : AbstractAssemblyNode
    {
        private bool populated;

        private Assembly ass;

        public ResourcesNode(Assembly ass)
        {
            this.ass = ass;


            UpdateText(false);
            int iconIdx = (int)IconEnum.ResourcesFile;
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;

            Nodes.Add("");
        }

        public override void Populate(string filterstring)
        {
            if (populated)
                return;

            Nodes.Clear();

            try
            {
                foreach (var name in ass.GetManifestResourceNames())
                {
                    Nodes.Add(new ResourceNode(ass, name));
                }

            }
            catch (Exception ex)
            {
                Nodes.Add("Error: " + ex.GetType().FullName + " - " + ex.Message);
            }
            populated = true;
        }




        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);
            this.Text = "Resources";
        }

        public override string Visualization
        {
            get { return ""; }
        }

    }
}
