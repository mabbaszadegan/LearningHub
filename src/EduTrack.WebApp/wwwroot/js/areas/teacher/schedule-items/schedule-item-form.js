/**
 * Schedule Item Form JavaScript
 * Handles form interactions and content design for different item types
 */

class ScheduleItemFormManager {
    constructor() {
        this.currentType = null;
        this.contentData = {};
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.loadInitialData();
    }

    setupEventListeners() {
        // Item type change
        const itemTypeSelect = document.getElementById('itemType');
        if (itemTypeSelect) {
            itemTypeSelect.addEventListener('change', (e) => {
                this.changeItemType(parseInt(e.target.value));
            });
        }

        // Form submission
        const createForm = document.getElementById('createScheduleItemForm');
        if (createForm) {
            createForm.addEventListener('submit', (e) => {
                this.handleFormSubmit(e, 'create');
            });
        }

        const editForm = document.getElementById('editScheduleItemForm');
        if (editForm) {
            editForm.addEventListener('submit', (e) => {
                this.handleFormSubmit(e, 'edit');
            });
        }
    }

    loadInitialData() {
        // Load groups and lessons if needed
        this.loadGroups();
        this.loadLessons();
    }

    async loadGroups() {
        try {
            // You can implement group loading here
            // For now, we'll add a placeholder
            const groupSelect = document.getElementById('groupId');
            if (groupSelect) {
                groupSelect.innerHTML = '<option value="">همه گروه‌ها</option>';
            }
        } catch (error) {
            console.error('Error loading groups:', error);
        }
    }

    async loadLessons() {
        try {
            // You can implement lesson loading here
            // For now, we'll add a placeholder
            const lessonSelect = document.getElementById('lessonId');
            if (lessonSelect) {
                lessonSelect.innerHTML = '<option value="">انتخاب درس...</option>';
            }
        } catch (error) {
            console.error('Error loading lessons:', error);
        }
    }

    changeItemType(type) {
        this.currentType = type;
        this.showContentDesign(type);
    }

    showContentDesign(type) {
        const contentSection = document.getElementById('contentDesignSection');
        const contentContainer = document.getElementById('contentDesignContainer');
        
        if (!contentSection || !contentContainer) return;

        if (type && type !== 0) { // 0 is Reminder, doesn't need content design
            contentSection.style.display = 'block';
            contentContainer.innerHTML = this.getContentDesign(type);
            contentContainer.classList.add('has-content');
        } else {
            contentSection.style.display = 'none';
            contentContainer.innerHTML = '';
            contentContainer.classList.remove('has-content');
        }
    }

    getContentDesign(type) {
        switch (type) {
            case 1: // Writing
                return this.getWritingDesign();
            case 2: // Audio
                return this.getAudioDesign();
            case 3: // Gap Fill
                return this.getGapFillDesign();
            case 4: // Multiple Choice
                return this.getMultipleChoiceDesign();
            case 5: // Matching
                return this.getMatchingDesign();
            case 6: // Error Finding
                return this.getErrorFindingDesign();
            case 7: // Code Exercise
                return this.getCodeExerciseDesign();
            case 8: // Quiz
                return this.getQuizDesign();
            default:
                return '<p class="text-muted">محتوای این نوع آیتم نیازی به طراحی خاص ندارد.</p>';
        }
    }

