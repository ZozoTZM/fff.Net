using System;

namespace FFF.Net.Models;

/// <summary>
/// Represents a single file match item returned by <see cref="FffFinder.Search"/> or <see cref="FffFinder.Glob"/>.
/// </summary>
public readonly record struct FffFileItem
{
    /// <summary>
    /// Gets the relative path of the file.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the Git status string (e.g. <c>"M"</c>, <c>"??"</c>), or <c>null</c> if unchanged/clean.
    /// </summary>
    public string? GitStatus { get; init; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public ulong Size { get; init; }

    /// <summary>
    /// Gets the last modification timestamp.
    /// </summary>
    public DateTimeOffset Modified { get; init; }

    /// <summary>
    /// Gets the total frecency score for ranking.
    /// </summary>
    public long TotalFrecencyScore { get; init; }

    /// <summary>
    /// Gets whether this file is recognized as binary.
    /// </summary>
    public bool IsBinary { get; init; }
}