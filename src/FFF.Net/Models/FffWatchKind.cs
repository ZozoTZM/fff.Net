using System;
using System.Collections.Generic;
using System.Text;

namespace FFF.Net.Models;

/// <summary>
/// Specifies the type of file system change event emitted by the live file watcher.
/// </summary>
public enum FffWatchKind : byte
{
    /// <summary>
    /// A new file or directory was created.
    /// </summary>
    Created = 0,

    /// <summary>
    /// An existing file or directory's contents or attributes were modified.
    /// </summary>
    Modified = 1,

    /// <summary>
    /// A file or directory was deleted or moved out of the watched directory.
    /// </summary>
    Removed = 2,

    /// <summary>
    /// A bulk rescan or directory reload occurred due to buffer overflow or structural updates.
    /// </summary>
    Rescan = 3
}
