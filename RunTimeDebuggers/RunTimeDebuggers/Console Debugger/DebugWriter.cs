using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RunTimeDebuggers.ConsoleDebugger
{
    class DebugWriter : TraceWriter 
    {

        public override void Bind()
        {
            if (traceListener == null)
                traceListener = new DebugTraceListener(this);

            var listeners = System.Diagnostics.Debug.Listeners;
            if (!listeners.Contains(traceListener))
                listeners.Add(traceListener);
        }

        //protected override StackFrame GetCallingFrame(StackTrace trace)
        //{
        //    var frame = trace.GetFrames().SkipWhile(f =>
        //                                    {
        //                                        var method = f.GetMethod();
        //                                        if (method.DeclaringType != null)
        //                                        {
        //                                            if (method.DeclaringType.GUID != typeof(System.Diagnostics.Debug).GUID &&
        //                                                method.DeclaringType.GUID != typeof(System.Diagnostics.Trace).GUID)
        //                                                return true;

                                                    
        //                                        }
        //                                        return false;
        //                                    })
        //                                    .Skip(1)
        //                                    .First();

        //    return frame;
        //}


    }
}
