using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.Timers
{
    public class Timer : IDisposable
    {
        static StringWriter writer;
        protected List<string> disposed;
        string name;
        int level;
        Timer parent;
        Stopwatch watch;

        public Timer(StringWriter stringWriter, string name, int level, Timer parent = null)
        {
            writer = stringWriter;
            disposed = new List<string>();
            this.name = name;
            this.level = level;
            this.parent = parent;
            watch = new Stopwatch();
            watch.Start();
        }

        public static Timer Start(StringWriter writer, string name = "")
        {
            return new Timer(writer, name, 0);
        }

        public Timer StartChildTimer(string name)
        {
            var timer = new Timer(writer, name, level + 1, this);
            return timer;
        }

        private static string FormatReportLine(string timerName, int level, long value)
        {
            var intro = new string(' ', level * 4) + timerName;
            return $"{intro,-20}: {value}\n";
        }

        private bool isDisposed = false;

        ~Timer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); //финализатор не будет вызываться
        }

        protected virtual void Dispose(bool fromDisposeMethod)
        {
            if (!isDisposed)
            {
                watch.Stop();
                long value = watch.ElapsedMilliseconds;
                var text = FormatReportLine(name == "" ? "*" : name, level, value);

                if (parent != null)
                {
                    parent.disposed.Add(text);
                    parent.disposed.AddRange(disposed);
                }
                else
                {
                    writer.Write(text);
                    disposed.ForEach(d => writer.Write(d));
                }

                if (disposed.Count != 0) 
                    WriteRestText(value);

                isDisposed = true;
            }
        }

        private void WriteRestText(long value)
        {
            long sum = 0;
            foreach (var d in disposed)
            {
                var parts = d.Split();
                var number = parts[parts.Length - 2];
                sum += long.Parse(number);
            }

            var restText = FormatReportLine("Rest", level + 1, value - sum);

            if (parent != null) 
                parent.disposed.Add(restText);
            else 
                writer.Write(restText);
        }
    }
}
