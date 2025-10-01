using EduTrack.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduTrack.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storageRoot;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _storageRoot = configuration["Paths:StorageRoot"] ?? "wwwroot/storage";
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            var filePath = Path.Combine(_storageRoot, sanitizedFileName);
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);

            _logger.LogInformation("File saved successfully: {FilePath}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            throw;
        }
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
            throw;
        }
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            }
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw;
        }
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
}
