using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    public class ILDebugManager
    {

        private static ILDebugManager instance;

        public static ILDebugManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new ILDebugManager();

                return instance;
            }
        }

        private ILDebugManager()
        {
        }

        private ILDebugger debugger;

        public ILDebugger Debugger
        {
            get { return debugger; }
            set
            {
                bool changed = debugger != value;
                debugger = value;
                if (changed)
                {
                    EventHandler temp = DebuggerChanged;
                    if (temp != null)
                        temp(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler DebuggerChanged;

        public event EventHandler BreakpointHit;

        public event EventHandler Stepped;

        private ILInstruction currentBreakedInstruction = null;

        public void Run()
        {
            if (debugger != null)
            {


                if (!debugger.Returned)
                {
                    var breakpoints = BreakpointManager.Instance.GetBreakpoints(debugger.CurrentMethod);
                    if (currentBreakedInstruction != debugger.CurrentInstruction &&  breakpoints.Contains(debugger.CurrentInstruction.Offset))
                    {
                        currentBreakedInstruction = debugger.CurrentInstruction;
                        OnBreakPointHit();
                        return;
                    }
                }
                
                while (!debugger.Returned)
                {
                    debugger.Next(ILDebugger.StepEnum.StepInto);
                    OnStepped();

                    if (!debugger.Returned)
                    {
                        var breakpoints = BreakpointManager.Instance.GetBreakpoints(debugger.CurrentMethod);
                        if (breakpoints.Contains(debugger.CurrentInstruction.Offset))
                        {
                            currentBreakedInstruction = debugger.CurrentInstruction;
                            OnBreakPointHit();
                            return;
                        }
                    }

                }
            }
        }

        private void OnBreakPointHit()
        {
            EventHandler temp = BreakpointHit;
            if (temp != null)
                temp(this, EventArgs.Empty);
        }

        private void OnStepped()
        {
            EventHandler temp = Stepped;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        public void StepInto()
        {
            if (debugger != null)
            {
                if (!debugger.Returned)
                {
                    debugger.Next(ILDebugger.StepEnum.StepInto);
                    OnStepped();
                }
            }
        }
    }


}
