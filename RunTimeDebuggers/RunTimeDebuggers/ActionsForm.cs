using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.LocalsDebugger;

namespace RunTimeDebuggers
{
    public partial class ActionsForm : Form
    {

        public ActionsForm()
        {

            InitializeComponent();

            lblExecutionPlace.Text = "Executing in process: " + System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            this.Text = "Actions for " + System.Diagnostics.Process.GetCurrentProcess().ProcessName + " - created by drake7707";


            FillForms();
        }

        private void btnOpenAssemblyExplorer_Click(object sender, EventArgs e)
        {
            AssemblyExplorer.AssemblyExplorer.Open();
        }




        private void FillForms()
        {
            try
            {
                lstOpenForms.Items.Clear();

                lstOpenForms.BeginUpdate();
                foreach (Form frm in Application.OpenForms)
                    lstOpenForms.Items.Add(new FormItem(frm));

                lstOpenForms.EndUpdate();
            }
            catch (Exception)
            {

            }
        }

        private class FormItem
        {
            public FormItem(Form frm)
            {
                this.Form = frm;
            }
            public Form Form { get; set; }

            public override string ToString()
            {
                return Form.GetType().Name + " - [Text: " + Form.Text + "]";
            }
        }

        private void btnOpenLocals_Click(object sender, EventArgs e)
        {
            if (lstOpenForms.SelectedIndex >= 0)
            {
                Form frm = ((FormItem)lstOpenForms.Items[lstOpenForms.SelectedIndex]).Form;
                LocalsWindow dlg = new LocalsWindow("", frm);
                dlg.FormClosed += (s, ev) => dlg.Dispose();
                dlg.Show();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            FillForms();
        }

        private void btnConsole_Click(object sender, EventArgs e)
        {
            ConsoleDebugger.ConsoleDebugger.Instance.OpenForm();
        }

        private void btnOpenEmptyWatchWindow_Click(object sender, EventArgs e)
        {
            LocalsWindow dlg = new LocalsWindow("", null);
            dlg.Show();
            dlg.FormClosed += (s, ev) => dlg.Dispose();
        }
    }
}
