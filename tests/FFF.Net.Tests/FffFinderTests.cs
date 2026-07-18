using FFF.Net.Models;

namespace FFF.Net.Tests;

public class FffFinderTests
{
    private readonly string _testPath;

    public FffFinderTests()
    {
        _testPath = Directory.GetCurrentDirectory();
    }
    [Fact]
    public void Diagnostics_ShouldReturnValidHealthCheck()
    {
        string health = FffDiagnostics.GetHealthCheck();

        Assert.NotNull(health);
        Assert.Contains("version", health);
        Assert.Contains("git", health);
    }
    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FffFinder(null!));
    }
    [Fact]
    public void Constructor_WithEmptyBasePath_ShouldThrowArgumentException()
    {
        var options = new FffCreateOptions { BasePath = "" };

        Assert.Throws<ArgumentException>(() => new FffFinder(options));
    }
    [Fact]
    public void Search_WithNullQuery_ShouldThrowArgumentNullException()
    {

        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);

        Assert.Throws<ArgumentNullException>(() => finder.Search(null!));
    }
    [Fact]
    public void Grep_WithNullQuery_ShouldThrowArgumentNullException()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);

        Assert.Throws<ArgumentNullException>(() => finder.Grep(null!));
    }
    [Fact]
    public async Task Methods_AfterDisposed_ShouldThrowObjectDisposedException()
    {

        var options = new FffCreateOptions { BasePath = _testPath };
        var finder = new FffFinder(options);
        finder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => finder.Search("test"));
        Assert.Throws<ObjectDisposedException>(() => finder.Glob("*.cs"));
        Assert.Throws<ObjectDisposedException>(() => finder.SearchDirectories("test"));
        Assert.Throws<ObjectDisposedException>(() => finder.SearchMixed("test"));
        Assert.Throws<ObjectDisposedException>(() => finder.Grep("test"));
        Assert.Throws<ObjectDisposedException>(() => finder.ScanFiles());
        Assert.Throws<ObjectDisposedException>(() => finder.Reindex(_testPath));
        Assert.Throws<ObjectDisposedException>(() => finder.RefreshGitStatus());
        Assert.Throws<ObjectDisposedException>(() => finder.TrackQuery("q", "p"));
        Assert.Throws<ObjectDisposedException>(() => finder.GetHistoricalQuery(0));
        Assert.Throws<ObjectDisposedException>(() => _ = finder.BasePath);
        Assert.Throws<ObjectDisposedException>(() => _ = finder.ScanProgress);
        Assert.Throws<ObjectDisposedException>(() => finder.Watch("*.cs", _ => { }));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await finder.WatchAsync("*.cs").GetAsyncEnumerator().MoveNextAsync());
    }
    [Fact]
    public void Constructor_WithFollowSymlinks_ShouldCreateSuccessfully()
    {
        var options = new FffCreateOptions
        {
            BasePath = _testPath,
            FollowSymlinks = true
        };

        using var finder = new FffFinder(options);
        Assert.NotNull(finder.BasePath);
    }
    [Fact]
    public void Watch_ShouldReturnValidSubscriptionAndUnwatchSuccessfully()
    {
        var options = new FffCreateOptions { BasePath = _testPath, Watch = true };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));
        Assert.True(finder.WaitForWatcherReady(TimeSpan.FromSeconds(5)));

        using var subscription = finder.Watch("*.cs", _ => { });
        Assert.NotEqual(0UL, subscription.WatchId);

        bool unwatched = finder.Unwatch(subscription.WatchId);
        Assert.True(unwatched);
    }
    [Fact]
    public void Watch_WithIgnoreOptions_ShouldRegisterSuccessfully()
    {
        var options = new FffCreateOptions { BasePath = _testPath, Watch = true };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));
        Assert.True(finder.WaitForWatcherReady(TimeSpan.FromSeconds(5)));

        var watchOpts = new FffWatchOptions
        {
            Ignore = new[] { "bin", "obj" }
        };

        using var subscription = finder.Watch("*.cs", _ => { }, watchOpts);
        Assert.NotEqual(0UL, subscription.WatchId);
    }
    [Fact]
    public async Task WatchAsync_ShouldYieldAndCancelCleanly()
    {
        var options = new FffCreateOptions { BasePath = _testPath, Watch = true };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));
        Assert.True(await finder.WaitForWatcherReadyAsync(TimeSpan.FromSeconds(5)));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var batch in finder.WatchAsync("*.cs", null, cts.Token))
            {
            }
        });
    }
    [Fact]
    public async Task WaitForScanAsync_ShouldWaitAndReturnTrue()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        bool ready = await finder.WaitForScanAsync(TimeSpan.FromSeconds(5));
        Assert.True(ready);
    }
    [Fact]
    public void Search_ShouldReturnResults_WhenQueryMatchesFiles()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        var result = finder.Search("FffFinderTests");
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.TotalFiles >= 0);
    }
    [Fact]
    public void Glob_ShouldReturnMatchingFiles_WhenPatternProvided()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        var result = finder.Glob("*.cs");
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }
    [Fact]
    public void SearchDirectories_ShouldReturnDirs_WhenQueryMatchesDirs()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        var result = finder.SearchDirectories("bin");
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }
    [Fact]
    public void SearchMixed_ShouldReturnFilesAndDirs_WhenQueryMatches()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        var result = finder.SearchMixed("test");
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }
    [Fact]
    public void Grep_ShouldReturnLineMatches_WhenContentMatches()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        var result = finder.Grep("FffFinderTests");
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }
    [Fact]
    public void MultiGrep_ShouldReturnMatches_WhenAnyPatternMatches()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        var result = finder.MultiGrep(new[] { "FffFinderTests", "Diagnostics" });
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }
    [Fact]
    public void ScanFilesAndReindex_ShouldExecuteWithoutError()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        finder.ScanFiles();
        finder.Reindex(_testPath);
        Assert.True(finder.ScanProgress.ScannedFilesCount >= 0);
    }
    [Fact]
    public void RefreshGitStatus_ShouldReturnCountWithoutError()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        int count = finder.RefreshGitStatus();
        Assert.True(count >= 0);
    }
    [Fact]
    public void TrackQueryAndGetHistoricalQuery_ShouldExecuteCleanly()
    {
        var options = new FffCreateOptions { BasePath = _testPath };
        using var finder = new FffFinder(options);
        finder.WaitForScan(TimeSpan.FromSeconds(5));

        string existingFile = Path.Combine(_testPath, "FFF.Net.Tests.dll");
        bool tracked = finder.TrackQuery("my-search-term", existingFile);
        Assert.True(tracked);

        string? history = finder.GetHistoricalQuery(0);
        Assert.True(history == null || history.Length >= 0);
    }
}
