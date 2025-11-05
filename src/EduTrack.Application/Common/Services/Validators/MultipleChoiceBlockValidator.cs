using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using Newtonsoft.Json;

namespace EduTrack.Application.Common.Services.Validators;

/// <summary>
/// Validator for MultipleChoice block answers
/// </summary>
public class MultipleChoiceBlockValidator : IBlockAnswerValidator
{
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public ScheduleItemType SupportedType => ScheduleItemType.MultipleChoice;

    public MultipleChoiceBlockValidator(IScheduleItemRepository scheduleItemRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
    }

    public async Task<BlockValidationResult> ValidateAnswerAsync(
        int scheduleItemId,
        string blockId,
        Dictionary<string, object> submittedAnswer,
        CancellationToken cancellationToken = default)
    {
        if (blockId != "main")
        {
            throw new ArgumentException("MultipleChoice only supports 'main' block", nameof(blockId));
        }

        // Get schedule item
        var scheduleItem = await _scheduleItemRepository.GetByIdAsync(scheduleItemId, cancellationToken);
        if (scheduleItem == null)
        {
            throw new ArgumentException("Schedule item not found", nameof(scheduleItemId));
        }

        if (scheduleItem.Type != ScheduleItemType.MultipleChoice)
        {
            throw new ArgumentException("Schedule item is not of MultipleChoice type", nameof(scheduleItemId));
        }

        // Parse content JSON
        var contentJson = scheduleItem.ContentJson;
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            throw new InvalidOperationException("Schedule item content is empty");
        }

        var content = JsonConvert.DeserializeObject<MultipleChoiceContent>(contentJson);
        if (content == null)
        {
            throw new InvalidOperationException("Failed to parse schedule item content");
        }

        // Extract submitted answers
        if (!submittedAnswer.TryGetValue("selectedOptions", out var selectedOptionsValue))
        {
            throw new ArgumentException("Submitted answer must contain 'selectedOptions' field", nameof(submittedAnswer));
        }

        List<int> selectedOptions;
        if (selectedOptionsValue is List<object> list)
        {
            selectedOptions = list.Select(x => Convert.ToInt32(x)).ToList();
        }
        else if (selectedOptionsValue is string str && str.StartsWith("["))
        {
            selectedOptions = JsonConvert.DeserializeObject<List<int>>(str) ?? new List<int>();
        }
        else
        {
            throw new ArgumentException("Invalid format for selectedOptions", nameof(submittedAnswer));
        }

        // Get correct answers
        var correctAnswers = content.CorrectAnswers ?? new List<int>();
        var isCorrect = content.AnswerType == "multiple"
            ? CompareMultipleChoice(selectedOptions, correctAnswers)
            : CompareSingleChoice(selectedOptions, correctAnswers);

        // Calculate points
        var maxPoints = content.Options?.Any(o => o.IsCorrect) == true 
            ? content.Options.Count(o => o.IsCorrect) 
            : 1m;
        var pointsEarned = isCorrect ? maxPoints : 0;

        // Prepare result
        var result = new BlockValidationResult
        {
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            MaxPoints = maxPoints,
            CorrectAnswer = new Dictionary<string, object> { { "selectedOptions", correctAnswers } },
            SubmittedAnswer = new Dictionary<string, object> { { "selectedOptions", selectedOptions } },
            Feedback = isCorrect ? "عالی! پاسخ شما صحیح است." : "متأسفانه پاسخ شما صحیح نیست. لطفاً دوباره تلاش کنید.",
            DetailedFeedback = new Dictionary<string, object>
            {
                { "submittedOptions", selectedOptions },
                { "correctOptions", correctAnswers },
                { "answerType", content.AnswerType }
            }
        };

        return result;
    }

    private bool CompareSingleChoice(List<int> submitted, List<int> correct)
    {
        if (submitted.Count != 1 || correct.Count != 1)
            return false;

        return submitted[0] == correct[0];
    }

    private bool CompareMultipleChoice(List<int> submitted, List<int> correct)
    {
        if (submitted.Count != correct.Count)
            return false;

        var submittedSet = new HashSet<int>(submitted);
        var correctSet = new HashSet<int>(correct);

        return submittedSet.SetEquals(correctSet);
    }
}

