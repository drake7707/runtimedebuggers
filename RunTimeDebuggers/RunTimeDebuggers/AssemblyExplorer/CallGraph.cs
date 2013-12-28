using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Controls;
using System.Reflection;
using System.Drawing.Drawing2D;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class CallGraph : Form
    {
        private IAssemblyBrowser browser;

        public CallGraph(IAssemblyBrowser browser, MethodBase mb)
        {
            this.browser = browser;

            InitializeComponent();

            imgs.Images.AddRange(IconHelper.GetIcons().ToArray());

            graph.Orientation = Orientation.Vertical;

            graph.NodePaintHandler = PaintNode;
            GraphExplorer.GraphNode n = new GraphExplorer.GraphNode()
            {
                Tag = mb
            };
            graph.RootNode = n;

            graph.SelectedNode = n;
            graph_SelectionChanged(graph, EventArgs.Empty);
            graph.Invalidate();
        }

        private void PaintNode(GraphExplorer.GraphNode node, PaintEventArgs e, RectangleF bounds)
        {
            Graphics g = e.Graphics;
            RectangleF zoomedBounds = bounds;
            zoomedBounds.X *= graph.Zoom;
            zoomedBounds.Y *= graph.Zoom;
            zoomedBounds.Width *= graph.Zoom;
            zoomedBounds.Height *= graph.Zoom;

            if (zoomedBounds.IntersectsWith(e.ClipRectangle))
            {

                Color fillColor;
                if (graph.SelectedNode == node)
                    fillColor = Color.Gold;
                else
                    fillColor = Color.SkyBlue;

                GraphicsPath roundedRect = GetRoundedRect(bounds, 5, 5);

                using (LinearGradientBrush brush = new LinearGradientBrush(bounds, fillColor, Color.White, LinearGradientMode.Vertical))
                    g.FillPath(brush, roundedRect);

                using (Pen p = new Pen(Color.Black))
                    g.DrawPath(p, roundedRect);

                if (node.Tag != null)
                {
                    if (bounds.Width > 0 && bounds.Height > 0)
                    {
                        bounds.Inflate(-3, -3);
                        MethodBase mb=  (MethodBase)node.Tag;

                        try
                        {
                            var typeIcon = imgs.Images[IconHelper.GetIcon(mb.DeclaringType)];
                            var mbIcon = imgs.Images[IconHelper.GetIcon(mb)];

                            var typeStringBounds = new RectangleF(bounds.Left + 20, bounds.Top, bounds.Width, bounds.Height / 2);
                            var methodStringBounds = new RectangleF(bounds.Left + 20, bounds.Top + bounds.Height / 2, bounds.Width, bounds.Height / 2);

                            g.DrawImage(typeIcon, new PointF(bounds.Left, bounds.Top + (typeStringBounds.Height / 2 - typeIcon.Height / 2)));
                            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                                g.DrawString(mb.DeclaringType.ToSignatureString(true), Font, Brushes.Black, typeStringBounds, sf);

                            g.DrawImage(mbIcon, new PointF(bounds.Left, bounds.Top + bounds.Height / 2 + (methodStringBounds.Height / 2 - mbIcon.Height / 2)));
                            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                                g.DrawString(mb.ToSignatureString(), Font, Brushes.Black, methodStringBounds, sf);

                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private static GraphicsPath GetRoundedRect(RectangleF r, float radiusX, float radiusY)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();

            if (radiusX <= 0.0F || radiusY <= 0.0F)
            {
                gp.AddRectangle(r);
            }
            else
            {
                //arcs work with diameters (radius * 2)
                PointF d = new PointF(Math.Min(radiusX * 2, r.Width)
                          , Math.Min(radiusY * 2, r.Height));
                gp.AddArc(r.X, r.Y, d.X, d.Y, 180, 90);
                gp.AddArc(r.Right - d.X, r.Y, d.X, d.Y, 270, 90);
                gp.AddArc(r.Right - d.X, r.Bottom - d.Y, d.X, d.Y, 0, 90);
                gp.AddArc(r.X, r.Bottom - d.Y, d.X, d.Y, 90, 90);
            }
            gp.CloseFigure();
            return gp;
        }

        private void graph_SelectionChanged(object sender, EventArgs e)
        {
            var n = graph.SelectedNode;
            if (n.ChildNodes.Count == 0)
            {
                Expand(n);
            }

            MethodBase mb = (MethodBase)n.Tag;
            browser.SelectMember(mb);
        }

        private void Expand(GraphExplorer.GraphNode n)
        {
            MethodBase mb = (MethodBase)n.Tag;
            var memberCache = (MethodBaseCache)AnalysisManager.Instance.GetMemberCache(mb);

            foreach (var call in memberCache.Uses.Select(entry => entry.Member).Where(m => m is MethodBase))
            {
                if (!chkOnlySameAssembly.Checked || call.DeclaringType.Assembly == ((MethodBase)graph.RootNode.Tag).DeclaringType.Assembly)
                {
                    //expand
                    GraphExplorer.GraphNode child;
                    child = new GraphExplorer.GraphNode() { Tag = call };
                    n.ChildNodes.Add(child);
                    child.ParentNodes.Add(n);
                }
            }
        }


        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
                graph.Zoom += 0.01f;
            else if (e.Delta < 0)
                graph.Zoom -= 0.01f;
        }

        private void graph_NodeDoubleClick(object sender, GraphExplorer.GraphNode node)
        {
            if (node.ChildNodes.Count > 0)
                Collapse(node);
            else
                Expand(node);
        }

        private void Collapse(GraphExplorer.GraphNode node)
        {
            foreach (var childNode in node.ChildNodes)
                childNode.ParentNodes.Remove(node);
            node.ChildNodes.Clear();
            graph.Invalidate();
        }

        private void chkOnlySameAssembly_CheckedChanged(object sender, EventArgs e)
        {
            Collapse(graph.RootNode);
            Expand(graph.RootNode);
        }

    }
}
