using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Services.Validators;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EduTrack.Application.Tests.Common.Services.Validators;

public class GapFillBlockValidatorTests
{
    [Theory]
    [InlineData("\u0645\u06cc\u200c\u0631\u0648\u0645", "\u0645\u06cc\u0631\u0648\u0645")] // می‌روم vs میروم
    [InlineData("\u0645\u06cc\u200c\u0631\u0648\u0645", "\u0645\u06cc \u0631\u0648\u0645")] // می‌روم vs می روم
    [InlineData("\u0645\u06cc \u0631\u0648\u0645", "\u0645\u06cc\u0631\u0648\u0645")]      // می روم vs میروم
    [InlineData("\u06a9\u062a\u0627\u0628\u06cc", "\u0643\u062a\u0627\u0628\u064a")]       // کتابی vs كتابي (Arabic forms)
    [InlineData("\u0633\u0627\u0644 123", "\u0633\u0627\u0644 \u06f1\u06f2\u06f3")]         // سال 123 vs سال ۱۲۳
    public async Task ValidateAnswerAsync_ShouldAcceptNormalizedPersianInputs(string correctAnswer, string submittedValue)
    {
        // Arrange
        const int scheduleItemId = 42;
        const string blockId = "block-1";
        var contentJson = BuildGapFillContentJson(blockId, correctAnswer);
        var scheduleItem = CreateScheduleItem(scheduleItemId, contentJson);

        var repositoryMock = new Mock<IScheduleItemRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scheduleItem);

        var validator = new GapFillBlockValidator(repositoryMock.Object);

