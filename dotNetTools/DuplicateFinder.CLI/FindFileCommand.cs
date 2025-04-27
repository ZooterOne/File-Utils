using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System.IO;
using System.Threading;

/// <summary>
/// Defines a command to find duplicates of a file within a directory.
/// </summary>
internal sealed class FindFileCommand : Command<FindFileCommandSettings>
{
    /// <inheritdoc/>
    public override ValidationResult Validate(CommandContext context, FindFileCommandSettings settings)
    {
        if (!File.Exists(settings.File))
        {
            return ValidationResult.Error($"File {settings.File} does not exist.");
        }

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
    public override int Execute(CommandContext context, FindFileCommandSettings settings)
    {
        var logger = CommandHelpers.CreateLogger<FindFileCommand>();

        if (string.IsNullOrEmpty(settings.File))
        {
            logger.Log(LogLevel.Critical, "Filename is empty.");
            return -1;
        }
        
        var file = new FileInfo(settings.File);
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

        if (!fileCache.FindDuplicates(file, out var duplicates))
        {
            AnsiConsole.MarkupLine("[yellow]No duplicate files found.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine("[steelblue]Duplicate files found.[/]");

        var table = CommandHelpers.GenerateFilesTable(duplicates, directory);
        AnsiConsole.Write(table);

        return 0;
    }
}