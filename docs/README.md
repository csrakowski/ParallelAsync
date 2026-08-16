# ParallelAsync Documentation

Welcome to the ParallelAsync library documentation. This library provides utilities for running async methods in parallel batches in .NET applications.

## Table of Contents

- [Getting Started](getting-started.md) - Installation and setup
- [Basic Usage](basic-usage.md) - Core concepts and simple examples
- [Fluent API](fluent-api.md) - Using the fluent syntax
- [Async Streams](async-streams.md) - Working with IAsyncEnumerable pipelines
- [Advanced Topics](advanced-topics.md) - Performance tuning and best practices
- [API Reference](api-reference.md) - Complete API documentation
- [Migration Guide](migration-guide.md) - Upgrading between versions

## Quick Example

```csharp
using CSRakowski.Parallel;

List<string> urls = GetFileUrls();

var files = await ParallelAsync.ForEachAsync(urls, 
    async (url) => await DownloadFileAsync(url),
    maxBatchSize: 8, 
    allowOutOfOrderProcessing: true);
```

## Features

- **Batch Processing**: Control the degree of parallelism with configurable batch sizes
- **Fluent API**: Chain configuration and execution with an intuitive syntax
- **Async Streams**: Support for `IAsyncEnumerable<T>` for streaming results
- **Cancellation Support**: Full support for `CancellationToken`
- **Performance Options**: Choose between ordered and out-of-order processing
- **Cross-Platform**: Targets .NET 10.0, .NET 8.0, .NET Framework 4.7.2, and .NET Standard 2.0

## Supported Frameworks

- .NET 10.0+
- .NET 8.0+
- .NET Framework 4.7.2+
- .NET Standard 2.0+

## Links

- [NuGet Package](https://www.nuget.org/packages/CSRakowski.ParallelAsync/)
- [GitHub Repository](https://github.com/csrakowski/ParallelAsync/)
- [Report Issues](https://github.com/csrakowski/ParallelAsync/issues)

## License

This library is licensed under the MIT License. See the [LICENSE](../LICENSE) file for details.
