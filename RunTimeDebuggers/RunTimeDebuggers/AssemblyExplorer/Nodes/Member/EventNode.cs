using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class EventNode : MemberNode
    {
        private bool populated;
        private EventInfo ev;

        public EventInfo Event
        {
            get { return ev; }
            set { ev = value; }
        }

        public EventNode(EventInfo ev,bool prefixDeclaringType)
            : base(prefixDeclaringType)
        {
            this.ev = ev;


            UpdateText(false);
            int iconIdx = (int)ev.GetIcon();
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;

            Nodes.Add("");
        }

        public override void Populate(string filterstring)
        {
            if (populated)
                return;

            Nodes.Clear();
            var addMethod = ev.GetAddMethod(true);
            var removeMethod = ev.GetRemoveMethod(true);
            var raiseMethod = ev.GetRaiseMethod(true);
            if (addMethod != null)
                Nodes.Add(new MethodNode(addMethod, false));

            if (removeMethod != null)
                Nodes.Add(new MethodNode(removeMethod, false));

            if (raiseMethod != null)
                Nodes.Add(new MethodNode(raiseMethod, false));


            populated = true;
        }


        private string GetValueIfStaticAndReadable()
        {
            //try
            //{
            //    if (ev.GetAddMethod(true).IsStatic)
            //    {
            //        return "Value: " + ev.GetValue(null);

            //    }
            //}
            //catch (Exception ex)
            //{
            //    return "Unable to evaluate: " + ex.GetType().FullName + " - " + ex.Message;
            //}
            return "";
        }

        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);
            string str = "";

            str += ev.GetName(prefixDeclaringType) + " : " + ev.EventHandlerType.ToSignatureString();

            if (this.Text != str)
                this.Text = str;

            this.StatusText = ev.GetName(false) + " - Token: " + ev.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(ev);
        }

        internal override void OnAliasChanged(object obj, string alias)
        {
            if (obj is Type && ev.EventHandlerType.GUID == ((Type)obj).GUID)
                UpdateText(false);
            else if (obj is EventInfo && (EventInfo)obj == ev)
                UpdateText(false);

            base.OnAliasChanged(obj, alias);
        }


        public override List<RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock> Visualization
        {
            get
            {
                return new List<VisualizerHelper.CodeBlock>()
                {
                    new RunTimeDebuggers.Helpers.VisualizerHelper.CodeBlock(this.Text + Environment.NewLine + GetValueIfStaticAndReadable())
                };
            }
        }

        public override MemberInfo Member
        {
            get { return Event; }
        }
    }
}
