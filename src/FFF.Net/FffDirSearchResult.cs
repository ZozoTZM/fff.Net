using System;
using System.Collections.Generic;
using System.Text;

namespace FFF.Net;

public sealed class FffDirSearchResult
{
    public required IReadOnlyList<FffDirItem> Items { get; init; }
    public uint TotalMatched { get; init; }
    public uint TotalDirs { get; init; }
}
