using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.WebApp.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Create roles
        await CreateRolesAsync(roleManager);

        // Create admin user
        await CreateAdminUserAsync(userManager);

        // Create sample data if database is empty
        if (!await context.Courses.AnyAsync())
        {
            await CreateSampleDataAsync(context, userManager);
        }
    }

    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Teacher", "Student" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task CreateAdminUserAsync(UserManager<User> userManager)
    {
        var adminEmail = "admin@local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = User.Create("Admin", "User", adminEmail);
            adminUser.EmailConfirmed = true;

            var result = await userManager.CreateAsync(adminUser, "Passw0rd!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task CreateSampleDataAsync(AppDbContext context, UserManager<User> userManager)
    {
        var now = DateTimeOffset.UtcNow;

        // Create teachers
        var teacherEmail = "teacher@local";
        var teacher = await userManager.FindByEmailAsync(teacherEmail);
        if (teacher == null)
        {
            teacher = User.Create("احمد", "احمدی", teacherEmail);
            teacher.EmailConfirmed = true;
            await userManager.CreateAsync(teacher, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher, "Teacher");
        }

        // Create additional teachers
        var teacher2Email = "teacher2@local";
        var teacher2 = await userManager.FindByEmailAsync(teacher2Email);
        if (teacher2 == null)
        {
            teacher2 = User.Create("فاطمه", "محمدی", teacher2Email);
            teacher2.EmailConfirmed = true;
            await userManager.CreateAsync(teacher2, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher2, "Teacher");
        }

        var teacher3Email = "teacher3@local";
        var teacher3 = await userManager.FindByEmailAsync(teacher3Email);
        if (teacher3 == null)
        {
            teacher3 = User.Create("علی", "رضایی", teacher3Email);
            teacher3.EmailConfirmed = true;
            await userManager.CreateAsync(teacher3, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher3, "Teacher");
        }

        // Create students
        var student1Email = "student1@local";
        var student1 = await userManager.FindByEmailAsync(student1Email);
        if (student1 == null)
        {
            student1 = User.Create("Alice", "Student", student1Email);
            student1.EmailConfirmed = true;
            await userManager.CreateAsync(student1, "Passw0rd!");
            await userManager.AddToRoleAsync(student1, "Student");
        }

        var student2Email = "student2@local";
        var student2 = await userManager.FindByEmailAsync(student2Email);
        if (student2 == null)
        {
            student2 = User.Create("Bob", "Student", student2Email);
            student2.EmailConfirmed = true;
            await userManager.CreateAsync(student2, "Passw0rd!");
            await userManager.AddToRoleAsync(student2, "Student");
        }

        // Create additional students for TeachingPlan demo
        var students = new List<User>();
        for (int i = 3; i <= 15; i++)
        {
            var studentEmail = $"student{i}@local";
            var student = await userManager.FindByEmailAsync(studentEmail);
            if (student == null)
            {
                student = User.Create($"Student{i}", "Demo", studentEmail);
                student.EmailConfirmed = true;
                await userManager.CreateAsync(student, "Passw0rd!");
                await userManager.AddToRoleAsync(student, "Student");
            }
            students.Add(student);
        }
        
        // Add the first two students to the list
        students.Add(student1);
        students.Add(student2);

        // Create course
        var course = Course.Create(
            "General English A1",
            "Beginner level English course covering basic grammar, vocabulary, and conversation skills.",
            null,
            1,
            teacher.Id);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Create module
        var module = Module.Create(
            course.Id,
            "Introduction to English",
            "Basic introduction to English language fundamentals.",
            1);
        context.Modules.Add(module);
        await context.SaveChangesAsync();

        // Create lessons
        var lesson1 = Lesson.Create(
            module.Id,
            "Greetings and Introductions",
            "Learn how to greet people and introduce yourself in English.",
            null,
            30,
            1);
        context.Lessons.Add(lesson1);

        var lesson2 = Lesson.Create(
            module.Id,
            "Numbers and Colors",
            "Learn basic numbers and colors in English.",
            null,
            25,
            2);
        context.Lessons.Add(lesson2);
        await context.SaveChangesAsync();

        // Create resources
        var resource1 = Resource.Create(
            lesson1.Id,
            "Greetings Vocabulary",
            ResourceType.PDF,
            "greetings.pdf",
            null,
            null,
            null,
            1);
        context.Resources.Add(resource1);

        var resource2 = Resource.Create(
            lesson2.Id,
            "Numbers Video",
            ResourceType.Video,
            "-",
            "https://example.com/numbers-video",
            null,
            null,
            1);
        context.Resources.Add(resource2);
        await context.SaveChangesAsync();

        // Create class
        var classEntity = Class.Create(
            course.Id,
            "English A1 - Class 1",
            "First class for English A1 course",
            teacher.Id,
            now.AddDays(7),
            now.AddDays(90));
        context.Classes.Add(classEntity);
        await context.SaveChangesAsync();

        // Create enrollments
        var enrollment1 = Enrollment.Create(classEntity.Id, student1.Id);
        context.Enrollments.Add(enrollment1);

        var enrollment2 = Enrollment.Create(classEntity.Id, student2.Id);
        context.Enrollments.Add(enrollment2);
        await context.SaveChangesAsync();

        // Create questions
        var questions = new List<Question>
        {
            Question.Create(
                "What is the correct way to greet someone in English?",
                QuestionType.MultipleChoice,
                "Hello is the most common greeting in English.",
                1,
                teacher.Id),
            Question.Create(
                "The word 'red' is a color.",
                QuestionType.TrueFalse,
                "Red is indeed a color.",
                1,
                teacher.Id),
            Question.Create(
                "How do you say 'thank you' in English?",
                QuestionType.ShortAnswer,
                "Thank you is the correct way to express gratitude.",
                1,
                teacher.Id)
        };
        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();

        // Create choices for multiple choice question
        var choice1 = Choice.Create(questions[0].Id, "Hello", true, 1);
        var choice2 = Choice.Create(questions[0].Id, "Goodbye", false, 2);
        var choice3 = Choice.Create(questions[0].Id, "See you later", false, 3);
        context.Choices.AddRange(choice1, choice2, choice3);
        await context.SaveChangesAsync();

        // Create exam
        var exam = Exam.Create(
            "English A1 Assessment",
            "Basic assessment for English A1 level",
            30,
            75,
            true,
            teacher.Id);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        // Add questions to exam
        var examQuestions = new List<ExamQuestion>
        {
            ExamQuestion.Create(exam.Id, questions[0].Id, 1),
            ExamQuestion.Create(exam.Id, questions[1].Id, 2),
            ExamQuestion.Create(exam.Id, questions[2].Id, 3)
        };
        context.ExamQuestions.AddRange(examQuestions);
        await context.SaveChangesAsync();

        // Create TeachingPlan demo data
        await CreateTeachingPlanDemoDataAsync(context, teacher, course, students);
    }

    private static async Task CreateTeachingPlanDemoDataAsync(AppDbContext context, User teacher, Course course, List<User> students)
    {
        // Create TeachingPlan
        var teachingPlan = TeachingPlan.Create(
            course.Id,
            teacher.Id,
            "Plan A (Fall 2024)",
            "Comprehensive teaching plan for the fall semester covering all course modules with structured assignments and group activities.");

        context.TeachingPlans.Add(teachingPlan);
        await context.SaveChangesAsync();

        // Create StudentGroups
        var groupA = StudentGroup.Create(teachingPlan.Id, "Group A");
        var groupB = StudentGroup.Create(teachingPlan.Id, "Group B");
        var groupC = StudentGroup.Create(teachingPlan.Id, "Group C");
        var studentGroups = new List<StudentGroup> { groupA, groupB, groupC };

        context.StudentGroups.AddRange(studentGroups);
        await context.SaveChangesAsync();

        // Add students to groups (5 students per group)
        var groupMembers = new List<GroupMember>();
        for (int i = 0; i < students.Count && i < 15; i++)
        {
            var group = i < 5 ? groupA : i < 10 ? groupB : groupC;
            groupMembers.Add(GroupMember.Create(group.Id, students[i].Id));
        }
        context.GroupMembers.AddRange(groupMembers);
        await context.SaveChangesAsync();

        // Create ScheduleItems
        var now = DateTimeOffset.UtcNow;
        var scheduleItems = new List<ScheduleItem>
        {
            // Reminder
            ScheduleItem.Create(
                teachingPlan.Id,
                ScheduleItemType.Reminder,
                "Welcome to the Course",
                "Welcome message and course overview",
                now.AddDays(-7),
                null,
                false,
                """{"type":"Reminder","text":"Welcome to our English course! Make sure to review the course materials and join your assigned group.","links":[{"title":"Course Materials","url":"/courses/1"}]}""",
                null,
                null,
                null,
                DisciplineType.Language),

            // Multiple Choice Assignment
            ScheduleItem.Create(
                teachingPlan.Id,
                ScheduleItemType.MultipleChoice,
                "Basic English Grammar Quiz",
                "Test your knowledge of basic English grammar",
                now.AddDays(-3),
                now.AddDays(2),
                true,
                """{"type":"MultipleChoice","stem":"Choose the correct form of the verb","choices":[{"text":"I am go","correct":false},{"text":"I go","correct":true},{"text":"I goes","correct":false}]}""",
                10m,
                groupA.Id,
                null,
                DisciplineType.Language),

            // Writing Assignment
            ScheduleItem.Create(
                teachingPlan.Id,
                ScheduleItemType.Writing,
                "My Weekend Story",
                "Write about your weekend activities",
                now.AddDays(-1),
                now.AddDays(5),
                true,
                """{"type":"Writing","prompt":"Write about your weekend activities in 120-150 words.","maxWords":150,"rubric":"basic"}""",
                15m,
                groupB.Id,
                null,
                DisciplineType.Language),

            // Gap Fill Assignment
            ScheduleItem.Create(
                teachingPlan.Id,
                ScheduleItemType.GapFill,
                "Fill in the Blanks",
                "Complete the sentences with the correct words",
                now,
                now.AddDays(7),
                true,
                """{"type":"GapFill","text":"I ___ to school yesterday.","answers":["went"]}""",
                5m,
                groupC.Id,
                null,
                DisciplineType.Language)
        };

        context.ScheduleItems.AddRange(scheduleItems);
        await context.SaveChangesAsync();

        // Create sample submissions
        var submissions = new List<Submission>
        {
            // Completed MCQ submission
            Submission.Create(scheduleItems[1].Id, students[0].Id, """{"selectedChoice":1}"""),
            // In-progress writing submission
            Submission.Create(scheduleItems[2].Id, students[5].Id, """{"text":"Last weekend I went to the park with my family..."}""")
        };

        // Set submission statuses
        submissions[0].Start();
        submissions[0].Submit();
        submissions[0].SetGrade(10m, "Excellent work! You got all questions correct.", teacher.Id);

        submissions[1].Start();

        context.Submissions.AddRange(submissions);
        await context.SaveChangesAsync();

        // Create TeachingSessionReport demo data
        await CreateTeachingSessionReportDemoDataAsync(context, teacher, teachingPlan, students, studentGroups, scheduleItems);
    }

    private static async Task CreateTeachingSessionReportDemoDataAsync(AppDbContext context, User teacher, TeachingPlan teachingPlan, List<User> students, List<StudentGroup> studentGroups, List<ScheduleItem> scheduleItems)
    {
        // Create sample teaching session report
        var sessionReport = new TeachingSessionReport
        {
            TeachingPlanId = teachingPlan.Id,
            Title = "Past Simple Review Session",
            SessionDate = DateTimeOffset.UtcNow.AddDays(-1),
            Mode = SessionMode.InPerson,
            Location = "Classroom A-101",
            Notes = "Good participation overall. Students struggled with irregular verbs but showed improvement by the end of the session.",
            CreatedByTeacherId = teacher.Id,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        context.TeachingSessionReports.Add(sessionReport);
        await context.SaveChangesAsync();

        // Create attendance records
        var attendance = new List<TeachingSessionAttendance>
        {
            new() { TeachingSessionReportId = sessionReport.Id, StudentId = students[0].Id, Status = AttendanceStatus.Present, ParticipationScore = 9.5m, Comment = "Excellent participation" },
            new() { TeachingSessionReportId = sessionReport.Id, StudentId = students[1].Id, Status = AttendanceStatus.Present, ParticipationScore = 8.0m, Comment = "Good effort" },
            new() { TeachingSessionReportId = sessionReport.Id, StudentId = students[2].Id, Status = AttendanceStatus.Late, ParticipationScore = 7.5m, Comment = "Arrived 15 minutes late" },
            new() { TeachingSessionReportId = sessionReport.Id, StudentId = students[3].Id, Status = AttendanceStatus.Absent, Comment = "Sick leave" },
            new() { TeachingSessionReportId = sessionReport.Id, StudentId = students[4].Id, Status = AttendanceStatus.Present, ParticipationScore = 8.5m, Comment = "Active participation" }
        };

        context.TeachingSessionAttendances.AddRange(attendance);
        await context.SaveChangesAsync();

        // Create sample ScheduleItemAssignment for targeted assignments
        var scheduleItemAssignments = new List<ScheduleItemAssignment>
        {
            new() { ScheduleItemId = scheduleItems[0].Id, StudentId = int.Parse(students[0].Id) }, // Reminder for specific student
            new() { ScheduleItemId = scheduleItems[1].Id, GroupId = studentGroups[0].Id }, // MCQ for Group A
            new() { ScheduleItemId = scheduleItems[2].Id, GroupId = studentGroups[1].Id }, // Writing for Group B
            new() { ScheduleItemId = scheduleItems[3].Id, StudentId = int.Parse(students[5].Id) } // GapFill for specific student
        };

        context.ScheduleItemAssignments.AddRange(scheduleItemAssignments);
        await context.SaveChangesAsync();
    }
}
