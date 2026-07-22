using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace FFF.Net.Models;

/// <summary>
/// Options for configuring a live file system subscription (<see cref="FffFinder.Watch"/> or <see cref="FffFinder.WatchAsync"/>).
/// </summary>
public sealed class FffWatchOptions
{
    /// <summary>
    /// A list of relative paths, directory names, or glob patterns to exclude from watch event notifications (e.g. <c>"bin"</c>, <c>"obj"</c>).
    /// </summary>
    public IReadOnlyList<string>? Ignore { get; set; }

    /// <summary>
    /// Optional custom <see cref="System.Threading.Channels.ChannelOptions"/> (e.g. <see cref="BoundedChannelOptions"/> or <see cref="UnboundedChannelOptions"/>) 
    /// for controlling backpressure when using <see cref="FffFinder.WatchAsync"/>.
    /// </summary>
    public ChannelOptions? ChannelOptions { get; set; }

    /// <summary>
    /// Optional per-subscription error handler invoked if an unhandled exception occurs inside the watch callback.
    /// </summary>
    public Action<Exception>? OnError { get; set; }
}
