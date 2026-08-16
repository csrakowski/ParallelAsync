# API Reference

Complete API documentation for the ParallelAsync library.

## Namespaces

- `CSRakowski.Parallel` - Core functionality
- `CSRakowski.Parallel.Extensions` - Fluent API and extension methods
- `CSRakowski.Parallel.Helpers` - Internal helpers and EventSource

## ParallelAsync Class

The main static class providing parallel async processing capabilities.

**Namespace**: `CSRakowski.Parallel`

### ForEachAsync Methods

#### ForEachAsync&lt;TInput, TResult&gt; (IEnumerable&lt;TInput&gt;)

Processes a collection in parallel and returns results.

```csharp
public static Task<IEnumerable<TResult>> ForEachAsync<TInput, TResult>(
    IEnumerable<TInput> collection,
    Func<TInput, Task<TResult>> func,
    int maxBatchSize = 0,
    bool allowOutOfOrderProcessing = false,
    int estimatedResultSize = 0,
    CancellationToken cancellationToken = default
)
```

**Type Parameters:**
- `TInput` - Type of input items
- `TResult` - Type of output items

**Parameters:**
- `collection` - The input collection to process
- `func` - Async function to execute for each item
- `maxBatchSize` - Maximum concurrent operations (0 = use processor count)
- `allowOutOfOrderProcessing` - Enable out-of-order processing for performance
- `estimatedResultSize` - Estimated result collection size (for optimization)
- `cancellationToken` - Cancellation token

**Returns:** `Task<IEnumerable<TResult>>` containing all results

**Exceptions:**
- `ArgumentNullException` - If `collection` or `func` is null
- `ArgumentOutOfRangeException` - If `maxBatchSize` is negative
- `OperationCanceledException` - If operation is cancelled

#### ForEachAsync&lt;TInput, TResult&gt; with CancellationToken parameter

```csharp
public static Task<IEnumerable<TResult>> ForEachAsync<TInput, TResult>(
    IEnumerable<TInput> collection,
    Func<TInput, CancellationToken, Task<TResult>> func,
    int maxBatchSize = 0,
    bool allowOutOfOrderProcessing = false,
    int estimatedResultSize = 0,
    CancellationToken cancellationToken = default
)
```

Same as above, but `func` receives a `CancellationToken` parameter.

#### ForEachAsync&lt;TInput&gt; (No Results)

Processes a collection in parallel without collecting results.

```csharp
public static Task ForEachAsync<TInput>(
    IEnumerable<TInput> collection,
    Func<TInput, Task> func,
    int maxBatchSize = 0,
    bool allowOutOfOrderProcessing = false,
    CancellationToken cancellationToken = default
)
```

**Returns:** `Task` that completes when all operations finish

#### ForEachAsync&lt;TInput&gt; with CancellationToken parameter (No Results)

```csharp
public static Task ForEachAsync<TInput>(
    IEnumerable<TInput> collection,
    Func<TInput, CancellationToken, Task> func,
    int maxBatchSize = 0,
    bool allowOutOfOrderProcessing = false,
    CancellationToken cancellationToken = default
)
```

#### IAsyncEnumerable Overloads

All above methods also have overloads accepting `IAsyncEnumerable<TInput>` as the first parameter.

### ForEachAsyncStream Methods

Returns results as an async stream (`IAsyncEnumerable<TResult>`).

#### ForEachAsyncStream&lt;TInput, TResult&gt; (IEnumerable&lt;TInput&gt;)

```csharp
public static IAsyncEnumerable<TResult> ForEachAsyncStream<TInput, TResult>(
    IEnumerable<TInput> collection,
    Func<TInput, Task<TResult>> func,
    int maxBatchSize = 0,
    bool allowOutOfOrderProcessing = false,
    int estimatedResultSize = 0,
    CancellationToken cancellationToken = default
)
```

**Returns:** `IAsyncEnumerable<TResult>` that yields results as they complete

#### ForEachAsyncStream&lt;TInput, TResult&gt; with CancellationToken parameter

```csharp
public static IAsyncEnumerable<TResult> ForEachAsyncStream<TInput, TResult>(
    IEnumerable<TInput> collection,
    Func<TInput, CancellationToken, Task<TResult>> func,
    int maxBatchSize = 0,
    bool allowOutOfOrderProcessing = false,
    int estimatedResultSize = 0,
    CancellationToken cancellationToken = default
)
```

