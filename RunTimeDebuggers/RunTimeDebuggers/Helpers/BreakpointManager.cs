using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace RunTimeDebuggers.Helpers
{
    class BreakpointManager
    {

        private static BreakpointManager instance;

        public static BreakpointManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new BreakpointManager();

                return instance;
            }
        }

        private BreakpointManager()
        {
            Breakpoints = new HashSet<Breakpoint>();
        }

        private HashSet<Breakpoint> Breakpoints { get; set; }

        private struct Breakpoint
        {
            public Breakpoint(MethodBase method, int ilOffset)
            {
                this.assembly = method.Module.Assembly.GetHashCode();
                this.methodtoken = method.MetadataToken;
                this.ilOffset = ilOffset;
            }

            private int assembly;
            private int methodtoken;

            public int MethodToken
            {
                get { return methodtoken; }

            }
            private int ilOffset;

            public int ILOffset
            {
                get { return ilOffset; }
            }
        }

        public HashSet<int> GetBreakpoints(MethodBase method)
        {
            return new HashSet<int>(Breakpoints.Where(b => b.MethodToken == method.MetadataToken)
                                               .Select(b => b.ILOffset));
        }

        public void AddBreakpoint(MethodBase method, int ilOffset)
        {
            Breakpoint b = new Breakpoint(method, ilOffset);

            if (!Breakpoints.Contains(b))
            {
                Breakpoints.Add(b);

                BreakpointHandler temp = BookmarkAdded;
                if (temp != null)
                    temp(method, ilOffset);
            }
        }

        public void RemoveBreakpoint(MethodBase method, int ilOffset)
        {
            Breakpoint b = new Breakpoint(method, ilOffset);
            if (Breakpoints.Contains(b))
            {
                Breakpoints.Remove(b);

                BreakpointHandler temp = BookmarkRemoved;
                if (temp != null)
                    temp(method, ilOffset);
            }
        }

        public event BreakpointHandler BookmarkAdded;
        public event BreakpointHandler BookmarkRemoved;

        public delegate void BreakpointHandler(MethodBase method, int ilOffset);


        public void ToggleBreakpoint(MethodBase method, int ilOffset)
        {
            Breakpoint b = new Breakpoint(method, ilOffset);
            if (!Breakpoints.Contains(b))
                Breakpoints.Add(b);
            else
                Breakpoints.Remove(b);
        }
    }
}
