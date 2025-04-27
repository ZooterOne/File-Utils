using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Spectre.Console;

/// <summary>
/// Defines helper functions.
/// </summary>
public static class CommandHelpers 
{
    /// <summary>
    /// Create a logger.
    /// </summary>
    /// <typeparam name="T">The command type to create the logger for.</typeparam>
    /// <returns>The logger.</returns>
    internal static ILogger<T> CreateLogger<T>()
    {
        var loggerFactory = LoggerFactory.Create(
            builder => builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Warning));
        var logger = loggerFactory.CreateLogger<T>();
        return logger;
    }

    /// <summary>
    /// Build the file cache displaying progress feedback.
    /// </summary>
    /// <param name="fileCache">The file cache to fill-in.</param>
    /// <param name="directory">The directory to add to the cache.</param>
    internal static void BuildDirectoryCache(DuplicateFinder.FileCache fileCache, DirectoryInfo directory)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Building directory cache...", _ =>
            {
                fileCache.AddDirectory(directory, CancellationToken.None);
            });
    }

    /// <summary>
    /// Load the file cache displaying progress feedback.
    /// </summary>
    /// <param name="fileCache">The file cache to fill-in.</param>
    /// <param name="cache">The cache file to load.</param>
    /// <param name="verify">True to check if files still exist.</param>
    /// <param name="append">True to append the loaded data to the existing data.
    /// False to clear the data before loading the saved data.</param>
    internal static void LoadDirectoryCache(DuplicateFinder.FileCache fileCache, 
        string cache, bool verify, bool append)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Loading directory cache...", _ =>
            {
                fileCache.Load(cache, verify, append);
            });
    }

    /// <summary>
    /// Generate a Spectre.Console selection prompt from the collection of duplicate files.
    /// </summary>
    /// <param name="duplicates">The collection of duplicate files to display.</param>
    /// <param name="directory">The reference directory.</param>
    /// <returns>The selection prompt to display.</returns>
    internal static MultiSelectionPrompt<string> GenerateDuplicatesSelectionPrompt(IReadOnlyCollection<IReadOnlyCollection<string>> duplicates,
        DirectoryInfo directory)
    {
        const int MultiSelectionPageSize = 15;
        var multiSelectionPrompt = new MultiSelectionPrompt<string>
        {
            Title = "Select duplicate files to [red]delete[/]:",
            Required = false,
            PageSize = MultiSelectionPageSize,
            MoreChoicesText = "[grey]Move up and down to reveal more entries[/]",
            InstructionsText = "[grey]Press [dodgerblue2]<space>[/] to select/deselect an entry, " +
                               "and [dodgerblue2]<enter>[/] to accept.[/]"
        };

        foreach (var duplicate in duplicates)
        {
            multiSelectionPrompt.AddChoiceGroup($"{duplicate.Count} duplicates found.", 
                duplicate.Select(file => Path.GetRelativePath(directory.FullName, file)));
        }

        return multiSelectionPrompt;
    }

    /// <summary>
    /// Generate a Spectre.Console table from the collection of files.
    /// </summary>
    /// <param name="files">The files to display in the table.</param>
    /// <param name="directory">The reference directory.</param>
    /// <returns>The table to display.</returns>
    internal static Table GenerateFilesTable(IReadOnlyCollection<string> files, DirectoryInfo directory)
    {
        var table = new Table()
            .BorderColor(Color.Grey)
            .RoundedBorder();
        table.AddColumn(new TableColumn(
            new TextPath(directory.FullName)
                .Centered()
                .RootColor(Color.CornflowerBlue)
                .SeparatorColor(Color.CornflowerBlue)
                .StemColor(Color.CornflowerBlue)
                .LeafColor(Color.CornflowerBlue)));

        foreach (var line in files)
        {
            var path = Path.GetRelativePath(directory.FullName, line);
            table.AddRow(new TextPath(path).LeftJustified());
        }

        return table;
    }

    /// <summary>
    /// Generate a Spectre.Console table from the collection of duplicate files.
    /// </summary>
    /// <param name="duplicates">The collection of duplicate files to display.</param>
    /// <param name="directory">The reference directory.</param>
    /// <returns>The table to display.</returns>
    internal static Table GenerateDuplicatesTable(IReadOnlyCollection<IReadOnlyCollection<string>> duplicates,
        DirectoryInfo directory)
    {
        var table = new Table()
            .BorderColor(Color.Grey)
            .RoundedBorder();
        table.AddColumn(new TableColumn(
            new TextPath("Folder")
                .LeftJustified()
                .RootColor(Color.CornflowerBlue)
                .SeparatorColor(Color.CornflowerBlue)
                .StemColor(Color.CornflowerBlue)
                .LeafColor(Color.CornflowerBlue)));
        table.AddColumn(new TableColumn(
            new TextPath("File")
                .LeftJustified()
                .RootColor(Color.CornflowerBlue)
                .SeparatorColor(Color.CornflowerBlue)
                .StemColor(Color.CornflowerBlue)
                .LeafColor(Color.CornflowerBlue)));

        foreach (var duplicate in duplicates)
        {
            table.AddEmptyRow();
            foreach (var file in duplicate)
            {
                var folder = Path.GetRelativePath(directory.FullName, Path.GetDirectoryName(file) ?? string.Empty);
                var filename = Path.GetFileName(file);
                table.AddRow(new TextPath(folder).LeftJustified(), new TextPath(filename).LeftJustified());
            }
        }
        
        return table;
    }
}