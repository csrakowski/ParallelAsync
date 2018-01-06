using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Helpers
{
    /// <summary>
    /// The <see cref="EventSource"/> for <see cref="ParallelAsync"/>
    /// </summary>
    [EventSource(Name = nameof(ParallelAsync))]
    internal sealed class ParallelAsyncEventSource : EventSource
    {
        /// <summary>
        /// The <see cref="ParallelAsyncEventSource"/> instance used for logging
        /// </summary>
        public static ParallelAsyncEventSource Log = new ParallelAsyncEventSource();

        /// <summary>
        /// Writes a <c>RunStart</c> event
        /// </summary>
        /// <param name="maxBatchSize">The value of maxBatchSize for the run</param>
        /// <param name="allowOutOfOrderProcessing">The value of allowOutOfOrderProcessing for the run</param>
        /// <param name="estimatedResultSize">The value of estimatedResultSize for the run</param>
        [Event(1, Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void RunStart(int maxBatchSize, bool allowOutOfOrderProcessing, int estimatedResultSize)
        {
            WriteEvent(1, maxBatchSize, allowOutOfOrderProcessing, estimatedResultSize);
        }

        /// <summary>
        /// Writes a <c>RunStop</c> event
        /// </summary>
        [Event(2, Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void RunStop()
        {
            WriteEvent(2);
        }

        /// <summary>
        /// Writes a <c>BatchStart</c> event
        /// </summary>
        /// <param name="batchSize">The size of the current batch</param>
        [Event(3, Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void BatchStart(int batchSize)
        {
            if (IsEnabled())
            {
                WriteEvent(3, batchSize);
            }
        }

        /// <summary>
        /// Writes a <c>BatchStop</c> event
        /// </summary>
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
