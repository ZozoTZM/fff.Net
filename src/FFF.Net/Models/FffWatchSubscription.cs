using System;
using System.Threading;

namespace FFF.Net.Models;

/// <summary>
/// Represents an active file system watch subscription registered with <see cref="FffFinder.Watch"/>.
/// Disposing this instance automatically unsubscribes the callback and frees native watch resources.
/// </summary>
public sealed class FffWatchSubscription : IDisposable
{
    private readonly WeakReference<FffFinder> _finderRef;
    private int _disposedState;

    /// <summary>
    /// The unique native watch ID assigned to this subscription.
    /// </summary>
    public ulong WatchId { get; }

    internal FffWatchSubscription(FffFinder finder, ulong watchId)
    {
        ArgumentNullException.ThrowIfNull(finder);
        _finderRef = new WeakReference<FffFinder>(finder);
        WatchId = watchId;
    }

    private void DisposeCore()
    {
        if (Interlocked.Exchange(ref _disposedState, 1) == 0)
        {
            if (WatchId != 0 && _finderRef.TryGetTarget(out var finder))
            {
                finder.Unwatch(WatchId);
            }
        }
    }

    /// <summary>
    /// Unsubscribes the live file watcher and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to safely clean up native watch subscriptions if neglected by caller code.
    /// </summary>
    ~FffWatchSubscription()
    {
        DisposeCore();
    }
}
