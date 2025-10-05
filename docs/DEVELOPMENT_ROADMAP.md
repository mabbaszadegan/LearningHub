# EduTrack Development Roadmap

## Current Status Analysis

### ✅ Existing Features
- **Architecture**: Clean Architecture with DDD + CQRS + MediatR
- **Database**: Multi-provider support (SQLite, SQL Server, PostgreSQL)
- **PWA**: Offline capabilities with service worker
- **Authentication**: Role-based auth (Admin, Teacher, Student)
- **Content Structure**: 
  - Course → Module → Lesson → Resource hierarchy
  - Chapter → SubChapter → EducationalContent structure
- **Assessment**: Exam system with multiple question types
- **Interactive Learning**: InteractiveLesson with InteractiveContentItem
- **Enrollment**: Class enrollment system (Enrollment entity)
- **Progress**: Basic progress tracking (Progress entity with streak system)
- **UI**: Student/Teacher/Admin UI areas with Bootstrap 5

### ❌ Missing Critical Features
- **Course Enrollment**: Direct course enrollment (only Class enrollment exists)
- **Gamification**: Complete gamification system
- **Student Dashboard**: Enhanced student experience
- **Assignments**: Homework/assignment system
- **Grading**: Advanced grading system
- **Notifications**: System notifications
- **Calendar**: Scheduling system

---

## Priority Development Plan

### Phase 1: Course Enrollment & Student Dashboard (HIGH PRIORITY)
**Goal**: Enable students to enroll directly in courses and access lessons

#### Tasks:
1. **Create CourseEnrollment Entity**
   - StudentId, CourseId, EnrolledAt, IsActive, CompletedAt
   - Commands: EnrollInCourse, UnenrollFromCourse
   - Queries: GetStudentCourses, GetCourseStudents

2. **Build Student Dashboard**
   - Controller: StudentDashboardController
   - Views: Dashboard, CourseList, CourseDetails
   - Features: Course enrollment, progress tracking, recent activity

3. **Implement Course Access Management**
   - CourseAccess entity for access control
   - Permission checking for course content
   - Enrollment validation

4. **Enhance Student Navigation**
   - Course catalog for students
   - Easy course discovery
   - Enrollment process UI

#### Entities to Create:
```csharp
// CourseEnrollment.cs
public class CourseEnrollment
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int CourseId { get; private set; }
    public DateTimeOffset EnrolledAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public bool IsActive { get; private set; }
    // Navigation properties
}

// CourseAccess.cs
public class CourseAccess
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int CourseId { get; private set; }
    public AccessLevel Level { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }
    // Navigation properties
}
```

---

### Phase 2: Basic Gamification System (HIGH PRIORITY)
**Goal**: Implement points, levels, and basic badges

#### Tasks:
1. **Create Points System**
   - StudentPoints entity for tracking XP
   - XP earning rules and calculations
   - Level progression system

2. **Implement Badge System**
   - Badge entity with categories
   - StudentBadge for earned badges
   - Badge earning logic

3. **Build Gamification UI**
   - Points display in student dashboard
   - Badge showcase
   - Progress visualization

4. **Integrate with Existing Progress**
   - Connect XP earning with lesson completion
   - Link badges to achievements
   - Update progress tracking

#### Entities to Create:
```csharp
// StudentPoints.cs
public class StudentPoints
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int TotalPoints { get; private set; }
    public int CurrentLevel { get; private set; }
    public int PointsToNextLevel { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; }
    // Navigation properties
}

// Badge.cs
public class Badge
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Icon { get; private set; }
    public int RequiredPoints { get; private set; }
    public BadgeCategory Category { get; private set; }
    public bool IsActive { get; private set; }
}

// StudentBadge.cs
public class StudentBadge
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int BadgeId { get; private set; }
    public DateTimeOffset EarnedAt { get; private set; }
    public bool IsActive { get; private set; }
    // Navigation properties
}
```

#### Badge Ideas:
- 🎯 **First Lesson** (10 XP) - Complete first lesson
- 🔥 **Streak Master** (50 XP) - Complete 10 lessons in a row
- 📚 **Bookworm** (100 XP) - Complete 50 lessons
- ⚡ **Speed Learner** (75 XP) - Complete lesson quickly
- 🧠 **Quiz Master** (100 XP) - High exam scores
- 💎 **Perfect Score** (200 XP) - Perfect exam score
- 🏆 **Course Champion** (500 XP) - Complete entire course
- 🌟 **Early Bird** (25 XP) - Complete lesson before deadline

#### XP System:
- **Lesson completed**: 10 XP
- **Exam passed**: 20 XP
- **Perfect score**: 50 XP
- **Early completion**: +5 XP bonus
- **Daily study streak**: +2 XP bonus

#### Level System:
- **Level 1-5**: مبتدی (0-500 XP)
- **Level 6-10**: متوسط (500-1500 XP)
- **Level 11-15**: پیشرفته (1500-3000 XP)
- **Level 16+**: استاد (3000+ XP)

---

### Phase 3: Enhanced Lesson Study Experience (MEDIUM PRIORITY)
**Goal**: Improve student learning experience

#### Tasks:
1. **Enhance Lesson Progress Tracking**
   - Detailed progress metrics
   - Time spent tracking
   - Completion percentage

2. **Implement Bookmark System**
   - LessonBookmark entity
   - Quick access to bookmarked lessons
   - Bookmark management UI

3. **Add Note-taking System**
   - StudentNote entity
   - Rich text notes
   - Note organization

4. **Improve Lesson Viewer**
   - Better UI/UX
   - Progress indicators
   - Navigation improvements

