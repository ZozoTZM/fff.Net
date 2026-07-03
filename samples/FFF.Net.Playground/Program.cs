using FFF.Net;
using FFF.Net.Models;

Console.Title = "FFF.Net Feature Showcase";
Console.WriteLine("========================================");
Console.WriteLine("       FFF.Net Feature Showcase");
Console.WriteLine("========================================");

// 1. Diagnostics
try
{
    string health = FffDiagnostics.GetHealthCheck();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Native DLL Health Check passed.");
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"✘ Health check failed: {ex.Message}");
    Console.ResetColor();
    return;
}

// Select Directory 
string defaultPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
Console.WriteLine($"\nDefault workspace directory is: {defaultPath}");
Console.Write("Enter directory path to index (Press Enter to use default): ");
string inputPath = Console.ReadLine() ?? "";
string targetPath = string.IsNullOrWhiteSpace(inputPath) ? defaultPath : Path.GetFullPath(inputPath);

if (!Directory.Exists(targetPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"✘ Directory does not exist: {targetPath}");
    Console.ResetColor();
    return;
}

// 3. Initialize Finder
FffFinder finder;
try
{
    var options = new FffCreateOptions
    {
        BasePath = targetPath,
        Watch = false,
        EnableMmapCache = false,
        EnableContentIndexing = false
    };

    Console.WriteLine($"\nInitializing FffFinder for: {options.BasePath} ...");
    finder = new FffFinder(options);

    Console.WriteLine("Scanning files in background...");
    bool done = finder.WaitForScan(TimeSpan.FromSeconds(10));
    if (done)
    {
        var progress = finder.ScanProgress;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Initial Scan completed successfully! Indexed {progress.ScannedFilesCount} files.");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Scan is taking longer than 10 seconds. Continuing anyway...");
        Console.ResetColor();
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Failed to initialize: {ex.Message}");
    Console.ResetColor();
    return;
}

