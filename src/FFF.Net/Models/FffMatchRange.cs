using System.Runtime.InteropServices;

namespace FFF.Net.Models;

/// <summary>
/// Represents a start and end character offset range for highlighting matched substrings.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FffMatchRange
{
    /// <summary>Gets or sets the 0-based start character offset.</summary>
    public uint Start { get; set; }

    /// <summary>Gets or sets the 0-based end character offset.</summary>
    public uint End { get; set; }
}