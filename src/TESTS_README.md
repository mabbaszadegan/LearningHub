# EduTrack Testing Architecture

## ساختار تست‌نویسی جامع

این پروژه از یک معماری تست‌نویسی جامع و حرفه‌ای استفاده می‌کند که شامل انواع مختلف تست‌ها می‌باشد:

## 📁 ساختار پروژه‌های تست

```
Tests/
├── EduTrack.Domain.Tests/          # Unit Tests - Domain Layer
├── EduTrack.Application.Tests/     # Unit Tests - Application Layer  
├── EduTrack.Infrastructure.Tests/  # Integration Tests - Infrastructure
├── EduTrack.WebApp.Tests/          # E2E Tests - Web Application
├── EduTrack.Integration.Tests/     # Integration Tests - Cross Layer
├── EduTrack.BDD.Tests/             # BDD Tests - SpecFlow
└── EduTrack.TestUtilities/         # Test Utilities & Helpers
```

## 🧪 انواع تست‌ها

### 1. Unit Tests (تست‌های واحد)

#### Domain Tests (`EduTrack.Domain.Tests`)
- **هدف**: تست منطق کسب‌وکار و قوانین دامنه
- **پوشش**: Entities, Value Objects, Domain Services
- **مثال‌ها**:
  - `UserTests.cs` - تست ایجاد کاربر، اعتبارسنجی، و منطق کسب‌وکار
  - `CourseTests.cs` - تست ایجاد دوره، بروزرسانی، و قوانین دامنه
  - `ScheduleItemTests.cs` - تست ایجاد آیتم برنامه، مراحل، و تکمیل
  - `EmailTests.cs` - تست Value Object ایمیل و اعتبارسنجی
  - `UserDomainServiceTests.cs` - تست سرویس‌های دامنه

#### Application Tests (`EduTrack.Application.Tests`)
- **هدف**: تست Command Handlers, Query Handlers, و Application Logic
- **پوشش**: CQRS Pattern, Validation, Business Rules
- **مثال‌ها**:
  - `CreateScheduleItemCommandHandlerTests.cs` - تست ایجاد آیتم برنامه
  - `SaveScheduleItemStepCommandHandlerTests.cs` - تست ذخیره مرحله‌ای

### 2. Integration Tests (تست‌های یکپارچگی)

#### Infrastructure Tests (`EduTrack.Infrastructure.Tests`)
- **هدف**: تست Repository ها، Database Context، و عملیات پایگاه داده
- **پوشش**: Entity Framework, Database Operations, Data Persistence
- **مثال‌ها**:
  - `ScheduleItemRepositoryTests.cs` - تست عملیات Repository
  - `AppDbContextTests.cs` - تست Database Context و Transaction ها

#### WebApp Tests (`EduTrack.WebApp.Tests`)
- **هدف**: تست HTTP Endpoints، Controllers، و Integration با Web Layer
- **پوشش**: MVC Controllers, HTTP Requests/Responses, Authentication
- **مثال‌ها**:
  - `ScheduleItemControllerTests.cs` - تست Controller Actions
  - `ScheduleItemIntegrationTests.cs` - تست End-to-End Workflows

### 3. BDD Tests (تست‌های رفتاری)

#### BDD Tests (`EduTrack.BDD.Tests`)
- **هدف**: تست رفتار سیستم از دیدگاه کاربر
- **ابزار**: SpecFlow, Gherkin Syntax
- **مثال‌ها**:
  - `ScheduleItemCreation.feature` - سناریوهای ایجاد آیتم برنامه
  - `ScheduleItemStepDefinitions.cs` - تعریف مراحل BDD

### 4. Test Utilities (ابزارهای تست)

#### Test Utilities (`EduTrack.TestUtilities`)
- **هدف**: ارائه ابزارها و Helper های مشترک برای تست‌ها
- **محتوا**:
  - `TestBase.cs` - کلاس پایه برای Integration Tests
  - `TestDataBuilder.cs` - Fluent Builder برای ایجاد داده‌های تست

## 🎯 اصول تست‌نویسی

### TDD (Test-Driven Development)
- **قاعده**: ابتدا تست بنویسید، سپس کد را پیاده‌سازی کنید
- **مراحل**: Red → Green → Refactor
- **مزایا**: کد تمیز، طراحی بهتر، پوشش کامل

### BDD (Behavior-Driven Development)
- **قاعده**: تست‌ها بر اساس رفتار مورد انتظار سیستم
- **زبان**: Gherkin (Given-When-Then)
- **مزایا**: درک بهتر نیازمندی‌ها، ارتباط بهتر با ذینفعان

### AAA Pattern (Arrange-Act-Assert)
```csharp
[Fact]
public void Method_WithCondition_ShouldReturnExpectedResult()
{
    // Arrange
    var input = "test data";
    
    // Act
    var result = methodUnderTest(input);
    
    // Assert
    result.Should().Be("expected result");
}
```

