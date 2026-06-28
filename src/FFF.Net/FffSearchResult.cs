using System;
using System.Collections.Generic;
using System.Text;

namespace FFF.Net;

public sealed class FffSearchResult
{
    public required IReadOnlyList<FffFileItem> Items { get; init; }
    public uint TotalMatched { get; init; }
    public uint TotalFiles { get; init; }
}
