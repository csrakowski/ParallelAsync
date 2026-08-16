# Getting Started

This guide will help you install and set up the ParallelAsync library in your .NET project.

## Installation

### Via NuGet Package Manager

The easiest way to install ParallelAsync is via NuGet:

```bash
dotnet add package CSRakowski.ParallelAsync
```

### Via Package Manager Console (Visual Studio)

```powershell
Install-Package CSRakowski.ParallelAsync
```

### Via NuGet.org

You can also add the package reference directly to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="CSRakowski.ParallelAsync" Version="1.8.1" />
</ItemGroup>
```

> **Note**: Replace `1.8.1` with the latest version available on [NuGet](https://www.nuget.org/packages/CSRakowski.ParallelAsync/).

## System Requirements

### Supported Frameworks

ParallelAsync targets multiple .NET frameworks:

- **.NET 10.0** or later
- **.NET 8.0** or later
- **.NET Framework 4.7.2** or later
- **.NET Standard 2.0** (compatible with a wide range of platforms)

### Dependencies

For .NET Standard 2.0 and .NET Framework 4.7.2 projects, the library automatically includes:
- `Microsoft.Bcl.AsyncInterfaces` (version 6.0.0 or later) for async stream support

No additional dependencies are required for modern .NET versions (.NET 8.0+).

## Basic Setup

Once installed, add the using directive to your C# files:

```csharp
using CSRakowski.Parallel;
```

For fluent API usage, also include:

```csharp
using CSRakowski.Parallel.Extensions;
```

## Verify Installation

Create a simple test to verify the library is working:

```csharp
using CSRakowski.Parallel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var numbers = Enumerable.Range(1, 10).ToList();
        
        var results = await ParallelAsync.ForEachAsync(
            numbers, 
            async (n) => 
            {
                await Task.Delay(100);
                return n * 2;
            },
            maxBatchSize: 3
        );
        
        Console.WriteLine($"Processed {results.Count()} items");
        Console.WriteLine(string.Join(", ", results));
    }
}
```

Expected output:
```
Processed 10 items
2, 4, 6, 8, 10, 12, 14, 16, 18, 20
```

## Next Steps

Now that you have ParallelAsync installed, proceed to:

- [Basic Usage](basic-usage.md) - Learn the core concepts and simple examples
- [Fluent API](fluent-api.md) - Explore the fluent syntax for cleaner code
- [Async Streams](async-streams.md) - Work with streaming data pipelines

## Troubleshooting

### Missing AsyncInterfaces

If you encounter errors related to `IAsyncEnumerable<T>` on older frameworks:

1. Ensure you're targeting .NET Standard 2.0 or later, or .NET Framework 4.7.2 or later
2. Verify that `Microsoft.Bcl.AsyncInterfaces` is properly restored (run `dotnet restore`)

### Version Conflicts

If you experience version conflicts with other packages:

1. Update all packages to their latest compatible versions
2. Ensure your project targets a recent SDK version
3. Check for conflicting `Microsoft.Bcl.AsyncInterfaces` references

### Getting Help

- Check the [API Reference](api-reference.md) for detailed method signatures
- Review [examples in the repository](https://github.com/csrakowski/ParallelAsync/tree/master/tests)
- [Open an issue](https://github.com/csrakowski/ParallelAsync/issues) on GitHub
