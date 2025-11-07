using EduTrack.Application.Common.Models.ScheduleItems;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.Queries;

// Get written content answer by student and schedule item
public record GetWrittenContentAnswerQuery(
    int ScheduleItemId,
    string StudentId,
    int? StudentProfileId = null
) : IRequest<WrittenContentAnswer?>;

// Get all answers for a written content schedule item (for teacher grading)
public record GetWrittenContentAnswersQuery(
    int ScheduleItemId
) : IRequest<List<WrittenContentAnswer>>;

// Get written content answer by ID
public record GetWrittenContentAnswerByIdQuery(
    int AnswerId
) : IRequest<WrittenContentAnswer?>;
