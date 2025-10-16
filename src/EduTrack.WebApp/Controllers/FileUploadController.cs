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

    // POST: Upload course resource
    [HttpPost]
    public async Task<IActionResult> UploadResource(int lessonId, IFormFile file, string title, string description, ResourceType type)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded" });
        }

        try
        {
            // Validate file type and size
            if (!IsValidFileType(file.ContentType, type))
            {
                return Json(new { success = false, message = "Invalid file type for the selected resource type" });
            }

            if (file.Length > 50 * 1024 * 1024) // 50MB limit
            {
                return Json(new { success = false, message = "File size exceeds 50MB limit" });
            }

            // Save file
            using var stream = file.OpenReadStream();
            var (filePath, _, _) = await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType);

            // Create resource record
            var command = new CreateResourceCommand(
                lessonId,
                title ?? file.FileName,
                description,
                type,
                filePath,
                null,
                file.Length,
                file.ContentType,
                0);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Resource uploaded successfully", resourceId = result.Value!.Id });
            }

            return Json(new { success = false, message = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            return Json(new { success = false, message = "Error uploading file" });
        }
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

            // Save file with MD5 deduplication
            using var stream = file.OpenReadStream();
            var (filePath, md5Hash, fileSize) = await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType);

            // Check if file already exists by MD5
            var existingFileResult = await _mediator.Send(new GetFileByMD5Query(md5Hash));
            
            if (existingFileResult.IsSuccess && existingFileResult.Value != null)
            {
                // File already exists, increment reference count
                var incrementCommand = new IncrementFileReferenceCountCommand(existingFileResult.Value.Id);
                await _mediator.Send(incrementCommand);
                
                // Delete the newly uploaded file since we're using the existing one
                await _fileStorageService.DeleteFileAsync(filePath);
                
                return Json(new { 
                    success = true, 
                    message = "فایل با موفقیت ذخیره شد",
                    data = new {
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
                    Path.GetFileName(filePath),
                    file.FileName,
                    filePath,
                    file.ContentType,
                    fileSize,
                    md5Hash
                );

                var result = await _mediator.Send(createFileCommand);
                
                if (result.IsSuccess)
                {
                    return Json(new { 
                        success = true, 
                        message = "فایل با موفقیت آپلود شد",
                        data = new {
                            id = result.Value,
                            fileName = Path.GetFileName(filePath),
                            originalFileName = file.FileName,
                            url = $"/FileUpload/GetFile/{result.Value}",
                            size = fileSize,
                            mimeType = file.ContentType
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
    [HttpGet("GetFile")]
    public async Task<IActionResult> GetFile(int id)
    {
        try
        {
            var fileResult = await _mediator.Send(new GetFileByIdQuery(id));
            if (!fileResult.IsSuccess || fileResult.Value == null)
            {
                return NotFound();
            }

            var file = fileResult.Value;
            
            // Check if file exists - handle both relative and absolute paths
            var filePath = file.FilePath;
            if (filePath.StartsWith("/uploads/"))
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", Path.GetFileName(filePath));
            }
            
            if (!await _fileStorageService.FileExistsAsync(filePath))
            {
                return NotFound("File not found on disk");
            }

            // Get file stream
            var fileStream = await _fileStorageService.GetFileAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(fileStream, file.MimeType ?? "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file: {FileId}", id);
            return NotFound();
        }
    }

    // GET: Download resource
    public async Task<IActionResult> DownloadResource(int resourceId)
    {
        try
        {
            // Get resource details from database
            var resourceResult = await _mediator.Send(new GetResourceByIdQuery(resourceId));
            if (!resourceResult.IsSuccess)
            {
                return NotFound();
            }

            var resource = resourceResult.Value!;
            
            // Check if file exists
            if (!await _fileStorageService.FileExistsAsync(resource.FilePath))
            {
                return NotFound("File not found on disk");
            }

            // Get file stream
            var fileStream = await _fileStorageService.GetFileAsync(resource.FilePath);
            var fileName = Path.GetFileName(resource.FilePath);

            return File(fileStream, resource.MimeType ?? "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading resource: {ResourceId}", resourceId);
            return StatusCode(500, "Error downloading file");
        }
    }

    // GET: Serve educational content files
    [HttpGet("api/files/{*filePath}")]
    public async Task<IActionResult> ServeFile(string filePath)
    {
        try
        {
            // Decode the file path
            filePath = Uri.UnescapeDataString(filePath);
            
            // Ensure the file path is relative to the storage root
            if (Path.IsPathRooted(filePath))
            {
                _logger.LogWarning("Absolute path not allowed: {FilePath}", filePath);
                return BadRequest("Invalid file path");
            }
            
            // Check if file exists
            if (!await _fileStorageService.FileExistsAsync(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return NotFound("File not found");
            }

            // Get file stream
            var fileStream = await _fileStorageService.GetFileAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);

            return File(fileStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving file: {FilePath}", filePath);
            return StatusCode(500, "Error serving file");
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".avi" => "video/avi",
            ".mov" => "video/quicktime",
            ".webm" => "video/webm",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private static bool IsValidFileType(string contentType, ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.PDF => contentType == "application/pdf",
            ResourceType.Video => contentType.StartsWith("video/"),
            ResourceType.Image => contentType.StartsWith("image/"),
            ResourceType.Document => contentType.Contains("document") || contentType.Contains("text") || contentType.Contains("application"),
            ResourceType.URL => true, // URLs don't have file content
            _ => false
        };
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

    // POST: Upload and transcode audio to MP3
    [HttpPost("UploadAudio")]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "فایل صوتی انتخاب نشده است" });
        }

        // Validate audio content type
        var allowedTypes = new[] { "audio/webm", "audio/wav", "audio/mp4", "audio/ogg" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return Json(new { success = false, message = $"فرمت صوتی پشتیبانی نمی‌شود: {file.ContentType}" });
        }

        // Check file size (50MB limit)
        if (file.Length > 50 * 1024 * 1024)
        {
            return Json(new { success = false, message = "حجم فایل بیش از 50 مگابایت است" });
        }

        var fileId = Guid.NewGuid().ToString("N");
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        var tempPath = Path.Combine(uploadsPath, "tmp");
        var finalPath = Path.Combine(uploadsPath, $"{fileId}.mp3");

        try
        {
            // Ensure directories exist
            Directory.CreateDirectory(uploadsPath);
            Directory.CreateDirectory(tempPath);

            // Get file extension from content type
            var extension = GetAudioFileExtension(file.ContentType);
            var tempFilePath = Path.Combine(tempPath, $"{fileId}.{extension}");

            // Save uploaded file to temp location
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Saved uploaded file to: {TempPath}", tempFilePath);

            // Transcode to MP3 using FFmpeg
            var success = await TranscodeToMp3(tempFilePath, finalPath);
            
            if (!success)
            {
                // Clean up temp file
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
                return Json(new { success = false, message = "خطا در تبدیل فایل صوتی" });
            }

            // Clean up temp file
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }

            // Get final file size
            var finalFileInfo = new FileInfo(finalPath);
            var finalSize = finalFileInfo.Exists ? finalFileInfo.Length : 0;

            _logger.LogInformation("Successfully transcoded audio file: {FinalPath}, Size: {Size} bytes", finalPath, finalSize);

            // Save MP3 file to database using existing file management system
            try
            {
                using var mp3Stream = new FileStream(finalPath, FileMode.Open, FileAccess.Read);
                var (_, md5Hash, _) = await _fileStorageService.SaveFileAsync(mp3Stream, $"{fileId}.mp3", "audio/mpeg");
                
                // Check if file already exists by MD5
                var existingFileResult = await _mediator.Send(new GetFileByMD5Query(md5Hash));
                
                if (existingFileResult.IsSuccess && existingFileResult.Value != null)
                {
                    // File already exists, increment reference count
                    var incrementCommand = new IncrementFileReferenceCountCommand(existingFileResult.Value.Id);
                    await _mediator.Send(incrementCommand);
                    
                    return Json(new
                    {
                        success = true,
                        message = "فایل صوتی با موفقیت آپلود و در دیتابیس ذخیره شد",
                        url = $"/FileUpload/GetFile/{existingFileResult.Value.Id}",
                        contentType = "audio/mpeg",
                        sizeBytes = finalSize,
                        originalSizeBytes = file.Length,
                        originalContentType = file.ContentType,
                        fileId = existingFileResult.Value.Id
                    });
                }
                else
                {
                    // Create new file record
                    var createFileCommand = new CreateFileCommand(
                        $"{fileId}.mp3",
                        $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp3",
                        finalPath,
                        "audio/mpeg",
                        finalSize,
                        md5Hash
                    );

                    var result = await _mediator.Send(createFileCommand);
                    
                    if (result.IsSuccess)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "فایل صوتی با موفقیت آپلود و در دیتابیس ذخیره شد",
                            url = $"/FileUpload/GetFile/{result.Value}",
                            contentType = "audio/mpeg",
                            sizeBytes = finalSize,
                            originalSizeBytes = file.Length,
                            originalContentType = file.ContentType,
                            fileId = result.Value
                        });
                    }
                    else
                    {
                        _logger.LogError("Failed to save audio file to database: {Error}", result.Error);
                        return Json(new
                        {
                            success = true,
                            message = "فایل صوتی آپلود شد اما در دیتابیس ذخیره نشد",
                            url = $"/uploads/{fileId}.mp3",
                            contentType = "audio/mpeg",
                            sizeBytes = finalSize,
                            originalSizeBytes = file.Length,
                            originalContentType = file.ContentType
                        });
                    }
                }
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Error saving audio file to database");
                return Json(new
                {
                    success = true,
                    message = "فایل صوتی آپلود شد اما در دیتابیس ذخیره نشد",
                    url = $"/uploads/{fileId}.mp3",
                    contentType = "audio/mpeg",
                    sizeBytes = finalSize,
                    originalSizeBytes = file.Length,
                    originalContentType = file.ContentType
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio upload: {FileName}", file.FileName);
            
            // Clean up any temp files
            var tempFilePath = Path.Combine(tempPath, $"{fileId}.{GetAudioFileExtension(file.ContentType)}");
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }
            
            if (System.IO.File.Exists(finalPath))
            {
                System.IO.File.Delete(finalPath);
            }

            return Json(new { success = false, message = "خطا در پردازش فایل صوتی" });
        }
    }

    /// <summary>
    /// Transcode audio file to MP3 using FFmpeg
    /// </summary>
    private async Task<bool> TranscodeToMp3(string inputPath, string outputPath)
    {
        try
        {
            // FFmpeg command: -y (overwrite), -i (input), -vn (no video), -ar 48000 (sample rate), -ac 2 (stereo), -b:a 160k (bitrate)
            var ffmpegArgs = $"-y -i \"{inputPath}\" -vn -ar 48000 -ac 2 -b:a 160k \"{outputPath}\"";

            _logger.LogInformation("Starting FFmpeg transcoding: ffmpeg {Args}", ffmpegArgs);

            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            
            var output = new List<string>();
            var error = new List<string>();

            process.OutputDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.Add(e.Data);
                    _logger.LogDebug("FFmpeg output: {Output}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    error.Add(e.Data);
                    _logger.LogDebug("FFmpeg error: {Error}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for completion with timeout (5 minutes)
            var completed = await Task.Run(() => process.WaitForExit(300000));
            
            if (!completed)
            {
                _logger.LogError("FFmpeg process timed out");
                process.Kill();
                return false;
            }

            if (process.ExitCode != 0)
            {
                _logger.LogError("FFmpeg failed with exit code {ExitCode}. Error: {Error}", 
                    process.ExitCode, string.Join(Environment.NewLine, error));
                return false;
            }

            // Verify output file exists and has content
            if (!System.IO.File.Exists(outputPath))
            {
                _logger.LogError("FFmpeg completed but output file does not exist: {OutputPath}", outputPath);
                return false;
            }

            var outputFileInfo = new FileInfo(outputPath);
            if (outputFileInfo.Length == 0)
            {
                _logger.LogError("FFmpeg output file is empty: {OutputPath}", outputPath);
                return false;
            }

            _logger.LogInformation("FFmpeg transcoding completed successfully. Output size: {Size} bytes", outputFileInfo.Length);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during FFmpeg transcoding");
            return false;
        }
    }

    /// <summary>
    /// Get file extension from audio content type
    /// </summary>
    private static string GetAudioFileExtension(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "audio/webm" => "webm",
            "audio/wav" => "wav",
            "audio/mp4" => "mp4",
            "audio/ogg" => "ogg",
            _ => "webm" // default
        };
    }


}
