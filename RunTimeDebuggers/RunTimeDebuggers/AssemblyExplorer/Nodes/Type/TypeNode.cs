using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class TypeNode : AbstractAssemblyNode
    {
        private bool populated;
        private Type type;

        public Type Type
        {
            get { return type; }
            set { type = value; }
        }

        public TypeNode(Type type)
        {
            this.type = type;

            UpdateText(false);
            var iconIdx = type.GetIcon();
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;

            Nodes.Add("");
        }


        public override void Populate(string filterstring)
        {
            if (populated)
                return;

            Nodes.Clear();

            Nodes.Add(new BaseTypesNode(type));

            foreach (var t in type.GetNestedTypesOfType().OrderBy(t => t.Name))
            {
                Nodes.Add(new TypeNode(t));
            }

            foreach (var field in type.GetFieldsOfType(false, true).OrderBy(f => f.Name))
            {
                if (!field.Name.Contains(">k__BackingField") && (string.IsNullOrEmpty(filterstring) || field.Name.ToLower().Contains(filterstring) || AliasManager.Instance.FilterOnAliasName(field, filterstring)))
                    Nodes.Add(new FieldNode(field, false));
            }
            foreach (var prop in type.GetPropertiesOfType(false, true).OrderBy(p => p.Name))
            {
                if ((string.IsNullOrEmpty(filterstring) || prop.Name.ToLower().Contains(filterstring) || AliasManager.Instance.FilterOnAliasName(prop, filterstring)))
                    Nodes.Add(new PropertyNode(prop, false));
            }

            foreach (var ev in type.GetEventsOfType(false, true).OrderBy(c => c.Name))
            {
                if ((string.IsNullOrEmpty(filterstring) || ev.Name.ToLower().Contains(filterstring) || AliasManager.Instance.FilterOnAliasName(ev, filterstring)))
                    Nodes.Add(new EventNode(ev, false));
            }

            foreach (var ctor in type.GetConstructorsOfType(true).OrderBy(c => c.Name))
            {
                if ((string.IsNullOrEmpty(filterstring) || ctor.Name.ToLower().Contains(filterstring) || AliasManager.Instance.FilterOnAliasName(ctor, filterstring)))
                    Nodes.Add(new ConstructorNode(ctor, false));
            }

            foreach (var method in type.GetMethodsOfType(false, true).OrderBy(m => m.Name))
            {
                if ((string.IsNullOrEmpty(filterstring) || method.Name.ToLower().Contains(filterstring) || AliasManager.Instance.FilterOnAliasName(method, filterstring)))
                    Nodes.Add(new MethodNode(method, false));
            }

            populated = true;
        }

        protected override void RemoveNodeIfEmpty(string filterstring)
        {
            if (this.type.Name.ToLower().Contains(filterstring) || 
                this.type.FullName.ToLower().Contains(filterstring)  ||
                AliasManager.Instance.FilterOnAliasName(type, filterstring))
            {
                // don't remove, namespace name contains filterstring
            }
            else
                base.RemoveNodeIfEmpty(filterstring);
        }

        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);
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
            get { return type.GetTypeVisualization(); }
        }
    }
}
