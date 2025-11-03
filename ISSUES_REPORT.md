# گزارش مشکلات بخش معلم و ScheduleItem

## 🔴 مشکلات بحرانی (Critical Issues)

### 1. نبود بررسی دسترسی در CreateOrEdit
**مسیر:** `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs`

**مشکل:**
- متد `CreateOrEdit` در `ScheduleItemController` بررسی نمی‌کند که معلم مالک TeachingPlan است یا نه
- در حالی که `ScheduleController` این بررسی را دارد (خطوط 49, 94, 244)

**ریسک:** معلم‌ها می‌توانند آیتم‌های آموزشی سایر معلم‌ها را ایجاد/ویرایش کنند

**کد مشکل‌دار:**
```csharp
// خط 76-83: هیچ بررسی دسترسی وجود ندارد
public async Task<IActionResult> CreateOrEdit(int teachingPlanId, int id = 0)
{
    var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
    if (!teachingPlan.IsSuccess || teachingPlan.Value == null)
    {
        return NotFound("برنامه آموزشی یافت نشد.");
    }
    // ❌ هیچ چک برای TeacherId وجود ندارد
}
```

---

### 2. عدم بارگذاری GroupIds, SubChapterIds, StudentIds در حالت Edit
**مسیر:** `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs:108-131`

**مشکل:**
هنگام ویرایش، مدل `CreateScheduleItemRequest` فقط فیلدهای پایه را بارگذاری می‌کند و `GroupIds`, `SubChapterIds`, `StudentIds` را از DTO نمی‌گیرد.

**نتیجه:** در فرم ویرایش، گروه‌ها و زیرمباحث انتخاب شده نمایش داده نمی‌شوند

**کد مشکل‌دار:**
```csharp
// خط 119-131
model = new CreateScheduleItemRequest
{
    TeachingPlanId = teachingPlanId,
    Title = scheduleItem.Value.Title,
    // ... سایر فیلدها
    Type = scheduleItem.Value.Type,
    DisciplineHint = scheduleItem.Value.DisciplineHint
    // ❌ GroupIds, SubChapterIds, StudentIds بارگذاری نمی‌شوند
    // در حالی که scheduleItem.Value این مقادیر را دارد
};
```

---

### 3. عدم ارسال GroupIds/SubChapterIds/StudentIds در Update
**مسیر:** `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs:160-184`

**مشکل:**
در متد POST `CreateOrEdit` هنگام به‌روزرسانی، `GroupIds`, `SubChapterIds`, `StudentIds` به `UpdateScheduleItemCommand` ارسال نمی‌شوند.

**کد مشکل‌دار:**
```csharp
// خط 163-184
var updateRequest = new UpdateScheduleItemRequest
{
    Id = id,
    Title = request.Title,
    // ... فیلدهای دیگر
    MaxScore = request.MaxScore
    // ❌ GroupIds, SubChapterIds, StudentIds وجود ندارند
};

var updateCommand = new UpdateScheduleItemCommand(
    updateRequest.Id,
    // ...
    updateRequest.MaxScore
    // ❌ GroupIds, SubChapterIds ارسال نمی‌شوند
);
```

---

### 4. نبود StudentIds در UpdateScheduleItemCommand
**مسیر:** `src/EduTrack.Application/Features/ScheduleItems/Commands/ScheduleItemCommands.cs:25-36`

**مشکل:**
`UpdateScheduleItemCommand` فیلد `StudentIds` ندارد، در حالی که `CreateScheduleItemCommand` دارد.

**کد مشکل‌دار:**
```csharp
// خط 25-36
public record UpdateScheduleItemCommand(
    int Id,
    // ...
    decimal? MaxScore,
    List<int>? GroupIds = null,
    List<int>? SubChapterIds = null
    // ❌ StudentIds وجود ندارد
) : IRequest<Result>;
```

**مقایسه با Create:**
```csharp
public record CreateScheduleItemCommand(
    // ...
    List<int>? GroupIds = null,
    List<int>? SubChapterIds = null,
    List<string>? StudentIds = null // ✅ وجود دارد
) : IRequest<Result<int>>;
```

---

