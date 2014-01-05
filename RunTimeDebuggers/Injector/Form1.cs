using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
namespace Injector
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            grid.AutoGenerateColumns = false;


            
            
        }


        private void btnRefresh_Click(object sender, EventArgs e)
        {
            FillWindowList();
        }



        private void FillWindowList()
        {


            List<Info> lst = new List<Info>();

            HashSet<int> processes = new HashSet<int>();
            foreach (IntPtr windowHandle in NativeMethods.ToplevelWindows)
            {
                WindowInfo window = new WindowInfo(windowHandle);
                //if(window.IsValidProcess)

                if (!processes.Contains(window.OwningProcess.Id))
                {
                    HashSet<CLRVersion> versions = new HashSet<CLRVersion>();
                    try
                    {
                        var pmodules = window.OwningProcess.Modules.Cast<System.Diagnostics.ProcessModule>().ToArray();

                        var allModules = NativeMethods.GetProcessModules(window.HWND);

                        foreach (var pmodule in allModules)
                        {
                            string moduleName = string.IsNullOrEmpty(pmodule) ? "" : System.IO.Path.GetFileName(pmodule);

                            var fi = new System.IO.FileInfo(pmodule);
                            if (fi.Directory.Parent.Name.Contains("Framework"))
                            {
                                if (moduleName.ToLower() == "mscorwks.dll" || moduleName.ToLower() == "clr.dll")
                                {
                                    var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(pmodule);

                                    if (version.FileMajorPart == 4)
                                        versions.Add(CLRVersion.v4_0);
                                    else if (version.FileMajorPart == 2)
                                        versions.Add(CLRVersion.v2_0);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        versions.Add(CLRVersion.Unknown);
                    }

                    lst.Add(new Info()
                    {
                        Window = window.Description,
                        Process = window.OwningProcess.ProcessName,
                        CLR = string.Join(", ", versions.Select(v => v + "").ToArray()),
                        CLRVersions = versions.ToList(),
                        IsX64 = NativeMethods.Is64BitProcess(window.OwningProcess),

                        WindowInfo = window,
                        //    Type = managedProcesses.Contains(window.OwningProcess.Id) ? "Managed" : "Unmanaged"
                    });



                    processes.Add(window.OwningProcess.Id);
                }

            }

            grid.DataSource = lst.OrderBy(i => i.Process).ToArray();
        }

        private class Info
        {
            public string Window { get; set; }
            public string Process { get; set; }
            public string CLR { get; set; }
            public List<CLRVersion> CLRVersions { get; set; }
            public bool IsX64 { get; set; }

            public string Bitness { get { return IsX64 ? "x64" : "x86"; } }

            public string Type { get; set; }

            public WindowInfo WindowInfo { get; set; }
        }
        private enum CLRVersion
        {
            Unknown,
            v2_0,
            v4_0
        }
        private enum FileBitness
        {
            x86,
            x64
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            var selectedRow = grid.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            if (selectedRow == null)
            {
                MessageBox.Show("No window selected");
                return;
            }

            var wnd = ((Info)selectedRow.DataBoundItem);

            if (wnd.CLRVersions.Count > 1)
            {
                // if there are more than 1 loaded CLR runtime, ask which one needs to be injected for
            }
            else
            {
                this.BeginInvoke(new Action(() =>
                {
                    Inject(wnd, wnd.CLRVersions.First());
                }));
            }
        }

        private void Inject(Info wnd, CLRVersion version)
        {
            string runtimeDebuggerDll = GetRuntimeDebuggerAssemblyLocation(wnd.IsX64, version);

            if (!string.IsNullOrEmpty(runtimeDebuggerDll))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = runtimeDebuggerDll,
                    Arguments = wnd.WindowInfo.HWND.ToString(),

                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                });
            }
        }

        private static string GetRuntimeDebuggerAssemblyLocation(bool isX64, CLRVersion version)
        {
            string runtimeDebuggerDll = "";
            if (isX64)
            {
                if (version == CLRVersion.v2_0)
                    runtimeDebuggerDll = "RunTimeDebuggers_clr20x64.exe";
                else if (version == CLRVersion.v4_0)
                    runtimeDebuggerDll = "RunTimeDebuggers_clr40x64.exe";
            }
            else
            {
                if (version == CLRVersion.v2_0)
                    runtimeDebuggerDll = "RunTimeDebuggers_clr20x86.exe";

                else if (version == CLRVersion.v4_0)
                    runtimeDebuggerDll = "RunTimeDebuggers_clr40x86.exe";
            }
            if (string.IsNullOrEmpty(runtimeDebuggerDll))
                return "";

            runtimeDebuggerDll = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), runtimeDebuggerDll);
            return runtimeDebuggerDll;
        }

        private void btnTestAssemblyExplorer_Click(object sender, EventArgs e)
        {
            // ensure test assembly is loaded
            TestForm.MainForm frm = new TestForm.MainForm();
            frm.Show();

            //RunTimeDebuggers.Program.InjectedMain();
            // invoke injector on own process
            string runtimeDebuggerDll = GetRuntimeDebuggerAssemblyLocation(IntPtr.Size == 8 ? true : false, Environment.Version.Major == 4 ? CLRVersion.v4_0 : CLRVersion.v2_0);
            Assembly ass = Assembly.LoadFile(runtimeDebuggerDll);
            ass.GetType("RunTimeDebuggers.Program").GetMethod("InjectedMain", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

        }
    }
}
