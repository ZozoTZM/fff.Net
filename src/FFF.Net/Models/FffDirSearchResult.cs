namespace FFF.Net.Models;

public sealed class FffDirSearchResult
{
    public required IReadOnlyList<FffDirItem> Items { get; init; }
    public uint TotalMatched { get; init; }
    public uint TotalDirs { get; init; }
}