    getWritingDesign() {
        return `
            <div class="item-design writing-design active">
                <div class="design-header">
                    <h6><i class="fas fa-pen"></i> طراحی تمرین نوشتاری</h6>
                </div>
                
                <div class="problem-section">
                    <div class="form-group">
                        <label class="form-label">صورت مسئله:</label>
                        <textarea class="form-control" id="writingPrompt" rows="4" placeholder="صورت مسئله را اینجا بنویسید..."></textarea>
                    </div>
                </div>

                <div class="settings-section">
                    <div class="settings-row">
                        <div class="form-group">
                            <label class="form-label">حد کلمات:</label>
                            <input type="number" class="form-control" id="wordLimit" min="50" max="2000" value="200" placeholder="200">
                        </div>
                        <div class="form-group">
                            <label class="form-label">دستورالعمل‌ها:</label>
                            <textarea class="form-control" id="writingInstructions" rows="2" placeholder="دستورالعمل‌های اضافی..."></textarea>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getAudioDesign() {
        return `
            <div class="item-design audio-design active">
                <div class="design-header">
                    <h6><i class="fas fa-volume-up"></i> طراحی تمرین صوتی</h6>
                </div>
                
                <div class="instruction-section">
                    <div class="form-group">
                        <label class="form-label">دستورالعمل:</label>
                        <textarea class="form-control" id="audioInstruction" rows="3" placeholder="دستورالعمل تمرین صوتی..."></textarea>
                    </div>
                </div>

                <div class="audio-section">
                    <div class="form-group">
                        <label class="form-label">فایل صوتی:</label>
                        <input type="file" class="form-control" id="audioFile" accept="audio/*">
                        <small class="form-text">فرمت‌های پشتیبانی شده: MP3, WAV, OGG</small>
                    </div>
                </div>

                <div class="settings-section">
                    <div class="settings-row">
                        <div class="form-group">
                            <label class="form-label">مدت زمان (ثانیه):</label>
                            <input type="number" class="form-control" id="audioDuration" min="10" max="600" value="60" placeholder="60">
                        </div>
                        <div class="form-group">
                            <div class="form-check">
                                <input type="checkbox" class="form-check-input" id="allowRecording">
                                <label class="form-check-label" for="allowRecording">اجازه ضبط پاسخ</label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getGapFillDesign() {
        return `
            <div class="item-design gap-fill-design active">
                <div class="design-header">
                    <h6><i class="fas fa-edit"></i> طراحی تمرین پر کردن جای خالی</h6>
                </div>
                
                <div class="text-section">
                    <div class="form-group">
                        <label class="form-label">متن اصلی:</label>
                        <div class="gap-fill-editor">
                            <div class="editor-toolbar">
                                <button type="button" class="btn btn-sm btn-outline-secondary" onclick="scheduleItemFormManager.addGap()">
                                    <i class="fas fa-square"></i> افزودن جای خالی
                                </button>
                                <button type="button" class="btn btn-sm btn-outline-info" onclick="scheduleItemFormManager.previewGapFill()">
                                    <i class="fas fa-eye"></i> پیش‌نمایش
                                </button>
                            </div>
                            <div class="editor-content" id="gapFillEditor" contenteditable="true">
                                متن خود را اینجا بنویسید و برای ایجاد جای خالی روی دکمه "افزودن جای خالی" کلیک کنید.
                            </div>
                        </div>
                    </div>
                </div>

                <div class="gaps-section">
                    <div class="gaps-header">
                        <label class="form-label">جای‌های خالی:</label>
                        <span class="badge bg-primary" id="gapCount">0 جای خالی</span>
                    </div>
                    
                    <div class="gaps-container" id="gapsContainer">
                        <!-- جای‌های خالی اینجا نمایش داده می‌شوند -->
                    </div>
                </div>

                <div class="settings-section">
                    <div class="settings-row">
                        <div class="form-group">
                            <label class="form-label">نوع پاسخ:</label>
                            <select class="form-select" id="gapAnswerType">
                                <option value="exact">دقیق</option>
                                <option value="similar">مشابه</option>
                                <option value="keyword">کلمه کلیدی</option>
                            </select>
                        </div>
                        <div class="form-group">
                            <div class="form-check">
                                <input type="checkbox" class="form-check-input" id="caseSensitive">
                                <label class="form-check-label" for="caseSensitive">حساسیت به حروف</label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getMultipleChoiceDesign() {
        return `
            <div class="item-design multiple-choice-design active">
                <div class="design-header">
                    <h6><i class="fas fa-list-ul"></i> طراحی سوال چند گزینه‌ای</h6>
                </div>
                
                <div class="question-section">
                    <div class="form-group">
                        <label class="form-label">متن سوال:</label>
                        <textarea class="form-control" id="questionText" rows="3" placeholder="سوال خود را اینجا بنویسید..."></textarea>
                    </div>
                </div>

                <div class="options-section">
                    <div class="options-header">
                        <label class="form-label">گزینه‌ها:</label>
                        <button type="button" class="add-option-btn" onclick="scheduleItemFormManager.addOption()">
                            <i class="fas fa-plus"></i> افزودن گزینه
                        </button>
                    </div>
                    
                    <div class="options-container" id="optionsContainer">
                        <!-- گزینه‌ها اینجا اضافه می‌شوند -->
                    </div>
                </div>

                <div class="settings-section">
                    <div class="settings-row">
                        <div class="form-group">
                            <label class="form-label">نوع پاسخ:</label>
                            <select class="form-select" id="answerType">
                                <option value="single">تک انتخابی</option>
                                <option value="multiple">چند انتخابی</option>
                            </select>
                        </div>
                        <div class="form-group">
                            <div class="form-check">
                                <input type="checkbox" class="form-check-input" id="randomizeOptions">
                                <label class="form-check-label" for="randomizeOptions">ترتیب تصادفی گزینه‌ها</label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getMatchingDesign() {
        return `
            <div class="item-design matching-design active">
                <div class="design-header">
                    <h6><i class="fas fa-link"></i> طراحی تمرین تطبیق</h6>
                </div>
                
                <div class="matching-section">
                    <div class="matching-columns">
                        <div class="matching-column">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <label class="form-label">ستون چپ (مطابقت‌ها):</label>
                                <button type="button" class="btn btn-sm btn-outline-primary" onclick="scheduleItemFormManager.addLeftItem()">
                                    <i class="fas fa-plus"></i>
                                </button>
                            </div>
                            <div class="matching-items" id="leftItems">
                                <!-- آیتم‌های چپ -->
                            </div>
                        </div>
                        
                        <div class="matching-column">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <label class="form-label">ستون راست (پاسخ‌ها):</label>
                                <button type="button" class="btn btn-sm btn-outline-primary" onclick="scheduleItemFormManager.addRightItem()">
                                    <i class="fas fa-plus"></i>
                                </button>
                            </div>
                            <div class="matching-items" id="rightItems">
                                <!-- آیتم‌های راست -->
                            </div>
                        </div>
                    </div>
                </div>

                <div class="connections-section">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <label class="form-label">اتصالات:</label>
                        <button type="button" class="btn btn-sm btn-outline-success" onclick="scheduleItemFormManager.addConnection()">
                            <i class="fas fa-link"></i> افزودن اتصال
                        </button>
                    </div>
                    
                    <div class="connections-container" id="connectionsContainer">
                        <!-- اتصالات اینجا نمایش داده می‌شوند -->
                    </div>
                </div>

                <div class="preview-section">
                    <button type="button" class="btn btn-outline-info" onclick="scheduleItemFormManager.previewMatching()">
                        <i class="fas fa-eye"></i> پیش‌نمایش تمرین تطبیق
                    </button>
                </div>
            </div>
        `;
    }

    getErrorFindingDesign() {
        return `
            <div class="item-design error-finding-design active">
                <div class="design-header">
                    <h6><i class="fas fa-search"></i> طراحی تمرین پیدا کردن خطا</h6>
                </div>
                
                <div class="text-section">
                    <div class="form-group">
                        <label class="form-label">متن حاوی خطا:</label>
                        <textarea class="form-control" id="errorText" rows="8" placeholder="متن حاوی خطا را اینجا بنویسید..."></textarea>
                    </div>
                </div>

                <div class="errors-section">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <label class="form-label">خطاهای موجود:</label>
                        <button type="button" class="btn btn-sm btn-outline-primary" onclick="scheduleItemFormManager.addError()">
                            <i class="fas fa-plus"></i> افزودن خطا
                        </button>
                    </div>
                    
                    <div class="errors-container" id="errorsContainer">
                        <!-- خطاها اینجا نمایش داده می‌شوند -->
                    </div>
                </div>

                <div class="settings-section">
                    <div class="form-group">
                        <div class="form-check">
                            <input type="checkbox" class="form-check-input" id="showLineNumbers">
                            <label class="form-check-label" for="showLineNumbers">نمایش شماره خطوط</label>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getCodeExerciseDesign() {
        return `
            <div class="item-design code-exercise-design active">
                <div class="design-header">
                    <h6><i class="fas fa-code"></i> طراحی تمرین برنامه‌نویسی</h6>
                </div>
                
                <div class="problem-section">
                    <div class="form-group">
                        <label class="form-label">صورت مسئله:</label>
                        <textarea class="form-control" id="problemStatement" rows="4" placeholder="صورت مسئله را اینجا بنویسید..."></textarea>
                    </div>
                </div>

                <div class="code-section">
                    <div class="form-group">
                        <label class="form-label">زبان برنامه‌نویسی:</label>
                        <select class="form-select" id="programmingLanguage">
                            <option value="javascript">JavaScript</option>
                            <option value="python">Python</option>
                            <option value="java">Java</option>
                            <option value="csharp">C#</option>
                            <option value="cpp">C++</option>
                            <option value="sql">SQL</option>
                        </select>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label">کد اولیه (اختیاری):</label>
                        <div class="code-editor-container">
                            <div class="code-editor-toolbar">
                                <button type="button" class="btn btn-sm btn-outline-secondary" onclick="scheduleItemFormManager.formatCode()">
                                    <i class="fas fa-magic"></i> فرمت
                                </button>
                                <button type="button" class="btn btn-sm btn-outline-info" onclick="scheduleItemFormManager.runCode()">
                                    <i class="fas fa-play"></i> اجرا
                                </button>
                            </div>
                            <textarea class="code-editor" id="initialCode" rows="10" placeholder="کد اولیه را اینجا بنویسید..."></textarea>
                        </div>
                    </div>
                </div>

                <div class="test-cases-section">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <label class="form-label">تست کیس‌ها:</label>
                        <button type="button" class="btn btn-sm btn-outline-primary" onclick="scheduleItemFormManager.addTestCase()">
                            <i class="fas fa-plus"></i> افزودن تست کیس
                        </button>
                    </div>
                    
                    <div class="test-cases-container" id="testCasesContainer">
                        <!-- تست کیس‌ها اینجا نمایش داده می‌شوند -->
                    </div>
                </div>

                <div class="settings-section">
                    <div class="settings-row">
                        <div class="form-group">
                            <label class="form-label">زمان محدود (دقیقه):</label>
                            <input type="number" class="form-control" id="timeLimit" min="1" max="120" value="30">
                        </div>
                        <div class="form-group">
                            <label class="form-label">امتیاز:</label>
                            <input type="number" class="form-control" id="codeScore" min="1" max="100" value="10">
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getQuizDesign() {
        return `
            <div class="item-design quiz-design active">
                <div class="design-header">
                    <h6><i class="fas fa-question-circle"></i> طراحی کویز</h6>
                </div>
                
                <div class="quiz-info-section">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="form-group">
                                <label class="form-label">عنوان کویز:</label>
                                <input type="text" class="form-control" id="quizTitle" placeholder="عنوان کویز...">
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-group">
                                <label class="form-label">زمان محدود (دقیقه):</label>
                                <input type="number" class="form-control" id="quizTimeLimit" min="5" max="180" value="30">
                            </div>
                        </div>
                    </div>
                    
                    <div class="form-group">
                        <label class="form-label">توضیحات کویز:</label>
                        <textarea class="form-control" id="quizDescription" rows="2" placeholder="توضیحات کویز..."></textarea>
                    </div>
                </div>

                <div class="questions-section">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <label class="form-label">سوالات کویز:</label>
                        <button type="button" class="btn btn-sm btn-outline-primary" onclick="scheduleItemFormManager.addQuizQuestion()">
                            <i class="fas fa-plus"></i> افزودن سوال
                        </button>
                    </div>
                    
                    <div class="questions-container" id="questionsContainer">
                        <!-- سوالات اینجا نمایش داده می‌شوند -->
                    </div>
                </div>

                <div class="settings-section">
                    <div class="settings-row">
                        <div class="form-group">
                            <div class="form-check">
                                <input type="checkbox" class="form-check-input" id="randomizeQuestions">
                                <label class="form-check-label" for="randomizeQuestions">ترتیب تصادفی سوالات</label>
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="form-check">
                                <input type="checkbox" class="form-check-input" id="showResultsImmediately">
                                <label class="form-check-label" for="showResultsImmediately">نمایش نتایج فوری</label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Methods for handling different item types
    addOption() {
        const container = document.getElementById('optionsContainer');
        if (!container) return;

        const optionIndex = container.children.length;
        const optionHtml = `
            <div class="option-item" data-index="${optionIndex}">
                <input type="radio" name="correctAnswer" value="${optionIndex}" class="option-radio">
                <input type="text" class="option-text" placeholder="متن گزینه...">
                <button type="button" class="option-remove" onclick="scheduleItemFormManager.removeOption(${optionIndex})">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', optionHtml);
    }

    removeOption(index) {
        const option = document.querySelector(`[data-index="${index}"]`);
        if (option) {
            option.remove();
        }
    }

    addGap() {
        const editor = document.getElementById('gapFillEditor');
        if (!editor) return;

        const gapHtml = '<span class="gap-placeholder" contenteditable="false" data-gap-index="' + Date.now() + '">____</span>';
        editor.insertAdjacentHTML('beforeend', gapHtml);
        
        this.updateGapCount();
    }

    updateGapCount() {
        const gapCount = document.getElementById('gapCount');
        const gaps = document.querySelectorAll('.gap-placeholder');
        if (gapCount) {
            gapCount.textContent = `${gaps.length} جای خالی`;
        }
    }

    addLeftItem() {
        // Implementation for adding left items in matching exercise
    }

    addRightItem() {
        // Implementation for adding right items in matching exercise
    }

    addConnection() {
        // Implementation for adding connections in matching exercise
    }

    addError() {
        // Implementation for adding errors in error finding exercise
    }

    addTestCase() {
        // Implementation for adding test cases in code exercise
    }

    addQuizQuestion() {
        // Implementation for adding questions in quiz
    }

    previewGapFill() {
        // Implementation for previewing gap fill exercise
    }

    previewMatching() {
        // Implementation for previewing matching exercise
    }

    formatCode() {
        // Implementation for formatting code
    }

    runCode() {
        // Implementation for running code
    }

    handleFormSubmit(e, formType) {
        e.preventDefault();
        
        // Collect form data
        const formData = new FormData(e.target);
        const data = Object.fromEntries(formData.entries());
        
        // Collect content data based on item type
        if (this.currentType) {
            data.ContentJson = this.collectContentData();
        }
        
        // Submit form
        this.submitForm(data, formType);
    }

    collectContentData() {
        const contentData = {
            type: this.currentType
        };

        switch (this.currentType) {
            case 1: // Writing
                contentData.prompt = document.getElementById('writingPrompt')?.value || '';
                contentData.wordLimit = parseInt(document.getElementById('wordLimit')?.value) || 200;
                contentData.instructions = document.getElementById('writingInstructions')?.value || '';
                break;
                
            case 2: // Audio
                contentData.instruction = document.getElementById('audioInstruction')?.value || '';
                contentData.duration = parseInt(document.getElementById('audioDuration')?.value) || 60;
                contentData.allowRecording = document.getElementById('allowRecording')?.checked || false;
                break;
                
            case 3: // Gap Fill
                contentData.text = document.getElementById('gapFillEditor')?.innerHTML || '';
                contentData.answerType = document.getElementById('gapAnswerType')?.value || 'exact';
                contentData.caseSensitive = document.getElementById('caseSensitive')?.checked || false;
                break;
                
            case 4: // Multiple Choice
                contentData.question = document.getElementById('questionText')?.value || '';
                contentData.answerType = document.getElementById('answerType')?.value || 'single';
                contentData.randomizeOptions = document.getElementById('randomizeOptions')?.checked || false;
                contentData.options = this.collectOptions();
                break;
                
            case 7: // Code Exercise
                contentData.problemStatement = document.getElementById('problemStatement')?.value || '';
                contentData.programmingLanguage = document.getElementById('programmingLanguage')?.value || 'javascript';
                contentData.initialCode = document.getElementById('initialCode')?.value || '';
                contentData.timeLimit = parseInt(document.getElementById('timeLimit')?.value) || 30;
                break;
                
            case 8: // Quiz
                contentData.title = document.getElementById('quizTitle')?.value || '';
                contentData.description = document.getElementById('quizDescription')?.value || '';
                contentData.timeLimit = parseInt(document.getElementById('quizTimeLimit')?.value) || 30;
                contentData.randomizeQuestions = document.getElementById('randomizeQuestions')?.checked || false;
                contentData.showResultsImmediately = document.getElementById('showResultsImmediately')?.checked || false;
                break;
        }

        return JSON.stringify(contentData);
    }

    collectOptions() {
        const options = [];
        const optionItems = document.querySelectorAll('.option-item');
        
        optionItems.forEach((item, index) => {
            const text = item.querySelector('.option-text')?.value || '';
            const isCorrect = item.querySelector('.option-radio')?.checked || false;
            
            if (text.trim()) {
                options.push({
                    index: index,
                    text: text,
                    isCorrect: isCorrect
                });
            }
        });
        
        return options;
    }

    async submitForm(data, formType) {
        try {
            const url = formType === 'create' 
                ? '/Teacher/ScheduleItem/CreateScheduleItem'
                : '/Teacher/ScheduleItem/UpdateScheduleItem';
                
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data)
            });
            
            const result = await response.json();
            
            if (result.success) {
                this.showSuccess(formType === 'create' ? 'آیتم آموزشی با موفقیت ایجاد شد' : 'آیتم آموزشی با موفقیت به‌روزرسانی شد');
                this.closeModal();
            } else {
                this.showError('خطا: ' + result.message);
            }
        } catch (error) {
            console.error('Error submitting form:', error);
            this.showError('خطا در ارسال فرم');
        }
    }

    closeModal() {
        // Close the current modal
        const modals = document.querySelectorAll('.modal.show');
        modals.forEach(modal => {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
        });
    }

    showSuccess(message) {
        // You can implement a toast notification system here
        alert(message);
    }

    showError(message) {
        // You can implement a toast notification system here
        alert(message);
    }
}

// Initialize the form manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.scheduleItemFormManager = new ScheduleItemFormManager();
});
