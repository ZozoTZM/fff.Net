using System.Runtime.InteropServices;

namespace FFF.Net.Models;

/// <summary>
/// Configuration options used when initializing an <see cref="FffFinder"/> instance.
/// Maps to native <c>FffCreateOptions</c> struct (version 2).
/// </summary>
public sealed class FffCreateOptions
{
    /// <summary>
    /// The root directory path to scan and index (required).
    /// </summary>
    public required string BasePath { get; set; }

    /// <summary>
    /// Optional file path for persisting the frecency scoring database.
    /// </summary>
    public string? FrecencyDbPath { get; set; }

    /// <summary>
    /// Optional file path for persisting the search history database.
    /// </summary>
    public string? HistoryDbPath { get; set; }

    /// <summary>
    /// Enable memory-mapped file cache for faster content scanning and grep operations.
    /// </summary>
    public bool EnableMmapCache { get; set; }

    /// <summary>
    /// Enable background content indexing for fast full-text queries (<see cref="FffFinder.Grep"/>).
    /// </summary>
    public bool EnableContentIndexing { get; set; }

    /// <summary>
    /// Enable background file system watching so index updates live as files change on disk.
    /// Must be set to <c>true</c> to use <see cref="FffFinder.Watch"/> or <see cref="FffFinder.WatchAsync"/>.
    /// </summary>
    public bool Watch { get; set; }

    /// <summary>
    /// Enable AI mode optimizations (e.g. enhanced frecency and token tracking).
    /// </summary>
    public bool AiMode { get; set; }

    /// <summary>
    /// Optional path to write internal diagnostic log files.
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Minimum log severity level (e.g. <c>"debug"</c>, <c>"info"</c>, <c>"warn"</c>, <c>"error"</c>).
    /// </summary>
    public string? LogLevel { get; set; }

    /// <summary>
    /// Maximum number of files to retain in memory cache.
    /// </summary>
    public ulong CacheBudgetMaxFiles { get; set; }

    /// <summary>
    /// Maximum total bytes allocated for memory-mapped file cache.
    /// </summary>
    public ulong CacheBudgetMaxBytes { get; set; }

    /// <summary>
    /// Maximum individual file size in bytes to cache in memory.
    /// </summary>
    public ulong CacheBudgetMaxFileSize { get; set; }

    /// <summary>
    /// Allow scanning from the root of the file system.
    /// </summary>
    public bool EnableFsRootScanning { get; set; }

    /// <summary>
    /// Allow scanning inside the user's home directory across different workspaces.
    /// </summary>
    public bool EnableHomeDirScanning { get; set; }

    /// <summary>
    /// If <c>true</c>, symbolic links will be followed when scanning directories.
    /// Requires native <c>FffCreateOptions</c> struct version 2 (`fff v0.10.0+`).
    /// </summary>
    public bool FollowSymlinks { get; set; }

    internal FffNative.FffCreateOptions ToNative(out List<IntPtr> allocatedStrings)
    {
        var list = new List<IntPtr>();

        IntPtr MarshalString(string? value)
        {
            if (value == null) return IntPtr.Zero;
            IntPtr ptr = Marshal.StringToCoTaskMemUTF8(value);
            list.Add(ptr);
            return ptr;
        }

        var native = new FffNative.FffCreateOptions
        {
            Version = 2,
            BasePath = MarshalString(BasePath),
            FrecencyDbPath = MarshalString(FrecencyDbPath),
            HistoryDbPath = MarshalString(HistoryDbPath),
            EnableMmapCache = EnableMmapCache,
            EnableContentIndexing = EnableContentIndexing,
            Watch = Watch,
            AiMode = AiMode,
            LogFilePath = MarshalString(LogFilePath),
            LogLevel = MarshalString(LogLevel),
            CacheBudgetMaxFiles = CacheBudgetMaxFiles,
            CacheBudgetMaxBytes = CacheBudgetMaxBytes,
            CacheBudgetMaxFileSize = CacheBudgetMaxFileSize,
            EnableFsRootScanning = EnableFsRootScanning,
            EnableHomeDirScanning = EnableHomeDirScanning,
            FollowSymlinks = FollowSymlinks
        };

        allocatedStrings = list;
        return native;
    }
}