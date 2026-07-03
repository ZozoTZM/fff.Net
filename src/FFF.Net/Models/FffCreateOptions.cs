using System.Runtime.InteropServices;

namespace FFF.Net.Models;

public sealed class FffCreateOptions
{
    public required string BasePath { get; set; }
    public string? FrecencyDbPath { get; set; }
    public string? HistoryDbPath { get; set; }
    public bool EnableMmapCache { get; set; }
    public bool EnableContentIndexing { get; set; }
    public bool Watch { get; set; }
    public bool AiMode { get; set; }
    public string? LogFilePath { get; set; }
    public string? LogLevel { get; set; }
    public ulong CacheBudgetMaxFiles { get; set; }
    public ulong CacheBudgetMaxBytes { get; set; }
    public ulong CacheBudgetMaxFileSize { get; set; }
    public bool EnableFsRootScanning { get; set; }
    public bool EnableHomeDirScanning { get; set; }

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
            Version = 1,
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
            EnableHomeDirScanning = EnableHomeDirScanning
        };

        allocatedStrings = list;
        return native;
    }
}