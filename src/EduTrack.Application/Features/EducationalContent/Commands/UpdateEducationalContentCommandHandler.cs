using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.EducationalContent.Commands;

public class UpdateEducationalContentCommandValidator : AbstractValidator<UpdateEducationalContentCommand>
{
    public UpdateEducationalContentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Content ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Title is required and must be less than 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must be less than 1000 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid content type");

        RuleFor(x => x.TextContent)
            .NotEmpty()
            .When(x => x.Type == EducationalContentType.Text)
            .WithMessage("Text content is required for text type");

        RuleFor(x => x.File)
            .NotNull()
            .When(x => x.Type == EducationalContentType.Image || 
                      x.Type == EducationalContentType.Video || 
                      x.Type == EducationalContentType.Audio || 
                      x.Type == EducationalContentType.PDF || 
                      x.Type == EducationalContentType.File)
            .WithMessage("File is required for file-based content types");

        RuleFor(x => x.ExternalUrl)
            .NotEmpty()
            .Must(BeValidUrl)
            .When(x => x.Type == EducationalContentType.ExternalUrl)
            .WithMessage("Valid external URL is required for external content type");
    }

    private static bool BeValidUrl(string? url)
    {
        return !string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class UpdateEducationalContentCommandHandler : IRequestHandler<UpdateEducationalContentCommand, Result<EducationalContentDto>>
{
    private readonly IRepository<Domain.Entities.EducationalContent> _contentRepository;
    private readonly IRepository<Domain.Entities.File> _fileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UpdateEducationalContentCommandHandler(
        IRepository<Domain.Entities.EducationalContent> contentRepository,
        IRepository<Domain.Entities.File> fileRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _contentRepository = contentRepository;
        _fileRepository = fileRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<EducationalContentDto>> Handle(UpdateEducationalContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (content == null)
        {
            return Result<EducationalContentDto>.Failure("Educational content not found");
        }

        // Handle file update if new file is provided
        if (request.File != null)
        {
            // Decrement reference count for old file if exists
            if (content.FileId.HasValue)
            {
                var oldFile = await _fileRepository.GetByIdAsync(content.FileId.Value, cancellationToken);
                if (oldFile != null)
                {
                    oldFile.ReferenceCount--;
                    if (oldFile.ReferenceCount <= 0)
                    {
                        await _fileStorageService.DeleteFileAsync(oldFile.FilePath, cancellationToken);
                        await _fileRepository.DeleteAsync(oldFile, cancellationToken);
                    }
                    else
                    {
                        await _fileRepository.UpdateAsync(oldFile, cancellationToken);
                    }
                }
            }

            // Upload new file
            using var fileStream = request.File.OpenReadStream();
            var (filePath, md5Hash, fileSize) = await _fileStorageService.SaveFileAsync(fileStream, request.File.FileName, request.File.ContentType, cancellationToken);

            // Check if file with same MD5 already exists
            var existingFile = await _fileRepository.GetAll()
                .FirstOrDefaultAsync(f => f.MD5Hash == md5Hash, cancellationToken);

            if (existingFile != null)
            {
                // File already exists, increment reference count and use existing file
                existingFile.ReferenceCount++;
                await _fileRepository.UpdateAsync(existingFile, cancellationToken);
                content.FileId = existingFile.Id;

                // Delete the newly uploaded file since we're using the existing one
                await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            }
            else
            {
                // Create new file record
                var fileCreatedAt = _clock.UtcNow;
                var newFile = new Domain.Entities.File
                {
                    FileName = Path.GetFileName(filePath),
                    OriginalFileName = request.File.FileName,
                    FilePath = filePath,
                    MimeType = request.File.ContentType,
                    FileSizeBytes = fileSize,
                    MD5Hash = md5Hash,
                    CreatedAt = fileCreatedAt,
                    CreatedBy = _currentUserService.UserId ?? "system",
                    ReferenceCount = 1
                };

                await _fileRepository.AddAsync(newFile, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken); // Save the file first
                content.FileId = newFile.Id;
            }
        }

        // Update content properties
        content.Title = request.Title;
        content.Description = request.Description;
        content.Type = request.Type;
        content.TextContent = request.TextContent;
        content.ExternalUrl = request.ExternalUrl;
        content.IsActive = request.IsActive;
        content.Order = request.Order;
        content.UpdatedAt = _clock.UtcNow;

        await _contentRepository.UpdateAsync(content, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load the updated content with file information
        var updatedContent = await _contentRepository.GetAll()
            .Include(c => c.File)
            .FirstOrDefaultAsync(c => c.Id == content.Id, cancellationToken);

        var dto = new EducationalContentDto
        {
            Id = updatedContent!.Id,
            SubChapterId = updatedContent.SubChapterId,
            Title = updatedContent.Title,
            Description = updatedContent.Description,
            Type = updatedContent.Type,
            TextContent = updatedContent.TextContent,
            FileId = updatedContent.FileId,
            ExternalUrl = updatedContent.ExternalUrl,
            IsActive = updatedContent.IsActive,
            Order = updatedContent.Order,
            CreatedAt = updatedContent.CreatedAt,
            UpdatedAt = updatedContent.UpdatedAt,
            CreatedBy = updatedContent.CreatedBy,
            File = updatedContent.File != null ? new FileDto
            {
                Id = updatedContent.File.Id,
                FileName = updatedContent.File.FileName,
                OriginalFileName = updatedContent.File.OriginalFileName,
                FilePath = updatedContent.File.FilePath,
                MimeType = updatedContent.File.MimeType,
                FileSizeBytes = updatedContent.File.FileSizeBytes,
                MD5Hash = updatedContent.File.MD5Hash,
                CreatedAt = updatedContent.File.CreatedAt,
                CreatedBy = updatedContent.File.CreatedBy,
                ReferenceCount = updatedContent.File.ReferenceCount,
                FileUrl = $"/api/files/{updatedContent.File.FilePath}"
            } : null
        };

        return Result<EducationalContentDto>.Success(dto);
    }
}