#### IAsyncEnumerable Overloads

All above methods also have overloads accepting `IAsyncEnumerable<TInput>` as the first parameter.

## Extension Methods (Fluent API)

**Namespace**: `CSRakowski.Parallel.Extensions`

### AsParallelAsync

Wraps a collection as an `IParallelAsyncEnumerable<T>`.

```csharp
public static IParallelAsyncEnumerable<T> AsParallelAsync<T>(
    this IEnumerable<T> enumerable
)
```

```csharp
public static IParallelAsyncEnumerable<T> AsParallelAsync<T>(
    this IAsyncEnumerable<T> enumerable
)
```

**Returns:** `IParallelAsyncEnumerable<T>` for configuration and execution

**Exceptions:**
- `ArgumentNullException` - If `enumerable` is null

### WithMaxDegreeOfParallelism

Configures maximum concurrent operations.

```csharp
public static IParallelAsyncEnumerable<T> WithMaxDegreeOfParallelism<T>(
    this IParallelAsyncEnumerable<T> parallelAsync,
    int maxDegreeOfParallelism
)
```

**Parameters:**
- `maxDegreeOfParallelism` - Maximum concurrency (0 = use processor count)

**Returns:** Configured `IParallelAsyncEnumerable<T>` for chaining

**Exceptions:**
- `ArgumentOutOfRangeException` - If negative

### WithOutOfOrderProcessing

Configures out-of-order processing.

```csharp
public static IParallelAsyncEnumerable<T> WithOutOfOrderProcessing<T>(
    this IParallelAsyncEnumerable<T> parallelAsync,
    bool allowOutOfOrderProcessing
)
```

**Parameters:**
- `allowOutOfOrderProcessing` - Enable/disable out-of-order processing

**Returns:** Configured `IParallelAsyncEnumerable<T>` for chaining

### WithEstimatedResultSize

Configures estimated result size for memory optimization.

```csharp
public static IParallelAsyncEnumerable<T> WithEstimatedResultSize<T>(
    this IParallelAsyncEnumerable<T> parallelAsync,
    int estimatedResultSize
)
```

**Parameters:**
- `estimatedResultSize` - Estimated size of result collection

**Returns:** Configured `IParallelAsyncEnumerable<T>` for chaining

**Exceptions:**
- `ArgumentOutOfRangeException` - If negative

### ForEachAsync (Extension Methods)

Execute configured parallel async operations.

#### With Results

```csharp
public static Task<IEnumerable<TResult>> ForEachAsync<TInput, TResult>(
    this IParallelAsyncEnumerable<TInput> parallelAsync,
    Func<TInput, Task<TResult>> func,
    CancellationToken cancellationToken = default
)
```

```csharp
public static Task<IEnumerable<TResult>> ForEachAsync<TInput, TResult>(
    this IParallelAsyncEnumerable<TInput> parallelAsync,
    Func<TInput, CancellationToken, Task<TResult>> func,
    CancellationToken cancellationToken = default
)
```

#### Without Results

```csharp
public static Task ForEachAsync<TInput>(
    this IParallelAsyncEnumerable<TInput> parallelAsync,
    Func<TInput, Task> func,
    CancellationToken cancellationToken = default
)
```

```csharp
public static Task ForEachAsync<TInput>(
    this IParallelAsyncEnumerable<TInput> parallelAsync,
    Func<TInput, CancellationToken, Task> func,
    CancellationToken cancellationToken = default
)
```

### ForEachAsyncStream (Extension Methods)

Execute and stream results.

```csharp
public static IAsyncEnumerable<TResult> ForEachAsyncStream<TInput, TResult>(
    this IParallelAsyncEnumerable<TInput> parallelAsync,
    Func<TInput, Task<TResult>> func,
    CancellationToken cancellationToken = default
)
```

```csharp
public static IAsyncEnumerable<TResult> ForEachAsyncStream<TInput, TResult>(
    this IParallelAsyncEnumerable<TInput> parallelAsync,
    Func<TInput, CancellationToken, Task<TResult>> func,
    CancellationToken cancellationToken = default
)
```

