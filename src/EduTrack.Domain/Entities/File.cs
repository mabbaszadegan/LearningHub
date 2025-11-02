using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// File entity - represents a file stored in the system
/// </summary>
public class File
{

    public int Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string MD5Hash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public int ReferenceCount { get; private set; } = 1;

    // Navigation properties

    // Private constructor for EF Core
    private File() { }

    public static File Create(string fileName, string originalFileName, string filePath, 
        string mimeType, long fileSizeBytes, string md5Hash, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName cannot be null or empty", nameof(fileName));
        
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("OriginalFileName cannot be null or empty", nameof(originalFileName));
        
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("FilePath cannot be null or empty", nameof(filePath));
        
        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MimeType cannot be null or empty", nameof(mimeType));
        
        if (fileSizeBytes < 0)
            throw new ArgumentException("FileSizeBytes cannot be negative", nameof(fileSizeBytes));
        
        if (string.IsNullOrWhiteSpace(md5Hash))
            throw new ArgumentException("MD5Hash cannot be null or empty", nameof(md5Hash));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        return new File
        {
            FileName = fileName,
            OriginalFileName = originalFileName,
            FilePath = filePath,
            MimeType = mimeType,
            FileSizeBytes = fileSizeBytes,
            MD5Hash = md5Hash,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
            ReferenceCount = 1
        };
    }

    public void IncrementReferenceCount()
    {
        ReferenceCount++;
    }

    public void DecrementReferenceCount()
    {
        ReferenceCount = Math.Max(0, ReferenceCount - 1);
    }

    public bool IsReferenced => ReferenceCount > 0;

    public string GetFileExtension()
    {
        return Path.GetExtension(OriginalFileName);
    }

    public string GetFileSizeFormatted()
    {
        if (FileSizeBytes < 1024)
            return $"{FileSizeBytes} B";
        
        if (FileSizeBytes < 1024 * 1024)
            return $"{FileSizeBytes / 1024:F1} KB";
        
        if (FileSizeBytes < 1024 * 1024 * 1024)
            return $"{FileSizeBytes / (1024 * 1024):F1} MB";
        
        return $"{FileSizeBytes / (1024 * 1024 * 1024):F1} GB";
    }

    public bool IsImage()
    {
        return MimeType.StartsWith("image/");
    }

    public bool IsVideo()
    {
        return MimeType.StartsWith("video/");
    }

    public bool IsAudio()
    {
        return MimeType.StartsWith("audio/");
    }

    public bool IsPdf()
    {
        return MimeType == "application/pdf";
    }
}
