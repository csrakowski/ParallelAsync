# Async Streams

Since version 1.6, ParallelAsync supports async streams through `IAsyncEnumerable<T>`, allowing you to process and stream results as they become available instead of waiting for all operations to complete.

## Overview

Async streams enable you to:
- **Stream results** as they're produced instead of waiting for all to complete
- **Chain operations** in pipeline fashion
- **Reduce memory usage** by not holding all results in memory at once
- **Start processing earlier** without waiting for the entire batch to finish

## Basic Concepts

### Traditional Approach (Collect All)

```csharp
// Wait for ALL downloads to complete
var allFiles = await ParallelAsync.ForEachAsync(
    urls,
    DownloadFileAsync,
    maxBatchSize: 5
);

// Then process them
foreach (var file in allFiles)
{
    ProcessFile(file);
}
```

### Streaming Approach

```csharp
// Stream results as they complete
var fileStream = ParallelAsync.ForEachAsyncStream(
    urls,
    DownloadFileAsync,
    maxBatchSize: 5
);

// Process each file immediately when ready
await foreach (var file in fileStream)
{
    ProcessFile(file);
}
```

## Using ForEachAsyncStream

### Direct API

```csharp
using CSRakowski.Parallel;
using System.Collections.Generic;
using System.Threading.Tasks;

IAsyncEnumerable<Result> stream = ParallelAsync.ForEachAsyncStream(
    items,
    async (item) => await ProcessAsync(item),
    maxBatchSize: 5,
    allowOutOfOrderProcessing: true
);

await foreach (var result in stream)
{
    // Handle each result as it becomes available
    Console.WriteLine($"Processed: {result}");
}
```

### Fluent API

```csharp
using CSRakowski.Parallel.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

var stream = items
    .AsParallelAsync()
    .WithMaxDegreeOfParallelism(5)
    .WithOutOfOrderProcessing(true)
    .ForEachAsyncStream(async (item) => await ProcessAsync(item));

await foreach (var result in stream)
{
    Console.WriteLine($"Processed: {result}");
}
```

## Pipeline Processing

One of the most powerful features is chaining multiple parallel operations:

### Example: Multi-Stage Pipeline

```csharp
using CSRakowski.Parallel;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task ProcessDataPipelineAsync(List<string> urls)
{
    // Stage 1: Download files in parallel
    var downloadStream = ParallelAsync.ForEachAsyncStream(
        urls,
        async (url) => await DownloadFileAsync(url),
        maxBatchSize: 5,
        allowOutOfOrderProcessing: true
    );
    
    // Stage 2: Parse downloaded files in parallel
    var parseStream = ParallelAsync.ForEachAsyncStream(
        downloadStream,  // Feed output of stage 1 into stage 2
        async (fileData) => await ParseFileAsync(fileData),
        maxBatchSize: 3,
        allowOutOfOrderProcessing: true
    );
    
    // Stage 3: Validate parsed data in parallel
    var validationStream = ParallelAsync.ForEachAsyncStream(
        parseStream,     // Feed output of stage 2 into stage 3
        async (parsed) => await ValidateDataAsync(parsed),
        maxBatchSize: 4,
        allowOutOfOrderProcessing: true
    );
    
    // Consume final results
    await foreach (var validatedData in validationStream)
    {
        await SaveToDatabase(validatedData);
    }
}
```

**Benefits:**
- Each stage processes items as they become available from the previous stage
- No need to wait for all downloads before starting parsing
- Reduced memory footprint (items flow through pipeline)
- Better resource utilization (all stages work concurrently)

### Example: Fluent Pipeline

```csharp
using CSRakowski.Parallel.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task ProcessImagePipelineAsync(List<string> imageUrls)
{
    // Download → Resize → Compress → Upload
    var downloadStream = imageUrls
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(8)
        .ForEachAsyncStream(DownloadImageAsync);
    
    var resizeStream = downloadStream
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(4)  // CPU-intensive, fewer parallel ops
        .ForEachAsyncStream(ResizeImageAsync);
    
    var compressStream = resizeStream
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(4)
        .ForEachAsyncStream(CompressImageAsync);
    
    var uploadStream = compressStream
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(6)
        .ForEachAsyncStream(UploadImageAsync);
    
    // Collect results
    int count = 0;
    await foreach (var uploadedUrl in uploadStream)
    {
        Console.WriteLine($"Uploaded: {uploadedUrl}");
        count++;
    }
    
    Console.WriteLine($"Processed {count} images");
}
```

## Real-World Examples

### Example 1: Web Scraping Pipeline

```csharp
using CSRakowski.Parallel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public async Task<List<Article>> ScrapeArticlesAsync(List<string> siteUrls)
{
    // Stage 1: Download pages
    var pageStream = ParallelAsync.ForEachAsyncStream(
        siteUrls,
        DownloadPageAsync,
        maxBatchSize: 10,
        allowOutOfOrderProcessing: true
    );
    
    // Stage 2: Extract article links
    var linkStream = ParallelAsync.ForEachAsyncStream(
        pageStream,
        ExtractArticleLinksAsync,
        maxBatchSize: 5,
        allowOutOfOrderProcessing: true
    );
    
    // Stage 3: Download articles
    var articleStream = ParallelAsync.ForEachAsyncStream(
        linkStream,
        DownloadArticleAsync,
        maxBatchSize: 8,
        allowOutOfOrderProcessing: true
    );
    
    // Collect all articles
    var articles = new List<Article>();
    await foreach (var article in articleStream)
    {
        articles.Add(article);
    }
    
    return articles;
}
```

### Example 2: ETL (Extract, Transform, Load)

