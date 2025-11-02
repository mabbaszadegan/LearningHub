<!-- 0a7e8400-dd18-45ed-9069-94cdba4d0b8f 2b9c948c-86bf-4200-97b3-c52f0d793a43 -->
# پلن یکپارچه‌سازی مرحله 4 - ساختار محتوای آموزشی

## اهداف اصلی

1. یک View واحد برای همه انواع ScheduleItem
2. ساختار ماژولار با Partial View و کامپوننت‌های JS جداگانه
3. حذف کامل کد Script/Style از فایل‌های CSHTML
4. استایل‌های ماژولار و یکسان
5. حذف فایل‌های قدیمی و اضافی

---

## فاز 1: حذف فایل‌های قدیمی

### فایل‌های View برای حذف:

- `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/_Step4GapFillContent.cshtml`
- `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/_Step4MultipleChoiceContent.cshtml`
- `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/_Step4ReminderContent.cshtml`
- `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/_Step4WrittenContent.cshtml`

### فایل‌های JavaScript Manager برای حذف:

- `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/managers/reminder-content-manager.js`
- `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/managers/written-content-manager.js`
- `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/managers/audio-content-manager.js`
- `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/managers/gapfill-content-manager.js`
- `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/managers/multiplechoice-content-manager.js`
- `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/managers/step4-content-manager.js`

### فایل‌های CSS برای حذف:

- `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/gapfill-content-builder.css`
- `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/multiple-choice-content-builder.css`
- `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/reminder-content-builder.css`
- `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/written-content-builder.css`
- `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/stage4-enhancements.css` (در صورت نیاز به بررسی)

---

## فاز 2: ایجاد ساختار جدید - Partial Views

### 2.1 Partial View اصلی Step4:

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/_Step4Content.cshtml`

- بدون هیچ کد `<script>` یا `<style>`
- فقط شامل لینک‌های CSS و Script
- استفاده از Partial View‌های ماژولار

### 2.2 Partial View‌های ساختار اصلی:

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/_Step4ContentBuilder.cshtml`

- ساختار اصلی builder (header, sidebar, main area)
- Empty state
- بدون logic

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/_Step4ContentHeader.cshtml`

- Header بخش Step4
- دکمه‌های action (افزودن بلاک، پیش‌نمایش)

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/_Step4ContentSidebar.cshtml`

- Sidebar برای navigation بلاک‌ها
- Counter و فهرست

### 2.3 Partial View‌های بلاک‌های سوال (Question Type Blocks):

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/Blocks/_MultipleChoiceBlock.cshtml`

- Template بلاک چندگزینه‌ای
- بدون script/style inline

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/Blocks/_GapFillBlock.cshtml`

- Template بلاک جای‌خالی
- بدون script/style inline

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/Blocks/_OrderingBlock.cshtml`

- Template بلاک مرتب‌سازی (جدید)
- بدون script/style inline

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/Blocks/_MatchingBlock.cshtml`

- Template بلاک تطبیقی (برای آینده)
- بدون script/style inline

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/Blocks/_ErrorFindingBlock.cshtml`

- Template بلاک پیدا کردن خطا (برای آینده)
- بدون script/style inline

### 2.4 Partial View‌های کامپوننت:

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/Components/_EmptyState.cshtml`

- Empty state قابل استفاده مجدد
- Props: icon, title, description

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/Components/_BlockCard.cshtml`

- Wrapper برای هر بلاک
- Header, content area, actions

### 2.5 بهبود Partial View موجود:

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/Shared/_BlockTypeOptions.cshtml`

- اضافه کردن بخش Question Type Blocks
- پارامتر `questionTypeBlocks` برای لیست انواع سوال
- جدا کردن Regular Blocks و Question Blocks و Question Type Blocks

---

## فاز 3: ساختار CSS ماژولار

### 3.1 CSS Variables و Base Styles:

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/variables.css`

- CSS Variables برای رنگ‌ها، فاصله‌ها، سایزها
- Typography scale
- Spacing scale
- Shadow definitions

### 3.2 CSS Components:

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/components/buttons.css`

- استایل‌های یکسان برای همه دکمه‌ها
- Variants: primary, secondary, danger, success
- Sizes: sm, md, lg

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/components/headers.css`

- استایل یکسان برای headerها
- Step header, builder header, block header

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/components/sidebar.css`

- استایل یکسان برای sidebarها
- Navigation, counter, list styles

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/components/block-card.css`

- استایل یکسان برای کارت‌های بلاک
- Header, content, actions area

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/components/empty-state.css`

- استایل یکسان برای empty state
- Icon, title, description

### 3.3 CSS Layouts:

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/layouts/content-builder-layout.css`

- Layout اصلی builder
- Grid/Flex برای main area + sidebar

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/layouts/blocks-list.css`

- Layout برای لیست بلاک‌ها
- Spacing, drag & drop styles

### 3.4 CSS برای بلاک‌های خاص:

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/blocks/multiple-choice-block.css`

- استایل اختصاصی بلاک چندگزینه‌ای

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/blocks/gapfill-block.css`

- استایل اختصاصی بلاک جای‌خالی

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/blocks/ordering-block.css`

- استایل اختصاصی بلاک مرتب‌سازی

### 3.5 CSS Main:

**فایل:** `src/EduTrack.WebApp/wwwroot/css/areas/teacher/schedule-items/step4-content.css`

- Import همه فایل‌های CSS بالا
- Order: variables → components → layouts → blocks
- Override minimal

---

## فاز 4: ساختار JavaScript ماژولار

### 4.1 Base Classes:

**فایل موجود:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/content-builder.js`

