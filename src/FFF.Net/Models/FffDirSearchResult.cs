using System.Collections.Generic;

namespace FFF.Net.Models;

/// <summary>
/// Represents the result payload returned by <see cref="FffFinder.SearchDirectories"/>.
/// </summary>
public sealed class FffDirSearchResult
{
    /// <summary>
    /// Gets the list of matched directory items.
    /// </summary>
    public required IReadOnlyList<FffDirItem> Items { get; init; }

    /// <summary>
    /// Gets the total number of directories that matched the query.
    /// </summary>
    public uint TotalMatched { get; init; }

    /// <summary>
    /// Gets the total number of indexed directories.
    /// </summary>
    public uint TotalDirs { get; init; }
}
