using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Files;
using MediatR;

namespace EduTrack.Application.Features.Files.Queries;

public record GetFileByMD5Query(string MD5Hash) : IRequest<Result<FileDto>>;
