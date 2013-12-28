using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.AssemblyExplorer;
using System.Reflection;


namespace RunTimeDebuggers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1)
            {

                string injectDLLName = "";
                if (IntPtr.Size == 8)
                {
                    if (Environment.Version.Major == 2)
                        injectDLLName = "ManagedInjector_clr20x64.dll";
                    else if (Environment.Version.Major == 4)
                        injectDLLName = "ManagedInjector_clr40x64.dll";
                }
                else
                {
                    if (Environment.Version.Major == 2)
                        injectDLLName = "ManagedInjector_clr20x86.dll";
                    else if (Environment.Version.Major == 4)
                        injectDLLName = "ManagedInjector_clr40x86.dll";
                }

                injectDLLName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), injectDLLName);

                try
                {
                    var ass = Assembly.LoadFile(injectDLLName);
                    ass.GetType("ManagedInjector.Injector")
                       .GetMethod("Inject", BindingFlags.Static | BindingFlags.Public)
                       .Invoke(null, new object[] { (IntPtr.Size == 8 ? new IntPtr(long.Parse(args[0])) : new IntPtr(int.Parse(args[0]))), 
                                                 injectDLLName, 
                                                 Assembly.GetEntryAssembly().Location, typeof(Program).FullName, 
                                                 "InjectedMain" });


                }
                catch (Exception ex)
                {
                    if (ex is TargetInvocationException)
                    {
                        MessageBox.Show("Error: " + ex.InnerException.GetType().FullName + " - " + ex.InnerException.Message);
                    }
                    else
                        MessageBox.Show("Error: " + ex.GetType().FullName + " - " + ex.Message);
                }
            }
            else
                MessageBox.Show("Invalid arguments, usage: " + Environment.GetCommandLineArgs()[0] + " <hwnd>");
        }

        public static void InjectedMain()
        {
            //ILDebugger debugger = new ILDebugger();

            ConsoleDebugger.ConsoleDebugger.Instance.Initialize();
            // ensure missing assemblies are loaded
            MissingAssemblyManager.Initialize(); 


            //var result = MessageBox.Show("Open the actions dialog as modal window? A modal window will retain the call stack to the message pump of the open window, but changes can't be made to the initial form.", "Open as modal dialog?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            //if (result == DialogResult.Yes)
            //{
            //    using (ActionsForm dlg = new ActionsForm())
            //    {
            //        dlg.ShowDialog();
            //    }
            //}
            //else
            //{
                ActionsForm dlg = new ActionsForm();
                dlg.Show();
                dlg.FormClosed += (s, ev) => dlg.Dispose();

                
            //}
        }
    }
}
