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
    public void Methods_AfterDisposed_ShouldThrowObjectDisposedException()
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
    }
}
