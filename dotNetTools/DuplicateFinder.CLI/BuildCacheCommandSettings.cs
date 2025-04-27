using System.ComponentModel;
using Spectre.Console.Cli;

/// <summary>
/// Defines settings for a command to build the cache for a directory.
/// </summary>
internal sealed class BuildCacheCommandSettings : CommandSettings
{
    [CommandArgument(0, "[directory]")]
    [Description("The directory to build the cache for.")]
    public string? Directory { get; init; }
    
    [CommandArgument(0, "[filename]")]
    [Description("The filename to save the content of the cache to.")]
    public string? Filename { get; init; }
}