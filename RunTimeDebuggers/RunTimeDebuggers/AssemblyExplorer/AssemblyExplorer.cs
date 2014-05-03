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

using System.Reflection.Emit;
using RunTimeDebuggers.Controls;

namespace RunTimeDebuggers.AssemblyExplorer
{


    partial class AssemblyExplorer : Form, IAssemblyBrowser
    {

        private static AssemblyExplorer openAssemblyExplorer;

        public static AssemblyExplorer Open()
        {
            if (openAssemblyExplorer == null || openAssemblyExplorer.IsDisposed)
                openAssemblyExplorer = new AssemblyExplorer();

            openAssemblyExplorer.Show();
            openAssemblyExplorer.BringToFront();

            return openAssemblyExplorer;
        }

        private MemberMenu mnuMember;
        private CodePane codePane;

        public AssemblyExplorer()
        {
            InitializeComponent();
            mnuMember = new MemberMenu(this);
            codePane = new CodePane(this, mnuMember);
            codePane.Dock = DockStyle.Fill;
            rightSplit.Panel1.Controls.Add(codePane);

            LoadAnalysisCache();

            imgs.Images.AddRange(IconHelper.GetIcons().ToArray());

            var searchControl = new SearchTypeOrMember(this);
            searchControl.Dock = DockStyle.Fill;
            tpSearch.Controls.Add(searchControl);

            AliasManager.Instance.AliasChanged += new AliasManager.AliasChangedHandler(AliasManager_AliasChanged);
            ILDebugManager.Instance.DebuggerChanged += new EventHandler(ILDebugManager_DebuggerChanged);
            ILDebugManager.Instance.BreakpointHit += new EventHandler(ILDebugManager_BreakpointHit);
            FillAssemblies();

            showOnlyAliasesWhenPresentToolStripMenuItem_Click(showOnlyAliasesWhenPresentToolStripMenuItem, EventArgs.Empty);
        }

        void ILDebugManager_BreakpointHit(object sender, EventArgs e)
        {
            UpdateGUIForDebugger();
        }

        void ILDebugManager_DebuggerChanged(object sender, EventArgs e)
        {
            UpdateGUIForDebugger();
        }


        private void LoadAnalysisCache()
        {

            lblLoadingCache.Visible = true;
            findStringToolStripMenuItem.Enabled = false;
            mnuDecryptStrings.Enabled = false;
            autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem.Enabled = false;
            showFormsFlowToolStripMenuItem.Enabled = false;
            loadAssemblyToolStripMenuItem.Enabled = false;

            mnuMember.OnAnalysisStart();


            pbar.Visible = true;
            pbar.Style = ProgressBarStyle.Continuous;
            TaskFactory f = new TaskFactory(1, System.Threading.ThreadPriority.BelowNormal);
            var ui = WindowsFormsSynchronizationContext.Current;

            Timer tmr = new Timer();
            tmr.Tick += (s, e) =>
            {
                try
                {
                    if (pbar == null)
                        return;
                    pbar.Value = (int)(AnalysisManager.Instance.CacheBuildProgress * pbar.Maximum);
                }
                catch (Exception)
                { }
            };

            tmr.Enabled = true;

            f.StartTask(() =>
            {
                AnalysisManager.Instance.BuildCache();
            }, () =>
            {
                try
                {
                    ui.Send(o =>
                          {
                              tmr.Enabled = false;
                              tmr.Dispose();
                              var ae = ((AssemblyExplorer)o);
                              ae.lblLoadingCache.Visible = false;
                              ae.pbar.Visible = false;
                              ae.findStringToolStripMenuItem.Enabled = true;
                              ae.mnuDecryptStrings.Enabled = true;
                              ae.autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem.Enabled = true;
                              ae.showFormsFlowToolStripMenuItem.Enabled = true;
                              ae.loadAssemblyToolStripMenuItem.Enabled = true;
                              ae.mnuMember.OnAnalysisComplete();
                          }, this);
                }
                catch (Exception)
                {
                }
            });
        }

