using System.Collections.Generic;

namespace FFF.Net.Models;

/// <summary>
/// Represents the result payload returned by <see cref="FffFinder.SearchMixed"/>.
/// </summary>
public sealed class FffMixedSearchResult
{
    /// <summary>
    /// Gets the list of matched files and directories.
    /// </summary>
    public required IReadOnlyList<FffMixedItem> Items { get; init; }

    /// <summary>
    /// Gets the total number of items that matched the query.
    /// </summary>
    public uint TotalMatched { get; init; }

    /// <summary>
    /// Gets the total number of indexed files.
    /// </summary>
    public uint TotalFiles { get; init; }

    /// <summary>
    /// Gets the total number of indexed directories.
    /// </summary>
    public uint TotalDirs { get; init; }
}