```csharp
using CSRakowski.Parallel.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public async Task RunEtlPipelineAsync(
    List<string> sourceFiles,
    CancellationToken cancellationToken = default)
{
    // Extract: Read files
    var extractStream = sourceFiles
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(5)
        .ForEachAsyncStream(
            async (file, ct) => await ReadFileAsync(file, ct),
            cancellationToken);
    
    // Transform: Process data
    var transformStream = extractStream
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(8)
        .ForEachAsyncStream(
            async (data, ct) => await TransformDataAsync(data, ct),
            cancellationToken);
    
    // Load: Save to database (sequential to respect DB connection limits)
    var loadStream = transformStream
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(2)
        .ForEachAsyncStream(
            async (data, ct) => await SaveToDatabaseAsync(data, ct),
            cancellationToken);
    
    // Wait for completion
    int recordCount = 0;
    await foreach (var result in loadStream.WithCancellation(cancellationToken))
    {
        recordCount++;
        if (recordCount % 100 == 0)
        {
            Console.WriteLine($"Processed {recordCount} records...");
        }
    }
    
    Console.WriteLine($"ETL complete: {recordCount} records");
}
```

### Example 3: Real-Time Data Processing

```csharp
using CSRakowski.Parallel;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task ProcessSensorDataAsync(IAsyncEnumerable<SensorReading> sensorStream)
{
    // Stage 1: Validate readings
    var validatedStream = ParallelAsync.ForEachAsyncStream(
        sensorStream,
        ValidateSensorReadingAsync,
        maxBatchSize: 10,
        allowOutOfOrderProcessing: true
    );
    
    // Stage 2: Enrich with metadata
    var enrichedStream = ParallelAsync.ForEachAsyncStream(
        validatedStream,
        EnrichWithMetadataAsync,
        maxBatchSize: 5,
        allowOutOfOrderProcessing: true
    );
    
    // Stage 3: Store and alert if needed
    await foreach (var reading in enrichedStream)
    {
        await StoreReadingAsync(reading);
        
        if (reading.RequiresAlert)
        {
            await SendAlertAsync(reading);
        }
    }
}
```

## Cancellation Support

Async streams fully support cancellation:

```csharp
using System.Threading;
using System.Threading.Tasks;

public async Task ProcessWithCancellationAsync(CancellationTokenSource cts)
{
    var stream = items
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(5)
        .ForEachAsyncStream(
            async (item, ct) => await ProcessAsync(item, ct),
            cts.Token);
    
    try
    {
        await foreach (var result in stream.WithCancellation(cts.Token))
        {
            Console.WriteLine($"Result: {result}");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Processing cancelled");
    }
}
```

## Memory Considerations

### Traditional: All Results in Memory

```csharp
// Holds ALL results in memory at once
var allResults = await ParallelAsync.ForEachAsync(
    hugeCollection,  // 1 million items
    ProcessAsync,
    maxBatchSize: 10
);

// Memory spike: All 1 million results in memory
ProcessResults(allResults);
```

### Streaming: Bounded Memory Usage

```csharp
// Streams results, bounded memory usage
var resultStream = ParallelAsync.ForEachAsyncStream(
    hugeCollection,  // 1 million items
    ProcessAsync,
    maxBatchSize: 10
);

// Only keeps ~10 items in memory at a time
await foreach (var result in resultStream)
{
    ProcessResult(result);  // Process and release
}
```

## Performance Characteristics

### When to Use Streams

**Use `ForEachAsyncStream` when:**
- Processing very large collections
- Building multi-stage pipelines
- Memory usage is a concern
- You want to start processing results early
- Chaining multiple parallel operations

**Use `ForEachAsync` when:**
- You need all results at once for final processing
- Collection size is small to moderate
- Simplicity is preferred over streaming
- You need to sort/filter all results together

### Performance Tips

1. **Tune batch sizes per stage**: CPU-intensive stages may need smaller batches
2. **Consider bottlenecks**: The slowest stage limits overall throughput
3. **Balance stages**: Try to keep all stages equally busy
4. **Monitor memory**: Streams reduce but don't eliminate memory usage

## Collecting Stream Results

If you need to collect all results from a stream:

```csharp
using System.Linq;
using System.Threading.Tasks;

var stream = items
    .AsParallelAsync()
    .ForEachAsyncStream(ProcessAsync);

// Method 1: Manual collection
var results = new List<Result>();
await foreach (var result in stream)
{
    results.Add(result);
}

// Method 2: ToListAsync (if available in your environment)
var results = await stream.ToListAsync();

// Method 3: LINQ
var results = await stream.ToArrayAsync();
```

**Note**: Collecting all results negates the memory benefits of streaming.

## Advanced: Custom Async Sources

You can create your own `IAsyncEnumerable<T>` sources:

```csharp
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public async IAsyncEnumerable<DataItem> GenerateDataAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    for (int i = 0; i < 1000; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await Task.Delay(10, cancellationToken);
        
        yield return new DataItem { Id = i, Value = $"Item {i}" };
    }
}

// Use with ParallelAsync
public async Task ProcessGeneratedDataAsync()
{
    var dataStream = GenerateDataAsync();
    
    var resultStream = dataStream
        .AsParallelAsync()
        .WithMaxDegreeOfParallelism(5)
        .ForEachAsyncStream(ProcessDataItemAsync);
    
    await foreach (var result in resultStream)
    {
        Console.WriteLine(result);
    }
}
```

## Next Steps

- [Advanced Topics](advanced-topics.md) - Performance tuning and best practices
- [API Reference](api-reference.md) - Complete API documentation
- [Basic Usage](basic-usage.md) - Review fundamental concepts
