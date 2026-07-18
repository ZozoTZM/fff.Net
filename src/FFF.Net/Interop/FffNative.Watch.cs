using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FFF.Net;

internal static partial class FffNative
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FffNativeWatchCallback(ulong watchId, IntPtr batchPtr, IntPtr userData);

    [LibraryImport(DllName, EntryPoint = "fff_set_watch_callback")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffSetWatchCallback(IntPtr fffHandle, FffNativeWatchCallback callback, IntPtr userData);

    [LibraryImport(DllName, EntryPoint = "fff_watch")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffWatch(IntPtr fffHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string? pattern, in FffNativeWatchOptions opts); 

    [LibraryImport(DllName, EntryPoint = "fff_watch_args")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffWatchArgs(IntPtr fffHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string? pattern, IntPtr ignore, uint ignoreCount);

    [LibraryImport(DllName, EntryPoint = "fff_unwatch")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffUnwatch(IntPtr fffHandle, ulong watchId);

    [LibraryImport(DllName, EntryPoint = "fff_watch_events_count")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint FffWatchEventsCount(IntPtr batchPtr);

    [LibraryImport(DllName, EntryPoint = "fff_watch_events_get_path")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial IntPtr FffWatchEventGetPath(IntPtr batchPtr, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_watch_events_get_kind")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial byte FffWatchEventGetKind(IntPtr batchPtr, uint index);

    [LibraryImport(DllName, EntryPoint = "fff_free_watch_events")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FffFreeWatchEvents(IntPtr batchPtr);

}
