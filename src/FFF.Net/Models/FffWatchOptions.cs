using System;
using System.Collections.Generic;
using System.Text;

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
}
