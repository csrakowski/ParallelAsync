using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSRakowski.Parallel.Benchmarks
{
    public static class TestFunctions
    {
        public static Task<int> JustAddOne(int number)
        {
            return Task.FromResult(number + 1);
        }

        public static Task<int> JustAddOne_WithCancellationToken(int number, CancellationToken cancellationToken)
        {
            return Task.FromResult(number + 1);
        }

        public static Task ReturnCompletedTask(int number)
        {
            return Task.CompletedTask;
        }

        public static Task ReturnCompletedTask_WithCancellationToken(int number, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
