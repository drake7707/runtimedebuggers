using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;
using RunTimeDebuggers.Controls;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.LocalsDebugger
{
    internal class MemberNode : TLNode, ILocalsNode
    {
        public MemberNode(MemberInfo member)
        {
            //this.ParentObject = parentObject;
            this.Member = member;
            this[0] = member.GetName(false);

            ImageId = member.GetIcon();
            Evaluate();
            Changed = false;
        }

        public MemberNode(params object[] fields)
            : base(fields)
        {
        }

        protected override void OnParentChanged(EventArgs e)
        {
            Evaluate();
            Changed = false;
        }

        public object ParentObject
        {
            get
            {
                if (Parent != null && Parent is MemberNode)
                    return ((MemberNode)Parent).Object;
                else
                    return null;
            }
        }

        public object Object { get; set; }
        public Type ObjectType { get; set; }
        public MemberInfo Member { get; set; }

        public object Key
        {
            get
            {
                return new KeyValuePair<object, KeyValuePair<Object, MemberInfo>>(this[0], new KeyValuePair<Object, MemberInfo>(Object, Member));
            }
        }


        public virtual void Evaluate()
        {
            if (ParentObject != null && Member != null)
            {
                object oldValue = Object;
                Type oldType = ObjectType;

                if (Member is PropertyInfo)
                {
                    var prop = (PropertyInfo)Member;
                    try
                    {
                        if (prop.GetIndexParameters().Length == 0)
                        {
                            var obj = prop.GetValue(ParentObject, null);
                            Object = obj;
                            ObjectType = obj == null ? null : obj.GetType();

                            if (obj == null)
                            {
                                this[1] = "null";
                                this[2] = "";
                            }
                            else
                            {
                                this[1] = obj;
                                this[2] = ObjectType.ToSignatureString();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        this[1] = "(ERROR)";
                    }
                }
                else if (Member is FieldInfo)
                {
                    var field = (FieldInfo)Member;
                    try
                    {
                        var obj = field.GetValue(ParentObject);

                        Object = obj;
                        ObjectType = obj == null ? null : obj.GetType();
                        if (obj == null)
                        {
                            this[1] = "null";
                            this[2] = "";
                        }
                        else
                        {
                            this[1] = obj;
                            this[2] = ObjectType.ToSignatureString();
                        }

                    }
                    catch (Exception)
                    {
                        this[1] = "(ERROR)";
                    }
                }

                SetChanged(oldValue, oldType);
            }
        }

        protected void SetChanged(object oldValue, Type oldType)
        {
            Changed = !object.Equals(oldValue, Object);

            if (ObjectType != oldType)
            {
                // properties can be different
                this.Collapse();
                this.Nodes.Clear();
            }

            if (CanExpandNodeWithType(ObjectType))
                HasChildren = true;
            else
                HasChildren = false;
        }

        //public static string ToGenericTypeString(Type t)
        //{
        //    if (!t.IsGenericType)
        //        return t.Name;
        //    string genericTypeName = t.GetGenericTypeDefinition().Name;
        //    genericTypeName = genericTypeName.Substring(0,
        //        genericTypeName.IndexOf('`'));
        //    string genericArgs = string.Join(",",
        //        t.GetGenericArguments()
        //            .Select(ta => ToGenericTypeString(ta)).ToArray());
        //    return genericTypeName + "<" + genericArgs + ">";
        //}

        public bool Changed { get; set; }

        public virtual void ExpandMembers(bool showControls, bool showFields, bool showProperties)
        {
            if (Object == null)
                return;



            Nodes.Clear();

            if (ObjectType.BaseType != null && (ObjectType.BaseType != typeof(object) && ObjectType.BaseType != typeof(System.ValueType)))
            {
                MemberNode n = new MemberNode("(base)", "", ObjectType.BaseType)
                {
                    ObjectType = ObjectType.BaseType,
                    Object = Object,
                    Member = null
                };
                n.ImageId = (int)IconEnum.PublicField;

                if (CanExpandNodeWithType(n.ObjectType))
                    n.HasChildren = true;

                Nodes.Add(n);
            }

            var events = ObjectType.GetEventsOfType(true, false);
            if (events.Count > 0)
            {
                MemberNode n = new MemberNode("(all wired events)", "", ObjectType)
                {
                    ObjectType = ObjectType,
                    Object = Object,
                    Member = null
                };
                n.ImageId = (int)IconEnum.PublicEvent;

                foreach (var ev in events)
                {
                    EventNode eventNode = new EventNode(ev, Object);
                    eventNode.ExpandMembers();

                    // add if has wired events
                    if (eventNode.Nodes.Count > 0)
                        n.Nodes.Add(eventNode);
                }

                Nodes.Add(n);
            }

            if (ObjectType != typeof(string))
            {
                try
                {
                    object[] array = LocalsHelper.AsObjectArrayFromIenumerable(Object);
                    if (array != null)
                        AddIEnumerableNodes(array);

                    //var ienumerableInterface = ObjectType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).FirstOrDefault();
                    //if (ienumerableInterface != null)
                    //{
                    //    object[] items = LocalsHelper.AsObjectArrayFromGenericIenumerable(Object);
                    //    AddIEnumerableNodes(items);
                    //}
                    //else
                    //{
                    //    var ienumerableNonGenericInterface = ObjectType.GetInterfaces().Where(i => i.IsAssignableFrom(typeof(IEnumerable))).FirstOrDefault();
                    //    if (ienumerableInterface != null)
                    //    {
                    //        object[] items = LocalsHelper.AsObjectArrayFromIenumerable(Object);
                    //        AddIEnumerableNodes(items);
                    //    }
                    //    else
                    //    {
                    //        var icollection = ObjectType.GetInterfaces().Where(i => i.IsAssignableFrom(typeof(ICollection))).FirstOrDefault();
                    //        if (icollection != null)
                    //        {
                    //            object[] items = LocalsHelper.AsObjectArrayFromIenumerable(Object);
                    //            AddIEnumerableNodes(items);
                    //        }

                    //    }
                    //}
                }
                catch (Exception)
                {

                }
            }

            foreach (var f in ObjectType.GetFieldsOfType(false, false)
                                        .OrderBy(f => f.Name))
            {
                if (!f.Name.Contains(">k__BackingField"))
                {
                    MemberNode n = new MemberNode(f);
                    AddNode(this, n, showControls, showFields, showProperties);
                }
            }

            foreach (var p in ObjectType.GetPropertiesOfType(false, true)
                              .Where(p => p.GetIndexParameters().Length <= 0)
                              .OrderBy(p => p.Name))
            {
                MemberNode n = new MemberNode(p);
                AddNode(this, n, showControls, showFields, showProperties);
            }

            foreach (var ev in ObjectType.GetEventsOfType(false, true)
                                         .OrderBy(e => e.Name))
            {
                EventNode n = new EventNode(ev, null);
                this.Nodes.Add(n);
                n.ExpandMembers();
            }

            if (Nodes.Count <= 0)
                HasChildren = false;
        }

        private void AddIEnumerableNodes(object[] items)
        {
            for (int i = 0; i < items.Length; i += 100)
            {
                int offset = i;
                int count = offset + 100 > items.Length ? items.Length - offset : 100;
                IEnumerableNode ienode = new IEnumerableNode("(items [" + offset + "-" + (offset + count) + "])", "", items.GetType())
                {
                    List = items,
                    Offset = offset,
                    Count = count
                };
                ienode.HasChildren = true;
                ienode.ImageId = (int)IconEnum.PublicField;
                Nodes.Add(ienode);
            }
        }


        private bool CanExpandNodeWithType(Type t)
        {
            if (t == null)
                return false;

            if (t == typeof(object))
                return false;

            if (t == typeof(Int16) || t == typeof(Int32) || t == typeof(Int64) || t == typeof(Byte) ||
                t == typeof(UInt16) || t == typeof(UInt32) || t == typeof(UInt64) || t == typeof(Char) ||
                t == typeof(Single) || t == typeof(Double) || t == typeof(Boolean))
                return false;

            return true;
        }

        protected void AddNode(TLNode parentNode, MemberNode n, bool showControls, bool showFields, bool showProperties)
        {
            bool add = true;
            if (!showControls && typeof(Control).IsAssignableFrom(n.Member.GetReturnType()))
            {
                // don't add
                add = false;
            }

            if (!showFields && n.Member is FieldInfo)
            {
                // don't add
                add = false;
            }

            if (!showProperties && n.Member is PropertyInfo)
            {
                // don't add
                add = false;
            }

            if (add)
                parentNode.Nodes.Add(n);
        }
    }
}
