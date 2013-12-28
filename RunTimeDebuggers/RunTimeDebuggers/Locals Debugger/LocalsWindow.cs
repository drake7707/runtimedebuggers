using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using RunTimeDebuggers.Properties;
using System.Runtime.CompilerServices;
using RunTimeDebuggers.Controls;
using RunTimeDebuggers.Helpers;


namespace RunTimeDebuggers.LocalsDebugger
{
    public partial class LocalsWindow : Form
    {
        private ExpressionBox exprBox;

        private object thisObject;

        private MemberNode rootNode;

        public LocalsWindow(string thisName, object obj)
        {
            this.thisObject = obj;

            InitializeComponent();

            if(!string.IsNullOrEmpty(thisName))
                Text = "Locals & Watches - " + thisName;

            tree.Columns.Add(new TreeListColumn("Name", "Name") { AutoSize = true });

            var valueCol = new TreeListColumn("Value", "Value") { AutoSize = true };
            valueCol.CellFormatConditions = new RunTimeDebuggers.Controls.TreeList.ConditionalFormatting[] {
             new RunTimeDebuggers.Controls.TreeList.ConditionalFormatting(n => n is ILocalsNode && ((ILocalsNode)n).Changed, 
                                                new RunTimeDebuggers.Controls.TreeList.TextFormatting(valueCol.CellFormat) { ForeColor = Color.Red })
            };

            tree.Columns.Add(valueCol);
            tree.Columns.Add(new TreeListColumn("Type", "Type") { AutoSize = true });

            FillImageListForMemberIcons(imgs);

            tree.Images = imgs;

            if (obj != null)
            {
                rootNode = new MemberNode(thisName + " (this)", obj, obj == null ? null : obj.GetType().ToSignatureString())
                {
                    Object = obj,
                    ObjectType = obj.GetType(),
                    Member = null
                };
                rootNode.HasChildren = true;
                rootNode.ImageId = (int)IconEnum.PublicField;
                rootNode.ExpandMembers(btnShowControls.Checked, btnShowFields.Checked, btnShowProperties.Checked);
                rootNode.Expand();
                tree.Nodes.Add(rootNode);
            }

            tree.RowOptions.ShowHeader = false;

            tree.NotifyBeforeExpand += new TreeListView.NotifyBeforeExpandHandler(tree_NotifyBeforeExpand);

            exprBox = new ExpressionBox(obj);
            exprBox.ExpressionEvaluated += new ExpressionBox.ExpressionEvaluatedHandler(exprBox_ExpressionEvaluated);
            exprBox.Dock = DockStyle.Fill;
            pnl.Controls.Add(exprBox);

            FillControlTree();

        }

