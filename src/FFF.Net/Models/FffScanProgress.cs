namespace FFF.Net.Models;

public sealed class FffScanProgress
{
    public ulong ScannedFilesCount { get; init; }
    public bool IsScanning { get; init; }
    public bool IsWatcherReady { get; init; }
    public bool IsWarmupComplete { get; init; }
}