using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class BaseTypeNode : AbstractAssemblyNode
    {
        private bool populated;

        private Type type;

        public Type Type
        {
            get { return type; }
        }

        public BaseTypeNode(Type type)
        {
            this.type = type;

            UpdateText(false);
            int iconIdx = (int)type.GetIcon();
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
            if (baseType != null)
                Nodes.Add(new BaseTypeNode(baseType));

            foreach (var t in type.GetInterfacesOfType(false))
            {
                Nodes.Add(new BaseTypeNode(t));
            }

            populated = true;
        }

        public override void UpdateText(bool recursive)
        {
            this.Text = type.ToSignatureString();
            this.StatusText = type.ToSignatureString(false) + " - Token: " + type.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(type);
        }

        internal override void OnAliasChanged(object obj, string alias)
        {
            if (obj is Type && ((Type)obj).GUID == type.GUID)
                UpdateText(false);

            base.OnAliasChanged(obj, alias);
        }

        public override string Visualization
        {
            get { return type.ToSignatureString(); }
        }

    }
}
