# Difficulty of Sorting Algorithms

Desktop app that measures how long sorting algorithms take on increasingly larger arrays of integers and plots
the results live. It compares the original hand-written algorithms with the ones in
[Bojalib](https://www.nuget.org/packages/Bojalib).

Built with .NET 11 and Avalonia, so it runs on Windows, Linux and macOS. The first version used WinForms and
Excel interop; results now export to CSV, which Excel opens directly.

## Algorithms

| Original | Bojalib |
|---|---|
| Bubble sort (naive, always n − 1 full passes) | Bubble sort |
| Improved bubble sort (shrinking passes + early exit) | Insertion sort |
| Insertion sort | Heapsort |
| Min-max selection sort | Merge sort (TimSort-style) |
| Merge sort | Quicksort (pdqsort, SIMD) |
| | Radix sort |
| | Parallel sort (sample sort) |

The original algorithms also count comparisons and writes.

## How it measures

- Sizes follow the original series 100·i² for i = 1 … *steps* (100, 400, 900, …).
- Inputs can be random, sorted, reversed, nearly sorted or have only a few distinct values.
- Every algorithm sorts the same arrays for a given size. Each size runs several times and the median time is
  reported (picked with Bojalib's `Selection`).
- Every result is checked with Bojalib's `SortedSpan.IsSorted`.
- Once a single sort of an algorithm takes longer than the time budget, it skips the larger sizes.

## Build, run, test

```bash
dotnet run --project src/DifficultyOfSortingAlgorithms -c Release
```

```bash
dotnet test --solution DifficultyOfSortingAlgorithms.slnx
```

Measure in Release: Debug builds are several times slower.

## Structure

```
src/DifficultyOfSortingAlgorithms.Core/   algorithms (Algorithms/) and the benchmark runner (Benchmarking/)
src/DifficultyOfSortingAlgorithms/        Avalonia app: view model, chart control, window
tests/DifficultyOfSortingAlgorithms.Tests/ NUnit + CsCheck tests
```
