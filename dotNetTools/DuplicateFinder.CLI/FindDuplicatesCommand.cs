using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.IO;
using System.Threading;

/// <summary>
/// Defines a command to find all duplicate files within a directory.
/// </summary>
internal sealed class FindDuplicatesCommand : Command<FindDuplicatesCommandSettings>
{
    /// <inheritdoc/>
    public override ValidationResult Validate(CommandContext context, FindDuplicatesCommandSettings settings)
    {
        if (settings.Directory != null && !Directory.Exists(settings.Directory))
        {
            return ValidationResult.Error($"Directory {settings.Directory} does not exist.");
        }
        
        if (settings.Cache != null && !File.Exists(settings.Cache))
        {
            return ValidationResult.Error($"Cache file {settings.Cache} does not exist.");
        }

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public override int Execute(CommandContext context, FindDuplicatesCommandSettings settings)
    {
        var logger = CommandHelpers.CreateLogger<FindDuplicatesCommand>();

        var directory = new DirectoryInfo(settings.Directory ?? Directory.GetCurrentDirectory());
        var fileCache = new DuplicateFinder.FileCache(logger);

        if (settings.Cache != null)
        {
            CommandHelpers.LoadDirectoryCache(fileCache, settings.Cache, true, false);
        }
        else
        {
            CommandHelpers.BuildDirectoryCache(fileCache, directory);
        }
        
        AnsiConsole.MarkupLine($"[steelblue]{fileCache.Count()} files parsed.[/]");

        var duplicates = fileCache.FindAllDuplicates();
        if (duplicates.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No duplicate files found.[/]");
            return 0;
        }
        
        AnsiConsole.MarkupLine($"[silver]Reference folder: [grey]{directory.FullName}[/].[/]");

        var multiSelectionPrompt = CommandHelpers.GenerateDuplicatesSelectionPrompt(duplicates, directory);
        var deleteDuplicates = AnsiConsole.Prompt(multiSelectionPrompt);
        if (deleteDuplicates.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No duplicate file selected.[/]");
        }
        else
        {
            AnsiConsole.Progress()
                .AutoClear(true)
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn())
                .Start(ctx =>
                {
                    var task = ctx.AddTask("[grey]Deleting files[/]", maxValue: deleteDuplicates.Count);
                    foreach (var line in deleteDuplicates)
                    {
                        var file = new FileInfo(line);
                        try
                        {
                            file.Delete();
                        }
                        catch (IOException ex)
                        {
                            logger?.Log(LogLevel.Error, "I/O Exception deleting {FilePath}: {Message}.",
                                file.FullName, ex.Message);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            logger?.Log(LogLevel.Error, "Access Exception deleting {FilePath}: {Message}.",
                                file.FullName, ex.Message);
                        }
                        task.Increment(1);
                    }
                });
        }

        AnsiConsole.MarkupLine($"[steelblue]{duplicates.Count} files found with at least one duplicate.[/]");
        AnsiConsole.MarkupLine($"[steelblue]{deleteDuplicates.Count} duplicate files deleted.[/]");

        return 0;
    }
}