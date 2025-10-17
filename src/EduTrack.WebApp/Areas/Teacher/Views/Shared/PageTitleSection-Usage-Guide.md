# PageTitleSection - راهنمای استفاده

## معرفی
`PageTitleSection` یک partial view مدرن و مینیمال است که برای نمایش یکنواخت عنوان صفحات، breadcrumb و توضیحات در پنل معلم استفاده می‌شود.

## ویژگی‌ها
- ✅ Breadcrumb قابل کلیک و دقیق
- ✅ عنوان صفحه با استایل مدرن و مینیمال
- ✅ توضیحات متناظر با هر صفحه
- ✅ دکمه‌های عملیات قابل تنظیم
- ✅ طراحی ریسپانسیو
- ✅ پشتیبانی از accessibility
- ✅ انیمیشن‌های نرم و زیبا

## نحوه استفاده در Controller

### 1. اضافه کردن using statement
```csharp
using EduTrack.WebApp.Extensions;
```

### 2. تنظیم PageTitleSection در Action
```csharp
public async Task<IActionResult> Index()
{
    // کدهای معمول controller...
    
    // Setup page title section
    var breadcrumbItems = new List<BreadcrumbItem>
    {
        CreateBreadcrumbItem("خانه", Url.Action("Index", "Home"), "fas fa-home"),
        CreateBreadcrumbItem("دوره‌ها", Url.Action("Index", "Courses"), "fas fa-book"),
        CreateBreadcrumbItem("مدیریت دوره‌ها", null, "fas fa-book", true)
    };

    var actions = new List<PageAction>
    {
        CreatePageAction("دوره جدید", Url.Action("Create"), "btn-primary", "fas fa-plus"),
        CreatePageAction("بازگشت", Url.Action("Index", "Home"), "btn-secondary", "fas fa-arrow-right")
    };

    SetPageTitleSection(
        title: "مدیریت دوره‌ها",
        titleIcon: "fas fa-book",
        description: "مدیریت و سازماندهی دوره‌های آموزشی شما",
        breadcrumbItems: breadcrumbItems,
        actions: actions
    );
    
    return View(model);
}
```

## پارامترهای SetPageTitleSection

| پارامتر | نوع | توضیحات |
|---------|-----|---------|
| `title` | string | عنوان اصلی صفحه |
| `titleIcon` | string? | آیکون عنوان (اختیاری) |
| `description` | string? | توضیحات صفحه (اختیاری) |
| `breadcrumbItems` | List<BreadcrumbItem>? | لیست breadcrumb (اختیاری) |
| `actions` | List<PageAction>? | لیست دکمه‌های عملیات (اختیاری) |

## ایجاد BreadcrumbItem

```csharp
CreateBreadcrumbItem(
    text: "متن نمایشی",
    url: "URL یا null برای آیتم فعال",
    icon: "کلاس آیکون FontAwesome",
    isActive: false // true برای آیتم فعلی
)
```

## ایجاد PageAction

```csharp
CreatePageAction(
    text: "متن دکمه",
    url: "URL یا #",
    cssClass: "btn-primary", // btn-primary, btn-secondary, btn-success, btn-danger, btn-warning
    icon: "کلاس آیکون FontAwesome",
    isModal: false, // true اگر دکمه modal باز کند
    modalTarget: "modal-id" // ID modal (اختیاری)
)
```

## مثال‌های کاربردی

### صفحه لیست دوره‌ها
```csharp
var breadcrumbItems = new List<BreadcrumbItem>
{
    CreateBreadcrumbItem("خانه", Url.Action("Index", "Home"), "fas fa-home"),
    CreateBreadcrumbItem("دوره‌ها", null, "fas fa-book", true)
};

var actions = new List<PageAction>
{
    CreatePageAction("دوره جدید", Url.Action("Create"), "btn-primary", "fas fa-plus")
};

SetPageTitleSection(
    title: "مدیریت دوره‌ها",
    titleIcon: "fas fa-book",
    description: "مدیریت و سازماندهی دوره‌های آموزشی شما",
    breadcrumbItems: breadcrumbItems,
    actions: actions
);
```

