using System;

namespace FFF.Net.Models;

/// <summary>
/// Exception thrown when a native <c>fff</c> operation returns an error result.
/// </summary>
public class FffNativeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FffNativeException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing the native failure.</param>
    public FffNativeException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FffNativeException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message describing the native failure.</param>
    /// <param name="innerException">The inner exception that is the cause of this exception.</param>
    public FffNativeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Event arguments supplied when an unhandled exception occurs inside a file watcher callback delegate.
/// </summary>
public sealed class FffWatchErrorEventArgs : EventArgs
{
    /// <summary>
    /// The watch subscription ID where the error occurred.
    /// </summary>
    public ulong WatchId { get; }

    /// <summary>
    /// The unhandled exception thrown inside the watch callback delegate.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FffWatchErrorEventArgs"/> class.
    /// </summary>
    /// <param name="watchId">The native watch subscription ID.</param>
    /// <param name="exception">The caught exception.</param>
    public FffWatchErrorEventArgs(ulong watchId, Exception exception)
    {
        WatchId = watchId;
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}