        void exprBox_ExpressionEvaluated(object sender, ExpressionBox.EvaluateArgs args)
        {
            if (args.Result.ResultType == typeof(void))
            {
                MessageBox.Show("Expression is of type void and has been evaluated");
            }
            else
            {

                ExpressionNode n = new ExpressionNode(thisObject, args.Statement);

                tree.Nodes.Add(n);
                if (tree.Nodes.Count == 1)
                    tree.RecalcLayout();

                n.MakeVisible();
                n.HasChildren = true;
                n.ImageId = (int)IconEnum.EvaluatedStatement;
                tree.NodesSelection.Clear();
                tree.NodesSelection.Add(n);
                tree.FocusedNode = n;

                tree.Refresh();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            tree.Columns.RecalcVisibleColumsRect();
            tree.UpdateScrollBars();
        }

        public static void FillImageListForMemberIcons(ImageList imgs)
        {
            imgs.Images.AddRange(IconHelper.GetIcons().ToArray());
        }

        void tree_NotifyBeforeExpand(TLNode node, bool isExpanding)
        {
            ExpandMembersOfNode(node);
        }

        private void ExpandMembersOfNode(TLNode node)
        {
            if (node is MemberNode)
            {
                MemberNode n = (MemberNode)node;
                if (n.Nodes.Count == 0)
                {
                    n.ExpandMembers(btnShowControls.Checked, btnShowFields.Checked, btnShowProperties.Checked);
                }
            }
            else if (node is IEnumerableNode)
            {
                IEnumerableNode n = (IEnumerableNode)node;
                if (n.Nodes.Count == 0)
                {
                    n.ExpandMembers();
                }
            }
        }

        private void btnShowControls_Click(object sender, EventArgs e)
        {
            UpdateNodeMemberVisibility();
        }

        private void btnShowFields_Click(object sender, EventArgs e)
        {
            UpdateNodeMemberVisibility();
        }

        private void btnShowProperties_Click(object sender, EventArgs e)
        {
            UpdateNodeMemberVisibility();
        }

        private void UpdateNodeMemberVisibility()
        {
            foreach (TLNode n in tree.Nodes)
            {
                if (n is MemberNode)
                    ((MemberNode)n).ExpandMembers(btnShowControls.Checked, btnShowFields.Checked, btnShowProperties.Checked);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshValues();
        }

        private void RefreshValues()
        {
            foreach (TLNode n in tree.Nodes)
            {
                var memberNode = n as ILocalsNode;
                if (memberNode != null)
                {
                    memberNode.Evaluate();

                    foreach (TLNode subn in n.GetAllNodes())
                    {
                        if (subn is ILocalsNode)
                            ((ILocalsNode)subn).Evaluate();
                    }
                }
            }
            tree.Invalidate();
        }

        private void tree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var n = tree.FocusedNode;
                if (n is MemberNode && ((MemberNode)n).Member != null)
                    openMemberInAssemblyExplorerToolStripMenuItem.Enabled = true;
                else
                    openMemberInAssemblyExplorerToolStripMenuItem.Enabled = false;

                mnuLookupInAssemblyExplorer.Show(tree, e.X, e.Y);
            }
        }

        private void tmr_Tick(object sender, EventArgs e)
        {
            RefreshValues();
        }

        private void tree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (tree.NodesSelection.Count > 0)
                {
                    try
                    {
                        Clipboard.Clear();
                        var obj = tree.NodesSelection[0][1];
                        if (obj is Image)
                            Clipboard.SetImage((Image)obj);
                        else
                            Clipboard.SetText(obj + "");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Value could not be copied to the clipboard");
                    }
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (tree.NodesSelection.Count > 0 && tree.NodesSelection[0] is ExpressionNode)
                {
                    var nextNode = tree.NodesSelection[0].NextSibling;
                    //tree.NodesSelection[0].Remove();
                    tree.Nodes.Remove(tree.NodesSelection[0]);

                    if (nextNode != null)
                        tree.FocusedNode = nextNode;

                }
            }
        }

        private void openMemberInAssemblyExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var n = tree.FocusedNode;

            MemberInfo member = null;
            if (n is MemberNode)
                member = ((MemberNode)n).Member;
            else if (n is EventNode)
                member = ((EventNode)n).Event;

