namespace FFF.Net.Models;

public sealed class FffSearchResult
{
    public required IReadOnlyList<FffFileItem> Items { get; init; }
    public uint TotalMatched { get; init; }
    public uint TotalFiles { get; init; }
}
