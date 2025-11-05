using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

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

        // Parse content JSON - use the same method as Student Controller
        var contentJson = scheduleItem.ContentJson;
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            throw new InvalidOperationException("Schedule item content is empty");
        }

        // Parse JSON directly (same as ParseOrderingFromBlocks in Student Controller)
        var contentObj = JObject.Parse(contentJson);
        OrderingBlock? block = null;

        // Check if JSON has the new structure with blocks
        if (contentObj["blocks"] is JArray blocksArray)
        {
            // Find the block by ID
            foreach (var blockToken in blocksArray)
            {
                var blockObj = blockToken as JObject;
                if (blockObj == null) continue;

                var blockType = blockObj["type"]?.ToString().ToLower();
                if (!string.Equals(blockType, "ordering", StringComparison.OrdinalIgnoreCase))
                    continue;

                var currentBlockId = blockObj["id"]?.ToString();
                if (string.IsNullOrEmpty(currentBlockId) || !string.Equals(currentBlockId, blockId, StringComparison.OrdinalIgnoreCase))
                    continue;

                var order = blockObj["order"]?.Value<int>() ?? 0;
                var data = blockObj["data"] as JObject;
                if (data == null) continue;

                // Parse block exactly like ParseOrderingFromBlocks
                block = new OrderingBlock
                {
                    Id = currentBlockId,
                    Order = order,
                    Instruction = data["instruction"]?.ToString() ?? string.Empty,
                    AllowDragDrop = data["allowDragDrop"]?.Value<bool>() ?? true,
                    Direction = data["direction"]?.ToString() ?? "vertical",
                    Alignment = data["alignment"]?.ToString() ?? "right",
                    ShowNumbers = data["showNumbers"]?.Value<bool>() ?? true,
                    IsRequired = data["isRequired"]?.Value<bool>() ?? true
                };

                // Parse points
                var pointsToken = data["points"];
                if (pointsToken != null)
                {
                    if (pointsToken.Type == JTokenType.Integer || pointsToken.Type == JTokenType.Float)
                    {
                        block.Points = pointsToken.Value<decimal>();
                    }
                    else if (pointsToken.Type == JTokenType.String && decimal.TryParse(pointsToken.ToString(), out var pts))
                    {
                        block.Points = pts;
                    }
                }

                // Parse items
                if (data["items"] is JArray items)
                {
                    foreach (var t in items)
                    {
                        if (t is not JObject itemObj) continue;
                        var item = new OrderingItem
                        {
                            Id = itemObj["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                            Type = itemObj["type"]?.ToString() ?? "text",
                            Include = itemObj["include"]?.Value<bool>() ?? true,
                            Value = itemObj["value"]?.ToString(),
                            FileUrl = itemObj["fileUrl"]?.ToString(),
                            FileName = itemObj["fileName"]?.ToString(),
                            MimeType = itemObj["mimeType"]?.ToString(),
                            FileId = itemObj["fileId"]?.Type == JTokenType.Integer ? itemObj["fileId"]!.Value<int?>() : null
                        };
                        block.Items.Add(item);
                    }
                }

                // Parse correctOrder - THIS IS THE KEY PART!
                if (data["correctOrder"] is JArray correct)
                {
                    block.CorrectOrder = correct.Select(x => x?.ToString() ?? string.Empty)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();
                }

                break; // Found the block, exit loop
            }
        }
        else
        {
            // Legacy format - parse as old structure
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            var content = JsonConvert.DeserializeObject<OrderingContent>(contentJson, jsonSettings);
            if (content != null && (blockId == "main" || blockId == "legacy"))
            {
                block = new OrderingBlock
                {
                    Id = "main",
                    Items = content.Items ?? new List<OrderingItem>(),
                    CorrectOrder = content.CorrectOrder ?? new List<string>(),
                    Points = content.Points,
                    Instruction = content.Instruction ?? string.Empty
                };
            }
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
        
        // Handle different JSON formats (System.Text.Json JsonElement, Newtonsoft.Json JToken, etc.)
        if (orderValue == null)
        {
            submittedOrder = new List<string>();
        }
        // Handle System.Text.Json JsonElement (ASP.NET Core default)
        else if (orderValue is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                submittedOrder = jsonElement.EnumerateArray()
                    .Select(x => x.GetString() ?? x.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();
            }
            else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var str = jsonElement.GetString() ?? string.Empty;
                if (str.StartsWith("[") && str.EndsWith("]"))
                {
                    try
                    {
                        var deserialized = JsonConvert.DeserializeObject<List<string>>(str) ?? new List<string>();
                        submittedOrder = deserialized
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(x => x.Trim())
                            .ToList();
                    }
                    catch
                    {
                        submittedOrder = !string.IsNullOrWhiteSpace(str) 
                            ? new List<string> { str.Trim() } 
                            : new List<string>();
                    }
                }
                else
                {
                    submittedOrder = !string.IsNullOrWhiteSpace(str) 
                        ? new List<string> { str.Trim() } 
                        : new List<string>();
                }
            }
            else
            {
                throw new ArgumentException($"Invalid format for submitted order. Expected array, got {jsonElement.ValueKind}", nameof(submittedAnswer));
            }
        }
        // Handle Newtonsoft.Json JArray
        else if (orderValue is JArray jArray)
        {
            submittedOrder = jArray.Select(x => x?.ToString() ?? string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();
        }
        // Handle Newtonsoft.Json JToken
        else if (orderValue is JToken jTokenValue && jTokenValue.Type == JTokenType.Array)
        {
            submittedOrder = jTokenValue.Select(x => x?.ToString() ?? string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();
        }
        // Handle List<string> or IEnumerable<string>
        else if (orderValue is List<string> stringList)
        {
            submittedOrder = stringList
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();
        }
        // Handle IEnumerable<object>
        else if (orderValue is IEnumerable<object> enumerable)
        {
            submittedOrder = enumerable.Select(x => x?.ToString() ?? string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();
        }
        // Handle string
        else if (orderValue is string str)
        {
            if (str.StartsWith("[") && str.EndsWith("]"))
            {
                var deserialized = JsonConvert.DeserializeObject<List<string>>(str) ?? new List<string>();
                submittedOrder = deserialized
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();
            }
            else
            {
                submittedOrder = !string.IsNullOrWhiteSpace(str) 
                    ? new List<string> { str.Trim() } 
                    : new List<string>();
            }
        }
        else
        {
            // Try to convert using JSON serialization
            try
            {
                // First try to serialize and deserialize as JSON
                var jsonString = JsonConvert.SerializeObject(orderValue);
                var deserialized = JsonConvert.DeserializeObject<List<string>>(jsonString);
                if (deserialized != null)
                {
                    submittedOrder = deserialized
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .ToList();
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
        
        // Get the correct order from the block (already parsed from JSON)
        var correctOrder = block.CorrectOrder ?? new List<string>();
        
        // If CorrectOrder is empty, try to derive it from Items (fallback)
        if (!correctOrder.Any() && block.Items != null && block.Items.Any())
        {
            // If no explicit CorrectOrder, use the order of items as they appear
            // But this is not ideal - CorrectOrder should be set in the block
            correctOrder = block.Items
                .Where(i => i.Include)
                .Select(i => i.Id)
                .ToList();
        }
        
        var normalizedCorrectOrder = correctOrder
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList();

        // Compare with correct order (already normalized)
        var isCorrect = CompareOrder(submittedOrder, normalizedCorrectOrder);

        // Calculate points
        var maxPoints = block.Points;
        var pointsEarned = isCorrect ? maxPoints : 0;

        // Prepare result with full block context
        var result = new BlockValidationResult
        {
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            MaxPoints = maxPoints,
            CorrectAnswer = new Dictionary<string, object> 
            { 
                { "order", normalizedCorrectOrder }
            },
            SubmittedAnswer = new Dictionary<string, object> 
            { 
                { "order", submittedOrder }
            },
            Feedback = isCorrect ? "عالی! پاسخ شما صحیح است." : "متأسفانه پاسخ شما صحیح نیست. لطفاً دوباره تلاش کنید.",
            DetailedFeedback = new Dictionary<string, object>
            {
                { "submittedOrder", submittedOrder },
                { "correctOrder", normalizedCorrectOrder },
                { "submittedCount", submittedOrder.Count },
                { "correctCount", normalizedCorrectOrder.Count }
            }
        };

        return result;
    }

    private bool CompareOrder(List<string> submitted, List<string> correct)
    {
        if (submitted == null || correct == null)
            return false;

        if (submitted.Count != correct.Count)
            return false;

        // Normalize and compare - trim whitespace
        // Use case-sensitive comparison for IDs (GUIDs and IDs are case-sensitive)
        for (int i = 0; i < submitted.Count; i++)
        {
            var submittedItem = (submitted[i] ?? string.Empty).Trim();
            var correctItem = (correct[i] ?? string.Empty).Trim();
            
            // Compare case-sensitive for IDs
            if (!string.Equals(submittedItem, correctItem, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}

