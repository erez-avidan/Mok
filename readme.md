Mok
===

<!-- #content -->
Fast and familiar .NET mocking library

```csharp
  var mock = new Mok<IManager>();

  // Setup your mocked methods
  mock.Setup(manager => manager.Generate(1, 2, 3))
      .Returns(true);

  // Get the mocked object from the Object property
  IManager manager = mock.Object;
  bool generated = manager.Generate(1, 2, 3);

  // Verify the method was called
  mock.Verify(manager => manager.Generate(1, 2, It.IsAny<int>()), Times.Once);
```

## What?

Mok is a made to be fast, using Source Generators makes it blazing fast while most other mocking libraries still use reflection ([Castle.DynamicProxy](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md))

Another important aspect for this library is to be familiar, taking the well known syntax from [Moq](https://github.com/devlooped/moq/) and not reinventing the API, making it easier to migrate an existing project

## Why?

This library was made for developers who likes the good readability of existing mocking libraries but faster runtime and a leaner feature list.
You get what you need.

## How?

the internal code is made with Source Generators, build once before starting to write code and you get everything needed to start.

one nice benefit from this is you can see and debug all generated classes, including mocks, without decompiling.


## Todo
- support `async` methods
- thread safe call counting
- support properties
- support generic methods
- support mocking classes instead of interfaces
- other missing features from largely used mocking libraries
- optimizations of generated code (only generate what's used)


## Benchmark

| Method       | Mean         | Error        | StdDev       | Median       | Ratio    | RatioSD  | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|------------- |-------------:|-------------:|-------------:|-------------:|---------:|---------:|-------:|-------:|-------:|----------:|------------:|
| Stub         |     11.09 ns |     75.14 ns |     4.119 ns |     12.47 ns |     1.13 |     0.59 |      - |      - |      - |      24 B |        1.00 |
| FakeItEasy   |  4,675.58 ns | 60,181.96 ns | 3,298.776 ns |  3,179.87 ns |   475.11 |   362.48 | 0.4200 | 0.0200 | 0.0100 |    3971 B |      165.46 |
| JustMockLite | 21,095.09 ns | 99,174.21 ns | 5,436.073 ns | 18,087.28 ns | 2,143.56 |   990.44 | 2.2900 | 0.1500 |      - |   21566 B |      898.58 |
| Moq          |  3,367.95 ns |  2,116.79 ns |   116.029 ns |  3,383.94 ns |   342.23 |   135.93 | 0.2600 |      - |      - |    2464 B |      102.67 |
| NSubstitute  |  3,791.41 ns | 35,926.74 ns | 1,969.266 ns |  4,884.19 ns |   385.26 |   239.75 | 0.6200 | 0.0100 |      - |    5904 B |      246.00 |
| PCLMock      | 54,295.96 ns | 92,622.97 ns | 5,076.978 ns | 51,595.46 ns | 5,517.25 | 2,235.82 | 1.1100 | 1.1000 |      - |   10521 B |      438.38 |
| Rocks        |    142.30 ns |    492.84 ns |    27.014 ns |    130.72 ns |    14.46 |     6.26 | 0.0400 |      - |      - |     392 B |       16.33 |
| Mok          |    271.77 ns |    358.37 ns |    19.643 ns |    266.16 ns |    27.62 |    11.09 | 0.0700 |      - |      - |     672 B |       28.00 |

Made by running [BenchmarkMockNet](https://github.com/ecoAPM/BenchmarkMockNet) and adding the Mok library to the existing benchmarks.

this is just one benchmark example of OneParameter but the ratio is about the same for all benchmarks.

