# ParallelAsync
A .NET utility library for running async methods in parallel batches.

Example:


	List<string> fileUrls = ...;

	var files = await Parallel.ForEachAsync(fileUrls, (url) =>
	{
		return DownloadFileAsync(url);
	}, maxBatchSize: 8, allowOutOfOrderProcessing: true);
