using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class FormsFlow : Form
    {
        private NodeControl.NodeDiagram nodeControl;

        public FormsFlow(IAssemblyBrowser browser)
        {
            InitializeComponent();
            nodeControl = new NodeControl.NodeDiagram();
            nodeControl.Dock = DockStyle.Fill;
            nodeControl.Factories.Clear();

            this.Controls.Add(nodeControl);

            pnlLoading.BringToFront();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Visible = true;
            TaskFactory f = new TaskFactory(1);
            var ui = WindowsFormsSynchronizationContext.Current;

            f.StartTask(() =>
            {
                DoAnalysis(nodeControl);
            }, () =>
            {
                ui.Send((c) =>
                {
                    pnlLoading.Visible = false;

                    foreach (var n in nodes)
                        nodeControl.Nodes.Add(n);

                    nodeControl.AutoLayout(false);

                    
                }, null);
            });

        }
        private void SetStatus(string msg)
        {
            lblStatus.BeginInvoke(new Action(() => lblStatus.Text = msg));
            Console.WriteLine(msg);
        }

        private void SetProgress(float progress)
        {
            pb.BeginInvoke(new Action(() => pb.Value = (int)(progress * 100)));
        }

        private List<NodeControl.Nodes.Node> nodes = new List<NodeControl.Nodes.Node>();

        private void DoAnalysis(NodeControl.NodeDiagram nodeControl)
        {


            var formsCache = AnalysisManager.Instance.GetTypeCache(typeof(System.Windows.Forms.Form));

            var allExistingFormTypes = formsCache.DerivedBy.ToArray();

            Dictionary<Type, NodeControl.Nodes.ConditionNode> nodePerType = new Dictionary<Type, NodeControl.Nodes.ConditionNode>();

            int count = 0;

            foreach (var formType in allExistingFormTypes)
            {
                NodeControl.Nodes.ConditionNode cn = new NodeControl.Nodes.ConditionNode(nodeControl);
                cn.Direction = NodeControl.Nodes.Node.DirectionEnum.Vertical;
                cn.Text = formType.Name;
                
                nodePerType[formType] = cn;
                nodes.Add(cn);
                SetStatus("Adding form " + formType.FullName);
                SetProgress((float)count++ / allExistingFormTypes.Length);
            }

            HashSet<MemberInfo> memberTracker = new HashSet<MemberInfo>();

            count = 0;
            foreach (var formType in allExistingFormTypes)
            {
                SetStatus("Scanning links for " + formType.FullName);

                foreach (var ctor in formType.GetConstructors())
                {
                    var memberCache = (MethodBaseCache)AnalysisManager.Instance.GetMemberCache(ctor);


                    foreach (var frmUsedBy in memberCache.UsedBy)
                    {
                        var member = frmUsedBy.Member;

                        Stack<string> path = new Stack<string>();
                        CheckMember(nodePerType, formType, member, memberTracker, path);
                    }
                }


                SetProgress((float)count++ / allExistingFormTypes.Length);
            }
        }

        private void CheckMember(Dictionary<Type, NodeControl.Nodes.ConditionNode> nodePerType, Type formType, MemberInfo member, HashSet<MemberInfo> memberTracker, Stack<string> path)
        {
            if (memberTracker.Contains(member))
                return;

            memberTracker.Add(member);


            path.Push(member.GetName(true));

            CheckForEventWire(nodePerType, formType, member, memberTracker, path);

            var usedByCache = (MethodBaseCache)AnalysisManager.Instance.GetMemberCache(member);
            foreach (var callee in usedByCache.CalledBy)
            {
                CheckMember(nodePerType, formType, callee, memberTracker, path);
            }

            path.Pop();
        }

        private void CheckForEventWire(Dictionary<Type, NodeControl.Nodes.ConditionNode> nodePerType, Type formType, MemberInfo member, HashSet<MemberInfo> memberTracker, Stack<string> path)
        {
            var usedByCache = (MethodBaseCache)AnalysisManager.Instance.GetMemberCache(member);
            foreach (var wire in usedByCache.WiredForEvent)
            {

                if (wire.Source != null)
                {

                    path.Push(wire.Source.GetName(false));

                    if (typeof(System.Windows.Forms.Control).IsAssignableFrom(wire.Source.DeclaringType) ||
                         typeof(System.Windows.Forms.MenuItem).IsAssignableFrom(wire.Source.DeclaringType))
                    {
                        var srcFrmType = FindOwnersOfControl(wire.Source);
                        if (srcFrmType != null)
                        {
                            if (typeof(System.Windows.Forms.Form).IsAssignableFrom(srcFrmType) && nodePerType.ContainsKey(srcFrmType))
                            {
                                string pathstr = string.Join(" -> ", path.ToArray());
                                SetStatus(string.Format("Found link between forms ({0} to {1}): {2}", nodePerType[srcFrmType].Text,nodePerType[formType].Text, pathstr));

                                if (nodePerType[srcFrmType].LinksTo.Where(l => l.LinksTo == nodePerType[formType]).FirstOrDefault() == null)
                                    nodePerType[srcFrmType].LinksTo.Add(new NodeControl.Nodes.Condition() { Text = path.Last(), LinksTo = nodePerType[formType] });
                            }
                            else
                            {
                                foreach (var ctor in srcFrmType.GetConstructors())
                                {
                                    CheckMember(nodePerType, formType, ctor, memberTracker, path);
                                }
                            }
                        }
                    }

                    path.Pop();

                }
            }
        }


        public Type FindOwnersOfControl(MemberInfo member)
        {
            var cache = AnalysisManager.Instance.GetMemberCache(member);

            foreach (var used in cache.UsedBy)
            {
                if (used.Member.Name == "InitializeComponent")
                {
                    return used.Member.DeclaringType;
                }
            }

            return null;
        }
    }
}
