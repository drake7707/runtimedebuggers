using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RunTimeDebuggers.ConsoleDebugger
{
    class TraceWriter : AbstractDebugWriter
    {

        protected DebugTraceListener traceListener;
        public TraceWriter()
            : base()
        {

        }

        public override void Bind()
        {
            if (traceListener == null)
                traceListener = new DebugTraceListener(this);

            var listeners = System.Diagnostics.Trace.Listeners;
            if (!listeners.Contains(traceListener))
                listeners.Add(traceListener);
        }

        private Type traceInternal;
        protected override StackFrame GetCallingFrame(StackTrace trace)
        {
            if (traceInternal == null)
                traceInternal = System.Reflection.Assembly.GetAssembly(typeof(System.Diagnostics.Trace)).GetType("System.Diagnostics.TraceInternal");
            var frame = trace.GetFrames().SkipWhile((f, idx) =>
                                            {
                                                var method = f.GetMethod();
                                                if (method.DeclaringType != null)
                                                {
                                                    if (method.DeclaringType.GUID != typeof(System.Diagnostics.Debug).GUID &&
                                                        method.DeclaringType.GUID != typeof(System.Diagnostics.Trace).GUID)
                                                    {
                                                        if (method.DeclaringType.GUID != traceInternal.GUID)
                                                            return true;
                                                        else
                                                        {
                                                            if (idx + 1 < trace.FrameCount && trace.GetFrame(idx + 1).GetMethod().DeclaringType.GUID != typeof(Trace).GUID &&
                                                                                              trace.GetFrame(idx + 1).GetMethod().DeclaringType.GUID != typeof(Debug).GUID)
                                                                return false;
                                                            else
                                                                return true;
                                                        }
                                                    }
                                                }
                                                return false;
                                            })
                                            .Skip(1)
                                            .First();

            return frame;
        }


        protected class DebugTraceListener : System.Diagnostics.TraceListener
        {
            private TraceWriter writer;
            public DebugTraceListener(TraceWriter writer)
            {
                this.writer = writer;
            }

            public override void Write(object o)
            {
                writer.Write(o);
                base.Write(o);
            }
            public override void Write(object o, string category)
            {
                writer.Write("[" + category + "]" + o);
                base.Write(o, category);
            }

            public override void Write(string message)
            {
                writer.Write(message);
            }

            public override void Write(string message, string category)
            {
                writer.Write("[" + category + "]" + message);
                base.Write(message, category);
            }
            public override void WriteLine(string message)
            {
                writer.WriteLine(message);
            }

            public override void WriteLine(object o)
            {
                writer.WriteLine(o);
            }

            public override void WriteLine(object o, string category)
            {
                writer.WriteLine("[" + category + "]" + o);
                base.WriteLine(o, category);
            }

            public override void WriteLine(string message, string category)
            {
                base.WriteLine(message, category);
            }
        }
    }
}
