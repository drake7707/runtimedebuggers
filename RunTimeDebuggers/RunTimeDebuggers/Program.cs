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
        [STAThread]
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
            {
                MessageBox.Show("No hwnd specified, opening actions form on local assembly");
                Initialize(true);
            }
        }

        public static void InjectedMain()
        {

            Initialize(false);
        }

        public static void Initialize(bool showActionsFormAsDialog)
        {
            ConsoleDebugger.ConsoleDebugger.Instance.Initialize();
            // ensure missing assemblies are loaded
            MissingAssemblyManager.Initialize();

            if (showActionsFormAsDialog)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ActionsForm());
                
            }
            else
            {
                ActionsForm dlg = new ActionsForm();
                dlg.Show();
                dlg.FormClosed += (s, ev) => dlg.Dispose();
            }
        }
    }
}
