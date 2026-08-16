# Basic Usage

This guide covers the fundamental concepts and common usage patterns of the ParallelAsync library.

## Core Concepts

### What is ParallelAsync?

ParallelAsync allows you to process collections asynchronously with controlled parallelism. Instead of processing items one-by-one or all at once (which could overwhelm resources), you can process them in batches.

### Why Use ParallelAsync?

Traditional `Parallel.ForEach` doesn't work well with async operations. ParallelAsync solves this by:

- **Controlling Concurrency**: Limit the number of simultaneous async operations
- **Async/Await Support**: Full support for modern async patterns
- **Resource Management**: Prevent overwhelming external services or local resources
- **Flexible Processing**: Choose between ordered or out-of-order processing for performance

## The ParallelAsync Class

The static `ParallelAsync` class provides the main entry points for parallel async operations.

### Basic Syntax

```csharp
using CSRakowski.Parallel;

var results = await ParallelAsync.ForEachAsync(
    collection,           // Input collection
    asyncFunction,        // Async function to execute
    maxBatchSize,        // Maximum concurrent operations
    allowOutOfOrder      // Performance optimization flag
);
```

## Simple Examples

### Example 1: Download Files in Parallel

```csharp
using CSRakowski.Parallel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public async Task<List<string>> DownloadFilesAsync(List<string> urls)
{
    using var httpClient = new HttpClient();
    
    var results = await ParallelAsync.ForEachAsync(
        urls,
        async (url) => await httpClient.GetStringAsync(url),
        maxBatchSize: 5,
        allowOutOfOrderProcessing: true
    );
    
    return results.ToList();
}
```

**What's happening:**
- Downloads up to 5 files concurrently
- `allowOutOfOrderProcessing: true` improves performance when download times vary
- Returns results in completion order (not input order when out-of-order is enabled)

### Example 2: Process Database Records

```csharp
using CSRakowski.Parallel;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task ProcessCustomersAsync(List<Customer> customers)
{
    var results = await ParallelAsync.ForEachAsync(
        customers,
        async (customer) => await UpdateCustomerRecordAsync(customer),
        maxBatchSize: 10,
        allowOutOfOrderProcessing: false
    );
    
    Console.WriteLine($"Updated {results.Count()} customers");
}
```

**What's happening:**
- Processes up to 10 customers concurrently
- `allowOutOfOrderProcessing: false` maintains input order in results
- Good for when order matters in your business logic

### Example 3: API Calls with Rate Limiting

```csharp
using CSRakowski.Parallel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task<List<ApiResponse>> CallApiAsync(List<string> endpoints)
{
    var results = await ParallelAsync.ForEachAsync(
        endpoints,
        async (endpoint) =>
        {
            var response = await apiClient.GetAsync(endpoint);
            await Task.Delay(100); // Simple rate limiting
            return await response.Content.ReadAsAsync<ApiResponse>();
        },
        maxBatchSize: 3,  // Limit concurrent API calls
        allowOutOfOrderProcessing: true
    );
    
    return results.ToList();
}
```

**What's happening:**
- Limits to 3 concurrent API calls to respect rate limits
- Adds a small delay between calls
- Out-of-order processing improves throughput

### Example 4: Processing Without Return Values

When you don't need return values, use the overload that returns `Task`:

```csharp
using CSRakowski.Parallel;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task SendEmailsAsync(List<User> users)
{
    await ParallelAsync.ForEachAsync(
        users,
        async (user) => await emailService.SendWelcomeEmailAsync(user.Email),
        maxBatchSize: 20,
        allowOutOfOrderProcessing: true
    );
    
    Console.WriteLine("All emails sent!");
}
```

## Understanding Parameters

### maxBatchSize

Controls how many operations run concurrently:

- **0**: Uses `Environment.ProcessorCount` (number of CPU cores)
- **1**: Sequential processing (no parallelism)
- **> 1**: Specific number of concurrent operations

**Choosing the right value:**
- CPU-bound operations: Use `Environment.ProcessorCount`
- I/O-bound operations (network, disk): Start with 5-20, tune based on testing
- External API calls: Check rate limits, start conservative (3-5)