### صفحه ایجاد آیتم آموزشی
```csharp
var breadcrumbItems = new List<BreadcrumbItem>
{
    CreateBreadcrumbItem("خانه", Url.Action("Index", "Home"), "fas fa-home"),
    CreateBreadcrumbItem("دوره‌ها", Url.Action("Index", "Courses"), "fas fa-book"),
    CreateBreadcrumbItem(courseTitle, Url.Action("Index", "TeachingPlan", new { courseId }), "fas fa-graduation-cap"),
    CreateBreadcrumbItem(teachingPlanTitle, Url.Action("Index", "TeachingPlan", new { id = teachingPlanId }), "fas fa-calendar-alt"),
    CreateBreadcrumbItem("آیتم‌های آموزشی", Url.Action("Index", "ScheduleItem", new { teachingPlanId }), "fas fa-tasks"),
    CreateBreadcrumbItem("ایجاد آیتم آموزشی جدید", null, "fas fa-plus", true)
};

var actions = new List<PageAction>
{
    CreatePageAction("بازگشت", Url.Action("Index", "ScheduleItem", new { teachingPlanId }), "btn-secondary", "fas fa-arrow-right")
};

SetPageTitleSection(
    title: "ایجاد آیتم آموزشی جدید",
    titleIcon: "fas fa-plus-circle",
    description: $"ایجاد آیتم آموزشی جدید برای برنامه \"{teachingPlanTitle}\"",
    breadcrumbItems: breadcrumbItems,
    actions: actions
);
```

## کلاس‌های CSS موجود

### دکمه‌های عملیات
- `btn-primary` - آبی اصلی
- `btn-secondary` - خاکستری
- `btn-success` - سبز
- `btn-danger` - قرمز
- `btn-warning` - زرد

### آیکون‌های پیشنهادی
- `fas fa-home` - خانه
- `fas fa-book` - کتاب/دوره
- `fas fa-graduation-cap` - کلاس
- `fas fa-calendar-alt` - برنامه آموزشی
- `fas fa-tasks` - آیتم‌های آموزشی
- `fas fa-plus` - اضافه کردن
- `fas fa-edit` - ویرایش
- `fas fa-arrow-right` - بازگشت
- `fas fa-eye` - مشاهده
- `fas fa-save` - ذخیره

## نکات مهم

1. **حذف Page Header قدیمی**: از صفحات view، بخش‌های page header قدیمی را حذف کنید
2. **استفاده از ViewBag**: همچنان می‌توانید از ViewBag برای داده‌های اضافی استفاده کنید
3. **Responsive Design**: طراحی به صورت خودکار ریسپانسیو است
4. **Accessibility**: تمام المان‌ها دارای ARIA labels مناسب هستند
5. **Performance**: CSS و JavaScript به صورت lazy load بارگذاری می‌شوند

## فایل‌های مرتبط

- `_PageTitleSection.cshtml` - Partial view اصلی
- `PageTitleSectionViewModel.cs` - مدل داده
- `PageTitleSectionExtensions.cs` - متدهای کمکی
- `page-title-section.css` - استایل‌ها
- `page-title-section.js` - JavaScript

## مثال کامل Controller

```csharp
[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class ExampleController : Controller
{
    public async Task<IActionResult> Index(int? id)
    {
        // کدهای معمول...
        
        // Setup page title section
        var breadcrumbItems = new List<BreadcrumbItem>
        {
            CreateBreadcrumbItem("خانه", Url.Action("Index", "Home"), "fas fa-home"),
            CreateBreadcrumbItem("بخش اصلی", Url.Action("Index", "Main"), "fas fa-folder"),
            CreateBreadcrumbItem("صفحه فعلی", null, "fas fa-file", true)
        };

        var actions = new List<PageAction>
        {
            CreatePageAction("عملیات جدید", Url.Action("Create"), "btn-primary", "fas fa-plus"),
            CreatePageAction("بازگشت", Url.Action("Index", "Home"), "btn-secondary", "fas fa-arrow-right")
        };

        SetPageTitleSection(
            title: "عنوان صفحه",
            titleIcon: "fas fa-file",
            description: "توضیحات مربوط به این صفحه",
            breadcrumbItems: breadcrumbItems,
            actions: actions
        );
        
        return View(model);
    }
}
```

این سیستم به شما امکان ایجاد صفحات یکنواخت، مدرن و کاربرپسند را می‌دهد.