## Interfaces

### IParallelAsyncEnumerable&lt;out T&gt;

Interface for configuring parallel async operations.

**Namespace**: `CSRakowski.Parallel.Extensions`

```csharp
public interface IParallelAsyncEnumerable<out T> : IEnumerable<T>
{
    // No public members - configuration is done via extension methods
}
```

The interface is covariant (`out T`) allowing for flexible type assignments.

## Helpers and Diagnostics

### ParallelAsyncEventSource

EventSource for diagnostics and monitoring.

**Namespace**: `CSRakowski.Parallel.Helpers`

**Source Name**: `"CSRakowski-ParallelAsync"`

#### Events

##### BatchStart

Fired when a batch begins processing.

**Event ID**: 1  
**Level**: Informational  
**Payload**:
- `RunId` (Guid) - Unique identifier for the run
- `BatchSize` (int) - Size of the batch

##### BatchStop

Fired when a batch completes.

**Event ID**: 2  
**Level**: Informational  
**Payload**:
- `RunId` (Guid) - Unique identifier for the run
- `BatchSize` (int) - Size of the batch
- `DurationMs` (long) - Duration in milliseconds

#### Usage Example

```csharp
using System.Diagnostics.Tracing;
using CSRakowski.Parallel.Helpers;

public class MyEventListener : EventListener
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
```

## Type Constraints and Requirements

### Supported Collection Types

All methods work with:
- `IEnumerable<T>` - Standard collections, arrays, LINQ queries
- `IAsyncEnumerable<T>` - Async streams, async LINQ
- `T[]` - Arrays (optimized path)
- `IList<T>` - Lists (size detection)
- `ICollection<T>` - Collections (size detection)
- `IReadOnlyCollection<T>` - Read-only collections (size detection)

### Async Function Signatures

Supported function signatures:

```csharp
// Basic - returns result
Func<TInput, Task<TResult>>

// With cancellation - returns result
Func<TInput, CancellationToken, Task<TResult>>

// Basic - no result
Func<TInput, Task>

// With cancellation - no result
Func<TInput, CancellationToken, Task>
```

## Default Values

| Parameter | Default Value | Meaning |
|-----------|---------------|---------|
| `maxBatchSize` | `0` | Use `Environment.ProcessorCount` |
| `allowOutOfOrderProcessing` | `false` | Maintain input order |
| `estimatedResultSize` | `0` | Auto-detect or dynamic allocation |
| `cancellationToken` | `default` | No cancellation |

## Performance Characteristics

| Operation | Time Complexity | Space Complexity | Notes |
|-----------|----------------|------------------|-------|
| `ForEachAsync` (array input) | O(n/p) | O(n) | p = batch size |
| `ForEachAsync` (IEnumerable) | O(n/p) | O(n) | Plus enumeration overhead |
| `ForEachAsyncStream` | O(n/p) | O(p) | Bounded memory usage |
| Out-of-order | Variable | O(n or p) | Better with variable task durations |
| In-order | O(n/p) | O(n or p) | Small ordering overhead |

## Thread Safety

- All public methods are thread-safe
- Input collections should not be modified during processing
- Output collections from `ForEachAsync` are thread-safe
- `IAsyncEnumerable<T>` results from `ForEachAsyncStream` are not thread-safe (standard behavior)

## Version Compatibility

| Version | Feature |
|---------|---------|
| 1.0 | Initial release with `ParallelAsync.ForEachAsync` |
| 1.1 | Fluent API with extension methods |
| 1.2 | EventSource for diagnostics |
| 1.3+ | Performance improvements, signing changes |
| 1.4 | Gist support for `IAsyncEnumerable<T>` |
| 1.6 | `ForEachAsyncStream` methods |
| 1.7+ | Updated dependencies |
| 1.8+ | Updated target frameworks |

## See Also

- [Getting Started](getting-started.md) - Installation and setup
- [Basic Usage](basic-usage.md) - Common usage patterns
- [Fluent API](fluent-api.md) - Fluent syntax examples
- [Async Streams](async-streams.md) - Streaming and pipelines
- [Advanced Topics](advanced-topics.md) - Performance tuning
- [GitHub Repository](https://github.com/csrakowski/ParallelAsync) - Source code
