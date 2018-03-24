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
        public static readonly ParallelAsyncEventSource Log = new ParallelAsyncEventSource();

#pragma warning disable CA1822 // Mark members as static

        /// <summary>
        /// Get's a unique number to use as the RunId
        /// </summary>
        /// <returns>A unique number to be used as the RunId</returns>
        public long GetRunId() => DateTime.UtcNow.Ticks;

#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// Writes a <c>RunStart</c> event
        /// </summary>
        /// <param name="runId">The id of the current run</param>
        /// <param name="maxBatchSize">The value of maxBatchSize for the run</param>
        /// <param name="allowOutOfOrderProcessing">The value of allowOutOfOrderProcessing for the run</param>
        /// <param name="estimatedResultSize">The value of estimatedResultSize for the run</param>
        [Event(1, Message = "Starting a new run (id: {0})", Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void RunStart(long runId, int maxBatchSize, bool allowOutOfOrderProcessing, int estimatedResultSize)
        {
            WriteEvent(1, runId, maxBatchSize, allowOutOfOrderProcessing, estimatedResultSize);
        }

        /// <summary>
        /// Writes a <c>RunStop</c> event
        /// </summary>
        /// <param name="runId">The id of the current run</param>
        [Event(2, Message = "Completed run (id: {0})", Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void RunStop(long runId)
        {
            WriteEvent(2, runId);
        }

        /// <summary>
        /// Writes a <c>BatchStart</c> event
        /// </summary>
        /// <param name="runId">The id of the current run</param>
        /// <param name="batchId">The id of the current batch</param>
        /// <param name="batchSize">The size of the current batch</param>
        [Event(3, Message = "Starting a new batch (runId: {0}, batchId: {1})", Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void BatchStart(long runId, int batchId, int batchSize)
        {
            WriteEvent(3, runId, batchId, batchSize);
        }

        /// <summary>
        /// Writes a <c>BatchStop</c> event
        /// </summary>
        /// <param name="runId">The id of the current run</param>
        /// <param name="batchId">The id of the current batch</param>
        [Event(4, Message = "Completed batch (runId: {0}, batchId: {1})", Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void BatchStop(long runId, int batchId)
        {
            WriteEvent(4, runId, batchId);
        }
    }
}
