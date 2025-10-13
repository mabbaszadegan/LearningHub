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
        this.selectedGroups = [];
        this.selectedSubChapters = [];
        this.selectedStudents = [];
        this.allStudents = []; // All students from all selected groups
        this.init();
    }

    async init() {
        this.setupEventListeners();
        this.setupRichEditor();
        this.setupStepNavigation();
        this.setupFormValidation();
        this.loadContentTypes();
        this.updateProgress();
        this.setupAutoSave();
        this.setupPersianDatePicker();
        this.setupContentBuilder();
        await this.checkForExistingItem();
        
        // Initialize step 4 content based on current selection
        this.updateStep4Content();
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

        // Badge-based selection components
        this.setupBadgeSelectionListeners();

        // Score presets
        this.setupScorePresets();

        // Step progress indicators
        this.setupStepProgressIndicators();

        // Student selection
        this.setupStudentSelection();
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
        const startDateInput = document.getElementById('StartDate');
        const dueDateInput = document.getElementById('DueDate');

        if (startDateInput) {
            startDateInput.addEventListener('change', () => {
                this.updateDurationCalculator();
                this.validateField('StartDate', startDateInput.value);
            });
        }

        if (dueDateInput) {
            dueDateInput.addEventListener('change', () => {
                this.updateDurationCalculator();
                this.validateField('DueDate', dueDateInput.value);
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
        const typeSelect = document.getElementById('itemType');
        if (typeSelect) {
            typeSelect.addEventListener('change', (e) => {
                this.selectedContentType = e.target.value;
                this.updateContentTypePreview();
                this.updateStep4Content();
            });
        }
    }

    setupContentBuilder() {
        // Initialize content builder integration
        this.contentBuilder = null;
        
        // Wait for content builder to be available
        const checkContentBuilder = () => {
            if (window.contentBuilder) {
                this.contentBuilder = window.contentBuilder;
                this.setupContentBuilderEvents();
            } else {
                setTimeout(checkContentBuilder, 100);
            }
        };
        
        checkContentBuilder();
    }

    setupContentBuilderEvents() {
        if (!this.contentBuilder) return;

        // Save content when save button is clicked
        const saveBtn = document.getElementById('saveContentBtn');
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                this.saveContentBuilderData();
            });
        }

        // Preview content when preview button is clicked
        const previewBtn = document.getElementById('previewContentBtn');
        if (previewBtn) {
            previewBtn.addEventListener('click', () => {
                this.previewContentBuilderData();
            });
        }
    }

    updateStep4Content() {
        console.log('updateStep4Content called');
        
        const contentTypeSelector = document.getElementById('contentTypeSelector');
        const contentDesigner = document.getElementById('contentDesigner');
        const contentBuilder = document.getElementById('contentBuilder');
        const contentTemplates = document.getElementById('contentTemplates');

        console.log('Elements found:', {
            contentTypeSelector: !!contentTypeSelector,
            contentDesigner: !!contentDesigner,
            contentBuilder: !!contentBuilder,
            contentTemplates: !!contentTemplates
        });

        // Hide all content sections first
        if (contentTypeSelector) contentTypeSelector.style.display = 'none';
        if (contentDesigner) contentDesigner.style.display = 'none';
        if (contentBuilder) contentBuilder.style.display = 'none';
        if (contentTemplates) contentTemplates.style.display = 'none';

        // Get the selected schedule item type from step 1
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';
        
        console.log('Selected type:', selectedType);
        console.log('ItemTypeSelect element:', itemTypeSelect);

        // Show appropriate content section based on selected type
        if (selectedType === '0') { // Reminder type
            console.log('Showing contentBuilder for Reminder type');
            if (contentBuilder) {
                contentBuilder.style.display = 'block';
                console.log('ContentBuilder display set to block');
                
                // Initialize content builder if not already done
                if (!this.contentBuilder) {
                    this.setupContentBuilder();
                }
            } else {
                console.log('ContentBuilder element not found!');
            }
        } else {
            console.log('Showing contentTypeSelector for other types');
            // For other types, show content type selector
            if (contentTypeSelector) {
                contentTypeSelector.style.display = 'block';
                console.log('ContentTypeSelector display set to block');
            } else {
                console.log('ContentTypeSelector element not found!');
            }
        }
    }

    saveContentBuilderData() {
        if (!this.contentBuilder) return;

        const contentData = this.contentBuilder.getContentData();
        const contentJson = JSON.stringify(contentData);
        
        // Update the hidden content field
        const contentField = document.getElementById('contentJson');
        if (contentField) {
            contentField.value = contentJson;
        }

        // Show success message
        this.showSuccess('محتوای آموزشی با موفقیت ذخیره شد.');
    }

    previewContentBuilderData() {
        if (!this.contentBuilder) return;

        const contentData = this.contentBuilder.getContentData();
        this.showContentPreview(contentData);
    }

    showContentPreview(contentData) {
        const modal = document.getElementById('previewModal');
        const previewContent = document.getElementById('previewContent');
        
        if (!modal || !previewContent) return;

        // Generate preview HTML
        let previewHTML = '<div class="content-preview">';
        
        if (contentData.boxes && contentData.boxes.length > 0) {
            contentData.boxes.forEach(box => {
                previewHTML += this.generateBoxPreview(box);
            });
        } else {
            previewHTML += '<div class="empty-content">هیچ محتوایی اضافه نشده است.</div>';
        }
        
        previewHTML += '</div>';
        
        previewContent.innerHTML = previewHTML;
        
        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }

    generateBoxPreview(box) {
        let html = `<div class="preview-box preview-box-${box.type}">`;
        
        switch (box.type) {
            case 'text':
                html += `<div class="text-content">${box.data.content || ''}</div>`;
                break;
            case 'image':
                if (box.data.fileId) {
                    html += `<div class="image-content" style="text-align: ${box.data.align || 'center'};">
                        <img src="/uploads/${box.data.fileId}" alt="تصویر" style="max-width: ${this.getImageSize(box.data.size)};" />
                        ${box.data.caption ? `<div class="image-caption">${box.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
            case 'video':
                if (box.data.fileId) {
                    html += `<div class="video-content" style="text-align: ${box.data.align || 'center'};">
                        <video controls style="max-width: ${this.getVideoSize(box.data.size)};">
                            <source src="/uploads/${box.data.fileId}" type="video/mp4">
                        </video>
                        ${box.data.caption ? `<div class="video-caption">${box.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
            case 'audio':
                if (box.data.fileId) {
                    html += `<div class="audio-content">
                        <audio controls>
                            <source src="/uploads/${box.data.fileId}" type="audio/mpeg">
                        </audio>
                        ${box.data.caption ? `<div class="audio-caption">${box.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
        }
        
        html += '</div>';
        return html;
    }

    getImageSize(size) {
        switch (size) {
            case 'small': return '200px';
            case 'medium': return '400px';
            case 'large': return '600px';
            case 'full': return '100%';
            default: return '400px';
        }
    }

    getVideoSize(size) {
        switch (size) {
            case 'small': return '400px';
            case 'medium': return '600px';
            case 'large': return '800px';
            case 'full': return '100%';
            default: return '600px';
        }
    }

    showSuccess(message) {
        // You can implement a toast notification system here
        console.log('Success:', message);
    }

    setupDatetimeHelpers() {
        const setNowBtn = document.getElementById('setNowBtn');
        const setWeekBtn = document.getElementById('setWeekBtn');

        if (setNowBtn) {
            setNowBtn.addEventListener('click', () => {
                const now = new Date();
                const persianStartDateInput = document.getElementById('PersianStartDate');
                const startDateInput = document.getElementById('StartDate');
                const startTimeInput = document.getElementById('StartTime');
                
                if (persianStartDateInput && startDateInput) {
                    // Convert to Persian date
                    const persianDate = this.convertToPersianDate(now);
                    persianStartDateInput.value = persianDate;
                    
                    // Set hidden field with current date and time
                    startDateInput.value = now.toISOString();
                    
                    // Set time to current time
                    if (startTimeInput) {
                        const timeString = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;
                        startTimeInput.value = timeString;
                    }
                    
                    // Trigger datepicker update
                    this.updateDatePicker(persianStartDateInput, persianDate);
                    
                    // Update duration calculator
                    this.updateDurationCalculator();
                    
                    console.log('Set to current time:', {
                        persianDate: persianDate,
                        isoDate: now.toISOString(),
                        time: startTimeInput ? startTimeInput.value : 'N/A'
                    });
                }
            });
        }

        if (setWeekBtn) {
            setWeekBtn.addEventListener('click', () => {
                const now = new Date();
                const weekFromNow = new Date(now.getTime() + (7 * 24 * 60 * 60 * 1000)); // Add 7 days
                
                const persianDueDateInput = document.getElementById('PersianDueDate');
                const dueDateInput = document.getElementById('DueDate');
                const dueTimeInput = document.getElementById('DueTime');
                
                if (persianDueDateInput && dueDateInput) {
                    // Convert to Persian date
                    const persianDate = this.convertToPersianDate(weekFromNow);
                    persianDueDateInput.value = persianDate;
                    
                    // Set hidden field with date one week from now
                    dueDateInput.value = weekFromNow.toISOString();
                    
                    // Set time to current time (or you can set to end of day)
                    if (dueTimeInput) {
                        const timeString = `${weekFromNow.getHours().toString().padStart(2, '0')}:${weekFromNow.getMinutes().toString().padStart(2, '0')}`;
                        dueTimeInput.value = timeString;
                    }
                    
                    // Trigger datepicker update
                    this.updateDatePicker(persianDueDateInput, persianDate);
                    
                    // Update duration calculator
                    this.updateDurationCalculator();
                    
                    console.log('Set to one week from now:', {
                        persianDate: persianDate,
                        isoDate: weekFromNow.toISOString(),
                        time: dueTimeInput ? dueTimeInput.value : 'N/A'
                    });
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
        
        // Update step 4 content when entering step 4
        if (step === 4) {
            this.updateStep4Content();
        }
    }

    async goToNextStep() {
        if (this.currentStep < this.totalSteps) {
            try {
                // Save current step before moving to next
                await this.saveCurrentStep();
                this.goToStep(this.currentStep + 1);
            } catch (error) {
                console.error('Error saving step:', error);
                this.showErrorMessage('خطا در ذخیره مرحله');
            }
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
        // Update step progress indicators
        this.updateStepProgress();
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
        const startDateInput = document.getElementById('StartDate');
        const dueDateInput = document.getElementById('DueDate');
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

        // Add selected groups and subchapters for badge-based selection
        if (this.selectedGroups.length > 0) {
            data.GroupIds = this.selectedGroups.map(g => g.id);
        }
        if (this.selectedSubChapters.length > 0) {
            data.SubChapterIds = this.selectedSubChapters.map(sc => sc.id);
        }
        if (this.selectedStudents.length > 0) {
            data.StudentIds = this.selectedStudents.map(s => s.id);
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
        // Validate subchapter selection before submission
        if (this.selectedSubChapters.length === 0) {
            this.showErrorMessage('انتخاب حداقل یک زیرمبحث اجباری است');
            this.goToStep(3); // Go to assignment step
            return;
        }

        // Add selected groups and subchapters to form data
        if (this.selectedGroups.length > 0) {
            formData.GroupIds = this.selectedGroups.map(g => g.id);
        }
        if (this.selectedSubChapters.length > 0) {
            formData.SubChapterIds = this.selectedSubChapters.map(sc => sc.id);
        }

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
        // Show success message using Toastr
        if (typeof toastr !== 'undefined') {
            toastr.success(message, 'موفقیت', {
                timeOut: 3000,
                closeButton: true,
                progressBar: true
            });
        } else {
            // Fallback to alert if toastr is not available
            alert('موفق: ' + message);
        }
    }

    showErrorMessage(message) {
        // Show error message using Toastr
        if (typeof toastr !== 'undefined') {
            toastr.error(message, 'خطا', {
                timeOut: 5000,
                closeButton: true,
                progressBar: true
            });
        } else {
            // Fallback to alert if toastr is not available
            alert('خطا: ' + message);
        }
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
        // Wait for DOM to be ready
        setTimeout(() => {
            // Initialize Persian date pickers
            if (typeof initializeAllDatePickers === 'function') {
                initializeAllDatePickers();
            }

            // Setup custom datepicker events
            this.setupDatePickerEvents();

            // Setup time inputs
            this.setupTimeInputs();
        }, 100);
    }

    setupDatePickerEvents() {
        // Setup events for Persian date pickers
        const persianDateInputs = document.querySelectorAll('.persian-datepicker');
        
        persianDateInputs.forEach(input => {
            input.addEventListener('click', () => {
                // Ensure datepicker is properly initialized
                if (!input._persianDatePicker) {
                    setTimeout(() => {
                        if (typeof initializeAllDatePickers === 'function') {
                            initializeAllDatePickers();
                        }
                    }, 50);
                }
            });
        });
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

    convertToPersianDate(date) {
        try {
            // Validate input
            if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
                console.warn('Invalid date provided to convertToPersianDate');
                return '0000/00/00';
            }

            // Use official jalaali-js library if available
            if (typeof window.jalaali !== 'undefined' && window.jalaali.toJalaali) {
                try {
                    const jalaali = window.jalaali.toJalaali(date);
                    return `${jalaali.jy}/${jalaali.jm.toString().padStart(2, '0')}/${jalaali.jd.toString().padStart(2, '0')}`;
                } catch (jalaaliError) {
                    console.warn('jalaali-js library error:', jalaaliError);
                    // Fall through to next method
                }
            }
            
            // Fallback to old PersianDate library if available
            if (typeof PersianDate !== 'undefined') {
                try {
                    const persianDate = PersianDate.fromDate(date);
                    return persianDate.format('YYYY/MM/DD');
                } catch (persianError) {
                    console.warn('PersianDate library error:', persianError);
                    // Fall through to next method
                }
            }
            
            // Fallback to persianDateUtils if available
            if (typeof window.persianDateUtils !== 'undefined' && window.persianDateUtils.gregorianToPersian) {
                try {
                    return window.persianDateUtils.gregorianToPersian(date);
                } catch (utilsError) {
                    console.warn('persianDateUtils error:', utilsError);
                    // Fall through to next method
                }
            }
            
            // Final fallback: simple approximation
            console.warn('Using simple Persian date approximation');
            const year = date.getFullYear() - 621;
            const month = date.getMonth() + 1;
            const day = date.getDate();
            return `${year}/${month.toString().padStart(2, '0')}/${day.toString().padStart(2, '0')}`;
            
        } catch (error) {
            console.error('Error converting to Persian date:', error);
            
            // Return current date as fallback
            const now = new Date();
            const year = now.getFullYear() - 621;
            const month = now.getMonth() + 1;
            const day = now.getDate();
            return `${year}/${month.toString().padStart(2, '0')}/${day.toString().padStart(2, '0')}`;
        }
    }

    updateDatePicker(inputElement, persianDate) {
        try {
            // Try to update the datepicker if it exists
            if (inputElement._persianDatePicker && typeof inputElement._persianDatePicker.setDate === 'function') {
                inputElement._persianDatePicker.setDate(persianDate);
            } else {
                // Trigger change event to update the calendar
                const changeEvent = new Event('change', { bubbles: true });
                inputElement.dispatchEvent(changeEvent);
                
                // Also trigger input event
                const inputEvent = new Event('input', { bubbles: true });
                inputElement.dispatchEvent(inputEvent);
            }
        } catch (error) {
            console.error('Error updating datepicker:', error);
        }
    }

    updateDatePickers() {
        console.log('schedule-item-form: Updating datepickers');
        
        // Update PersianStartDate datepicker
        const persianStartDateInput = document.getElementById('PersianStartDate');
        if (persianStartDateInput && persianStartDateInput.value) {
            console.log('schedule-item-form: Updating PersianStartDate datepicker', persianStartDateInput.value);
            if (persianStartDateInput.datePicker) {
                persianStartDateInput.datePicker.setDate(persianStartDateInput.value);
            } else {
                console.warn('schedule-item-form: PersianStartDate datepicker not found');
            }
        }
        
        // Update PersianDueDate datepicker
        const persianDueDateInput = document.getElementById('PersianDueDate');
        if (persianDueDateInput && persianDueDateInput.value) {
            console.log('schedule-item-form: Updating PersianDueDate datepicker', persianDueDateInput.value);
            if (persianDueDateInput.datePicker) {
                persianDueDateInput.datePicker.setDate(persianDueDateInput.value);
            } else {
                console.warn('schedule-item-form: PersianDueDate datepicker not found');
            }
        }
    }

    // Test method to verify Persian date conversion
    testPersianDateConversion() {
        const testDate = new Date();
        const persianDate = this.convertToPersianDate(testDate);
        
        console.log('Official Persian Date Conversion Test:', {
            gregorian: testDate.toISOString(),
            persian: persianDate,
            jalaaliJs: typeof window.jalaali,
            PersianDateClass: typeof PersianDate,
            persianDateUtils: typeof window.persianDateUtils,
            oldPersianDate: typeof window.persianDate,
            jalaaliLoaded: typeof window.jalaali !== 'undefined'
        });
        
        // Test jalaali-js conversion accuracy
        if (typeof window.jalaali !== 'undefined') {
            try {
                const jalaali = window.jalaali.toJalaali(testDate);
                const backToGregorian = window.jalaali.toGregorian(jalaali.jy, jalaali.jm, jalaali.jd);
                const accuracy = Math.abs(testDate.getTime() - new Date(backToGregorian.gy, backToGregorian.gm - 1, backToGregorian.gd).getTime()) < 24 * 60 * 60 * 1000; // Within 1 day
                
                console.log('Jalaali-js Conversion Accuracy Test:', {
                    original: testDate.toISOString(),
                    converted: `${jalaali.jy}/${jalaali.jm.toString().padStart(2, '0')}/${jalaali.jd.toString().padStart(2, '0')}`,
                    backToGregorian: `${backToGregorian.gy}/${backToGregorian.gm.toString().padStart(2, '0')}/${backToGregorian.gd.toString().padStart(2, '0')}`,
                    accurate: accuracy
                });
            } catch (error) {
                console.error('Jalaali-js accuracy test failed:', error);
            }
        }
        
        // Test conversion accuracy with old PersianDate library
        if (typeof PersianDate !== 'undefined') {
            try {
                const officialPersian = PersianDate.fromDate(testDate);
                const backToGregorian = officialPersian.toDate();
                const accuracy = Math.abs(testDate.getTime() - backToGregorian.getTime()) < 24 * 60 * 60 * 1000; // Within 1 day
                
                console.log('Old PersianDate Conversion Accuracy Test:', {
                    original: testDate.toISOString(),
                    converted: officialPersian.format('YYYY/MM/DD'),
                    backToGregorian: backToGregorian.toISOString(),
                    accurate: accuracy
                });
            } catch (error) {
                console.error('Old PersianDate accuracy test failed:', error);
            }
        }
        
        return persianDate;
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
                    try {
                        const url = new URL(window.location);
                        url.searchParams.set('id', result.id);
                        window.history.replaceState({}, '', url);
                    } catch (error) {
                        console.error('Error updating URL:', error);
                        // Don't throw error here, just log it
                    }
                }

                // Load existing data after first save
                if (this.currentStep === 1) {
                    try {
                        await this.loadExistingItem();
                    } catch (error) {
                        console.error('Error loading existing item after save:', error);
                        // Don't throw error here, just log it
                    }
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
                stepData.PersianStartDate = document.getElementById('PersianStartDate')?.value;
                stepData.PersianDueDate = document.getElementById('PersianDueDate')?.value;
                stepData.StartTime = document.getElementById('StartTime')?.value;
                stepData.DueTime = document.getElementById('DueTime')?.value;
                stepData.MaxScore = parseFloat(document.getElementById('maxScore')?.value) || null;
                stepData.IsMandatory = document.getElementById('isMandatory')?.checked || false;
                break;
        case 3:
            stepData.GroupId = parseInt(document.getElementById('groupId')?.value) || null;
            // Add selected groups and subchapters for badge-based selection
            if (this.selectedGroups.length > 0) {
                stepData.GroupIds = this.selectedGroups.map(g => g.id);
            }
            if (this.selectedSubChapters.length > 0) {
                stepData.SubChapterIds = this.selectedSubChapters.map(sc => sc.id);
            }
            if (this.selectedStudents.length > 0) {
                stepData.StudentIds = this.selectedStudents.map(s => s.id);
            }
            break;
            case 4:
                stepData.ContentJson = this.collectContentData();
                break;
        }

        return stepData;
    }

    // Check for existing item
    async checkForExistingItem() {
        const urlParams = new URLSearchParams(window.location.search);
        const itemId = urlParams.get('id');

        if (itemId) {
            this.currentItemId = parseInt(itemId);
            this.isEditMode = true;
            try {
                await this.loadExistingItem();
            } catch (error) {
                console.error('Error loading existing item:', error);
                this.showErrorMessage('خطا در بارگذاری آیتم موجود');
            }
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
        if (data.title) {
            const itemTitle = document.getElementById('itemTitle');
            if (itemTitle) {
                itemTitle.value = data.title;
            }
        }
        if (data.type >= 0) {
            const itemType = document.getElementById('itemType');
            if (itemType) {
                itemType.value = data.type;
            }
        }
        if (data.description) {
            const descriptionEditor = document.getElementById('descriptionEditor');
            const descriptionHidden = document.getElementById('descriptionHidden');
            if (descriptionEditor) {
                descriptionEditor.innerHTML = data.description;
            }
            if (descriptionHidden) {
                descriptionHidden.value = data.description;
            }
        }
        if (data.persianStartDate) {
            console.log('schedule-item-form: Setting PersianStartDate', data.persianStartDate);
            const persianStartDate = document.getElementById('PersianStartDate');
            if (persianStartDate) {
                persianStartDate.value = data.persianStartDate;
            }
            const startTime = document.getElementById('StartTime');
            if (startTime) {
                this.updateTimeInput('StartTime', data.startDate);
            }
        }
        if (data.persianDueDate) {
            console.log('schedule-item-form: Setting PersianDueDate', data.persianDueDate);
            const persianDueDate = document.getElementById('PersianDueDate');
            if (persianDueDate) {
                persianDueDate.value = data.persianDueDate;
            }
            const dueTime = document.getElementById('DueTime');
            if (dueTime) {
                this.updateTimeInput('DueTime', data.dueDate);
            }
        }
        if (data.maxScore) {
            const maxScore = document.getElementById('maxScore');
            if (maxScore) {
                maxScore.value = data.maxScore;
            }
        }
        if (data.isMandatory) {
            const isMandatory = document.getElementById('isMandatory');
            if (isMandatory) {
                isMandatory.checked = data.isMandatory;
            }
        }
        if (data.groupId) {
            const groupId = document.getElementById('groupId');
            if (groupId) {
                groupId.value = data.groupId;
            }
        }
        if (data.lessonId) {
            const lessonId = document.getElementById('lessonId');
            if (lessonId) {
                lessonId.value = data.lessonId;
            }
        }

        // Load selected groups and subchapters for badge-based selection
        if (data.groupIds && data.groupIds.length > 0) {
            this.selectedGroups = data.groupIds.map(id => ({ id: id, name: '' })); // Name will be loaded later
        }
        if (data.subChapterIds && data.subChapterIds.length > 0) {
            this.selectedSubChapters = data.subChapterIds.map(id => ({ id: id, title: '' })); // Title will be loaded later
        }
        if (data.studentIds && data.studentIds.length > 0) {
            this.selectedStudents = data.studentIds.map(id => ({ id: id, firstName: '', lastName: '' })); // Details will be loaded later
        }

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
        
        // Update datepickers after form is populated
        setTimeout(() => {
            this.updateDatePickers();
            // Load and select groups and subchapters after UI is ready
            this.loadAndSelectExistingAssignments();
        }, 200);
    }

    updatePersianDateDisplay(inputId, isoDate) {
        if (isoDate) {
            const date = new Date(isoDate);
            const persianDateString = this.convertToPersianDate(date);
            const inputElement = document.getElementById(inputId);
            if (inputElement && persianDateString !== '0000/00/00') {
                inputElement.value = persianDateString;
                // Update the datepicker if it exists
                this.updateDatePicker(inputElement, persianDateString);
            }
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

    async loadAndSelectExistingAssignments() {
        // Load groups and subchapters, then select the existing ones
        try {
            // Load groups first
            await this.loadGroupsAsBadges();
            
            // Select existing groups
            this.selectedGroups.forEach(group => {
                const badge = document.querySelector(`[data-group-id="${group.id}"]`);
                if (badge) {
                    badge.classList.add('selected');
                    // Update the group name if we have it
                    const nameElement = badge.querySelector('.group-badge-name');
                    if (nameElement) {
                        group.name = nameElement.textContent;
                    }
                }
            });

            // Load subchapters
            await this.loadChaptersWithSubChapters();
            
            // Select existing subchapters and expand parent chapters
            this.selectedSubChapters.forEach(subChapter => {
                const badge = document.querySelector(`[data-sub-chapter-id="${subChapter.id}"]`);
                if (badge) {
                    badge.classList.add('selected');
                    // Update the subchapter title if we have it
                    const titleElement = badge.querySelector('.subchapter-title');
                    if (titleElement) {
                        subChapter.title = titleElement.textContent;
                    }
                    
                    // Expand the parent chapter if it contains selected subchapters
                    const chapterItem = badge.closest('.chapter-item');
                    if (chapterItem && !chapterItem.classList.contains('expanded')) {
                        chapterItem.classList.add('expanded');
                    }
                }
            });

            // Update summaries and preview
            this.updateGroupSelectionSummary();
            this.updateSubChapterSelectionSummary();
            this.updateAssignmentPreview();
            this.updateHiddenInputs();
            
            // Load students for selected groups
            if (this.selectedGroups.length > 0) {
                this.updateStudentSelectionVisibility();
            }

        } catch (error) {
            console.error('Error loading existing assignments:', error);
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

    setupBadgeSelectionListeners() {
        // Badge-based Group selection
        this.setupGroupBadgeSelection();
        
        // Badge-based SubChapter selection
        this.setupSubChapterBadgeSelection();
    }

    setupGroupBadgeSelection() {
        // Load groups and render as badges
        this.loadGroupsAsBadges();
        
        // Add click listeners for group badges
        document.addEventListener('click', (e) => {
            if (e.target.closest('.group-badge')) {
                const badge = e.target.closest('.group-badge');
                const groupId = parseInt(badge.dataset.groupId);
                this.toggleGroupSelection(groupId);
            }
        });
    }

    setupSubChapterBadgeSelection() {
        // Load chapters and subchapters
        this.loadChaptersWithSubChapters();
        
        // Add click listeners for chapter headers
        document.addEventListener('click', (e) => {
            if (e.target.closest('.chapter-header')) {
                const chapterItem = e.target.closest('.chapter-item');
                this.toggleChapterExpansion(chapterItem);
            }
            
            if (e.target.closest('.subchapter-badge')) {
                const badge = e.target.closest('.subchapter-badge');
                const subChapterId = parseInt(badge.dataset.subChapterId);
                this.toggleSubChapterSelection(subChapterId);
            }
        });
    }

    async loadGroupsAsBadges() {
        try {
            const teachingPlanId = this.getTeachingPlanId();
            const response = await fetch(`/Teacher/Schedule/GetGroups?teachingPlanId=${teachingPlanId}`);
            const groups = await response.json();

            if (groups.success) {
                this.renderGroupBadges(groups.data);
            }
        } catch (error) {
            console.error('Error loading groups:', error);
        }
    }

    renderGroupBadges(groups) {
        const container = document.getElementById('groupBadgeGrid');
        if (!container) return;

        container.innerHTML = groups.map(group => `
            <div class="group-badge" data-group-id="${group.id}">
                <div class="group-badge-content">
                    <div class="group-badge-icon">
                        <i class="fas fa-users"></i>
                    </div>
                    <div class="group-badge-info">
                        <div class="group-badge-name">${group.name}</div>
                        <div class="group-badge-count">گروه دانشجویی</div>
                    </div>
                </div>
                <div class="group-badge-check"></div>
            </div>
        `).join('');
    }

    toggleGroupSelection(groupId) {
        const badge = document.querySelector(`[data-group-id="${groupId}"]`);
        if (!badge) return;

        const isSelected = badge.classList.contains('selected');
        
        if (isSelected) {
            badge.classList.remove('selected');
            this.selectedGroups = this.selectedGroups.filter(g => g.id !== groupId);
        } else {
            badge.classList.add('selected');
            const group = { id: groupId, name: badge.querySelector('.group-badge-name').textContent };
            this.selectedGroups.push(group);
        }

        this.updateGroupSelectionSummary();
        this.updateAssignmentPreview();
        this.updateHiddenInputs();
        this.updateStudentSelectionVisibility(); // Update student list when groups change
    }

    toggleGroupSelectionLegacy(groupId, groupName, optionElement) {
        const index = this.selectedGroups.findIndex(g => g.id === groupId);
        
        if (index > -1) {
            // Remove selection
            this.selectedGroups.splice(index, 1);
            optionElement.classList.remove('selected');
        } else {
            // Add selection
            this.selectedGroups.push({ id: groupId, name: groupName });
            optionElement.classList.add('selected');
        }

        this.updateGroupSelection();
        this.updateAssignmentPreview();
    }

    updateGroupSelectionSummary() {
        const summary = document.getElementById('groupSelectionSummary');
        if (!summary) return;

        const summaryText = summary.querySelector('.summary-text');
        if (this.selectedGroups.length === 0) {
            summaryText.textContent = 'هیچ گروهی انتخاب نشده - برای همه گروه‌ها';
        } else {
            const groupNames = this.selectedGroups.map(g => g.name).join('، ');
            summaryText.textContent = `گروه‌های انتخاب شده: ${groupNames}`;
        }
    }

    async loadChaptersWithSubChapters() {
        try {
            const teachingPlanId = this.getTeachingPlanId();
            const response = await fetch(`/Teacher/Schedule/GetSubChapters?teachingPlanId=${teachingPlanId}`);
            const subChapters = await response.json();

            if (subChapters.success) {
                this.renderChapterHierarchy(subChapters.data);
            }
        } catch (error) {
            console.error('Error loading chapters:', error);
        }
    }

    renderChapterHierarchy(subChapters) {
        const container = document.getElementById('chapterList');
        if (!container) return;

        // Group subchapters by chapter
        const chaptersMap = new Map();
        subChapters.forEach(subChapter => {
            const chapterTitle = subChapter.chapterTitle;
            if (!chaptersMap.has(chapterTitle)) {
                chaptersMap.set(chapterTitle, []);
            }
            chaptersMap.get(chapterTitle).push(subChapter);
        });

        container.innerHTML = Array.from(chaptersMap.entries()).map(([chapterTitle, chapterSubChapters]) => `
            <div class="chapter-item">
                <div class="chapter-header">
                    <div class="chapter-icon">
                        <i class="fas fa-book"></i>
                    </div>
                    <div class="chapter-info">
                        <div class="chapter-title">${chapterTitle}</div>
                        <div class="chapter-description">${chapterSubChapters.length} زیرمبحث</div>
                    </div>
                    <div class="chapter-toggle">
                        <i class="fas fa-chevron-down"></i>
                    </div>
                </div>
                <div class="subchapter-grid">
                    ${chapterSubChapters.map(subChapter => `
                        <div class="subchapter-badge" data-sub-chapter-id="${subChapter.id}">
                            <div class="subchapter-content">
                                <div class="subchapter-icon">
                                    <i class="fas fa-list"></i>
                                </div>
                                <div class="subchapter-info">
                                    <div class="subchapter-title">${subChapter.title}</div>
                                    <div class="subchapter-meta">زیرمبحث</div>
                                </div>
                            </div>
                            <div class="subchapter-check"></div>
                        </div>
                    `).join('')}
                </div>
            </div>
        `).join('');
    }

    toggleChapterExpansion(chapterItem) {
        chapterItem.classList.toggle('expanded');
    }

    toggleSubChapterSelection(subChapterId) {
        const badge = document.querySelector(`[data-sub-chapter-id="${subChapterId}"]`);
        if (!badge) return;

        const isSelected = badge.classList.contains('selected');
        
        if (isSelected) {
            badge.classList.remove('selected');
            this.selectedSubChapters = this.selectedSubChapters.filter(sc => sc.id !== subChapterId);
        } else {
            badge.classList.add('selected');
            const subChapter = { 
                id: subChapterId, 
                title: badge.querySelector('.subchapter-title').textContent 
            };
            this.selectedSubChapters.push(subChapter);
        }

        this.updateSubChapterSelectionSummary();
        this.updateAssignmentPreview();
        this.updateHiddenInputs();
        this.validateSubChapterSelection();
    }

    updateSubChapterSelectionSummary() {
        const summary = document.getElementById('subChapterSelectionSummary');
        if (!summary) return;

        const summaryText = summary.querySelector('.summary-text');
        if (this.selectedSubChapters.length === 0) {
            summaryText.textContent = 'هیچ زیرمبحثی انتخاب نشده';
        } else {
            const subChapterNames = this.selectedSubChapters.map(sc => sc.title).join('، ');
            summaryText.textContent = `زیرمباحث انتخاب شده: ${subChapterNames}`;
        }
    }

    getTeachingPlanId() {
        const teachingPlanIdInput = document.querySelector('input[name="TeachingPlanId"]');
        return teachingPlanIdInput ? teachingPlanIdInput.value : null;
    }

    setupStepProgressIndicators() {
        const stepIndicators = document.querySelectorAll('.step-indicator');
        const progressFill = document.getElementById('stepProgressFill');
        
        stepIndicators.forEach((indicator, index) => {
            indicator.addEventListener('click', () => {
                const stepNumber = parseInt(indicator.dataset.step);
                this.goToStep(stepNumber);
            });
        });

        // Update progress bar
        this.updateStepProgress();
    }

    updateStepProgress() {
        const progressFill = document.getElementById('stepProgressFill');
        if (progressFill) {
            const progress = (this.currentStep / this.totalSteps) * 100;
            progressFill.style.width = `${progress}%`;
        }

        // Update step indicators
        const stepIndicators = document.querySelectorAll('.step-indicator');
        stepIndicators.forEach((indicator, index) => {
            const stepNumber = index + 1;
            indicator.classList.remove('current', 'completed');
            
            if (stepNumber === this.currentStep) {
                indicator.classList.add('current');
            } else if (stepNumber < this.currentStep) {
                indicator.classList.add('completed');
            }
        });
    }

    updateHiddenInputs() {
        // Update hidden inputs for server submission
        const groupIdsInput = document.getElementById('selectedGroupIds');
        const subChapterIdsInput = document.getElementById('selectedSubChapterIds');
        const studentIdsInput = document.getElementById('selectedStudentIds');
        
        if (groupIdsInput) {
            groupIdsInput.value = this.selectedGroups.map(g => g.id).join(',');
        }
        
        if (subChapterIdsInput) {
            subChapterIdsInput.value = this.selectedSubChapters.map(sc => sc.id).join(',');
        }
        
        if (studentIdsInput) {
            studentIdsInput.value = this.selectedStudents.map(s => s.id).join(',');
        }
    }

    setupStudentSelection() {
        // Setup student selection functionality
        this.setupStudentSelectionListeners();
    }

    setupStudentSelectionListeners() {
        // Listen for group selection changes to show/hide student selection
        document.addEventListener('click', (e) => {
            if (e.target.closest('.group-badge')) {
                setTimeout(() => {
                    this.updateStudentSelectionVisibility();
                }, 100);
            }
        });

        // Setup select all and clear all buttons
        const selectAllBtn = document.getElementById('selectAllStudents');
        const clearAllBtn = document.getElementById('clearAllStudents');

        if (selectAllBtn) {
            selectAllBtn.addEventListener('click', () => {
                this.selectAllStudents();
            });
        }

        if (clearAllBtn) {
            clearAllBtn.addEventListener('click', () => {
                this.clearAllStudents();
            });
        }
    }

    updateStudentSelectionVisibility() {
        const studentSelectionContainer = document.getElementById('studentSelectionContainer');
        const studentListContainer = document.getElementById('studentListContainer');

        if (studentSelectionContainer && studentListContainer) {
            if (this.selectedGroups.length > 0) {
                // Show student selection for selected groups
                studentSelectionContainer.style.display = 'block';
                studentListContainer.style.display = 'block';
                
                // Load students for all selected groups
                this.loadStudentsForSelectedGroups();
            } else {
                // Hide student selection for no groups
                studentSelectionContainer.style.display = 'none';
                this.allStudents = [];
                this.renderStudents();
            }
        }
    }

    async loadStudentsForSelectedGroups() {
        try {
            this.allStudents = [];
            
            // Load students for each selected group
            for (const group of this.selectedGroups) {
                const response = await fetch(`/Teacher/StudentGroup/GetStudents?groupId=${group.id}`);
                if (response.ok) {
                    const students = await response.json();
                    // Add group info to each student
                    const studentsWithGroup = students.map(student => ({
                        ...student,
                        groupId: group.id,
                        groupName: group.name
                    }));
                    this.allStudents.push(...studentsWithGroup);
                }
            }
            
            // Remove duplicates based on student ID
            this.allStudents = this.allStudents.filter((student, index, self) => 
                index === self.findIndex(s => s.id === student.id)
            );
            
            // Update selected students with full details from loaded students
            this.updateSelectedStudentsWithDetails();
            
            this.renderStudents();
            this.updateStudentCount();
        } catch (error) {
            console.error('Error loading students:', error);
        }
    }

    updateSelectedStudentsWithDetails() {
        // Update selected students with full details from loaded students
        this.selectedStudents = this.selectedStudents.map(selectedStudent => {
            const fullStudentDetails = this.allStudents.find(student => student.id === selectedStudent.id);
            if (fullStudentDetails) {
                return {
                    ...fullStudentDetails,
                    // Keep the original selected state
                    isSelected: true
                };
            }
            return selectedStudent;
        });
        
        // Remove any selected students that are no longer in the allStudents list
        this.selectedStudents = this.selectedStudents.filter(selectedStudent => 
            this.allStudents.some(student => student.id === selectedStudent.id)
        );
        
        console.log('Updated selected students:', this.selectedStudents);
        console.log('All students:', this.allStudents);
    }

    async loadStudentsForGroup(groupId) {
        try {
            const response = await fetch(`/Teacher/StudentGroup/GetStudents?groupId=${groupId}`);
            if (response.ok) {
                const students = await response.json();
                this.allStudents = students;
                this.updateSelectedStudentsWithDetails();
                this.renderStudents();
                this.updateStudentCount();
            }
        } catch (error) {
            console.error('Error loading students:', error);
        }
    }

    renderStudents() {
        const studentGrid = document.getElementById('studentGrid');
        if (!studentGrid) return;

        studentGrid.innerHTML = '';

        console.log('Rendering students. All students:', this.allStudents.length);
        console.log('Selected students:', this.selectedStudents.length);

        this.allStudents.forEach(student => {
            const studentBadge = document.createElement('div');
            studentBadge.className = 'student-badge';
            studentBadge.dataset.studentId = student.id;
            
            const isSelected = this.selectedStudents.some(s => s.id === student.id);
            console.log(`Student ${student.id} (${student.firstName} ${student.lastName}) isSelected:`, isSelected);
            
            if (isSelected) {
                studentBadge.classList.add('selected');
            }

            studentBadge.innerHTML = `
                <div class="student-avatar">
                    ${student.firstName ? student.firstName.charAt(0) : '?'}
                </div>
                <div class="student-info">
                    <div class="student-name">${student.firstName || ''} ${student.lastName || ''}</div>
                    <div class="student-group">${student.groupName || ''}</div>
                </div>
            `;

            studentBadge.addEventListener('click', () => {
                this.toggleStudentSelection(student);
            });

            studentGrid.appendChild(studentBadge);
        });
    }

    toggleStudentSelection(student) {
        const existingIndex = this.selectedStudents.findIndex(s => s.id === student.id);
        
        if (existingIndex >= 0) {
            // Remove student
            this.selectedStudents.splice(existingIndex, 1);
        } else {
            // Add student
            this.selectedStudents.push(student);
        }

        // Update UI
        this.renderStudents();
        this.updateStudentCount();
        this.updateSelectedStudentsSummary();
        this.updateHiddenInputs();
    }

    selectAllStudents() {
        this.selectedStudents = [...this.currentGroupStudents];
        this.renderStudents();
        this.updateStudentCount();
        this.updateSelectedStudentsSummary();
        this.updateHiddenInputs();
    }

    clearAllStudents() {
        this.selectedStudents = [];
        this.renderStudents();
        this.updateStudentCount();
        this.updateSelectedStudentsSummary();
        this.updateHiddenInputs();
    }

    updateStudentCount() {
        const studentCount = document.getElementById('studentCount');
        if (studentCount) {
            studentCount.textContent = `${this.allStudents.length} دانش‌آموز`;
        }
    }

    updateSelectedStudentsSummary() {
        const selectedStudentsText = document.getElementById('selectedStudentsText');
        if (!selectedStudentsText) return;

        if (this.selectedStudents.length === 0) {
            selectedStudentsText.textContent = 'هیچ دانش‌آموزی انتخاب نشده - برای همه دانش‌آموزان گروه‌های انتخاب شده';
        } else if (this.selectedStudents.length === this.allStudents.length) {
            selectedStudentsText.textContent = 'همه دانش‌آموزان گروه‌های انتخاب شده انتخاب شده‌اند';
        } else {
            selectedStudentsText.textContent = `${this.selectedStudents.length} دانش‌آموز انتخاب شده`;
        }
    }

    setupModernGroupMultiSelect() {
        const toggle = document.getElementById('groupSelectToggle');
        const dropdown = document.getElementById('groupSelectDropdown');
        const container = document.getElementById('groupMultiSelect');
        const searchInput = document.getElementById('groupSearchInput');
        const selectAllBtn = document.getElementById('selectAllGroupsBtn');
        const clearBtn = document.getElementById('clearGroupsBtn');

        if (!toggle || !dropdown || !container) return;

        // Toggle dropdown
        toggle.addEventListener('click', (e) => {
            e.preventDefault();
            this.toggleMultiSelectDropdown(container, dropdown);
        });

        // Search functionality
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.filterMultiSelectOptions('groupOptionsList', e.target.value);
            });
        }

        // Select all
        if (selectAllBtn) {
            selectAllBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.selectAllGroups();
            });
        }

        // Clear all
        if (clearBtn) {
            clearBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.clearAllGroups();
            });
        }

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!container.contains(e.target)) {
                dropdown.style.display = 'none';
                container.classList.remove('open');
            }
        });

        // Load groups
        this.loadGroups();
    }

    setupModernSubChapterMultiSelect() {
        const toggle = document.getElementById('subChapterSelectToggle');
        const dropdown = document.getElementById('subChapterSelectDropdown');
        const container = document.getElementById('subChapterMultiSelect');
        const searchInput = document.getElementById('subChapterSearchInput');
        const selectAllBtn = document.getElementById('selectAllSubChaptersBtn');
        const clearBtn = document.getElementById('clearSubChaptersBtn');

        if (!toggle || !dropdown || !container) return;

        // Toggle dropdown
        toggle.addEventListener('click', (e) => {
            e.preventDefault();
            this.toggleMultiSelectDropdown(container, dropdown);
        });

        // Search functionality
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.filterMultiSelectOptions('subChapterOptionsList', e.target.value);
            });
        }

        // Select all
        if (selectAllBtn) {
            selectAllBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.selectAllSubChapters();
            });
        }

        // Clear all
        if (clearBtn) {
            clearBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.clearAllSubChapters();
            });
        }

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!container.contains(e.target)) {
                dropdown.style.display = 'none';
                container.classList.remove('open');
            }
        });

        // Load subchapters
        this.loadSubChapters();
    }

    toggleMultiSelectDropdown(container, dropdown) {
        const isOpen = dropdown.style.display === 'block';
        if (isOpen) {
            dropdown.style.display = 'none';
            container.classList.remove('open');
        } else {
            dropdown.style.display = 'block';
            container.classList.add('open');
        }
    }

    filterMultiSelectOptions(optionsListId, searchTerm) {
        const optionsList = document.getElementById(optionsListId);
        if (!optionsList) return;

        const options = optionsList.querySelectorAll('.option-item');
        const term = searchTerm.toLowerCase();

        options.forEach(option => {
            const text = option.querySelector('.option-text').textContent.toLowerCase();
            const subtitle = option.querySelector('.option-subtitle')?.textContent.toLowerCase() || '';
            
            if (text.includes(term) || subtitle.includes(term)) {
                option.style.display = 'flex';
            } else {
                option.style.display = 'none';
            }
        });
    }

    async loadGroups() {
        try {
            const teachingPlanId = document.querySelector('input[name="TeachingPlanId"]').value;
            const response = await fetch(`/Teacher/Schedule/GetGroups?teachingPlanId=${teachingPlanId}`);
            const groups = await response.json();

            this.renderGroupOptions(groups.data);
        } catch (error) {
            console.error('Error loading groups:', error);
        }
    }

    async loadSubChapters() {
        try {
            const teachingPlanId = document.querySelector('input[name="TeachingPlanId"]').value;
            const response = await fetch(`/Teacher/Schedule/GetSubChapters?teachingPlanId=${teachingPlanId}`);
            const subChapters = await response.json();

            this.renderSubChapterOptions(subChapters.data);
        } catch (error) {
            console.error('Error loading subchapters:', error);
        }
    }

    renderGroupOptions(groups) {
        const optionsList = document.getElementById('groupOptionsList');
        if (!optionsList) return;

        optionsList.innerHTML = '';

        if (groups.length === 0) {
            optionsList.innerHTML = '<div class="no-items-message">هیچ گروهی یافت نشد</div>';
            return;
        }

        groups.forEach(group => {
            const option = document.createElement('button');
            option.className = 'option-item';
            option.dataset.groupId = group.id;
            option.innerHTML = `
                <div class="option-checkbox"></div>
                <div class="option-text">${group.name}</div>
            `;

            option.addEventListener('click', (e) => {
                e.preventDefault();
                this.toggleGroupSelection(group.id, group.name, option);
            });

            optionsList.appendChild(option);
        });
    }

    renderSubChapterOptions(subChapters) {
        const optionsList = document.getElementById('subChapterOptionsList');
        if (!optionsList) return;

        optionsList.innerHTML = '';

        if (subChapters.length === 0) {
            optionsList.innerHTML = '<div class="no-items-message">هیچ زیرمبحثی یافت نشد</div>';
            return;
        }

        subChapters.forEach(subChapter => {
            const option = document.createElement('button');
            option.className = 'option-item';
            option.dataset.subChapterId = subChapter.id;
            option.innerHTML = `
                <div class="option-checkbox"></div>
                <div class="option-text">${subChapter.title}</div>
                <div class="option-subtitle">${subChapter.chapterTitle}</div>
            `;

            option.addEventListener('click', (e) => {
                e.preventDefault();
                this.toggleSubChapterSelectionLegacy(subChapter.id, subChapter.title, option);
            });

            optionsList.appendChild(option);
        });
    }


    toggleSubChapterSelectionLegacy(subChapterId, subChapterTitle, optionElement) {
        const index = this.selectedSubChapters.findIndex(sc => sc.id === subChapterId);
        
        if (index > -1) {
            // Remove selection
            this.selectedSubChapters.splice(index, 1);
            optionElement.classList.remove('selected');
        } else {
            // Add selection
            this.selectedSubChapters.push({ id: subChapterId, title: subChapterTitle });
            optionElement.classList.add('selected');
        }

        this.updateSubChapterSelection();
        this.updateAssignmentPreview();
        this.validateSubChapterSelection();
    }

    updateGroupSelection() {
        const selectedGroupsContainer = document.getElementById('selectedGroups');
        const groupIdsInput = document.getElementById('selectedGroupIds');
        const toggleText = document.querySelector('#groupSelectToggle .select-text');

        if (!selectedGroupsContainer || !groupIdsInput) return;

        // Update hidden input
        groupIdsInput.value = this.selectedGroups.map(g => g.id).join(',');

        // Update toggle text
        if (this.selectedGroups.length === 0) {
            toggleText.textContent = 'همه گروه‌ها';
        } else if (this.selectedGroups.length === 1) {
            toggleText.textContent = this.selectedGroups[0].name;
        } else {
            toggleText.textContent = `${this.selectedGroups.length} گروه انتخاب شده`;
        }

        // Update selected items display
        selectedGroupsContainer.innerHTML = '';
        this.selectedGroups.forEach(group => {
            const item = document.createElement('div');
            item.className = 'selected-item';
            item.innerHTML = `
                <span>${group.name}</span>
                <button type="button" class="selected-item-remove" data-group-id="${group.id}">
                    <i class="fas fa-times"></i>
                </button>
            `;

            item.querySelector('.selected-item-remove').addEventListener('click', (e) => {
                e.preventDefault();
                this.removeGroupSelection(group.id);
            });

            selectedGroupsContainer.appendChild(item);
        });
    }

    updateSubChapterSelection() {
        const selectedSubChaptersContainer = document.getElementById('selectedSubChapters');
        const subChapterIdsInput = document.getElementById('selectedSubChapterIds');
        const toggleText = document.querySelector('#subChapterSelectToggle .select-text');

        if (!selectedSubChaptersContainer || !subChapterIdsInput) return;

        // Update hidden input
        subChapterIdsInput.value = this.selectedSubChapters.map(sc => sc.id).join(',');

        // Update toggle text
        if (this.selectedSubChapters.length === 0) {
            toggleText.textContent = 'انتخاب زیرمباحث...';
        } else if (this.selectedSubChapters.length === 1) {
            toggleText.textContent = this.selectedSubChapters[0].title;
        } else {
            toggleText.textContent = `${this.selectedSubChapters.length} زیرمبحث انتخاب شده`;
        }

        // Update selected items display
        selectedSubChaptersContainer.innerHTML = '';
        this.selectedSubChapters.forEach(subChapter => {
            const item = document.createElement('div');
            item.className = 'selected-item';
            item.innerHTML = `
                <span>${subChapter.title}</span>
                <button type="button" class="selected-item-remove" data-sub-chapter-id="${subChapter.id}">
                    <i class="fas fa-times"></i>
                </button>
            `;

            item.querySelector('.selected-item-remove').addEventListener('click', (e) => {
                e.preventDefault();
                this.removeSubChapterSelection(subChapter.id);
            });

            selectedSubChaptersContainer.appendChild(item);
        });
    }

    removeGroupSelection(groupId) {
        this.selectedGroups = this.selectedGroups.filter(g => g.id !== groupId);
        
        // Update option element
        const option = document.querySelector(`[data-group-id="${groupId}"]`);
        if (option) {
            option.classList.remove('selected');
        }

        this.updateGroupSelection();
        this.updateAssignmentPreview();
    }

    removeSubChapterSelection(subChapterId) {
        this.selectedSubChapters = this.selectedSubChapters.filter(sc => sc.id !== subChapterId);
        
        // Update option element
        const option = document.querySelector(`[data-sub-chapter-id="${subChapterId}"]`);
        if (option) {
            option.classList.remove('selected');
        }

        this.updateSubChapterSelection();
        this.updateAssignmentPreview();
        this.validateSubChapterSelection();
    }

    selectAllGroups() {
        const options = document.querySelectorAll('#groupOptionsList .option-item');
        options.forEach(option => {
            const groupId = parseInt(option.dataset.groupId);
            const groupName = option.querySelector('.option-text').textContent;
            
            if (!this.selectedGroups.find(g => g.id === groupId)) {
                this.selectedGroups.push({ id: groupId, name: groupName });
                option.classList.add('selected');
            }
        });

        this.updateGroupSelection();
        this.updateAssignmentPreview();
    }

    clearAllGroups() {
        this.selectedGroups = [];
        
        // Update option elements
        const options = document.querySelectorAll('#groupOptionsList .option-item');
        options.forEach(option => {
            option.classList.remove('selected');
        });

        this.updateGroupSelection();
        this.updateAssignmentPreview();
    }

    selectAllSubChapters() {
        const options = document.querySelectorAll('#subChapterOptionsList .option-item');
        options.forEach(option => {
            const subChapterId = parseInt(option.dataset.subChapterId);
            const subChapterTitle = option.querySelector('.option-text').textContent;
            
            if (!this.selectedSubChapters.find(sc => sc.id === subChapterId)) {
                this.selectedSubChapters.push({ id: subChapterId, title: subChapterTitle });
                option.classList.add('selected');
            }
        });

        this.updateSubChapterSelection();
        this.updateAssignmentPreview();
        this.validateSubChapterSelection();
    }

    clearAllSubChapters() {
        this.selectedSubChapters = [];
        
        // Update option elements
        const options = document.querySelectorAll('#subChapterOptionsList .option-item');
        options.forEach(option => {
            option.classList.remove('selected');
        });

        this.updateSubChapterSelection();
        this.updateAssignmentPreview();
        this.validateSubChapterSelection();
    }

    validateSubChapterSelection() {
        const errorElement = document.getElementById('subChapterValidationError');
        const container = document.getElementById('subChapterMultiSelect');
        
        if (!errorElement || !container) return;

        if (this.selectedSubChapters.length === 0) {
            errorElement.style.display = 'block';
            container.classList.add('error');
            this.validationErrors.subChapters = 'انتخاب حداقل یک زیرمبحث اجباری است';
        } else {
            errorElement.style.display = 'none';
            container.classList.remove('error');
            delete this.validationErrors.subChapters;
        }
    }

    updateAssignmentPreview() {
        const targetGroupsElement = document.getElementById('targetGroups');
        const relatedSubChaptersElement = document.getElementById('relatedSubChapters');

        if (targetGroupsElement) {
            if (this.selectedGroups.length === 0) {
                targetGroupsElement.textContent = 'همه گروه‌ها';
            } else {
                targetGroupsElement.textContent = this.selectedGroups.map(g => g.name).join(', ');
            }
        }

        if (relatedSubChaptersElement) {
            if (this.selectedSubChapters.length === 0) {
                relatedSubChaptersElement.textContent = 'انتخاب نشده';
            } else {
                relatedSubChaptersElement.textContent = this.selectedSubChapters.map(sc => sc.title).join(', ');
            }
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ModernScheduleItemFormManager();
});