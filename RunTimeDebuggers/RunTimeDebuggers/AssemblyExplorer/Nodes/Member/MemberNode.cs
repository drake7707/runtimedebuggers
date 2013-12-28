using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace RunTimeDebuggers.AssemblyExplorer
{
    abstract class MemberNode : AbstractAssemblyNode
    {

        protected bool prefixDeclaringType;

        public MemberNode(bool prefixDeclaringType)
        {
            this.prefixDeclaringType = prefixDeclaringType;
        }
        public abstract MemberInfo Member { get; }

        protected override void RemoveNodeIfEmpty(string filterstring)
        {
            // no removing if empty, leaf node
        }

        public static MemberNode GetNodeOfMember(MemberInfo m, bool prefixDeclaringType)
        {
            if (m is FieldInfo)
                return new FieldNode((FieldInfo)m, prefixDeclaringType);
            else if (m is PropertyInfo)
            {
                var n = new PropertyNode((PropertyInfo)m, prefixDeclaringType);
                n.Nodes.Clear();
                return n;
            }
            else if (m is MethodInfo)
                return new MethodNode((MethodInfo)m, prefixDeclaringType);
            else if (m is ConstructorInfo)
                return new ConstructorNode((ConstructorInfo)m, prefixDeclaringType);
            else if (m is EventInfo)
                return new EventNode((EventInfo)m, prefixDeclaringType);

            return null;
        }
    }
}
