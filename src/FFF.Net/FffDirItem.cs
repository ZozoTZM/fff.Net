using System;
using System.Collections.Generic;
using System.Text;

namespace FFF.Net;


public sealed class FffDirItem
{
    public required string RelativePath { get; init; }
    public required string DirName { get; init; }
    public int MaxAccessFrecency { get; init; }
}
