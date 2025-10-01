using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.Exams.Commands;
using EduTrack.Application.Features.Exams.Queries;
using EduTrack.Application.Features.Classroom.Commands;
using EduTrack.Application.Features.Classroom.Queries;
using EduTrack.Application.Features.Progress.Commands;
using EduTrack.Application.Features.Progress.Queries;
using EduTrack.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EduTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IClock, Clock>();

        return services;
    }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    public UserRole? Role => Enum.TryParse<UserRole>(_httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value, out var role) ? role : null;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

public class Clock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
