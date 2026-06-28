using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FFF.Net;

public sealed class FffFinder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    public FffFinder(FffCreateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.BasePath))
        {
            throw new ArgumentException("Base path must not be null or empty.", nameof(options));
        }

        var nativeOpts = options.ToNative(out var allocatedStrings);
        try
        {
            IntPtr resultPtr = FffNative.FffCreateInstanceWith(in nativeOpts);
            if (resultPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to initialize FffFinder: Native function returned null result pointer.");
            }

            try
            {
                var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
                if (result.Success == 0)
                {
                    string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                    throw new InvalidOperationException($"Failed to initialize FffFinder: {errorMsg}");
                }

                _handle = result.Handle;
            }
            finally
            {
                FffNative.FffFreeResult(resultPtr);
            }
        }
        finally
        {
            foreach (var ptr in allocatedStrings)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }
    }

    public bool WaitForScan(TimeSpan timeout)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ulong timeoutMs = (ulong)timeout.TotalMilliseconds;
        IntPtr resultPtr = FffNative.FffWaitForScan(_handle, timeoutMs);
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to wait for scan: Native function returned null result pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (result.Success == 0)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Wait for scan failed: {errorMsg}");
            }

            return result.IntValue == 1;
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    public FffSearchResult Search(
        string query,
        string? currentFile = null,
        uint maxThreads = 0,
        uint pageIndex = 0,
        uint pageSize = 0,
        int comboBoostMultiplier = 0,
        uint minComboCount = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        IntPtr resultPtr = FffNative.FffSearch(
            _handle,
            query,
            currentFile,
            maxThreads,
            pageIndex,
            pageSize,
            comboBoostMultiplier,
            minComboCount);

        return ParseSearchResult(resultPtr);
    }
    public FffSearchResult Glob(
        string pattern,
        string? currentFile = null,
        uint maxThreads = 0,
        uint pageIndex = 0,
        uint pageSize = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(pattern);
        IntPtr resultPtr = FffNative.FffGlob(
            _handle,
            pattern,
            currentFile,
            maxThreads,
            pageIndex,
            pageSize);
        return ParseSearchResult(resultPtr);
    }
    private FffSearchResult ParseSearchResult(IntPtr resultPtr)
    {
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to get search result: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (result.Success == 0)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Search failed: {errorMsg}");
            }
            IntPtr searchResultPtr = result.Handle;
            if (searchResultPtr == IntPtr.Zero)
            {
                return new FffSearchResult
                {
                    Items = [],
                    TotalMatched = 0,
                    TotalFiles = 0
                };
            }
            try
            {
                uint count = FffNative.FffSearchResultGetCount(searchResultPtr);
                uint totalMatched = FffNative.FffSearchResultGetTotalMatched(searchResultPtr);
                uint totalFiles = FffNative.FffSearchResultGetTotalFiles(searchResultPtr);
                var items = new List<FffFileItem>((int)count);
                for (uint i = 0; i < count; i++)
                {
                    IntPtr itemPtr = FffNative.FffSearchResultGetItem(searchResultPtr, i);
                    if (itemPtr == IntPtr.Zero) continue;
                    string? relativePath = Marshal.PtrToStringUTF8(FffNative.FffFileItemGetRelativePath(itemPtr));
                    string? fileName = Marshal.PtrToStringUTF8(FffNative.FffFileItemGetFileName(itemPtr));
                    string? gitStatus = Marshal.PtrToStringUTF8(FffNative.FffFileItemGetGitStatus(itemPtr));
                    ulong size = FffNative.FffFileItemGetSize(itemPtr);
                    ulong modifiedSeconds = FffNative.FffFileItemGetModified(itemPtr);
                    long totalScore = FffNative.FffFileItemGetTotalFrecencyScore(itemPtr);
                    bool isBinary = FffNative.FffFileItemGetIsBinary(itemPtr) != 0;
                    items.Add(new FffFileItem
                    {
                        RelativePath = relativePath ?? string.Empty,
                        FileName = fileName ?? string.Empty,
                        GitStatus = string.IsNullOrWhiteSpace(gitStatus) ? null : gitStatus,
                        Size = size,
                        Modified = DateTimeOffset.FromUnixTimeSeconds((long)modifiedSeconds),
                        TotalFrecencyScore = totalScore,
                        IsBinary = isBinary
                    });
                }
                return new FffSearchResult
                {
                    Items = items,
                    TotalMatched = totalMatched,
                    TotalFiles = totalFiles
                };
            }
            finally
            {
                FffNative.FffFreeSearchResult(searchResultPtr);
            }
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }
    public FffDirSearchResult SearchDirectories(
    string query,
    string? currentFile = null,
    uint maxThreads = 0,
    uint pageIndex = 0,
    uint pageSize = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        IntPtr resultPtr = FffNative.FffSearchDirectories(
            _handle,
            query,
            currentFile,
            maxThreads,
            pageIndex,
            pageSize);

        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to search directories: Native function returned null result pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (result.Success == 0)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Search directories failed: {errorMsg}");
            }

            IntPtr searchResultPtr = result.Handle;
            if (searchResultPtr == IntPtr.Zero)
            {
                return new FffDirSearchResult
                {
                    Items = [],
                    TotalMatched = 0,
                    TotalDirs = 0
                };
            }

            try
            {
                
                var nativeResult = Marshal.PtrToStructure<FffNative.FffNativeDirSearchResult>(searchResultPtr);
                uint count = nativeResult.Count;
                uint totalMatched = nativeResult.TotalMatched;
                uint totalDirs = nativeResult.TotalDirs;

                var items = new List<FffDirItem>((int)count);
                for (uint i = 0; i < count; i++)
                {
                    IntPtr itemPtr = FffNative.FffDirSearchResultGetItem(searchResultPtr, i);
                    if (itemPtr == IntPtr.Zero) continue;

                    var nativeItem = Marshal.PtrToStructure<FffNative.FffNativeDirItem>(itemPtr);
                    items.Add(new FffDirItem
                    {
                        RelativePath = Marshal.PtrToStringUTF8(nativeItem.RelativePath) ?? string.Empty,
                        DirName = Marshal.PtrToStringUTF8(nativeItem.DirName) ?? string.Empty,
                        MaxAccessFrecency = nativeItem.MaxAccessFrecency
                    });
                }

                return new FffDirSearchResult
                {
                    Items = items,
                    TotalMatched = totalMatched,
                    TotalDirs = totalDirs
                };
            }
            finally
            {
                FffNative.FffFreeDirSearchResult(searchResultPtr);
            }
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                FffNative.FffDestroy(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    ~FffFinder()
    {
        Dispose(false);
    }


}