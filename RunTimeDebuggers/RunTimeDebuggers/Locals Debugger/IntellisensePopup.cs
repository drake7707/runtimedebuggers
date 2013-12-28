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

namespace RunTimeDebuggers.LocalsDebugger
{
    public partial class IntellisensePopup : Form
    {
        public IntellisensePopup()
        {
            InitializeComponent();

            LocalsWindow.FillImageListForMemberIcons(imgs);
            lstItems.DrawMode = DrawMode.OwnerDrawFixed;

            lstItems.ItemHeight = 18;
            MaxItemsShown = 10;

        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public Type CurrentType { get; set; }


        public void SetItems(IEnumerable<ListItem> items)
        {
            lstItems.Items.Clear();
            lstItems.BeginUpdate();
            lstItems.Items.AddRange(items.ToArray());
            lstItems.EndUpdate();

            if (ItemCount > MaxItemsShown)
                this.Height = lstItems.ItemHeight * MaxItemsShown + 4;
            else
                this.Height = lstItems.ItemHeight * ItemCount + 4;
        }

        public class ListItem
        {
            public MemberInfo Member { get; private set; }

            private string text;

            public ListItem(MemberInfo member, string text)
            {
                this.Member = member;
                this.text = text;
            }

            public ListItem(MemberInfo member)
                : this(member, member.GetName(false))
            {
            }

            public override string ToString()
            {
                return text;
            }
        }

        private void lstItems_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0 && e.Index < lstItems.Items.Count)
            {
                ListItem itm = (ListItem)lstItems.Items[e.Index];

                using (Bitmap bmp = new Bitmap(e.Bounds.Width, e.Bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                        if (e.Index == lstItems.SelectedIndex)
                            g.FillRectangle(Brushes.SkyBlue, new Rectangle(16, 0, bmp.Width, bmp.Height));
                        else
                            g.FillRectangle(Brushes.White, new Rectangle(16, 0, bmp.Width, bmp.Height));
                        //e.Graphics.FillRectangle(Brushes.White, e.Bounds);

                        int iconId = itm.Member.GetIcon();

                        if (iconId >= 0)
                        {
                            Image img = imgs.Images[iconId];
                            //e.Graphics.DrawImage(img, new Point(e.Bounds.Left, e.Bounds.Top));
                            g.DrawImage(img, new Point(0, 0));
                        }

                        string str = itm.ToString();
                        if (!string.IsNullOrEmpty(str))
                            //e.Graphics.DrawString(str, Font, Brushes.Black, new PointF(e.Bounds.Left + 16 + 2, e.Bounds.Top));
                            g.DrawString(str, Font, Brushes.Black, new PointF(16f + 2, 0f));

                        e.Graphics.DrawImage(bmp, new Point(e.Bounds.Left, e.Bounds.Top));
                    }
                }
            }
            else
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
        }

        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstItems.Invalidate();

            var selectedMember = GetSelectedMember();
            if (selectedMember != null)
            {
                ShowToolTip(selectedMember);
            }
        }

        private void ShowToolTip(MemberInfo selectedMember)
        {
            if (!Visible)
                return;

            if (selectedMember is FieldInfo)
            {
                string sig = ((FieldInfo)selectedMember).ToSignatureString();
                var attr = selectedMember.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                if (attr != null)
                    sig += Environment.NewLine + ((DescriptionAttribute)attr).Description;
                signatures.RemoveAll();
                signatures.Show(sig, this, new Point(this.Width, 0));
            }
            else if (selectedMember is PropertyInfo)
            {
                string sig = ((PropertyInfo)selectedMember).ToSignatureString();
                var attr = selectedMember.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                if (attr != null)
                    sig += Environment.NewLine + ((DescriptionAttribute)attr).Description;

                signatures.RemoveAll();
                signatures.Show(sig, this, new Point(this.Width, 0));
            }
            else if (selectedMember is MethodInfo)
            {
                var methods = ((MethodInfo)selectedMember).DeclaringType.GetMethodsOfType(true, true).Where(m => m.Name == selectedMember.Name);
                string methodSignatures = string.Join(Environment.NewLine, methods.Select(m =>
                {
                    string sig = m.ToSignatureString();
                    var attr = m.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                    if (attr != null)
                        sig += Environment.NewLine + ((DescriptionAttribute)attr).Description;
                    return sig;
                }).ToArray());

                //signatures.Hide(this);
                Point p = new Point(0, 0);
                p.Offset(-this.Left, -this.Top);

                signatures.RemoveAll();
                signatures.Show(methodSignatures, this, new Point(this.Width, 0));
            }
        }

        public int MaxItemsShown { get; set; }

        public new void Hide()
        {
            base.Hide();
            signatures.Hide(this);
        }

        internal void SelectCurrentPart(string part)
        {
            for (int i = 0; i < lstItems.Items.Count; i++)
            {
                ListItem lstItem = (ListItem)lstItems.Items[i];

                if (lstItem.ToString().ToLower().StartsWith(part.ToLower()))
                {
                    int nrItemsVisible = lstItems.Height / lstItems.ItemHeight;

                    lstItems.BeginUpdate();

                    // scroll down so that the selection is in center
                    if (i + nrItemsVisible / 2 < ItemCount)
                        lstItems.SelectedIndex = i + nrItemsVisible / 2;

                    lstItems.SelectedIndex = i;

                    lstItems.EndUpdate();

                    return;
                }

            }
        }


        public MemberInfo GetSelectedMember()
        {
            if (lstItems.SelectedIndex >= 0 && lstItems.SelectedIndex < lstItems.Items.Count)
            {
                return ((ListItem)lstItems.Items[lstItems.SelectedIndex]).Member;
            }

            return null;
        }

        public int ItemCount { get { return lstItems.Items.Count; } }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (GetSelectedMember() != null)
                {
                    EventHandler temp = MemberChosen;
                    if (temp != null)
                    {
                        temp(this, EventArgs.Empty);
                    }
                }

            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public event EventHandler MemberChosen;

        public void Up()
        {
            if (lstItems.SelectedIndex > 0)
                lstItems.SelectedIndex--;
        }

        public void Down()
        {
            if (lstItems.SelectedIndex + 1 < ItemCount)
                lstItems.SelectedIndex++;
        }


        public void PageUp()
        {
            int nrItemsVisible = lstItems.Height / lstItems.ItemHeight;

            int idx = lstItems.SelectedIndex - nrItemsVisible;
            if (idx < 0)
                idx = 0;
            if (ItemCount > 0)
                lstItems.SelectedIndex = idx;
        }

        public void PageDown()
        {
            int nrItemsVisible = lstItems.Height / lstItems.ItemHeight;

            int idx = lstItems.SelectedIndex + nrItemsVisible;
            if (idx >= ItemCount)
                lstItems.SelectedIndex = ItemCount - 1;

            if (idx < 0)
                idx = 0;

            lstItems.SelectedIndex = idx;
        }

        public override bool Focused
        {
            get
            {
                return base.Focused || lstItems.Focused;
            }
        }
    }

    internal class NoFlickerListBox : ListBox
    {
        public NoFlickerListBox()
        {
            DoubleBuffered = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg != 0x14)
                base.WndProc(ref m);
        }
    }

}
