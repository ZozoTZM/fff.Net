using System;
using FFF.Net;

Console.WriteLine("=== Testing FFF.Net Core Interop ===");

try
{
    //workspace root
    string repoPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));

    var options = new FffCreateOptions
    {
        BasePath = repoPath,
        Watch = false,
        EnableMmapCache = false,
        EnableContentIndexing = false
    };

    Console.WriteLine($"Initializing FffFinder for: {options.BasePath}");
    using var finder = new FffFinder(options);

    Console.WriteLine("Waiting for scan to complete...");
    finder.WaitForScan(TimeSpan.FromSeconds(5));

    Console.WriteLine("Scanning complete. Searching for directories containing 'Fff'...");
    var dirResult = finder.SearchDirectories("Fff");

    Console.WriteLine($"Dir Search completed! Total Folders: {dirResult.TotalDirs}, Matched: {dirResult.TotalMatched}");
    Console.WriteLine("Matches:");
    foreach (var item in dirResult.Items)
    {
        Console.WriteLine($" - {item.RelativePath} (Max Access Frecency: {item.MaxAccessFrecency})");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex}");
}