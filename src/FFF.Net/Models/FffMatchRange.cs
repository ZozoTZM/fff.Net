using System.Runtime.InteropServices;

namespace FFF.Net.Models;

[StructLayout(LayoutKind.Sequential)]
public struct FffMatchRange
{
    public uint Start { get; set; }
    public uint End { get; set; }
}