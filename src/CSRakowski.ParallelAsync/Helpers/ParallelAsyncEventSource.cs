using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Helpers
{
    [EventSource(Name = nameof(ParallelAsync))]
    public sealed class ParallelAsyncEventSource : EventSource
    {
        [Event(1, Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void RunStart(int maxBatchSize, bool allowOutOfOrderProcessing, int estimatedResultSize)
        {
            WriteEvent(1, maxBatchSize, allowOutOfOrderProcessing, estimatedResultSize);
        }

        [Event(2, Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void RunStop()
        {
            WriteEvent(2);
        }

        [Event(3, Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void BatchStart(int batchSize)
        {
            if (IsEnabled())
            {
                WriteEvent(3, batchSize);
            }
        }

        [Event(4, Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void BatchStop()
        {
            if (IsEnabled())
            {
                WriteEvent(4);
            }
        }
    }
}
