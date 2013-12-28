using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunTimeDebuggers.Controls;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.LocalsDebugger
{
    internal class IEnumerableNode : TLNode, ILocalsNode
    {
        public IEnumerableNode()
        {
        }

        public IEnumerableNode(params object[] fields)
            : base(fields)
        {
        }

        public object[] List { get; set; }

        public int Offset { get; set; }
        public int Count { get; set; }

        public bool Changed { get; set; }

        public void Evaluate()
        {
            var parent = (MemberNode)Parent;
            object[] items = LocalsHelper.AsObjectArrayFromIenumerable(parent.Object);
            if(items != null)
                List = items;

            if (Offset + Count > items.Length)
                Count = items.Length - Offset;

            for (int i = Offset; i < Offset + Count; i++)
            {
                var itemValue = List[i];
                var typeName = itemValue == null ? "" : itemValue.GetType().ToSignatureString();


                if (i - Offset < Nodes.Count)
                {
                    var n = (MemberNode)Nodes[i - Offset];
                    var oldValue = n.Object;
                    n.Object = List[i];
                    n.ObjectType = List[i] == null ? null : List[i].GetType();

                    n[1] = List[i];
                    n[2] = typeName;

                    n.Changed = !object.Equals(oldValue, n.Object);
                }
                else
                {
                    MemberNode item = new MemberNode("[" + i + "]", List[i], typeName)
                    {
                        ObjectType = itemValue == null ? null : itemValue.GetType(),
                        Object = itemValue,
                        Member = null,
                        Changed = true
                    };
                    item.HasChildren = true;
                    this.Nodes.Add(item);
                }
            }
        }

        public void ExpandMembers()
        {
            Nodes.Clear();

            for (int i = Offset; i < Count; i++)
            {
                var itemValue = List[i];
                var typeName = itemValue == null ? "" : itemValue.GetType().ToSignatureString();
                MemberNode item = new MemberNode("[" + i + "]", List[i], typeName)
                {
                    ObjectType = itemValue == null ? null : itemValue.GetType(),
                    Object = itemValue,
                    Member = null
                };
                item.HasChildren = true;
                Nodes.Add(item);
            }
        }
    }
}
