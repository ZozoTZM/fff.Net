using System;
using System.Collections.Generic;
using System.Text;

namespace FFF.Net;

public sealed class FffFileItem
{
    public required string RelativePath { get; init; }
    public required string FileName { get; init; }
    public string? GitStatus { get; init; }
    public ulong Size { get; init; }
    public DateTimeOffset Modified { get; init; }
    public long TotalFrecencyScore { get; init; }
    public bool IsBinary { get; init; }
}