using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

AnsiConsole.Clear();

var version = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3); 
AnsiConsole.MarkupLine($"[blueviolet bold]Duplicate-Finder v{version}[/]");

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<FindDuplicatesCommand>("duplicates")
        .WithDescription("Find all duplicate files within a directory and its sub-directories.")
        .WithExample("duplicates", "./Documents");
    config.AddCommand<FindFileCommand>("find")
        .WithDescription("Find duplicates of a file within a directory and its sub-directories.")
        .WithExample("find", "photo.jpg", "./Documents");
    config.AddCommand<BuildCacheCommand>("build")
        .WithDescription("Build the cache for a directory.")
        .WithExample("build", "./Documents", "documents.cache");
    config.AddCommand<ReportCommand>("report")
        .WithDescription("Compare the cache for multiple directories and report the duplicates.")
        .WithExample("report", "--cache", "documents.cache");
});
return app.Run(args);
