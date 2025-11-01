# EduTrack JavaScript Architecture

## ساختار جدید

پروژه به صورت ماژولار و قابل استفاده مجدد بازنویسی شده است.

## فایل‌های Core

### 1. Utilities (`js/utils/`)
- **common.js**: توابع مشترک (debounce, throttle, deepClone, etc.)
- **validation.js**: توابع اعتبارسنجی
- **dom-helpers.js**: توابع کمک‌کننده DOM

### 2. API Services (`js/api/`)
- **schedule-item-api.js**: API calls برای Schedule Items
- **schedule-api.js**: API calls برای Groups و SubChapters
- **student-group-api.js**: API calls برای Student Groups

### 3. Services (`js/services/`)
- **notification-service.js**: سیستم یکپارچه Notification (جایگزین alert/toast)
- **modal-service.js**: سیستم Modal (جایگزین confirm)

## نحوه استفاده

### بارگذاری فایل‌ها در View

```html
@section Scripts {
    <!-- Core Utilities -->
    <script src="~/js/utils/common.js" asp-append-version="true"></script>
    <script src="~/js/utils/validation.js" asp-append-version="true"></script>
    <script src="~/js/utils/dom-helpers.js" asp-append-version="true"></script>
    
    <!-- Services -->
    <script src="~/js/services/notification-service.js" asp-append-version="true"></script>
    <script src="~/js/services/modal-service.js" asp-append-version="true"></script>
    
    <!-- API Services -->
    <script src="~/js/api/schedule-item-api.js" asp-append-version="true"></script>
    <script src="~/js/api/schedule-api.js" asp-append-version="true"></script>
    <script src="~/js/api/student-group-api.js" asp-append-version="true"></script>
    
    <!-- Core Initialization -->
    <script src="~/js/edutrack-core.js" asp-append-version="true"></script>
    
    <!-- Feature-specific scripts -->
    <script src="~/js/areas/teacher/schedule-items/schedule-items.js" asp-append-version="true"></script>
}
```

### مثال استفاده از Notification Service

```javascript
// استفاده از Notification Service
const notification = window.EduTrack?.Services?.Notification;

// نمایش پیام موفقیت
notification.success('عملیات با موفقیت انجام شد');

// نمایش خطا
notification.error('خطایی رخ داد');

// نمایش اطلاعات
notification.info('این یک پیام اطلاعاتی است');

// نمایش هشدار
notification.warning('توجه کنید!');
```

### مثال استفاده از Modal Service

```javascript
// استفاده از Modal Service (جایگزین confirm)
const modal = window.EduTrack?.Services?.Modal;

// نمایش confirm dialog
const confirmed = await modal.confirm(
    'آیا از حذف این آیتم اطمینان دارید؟',
    'حذف آیتم',
    'بله، حذف کن',
    'انصراف'
);

if (confirmed) {
    // عملیات حذف
}

// نمایش alert (جایگزین alert)
await modal.alert(
    'عملیات با موفقیت انجام شد',
    'موفقیت',
    'success' // نوع: success, error, warning, info
);
```

### مثال استفاده از API Services

```javascript
// استفاده از API Service
const api = window.EduTrack?.API?.ScheduleItem;

// دریافت لیست آیتم‌ها
const result = await api.getScheduleItems(teachingPlanId);
if (result.success) {
    console.log(result.data);
}

// ایجاد آیتم جدید
const createResult = await api.createScheduleItem({
    teachingPlanId: 1,
    title: 'عنوان',
    type: 0,
    // ...
});

// به‌روزرسانی آیتم
await api.updateScheduleItem({
    id: 1,
    title: 'عنوان جدید',
    // ...
});

// حذف آیتم
await api.deleteScheduleItem(1);
```

### مثال استفاده از Utilities

```javascript
// استفاده از Utils
const Utils = window.EduTrack?.Utils;

// Debounce
const debouncedFunction = Utils.debounce(() => {
    console.log('Debounced!');
}, 300);

// Parse query string
const params = Utils.parseQueryString(window.location.search);
const teachingPlanId = params.teachingPlanId;

// Get CSRF token
const token = Utils.getCsrfToken();

// Deep clone
const cloned = Utils.deepClone(originalObject);
```

### مثال استفاده از Validation

```javascript
// استفاده از Validation
const Validation = window.EduTrack?.Validation;

// Validate email
const isValid = Validation.isValidEmail('test@example.com');

// Validate JSON
const jsonResult = Validation.isValidJson('{"key": "value"}');
if (jsonResult.isValid) {
    console.log(jsonResult.data);
}

// Validate required
const requiredResult = Validation.validateRequired(value, 'فیلد عنوان');
if (!requiredResult.isValid) {
    console.log(requiredResult.error);
}
```

## Backward Compatibility

همه تغییرات backward compatible هستند:
- اگر سرویس‌های جدید موجود نباشند، کد به صورت fallback از روش‌های قبلی استفاده می‌کند
- توابع global مثل `toastSuccess` و `toastError` هنوز کار می‌کنند
- کدهای قدیمی بدون تغییر کار می‌کنند

## Migration Guide

### 1. جایگزینی alert/confirm

**قبل:**
```javascript
alert('پیام');
const confirmed = confirm('آیا مطمئن هستید؟');
```

**بعد:**
```javascript
const modal = window.EduTrack?.Services?.Modal;
await modal.alert('پیام');
const confirmed = await modal.confirm('آیا مطمئن هستید؟');
```

### 2. جایگزینی fetch مستقیم

**قبل:**
```javascript
const response = await fetch('/Teacher/ScheduleItem/GetScheduleItems?teachingPlanId=1');
const result = await response.json();
```

**بعد:**
```javascript
const api = window.EduTrack?.API?.ScheduleItem;
const result = await api.getScheduleItems(1);
```

### 3. جایگزینی showSuccess/showError

**قبل:**
```javascript
showSuccess('موفق');
showError('خطا');
```

**بعد:**
```javascript
const notification = window.EduTrack?.Services?.Notification;
notification.success('موفق');
notification.error('خطا');
```

## Namespace Structure

```
window.EduTrack
├── Utils
│   ├── debounce()
│   ├── throttle()
│   ├── deepClone()
│   └── ...
├── Validation
│   ├── isValidEmail()
│   ├── isValidJson()
│   └── ...
├── DOM
│   ├── querySelector()
│   ├── addEventListener()
│   └── ...
├── API
│   ├── ScheduleItem
│   ├── Schedule
│   └── StudentGroup
└── Services
    ├── Notification
    └── Modal
```

