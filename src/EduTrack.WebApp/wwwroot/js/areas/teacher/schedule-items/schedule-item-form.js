/**
 * Modern Schedule Item Form JavaScript
 * Advanced form management with step-by-step creation and rich content editing
 */

class ModernScheduleItemFormManager {
    constructor() {
        this.currentStep = 1;
        this.totalSteps = 4;
        this.formData = {};
        this.contentTypes = {};
        this.selectedContentType = null;
        this.isPreviewMode = false;
        this.validationErrors = {};
        this.currentItemId = null;
        this.isEditMode = false;
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.setupRichEditor();
        this.setupStepNavigation();
        this.setupFormValidation();
        this.loadContentTypes();
        this.updateProgress();
        this.setupAutoSave();
        this.setupPersianDatePicker();
        this.checkForExistingItem();
    }

    setupEventListeners() {
        // Step navigation
        this.setupStepNavigationListeners();
        
        // Form inputs
        this.setupFormInputListeners();
        
        // Rich editor
        this.setupRichEditorListeners();
        
        // Preview functionality
        this.setupPreviewListeners();
        
        // Content type selection
        this.setupContentTypeListeners();
        
        // Datetime helpers
        this.setupDatetimeHelpers();
        
        // Score presets
        this.setupScorePresets();
    }

    setupStepNavigationListeners() {
        const prevBtn = document.getElementById('prevStepBtn');
        const nextBtn = document.getElementById('nextStepBtn');
        const submitBtn = document.getElementById('submitBtn');

        if (prevBtn) {
            prevBtn.addEventListener('click', () => this.goToPreviousStep());
        }

        if (nextBtn) {
            nextBtn.addEventListener('click', () => this.goToNextStep());
        }

        if (submitBtn) {
            submitBtn.addEventListener('click', (e) => this.handleFormSubmit(e));
        }

        // Sidebar step navigation
        document.querySelectorAll('.step-item').forEach(item => {
            item.addEventListener('click', (e) => {
                const step = parseInt(e.currentTarget.dataset.step);
                if (step <= this.currentStep) {
                    this.goToStep(step);
                }
            });
        });
    }

    setupFormInputListeners() {
        // Title character count
        const titleInput = document.getElementById('itemTitle');
        if (titleInput) {
            titleInput.addEventListener('input', (e) => {
                this.updateCharacterCount(e.target, 100);
                this.validateField('title', e.target.value);
            });
        }

        // Item type selection
        const itemTypeSelect = document.getElementById('itemType');
        if (itemTypeSelect) {
            itemTypeSelect.addEventListener('change', (e) => {
                this.changeItemType(parseInt(e.target.value));
                this.validateField('type', e.target.value);
            });
        }

        // Datetime inputs
        const startDateInput = document.getElementById('startDate');
        const dueDateInput = document.getElementById('dueDate');
        
        if (startDateInput) {
            startDateInput.addEventListener('change', () => {
                this.updateDurationCalculator();
                this.validateField('startDate', startDateInput.value);
            });
        }

        if (dueDateInput) {
            dueDateInput.addEventListener('change', () => {
                this.updateDurationCalculator();
                this.validateField('dueDate', dueDateInput.value);
            });
        }

        // Group and lesson selection
        const groupSelect = document.getElementById('groupId');
        const lessonSelect = document.getElementById('lessonId');

        if (groupSelect) {
            groupSelect.addEventListener('change', () => {
                this.updateAssignmentPreview();
            });
        }

        if (lessonSelect) {
            lessonSelect.addEventListener('change', () => {
                this.updateAssignmentPreview();
            });
        }

        // Mandatory checkbox
        const mandatoryCheckbox = document.getElementById('isMandatory');
        if (mandatoryCheckbox) {
            mandatoryCheckbox.addEventListener('change', () => {
                this.updateAssignmentPreview();
            });
        }
    }

