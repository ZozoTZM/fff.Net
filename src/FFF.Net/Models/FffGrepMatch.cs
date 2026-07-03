namespace FFF.Net.Models;

public sealed class FffGrepMatch
{
    public required string RelativePath { get; init; }
    public required string FileName { get; init; }
    public string? GitStatus { get; init; }
    public required string LineContent { get; init; }
    public ulong LineNumber { get; init; }
    public uint Col { get; init; }
    public ulong ByteOffset { get; init; }
    public ulong Size { get; init; }
    public long TotalFrecencyScore { get; init; }
    public long AccessFrecencyScore { get; init; }
    public long ModificationFrecencyScore { get; init; }
    public DateTimeOffset Modified { get; init; }
    public required IReadOnlyList<FffMatchRange> MatchRanges { get; init; }
    public required IReadOnlyList<string> ContextBefore { get; init; }
    public required IReadOnlyList<string> ContextAfter { get; init; }
    public ushort FuzzyScore { get; init; }
    public bool HasFuzzyScore { get; init; }
    public bool IsDefinition { get; init; }
    public bool IsBinary { get; init; }
}
