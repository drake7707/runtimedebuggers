using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Text;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace Injector
{
    // *******************************************************************************************
    // NOTE: All of the code in this file came from Pete Blois's excellent utility called Snoop.
    // You can download Snoop, the runtime WPF application viewer, here: http://blois.us/Snoop/
    // I am using Mr. Blois's code with his permission.
    // - Josh Smith 10/2008
    // *******************************************************************************************

    public static class NativeMethods
    {
        public static IntPtr[] ToplevelWindows
        {
            get
            {
                List<IntPtr> windowList = new List<IntPtr>();
                GCHandle handle = GCHandle.Alloc(windowList);
                try
                {
                    NativeMethods.EnumWindows(NativeMethods.EnumWindowsCallback, (IntPtr)handle);
                }
                finally
                {
                    handle.Free();
                }

                return windowList.ToArray();
            }
        }

        public static Process GetWindowThreadProcess(IntPtr hwnd)
        {
            int processID;
            NativeMethods.GetWindowThreadProcessId(hwnd, out processID);

            try
            {
                return Process.GetProcessById(processID);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static bool Is64BitProcess(Process process)
        {
            if (IntPtr.Size == 4) // injector is running in 32-bit, while compiled to AnyCpu. This means it's either forced to x86 or the it's a 32 bit OS. Not really sure if this will do
                return false;

            bool isWow64Process;
            if (!IsWow64Process(process.Handle, out isWow64Process))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return !isWow64Process;
        }

        // Microsoft.Win32.Win32Native
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        // Microsoft.Win32.Win32Native
        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string methodName);

        // Microsoft.Win32.Win32Native
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        internal static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            IntPtr procAddress = GetProcAddress(moduleHandle, methodName);
            return procAddress != IntPtr.Zero;
        }


        // Microsoft.Win32.Win32Native
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr hSourceProcessHandle, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);

        private delegate bool EnumWindowsCallBackDelegate(IntPtr hwnd, IntPtr lParam);
        [DllImport("user32.Dll")]
        private static extern int EnumWindows(EnumWindowsCallBackDelegate callback, IntPtr lParam);

        [DllImport("user32.Dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int processId);

        private static bool EnumWindowsCallback(IntPtr hwnd, IntPtr lParam)
        {
            ((List<IntPtr>)((GCHandle)lParam).Target).Add(hwnd);
            return true;
        }



        public const uint LIST_MODULES_ALL = 0x03;

        [DllImport("psapi.dll")]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] IntPtr[] lphModule,
            int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName,
            [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("User32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public const uint PROCESS_ALL_ACCESS = (uint)(0x000F0000L | 0x00100000L | 0xFFF);

        [DllImport("kernel32")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        public static List<string> GetProcessModules(IntPtr windowHwnd)
        {
            List<string> modules = new List<string>();

            // -- Get process ID
            uint processID = 0;
            GetWindowThreadProcessId(windowHwnd, out processID);

            // -- Get process handle
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processID);
            if (hProcess != IntPtr.Zero)
            {
                IntPtr[] modhWnds = new IntPtr[0];
                int lpcbNeeded = 0;

                try
                {
                    // -- call EnumProcessModules the first time to get the size of the array needed
                    EnumProcessModulesEx(hProcess, modhWnds, 0, out lpcbNeeded, LIST_MODULES_ALL);

                    modhWnds = new IntPtr[lpcbNeeded / IntPtr.Size];
                    EnumProcessModulesEx(hProcess, modhWnds, modhWnds.Length * IntPtr.Size, out lpcbNeeded, LIST_MODULES_ALL);
                }
                // -- On a 32 bit machine EnumProcessModulesEx may not exist in psapi.dll but this is fine as
                //    GetProcessModules is intended to be used on a 64 bit OS
                catch (EntryPointNotFoundException)
                {
                    return modules;
                }

                for (int i = 0; i < modhWnds.Length; i++)
                {
                    StringBuilder modName = new StringBuilder(256);
                    if (GetModuleFileNameEx(hProcess, modhWnds[i], modName, modName.Capacity) != 0)
                        modules.Add(modName.ToString());
                }
            }

            if (hProcess != IntPtr.Zero)
                CloseHandle(hProcess);

            return modules;
        }

 
    }
}