using System.Collections.Generic;

namespace FFF.Net.Models;

/// <summary>
/// Represents the result payload returned by <see cref="FffFinder.Search"/> or <see cref="FffFinder.Glob"/>.
/// </summary>
public sealed class FffSearchResult
{
    /// <summary>
    /// Gets the list of matched file items.
    /// </summary>
    public required IReadOnlyList<FffFileItem> Items { get; init; }

    /// <summary>
    /// Gets the total number of files that matched the search query.
    /// </summary>
    public uint TotalMatched { get; init; }

    /// <summary>
    /// Gets the total number of indexed files.
    /// </summary>
    public uint TotalFiles { get; init; }
}
