using System.ComponentModel;
using Spectre.Console.Cli;

/// <summary>
/// Defines settings for a command to find duplicates of a file within a directory.
/// </summary>
internal sealed class FindFileCommandSettings : CommandSettings
{
    [CommandArgument(0, "<file>")]
    [Description("The file to find duplicates for.")]
    public string File { get; init; } = "";

    [CommandArgument(1, "[directory]")]
    [Description("The directory to search.")]
    public string? Directory { get; init; }
    
    [CommandOption("-c|--cache")]
    [Description("The cache to use.")]
    public string? Cache { get; init; }
}