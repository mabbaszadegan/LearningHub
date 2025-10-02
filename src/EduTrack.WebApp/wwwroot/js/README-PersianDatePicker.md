# Persian DatePicker Component

یک کامپوننت تقویم شمسی مدرن و زیبا برای انتخاب تاریخ در پروژه‌های وب.

## ویژگی‌ها

- ✅ تقویم کاملاً شمسی
- ✅ طراحی مدرن و زیبا
- ✅ پشتیبانی از تاریخ حداقل و حداکثر
- ✅ انیمیشن‌های نرم
- ✅ پشتیبانی از حالت تاریک
- ✅ Responsive Design
- ✅ دکمه‌های "امروز" و "پاک کردن"
- ✅ تبدیل خودکار به تاریخ میلادی

## نحوه استفاده

### 1. اضافه کردن فایل‌های CSS و JS

```html
<!-- در head -->
<link rel="stylesheet" href="~/css/persian-datepicker.css" />

<!-- قبل از closing body -->
<script src="~/js/persian-date.js"></script>
<script src="~/js/persian-datepicker.js"></script>
```

### 2. HTML ساده

```html
<div class="mb-3">
    <label for="birthDate" class="form-label">تاریخ تولد</label>
    <input type="text" id="birthDate" class="form-control persian-datepicker" 
           placeholder="انتخاب تاریخ تولد" />
    <input name="BirthDate" type="hidden" />
</div>
```

### 3. استفاده با گزینه‌های پیشرفته

```html
<input type="text" id="startDate" class="form-control persian-datepicker" 
       data-min-date="1403/01/01" 
       data-max-date="1403/12/29"
       placeholder="انتخاب تاریخ شروع" />
```

### 4. استفاده با JavaScript

```javascript
// ایجاد دستی
const datePicker = new PersianDatePicker(document.getElementById('myDate'), {
    placeholder: 'انتخاب تاریخ',
    minDate: '1403/01/01',
    maxDate: '1403/12/29',
    onSelect: function(date, dateString) {
        console.log('تاریخ انتخاب شده:', dateString);
        // تبدیل به میلادی
        const gregorianDate = window.persianDate.persianStringToGregorianDate(dateString);
        document.getElementById('hiddenDate').value = gregorianDate.toISOString();
    }
});
```

## گزینه‌های موجود

| گزینه | نوع | پیش‌فرض | توضیح |
|--------|-----|---------|-------|
| `placeholder` | string | 'انتخاب تاریخ' | متن placeholder |
| `minDate` | string | null | حداقل تاریخ قابل انتخاب |
| `maxDate` | string | null | حداکثر تاریخ قابل انتخاب |
| `initialDate` | string | null | تاریخ اولیه |
| `onSelect` | function | null | تابع callback هنگام انتخاب |

## مثال‌های کاربردی

### انتخاب بازه تاریخ

```javascript
const startDatePicker = new PersianDatePicker(startInput, {
    onSelect: function(date, dateString) {
        // تنظیم حداقل تاریخ برای تاریخ پایان
        endDatePicker.options.minDate = dateString;
    }
});

const endDatePicker = new PersianDatePicker(endInput, {
    onSelect: function(date, dateString) {
        // بررسی اینکه تاریخ پایان بعد از شروع باشد
        if (startInput.value && dateString < startInput.value) {
            alert('تاریخ پایان باید بعد از تاریخ شروع باشد');
            endDatePicker.clearDate();
        }
    }
});
```

### فرم ثبت‌نام

```html
<form>
    <div class="row">
        <div class="col-md-6">
            <label>تاریخ تولد</label>
            <input type="text" class="form-control persian-datepicker" 
                   data-max-date="1385/12/29" placeholder="تاریخ تولد" />
            <input name="BirthDate" type="hidden" />
        </div>
        <div class="col-md-6">
            <label>تاریخ شروع کار</label>
            <input type="text" class="form-control persian-datepicker" 
                   placeholder="تاریخ شروع کار" />
            <input name="StartWorkDate" type="hidden" />
        </div>
    </div>
</form>
```

## API Methods

```javascript
// تنظیم تاریخ
datePicker.setDate('1403/05/15');

// پاک کردن تاریخ
datePicker.clearDate();

// نمایش تقویم
datePicker.show();

// مخفی کردن تقویم
datePicker.hide();

// انتخاب امروز
datePicker.selectToday();
```

## Styling سفارشی

```css
/* تغییر رنگ اصلی */
.persian-datepicker-calendar {
    --primary-color: #your-color;
}

/* تغییر اندازه */
.persian-datepicker-calendar {
    min-width: 320px;
}

/* استایل سفارشی برای روز انتخاب شده */
.datepicker-day.selected {
    background: linear-gradient(45deg, #ff6b6b, #ee5a24);
}
```

## نکات مهم

1. **وابستگی**: نیاز به `persian-date.js` دارد
2. **Hidden Fields**: برای ارسال به سرور از hidden field استفاده کنید
3. **Validation**: با کتابخانه‌های validation سازگار است
4. **Performance**: تنها یک instance برای هر input ایجاد کنید

## مثال کامل

```html
<!DOCTYPE html>
<html>
<head>
    <link rel="stylesheet" href="persian-datepicker.css">
</head>
<body>
    <form>
        <div class="mb-3">
            <label>تاریخ شروع دوره</label>
            <input type="text" id="courseStart" class="form-control persian-datepicker" />
            <input name="CourseStartDate" type="hidden" />
        </div>
        
        <button type="submit">ثبت</button>
    </form>

    <script src="persian-date.js"></script>
    <script src="persian-datepicker.js"></script>
    <script>
        // خودکار initialize می‌شود
        // یا دستی:
        new PersianDatePicker(document.getElementById('courseStart'), {
            onSelect: function(date, dateString) {
                document.querySelector('input[name="CourseStartDate"]').value = 
                    window.persianDate.persianStringToGregorianDate(dateString).toISOString();
            }
        });
    </script>
</body>
</html>
```
