# Step 4 Content Managers Structure

## Overview
ساختار منظم و قابل توسعه برای مدیریت محتوای مرحله 4 آیتم‌های آموزشی

## Directory Structure

```
schedule-items/
├── base/                          # کلاس‌های پایه مشترک
│   └── question-block-base.js    # توابع مشترک برای question blocks
├── managers/                      # Managerهای اختصاصی هر نوع آیتم
│   ├── step4-content-manager.js  # Manager اصلی مرحله 4 (هماهنگ‌کننده)
│   ├── written-content-manager.js    # Manager محتوای نوشتاری (Writing, MultipleChoice)
│   ├── audio-content-manager.js      # Manager محتوای صوتی (Audio)
│   ├── reminder-content-manager.js   # Manager محتوای یادآوری (Reminder)
│   └── gapfill-content-manager.js    # Manager محتوای جای خالی (GapFill)
├── content-builder.js             # کلاس پایه ContentBuilderBase
├── content-manager-base.js        # FieldManager, EventManager, PreviewManager, ContentSyncManager
└── content-sidebar.js             # مدیریت sidebar مشترک
```

## Architecture

### 1. Base Classes (کدهای مشترک)
- **ContentBuilderBase**: کلاس پایه برای مدیریت بلاک‌ها
- **QuestionBlockBase**: توابع مشترک برای question blocks (convertToQuestionType, initializeCKEditorForBlock, collectQuestionFields, etc.)
- **FieldManager**: مدیریت فیلدهای فرم
- **EventManager**: مدیریت رویدادهای سفارشی
- **PreviewManager**: تولید پیش‌نمایش محتوا
- **ContentSyncManager**: همگام‌سازی محتوا

### 2. Content Type Managers (کدهای اختصاصی)
هر manager فقط کدهای اختصاصی نوع محتوای خود را دارد:

#### WrittenContentManager
- مدیریت محتوای نوشتاری (Writing, MultipleChoice)
- تبدیل بلاک‌های معمولی به question blocks
- مدیریت CKEditor برای بلاک‌های متنی
- پشتیبانی از MultipleChoice mode

#### AudioContentManager
- مدیریت محتوای صوتی (Audio)
- استفاده از ساختار مشابه WrittenContentManager
- تبدیل بلاک‌ها به question blocks

#### ReminderContentManager
- مدیریت محتوای یادآوری
- استفاده از بلاک‌های معمولی (بدون question)

#### GapFillContentManager
- مدیریت محتوای جای خالی
- فقط از بلاک‌های questionText استفاده می‌کند
- مدیریت gap tokens ([[blank1]], [[blank2]], ...)
- قابلیت تعریف چندگزینه‌ای برای هر بلاک
- دیزاین مینیمال و کاربرپسند

### 3. Step4ContentManager (Manager اصلی)
- هماهنگی بین managerهای مختلف
- مدیریت تغییر نوع محتوا
- بارگذاری و ذخیره داده‌ها
- اعتبارسنجی محتوا

## Principles

1. **Single Responsibility**: هر فایل فقط یک مسئولیت دارد
2. **DRY**: کدهای مشترک در فایل‌های base قرار می‌گیرند
3. **Separation of Concerns**: کدهای اختصاصی از کدهای مشترک جدا هستند
4. **Extensibility**: افزودن نوع جدید محتوا آسان است
5. **Minimal Code Duplication**: استفاده از QuestionBlockBase برای کدهای مشترک

## Content Type Mapping

| ScheduleItemType | Manager Type | Manager Class |
|-----------------|--------------|---------------|
| Reminder (0) | reminder | ReminderContentManager |
| Writing (1) | written | WrittenContentManager |
| Audio (2) | audio | AudioContentManager |
| GapFill (3) | gapfill | GapFillContentManager |
| MultipleChoice (4) | written | WrittenContentManager (with multipleChoiceMode) |

## Usage

### Initialization
```javascript
// در _Step4Content.cshtml
<script src="~/js/areas/teacher/schedule-items/base/question-block-base.js"></script>
<script src="~/js/areas/teacher/schedule-items/managers/step4-content-manager.js"></script>

// در schedule-item-form.js
if (typeof Step4ContentManager !== 'undefined') {
    window.step4Manager = new Step4ContentManager(formManager);
}
```

### Adding New Content Type

1. ایجاد manager جدید در `managers/`:
```javascript
class NewContentManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'newContentBlocksList',
            emptyStateId: 'newEmptyState',
            previewId: 'newPreview',
            hiddenFieldId: 'newContentJson',
            contentType: 'new'
        });
    }
    
    // Implement required methods
    loadExistingContent() { /* ... */ }
    collectContentData() { /* ... */ }
}
```

2. ثبت در Step4ContentManager:
```javascript
registerContentManagers() {
    this.contentManagers.set('5', 'new'); // Type ID -> Manager Type
}
```

3. افزودن به _Step4Content.cshtml:
```html
@if (Model.Type == ScheduleItemType.NewType)
{
    <div id="newContentBuilder">
        @await Html.PartialAsync("_Step4NewContent", Model)
    </div>
}
```

## GapFill Content Manager Features

### Gap Fill Editor
- درج توکن‌های جای‌خالی: `[[blank1]]`, `[[blank2]]`, ...
- تنظیم نوع تصحیح: دقیق، مشابه، کلیدواژه
- تنظیم حساسیت به حروف بزرگ/کوچک
- تعریف پاسخ صحیح و پاسخ‌های جایگزین
- افزودن راهنمایی برای هر جای‌خالی

### Multiple Choice Questions
- افزودن یک یا چند سوال چندگزینه‌ای برای هر بلاک
- نوع پاسخ: تک‌گزینه‌ای یا چندپاسخه
- به‌هم‌ریختن گزینه‌ها
- مدیریت گزینه‌ها (افزودن، ویرایش، حذف)

### Design Features
- دیزاین مینیمال و تمیز
- استفاده از رنگ‌های مناسب برای هر بخش
- Responsive برای موبایل
- افکت‌های hover و transition

## Migration Notes

### فایل‌های حذف شده (Deprecated)
- `written-content-builder.js` → `managers/written-content-manager.js`
- `reminder-content-builder.js` → `managers/reminder-content-manager.js`
- `audio-content-manager.js` → `managers/audio-content-manager.js` (جدید)
- `gap-fill-content-builder.js` → `managers/gapfill-content-manager.js`
- `gap-fill-block-manager.js` → حذف (ادغام در gapfill-content-manager.js)
- `unified-content-loader.js` → ادغام در `step4-content-manager.js`
- `step4-content.js` → `managers/step4-content-manager.js`
- `multiple-choice-content-builder.js` → حذف (استفاده از written-content-manager با mode)

### فایل‌های نگه‌داری شده
- `content-builder.js` - کلاس پایه ContentBuilderBase
- `content-manager-base.js` - کلاس‌های مشترک
- `content-sidebar.js` - مدیریت sidebar

### فایل‌های جدید
- `base/question-block-base.js` - توابع مشترک برای question blocks
- `managers/audio-content-manager.js` - Manager محتوای صوتی
- `managers/gapfill-content-manager.js` - Manager محتوای جای خالی کامل
