using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.EducationalContent.Commands;

public record CreateEducationalContentCommand(
    int SubChapterId,
    string Title,
    string? Description,
    EducationalContentType Type,
    string? TextContent,
    IFormFile? File,
    string? ExternalUrl,
    int Order = 0
) : IRequest<Result<EducationalContentDto>>;

public record UpdateEducationalContentCommand(
    int Id,
    string Title,
    string? Description,
    EducationalContentType Type,
    string? TextContent,
    IFormFile? File,
    string? ExternalUrl,
    bool IsActive,
    int Order
) : IRequest<Result<EducationalContentDto>>;

public record DeleteEducationalContentCommand(int Id) : IRequest<Result<bool>>;

public class CreateEducationalContentCommandValidator : AbstractValidator<CreateEducationalContentCommand>
{
    public CreateEducationalContentCommandValidator()
    {
        RuleFor(x => x.SubChapterId)
            .GreaterThan(0)
            .WithMessage("SubChapter ID is required");

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

public class CreateEducationalContentCommandHandler : IRequestHandler<CreateEducationalContentCommand, Result<EducationalContentDto>>
{
    private readonly IRepository<Domain.Entities.EducationalContent> _contentRepository;
    private readonly IRepository<Domain.Entities.File> _fileRepository;
    private readonly IRepository<Domain.Entities.SubChapter> _subChapterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public CreateEducationalContentCommandHandler(
        IRepository<Domain.Entities.EducationalContent> contentRepository,
        IRepository<Domain.Entities.File> fileRepository,
        IRepository<Domain.Entities.SubChapter> subChapterRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _contentRepository = contentRepository;
        _fileRepository = fileRepository;
        _subChapterRepository = subChapterRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<EducationalContentDto>> Handle(CreateEducationalContentCommand request, CancellationToken cancellationToken)
    {
        // Validate subchapter exists
        var subChapter = await _subChapterRepository.GetByIdAsync(request.SubChapterId, cancellationToken);
        if (subChapter == null)
        {
            return Result<EducationalContentDto>.Failure("SubChapter not found");
        }

        // Get current user
        var currentUser = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUser))
        {
            return Result<EducationalContentDto>.Failure("User not authenticated");
        }

        Domain.Entities.File? file = null;

        // Handle file upload if present
        if (request.File != null)
        {
            using var fileStream = request.File.OpenReadStream();
            var (filePath, md5Hash, fileSize) = await _fileStorageService.SaveFileAsync(fileStream, request.File.FileName, request.File.ContentType, cancellationToken);

            // Check if file with same MD5 already exists
            var existingFile = await _fileRepository.GetAll()
                .FirstOrDefaultAsync(f => f.MD5Hash == md5Hash, cancellationToken);

            if (existingFile != null)
            {
                // File already exists, increment reference count and use existing file
                existingFile.IncrementReferenceCount();
                await _fileRepository.UpdateAsync(existingFile, cancellationToken);
                file = existingFile;

                // Delete the newly uploaded file since we're using the existing one
                await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            }
            else
            {
                // Create new file record
                file = Domain.Entities.File.Create(
                    Path.GetFileName(filePath),
                    request.File.FileName,
                    filePath,
                    request.File.ContentType,
                    fileSize,
                    md5Hash,
                    currentUser);

                await _fileRepository.AddAsync(file, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken); // Save the file first
            }
        }

        // Get the next order number if not provided
        var order = request.Order;
        if (order == 0)
        {
            var existingContents = await _contentRepository.GetAll()
                .Where(ec => ec.SubChapterId == request.SubChapterId)
                .ToListAsync(cancellationToken);
            order = existingContents.Any() ? existingContents.Max(ec => ec.Order) + 1 : 1;
        }

        var content = Domain.Entities.EducationalContent.Create(
            request.SubChapterId,
            request.Title,
            request.Type,
            request.TextContent,
            file?.Id,
            request.ExternalUrl,
            order,
            currentUser);

        await _contentRepository.AddAsync(content, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new EducationalContentDto
        {
            Id = content.Id,
            SubChapterId = content.SubChapterId,
            Title = content.Title,
            Description = content.Description,
            Type = content.Type,
            TextContent = content.TextContent,
            FileId = content.FileId,
            ExternalUrl = content.ExternalUrl,
            IsActive = content.IsActive,
            Order = content.Order,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt,
            CreatedBy = content.CreatedBy,
            File = file != null ? new FileDto
            {
                Id = file.Id,
                FileName = file.FileName,
                OriginalFileName = file.OriginalFileName,
                FilePath = file.FilePath,
                MimeType = file.MimeType,
                FileSizeBytes = file.FileSizeBytes,
                MD5Hash = file.MD5Hash,
                CreatedAt = file.CreatedAt,
                CreatedBy = file.CreatedBy,
                ReferenceCount = file.ReferenceCount,
                FileUrl = $"/api/files/{file.FilePath}"
            } : null
        };

        return Result<EducationalContentDto>.Success(dto);
    }
}