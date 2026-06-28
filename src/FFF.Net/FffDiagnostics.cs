using System;
using System.Runtime.InteropServices;

namespace FFF.Net;

public static class FffDiagnostics
{
    public static string GetHealthCheck()
    {
        IntPtr resultPtr = FffNative.FffHealthCheck(IntPtr.Zero, null);
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to call fff_health_check: returned a null pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);

            if (result.Success == 0)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Health check failed: {errorMsg}");
            }

            string? json = Marshal.PtrToStringUTF8(result.Handle);

            if (result.Handle != IntPtr.Zero)
            {
                FffNative.FffFreeString(result.Handle);
            }

            return json ?? "{}";
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }
}