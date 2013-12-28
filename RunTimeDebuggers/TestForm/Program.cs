using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace TestForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            
        }


    }
}
