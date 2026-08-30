# Copilot Instructions

## Project Guidelines
- The user clarified that xUnit v3 uses the package ID `xunit.v3`, and they want version `4.0.0`.
- Preserve verified project guidance, including multi-target compatibility across `net10.0`, `net8.0`, `net472`, and `netstandard2.0`.

## Repository Overview
- `src/CSRakowski.Parallel` is the shippable library. Its public API is the partial static `CSRakowski.Parallel.ParallelAsync` class, with fluent extensions in `CSRakowski.Parallel.Extensions`.
- The library targets `net10.0`, `net8.0`, `net472`, and `netstandard2.0`. Preserve compatibility with all four targets; legacy targets conditionally use `Microsoft.Bcl.AsyncInterfaces`.
- The test project targets `net48`, `net472`, `net10.0`, `net9.0`, and `net8.0`. The benchmark project uses the same targets; `tests/Profiling` is a legacy .NET Framework 4.8 profiling console app.

## API and Behavior
- Keep overload parity across `IEnumerable<T>` and `IAsyncEnumerable<T>`, result-returning and `Task`-returning delegates, and delegate overloads with or without `CancellationToken`.
- `maxBatchSize == 0` means `Environment.ProcessorCount`; negative batch sizes and negative estimated result sizes are invalid and must throw `ArgumentOutOfRangeException`.
- Ordered processing preserves result order; unordered processing emits results in completion order and is intended for workloads with variable operation duration. Do not change either semantic without updating tests and documentation.
- `ForEachAsyncStream` returns a lazy `IAsyncEnumerable<TResult>` pipeline. Async enumerators must receive cancellation where supported and must be disposed in `finally` blocks.
- Cancellation is cooperative: implementations poll between scheduling items/batches and pass the token to token-aware delegates and async enumerators. Overloads without a token parameter intentionally wrap delegates without consuming the token.
- Preserve array/list/enumerable fast paths and `ListHelpers` result-capacity inference; these are deliberate performance optimizations.
- Validate null inputs at public boundaries. Fluent configuration is mutable on the internal wrapper, defaults batch size/result estimate to `0`, and defaults to ordered processing. An async-only wrapper cannot be synchronously enumerated and throws `NotSupportedException` unless it also implements `IEnumerable<T>`.

## Implementation Conventions
- Keep the execution implementations split across the partial files `ParallelAsync.Ordered.cs`, `ParallelAsync.Unordered.cs`, `ParallelAsync.Unbatched.cs`, and `ParallelAsync.AsyncStreams.cs` rather than duplicating public dispatch logic.
- Use `ConfigureAwait(false)` in library awaits and retain target-framework conditional compilation where newer APIs are unavailable on legacy targets.
- The library is strong-named and emits XML documentation. Package builds include `README.md` and `LICENSE`; shared packaging, SourceLink, version, and Release settings are in `Directory.Build.props`.
- `ParallelAsyncEventSource` is the internal EventSource for run/batch diagnostics. Preserve run and batch lifecycle events when changing execution paths.

## Testing and Performance
- Tests use xUnit v3 `[Fact]` methods and are grouped by core APIs, async streams, `IAsyncEnumerable<T>`, extensions, and helpers. Add coverage for both source types and relevant ordered/unordered/unbatched paths when changing behavior.
- Existing tests cover ordering, cancellation, invalid arguments, empty/misaligned inputs, fluent configuration, and async-stream consumption. Prefer deterministic tests; use `Interlocked` for counters shared by parallel delegates.
- Benchmarks use BenchmarkDotNet with `[MemoryDiagnoser]`, batch-size/order parameters, and framework comparisons. Use benchmarks or the .NET Framework 4.8 profiling app for performance claims rather than inferring them from unit tests.
- Follow the root `.editorconfig`: four spaces, braces for control flow, `using` directives outside namespaces, and the established private/internal field naming conventions.

## Validation
- Run `dotnet test --configuration Release` for the full test suite; validate framework-specific changes on the affected target(s).
- CI covers Windows and Ubuntu for `net8.0`, `net9.0`, and `net10.0`, plus Windows `.NET Framework` `net472` and `net48`. Keep changes compatible with this matrix.
