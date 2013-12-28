using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Collections;

namespace RunTimeDebuggers.AssemblyExplorer.Components
{
    public partial class ResourcePane : UserControl
    {
        public ResourcePane()
        {
            InitializeComponent();
        }

        internal void SetResource(string resourceName, byte[] resourceBytes)
        {
            //ClearAndDispose();

            tv.Visible = false;
            tv.Nodes.Clear();
            pic.Visible = false;
            txt.Visible = false;

            if (resourceName.ToLower().EndsWith("bmp") ||
               resourceName.ToLower().EndsWith("jpg") ||
               resourceName.ToLower().EndsWith("gif") ||
               resourceName.ToLower().EndsWith("tiff") ||
               resourceName.ToLower().EndsWith("png"))
            {
                MemoryStream stream = new MemoryStream(resourceBytes);
                Image bmp = Image.FromStream(stream);
                stream.Dispose();

                SetImage(bmp);
            }
            else if (resourceName.ToLower().EndsWith("xml") ||
                     resourceName.ToLower().EndsWith("txt"))
            {
                string str = System.Text.Encoding.ASCII.GetString(resourceBytes);
                SetText(str);
            }
            else if (resourceName.ToLower().EndsWith("resources"))
            {
                tv.Visible = true;
                MemoryStream stream = new MemoryStream(resourceBytes);
                ResourceReader reader = new ResourceReader(stream);


                Dictionary<Type, List<TreeNode>> nodesPerType = new Dictionary<Type, List<TreeNode>>();
                foreach (DictionaryEntry entry in reader)
                {
                    TreeNode tn = new TreeNode(entry.Key + "");
                    tn.Tag = entry.Value;

                    List<TreeNode> nodes;
                    if (!nodesPerType.TryGetValue(entry.Value.GetType(), out nodes))
                        nodesPerType[entry.Value.GetType()] = nodes = new List<TreeNode>();

                    nodes.Add(tn);
                }

                tv.BeginUpdate();
                foreach (var pair in nodesPerType.OrderBy(n => n.Key.Name))
                {
                    TreeNode tn = new TreeNode(pair.Key.Name);
                    tn.Nodes.AddRange(pair.Value.OrderBy(n => n.Text).ToArray());
                    tv.Nodes.Add(tn);
                }
                tv.ExpandAll();
                tv.EndUpdate();

                stream.Dispose();
            }
        }

        private void SetText(string str)
        {
            //TextBox txt = new TextBox();
            txt.Text = str;

            if (!txt.Visible)
                txt.Visible = true;
            txt.BringToFront();

            //txt.Multiline = true;
            //txt.Dock = DockStyle.Fill;
            //pnlContainer.Controls.Add(txt);
        }

        private void SetImage(Image bmp)
        {
            //PictureBox pic = new PictureBox();
            pic.Image = bmp;

            if (!pic.Visible)
                pic.Visible = true;
            pic.BringToFront();

            //pic.SizeMode = PictureBoxSizeMode.CenterImage;
            //pic.Dock = DockStyle.Fill;
            //pnlContainer.Controls.Add(pic);
        }

        private void ClearAndDispose()
        {
            while (pnlContainer.Controls.Count > 0)
            {
                Control c = pnlContainer.Controls[0];
                pnlContainer.Controls.RemoveAt(0);
                c.Dispose();
            }
        }

        private void tv_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null)
            {
                object obj = e.Node.Tag;

                //ClearAndDispose();

                if (obj is Image)
                    SetImage((Image)obj);
                else if (obj is string)
                    SetText((string)obj);
                else
                {
                    SetText(obj + "");
                }
            }
        }


    }
}
