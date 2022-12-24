using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Tests.Helpers
{
    /// <summary>
    /// As Task.CompletedTask is not available on 4.5, I slightly modified the code found here: http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,66f1c3e3e272f591
    /// </summary>
    internal static class TaskHelper
    {
        /// <summary>
        /// A task that's already been completed successfully.
        /// </summary>
        private static Task s_completedTask;

        /// <summary>Gets a task that's already been completed successfully.</summary>
        /// <remarks>May not always return the same instance.</remarks>
        public static Task CompletedTask
        {
            get
            {
                var completedTask = s_completedTask;
                if (completedTask == null)
                {
#if NET452
                    s_completedTask = completedTask = Task.FromResult(true);
#else
                    s_completedTask = completedTask = Task.CompletedTask;
#endif
                }
                return completedTask;
            }
        }
    }
}
