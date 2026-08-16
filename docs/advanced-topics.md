# Advanced Topics

This guide covers advanced usage patterns, performance optimization, and best practices for the ParallelAsync library.

## Performance Optimization

### Choosing the Right Batch Size

The optimal `maxBatchSize` depends on your workload characteristics:

#### CPU-Bound Operations

```csharp
// Use processor count for CPU-bound work
var results = await data
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(Environment.ProcessorCount)
    .ForEachAsync(CpuIntensiveOperation);
```

**Guidelines:**
- Start with `Environment.ProcessorCount`
- Test with `ProcessorCount * 1.5` for operations with some I/O waits
- Avoid over-subscribing (too many concurrent CPU-bound tasks hurts performance)

#### I/O-Bound Operations

```csharp
// Network I/O can handle more concurrency
var results = await urls
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(20)
    .ForEachAsync(DownloadAsync);
```

**Guidelines:**
- Network operations: Start with 10-20, tune based on testing
- Database operations: Consider connection pool size (often 2-10)
- File I/O: Start with 4-8, depends on disk speed and RAID configuration
- External API calls: Respect rate limits, start conservative (3-5)

#### Mixed Workloads

```csharp
// Download (I/O) + Process (CPU)
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)  // Balance between I/O and CPU
    .ForEachAsync(async (item) => 
    {
        var data = await DownloadAsync(item);  // I/O-bound
        return ProcessData(data);              // CPU-bound
    });
```

### Out-of-Order Processing

Enable out-of-order processing when task durations vary:

```csharp
// Task durations vary significantly - use out-of-order
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)
    .WithOutOfOrderProcessing(true)  // Performance improvement
    .ForEachAsync(VariableDurationTaskAsync);
```

**Performance impact:**
- **Variable durations**: 10-50% performance improvement
- **Uniform durations**: Slight performance degradation (1-5%)
- **Trade-off**: Loses result ordering

**Benchmark your scenario** to determine if out-of-order processing helps.

### Estimated Result Size

Pre-allocate result collection when size is known:

```csharp
var knownSize = 10000;

var results = await items
    .AsParallelAsync()
    .WithEstimatedResultSize(knownSize)
    .ForEachAsync(ProcessAsync);
```

**When it matters:**
- Large collections (10,000+ items)
- Memory-constrained environments
- When you know or can estimate the result count

**Auto-detection**: The library tries to detect collection size automatically for:
- Arrays (`T[]`)
- `ICollection<T>`
- `IReadOnlyCollection<T>`
- `IList<T>`

## Resource Management

### Database Connection Pooling

Limit parallelism to respect connection pool size:

```csharp
// Connection pool size is typically 100 by default
var results = await records
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)  // Well within pool size
    .ForEachAsync(async (record) =>
    {
        using var connection = await GetDatabaseConnectionAsync();
        return await connection.SaveAsync(record);
    });
```

### HTTP Client Best Practices

Reuse `HttpClient` instances:

```csharp
// Good: Reuse HttpClient
using var httpClient = new HttpClient();

var results = await urls
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)
    .ForEachAsync(async (url) => 
        await httpClient.GetStringAsync(url));

// Bad: Creating HttpClient per request
var results = await urls
    .AsParallelAsync()
    .ForEachAsync(async (url) =>
    {
        using var client = new HttpClient();  // DON'T DO THIS
        return await client.GetStringAsync(url);
    });
```

### Rate Limiting

Implement rate limiting for external services:

```csharp
using System.Threading;

// Simple rate limiting with semaphore
var rateLimiter = new SemaphoreSlim(5, 5);  // Max 5 concurrent calls

var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)
    .ForEachAsync(async (item) =>
    {
        await rateLimiter.WaitAsync();
        try
        {
            return await CallExternalApiAsync(item);
        }
        finally
        {
            rateLimiter.Release();
        }
    });
```

**Advanced rate limiting** with token bucket or sliding window:

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class TokenBucketRateLimiter
{
    private readonly SemaphoreSlim _semaphore;
    private readonly int _tokensPerSecond;
    private readonly Timer _refillTimer;

    public TokenBucketRateLimiter(int tokensPerSecond, int bucketSize)
    {
        _tokensPerSecond = tokensPerSecond;
        _semaphore = new SemaphoreSlim(bucketSize, bucketSize);
        
        // Refill tokens periodically
        _refillTimer = new Timer(_ => 
        {
            if (_semaphore.CurrentCount < bucketSize)
                _semaphore.Release();
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000.0 / tokensPerSecond));
    }

    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
    }
}