        var submittedAnswer = new Dictionary<string, object>
        {
            ["blanks"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["blankId"] = "blank1",
                    ["index"] = 1,
                    ["value"] = submittedValue
                }
            }
        };

        // Act
        var result = await validator.ValidateAnswerAsync(scheduleItemId, blockId, submittedAnswer, CancellationToken.None);

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.PointsEarned.Should().Be(result.MaxPoints);
        repositoryMock.Verify(repo => repo.GetByIdAsync(scheduleItemId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAnswerAsync_WithBlankOptions_ShouldMatchByOptionValue()
    {
        // Arrange
        const int scheduleItemId = 100;
        const string blockId = "block-2";

        var contentJson = BuildGapFillContentWithOptionsJson();

        var scheduleItem = CreateScheduleItem(scheduleItemId, contentJson);

        var repositoryMock = new Mock<IScheduleItemRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scheduleItem);

        var validator = new GapFillBlockValidator(repositoryMock.Object);

        var submittedAnswer = new Dictionary<string, object>
        {
            ["blanks"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["blankId"] = "blank1",
                    ["index"] = 1,
                    ["value"] = "This",
                    ["optionId"] = "23c39497-4dc0-4dd7-8efe-eee67e5a5476"
                },
                new Dictionary<string, object>
                {
                    ["blankId"] = "blank2",
                    ["index"] = 2,
                    ["value"] = "a",
                    ["optionId"] = "21b5a56c-53ba-42df-8a6b-50fcb46e475d"
                },
                new Dictionary<string, object>
                {
                    ["blankId"] = "blank3",
                    ["index"] = 3,
                    ["value"] = "am",
                    ["optionId"] = "b6ed1d49-0563-483b-a319-543470ca91f0"
                }
            }
        };

        // Act
        var result = await validator.ValidateAnswerAsync(scheduleItemId, blockId, submittedAnswer, CancellationToken.None);

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.PointsEarned.Should().Be(result.MaxPoints);
        repositoryMock.Verify(repo => repo.GetByIdAsync(scheduleItemId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAnswerAsync_WithStringifiedBlankEntries_ShouldParseCorrectly()
    {
        // Arrange
        const int scheduleItemId = 102;
        const string blockId = "block-string";

        var contentJson = BuildGapFillContentJson(blockId, "desk");

        var scheduleItem = CreateScheduleItem(scheduleItemId, contentJson);

        var repositoryMock = new Mock<IScheduleItemRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scheduleItem);

        var validator = new GapFillBlockValidator(repositoryMock.Object);

        var submittedAnswer = new Dictionary<string, object>
        {
            ["blanks"] = new[]
            {
                @"{""blankId"":""blank1"",""index"":1,""value"":""desk""}"
            }
        };

        // Act
        var result = await validator.ValidateAnswerAsync(scheduleItemId, blockId, submittedAnswer, CancellationToken.None);

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.PointsEarned.Should().Be(result.MaxPoints);
        repositoryMock.Verify(repo => repo.GetByIdAsync(scheduleItemId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAnswerAsync_WithBlankOptionValueButMissingOptionId_ShouldStillSucceed()
    {
        // Arrange
        const int scheduleItemId = 101;
        const string blockId = "block-3";

        var contentJson = @"{""blocks"":[{""id"":""block-3"",""type"":""gapFill"",""data"":{""answerType"":""exact"",""caseSensitive"":false,""blanks"":[{""id"":""blank1"",""index"":1,""correctAnswer"":""desk"",""allowManualInput"":false,""allowGlobalOptions"":false,""allowBlankOptions"":true,""options"":[{""id"":""opt-1"",""value"":""desk"",""displayText"":""desk""},{""id"":""opt-2"",""value"":""table"",""displayText"":""table""}]}]}}]}";

        var scheduleItem = CreateScheduleItem(scheduleItemId, contentJson);

        var repositoryMock = new Mock<IScheduleItemRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scheduleItem);

        var validator = new GapFillBlockValidator(repositoryMock.Object);

        var submittedAnswer = new Dictionary<string, object>
        {
            ["blanks"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["blankId"] = "blank1",
                    ["index"] = 1,
                    ["value"] = "desk"
                }
            }
        };

        // Act
        var result = await validator.ValidateAnswerAsync(scheduleItemId, blockId, submittedAnswer, CancellationToken.None);

        // Assert
        result.IsCorrect.Should().BeTrue();
    }

    private static string BuildGapFillContentWithOptionsJson()
    {
        return @"{""itemType"":""gapfill"",""blocks"":[{""id"":""block-2"",""type"":""gapFill"",""order"":0,""data"":{""text"":"""",""gaps"":[{""index"":1,""correctAnswer"":""This"",""alternativeAnswers"":[],""hint"":""""},{""index"":2,""correctAnswer"":""a"",""alternativeAnswers"":[],""hint"":""""},{""index"":3,""correctAnswer"":""am"",""alternativeAnswers"":[],""hint"":""""}],""answerType"":""exact"",""caseSensitive"":false,""content"":""<p style=\""text-align:left;\"" dir=\""ltr\"">[[blank1]] is a desk.</p><p style=\""text-align:left;\"" dir=\""ltr\"">He is [[blank2]] doctor.</p><p style=\""text-align:left;\"" dir=\""ltr\"">I [[blank3]] a doctor.</p>"",""textContent"":""[[blank1]] is a desk.He is [[blank2]] doctor.I [[blank3]] a doctor."",""showOptions"":true,""fileUrl"":""/FileUpload/GetFile/65"",""mimeType"":""image//fileupload/getfile/65"",""fileId"":""65"",""showGlobalOptions"":true,""globalOptions"":[],""blanks"":[{""id"":""blank1"",""index"":1,""correctAnswer"":""This"",""alternativeAnswers"":[],""hint"":"""",""allowManualInput"":false,""allowGlobalOptions"":false,""allowBlankOptions"":true,""options"":[{""id"":""23c39497-4dc0-4dd7-8efe-eee67e5a5476"",""value"":""This"",""displayText"":""This""},{""id"":""8dd242fc-9452-44a7-86e8-b5c05247343a"",""value"":""Those"",""displayText"":""Those""}],""correctOptionId"":null,""alternativeOptionIds"":[]},{""id"":""blank2"",""index"":2,""correctAnswer"":""a"",""alternativeAnswers"":[],""hint"":"""",""allowManualInput"":false,""allowGlobalOptions"":false,""allowBlankOptions"":true,""options"":[{""id"":""21b5a56c-53ba-42df-8a6b-50fcb46e475d"",""value"":""a"",""displayText"":""a""},{""id"":""97ad33ba-c07a-45cc-b413-577b08e0df81"",""value"":""an"",""displayText"":""an""}],""correctOptionId"":null,""alternativeOptionIds"":[]},{""id"":""blank3"",""index"":3,""correctAnswer"":""am"",""alternativeAnswers"":[],""hint"":"""",""allowManualInput"":false,""allowGlobalOptions"":false,""allowBlankOptions"":true,""options"":[{""id"":""b6ed1d49-0563-483b-a319-543470ca91f0"",""value"":""am"",""displayText"":""am""},{""id"":""8d82b8b9-e52f-4d94-89f0-acb4478357a5"",""value"":""are"",""displayText"":""are""}],""correctOptionId"":null,""alternativeOptionIds"":[]}]}}],""settings"":{}}";
    }

    private static ScheduleItem CreateScheduleItem(int id, string contentJson)
    {
        var item = ScheduleItem.Create(
            teachingPlanId: 1,
            type: ScheduleItemType.GapFill,
            title: "Gap Fill Test",
            description: "Test description",
            startDate: DateTimeOffset.UtcNow,
            dueDate: DateTimeOffset.UtcNow.AddDays(1),
            isMandatory: true,
            contentJson: contentJson,
            maxScore: 1m,
            groupId: null,
            lessonId: null,
            disciplineHint: DisciplineType.Language);

        typeof(ScheduleItem)
            .GetProperty(nameof(ScheduleItem.Id), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
            .SetValue(item, id);

        return item;
    }

    private static string BuildGapFillContentJson(string blockId, string correctAnswer)
    {
        var content = new
        {
            blocks = new[]
            {
                new
                {
                    id = blockId,
                    type = "gapFill",
                    data = new
                    {
                        order = 1,
                        answerType = "exact",
                        caseSensitive = false,
                        showGlobalOptions = false,
                        points = 1,
                        blanks = new[]
                        {
                            new
                            {
                                id = "blank1",
                                index = 1,
                                correctAnswer = correctAnswer,
                                allowManualInput = true,
                                allowGlobalOptions = false,
                                allowBlankOptions = false,
                                alternativeAnswers = new string[0],
                                options = new object[0]
                            }
                        }
                    }
                }
            }
        };

        return JsonConvert.SerializeObject(content);
    }
}

