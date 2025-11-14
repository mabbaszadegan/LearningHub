using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Statistics;
using MediatR;

namespace EduTrack.Application.Features.Statistics.Queries;

public record GetStudentLearningStatisticsQuery(
    string StudentId,
    int? StudentProfileId = null,
    int RecentTopicsLimit = 5,
    int MostIncorrectTopicsLimit = 5) : IRequest<Result<LearningStatisticsDto>>;

