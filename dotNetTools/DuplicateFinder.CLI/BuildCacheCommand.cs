using Spectre.Console;
using Spectre.Console.Cli;
using System.IO;
using System.Threading;

/// <summary>
/// Defines a command to build the cache for a directory.
/// </summary>
internal sealed class BuildCacheCommand : Command<BuildCacheCommandSettings>
{
    /// <inheritdoc/>
    public override ValidationResult Validate(CommandContext context, BuildCacheCommandSettings settings)
    {
        if (settings.Directory != null && !Directory.Exists(settings.Directory))
        {
            return ValidationResult.Error($"Directory {settings.Directory} does not exist.");
        }

        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public override int Execute(CommandContext context, BuildCacheCommandSettings settings)
    {
        var logger = CommandHelpers.CreateLogger<BuildCacheCommand>();

        var directory = new DirectoryInfo(settings.Directory ?? Directory.GetCurrentDirectory());
        var fileCache = new DuplicateFinder.FileCache(logger);

        CommandHelpers.BuildDirectoryCache(fileCache, directory);
        
        AnsiConsole.MarkupLine($"[steelblue]{fileCache.Count()} files added to the cache.[/]");

        var filename = settings.Filename ?? $"{directory.Name}.cache";
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Saving directory cache...", _ =>
            {
                fileCache.Save(filename);
            });
        
        AnsiConsole.MarkupLine($"[steelblue]Cache saved to {filename}.[/]");
        
        return 0;
    }
}