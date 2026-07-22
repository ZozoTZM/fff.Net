using FFF.Net.Interop;
using FFF.Net.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FFF.Net;

/// <summary>
/// Provides high-performance fuzzy file finding, live content search (grep), and file system watching over Dmitriy Kovalenko's native fff library.
/// </summary>
public sealed class FffFinder : IDisposable
{
    private readonly SafeFffHandle _handle;
    private bool _disposed;
    private readonly Lock _lock = new();
    private bool _isWatchCallbackRegistered;
    private GCHandle _weakGcHandle;
    private readonly ConcurrentDictionary<ulong, (Action<IReadOnlyList<FffWatchEvent>> Callback, Action<Exception>? OnError)> _watchCallbacks = new();

    /// <summary>
    /// Occurs when an unhandled exception is thrown inside a file watcher callback delegate.
    /// </summary>
    public event EventHandler<FffWatchErrorEventArgs>? OnWatchError;

    /// <summary>
    /// Gets the base path directory being indexed.
    /// </summary>
    public string BasePath
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            IntPtr resultPtr = FffNative.FffGetBasePath(_handle);
            if (resultPtr == IntPtr.Zero)
            {
                throw new FffNativeException("Failed to get base path: Native function returned null result pointer.");
            }
            try
            {
                var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
                if (!result.Success)
                {
                    string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                    throw new FffNativeException($"Get base path failed: {errorMsg}");
                }
                string? path = Marshal.PtrToStringUTF8(result.Handle);
                if (result.Handle != IntPtr.Zero)
                {
                    FffNative.FffFreeString(result.Handle);
                }
                return path ?? throw new FffNativeException("Native library returned a null base path.");
            }
            finally
            {
                FffNative.FffFreeResult(resultPtr);
            }
        }
    }

    /// <summary>
    /// Gets whether index scanning is currently in progress.
    /// </summary>
    public bool IsScanning
    {
        get
        {
            if (_disposed || _handle == null || _handle.IsInvalid || _handle.IsClosed) return false;
            return FffNative.FffIsScanning(_handle) != 0;
        }
    }

    /// <summary>
    /// Gets the background indexing progress and file watcher readiness details.
    /// </summary>
    public FffScanProgress ScanProgress
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            IntPtr resultPtr = FffNative.FffGetScanProgress(_handle);
            if (resultPtr == IntPtr.Zero)
            {
                throw new FffNativeException("Failed to get scan progress: Native function returned null result pointer.");
            }
            try
            {
                var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
                if (!result.Success)
                {
                    string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                    throw new FffNativeException($"Get scan progress failed: {errorMsg}");
                }
                IntPtr progressPtr = result.Handle;
                if (progressPtr == IntPtr.Zero)
                {
                    throw new FffNativeException("Scan progress handle is null.");
                }
                try
                {
                    var nativeProgress = Marshal.PtrToStructure<FffNative.FffNativeScanProgress>(progressPtr);
                    return new FffScanProgress
                    {
                        ScannedFilesCount = nativeProgress.ScannedFilesCount,
                        IsScanning = nativeProgress.IsScanning != 0,
                        IsWatcherReady = nativeProgress.IsWatcherReady != 0,
                        IsWarmupComplete = nativeProgress.IsWarmupComplete != 0
                    };
                }
                finally
                {
                    FffNative.FffFreeScanProgress(progressPtr);
                }
            }
            finally
            {
                FffNative.FffFreeResult(resultPtr);
            }
        }
    }

    /// <summary>
    /// Create a new file finder instance from an <see cref="FffCreateOptions"/> struct.
    /// </summary>
    /// <param name="options">The initialization options.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <see cref="FffCreateOptions.BasePath"/> is null, empty, or whitespace.</exception>
    /// <exception cref="FffNativeException">
    /// Thrown if the native library fails to create the instance or returns a null result pointer.
    /// </exception>
    public FffFinder(FffCreateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.BasePath);

        _weakGcHandle = GCHandle.Alloc(this, GCHandleType.Weak);

        var nativeOpts = options.ToNative(out var allocatedStrings);
        try
        {
            IntPtr resultPtr = FffNative.FffCreateInstanceWith(in nativeOpts);
            if (resultPtr == IntPtr.Zero)
            {
                throw new FffNativeException("Failed to initialize FffFinder: Native function returned null result pointer.");
            }

            try
            {
                var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
                if (!result.Success)
                {
                    string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                    throw new FffNativeException($"Failed to initialize FffFinder: {errorMsg}");
                }

                _handle = new SafeFffHandle(result.Handle, ownsHandle: true);
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

    /// <summary>
    /// Trigger a rescan of the file index.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="FffNativeException">Thrown if the native scan operation fails.</exception>
    public void ScanFiles()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IntPtr resultPtr = FffNative.FffScanFiles(_handle);
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to scan files: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Scan files failed: {errorMsg}");
            }
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Restart indexing in a new directory.
    /// </summary>
    /// <param name="newPath">The new directory path.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="newPath"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="newPath"/> is empty or whitespace.</exception>
    /// <exception cref="FffNativeException">Thrown if reindexing fails.</exception>
    public void Reindex(string newPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(newPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(newPath);

        IntPtr resultPtr = FffNative.FffRestartIndex(_handle, newPath);
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to restart index: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Reindexing failed: {errorMsg}");
            }
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Refresh git status cache.
    /// </summary>
    /// <returns>The number of files updated.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="FffNativeException">Thrown if refreshing Git status fails.</exception>
    public int RefreshGitStatus()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IntPtr resultPtr = FffNative.FffRefreshGitStatus(_handle);
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to refresh Git status: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Refresh Git status failed: {errorMsg}");
            }
            return (int)result.IntValue;
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Track query completion for smart suggestions.
    /// </summary>
    /// <param name="query">The search query typed by the user.</param>
    /// <param name="filePath">The path of the selected file.</param>
    /// <returns><c>true</c> if the selection was successfully tracked; otherwise, <c>false</c>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="FffNativeException">Thrown if query tracking fails.</exception>
    public bool TrackQuery(string query, string filePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(filePath);

        IntPtr resultPtr = FffNative.FffTrackQuery(_handle, query, filePath);
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to track query: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Query tracking failed: {errorMsg}");
            }
            return result.IntValue == 1;
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Get historical query by offset (0 = most recent).
    /// </summary>
    /// <param name="offset">History index offset.</param>
    /// <returns>The query string, or <c>null</c> if no query exists at the specified offset.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="FffNativeException">Thrown if getting historical query fails.</exception>
    public string? GetHistoricalQuery(ulong offset)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IntPtr resultPtr = FffNative.FffGetHistoricalQuery(_handle, offset);
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to get historical query: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Get historical query failed: {errorMsg}");
            }
            string? query = Marshal.PtrToStringUTF8(result.Handle);
            if (result.Handle != IntPtr.Zero)
            {
                FffNative.FffFreeString(result.Handle);
            }
            return query;
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Wait for initial scan to complete.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns><c>true</c> if the scan completed; <c>false</c> if the timeout expired.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="FffNativeException">Thrown if the native wait fails.</exception>
    public bool WaitForScan(TimeSpan timeout)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ulong timeoutMs = (ulong)timeout.TotalMilliseconds;
        IntPtr resultPtr = FffNative.FffWaitForScan(_handle, timeoutMs);
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to wait for scan: Native function returned null result pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Wait for scan failed: {errorMsg}");
            }

            return result.IntValue == 1;
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Asynchronously wait for initial index scanning to complete without blocking the calling thread.
    /// Offloads native blocking condition wait to the thread pool for 0ms latency wakeup upon completion.
    /// </summary>
    /// <param name="timeout">The maximum duration to wait.</param>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    /// <returns><c>true</c> if the scan completed; <c>false</c> if the timeout expired or cancelled.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public async Task<bool> WaitForScanAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await Task.Run(() => WaitForScan(timeout), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously wait for initial index scanning to complete indefinitely until finished or cancelled.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    /// <returns><c>true</c> if the scan completed; <c>false</c> if cancelled.</returns>
    public Task<bool> WaitForScanAsync(CancellationToken cancellationToken) =>
        WaitForScanAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <summary>
    /// Synchronously wait for the background file system watcher to finish initializing (`notify` thread warmup).
    /// </summary>
    /// <param name="timeout">The maximum duration to wait, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.</param>
    /// <returns><c>true</c> if the watcher became ready; <c>false</c> if the timeout expired.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public bool WaitForWatcherReady(TimeSpan timeout)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var start = DateTime.UtcNow;
        while (!ScanProgress.IsWatcherReady)
        {
            if (timeout != Timeout.InfiniteTimeSpan && DateTime.UtcNow - start > timeout)
            {
                return false;
            }
            Thread.Sleep(10);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously wait for the background file system watcher to finish initializing using modern PeriodicTimer.
    /// </summary>
    /// <param name="timeout">The maximum duration to wait, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.</param>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    /// <returns><c>true</c> if the watcher became ready; <c>false</c> if the timeout expired or cancelled.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public async Task<bool> WaitForWatcherReadyAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (ScanProgress.IsWatcherReady) return true;

        var start = DateTime.UtcNow;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));

        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            if (ScanProgress.IsWatcherReady) return true;

            if (timeout != Timeout.InfiniteTimeSpan && DateTime.UtcNow - start > timeout)
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Asynchronously wait for the background file system watcher to finish initializing indefinitely until ready or cancelled.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    /// <returns><c>true</c> if the watcher became ready; <c>false</c> if cancelled.</returns>
    public Task<bool> WaitForWatcherReadyAsync(CancellationToken cancellationToken) =>
        WaitForWatcherReadyAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <summary>
    /// Perform fuzzy search on indexed files.
    /// </summary>
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

    /// <summary>
    /// Glob-only search: filter indexed files by a single glob pattern.
    /// </summary>
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

    private static FffSearchResult ParseSearchResult(IntPtr resultPtr)
    {
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to get search result: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Search failed: {errorMsg}");
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
            using var safeSearchResultHandle = new SafeFffSearchResultHandle(searchResultPtr, ownsHandle: true);
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
                bool isBinary = FffNative.FffFileItemGetIsBinary(itemPtr);
                items.Add(new FffFileItem
                {
                    RelativePath = relativePath ?? throw new FffNativeException("File item has a null relative path."),
                    FileName = fileName ?? throw new FffNativeException("File item has a null filename."),
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
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Perform fuzzy search on indexed directories.
    /// </summary>
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
            throw new FffNativeException("Failed to search directories: Native function returned null result pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Search directories failed: {errorMsg}");
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

            using var safeDirHandle = new SafeFffDirSearchResultHandle(searchResultPtr, ownsHandle: true);
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
                    RelativePath = Marshal.PtrToStringUTF8(nativeItem.RelativePath) ?? throw new FffNativeException("Directory item has a null relative path."),
                    DirName = Marshal.PtrToStringUTF8(nativeItem.DirName) ?? throw new FffNativeException("Directory item has a null name."),
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
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Perform a mixed fuzzy search across both files and directories.
    /// </summary>
    public FffMixedSearchResult SearchMixed(
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

        IntPtr resultPtr = FffNative.FffSearchMixed(
            _handle,
            query,
            currentFile,
            maxThreads,
            pageIndex,
            pageSize,
            comboBoostMultiplier,
            minComboCount);

        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to mixed search: Native function returned null result pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Mixed search failed: {errorMsg}");
            }

            IntPtr searchResultPtr = result.Handle;
            if (searchResultPtr == IntPtr.Zero)
            {
                return new FffMixedSearchResult
                {
                    Items = [],
                    TotalMatched = 0,
                    TotalFiles = 0,
                    TotalDirs = 0
                };
            }

            using var safeMixedHandle = new SafeFffMixedSearchResultHandle(searchResultPtr, ownsHandle: true);
            var nativeResult = Marshal.PtrToStructure<FffNative.FffNativeMixedSearchResult>(searchResultPtr);
            uint count = nativeResult.Count;
            uint totalMatched = nativeResult.TotalMatched;
            uint totalFiles = nativeResult.TotalFiles;
            uint totalDirs = nativeResult.TotalDirs;

            var items = new List<FffMixedItem>((int)count);
            for (uint i = 0; i < count; i++)
            {
                IntPtr itemPtr = FffNative.FffMixedSearchResultGetItem(searchResultPtr, i);
                if (itemPtr == IntPtr.Zero) continue;

                var nativeItem = Marshal.PtrToStructure<FffNative.FffNativeMixedItem>(itemPtr);
                items.Add(new FffMixedItem
                {
                    ItemType = nativeItem.ItemType,
                    RelativePath = Marshal.PtrToStringUTF8(nativeItem.RelativePath) ?? throw new FffNativeException("Mixed item has a null relative path."),
                    DisplayName = Marshal.PtrToStringUTF8(nativeItem.DisplayName) ?? throw new FffNativeException("Mixed item has a null display name."),
                    GitStatus = Marshal.PtrToStringUTF8(nativeItem.GitStatus),
                    Size = nativeItem.Size,
                    Modified = DateTimeOffset.FromUnixTimeSeconds((long)nativeItem.Modified),
                    AccessFrecencyScore = nativeItem.AccessFrecencyScore,
                    ModificationFrecencyScore = nativeItem.ModificationFrecencyScore,
                    TotalFrecencyScore = nativeItem.TotalFrecencyScore,
                    IsBinary = nativeItem.IsBinary != 0
                });
            }

            return new FffMixedSearchResult
            {
                Items = items,
                TotalMatched = totalMatched,
                TotalFiles = totalFiles,
                TotalDirs = totalDirs
            };
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Perform content search (grep) across indexed files.
    /// </summary>
    public FffGrepResult Grep(string query, FffGrepOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);

        var opts = options ?? new FffGrepOptions();

        IntPtr resultPtr = FffNative.FffLiveGrep(
            _handle,
            query,
            (byte)opts.Mode,
            opts.MaxFileSize,
            opts.MaxMatchesPerFile,
            opts.SmartCase,
            opts.FileOffset,
            opts.PageLimit,
            opts.TimeBudgetMs,
            opts.BeforeContext,
            opts.AfterContext,
            opts.ClassifyDefinitions);

        return ParseGrepResult(resultPtr);
    }

    private static FffGrepResult ParseGrepResult(IntPtr resultPtr)
    {
        if (resultPtr == IntPtr.Zero)
        {
            throw new FffNativeException("Failed to get grep result: Native function returned null result pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Grep failed: {errorMsg}");
            }

            IntPtr grepResultPtr = result.Handle;
            if (grepResultPtr == IntPtr.Zero)
            {
                return new FffGrepResult
                {
                    Items = [],
                    TotalMatched = 0,
                    TotalFilesSearched = 0,
                    TotalFiles = 0,
                    FilteredFileCount = 0,
                    NextFileOffset = 0
                };
            }

            using var safeGrepHandle = new SafeFffGrepResultHandle(grepResultPtr, ownsHandle: true);
            uint count = FffNative.FffGrepResultGetCount(grepResultPtr);
            uint totalMatched = FffNative.FffGrepResultGetTotalMatched(grepResultPtr);
            uint totalFilesSearched = FffNative.FffGrepResultGetTotalFilesSearched(grepResultPtr);
            uint totalFiles = FffNative.FffGrepResultGetTotalFiles(grepResultPtr);
            uint filteredFileCount = FffNative.FffGrepResultGetFilteredFileCount(grepResultPtr);
            uint nextFileOffset = FffNative.FffGrepResultGetNextFileOffset(grepResultPtr);
            string? fallbackError = Marshal.PtrToStringUTF8(FffNative.FffGrepResultGetRegexFallbackError(grepResultPtr));

            var items = new List<FffGrepMatch>((int)count);
            for (uint i = 0; i < count; i++)
            {
                IntPtr matchPtr = FffNative.FffGrepResultGetMatch(grepResultPtr, i);
                if (matchPtr == IntPtr.Zero) continue;

                string? relativePath = Marshal.PtrToStringUTF8(FffNative.FffGrepMatchGetRelativePath(matchPtr));
                string? fileName = Marshal.PtrToStringUTF8(FffNative.FffGrepMatchGetFileName(matchPtr));
                string? gitStatus = Marshal.PtrToStringUTF8(FffNative.FffGrepMatchGetGitStatus(matchPtr));
                string? lineContent = Marshal.PtrToStringUTF8(FffNative.FffGrepMatchGetLineContent(matchPtr));
                ulong lineNumber = FffNative.FffGrepMatchGetLineNumber(matchPtr);
                uint col = FffNative.FffGrepMatchGetCol(matchPtr);
                ulong byteOffset = FffNative.FffGrepMatchGetByteOffset(matchPtr);
                ulong size = FffNative.FffGrepMatchGetSize(matchPtr);
                long totalScore = FffNative.FffGrepMatchGetTotalFrecencyScore(matchPtr);
                long accessScore = FffNative.FffGrepMatchGetAccessFrecencyScore(matchPtr);
                long modScore = FffNative.FffGrepMatchGetModificationFrecencyScore(matchPtr);
                ulong modifiedSeconds = FffNative.FffGrepMatchGetModified(matchPtr);
                ushort fuzzyScore = FffNative.FffGrepMatchGetFuzzyScore(matchPtr);
                bool hasFuzzyScore = FffNative.FffGrepMatchGetHasFuzzyScore(matchPtr);
                bool isDefinition = FffNative.FffGrepMatchGetIsDefinition(matchPtr);
                bool isBinary = FffNative.FffGrepMatchGetIsBinary(matchPtr);

                uint rangesCount = FffNative.FffGrepMatchGetMatchRangesCount(matchPtr);
                var matchRanges = new List<FffMatchRange>((int)rangesCount);
                for (uint r = 0; r < rangesCount; r++)
                {
                    IntPtr rangePtr = FffNative.FffGrepMatchGetMatchRange(matchPtr, r);
                    if (rangePtr != IntPtr.Zero)
                    {
                        matchRanges.Add(Marshal.PtrToStructure<FffMatchRange>(rangePtr));
                    }
                }

                uint beforeCount = FffNative.FffGrepMatchGetContextBeforeCount(matchPtr);
                var contextBefore = new List<string>((int)beforeCount);
                for (uint c = 0; c < beforeCount; c++)
                {
                    string? line = Marshal.PtrToStringUTF8(FffNative.FffGrepMatchGetContextBefore(matchPtr, c));
                    if (line != null) contextBefore.Add(line);
                }

                uint afterCount = FffNative.FffGrepMatchGetContextAfterCount(matchPtr);
                var contextAfter = new List<string>((int)afterCount);
                for (uint c = 0; c < afterCount; c++)
                {
                    string? line = Marshal.PtrToStringUTF8(FffNative.FffGrepMatchGetContextAfter(matchPtr, c));
                    if (line != null) contextAfter.Add(line);
                }

                items.Add(new FffGrepMatch
                {
                    RelativePath = relativePath ?? throw new FffNativeException("Native library returned a null file path."),
                    FileName = fileName ?? throw new FffNativeException("Native library returned a null file name."),
                    GitStatus = string.IsNullOrWhiteSpace(gitStatus) ? null : gitStatus,
                    LineContent = lineContent ?? throw new FffNativeException("Native library returned a null line content."),
                    LineNumber = lineNumber,
                    Col = col,
                    ByteOffset = byteOffset,
                    Size = size,
                    TotalFrecencyScore = totalScore,
                    AccessFrecencyScore = accessScore,
                    ModificationFrecencyScore = modScore,
                    Modified = DateTimeOffset.FromUnixTimeSeconds((long)modifiedSeconds),
                    MatchRanges = matchRanges,
                    ContextBefore = contextBefore,
                    ContextAfter = contextAfter,
                    FuzzyScore = fuzzyScore,
                    HasFuzzyScore = hasFuzzyScore,
                    IsDefinition = isDefinition,
                    IsBinary = isBinary
                });
            }

            return new FffGrepResult
            {
                Items = items,
                TotalMatched = totalMatched,
                TotalFilesSearched = totalFilesSearched,
                TotalFiles = totalFiles,
                FilteredFileCount = filteredFileCount,
                NextFileOffset = nextFileOffset,
                RegexFallbackError = string.IsNullOrWhiteSpace(fallbackError) ? null : fallbackError
            };
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Perform multi-pattern OR search across indexed files.
    /// </summary>
    public FffGrepResult MultiGrep(IReadOnlyList<string> patterns, string? constraints = null, FffGrepOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(patterns);
        if (patterns.Count == 0)
        {
            throw new ArgumentException("Patterns list must contain at least 1 element.", nameof(patterns));
        }

        var opts = options ?? new FffGrepOptions();
        string patternsJoined = string.Join("\n", patterns);

        IntPtr resultPtr = FffNative.FffMultiGrep(
            _handle,
            patternsJoined,
            constraints,
            opts.MaxFileSize,
            opts.MaxMatchesPerFile,
            opts.SmartCase,
            opts.FileOffset,
            opts.PageLimit,
            opts.TimeBudgetMs,
            opts.BeforeContext,
            opts.AfterContext,
            opts.ClassifyDefinitions);

        return ParseGrepResult(resultPtr);
    }

    /// <summary>
    /// ReadOnlySpan overload for multi-pattern OR search.
    /// </summary>
    public FffGrepResult MultiGrep(ReadOnlySpan<string> patterns, string? constraints = null, FffGrepOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (patterns.IsEmpty)
        {
            throw new ArgumentException("Patterns list must contain at least 1 element.", nameof(patterns));
        }

        string patternsJoined = string.Join("\n", patterns.ToArray());

        var opts = options ?? new FffGrepOptions();
        IntPtr resultPtr = FffNative.FffMultiGrep(
            _handle,
            patternsJoined,
            constraints,
            opts.MaxFileSize,
            opts.MaxMatchesPerFile,
            opts.SmartCase,
            opts.FileOffset,
            opts.PageLimit,
            opts.TimeBudgetMs,
            opts.BeforeContext,
            opts.AfterContext,
            opts.ClassifyDefinitions);

        return ParseGrepResult(resultPtr);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void NativeWatchCallback(ulong watchId, IntPtr batchPtr, IntPtr userData)
    {
        if (batchPtr == IntPtr.Zero || userData == IntPtr.Zero) return;

        try
        {
            var gcHandle = GCHandle.FromIntPtr(userData);
            if (!gcHandle.IsAllocated || gcHandle.Target is not FffFinder finder || finder._disposed) return;

            finder.HandleNativeWatchCallback(watchId, batchPtr);
        }
        catch
        {
            // UnmanagedCallersOnly methods must NEVER let an exception escape into unmanaged callers!
        }
    }

    private void HandleNativeWatchCallback(ulong watchId, IntPtr batchPtr)
    {
        uint count = FffNative.FffWatchEventsCount(batchPtr);
        var list = new List<FffWatchEvent>((int)count);

        for (uint i = 0; i < count; i++)
        {
            IntPtr pathPtr = FffNative.FffWatchEventGetPath(batchPtr, i);
            string? path = Marshal.PtrToStringUTF8(pathPtr);
            byte kind = FffNative.FffWatchEventGetKind(batchPtr, i);

            if (path != null)
            {
                list.Add(new FffWatchEvent
                {
                    Path = path,
                    Kind = (FffWatchKind)kind
                });
            }
        }

        if (_watchCallbacks.TryGetValue(watchId, out var tuple))
        {
            try
            {
                tuple.Callback(list);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"FFF.Net watch callback exception (WatchId {watchId}): {ex}");
                try
                {
                    tuple.OnError?.Invoke(ex);
                    OnWatchError?.Invoke(this, new FffWatchErrorEventArgs(watchId, ex));
                }
                catch
                {
                    // Prevent event handler exception from escaping
                }
            }
        }
    }

    private unsafe void EnsureWatchCallbackRegistered()
    {
        if (_isWatchCallbackRegistered) return;

        lock (_lock)
        {
            if (_isWatchCallbackRegistered) return;

            IntPtr userData = GCHandle.ToIntPtr(_weakGcHandle);
            IntPtr resultPtr = FffNative.FffSetWatchCallback(_handle, &NativeWatchCallback, userData);
            if (resultPtr != IntPtr.Zero)
            {
                try
                {
                    var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
                    if (!result.Success)
                    {
                        string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                        throw new FffNativeException($"Failed to set watch callback: {errorMsg}");
                    }
                    _isWatchCallbackRegistered = true;
                }
                finally
                {
                    FffNative.FffFreeResult(resultPtr);
                }
            }
        }
    }

    /// <summary>
    /// Subscribe to live file system changes matching a glob pattern or root prefix.
    /// </summary>
    public FffWatchSubscription Watch(string? pattern, Action<IReadOnlyList<FffWatchEvent>> callback, FffWatchOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(callback);

        EnsureWatchCallbackRegistered();

        IntPtr resultPtr = IntPtr.Zero;
        List<IntPtr> allocatedPointers = [];

        try
        {
            if (options?.Ignore != null && options.Ignore.Count > 0)
            {
                int count = options.Ignore.Count;
                IntPtr ignoreArrayPtr = Marshal.AllocCoTaskMem(IntPtr.Size * count);
                allocatedPointers.Add(ignoreArrayPtr);

                for (int i = 0; i < count; i++)
                {
                    IntPtr strPtr = Marshal.StringToCoTaskMemUTF8(options.Ignore[i]);
                    allocatedPointers.Add(strPtr);
                    Marshal.WriteIntPtr(ignoreArrayPtr, i * IntPtr.Size, strPtr);
                }

                resultPtr = FffNative.FffWatchArgs(_handle, pattern, ignoreArrayPtr, (uint)count);
            }
            else
            {
                var nativeOpts = new FffNative.FffNativeWatchOptions { Version = 1, Ignore = IntPtr.Zero, IgnoreCount = 0 };
                resultPtr = FffNative.FffWatch(_handle, pattern, in nativeOpts);
            }

            if (resultPtr == IntPtr.Zero)
            {
                throw new FffNativeException("Failed to register watch: Native function returned null result pointer.");
            }

            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new FffNativeException($"Watch registration failed: {errorMsg}");
            }

            ulong watchId = (ulong)result.IntValue;
            _watchCallbacks[watchId] = (callback, options?.OnError);

            return new FffWatchSubscription(this, watchId);
        }
        finally
        {
            if (resultPtr != IntPtr.Zero)
            {
                FffNative.FffFreeResult(resultPtr);
            }
            foreach (var ptr in allocatedPointers)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }
    }

    /// <summary>
    /// Unsubscribe an active watch handler by its subscription ID.
    /// </summary>
    public bool Unwatch(ulong watchId)
    {
        if (_disposed || _handle == null || _handle.IsInvalid || _handle.IsClosed || watchId == 0) return false;

        _watchCallbacks.TryRemove(watchId, out _);

        IntPtr resultPtr = FffNative.FffUnwatch(_handle, watchId);
        if (resultPtr == IntPtr.Zero) return false;

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            return result.Success && result.IntValue == 1;
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }

    /// <summary>
    /// Asynchronously stream live file system changes matching a glob pattern or root prefix using Channels.
    /// </summary>
    public async IAsyncEnumerable<IReadOnlyList<FffWatchEvent>> WatchAsync(
        string? pattern,
        FffWatchOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!ScanProgress.IsWatcherReady)
        {
            await WaitForWatcherReadyAsync(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
        }

        Channel<IReadOnlyList<FffWatchEvent>> channel = options?.ChannelOptions switch
        {
            BoundedChannelOptions boundedOpts => Channel.CreateBounded<IReadOnlyList<FffWatchEvent>>(boundedOpts),
            UnboundedChannelOptions unboundedOpts => Channel.CreateUnbounded<IReadOnlyList<FffWatchEvent>>(unboundedOpts),
            _ => Channel.CreateBounded<IReadOnlyList<FffWatchEvent>>(new BoundedChannelOptions(1000)
            {
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.DropOldest
            })
        };

        using var subscription = Watch(pattern, batch =>
        {
            channel.Writer.TryWrite(batch);
        }, options);

        using var ctr = cancellationToken.Register(() => channel.Writer.TryComplete());

        await foreach (var batch in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return batch;
        }
    }

    /// <summary>
    /// Disposes native handle resources and clears active watch subscriptions.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _watchCallbacks.Clear();
                        if (_weakGcHandle.IsAllocated)
                        {
                            _weakGcHandle.Free();
                        }
                        if (_handle != null && !_handle.IsInvalid && !_handle.IsClosed)
                        {
                            _handle.Dispose();
                        }
                    }
                    _disposed = true;
                }
            }
        }
    }
}