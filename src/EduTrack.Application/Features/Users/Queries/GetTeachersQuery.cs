using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.Users.Queries;

public record GetTeachersQuery() : IRequest<IEnumerable<User>>;

public class GetTeachersQueryHandler : IRequestHandler<GetTeachersQuery, IEnumerable<User>>
{
    private readonly IUserService _userService;

    public GetTeachersQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IEnumerable<User>> Handle(GetTeachersQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetUsersByRoleAsync(UserRole.Teacher, cancellationToken);
    }
}
