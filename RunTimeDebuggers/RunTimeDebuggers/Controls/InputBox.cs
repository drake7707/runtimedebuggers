using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RunTimeDebuggers.Controls
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.BeginInvoke(new Action(() => txtInput.Focus()));
        }
        public string DialogText
        {
            get { return lblText.Text; }
            set { lblText.Text = value; }
        }

        public string Value
        {
            get { return txtInput.Text; }
            set { txtInput.Text = value; }
        }
        
    }
}
