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




        private AbstractAssemblyNode node;

        public AbstractAssemblyNode Node
        {
            get { return node; }
            set
            {
                node = value;
                UpdateVisualization(node);
                txtNewInfo.Invalidate();
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
            {
                txtNewInfo.Clear();
            }
            else
            {
                if (n is ResourceNode)
                {
                    string resourceName = ((ResourceNode)n).ResourceName;
                    byte[] resourceBytes = ((ResourceNode)n).GetStream().ReadToEnd();
                    resourcePane.SetResource(resourceName, resourceBytes);
                    resourcePane.Visible = true;
                    tabsCode.Visible = false;

                    txtNewInfo.Clear();
                }
                else
                {
                    txtNewInfo.SetCodeBlocks(n.Visualization);

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
                //UpdateILOffsets();
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

        private void txtNewInfo_MouseUp(object sender, MouseEventArgs e)
        {
            var block = txtNewInfo.CodeBlockAtMouse();
            if (block != null)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (block is VisualizerHelper.TypeCodeBlock)
                    {
                        browser.SelectType((Type)block.Tag);
                    }
                    else if (block is VisualizerHelper.MemberCodeBlock)
                    {
                        browser.SelectMember((MemberInfo)block.Tag);
                    }
                    else if (block is VisualizerHelper.InstructionOffsetCodeBlock && ((VisualizerHelper.InstructionOffsetCodeBlock)block).IsTarget)
                    {
                        int offset = Convert.ToInt32(((VisualizerHelper.InstructionOffsetCodeBlock)block).Tag);
                        ScrollToInstruction(offset);
                    }
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    // operand
                    if (block is VisualizerHelper.TypeCodeBlock || block is VisualizerHelper.MemberCodeBlock)
                    {
                        mnuMember.Tag = block.Tag;
                        mnuMember.Show(Cursor.Position);
                        mnuMember.UpdateAnalyzeTypeMenuEnabledStatus(false);
                    }
                }
            }

            if (e.Button == System.Windows.Forms.MouseButtons.XButton1)
                browser.GoBack();
            else if (e.Button == System.Windows.Forms.MouseButtons.XButton2)
                browser.GoForward();
        }




        public void FocusOnActiveLine()
        {
            if (ILDebugManager.Instance.Debugger == null || ILDebugManager.Instance.Debugger.Returned)
                return;

            var debugger = ILDebugManager.Instance.Debugger;
            var currentMethod = GetCurrentSelectedMethod();
            if (debugger != null && currentMethod != null && debugger.CurrentMethod == currentMethod)
            {
                int line = txtNewInfo.ILOffsetsByLine.Where(p => p.Value == debugger.CurrentInstruction.Offset).Select(p => p.Key).First();

                txtNewInfo.Selection.Start = new FastColoredTextBoxNS.Place(0, line);
                txtNewInfo.DoSelectionVisible();
                txtNewInfo.Focus();

            }
        }

        private int jumpedToOffset;
        internal void ScrollToInstruction(int offset)
        {
            var targetBlock = txtNewInfo.CodeBlocks.OfType<VisualizerHelper.InstructionOffsetCodeBlock>()
                                                             .Where(b => !b.IsTarget && (int)b.Tag == offset).FirstOrDefault();

            txtNewInfo.Selection.Start = new FastColoredTextBoxNS.Place(0, targetBlock.Start.iLine);
            txtNewInfo.DoSelectionVisible();

            jumpedToOffset = offset;
            txtNewInfo.Invalidate();
        }

        private HashSet<int> breakPointILOffsets;
        private void txtNewInfo_PrepareForPaint(object sender, EventArgs e)
        {
            var currentMethod = GetCurrentSelectedMethod();
            if (currentMethod != null)
                breakPointILOffsets = BreakpointManager.Instance.GetBreakpoints(currentMethod);
            else
                breakPointILOffsets = null;
        }

        private void txtNewInfo_PaintLine(object sender, FastColoredTextBoxNS.PaintLineEventArgs e)
        {
            var currentMethod = GetCurrentSelectedMethod();
            if (currentMethod != null)
            {
                int ilOffset;
                if (txtNewInfo.ILOffsetsByLine.TryGetValue(e.LineIndex, out ilOffset))
                {
                    if (breakPointILOffsets.Contains(ilOffset))
                    {
                        using (var bmp = Resources.breakpoint)
                            e.Graphics.DrawImage(bmp, new PointF(0, e.LineRect.Y));
                    }

                    if (jumpedToOffset == ilOffset)
                    {
                        using (var bmp = Resources.jumpedto)
                            e.Graphics.DrawImage(bmp, new PointF(0, e.LineRect.Y));
                    }

                    var debugger = ILDebugManager.Instance.Debugger;
                    if (debugger != null && debugger.CurrentMethod == currentMethod && debugger.CurrentInstruction != null && debugger.CurrentInstruction.Offset == ilOffset)
                    {
                        e.Graphics.FillRectangle(Brushes.Yellow, e.LineRect);
                        using (var bmp = Resources.CurrentLine)
                            e.Graphics.DrawImage(bmp, new PointF(0, e.LineRect.Y));
                    }
                }
            }
        }

        private void txtNewInfo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.X < txtNewInfo.LeftPadding) // in margin
            {
                var currentMethod = GetCurrentSelectedMethod();
                if (currentMethod != null)
                {
                    int lineIndex = txtNewInfo.PointToPlace(new Point(e.X, e.Y)).iLine;

                    //int lineIndex = txtInfo.GetLineFromCharIndex(txtInfo.GetCharIndexFromPosition(new Point(e.X, e.Y)));
                    int ilOffset;
                    if (txtNewInfo.ILOffsetsByLine.TryGetValue(lineIndex, out ilOffset))
                        BreakpointManager.Instance.ToggleBreakpoint(currentMethod, ilOffset);

                    //int ilOffset;
                    //if (TryGetILOffset(txtInfo.Lines[lineIndex], out ilOffset))
                    //{
                    //    BreakpointManager.Instance.ToggleBreakpoint(currentMethod, ilOffset);
                    //}
                    txtNewInfo.Invalidate();

                }
            }
        }

        private void txtNewInfo_ToolTipNeeded(object sender, FastColoredTextBoxNS.ToolTipNeededEventArgs e)
        {
            var block = txtNewInfo.CodeBlockAt(e.Place);

            if (block is VisualizerHelper.InstructionOpCodeCodeBlock)
            {
                ILInstruction instruction = (ILInstruction)((VisualizerHelper.InstructionOpCodeCodeBlock)block).Tag;

                e.ToolTipTitle = instruction.Code.ToString();
                e.ToolTipText = instruction.Code.ToDescription();
            }
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
