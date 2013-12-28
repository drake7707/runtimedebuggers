using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RunTimeDebuggers.Controls
{
    class NavigatableTreeView : TreeView
    {

        public NavigatableTreeView()
        {
            HideSelection = false;
            ShowNodeToolTips = true;

            // Enable default double buffering processing (DoubleBuffered returns true)
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            // Disable default CommCtrl painting on non-Vista systems
            if (Environment.OSVersion.Version.Major < 6)
                SetStyle(ControlStyles.UserPaint, true);
        }
        private const int WM_PRINTCLIENT = 0x0318;
        private const int PRF_CLIENT = 0x00000004;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETBKCOLOR = TV_FIRST + 29;
        private const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;

        private const int TVS_EX_DOUBLEBUFFER = 0x0004;


        private void UpdateExtendedStyles()
        {
            int Style = 0;

            if (DoubleBuffered)
                Style |= TVS_EX_DOUBLEBUFFER;

            if (Style != 0)
                SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)Style);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateExtendedStyles();
            if (!IsWinXP)
                SendMessage(Handle, TVM_SETBKCOLOR, IntPtr.Zero, (IntPtr)System.Drawing.ColorTranslator.ToWin32(BackColor));
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static bool IsWinXP
        {
            get
            {
                OperatingSystem OS = Environment.OSVersion;
                return (OS.Platform == PlatformID.Win32NT) &&
                    ((OS.Version.Major > 5) || ((OS.Version.Major == 5) && (OS.Version.Minor == 1)));
            }
        }

        private static bool IsWinVista
        {
            get
            {
                OperatingSystem OS = Environment.OSVersion;
                return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            if (GetStyle(ControlStyles.UserPaint))
            {
                Message m = new Message();
                m.HWnd = Handle;
                m.Msg = WM_PRINTCLIENT;
                m.WParam = e.Graphics.GetHdc();
                m.LParam = (IntPtr)PRF_CLIENT;
                DefWndProc(ref m);
                e.Graphics.ReleaseHdc(m.WParam);
            }
            base.OnPaint(e);
        }
        private Stack<TreeNode> history = new Stack<TreeNode>();
        private Stack<TreeNode> forwardHistory = new Stack<TreeNode>();

        private bool ignoreBeforeSelect;

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            if (ignoreBeforeSelect)
                return;

            SetSelectedNode(e.Node, false);
            base.OnBeforeSelect(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //rbase.OnPaintBackground(pevent);
        }

        public void GoBack()
        {
            if (HasHistory)
            {
                var n = history.Pop();
                if (forwardHistory.Count <= 0 || forwardHistory.Peek() != n)
                    forwardHistory.Push(SelectedNode);

                n.EnsureVisible();
                ignoreBeforeSelect = true;
                SelectedNode = n;
                base.OnBeforeSelect(new TreeViewCancelEventArgs(n, false, TreeViewAction.Unknown));
                ignoreBeforeSelect = false;
            }
        }

        public void GoForward()
        {
            if (HasForwardHistory)
            {
                var n = forwardHistory.Pop();

                if (history.Count <= 0 || history.Peek() != n)
                    history.Push(SelectedNode);

                n.EnsureVisible();
                ignoreBeforeSelect = true;
                SelectedNode = n;
                base.OnBeforeSelect(new TreeViewCancelEventArgs(n, false, TreeViewAction.Unknown));
                ignoreBeforeSelect = false;
            }
        }

        private void SetSelectedNode(TreeNode n, bool setselection)
        {
            if (SelectedNode != null)
                history.Push(SelectedNode);

            if (setselection)
            {
                ignoreBeforeSelect = true;
                SelectedNode = n;
                ignoreBeforeSelect = false;
            }
            forwardHistory.Clear();
        }

        public bool HasHistory { get { return history.Count > 0; } }
        public bool HasForwardHistory { get { return forwardHistory.Count > 0; } }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                try
                {
                    if (SelectedNode != null)
                        Clipboard.SetText(SelectedNode.Text);

                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to copy the node text to the clipboard, error: " + ex.GetType().FullName + " - " + ex.Message);
                }
            }
            else
                base.OnKeyDown(e);
        }
    }
}
