# ParallelAsync
A .NET utility library for running async methods in parallel batches.

Available on NuGet: [![NuGet](https://img.shields.io/nuget/v/CSRakowski.ParallelAsync.svg)](https://www.nuget.org/packages/CSRakowski.ParallelAsync/)
 and GitHub: [![GitHub stars](https://img.shields.io/github/stars/csrakowski/ParallelAsync.svg)](https://github.com/csrakowski/ParallelAsync/)

Example usage:

	List<string> fileUrls = ...;

	var files = await Parallel.ForEachAsync(fileUrls, (url) =>
	{
		return DownloadFileAsync(url);
	}, maxBatchSize: 8, allowOutOfOrderProcessing: true);
