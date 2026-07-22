namespace FFF.Net.Models;

/// <summary>
/// Specifies the search mode used during content grep queries.
/// </summary>
public enum FffGrepMode : byte
{
    /// <summary>Plain substring search.</summary>
    Plain = 0,

    /// <summary>Regular expression search.</summary>
    Regex = 1,

    /// <summary>Fuzzy content search.</summary>
    Fuzzy = 2
}
