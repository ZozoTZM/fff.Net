using System;
using System.Runtime.InteropServices;

namespace FFF.Net;

internal static partial class FffNative
{
    private const string DllName = "c-lib-x86_64-pc-windows-msvc";

    [StructLayout(LayoutKind.Sequential)]
    public struct FffResult
    {
        public byte Success;
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
        public byte EnableMmapCache;
        public byte EnableContentIndexing;
        public byte Watch;
        public byte AiMode;
        public IntPtr LogFilePath;
        public IntPtr LogLevel;
        public ulong CacheBudgetMaxFiles;
        public ulong CacheBudgetMaxBytes;
        public ulong CacheBudgetMaxFileSize;
        public byte EnableFsRootScanning;
        public byte EnableHomeDirScanning;
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

    [LibraryImport(DllName, EntryPoint = "fff_search")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffSearch(
       IntPtr fffHandle,
       [MarshalAs(UnmanagedType.LPUTF8Str)] string? query,
       [MarshalAs(UnmanagedType.LPUTF8Str)] string? currentFile,
       uint maxThreads,
       uint pageIndex,
       uint pageSize,
       int comboBoostMultiplier,
       uint minComboCount);

    [LibraryImport(DllName, EntryPoint = "fff_free_search_result")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FffFreeSearchResult(IntPtr result);
    [LibraryImport(DllName, EntryPoint = "fff_search_result_get_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffSearchResultGetCount(IntPtr r);
    [LibraryImport(DllName, EntryPoint = "fff_search_result_get_total_matched")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffSearchResultGetTotalMatched(IntPtr r);
    [LibraryImport(DllName, EntryPoint = "fff_search_result_get_total_files")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffSearchResultGetTotalFiles(IntPtr r);
    [LibraryImport(DllName, EntryPoint = "fff_search_result_get_item")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffSearchResultGetItem(IntPtr result, uint index);
    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_relative_path")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffFileItemGetRelativePath(IntPtr item);
    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_file_name")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffFileItemGetFileName(IntPtr item);
    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_git_status")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffFileItemGetGitStatus(IntPtr item);
    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_size")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ulong FffFileItemGetSize(IntPtr item);
    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_modified")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ulong FffFileItemGetModified(IntPtr item);
    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_total_frecency_score")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long FffFileItemGetTotalFrecencyScore(IntPtr item);
    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_is_binary")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial byte FffFileItemGetIsBinary(IntPtr item);
    [LibraryImport(DllName, EntryPoint = "fff_glob")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGlob(
       IntPtr fffHandle,
       [MarshalAs(UnmanagedType.LPUTF8Str)] string? pattern,
       [MarshalAs(UnmanagedType.LPUTF8Str)] string? currentFile,
       uint maxThreads,
       uint pageIndex,
       uint pageSize);

    [LibraryImport(DllName, EntryPoint = "fff_search_directories")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffSearchDirectories(
    IntPtr fffHandle,
    [MarshalAs(UnmanagedType.LPUTF8Str)] string? query,
    [MarshalAs(UnmanagedType.LPUTF8Str)] string? currentFile,
    uint maxThreads,
    uint pageIndex,
    uint pageSize);

    [LibraryImport(DllName, EntryPoint = "fff_free_dir_search_result")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FffFreeDirSearchResult(IntPtr result);

    [LibraryImport(DllName, EntryPoint = "fff_dir_search_result_get_item")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffDirSearchResultGetItem(IntPtr result, uint index);
}