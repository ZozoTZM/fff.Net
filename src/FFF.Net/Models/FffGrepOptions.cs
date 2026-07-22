namespace FFF.Net.Models;

/// <summary>
/// Options for configuring live content grep queries.
/// </summary>
public sealed class FffGrepOptions
{
    /// <summary>Gets or sets the search mode (Plain, Regex, or Fuzzy).</summary>
    public FffGrepMode Mode { get; set; } = FffGrepMode.Plain;

    /// <summary>Gets or sets the maximum file size in bytes to inspect.</summary>
    public ulong MaxFileSize { get; set; }

    /// <summary>Gets or sets the maximum matches allowed per file.</summary>
    public uint MaxMatchesPerFile { get; set; }

    /// <summary>Gets or sets whether smart-case matching is enabled.</summary>
    public bool SmartCase { get; set; } = true;

    /// <summary>Gets or sets the file index offset for pagination.</summary>
    public uint FileOffset { get; set; }

    /// <summary>Gets or sets the maximum number of matches per page.</summary>
    public uint PageLimit { get; set; } = 50;

    /// <summary>Gets or sets the execution time budget limit in milliseconds.</summary>
    public ulong TimeBudgetMs { get; set; }

    /// <summary>Gets or sets the number of preceding context lines to return.</summary>
    public uint BeforeContext { get; set; }

    /// <summary>Gets or sets the number of following context lines to return.</summary>
    public uint AfterContext { get; set; }

    /// <summary>Gets or sets whether to classify symbol definitions.</summary>
    public bool ClassifyDefinitions { get; set; }
}