    setupRichEditorListeners() {
        const editor = document.getElementById('descriptionEditor');
        if (!editor) return;

        // Toolbar buttons
        document.querySelectorAll('.toolbar-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                this.executeEditorCommand(e.currentTarget.dataset.command);
            });
        });

        // Editor content changes
        editor.addEventListener('input', () => {
            this.updateHiddenDescription();
            this.validateField('description', editor.innerHTML);
        });

        // Editor focus/blur
        editor.addEventListener('focus', () => {
            editor.parentElement.classList.add('focused');
        });

        editor.addEventListener('blur', () => {
            editor.parentElement.classList.remove('focused');
        });
    }

    setupPreviewListeners() {
        const previewBtn = document.getElementById('previewBtn');
        const previewContentBtn = document.getElementById('previewContentBtn');
        const saveFromPreviewBtn = document.getElementById('saveFromPreviewBtn');

        if (previewBtn) {
            previewBtn.addEventListener('click', () => this.showPreview());
        }

        if (previewContentBtn) {
            previewContentBtn.addEventListener('click', () => this.showContentPreview());
        }

        if (saveFromPreviewBtn) {
            saveFromPreviewBtn.addEventListener('click', () => this.saveFromPreview());
        }
    }

    setupContentTypeListeners() {
        // Content type selection will be set up dynamically
    }

    setupDatetimeHelpers() {
        const setNowBtn = document.getElementById('setNowBtn');
        const setWeekBtn = document.getElementById('setWeekBtn');

        if (setNowBtn) {
            setNowBtn.addEventListener('click', () => {
                const now = new Date();
                const startDateInput = document.getElementById('startDate');
                if (startDateInput) {
                    startDateInput.value = this.formatDateTimeLocal(now);
                    this.updateDurationCalculator();
                }
            });
        }

        if (setWeekBtn) {
            setWeekBtn.addEventListener('click', () => {
                const weekFromNow = new Date();
                weekFromNow.setDate(weekFromNow.getDate() + 7);
                const dueDateInput = document.getElementById('dueDate');
                if (dueDateInput) {
                    dueDateInput.value = this.formatDateTimeLocal(weekFromNow);
                    this.updateDurationCalculator();
                }
            });
        }
    }

    setupScorePresets() {
        document.querySelectorAll('.score-preset').forEach(preset => {
            preset.addEventListener('click', (e) => {
                const score = e.currentTarget.dataset.score;
                const maxScoreInput = document.getElementById('maxScore');
                if (maxScoreInput) {
                    maxScoreInput.value = score;
                    this.updateScorePresets(score);
                }
            });
        });

        const maxScoreInput = document.getElementById('maxScore');
        if (maxScoreInput) {
            maxScoreInput.addEventListener('input', (e) => {
                this.updateScorePresets(e.target.value);
            });
        }
    }

    setupStepNavigation() {
        this.updateStepVisibility();
        this.updateStepIndicators();
    }

    setupFormValidation() {
        // Real-time validation setup
        this.validationRules = {
            title: { required: true, minLength: 3, maxLength: 100 },
            type: { required: true },
            startDate: { required: true },
            description: { maxLength: 1000 }
        };
    }

    setupAutoSave() {
        // Auto-save form data every 30 seconds
        setInterval(() => {
            this.saveFormData();
        }, 30000);
    }

    // Step Navigation Methods
    goToStep(step) {
        if (step < 1 || step > this.totalSteps) return;
        
        // Validate current step before moving
        if (step > this.currentStep && !this.validateCurrentStep()) {
            return;
        }

        this.currentStep = step;
        this.updateStepVisibility();
        this.updateStepIndicators();
        this.updateProgress();
        this.updateNavigationButtons();
    }

    goToNextStep() {
        if (this.currentStep < this.totalSteps) {
            // Save current step before moving to next
            this.saveCurrentStep().then(() => {
                this.goToStep(this.currentStep + 1);
            }).catch(error => {
                console.error('Error saving step:', error);
                this.showErrorMessage('خطا در ذخیره مرحله');
            });
        }
    }

    goToPreviousStep() {
        if (this.currentStep > 1) {
            this.goToStep(this.currentStep - 1);
        }
    }

    updateStepVisibility() {
        document.querySelectorAll('.form-step').forEach((step, index) => {
            step.classList.toggle('active', index + 1 === this.currentStep);
        });

        document.querySelectorAll('.step-item').forEach((item, index) => {
            const stepNumber = index + 1;
            item.classList.remove('active', 'completed');
            
            if (stepNumber === this.currentStep) {
                item.classList.add('active');
            } else if (stepNumber < this.currentStep) {
                item.classList.add('completed');
            }
        });
    }

    updateStepIndicators() {
        // Update sidebar step indicators
        document.querySelectorAll('.step-item').forEach((item, index) => {
            const stepNumber = index + 1;
            const stepNumberEl = item.querySelector('.step-number');
            
            if (stepNumberEl) {
                if (stepNumber < this.currentStep) {
                    stepNumberEl.innerHTML = '<i class="fas fa-check"></i>';
                } else {
                    stepNumberEl.textContent = stepNumber;
                }
            }
        });
    }

    updateProgress() {
        const progressFill = document.getElementById('progressFill');
        const progressText = document.getElementById('progressText');
        
        if (progressFill) {
            const percentage = (this.currentStep / this.totalSteps) * 100;
            progressFill.style.width = `${percentage}%`;
        }
        
        if (progressText) {
            progressText.textContent = `مرحله ${this.currentStep} از ${this.totalSteps}`;
        }
    }

    updateNavigationButtons() {
        const prevBtn = document.getElementById('prevStepBtn');
        const nextBtn = document.getElementById('nextStepBtn');
        const submitBtn = document.getElementById('submitBtn');

        if (prevBtn) {
            prevBtn.disabled = this.currentStep === 1;
        }

        if (nextBtn) {
            nextBtn.style.display = this.currentStep < this.totalSteps ? 'flex' : 'none';
        }

        if (submitBtn) {
            submitBtn.style.display = this.currentStep === this.totalSteps ? 'flex' : 'none';
        }
    }

    // Form Validation Methods
    validateCurrentStep() {
        const stepElement = document.querySelector(`.form-step[data-step="${this.currentStep}"]`);
        if (!stepElement) return true;

        let isValid = true;
        const requiredFields = stepElement.querySelectorAll('[required]');
        
        requiredFields.forEach(field => {
            if (!this.validateField(field.name, field.value)) {
                isValid = false;
            }
        });

        return isValid;
    }

    validateField(fieldName, value) {
        const rules = this.validationRules[fieldName];
        if (!rules) return true;

        let isValid = true;
        let errorMessage = '';

        // Required validation
        if (rules.required && (!value || value.trim() === '')) {
            isValid = false;
            errorMessage = 'این فیلد الزامی است';
        }

        // Length validations
        if (value && rules.minLength && value.length < rules.minLength) {
            isValid = false;
            errorMessage = `حداقل ${rules.minLength} کاراکتر لازم است`;
        }

        if (value && rules.maxLength && value.length > rules.maxLength) {
            isValid = false;
            errorMessage = `حداکثر ${rules.maxLength} کاراکتر مجاز است`;
        }

        // Update validation state
        this.validationErrors[fieldName] = isValid ? null : errorMessage;
        this.updateFieldValidation(fieldName, isValid, errorMessage);

        return isValid;
    }

    updateFieldValidation(fieldName, isValid, errorMessage) {
        const field = document.querySelector(`[name="${fieldName}"]`);
        if (!field) return;

        const errorElement = field.parentElement.querySelector('.validation-error-modern');
        
        if (errorElement) {
            errorElement.textContent = errorMessage || '';
            errorElement.style.display = errorMessage ? 'block' : 'none';
        }

        // Update field styling
        field.classList.toggle('is-invalid', !isValid);
        field.classList.toggle('is-valid', isValid && value);
    }

    // Rich Editor Methods
    setupRichEditor() {
        const editor = document.getElementById('descriptionEditor');
        if (!editor) return;

        // Set up basic editor functionality
        editor.addEventListener('paste', (e) => {
            e.preventDefault();
            const text = e.clipboardData.getData('text/plain');
            document.execCommand('insertText', false, text);
        });
    }

    executeEditorCommand(command) {
        document.execCommand(command, false, null);
        this.updateToolbarState();
    }

    updateToolbarState() {
        document.querySelectorAll('.toolbar-btn').forEach(btn => {
            const command = btn.dataset.command;
            btn.classList.toggle('active', document.queryCommandState(command));
        });
    }

    updateHiddenDescription() {
        const editor = document.getElementById('descriptionEditor');
        const hiddenField = document.getElementById('descriptionHidden');
        
        if (editor && hiddenField) {
            hiddenField.value = editor.innerHTML;
        }
    }

    // Content Type Methods
    async loadContentTypes() {
        try {
            // Load content types based on selected item type
            const itemType = document.getElementById('itemType')?.value;
            if (!itemType) return;

            // This would typically fetch from an API
            this.contentTypes = {
                0: { name: 'یادآوری', description: 'یادآوری ساده برای دانش‌آموزان' },
                1: { name: 'نوشتاری', description: 'تمرین نوشتاری و انشا' },
                2: { name: 'صوتی', description: 'فایل صوتی و پادکست' },
                3: { name: 'پر کردن جای خالی', description: 'سوالات جای خالی' },
                4: { name: 'چند گزینه‌ای', description: 'سوالات تستی' },
                5: { name: 'تطبیق', description: 'تطبیق آیتم‌ها' },
                6: { name: 'پیدا کردن خطا', description: 'شناسایی و تصحیح خطا' },
                7: { name: 'تمرین کد', description: 'برنامه‌نویسی و کدنویسی' },
                8: { name: 'کویز', description: 'آزمون کوتاه' }
            };

            this.showContentTypeSelector();
        } catch (error) {
            console.error('Error loading content types:', error);
        }
    }

    changeItemType(typeId) {
        this.currentType = typeId;
        this.selectedContentType = null;
        
        // Show type preview
        this.showTypePreview(typeId);
        
        // Load content types for this item type
        this.loadContentTypes();
        
        // Update sidebar
        this.updateStepIndicators();
    }

    showTypePreview(typeId) {
        const preview = document.getElementById('typePreview');
        const previewContent = document.getElementById('typePreviewContent');
        
        if (!preview || !previewContent) return;

        const typeInfo = this.contentTypes[typeId];
        if (typeInfo) {
            previewContent.innerHTML = `
                <div class="type-info">
                    <h4>${typeInfo.name}</h4>
                    <p>${typeInfo.description}</p>
                    <div class="type-features">
                        <span class="feature-tag">تعاملی</span>
                        <span class="feature-tag">جذاب</span>
                        <span class="feature-tag">آموزشی</span>
                    </div>
                </div>
            `;
            preview.style.display = 'block';
        } else {
            preview.style.display = 'none';
        }
    }

    showContentTypeSelector() {
        const selector = document.getElementById('contentTypeSelector');
        const grid = document.getElementById('contentTypesGrid');
        
        if (!selector || !grid) return;

        // Clear existing content
        grid.innerHTML = '';

        // Add content type options
        Object.entries(this.contentTypes).forEach(([id, type]) => {
            const typeCard = document.createElement('div');
            typeCard.className = 'content-type-card';
            typeCard.innerHTML = `
                <div class="type-icon">
                    <i class="fas fa-${this.getTypeIcon(id)}"></i>
                </div>
                <div class="type-info">
                    <h4>${type.name}</h4>
                    <p>${type.description}</p>
                </div>
                <button class="select-type-btn" data-type-id="${id}">
                    <i class="fas fa-arrow-left"></i>
                </button>
            `;
            
            typeCard.addEventListener('click', () => {
                this.selectContentType(parseInt(id));
            });
            
            grid.appendChild(typeCard);
        });

        selector.style.display = 'block';
    }

    selectContentType(typeId) {
        this.selectedContentType = typeId;
        
        // Hide selector and show designer
        document.getElementById('contentTypeSelector').style.display = 'none';
        document.getElementById('contentDesigner').style.display = 'block';
        
        // Load content designer for this type
        this.loadContentDesigner(typeId);
    }

    loadContentDesigner(typeId) {
        const container = document.getElementById('contentDesignContainer');
        if (!container) return;

        // Load appropriate designer based on type
        switch (typeId) {
            case 0: // Reminder
                this.loadReminderDesigner(container);
                break;
            case 1: // Writing
                this.loadWritingDesigner(container);
                break;
            case 4: // Multiple Choice
                this.loadMultipleChoiceDesigner(container);
                break;
            case 3: // Gap Fill
                this.loadGapFillDesigner(container);
                break;
            default:
                this.loadGenericDesigner(container);
        }
    }

    loadReminderDesigner(container) {
        container.innerHTML = `
            <div class="designer-section">
                <h4>طراحی یادآوری</h4>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-bell"></i>
                        <span>متن یادآوری</span>
                    </label>
                    <textarea class="form-input-modern" rows="4" placeholder="متن یادآوری را وارد کنید..."></textarea>
                </div>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-calendar"></i>
                        <span>تاریخ یادآوری</span>
                    </label>
                    <input type="datetime-local" class="form-input-modern" />
                </div>
            </div>
        `;
    }

    loadWritingDesigner(container) {
        container.innerHTML = `
            <div class="designer-section">
                <h4>طراحی تمرین نوشتاری</h4>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-question-circle"></i>
                        <span>سوال یا موضوع</span>
                    </label>
                    <textarea class="form-input-modern" rows="3" placeholder="سوال یا موضوع نوشتاری را وارد کنید..."></textarea>
                </div>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-align-left"></i>
                        <span>راهنمایی</span>
                    </label>
                    <textarea class="form-input-modern" rows="3" placeholder="راهنمایی‌های لازم برای دانش‌آموزان..."></textarea>
                </div>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-ruler"></i>
                        <span>حداقل تعداد کلمات</span>
                    </label>
                    <input type="number" class="form-input-modern" placeholder="100" min="0" />
                </div>
            </div>
        `;
    }

    loadMultipleChoiceDesigner(container) {
        container.innerHTML = `
            <div class="designer-section">
                <h4>طراحی سوالات چند گزینه‌ای</h4>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-question-circle"></i>
                        <span>سوال</span>
                    </label>
                    <textarea class="form-input-modern" rows="2" placeholder="سوال را وارد کنید..."></textarea>
                </div>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-list"></i>
                        <span>گزینه‌ها</span>
                    </label>
                    <div class="options-container">
                        <div class="option-item">
                            <input type="radio" name="correctAnswer" value="0" />
                            <input type="text" class="form-input-modern" placeholder="گزینه اول..." />
                            <button type="button" class="remove-option-btn">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                        <div class="option-item">
                            <input type="radio" name="correctAnswer" value="1" />
                            <input type="text" class="form-input-modern" placeholder="گزینه دوم..." />
                            <button type="button" class="remove-option-btn">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                    </div>
                    <button type="button" class="add-option-btn">
                        <i class="fas fa-plus"></i>
                        <span>افزودن گزینه</span>
                    </button>
                </div>
            </div>
        `;
    }

    loadGapFillDesigner(container) {
        container.innerHTML = `
            <div class="designer-section">
                <h4>طراحی سوالات جای خالی</h4>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-align-left"></i>
                        <span>متن با جای خالی</span>
                    </label>
                    <div class="gap-fill-editor">
                        <div class="editor-toolbar">
                            <button type="button" class="toolbar-btn" data-action="add-gap">
                                <i class="fas fa-plus"></i>
                                <span>افزودن جای خالی</span>
                            </button>
                        </div>
                        <div class="editor-content" contenteditable="true" placeholder="متن خود را بنویسید و برای افزودن جای خالی از دکمه بالا استفاده کنید..."></div>
                    </div>
                </div>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-list"></i>
                        <span>پاسخ‌های صحیح</span>
                    </label>
                    <div class="answers-container">
                        <div class="answer-item">
                            <span class="answer-label">جای خالی 1:</span>
                            <input type="text" class="form-input-modern" placeholder="پاسخ صحیح..." />
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    loadGenericDesigner(container) {
        container.innerHTML = `
            <div class="designer-section">
                <h4>طراحی محتوا</h4>
                <div class="form-group-modern">
                    <label class="form-label-modern">
                        <i class="fas fa-edit"></i>
                        <span>محتوای آموزشی</span>
                    </label>
                    <div class="rich-editor-container">
                        <div class="editor-toolbar">
                            <button type="button" class="toolbar-btn" data-command="bold">
                                <i class="fas fa-bold"></i>
                            </button>
                            <button type="button" class="toolbar-btn" data-command="italic">
                                <i class="fas fa-italic"></i>
                            </button>
                            <button type="button" class="toolbar-btn" data-command="insertUnorderedList">
                                <i class="fas fa-list-ul"></i>
                            </button>
                        </div>
                        <div class="editor-content" contenteditable="true" placeholder="محتوای آموزشی خود را اینجا بنویسید..."></div>
                    </div>
                </div>
            </div>
        `;
    }

    getTypeIcon(typeId) {
        const icons = {
            0: 'bell',
            1: 'pen',
            2: 'volume-up',
            3: 'edit',
            4: 'list-ul',
            5: 'link',
            6: 'bug',
            7: 'code',
            8: 'clipboard-check'
        };
        return icons[typeId] || 'file';
    }

    // Utility Methods
    updateCharacterCount(input, maxLength) {
        const charCount = input.parentElement.querySelector('.char-count');
        if (charCount) {
            const currentLength = input.value.length;
            charCount.textContent = `${currentLength}/${maxLength}`;
            charCount.classList.toggle('over-limit', currentLength > maxLength);
        }
    }

    formatDateTimeLocal(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        
        return `${year}-${month}-${day}T${hours}:${minutes}`;
    }

    updateDurationCalculator() {
        const startDateInput = document.getElementById('startDate');
        const dueDateInput = document.getElementById('dueDate');
        const calculator = document.getElementById('durationCalculator');
        const durationValue = document.getElementById('durationValue');
        
        if (!startDateInput || !dueDateInput || !calculator || !durationValue) return;

        const startDate = new Date(startDateInput.value);
        const dueDate = new Date(dueDateInput.value);

        if (startDateInput.value && dueDateInput.value && dueDate > startDate) {
            const diffMs = dueDate - startDate;
            const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
            const diffHours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            
            durationValue.textContent = `${diffDays} روز و ${diffHours} ساعت`;
            calculator.style.display = 'block';
        } else {
            calculator.style.display = 'none';
        }
    }

    updateScorePresets(score) {
        document.querySelectorAll('.score-preset').forEach(preset => {
            preset.classList.toggle('active', preset.dataset.score === score);
        });
    }

    updateAssignmentPreview() {
        const groupSelect = document.getElementById('groupId');
        const lessonSelect = document.getElementById('lessonId');
        const targetGroups = document.getElementById('targetGroups');
        const relatedLesson = document.getElementById('relatedLesson');

        if (groupSelect && targetGroups) {
            const selectedOption = groupSelect.options[groupSelect.selectedIndex];
            targetGroups.textContent = selectedOption.text || 'همه گروه‌ها';
        }

        if (lessonSelect && relatedLesson) {
            const selectedOption = lessonSelect.options[lessonSelect.selectedIndex];
            relatedLesson.textContent = selectedOption.text || 'تعیین نشده';
        }
    }

    // Preview Methods
    showPreview() {
        const previewContent = document.getElementById('previewContent');
        if (!previewContent) return;

        // Collect form data
        const formData = this.collectFormData();
        
        // Generate preview HTML
        previewContent.innerHTML = this.generatePreviewHTML(formData);
        
        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('previewModal'));
        modal.show();
    }

    showContentPreview() {
        // Show content-specific preview
        console.log('Content preview functionality');
    }

    saveFromPreview() {
        // Save form from preview mode
        const form = document.getElementById('createItemForm');
        if (form) {
            form.submit();
        }
    }

    collectFormData() {
        const form = document.getElementById('createItemForm');
        if (!form) return {};

        const formData = new FormData(form);
        const data = {};
        
        for (let [key, value] of formData.entries()) {
            data[key] = value;
        }

        return data;
    }

    generatePreviewHTML(data) {
        return `
            <div class="preview-item">
                <div class="preview-header">
                    <h3>${data.title || 'عنوان آیتم'}</h3>
                    <span class="preview-type">${this.contentTypes[data.type]?.name || 'نوع آیتم'}</span>
                </div>
                <div class="preview-content">
                    <div class="preview-description">
                        ${data.description || 'توضیحات آیتم'}
                    </div>
                    <div class="preview-meta">
                        <div class="meta-item">
                            <i class="fas fa-calendar"></i>
                            <span>شروع: ${data.startDate || 'تعیین نشده'}</span>
                        </div>
                        <div class="meta-item">
                            <i class="fas fa-clock"></i>
                            <span>مهلت: ${data.dueDate || 'تعیین نشده'}</span>
                        </div>
                        <div class="meta-item">
                            <i class="fas fa-star"></i>
                            <span>امتیاز: ${data.maxScore || '0'}</span>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Form Submission
    handleFormSubmit(e) {
        e.preventDefault();
        
        // Validate all steps
        if (!this.validateAllSteps()) {
            this.showValidationErrors();
            return;
        }

        // Collect form data
        const formData = this.collectFormData();
        
        // Add content data
        formData.contentData = this.collectContentData();
        
        // Submit form
        this.submitForm(formData);
    }

    validateAllSteps() {
        let isValid = true;
        
        for (let step = 1; step <= this.totalSteps; step++) {
            const stepElement = document.querySelector(`.form-step[data-step="${step}"]`);
            if (stepElement) {
                const requiredFields = stepElement.querySelectorAll('[required]');
                requiredFields.forEach(field => {
                    if (!this.validateField(field.name, field.value)) {
                        isValid = false;
                    }
                });
            }
        }
        
        return isValid;
    }

    showValidationErrors() {
        // Show validation errors to user
        const errors = Object.values(this.validationErrors).filter(error => error);
        if (errors.length > 0) {
            alert('لطفاً خطاهای فرم را برطرف کنید:\n' + errors.join('\n'));
        }
    }

    collectContentData() {
        // Collect content-specific data based on selected type
        const container = document.getElementById('contentDesignContainer');
        if (!container) return "{}";

        const contentData = {};
        
        // Collect data from content designer
        container.querySelectorAll('input, textarea, select').forEach(input => {
            contentData[input.name || input.id] = input.value;
        });

        return JSON.stringify(contentData);
    }

    async submitForm(formData) {
        try {
            const response = await fetch('/Teacher/ScheduleItem/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(formData)
            });

            if (response.ok) {
                // Success
                this.showSuccessMessage();
                // Redirect or close modal
            } else {
                // Error
                const error = await response.json();
                this.showErrorMessage(error.message || 'خطا در ایجاد آیتم');
            }
        } catch (error) {
            console.error('Error submitting form:', error);
            this.showErrorMessage('خطا در ارسال فرم');
        }
    }

    showSuccessMessage(message = 'عملیات با موفقیت انجام شد') {
        // Show success message
        console.log(message);
        // You can replace this with a proper notification system
        alert('موفق: ' + message);
    }

    showErrorMessage(message) {
        // Show error message
        alert('خطا: ' + message);
    }

    saveFormData() {
        // Auto-save form data to localStorage
        const formData = this.collectFormData();
        localStorage.setItem('scheduleItemFormData', JSON.stringify(formData));
    }

    loadFormData() {
        // Load form data from localStorage
        const savedData = localStorage.getItem('scheduleItemFormData');
        if (savedData) {
            const formData = JSON.parse(savedData);
            this.populateForm(formData);
        }
    }

    populateForm(data) {
        // Populate form fields with saved data
        Object.entries(data).forEach(([key, value]) => {
            const field = document.querySelector(`[name="${key}"]`);
            if (field) {
                field.value = value;
            }
        });
    }

    // Persian Date Picker Setup
    setupPersianDatePicker() {
        // Initialize Persian date pickers
        if (typeof initializeAllDatePickers === 'function') {
            initializeAllDatePickers();
        }

        // Setup time inputs
        this.setupTimeInputs();
    }

    setupTimeInputs() {
        const startTimeInput = document.getElementById('StartTime');
        const dueTimeInput = document.getElementById('DueTime');
        const startDateInput = document.getElementById('StartDate');
        const dueDateInput = document.getElementById('DueDate');

        if (startTimeInput && startDateInput) {
            startTimeInput.addEventListener('change', () => {
                this.updateDateTimeField(startDateInput, startTimeInput);
            });
        }

        if (dueTimeInput && dueDateInput) {
            dueTimeInput.addEventListener('change', () => {
                this.updateDateTimeField(dueDateInput, dueTimeInput);
            });
        }
    }

    updateDateTimeField(dateInput, timeInput) {
        if (dateInput.value && timeInput.value) {
            const date = new Date(dateInput.value);
            const [hours, minutes] = timeInput.value.split(':');
            date.setHours(parseInt(hours), parseInt(minutes));
            dateInput.value = date.toISOString();
        }
    }

    // Step Saving Methods
    async saveCurrentStep() {
        const stepData = this.collectStepData(this.currentStep);
        
        const requestData = {
            Id: this.currentItemId,
            TeachingPlanId: parseInt(document.querySelector('[name="TeachingPlanId"]').value),
            Step: this.currentStep,
            ...stepData
        };

        try {
            console.log('Request data:', requestData);
            
            const response = await fetch('/Teacher/ScheduleItem/SaveStep', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });

            // Check if response is ok and has content
            if (!response.ok) {
                const errorText = await response.text();
                console.error('Response not OK:', response.status, errorText);
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const responseText = await response.text();
            console.log('Response text:', responseText);
            
            if (!responseText) {
                throw new Error('Empty response from server');
            }

            const result = JSON.parse(responseText);
            
            if (result.success) {
                this.currentItemId = result.id;
                this.isEditMode = true;
                this.showSuccessMessage('مرحله با موفقیت ذخیره شد');
                
                // Update URL to include ID for edit mode
                if (!window.location.search.includes('id=')) {
                    const url = new URL(window.location);
                    url.searchParams.set('id', result.id);
                    window.history.replaceState({}, '', url);
                }
                
                // Load existing data after first save
                if (this.currentStep === 1) {
                    await this.loadExistingItem();
                }
                
                return result;
            } else {
                throw new Error(result.message || 'خطا در ذخیره مرحله');
            }
        } catch (error) {
            console.error('Error saving step:', error);
            throw error;
        }
    }

    collectStepData(step) {
        const stepData = {};

        switch (step) {
            case 1:
                stepData.Type = parseInt(document.getElementById('itemType')?.value);
                stepData.Title = document.getElementById('itemTitle')?.value;
                stepData.Description = document.getElementById('descriptionHidden')?.value;
                break;
            case 2:
                stepData.StartDate = document.getElementById('StartDate')?.value;
                stepData.DueDate = document.getElementById('DueDate')?.value;
                stepData.MaxScore = parseFloat(document.getElementById('maxScore')?.value) || null;
                stepData.IsMandatory = document.getElementById('isMandatory')?.checked || false;
                break;
            case 3:
                stepData.GroupId = parseInt(document.getElementById('groupId')?.value) || null;
                stepData.LessonId = parseInt(document.getElementById('lessonId')?.value) || null;
                break;
            case 4:
                stepData.ContentJson = this.collectContentData();
                break;
        }

        return stepData;
    }

    // Check for existing item
    checkForExistingItem() {
        const urlParams = new URLSearchParams(window.location.search);
        const itemId = urlParams.get('id');
        
        if (itemId) {
            this.currentItemId = parseInt(itemId);
            this.isEditMode = true;
            this.loadExistingItem();
        }
    }

    async loadExistingItem() {
        if (!this.currentItemId) return;

        try {
            const response = await fetch(`/Teacher/ScheduleItem/GetById/${this.currentItemId}`);
            const result = await response.json();
            
            if (result.success) {
                this.populateFormWithExistingData(result.data);
            }
        } catch (error) {
            console.error('Error loading existing item:', error);
        }
    }

    populateFormWithExistingData(data) {
        // Populate form fields with existing data
        if (data.title) document.getElementById('itemTitle').value = data.title;
        if (data.type) document.getElementById('itemType').value = data.type;
        if (data.description) {
            document.getElementById('descriptionEditor').innerHTML = data.description;
            document.getElementById('descriptionHidden').value = data.description;
        }
        if (data.startDate) {
            document.getElementById('StartDate').value = data.startDate;
            this.updatePersianDateDisplay('PersianStartDate', data.startDate);
            this.updateTimeInput('StartTime', data.startDate);
        }
        if (data.dueDate) {
            document.getElementById('DueDate').value = data.dueDate;
            this.updatePersianDateDisplay('PersianDueDate', data.dueDate);
            this.updateTimeInput('DueTime', data.dueDate);
        }
        if (data.maxScore) document.getElementById('maxScore').value = data.maxScore;
        if (data.isMandatory) document.getElementById('isMandatory').checked = data.isMandatory;
        if (data.groupId) document.getElementById('groupId').value = data.groupId;
        if (data.lessonId) document.getElementById('lessonId').value = data.lessonId;

        // Update current step
        if (data.currentStep) {
            this.currentStep = data.currentStep;
            this.updateStepVisibility();
            this.updateStepIndicators();
            this.updateProgress();
            this.updateNavigationButtons();
        }

        // Update form state indicators
        this.updateFormStateIndicators();
    }

    updatePersianDateDisplay(inputId, isoDate) {
        if (isoDate && typeof window.persianDate !== 'undefined') {
            const date = new Date(isoDate);
            const persianDate = window.persianDate.gregorianToPersian(date);
            const formattedDate = `${persianDate.year}/${persianDate.month.toString().padStart(2, '0')}/${persianDate.day.toString().padStart(2, '0')}`;
            document.getElementById(inputId).value = formattedDate;
        }
    }

    updateTimeInput(inputId, isoDate) {
        if (isoDate) {
            const date = new Date(isoDate);
            const timeString = `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
            const timeInput = document.getElementById(inputId);
            if (timeInput) {
                timeInput.value = timeString;
            }
        }
    }

    updateFormStateIndicators() {
        // Update page title to show edit mode
        if (this.isEditMode && this.currentItemId) {
            const pageTitle = document.querySelector('.modern-page-header h1');
            if (pageTitle) {
                pageTitle.textContent = 'ویرایش آیتم آموزشی';
            }
        }

        // Update sidebar tips for edit mode
        const tipsList = document.querySelector('.tips-list');
        if (tipsList && this.isEditMode) {
            tipsList.innerHTML = `
                <li class="tip-item">
                    <i class="fas fa-edit"></i>
                    <span>در حال ویرایش آیتم آموزشی هستید</span>
                </li>
                <li class="tip-item">
                    <i class="fas fa-save"></i>
                    <span>تغییرات به صورت خودکار ذخیره می‌شوند</span>
                </li>
                <li class="tip-item">
                    <i class="fas fa-step-forward"></i>
                    <span>می‌توانید بین مراحل جابجا شوید</span>
                </li>
            `;
        }
    }

    // Enhanced form submission
    async submitForm(formData) {
        try {
            // Save final step first
            await this.saveCurrentStep();
            
            // Complete the item
            const response = await fetch('/Teacher/ScheduleItem/Complete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ Id: this.currentItemId })
            });

            const result = await response.json();

            if (result.success) {
                this.showSuccessMessage('آیتم آموزشی با موفقیت تکمیل شد');
                // Redirect to index page
                setTimeout(() => {
                    window.location.href = `/Teacher/ScheduleItem/Index?teachingPlanId=${formData.TeachingPlanId}`;
                }, 2000);
            } else {
                throw new Error(result.message || 'خطا در تکمیل آیتم');
            }
        } catch (error) {
            console.error('Error submitting form:', error);
            this.showErrorMessage('خطا در ارسال فرم');
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ModernScheduleItemFormManager();
});