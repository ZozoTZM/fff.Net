using FFF.Net.Interop;
using FFF.Net.Models;
using System;
using System.Runtime.CompilerServices;
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
        public NativeBool FollowSymlinks;
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
        public IntPtr Items;
        public IntPtr Scores;
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeWatchEvent
    {
        public IntPtr Path;
        public byte Kind;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeWatchEventBatch
    {
        public IntPtr Events;
        public uint Count;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FffNativeWatchOptions
    {
        public uint Version;
        public IntPtr Ignore;
        public uint IgnoreCount;
    }

    [LibraryImport(DllName, EntryPoint = "fff_health_check")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffHealthCheck(SafeFffHandle fffHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string? testPath);

    [LibraryImport(DllName, EntryPoint = "fff_create_instance_with")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffCreateInstanceWith(in FffCreateOptions opts);

    [LibraryImport(DllName, EntryPoint = "fff_destroy")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void FffDestroy(IntPtr fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_free_result")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void FffFreeResult(IntPtr resultPtr);

    [LibraryImport(DllName, EntryPoint = "fff_free_string")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void FffFreeString(IntPtr s);

    [LibraryImport(DllName, EntryPoint = "fff_wait_for_scan")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffWaitForScan(SafeFffHandle fffHandle, ulong timeoutMs);

    [LibraryImport(DllName, EntryPoint = "fff_get_base_path")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffGetBasePath(SafeFffHandle fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_result_get_success")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial NativeBool FffResultGetSuccess(IntPtr resultPtr);

    [LibraryImport(DllName, EntryPoint = "fff_result_get_error")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffResultGetError(IntPtr resultPtr);

    [LibraryImport(DllName, EntryPoint = "fff_result_get_handle")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffResultGetHandle(IntPtr resultPtr); 

    [LibraryImport(DllName, EntryPoint = "fff_result_get_int_value")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial long FffResultGetIntValue(IntPtr resultPtr);
}