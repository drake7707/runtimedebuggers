using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.ConsoleDebugger
{

    class ConsoleWriter : AbstractDebugWriter
    {

        private object thisWriter;
        public ConsoleWriter()
        {

        }

        public override void Bind()
        {
            if (Console.Out != thisWriter)
            {
                stdOutput = Console.Out;
                Console.SetOut(this);
                thisWriter = Console.Out; // console wraps this in a synctextwriter in SetOut, so compare with that one
            }
        }

        private Type syncTextWriter;

        protected override StackFrame GetCallingFrame(StackTrace trace)
        {
            if (syncTextWriter == null)
                syncTextWriter = typeof(TextWriter).GetNestedType("SyncTextWriter", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

            var frame = trace.GetFrames().SkipWhile((f, idx) =>
                                            {
                                                var method = f.GetMethod();
                                                if (method.DeclaringType != null)
                                                {

                                                    if (method.DeclaringType.GUID != typeof(Console).GUID)
                                                    {
                                                        if (method.DeclaringType.GUID != syncTextWriter.GUID)
                                                            return true;
                                                        else
                                                        {
                                                            if (idx + 1 < trace.FrameCount && trace.GetFrame(idx + 1).GetMethod().DeclaringType.GUID != typeof(Console).GUID)
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

    }
}