        void AliasManager_AliasChanged(object obj, string alias)
        {
            foreach (var n in tvNodes.Nodes)
            {
                if (n is AbstractAssemblyNode)
                    ((AbstractAssemblyNode)n).OnAliasChanged(obj, alias);
            }

            if (tvNodes.SelectedNode is AbstractAssemblyNode)
                codePane.Node = (AbstractAssemblyNode)tvNodes.SelectedNode;
        }

        protected override void OnClosed(EventArgs e)
        {
            AliasManager.Instance.AliasChanged -= new AliasManager.AliasChangedHandler(AliasManager_AliasChanged);
            ILDebugManager.Instance.DebuggerChanged -= new EventHandler(ILDebugManager_DebuggerChanged);
            ILDebugManager.Instance.BreakpointHit -= new EventHandler(ILDebugManager_BreakpointHit);


            if (openAssemblyExplorer == this)
                openAssemblyExplorer = null;

            base.OnClosed(e);
        }

        public void FillAssemblies()
        {

            TaskFactory tasks = new TaskFactory(4);

            var ui = WindowsFormsSynchronizationContext.Current;
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {

                AssemblyNode an = new AssemblyNode(a);
                tvNodes.Nodes.Add(an);
            }
        }

        public void SelectType(Type t)
        {
            if (t == null)
                return;

            var assemblyNode = GetNodes().Where(n => n is AssemblyNode && ((AssemblyNode)n).Assembly == t.Assembly).FirstOrDefault();
            if (assemblyNode == null)
                return;

            assemblyNode.Populate(null);

            var namespaceNode = assemblyNode.GetNodes().Where(n => n is NamespaceNode && ((NamespaceNode)n).Namespace == t.Namespace).FirstOrDefault();
            if (namespaceNode == null)
                return;

            namespaceNode.Populate(null);


            AbstractAssemblyNode typeNode = namespaceNode;
            List<Type> pathToType = GetPathFromNonNestedTypeToType(t);
            foreach (var pathType in pathToType)
            {
                typeNode = typeNode.GetNodes().Where(n => n is TypeNode && ((TypeNode)n).Type.GUID == pathType.GUID).FirstOrDefault();

                if (typeNode == null)
                    return;
                typeNode.Populate(null);
            }

            typeNode.EnsureVisible();
            tvNodes.SelectedNode = typeNode;


        }

        private List<Type> GetPathFromNonNestedTypeToType(Type nestedType)
        {
            if (!nestedType.IsNested)
                return new List<Type>() { nestedType };

            Stack<Type> path = new Stack<Type>();
            Type cur = nestedType;
            while (cur != null && cur.IsNested)
            {
                path.Push(cur);
                cur = cur.DeclaringType;
            }
            if (cur != null && !cur.IsNested)
                path.Push(cur);

            return path.ToList();
        }

