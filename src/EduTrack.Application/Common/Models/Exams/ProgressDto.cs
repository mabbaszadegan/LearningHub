using EduTrack.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.Common.Models.Exams
{
    public class ProgressDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int? LessonId { get; set; }
        public string? LessonTitle { get; set; }
        public int? ExamId { get; set; }
        public string? ExamTitle { get; set; }
        public ProgressStatus Status { get; set; }
        public int CorrectCount { get; set; }
        public int Streak { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
