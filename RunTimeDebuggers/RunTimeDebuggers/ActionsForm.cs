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
            MissingAssemblyManager.Initialize();

            InitializeComponent();

            lblExecutionPlace.Text = "Executing in process: " + System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            this.Text = "Actions for " + System.Diagnostics.Process.GetCurrentProcess().ProcessName;


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
                    lstOpenForms.Items.Add(new WinFormsFormItem(frm));

                foreach (var frm in System.Windows.Application.Current.Windows)
                    lstOpenForms.Items.Add(new WPFFormItem(frm));

            }
            catch (Exception)
            {

            }
            finally
            {
                lstOpenForms.EndUpdate();
            }

            if (lstOpenForms.Items.Count > 0)
                lstOpenForms.SelectedIndex = 0;
        }

        private abstract class FormItem
        {
            public FormItem(object frm)
            {
                this.Form = frm;
            }
            public object Form { get; set; }
        }

        private class WinFormsFormItem : FormItem
        {
            public WinFormsFormItem(object frm)
                : base(frm)
            { }

            public override string ToString()
            {
                return Form.GetType().Name + " - [Text: " + ((System.Windows.Forms.Form)Form).Text + "] <<WinForms>>";
            }
        }

        private class WPFFormItem : FormItem
        {
            public WPFFormItem(object frm)
                : base(frm)
            { }

            public override string ToString()
            {
                return Form.GetType().Name + " - [Text: " + ((System.Windows.Window)Form).Title + "] <<WPF>>";
            }
        }


        private void btnOpenLocals_Click(object sender, EventArgs e)
        {
            if (lstOpenForms.SelectedIndex >= 0)
            {
                object frm = ((FormItem)lstOpenForms.Items[lstOpenForms.SelectedIndex]).Form;
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
