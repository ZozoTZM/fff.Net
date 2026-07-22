using System.Collections.Generic;

namespace FFF.Net.Models;

/// <summary>
/// Represents the result payload returned by <see cref="FffFinder.Grep(string, FffGrepOptions?)"/> or <see cref="FffFinder.MultiGrep(IReadOnlyList{string}, string?, FffGrepOptions?)"/>.
/// </summary>
public sealed class FffGrepResult
{
    /// <summary>Gets the list of matching line entries.</summary>
    public required IReadOnlyList<FffGrepMatch> Items { get; init; }

    /// <summary>Gets the total number of line matches found across all files.</summary>
    public uint TotalMatched { get; init; }

    /// <summary>Gets the total number of files searched.</summary>
    public uint TotalFilesSearched { get; init; }

    /// <summary>Gets the total number of indexed files.</summary>
    public uint TotalFiles { get; init; }

    /// <summary>Gets the count of files filtered out by options.</summary>
    public uint FilteredFileCount { get; init; }

    /// <summary>Gets the file offset for fetching the next page of results.</summary>
    public uint NextFileOffset { get; init; }

    /// <summary>Gets any regex fallback warning/error message if regex compilation fell back to plain search.</summary>
    public string? RegexFallbackError { get; init; }
}
