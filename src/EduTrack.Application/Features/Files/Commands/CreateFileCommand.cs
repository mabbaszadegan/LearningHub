using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.Files.Commands;

public record CreateFileCommand(
    string FileName,
    string OriginalFileName,
    string FilePath,
    string MimeType,
    long FileSizeBytes,
    string MD5Hash
) : IRequest<Result<int>>;
