using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class BaseTypesNode : AbstractAssemblyNode
    {
           private bool populated;

        private Type type;

        public BaseTypesNode(Type type)
        {
            this.type = type;

            UpdateText(false);
            int iconIdx = (int)IconEnum.BaseTypes;
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;

            Nodes.Add("");
        }

        public override void Populate(string filterstring)
        {
            if (populated)
                return;

            Nodes.Clear();

            Type baseType = type.BaseType;
            if(baseType != null)
                Nodes.Add(new BaseTypeNode(baseType));

            foreach (var t in type.GetInterfacesOfType(false))
            {
                Nodes.Add(new BaseTypeNode(t));
            }

            populated = true;
        }

        

   
        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);
            this.Text = "Base Types";
        }

        public override string Visualization
        {
            get { return ""; }
        }

    
    }
}