            if (member != null)
            {
                var explorer = AssemblyExplorer.AssemblyExplorer.Open();
                explorer.SelectMember(member);
            }
        }

        private void openReturnTypeInAssemblyExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var n = tree.FocusedNode;

            Type t = null;
            if (n is MemberNode)
            {
                var member = ((MemberNode)n).Member;
                if (member == null)
                    t = ((MemberNode)n).ObjectType;
                else
                    t = member.GetReturnType();
            }
            else if (n is EventNode)
                t = ((EventNode)n).Event.EventHandlerType;

            if (t != null)
            {
                var explorer = AssemblyExplorer.AssemblyExplorer.Open();
                explorer.SelectType(t);
            }
        }

        private void openValueAsMemberInAssemblyExploreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var n = tree.FocusedNode;

            object val;
            if (n is MemberNode)
            {
                val = ((MemberNode)n).Object;
                if (val is Type)
                {
                    var explorer = AssemblyExplorer.AssemblyExplorer.Open();
                    explorer.SelectType((Type)val);
                }
                else if (val is MemberInfo)
                {
                    var explorer = AssemblyExplorer.AssemblyExplorer.Open();
                    explorer.SelectMember((MemberInfo)val);
                }
                else
                    MessageBox.Show("The value is not a MemberInfo or Type instance");

            }
        }

        private void openValueInNewLocalsWatchesWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var n = tree.FocusedNode;
            if (n is MemberNode)
            {
                object val = ((MemberNode)n).Object;

                var member = ((MemberNode)n).Member;
                if (member != null)
                {
                    LocalsWindow dlg = new LocalsWindow(member.GetName(true), val);
                    dlg.FormClosed += (s, ev) => dlg.Dispose();
                    dlg.Show();
                }
            }
        }

        public void SetInput(string text)
        {
            exprBox.SetInput(text);
        }

        private void btnEvaluate_Click(object sender, EventArgs e)
        {
            tmr.Enabled = btnEvaluate.Checked;
        }

        private void changeAliasOfMemberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var n = tree.FocusedNode;

            MemberInfo member = null;
            if (n is MemberNode)
                member = ((MemberNode)n).Member;
            else if (n is EventNode)
                member = ((EventNode)n).Event;

            if (member != null)
            {
                bool ret = AliasManager.Instance.PromptAlias(this, member);
                if (ret)
                {
                    n[0] = member.GetName(false);
                    exprBox.UpdateIntellisense();
                }
            }
        }

        private void btnTogglePropertyGrid_Click(object sender, EventArgs e)
        {
            splitVert.Panel2Collapsed = !btnTogglePropertyGrid.Checked;
            propGrid.Visible = btnTogglePropertyGrid.Checked;
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tree.NodesSelection.Count > 0)
            {
                try
                {
                    var obj = tree.NodesSelection[0][1];
                    propGrid.SelectedObject = obj;
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void FillControlTree()
        {
            if (thisObject is Control)
            {
                Control c = (Control)thisObject;
                tvControlTree.Nodes.AddRange(GetNodesFromControls(c.Controls).ToArray());
            }
            tvControlTree.ExpandAll();
        }

        private IEnumerable<TreeNode> GetNodesFromControls(Control.ControlCollection cc)
        {
            foreach (Control c in cc)
            {
                var node = new ControlTreeNode(c);

                foreach (TreeNode subc in GetNodesFromControls(c.Controls))
                    node.Nodes.Add(subc);

                if (c is ToolStrip)
                {
                    foreach (TreeNode subc in GetNodesFromToolStrip(((ToolStrip)c).Items))
                    node.Nodes.Add(subc);
                    
                }
                    
                yield return node;
            }
        }

        private IEnumerable<TreeNode> GetNodesFromToolStrip(ToolStripItemCollection toolStripItemCollection)
        {
            foreach (ToolStripItem itm in toolStripItemCollection)
            {
                var node =new ControlTreeNode(itm);
                if (itm is ToolStripDropDownItem)
                {
                    foreach (TreeNode subc in GetNodesFromToolStrip(((ToolStripDropDownItem)itm).DropDownItems))
                        node.Nodes.Add(subc);
                }
                yield return node;
            }
        }

        private void tvControlTree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            ((ControlTreeNode)e.Node).Draw(tvControlTree, e);
        }

        private void tvControlTree_MouseDown(object sender, MouseEventArgs e)
        {
            var node = tvControlTree.GetNodeAt(0, e.Y);
            if (node != null)
                tvControlTree.SelectedNode = node;
        }

        private void openValueInNewLocalsWatchesWindowToolStripControlTreeMenuItem_Click(object sender, EventArgs e)
        {
            if (tvControlTree.SelectedNode != null)
            {
                var node = ((ControlTreeNode)tvControlTree.SelectedNode);
                object c = node.Object;
                LocalsWindow dlg = new LocalsWindow(node.GetName(), c);
                dlg.FormClosed += (s, ev) => dlg.Dispose();
                dlg.Show();
            }
        }

        private void tvControlTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                propGrid.SelectedObject = ((ControlTreeNode)e.Node).Object;
            }
        }

        private void propGrid_SelectedObjectsChanged(object sender, EventArgs e)
        {
            if (propGrid.SelectedObject is Control)
                PrepareRectangleAndShowBounds((Control)propGrid.SelectedObject);
        }

        private Panel[] controlHighlightRectangle = new Panel[] {
        
            new Panel() { BackColor = Color.Red },
            new Panel() { BackColor = Color.Red },
            new Panel() { BackColor = Color.Red },
            new Panel() { BackColor = Color.Red }
        };

        private Control controlHighlightRectangleContainer;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (controlHighlightRectangleContainer != null && !controlHighlightRectangleContainer.IsDisposed)
            {
                controlHighlightRectangleContainer.Controls.Remove(controlHighlightRectangle[0]);
                controlHighlightRectangleContainer.Controls.Remove(controlHighlightRectangle[1]);
                controlHighlightRectangleContainer.Controls.Remove(controlHighlightRectangle[2]);
                controlHighlightRectangleContainer.Controls.Remove(controlHighlightRectangle[3]);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();

                foreach (var r in controlHighlightRectangle)
                    r.Dispose();
            }

            base.Dispose(disposing);
        }


        private void PrepareRectangleAndShowBounds(Control c)
        {
            try
            {
                if (controlHighlightRectangleContainer != null && !controlHighlightRectangleContainer.IsDisposed)
                {
                    controlHighlightRectangleContainer.Controls.Remove(controlHighlightRectangle[0]);
                    controlHighlightRectangleContainer.Controls.Remove(controlHighlightRectangle[1]);
                    controlHighlightRectangleContainer.Controls.Remove(controlHighlightRectangle[2]);
                    controlHighlightRectangleContainer.Controls.Remove(controlHighlightRectangle[3]);
                }

                if (!c.IsDisposed && !c.Disposing)
                {
                    propGrid.SelectedObject = c;

                    var topLevelControl = c.TopLevelControl;
                    if (topLevelControl != null)
                    {
                        topLevelControl.Controls.Add(controlHighlightRectangle[0]);
                        topLevelControl.Controls.Add(controlHighlightRectangle[1]);
                        topLevelControl.Controls.Add(controlHighlightRectangle[2]);
                        topLevelControl.Controls.Add(controlHighlightRectangle[3]);

                        controlHighlightRectangle[0].Visible = true;
                        controlHighlightRectangle[1].Visible = true;
                        controlHighlightRectangle[2].Visible = true;
                        controlHighlightRectangle[3].Visible = true;
                        controlHighlightRectangle[0].BringToFront();
                        controlHighlightRectangle[1].BringToFront();
                        controlHighlightRectangle[2].BringToFront();
                        controlHighlightRectangle[3].BringToFront();
                    }

                    SetBoundsFromControl(c, 2);

                    controlHighlightRectangleContainer = topLevelControl;
                }
            }
            catch (Exception)
            {

            }
        }

        private void SetBoundsFromControl(Control c, int borderWidth)
        {
            var topLevelControl = c.TopLevelControl;

            if (topLevelControl != null)
            {
                Rectangle controlAreaRelativeToTopLevel = new Rectangle(
                    c.PointToScreen(new Point(0, 0)).X - topLevelControl.PointToScreen(new Point(0, 0)).X,
                    c.PointToScreen(new Point(0, 0)).Y - topLevelControl.PointToScreen(new Point(0, 0)).Y,
                    c.Width,
                    c.Height
                );

                // top
                controlHighlightRectangle[0].Location = new Point(controlAreaRelativeToTopLevel.Left, controlAreaRelativeToTopLevel.Top);
                controlHighlightRectangle[0].Size = new Size(c.Width, borderWidth);

                // left
                controlHighlightRectangle[1].Location = new Point(controlAreaRelativeToTopLevel.Left, controlAreaRelativeToTopLevel.Top);
                controlHighlightRectangle[1].Size = new Size(borderWidth, c.Height);

                // bottom
                controlHighlightRectangle[2].Location = new Point(controlAreaRelativeToTopLevel.Left, controlAreaRelativeToTopLevel.Top + c.Height - borderWidth);
                controlHighlightRectangle[2].Size = new Size(c.Width, borderWidth);

                // right
                controlHighlightRectangle[3].Location = new Point(controlAreaRelativeToTopLevel.Left + c.Width - borderWidth, controlAreaRelativeToTopLevel.Top);
                controlHighlightRectangle[3].Size = new Size(borderWidth, c.Height);
            }
        }

        

    }
}
