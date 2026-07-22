using FFF.Net.Interop;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FFF.Net;

internal static partial class FffNative
{
    [LibraryImport(DllName, EntryPoint = "fff_search")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffSearch(
        SafeFffHandle fffHandle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? query,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? currentFile,
        uint maxThreads,
        uint pageIndex,
        uint pageSize,
        int comboBoostMultiplier,
        uint minComboCount);

    [LibraryImport(DllName, EntryPoint = "fff_glob")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffGlob(
        SafeFffHandle fffHandle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? pattern,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? currentFile,
        uint maxThreads,
        uint pageIndex,
        uint pageSize);

    [LibraryImport(DllName, EntryPoint = "fff_search_directories")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffSearchDirectories(
        SafeFffHandle fffHandle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? query,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? currentFile,
        uint maxThreads,
        uint pageIndex,
        uint pageSize);

    [LibraryImport(DllName, EntryPoint = "fff_search_mixed")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffSearchMixed(
        SafeFffHandle fffHandle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? query,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? currentFile,
        uint maxThreads,
        uint pageIndex,
        uint pageSize,
        int comboBoostMultiplier,
        uint minComboCount);

    [LibraryImport(DllName, EntryPoint = "fff_free_search_result")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void FffFreeSearchResult(IntPtr result);

    [LibraryImport(DllName, EntryPoint = "fff_free_dir_search_result")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void FffFreeDirSearchResult(IntPtr result);

    [LibraryImport(DllName, EntryPoint = "fff_free_mixed_search_result")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void FffFreeMixedSearchResult(IntPtr result);

    [LibraryImport(DllName, EntryPoint = "fff_search_result_get_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial uint FffSearchResultGetCount(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_search_result_get_total_matched")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial uint FffSearchResultGetTotalMatched(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_search_result_get_total_files")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial uint FffSearchResultGetTotalFiles(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_search_result_get_item")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffSearchResultGetItem(IntPtr result, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_dir_search_result_get_item")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffDirSearchResultGetItem(IntPtr result, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_mixed_search_result_get_item")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffMixedSearchResultGetItem(IntPtr result, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_relative_path")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffFileItemGetRelativePath(IntPtr item);

    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_file_name")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffFileItemGetFileName(IntPtr item);

    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_git_status")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffFileItemGetGitStatus(IntPtr item);

    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_size")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial ulong FffFileItemGetSize(IntPtr item);

    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_modified")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial ulong FffFileItemGetModified(IntPtr item);

    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_total_frecency_score")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial long FffFileItemGetTotalFrecencyScore(IntPtr item);

    [LibraryImport(DllName, EntryPoint = "fff_file_item_get_is_binary")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial NativeBool FffFileItemGetIsBinary(IntPtr item);
}
