# گزارش تحلیل صفحه ایجاد/ویرایش آیتم آموزشی

## 📋 خلاصه بررسی

این گزارش حاصل بررسی دقیق مکانیزم ایجاد و ویرایش `ScheduleItem` و رفتار انواع مختلف آیتم‌ها است.

---

## 🔍 ساختار فعلی

### مراحل فرم (Steps)

1. **Step 1: اطلاعات کلی**
   - عنوان آیتم (Title)
   - نوع آیتم (Type) - **9 نوع مختلف**
   - توضیحات (Description) - Rich Text Editor

2. **Step 2: زمان‌بندی**
   - تاریخ شروع (StartDate)
   - تاریخ پایان (DueDate)
   - نمره (MaxScore)
   - اجباری بودن (IsMandatory)

3. **Step 3: تخصیص**
   - انتخاب گروه‌ها (GroupIds)
   - انتخاب زیرمباحث (SubChapterIds)
   - انتخاب دانش‌آموزان (StudentIds)

4. **Step 4: محتوای آموزشی**
   - بسته به نوع آیتم، builder متفاوتی نمایش داده می‌شود

---

## 📊 انواع آیتم و رفتار هر کدام

### ✅ Reminder (نوع 0)
**Builder**: `_Step4ReminderContent.cshtml`
- **بلاک‌های مجاز**: Text, Image, Video, Audio, Code, QuestionBlocks
- **ویژگی**: می‌تواند بلاک‌های سوالی (Question Blocks) هم داشته باشد
- **Validation**: حداقل یک بلاک محتوا الزامی است
- **وضعیت**: ✅ پیاده‌سازی کامل

### ✅ Writing (نوع 1)
**Builder**: `_Step4WrittenContent.cshtml`
- **بلاک‌های مجاز**: QuestionBlocks (Text, Image, Video, Audio, Code)
- **ویژگی**: تمرین نوشتاری با سوالات چندگانه
- **Validation**: حداقل یک سوال الزامی است
- **وضعیت**: ✅ پیاده‌سازی کامل

### ⚠️ Audio (نوع 2)
**Builder**: `_Step4WrittenContent.cshtml` (استفاده از همان builder نوشتاری)
- **بلاک‌های مجاز**: QuestionBlocks
- **ویژگی**: تمرین صوتی - اما از builder نوشتاری استفاده می‌کند
- **مشکل**: ممکن است نیاز به builder خاص داشته باشد
- **وضعیت**: ⚠️ نیاز به بررسی

### ✅ GapFill (نوع 3)
**Builder**: `_Step4GapFillContent.cshtml` + `_Step4WrittenContent.cshtml`
- **بلاک‌های مجاز**: Gap Fill Blocks (متن با جای‌خالی)
- **ویژگی**: هر سوال متنی باید حداقل یک جای‌خالی داشته باشد
- **Validation**: 
  - حداقل یک سوال جای‌خالی الزامی است
  - هر سوال متنی باید حداقل یک gap داشته باشد
- **وضعیت**: ✅ پیاده‌سازی کامل

### ✅ MultipleChoice (نوع 4)
**Builder**: `_Step4WrittenContent.cshtml` + `_Step4MultipleChoiceContent.cshtml`
- **بلاک‌های مجاز**: MCQ Blocks (سوالات چندگزینه‌ای)
- **ویژگی**: 
  - می‌تواند Context Blocks (Text, Image, Video, Audio) داشته باشد
  - هر MCQ باید گزینه‌های صحیح داشته باشد
- **Validation**: حداقل یک بلاک محتوا الزامی است
- **وضعیت**: ✅ پیاده‌سازی کامل

### ❌ Match (نوع 5)
**Builder**: `contentBuilder` عمومی (در حال توسعه)
- **وضعیت**: ❌ هنوز پیاده‌سازی نشده

### ❌ ErrorFinding (نوع 6)
**Builder**: `contentBuilder` عمومی (در حال توسعه)
- **وضعیت**: ❌ هنوز پیاده‌سازی نشده

