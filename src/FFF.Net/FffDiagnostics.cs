using System.Runtime.InteropServices;

namespace FFF.Net;

public static class FffDiagnostics
{
    /// <summary>
    /// Get health check information.
    /// </summary>
    /// <returns>A JSON string containing the health check data (version + git info).</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library fails to execute the health check, returns a null result pointer, 
    /// or if the operation success flag is false.
    /// </exception>
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

            if (!result.Success)
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