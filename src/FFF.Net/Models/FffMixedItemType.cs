namespace FFF.Net.Models;

/// <summary>
/// Specifies whether a mixed search result item is a file or a directory.
/// </summary>
public enum FffMixedItemType : byte
{
    /// <summary>
    /// File item.
    /// </summary>
    File = 0,

    /// <summary>
    /// Directory item.
    /// </summary>
    Directory = 1
}