### ❌ CodeExercise (نوع 7)
**Builder**: `contentBuilder` عمومی (در حال توسعه)
- **وضعیت**: ❌ هنوز پیاده‌سازی نشده

### ❌ Quiz (نوع 8)
**Builder**: `contentBuilder` عمومی (در حال توسعه)
- **وضعیت**: ❌ هنوز پیاده‌سازی نشده

---

## 🐛 مشکلات شناسایی شده

### 1. ⚠️ تغییر نوع آیتم در Edit Mode

**مشکل**: در Edit Mode، کاربر می‌تواند نوع آیتم را تغییر دهد اما:
- اگر ContentJson برای نوع قبلی باشد و نوع جدید متفاوت باشد، باعث خطا می‌شود
- هیچ هشداری به کاربر نشان داده نمی‌شود
- محتوای قبلی ممکن است از دست برود

**راه‌حل پیشنهادی**:
```javascript
// در step1-basics.js
changeItemType(typeId) {
    // اگر در Edit Mode هستیم و محتوا وجود دارد
    if (this.formManager?.isEditMode && this.formManager?.existingItemData?.contentJson) {
        const currentType = this.currentType;
        if (currentType !== null && currentType !== typeId) {
            // نمایش هشدار
            const confirmed = await this.confirmTypeChange();
            if (!confirmed) {
                // بازگشت به نوع قبلی
                itemTypeSelect.value = currentType;
                return;
            }
        }
    }
    // ادامه تغییر نوع
}
```

### 2. ⚠️ عدم غیرفعال‌سازی Type Select در Edit Mode

**مشکل**: Type Select در Edit Mode هنوز قابل تغییر است که می‌تواند مشکل ایجاد کند.

**راه‌حل**:
```html
<!-- در _Step1BasicInfo.cshtml -->
<select asp-for="Type" 
        class="form-select-modern" 
        id="itemType" 
        required
        disabled="@isEditMode"
        title="@(isEditMode ? "نوع آیتم پس از ایجاد قابل تغییر نیست" : "")">
```

### 3. ⚠️ عدم Validation برای DueDate

**مشکل**: در Entity و View، بررسی نمی‌شود که DueDate بعد از StartDate باشد.

**راه‌حل**: اضافه کردن validation در:
- Domain Entity (`UpdateDates` method)
- JavaScript validation در Step 2
- Client-side validation

### 4. ⚠️ Content Builder برای انواع 5, 6, 7, 8

**مشکل**: انواع Match, ErrorFinding, CodeExercise, Quiz هنوز builder ندارند.

**وضعیت فعلی**: فقط پیام "در حال توسعه" نمایش داده می‌شود.

**پیشنهاد**: 
- یا builder برای آنها پیاده‌سازی شود
- یا از لیست نوع‌ها حذف شوند (موقتاً)

### 5. ⚠️ Audio Type از Written Builder استفاده می‌کند

**مشکل**: نوع Audio از همان builder نوشتاری استفاده می‌کند که ممکن است مناسب نباشد.

**پیشنهاد**: بررسی شود آیا نیاز به builder خاص دارد یا نه.

### 6. ⚠️ GapFill دو Builder دارد

**مشکل**: GapFill هم از `gapFillContentBuilder` و هم از `writtenContentBuilder` استفاده می‌کند که ممکن است گیج‌کننده باشد.

**وضعیت**: این کار عمدی است - gapFill برای ساخت متن با جای‌خالی از written builder هم استفاده می‌کند.

---

## ✅ بهبودهای اعمال شده

### 1. ✅ View ها به‌روزرسانی شدند
- Script های جدید (Utils, Services, API) اضافه شدند
- ترتیب لود شدن فایل‌ها درست شد

### 2. ✅ API Calls یکپارچه شدند
- همه fetch ها به API Service منتقل شدند
- Fallback برای سازگاری با کدهای قدیمی

### 3. ✅ Notification System یکپارچه شد
- همه alert/confirm به Modal Service منتقل شدند
- Toast notifications یکپارچه شد

