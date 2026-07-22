using FFF.Net.Interop;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FFF.Net;

internal static partial class FffNative
{
    [LibraryImport(DllName, EntryPoint = "fff_set_watch_callback")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static unsafe partial IntPtr FffSetWatchCallback(SafeFffHandle fffHandle, delegate* unmanaged[Cdecl]<ulong, IntPtr, IntPtr, void> callback, IntPtr userData);

    [LibraryImport(DllName, EntryPoint = "fff_watch")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffWatch(SafeFffHandle fffHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string? pattern, in FffNativeWatchOptions opts); 

    [LibraryImport(DllName, EntryPoint = "fff_watch_args")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffWatchArgs(SafeFffHandle fffHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string? pattern, IntPtr ignore, uint ignoreCount);

    [LibraryImport(DllName, EntryPoint = "fff_unwatch")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial IntPtr FffUnwatch(SafeFffHandle fffHandle, ulong watchId);

    [LibraryImport(DllName, EntryPoint = "fff_watch_events_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial uint FffWatchEventsCount(IntPtr batchPtr);

    [LibraryImport(DllName, EntryPoint = "fff_watch_events_get_path")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial IntPtr FffWatchEventGetPath(IntPtr batchPtr, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_watch_events_get_kind")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [SuppressGCTransition]
    public static partial byte FffWatchEventGetKind(IntPtr batchPtr, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_free_watch_events")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void FffFreeWatchEvents(IntPtr batchPtr);
}
