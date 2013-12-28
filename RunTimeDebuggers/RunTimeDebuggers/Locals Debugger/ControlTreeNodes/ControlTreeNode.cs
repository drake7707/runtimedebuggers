using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.LocalsDebugger
{
    class ControlTreeNode : TreeNode
    {

        private Image icon;
        private string name;
        private object control;

        public ControlTreeNode(object c)
        {

            this.control = c;
            icon = ControlIconManager.GetImage(c);
            //icon = null;
            //var attr = c.GetType().GetCustomAttributes(typeof(ToolboxBitmapAttribute), true).FirstOrDefault();
            //if (attr != null)
            //    icon = ((ToolboxBitmapAttribute)attr).GetImage(c.GetType());

            this.name = (GetName() + "");

        }

        public string GetName()
        {
            if (control is Control)
                return ((Control)control).Name;
            else if (control is MenuItem)
                return ((MenuItem)control).Name;
            else if (control is ToolStripItem)
                return ((ToolStripItem)control).Name;


            return "";
        }


        internal virtual void Draw(TreeView tvControlTree, DrawTreeNodeEventArgs e)
        {

            if ((e.State & TreeNodeStates.Selected) != 0)
                e.Graphics.FillRectangle(Brushes.SkyBlue, new Rectangle(e.Bounds.Left, e.Bounds.Top, tvControlTree.Width, e.Bounds.Height));
            else
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(e.Bounds.Left, e.Bounds.Top, tvControlTree.Width, e.Bounds.Height));

            int left = e.Bounds.Left;

            if (icon != null)
            {
                ImageAttributes ia = new ImageAttributes();
                ia.SetColorKey(Color.Magenta, Color.Magenta);
                e.Graphics.DrawImage(icon, new Rectangle(left, e.Bounds.Top, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, ia);
            }

            left += 16;

            if (!string.IsNullOrEmpty(name))
            {
                var bold = new Font(tvControlTree.Font, FontStyle.Bold);
                var measure = e.Graphics.MeasureString(name, bold);

                e.Graphics.DrawString(name, bold, Brushes.Black, new PointF(left, e.Bounds.Top + e.Bounds.Height / 2f - measure.Height / 2f));
                left += (int)measure.Width + 3;
            }

            string typeName = control.GetType().GetName(false);
            var measureType = e.Graphics.MeasureString(typeName, tvControlTree.Font);
            e.Graphics.DrawString(typeName, tvControlTree.Font, Brushes.Black, new PointF(left, e.Bounds.Top + e.Bounds.Height / 2f - measureType.Height / 2f));

            e.DrawDefault = false;

        }

        public object Object { get { return control; } }
    }
}
