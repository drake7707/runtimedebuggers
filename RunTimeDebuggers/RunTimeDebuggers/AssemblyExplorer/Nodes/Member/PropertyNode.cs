using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class PropertyNode : MemberNode
    {

        private bool populated;

        private PropertyInfo prop;

        public PropertyInfo Property
        {
            get { return prop; }
            set { prop = value; }
        }

        public PropertyNode(PropertyInfo prop, bool prefixDeclaringType)
            : base(prefixDeclaringType)
        {
            this.prop = prop;

            UpdateText(false);
            int iconIdx = (int)prop.GetIcon();
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;

            Nodes.Add("");
        }

        public override void Populate(string filterstring)
        {
            if (populated)
                return;

            Nodes.Clear();
            var getMethod = prop.GetGetMethod(true);
            var setMethod = prop.GetSetMethod(true);

            if (getMethod != null)
                Nodes.Add(new MethodNode(getMethod, false));

            if (setMethod != null)
                Nodes.Add(new MethodNode(setMethod, false));

            populated = true;
        }

        private object GetValueIfStaticAndReadable()
        {
            try
            {
                var getMethod = prop.GetGetMethod(true);
                if (getMethod != null && getMethod.IsStatic)
                    return "Value: " + getMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                return "Unable to evaluate: " + ex.ToExceptionString();
            }
            return "";
        }

        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);
            string str = "";

            str+= prop.GetName(prefixDeclaringType) + " : " + prop.PropertyType.ToSignatureString();

            if (this.Text != str)
                this.Text = str;

            this.StatusText = prop.GetName(false) + " - Token: " + prop.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(prop);
        }

        internal override void OnAliasChanged(object obj, string alias)
        {
            if (obj is Type && prop.PropertyType.GUID == ((Type)obj).GUID)
                UpdateText(false);
            else if (obj is PropertyInfo && (PropertyInfo)obj == prop)
                UpdateText(false);

            base.OnAliasChanged(obj, alias);
        }

        public override List<RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock> Visualization
        {
            get {

                var blocks = new List<VisualizerHelper.CodeBlock>();
                blocks.AddRange(VisualizerHelper.GetMemberVisualization(Property));
                blocks.Add(new RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock(Environment.NewLine));
                blocks.Add(new RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock(GetValueIfStaticAndReadable() + ""));
                return blocks;
            }
        }

        public override MemberInfo Member
        {
            get { return Property; }
        }
    }
}
