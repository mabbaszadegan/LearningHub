using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Files;
using MediatR;

namespace EduTrack.Application.Features.Files.Queries;

public record GetFileByIdQuery(int Id) : IRequest<Result<FileDto>>;
