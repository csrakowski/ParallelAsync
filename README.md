# ParallelAsync
A .NET utility library for running async methods in parallel batches.

Available on NuGet: [![NuGet](https://img.shields.io/nuget/v/CSRakowski.ParallelAsync.svg)](https://www.nuget.org/packages/CSRakowski.ParallelAsync/)
 and GitHub: [![GitHub stars](https://img.shields.io/github/stars/csrakowski/ParallelAsync.svg)](https://github.com/csrakowski/ParallelAsync/)

Example usage:
```cs
using CSRakowski.Parallel;

List<string> fileUrls = GetFileUrls();

var files = await ParallelAsync.ForEachAsync(fileUrls, (url) => {
    return DownloadFileAsync(url);
}, maxBatchSize: 8, allowOutOfOrderProcessing: true);
```

As of version 1.1 a fluent syntax is also available:
```cs
using CSRakowski.Parallel.Extensions;

List<string> fileUrls = GetFileUrls();

var files = await fileUrls
                    .AsParallelAsync()
                    .WithMaxDegreeOfParallelism(8)
                    .WithOutOfOrderProcessing(false)
                    .ForEachAsync((url) => {
                        return DownloadFileAsync(url);
                    });
```

# Release notes

### 1.6.0
* Added support for Async Streams, so you produce an `IAsyncEnumerable<T>`.

### 1.5.4
* Added .NET 6.0 TargetFramework

### 1.5.2
* Fixed dependency misconfiguration on net50

### 1.5.1
* Updated target frameworks

### 1.5.0
* Updated target frameworks

### 1.4.1
* Updated dependencies
 
### 1.4
* Added gist support for `IAsyncEnumberable<T>`

### 1.3.2
* Added the RunId to the BatchStart and BatchStop events

### 1.3.1
* Reduced overhead in code paths where the input collection is a `T[]`, `maxBatchSize` is greater than `1` and `allowOutOfOrder` is `false`

### 1.3
* Changed assembly signing key
* Further changes to internal implementation details
* Performance improvements when the input collection is a `T[]` or `IList<T>` and `maxBatchSize` is set to `1`
* Performance improvements in the `allowOutOfOrder` code paths.

### 1.2.1
* Marked the `T` on the `IParallelAsyncEnumerable` as covariant
* Changes to internal implementation details

### 1.2
* Added an `EventSource` to expose some diagnostic information.
* Changed minimum supported NetStandard from 1.0 to 1.1 (Because of the `EventSource`).

### 1.1.1
* Added support for `IReadOnlyCollection<T>` to the `ListHelper`.
* Added more XmlDoc to methods and classes.

### 1.1
* Renamed class to `ParallelAsync` to prevent naming conflicts with the `System.Threading.Tasks.Parallel`.
* Renamed namespace to `CSRakowski.Parallel` to prevent ambiguous name conflicts between the class and the namespace.
* Added new extension methods to allow for fluent sytax usage.

### 1.0.1
* Enabled Strong Naming.

### 1.0
* Initial release.