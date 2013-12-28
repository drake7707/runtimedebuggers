using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;
using System.Drawing;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class TreeNodeControl : UserControl
    {
        protected IAssemblyBrowser browser;
        private TreeView tv;


        public TreeNodeControl(IAssemblyBrowser browser)
        {
            this.browser = browser;
        }

        public void Initialize(TreeView tvNodes)
        {
            this.tv = tvNodes;
            tvNodes.ImageList = browser.GetNodeImageList();

            tvNodes.AfterSelect += new TreeViewEventHandler(tvNodes_AfterSelect);
            tvNodes.NodeMouseClick += new TreeNodeMouseClickEventHandler(tvNodes_NodeMouseClick);
            tvNodes.MouseMove += new MouseEventHandler(tvNodes_MouseMove);
            tvNodes.KeyDown += new KeyEventHandler(tvNodes_KeyDown);
            
            AliasManager.Instance.AliasChanged += new AliasManager.AliasChangedHandler(AliasManager_AliasChanged);

        }



        void tvNodes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                try
                {
                    if (tv.SelectedNode != null)
                        Clipboard.SetText(tv.SelectedNode.Text);

                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to copy the node text to the clipboard, error: " + ex.GetType().FullName + " - " + ex.Message);
                }
            }
        }


        private IEnumerable<TreeNode> GetAllNodes()
        {
            foreach (TreeNode n in tv.Nodes)
            {
                yield return n;
                foreach (TreeNode subn in GetAllNodes(n))
                    yield return subn;
            }
        }

        private IEnumerable<TreeNode> GetAllNodes(TreeNode n)
        {
            foreach (TreeNode subn in n.Nodes)
            {
                yield return subn;
                foreach (TreeNode subsubn in GetAllNodes(subn))
                    yield return subsubn;
            }
        }

        void tvNodes_MouseMove(object sender, MouseEventArgs e)
        {
            var n = tv.GetNodeAt(e.X, e.Y);

            if (n is AbstractAssemblyNode)
                browser.SetStatusText(((AbstractAssemblyNode)n).StatusText);
            else
                browser.SetStatusText("");

            bool changed = false;
            foreach (TreeNode subn in GetAllNodes())
            {
                if (subn.IsVisible)
                {
                    if (subn.ForeColor != Color.Black && subn != n)
                    {
                        changed = true;
                        subn.ForeColor = Color.Black;
                        subn.NodeFont = null;
                    }
                }
            }

            if ((ModifierKeys & Keys.Control) == Keys.Control && (n is AbstractAssemblyNode))
            {
                if (n != null && n.ForeColor != Color.Blue)
                {
                    changed = true;
                    n.ForeColor = Color.Blue;
                    n.NodeFont = new Font(tv.Font, FontStyle.Underline);
                    if (Cursor != Cursors.Hand)
                        Cursor = Cursors.Hand;
                }
            }
            else
            {
                if (n != null && n.ForeColor != Color.Black)
                {
                    changed = true;
                    n.ForeColor = Color.Black;
                    n.NodeFont = null;
                }

                if (Cursor != Cursors.Arrow)
                    Cursor = Cursors.Arrow;
            }

            if (changed)
                tv.Invalidate();
        }


        void tvNodes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectInBrowser();
        }

        private void SelectInBrowser()
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                var selNode = tv.SelectedNode;
                if (selNode is MemberNode)
                {
                    // contains offset
                    if (selNode.Tag != null)
                        browser.SelectMember(((MemberNode)selNode).Member, (int)selNode.Tag);
                    else
                        browser.SelectMember(((MemberNode)selNode).Member);
                }
                else if (selNode is TypeNode)
                    browser.SelectType(((TypeNode)selNode).Type);
            }
        }

        protected virtual void AliasManager_AliasChanged(object obj, string alias)
        {
            foreach (var n in tv.Nodes)
            {
                if (n is AbstractAssemblyNode)
                    ((AbstractAssemblyNode)n).OnAliasChanged(obj, alias);
            }
        }

        private void tvNodes_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ((TreeView)sender).SelectedNode = e.Node;
                var n = e.Node;
                if (n is AbstractAssemblyNode)
                    browser.OnNodeRightClicked((AbstractAssemblyNode)n, new Point(e.X, e.Y), false);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                SelectInBrowser();
            }

        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                AliasManager.Instance.AliasChanged -= new AliasManager.AliasChangedHandler(AliasManager_AliasChanged);

            base.Dispose(disposing);
        }

    }
}