// 4. Interactive Menu Loop
using (finder)
{
    bool exit = false;
    while (!exit)
    {
        Console.WriteLine("\n----------------------------------------");
        Console.WriteLine("Choose an FFF operation to showcase:");
        Console.WriteLine("1. Fuzzy File Search (Search file names)");
        Console.WriteLine("2. Glob Search (Strict wildcards, e.g. **/*.cs)");
        Console.WriteLine("3. Directory Search (Search folder names only)");
        Console.WriteLine("4. Mixed Search (Files + Folders interleaved)");
        Console.WriteLine("5. Live Grep (Search INSIDE files for text)");
        Console.WriteLine("6. Multi-Grep (OR search for multiple keywords)");
        Console.WriteLine("7. View Scan Progress & Metadata");
        Console.WriteLine("8. Re-index a different directory");
        Console.WriteLine("9. Exit");
        Console.Write("Enter your choice (1-9): ");

        string choice = Console.ReadLine() ?? "";
        Console.WriteLine();

        try
        {
            switch (choice)
            {
                case "1":
                    RunFileSearch(finder);
                    break;
                case "2":
                    RunGlobSearch(finder);
                    break;
                case "3":
                    RunDirectorySearch(finder);
                    break;
                case "4":
                    RunMixedSearch(finder);
                    break;
                case "5":
                    RunLiveGrep(finder);
                    break;
                case "6":
                    RunMultiGrep(finder);
                    break;
                case "7":
                    ShowMetadata(finder);
                    break;
                case "8":
                    Reindex(finder);
                    break;
                case "9":
                    exit = true;
                    Console.WriteLine("Goodbye!");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Invalid choice. Try again.");
                    Console.ResetColor();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error executing operation: {ex.Message}");
            Console.ResetColor();
        }
    }
}

// Helper

static void RunFileSearch(FffFinder finder)
{
    Console.Write("Enter fuzzy query to search file names (e.g. Program): ");
    string query = Console.ReadLine() ?? "";
    if (string.IsNullOrWhiteSpace(query)) return;

    var result = finder.Search(query);
    Console.WriteLine($"\nFound {result.TotalMatched} matches out of {result.TotalFiles} files:");
    foreach (var item in result.Items)
    {
        Console.WriteLine($" - {item.RelativePath} (Size: {item.Size} bytes, Frecency Score: {item.TotalFrecencyScore})");
    }
}

static void RunGlobSearch(FffFinder finder)
{
    Console.Write("Enter glob pattern (e.g. **/*.csproj): ");
    string pattern = Console.ReadLine() ?? "";
    if (string.IsNullOrWhiteSpace(pattern)) return;

    var result = finder.Glob(pattern);
    Console.WriteLine($"\nFound {result.TotalMatched} matches out of {result.TotalFiles} files:");
    foreach (var item in result.Items)
    {
        Console.WriteLine($" - {item.RelativePath} (Size: {item.Size} bytes)");
    }
}

static void RunDirectorySearch(FffFinder finder)
{
    Console.Write("Enter query to search folder names (e.g. src): ");
    string query = Console.ReadLine() ?? "";
    if (string.IsNullOrWhiteSpace(query)) return;

    var result = finder.SearchDirectories(query);
    Console.WriteLine($"\nFound {result.TotalMatched} directory matches:");
    foreach (var item in result.Items)
    {
        Console.WriteLine($" - {item.RelativePath} (Dir: {item.DirName})");
    }
}

static void RunMixedSearch(FffFinder finder)
{
    Console.Write("Enter query to search both files and folders: ");
    string query = Console.ReadLine() ?? "";
    if (string.IsNullOrWhiteSpace(query)) return;

    var result = finder.SearchMixed(query);
    Console.WriteLine($"\nFound {result.TotalMatched} mixed matches:");
    foreach (var item in result.Items)
    {
        Console.WriteLine($" - [{item.ItemType}] {item.RelativePath} (Score: {item.TotalFrecencyScore})");
    }
}

static void RunLiveGrep(FffFinder finder)
{
    Console.Write("Enter text pattern to search INSIDE files: ");
    string query = Console.ReadLine() ?? "";
    if (string.IsNullOrWhiteSpace(query)) return;

    Console.Write("Context lines to show before/after (default 1): ");
    uint.TryParse(Console.ReadLine(), out uint context);
    if (context == 0) context = 1;

    var result = finder.Grep(query, new FffGrepOptions { BeforeContext = context, AfterContext = context });
    Console.WriteLine($"\nFound {result.TotalMatched} occurrences in {result.TotalFilesSearched} files:");
    foreach (var match in result.Items)
    {
        Console.WriteLine($"\nFile: {match.RelativePath}:{match.LineNumber}");
        foreach (var line in match.ContextBefore) Console.WriteLine($"  - {line}");
        Console.WriteLine($"  > {match.LineContent.Trim()}");
        foreach (var line in match.ContextAfter) Console.WriteLine($"  + {line}");
    }
}

static void RunMultiGrep(FffFinder finder)
{
    Console.WriteLine("Enter search patterns (one per line, empty line to finish):");
    var patterns = new List<string>();
    while (true)
    {
        string p = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(p)) break;
        patterns.Add(p);
    }
    if (patterns.Count == 0) return;

    var result = finder.MultiGrep(patterns);
    Console.WriteLine($"\nFound {result.TotalMatched} matches:");
    foreach (var match in result.Items)
    {
        Console.WriteLine($" - {match.RelativePath}:{match.LineNumber} -> {match.LineContent.Trim()}");
    }
}

static void ShowMetadata(FffFinder finder)
{
    var progress = finder.ScanProgress;
    Console.WriteLine($"\nActive Base Path: {finder.BasePath}");
    Console.WriteLine($"Is Scanning: {finder.IsScanning}");
    Console.WriteLine($"Total Files Scanned: {progress.ScannedFilesCount}");
    Console.WriteLine($"Watcher Ready: {progress.IsWatcherReady}");
    Console.WriteLine($"Warmup / Content Indexing Complete: {progress.IsWarmupComplete}");
}

static void Reindex(FffFinder finder)
{
    Console.Write("Enter new directory path to index: ");
    string path = Console.ReadLine() ?? "";
    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Invalid directory path.");
        Console.ResetColor();
        return;
    }

    Console.WriteLine($"Re-indexing to: {path} ...");
    finder.Reindex(path);
    finder.WaitForScan(TimeSpan.FromSeconds(10));
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Re-indexing complete.");
    Console.ResetColor();
}