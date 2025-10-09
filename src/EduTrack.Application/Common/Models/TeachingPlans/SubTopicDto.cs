using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.Common.Models.TeachingPlans
{
    public class SubTopicDto
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Objective { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int Order { get; set; }
        public string ChapterTitle { get; set; } = string.Empty;
    }
}
