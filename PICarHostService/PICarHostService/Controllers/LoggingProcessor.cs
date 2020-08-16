using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICarHostService
{

    class LoggingProcessor
    {
        public static bool TracingEnabled { get; set; }
        private static List<TraceMessage> traceMessages { get; set; } = new List<TraceMessage>();
        private static readonly object lockObject = new object();

        static LoggingProcessor()
        {
            TracingEnabled = false;
        }

        public static void AddTrace(String traceMessage)
        {
            if (TracingEnabled)
            {
                lock (lockObject)
                {
                    var traceMess = new TraceMessage { index = traceMessages.Count, Message = traceMessage, Time = DateTime.Now };
                    traceMessages.Add(traceMess);
                }
            }


        }

        public static void ClearTraceMessages()
        {
            lock(lockObject)
            {
                traceMessages.Clear();
            }

        }

        public static List<String> GetNewTraceMessages()
        {
            lock (lockObject)
            {
                var messStringLst = new List<String>();
                if (TracingEnabled)
                {
                    messStringLst.AddRange(traceMessages.Select(x => x.ToString()).ToList());
                    traceMessages.Clear();
                    return messStringLst;
                }
                return messStringLst;
            }

        }
    }
}
