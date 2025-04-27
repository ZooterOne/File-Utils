using System.Collections.Generic;
using System.Globalization;
using Spectre.Console;
using Spectre.Console.Cli;
using System.IO;
using CsvHelper;

/// <summary>
/// Defines a command to report duplicates on existing cache files.
/// </summary>
internal sealed class ReportCommand : Command<ReportCommandSettings>
{
    /// <inheritdoc/>
    public override ValidationResult Validate(CommandContext context, ReportCommandSettings settings)
    {
        if (settings.Caches.Length == 0)
        {
            return ValidationResult.Error($"Provide at least one cache file to load.");
        }

        foreach (var filename in settings.Caches)
        {
            if (!File.Exists(filename))
            {
                return ValidationResult.Error($"File {filename} does not exist.");
            }
        }

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public override int Execute(CommandContext context, ReportCommandSettings settings)
    {
        var logger = CommandHelpers.CreateLogger<ReportCommand>();
        
        var fileCache = new DuplicateFinder.FileCache(logger);
        foreach (var filename in settings.Caches)
        {
            CommandHelpers.LoadDirectoryCache(fileCache, filename, false, true);
        }
        
        AnsiConsole.MarkupLine($"[steelblue]{fileCache.Count()} files parsed.[/]");

        var referenceDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        AnsiConsole.MarkupLine($"[silver]Reference folder: [grey]{referenceDirectory.FullName}[/].[/]");
        
        if (settings.Distinct)
        {
            var distincts = fileCache.FindAllDistincts();
            ReportDisctints(distincts, referenceDirectory, settings.OutputFile);
        }
        else
        {
            var duplicates = fileCache.FindAllDuplicates();
            ReportDuplicates(duplicates, referenceDirectory, settings.OutputFile);
        }

        return 0;
    }

    /// <summary>
    /// Report the duplicate files.
    /// </summary>
    /// <param name="duplicates">The collection of duplicate files to display.</param>
    /// <param name="directory">The reference directory.</param>
    /// <param name="outputFile">The optional csv output file to generate.</param>
    private static void ReportDuplicates(IReadOnlyCollection<IReadOnlyCollection<string>> duplicates, 
        DirectoryInfo directory, string? outputFile)
    {
        if (duplicates.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No duplicate files found.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[steelblue]{duplicates.Count} files found with at least one duplicate.[/]");

        var table = CommandHelpers.GenerateDuplicatesTable(duplicates, directory);
        AnsiConsole.Write(table);

        if (outputFile != null)
        {
            AnsiConsole.Progress()
                .AutoRefresh(false)
                .AutoClear(true)
                .HideCompleted(false)
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn()
                })
                .Start(ctx =>
                {
                    var task = ctx.AddTask("[steelblue]Writing csv file[/]");
                    using var writer = new StreamWriter(outputFile);
                    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csv.WriteField("Folder");
                    csv.WriteField("File");
                    csv.NextRecord();

                    var progressIncrement = 100 / duplicates.Count;
                    foreach (var duplicate in duplicates)
                    {
                        var fileIncrement = progressIncrement / duplicate.Count;
                        csv.NextRecord();
                        foreach (var file in duplicate)
                        {
                            var folder = Path.GetDirectoryName(file) ?? string.Empty;
                            csv.WriteField(folder);
                            var filename = Path.GetFileName(file);
                            csv.WriteField(filename);
                            csv.NextRecord();
                            task.Increment(fileIncrement);
                            ctx.Refresh();
                        }
                    }

                    task.StopTask();
                    ctx.Refresh();
                });

            AnsiConsole.MarkupLine($"[steelblue]Report saved to {outputFile}.[/]");
        }
    }

    /// <summary>
    /// Report the distinct files.
    /// </summary>
    /// <param name="distincts">The collection of distinct files to display.</param>
    /// <param name="directory">The reference directory.</param>
    /// <param name="outputFile">The optional csv output file to generate.</param>
    private static void ReportDisctints(IReadOnlyCollection<string> distincts, 
        DirectoryInfo directory, string? outputFile)
    {
        if (distincts.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No distinct files found.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[steelblue]{distincts.Count} distinct files found.[/]");

        var table = CommandHelpers.GenerateFilesTable(distincts, directory);
        AnsiConsole.Write(table);

        if (outputFile != null)
        {
            AnsiConsole.Progress()
                .AutoRefresh(false)
                .AutoClear(true)
                .HideCompleted(false)
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn()
                })
                .Start(ctx =>
                {
                    var task = ctx.AddTask("[steelblue]Writing csv file[/]");
                    using var writer = new StreamWriter(outputFile);
                    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csv.WriteField("Folder");
                    csv.WriteField("File");
                    csv.NextRecord();

                    var progressIncrement = 100 / distincts.Count;
                    foreach (var file in distincts)
                    {
                        var folder = Path.GetDirectoryName(file) ?? string.Empty;
                        csv.WriteField(folder);
                        var filename = Path.GetFileName(file);
                        csv.WriteField(filename);
                        csv.NextRecord();
                        task.Increment(progressIncrement);
                        ctx.Refresh();
                    }

                    task.StopTask();
                    ctx.Refresh();
                });

            AnsiConsole.MarkupLine($"[steelblue]Report saved to {outputFile}.[/]");
        }
    }
}