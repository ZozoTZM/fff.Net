namespace FFF.Net.Models;


public readonly record struct FffDirItem
{
    public required string RelativePath { get; init; }
    public required string DirName { get; init; }
    public int MaxAccessFrecency { get; init; }
}
