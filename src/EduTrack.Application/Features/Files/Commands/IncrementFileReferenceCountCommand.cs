using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.Files.Commands;

public record IncrementFileReferenceCountCommand(int FileId) : IRequest<Result>;
