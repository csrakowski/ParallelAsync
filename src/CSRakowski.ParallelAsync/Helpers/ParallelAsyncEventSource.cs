using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Helpers
{
    [EventSource(Name = nameof(ParallelAsync))]
    internal class ParallelAsyncEventSource : EventSource
    {
        [Event(1, Message = "Starting", Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void RunStart()
        {
            WriteEvent(1);
        }

        [Event(2, Message = "Done", Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void RunStop()
        {
            WriteEvent(2);
        }

        [Event(3, Message = "Starting batch of {0}", Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void BatchStart(int batchSize)
        {
            if (IsEnabled())
            {
                WriteEvent(3, batchSize);
            }
        }

        [Event(4, Message = "Batch done", Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void BatchStop()
        {
            if (IsEnabled())
            {
                WriteEvent(4);
            }
        }
    }
}