        public void SelectMember(MemberInfo member, int offset = -1)
        {
            Type t = member.DeclaringType;


            if (t == null)
                System.Diagnostics.Debug.Assert(t != null, "Declaring type is null");

            var assemblyNode = GetNodes().Where(n => n is AssemblyNode && ((AssemblyNode)n).Assembly == t.Assembly).FirstOrDefault();
            if (assemblyNode == null)
                return;

            assemblyNode.Populate(null);

            var namespaceNode = assemblyNode.GetNodes().Where(n => n is NamespaceNode && ((NamespaceNode)n).Namespace == t.Namespace).FirstOrDefault();
            if (namespaceNode == null)
                return;

            namespaceNode.Populate(null);

            AbstractAssemblyNode typeNode = namespaceNode;
            List<Type> pathToType = GetPathFromNonNestedTypeToType(t);
            foreach (var pathType in pathToType)
            {
                typeNode = typeNode.GetNodes().Where(n => n is TypeNode && ((TypeNode)n).Type.GUID == pathType.GUID).FirstOrDefault();

                if (typeNode == null)
                    return;
                typeNode.Populate(null);

            }


            var memberNode = typeNode.GetNodes().Where(n => n is MemberNode && ((MemberNode)n).Member.IsEqual(member))
                                                .FirstOrDefault();

            if (memberNode == null)
            {
                var propNode = typeNode.GetNodes().Where(n =>
                                                    {
                                                        if (n is PropertyNode)
                                                        {
                                                            var pNode = ((PropertyNode)n);
                                                            var getMethod = pNode.Property.GetGetMethod(true);
                                                            if (getMethod != null && member.IsEqual(getMethod))
                                                                return true;
                                                            var setMethod = pNode.Property.GetSetMethod(true);
                                                            if (setMethod != null && member.IsEqual(setMethod))
                                                                return true;
                                                        }
                                                        return false;
                                                    }).FirstOrDefault();

                if (propNode == null)
                    return;

                propNode.Populate(null);

                memberNode = propNode.GetNodes().Where(n => n is MemberNode && ((MemberNode)n).Member.IsEqual(member))
                                                .FirstOrDefault();

                if (memberNode == null)
                    return;
            }

            memberNode.EnsureVisible();

            tvNodes.SelectedNode = memberNode;
            if (offset != -1)
                codePane.ScrollToInstruction(offset);
        }

        private IEnumerable<AbstractAssemblyNode> GetNodes()
        {
            foreach (TreeNode n in tvNodes.Nodes)
            {
                if (n is AbstractAssemblyNode)
                    yield return (AbstractAssemblyNode)n;
            }
        }

        private void tvNodes_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            ((AbstractAssemblyNode)e.Node).Populate(null);
        }

