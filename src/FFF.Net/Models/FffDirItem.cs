namespace FFF.Net.Models;

/// <summary>
/// Represents a single matched directory item returned by <see cref="FffFinder.SearchDirectories"/>.
/// </summary>
public readonly record struct FffDirItem
{
    /// <summary>
    /// Gets the relative path of the directory.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets the folder name.
    /// </summary>
    public required string DirName { get; init; }

    /// <summary>
    /// Gets the maximum access frecency score among immediate child files.
    /// </summary>
    public int MaxAccessFrecency { get; init; }
}
