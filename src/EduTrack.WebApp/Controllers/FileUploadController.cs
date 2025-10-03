using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Controllers;

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
}
