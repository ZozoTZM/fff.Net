using System;

namespace FFF.Net.Models;

/// <summary>
/// Represents a single file or directory item returned by a mixed search (<see cref="FffFinder.SearchMixed"/>).
/// </summary>
public readonly record struct FffMixedItem
{
    /// <summary>
    /// Gets whether this item is a file or a directory.
    /// </summary>
    public required FffMixedItemType ItemType { get; init; }

    /// <summary>
    /// Gets the relative path of the file or directory.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the Git status string (e.g. <c>"M"</c>, <c>"??"</c>), or <c>null</c> if unchanged/clean.
    /// </summary>
    public string? GitStatus { get; init; }

    /// <summary>
    /// Gets the file size in bytes (0 for directories).
    /// </summary>
    public ulong Size { get; init; }

    /// <summary>
    /// Gets the last modification time of the file or directory.
    /// </summary>
    public DateTimeOffset Modified { get; init; }

    /// <summary>
    /// Gets the access frecency score component.
    /// </summary>
    public long AccessFrecencyScore { get; init; }

    /// <summary>
    /// Gets the modification frecency score component.
    /// </summary>
    public long ModificationFrecencyScore { get; init; }

    /// <summary>
    /// Gets the total frecency score combining access and modification frecency.
    /// </summary>
    public long TotalFrecencyScore { get; init; }

    /// <summary>
    /// Gets whether this file is recognized as binary.
    /// </summary>
    public bool IsBinary { get; init; }
}
