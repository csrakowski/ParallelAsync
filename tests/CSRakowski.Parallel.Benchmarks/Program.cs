﻿using System;
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
    class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<UsingForEachForOrdered>();
        }
    }
}
