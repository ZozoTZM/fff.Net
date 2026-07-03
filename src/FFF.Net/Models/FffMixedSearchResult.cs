namespace FFF.Net.Models;

public sealed class FffMixedSearchResult
{
    public required IReadOnlyList<FffMixedItem> Items { get; init; }
    public uint TotalMatched { get; init; }
    public uint TotalFiles { get; init; }
    public uint TotalDirs { get; init; }
}
