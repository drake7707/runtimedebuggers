using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class FieldNode : MemberNode
    {
        private FieldInfo field;

        public FieldInfo Field
        {
            get { return field; }
            set { field = value; }
        }

        public FieldNode(FieldInfo field, bool prefixDeclaringType)
            : base(prefixDeclaringType)
        {
            this.field = field;


            UpdateText(false);
            int iconIdx = (int)field.GetIcon();
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;
        }

        public override void Populate(string filterstring)
        {
        }


        private string GetValueIfStaticAndReadable()
        {
            try
            {
                if (field.IsStatic)
                    return "Value: " + field.GetValue(null);
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

            str += field.GetName(prefixDeclaringType) + " : " + field.FieldType.ToSignatureString();

            if (this.Text != str)
                this.Text = str;

            this.StatusText = field.GetName(false) + " - Token: " + field.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(field);
        }

        internal override void OnAliasChanged(object obj, string alias)
        {
            if (obj is Type && field.FieldType.GUID == ((Type)obj).GUID)
                UpdateText(false);
            else if (obj is FieldInfo && (FieldInfo)obj == field)
                UpdateText(false);

            base.OnAliasChanged(obj, alias);
        }

        public override List<RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock> Visualization
        {
            get
            {

                var blocks = new List<VisualizerHelper.CodeBlock>();
                blocks.AddRange(VisualizerHelper.GetMemberVisualization(Field));
                blocks.Add(new RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock(Environment.NewLine));
                blocks.Add(new RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock(GetValueIfStaticAndReadable() + ""));
                return blocks;
            }
        }

        public override MemberInfo Member
        {
            get { return Field; }
        }
    }
}
