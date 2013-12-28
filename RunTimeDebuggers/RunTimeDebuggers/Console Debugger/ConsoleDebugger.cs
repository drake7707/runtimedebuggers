using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace RunTimeDebuggers.ConsoleDebugger
{
    public class ConsoleDebugger
    {

        private static ConsoleDebugger instance;

        public static ConsoleDebugger Instance
        {
            get
            {
                if (instance == null)
                    instance = new ConsoleDebugger();
                return instance;
            }
        }


        private ConsoleWriter consoleWriter;
        private TraceWriter traceWriter;
        //private DebugWriter  debugWriter;

        private ConsoleDebugger()
        {
            consoleWriter = new ConsoleWriter();
            traceWriter = new TraceWriter();
            //debugWriter = new DebugWriter();
        }

        internal void Initialize()
        {
        }

        public Form OpenForm()
        {
            try
            {

                ConsoleWindow cwnd;

                if (ConsoleWindow.ActiveWindow != null)
                    cwnd = ConsoleWindow.ActiveWindow;
                else
                    cwnd = new ConsoleWindow(consoleWriter, traceWriter);

                cwnd.Show();
                cwnd.BringToFront();

                return cwnd;
            }
            catch (Exception)
            {
                // it's a debugger, make sure it doesn't crash the application
                return null;
            }
        }
      
    }


}
