using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class InstanceNode : MemberNode
    {

        private InstanceFinderManager.InstanceResult result;

        internal InstanceFinderManager.InstanceResult InstanceResult
        {
            get { return result; }
            set { result = value; }
        }

        public InstanceNode(InstanceFinderManager.InstanceResult result)
            : base(false)
        {
            this.result = result;


            UpdateText(false);
            int iconIdx = (int)result.Origin.GetIcon();
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;
        }

        public override void Populate(string filterstring)
        {
            
        }

        public override void UpdateText(bool recursive)
        {
            this.Text = "(" + result.Instance + ")" + " at " + result.Origin.GetName(true);
        }

        public override string Visualization
        {
            get { return this.Text; }
        }

        public override System.Reflection.MemberInfo Member
        {
            get { return result.Origin; }
        }
    }
}