## 🔧 ابزارها و کتابخانه‌ها

### Testing Frameworks
- **xUnit** - Framework اصلی تست‌نویسی
- **FluentAssertions** - Assertions خوانا و قدرتمند
- **Moq** - Mocking Framework
- **AutoFixture** - تولید خودکار داده‌های تست

### BDD Tools
- **SpecFlow** - BDD Framework برای .NET
- **Gherkin** - زبان توصیف سناریوها

### Integration Testing
- **Microsoft.AspNetCore.Mvc.Testing** - تست Web Applications
- **Microsoft.EntityFrameworkCore.InMemory** - پایگاه داده در حافظه
- **Selenium WebDriver** - تست End-to-End

## 📊 Coverage و Metrics

### Coverage Goals
- **Unit Tests**: > 90% Code Coverage
- **Integration Tests**: > 80% API Coverage
- **BDD Tests**: 100% User Story Coverage

### Test Categories
- **Smoke Tests** - تست‌های سریع برای بررسی عملکرد اولیه
- **Regression Tests** - تست‌های جامع برای جلوگیری از بازگشت مشکلات
- **Performance Tests** - تست‌های عملکرد و کارایی
- **Security Tests** - تست‌های امنیتی

## 🚀 اجرای تست‌ها

### اجرای تمام تست‌ها
```bash
dotnet test
```

### اجرای تست‌های خاص
```bash
# Unit Tests
dotnet test --filter "Category=Unit"

# Integration Tests
dotnet test --filter "Category=Integration"

# BDD Tests
dotnet test --filter "Category=BDD"

# Smoke Tests
dotnet test --filter "Category=Smoke"
```

### اجرای تست‌ها با Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📋 Best Practices

### 1. Naming Conventions
- **Test Methods**: `MethodName_WithCondition_ShouldReturnExpectedResult`
- **Test Classes**: `ClassNameTests`
- **Test Projects**: `ProjectName.Tests`

### 2. Test Organization
- **Arrange**: آماده‌سازی داده‌ها و شرایط
- **Act**: اجرای عملیات مورد تست
- **Assert**: بررسی نتایج

### 3. Test Data Management
- استفاده از **TestDataBuilder** برای ایجاد داده‌های تست
- استفاده از **AutoFixture** برای تولید خودکار داده‌ها
- پاک‌سازی داده‌ها پس از هر تست

### 4. Mocking Strategy
- Mock کردن Dependencies خارجی
- استفاده از **Moq** برای ایجاد Mock Objects
- تست کردن Interactions با Mock ها

### 5. Error Handling
- تست کردن سناریوهای خطا
- بررسی پیام‌های خطای مناسب
- تست کردن Exception Handling

## 🔍 Debugging Tests

### Visual Studio
- استفاده از Test Explorer
- Breakpoint گذاری در تست‌ها
- مشاهده Test Output

### Command Line
```bash
# اجرای تست با جزئیات بیشتر
dotnet test --verbosity detailed

# اجرای تست خاص
dotnet test --filter "FullyQualifiedName~TestClassName"
```

## 📈 Continuous Integration

### GitHub Actions
- اجرای خودکار تست‌ها در هر Commit
- تولید Coverage Reports
- اجرای تست‌های Performance

### Test Reports
- **xUnit** Test Results
- **Coverage** Reports
- **BDD** Feature Reports

## 🎯 Test Scenarios for Step 3

### سناریوهای تست شده برای مرحله سوم:

1. **ایجاد تخصیص‌های جدید دانش‌آموز**
2. **بروزرسانی تخصیص‌های موجود**
3. **عدم تغییر دانش‌آموزان (بدون خطا)**
4. **مدیریت ID های تکراری**
5. **پاک کردن همه تخصیص‌ها**
6. **تخصیص مباحث موجود**
7. **مدیریت خطاهای Concurrency**
8. **Navigation بین مراحل**
9. **حفظ داده‌ها در Navigation**
10. **Performance با لیست‌های بزرگ**

## 🔧 Troubleshooting

### مشکلات رایج:
1. **Package Downgrade**: به‌روزرسانی Package References
2. **Foreign Key Constraints**: ایجاد داده‌های وابسته
3. **Concurrency Issues**: استفاده از InMemory Database
4. **Authentication**: Mock کردن Authentication در تست‌ها

### راه‌حل‌ها:
- استفاده از **TestUtilities** برای Setup مشترک
- پاک‌سازی Database در هر تست
- استفاده از **TestBase** برای Integration Tests
- Mock کردن External Dependencies

## 📚 منابع بیشتر

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [SpecFlow Documentation](https://specflow.org/)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/)
