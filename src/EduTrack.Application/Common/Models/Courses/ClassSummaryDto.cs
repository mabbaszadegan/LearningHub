using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.Common.Models.Courses
{
    public class ClassSummaryDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int CompletedStudents { get; set; }
        public double CompletionPercentage { get; set; }
        public double AverageScore { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
