using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DuplicateFinder;

/// <summary>
/// Defines a class to manage cache for files
/// in order to quickly find duplicates.
/// </summary>
public sealed class FileCache
{
    private const string Version = "1.0";
    private ILogger? m_logger;
    private Dictionary<string, HashSet<string>> m_cache;

    /// <summary>
    /// Defines an instance to manage cache.
    /// </summary>
    public FileCache()
    {
        m_cache = [];
    }

    /// <summary>
    /// Defines an instance to manage cache.
    /// </summary>
    /// <param name="logger">The logger used to log debug info or errors.</param>
    public FileCache(ILogger logger)
        : this()
    {
        m_logger = logger;
    }

    /// <summary>
    /// Gets the number of file managed by the cache.
    /// </summary>
    /// <returns>The number of files.</returns>
    public int Count()
    {
        return m_cache.Sum(item => item.Value.Count);
    }

    /// <summary>
    /// Add the content of the given directory and its subdirectories to the cache.
    /// </summary>
    /// <param name="directory">The directory to add to the cache.</param>
    /// <param name="cancellationToken">Token to propagate notification that operations should be canceled.</param>
    /// <exception cref="ArgumentException">Throws if the directory does not exist.</exception>
    public void AddDirectory(DirectoryInfo directory, CancellationToken cancellationToken)
    {
        if (!directory.Exists)
        {
            m_logger?.Log(LogLevel.Error, "Exception adding {Directory} to the cache: Directory does not exist.",
                directory.FullName);
            throw new ArgumentException("Directory does not exist.");
        }
        
        m_logger?.Log(LogLevel.Debug, "Adding {Directory} to the cache.", directory.FullName);
        
        foreach (var file in directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                m_logger?.Log(LogLevel.Debug, "Process cancelled by user.");
                break;
            }
            var fileHash = FileHash.CalculateFileHash(file, m_logger);
            if (fileHash == null)
            {
                continue;
            }
            if (!m_cache.TryGetValue(fileHash, out var matches))
            {
                matches = [];
                m_cache[fileHash] = matches;
            }
            var relativeFilePath = Path.GetRelativePath(directory.FullName, file.FullName);
            m_logger?.Log(LogLevel.Debug, "Adding {File} with key {FileHash}.", relativeFilePath, fileHash);
            matches.Add(file.FullName);
        }
    }

    /// <summary>
    /// Add a file to the cache.
    /// </summary>
    /// <param name="file">The file to add to the cache.</param>
    /// <exception cref="ArgumentException">Throws if the file does not exist.</exception>
    public void AddFile(FileInfo file)
    {
        if (!file.Exists)
        {
            m_logger?.Log(LogLevel.Error, "Exception adding {File} to the cache: File does not exist.",
                file.FullName);
            throw new ArgumentException("File does not exist.");
        }

        m_logger?.Log(LogLevel.Debug, "Adding {File} to the cache.", file.FullName);

        var fileHash = FileHash.CalculateFileHash(file, m_logger);
        if (fileHash == null)
        {
            return;
        }
        if (!m_cache.TryGetValue(fileHash, out var matches))
        {
            matches = [];
            m_cache[fileHash] = matches;
        }
        m_logger?.Log(LogLevel.Debug, "Adding {File} with key {FileHash}.", file.FullName, fileHash);
        matches.Add(file.FullName);
    }

    /// <summary>
    /// Find if a file has duplicates.
    /// </summary>
    /// <param name="file">The file to search duplicates for.</param>
    /// <param name="matches">The collection of all the matches, if any.</param>
    /// <returns>True if at least one duplicate has been found. False otherwise.</returns>
    public bool FindDuplicates(FileInfo file, out IReadOnlyCollection<string> matches)
    {
        var fileHash = FileHash.CalculateFileHash(file, m_logger);
        if (string.IsNullOrEmpty(fileHash) || !m_cache.TryGetValue(fileHash, out var files))
        {
            matches = [];
            return false;
        }
        matches = files;
        return true;
    }

    /// <summary>
    /// Find all the duplicate files. 
    /// </summary>
    /// <returns>The collection of all duplicate files.</returns>
    public IReadOnlyCollection<IReadOnlyCollection<string>> FindAllDuplicates()
    {
        var duplicates = m_cache.Where(item => item.Value.Count >= 2);
        return !duplicates.Any() ? [] : duplicates.Select(item => item.Value).ToArray();
    }

    /// <summary>
    /// Find all distinct files,
    /// i.e. files with no duplicates.
    /// </summary>
    /// <returns>The collection of all unique files.</returns>
    public IReadOnlyCollection<string> FindAllDistincts()
    {
        var distincts = m_cache.Where(item => item.Value.Count == 1);
        return !distincts.Any() ? [] : distincts.Select(item => item.Value.First()).ToArray();
    }

    /// <summary>
    /// Save the content of the cache to a file.
    /// </summary>
    /// <param name="filename">The filepath to save the cache to.</param>
    public void Save(string filename)
    {
        using var stream = new FileStream(filename, FileMode.Create);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        writer.Write(Version);
        writer.Write(m_cache.Count);
        foreach (var entry in m_cache)
        {
            writer.Write(entry.Key);
            writer.Write(entry.Value.Count);
            foreach (var item in entry.Value)
            {
                writer.Write(item);
            }
        }
    }

    /// <summary>
    /// Load a file containing the content of a previously generated cache.
    /// </summary>
    /// <remarks>
    /// The content of the cache will be replaced.
    /// Process can remove files which have been deleted since the creation of <paramref name="filename"/>,
    /// but it will not add newly created files or update data for modified files. 
    /// </remarks>
    /// <param name="filename">The filepath of the cache data to load.</param>
    /// <param name="verify">True to check if files still exist.</param>
    /// <param name="append">True to append the loaded data to the existing data.
    /// False to clear the data before loading the saved data.</param>
    /// <exception cref="FileNotFoundException">Throws when <paramref name="filename"/> is not found.</exception>
    /// <exception cref="InvalidDataException">Throws when file does not contain expected data.</exception>
    public void Load(string filename, bool verify, bool append)
    {
        if (!File.Exists(filename))
        {
            m_logger?.Log(LogLevel.Error, "File {FilePath} not found.", filename);
            throw new FileNotFoundException("File not found.", filename);
        }
        using var stream = new FileStream(filename, FileMode.Open);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);
        var version = reader.ReadString();
        if (version != Version)
        {
            throw new InvalidDataException("File version mismatch.");
        }
        var cacheCount = reader.ReadInt32();
        if (!append)
        {
            m_cache = new Dictionary<string, HashSet<string>>(cacheCount);
        }
        for (int entryIndex = 0; entryIndex < cacheCount; ++entryIndex)
        {
            var fileHash = reader.ReadString();
            var itemCount = reader.ReadInt32();
            if (!m_cache.TryGetValue(fileHash, out var files))
            {
                files = new HashSet<string>(itemCount);
                m_cache[fileHash] = files;
            }
            for (int itemIndex = 0; itemIndex < itemCount; ++itemIndex)
            {
                var item = reader.ReadString();
                if (verify && !File.Exists(item))
                {
                    continue;
                }
                files.Add(item);
            }
        }
    }
}