#### Entities to Create:
```csharp
// LessonBookmark.cs
public class LessonBookmark
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int LessonId { get; private set; }
    public DateTimeOffset BookmarkedAt { get; private set; }
    // Navigation properties
}

// StudentNote.cs
public class StudentNote
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int LessonId { get; private set; }
    public string Content { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    // Navigation properties
}

// LessonTimeTracking.cs
public class LessonTimeTracking
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int LessonId { get; private set; }
    public TimeSpan TimeSpent { get; private set; }
    public DateTimeOffset LastAccessed { get; private set; }
    // Navigation properties
}
```

---

### Phase 4: Advanced Gamification (MEDIUM PRIORITY)
**Goal**: Complete gamification with achievements and competition

#### Tasks:
1. **Build Achievement System**
   - Achievement entity with complex criteria
   - Achievement tracking
   - Achievement notifications

2. **Implement Leaderboard**
   - Leaderboard entity
   - Ranking system
   - Public/private leaderboards

3. **Add Competition System**
   - Competition entity
   - Time-limited challenges
   - Prize system

4. **Create Social Features**
   - Student profiles
   - Achievement sharing
   - Peer comparison

#### Entities to Create:
```csharp
// Achievement.cs
public class Achievement
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Criteria { get; private set; }
    public int Points { get; private set; }
    public AchievementCategory Category { get; private set; }
    public bool IsActive { get; private set; }
}

// StudentAchievement.cs
public class StudentAchievement
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int AchievementId { get; private set; }
    public DateTimeOffset EarnedAt { get; private set; }
    // Navigation properties
}

// Leaderboard.cs
public class Leaderboard
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int Rank { get; private set; }
    public int Points { get; private set; }
    public int Level { get; private set; }
    public DateTimeOffset LastUpdated { get; private set; }
    // Navigation properties
}

// Competition.cs
public class Competition
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public string Prize { get; private set; }
    public bool IsActive { get; private set; }
}
```

---

### Phase 5: Assignment & Homework System (MEDIUM PRIORITY)
**Goal**: Enable teachers to assign homework and students to submit

#### Tasks:
1. **Create Assignment System**
   - Assignment entity
   - Assignment management for teachers
   - Assignment submission for students

2. **Build Grading System**
   - Grade entity
   - Grading interface for teachers
   - Grade display for students

3. **Add Assignment Dashboard**
   - Student assignment list
   - Teacher assignment management
   - Due date tracking

#### Entities to Create:
```csharp
// Assignment.cs
public class Assignment
{
    public int Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int CourseId { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public int Points { get; private set; }
    public bool IsActive { get; private set; }
    public string CreatedBy { get; private set; }
    // Navigation properties
}

// AssignmentSubmission.cs
public class AssignmentSubmission
{
    public int Id { get; private set; }
    public int AssignmentId { get; private set; }
    public string StudentId { get; private set; }
    public string Content { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public int? Grade { get; private set; }
    public string? Feedback { get; private set; }
    // Navigation properties
}

// Grade.cs
public class Grade
{
    public int Id { get; private set; }
    public string StudentId { get; private set; }
    public int AssignmentId { get; private set; }
    public int Points { get; private set; }
    public int MaxPoints { get; private set; }
    public string? Feedback { get; private set; }
    public DateTimeOffset GradedAt { get; private set; }
    public string GradedBy { get; private set; }
    // Navigation properties
}
```

---

### Phase 6: Advanced Features (LOW PRIORITY)
**Goal**: Complete the LMS with advanced features

#### Tasks:
1. **Notification System**
   - Notification entity
   - Real-time notifications
   - Email/SMS integration

2. **Calendar/Scheduling System**
   - Calendar entity
   - Event scheduling
   - Deadline management

3. **Advanced Reporting**
   - Analytics dashboard
   - Custom reports
   - Export functionality

4. **Parent Portal**
   - Parent access
   - Student progress viewing
   - Communication with teachers

5. **Mobile App Integration**
   - API endpoints
   - Mobile-specific features
   - Offline synchronization

---

## Technical Implementation Notes

### Architecture Guidelines:
- Follow existing Clean Architecture patterns
- Use CQRS with MediatR for all new features
- Maintain existing database provider support
- Ensure PWA compatibility
- Follow existing UI patterns (Bootstrap 5, Persian RTL)
- Use existing validation patterns (FluentValidation)

### Database Considerations:
- Add new entities to AppDbContext
- Create migrations for each phase
- Maintain backward compatibility
- Consider indexing for performance

### UI/UX Guidelines:
- Maintain existing design system
- Use Persian RTL layout
- Ensure mobile responsiveness
- Follow accessibility standards
- Implement progressive enhancement

### Performance Considerations:
- Optimize database queries
- Implement caching where appropriate
- Use pagination for large datasets
- Monitor performance metrics

---

## Success Metrics

### Student Engagement:
- Time spent on platform
- Lessons completed per week
- Course completion rates
- Student satisfaction scores

### Teacher Efficiency:
- Time to create assignments
- Grading efficiency
- Student progress visibility
- Communication effectiveness

### System Performance:
- Page load times
- Database query performance
- User session duration
- Error rates

### Gamification Impact:
- Badge earning rates
- Leaderboard participation
- Competition engagement
- XP earning patterns

---

## Next Steps

1. **Start with Phase 1**: Course Enrollment & Student Dashboard
2. **Create detailed task breakdown** for each phase
3. **Set up development environment** for new features
4. **Begin implementation** following the roadmap
5. **Monitor progress** and adjust priorities as needed

---

*Last Updated: [Current Date]*
*Version: 1.0*
*Status: Planning Phase*
