using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class StackFrameNode : MemberNode
    {

        private StackFrame frame;

        public StackFrameNode(StackFrame frame, bool prefixDeclaringType)
            : base(prefixDeclaringType)
        {
            this.frame = frame;

            var iconIdx = frame.GetMethod().GetIcon();
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;
            Tag = frame.GetILOffset();
            UpdateText(false);
        }

        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);

            var mInfo = frame.GetMethod();

            string str = "";
            str += frame.GetILOffset().ToString("x4") + " in ";

            if (mInfo != null)
            {
                if (prefixDeclaringType)
                {
                    if (mInfo.DeclaringType != null)
                        str += mInfo.DeclaringType.ToSignatureString() + "::";
                }

                str += mInfo.ToSignatureString();
            }
            else
                str += "unknown method";

            if (this.Text != str)
                this.Text = str;
        }

        internal override void OnAliasChanged(object obj, string alias)
        {
            if (obj is Type) // can be any parameter or return type
                UpdateText(false);
            else if (obj is MethodBase && (MethodBase)obj == frame.GetMethod())
                UpdateText(false);

            base.OnAliasChanged(obj, alias);
        }

        public override List<RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock> Visualization
        {
            get
            {
                return MethodNode.GetVisualisation(this, frame.GetMethod());
            }
        }

        public override System.Reflection.MemberInfo Member
        {
            get { return frame.GetMethod(); }
        }

        public override void Populate(string filterstring)
        {
        }

    }
}
