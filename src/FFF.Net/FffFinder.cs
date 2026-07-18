using FFF.Net.Models;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace FFF.Net;

public sealed class FffFinder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private FffNative.FffNativeWatchCallback? _nativeWatchCallback;
    private readonly ConcurrentDictionary<ulong, Action<IReadOnlyList<FffWatchEvent>>> _watchCallbacks = new();
    public string BasePath
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            IntPtr resultPtr = FffNative.FffGetBasePath(_handle);
            if (resultPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to get base path: Native function returned null result pointer.");
            }
            try
            {
                var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
                if (!result.Success)
                {
                    string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                    throw new InvalidOperationException($"Get base path failed: {errorMsg}");
                }
                string? path = Marshal.PtrToStringUTF8(result.Handle);
                if (result.Handle != IntPtr.Zero)
                {
                    FffNative.FffFreeString(result.Handle);
                }
                return path ?? throw new InvalidOperationException("Native library returned a null base path.");
            }
            finally
            {
                FffNative.FffFreeResult(resultPtr);
            }
        }
    }

    public bool IsScanning
    {
        get
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return FffNative.FffIsScanning(_handle) != 0;
        }
    }

    public FffScanProgress ScanProgress
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            IntPtr resultPtr = FffNative.FffGetScanProgress(_handle);
            if (resultPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to get scan progress: Native function returned null result pointer.");
            }
            try
            {
                var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
                if (!result.Success)
                {
                    string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                    throw new InvalidOperationException($"Get scan progress failed: {errorMsg}");
                }
                IntPtr progressPtr = result.Handle;
                if (progressPtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Scan progress handle is null.");
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library fails to create the instance, returns a null result pointer, 
    /// or if the operation success flag is false.
    /// </exception>
    public FffFinder(FffCreateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.BasePath);

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
                if (!result.Success)
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
    /// <summary>
    /// Trigger a rescan of the file index.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer or if the operation success flag is false.
    /// </exception>
    public void ScanFiles()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IntPtr resultPtr = FffNative.FffScanFiles(_handle);
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to scan files: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Scan files failed: {errorMsg}");
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer or if the operation success flag is false.
    /// </exception>
    public void Reindex(string newPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(newPath);

        IntPtr resultPtr = FffNative.FffRestartIndex(_handle, newPath);
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to restart index: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Reindexing failed: {errorMsg}");
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer or if the operation success flag is false.
    /// </exception>
    public int RefreshGitStatus()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IntPtr resultPtr = FffNative.FffRefreshGitStatus(_handle);
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to refresh Git status: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Refresh Git status failed: {errorMsg}");
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer or if the operation success flag is false.
    /// </exception>
    public bool TrackQuery(string query, string filePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(filePath);

        IntPtr resultPtr = FffNative.FffTrackQuery(_handle, query, filePath);
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to track query: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Query tracking failed: {errorMsg}");
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer or if the operation success flag is false.
    /// </exception>
    public string? GetHistoricalQuery(ulong offset)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IntPtr resultPtr = FffNative.FffGetHistoricalQuery(_handle, offset);
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to get historical query: Native function returned null result pointer.");
        }
        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Get historical query failed: {errorMsg}");
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
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer or if the operation success flag is false.
    /// </exception>
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
            if (!result.Success)
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

    /// <summary>
    /// Asynchronously wait for initial index scanning to complete without blocking the calling thread.
    /// </summary>
    /// <param name="timeout">The maximum duration to wait.</param>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    /// <returns><c>true</c> if the scan completed; <c>false</c> if the timeout expired or cancelled.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public async Task<bool> WaitForScanAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var start = DateTime.UtcNow;
        while (ScanProgress.IsScanning)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (timeout != Timeout.InfiniteTimeSpan && DateTime.UtcNow - start > timeout)
            {
                return false;
            }
            await Task.Delay(50, cancellationToken).ConfigureAwait(false);
        }
        return true;
    }

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
            Thread.Sleep(50);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously wait for the background file system watcher to finish initializing (`notify` thread warmup).
    /// </summary>
    /// <param name="timeout">The maximum duration to wait, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.</param>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    /// <returns><c>true</c> if the watcher became ready; <c>false</c> if the timeout expired or cancelled.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public async Task<bool> WaitForWatcherReadyAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var start = DateTime.UtcNow;
        while (!ScanProgress.IsWatcherReady)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (timeout != Timeout.InfiniteTimeSpan && DateTime.UtcNow - start > timeout)
            {
                return false;
            }
            await Task.Delay(50, cancellationToken).ConfigureAwait(false);
        }
        return true;
    }

    /// <summary>
    /// Perform fuzzy search on indexed files.
    /// </summary>
    /// <param name="query">Search query string.</param>
    /// <param name="currentFile">Path of the currently open file for deprioritization (null/empty to skip).</param>
    /// <param name="maxThreads">Maximum worker threads (0 = auto-detect).</param>
    /// <param name="pageIndex">Pagination offset (0 = first page).</param>
    /// <param name="pageSize">Results per page (0 = default 100).</param>
    /// <param name="comboBoostMultiplier">Score multiplier for combo matches (0 = default 100).</param>
    /// <param name="minComboCount">Minimum combo count before boost applies (0 = default 3).</param>
    /// <returns>A <see cref="FffSearchResult"/> containing the matching file items.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer, if the operation success flag is false, 
    /// or if any returned file item contains a null relative path or filename.
    /// </exception>
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
    /// Glob-only search: filter indexed files by a single glob pattern, rank by
    /// frecency, and paginate. Bypasses the regular query parser entirely.
    /// 
    /// Use this when you already have a literal glob pattern (e.g. `*.rs`, a
    /// recursive `**` match, or `src/components` prefix) and want neither fuzzy
    /// matching nor multi-token constraint parsing. Ranking falls back to
    /// frecency because there is no fuzzy score to combine with.
    /// </summary>
    /// <param name="pattern">Glob pattern (required, no parsing - passed through verbatim).</param>
    /// <param name="currentFile">Path of the currently open file for deprioritization (null/empty to skip).</param>
    /// <param name="maxThreads">Maximum worker threads (0 = auto-detect).</param>
    /// <param name="pageIndex">Pagination offset (0 = first page).</param>
    /// <param name="pageSize">Results per page (0 = default 100).</param>
    /// <returns>A <see cref="FffSearchResult"/> containing the matching file items.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pattern"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer, if the operation success flag is false, 
    /// or if any returned file item contains a null relative path or filename.
    /// </exception>
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
            if (!result.Success)
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
                    bool isBinary = FffNative.FffFileItemGetIsBinary(itemPtr);
                    items.Add(new FffFileItem
                    {
                        RelativePath = relativePath ?? throw new InvalidOperationException("File item has a null relative path."),
                        FileName = fileName ?? throw new InvalidOperationException("File item has a null filename."),
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
    /// <summary>
    /// Perform fuzzy search on indexed directories.
    /// </summary>
    /// <param name="query">Search query string.</param>
    /// <param name="currentFile">Path of the currently open file for distance scoring (null/empty to skip).</param>
    /// <param name="maxThreads">Maximum worker threads (0 = auto-detect).</param>
    /// <param name="pageIndex">Pagination offset (0 = first page).</param>
    /// <param name="pageSize">Results per page (0 = default 100).</param>
    /// <returns>A <see cref="FffDirSearchResult"/> containing the matching directory items.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer, if the operation success flag is false, 
    /// or if any returned directory item contains a null relative path or folder name.
    /// </exception>
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
            if (!result.Success)
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
                        RelativePath = Marshal.PtrToStringUTF8(nativeItem.RelativePath) ?? throw new InvalidOperationException("Directory item has a null relative path."),
                        DirName = Marshal.PtrToStringUTF8(nativeItem.DirName) ?? throw new InvalidOperationException("Directory item has a null name."),
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
    /// <summary>
    /// Perform a mixed fuzzy search across both files and directories.
    /// 
    /// Returns a single flat list where files and directories are interleaved
    /// by total score in descending order. Each item has an `item_type` field
    /// (0 = file, 1 = directory).
    /// </summary>
    /// <param name="query">Search query string.</param>
    /// <param name="currentFile">Path of the currently open file (null/empty to skip).</param>
    /// <param name="maxThreads">Maximum worker threads (0 = auto-detect).</param>
    /// <param name="pageIndex">Pagination offset (0 = first page).</param>
    /// <param name="pageSize">Results per page (0 = default 100).</param>
    /// <param name="comboBoostMultiplier">Score multiplier for combo matches (0 = default 100).</param>
    /// <param name="minComboCount">Minimum combo count before boost applies (0 = default 3).</param>
    /// <returns>A <see cref="FffMixedSearchResult"/> containing the matched items.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer, if the operation success flag is false, 
    /// or if any returned mixed item contains a null relative path or display name.
    /// </exception>
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
            throw new InvalidOperationException("Failed to mixed search: Native function returned null result pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Mixed search failed: {errorMsg}");
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

            try
            {

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
                        ItemType = (FffMixedItemType)nativeItem.ItemType,
                        RelativePath = Marshal.PtrToStringUTF8(nativeItem.RelativePath) ?? throw new InvalidOperationException("Mixed item has a null relative path."),
                        DisplayName = Marshal.PtrToStringUTF8(nativeItem.DisplayName) ?? throw new InvalidOperationException("Mixed item has a null display name."),
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
                FffNative.FffFreeMixedSearchResult(searchResultPtr);
            }
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }
    /// <summary>
    /// Perform content search (grep) across indexed files.
    /// </summary>
    /// <param name="query">Search query (supports constraint syntax like `*.rs pattern`).</param>
    /// <param name="options">Optional configurations for file size limits, pagination, and context lines.</param>
    /// <returns>A <see cref="FffGrepResult"/> containing matched lines, highlighting spans, and context lines.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer, if the operation success flag is false, 
    /// or if any returned grep match contains a null relative path, filename, or line content.
    /// </exception>
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

    private FffGrepResult ParseGrepResult(IntPtr resultPtr)
    {
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to get grep result: Native function returned null result pointer.");
        }

        try
        {
            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Grep failed: {errorMsg}");
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

            try
            {

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
                        RelativePath = relativePath ?? throw new InvalidOperationException("Native library returned a null file path."),
                        FileName = fileName ?? throw new InvalidOperationException("Native library returned a null file name."),
                        GitStatus = string.IsNullOrWhiteSpace(gitStatus) ? null : gitStatus,
                        LineContent = lineContent ?? throw new InvalidOperationException("Native library returned a null line content."),
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
                FffNative.FffFreeGrepResult(grepResultPtr);
            }
        }
        finally
        {
            FffNative.FffFreeResult(resultPtr);
        }
    }
    /// <summary>
    /// Perform multi-pattern OR search (Aho-Corasick) across indexed files.
    /// 
    /// Searches for lines matching ANY of the provided patterns using
    /// SIMD-accelerated multi-needle matching.
    /// </summary>
    /// <param name="patterns">The list of search terms to match.</param>
    /// <param name="constraints">File filter like `"*.rs"` or `"/src/"` (null/empty to skip).</param>
    /// <param name="options">Optional configurations for file size limits, pagination, and context lines.</param>
    /// <returns>A <see cref="FffGrepResult"/> containing matched occurrences.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="patterns"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="patterns"/> list is empty.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the native library returns a null result pointer, if the operation success flag is false, 
    /// or if any returned grep match contains a null relative path, filename, or line content.
    /// </exception>
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
    private void EnsureWatchCallbackRegistered()
    {
        if (_nativeWatchCallback != null) return;

        _nativeWatchCallback = (watchId, batchPtr, userData) =>
        {
            if (batchPtr == IntPtr.Zero) return;
            try
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

                if (_watchCallbacks.TryGetValue(watchId, out var action))
                {
                    action(list);
                }
            }
            finally
            {
                FffNative.FffFreeWatchEvents(batchPtr);
            }
        };

        IntPtr resultPtr = FffNative.FffSetWatchCallback(_handle, _nativeWatchCallback, IntPtr.Zero);
        if (resultPtr != IntPtr.Zero)
        {
            try
            {
                var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
                if (!result.Success)
                {
                    string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                    throw new InvalidOperationException($"Failed to set watch callback: {errorMsg}");
                }
            }
            finally
            {
                FffNative.FffFreeResult(resultPtr);
            }
        }
    }
    /// <summary>
    /// Subscribe to live file system changes matching a glob pattern or root prefix.
    /// Requires <see cref="FffCreateOptions.Watch"/> to be set to <c>true</c> when initializing this instance.
    /// </summary>
    /// <param name="pattern">The glob pattern to filter watched files (e.g. <c>*.cs</c> or <c>null</c> for all).</param>
    /// <param name="callback">The callback delegate invoked when a batch of watch events occurs.</param>
    /// <param name="options">Optional watching options, including paths or directories to ignore.</param>
    /// <returns>An <see cref="FffWatchSubscription"/> representing the active subscription. Disposing this token unsubscribes automatically.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if watch registration fails or if file watching was disabled.</exception>
    public FffWatchSubscription Watch(string? pattern, Action<IReadOnlyList<FffWatchEvent>> callback, FffWatchOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(callback);

        EnsureWatchCallbackRegistered();

        IntPtr resultPtr = IntPtr.Zero;
        List<IntPtr> allocatedPointers = new();

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
                throw new InvalidOperationException("Failed to register watch: Native function returned null result pointer.");
            }

            var result = Marshal.PtrToStructure<FffNative.FffResult>(resultPtr);
            if (!result.Success)
            {
                string? errorMsg = Marshal.PtrToStringUTF8(result.Error);
                throw new InvalidOperationException($"Watch registration failed: {errorMsg}");
            }

            ulong watchId = (ulong)result.IntValue;
            _watchCallbacks[watchId] = callback;

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
    /// <param name="watchId">The watch ID returned when subscribing.</param>
    /// <returns><c>true</c> if the subscription was found and unregistered; otherwise, <c>false</c>.</returns>
    public bool Unwatch(ulong watchId)
    {
        if (_disposed || _handle == IntPtr.Zero || watchId == 0) return false;

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
    /// Asynchronously stream live file system changes matching a glob pattern or root prefix using an unbounded channel.
    /// Requires <see cref="FffCreateOptions.Watch"/> to be set to <c>true</c> when initializing this instance.
    /// </summary>
    /// <param name="pattern">The glob pattern to filter watched files (e.g. <c>*.cs</c> or <c>null</c> for all).</param>
    /// <param name="options">Optional watching options, including paths or directories to ignore.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous stream and automatically dispose the watch subscription.</param>
    /// <returns>An asynchronous stream yielding batches of file watch events as they occur.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if watch registration fails or if file watching was disabled.</exception>
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

        var channel = Channel.CreateUnbounded<IReadOnlyList<FffWatchEvent>>(new UnboundedChannelOptions
        {
            SingleWriter = true
        });

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
                _watchCallbacks.Clear();
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