using System;
using System.Collections.Generic;

namespace FFF.Net.Models;

/// <summary>
/// Represents a single line match item returned by <see cref="FffFinder.Grep(string, FffGrepOptions?)"/> or <see cref="FffFinder.MultiGrep(IReadOnlyList{string}, string?, FffGrepOptions?)"/>.
/// </summary>
public sealed class FffGrepMatch
{
    /// <summary>Gets the relative path of the file containing the match.</summary>
    public required string RelativePath { get; init; }

    /// <summary>Gets the filename containing the match.</summary>
    public required string FileName { get; init; }

    /// <summary>Gets the Git status string, or <c>null</c> if clean.</summary>
    public string? GitStatus { get; init; }

    /// <summary>Gets the full content of the matching line.</summary>
    public required string LineContent { get; init; }

    /// <summary>Gets the 1-based line number where the match occurred.</summary>
    public ulong LineNumber { get; init; }

    /// <summary>Gets the 1-based column offset where the match begins.</summary>
    public uint Col { get; init; }

    /// <summary>Gets the byte offset into the file.</summary>
    public ulong ByteOffset { get; init; }

    /// <summary>Gets the file size in bytes.</summary>
    public ulong Size { get; init; }

    /// <summary>Gets the total frecency score for ranking.</summary>
    public long TotalFrecencyScore { get; init; }

    /// <summary>Gets the access frecency score component.</summary>
    public long AccessFrecencyScore { get; init; }

    /// <summary>Gets the modification frecency score component.</summary>
    public long ModificationFrecencyScore { get; init; }

    /// <summary>Gets the last modification timestamp.</summary>
    public DateTimeOffset Modified { get; init; }

    /// <summary>Gets the ranges of character indices highlighted within the line content.</summary>
    public required IReadOnlyList<FffMatchRange> MatchRanges { get; init; }

    /// <summary>Gets lines preceding the match when context is requested.</summary>
    public required IReadOnlyList<string> ContextBefore { get; init; }

    /// <summary>Gets lines following the match when context is requested.</summary>
    public required IReadOnlyList<string> ContextAfter { get; init; }

    /// <summary>Gets the fuzzy match score if fuzzy grep mode was used.</summary>
    public ushort FuzzyScore { get; init; }

    /// <summary>Gets whether a fuzzy score was computed for this match.</summary>
    public bool HasFuzzyScore { get; init; }

    /// <summary>Gets whether this line match represents a symbol definition.</summary>
    public bool IsDefinition { get; init; }

    /// <summary>Gets whether the containing file is binary.</summary>
    public bool IsBinary { get; init; }
}
