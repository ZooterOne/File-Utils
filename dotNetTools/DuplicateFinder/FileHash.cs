using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DuplicateFinder;

internal static class FileHash
{
    /// <summary>
    /// Calculate the SHA256 hash of a file.
    /// </summary>
    /// <param name="file">The file to calculate the hash for.</param>
    /// <param name="logger">The logger used to log debug info or errors.</param>
    /// <returns>Calculated SHA56 hash for <paramref name="file"/>.</returns>
    public static string? CalculateFileHash(FileInfo file, ILogger? logger)
    {
        using var sha356Hash = SHA256.Create();
        try
        {
            using var fileStream = file.Open(FileMode.Open);
            fileStream.Position = 0;
            var fileHash = new StringBuilder();
            var hashValues = sha356Hash.ComputeHash(fileStream);
            foreach (var hashValue in hashValues)
            {
                fileHash.Append(hashValue.ToString("x2"));
            }
            return fileHash.ToString();
        }
        catch (IOException ex)
        {
            logger?.Log(LogLevel.Error, "I/O Exception calculating file hash for {FilePath}: {Message}.",
                file.FullName, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger?.Log(LogLevel.Error, "Access Exception calculating file hash for {FilePath}: {Message}.",
                file.FullName, ex.Message);
        }
        return null;
    }
}