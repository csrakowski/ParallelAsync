# Fluent API

Since version 1.1, ParallelAsync provides a fluent API that allows for more readable and chainable configuration of parallel async operations.

## Overview

The fluent API uses extension methods to wrap collections and configure parallel processing options before execution. This approach is more intuitive and follows modern .NET patterns.

## Basic Syntax

```csharp
using CSRakowski.Parallel.Extensions;

var results = await collection
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .WithOutOfOrderProcessing(true)
    .ForEachAsync(asyncFunction);
```

## Getting Started

### Converting Collections

Use `AsParallelAsync()` to wrap any `IEnumerable<T>` or `IAsyncEnumerable<T>`:

```csharp
using CSRakowski.Parallel.Extensions;

List<string> urls = GetUrls();

// Wrap the collection
IParallelAsyncEnumerable<string> parallelCollection = urls.AsParallelAsync();
```

### Complete Example

```csharp
using CSRakowski.Parallel.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public async Task<List<string>> DownloadFilesFluentAsync(List<string> urls)
{
    using var httpClient = new HttpClient();
    
    var results = await urls
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(5)
        .WithOutOfOrderProcessing(true)
        .ForEachAsync(async (url) => await httpClient.GetStringAsync(url));
    
    return results.ToList();
}
```

## Configuration Methods

### WithMaxDegreeOfParallelism

Sets the maximum number of concurrent operations:

```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)  // Up to 10 concurrent operations
    .ForEachAsync(ProcessAsync);
```

**Parameter values:**
- **0**: Uses `Environment.ProcessorCount`
- **Positive integer**: Specific concurrency limit
- **Throws**: `ArgumentOutOfRangeException` if negative

### WithOutOfOrderProcessing

Enables or disables out-of-order processing for performance:

```csharp
// Enable out-of-order processing (better performance with variable task durations)
var results = await items
    .AsParallelAsync()
    .WithOutOfOrderProcessing(true)
    .ForEachAsync(ProcessAsync);

// Disable out-of-order processing (maintains input order)
var results = await items
    .AsParallelAsync()
    .WithOutOfOrderProcessing(false)
    .ForEachAsync(ProcessAsync);
```

### WithEstimatedResultSize

Optimizes memory allocation when you know the approximate result size:

```csharp
var results = await items
    .AsParallelAsync()
    .WithEstimatedResultSize(1000)  // Expecting ~1000 results
    .ForEachAsync(ProcessAsync);
```

**When to use:**
- You know the approximate result collection size beforehand
- Processing large collections where initial allocation matters
- The library auto-detects size when possible, this is a fallback

## Execution Methods

### ForEachAsync - With Results

Execute async operations and collect results:

```csharp
// Basic overload
var results = await items
    .AsParallelAsync()
    .ForEachAsync(async (item) => await ProcessAsync(item));

// With CancellationToken support
var results = await items
    .AsParallelAsync()
    .ForEachAsync(
        async (item, ct) => await ProcessAsync(item, ct),
        cancellationToken);
```

### ForEachAsync - Without Results

Execute async operations without collecting results:

```csharp
// Basic overload
await items
    .AsParallelAsync()
    .ForEachAsync(async (item) => await ProcessAsync(item));

// With CancellationToken support
await items
    .AsParallelAsync()
    .ForEachAsync(
        async (item, ct) => await ProcessAsync(item, ct),
        cancellationToken);
```

### ForEachAsyncStream

Execute async operations and stream results (see [Async Streams](async-streams.md)):

```csharp
IAsyncEnumerable<Result> stream = items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(5)
    .ForEachAsyncStream(async (item) => await ProcessAsync(item));

await foreach (var result in stream)
{
    HandleResult(result);
}
```

## Complete Examples

### Example 1: File Processing Pipeline

```csharp
using CSRakowski.Parallel.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public async Task<List<ProcessedFile>> ProcessFilesAsync(List<string> filePaths)
{
    var results = await filePaths
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(4)              // Process 4 files at once
        .WithOutOfOrderProcessing(true)             // Order doesn't matter
        .WithEstimatedResultSize(filePaths.Count)   // Pre-allocate result list
        .ForEachAsync(async (path) =>
        {
            var content = await File.ReadAllTextAsync(path);
            var processed = await ProcessContentAsync(content);
            return new ProcessedFile 
            { 
                Path = path, 
                Result = processed 
            };
        });
    
    return results.ToList();
}
```

