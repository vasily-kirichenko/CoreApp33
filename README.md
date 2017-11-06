This is Jaro-Winkler algorithm taken from https://github.com/Microsoft/visualfsharp/blob/master/src/utils/EditDistance.fs (`Base` benchmark), then optimized (`Opt`) and the `Opt` + `ArrayPool` to eliminate allocation entirely.

``` ini
BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.674)
Processor=Intel Core i7-4790K CPU 4.00GHz (Haswell), ProcessorCount=8
Frequency=3906255 Hz, Resolution=255.9997 ns, Timer=TSC
.NET Core SDK=2.1.1-preview-007094
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT


```
|       Method |     Mean |     Error |    StdDev | Scaled |  Gen 0 | Allocated |
|------------- |---------:|----------:|----------:|-------:|-------:|----------:|
|         Base | 487.8 ns | 3.9748 ns | 3.7180 ns |   1.00 | 0.2661 |    1120 B |
|          Opt | 179.3 ns | 0.8057 ns | 0.7537 ns |   0.37 | 0.0226 |      96 B |
| OptArrayPool | 282.6 ns | 1.0996 ns | 1.0286 ns |   0.58 |      - |       0 B |

Conclusion: allocations are not always your enemy. 
