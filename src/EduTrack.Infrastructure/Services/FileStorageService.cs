using EduTrack.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

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

    public async Task<(string FilePath, string MD5Hash, long FileSize)> SaveFileAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate unique filename
            var uniqueFileName = GenerateUniqueFileName(originalFileName);
            var filePath = Path.Combine(_storageRoot, uniqueFileName);
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Calculate MD5 hash while saving
            using var md5 = MD5.Create();
            using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
            using var cryptoStream = new CryptoStream(fileStreamWriter, md5, CryptoStreamMode.Write);
            
            await fileStream.CopyToAsync(cryptoStream, cancellationToken);
            await cryptoStream.FlushFinalBlockAsync(cancellationToken);
            
            var md5Hash = Convert.ToHexString(md5.Hash!).ToLowerInvariant();
            var fileSize = fileStreamWriter.Length;

            _logger.LogInformation("File saved successfully: {FilePath}, MD5: {MD5Hash}, Size: {FileSize}", filePath, md5Hash, fileSize);
            return (filePath, md5Hash, fileSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", originalFileName);
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

            var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
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

    private static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var sanitizedBaseName = SanitizeFileName(Path.GetFileNameWithoutExtension(originalFileName));
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID
        
        return $"{sanitizedBaseName}_{timestamp}_{guid}{extension}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
}
