namespace FFF.Net.Models;

public readonly record struct FffMixedItem
{
    public required FffMixedItemType ItemType { get; init; }
    public required string RelativePath { get; init; }
    public required string DisplayName { get; init; }
    public string? GitStatus { get; init; }
    public ulong Size { get; init; }
    public DateTimeOffset Modified { get; init; }
    public long AccessFrecencyScore { get; init; }
    public long ModificationFrecencyScore { get; init; }
    public long TotalFrecencyScore { get; init; }
    public bool IsBinary { get; init; }
}
