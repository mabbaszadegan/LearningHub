using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.Files.Commands;
using EduTrack.Application.Features.Files.Queries;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EduTrack.WebApp.Controllers;

[Authorize]
[Route("[controller]")]
public class FileUploadController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IMediator _mediator;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(
        IFileStorageService fileStorageService,
        IMediator mediator,
        ILogger<FileUploadController> logger)
    {
        _fileStorageService = fileStorageService;
        _mediator = mediator;
        _logger = logger;
    }

    // POST: Upload content file for schedule items
    [HttpPost("UploadContentFile")]
    public async Task<IActionResult> UploadContentFile(IFormFile file, string type)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "فایل انتخاب نشده است" });
        }

        try
        {
            // Validate file type
            if (!IsValidContentFileType(file.ContentType, type))
            {
                return Json(new { success = false, message = "نوع فایل نامعتبر است" });
            }

            // Check file size (50MB limit)
            if (file.Length > 50 * 1024 * 1024)
            {
                return Json(new { success = false, message = "حجم فایل بیش از حد مجاز است (50MB)" });
            }

            string finalFilePath = string.Empty;
            string finalMimeType = file.ContentType;
            long finalFileSize = file.Length;

            // Save file with MD5 deduplication (original behavior)
            var stream = file.OpenReadStream();
            finalFilePath = (await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType)).Item1;
            finalMimeType = file.ContentType;
            finalFileSize = file.Length;

            // Calculate MD5 hash for the final file
            string md5Hash;
            using (stream = new FileStream(finalFilePath, FileMode.Open, FileAccess.Read))
            {
                md5Hash = await CalculateMD5Async(stream);
            }

            // Check if file already exists by MD5
            var existingFileResult = await _mediator.Send(new GetFileByMD5Query(md5Hash));

            if (existingFileResult.IsSuccess && existingFileResult.Value != null)
            {
                // File already exists, increment reference count
                var incrementCommand = new IncrementFileReferenceCountCommand(existingFileResult.Value.Id);
                await _mediator.Send(incrementCommand);

                // Delete the newly uploaded file since we're using the existing one
                await _fileStorageService.DeleteFileAsync(finalFilePath);

                return Json(new
                {
                    success = true,
                    message = "فایل با موفقیت ذخیره شد",
                    data = new
                    {
                        id = existingFileResult.Value.Id,
                        fileName = existingFileResult.Value.FileName,
                        originalFileName = existingFileResult.Value.OriginalFileName,
                        url = $"/FileUpload/GetFile/{existingFileResult.Value.Id}",
                        size = existingFileResult.Value.FileSizeBytes,
                        mimeType = existingFileResult.Value.MimeType
                    }
                });
            }
            else
            {
                // Create new file record
                var createFileCommand = new CreateFileCommand(
                    Path.GetFileName(finalFilePath),
                    file.FileName,
                    finalFilePath,
                    finalMimeType,
                    finalFileSize,
                    md5Hash
                );

                var result = await _mediator.Send(createFileCommand);

                if (result.IsSuccess)
                {
                    return Json(new
                    {
                        success = true,
                        message = "فایل با موفقیت آپلود شد",
                        data = new
                        {
                            id = result.Value,
                            fileName = Path.GetFileName(finalFilePath),
                            originalFileName = file.FileName,
                            url = $"/FileUpload/GetFile/{result.Value}",
                            size = finalFileSize,
                            mimeType = finalMimeType
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Error });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading content file: {FileName}", file.FileName);
            return Json(new { success = false, message = "خطا در آپلود فایل" });
        }
    }

    // GET: Get file by ID
    [HttpGet("GetFile/{id}")]
    public async Task<IActionResult> GetFile(int id)
    {
        try
        {
            var fileResponse = await _mediator.Send(new GetFileByIdQuery(id));
            if (!fileResponse.IsSuccess || fileResponse.Value == null)
            {
                return NotFound();
            }

            var file = fileResponse.Value;

            // Check if file exists
            if (!await _fileStorageService.FileExistsAsync(file.FilePath))
            {
                return NotFound("File not found on disk");
            }

            // Get file stream
            var fileStream = await _fileStorageService.GetFileAsync(file.FilePath);
            var fileName = Path.GetFileName(file.FilePath);

            // For audio files, determine MIME type from file extension
            string mimeType = file.MimeType ?? "application/octet-stream";
            if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                mimeType = "audio/mpeg";
            }
            else if (fileName.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase))
            {
                mimeType = "audio/mp4";
            }
            else if (fileName.EndsWith(".webm", StringComparison.OrdinalIgnoreCase))
            {
                mimeType = "audio/webm";
            }
            else if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                mimeType = "audio/wav";
            }

            var fileStreamResult = File(fileStream, mimeType, fileName);
            fileStreamResult.EnableRangeProcessing = true;
            return fileStreamResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file: {FileId}", id);
            return NotFound();
        }
    }

    private static bool IsValidContentFileType(string contentType, string type)
    {
        return type switch
        {
            "image" => contentType.StartsWith("image/"),
            "video" => contentType.StartsWith("video/"),
            "audio" => contentType.StartsWith("audio/"),
            _ => false
        };
    }


    /// <summary>
    /// Get file extension from audio content type
    /// </summary>
    private static string GetAudioFileExtension(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "audio/mpeg" => "mp3",
            "audio/webm" => "webm",
            "audio/wav" => "wav",
            "audio/mp4" => "mp4",
            "audio/ogg" => "ogg",
            _ => "mp3" // default to MP3
        };
    }

    /// <summary>
    /// Calculate MD5 hash of a stream
    /// </summary>
    private static async Task<string> CalculateMD5Async(Stream stream)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hashBytes = await md5.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }



}