### Example 2: API Batch Operations

```csharp
using CSRakowski.Parallel.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public async Task<List<User>> UpdateUsersAsync(
    List<int> userIds, 
    CancellationToken cancellationToken = default)
{
    var results = await userIds
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(10)
        .WithOutOfOrderProcessing(true)
        .ForEachAsync(
            async (userId, ct) =>
            {
                var user = await apiClient.GetUserAsync(userId, ct);
                user.LastUpdated = DateTime.UtcNow;
                await apiClient.UpdateUserAsync(user, ct);
                return user;
            },
            cancellationToken);
    
    return results.ToList();
}
```

### Example 3: Data Transformation

```csharp
using CSRakowski.Parallel.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public async Task<Dictionary<int, string>> TransformDataAsync(List<DataRecord> records)
{
    var results = await records
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(Environment.ProcessorCount)
        .WithOutOfOrderProcessing(false)  // Maintain order for consistent results
        .ForEachAsync(async (record) =>
        {
            var transformed = await TransformRecordAsync(record);
            return new KeyValuePair<int, string>(record.Id, transformed);
        });
    
    return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
```

### Example 4: Fire-and-Forget Operations

```csharp
using CSRakowski.Parallel.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task NotifyUsersAsync(List<string> emailAddresses)
{
    await emailAddresses
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(20)
        .WithOutOfOrderProcessing(true)
        .ForEachAsync(async (email) =>
        {
            await emailService.SendNotificationAsync(email);
            await logService.LogNotificationSentAsync(email);
        });
    
    Console.WriteLine($"Sent {emailAddresses.Count} notifications");
}
```

## Method Chaining Order

The configuration methods can be called in any order:

```csharp
// These are equivalent
var results1 = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(5)
    .WithOutOfOrderProcessing(true)
    .WithEstimatedResultSize(100)
    .ForEachAsync(ProcessAsync);

var results2 = await items
    .AsParallelAsync()
    .WithEstimatedResultSize(100)
    .WithOutOfOrderProcessing(true)
    .WithMaxDegreeOfParallelism(5)
    .ForEachAsync(ProcessAsync);
```

**Best practice**: Use a consistent order for readability.

## Default Configuration

If you don't configure options, sensible defaults are used:

```csharp
// Uses defaults
var results = await items
    .AsParallelAsync()
    .ForEachAsync(ProcessAsync);

// Equivalent to:
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(0)          // Uses Environment.ProcessorCount
    .WithOutOfOrderProcessing(false)        // Maintains order
    .WithEstimatedResultSize(0)             // Auto-detected or dynamic allocation
    .ForEachAsync(ProcessAsync);
```

## Working with IAsyncEnumerable

The fluent API supports `IAsyncEnumerable<T>` input:

```csharp
using CSRakowski.Parallel.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

public async IAsyncEnumerable<DataItem> GetDataStreamAsync()
{
    // Yield items asynchronously
    for (int i = 0; i < 100; i++)
    {
        await Task.Delay(10);
        yield return new DataItem { Id = i };
    }
}

public async Task ProcessStreamAsync()
{
    var stream = GetDataStreamAsync();
    
    var results = await stream
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(5)
        .ForEachAsync(async (item) => await ProcessItemAsync(item));
    
    Console.WriteLine($"Processed {results.Count()} items");
}
```

## Comparison: Fluent vs. Direct API

Both APIs are functionally equivalent. Choose based on preference:

### Direct API
```csharp
var results = await ParallelAsync.ForEachAsync(
    items,
    ProcessAsync,
    maxBatchSize: 8,
    allowOutOfOrderProcessing: true,
    estimatedResultSize: 100
);
```

### Fluent API
```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .WithOutOfOrderProcessing(true)
    .WithEstimatedResultSize(100)
    .ForEachAsync(ProcessAsync);
```

**Fluent API advantages:**
- More readable, self-documenting code
- Easier to chain with LINQ operations
- Better IDE autocomplete experience
- Clearer parameter names

**Direct API advantages:**
- Slightly more concise for simple cases
- Familiar to users of `Parallel.ForEach`
- All parameters visible in one place

## Next Steps

- [Async Streams](async-streams.md) - Learn about streaming results with `IAsyncEnumerable<T>`
- [Advanced Topics](advanced-topics.md) - Performance tuning and optimization
- [API Reference](api-reference.md) - Complete method signatures and overloads