- نگه‌داشتن ContentBuilderBase
- بهبود و cleanup

**فایل موجود:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/content-manager-base.js`

- نگه‌داشتن FieldManager, EventManager, etc.
- بهبود و cleanup

### 4.2 Unified Manager:

**فایل جدید:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/managers/unified-content-manager.js`

- Manager یکپارچه برای همه انواع
- تشخیص نوع آیتم از select
- Route کردن به handler مناسب
- مدیریت lifecycle

### 4.3 Block Type Handlers:

**فایل جدید:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/blocks/handlers/regular-block-handler.js`

- Handle کردن بلاک‌های معمولی (text, image, video, audio, code)
- استفاده از ContentBuilderBase

**فایل جدید:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/blocks/handlers/question-block-handler.js`

- Handle کردن بلاک‌های سوال (questionText, questionImage, etc.)
- استفاده از QuestionBlockBase

**فایل جدید:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/blocks/handlers/multiple-choice-handler.js`

- Handle کردن بلاک چندگزینه‌ای
- Initialize, render, collect data

**فایل جدید:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/blocks/handlers/gapfill-handler.js`

- Handle کردن بلاک جای‌خالی
- Gap editor, token insertion

**فایل جدید:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/blocks/handlers/ordering-handler.js`

- Handle کردن بلاک مرتب‌سازی
- Drag & drop, correct order setup

### 4.4 Configuration:

**فایل جدید:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/config/block-type-config.js`

- Configuration برای هر نوع آیتم
- چه بلاک‌هایی مجاز هستند
- Structure: { itemType: { regularBlocks: [], questionBlocks: [], questionTypeBlocks: [] } }

### 4.5 Utilities:

**فایل جدید:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/schedule-items/utils/content-migrator.js`

- تبدیل ساختار قدیمی به جدید
- برای backward compatibility در loadExistingContent

### 4.6 بهبود فایل‌های موجود:

**فایل:** `src/EduTrack.WebApp/wwwroot/js/areas/teacher/shared/block-type-selection-modal.js`

- استفاده از block-type-config.js
- پشتیبانی از questionTypeBlocks

---

## فاز 5: بهبود Controller

### 5.1 Update Action Method:

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Controllers/ScheduleItemController.cs`

**Method:** `GetBlockTypeOptions`

- اضافه کردن پارامتر `questionTypeBlocks`
- پاس دادن به ViewData

---

## فاز 6: به‌روزرسانی فایل‌های اصلی

### 6.1 Update CreateOrEdit:

**فایل:** `src/EduTrack.WebApp/Areas/Teacher/Views/ScheduleItem/CreateOrEdit.cshtml`

- بررسی که از `_Step4Content` جدید استفاده می‌کند

### 6.2 Update ScheduleItemType Enum:

**فایل:** `src/EduTrack.Domain/Enums/ScheduleItemType.cs`

- اضافه کردن `Ordering = 9` (اگر لازم باشد)

---

## فاز 7: Testing و Validation

### 7.1 بررسی موارد زیر:

- همه انواع ScheduleItem کار می‌کنند
- بلاک‌ها درست render می‌شوند
- Data collection درست است
- Preview کار می‌کند
- Save/Load درست است
- Backward compatibility (داده‌های قدیمی)

---

## ساختار نهایی فایل‌ها

```
Views/ScheduleItem/
├── _Step4Content.cshtml (اصلی - فقط imports)
├── _Step4ContentBuilder.cshtml
├── _Step4ContentHeader.cshtml
├── _Step4ContentSidebar.cshtml
└── Blocks/
    ├── _MultipleChoiceBlock.cshtml
    ├── _GapFillBlock.cshtml
    ├── _OrderingBlock.cshtml
    ├── _MatchingBlock.cshtml
    └── _ErrorFindingBlock.cshtml
└── Components/
    ├── _EmptyState.cshtml
    └── _BlockCard.cshtml

wwwroot/css/areas/teacher/schedule-items/
├── step4-content.css (main)
├── variables.css
├── components/
│   ├── buttons.css
│   ├── headers.css
│   ├── sidebar.css
│   ├── block-card.css
│   └── empty-state.css
├── layouts/
│   ├── content-builder-layout.css
│   └── blocks-list.css
└── blocks/
    ├── multiple-choice-block.css
    ├── gapfill-block.css
    └── ordering-block.css

wwwroot/js/areas/teacher/schedule-items/
├── managers/
│   └── unified-content-manager.js
├── blocks/handlers/
│   ├── regular-block-handler.js
│   ├── question-block-handler.js
│   ├── multiple-choice-handler.js
│   ├── gapfill-handler.js
│   └── ordering-handler.js
├── config/
│   └── block-type-config.js
└── utils/
    └── content-migrator.js
```

---

## نکات مهم پیاده‌سازی

1. **Backward Compatibility**: در unified-content-manager.js، هنگام loadExistingContent، ساختار قدیمی را به جدید تبدیل کنیم
2. **Event System**: استفاده از EventManager موجود برای communication بین components
3. **State Management**: Single source of truth در unified-content-manager
4. **Error Handling**: Try-catch در همه handlers
5. **Performance**: Lazy loading برای handlers
6. **Accessibility**: ARIA labels و keyboard navigation
7. **RTL Support**: همه استایل‌ها RTL-ready باشند

### To-dos

- [ ] 