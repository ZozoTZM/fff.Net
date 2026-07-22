using FFF.Net.Interop;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FFF.Net;

internal static partial class FffNative
{
    [LibraryImport(DllName, EntryPoint = "fff_scan_files")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffScanFiles(SafeFffHandle fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_is_scanning")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial NativeBool FffIsScanning(SafeFffHandle fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_get_scan_progress")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffGetScanProgress(SafeFffHandle fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_free_scan_progress")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void FffFreeScanProgress(IntPtr result);

    [LibraryImport(DllName, EntryPoint = "fff_restart_index")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffRestartIndex(SafeFffHandle fffHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath);

    [LibraryImport(DllName, EntryPoint = "fff_refresh_git_status")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffRefreshGitStatus(SafeFffHandle fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_track_query")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffTrackQuery(
        SafeFffHandle fffHandle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string query,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string filePath);

    [LibraryImport(DllName, EntryPoint = "fff_get_historical_query")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffGetHistoricalQuery(SafeFffHandle fffHandle, ulong offset);
}
