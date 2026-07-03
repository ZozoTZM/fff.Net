using System.Runtime.InteropServices;

namespace FFF.Net;

internal static partial class FffNative
{
    [LibraryImport(DllName, EntryPoint = "fff_live_grep")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffLiveGrep(
    IntPtr fffHandle,
    [MarshalAs(UnmanagedType.LPUTF8Str)] string query,
    byte mode,
    ulong maxFileSize,
    uint maxMatchesPerFile,
    NativeBool smartCase,
    uint fileOffset,
    uint pageLimit,
    ulong timeBudgetMs,
    uint beforeContext,
    uint afterContext,
    NativeBool classifyDefinitions);

    [LibraryImport(DllName, EntryPoint = "fff_free_grep_result")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FffFreeGrepResult(IntPtr result);

    [LibraryImport(DllName, EntryPoint = "fff_grep_result_get_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepResultGetCount(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_grep_result_get_total_matched")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepResultGetTotalMatched(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_grep_result_get_total_files_searched")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepResultGetTotalFilesSearched(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_grep_result_get_total_files")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepResultGetTotalFiles(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_grep_result_get_filtered_file_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepResultGetFilteredFileCount(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_grep_result_get_next_file_offset")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepResultGetNextFileOffset(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_grep_result_get_regex_fallback_error")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepResultGetRegexFallbackError(IntPtr r);

    [LibraryImport(DllName, EntryPoint = "fff_grep_result_get_match")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepResultGetMatch(IntPtr result, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_relative_path")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepMatchGetRelativePath(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_file_name")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepMatchGetFileName(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_git_status")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepMatchGetGitStatus(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_line_content")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepMatchGetLineContent(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_line_number")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ulong FffGrepMatchGetLineNumber(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_col")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepMatchGetCol(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_byte_offset")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ulong FffGrepMatchGetByteOffset(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_size")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ulong FffGrepMatchGetSize(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_total_frecency_score")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long FffGrepMatchGetTotalFrecencyScore(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_access_frecency_score")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long FffGrepMatchGetAccessFrecencyScore(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_modification_frecency_score")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long FffGrepMatchGetModificationFrecencyScore(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_modified")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ulong FffGrepMatchGetModified(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_match_ranges_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepMatchGetMatchRangesCount(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_match_range")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepMatchGetMatchRange(IntPtr m, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_context_before_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepMatchGetContextBeforeCount(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_context_before")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepMatchGetContextBefore(IntPtr m, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_context_after_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffGrepMatchGetContextAfterCount(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_context_after")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGrepMatchGetContextAfter(IntPtr m, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_fuzzy_score")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial ushort FffGrepMatchGetFuzzyScore(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_has_fuzzy_score")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial NativeBool FffGrepMatchGetHasFuzzyScore(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_is_definition")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial NativeBool FffGrepMatchGetIsDefinition(IntPtr m);

    [LibraryImport(DllName, EntryPoint = "fff_grep_match_get_is_binary")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial NativeBool FffGrepMatchGetIsBinary(IntPtr m);
    [LibraryImport(DllName, EntryPoint = "fff_multi_grep")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffMultiGrep(
    IntPtr fffHandle,
    [MarshalAs(UnmanagedType.LPUTF8Str)] string? patternsJoined,
    [MarshalAs(UnmanagedType.LPUTF8Str)] string? constraints,
    ulong maxFileSize,
    uint maxMatchesPerFile,
    NativeBool smartCase,
    uint fileOffset,
    uint pageLimit,
    ulong timeBudgetMs,
    uint beforeContext,
    uint afterContext,
    NativeBool classifyDefinitions);
}
