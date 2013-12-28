using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class MemberMenu : ContextMenuStrip
    {

        private IAssemblyBrowser browser;
        public MemberMenu(IAssemblyBrowser browser)
        {
            this.browser = browser;

            InitializeComponents();
        }

        private void gotoMemberTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var member = Tag;
            if (member is Type)
                browser.SelectType((Type)member);
            else if (member is MemberInfo)
                browser.SelectType(((MemberInfo)member).GetReturnType());
        }

        public void OnAnalysisStart()
        {
            analyzeToolStripMenuItem.Enabled = false;
            openCallGraphToolStripMenuItem.Enabled = false;
        }

        public void OnAnalysisComplete()
        {
            analyzeToolStripMenuItem.Enabled = true;
            openCallGraphToolStripMenuItem.Enabled = true;
        }


        private void gotoMemberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var member = Tag;
            if (member is MemberInfo)
                browser.SelectMember((MemberInfo)member);
        }

        private void openValueInLocalsWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var member = Tag;

                if (member is FieldInfo)
                {
                    object value = ((FieldInfo)member).GetValue(null);
                    LocalsDebugger.LocalsWindow dlg = new LocalsDebugger.LocalsWindow(((FieldInfo)member).GetName(true), value);
                    dlg.FormClosed += (s, ev) => dlg.Dispose();
                    dlg.Show(this);
                }
                else if (member is PropertyInfo)
                {
                    object value = ((PropertyInfo)member).GetValue(null, null);
                    LocalsDebugger.LocalsWindow dlg = new LocalsDebugger.LocalsWindow(((PropertyInfo)member).GetName(true), value);
                    dlg.FormClosed += (s, ev) => dlg.Dispose();
                    dlg.Show(this);
                }
                else if (member is InstanceFinderManager.InstanceResult)
                {
                    var result = ((InstanceFinderManager.InstanceResult)member);
                    object value = result.Instance;
                    LocalsDebugger.LocalsWindow dlg = new LocalsDebugger.LocalsWindow(result.Origin.GetName(true), value);
                    dlg.FormClosed += (s, ev) => dlg.Dispose();
                    dlg.Show(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to evaluate member: " + ex.ToExceptionString());
            }
        }

        private void assignremoveAliasToSelectedMemberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = Tag;
            AliasManager.Instance.PromptAlias(this, obj);
        }

        private void analyzeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = Tag;
            if (obj is Type)
            {
                TypeAnalysis ma = new TypeAnalysis(browser, AnalysisManager.Instance, (Type)obj);
                browser.AddTab(ma, "Analysis: " + ((Type)obj).ToSignatureString());
            }
            else if (obj is MemberInfo)
            {
                MemberAnalysis ma = new MemberAnalysis(browser, AnalysisManager.Instance, (MemberInfo)obj);
                browser.AddTab(ma, "Analysis: " + ((MemberInfo)obj).GetName(true));
            }

        }

        public void UpdateAnalyzeTypeMenuEnabledStatus(bool isFromBrowser)
        {
            gotoMemberToolStripMenuItem.Visible = !isFromBrowser;

            if (Tag is Type)
            {
                openValueInLocalsWindowToolStripMenuItem.Enabled = false;
                gotoMemberTypeToolStripMenuItem.Enabled = true;
                useStaticMemberAsBaseInLocalsWindowToolStripMenuItem.Enabled = false;
                findAllInstancesToolStripMenuItem.Enabled = true;
            }
            else if (Tag is MemberInfo)
            {
                var isStatic = ((MemberInfo)Tag).IsStatic();
                openValueInLocalsWindowToolStripMenuItem.Enabled = isStatic;
                useStaticMemberAsBaseInLocalsWindowToolStripMenuItem.Enabled = isStatic;
                gotoMemberTypeToolStripMenuItem.Enabled = true;
                findAllInstancesToolStripMenuItem.Enabled = false;

                if (object.Equals(Tag, StringDecryptionMethod))
                    useAsStringDecryptMethodToolStripMenuItem.Checked = true;
                else
                    useAsStringDecryptMethodToolStripMenuItem.Checked = false;

            }
            else if (Tag is InstanceFinderManager.InstanceResult)
            {
                openValueInLocalsWindowToolStripMenuItem.Enabled = true;
            }
        }



        private void findAllInstancesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(Tag is Type))
                return;

            var type = (Type)Tag;
            Instances inst = new Instances(browser, type);
            browser.AddTab(inst, "Instances of " + type.ToSignatureString());
        }

        private void useStaticMemberAsBaseInLocalsWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(Tag is MemberInfo))
                return;

            var member = (MemberInfo)Tag;

            LocalsDebugger.LocalsWindow dlg = new LocalsDebugger.LocalsWindow("", null);
            dlg.Show();
            dlg.FormClosed += (s, ev) => dlg.Dispose();

            if (member is Type)
                dlg.SetInput(member.ToSignatureString());
            else
            {

                string typeName;
                if(member.DeclaringType != null) {
                typeName = AliasManager.Instance.GetAlias(member.DeclaringType);
                if(string.IsNullOrEmpty(typeName))
                    typeName = member.DeclaringType.Namespace + "." + member.DeclaringType.Name;
                }
                else
                    typeName = "";

                string memberName =  AliasManager.Instance.GetAlias(member);
                if(string.IsNullOrEmpty(memberName))
                    memberName = member.Name;

                dlg.SetInput((string.IsNullOrEmpty(typeName) ? "" : typeName + ".") + memberName);
            }
        }

        private void addAsBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = Tag;
            BookmarkManager.Instance.AddBookmark(obj);
        }

        private void useAsStringDecryptMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (useAsStringDecryptMethodToolStripMenuItem.Checked)
            {
                var obj = Tag;
                if (obj is MethodInfo)
                {
                    MethodInfo m = (MethodInfo)obj;
                    if (m.ReturnType != typeof(string))
                    {
                        MessageBox.Show("Return type of selected method is not a string");
                        return;
                    }

                    if (!m.IsStatic)
                    {
                        MessageBox.Show("The method is not static");
                        return;
                    }

                    StringDecryptionMethod = m;
                }
                else
                    MessageBox.Show("Selected member is not a method");
            }
            else
                StringDecryptionMethod = null;
        }

        private void decryptStringsInSelectedMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = Tag;
            if (obj is MethodBase)
            {

                var decryptionMethod = StringDecryptionMethod;
                if (decryptionMethod == null)
                {
                    MessageBox.Show("No string decryption method set!");
                    return;
                }

                var decyptStringControl = new DecryptStrings(browser, decryptionMethod, ((MethodBase)obj));
                browser.AddTab(decyptStringControl, "Decrypted strings in " + ((MethodBase)obj).GetName(true));
            }
            else
            {
                MessageBox.Show("The selected member is not a method or constructor");
            }
        }

        private void openCallGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = Tag;
            if (obj is MethodBase)
            {
                CallGraph cg = new CallGraph(browser, (MethodBase)obj);
                cg.FormClosed += (s, ev) => cg.Dispose();
                cg.Show(this);
            }
            else
            {
                MessageBox.Show("The selected member is not a method or constructor");
            }
        }

        public MethodInfo StringDecryptionMethod { get; private set; }


        private void InitializeComponents()
        {
            this.analyzeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findAllInstancesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.gotoMemberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoMemberTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.openValueInLocalsWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useStaticMemberAsBaseInLocalsWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.assignremoveAliasToSelectedMemberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addAsBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.useAsStringDecryptMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decryptStringsInSelectedMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.openCallGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.analyzeToolStripMenuItem,
            this.findAllInstancesToolStripMenuItem,
            this.toolStripSeparator3,
            this.gotoMemberToolStripMenuItem,
            this.gotoMemberTypeToolStripMenuItem,
            this.toolStripMenuItem3,
            this.openValueInLocalsWindowToolStripMenuItem,
            this.useStaticMemberAsBaseInLocalsWindowToolStripMenuItem,
            this.toolStripMenuItem1,
            this.assignremoveAliasToSelectedMemberToolStripMenuItem,
            this.addAsBookmarkToolStripMenuItem,
            this.toolStripMenuItem4,
            this.useAsStringDecryptMethodToolStripMenuItem,
            this.decryptStringsInSelectedMethodToolStripMenuItem,
            this.toolStripMenuItem5,
            this.openCallGraphToolStripMenuItem});
            Name = "mnuAnalyzeType";
            Size = new System.Drawing.Size(305, 298);

            // 
            // analyzeToolStripMenuItem
            // 
            this.analyzeToolStripMenuItem.Name = "analyzeToolStripMenuItem";
            this.analyzeToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.analyzeToolStripMenuItem.Text = "Analyze";
            this.analyzeToolStripMenuItem.Click += new System.EventHandler(this.analyzeToolStripMenuItem_Click);
            // 
            // findAllInstancesToolStripMenuItem
            // 
            this.findAllInstancesToolStripMenuItem.Name = "findAllInstancesToolStripMenuItem";
            this.findAllInstancesToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.findAllInstancesToolStripMenuItem.Text = "Find all instances";
            this.findAllInstancesToolStripMenuItem.Click += new System.EventHandler(this.findAllInstancesToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(301, 6);
            // 
            // gotoMemberToolStripMenuItem
            // 
            this.gotoMemberToolStripMenuItem.Name = "gotoMemberToolStripMenuItem";
            this.gotoMemberToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Click";
            this.gotoMemberToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.gotoMemberToolStripMenuItem.Text = "Goto member";
            this.gotoMemberToolStripMenuItem.Click += new System.EventHandler(this.gotoMemberToolStripMenuItem_Click);
            // 
            // gotoMemberTypeToolStripMenuItem
            // 
            this.gotoMemberTypeToolStripMenuItem.Name = "gotoMemberTypeToolStripMenuItem";
            this.gotoMemberTypeToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.gotoMemberTypeToolStripMenuItem.Text = "Goto member type";
            this.gotoMemberTypeToolStripMenuItem.Click += new System.EventHandler(this.gotoMemberTypeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(301, 6);
            // 
            // openValueInLocalsWindowToolStripMenuItem
            // 
            this.openValueInLocalsWindowToolStripMenuItem.Name = "openValueInLocalsWindowToolStripMenuItem";
            this.openValueInLocalsWindowToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.openValueInLocalsWindowToolStripMenuItem.Text = "Open value from member in locals window";
            this.openValueInLocalsWindowToolStripMenuItem.Click += new System.EventHandler(this.openValueInLocalsWindowToolStripMenuItem_Click);
            // 
            // useStaticMemberAsBaseInLocalsWindowToolStripMenuItem
            // 
            this.useStaticMemberAsBaseInLocalsWindowToolStripMenuItem.Name = "useStaticMemberAsBaseInLocalsWindowToolStripMenuItem";
            this.useStaticMemberAsBaseInLocalsWindowToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.useStaticMemberAsBaseInLocalsWindowToolStripMenuItem.Text = "Use static member as base in locals window";
            this.useStaticMemberAsBaseInLocalsWindowToolStripMenuItem.Click += new System.EventHandler(this.useStaticMemberAsBaseInLocalsWindowToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(301, 6);
            // 
            // assignremoveAliasToSelectedMemberToolStripMenuItem
            // 
            this.assignremoveAliasToSelectedMemberToolStripMenuItem.Name = "assignremoveAliasToSelectedMemberToolStripMenuItem";
            this.assignremoveAliasToSelectedMemberToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.assignremoveAliasToSelectedMemberToolStripMenuItem.Text = "Change alias";
            this.assignremoveAliasToSelectedMemberToolStripMenuItem.Click += new System.EventHandler(this.assignremoveAliasToSelectedMemberToolStripMenuItem_Click);
            // 
            // addAsBookmarkToolStripMenuItem
            // 
            this.addAsBookmarkToolStripMenuItem.Name = "addAsBookmarkToolStripMenuItem";
            this.addAsBookmarkToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.addAsBookmarkToolStripMenuItem.Text = "Add as bookmark";
            this.addAsBookmarkToolStripMenuItem.Click += new System.EventHandler(this.addAsBookmarkToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(301, 6);
            // 
            // useAsStringDecryptMethodToolStripMenuItem
            // 
            this.useAsStringDecryptMethodToolStripMenuItem.CheckOnClick = true;
            this.useAsStringDecryptMethodToolStripMenuItem.Name = "useAsStringDecryptMethodToolStripMenuItem";
            this.useAsStringDecryptMethodToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.useAsStringDecryptMethodToolStripMenuItem.Text = "Use as string decrypt method";
            this.useAsStringDecryptMethodToolStripMenuItem.Click += new System.EventHandler(this.useAsStringDecryptMethodToolStripMenuItem_Click);
            // 
            // decryptStringsInSelectedMethodToolStripMenuItem
            // 
            this.decryptStringsInSelectedMethodToolStripMenuItem.Name = "decryptStringsInSelectedMethodToolStripMenuItem";
            this.decryptStringsInSelectedMethodToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.decryptStringsInSelectedMethodToolStripMenuItem.Text = "Decrypt strings in selected method";
            this.decryptStringsInSelectedMethodToolStripMenuItem.Click += new System.EventHandler(this.decryptStringsInSelectedMethodToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(301, 6);
            // 
            // openCallGraphToolStripMenuItem
            // 
            this.openCallGraphToolStripMenuItem.Name = "openCallGraphToolStripMenuItem";
            this.openCallGraphToolStripMenuItem.Size = new System.Drawing.Size(304, 22);
            this.openCallGraphToolStripMenuItem.Text = "Open call graph";
            this.openCallGraphToolStripMenuItem.Click += new System.EventHandler(this.openCallGraphToolStripMenuItem_Click);
        }

        private System.Windows.Forms.ToolStripMenuItem openValueInLocalsWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem assignremoveAliasToSelectedMemberToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem gotoMemberTypeToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem analyzeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;

        private System.Windows.Forms.ToolStripMenuItem addAsBookmarkToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem useStaticMemberAsBaseInLocalsWindowToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem gotoMemberToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem findAllInstancesToolStripMenuItem;

        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem useAsStringDecryptMethodToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem decryptStringsInSelectedMethodToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem openCallGraphToolStripMenuItem;
    }
}
