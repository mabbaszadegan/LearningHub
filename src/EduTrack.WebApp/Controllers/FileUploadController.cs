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

            // Handle MP3 conversion for recorded audio files only
            // Check if this is a recorded audio file by looking at the filename or content type
            bool isRecordedAudio = type == "audio" && 
                (file.FileName.Contains("recording_") || 
                 file.ContentType == "audio/webm" || 
                 file.FileName.EndsWith(".webm", StringComparison.OrdinalIgnoreCase));
            
            if (isRecordedAudio)
            {
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
                    using (var tempStream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(tempStream);
                    }

                    _logger.LogInformation("Saved uploaded file to: {TempPath}", tempFilePath);

                    // Transcode to MP3 using FFmpeg
                    var success = await TranscodeToMp3(tempFilePath, finalPath);

                    if (!success)
                    {
                        // Fallback: Save the original file without MP3 conversion
                        _logger.LogWarning("FFmpeg conversion failed, falling back to original file format");
                        finalMimeType = file.ContentType;
                        finalFileSize = file.Length;
                        
                        // Move temp file to final location
                        var fallbackPath = Path.Combine(uploadsPath, $"{fileId}.{extension}");
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Move(tempFilePath, fallbackPath);
                            finalFilePath = fallbackPath;
                        }
                    }
                    else
                    {
                        // Clean up temp file after successful conversion
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(tempFilePath);
                        }
                    }

                    if (success)
                    {
                        // Get final file size for MP3
                        var finalFileInfo = new FileInfo(finalPath);
                        finalFileSize = finalFileInfo.Exists ? finalFileInfo.Length : 0;
                        finalMimeType = "audio/mpeg";
                        finalFilePath = finalPath;
                    }

                    _logger.LogInformation("Successfully transcoded audio file: {FinalPath}, Size: {Size} bytes", finalPath, finalFileSize);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing audio conversion: {FileName}", file.FileName);
                    return Json(new { success = false, message = "خطا در پردازش فایل صوتی" });
                }
            }
            else
            {
                // Save file with MD5 deduplication (original behavior)
                using var stream = file.OpenReadStream();
                finalFilePath = (await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType)).Item1;
                finalMimeType = file.ContentType;
                finalFileSize = file.Length;
            }

            // Calculate MD5 hash for the final file
            string md5Hash;
            using (var stream = new FileStream(finalFilePath, FileMode.Open, FileAccess.Read))
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
                if (isRecordedAudio)
                {
                    // For MP3 converted files, delete the physical file
                    if (System.IO.File.Exists(finalFilePath))
                    {
                        System.IO.File.Delete(finalFilePath);
                    }
                }
                else
                {
                    // For regular files, use the file storage service
                    await _fileStorageService.DeleteFileAsync(finalFilePath);
                }


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
    /// Transcode audio file to MP3 using FFmpeg
    /// </summary>
    private async Task<bool> TranscodeToMp3(string inputPath, string outputPath)
    {
        try
        {
            // Check if ffmpeg is available
            if (!IsFFmpegAvailable())
            {
                _logger.LogWarning("FFmpeg is not available on this system. Audio conversion will be skipped.");
                return false;
            }

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

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.Add(e.Data);
                    _logger.LogDebug("FFmpeg output: {Output}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
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

    /// <summary>
    /// Calculate MD5 hash of a stream
    /// </summary>
    private static async Task<string> CalculateMD5Async(Stream stream)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hashBytes = await md5.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Check if FFmpeg is available on the system
    /// </summary>
    private static bool IsFFmpegAvailable()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit(5000); // 5 second timeout
            
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }


}
