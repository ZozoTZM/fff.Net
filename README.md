# FFF.Net

C# wrapper for Dmitriy Kovalenko's [fff](https://github.com/dmtrKovalenko/fff) fuzzy file finder and live grep library.

## Quick Start

```csharp
using FFF.Net;
using FFF.Net.Models;

var options = new FffCreateOptions
{
    BasePath = @"C:\my-project",
    Watch = true
};

using var finder = new FffFinder(options);

// Wait for the background indexer
finder.WaitForScan(TimeSpan.FromSeconds(5));

// Fuzzy search file names
var searchResult = finder.Search("Program");
foreach (var item in searchResult.Items)
{
    Console.WriteLine($"Found: {item.RelativePath}");
}
```

More examples available in the samples folder. 