// Usage
var rateLimiter = new TokenBucketRateLimiter(tokensPerSecond: 10, bucketSize: 20);

var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(15)
    .ForEachAsync(async (item, ct) =>
    {
        await rateLimiter.WaitAsync(ct);
        return await CallApiAsync(item, ct);
    });
```

## Error Handling Patterns

### Fail-Fast (Default Behavior)

By default, any exception stops all processing:

```csharp
try
{
    var results = await items
        .AsParallelAsync()
        .ForEachAsync(ProcessAsync);
}
catch (Exception ex)
{
    // Processing stopped at first error
    Logger.LogError(ex, "Processing failed");
}
```

### Graceful Degradation

Handle errors per-item to continue processing:

```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)
    .ForEachAsync(async (item) =>
    {
        try
        {
            return await ProcessAsync(item);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed to process item {item.Id}");
            return null;  // Or a default/error result
        }
    });

// Filter out failed items
var successfulResults = results.Where(r => r != null).ToList();
var failedCount = results.Count(r => r == null);

Logger.LogInformation($"Success: {successfulResults.Count}, Failed: {failedCount}");
```

### Result Wrapper Pattern

Use a result wrapper to distinguish success from failure:

```csharp
public class ProcessingResult<T>
{
    public bool Success { get; set; }
    public T Value { get; set; }
    public Exception Error { get; set; }
    public string ItemId { get; set; }
}

var results = await items
    .AsParallelAsync()
    .ForEachAsync(async (item) =>
    {
        try
        {
            var value = await ProcessAsync(item);
            return new ProcessingResult<Data>
            {
                Success = true,
                Value = value,
                ItemId = item.Id
            };
        }
        catch (Exception ex)
        {
            return new ProcessingResult<Data>
            {
                Success = false,
                Error = ex,
                ItemId = item.Id
            };
        }
    });

// Analyze results
var successful = results.Where(r => r.Success).ToList();
var failed = results.Where(r => !r.Success).ToList();

foreach (var failure in failed)
{
    Logger.LogError(failure.Error, $"Item {failure.ItemId} failed");
}
```

### Retry Logic

Implement retry logic for transient failures:

```csharp
using Polly;

// Define retry policy
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(5)
    .ForEachAsync(async (item) =>
    {
        return await retryPolicy.ExecuteAsync(async () =>
            await ProcessAsync(item));
    });
```

## Memory Management

### Streaming for Large Collections

Use async streams for large datasets:

```csharp
// Bad: Loads all results into memory
var allResults = await hugeCollection
    .AsParallelAsync()
    .ForEachAsync(ProcessAsync);

// Good: Streams results
var resultStream = hugeCollection
    .AsParallelAsync()
    .ForEachAsyncStream(ProcessAsync);

await foreach (var result in resultStream)
{
    await SaveAsync(result);  // Process and release
}
```

### Batching Input Collections

Process very large collections in chunks:

```csharp
public async Task ProcessInBatchesAsync<T>(IEnumerable<T> items, int batchSize = 1000)
{
    var batches = items
        .Select((item, index) => new { item, index })
        .GroupBy(x => x.index / batchSize)
        .Select(g => g.Select(x => x.item));

    foreach (var batch in batches)
    {
        var results = await batch
            .AsParallelAsync()
            .WithMaxDegreeOfParallelism(10)
            .ForEachAsync(ProcessItemAsync);
        
        await SaveBatchAsync(results);
    }
}
```

## Diagnostic EventSource

ParallelAsync includes an `EventSource` for diagnostics (added in version 1.2).

### Listening to Events

```csharp
using System.Diagnostics.Tracing;
using CSRakowski.Parallel.Helpers;

public class ParallelAsyncEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "CSRakowski-ParallelAsync")
        {
            EnableEvents(eventSource, EventLevel.Informational);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        Console.WriteLine($"[{eventData.EventName}] {string.Join(", ", eventData.Payload)}");
    }
}

// Use the listener
using (var listener = new ParallelAsyncEventListener())
{
    var results = await items
        .AsParallelAsync()
        .ForEachAsync(ProcessAsync);
}
```

### Available Events

- **BatchStart**: Fired when a batch begins processing
- **BatchStop**: Fired when a batch completes
- Includes timing and batch information

## Testing Strategies

### Unit Testing with Mocks

```csharp
using Moq;
using Xunit;

