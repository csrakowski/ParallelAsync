# Migration Guide

This guide helps you upgrade between major versions of ParallelAsync and migrate from other parallel processing libraries.

## Table of Contents

- [Upgrading from 1.7.x to 1.8.x](#upgrading-from-17x-to-18x)
- [Upgrading from 1.5.x to 1.6.x](#upgrading-from-15x-to-16x)
- [Upgrading from 1.0 to 1.1](#upgrading-from-10-to-11)
- [Migrating from System.Threading.Tasks.Parallel](#migrating-from-systemthreadingtasksparallel)
- [Migrating from PLINQ](#migrating-from-plinq)
- [Migrating from Manual Task.WhenAll](#migrating-from-manual-taskwhenall)

## Upgrading from 1.7.x to 1.8.x

### Breaking Changes

#### Dropped .NET 6.0 Support

ParallelAsync 1.8.x drops support for .NET 6.0, which reached end-of-life.

**Supported Frameworks:**
- .NET 10.0+
- .NET 8.0+
- .NET Framework 4.7.2+
- .NET Standard 2.0+

**Migration Steps:**

1. **Check your target framework** in your `.csproj`:

```xml
<PropertyGroup>
  <TargetFramework>net6.0</TargetFramework>  <!-- Unsupported -->
</PropertyGroup>
```

2. **Update to a supported framework**:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>  <!-- Recommended -->
</PropertyGroup>
```

3. **Update the package**:

```bash
dotnet add package CSRakowski.ParallelAsync --version 1.8.1
```

### No Code Changes Required

The API remains unchanged. Your existing code will work without modifications once you update the target framework.

```csharp
// This code works identically in 1.7.x and 1.8.x
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .ForEachAsync(ProcessAsync);
```

## Upgrading from 1.5.x to 1.6.x

### New Features

Version 1.6 introduces async streams support with `ForEachAsyncStream`.

### Migration to Streaming

If you were manually streaming results, you can now use the built-in feature:

**Before (1.5.x):**
```csharp
// Manual streaming implementation
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(5)
    .ForEachAsync(ProcessAsync);

foreach (var result in results)
{
    await HandleResultAsync(result);
}
```

**After (1.6.x+):**
```csharp
// Built-in streaming
var resultStream = items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(5)
    .ForEachAsyncStream(ProcessAsync);

await foreach (var result in resultStream)
{
    await HandleResultAsync(result);
}
```

**Benefits:**
- Lower memory usage for large result sets
- Results available as they complete
- Better support for pipelines

### Backward Compatibility

All 1.5.x code continues to work without changes. `ForEachAsyncStream` is an addition, not a replacement.

## Upgrading from 1.0 to 1.1

### Namespace Changes

Version 1.1 renamed the main class from `Parallel` to `ParallelAsync` and changed the namespace to prevent conflicts with `System.Threading.Tasks.Parallel`.

**Before (1.0):**
```csharp
using CSRakowski.AsyncParallel;

var results = await Parallel.ForEachAsync(
    items,
    ProcessAsync,
    maxBatchSize: 8
);
```

**After (1.1+):**
```csharp
using CSRakowski.Parallel;

var results = await ParallelAsync.ForEachAsync(
    items,
    ProcessAsync,
    maxBatchSize: 8
);
```

### Migration Steps

1. **Update using directive**:
   - Change: `using CSRakowski.AsyncParallel;`
   - To: `using CSRakowski.Parallel;`

2. **Update class name**:
   - Change: `Parallel.ForEachAsync`
   - To: `ParallelAsync.ForEachAsync`

3. **Consider adopting fluent API**:

```csharp
// New fluent syntax (optional)
using CSRakowski.Parallel.Extensions;

var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .ForEachAsync(ProcessAsync);
```

### Find and Replace

Use your IDE to find and replace:
- Find: `using CSRakowski.AsyncParallel;`
- Replace: `using CSRakowski.Parallel;`

Then:
- Find: `Parallel.ForEachAsync`
- Replace: `ParallelAsync.ForEachAsync`

## Migrating from System.Threading.Tasks.Parallel

`System.Threading.Tasks.Parallel` doesn't support async/await properly. ParallelAsync is designed specifically for async operations.

### Basic For Loop

**Before (Parallel.ForEach - INCORRECT for async):**
```csharp
// This doesn't actually await properly!
Parallel.ForEach(items, item =>
{
    ProcessAsync(item).Wait();  // Blocking!
});
```

**After (ParallelAsync):**
```csharp
using CSRakowski.Parallel;

await ParallelAsync.ForEachAsync(
    items,
    async item => await ProcessAsync(item),
    maxBatchSize: 8
);
```

### With Results

**Before (INCORRECT):**
```csharp
var results = new ConcurrentBag<Result>();

Parallel.ForEach(items, item =>
{
    var result = ProcessAsync(item).Result;  // Blocking!
    results.Add(result);
});
```

**After:**
```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .ForEachAsync(async item => await ProcessAsync(item));
```

### Parallel Options

**Before:**
```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = 8,
    CancellationToken = cancellationToken
};

Parallel.ForEach(items, options, item =>
{
    ProcessAsync(item).Wait();
});
```

**After:**
```csharp
await ParallelAsync.ForEachAsync(
    items,
    async item => await ProcessAsync(item),
    maxBatchSize: 8,
    cancellationToken: cancellationToken
);
```

## Migrating from PLINQ

PLINQ (Parallel LINQ) also struggles with async operations.

### Basic Query

**Before (PLINQ - INCORRECT for async):**
```csharp
var results = items
    .AsParallel()
    .WithDegreeOfParallelism(8)
    .Select(item => ProcessAsync(item).Result)  // Blocking!
    .ToList();
```

**After:**
```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .ForEachAsync(async item => await ProcessAsync(item));
```

### Ordered Results

**Before:**
```csharp
var results = items
    .AsParallel()
    .AsOrdered()
    .WithDegreeOfParallelism(8)
    .Select(item => ProcessAsync(item).Result)
    .ToList();
```

**After:**
```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .WithOutOfOrderProcessing(false)  // Maintains order
    .ForEachAsync(async item => await ProcessAsync(item));
```

### Unordered Results (Performance)

**Before:**
```csharp
var results = items
    .AsParallel()
    .AsUnordered()
    .WithDegreeOfParallelism(8)
    .Select(item => ProcessAsync(item).Result)
    .ToList();
```

**After:**
```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .WithOutOfOrderProcessing(true)  // Better performance
    .ForEachAsync(async item => await ProcessAsync(item));
```

## Migrating from Manual Task.WhenAll

### Basic Batch Processing

**Before (Manual batching):**
```csharp
var batchSize = 8;
var results = new List<Result>();

for (int i = 0; i < items.Count; i += batchSize)
{
    var batch = items.Skip(i).Take(batchSize);
    var tasks = batch.Select(item => ProcessAsync(item));
    var batchResults = await Task.WhenAll(tasks);
    results.AddRange(batchResults);
}
```

**After:**
```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .ForEachAsync(async item => await ProcessAsync(item));
```

### With SemaphoreSlim

**Before:**
```csharp
var semaphore = new SemaphoreSlim(8, 8);
var tasks = items.Select(async item =>
{
    await semaphore.WaitAsync();
    try
    {
        return await ProcessAsync(item);
    }
    finally
    {
        semaphore.Release();
    }
});

var results = await Task.WhenAll(tasks);
```

**After:**
```csharp
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(8)
    .ForEachAsync(async item => await ProcessAsync(item));
```

**Benefits:**
- Much cleaner code
- No manual semaphore management
- Built-in error handling
- Better performance characteristics

## Common Migration Patterns

### Pattern 1: Download Files

**Before (various approaches):**
```csharp
// Approach 1: Sequential (slow)
var files = new List<string>();
foreach (var url in urls)
{
    files.Add(await DownloadAsync(url));
}

// Approach 2: All at once (overwhelming)
var tasks = urls.Select(url => DownloadAsync(url));
var files = await Task.WhenAll(tasks);

// Approach 3: Manual batching (complex)
// ... (many lines of batching code)
```

**After:**
```csharp
var files = await urls
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(10)
    .ForEachAsync(DownloadAsync);
```

### Pattern 2: Database Updates

**Before:**
```csharp
var semaphore = new SemaphoreSlim(5, 5);
var tasks = records.Select(async record =>
{
    await semaphore.WaitAsync();
    try
    {
        await UpdateRecordAsync(record);
    }
    finally
    {
        semaphore.Release();
    }
});

await Task.WhenAll(tasks);
```

**After:**
```csharp
await records
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(5)
    .ForEachAsync(UpdateRecordAsync);
```

### Pattern 3: API Calls with Results

**Before:**
```csharp
var results = new ConcurrentBag<ApiResponse>();
var semaphore = new SemaphoreSlim(3, 3);

var tasks = endpoints.Select(async endpoint =>
{
    await semaphore.WaitAsync();
    try
    {
        var response = await CallApiAsync(endpoint);
        results.Add(response);
    }
    finally
    {
        semaphore.Release();
    }
});

await Task.WhenAll(tasks);
return results.ToList();
```

**After:**
```csharp
return await endpoints
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(3)
    .ForEachAsync(CallApiAsync);
```

## Terminology Mapping

| Other Libraries | ParallelAsync |
|----------------|---------------|
| `MaxDegreeOfParallelism` | `maxBatchSize` or `WithMaxDegreeOfParallelism()` |
| `AsParallel()` | `AsParallelAsync()` |
| `AsOrdered()` | `WithOutOfOrderProcessing(false)` |
| `AsUnordered()` | `WithOutOfOrderProcessing(true)` |
| `Parallel.ForEach` | `ParallelAsync.ForEachAsync` |
| `ConcurrentBag<T>` | Not needed (results collected automatically) |
| `SemaphoreSlim` | Not needed (built into `maxBatchSize`) |

## Performance Considerations

When migrating, consider these performance differences:

### PLINQ vs ParallelAsync

- **PLINQ**: Optimized for CPU-bound synchronous operations
- **ParallelAsync**: Optimized for async I/O-bound operations

### Task.WhenAll vs ParallelAsync

- **Task.WhenAll**: Starts all tasks immediately (can overwhelm resources)
- **ParallelAsync**: Batches execution (controlled resource usage)

### Parallel.ForEach vs ParallelAsync

- **Parallel.ForEach**: Thread-pool threads, good for CPU work
- **ParallelAsync**: Async/await, good for I/O work

## Troubleshooting

### Issue: Different Results Order

**Problem:** Results are in different order after migration.

**Solution:** Use `WithOutOfOrderProcessing(false)`:

```csharp
var results = await items
    .AsParallelAsync()
    .WithOutOfOrderProcessing(false)  // Maintain order
    .ForEachAsync(ProcessAsync);
```

### Issue: Performance Degradation

**Problem:** Code is slower after migration.

**Solution:** Tune `maxBatchSize` for your workload:

```csharp
// Try different values
var results = await items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(20)  // Increase for I/O-bound
    .ForEachAsync(ProcessAsync);
```

### Issue: Memory Usage Increase

**Problem:** Using more memory than before.

**Solution:** Use streaming for large collections:

```csharp
var resultStream = items
    .AsParallelAsync()
    .ForEachAsyncStream(ProcessAsync);

await foreach (var result in resultStream)
{
    // Process immediately, reducing memory usage
}
```

## Getting Help

- Check the [API Reference](api-reference.md) for detailed method signatures
- Review [Advanced Topics](advanced-topics.md) for optimization strategies
- [Open an issue](https://github.com/csrakowski/ParallelAsync/issues) on GitHub
- See examples in the [repository tests](https://github.com/csrakowski/ParallelAsync/tree/master/tests)

## Next Steps

After migration:
- Review [Best Practices](advanced-topics.md#best-practices-summary)
- Optimize [batch sizes](advanced-topics.md#choosing-the-right-batch-size)
- Consider [async streams](async-streams.md) for large datasets
- Add [error handling](advanced-topics.md#error-handling-patterns)
