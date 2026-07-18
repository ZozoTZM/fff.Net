using System;
using System.Collections.Generic;
using System.Text;

namespace FFF.Net.Models;

/// <summary>
/// Represents an active file system watch subscription registered with <see cref="FffFinder.Watch"/>.
/// Disposing this instance automatically unsubscribes the callback and frees native watch resources.
/// </summary>
public sealed class FffWatchSubscription : IDisposable
{
    private FffFinder? _finder;
    private bool _disposedValue;

    /// <summary>
    /// The unique native watch ID assigned to this subscription.
    /// </summary>
    public ulong WatchId { get; }

    internal FffWatchSubscription(FffFinder finder, ulong watchId)
    {
        _finder = finder;
        WatchId = watchId;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_finder != null && WatchId != 0)
                {
                    _finder.Unwatch(WatchId);
                    _finder = null;
                }
            }
            _disposedValue = true;
        }
    }

    /// <summary>
    /// Unsubscribes the live file watcher and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
