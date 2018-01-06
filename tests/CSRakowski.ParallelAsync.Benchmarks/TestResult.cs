namespace CSRakowski.Parallel.Benchmarks
{
    public class TestResult
    {
        public TestResult(int runId, int numberOfJobs, int numberOfResults, double elapsedMilliseconds)
        {
            RunID = (runId == -1) ? "Average" : runId.ToString();
            NumberOfJobs = numberOfJobs;
            NumberOfResults = numberOfResults;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public string RunID { get; }

        public int NumberOfJobs { get; }

        public int NumberOfResults { get; }

        public double ElapsedMilliseconds { get; }

        public double MessagesPerMillisecond => NumberOfResults / ElapsedMilliseconds;
    }
}