        private void tvNodes_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node is AbstractAssemblyNode)
            {
                codePane.Node = (AbstractAssemblyNode)e.Node;
                UpdateNavigationButtons();

                tabs.SelectedIndex = 0;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            tvNodes.GoBack();
            UpdateNavigationButtons();
            tvNodes.Focus();
        }

        private void UpdateNavigationButtons()
        {
            btnBack.Enabled = tvNodes.HasHistory;
            btnForward.Enabled = tvNodes.HasForwardHistory;
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            tvNodes.GoForward();
            UpdateNavigationButtons();
            tvNodes.Focus();

        }

        private void Treeview_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {


            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ((TreeView)sender).SelectedNode = e.Node;
                var n = e.Node;
                if (n is AbstractAssemblyNode)
                    ((IAssemblyBrowser)this).OnNodeRightClicked((AbstractAssemblyNode)n, new Point(e.X, e.Y), true);
            }
        }

        private MethodBase GetCurrentSelectedMethod()
        {
            if (tvNodes.SelectedNode is MethodNode)
                return ((MethodNode)tvNodes.SelectedNode).Method;
            else if (tvNodes.SelectedNode is ConstructorNode)
                return ((ConstructorNode)tvNodes.SelectedNode).Constructor;

            return null;
        }

        private void rightTabs_TabAdded(object sender, EventArgs e)
        {
            rightSplit.Panel2Collapsed = (rightTabs.TabCount == 0);
        }
        private void rightTabs_TabRemoved(object sender, EventArgs e)
        {
            rightSplit.Panel2Collapsed = (rightTabs.TabCount == 0);
        }


        void IAssemblyBrowser.OnNodeRightClicked(AbstractAssemblyNode n, Point pos, bool isFromBrowser)
        {
            if (n is MemberNode)
            {
                mnuMember.Show(Cursor.Position);
                mnuMember.Tag = ((MemberNode)n).Member;
            }
            else if (n is TypeNode)
            {
                mnuMember.Show(Cursor.Position);
                mnuMember.Tag = ((TypeNode)n).Type;
            }
            else if (n is BaseTypeNode)
            {
                mnuMember.Show(Cursor.Position);
                mnuMember.Tag = ((BaseTypeNode)n).Type;
            }
            mnuMember.UpdateAnalyzeTypeMenuEnabledStatus(isFromBrowser);
        }



        private void showCallStackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var callStackControl = new CallStack(this, new System.Diagnostics.StackTrace());
            rightTabs.AddTab(callStackControl, "Call stack");
        }

        private void findStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var findStringControl = new FindString(this);
            rightTabs.AddTab(findStringControl, "Find string");
        }


        private void mnuDecryptStrings_Click(object sender, EventArgs e)
        {
            if (mnuMember.StringDecryptionMethod == null)
            {
                MessageBox.Show("There is no method set to use for the string decryption. Find the static method that is used to decrypt the strings in the code (usually just after the loaded encrypted string) and set it to use for decrypting strings (by right clicking on it)");
                return;
            }

            var decyptStringControl = new DecryptStrings(this, mnuMember.StringDecryptionMethod);
            rightTabs.AddTab(decyptStringControl, "Decrypt strings");
        }

        private void aliasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aliasesControl = new Aliases(this);
            rightTabs.AddTab(aliasesControl, "Aliases");
        }

        private void bookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bookMarksControl = new Bookmarks(this);
            rightTabs.AddTab(bookMarksControl, "Bookmarks");
        }


        public ImageList GetNodeImageList()
        {
            return imgs;
        }

        private void tvNodes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Node is BaseTypeNode)
                    SelectType(((BaseTypeNode)e.Node).Type);
            }
        }

        private void tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabs.SelectedIndex >= 0 && tabs.TabPages[tabs.SelectedIndex].Controls.Count > 0)
                tabs.TabPages[tabs.SelectedIndex].Controls[0].Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F))
            {
                tabs.SelectedIndex = 1;
                tabs_SelectedIndexChanged(tabs, EventArgs.Empty);
                return true;
            }
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }

        private void loadAssemblyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "*.dll;*.exe|*.dll;*.exe";
                    if (ofd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        Assembly a = Assembly.LoadFile(ofd.FileName);

                        AssemblyNode an = new AssemblyNode(a);
                        tvNodes.Nodes.Add(an);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading assembly: " + ex.GetType().FullName + " - " + ex.Message);
            }
        }

        private void dumpResourcesOfSelectedAssemblyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selNode = tvNodes.SelectedNode;

            TreeNode n = selNode;
            while (n != null && n.Parent != null)
                n = n.Parent;

            if (n is AssemblyNode)
            {
                Assembly a = ((AssemblyNode)n).Assembly;

                try
                {
                    using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                    {
                        dlg.Description = "Save resources of the '" + a.FullName + "' assembly";

                        if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            var invalidChars = System.IO.Path.GetInvalidPathChars();
                            foreach (var resourceName in a.GetManifestResourceNames())
                            {
                                using (System.IO.Stream s = a.GetManifestResourceStream(resourceName))
                                {
                                    string cleanName = new string(resourceName.Select(c => (invalidChars.Contains(c) ? '_' : c)).ToArray());
                                    string path = System.IO.Path.Combine(dlg.SelectedPath, cleanName);
                                    System.IO.File.WriteAllBytes(path, s.ReadToEnd());
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save resource: " + ex.GetType().FullName + " - " + ex.Message);
                }
            }
        }

        private void dumpAssemblyWIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selNode = tvNodes.SelectedNode;

            TreeNode n = selNode;
            while (n != null && n.Parent != null)
                n = n.Parent;

            if (n is AssemblyNode)
            {
                Assembly a = ((AssemblyNode)n).Assembly;

                try
                {
                    using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                    {
                        dlg.Description = "Dump assembly to";

                        if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            DumpManager mgr = new DumpManager();
                            mgr.Dump(a, dlg.SelectedPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to dump assembly: " + ex.GetType().FullName + " - " + ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
        }

        public void AddTab(Control c, string tabName)
        {
            rightTabs.AddTab(c, tabName);
        }


        public void GoBack()
        {
            btnBack_Click(btnBack, EventArgs.Empty);
        }

        public void GoForward()
        {
            btnForward_Click(btnForward, EventArgs.Empty);
        }



        private void mnuRunDebugger_Click(object sender, EventArgs e)
        {
            try
            {
                if (ILDebugManager.Instance.Debugger != null)
                {
                    ILDebugManager.Instance.Run();

                    UpdateGUIForDebugger();
                }
                else
                    MessageBox.Show("No active debugger present, evaluate a statement with Ctrl+Enter in the locals window to run it through the interpreter.");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().FullName + " -  " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private MethodBase oldMethod;
        private int oldOffset;

        private void mnuStepInto_Click(object sender, EventArgs e)
        {
            try
            {
                if (ILDebugManager.Instance.Debugger != null)
                {
                    ILDebugManager.Instance.StepInto();

                    UpdateGUIForDebugger();
                }
                else
                    MessageBox.Show("No active debugger present, evaluate a statement with Ctrl+Enter in the locals window to run it through the interpreter.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.GetType().FullName + " -  " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void UpdateGUIForDebugger()
        {
            if (ILDebugManager.Instance.Debugger == null)
            {
                codePane.Invalidate(true);
            }
            else
            {
                if (ILDebugManager.Instance.Debugger.CurrentMethod != oldMethod)
                {
                    SelectMember(ILDebugManager.Instance.Debugger.CurrentMethod);
                    oldMethod = ILDebugManager.Instance.Debugger.CurrentMethod;
                    codePane.Invalidate(true);
                }
                if (ILDebugManager.Instance.Debugger.Returned)
                {
                    codePane.Invalidate(true);
                    codePane.FocusOnActiveLine();
                }
                else
                {
                    if (ILDebugManager.Instance.Debugger.CurrentInstruction.Offset != oldOffset)
                    {
                        codePane.Invalidate(true);
                        codePane.FocusOnActiveLine();
                        oldOffset = ILDebugManager.Instance.Debugger.CurrentInstruction.Offset;
                    }
                }
            }
        }

        private void debuggerStackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var debuggerStackControl = new DebuggerStack(this);
            rightTabs.AddTab(debuggerStackControl, "Debugger stack");

        }

        private void debuggerLocalsArgumentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var debuggerStackControl = new DebuggerVariables(this);
            rightTabs.AddTab(debuggerStackControl, "Debugger variables");
        }

        private void loadAliasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "*.xml|*.xml";
                if (ofd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    AliasManager.Instance.LoadAliases(ofd.FileName);
                }
            }
        }

        private void saveAliasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "*.xml|*.xml";
                if (sfd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    AliasManager.Instance.SaveAliases(sfd.FileName);
                }
            }
        }

        private void autoAliasAllMembersOfCurrentAssemblyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selNode = tvNodes.SelectedNode;

            TreeNode n = selNode;
            while (n != null && n.Parent != null)
                n = n.Parent;

            if (n is AssemblyNode)
            {

                Assembly a = ((AssemblyNode)n).Assembly;

                var yesno = MessageBox.Show("Include all public members?", "Include public members", MessageBoxButtons.YesNoCancel);
                if (yesno == System.Windows.Forms.DialogResult.Cancel)
                {
                    // cancelled
                }
                else
                    AliasManager.Instance.AutoAliasAssembly(a, yesno == System.Windows.Forms.DialogResult.No);
            }
        }


        public void SetStatusText(string str)
        {
            lblStatus.Text = str;
        }

        private void tvNodes_MouseMove(object sender, MouseEventArgs e)
        {
            var n = tvNodes.GetNodeAt(e.X, e.Y);

            if (n is AbstractAssemblyNode)
                SetStatusText(((AbstractAssemblyNode)n).StatusText);
            else
                SetStatusText("");
        }

        private void showOnlyAliasesWhenPresentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AliasManager.Instance.HideNameIfAliasIsPresent = showOnlyAliasesWhenPresentToolStripMenuItem.Checked;
        }

        private void showFormsFlowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormsFlow dlg = new FormsFlow(this))
            {
                dlg.ShowDialog(this);
            }
        }


    }
}
