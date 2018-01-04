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
