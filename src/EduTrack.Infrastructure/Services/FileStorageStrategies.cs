using EduTrack.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduTrack.Infrastructure.Services;

/// <summary>
/// Factory pattern for creating file storage services
/// </summary>
public interface IFileStorageServiceFactory
{
    IFileStorageService CreateFileStorageService(string storageType);
}

/// <summary>
/// Strategy pattern for different file storage implementations
/// </summary>
public interface IFileStorageStrategy
{
    Task<(string FilePath, string MD5Hash, long FileSize)> SaveFileAsync(
        Stream fileStream, 
        string originalFileName, 
        string contentType, 
        CancellationToken cancellationToken = default);
    
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Local file system storage strategy
/// </summary>
public class LocalFileStorageStrategy : IFileStorageStrategy
{
    private readonly string _storageRoot;
    private readonly ILogger<LocalFileStorageStrategy> _logger;

    public LocalFileStorageStrategy(string storageRoot, ILogger<LocalFileStorageStrategy> logger)
    {
        _storageRoot = storageRoot;
        _logger = logger;
    }

    public async Task<(string FilePath, string MD5Hash, long FileSize)> SaveFileAsync(
        Stream fileStream, 
        string originalFileName, 
        string contentType, 
        CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
        var filePath = Path.Combine(_storageRoot, fileName);
        
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);
        
        var fileSize = new FileInfo(filePath).Length;
        var md5Hash = await CalculateMD5HashAsync(filePath);

        _logger.LogInformation("File saved: {FilePath}, Size: {FileSize} bytes", filePath, fileSize);
        
        return (filePath, md5Hash, fileSize);
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("File deleted: {FilePath}", filePath);
        }
        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    private async Task<string> CalculateMD5HashAsync(string filePath)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = await Task.Run(() => md5.ComputeHash(stream));
        return Convert.ToHexString(hash);
    }
}

/// <summary>
/// Factory implementation for file storage services
/// </summary>
public class FileStorageServiceFactory : IFileStorageServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FileStorageServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileStorageService CreateFileStorageService(string storageType)
    {
        return storageType.ToLowerInvariant() switch
        {
            "local" => _serviceProvider.GetRequiredService<IFileStorageService>(),
            "azure" => throw new NotImplementedException("Azure storage not implemented yet"),
            "aws" => throw new NotImplementedException("AWS storage not implemented yet"),
            _ => throw new ArgumentException($"Unsupported storage type: {storageType}")
        };
    }
}