public class ParallelProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ProcessesAllItems()
    {
        // Arrange
        var mockService = new Mock<IDataService>();
        mockService
            .Setup(s => s.ProcessAsync(It.IsAny<DataItem>()))
            .ReturnsAsync((DataItem item) => new Result { Id = item.Id });

        var items = Enumerable.Range(1, 10)
            .Select(i => new DataItem { Id = i })
            .ToList();

        // Act
        var results = await items
            .AsParallelAsync()
            .WithMaxDegreeOfParallelism(3)
            .ForEachAsync(item => mockService.Object.ProcessAsync(item));

        // Assert
        Assert.Equal(10, results.Count());
        mockService.Verify(s => s.ProcessAsync(It.IsAny<DataItem>()), Times.Exactly(10));
    }
}
```

### Integration Testing

```csharp
using Xunit;

public class ParallelIntegrationTests
{
    [Fact]
    public async Task ProcessAsync_HandlesRealHttpCalls()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var urls = new[] 
        { 
            "https://jsonplaceholder.typicode.com/posts/1",
            "https://jsonplaceholder.typicode.com/posts/2",
            "https://jsonplaceholder.typicode.com/posts/3"
        };

        // Act
        var results = await urls
            .AsParallelAsync()
            .WithMaxDegreeOfParallelism(2)
            .ForEachAsync(async url => 
                await httpClient.GetStringAsync(url));

        // Assert
        Assert.Equal(3, results.Count());
        Assert.All(results, r => Assert.NotEmpty(r));
    }
}
```

### Performance Testing

```csharp
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

public class PerformanceTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task ComparePerformance_ByBatchSize(int batchSize)
    {
        var items = Enumerable.Range(1, 100).ToList();
        var sw = Stopwatch.StartNew();

        var results = await items
            .AsParallelAsync()
            .WithMaxDegreeOfParallelism(batchSize)
            .ForEachAsync(async item =>
            {
                await Task.Delay(10);  // Simulate work
                return item * 2;
            });

        sw.Stop();
        _output.WriteLine($"Batch size {batchSize}: {sw.ElapsedMilliseconds}ms");

        Assert.Equal(100, results.Count());
    }
}
```

## Common Patterns

### Map-Reduce Pattern

```csharp
// Map phase: Transform data in parallel
var mappedStream = sourceData
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)
    .ForEachAsyncStream(MapAsync);

// Reduce phase: Aggregate results
var aggregatedResults = new Dictionary<string, int>();
await foreach (var item in mappedStream)
{
    if (!aggregatedResults.ContainsKey(item.Key))
        aggregatedResults[item.Key] = 0;
    
    aggregatedResults[item.Key] += item.Value;
}
```

### Fan-Out/Fan-In Pattern

```csharp
// Fan-out: Distribute work to multiple processors
var tasks = items.Select(async item =>
{
    var results = await item.SubItems
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(5)
        .ForEachAsync(ProcessSubItemAsync);
    
    return new { Item = item, Results = results };
});

// Fan-in: Collect all results
var allResults = await Task.WhenAll(tasks);
```

### Cache-Aside Pattern

```csharp
private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

public async Task<List<Data>> GetDataWithCacheAsync(List<string> keys)
{
    return await keys
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(10)
        .ForEachAsync(async (key) =>
        {
            // Check cache first
            if (_cache.TryGetValue(key, out Data cachedData))
                return cachedData;

            // Cache miss - fetch from source
            var data = await FetchFromSourceAsync(key);
            
            // Store in cache
            _cache.Set(key, data, TimeSpan.FromMinutes(10));
            
            return data;
        });
}
```

## Best Practices Summary

1. **Benchmark your scenario**: Actual performance varies by workload
2. **Start conservative**: Begin with lower parallelism, increase gradually
3. **Respect resource limits**: Connection pools, rate limits, memory
4. **Use streaming for large datasets**: Reduces memory pressure
5. **Handle errors appropriately**: Choose fail-fast vs. graceful degradation
6. **Reuse expensive resources**: `HttpClient`, database connections, etc.
7. **Monitor in production**: Use logging, metrics, and EventSource
8. **Test with realistic data**: Unit tests don't reveal all performance characteristics
9. **Consider cancellation**: Always support `CancellationToken`
10. **Document your tuning**: Record batch size choices and rationale

## Next Steps

- [API Reference](api-reference.md) - Complete API documentation
- [Async Streams](async-streams.md) - Deep dive into streaming
- [GitHub Repository](https://github.com/csrakowski/ParallelAsync) - Source code and examples
