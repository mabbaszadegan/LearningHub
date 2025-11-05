using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EduTrack.Application.Common.Services.Validators;

/// <summary>
/// Validator for Ordering block answers
/// </summary>
public class OrderingBlockValidator : IBlockAnswerValidator
{
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public ScheduleItemType SupportedType => ScheduleItemType.Ordering;

    public OrderingBlockValidator(IScheduleItemRepository scheduleItemRepository)
    {
        _scheduleItemRepository = scheduleItemRepository;
    }

    public async Task<BlockValidationResult> ValidateAnswerAsync(
        int scheduleItemId,
        string blockId,
        Dictionary<string, object> submittedAnswer,
        CancellationToken cancellationToken = default)
    {
        // Get schedule item
        var scheduleItem = await _scheduleItemRepository.GetByIdAsync(scheduleItemId, cancellationToken);
        if (scheduleItem == null)
        {
            throw new ArgumentException("Schedule item not found", nameof(scheduleItemId));
        }

        if (scheduleItem.Type != ScheduleItemType.Ordering)
        {
            throw new ArgumentException("Schedule item is not of Ordering type", nameof(scheduleItemId));
        }

        // Parse content JSON
        var contentJson = scheduleItem.ContentJson;
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            throw new InvalidOperationException("Schedule item content is empty");
        }

        var content = JsonConvert.DeserializeObject<OrderingContent>(contentJson);
        if (content == null)
        {
            throw new InvalidOperationException("Failed to parse schedule item content");
        }

        // Find the block
        OrderingBlock? block = null;
        if (content.Blocks != null && content.Blocks.Any())
        {
            block = content.Blocks.FirstOrDefault(b => b.Id == blockId);
        }
        else if (blockId == "main" || blockId == "legacy")
        {
            // Legacy single block format
            block = new OrderingBlock
            {
                Id = "main",
                Items = content.Items,
                CorrectOrder = content.CorrectOrder,
                Points = content.Points,
                Instruction = content.Instruction ?? string.Empty
            };
        }

        if (block == null)
        {
            throw new ArgumentException($"Block with ID '{blockId}' not found", nameof(blockId));
        }

        // Extract submitted order
        if (!submittedAnswer.TryGetValue("order", out var orderValue))
        {
            throw new ArgumentException("Submitted answer must contain 'order' field", nameof(submittedAnswer));
        }

        List<string> submittedOrder = new List<string>();
        
        // Handle different JSON formats
        if (orderValue == null)
        {
            submittedOrder = new List<string>();
        }
        else if (orderValue is JArray jArray)
        {
            submittedOrder = jArray.Select(x => x?.ToString() ?? string.Empty).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        else if (orderValue is JToken jTokenValue && jTokenValue.Type == JTokenType.Array)
        {
            submittedOrder = jTokenValue.Select(x => x?.ToString() ?? string.Empty).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        else if (orderValue is IEnumerable<object> enumerable)
        {
            submittedOrder = enumerable.Select(x => x?.ToString() ?? string.Empty).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        else if (orderValue is string str)
        {
            if (str.StartsWith("[") && str.EndsWith("]"))
            {
                submittedOrder = JsonConvert.DeserializeObject<List<string>>(str) ?? new List<string>();
            }
            else
            {
                submittedOrder = new List<string> { str };
            }
        }
        else
        {
            // Try to convert to JToken and parse
            try
            {
                var jToken = orderValue as JToken ?? JToken.FromObject(orderValue);
                if (jToken.Type == JTokenType.Array)
                {
                    submittedOrder = jToken.Select(x => x?.ToString() ?? string.Empty).Where(x => !string.IsNullOrEmpty(x)).ToList();
                }
                else
                {
                    throw new ArgumentException($"Invalid format for submitted order. Expected array, got {orderValue?.GetType().Name}", nameof(submittedAnswer));
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid format for submitted order: {ex.Message}", nameof(submittedAnswer));
            }
        }

        // Compare with correct order
        var correctOrder = block.CorrectOrder ?? new List<string>();
        var isCorrect = CompareOrder(submittedOrder, correctOrder);

        // Calculate points
        var maxPoints = block.Points;
        var pointsEarned = isCorrect ? maxPoints : 0;

        // Prepare result
        var result = new BlockValidationResult
        {
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            MaxPoints = maxPoints,
            CorrectAnswer = new Dictionary<string, object> { { "order", correctOrder } },
            SubmittedAnswer = new Dictionary<string, object> { { "order", submittedOrder } },
            Feedback = isCorrect ? "عالی! پاسخ شما صحیح است." : "متأسفانه پاسخ شما صحیح نیست. لطفاً دوباره تلاش کنید.",
            DetailedFeedback = new Dictionary<string, object>
            {
                { "submittedOrder", submittedOrder },
                { "correctOrder", correctOrder }
            }
        };

        return result;
    }

    private bool CompareOrder(List<string> submitted, List<string> correct)
    {
        if (submitted.Count != correct.Count)
            return false;

        for (int i = 0; i < submitted.Count; i++)
        {
            if (submitted[i] != correct[i])
                return false;
        }

        return true;
    }
}

