using System.ComponentModel;
using Spectre.Console.Cli;

/// <summary>
/// Defines settings for a command to find all duplicate files within a directory.
/// </summary>
internal sealed class FindDuplicatesCommandSettings : CommandSettings
{
    [CommandArgument(0, "[directory]")]
    [Description("The directory to search for duplicate files.")]
    public string? Directory { get; init; }
    
    [CommandOption("-c|--cache")]
    [Description("The cache to use.")]
    public string? Cache { get; init; }
}