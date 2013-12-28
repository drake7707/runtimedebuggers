using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.ConsoleDebugger
{
    abstract class AbstractDebugWriter : TextWriter
    {

        private StringBuilder str;

        protected TextWriter stdOutput;

        private StringBuilder buffer;


        public AbstractDebugWriter()
        {
            try
            {
                Bind();

                str = new StringBuilder();
                buffer = new StringBuilder();
                PrefixTimeAndPlace = true;

            }
            catch (Exception)
            {
                // it's a debugger, make sure it doesn't crash the application
            }
        }

        public abstract void Bind();

        private void WritePrefix()
        {
            if (PrefixTimeAndPlace)
            {
                StackTrace trace = new StackTrace();

                var frame = GetCallingFrame(trace);
                string prefix = "[" + DateTime.Now.ToString("HH:mm:ss") + " @ " + frame.GetMethod().GetName(true) + "] ";

                foreach (char c in prefix.ToCharArray())
                    WriteToBuffer(c);
            }
        }

        protected abstract StackFrame GetCallingFrame(StackTrace trace);

        public bool PrefixTimeAndPlace { get; set; }

        public override void Write(char value)
        {
            try
            {
                // start of new line, prefix it if necessary
                if (str.Length == 0 || (str.Length >= 2 && str[str.Length - 2] == '\r' && str[str.Length - 1] == '\n'))
                    WritePrefix();

                WriteToBuffer(value);
            }
            catch (Exception)
            {
                // it's a debugger, make sure it doesn't crash the application
            }
        }

        private void WriteToBuffer(char value)
        {
            base.Write(value);

            lock (str)
            {
                str.Append(value);

                if (str.Length > 10485760)
                    str.Remove(0, 1000);
            }

            lock (buffer)
            {
                buffer.Append(value);

                if (buffer.Length > 10485760)
                    buffer.Remove(0, 1000);
            }

            // forward to actual output
            try
            {
                var forwardTo = stdOutput;
                if (forwardTo != null)
                    forwardTo.Write(value);
            }
            catch (Exception)
            { }
        }

        public override string ToString()
        {
            lock (str)
                return str.ToString();
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.ASCII; }
        }

        internal string DequeueBuffer()
        {
            try
            {
                lock (buffer)
                {
                    string strbuffer = buffer.ToString();
                    buffer = new StringBuilder();
                    return strbuffer;
                }
            }
            catch (Exception)
            {
                // it's a debugger, make sure it doesn't crash the application
                return "";
            }
        }
    }
}
