using System;
using System.Diagnostics;
using System.Linq;

namespace RevolutionaryStuff.Core.Diagnostics
{
    public class ConsoleTraceListener : TraceListenerBase
    {
        private static void SetCategoryColor(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case TraceEventType.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case TraceEventType.Information:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case TraceEventType.Resume:
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case TraceEventType.Start:
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case TraceEventType.Stop:
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case TraceEventType.Suspend:
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case TraceEventType.Transfer:
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case TraceEventType.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    break;
                case TraceEventType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    return;
            }
        }

        private static readonly int MaxEventTypeStringLen = Stuff.GetEnumValues<TraceEventType>().ConvertAll(z => z.ToString().Length).Max();

        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            var indent = new string(' ', this.IndentLevel * this.IndentSize);
            var eventTypeString = eventType.ToString();
            eventTypeString += new string(' ', MaxEventTypeStringLen - eventTypeString.Length);
            var msg = $"{eventTypeString} - {indent}{message}";
            lock (this)
            {
                try
                {
                    SetCategoryColor(eventType);
                    Console.WriteLine(msg);
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }
        /*

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            base.TraceData(eventCache, source, eventType, id, data);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        public override void WriteLine(string message, string category)
        {
            lock (this)
            {
                try
                {
                    var c = GetCategoryColor(category);
                    if (c != null)
                    {
                        Console.ForegroundColor = c.Value;
                    }
                    base.WriteLine(message, category);
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }

        public override void Write(string message, string category)
        {
            lock (this)
            {
                try
                {
                    var c = GetCategoryColor(category);
                    if (c != null)
                    {
                        Console.ForegroundColor = c.Value;
                    }
                    base.Write(message, category);
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }

        public override void WriteLine(string message)
            => Console.WriteLine(message);

        public override void Write(string message)
            => Console.Write(message);
            */
    }
}
