using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace RunTimeDebuggers.Controls
{
    public class GraphExplorer : Control
    {
        public GraphExplorer()
        {
            InitializeComponent();
            DoubleBuffered = true;
            NodePadding = 20;
            NodeSize = new System.Drawing.Size(150, 75);
            FocusOnSelectedNode = true;
            Zoom = 1;
        }

        private Timer tmr;
        private System.ComponentModel.IContainer components;

        private GraphNode rootNode;

        public GraphNode RootNode
        {
            get { return rootNode; }
            set { rootNode = value; }
        }


        public class GraphNode
        {
            public GraphNode()
            {
                ParentNodes = new List<GraphNode>();
                ChildNodes = new List<GraphNode>();
            }

            public object Tag { get; set; }
            public List<GraphNode> ParentNodes { get; set; }
            public List<GraphNode> ChildNodes { get; set; }

            public bool SelfReferencing { get; set; }
        }

        protected virtual void OnNodeExpand(GraphNode node)
        {

        }

        public Action<GraphNode, PaintEventArgs, RectangleF> NodePaintHandler { get; set; }

        public Action<GraphNode, PaintEventArgs, RectangleF, List<RectangleF>> NodeAssociationPaintHandler { get; set; }

        public GraphNode SelectedNode { get; set; }

        public Size NodeSize { get; set; }


        private float zoom;

        [DefaultValue(1f)]
        public float Zoom
        {
            get { return zoom; }
            set
            {
                bool changed = zoom != value;
                if (changed)
                {
                    float oldZoom = zoom;
                    zoom = value;
                    if (zoom <= 0)
                        zoom = 0.01f;

                    if (oldZoom != 0)
                    {
                        offset = new PointF((offset.X) / zoom * oldZoom,
                                            (offset.Y / zoom * oldZoom));
                    }

                    Invalidate();
                }
            }
        }


        private PointF offset;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.ScaleTransform(Zoom, Zoom);

            if (RootNode != null)
            {
                ForEachIn(RootNode, (n, bounds, childBounds) =>
                {
                    bounds.Offset(offset);

                    var handler = NodePaintHandler;
                    if (handler != null)
                        handler(n, e, bounds);
                    else
                        DefaultNodePaintHandler(n, e, bounds);

                    var associationhandler = NodeAssociationPaintHandler;
                    if (associationhandler != null)
                        associationhandler(n, e, bounds, childBounds);
                    else
                        DefaultAssociationPaintHandler(g, bounds, childBounds);

                    return true;
                });
            }
        }

        public enum DefaultAssociationStyleEnum
        {
            Straight,
            HVH
        }
        public DefaultAssociationStyleEnum DefaultAssociationStyle { get; set; }
        private void DefaultAssociationPaintHandler(Graphics g, RectangleF bounds, List<RectangleF> childBounds)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var b in childBounds)
            {
                b.Offset(offset);

                if (Orientation == System.Windows.Forms.Orientation.Horizontal)
                    DrawHorizontalAssociations(g, bounds, b);
                else
                    DrawVerticalAssociations(g, bounds, b);
            }
        }

        private void DrawHorizontalAssociations(Graphics g, RectangleF bounds, RectangleF b)
        {
            PointF start = new PointF(bounds.Right, bounds.Top + (bounds.Bottom - bounds.Top) / 2);
            PointF end = new PointF(b.Left, b.Top + (b.Bottom - b.Top) / 2);

            if (DefaultAssociationStyle == DefaultAssociationStyleEnum.Straight)
            {
                g.DrawLine(Pens.Black, start, end);
            }
            else if (DefaultAssociationStyle == DefaultAssociationStyleEnum.HVH)
            {
                g.DrawLine(Pens.Black, start, new PointF(start.X + (end.X - start.X) / 2, start.Y));
                g.DrawLine(Pens.Black, new PointF(start.X + (end.X - start.X) / 2, start.Y), new PointF(start.X + (end.X - start.X) / 2, end.Y));
                g.DrawLine(Pens.Black, new PointF(start.X + (end.X - start.X) / 2, end.Y), end);
            }
        }

        private void DrawVerticalAssociations(Graphics g, RectangleF bounds, RectangleF b)
        {
            PointF start = new PointF(bounds.Left + (bounds.Right - bounds.Left) / 2, bounds.Bottom);
            PointF end = new PointF(b.Left + (b.Right - b.Left) / 2, b.Top);

            if (DefaultAssociationStyle == DefaultAssociationStyleEnum.Straight)
            {
                g.DrawLine(Pens.Black, start, end);
            }
            else if (DefaultAssociationStyle == DefaultAssociationStyleEnum.HVH)
            {
                g.DrawLine(Pens.Black, start, new PointF(start.X, start.Y + (end.Y - start.Y) / 2));
                g.DrawLine(Pens.Black, new PointF(start.X, start.Y + (end.Y - start.Y) / 2), new PointF(end.X, start.Y + (end.Y - start.Y) / 2));
                g.DrawLine(Pens.Black, new PointF(end.X, start.Y + (end.Y - start.Y) / 2), end);
            }
        }

        public int NodePadding { get; set; }

        public bool FocusOnSelectedNode { get; set; }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            orgOffset = offset;
            mouseDownOffset = new PointF(e.X, e.Y);
        }

        private PointF orgOffset;
        private PointF mouseDownOffset;

        private PointF currentMousePos;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Cursor = Cursors.SizeAll;
                offset = new PointF(orgOffset.X + (e.X - mouseDownOffset.X) / Zoom, orgOffset.Y + (e.Y - mouseDownOffset.Y) / Zoom);
                Invalidate();
            }

            currentMousePos = new PointF(e.X, e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (Math.Abs(e.X - mouseDownOffset.X) > 10 || Math.Abs(e.Y - mouseDownOffset.Y) > 10)
            {
                // dragged
            }
            else
            {
                // clicked

                if (RootNode != null)
                {
                    Point mousePoint = new Point(e.X, e.Y);
                    ForEachIn(RootNode, (n, bounds, childBounds) =>
                    {
                        RectangleF orgBounds = bounds;
                        bounds.Offset(offset);
                        bounds.X *= Zoom;
                        bounds.Y *= Zoom;
                        bounds.Width *= Zoom;
                        bounds.Height *= Zoom;

                        if (bounds.Contains(mousePoint))
                        {
                            OnMouseDownInNode(n, orgBounds);
                            return false;
                        }
                        return true;
                    });
                }
            }
            Cursor = Cursors.Default;
        }
        private void OnMouseDownInNode(GraphNode n, RectangleF orgBounds)
        {
            bool changed = SelectedNode != n;
            if (changed)
            {
                SelectedNode = n;

                OnSelectionChanged(n);

                if (FocusOnSelectedNode)
                {
                    // set animation to move to new offset
                    PointF newOffset = new PointF(-orgBounds.Left + (this.Width / 2 - NodeSize.Width / 2) / Zoom, -orgBounds.Top + (this.Height / 2 - NodeSize.Height / 2) / Zoom);
                    anim.AddAnimation<PointF>(offset, newOffset, (start, end, progress) =>
                    {
                        offset = new PointF(start.X + (end.X - start.X) * progress, start.Y + (end.Y - start.Y) * progress);
                        Invalidate();
                    });
                }

                Invalidate();
            }
        }

        protected virtual void OnSelectionChanged(GraphNode selectedNode)
        {
            EventHandler temp = SelectionChanged;
            if (temp != null)
                temp(this, EventArgs.Empty);
        }
        public event EventHandler SelectionChanged;


        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if (RootNode != null)
            {
                PointF mousePoint = new PointF(currentMousePos.X, currentMousePos.Y);
                ForEachIn(RootNode, (n, bounds, childBounds) =>
                {
                    RectangleF orgBounds = bounds;
                    bounds.Offset(offset);
                    bounds.X *= Zoom;
                    bounds.Y *= Zoom;
                    bounds.Width *= Zoom;
                    bounds.Height *= Zoom;

                    if (bounds.Contains(mousePoint))
                    {
                        OnNodeDoubleClick(n);
                        return false;
                    }
                    return true;
                });
            }


        }

        private void OnNodeDoubleClick(GraphNode n)
        {
            NodeDoubleClickHandler temp = NodeDoubleClick;
            if (temp != null)
                temp(this, n);
        }

        public delegate void NodeDoubleClickHandler(object sender, GraphExplorer.GraphNode node);
        public event NodeDoubleClickHandler NodeDoubleClick;

        private void ForEachIn(GraphNode n, Func<GraphNode, RectangleF, List<RectangleF>, bool> action)
        {
            bool continueLoop;
            ForEachIn(n, action, 0, 0f, out continueLoop);
        }

        public Orientation Orientation { get; set; }

        private RectangleF ForEachIn(GraphNode n, Func<GraphNode, RectangleF, List<RectangleF>, bool> action, int colOffset, float childBoundsOffset, out bool continueLoop)
        {
            if (Orientation == System.Windows.Forms.Orientation.Vertical)
                return VerticalForEachIn(n, action, colOffset, childBoundsOffset, out continueLoop);
            else
                return HorizontalForEachIn(n, action, colOffset, childBoundsOffset, out continueLoop);
        }

        private RectangleF HorizontalForEachIn(GraphNode n, Func<GraphNode, RectangleF, List<RectangleF>, bool> action, int colOffset, float childBoundsOffset, out bool continueLoop)
        {
            RectangleF bounds;

            List<RectangleF> childBounds = new List<RectangleF>();

            float curOffset = childBoundsOffset;

            if (n.ChildNodes.Count > 0)
            {
                bounds = new RectangleF();
                foreach (GraphNode sub in n.ChildNodes)
                {
                    curOffset += bounds.Height;
                    bounds = ForEachIn(sub, action, colOffset + 1, curOffset, out continueLoop);
                    childBounds.Add(bounds);
                    if (!continueLoop)
                        return default(RectangleF);
                }
            }
            else
            {
                bounds = new RectangleF((NodeSize.Width + NodePadding) * colOffset, childBoundsOffset, NodeSize.Width + NodePadding, NodeSize.Height + NodePadding);
            }

            RectangleF boundingArea = new RectangleF((NodeSize.Width + NodePadding) * colOffset, childBoundsOffset, NodeSize.Width + NodePadding, bounds.Bottom - childBoundsOffset);


            RectangleF nodeBounds = new RectangleF(boundingArea.Left, boundingArea.Top + boundingArea.Height / 2f - NodeSize.Height / 2f, NodeSize.Width, NodeSize.Height);
            //RectangleF nodeBounds = new RectangleF(boundingArea.Left, boundingArea.Top, NodeWidth, NodeHeight);

            action(n, nodeBounds, childBounds);

            continueLoop = true;
            return boundingArea;
        }

        private RectangleF VerticalForEachIn(GraphNode n, Func<GraphNode, RectangleF, List<RectangleF>, bool> action, int colOffset, float childBoundsOffset, out bool continueLoop)
        {
            RectangleF bounds;

            List<RectangleF> childBounds = new List<RectangleF>();

            float curOffset = childBoundsOffset;

            int collapsable = 0;

            if (n.ChildNodes.Count > 0)
            {
                bounds = new RectangleF();
                foreach (GraphNode sub in n.ChildNodes)
                {
                    curOffset += bounds.Width;

                    bounds = ForEachIn(sub, action, colOffset + 1, curOffset - collapsable * (NodeSize.Width + NodePadding), out continueLoop);
                    childBounds.Add(bounds);

                    if (!continueLoop)
                        return default(RectangleF);

                    //if (sub.ChildNodes.Count > 0)
                    //{
                    //    collapsable = 0; 
                    //}
                    //else
                    //    collapsable++;

                }
            }
            else
            {
                bounds = new RectangleF(childBoundsOffset, (NodeSize.Height + NodePadding) * colOffset, NodeSize.Width + NodePadding, NodeSize.Height + NodePadding);
            }

            RectangleF boundingArea = new RectangleF(childBoundsOffset, (NodeSize.Height + NodePadding) * colOffset, bounds.Right - childBoundsOffset, NodeSize.Height + NodePadding);


            RectangleF nodeBounds = new RectangleF(boundingArea.Left + boundingArea.Width / 2f - NodeSize.Width / 2f, boundingArea.Top, NodeSize.Width, NodeSize.Height);
            //RectangleF nodeBounds = new RectangleF(boundingArea.Left, boundingArea.Top, NodeWidth, NodeHeight);

            action(n, nodeBounds, childBounds);

            continueLoop = true;
            return boundingArea;
        }

        private void DefaultNodePaintHandler(GraphNode n, PaintEventArgs e, RectangleF bounds)
        {
            Graphics g = e.Graphics;
            RectangleF zoomedBounds = bounds;
            zoomedBounds.X *= Zoom;
            zoomedBounds.Y *= Zoom;
            zoomedBounds.Width *= Zoom;
            zoomedBounds.Height *= Zoom;

            if (zoomedBounds.IntersectsWith(e.ClipRectangle))
            {
                g.FillRectangle(Brushes.White, bounds.X, bounds.Y, bounds.Width, bounds.Height);

                if (SelectedNode == n)
                    g.DrawRectangle(Pens.Red, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                else
                    g.DrawRectangle(Pens.Black, bounds.X, bounds.Y, bounds.Width, bounds.Height);

                if (n.Tag != null)
                {
                    if (bounds.Width > 0 && bounds.Height > 0)
                    {
                        try
                        {
                            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                                g.DrawString(n.Tag.ToString(), Font, Brushes.Black, bounds, sf);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }


        private RectangleF GetNodeBounds(GraphNode node, int idx)
        {
            return new RectangleF(0, NodeSize.Height * idx, NodeSize.Width, NodeSize.Height);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmr = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tmr
            // 
            this.tmr.Enabled = true;
            this.tmr.Interval = 25;
            this.tmr.Tick += new System.EventHandler(this.tmr_Tick);
            this.ResumeLayout(false);

        }


        private AnimManager anim = new AnimManager(10);

        private class AnimManager
        {
            public AnimManager(int nrOfFrames)
            {
                NrOfFrames = nrOfFrames;
            }

            public int NrOfFrames { get; set; }


            private interface IAnimJob
            {
                void Next(float newProgress);
                int Frame { get; }
            }
            private class AnimJob<T> : IAnimJob
            {
                public T OldState { get; set; }
                public T NewState { get; set; }
                public Action<T, T, float> NextHandler { get; set; }
                public int Frame { get; private set; }

                public void Next(float newProgress)
                {
                    NextHandler(OldState, NewState, newProgress);
                    Frame++;
                }
            }

            private List<IAnimJob> jobs = new List<IAnimJob>();
            public void AddAnimation<T>(T oldState, T newState, Action<T, T, float> next)
            {
                jobs.Add(new AnimJob<T>()
                {
                    OldState = oldState,
                    NewState = newState,
                    NextHandler = next
                });
            }

            public void Next()
            {
                foreach (var j in jobs.ToArray())
                {
                    j.Next(j.Frame / (float)NrOfFrames);
                    if (j.Frame >= NrOfFrames)
                        jobs.Remove(j);
                }
            }
        }

        private void tmr_Tick(object sender, EventArgs e)
        {
            anim.Next();
        }
    }
}
