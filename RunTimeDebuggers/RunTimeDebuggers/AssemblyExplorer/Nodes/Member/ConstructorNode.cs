using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class ConstructorNode : MemberNode
    {
        public ConstructorInfo Constructor { get; private set; }

        public ConstructorNode(ConstructorInfo constructor, bool prefixDeclaringType)
            : base(prefixDeclaringType)
        {
            this.Constructor = constructor;


            UpdateText(false);
            int iconIdx = (int)constructor.GetIcon();
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;
        }

        public override void Populate(string filterstring)
        {
        }

        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);


            string str = "";

            if (prefixDeclaringType)
                str += Constructor.DeclaringType.ToSignatureString() + "::";

            str += AliasManager.Instance.GetFullNameWithAlias(Constructor, Constructor.ToSignatureString());

            if (this.Text != str)
                this.Text = str;

            this.StatusText = Constructor.GetName(false) + " - Token: " + Constructor.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(Constructor);
        }

        internal override void OnAliasChanged(object obj, string alias)
        {
            if (obj is Type) // can be any parameter or return type
                UpdateText(false);
            else if (obj is ConstructorInfo && (ConstructorInfo)obj == Constructor)
                UpdateText(false);

            base.OnAliasChanged(obj, alias);
        }

        public override string Visualization
        {
            get
            {
                return MethodNode.GetVisualisation(this, Constructor);
            }
        }



        public override MemberInfo Member
        {
            get { return Constructor; }
        }
    }
}
