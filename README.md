# FFF.Net

C# wrapper for Dmitriy Kovalenko's [fff](https://github.com/dmtrKovalenko/fff) fuzzy file finder and live grep library.

## Quick Start

```csharp
using FFF.Net;
using FFF.Net.Models;

var options = new FffCreateOptions
{
    BasePath = @"C:\my-project",
    FollowSymlinks = true,
    Watch = true
};

using var finder = new FffFinder(options);

// Wait for initial scan and background watcher warmup
finder.WaitForScan(TimeSpan.FromSeconds(5));
finder.WaitForWatcherReady(TimeSpan.FromSeconds(5));

// Fuzzy search file names
var searchResult = finder.Search("Program");
foreach (var item in searchResult.Items)
{
    Console.WriteLine($"Found: {item.RelativePath} (Score: {item.TotalFrecencyScore})");
}
```

## Live File Watching

`FFF.Net` provides synchronous callback and asynchronous streaming (`IAsyncEnumerable`) APIs for live file watching:

### Callback Subscription (`Watch`)

```csharp
var watchOptions = new FffWatchOptions
{
    Ignore = new[] { "bin", "obj", ".git" }
};

using var subscription = finder.Watch("*.cs", events =>
{
    foreach (var evt in events)
    {
        Console.WriteLine($"[{evt.Kind}] {evt.Path}");
    }
}, watchOptions);
```

### Asynchronous Stream (`WatchAsync`)

```csharp
using var cts = new CancellationTokenSource();

await foreach (var batch in finder.WatchAsync("*.cs", watchOptions, cts.Token))
{
    foreach (var evt in batch)
    {
        Console.WriteLine($"Live Event: {evt.Path} was {evt.Kind}");
    }
}
```

More usage examples in `samples/FFF.Net.Playground`.