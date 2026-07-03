using System.Runtime.InteropServices;

namespace FFF.Net;

internal static partial class FffNative
{
    [LibraryImport(DllName, EntryPoint = "fff_scan_files")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffScanFiles(IntPtr fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_is_scanning")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial NativeBool FffIsScanning(IntPtr fffHandle);
    [LibraryImport(DllName, EntryPoint = "fff_get_scan_progress")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGetScanProgress(IntPtr fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_free_scan_progress")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FffFreeScanProgress(IntPtr result);

    [LibraryImport(DllName, EntryPoint = "fff_restart_index")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffRestartIndex(IntPtr fffHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath);

    [LibraryImport(DllName, EntryPoint = "fff_refresh_git_status")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffRefreshGitStatus(IntPtr fffHandle);

    [LibraryImport(DllName, EntryPoint = "fff_track_query")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffTrackQuery(
        IntPtr fffHandle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string query,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string filePath);

    [LibraryImport(DllName, EntryPoint = "fff_get_historical_query")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffGetHistoricalQuery(IntPtr fffHandle, ulong offset);
}
