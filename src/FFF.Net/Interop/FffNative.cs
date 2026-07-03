using FFF.Net.Models;
using System.Runtime.InteropServices;

namespace FFF.Net;

internal static partial class FffNative
{
    private const string DllName = "fff";

    [StructLayout(LayoutKind.Sequential)]
    public struct FffResult
    {
        public NativeBool Success;
        public IntPtr Error;
        public IntPtr Handle;
        public long IntValue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FffCreateOptions
    {
        public uint Version;
        public IntPtr BasePath;
        public IntPtr FrecencyDbPath;
        public IntPtr HistoryDbPath;
        public NativeBool EnableMmapCache;
        public NativeBool EnableContentIndexing;
        public NativeBool Watch;
        public NativeBool AiMode;
        public IntPtr LogFilePath;
        public IntPtr LogLevel;
        public ulong CacheBudgetMaxFiles;
        public ulong CacheBudgetMaxBytes;
        public ulong CacheBudgetMaxFileSize;
        public NativeBool EnableFsRootScanning;
        public NativeBool EnableHomeDirScanning;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeDirItem
    {
        public IntPtr RelativePath;
        public IntPtr DirName;
        public int MaxAccessFrecency;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeDirSearchResult
    {
        public IntPtr Items;
        public IntPtr Scores;
        public uint Count;
        public uint TotalMatched;
        public uint TotalDirs;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeLocation
    {
        public NativeBool Tag;
        public int Line;
        public int Col;
        public int EndLine;
        public int EndCol;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeMixedSearchResult
    {
        public IntPtr Items; // FffMixedItem*
        public IntPtr Scores; // FffScore*
        public uint Count;
        public uint TotalMatched;
        public uint TotalFiles;
        public uint TotalDirs;
        public FffNativeLocation Location;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeMixedItem
    {
        public FffMixedItemType ItemType;
        public IntPtr RelativePath;
        public IntPtr DisplayName;
        public IntPtr GitStatus;
        public ulong Size;
        public ulong Modified;
        public long AccessFrecencyScore;
        public long ModificationFrecencyScore;
        public long TotalFrecencyScore;
        public NativeBool IsBinary;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeScanProgress
    {
        public ulong ScannedFilesCount;
        public NativeBool IsScanning;
        public NativeBool IsWatcherReady;
        public NativeBool IsWarmupComplete;
    }
    [LibraryImport(DllName, EntryPoint = "fff_health_check")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffHealthCheck(IntPtr fffHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string? testPath);

    [LibraryImport(DllName, EntryPoint = "fff_create_instance_with")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffCreateInstanceWith(in FffCreateOptions opts);

    [LibraryImport(DllName, EntryPoint = "fff_destroy")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FffDestroy(IntPtr fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_free_result")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FffFreeResult(IntPtr resultPtr);

    [LibraryImport(DllName, EntryPoint = "fff_free_string")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FffFreeString(IntPtr s);

    [LibraryImport(DllName, EntryPoint = "fff_wait_for_scan")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffWaitForScan(IntPtr fffHandle, ulong timeoutMs);

    [LibraryImport(DllName, EntryPoint = "fff_get_base_path")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGetBasePath(IntPtr fffHandle);


}