using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers
{
    class MissingAssemblyManager
    {

        public static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
        }

        static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return ResolveAssembly(args);
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return ResolveAssembly(args);
        }


        private static Assembly ResolveAssembly(ResolveEventArgs args)
        {
            if (args.Name.Contains(".resources"))
                return null;

            if (args.Name.Contains("NodeControl"))
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RunTimeDebuggers.Assemblies.NodeControl.dll");
                byte[] bytes = stream.ReadToEnd();
                
                    return Assembly.Load(bytes);
                
            }

            var result = MessageBox.Show("Unable to load '" + args.Name + "', do you want to select the required file manually?", "Locate assembly", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Yes)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Open assembly '" + args.Name + "'";
                    ofd.Filter = "*.dll;*.exe|*.dll;*.exe";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            return Assembly.LoadFile(ofd.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Unable to load assembly: " + ex.GetType().FullName + " - " + ex.Message);
                            return null;
                        }
                    }
                    else
                        return null;
                }
            }
            else
                return null;
        }
    }
}
