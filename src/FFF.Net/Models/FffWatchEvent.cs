using System;
using System.Collections.Generic;
using System.Text;

namespace FFF.Net.Models;

/// <summary>
/// Represents a single file system change event captured by the background file watcher.
/// </summary>
public sealed class FffWatchEvent
{
    /// <summary>
    /// The relative or absolute file path associated with the change event.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// The nature of the change (e.g. <see cref="FffWatchKind.Created"/>, <see cref="FffWatchKind.Modified"/>).
    /// </summary>
    public FffWatchKind Kind { get; init; }
}
