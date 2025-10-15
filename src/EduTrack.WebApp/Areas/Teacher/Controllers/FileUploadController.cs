using EduTrack.Application.Features.Files.Commands;
using EduTrack.Application.Features.Files.Queries;
using EduTrack.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
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
    [HttpPost]
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
                        url = $"/Teacher/FileUpload/GetFile/{existingFileResult.Value.Id}",
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
                            url = $"/Teacher/FileUpload/GetFile/{result.Value}",
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
    [HttpGet]
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
            
            // Check if file exists
            if (!await _fileStorageService.FileExistsAsync(file.FilePath))
            {
                return NotFound("File not found on disk");
            }

            // Get file stream
            var fileStream = await _fileStorageService.GetFileAsync(file.FilePath);
            var fileName = Path.GetFileName(file.FilePath);

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
}