### 5. عدم پردازش StudentIds در UpdateScheduleItemCommandHandler
**مسیر:** `src/EduTrack.Application/Features/ScheduleItems/CommandHandlers/UpdateScheduleItemCommandHandler.cs`

**مشکل:**
Handler فقط `GroupIds` و `SubChapterIds` را پردازش می‌کند و `StudentIds` را ندارد.

---

## ⚠️ مشکلات مهم (Important Issues)

### 6. نبود Type و DisciplineHint در UpdateScheduleItemCommand
**مسیر:** `src/EduTrack.Application/Features/ScheduleItems/Commands/ScheduleItemCommands.cs:25-36`

**مشکل:**
`UpdateScheduleItemCommand` فیلدهای `Type` و `DisciplineHint` ندارد، اما ممکن است در برخی سناریوها نیاز به تغییر آن‌ها باشد.

**توصیه:** اگر این فیلدها نباید تغییر کنند، در Handler بررسی شود که تغییر نکرده‌اند.

---

### 7. نبود StudentIds در UpdateScheduleItemRequest
**مسیر:** `src/EduTrack.Application/Common/Models/ScheduleItems/ScheduleItemModels.cs:73-92`

**مشکل:**
مدل `UpdateScheduleItemRequest` فیلد `StudentIds` ندارد.

**کد مشکل‌دار:**
```csharp
// خط 73-92
public class UpdateScheduleItemRequest
{
    // ...
    public List<int>? GroupIds { get; set; }
    public List<int>? SubChapterIds { get; set; }
    // ❌ StudentIds وجود ندارد
}
```

---

### 8. متد Edit تکراری و ناسازگار
**مسیر:** `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs:235-283`

**مشکل:**
دو متد `Edit` وجود دارد:
- GET `Edit` (خط 235): فقط به `CreateOrEdit` ریدایرکت می‌کند
- POST `Edit` (خط 247): منطق مستقل دارد و با فرم چند مرحله‌ای سازگار نیست

**نتیجه:** سردرگمی و احتمال باگ در به‌روزرسانی

**توصیه:** متد POST `Edit` حذف شود و همه چیز از `CreateOrEdit` مدیریت شود.

---

## 🔵 مشکلات جزئی (Minor Issues)

### 9. استفاده از Console.WriteLine به جای Logger
**مسیر:** `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs:341`

```csharp
Console.WriteLine($"CreateScheduleItem request: TeachingPlanId={request.TeachingPlanId}...");
```

**توصیه:** از `_logger` استفاده شود.

---

### 10. نبود مدیریت خطا برای بارگذاری GroupAssignments در Edit
**مسیر:** `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs:108-144`

هنگام بارگذاری برای ویرایش، اگر خطایی در بارگذاری Assignments رخ دهد، مدیریت نمی‌شود.

---

### 11. عدم اعتبارسنجی TeachingPlanId در POST CreateOrEdit
**مسیر:** `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs:149`

هنگام POST، بررسی نمی‌شود که `request.TeachingPlanId` با `teachingPlanId` از query string همخوانی دارد.

---

## 📋 خلاصه اولویت‌بندی شده

### اولویت 1 (فوری):
1. ✅ افزودن بررسی دسترسی معلم در CreateOrEdit
2. ✅ بارگذاری GroupIds/SubChapterIds/StudentIds در Edit mode
3. ✅ ارسال GroupIds/SubChapterIds/StudentIds به UpdateCommand
4. ✅ افزودن StudentIds به UpdateScheduleItemCommand و Handler

### اولویت 2 (مهم):
5. ✅ افزودن StudentIds به UpdateScheduleItemRequest
6. ✅ حذف یا اصلاح متد POST Edit تکراری
7. ✅ جایگزینی Console.WriteLine با Logger

### اولویت 3 (بهبودی):
8. ✅ مدیریت خطا بهتر
9. ✅ اعتبارسنجی‌های بیشتر

---

## 🔍 فایل‌های نیازمند تغییر

1. `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs`
2. `src/EduTrack.Application/Features/ScheduleItems/Commands/ScheduleItemCommands.cs`
3. `src/EduTrack.Application/Common/Models/ScheduleItems/ScheduleItemModels.cs`
4. `src/EduTrack.Application/Features/ScheduleItems/CommandHandlers/UpdateScheduleItemCommandHandler.cs`


