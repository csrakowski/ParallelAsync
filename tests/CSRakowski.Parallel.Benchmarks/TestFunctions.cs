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

        public static Task<double> Compute_Double(int number)
        {
            var cosh = Math.Cosh(number);
            var sinh = Math.Sinh(number);

            var base16 = Math.Log(number, 16);
            var cosh16 = Math.Cosh(base16);
            var sinh16 = Math.Sinh(base16);

            double result = number;
            result *= cosh;
            result *= sinh;
            result *= base16;
            result *= cosh16;
            result *= sinh16;

            return Task.FromResult(result);
        }
    }
}
