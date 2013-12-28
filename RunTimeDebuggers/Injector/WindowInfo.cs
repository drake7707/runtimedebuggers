using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Injector
{
    // *******************************************************************************************
    // NOTE: Much of the code in this file came from Pete Blois's excellent utility called Snoop.
    // You can download Snoop, the runtime WPF application viewer, here: http://blois.us/Snoop/
    // I am using Mr. Blois's code with his permission.
    // - Josh Smith 10/2008
    // *******************************************************************************************

    public class WindowInfo
    {
        [DllImport("psapi.dll"), SuppressUnmanagedCodeSecurity]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess,
                                                       IntPtr hModule,
                                                       [Out] StringBuilder lpBaseName,
                                                       [In] [MarshalAs(UnmanagedType.U4)] uint nSize);

        [DllImport("psapi.dll", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        private static extern bool EnumProcessModules(IntPtr hProcess,
                                                    [In][Out] IntPtr[] lphModule,
                                                    uint cb,
                                                    [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded);

        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurityAttribute]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
                                         [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
                                         int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        private static Dictionary<int, bool> processIDToValidityMap = new Dictionary<int, bool>();

        readonly IntPtr _hwnd;

        public WindowInfo(IntPtr hwnd)
        {
            _hwnd = hwnd;
        }

        public static void ClearCachedProcessInfo()
        {
            WindowInfo.processIDToValidityMap.Clear();
        }

        public bool IsValidProcess
        {
            get
            {
                bool isValid = false;
                try
                {
                    if (_hwnd == IntPtr.Zero)
                        return false;

                    Process process = this.OwningProcess;
                    if (process == null)
                        return false;

                    if (WindowInfo.processIDToValidityMap.TryGetValue(process.Id, out isValid))
                        return isValid;

                    if (process.Id == Process.GetCurrentProcess().Id)
                        isValid = false;
                    else if (process.MainWindowHandle == IntPtr.Zero && !process.ProcessName.StartsWith("PresentationHost"))
                        isValid = false;
                    else if (process.ProcessName.StartsWith("devenv"))
                        isValid = false;
                    else
                    {
                        IntPtr[] lphModule = new IntPtr[2048];
                        uint byteCount = (uint)lphModule.Length * sizeof(uint);
                        uint bytesNeeded;

                        IntPtr hProcess = OpenProcess(ProcessAccessFlags.QueryInformation, true, process.Id);

                        if (hProcess != IntPtr.Zero)
                        {
                            bool enumModuleSuccess = EnumProcessModules(process.Handle, lphModule, byteCount, out bytesNeeded);

                            if (enumModuleSuccess)
                            {
                                uint moduleCount = (uint)(bytesNeeded / IntPtr.Size);

                                for (uint i = 0; i < moduleCount; i++)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    GetModuleFileNameEx(process.Handle, lphModule[i], sb, bytesNeeded);
                                    if (sb.ToString().Contains("mscorlib"))
                                    {
                                        isValid = true;
                                        break;
                                    }
                                }
                            }

                            CloseHandle(hProcess);
                        }
                    }

                    WindowInfo.processIDToValidityMap[process.Id] = isValid;
                }
                catch (Exception) { }
                return isValid;
            }
        }

        public Process OwningProcess
        {
            get { return NativeMethods.GetWindowThreadProcess(_hwnd); }
        }

        public IntPtr HWND
        {
            get { return _hwnd; }
        }

        public string Description
        {
            get
            {
                Process process = this.OwningProcess;

                string title;
                if (process.ProcessName.StartsWith("PresentationHost"))
                    title = "XBAP (must be Full Trust)";
                else
                    title = process.MainWindowTitle;

                return title;// +" - " + process.ProcessName + " [" + process.Id + "]";
            }
        }

        public override string ToString()
        {
            return this.Description;
        }

   
    }
}