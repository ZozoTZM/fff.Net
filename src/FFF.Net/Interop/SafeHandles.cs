using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace FFF.Net.Interop;

internal sealed class SafeFffHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeFffHandle() : base(ownsHandle: true) { }

    public SafeFffHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        FffNative.FffDestroy(handle);
        return true;
    }
}

internal sealed class SafeFffResultHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeFffResultHandle() : base(ownsHandle: true) { }

    public SafeFffResultHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        FffNative.FffFreeResult(handle);
        return true;
    }
}

internal sealed class SafeFffSearchResultHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeFffSearchResultHandle() : base(ownsHandle: true) { }

    public SafeFffSearchResultHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        FffNative.FffFreeSearchResult(handle);
        return true;
    }
}

internal sealed class SafeFffGrepResultHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeFffGrepResultHandle() : base(ownsHandle: true) { }

    public SafeFffGrepResultHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        FffNative.FffFreeGrepResult(handle);
        return true;
    }
}

internal sealed class SafeFffDirSearchResultHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeFffDirSearchResultHandle() : base(ownsHandle: true) { }

    public SafeFffDirSearchResultHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        FffNative.FffFreeDirSearchResult(handle);
        return true;
    }
}

internal sealed class SafeFffMixedSearchResultHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeFffMixedSearchResultHandle() : base(ownsHandle: true) { }

    public SafeFffMixedSearchResultHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        FffNative.FffFreeMixedSearchResult(handle);
        return true;
    }
}

internal sealed class SafeFffWatchEventBatchHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeFffWatchEventBatchHandle() : base(ownsHandle: true) { }

    public SafeFffWatchEventBatchHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        FffNative.FffFreeWatchEvents(handle);
        return true;
    }
}
