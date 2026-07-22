namespace FFF.Net.Models;

/// <summary>
/// Diagnostic details describing background index scanning and watcher warmup progress.
/// </summary>
public sealed class FffScanProgress
{
    /// <summary>
    /// Gets the number of files indexed so far.
    /// </summary>
    public ulong ScannedFilesCount { get; init; }

    /// <summary>
    /// Gets whether index scanning is currently in progress.
    /// </summary>
    public bool IsScanning { get; init; }

    /// <summary>
    /// Gets whether the background file watcher thread is warm and actively monitoring disk changes.
    /// </summary>
    public bool IsWatcherReady { get; init; }

    /// <summary>
    /// Gets whether background cache warmup has completed.
    /// </summary>
    public bool IsWarmupComplete { get; init; }
}