```csharp
// CPU-bound work
var results = await ParallelAsync.ForEachAsync(
    data,
    (item) => CpuIntensiveOperation(item),
    maxBatchSize: 0  // Use processor count
);

// Network I/O
var results = await ParallelAsync.ForEachAsync(
    urls,
    (url) => DownloadAsync(url),
    maxBatchSize: 10  // Tune based on network/server capacity
);
```

### allowOutOfOrderProcessing

Performance optimization flag:

- **true**: Results may be in different order than input; better performance when task durations vary
- **false**: Results maintain input order; slight overhead to preserve ordering

**When to use true:**
- Task durations vary significantly (e.g., network calls with varying latency)
- You don't care about result order
- Maximum performance is important

**When to use false:**
- Result order must match input order
- Downstream processing depends on order
- Task durations are uniform

```csharp
// Variable duration tasks - use out-of-order
var images = await ParallelAsync.ForEachAsync(
    imageUrls,
    (url) => DownloadImageAsync(url),
    maxBatchSize: 8,
    allowOutOfOrderProcessing: true  // Downloads complete at different times
);

// Order matters - keep in order
var processedRecords = await ParallelAsync.ForEachAsync(
    records,
    (record) => ProcessInSequenceAsync(record),
    maxBatchSize: 4,
    allowOutOfOrderProcessing: false  // Maintain record order
);
```

## Cancellation Support

All methods support `CancellationToken` for graceful cancellation:

```csharp
using System.Threading;

public async Task ProcessWithCancellationAsync(CancellationTokenSource cts)
{
    try
    {
        var results = await ParallelAsync.ForEachAsync(
            items,
            async (item, ct) => await ProcessItemAsync(item, ct),
            maxBatchSize: 5,
            allowOutOfOrderProcessing: true,
            cancellationToken: cts.Token
        );
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation was cancelled");
    }
}
```

**Note**: Your async function should also accept and respect the `CancellationToken`.

## Error Handling

Exceptions in async functions will propagate and cancel the entire operation:

```csharp
try
{
    var results = await ParallelAsync.ForEachAsync(
        items,
        async (item) =>
        {
            if (item.IsInvalid)
                throw new InvalidOperationException($"Invalid item: {item.Id}");
                
            return await ProcessAsync(item);
        },
        maxBatchSize: 5
    );
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Processing failed: {ex.Message}");
    // Handle the error
}
```

**Best practice**: Handle errors within your async function if you want partial success:

```csharp
var results = await ParallelAsync.ForEachAsync(
    items,
    async (item) =>
    {
        try
        {
            return await ProcessAsync(item);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed to process {item.Id}");
            return null;  // Or a default/error result
        }
    },
    maxBatchSize: 5
);

// Filter out failed items
var successfulResults = results.Where(r => r != null);
```

## Working with Different Collection Types

ParallelAsync works with various collection types:

```csharp
// List
var list = new List<int> { 1, 2, 3 };
await ParallelAsync.ForEachAsync(list, ProcessAsync);

// Array
var array = new[] { 1, 2, 3 };
await ParallelAsync.ForEachAsync(array, ProcessAsync);

// IEnumerable (LINQ)
var enumerable = Enumerable.Range(1, 100);
await ParallelAsync.ForEachAsync(enumerable, ProcessAsync);

// IAsyncEnumerable
await ParallelAsync.ForEachAsync(GetItemsAsync(), ProcessAsync);
```

## Performance Tips

1. **Tune batch size**: Test different values to find the optimal concurrency for your scenario
2. **Use out-of-order processing**: When applicable, it significantly improves performance
3. **Profile your workload**: Measure actual performance with realistic data
4. **Consider resource limits**: Database connections, API rate limits, memory constraints
5. **Estimate result size**: Use the `estimatedResultSize` parameter when known (see [Advanced Topics](advanced-topics.md))

## Next Steps

- [Fluent API](fluent-api.md) - Learn the more readable fluent syntax
- [Async Streams](async-streams.md) - Work with streaming data
- [Advanced Topics](advanced-topics.md) - Performance tuning and optimization
- [API Reference](api-reference.md) - Complete API documentation