### 4. ✅ Code Organization بهبود یافت
- Namespace مناسب ایجاد شد (`window.EduTrack`)
- Utilities قابل استفاده مجدد شدند

---

## 🔧 پیشنهادات بهبود

### 1. اضافه کردن Type Change Warning

```javascript
// در step1-basics.js
async changeItemType(newTypeId) {
    const oldType = this.currentType;
    
    // اگر در Edit Mode هستیم و محتوا وجود دارد
    if (this.formManager?.isEditMode && this.formManager?.existingItemData?.contentJson) {
        const contentJson = this.formManager.existingItemData.contentJson;
        
        if (contentJson && contentJson !== '{}') {
            const modal = window.EduTrack?.Services?.Modal;
            const confirmed = modal 
                ? await modal.confirm(
                    'تغییر نوع آیتم باعث از دست رفتن محتوای فعلی می‌شود. آیا ادامه می‌دهید؟',
                    'هشدار تغییر نوع',
                    'بله، ادامه',
                    'انصراف'
                )
                : confirm('تغییر نوع آیتم باعث از دست رفتن محتوای فعلی می‌شود. آیا ادامه می‌دهید؟');
            
            if (!confirmed) {
                // بازگشت به نوع قبلی
                const itemTypeSelect = document.getElementById('itemType');
                if (itemTypeSelect) {
                    itemTypeSelect.value = oldType || '';
                }
                return;
            }
            
            // پاک کردن محتوای قبلی
            if (window.step4Manager) {
                window.step4Manager.clearContent();
            }
        }
    }
    
    // ادامه تغییر نوع
    this.currentType = newTypeId;
    this.showTypePreview(newTypeId);
    
    // به‌روزرسانی Step 4 Content
    if (window.step4Manager) {
        window.step4Manager.updateStep4Content();
    }
}
```

### 2. غیرفعال کردن Type Select در Edit Mode

```html
<!-- در _Step1BasicInfo.cshtml -->
<div class="col-md-4">
    <div class="form-group-modern">
        <label asp-for="Type" class="form-label-modern">
            <i class="fas fa-tasks"></i>
            <span>نوع آیتم</span>
            <span class="required-indicator">*</span>
            @if (isEditMode)
            {
                <span class="badge bg-warning ms-2">غیرقابل تغییر</span>
            }
        </label>
        <select asp-for="Type" 
                class="form-select-modern" 
                id="itemType" 
                required
                disabled="@isEditMode"
                title="@(isEditMode ? "نوع آیتم پس از ایجاد قابل تغییر نیست. برای تغییر نوع باید آیتم جدید ایجاد کنید." : "")">
            <option value="">انتخاب نوع آیتم...</option>
            @foreach (dynamic type in scheduleItemTypes)
            {
                <option value="@type.Value" data-description="@type.Description">
                    @type.Text
                </option>
            }
        </select>
        @if (isEditMode)
        {
            <div class="input-hint text-warning">
                <i class="fas fa-info-circle"></i>
                نوع آیتم پس از ایجاد قابل تغییر نیست
            </div>
        }
        <span asp-validation-for="Type" class="validation-error-modern"></span>
    </div>
</div>
```

### 3. اضافه کردن Date Validation

```javascript
// در step2-timing.js
validateDates() {
    const startDateInput = document.getElementById('persianStartDate');
    const dueDateInput = document.getElementById('persianDueDate');
    
    if (!startDateInput || !dueDateInput) return true;
    
    const startDate = this.parsePersianDate(startDateInput.value);
    const dueDate = this.parsePersianDate(dueDateInput.value);
    
    if (startDate && dueDate && dueDate < startDate) {
        const validation = window.EduTrack?.Validation;
        if (validation) {
            const result = validation.validateDateRange(startDate, dueDate);
            if (!result.isValid) {
                this.showFieldError('persianDueDate', result.error);
                return false;
            }
        } else {
            this.showFieldError('persianDueDate', 'تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد');
            return false;
        }
    }
    
    return true;
}
```

### 4. بهبود Validation در Step 4

