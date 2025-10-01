# EduTrack API Documentation

## Overview

EduTrack provides a RESTful API through ASP.NET Core MVC controllers. All endpoints return JSON responses and support standard HTTP methods.

## Authentication

EduTrack uses cookie-based authentication. Include authentication cookies in requests or use the login endpoint to obtain a session.

### Login
```http
POST /Account/Login
Content-Type: application/x-www-form-urlencoded

Email=admin@local&Password=Passw0rd!&RememberMe=false
```

## API Endpoints

### Courses

#### Get All Courses
```http
GET /Catalog?pageNumber=1&pageSize=10&isActive=true
```

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "title": "General English A1",
      "description": "Beginner level English course",
      "thumbnail": "course-thumb.jpg",
      "isActive": true,
      "order": 1,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z",
      "createdBy": "admin-user-id",
      "moduleCount": 3,
      "lessonCount": 15
    }
  ],
  "pageNumber": 1,
  "totalPages": 1,
  "totalCount": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

#### Get Course by ID
```http
GET /Catalog/Details/1
```

#### Create Course
```http
POST /Catalog/Create
Content-Type: application/x-www-form-urlencoded

Title=New Course&Description=Course Description&Thumbnail=&Order=1
```

#### Update Course
```http
POST /Catalog/Edit/1
Content-Type: application/x-www-form-urlencoded

Id=1&Title=Updated Course&Description=Updated Description&Thumbnail=&IsActive=true&Order=1
```

#### Delete Course
```http
POST /Catalog/Delete/1
```

### Modules

#### Get Modules by Course
```http
GET /Catalog/Modules?courseId=1
```

#### Create Module
```http
POST /Catalog/CreateModule
Content-Type: application/x-www-form-urlencoded

CourseId=1&Title=New Module&Description=Module Description&Order=1
```

### Lessons

#### Get Lessons by Module
```http
GET /Catalog/Lessons?moduleId=1
```

#### Create Lesson
```http
POST /Catalog/CreateLesson
Content-Type: application/x-www-form-urlencoded

ModuleId=1&Title=New Lesson&Content=Lesson Content&VideoUrl=&Order=1&DurationMinutes=30
```

### Resources

#### Get Resources by Lesson
```http
GET /Catalog/Resources?lessonId=1
```

#### Upload Resource
```http
POST /Catalog/UploadResource
Content-Type: multipart/form-data

LessonId=1&Title=Resource Title&Description=Resource Description&Type=1&Order=1&File=[file data]
```

### Exams

#### Get All Exams
```http
GET /Exam?pageNumber=1&pageSize=10&isActive=true
```

#### Get Exam by ID
```http
GET /Exam/Details/1
```

#### Create Exam
```http
POST /Exam/Create
Content-Type: application/x-www-form-urlencoded

Title=New Exam&Description=Exam Description&DurationMinutes=60&PassingScore=75&ShowSolutions=true
```

#### Start Exam
```http
POST /Exam/Start/1
```

#### Submit Exam
```http
POST /Exam/Submit/1
Content-Type: application/x-www-form-urlencoded

Answers[0].QuestionId=1&Answers[0].SelectedChoiceId=1&Answers[1].QuestionId=2&Answers[1].TextAnswer=My answer
```

### Questions

#### Get All Questions
```http
GET /Exam/Questions?pageNumber=1&pageSize=10&isActive=true
```

#### Create Question
```http
POST /Exam/CreateQuestion
Content-Type: application/x-www-form-urlencoded

Text=What is the capital of France?&Type=1&Explanation=Paris is the capital&Points=1&Choices[0].Text=Paris&Choices[0].IsCorrect=true&Choices[0].Order=1&Choices[1].Text=London&Choices[1].IsCorrect=false&Choices[1].Order=2
```

### Classes

#### Get All Classes
```http
GET /Classroom?pageNumber=1&pageSize=10&isActive=true
```

#### Get Class by ID
```http
GET /Classroom/Details/1
```

#### Create Class
```http
POST /Classroom/Create
Content-Type: application/x-www-form-urlencoded

CourseId=1&Name=Class Name&Description=Class Description&TeacherId=teacher-id&StartDate=2024-01-01&EndDate=2024-06-01
```

#### Enroll Student
```http
POST /Classroom/EnrollStudent
Content-Type: application/x-www-form-urlencoded

classId=1&studentId=student-id
```

#### Get Class Summary
```http
GET /Classroom/Summary/1
```

#### Export Class Data
```http
GET /Classroom/Export/1
```

### Progress

#### Get Student Progress
```http
GET /Progress?pageNumber=1&pageSize=50
```

#### Get Student Stats
```http
GET /Progress/Stats
```

#### Get Lesson Progress
```http
GET /Progress/Lesson/1
```

#### Get Exam Progress
```http
GET /Progress/Exam/1
```

## Data Models

