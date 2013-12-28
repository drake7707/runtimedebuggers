using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;
using RunTimeDebuggers.Controls;

namespace RunTimeDebuggers.LocalsDebugger
{
    class EventNode : TLNode, ILocalsNode
    {

        public EventInfo Event { get; private set; }

        private object fixedParentObject;

        public EventNode(EventInfo ev, object parentObject)
        {
            this.Event = ev;
            this.fixedParentObject = parentObject;

            this[0] = Event.GetName(false);
            this[2] = ev.EventHandlerType.ToSignatureString();

            ImageId = Event.GetIcon();
            Evaluate();
            Changed = false;
        }

        public object ParentObject
        {
            get
            {
                if (fixedParentObject != null)
                    return fixedParentObject;

                if (Parent != null && Parent is MemberNode)
                    return ((MemberNode)Parent).Object;
                else
                    return null;
            }
        }

        public void ExpandMembers()
        {
            try
            {
                foreach (var m in Event.GetSubscribedMethods(ParentObject))
                {
                    MemberNode n = new MemberNode(m) { Object = m, ObjectType = m.GetType() };
                    n[1] = m.ToSignatureString();
                    n[2] = m.GetType().ToSignatureString();

                    this.Nodes.Add(n);
                }

            }
            catch (Exception)
            {
            }

            if (Nodes.Count <= 0)
                HasChildren = false;
            else
                HasChildren = true;
        }

        public void Evaluate()
        {
          
        }

        public bool Changed { get; set; }

    }
}
