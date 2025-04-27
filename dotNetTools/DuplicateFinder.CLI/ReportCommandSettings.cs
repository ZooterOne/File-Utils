using System.ComponentModel;
using Spectre.Console.Cli;

/// <summary>
/// Defines settings for a command to report duplicates on existing cache files.
/// </summary>
internal sealed class ReportCommandSettings : CommandSettings
{
    [CommandOption("-c|--cache <FILES>")]
    [Description("The cache files to load.")]
    public string[] Caches { get; init; } = [];
    
    [CommandOption("-o|--output <FILE>")]
    [Description("The csv file to save the report to.")]
    public string? OutputFile { get; init; }
    
    [CommandOption("--distinct")]
    [DefaultValue(false)]
    [Description("Report files with no duplicate.")]
    public bool Distinct { get; init; }
}