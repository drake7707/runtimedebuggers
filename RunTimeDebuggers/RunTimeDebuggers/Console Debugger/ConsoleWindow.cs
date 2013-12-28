using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RunTimeDebuggers.ConsoleDebugger
{
    partial class ConsoleWindow : Form
    {
        private ConsoleWriter consoleWriter;
        private TraceWriter traceWriter;
        //private DebugWriter debugWriter;

        public static ConsoleWindow ActiveWindow { get; private set; }

        public ConsoleWindow(ConsoleWriter consoleWriter, TraceWriter traceWriter)//, DebugWriter debugWriter)
        {
            this.consoleWriter = consoleWriter;
            this.traceWriter = traceWriter;
            //this.debugWriter = debugWriter;

            InitializeComponent();

            tmr_Tick(tmr, EventArgs.Empty);

            ActiveWindow = this;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            ActiveWindow = null;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            tmr.Enabled = !btnPause.Checked;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (tabs.SelectedIndex == 0)
            {
                if (!string.IsNullOrEmpty(txtConsole.Text))
                    Clipboard.SetText(txtConsole.Text);
            }
            else if (tabs.SelectedIndex == 1)
            {
                if (!string.IsNullOrEmpty(txtTrace.Text))
                    Clipboard.SetText(txtTrace.Text);
            }
            //else if (tabs.SelectedIndex == 2)
            //{
            //    if (!string.IsNullOrEmpty(txtDebug .Text))
            //        Clipboard.SetText(txtDebug.Text);
            //}
        }

        private void tmr_Tick(object sender, EventArgs e)
        {
            // ensure output passes through the consolewriter
            consoleWriter.Bind();
            traceWriter.Bind();


            string consoleBuffer = consoleWriter.DequeueBuffer();
            if (!string.IsNullOrEmpty(consoleBuffer))
            {                
                txtConsole.AppendText(consoleBuffer);
                txtConsole.SelectionStart = txtConsole.Text.Length;
                txtConsole.ScrollToCaret();
            }
            

            string traceBuffer = traceWriter.DequeueBuffer();
            if (!string.IsNullOrEmpty(traceBuffer))
            {
                txtTrace.AppendText(traceBuffer);
                txtTrace.SelectionStart = txtTrace.Text.Length;
                txtTrace.ScrollToCaret();
            }

            //string debugBuffer = debugWriter.DequeueBuffer();
            //if (!string.IsNullOrEmpty(debugBuffer))
            //{
            //    txtDebug.AppendText(debugBuffer);
            //    txtDebug.SelectionStart = txtDebug.Text.Length;
            //    txtDebug.ScrollToCaret();
            //}
        }

        private void btnShowPrefix_Click(object sender, EventArgs e)
        {
            consoleWriter.PrefixTimeAndPlace = btnShowPrefix.Checked;
            traceWriter.PrefixTimeAndPlace = btnShowPrefix.Checked;
        }
    }
}
