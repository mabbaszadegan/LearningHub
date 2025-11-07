using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EduTrack.Application.Features.StudentProfiles;
using EduTrack.Application.Features.StudentProfiles.Queries;
using EduTrack.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EduTrack.WebApp.Services;

public class StudentProfileContext : IStudentProfileContext
{
    private const string ProfileIdSessionKey = "StudentProfileContext.ProfileId";
    private const string ProfileNameSessionKey = "StudentProfileContext.ProfileName";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<StudentProfileContext> _logger;

    public StudentProfileContext(
        IHttpContextAccessor httpContextAccessor,
        IMediator mediator,
        UserManager<User> userManager,
        ILogger<StudentProfileContext> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
        _userManager = userManager;
        _logger = logger;
    }

    public Task<int?> GetActiveProfileIdAsync(CancellationToken cancellationToken = default)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return Task.FromResult<int?>(session?.GetInt32(ProfileIdSessionKey));
    }

    public Task<string?> GetActiveProfileNameAsync(CancellationToken cancellationToken = default)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return Task.FromResult(session?.GetString(ProfileNameSessionKey));
    }

    public async Task SetActiveProfileAsync(int? profileId, CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogWarning("Attempted to set student profile outside of HTTP context");
            return;
        }

        var session = httpContext.Session;
        if (profileId == null)
        {
            session.Remove(ProfileIdSessionKey);
            session.Remove(ProfileNameSessionKey);
            _logger.LogInformation("Cleared active student profile from session");
            return;
        }

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser == null)
        {
            _logger.LogWarning("Unable to resolve current user when setting student profile");
            return;
        }

        var profileResult = await _mediator.Send(new GetStudentProfileByIdQuery(profileId.Value, currentUser.Id), cancellationToken);
        if (!profileResult.IsSuccess || profileResult.Value == null)
        {
            _logger.LogWarning("Requested student profile {ProfileId} was not found for user {UserId}", profileId, currentUser.Id);
            return;
        }

        session.SetInt32(ProfileIdSessionKey, profileId.Value);
        session.SetString(ProfileNameSessionKey, profileResult.Value.DisplayName);
        _logger.LogInformation("Set active student profile {ProfileId} for user {UserId}", profileId, currentUser.Id);
    }

    public async Task<IReadOnlyList<StudentProfileDto>> GetProfilesForCurrentUserAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser == null)
        {
            return Array.Empty<StudentProfileDto>();
        }

        var result = await _mediator.Send(new GetStudentProfilesQuery(currentUser.Id, includeArchived), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            _logger.LogWarning("Failed to load student profiles for user {UserId}: {Error}", currentUser.Id, result.Error);
            return Array.Empty<StudentProfileDto>();
        }

        return result.Value;
    }

    private async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        return await _userManager.GetUserAsync(httpContext.User);
    }
}