### CourseDto
```json
{
  "id": 1,
  "title": "string",
  "description": "string",
  "thumbnail": "string",
  "isActive": true,
  "order": 1,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "createdBy": "string",
  "moduleCount": 3,
  "lessonCount": 15
}
```

### ModuleDto
```json
{
  "id": 1,
  "courseId": 1,
  "title": "string",
  "description": "string",
  "isActive": true,
  "order": 1,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "lessonCount": 5
}
```

### LessonDto
```json
{
  "id": 1,
  "moduleId": 1,
  "title": "string",
  "content": "string",
  "videoUrl": "string",
  "isActive": true,
  "order": 1,
  "durationMinutes": 30,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "resources": [],
  "progressStatus": 1
}
```

### ResourceDto
```json
{
  "id": 1,
  "lessonId": 1,
  "title": "string",
  "description": "string",
  "type": 1,
  "filePath": "string",
  "url": "string",
  "fileSizeBytes": 1024,
  "mimeType": "string",
  "isActive": true,
  "order": 1,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### ExamDto
```json
{
  "id": 1,
  "title": "string",
  "description": "string",
  "durationMinutes": 60,
  "passingScore": 75,
  "showSolutions": true,
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "createdBy": "string",
  "questionCount": 10
}
```

### QuestionDto
```json
{
  "id": 1,
  "text": "string",
  "type": 1,
  "explanation": "string",
  "points": 1,
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "createdBy": "string",
  "choices": []
}
```

### ChoiceDto
```json
{
  "id": 1,
  "questionId": 1,
  "text": "string",
  "isCorrect": true,
  "order": 1
}
```

### AttemptDto
```json
{
  "id": 1,
  "examId": 1,
  "studentId": "string",
  "studentName": "string",
  "startedAt": "2024-01-01T00:00:00Z",
  "submittedAt": "2024-01-01T00:30:00Z",
  "completedAt": "2024-01-01T00:30:00Z",
  "score": 85,
  "totalQuestions": 10,
  "correctAnswers": 8,
  "isPassed": true,
  "duration": "00:30:00"
}
```

### ProgressDto
```json
{
  "id": 1,
  "studentId": "string",
  "studentName": "string",
  "lessonId": 1,
  "lessonTitle": "string",
  "examId": 1,
  "examTitle": "string",
  "status": 1,
  "correctCount": 5,
  "streak": 3,
  "startedAt": "2024-01-01T00:00:00Z",
  "completedAt": "2024-01-01T00:30:00Z",
  "updatedAt": "2024-01-01T00:30:00Z"
}
```

### ClassDto
```json
{
  "id": 1,
  "courseId": 1,
  "name": "string",
  "description": "string",
  "teacherId": "string",
  "teacherName": "string",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-06-01T00:00:00Z",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "studentCount": 25
}
```

## Enums

### QuestionType
- `1` = MultipleChoice
- `2` = TrueFalse
- `3` = ShortAnswer

### ProgressStatus
- `0` = NotStarted
- `1` = InProgress
- `2` = Done
- `3` = Mastered

### ResourceType
- `1` = PDF
- `2` = Video
- `3` = Image
- `4` = URL
- `5` = Document

### UserRole
- `1` = Admin
- `2` = Teacher
- `3` = Student

## Error Handling

All endpoints return appropriate HTTP status codes:

- `200 OK` - Success
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

Error responses include a message:
```json
{
  "error": "Error message describing what went wrong"
}
```

## Rate Limiting

Currently no rate limiting is implemented. Consider implementing rate limiting for production deployments.

## CORS

CORS is not configured by default. Configure CORS if you need to access the API from different domains.

## PWA Support

The application includes PWA capabilities:
- Service worker for offline caching
- Background sync for offline submissions
- Installable web app
- Offline lesson viewing

## Examples

### Complete Workflow: Create Course with Exam

1. **Create Course**
```http
POST /Catalog/Create
Title=Advanced English&Description=Advanced level course&Order=1
```

2. **Create Module**
```http
POST /Catalog/CreateModule
CourseId=1&Title=Grammar&Description=Advanced grammar&Order=1
```

3. **Create Lesson**
```http
POST /Catalog/CreateLesson
ModuleId=1&Title=Conditionals&Content=Learn about conditional sentences&Order=1&DurationMinutes=45
```

4. **Create Question**
```http
POST /Exam/CreateQuestion
Text=Which conditional is used for hypothetical situations?&Type=1&Points=1&Choices[0].Text=Second conditional&Choices[0].IsCorrect=true&Choices[0].Order=1&Choices[1].Text=First conditional&Choices[1].IsCorrect=false&Choices[1].Order=2
```

5. **Create Exam**
```http
POST /Exam/Create
Title=Grammar Test&DurationMinutes=30&PassingScore=75&ShowSolutions=true
```

6. **Add Question to Exam**
```http
POST /Exam/AddQuestion
ExamId=1&QuestionId=1
```

---

For more detailed information, refer to the main documentation or explore the source code.