```javascript
// در step4-content.js
validateStep4() {
    const itemTypeSelect = document.getElementById('itemType');
    const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';
    
    // Clear all previous errors
    this.fieldManager.clearAllErrors();
    
    // Validate based on type with better error messages
    switch(selectedType) {
        case '0': // Reminder
            return this.validateReminderContent();
        case '1': // Writing
            return this.validateWrittenContent();
        case '3': // GapFill
            return this.validateGapFillContent();
        case '4': // MultipleChoice
            return this.validateMultipleChoiceContent();
        default:
            // For types without specific builder, just check if contentJson is not empty
            const contentJson = this.fieldManager.getFieldValue('contentJson');
            if (!contentJson || contentJson === '{}') {
                this.fieldManager.showFieldError('contentJson', 
                    `محتوای آموزشی برای نوع ${this.getItemTypeName(selectedType)} الزامی است`);
                return false;
            }
            return true;
    }
}
```

### 5. اضافه کردن Content Type Indicator

```html
<!-- در _Step4Content.cshtml -->
<div class="content-type-indicator" id="contentTypeIndicator">
    <div class="indicator-badge">
        <i class="fas fa-info-circle"></i>
        <span id="currentContentType">نوع محتوا: <strong>یادآوری</strong></span>
    </div>
    <div class="indicator-help">
        <small id="contentTypeHelp">می‌توانید بلاک‌های متن، تصویر، ویدیو، صوت و کد اضافه کنید</small>
    </div>
</div>
```

---

## 📝 چک‌لیست بررسی

### Step 1 ✅
- [x] Validation برای Title
- [x] Validation برای Type
- [x] Rich Text Editor برای Description
- [ ] ⚠️ غیرفعال‌سازی Type در Edit Mode (نیاز به پیاده‌سازی)
- [ ] ⚠️ هشدار تغییر نوع در Edit Mode (نیاز به پیاده‌سازی)

### Step 2 ✅
- [x] Persian Date Picker
- [x] Validation برای MaxScore
- [ ] ⚠️ Validation برای DueDate > StartDate (نیاز به پیاده‌سازی)

### Step 3 ✅
- [x] انتخاب گروه‌ها
- [x] انتخاب زیرمباحث
- [x] انتخاب دانش‌آموزان
- [x] Validation برای حداقل یک زیرمبحث

### Step 4 ✅
- [x] Reminder Builder (کامل)
- [x] Written Builder (کامل)
- [x] GapFill Builder (کامل)
- [x] MultipleChoice Builder (کامل)
- [ ] ❌ Match Builder (در حال توسعه)
- [ ] ❌ ErrorFinding Builder (در حال توسعه)
- [ ] ❌ CodeExercise Builder (در حال توسعه)
- [ ] ❌ Quiz Builder (در حال توسعه)
- [ ] ⚠️ Audio Builder (استفاده از Written - نیاز به بررسی)

---

## 🎯 اولویت‌بندی مشکلات

### 🔴 بالا (باید سریع رفع شود)
1. غیرفعال‌سازی Type Select در Edit Mode
2. اضافه کردن Date Validation (DueDate > StartDate)
3. هشدار تغییر نوع در Edit Mode

### 🟡 متوسط (بهتر است رفع شود)
4. بهبود Error Messages
5. Content Type Indicator
6. بررسی Audio Builder

### 🟢 پایین (می‌توان بعداً انجام داد)
7. پیاده‌سازی Builder های باقی‌مانده (Match, ErrorFinding, CodeExercise, Quiz)
8. بهبود UX در تغییر نوع

---

## 📌 نتیجه‌گیری

سیستم به طور کلی خوب کار می‌کند اما:
- ✅ View ها به‌روزرسانی شدند
- ✅ API Calls یکپارچه شدند
- ✅ Notification System بهبود یافت
- ⚠️ نیاز به غیرفعال‌سازی Type در Edit Mode
- ⚠️ نیاز به Date Validation
- ⚠️ نیاز به هشدار تغییر نوع

تمام تغییرات backward compatible هستند و کدهای قدیمی همچنان کار می‌کنند.

