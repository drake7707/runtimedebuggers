using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class MemberAnalysis : Analysis
    {
        private MemberInfo member;

        public MemberAnalysis(IAssemblyBrowser browser, AnalysisManager analysisManager, MemberInfo member)
            : base(browser, analysisManager)
        {
            this.member = member;

            Fill();
        }



        public override void Fill()
        {
            tvNodes.BeginUpdate();

            var cache = analysisManager.GetMemberCache(member);

            var usedByNode = new TreeNode("Used by", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
            foreach (MemberLookupEntry m in cache.UsedBy)
            {
                var n = MemberNode.GetNodeOfMember(m.Member, true);
                if (n != null)
                {
                    n.Tag = m.Offset;
                    usedByNode.Nodes.Add(n);
                }
            }

            TreeNode calledByNode = null;
            TreeNode usesNode = null;
            TreeNode wiredForNode = null;
            if (member is MethodInfo || member is ConstructorInfo)
            {
                var methodBaseCache = ((MethodBaseCache)cache);

                usesNode = new TreeNode("Uses", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
                foreach (MemberInfo m in methodBaseCache.Uses.Select(u => u.Member))
                {
                    var n = MemberNode.GetNodeOfMember(m, true);
                    if (n != null)
                        usesNode.Nodes.Add(n);
                }

                calledByNode = new TreeNode("Called by", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
                foreach (MemberInfo m in ((MethodBaseCache)cache).CalledBy)
                {
                    var n = MemberNode.GetNodeOfMember(m, true);
                    if (n != null)
                        calledByNode.Nodes.Add(n);
                }


                wiredForNode = new TreeNode("Wired to", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
                foreach (WiredLookupEntry m in ((MethodBaseCache)cache).WiredForEvent)
                {
                    var n = GetEventWireNode(m);
                    if (n != null)
                        wiredForNode.Nodes.Add(n);
                }
            }

            TreeNode wiresNode = null;
            if (member is EventInfo)
            {
                wiresNode = new TreeNode("Wires", (int)IconEnum.Bullet, (int)IconEnum.Bullet);

                var addMethod = ((EventInfo)member).GetAddMethod();
                if (addMethod != null)
                {
                    var addMethodCache = (MethodBaseCache)AnalysisManager.Instance.GetMemberCache(addMethod);
                    foreach (var wire in addMethodCache.WiresEvent)
                    {
                        var n = GetEventWireNode(wire);
                        if (n != null)
                            wiresNode.Nodes.Add(n);
                    }
                }
            }

            TreeNode assignedByNode = null;
            TreeNode readByNode = null;
            if (member is FieldInfo)
            {
                assignedByNode = new TreeNode("Assigned by", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
                foreach (MemberInfo m in ((FieldCache)cache).AssignedBy)
                {
                    var n = MemberNode.GetNodeOfMember(m, true);
                    if (n != null)
                        assignedByNode.Nodes.Add(n);
                }

                readByNode = new TreeNode("Read by", (int)IconEnum.Bullet, (int)IconEnum.Bullet);
                foreach (MemberInfo m in ((FieldCache)cache).ReadBy)
                {
                    var n = MemberNode.GetNodeOfMember(m, true);
                    if (n != null)
                        readByNode.Nodes.Add(n);
                }
            }

            if (usesNode != null)
                tvNodes.Nodes.Add(usesNode);

            if (usedByNode != null)
                tvNodes.Nodes.Add(usedByNode);

            if (calledByNode != null)
                tvNodes.Nodes.Add(calledByNode);
            if (assignedByNode != null)
                tvNodes.Nodes.Add(assignedByNode);

            if (wiredForNode != null)
                tvNodes.Nodes.Add(wiredForNode);
            if (wiresNode != null)
                tvNodes.Nodes.Add(wiresNode);

            if (readByNode != null)
                tvNodes.Nodes.Add(readByNode);

            tvNodes.ExpandAll();
            tvNodes.EndUpdate();
        }

        private static EventWireNode GetEventWireNode(WiredLookupEntry m)
        {
            var memberCache = (MethodBaseCache)AnalysisManager.Instance.GetMemberCache(m.EventMethod);

            MemberInfo source;
            if (m.Source is MethodBase)
            {
                // use property instead of get method of property
                var sourceCache = (MethodBaseCache)AnalysisManager.Instance.GetMemberCache(m.Source);
                if (sourceCache.SpecialReference is PropertyInfo)
                    source = sourceCache.SpecialReference;
                else
                    source = m.Source;
            }
            else
                source = m.Source;

            var n = new EventWireNode((EventInfo)memberCache.SpecialReference, source, true);
            n.Nodes.Clear();
            return n;
        }



        public class EventWireNode : MemberNode
        {

            public EventWireNode(EventInfo ev, MemberInfo source, bool prefixDeclaringType)
                : base(prefixDeclaringType)
            {
                this.Event = ev;
                this.Source = source;

                UpdateText(false);
                int iconIdx = (int)Event.GetIcon();
                this.ImageIndex = iconIdx;
                this.SelectedImageIndex = iconIdx;
            }

            public EventInfo Event { get; set; }
            public MemberInfo Source { get; set; }


            public override void UpdateText(bool recursive)
            {
                base.UpdateText(recursive);
                string str = "";

                if (Source == null)
                    str += Event.GetName(prefixDeclaringType);
                else
                    str += Source.GetName(prefixDeclaringType) + "." + Event.GetName(false);

                if (this.Text != str)
                    this.Text = str;

                if (Source == null)
                    this.StatusText = Event.GetName(false) + " - Token: " + Event.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(Event);
                else
                    this.StatusText = Source.GetName(false) + " - Token: " + Source.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(Source) + " with event " + Event.GetName(false) + " - Token: " + Event.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(Event);
            }

            internal override void OnAliasChanged(object obj, string alias)
            {
                if (obj is Type && (Event.EventHandlerType.GUID == ((Type)obj).GUID || (Source != null && Source.DeclaringType.GUID == ((Type)obj).GUID)))
                    UpdateText(false);
                else if (obj is EventInfo && (EventInfo)obj == Event)
                    UpdateText(false);
                else if (Source != null && (MemberInfo)obj == Source)
                    UpdateText(false);


                base.OnAliasChanged(obj, alias);
            }

            public override void Populate(string filterstring)
            {
            }

            public override string Visualization
            {
                get { return Text; }
            }

            public override MemberInfo Member
            {
                get { return Event; }
            }
        }


    }
}
