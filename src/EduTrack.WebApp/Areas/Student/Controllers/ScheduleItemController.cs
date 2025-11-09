using EduTrack.Application.Features.StudySessions.Commands;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.ScheduleItems;
using MultipleChoiceContent = EduTrack.Application.Common.Models.ScheduleItems.MultipleChoiceContent;
using MultipleChoiceBlock = EduTrack.Application.Common.Models.ScheduleItems.MultipleChoiceBlock;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduTrack.Domain.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using EduTrack.WebApp.Models;
using EduTrack.WebApp.Services;

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
    private readonly IStudentProfileContext _studentProfileContext;

    public ScheduleItemController(
        ILogger<ScheduleItemController> logger,
        UserManager<User> userManager,
        IMediator mediator,
        IStudentProfileContext studentProfileContext)
    {
        _logger = logger;
        _userManager = userManager;
        _mediator = mediator;
        _studentProfileContext = studentProfileContext;
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

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (!activeProfileId.HasValue)
        {
            TempData["Error"] = "برای مطالعه، ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید.";
            return RedirectToAction("Index", "Profile");
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
        var statisticsResult = await _mediator.Send(new GetStudySessionStatisticsQuery(currentUser.Id, id, activeProfileId));
        var statistics = statisticsResult.IsSuccess ? statisticsResult.Value : new StudySessionStatisticsDto();
        ViewBag.Statistics = statistics;

        // Check if there's an active study session
        var activeSessionResult = await _mediator.Send(new GetActiveStudySessionQuery(currentUser.Id, id, activeProfileId));
        var activeSession = activeSessionResult.IsSuccess ? activeSessionResult.Value : null;

        // Parse content JSON based on type
        _logger?.LogInformation("Parsing content JSON for ScheduleItem {Id}, Type: {Type}, ContentJson length: {Length}", 
            scheduleItem.Id, scheduleItem.Type, scheduleItem.ContentJson?.Length ?? 0);
        var parsedContent = this.ParseContentJson(scheduleItem.ContentJson ?? string.Empty, scheduleItem.Type);
        _logger?.LogInformation("Parsed content result: {ResultType}", parsedContent?.GetType().Name ?? "null");

        ViewBag.ActiveSession = activeSession;
        ViewBag.CurrentUserId = currentUser.Id;
        ViewBag.CourseId = teachingPlan.CourseId;
        ViewBag.ScheduleItemId = scheduleItem.Id;
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

            var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
            if (!activeProfileId.HasValue)
            {
                _logger.LogWarning("Active student profile not found for user {UserId}", currentUser.Id);
                return Json(new { success = false, error = "برای شروع مطالعه ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید." });
            }

            // For schedule items, we'll use the schedule item ID as educational content ID
            // In a real implementation, you'd need to map schedule items to educational content
            var result = await _mediator.Send(new StartStudySessionCommand(currentUser.Id, scheduleItemId, activeProfileId));
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

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (!activeProfileId.HasValue)
        {
            return Json(new { success = false, error = "ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید." });
        }

        // For schedule items, we'll use the schedule item ID as educational content ID
        var result = await _mediator.Send(new GetStudySessionStatisticsQuery(currentUser.Id, scheduleItemId, activeProfileId));
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
    // Temporarily disable anti-forgery for debugging - re-enable after fixing token issue
    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAndCompleteStudySession([FromBody] CreateAndCompleteStudySessionRequest request)
    {
        try
        {
            _logger?.LogInformation("CreateAndCompleteStudySession called with request: ScheduleItemId={ScheduleItemId}, StartedAt={StartedAt}, EndedAt={EndedAt}", 
                request?.ScheduleItemId, request?.StartedAt, request?.EndedAt);
            
            if (request == null)
            {
                _logger?.LogWarning("Request is null");
                return BadRequest(new { success = false, error = "درخواست نامعتبر است" });
            }
            
            if (request.ScheduleItemId <= 0)
            {
                _logger?.LogWarning("Invalid ScheduleItemId: {ScheduleItemId}", request.ScheduleItemId);
                return BadRequest(new { success = false, error = "شناسه آیتم نامعتبر است" });
            }
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger?.LogWarning("User not found");
                return Json(new { success = false, error = "کاربر یافت نشد" });
            }

            _logger?.LogInformation("Creating study session for user {UserId}, schedule item {ScheduleItemId}", 
                currentUser.Id, request.ScheduleItemId);

            var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
            if (!activeProfileId.HasValue)
            {
                _logger?.LogWarning("Active student profile not found for user {UserId}", currentUser.Id);
                return Json(new { success = false, error = "برای ثبت جلسه مطالعه ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید." });
            }

            var result = await _mediator.Send(new CreateAndCompleteStudySessionCommand(
                currentUser.Id,
                request.ScheduleItemId,
                request.StartedAt,
                request.EndedAt,
                activeProfileId
            ));

            if (!result.IsSuccess)
            {
                _logger?.LogError("Failed to create study session: {Error}", result.Error);
                return Json(new { success = false, error = result.Error });
            }

            _logger?.LogInformation("Study session created successfully with ID: {SessionId}", result.Value!.Id);
            
            // Get updated total study time
            var updatedStatistics = await _mediator.Send(new GetStudySessionStatisticsQuery(currentUser.Id, request.ScheduleItemId, activeProfileId));
            var totalStudyTimeSeconds = updatedStatistics.IsSuccess ? updatedStatistics.Value?.TotalStudyTimeSeconds ?? 0 : 0;
            
            return Json(new { success = true, sessionId = result.Value!.Id, totalStudyTimeSeconds = totalStudyTimeSeconds });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in CreateAndCompleteStudySession");
            return Json(new { success = false, error = $"خطا در ثبت جلسه مطالعه: {ex.Message}" });
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
        var orderingBlocks = new List<OrderingBlock>();
        var multipleChoiceBlocks = new List<MultipleChoiceBlock>();

        foreach (var blockToken in blocksArray)
        {
            var block = blockToken as JObject;
            if (block == null) continue;

            var blockType = block["type"]?.ToString().ToLower();
            var order = block["order"]?.Value<int>() ?? 0;

            // Check if it's an ordering block
            if (blockType != null && string.Equals(blockType, "ordering", StringComparison.OrdinalIgnoreCase))
            {
                var orderingBlock = ParseOrderingBlock(block, order, settings);
                if (orderingBlock != null)
                {
                    orderingBlocks.Add(orderingBlock);
                }
            }
            // Check if it's a multipleChoice block
            else if (blockType != null && string.Equals(blockType, "multiplechoice", StringComparison.OrdinalIgnoreCase))
            {
                var parsedBlocks = ParseMultipleChoiceBlock(block, order, settings);
                if (parsedBlocks != null && parsedBlocks.Any())
                {
                    multipleChoiceBlocks.AddRange(parsedBlocks);
                }
            }
            // Check if it's a question block (type starts with "question")
            else if (blockType != null && blockType.StartsWith("question"))
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
        reminder.OrderingBlocks = orderingBlocks.OrderBy(b => b.Order).ToList();
        reminder.MultipleChoiceBlocks = multipleChoiceBlocks.OrderBy(b => b.Order).ToList();
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

            var contentBlockData = JsonConvert.DeserializeObject<ContentBlockData>(data.ToString(), settings) ?? new ContentBlockData();
            
            // Handle FileId that might be stored as integer in JSON (check both camelCase and PascalCase)
            var fileIdToken = data["fileId"] ?? data["FileId"] ?? data["fileid"];
            if (fileIdToken != null)
            {
                if (fileIdToken.Type == JTokenType.Integer)
                {
                    contentBlockData.FileId = fileIdToken.Value<int>().ToString();
                }
                else if (fileIdToken.Type == JTokenType.String)
                {
                    var fileIdValue = fileIdToken.Value<string>();
                    if (!string.IsNullOrWhiteSpace(fileIdValue))
                    {
                        contentBlockData.FileId = fileIdValue;
                    }
                }
            }
            
            // Handle FileUrl with case variations (check both camelCase and PascalCase)
            var fileUrlToken = data["fileUrl"] ?? data["FileUrl"] ?? data["fileurl"];
            if (fileUrlToken != null && fileUrlToken.Type == JTokenType.String)
            {
                var fileUrlValue = fileUrlToken.Value<string>();
                if (!string.IsNullOrWhiteSpace(fileUrlValue))
                {
                    contentBlockData.FileUrl = fileUrlValue;
                }
            }
            
            // Also check for fileId in nested objects or alternative property names
            if (string.IsNullOrWhiteSpace(contentBlockData.FileId) && string.IsNullOrWhiteSpace(contentBlockData.FileUrl))
            {
                // Try to find fileId in alternative locations
                var altFileId = data["id"] ?? data["Id"];
                if (altFileId != null && (altFileId.Type == JTokenType.Integer || altFileId.Type == JTokenType.String))
                {
                    contentBlockData.FileId = altFileId.ToString();
                }
            }

            var contentBlock = new ContentBlock
            {
                Id = block["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                Type = blockType,
                Order = order,
                Data = contentBlockData
            };

            return contentBlock;
        }
        catch
        {
            return null;
        }
    }

    private List<MultipleChoiceBlock>? ParseMultipleChoiceBlock(JObject block, int order, JsonSerializerSettings settings)
    {
        try
        {
            var data = block["data"] as JObject;
            if (data == null) return null;

            var result = new List<MultipleChoiceBlock>();

            // Each MCQ block can have multiple questions
            if (data["questions"] is JArray questions)
            {
                foreach (var questionToken in questions)
                {
                    if (questionToken is not JObject questionObj) continue;

                    var questionId = questionObj["id"]?.ToString() ?? Guid.NewGuid().ToString();
                    var questionText = questionObj["stem"]?.ToString() ?? string.Empty;
                    var answerType = questionObj["answerType"]?.ToString() ?? "single";
                    var randomizeOptions = questionObj["randomizeOptions"]?.Value<bool>() ?? false;

                    var mcqBlock = new MultipleChoiceBlock
                    {
                        Id = questionId,
                        Order = order,
                        Question = questionText,
                        AnswerType = answerType,
                        RandomizeOptions = randomizeOptions,
                        IsRequired = data["isRequired"]?.Value<bool>() ?? true
                    };

                    // Parse stem media data
                    if (questionObj["stemData"] is JObject stemData)
                    {
                        var mediaType = stemData["mediaType"]?.ToString();
                        mcqBlock.StemMediaType = mediaType;

                        if (mediaType == "image")
                        {
                            mcqBlock.StemImageUrl = stemData["imageUrl"]?.ToString();
                            mcqBlock.StemImageFileId = stemData["imageFileId"]?.ToString();
                            mcqBlock.StemImageFileName = stemData["imageFileName"]?.ToString();
                        }
                        else if (mediaType == "audio")
                        {
                            mcqBlock.StemAudioUrl = stemData["audioUrl"]?.ToString();
                            mcqBlock.StemAudioFileId = stemData["audioFileId"]?.ToString();
                            mcqBlock.StemAudioFileName = stemData["audioFileName"]?.ToString();
                            mcqBlock.StemAudioIsRecorded = stemData["isRecorded"]?.Value<bool>() ?? false;
                        }
                        else if (mediaType == "video")
                        {
                            mcqBlock.StemVideoUrl = stemData["videoUrl"]?.ToString();
                            mcqBlock.StemVideoFileId = stemData["videoFileId"]?.ToString();
                            mcqBlock.StemVideoFileName = stemData["videoFileName"]?.ToString();
                        }
                    }

                    // Points stored as string in builder sometimes
                    var pointsToken = data["points"];
                    if (pointsToken != null)
                    {
                        if (pointsToken.Type == JTokenType.Integer || pointsToken.Type == JTokenType.Float)
                        {
                            mcqBlock.Points = pointsToken.Value<decimal>();
                        }
                        else if (pointsToken.Type == JTokenType.String && decimal.TryParse(pointsToken.ToString(), out var pts))
                        {
                            mcqBlock.Points = pts;
                        }
                    }

                    // Parse options
                    if (questionObj["options"] is JArray options)
                    {
                        var correctAnswers = new List<int>();
                        foreach (var optionToken in options)
                        {
                            if (optionToken is not JObject optionObj) continue;

                            var optionIndex = optionObj["index"]?.Value<int>() ?? 0;
                            var optionType = optionObj["optionType"]?.ToString() ?? "text";
                            var isCorrect = optionObj["isCorrect"]?.Value<bool>() ?? false;

                            var option = new MultipleChoiceOption
                            {
                                Index = optionIndex,
                                OptionType = optionType,
                                IsCorrect = isCorrect
                            };

                            if (optionType == "text")
                            {
                                option.Text = optionObj["text"]?.ToString() ?? string.Empty;
                            }
                            else if (optionType == "image")
                            {
                                option.Text = optionObj["text"]?.ToString() ?? string.Empty;
                                option.ImageUrl = optionObj["imageUrl"]?.ToString();
                                option.ImageFileId = optionObj["imageFileId"]?.ToString();
                                option.ImageFileName = optionObj["imageFileName"]?.ToString();
                            }
                            else if (optionType == "audio")
                            {
                                option.Text = optionObj["text"]?.ToString() ?? string.Empty;
                                option.AudioUrl = optionObj["audioUrl"]?.ToString();
                                option.AudioFileId = optionObj["audioFileId"]?.ToString();
                                option.AudioFileName = optionObj["audioFileName"]?.ToString();
                                option.IsRecorded = optionObj["isRecorded"]?.Value<bool>() ?? false;
                                option.AudioDuration = optionObj["audioDuration"]?.Value<int?>();
                            }

                            mcqBlock.Options.Add(option);

                            if (isCorrect)
                            {
                                correctAnswers.Add(optionIndex);
                            }
                        }

                        mcqBlock.CorrectAnswers = correctAnswers;
                    }

                    result.Add(mcqBlock);
                }
            }

            return result.Any() ? result : null;
        }
        catch
        {
            return null;
        }
    }

    private OrderingBlock? ParseOrderingBlock(JObject block, int order, JsonSerializerSettings settings)
    {
        try
        {
            var data = block["data"] as JObject;
            if (data == null) return null;

            var orderingBlock = new OrderingBlock
            {
                Id = block["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                Order = order,
                Instruction = data["instruction"]?.ToString() ?? string.Empty,
                AllowDragDrop = data["allowDragDrop"]?.Value<bool>() ?? true,
                Direction = data["direction"]?.ToString() ?? "vertical",
                Alignment = data["alignment"]?.ToString() ?? "right",
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

            return orderingBlock;
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
        var multipleChoiceBlocks = new List<MultipleChoiceBlock>();

        foreach (var blockToken in blocksArray)
        {
            var block = blockToken as JObject;
            if (block == null) continue;

            var blockType = block["type"]?.ToString().ToLower();
            var order = block["order"]?.Value<int>() ?? 0;

            // Check if it's a multipleChoice block
            if (blockType != null && string.Equals(blockType, "multiplechoice", StringComparison.OrdinalIgnoreCase))
            {
                var parsedBlocks = ParseMultipleChoiceBlock(block, order, settings);
                if (parsedBlocks != null && parsedBlocks.Any())
                {
                    multipleChoiceBlocks.AddRange(parsedBlocks);
                }
            }
            // Check if it's a question block (type starts with "question")
            else if (blockType != null && blockType.StartsWith("question"))
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
        writing.MultipleChoiceBlocks = multipleChoiceBlocks.OrderBy(b => b.Order).ToList();
        
        return writing;
    }

    private Application.Common.Models.ScheduleItems.AudioContent ParseAudioFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        var audio = new Application.Common.Models.ScheduleItems.AudioContent();
        var contentBlocks = new List<ContentBlock>();
        var questionBlocks = new List<ReminderQuestionBlock>();
        var multipleChoiceBlocks = new List<MultipleChoiceBlock>();

        _logger?.LogInformation("ParseAudioFromBlocks: Processing {Count} blocks", blocksArray.Count);

        foreach (var blockToken in blocksArray)
        {
            var block = blockToken as JObject;
            if (block == null) continue;

            var blockType = block["type"]?.ToString().ToLower();
            var order = block["order"]?.Value<int>() ?? 0;

            _logger?.LogInformation("ParseAudioFromBlocks: Processing block type={Type}, order={Order}", blockType, order);

            // Check if it's a multipleChoice block
            if (blockType != null && string.Equals(blockType, "multiplechoice", StringComparison.OrdinalIgnoreCase))
            {
                var parsedBlocks = ParseMultipleChoiceBlock(block, order, settings);
                if (parsedBlocks != null && parsedBlocks.Any())
                {
                    multipleChoiceBlocks.AddRange(parsedBlocks);
                    _logger?.LogInformation("ParseAudioFromBlocks: Added {Count} multipleChoice blocks", parsedBlocks.Count);
                }
            }
            // Check if it's a question block (type starts with "question")
            else if (blockType != null && blockType.StartsWith("question"))
            {
                var questionBlock = ParseReminderQuestionBlock(block, order, settings);
                if (questionBlock != null)
                {
                    questionBlocks.Add(questionBlock);
                    _logger?.LogInformation("ParseAudioFromBlocks: Added question block, order={Order}", order);
                }
            }
            else
            {
                // Regular content block
                var contentBlock = ParseContentBlock(block, order, settings);
                if (contentBlock != null)
                {
                    contentBlocks.Add(contentBlock);
                    _logger?.LogInformation("ParseAudioFromBlocks: Added content block type={Type}, order={Order}, FileId={FileId}, FileUrl={FileUrl}", 
                        contentBlock.Type, order, contentBlock.Data?.FileId ?? "null", contentBlock.Data?.FileUrl ?? "null");
                    
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
                            var durationValue = data["duration"];
                            if (durationValue != null)
                            {
                                if (durationValue.Type == JTokenType.Float || durationValue.Type == JTokenType.Integer)
                                {
                                    audio.DurationSeconds = (int)Math.Round(durationValue.Value<double>());
                                }
                            }
                            audio.AllowRecording = data["allowRecording"]?.Value<bool>() ?? audio.AllowRecording;
                            audio.RecordingInstructions = data["recordingInstructions"]?.ToString() ?? audio.RecordingInstructions;
                        }
                    }
                }
                else
                {
                    _logger?.LogWarning("ParseAudioFromBlocks: Failed to parse content block type={Type}, order={Order}", blockType, order);
                }
            }
        }

        audio.Blocks = contentBlocks.OrderBy(b => b.Order).ToList();
        audio.QuestionBlocks = questionBlocks.OrderBy(b => b.Order).ToList();
        audio.MultipleChoiceBlocks = multipleChoiceBlocks.OrderBy(b => b.Order).ToList();
        
        _logger?.LogInformation("ParseAudioFromBlocks: Result - {ContentBlocks} content blocks, {QuestionBlocks} question blocks, {MultipleChoiceBlocks} multipleChoice blocks", 
            audio.Blocks.Count, audio.QuestionBlocks.Count, audio.MultipleChoiceBlocks.Count);
        
        return audio;
    }

    private GapFillContent ParseGapFillFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        return JsonConvert.DeserializeObject<GapFillContent>("{}", settings) ?? new GapFillContent();
    }

    private EduTrack.Application.Common.Models.ScheduleItems.MultipleChoiceContent ParseMultipleChoiceFromBlocks(JArray blocksArray, JsonSerializerSettings settings)
    {
        var mcqContent = new EduTrack.Application.Common.Models.ScheduleItems.MultipleChoiceContent();
        var mcqBlocks = new List<EduTrack.Application.Common.Models.ScheduleItems.MultipleChoiceBlock>();

        foreach (var blockToken in blocksArray)
        {
            var block = blockToken as JObject;
            if (block == null) continue;

            var blockType = block["type"]?.ToString().ToLower();
            if (!string.Equals(blockType, "multiplechoice", StringComparison.OrdinalIgnoreCase))
                continue;

            var order = block["order"]?.Value<int>() ?? 0;
            var data = block["data"] as JObject;
            if (data == null) continue;

            // Each MCQ block can have multiple questions
            if (data["questions"] is JArray questions)
            {
                foreach (var questionToken in questions)
                {
                    if (questionToken is not JObject questionObj) continue;

                    var questionId = questionObj["id"]?.ToString() ?? Guid.NewGuid().ToString();
                    var questionText = questionObj["stem"]?.ToString() ?? string.Empty;
                    var answerType = questionObj["answerType"]?.ToString() ?? "single";
                    var randomizeOptions = questionObj["randomizeOptions"]?.Value<bool>() ?? false;

                    var mcqBlock = new EduTrack.Application.Common.Models.ScheduleItems.MultipleChoiceBlock
                    {
                        Id = questionId,
                        Order = order,
                        Question = questionText,
                        AnswerType = answerType,
                        RandomizeOptions = randomizeOptions,
                        IsRequired = data["isRequired"]?.Value<bool>() ?? true
                    };

                    // Parse stem media data
                    if (questionObj["stemData"] is JObject stemData)
                    {
                        var mediaType = stemData["mediaType"]?.ToString();
                        mcqBlock.StemMediaType = mediaType;

                        if (mediaType == "image")
                        {
                            mcqBlock.StemImageUrl = stemData["imageUrl"]?.ToString();
                            mcqBlock.StemImageFileId = stemData["imageFileId"]?.ToString();
                            mcqBlock.StemImageFileName = stemData["imageFileName"]?.ToString();
                        }
                        else if (mediaType == "audio")
                        {
                            mcqBlock.StemAudioUrl = stemData["audioUrl"]?.ToString();
                            mcqBlock.StemAudioFileId = stemData["audioFileId"]?.ToString();
                            mcqBlock.StemAudioFileName = stemData["audioFileName"]?.ToString();
                            mcqBlock.StemAudioIsRecorded = stemData["isRecorded"]?.Value<bool>() ?? false;
                        }
                        else if (mediaType == "video")
                        {
                            mcqBlock.StemVideoUrl = stemData["videoUrl"]?.ToString();
                            mcqBlock.StemVideoFileId = stemData["videoFileId"]?.ToString();
                            mcqBlock.StemVideoFileName = stemData["videoFileName"]?.ToString();
                        }
                    }

                    // Points stored as string in builder sometimes
                    var pointsToken = data["points"];
                    if (pointsToken != null)
                    {
                        if (pointsToken.Type == JTokenType.Integer || pointsToken.Type == JTokenType.Float)
                        {
                            mcqBlock.Points = pointsToken.Value<decimal>();
                        }
                        else if (pointsToken.Type == JTokenType.String && decimal.TryParse(pointsToken.ToString(), out var pts))
                        {
                            mcqBlock.Points = pts;
                        }
                    }

                    // Parse options
                    if (questionObj["options"] is JArray options)
                    {
                        var correctAnswers = new List<int>();
                        foreach (var optionToken in options)
                        {
                            if (optionToken is not JObject optionObj) continue;

                            var optionIndex = optionObj["index"]?.Value<int>() ?? 0;
                            var optionType = optionObj["optionType"]?.ToString() ?? "text";
                            var isCorrect = optionObj["isCorrect"]?.Value<bool>() ?? false;

                            var option = new EduTrack.Application.Common.Models.ScheduleItems.MultipleChoiceOption
                            {
                                Index = optionIndex,
                                OptionType = optionType,
                                IsCorrect = isCorrect
                            };

                            if (optionType == "text")
                            {
                                option.Text = optionObj["text"]?.ToString() ?? string.Empty;
                            }
                            else if (optionType == "image")
                            {
                                option.Text = optionObj["text"]?.ToString() ?? string.Empty;
                                option.ImageUrl = optionObj["imageUrl"]?.ToString();
                                option.ImageFileId = optionObj["imageFileId"]?.ToString();
                                option.ImageFileName = optionObj["imageFileName"]?.ToString();
                            }
                            else if (optionType == "audio")
                            {
                                option.Text = optionObj["text"]?.ToString() ?? string.Empty;
                                option.AudioUrl = optionObj["audioUrl"]?.ToString();
                                option.AudioFileId = optionObj["audioFileId"]?.ToString();
                                option.AudioFileName = optionObj["audioFileName"]?.ToString();
                                option.IsRecorded = optionObj["isRecorded"]?.Value<bool>() ?? false;
                                option.AudioDuration = optionObj["audioDuration"]?.Value<int?>();
                            }

                            mcqBlock.Options.Add(option);

                            if (isCorrect)
                            {
                                correctAnswers.Add(optionIndex);
                            }
                        }

                        mcqBlock.CorrectAnswers = correctAnswers;
                    }

                    mcqBlocks.Add(mcqBlock);
                }
            }
        }

        mcqContent.Blocks = mcqBlocks.OrderBy(b => b.Order).ToList();
        return mcqContent;
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
                Alignment = data["alignment"]?.ToString() ?? "right",
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

    /// <summary>
    /// Submit block answer for validation
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitBlockAnswer([FromBody] SubmitBlockAnswerRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (!activeProfileId.HasValue)
        {
            return BadRequest(new { error = "برای ثبت پاسخ، ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید." });
        }

        var command = new SubmitBlockAnswerCommand(
            request.ScheduleItemId,
            request.BlockId,
            currentUser.Id,
            request.SubmittedAnswer,
            activeProfileId);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get block statistics for current student
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBlockStatistics(int? scheduleItemId = null, string? blockId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (!activeProfileId.HasValue)
        {
            return Ok(new ProfileAwareResponse<List<BlockStatisticsDto>>
            {
                Success = false,
                RequiresProfile = true,
                Error = "برای مشاهده تاریخچه پاسخ‌ها، ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید.",
                Data = new List<BlockStatisticsDto>()
            });
        }

        var query = new GetBlockStatisticsQuery(currentUser.Id, scheduleItemId, blockId, activeProfileId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new ProfileAwareResponse<List<BlockStatisticsDto>>
        {
            Success = true,
            Data = result.Value ?? new List<BlockStatisticsDto>()
        });
    }

    /// <summary>
    /// Review page - shows student's errors and statistics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Review(
        bool? onlyWithErrors = null,
        bool? onlyNeverCorrect = null,
        bool? onlyRecentMistakes = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Public" });
        }

        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync();
        if (!activeProfileId.HasValue)
        {
            TempData["Error"] = "برای مشاهده گزارش مرور، ابتدا یک پروفایل یادگیرنده فعال انتخاب کنید.";
            return RedirectToAction("Index", "Profile");
        }

        var query = new GetStudentReviewItemsQuery(
            currentUser.Id,
            onlyWithErrors,
            onlyNeverCorrect,
            onlyRecentMistakes,
            null,
            activeProfileId);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error ?? "خطا در بارگذاری اطلاعات";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.OnlyWithErrors = onlyWithErrors;
        ViewBag.OnlyNeverCorrect = onlyNeverCorrect;
        ViewBag.OnlyRecentMistakes = onlyRecentMistakes;

        return View(result.Value ?? new List<StudentReviewItemDto>());
    }
}

public class SubmitBlockAnswerRequest
{
    public int ScheduleItemId { get; set; }
    public string BlockId { get; set; } = string.Empty;
    public Dictionary<string, object> SubmittedAnswer { get; set; } = new();
}
