using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Application.Features.Users.Queries;

public record GetTeachersQuery() : IRequest<IEnumerable<User>>;

public class GetTeachersQueryHandler : IRequestHandler<GetTeachersQuery, IEnumerable<User>>
{
    private readonly UserManager<User> _userManager;

    public GetTeachersQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<User>> Handle(GetTeachersQuery request, CancellationToken cancellationToken)
    {
        var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
        return teachers;
    }
}
