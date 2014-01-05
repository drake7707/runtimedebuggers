using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunTimeDebuggers.Helpers;
using FastColoredTextBoxNS;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RunTimeDebuggers.Controls
{
    public class CodeTextBox : FastColoredTextBoxNS.FastColoredTextBox
    {

        private TextStyle linkStyle = new TextStyle(null, null, FontStyle.Underline);
        private TextStyle redlinkStyle = new TextStyle(Brushes.Red, null, FontStyle.Underline);

        private TextStyle blueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);

        private TextStyle typeTealStyle = new TextStyle(new SolidBrush(Color.FromArgb(43,145, 175)), null, FontStyle.Regular);

        private TextStyle boldStyle = new TextStyle(null, null, FontStyle.Bold);
        private TextStyle boldUnderlineStyle = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        private TextStyle GrayStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        private TextStyle MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        private TextStyle GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        private TextStyle BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Regular);
        private TextStyle MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        private MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));


        public List<VisualizerHelper.CodeBlock> CodeBlocks { get; private set; }

        public Dictionary<int,int> ILOffsetsByLine { get; private set; }

        public CodeTextBox()
        {
            
            CodeBlocks = new List<VisualizerHelper.CodeBlock>();
            ILOffsetsByLine = new Dictionary<int, int>();
            DoLineSelectWhenClickedInMargin = false;
            AllowSeveralTextStyleDrawing = true;
            ReadOnly = true;

            AddStyle(blueStyle);
            AddStyle(boldUnderlineStyle);
            
            AddStyle(MagentaStyle);
            AddStyle(GreenStyle);
            AddStyle(BrownStyle);
            AddStyle(MaroonStyle);
            AddStyle(SameWordsStyle);

            AddStyle(GrayStyle);
            AddStyle(linkStyle);
            AddStyle(boldStyle);
            AddStyle(redlinkStyle);
            AddStyle(typeTealStyle);
        }




        protected override void OnTextChanged(TextChangedEventArgs args)
        {
            base.OnTextChanged(args);
            CSharpSyntaxHighlight(args);
        }

        private void CSharpSyntaxHighlight(TextChangedEventArgs e)
        {
            this.LeftBracket = '(';
            this.RightBracket = ')';
            this.LeftBracket2 = '\x0';
            this.RightBracket2 = '\x0';
            //clear style of changed range
            e.ChangedRange.ClearStyle(blueStyle, boldUnderlineStyle, MagentaStyle, GreenStyle, BrownStyle);

            //string highlighting
            e.ChangedRange.SetStyle(BrownStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
            //comment highlighting
            e.ChangedRange.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            
            //number highlighting
           // e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
            
            //attribute highlighting
            //e.ChangedRange.SetStyle(GrayStyle, @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);

            //class name highlighting
            e.ChangedRange.SetStyle(boldUnderlineStyle, @"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b");
            //keyword highlighting
            e.ChangedRange.SetStyle(blueStyle, @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b|#region\b|#endregion\b");

            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();

            //set folding markers
            e.ChangedRange.SetFoldingMarkers("{", "}");//allow to collapse brackets block
            e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");//allow to collapse #region blocks
            e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");//allow to collapse comment block
        }

        

        private void AppendText(VisualizerHelper.CodeBlock block)
        {
            var oldPlace = new Place(GetLineLength(LinesCount - 1), LinesCount - 1);
            if (block is VisualizerHelper.InstructionOpCodeCodeBlock)
                AppendText(block.Text, boldStyle);
            else if (block is VisualizerHelper.TypeCodeBlock)
            {
                AppendText(block.Text, typeTealStyle);
            }
            else if (block is VisualizerHelper.MemberCodeBlock)
            {
                AppendText(block.Text, linkStyle);
            }
            else if (block is VisualizerHelper.InstructionOffsetCodeBlock)
            {
                if (((VisualizerHelper.InstructionOffsetCodeBlock)block).IsTarget)
                    AppendText(block.Text, redlinkStyle);
                else
                    AppendText(block.Text, GrayStyle);
            }
            else
            {
                // todo apply styles
                AppendText(block.Text);
            }

            //if descriptor contains some additional data ...
            //save descriptor in sorted list
            block.Start = oldPlace;
            block.End = new Place(GetLineLength(LinesCount - 1), LinesCount - 1);

            if (block is VisualizerHelper.InstructionOffsetCodeBlock && !((VisualizerHelper.InstructionOffsetCodeBlock)block).IsTarget)
            {
                ILOffsetsByLine[block.Start.iLine] = (int)block.Tag;
            }


            CodeBlocks.Add(block);
        }

        public void SetCodeBlocks(List<VisualizerHelper.CodeBlock> list)
        {
            Clear();
            CodeBlocks.Clear();
            ILOffsetsByLine.Clear();

            foreach (var item in list)
                AppendText(item);
        }

        private Point lastMouseCoord;
        private Place lastPlace;
        private readonly Place emptyPlace = new Place(-1, -1);


        public VisualizerHelper.CodeBlock CodeBlockAtMouse()
        {
            return CodeBlockAt(lastPlace);
        }

        public VisualizerHelper.CodeBlock CodeBlockAt(Place place)
        {
            var index = CodeBlocks.BinarySearch(new VisualizerHelper.CodeBlock() { Start = place, End = place });
            if (index >= 0)
                return CodeBlocks[index];

            return null;
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            lastMouseCoord = e.Location;
            Cursor = Cursors.IBeam;

            //get place under mouse
            lastPlace = PointToPlace(lastMouseCoord);

            //check distance
            var p = PlaceToPoint(lastPlace);
            if (Math.Abs(p.X - lastMouseCoord.X) > CharWidth * 2 || Math.Abs(p.Y - lastMouseCoord.Y) > CharHeight * 2)
                lastPlace = emptyPlace;

            //check link style
            if (lastPlace != emptyPlace)
            {
                var block = CodeBlockAt(lastPlace);
                if (block != null && block.Clickable)
                {
                    Cursor = Cursors.Hand;
                }
            }

            base.OnMouseMove(e);
        }


    }
}
