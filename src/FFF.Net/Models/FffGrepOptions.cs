namespace FFF.Net.Models;

public sealed class FffGrepOptions
{
    public FffGrepMode Mode { get; set; } = FffGrepMode.Plain;
    public ulong MaxFileSize { get; set; }
    public uint MaxMatchesPerFile { get; set; }
    public bool SmartCase { get; set; } = true;
    public uint FileOffset { get; set; }
    public uint PageLimit { get; set; } = 50;
    public ulong TimeBudgetMs { get; set; }
    public uint BeforeContext { get; set; }
    public uint AfterContext { get; set; }
    public bool ClassifyDefinitions { get; set; }
}
