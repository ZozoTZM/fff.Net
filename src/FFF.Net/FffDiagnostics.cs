using FFF.Net.Interop;
using FFF.Net.Models;
using System;
using System.Runtime.InteropServices;

namespace FFF.Net;

/// <summary>
/// Provides diagnostic and health check utilities for the FFF native library.
/// </summary>
public static class FffDiagnostics
{
    /// <summary>
    /// Get health check information.
    /// </summary>
    /// <returns>A JSON string containing the health check data (version + git info).</returns>
    /// <exception cref="FffNativeException">
    /// Thrown if the native library fails to execute the health check or returns an error.
    /// </exception>
    public static string GetHealthCheck()
    {
        using var nullHandle = new SafeFffHandle(IntPtr.Zero, ownsHandle: false);
        IntPtr resultPtr = FffNative.FffHealthCheck(nullHandle, null);
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to call fff_health_check: returned a null pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);

            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Health check failed: {errorMsg}");
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