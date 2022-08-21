using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Attributes;

namespace CSRakowski.Parallel.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
#if NET6_0_OR_GREATER

            var summary = BenchmarkRunner.Run<CompareWith_Parallel_ForEachAsync>();
#endif
        }
    }
}
