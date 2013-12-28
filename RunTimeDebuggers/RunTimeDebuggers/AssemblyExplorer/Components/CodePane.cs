using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;
using System.Reflection;
using RunTimeDebuggers.Properties;
using System.Runtime.InteropServices;

namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class CodePane : UserControl
    {

        private IAssemblyBrowser browser;
        private MemberMenu mnuMember;
        public CodePane(IAssemblyBrowser browser, MemberMenu mnuMember)
        {
            this.browser = browser;
            this.mnuMember = mnuMember;

            DoubleBuffered = true;
            InitializeComponent();
        }

        private System.Reflection.Emit.OpCode opCodeShown;

        private void txtInfo_MouseMove(object sender, MouseEventArgs e)
        {
            DetermineInstructionAndExecuteAction(new Point(e.X, e.Y), (partIndex, instruction) =>
            {
                if (partIndex == 0)
                {
                    // offset
                    txtInfo.Cursor = Cursors.Arrow;
                    txtInfoTooltip.RemoveAll();
                }
                else if (partIndex == 1)
                {
                    // opcode
                    if (instruction.Code != opCodeShown)
                    {
                        browser.SetStatusText(instruction.Code.ToString() + " - " + instruction.Code.ToDescription());
                        //txtInfoTooltip.RemoveAll();
                        //var screenpoint = txtInfo.PointToScreen(new Point(e.X, e.Y));
                        //txtInfoTooltip.Show(instruction.Code.ToDescription(), this.TopLevelControl, screenpoint.X - this.TopLevelControl.Left, screenpoint.Y - this.TopLevelControl.Top);
                        txtInfo.Cursor = Cursors.Arrow;
                        opCodeShown = instruction.Code;
                    }
                }
                else if (partIndex == 2)
                {
                    // operand
                    if (instruction.Operand is MemberInfo || instruction.Operand is Type)
                    {
                        if (txtInfo.Cursor != Cursors.Hand)
                            txtInfo.Cursor = Cursors.Hand;
                    }
                    else if (instruction.Code.FlowControl == System.Reflection.Emit.FlowControl.Branch ||
                           instruction.Code.FlowControl == System.Reflection.Emit.FlowControl.Cond_Branch)
                    {
                        if (instruction.Operand is Int32[])
                        {
                        }
                        else
                        {
                            if (txtInfo.Cursor != Cursors.Hand)
                                txtInfo.Cursor = Cursors.Hand;
                        }
                    }
                    else
                        txtInfo.Cursor = Cursors.Arrow;

                    txtInfoTooltip.RemoveAll();
                }
                else
                {
                    txtInfo.Cursor = Cursors.Arrow;
                    txtInfoTooltip.RemoveAll();
                }
            });
        }

        private void DetermineInstructionAndExecuteAction(Point pos, Action<int, ILInstruction> action)
        {
            try
            {
                if (string.IsNullOrEmpty(txtInfo.Text))
                    return;

                int charIdx = txtInfo.GetCharIndexFromPosition(pos);
                int lineIdx = txtInfo.GetLineFromCharIndex(charIdx);


                var charpos = txtInfo.GetPositionFromCharIndex(charIdx);
                if (Math.Abs(charpos.X - pos.X) > 16 || Math.Abs(charpos.Y - pos.Y) > 16)
                {
                    action(-1, default(ILInstruction));
                    return;
                }

                string line = txtInfo.Lines[lineIdx];

                // space, the spaces in method signature are replaced with nbsp
                int partIndex = line.Substring(0, charIdx - txtInfo.GetFirstCharIndexFromLine(lineIdx)).Count(c => c == ' ');

                int offset;
                if (TryGetILOffset(line, out offset))
                {
                    var mbody = GetCurrentSelectedMethod();
                    if (mbody != null)
                    {

                        List<ILInstruction> instructions = mbody.GetILInstructions();
                        var instruction = instructions.Where(i => i.Offset == offset).FirstOrDefault();
                        if (instruction != default(ILInstruction))
                        {
                            action(partIndex, instruction);
                        }

                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private static bool TryGetILOffset(string line, out int offset)
        {
            offset = -1;
            return line.Trim().Length >= 4 && int.TryParse(line.Trim().Substring(0, 4), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out offset);
        }

        private AbstractAssemblyNode node;

        public AbstractAssemblyNode Node
        {
            get { return node; }
            set
            {
                node = value;
                UpdateVisualization(node);
                pnl.Invalidate();
                ResetJumpedToOffset();
            }
        }

        private void ResetJumpedToOffset()
        {
            jumpedToOffset = -1; // reset offset if there was one
        }



        private void UpdateVisualization(AbstractAssemblyNode n)
        {

            if (n == null)
                txtInfo.Clear();
            else
            {
                if (n is ResourceNode)
                {
                    string resourceName = ((ResourceNode)n).ResourceName;
                    byte[] resourceBytes = ((ResourceNode)n).GetStream().ReadToEnd();
                    resourcePane.SetResource(resourceName, resourceBytes);
                    resourcePane.Visible = true;
                    tabsCode.Visible = false;
                    txtInfo.Clear();
                }
                else
                {
                    try
                    {
                        txtInfo.Rtf = n.Visualization;
                    }
                    catch (Exception)
                    {
                        txtInfo.Text = n.Visualization;
                    }

                    try
                    {
                        if (n is MethodNode)
                            diagram.LoadIL(((MethodNode)n).Method.GetILInstructions());
                        else if (n is ConstructorNode)
                            diagram.LoadIL(((ConstructorNode)n).Constructor.GetILInstructions());
                    }
                    catch (Exception)
                    {


                    }


                    tabsCode.Visible = true;
                    resourcePane.Visible = false;
                }
                UpdateILOffsets();
            }
        }


        private MethodBase GetCurrentSelectedMethod()
        {

            if (Node is MethodNode)
                return (MethodBase)((MethodNode)Node).Member;
            else if (Node is ConstructorNode)
                return (ConstructorInfo)((ConstructorNode)Node).Member;

            return null;
        }

        private void txtInfo_MouseUp(object sender, MouseEventArgs e)
        {
            DetermineInstructionAndExecuteAction(new Point(e.X, e.Y), (partIndex, instruction) =>
            {
                if (partIndex == 0)
                {
                    // offset
                    txtInfo.Cursor = Cursors.Arrow;
                }
                else if (partIndex == 1)
                {
                    // opcode
                }
                else if (partIndex == 2)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        // operand
                        if (instruction.Operand is Type)
                        {
                            browser.SelectType((Type)instruction.Operand);
                        }
                        else if (instruction.Operand is MemberInfo)
                        {
                            browser.SelectMember((MemberInfo)instruction.Operand);
                        }
                        else if (instruction.Code.FlowControl == System.Reflection.Emit.FlowControl.Branch ||
                                 instruction.Code.FlowControl == System.Reflection.Emit.FlowControl.Cond_Branch)
                        {
                            if (instruction.Operand is Int32[])
                            {
                            }
                            else
                            {
                                int offset = Convert.ToInt32(instruction.Operand);
                                ScrollToInstruction(offset);
                            }
                        }
                    }
                    else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        // operand
                        if (instruction.Operand is Type || instruction.Operand is MemberInfo)
                        {
                            mnuMember.Tag = instruction.Operand;
                            mnuMember.Show(Cursor.Position);
                            mnuMember.UpdateAnalyzeTypeMenuEnabledStatus(false);
                        }
                    }
                }
            });


            if (e.Button == System.Windows.Forms.MouseButtons.XButton1)
                browser.GoBack();
            else if (e.Button == System.Windows.Forms.MouseButtons.XButton2)
                browser.GoForward();
        }

        private void pnl_MouseDown(object sender, MouseEventArgs e)
        {
            var currentMethod = GetCurrentSelectedMethod();
            if (currentMethod != null)
            {
                int lineIndex = txtInfo.GetLineFromCharIndex(txtInfo.GetCharIndexFromPosition(new Point(e.X, e.Y)));

                int ilOffset;
                if (TryGetILOffset(txtInfo.Lines[lineIndex], out ilOffset))
                {
                    BreakpointManager.Instance.ToggleBreakpoint(currentMethod, ilOffset);
                }

                pnl.Invalidate();
            }
        }

        private Dictionary<int, int> ilOffsetsByLine;
        private void UpdateILOffsets()
        {
            var lines = txtInfo.Lines; // ye gods, it builds the array in the get {} which is epic slow with 1000+ lines

            ilOffsetsByLine = new Dictionary<int, int>();
            for (int line = 0; line < lines.Length; line++)
            {
                int ilOffset;
                if (TryGetILOffset(lines[line], out ilOffset))
                {
                    ilOffsetsByLine[line] = ilOffset;
                }
            }
        }

        private void pnl_Paint(object sender, PaintEventArgs e)
        {
            var currentMethod = GetCurrentSelectedMethod();
            if (currentMethod != null)
            {
                HashSet<int> ilOffsets = new HashSet<int>(BreakpointManager.Instance.GetBreakpoints(currentMethod));


                foreach (var p in ilOffsetsByLine)
                {
                    var line = p.Key;
                    var ilOffset = p.Value;

                    bool posSet = false;
                    Point pos = new Point();
                    if (ilOffsets.Contains(ilOffset))
                    {
                        if (!posSet)
                        {
                            pos = txtInfo.GetPositionFromCharIndex(txtInfo.GetFirstCharIndexFromLine(line));
                            posSet = true;
                        }
                        e.Graphics.DrawImage(Resources.breakpoint, new PointF(0, pos.Y));
                    }

                    if (jumpedToOffset == ilOffset)
                    {
                        if (!posSet)
                        {
                            pos = txtInfo.GetPositionFromCharIndex(txtInfo.GetFirstCharIndexFromLine(line));
                            posSet = true;
                        }
                        e.Graphics.DrawImage(Resources.jumpedto, new PointF(0, pos.Y));
                    }

                    var debugger = ILDebugManager.Instance.Debugger;
                    if (debugger != null && debugger.CurrentMethod == currentMethod && debugger.CurrentInstruction != null && debugger.CurrentInstruction.Offset == ilOffset)
                    {
                        if (!posSet)
                        {
                            pos = txtInfo.GetPositionFromCharIndex(txtInfo.GetFirstCharIndexFromLine(line));
                            posSet = true;
                        }
                        e.Graphics.DrawImage(Resources.CurrentLine, new PointF(0, pos.Y));
                    }
                }
            }
        }


        private void txtInfo_VScroll(object sender, EventArgs e)
        {
            pnl.Invalidate();
        }

        public void FocusOnActiveLine()
        {
            if (ILDebugManager.Instance.Debugger == null || ILDebugManager.Instance.Debugger.Returned)
                return;

            var currentMethod = GetCurrentSelectedMethod();
            if (currentMethod != null)
            {

                foreach (var p in ilOffsetsByLine)
                {
                    var line = p.Key;
                    var ilOffset = p.Value;


                    var debugger = ILDebugManager.Instance.Debugger;
                    if (debugger != null && debugger.CurrentMethod == currentMethod && debugger.CurrentInstruction.Offset == ilOffset)
                    {
                        txtInfo.Select(txtInfo.GetFirstCharIndexFromLine(line), 0);
                        txtInfo.Focus();
                    }
                }
            }
        }

        private int jumpedToOffset;
        internal void ScrollToInstruction(int offset)
        {
            foreach (var p in ilOffsetsByLine)
            {
                var line = p.Key;
                var ilOffset = p.Value;

                if (ilOffset == offset)
                {
                    txtInfo.SelectionStart = txtInfo.GetFirstCharIndexFromLine(line);
                    txtInfo.ScrollToCaret();
                    jumpedToOffset = offset;
                }
            }
            pnl.Invalidate();
        }
    }


    public class HighlightRTB : RichTextBox
    {
        public HighlightRTB()
        {
            //Highlights = new List<Highlight>();
        }

        //private const int WM_PAINT = 15;
        //protected override void WndProc(ref System.Windows.Forms.Message m)
        //{
        //    base.WndProc(ref m);
        //    if (m.Msg == WM_PAINT)
        //    {
        //        // raise the paint event
        //        using (Graphics graphic = base.CreateGraphics())
        //            OnPaint(new PaintEventArgs(graphic, base.ClientRectangle));
        //    }

        //}

        //[System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public List<Highlight> Highlights { get; set; }

        //public  class Highlight
        //{
        //    public int Line { get; set; }
        //    public Color Color { get; set; }
        //}

        //[DllImport("gdi32.dll")]
        //static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);


        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    base.OnPaint(e);

        //    int width = this.ClientSize.Width;
        //    foreach (var h in Highlights)
        //    {
        //        Point pos = GetPositionFromCharIndex(GetFirstCharIndexFromLine(h.Line));

        //        Point pos2 = GetPositionFromCharIndex(GetFirstCharIndexFromLine(h.Line+1));

        //        var hdc = e.Graphics.GetHdc();
        //        uint pixel = GetPixel(hdc, 0, pos.Y);
        //        Color color = Color.FromArgb((int)(pixel & 0x000000FF),
        //              (int)(pixel & 0x0000FF00) >> 8,
        //              (int)(pixel & 0x00FF0000) >> 16);
        //        e.Graphics.ReleaseHdc(hdc);

        //        if (color == BackColor)
        //        {
        //            using (SolidBrush br = new SolidBrush(Color.FromArgb(32, h.Color)))
        //            {
        //                e.Graphics.FillRectangle(br, new Rectangle(pos.X, pos.Y, width, pos2.Y - pos.Y + 1));
        //            }
        //        }

        //    }

        //}
    }
}
