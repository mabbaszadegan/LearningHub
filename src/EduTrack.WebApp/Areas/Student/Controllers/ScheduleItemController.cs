using EduTrack.Application.Features.StudySessions.Commands;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Domain.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace EduTrack.WebApp.Areas.Student.Controllers;

public class CreateAndCompleteStudySessionRequest
{
    public int ScheduleItemId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset EndedAt { get; set; }
}

public class CompleteStudySessionRequest
{
    public int StudySessionId { get; set; }
}

[Area("Student")]
[Authorize(Roles = "Student")]
public class ScheduleItemController : Controller
{
    private readonly ILogger<ScheduleItemController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public ScheduleItemController(
        ILogger<ScheduleItemController> logger,
        UserManager<User> userManager,
        IMediator mediator)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
    }

    /// <summary>
    /// Display schedule item for study with timer and statistics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Study(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        // Get schedule item details
        var scheduleItemResult = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(id));
        if (!scheduleItemResult.IsSuccess || scheduleItemResult.Value == null)
        {
            TempData["Error"] = "آیتم آموزشی یافت نشد";
            return RedirectToAction("Index", "Home");
        }

        var scheduleItem = scheduleItemResult.Value;

        // Get teaching plan to get course ID
        var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.TeachingPlanId));
        if (!teachingPlanResult.IsSuccess || teachingPlanResult.Value == null)
        {
            TempData["Error"] = "طرح تدریس یافت نشد";
            return RedirectToAction("Index", "Home");
        }

        var teachingPlan = teachingPlanResult.Value;

        // Get study statistics
        var statisticsResult = await _mediator.Send(new GetStudySessionStatisticsQuery(currentUser.Id, id));
        var statistics = statisticsResult.IsSuccess ? statisticsResult.Value : new StudySessionStatisticsDto();

        // Check if there's an active study session
        var activeSessionResult = await _mediator.Send(new GetActiveStudySessionQuery(currentUser.Id, id));
        var activeSession = activeSessionResult.IsSuccess ? activeSessionResult.Value : null;

        // Parse content JSON based on type
        _logger?.LogInformation("Parsing content JSON for ScheduleItem {Id}, Type: {Type}, ContentJson length: {Length}", 
            scheduleItem.Id, scheduleItem.Type, scheduleItem.ContentJson?.Length ?? 0);
        var parsedContent = this.ParseContentJson(scheduleItem.ContentJson ?? string.Empty, scheduleItem.Type);
        _logger?.LogInformation("Parsed content result: {ResultType}", parsedContent?.GetType().Name ?? "null");

        ViewBag.ActiveSession = activeSession;
        ViewBag.CurrentUserId = currentUser.Id;
        ViewBag.CourseId = teachingPlan.CourseId;
        ViewBag.ParsedContent = parsedContent;

        return View(scheduleItem);
    }

    /// <summary>
    /// Start a new study session for schedule item
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StartStudySession(int scheduleItemId)
    {
        try
        {
            _logger.LogInformation("StartStudySession called with ScheduleItemId: {ScheduleItemId}", scheduleItemId);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found");
                return Json(new { success = false, error = "کاربر یافت نشد" });
            }

            _logger.LogInformation("User found: {UserId}", currentUser.Id);

            // For schedule items, we'll use the schedule item ID as educational content ID
            // In a real implementation, you'd need to map schedule items to educational content
            var result = await _mediator.Send(new StartStudySessionCommand(currentUser.Id, scheduleItemId));
            if (!result.IsSuccess)
            {
                _logger.LogError("StartStudySessionCommand failed: {Error}", result.Error);
                return Json(new { success = false, error = result.Error });
            }

            _logger.LogInformation("Study session started successfully with ID: {SessionId}", result.Value!.Id);
            return Json(new { success = true, sessionId = result.Value!.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StartStudySession");
            return Json(new { success = false, error = "خطا در شروع جلسه مطالعه" });
        }
    }

    /// <summary>
    /// Get study session statistics for a schedule item
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStudyStatistics(int scheduleItemId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, error = "کاربر یافت نشد" });
        }

        // For schedule items, we'll use the schedule item ID as educational content ID
        var result = await _mediator.Send(new GetStudySessionStatisticsQuery(currentUser.Id, scheduleItemId));
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, statistics = result.Value });
    }

    /// <summary>
    /// Create and complete a study session in one operation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAndCompleteStudySession([FromBody] CreateAndCompleteStudySessionRequest request)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, error = "کاربر یافت نشد" });
            }

            var result = await _mediator.Send(new CreateAndCompleteStudySessionCommand(
                currentUser.Id,
                request.ScheduleItemId,
                request.StartedAt,
                request.EndedAt
            ));

            if (!result.IsSuccess)
            {
                return Json(new { success = false, error = result.Error });
            }

            return Json(new { success = true, sessionId = result.Value!.Id });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in CreateAndCompleteStudySession");
            return Json(new { success = false, error = "خطا در ثبت جلسه مطالعه" });
        }
    }

    private object? ParseContentJson(string contentJson, ScheduleItemType type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentJson))
            {
                return null;
            }

            // Parse JSON to JObject first to check structure
            var jsonObject = JObject.Parse(contentJson);
            
            // Check if JSON has the new structure with blocks (supports both { type: "...", blocks: [...] } and { itemType: "...", blocks: [...] })
            if (jsonObject["blocks"] != null)
            {
                return ParseNewStructure(jsonObject, type);
            }
            
            // Otherwise, try to parse as old structure (direct content object)
            return ParseOldStructure(contentJson, type);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error parsing content JSON for type {Type}. JSON: {ContentJson}", type, contentJson?.Substring(0, Math.Min(200, contentJson?.Length ?? 0)));
            return null;
        }
    }

    private object? ParseNewStructure(JObject jsonObject, ScheduleItemType type)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        var blocksArray = jsonObject["blocks"] as JArray;
        if (blocksArray == null)
        {
            return null;
        }

        return type switch
        {
            ScheduleItemType.Reminder => ParseReminderFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.Writing => ParseWritingFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.Audio => ParseAudioFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.GapFill => ParseGapFillFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.MultipleChoice => ParseMultipleChoiceFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.Match => ParseMatchFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.ErrorFinding => ParseErrorFindingFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.CodeExercise => ParseCodeExerciseFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.Quiz => ParseQuizFromBlocks(blocksArray, jsonSettings),
            ScheduleItemType.Ordering => ParseOrderingFromBlocks(blocksArray, jsonSettings),
            _ => null
        };
    }

    private object? ParseOldStructure(string contentJson, ScheduleItemType type)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        
        object? result = type switch
        {
            ScheduleItemType.Reminder => JsonConvert.DeserializeObject<Application.Common.Models.ScheduleItems.ReminderContent>(contentJson, jsonSettings),
            ScheduleItemType.Writing => JsonConvert.DeserializeObject<Application.Common.Models.ScheduleItems.WritingContent>(contentJson, jsonSettings),
            ScheduleItemType.Audio => JsonConvert.DeserializeObject<Application.Common.Models.ScheduleItems.AudioContent>(contentJson, jsonSettings),
            ScheduleItemType.GapFill => JsonConvert.DeserializeObject<GapFillContent>(contentJson, jsonSettings),
            ScheduleItemType.MultipleChoice => JsonConvert.DeserializeObject<MultipleChoiceContent>(contentJson, jsonSettings),
            ScheduleItemType.Match => JsonConvert.DeserializeObject<MatchingContent>(contentJson, jsonSettings),
            ScheduleItemType.ErrorFinding => JsonConvert.DeserializeObject<ErrorFindingContent>(contentJson, jsonSettings),
            ScheduleItemType.CodeExercise => JsonConvert.DeserializeObject<CodeExerciseContent>(contentJson, jsonSettings),
            ScheduleItemType.Quiz => JsonConvert.DeserializeObject<QuizContent>(contentJson, jsonSettings),
            ScheduleItemType.Ordering => JsonConvert.DeserializeObject<OrderingContent>(contentJson, jsonSettings),
            _ => null
        };
        
        // If CamelCase deserialization failed, try with default settings (PascalCase)
        if (result == null)
        {
            var defaultSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            
            result = type switch
            {
                ScheduleItemType.Reminder => JsonConvert.DeserializeObject<Application.Common.Models.ScheduleItems.ReminderContent>(contentJson, defaultSettings),
                ScheduleItemType.Writing => JsonConvert.DeserializeObject<Application.Common.Models.ScheduleItems.WritingContent>(contentJson, defaultSettings),
                ScheduleItemType.Audio => JsonConvert.DeserializeObject<Application.Common.Models.ScheduleItems.AudioContent>(contentJson, defaultSettings),
                ScheduleItemType.GapFill => JsonConvert.DeserializeObject<GapFillContent>(contentJson, defaultSettings),
                ScheduleItemType.MultipleChoice => JsonConvert.DeserializeObject<MultipleChoiceContent>(contentJson, defaultSettings),
                ScheduleItemType.Match => JsonConvert.DeserializeObject<MatchingContent>(contentJson, defaultSettings),
                ScheduleItemType.ErrorFinding => JsonConvert.DeserializeObject<ErrorFindingContent>(contentJson, defaultSettings),
                ScheduleItemType.CodeExercise => JsonConvert.DeserializeObject<CodeExerciseContent>(contentJson, defaultSettings),
                ScheduleItemType.Quiz => JsonConvert.DeserializeObject<QuizContent>(contentJson, defaultSettings),
                ScheduleItemType.Ordering => JsonConvert.DeserializeObject<OrderingContent>(contentJson, defaultSettings),
                _ => null
            };
        }
        
        return result;
    }

    private Application.Common.Models.ScheduleItems.ReminderContent ParseReminderFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        var reminder = new Application.Common.Models.ScheduleItems.ReminderContent();
        var contentBlocks = new List<ContentBlock>();
        var questionBlocks = new List<ReminderQuestionBlock>();

        foreach (var blockToken in blocksArray)
        {
            var block = blockToken as JObject;
            if (block == null) continue;

            var blockType = block["type"]?.ToString().ToLower();
            var order = block["order"]?.Value<int>() ?? 0;

            // Check if it's a question block (type starts with "question")
            if (blockType != null && blockType.StartsWith("question"))
            {
                var questionBlock = ParseReminderQuestionBlock(block, order, settings);
                if (questionBlock != null)
                {
                    questionBlocks.Add(questionBlock);
                }
            }
            else
            {
                // Regular content block
                var contentBlock = ParseContentBlock(block, order, settings);
                if (contentBlock != null)
                {
                    contentBlocks.Add(contentBlock);
                }
            }
        }

        reminder.Blocks = contentBlocks.OrderBy(b => b.Order).ToList();
        reminder.QuestionBlocks = questionBlocks.OrderBy(b => b.Order).ToList();
        return reminder;
    }

    private ContentBlock? ParseContentBlock(JObject block, int order, JsonSerializerSettings settings)
    {
        try
        {
            var blockTypeStr = block["type"]?.ToString();
            if (string.IsNullOrEmpty(blockTypeStr)) return null;

            // Map string type to enum
            var blockType = blockTypeStr.ToLower() switch
            {
                "text" => ContentBlockType.Text,
                "image" => ContentBlockType.Image,
                "video" => ContentBlockType.Video,
                "audio" => ContentBlockType.Audio,
                "code" => ContentBlockType.Code,
                _ => ContentBlockType.Text
            };

            var data = block["data"] as JObject;
            if (data == null) return null;

            var contentBlock = new ContentBlock
            {
                Id = block["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                Type = blockType,
                Order = order,
                Data = JsonConvert.DeserializeObject<ContentBlockData>(data.ToString(), settings) ?? new ContentBlockData()
            };

            return contentBlock;
        }
        catch
        {
            return null;
        }
    }

    private ReminderQuestionBlock? ParseReminderQuestionBlock(JObject block, int order, JsonSerializerSettings settings)
    {
        try
        {
            var questionTypeStr = block["type"]?.ToString().ToLower();
            if (string.IsNullOrEmpty(questionTypeStr)) return null;

            // Extract question type from "questionText", "questionImage", etc.
            var questionType = questionTypeStr.Replace("question", "") switch
            {
                "text" => ReminderQuestionType.Text,
                "image" => ReminderQuestionType.Image,
                "video" => ReminderQuestionType.Video,
                "audio" => ReminderQuestionType.Audio,
                _ => ReminderQuestionType.Text
            };

            var data = block["data"] as JObject;
            if (data == null) return null;

            var questionBlock = new ReminderQuestionBlock
            {
                Id = block["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                Order = order,
                QuestionType = questionType,
                QuestionData = JsonConvert.DeserializeObject<ReminderQuestionData>(data.ToString(), settings) ?? new ReminderQuestionData(),
                Points = data["points"]?.Value<decimal>() ?? 1,
                IsRequired = data["isRequired"]?.Value<bool>() ?? true,
                TeacherGuidance = data["teacherGuidance"]?.ToString()
            };

            return questionBlock;
        }
        catch
        {
            return null;
        }
    }

    private Application.Common.Models.ScheduleItems.WritingContent ParseWritingFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        // For Writing, blocks can be content blocks and question blocks
        var writing = new Application.Common.Models.ScheduleItems.WritingContent();
        var contentBlocks = new List<ContentBlock>();
        var questionBlocks = new List<ReminderQuestionBlock>();

        foreach (var blockToken in blocksArray)
        {
            var block = blockToken as JObject;
            if (block == null) continue;

            var blockType = block["type"]?.ToString().ToLower();
            var order = block["order"]?.Value<int>() ?? 0;

            // Check if it's a question block (type starts with "question")
            if (blockType != null && blockType.StartsWith("question"))
            {
                var questionBlock = ParseReminderQuestionBlock(block, order, settings);
                if (questionBlock != null)
                {
                    questionBlocks.Add(questionBlock);
                    
                    // Extract prompt from first question block if not set
                    if (string.IsNullOrEmpty(writing.Prompt))
                    {
                        var data = block["data"] as JObject;
                        if (data != null)
                        {
                            writing.Prompt = data["textContent"]?.ToString() ?? 
                                            data["content"]?.ToString() ?? 
                                            data["questionText"]?.ToString() ?? "";
                            writing.Instructions = data["instructions"]?.ToString() ?? 
                                                  data["hint"]?.ToString() ?? "";
                            writing.WordLimit = data["wordLimit"]?.Value<int>() ?? 0;
                            if (data["keywords"] is JArray keywords)
                            {
                                writing.Keywords = keywords.Select(k => k.ToString()).ToList();
                            }
                        }
                    }
                }
            }
            else
            {
                // Regular content block
                var contentBlock = ParseContentBlock(block, order, settings);
                if (contentBlock != null)
                {
                    contentBlocks.Add(contentBlock);
                }
            }
        }

        writing.Blocks = contentBlocks.OrderBy(b => b.Order).ToList();
        writing.QuestionBlocks = questionBlocks.OrderBy(b => b.Order).ToList();
        
        return writing;
    }

    private Application.Common.Models.ScheduleItems.AudioContent ParseAudioFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        var audio = new Application.Common.Models.ScheduleItems.AudioContent();
        var contentBlocks = new List<ContentBlock>();
        var questionBlocks = new List<ReminderQuestionBlock>();

        foreach (var blockToken in blocksArray)
        {
            var block = blockToken as JObject;
            if (block == null) continue;

            var blockType = block["type"]?.ToString().ToLower();
            var order = block["order"]?.Value<int>() ?? 0;

            // Check if it's a question block (type starts with "question")
            if (blockType != null && blockType.StartsWith("question"))
            {
                var questionBlock = ParseReminderQuestionBlock(block, order, settings);
                if (questionBlock != null)
                {
                    questionBlocks.Add(questionBlock);
                }
            }
            else
            {
                // Regular content block
                var contentBlock = ParseContentBlock(block, order, settings);
                if (contentBlock != null)
                {
                    contentBlocks.Add(contentBlock);
                    
                    // Extract audio-specific data from audio blocks
                    if (blockType == "audio")
                    {
                        var data = block["data"] as JObject;
                        if (data != null)
                        {
                            audio.Instruction = data["instruction"]?.ToString() ?? 
                                              data["textContent"]?.ToString() ?? 
                                              audio.Instruction;
                            audio.AudioUrl = data["fileUrl"]?.ToString() ?? audio.AudioUrl;
                            audio.DurationSeconds = data["duration"]?.Value<int>() ?? audio.DurationSeconds;
                            audio.AllowRecording = data["allowRecording"]?.Value<bool>() ?? audio.AllowRecording;
                            audio.RecordingInstructions = data["recordingInstructions"]?.ToString() ?? audio.RecordingInstructions;
                        }
                    }
                }
            }
        }

        audio.Blocks = contentBlocks.OrderBy(b => b.Order).ToList();
        audio.QuestionBlocks = questionBlocks.OrderBy(b => b.Order).ToList();
        
        return audio;
    }

    private GapFillContent ParseGapFillFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        return JsonConvert.DeserializeObject<GapFillContent>("{}", settings) ?? new GapFillContent();
    }

    private MultipleChoiceContent ParseMultipleChoiceFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        return JsonConvert.DeserializeObject<MultipleChoiceContent>("{}", settings) ?? new MultipleChoiceContent();
    }

    private MatchingContent ParseMatchFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        return JsonConvert.DeserializeObject<MatchingContent>("{}", settings) ?? new MatchingContent();
    }

    private ErrorFindingContent ParseErrorFindingFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        return JsonConvert.DeserializeObject<ErrorFindingContent>("{}", settings) ?? new ErrorFindingContent();
    }

    private CodeExerciseContent ParseCodeExerciseFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        return JsonConvert.DeserializeObject<CodeExerciseContent>("{}", settings) ?? new CodeExerciseContent();
    }

    private QuizContent ParseQuizFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        return JsonConvert.DeserializeObject<QuizContent>("{}", settings) ?? new QuizContent();
    }

    private OrderingContent ParseOrderingFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        var ordering = new OrderingContent();
        var orderingBlocks = new List<OrderingBlock>();

        foreach (var blockToken in blocksArray)
        {
            var block = blockToken as JObject;
            if (block == null) continue;

            var blockType = block["type"]?.ToString().ToLower();
            if (!string.Equals(blockType, "ordering", StringComparison.OrdinalIgnoreCase))
                continue;

            var order = block["order"]?.Value<int>() ?? 0;
            var data = block["data"] as JObject;
            if (data == null) continue;

            var orderingBlock = new OrderingBlock
            {
                Id = block["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                Order = order,
                Instruction = data["instruction"]?.ToString() ?? string.Empty,
                AllowDragDrop = data["allowDragDrop"]?.Value<bool>() ?? true,
                Direction = data["direction"]?.ToString() ?? "vertical",
                ShowNumbers = data["showNumbers"]?.Value<bool>() ?? true,
                IsRequired = data["isRequired"]?.Value<bool>() ?? true
            };

            // points stored as string in builder sometimes
            var pointsToken = data["points"];
            if (pointsToken != null)
            {
                if (pointsToken.Type == JTokenType.Integer || pointsToken.Type == JTokenType.Float)
                {
                    orderingBlock.Points = pointsToken.Value<decimal>();
                }
                else if (pointsToken.Type == JTokenType.String && decimal.TryParse(pointsToken.ToString(), out var pts))
                {
                    orderingBlock.Points = pts;
                }
            }

            // items
            if (data["items"] is JArray items)
            {
                foreach (var t in items)
                {
                    if (t is not JObject obj) continue;
                    var item = new OrderingItem
                    {
                        Id = obj["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = obj["type"]?.ToString() ?? "text",
                        Include = obj["include"]?.Value<bool>() ?? true,
                        Value = obj["value"]?.ToString(),
                        FileUrl = obj["fileUrl"]?.ToString(),
                        FileName = obj["fileName"]?.ToString(),
                        MimeType = obj["mimeType"]?.ToString(),
                        FileId = obj["fileId"]?.Type == JTokenType.Integer ? obj["fileId"]!.Value<int?>() : null
                    };
                    orderingBlock.Items.Add(item);
                }
            }

            // correct order
            if (data["correctOrder"] is JArray correct)
            {
                orderingBlock.CorrectOrder = correct.Select(x => x?.ToString() ?? string.Empty).Where(x => !string.IsNullOrEmpty(x)).ToList();
            }

            orderingBlocks.Add(orderingBlock);
        }

        ordering.Blocks = orderingBlocks.OrderBy(b => b.Order).ToList();

        // Backward compatibility: if no blocks but we have items, use first block as main content
        if (orderingBlocks.Count == 0)
        {
            var first = blocksArray.FirstOrDefault(b => string.Equals(b?["type"]?.ToString(), "ordering", StringComparison.OrdinalIgnoreCase)) as JObject
                        ?? blocksArray.FirstOrDefault() as JObject;
            if (first != null)
            {
                var data = first["data"] as JObject;
                if (data != null)
                {
                    ordering.Instruction = data["instruction"]?.ToString() ?? string.Empty;
                    ordering.AllowDragDrop = data["allowDragDrop"]?.Value<bool>() ?? true;
                    ordering.Direction = data["direction"]?.ToString() ?? "vertical";
                    ordering.ShowNumbers = data["showNumbers"]?.Value<bool>() ?? true;
                    ordering.IsRequired = data["isRequired"]?.Value<bool>() ?? true;

                    var pointsToken = data["points"];
                    if (pointsToken != null)
                    {
                        if (pointsToken.Type == JTokenType.Integer || pointsToken.Type == JTokenType.Float)
                        {
                            ordering.Points = pointsToken.Value<decimal>();
                        }
                        else if (pointsToken.Type == JTokenType.String && decimal.TryParse(pointsToken.ToString(), out var pts))
                        {
                            ordering.Points = pts;
                        }
                    }

                    if (data["items"] is JArray items)
                    {
                        foreach (var t in items)
                        {
                            if (t is not JObject obj) continue;
                            var item = new OrderingItem
                            {
                                Id = obj["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                                Type = obj["type"]?.ToString() ?? "text",
                                Include = obj["include"]?.Value<bool>() ?? true,
                                Value = obj["value"]?.ToString(),
                                FileUrl = obj["fileUrl"]?.ToString(),
                                FileName = obj["fileName"]?.ToString(),
                                MimeType = obj["mimeType"]?.ToString(),
                                FileId = obj["fileId"]?.Type == JTokenType.Integer ? obj["fileId"]!.Value<int?>() : null
                            };
                            ordering.Items.Add(item);
                        }
                    }

                    if (data["correctOrder"] is JArray correct)
                    {
                        ordering.CorrectOrder = correct.Select(x => x?.ToString() ?? string.Empty).Where(x => !string.IsNullOrEmpty(x)).ToList();
                    }
                }
            }
        }
        else if (orderingBlocks.Count > 0)
        {
            // Use first block's properties for backward compatibility
            var firstBlock = orderingBlocks.OrderBy(b => b.Order).First();
            ordering.Instruction = firstBlock.Instruction;
            ordering.AllowDragDrop = firstBlock.AllowDragDrop;
            ordering.Direction = firstBlock.Direction;
            ordering.ShowNumbers = firstBlock.ShowNumbers;
            ordering.Points = firstBlock.Points;
            ordering.IsRequired = firstBlock.IsRequired;
            ordering.Items = firstBlock.Items;
            ordering.CorrectOrder = firstBlock.CorrectOrder;
        }

        return ordering;
    }
}
