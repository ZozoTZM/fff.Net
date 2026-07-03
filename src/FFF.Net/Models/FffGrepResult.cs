namespace FFF.Net.Models;

public sealed class FffGrepResult
{
    public required IReadOnlyList<FffGrepMatch> Items { get; init; }
    public uint TotalMatched { get; init; }
    public uint TotalFilesSearched { get; init; }
    public uint TotalFiles { get; init; }
    public uint FilteredFileCount { get; init; }
    public uint NextFileOffset { get; init; }
    public string? RegexFallbackError { get; init; }
}
