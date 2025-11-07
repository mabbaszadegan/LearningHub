using System;
using System.Collections.Generic;
using EduTrack.Application.Features.StudentProfiles;

namespace EduTrack.WebApp.Areas.Student.Models;

public class StudentProfileSelectorViewModel
{
    public IReadOnlyList<StudentProfileDto> Profiles { get; init; } = Array.Empty<StudentProfileDto>();
    public int? ActiveProfileId { get; init; }
    public string? ActiveProfileName { get; init; }
}

