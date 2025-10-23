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
        this.lastCreatedItemId = null;
        this.handleBadgeClick = null; // Initialize badge click handler
        this.isNavigating = false; // Prevent rapid step changes
        this.isSaving = false; // Prevent multiple save operations
        
        // Step 3 manager
        this.step3Manager = null;
        this.step3Initialized = false;
        
        // Track changes for each step
        this.stepChanges = {
            1: false,
            2: false,
            3: false,
            4: false
        };
        this.originalStepData = {
            1: null,
            2: null,
            3: null,
            4: null
        };
        
        this.init();
    }

    detectEditMode() {
        // Check if we're in edit mode by looking at the hidden input field
        const idInput = document.querySelector('input[name="id"]');
        if (idInput && parseInt(idInput.value) > 0) {
            this.isEditMode = true;
            this.currentItemId = parseInt(idInput.value);
            return;
        }
        
        // Also check URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        const idFromUrl = urlParams.get('id');
        if (idFromUrl && parseInt(idFromUrl) > 0) {
            this.isEditMode = true;
            this.currentItemId = parseInt(idFromUrl);
            
            // Update hidden input field if it exists
            if (idInput) {
                idInput.value = this.currentItemId;
            }
        }
    }

    async init() {
        // Check if we're in edit mode
        this.detectEditMode();
        
        this.setupEventListeners();
        // Step 1 basics handled by Step1BasicsManager
        this.setupStepNavigation();
        this.setupFormValidation();
        this.updateProgress();
        this.setupAutoSave();
        // Step 2 timing handled by Step2TimingManager
        // Initialize step 3 manager early
        this.initializeStep3Manager();
        
        // Check for existing item after a delay to ensure step managers are initialized
        setTimeout(async () => {
            await this.checkForExistingItem();
            
            // If we're already on step 3 or 4, initialize step 3 content
            // Step 4 needs step 3 data for getSelectedSubChapters()
            if (this.currentStep === 3 || this.currentStep === 4) {
                // Delay to ensure DOM is ready
                setTimeout(async () => {
                    await this.initializeStep3();
                }, 100);
            }
        }, 200);
        
       
    }

    setupEventListeners() {
        // Step navigation
        this.setupStepNavigationListeners();

        // Form submit prevention
        this.setupFormSubmitListener();

        // Step 1 form inputs and rich editor handled by Step1BasicsManager

        // Preview functionality
        this.setupPreviewListeners();

        // Content type selection handled by Step4ContentManager

        // Step 2 datetime helpers handled by Step2TimingManager

        // Step 3 components will be handled by Step3AssignmentManager
        // Score presets handled by Step2TimingManager

        // Step progress indicators
        this.setupStepProgressIndicators();

        // Student selection handled by Step3AssignmentManager
        
        // Setup change detection
        this.setupChangeDetection();
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
    }

    setupFormSubmitListener() {
        const form = document.getElementById('createItemForm');
        if (form) {
            form.addEventListener('submit', (e) => {
                e.preventDefault();
                e.stopPropagation();
                // Form submission is handled by step navigation buttons
                return false;
            });
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

    // Step 1 form inputs handled by Step1BasicsManager
    // Step 2 datetime inputs handled by Step2TimingManager
    // Step 3 group/lesson selection handled by Step3AssignmentManager

    async updateAssignmentPreview() {
        if (!this.step3Manager) return;
        
        const targetGroupsElement = document.getElementById('targetGroups');
        const relatedSubChaptersElement = document.getElementById('relatedSubChapters');
        
        // Ensure step 3 manager is ready before accessing its data
        await this.ensureStep3ManagerReady();
        
        const selectedGroups = this.step3Manager ? this.step3Manager.getSelectedGroups() : [];
        const selectedSubChapters = this.step3Manager ? this.step3Manager.getSelectedSubChapters() : [];

        if (targetGroupsElement) {
            if (selectedGroups.length === 0) {
                targetGroupsElement.textContent = 'همه گروه‌ها';
            } else {
                targetGroupsElement.textContent = selectedGroups.map(g => g.name).join(', ');
            }
        }

        if (relatedSubChaptersElement) {
            if (selectedSubChapters.length === 0) {
                relatedSubChaptersElement.textContent = 'انتخاب نشده';
            } else {
                relatedSubChaptersElement.textContent = selectedSubChapters.map(sc => sc.title).join(', ');
            }
        }
    }

    // Delegated to Step1BasicsManager

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

    // setupContentTypeListeners delegated to Step4ContentManager

    // Step 4 content builder handled by Step4ContentManager

    // Step 4 content builder events handled by Step4ContentManager

    // Step 4 content UI is handled by Step4ContentManager

    // Delegated to Step4ContentManager

    // Delegated to Step4ContentManager

    // Delegated to Step4ContentManager

    // Delegated to Step4ContentManager

    // Delegated to Step4ContentManager

    // Delegated to Step4ContentManager

    showSuccess(message) {
        // You can implement a toast notification system here
        // Success message displayed
    }

    // Step 2 datetime helpers handled by Step2TimingManager

    // Delegated to Step2TimingManager

    // Delegated to Step2TimingManager

    // Delegated to Step2TimingManager

    // Delegated to Step2TimingManager

    // All Step 2 methods delegated to Step2TimingManager

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
        if (step < 1 || step > this.totalSteps) {
            return;
        }

        // In create mode, only allow step 1
        if (!this.isEditMode && step > 1) {
            return;
        }

        // Prevent rapid step changes
        if (this.isNavigating) {
            return;
        }

        this.isNavigating = true;

        // Validate current step before moving forward
        if (step > this.currentStep && !this.validateCurrentStep()) {
            this.isNavigating = false;
            return;
        }

        const previousStep = this.currentStep;
        this.currentStep = step;
        
        
        // Update UI
        this.updateStepVisibility();
        this.updateStepIndicators();
        this.updateProgress();
        this.updateNavigationButtons();
        
        // Clear cache for the step we're going to
        this.clearStepCache(step);
        
        // Initialize step-specific content and load step data
        this.initializeStepContent(step);

        // Reset navigation flag after a short delay
        setTimeout(() => {
            this.isNavigating = false;
        }, 300);
    }

    async initializeStepContent(step) {
        
        switch (step) {
            case 1:
                // Step 1: Load basic info data
                if (window.step1Manager && typeof window.step1Manager.loadStepData === 'function') {
                    await window.step1Manager.loadStepData();
                }
                break;
                
            case 2:
                // Step 2: Load timing data
                if (window.step2Manager && typeof window.step2Manager.loadStepData === 'function') {
                    await window.step2Manager.loadStepData();
                    // Update duration calculator after loading data
                    setTimeout(() => {
                        if (window.step2Manager && typeof window.step2Manager.updateDurationCalculator === 'function') {
                            window.step2Manager.updateDurationCalculator();
                        }
                    }, 100);
                }
                break;
                
            case 3:
                // Step 3: Initialize assignment manager and load data
                await this.initializeStep3();
                // Wait a bit for UI to be ready, then load data
                setTimeout(async () => {
                    // Make sure step3Manager is available globally
                    if (this.step3Manager && !window.step3Manager) {
                        window.step3Manager = this.step3Manager;
                    }
                    if (window.step3Manager && typeof window.step3Manager.loadStepData === 'function') {
                        await window.step3Manager.loadStepData();
                    }
                }, 500);
                break;
                
            case 4:
                // Step 4: Load content data
                if (window.step4Manager && typeof window.step4Manager.updateStep4Content === 'function') {
                    window.step4Manager.updateStep4Content();
                }
                if (window.step4Manager && typeof window.step4Manager.loadStepData === 'function') {
                    await window.step4Manager.loadStepData();
                }
                break;
        }
    }

    initializeStep3Manager() {
        // Initialize step 3 manager if not already done
        if (!this.step3Manager) {
            // Check if Step3AssignmentManager is available
            if (typeof window.Step3AssignmentManager === 'undefined') {
                return;
            }
            this.step3Manager = new window.Step3AssignmentManager(this);
        }
    }

    // Ensure step 3 manager is ready and has data loaded
    async ensureStep3ManagerReady() {
        if (!this.step3Manager) {
            this.initializeStep3Manager();
        }
        
        if (this.step3Manager && !this.step3Initialized) {
            await this.initializeStep3();
        }
        
        return this.step3Manager;
    }

    async initializeStep3() {
        // Prevent double initialization
        if (this.step3Initialized) {
            return;
        }
        
        // Initialize step 3 manager if not already done
        if (!this.step3Manager) {
            this.initializeStep3Manager();
        }
        
        // Initialize step 3 content
        if (this.step3Manager) {
            await this.step3Manager.initializeStep3Content();
            this.step3Initialized = true;
        }
    }


    // Cleanup method for memory management
    cleanup() {
        
        // Cleanup step 3 manager
        if (this.step3Manager && typeof this.step3Manager.cleanup === 'function') {
            this.step3Manager.cleanup();
        }
        
    }

    async createItem() {
        try {
            // Collect form data from step 1
            const formData = this.collectStep1Data();
            
            // Create the item via API
            const response = await fetch('/Teacher/ScheduleItem/CreateScheduleItem', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(formData)
            });
            
            const result = await response.json();
            
            if (result.success) {
                this.lastCreatedItemId = result.id;
                this.showSuccessMessage('آیتم آموزشی با موفقیت ایجاد شد');
                return true;
            } else {
                this.showErrorMessage(result.message || 'خطا در ایجاد آیتم آموزشی');
                return false;
            }
        } catch (error) {
            console.error('Error creating item:', error);
            this.showErrorMessage('خطا در ایجاد آیتم آموزشی');
            return false;
        }
    }

    collectStep1Data() {
        const form = document.getElementById('createItemForm');
        const formData = new FormData(form);
        
        // Get values directly from form elements using correct names
        const titleInput = form.querySelector('input[name="Title"]');
        const descriptionInput = form.querySelector('textarea[name="Description"]');
        const typeSelect = form.querySelector('select[name="Type"]');
        
        // Debug: log the form data
        
        return {
            TeachingPlanId: parseInt(formData.get('TeachingPlanId')) || 0,
            GroupId: null, // Legacy single group assignment
            Title: titleInput ? titleInput.value : '',
            Description: descriptionInput ? descriptionInput.value : '',
            Type: typeSelect ? parseInt(typeSelect.value) : 1,
            StartDate: new Date().toISOString(), // Default start date
            DueDate: null, // No due date initially
            IsMandatory: false, // Default to false
            DisciplineHint: null, // No discipline hint initially
            ContentJson: '{}', // Empty content initially
            MaxScore: null, // No max score initially
            GroupIds: [],
            SubChapterIds: []
        };
    }

    async goToNextStep() {
        if (this.currentStep < this.totalSteps) {
            const nextBtn = document.getElementById('nextStepBtn');
            
            try {
                // In create mode, if we're on step 1, create the item first
                if (!this.isEditMode && this.currentStep === 1) {
                    // Disable next button and show loading state
                    this.setButtonLoadingState(nextBtn, true, 'در حال ایجاد آیتم...');
                    
                    // Create the item
                    const success = await this.createItem();
                    if (!success) {
                        this.setButtonLoadingState(nextBtn, false, 'ادامه');
                        return false;
                    }
                    
                    // After successful creation, switch to edit mode
                    this.isEditMode = true;
                    this.currentItemId = this.lastCreatedItemId;
                    
                    // Update the hidden input field
                    const idInput = document.querySelector('input[name="id"]');
                    if (idInput) {
                        idInput.value = this.currentItemId;
                    }
                    
                    // Update URL to include ID for edit mode
                    try {
                        const url = new URL(window.location);
                        url.searchParams.set('id', this.currentItemId);
                        window.history.replaceState({}, '', url);
                    } catch (error) {
                        console.error('Error updating URL:', error);
                    }
                    
                    // Now proceed to next step
                    this.goToStep(this.currentStep + 1);
                    this.setButtonLoadingState(nextBtn, false, 'ادامه');
                    return true;
                }
                
                // Validate current step before proceeding
                if (!this.validateCurrentStep()) {
                    this.setButtonLoadingState(nextBtn, false, 'ادامه');
                    return false;
                }
                
                // Only show confirmation dialog if there are changes
                if (this.shouldShowConfirmation()) {
                    const result = await this.showSaveConfirmation();
                    if (result.action === 'cancel') {
                        return false;
                    }
                    
                    if (result.action === 'save') {
                // Disable next button and show loading state
                this.setButtonLoadingState(nextBtn, true, 'در حال ذخیره...');
                
                // Save current step before moving to next
                await this.saveCurrentStep();
                        
                        // Mark step as saved
                        this.markStepAsSaved(this.currentStep);
                    }
                }
                
                // Show loading state for navigation
                this.setButtonLoadingState(nextBtn, true, 'در حال انتقال...');
                
                this.goToStep(this.currentStep + 1);
                
                // Re-enable button after navigation
                this.setButtonLoadingState(nextBtn, false, 'مرحله بعدی');
                
            } catch (error) {
                
                // Re-enable button on error
                this.setButtonLoadingState(nextBtn, false, 'مرحله بعدی');
                
                // Show more specific error message
                let errorMessage = 'خطا در ذخیره مرحله';
                if (error.message.includes('Duplicate key')) {
                    errorMessage = 'خطا: تخصیص تکراری دانش‌آموز یا مبحث';
                } else if (error.message.includes('Concurrency') || error.message.includes('تغییر کرده است')) {
                    errorMessage = 'خطا: داده‌ها توسط کاربر دیگری تغییر کرده است. لطفا صفحه را رفرش کنید';
                } else if (error.message.includes('HTTP 400')) {
                    errorMessage = 'خطا: داده‌های ارسالی نامعتبر است';
                }
                
                this.showErrorMessage(errorMessage);
                
                // Don't navigate if save failed
                return false;
            } finally {
                this.isSaving = false;
            }
        }
        return true;
    }

    async goToPreviousStep() {
        if (this.currentStep > 1) {
            const prevBtn = document.getElementById('prevStepBtn');
            
            try {
                // Disable previous button and show loading state
                this.setButtonLoadingState(prevBtn, true, 'در حال انتقال...');
                
                // Add a small delay to show loading state
                await new Promise(resolve => setTimeout(resolve, 300));
                
                this.goToStep(this.currentStep - 1);
                
                // Re-enable button after navigation
                this.setButtonLoadingState(prevBtn, false, 'مرحله قبلی');
                
            } catch (error) {
                
                // Re-enable button on error
                this.setButtonLoadingState(prevBtn, false, 'مرحله قبلی');
                
                this.showErrorMessage('خطا در انتقال به مرحله قبلی');
            }
        }
    }

    updateStepVisibility() {
        document.querySelectorAll('.form-step').forEach((step, index) => {
            const stepNumber = index + 1;
            // In create mode, only show step 1
            if (!this.isEditMode && stepNumber > 1) {
                step.style.display = 'none';
                step.classList.remove('active');
            } else {
                step.style.display = '';
                step.classList.toggle('active', stepNumber === this.currentStep);
            }
        });

        document.querySelectorAll('.step-item').forEach((item, index) => {
            const stepNumber = index + 1;
            item.classList.remove('active', 'completed');

            // In create mode, only allow step 1
            if (!this.isEditMode && stepNumber > 1) {
                item.classList.add('disabled');
            } else {
                item.classList.remove('disabled');
                if (stepNumber === this.currentStep) {
                    item.classList.add('active');
                } else if (stepNumber < this.currentStep) {
                    item.classList.add('completed');
                }
            }
        });
    }

    async updateStepIndicators() {
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

        // Show/hide previous button
        if (prevBtn) {
            if (this.currentStep === 1) {
                prevBtn.style.display = 'none';
            } else {
                prevBtn.style.display = 'flex';
                prevBtn.disabled = false;
            }
        }

        // Update next button text and visibility
        if (nextBtn) {
            if (this.currentStep === this.totalSteps) {
                nextBtn.style.display = 'none';
            } else {
                const stepNames = {
                    1: 'اطلاعات کلی',
                    2: 'زمان‌بندی',
                    3: 'تخصیص',
                    4: 'محتوای آموزشی'
                };
                const currentStepName = stepNames[this.currentStep] || 'مرحله';
                
                nextBtn.style.display = 'flex';
                
                // In create mode, change button text for step 1
                if (!this.isEditMode && this.currentStep === 1) {
                    nextBtn.innerHTML = `
                        <span>ایجاد آیتم و ادامه</span>
                        <i class="fas fa-chevron-left"></i>
                    `;
                } else {
                    nextBtn.innerHTML = `
                        <span>ثبت ${currentStepName} و ادامه</span>
                        <i class="fas fa-chevron-left"></i>
                    `;
                }
            }
        }

        // Show/hide submit button
        if (submitBtn) {
            if (this.currentStep === this.totalSteps) {
                const stepNames = {
                    1: 'اطلاعات کلی',
                    2: 'زمان‌بندی',
                    3: 'تخصیص',
                    4: 'محتوای آموزشی'
                };
                const currentStepName = stepNames[this.currentStep] || 'مرحله';
                
                submitBtn.style.display = 'flex';
                submitBtn.innerHTML = `
                    <i class="fas fa-check"></i>
                    <span>ثبت ${currentStepName}</span>
                `;
            } else {
                submitBtn.style.display = 'none';
            }
        }
    }

    setButtonLoadingState(button, isLoading, loadingText = null) {
        if (!button) return;

        if (isLoading) {
            // Store original HTML if not already stored
            if (!button.dataset.originalHTML) {
                button.dataset.originalHTML = button.innerHTML;
            }
            
            // Disable button and show loading state
            button.disabled = true;
            button.classList.add('loading');
            
            // For navigation buttons, keep the text and add minimal loading indicator
            if (button.classList.contains('nav-btn')) {
                // Add loading indicator to existing content
                const span = button.querySelector('span');
                if (span) {
                    span.innerHTML = loadingText || span.textContent;
                }
                return;
            }
            
            // For other buttons, use full loading state
            button.innerHTML = '';
            
            // Add spinner
            const spinner = document.createElement('span');
            spinner.className = 'spinner';
            spinner.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            button.appendChild(spinner);
            
            // Add loading text
            const textSpan = document.createElement('span');
            textSpan.textContent = loadingText || button.dataset.originalHTML;
            button.appendChild(textSpan);
            
        } else {
            // Re-enable button and remove loading state
            button.disabled = false;
            button.classList.remove('loading');
            
            // Restore original HTML
            if (button.dataset.originalHTML) {
                button.innerHTML = button.dataset.originalHTML;
                delete button.dataset.originalHTML;
            }
        }
    }

    // Check if current step has changes
    hasStepChanges(step) {
        return this.stepChanges[step] || false;
    }

    // Mark step as changed
    markStepAsChanged(step) {
        this.stepChanges[step] = true;
    }

    // Mark step as saved (no changes)
    markStepAsSaved(step) {
        this.stepChanges[step] = false;
        // Update original data after save
        this.originalStepData[step] = this.collectCurrentStepData();
    }

    // Clear step cache to force reload from server
    clearStepCache(step) {
        // Clear any cached data for the step
        if (this.originalStepData[step]) {
            this.originalStepData[step] = null;
        }
        // Reset change tracking
        this.stepChanges[step] = false;
    }


    // Setup change detection for form inputs
    setupChangeDetection() {
        // Use event delegation for better performance and coverage
        document.addEventListener('input', (e) => {
            const target = e.target;
            if (target.matches('input, select, textarea')) {
                this.markStepAsChanged(this.currentStep);
            }
        }, true);

        // Listen for changes on checkboxes and radio buttons
        document.addEventListener('change', (e) => {
            const target = e.target;
            if (target.matches('input[type="checkbox"], input[type="radio"], select')) {
                this.markStepAsChanged(this.currentStep);
            }
        }, true);

        // Listen for changes on rich text editors (contenteditable)
        document.addEventListener('input', (e) => {
            const target = e.target;
            if (target.matches('[contenteditable="true"]')) {
                this.markStepAsChanged(this.currentStep);
            }
        }, true);

        // Listen for changes on content builder (Step 4)
        document.addEventListener('contentChanged', (e) => {
            if (this.currentStep === 4) {
                this.markStepAsChanged(4);
            }
        });

        // Listen for changes on assignment selection (Step 3)
        document.addEventListener('assignmentChanged', (e) => {
            if (this.currentStep === 3) {
                this.markStepAsChanged(3);
            }
        });

        // Listen for changes on timing (Step 2)
        document.addEventListener('timingChanged', (e) => {
            if (this.currentStep === 2) {
                this.markStepAsChanged(2);
            }
        });

        // Also listen for click events on buttons that might change data
        document.addEventListener('click', (e) => {
            const target = e.target;
            if (target.matches('.date-preset-btn, .score-preset, .subchapter-badge, .student-badge, .group-badge, .toolbar-btn')) {
                this.markStepAsChanged(this.currentStep);
            }
        }, true);

        // Listen for changes on Persian date pickers
        document.addEventListener('change', (e) => {
            const target = e.target;
            if (target.matches('.persian-datepicker')) {
                this.markStepAsChanged(this.currentStep);
            }
        }, true);

        // Listen for changes on time inputs
        document.addEventListener('change', (e) => {
            const target = e.target;
            if (target.matches('input[type="time"]')) {
                this.markStepAsChanged(this.currentStep);
            }
        }, true);
    }

    // Check if we should show confirmation dialog
    shouldShowConfirmation() {
        const hasChanges = this.hasStepChanges(this.currentStep);
        return hasChanges;
    }

    // Force mark current step as changed (for testing)
    forceMarkCurrentStepAsChanged() {
        this.markStepAsChanged(this.currentStep);
    }

    // Debug method to check current state
    debugStepChanges() {
    }

    async showSaveConfirmation() {
        return new Promise((resolve) => {
            // Get current step name
            const stepNames = {
                1: 'اطلاعات کلی',
                2: 'زمان‌بندی',
                3: 'تخصیص',
                4: 'محتوای آموزشی'
            };
            const currentStepName = stepNames[this.currentStep] || 'مرحله';
            
            // Create confirmation dialog
            const dialog = document.createElement('div');
            dialog.className = 'save-confirmation-dialog';
            dialog.innerHTML = `
                <div class="dialog-overlay"></div>
                <div class="dialog-content">
                    <div class="dialog-header">
                        <i class="fas fa-save"></i>
                        <h4>ذخیره اطلاعات</h4>
                    </div>
                    <div class="dialog-body">
                        <p>آیا می‌خواهید اطلاعات <span class="step-name">${currentStepName}</span> را ذخیره کنید؟</p>
                    </div>
                    <div class="dialog-actions">
                        <button type="button" class="btn-cancel">انصراف</button>
                        <button type="button" class="btn-skip">ادامه بدون ذخیره</button>
                        <button type="button" class="btn-save">ذخیره و ادامه</button>
                    </div>
                </div>
            `;
            
            document.body.appendChild(dialog);
            
            // Handle button clicks
            const cancelBtn = dialog.querySelector('.btn-cancel');
            const skipBtn = dialog.querySelector('.btn-skip');
            const saveBtn = dialog.querySelector('.btn-save');
            
            const cleanup = () => {
                document.body.removeChild(dialog);
            };
            
            cancelBtn.addEventListener('click', () => {
                cleanup();
                resolve({ action: 'cancel' });
            });
            
            skipBtn.addEventListener('click', () => {
                cleanup();
                resolve({ action: 'skip' });
            });
            
            saveBtn.addEventListener('click', () => {
                cleanup();
                resolve({ action: 'save' });
            });
            
            // Handle overlay click
            dialog.querySelector('.dialog-overlay').addEventListener('click', () => {
                cleanup();
                resolve({ action: 'cancel' });
            });
        });
    }

    // Form Validation Methods
    validateCurrentStep() {
        let isValid = true;
        
        // Step-specific validation using step managers
        switch (this.currentStep) {
            case 1:
                // Use Step1BasicsManager validation
                if (window.step1BasicsManager && typeof window.step1BasicsManager.validateStep1 === 'function') {
                    isValid = window.step1BasicsManager.validateStep1();
                }
                break;
            case 2:
                // Use Step2TimingManager validation
                if (window.step2TimingManager && typeof window.step2TimingManager.validateStep2 === 'function') {
                    isValid = window.step2TimingManager.validateStep2();
                }
                break;
            case 3:
                // Use Step3AssignmentManager validation
                if (this.step3Manager && typeof this.step3Manager.validateStep3 === 'function') {
                    isValid = this.step3Manager.validateStep3();
                }
                break;
            case 4:
                // Use Step4ContentManager validation
                if (window.step4ContentManager && typeof window.step4ContentManager.validateStep4 === 'function') {
                    isValid = window.step4ContentManager.validateStep4();
                }
                break;
            default:
                isValid = true;
        }
        
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
    // All Step 1 methods delegated to Step1BasicsManager









    // Delegated to Step1BasicsManager

    // Utility Methods - updateCharacterCount delegated to Step1BasicsManager

    formatDateTimeLocal(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');

        return `${year}-${month}-${day}T${hours}:${minutes}`;
    }

    // Delegated to Step2TimingManager


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
    }

    async saveFromPreview() {
        // Save form from preview mode using AJAX
        try {
            await this.saveCurrentStep();
        } catch (error) {
            console.error('Error saving from preview:', error);
            this.showErrorMessage('خطا در ذخیره از پیش‌نمایش');
        }
    }

    async collectFormData() {
        const form = document.getElementById('createItemForm');
        if (!form) return {};

        const formData = new FormData(form);
        const data = {};

        for (let [key, value] of formData.entries()) {
            data[key] = value;
        }

        // Add selected groups and subchapters for badge-based selection
        if (this.step3Manager) {
            // Ensure step 3 manager is ready before accessing its data
            await this.ensureStep3ManagerReady();
            
            const selectedGroups = this.step3Manager ? this.step3Manager.getSelectedGroups() : [];
            const selectedSubChapters = this.step3Manager ? this.step3Manager.getSelectedSubChapters() : [];
            
            if (selectedGroups.length > 0) {
                data.GroupIds = selectedGroups.map(g => g.id);
            }
            if (selectedSubChapters.length > 0) {
                data.SubChapterIds = selectedSubChapters.map(sc => sc.id);
            }
        }
        if (this.step3Manager) {
            const selectedStudents = this.step3Manager.getSelectedStudents();
            if (selectedStudents.length > 0) {
                data.StudentIds = selectedStudents.map(s => s.id);
            }
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
    async handleFormSubmit(e) {
        e.preventDefault();
        const submitBtn = document.getElementById('submitBtn');

        try {
            // Disable submit button and show loading state
            this.setButtonLoadingState(submitBtn, true, 'در حال ذخیره نهایی...');

            // Validate all steps
            if (!this.validateAllSteps()) {
                this.setButtonLoadingState(submitBtn, false, 'ذخیره نهایی');
                this.showValidationErrors();
                return;
            }

            // Collect form data
            const formData = this.collectFormData();

            // Add content data from Step 4 manager
            if (window.step4Manager && typeof window.step4Manager.collectContentData === 'function') {
                formData.contentData = window.step4Manager.collectContentData();
            }

            // Submit form
            await this.submitForm(formData);
            
            // Re-enable button after successful submission
            this.setButtonLoadingState(submitBtn, false, 'ذخیره نهایی');
            
        } catch (error) {
            
            // Re-enable button on error
            this.setButtonLoadingState(submitBtn, false, 'ذخیره نهایی');
            
            this.showErrorMessage('خطا در ذخیره نهایی فرم');
        }
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

    // Delegated to Step4ContentManager

    async submitForm(formData) {
        // Validate subchapter selection before submission
        // Ensure step 3 manager is ready before accessing its data
        await this.ensureStep3ManagerReady();
        const selectedSubChapters = this.step3Manager ? this.step3Manager.getSelectedSubChapters() : [];
        if (selectedSubChapters.length === 0) {
            this.showErrorMessage('انتخاب حداقل یک زیرمبحث اجباری است');
            this.goToStep(3); // Go to assignment step
            throw new Error('Subchapter selection required');
        }

        // Add selected groups and subchapters to form data
        if (this.step3Manager) {
            const selectedGroups = this.step3Manager.getSelectedGroups();
            if (selectedGroups.length > 0) {
                formData.GroupIds = selectedGroups.map(g => g.id);
            }
            if (selectedSubChapters.length > 0) {
                formData.SubChapterIds = selectedSubChapters.map(sc => sc.id);
            }
        }

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
                // Stay on current step instead of redirecting
            } else {
                throw new Error(result.message || 'خطا در تکمیل آیتم');
            }
        } catch (error) {
            throw error; // Re-throw to be handled by caller
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

    // All Persian date picker methods delegated to Step2TimingManager


    // Step Saving Methods
    async saveCurrentStep() {
        
        // Prevent multiple simultaneous save operations
        if (this.isSaving) {
            return;
        }

        this.isSaving = true;
        
        try {
            // Collect step data from the appropriate step manager
            const stepData = await this.collectCurrentStepData();

            const requestData = {
                Id: this.currentItemId,
                TeachingPlanId: parseInt(document.querySelector('input[name="TeachingPlanId"]')?.value) || 0,
                Step: this.currentStep,
                ...stepData
            };

            // For step 3, ensure we have proper data structure
            if (this.currentStep === 3) {
                if (!requestData.GroupIds) requestData.GroupIds = [];
                if (!requestData.SubChapterIds) requestData.SubChapterIds = [];
                if (!requestData.StudentIds) requestData.StudentIds = [];
            }

            // Ensure ContentJson is a string, not an object
            if (requestData.ContentJson && typeof requestData.ContentJson === 'object') {
                requestData.ContentJson = JSON.stringify(requestData.ContentJson);
            }

            // Debug logging for step 4
            if (this.currentStep === 4) {
                console.log('=== Step 4 Save Debug ===');
                console.log('Request Data:', requestData);
                console.log('ContentJson value:', requestData.ContentJson);
                console.log('ContentJson type:', typeof requestData.ContentJson);
                console.log('========================');
            }

            const response = await fetch('/Teacher/ScheduleItem/SaveStep', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify(requestData)
            });

            // Check if response is ok and has content
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const responseText = await response.text();

            if (!responseText) {
                throw new Error('Empty response from server');
            }

            const result = JSON.parse(responseText);

            if (result.success) {
                this.currentItemId = result.id;
                this.isEditMode = true;
                this.showSuccessMessage('مرحله با موفقیت ذخیره شد');

                // Update URL to include ID for edit mode
                try {
                    const url = new URL(window.location);
                    url.searchParams.set('id', result.id);
                    window.history.replaceState({}, '', url);
                } catch (error) {
                    // Don't throw error here, just log it
                }

                // Load existing data after save
                try {
                    await this.loadExistingItem();
                } catch (error) {
                    console.error('Error loading existing item after save:', error);
                }

                return result;
            } else {
                throw new Error(result.message || 'خطا در ذخیره مرحله');
            }
        } catch (error) {
            this.isSaving = false; // Reset flag on error
            throw error;
        } finally {
            this.isSaving = false; // Ensure flag is always reset
        }
    }

    // Method to manually reset isSaving flag (for debugging)
    resetSavingFlag() {
        this.isSaving = false;
    }

    async collectCurrentStepData() {
        // Collect data from all steps that have been completed
        const stepData = {};

        if (this.currentStep >= 1 && window.step1Manager && typeof window.step1Manager.collectStep1Data === 'function') {
            const step1Data = window.step1Manager.collectStep1Data();
            Object.assign(stepData, step1Data);
        }
        
        if (this.currentStep >= 2 && window.step2Manager && typeof window.step2Manager.collectStep2Data === 'function') {
            const step2Data = window.step2Manager.collectStep2Data();
            Object.assign(stepData, step2Data);
        }
        
        if (this.currentStep >= 3) {
            // Make sure step3Manager is available
            if (this.step3Manager && !window.step3Manager) {
                window.step3Manager = this.step3Manager;
            }
            if (window.step3Manager && typeof window.step3Manager.collectStep3Data === 'function') {
                const step3Data = window.step3Manager.collectStep3Data();
                Object.assign(stepData, step3Data);
            }
        }
        
        if (this.currentStep >= 4) {
            // Make sure step4Manager is available
            if (!window.step4Manager) {
                console.warn('Step4Manager not available, attempting to initialize...');
                if (typeof window.Step4ContentManager !== 'undefined') {
                    window.step4Manager = new window.Step4ContentManager(this);
                    console.log('Step4Manager initialized in collectCurrentStepData');
                }
            }
            
            if (window.step4Manager && typeof window.step4Manager.collectStep4Data === 'function') {
                try {
                    const step4Data = await window.step4Manager.collectStep4Data();
                    Object.assign(stepData, step4Data);
                    console.log('Step4 data collected successfully:', step4Data);
                } catch (error) {
                    console.error('Error collecting step 4 data:', error);
                    // Fallback: try to get content from hidden fields
                    const mainContentField = document.getElementById('contentJson');
                    const reminderField = document.getElementById('reminderContentJson');
                    const writtenField = document.getElementById('writtenContentJson');
                    
                    if (mainContentField && mainContentField.value) {
                        stepData.ContentJson = mainContentField.value;
                    } else if (reminderField && reminderField.value) {
                        stepData.ContentJson = reminderField.value;
                    } else if (writtenField && writtenField.value) {
                        stepData.ContentJson = writtenField.value;
                    } else {
                        stepData.ContentJson = '{}';
                    }
                    console.log('Using fallback content:', stepData.ContentJson);
                }
            } else {
                console.warn('Step4Manager collectStep4Data method not available');
                // Fallback: try to get content from hidden fields
                const mainContentField = document.getElementById('contentJson');
                const reminderField = document.getElementById('reminderContentJson');
                const writtenField = document.getElementById('writtenContentJson');
                
                if (mainContentField && mainContentField.value) {
                    stepData.ContentJson = mainContentField.value;
                } else if (reminderField && reminderField.value) {
                    stepData.ContentJson = reminderField.value;
                } else if (writtenField && writtenField.value) {
                    stepData.ContentJson = writtenField.value;
                } else {
                    stepData.ContentJson = '{}';
                }
                console.log('Using fallback content:', stepData.ContentJson);
            }
        }

        return stepData;
    }

    // collectStepData method removed - now using collectCurrentStepData


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
                this.showErrorMessage('خطا در بارگذاری آیتم موجود');
            }
        }
    }

    async loadExistingItem() {
        
        if (!this.currentItemId) {
            return;
        }

        try {
            const response = await fetch(`/Teacher/ScheduleItem/GetById/${this.currentItemId}`);
            const result = await response.json();
            
            if (result.success) {
                
                // Store the loaded data for step managers to use
                this.existingItemData = result.data;
                
                // Update current step from server data
                if (result.data.currentStep) {
                    this.currentStep = result.data.currentStep;
                    this.updateStepVisibility();
                }
                
                // Update indicators and form state
                this.updateStepIndicators();
                this.updateProgress();
                this.updateNavigationButtons();
                
                // Notify step managers to load their data
                setTimeout(() => {
                    this.notifyStepManagersToLoadData();
                }, 1000);

                // Update form state indicators
                this.updateFormStateIndicators();
                
            } else {
                console.error('Server response failed:', result);
            }
        } catch (error) {
            console.error('Error in loadExistingItem:', error);
        }
    }

    // Get existing item data for step managers
    getExistingItemData() {
        return this.existingItemData || null;
    }

    // Notify step managers to load their data
    notifyStepManagersToLoadData() {
        
        // Only notify the current step manager to load data
        switch (this.currentStep) {
            case 1:
                if (window.step1Manager && typeof window.step1Manager.loadStepData === 'function') {
                    setTimeout(() => {
                        window.step1Manager.loadStepData();
                    }, 100);
                }
                break;
            case 2:
                if (window.step2Manager && typeof window.step2Manager.loadStepData === 'function') {
                    setTimeout(() => {
                        window.step2Manager.loadStepData();
                    }, 100);
                }
                break;
            case 3:
                if (window.step3Manager && typeof window.step3Manager.loadStepData === 'function') {
                    setTimeout(() => {
                        window.step3Manager.loadStepData();
                    }, 100);
                }
                break;
            case 4:
                // Step 4 needs step 3 data, so load step 3 first
                if (this.step3Manager && typeof this.step3Manager.loadStepData === 'function') {
                    setTimeout(() => {
                        this.step3Manager.loadStepData();
                    }, 100);
                }
                
                if (window.step4Manager && typeof window.step4Manager.loadStepData === 'function') {
                    setTimeout(() => {
                        window.step4Manager.loadStepData();
                    }, 200); // Increased delay to ensure step 3 loads first
                }
                break;
        }
    }

    // populateFormWithExistingData method removed - each step loads its own data

    // Delegated to Step2TimingManager

   
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










    getTeachingPlanId() {
        const teachingPlanIdInput = document.querySelector('input[name="TeachingPlanId"]');
        return teachingPlanIdInput ? teachingPlanIdInput.value : null;
    }

    setupStepProgressIndicators() {
        const stepIndicators = document.querySelectorAll('.step-indicator');
        const progressFill = document.getElementById('stepProgressFill');
        
        stepIndicators.forEach((indicator, index) => {
            indicator.addEventListener('click', async () => {
                const stepNumber = parseInt(indicator.dataset.step);
                
                // If clicking on current step, do nothing
                if (stepNumber === this.currentStep) {
                    return;
                }
                
                // Only show confirmation dialog if there are changes
                if (this.shouldShowConfirmation()) {
                    const result = await this.showSaveConfirmation();
                    if (result.action === 'cancel') {
                        return;
                    }
                    
                    if (result.action === 'save') {
                        try {
                            // Save current step before moving
                            await this.saveCurrentStep();
                            // Mark step as saved
                            this.markStepAsSaved(this.currentStep);
                        } catch (error) {
                            this.showErrorMessage('خطا در ذخیره مرحله فعلی');
                            return;
                        }
                    }
                }
                
                // Navigate to selected step
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
        // Update hidden inputs for server submission using step 3 manager
        if (this.step3Manager) {
            this.step3Manager.updateHiddenInputs();
        }
    }













}

// Initialize when DOM is loaded
let formManager = null;

document.addEventListener('DOMContentLoaded', () => {
    formManager = new ModernScheduleItemFormManager();
    
    // Initialize step managers
    window.step1Manager = new Step1BasicsManager(formManager);
    window.step2Manager = new Step2TimingManager(formManager);
    // Step3Manager is initialized by the main form manager
    window.step4Manager = new Step4ContentManager(formManager);
    
    // Make formManager globally accessible
    window.formManager = formManager;
    window.scheduleItemForm = formManager;
    
    // Add debug methods to window for testing
    window.debugStepChanges = () => formManager.debugStepChanges();
    window.forceMarkChanged = () => formManager.forceMarkCurrentStepAsChanged();
    window.debugStep4Elements = () => {
        if (window.step4Manager && typeof window.step4Manager.debugStep4Elements === 'function') {
            window.step4Manager.debugStep4Elements();
        }
    };
    
    // Debug method to check current step data
    window.debugCurrentStepData = async () => {
        if (formManager) {
            console.log('=== Current Step Data Debug ===');
            console.log('Current Step:', formManager.currentStep);
            console.log('Current Item ID:', formManager.currentItemId);
            console.log('Is Edit Mode:', formManager.isEditMode);
            
            try {
                const stepData = await formManager.collectCurrentStepData();
                console.log('Collected Step Data:', stepData);
            } catch (error) {
                console.error('Error collecting step data:', error);
            }
            console.log('===============================');
        }
    };
    
    // Debug method to manually save current step
    window.debugSaveCurrentStep = async () => {
        if (formManager) {
            console.log('=== Manual Save Debug ===');
            try {
                await formManager.saveCurrentStep();
                console.log('Save completed successfully');
            } catch (error) {
                console.error('Save failed:', error);
            }
            console.log('========================');
        }
    };
    
    // Debug method to check step managers status
    window.debugStepManagers = () => {
        console.log('=== Step Managers Status ===');
        console.log('Step 1 Manager:', !!window.step1Manager);
        console.log('Step 2 Manager:', !!window.step2Manager);
        console.log('Step 3 Manager:', !!window.step3Manager);
        console.log('Step 4 Manager:', !!window.step4Manager);
        console.log('Form Manager:', !!window.formManager);
        console.log('Current Step:', window.formManager?.currentStep);
        console.log('============================');
    };
    
    // Force step 4 content sync
    window.forceStep4Sync = () => {
        if (window.step4Manager && typeof window.step4Manager.syncContentWithMainField === 'function') {
            window.step4Manager.syncContentWithMainField();
        } else {
            console.warn('Step4 manager not available for sync');
        }
    };
    
    // Wait a bit for step3Manager to be initialized by the form manager
    setTimeout(() => {
        window.step3Manager = formManager.step3Manager;
        
        // Also ensure step4Manager is properly initialized
        if (!window.step4Manager) {
            console.warn('Step4Manager not initialized, attempting to reinitialize...');
            // Try to reinitialize step4Manager
            if (typeof window.Step4ContentManager !== 'undefined') {
                window.step4Manager = new window.Step4ContentManager(formManager);
                console.log('Step4Manager reinitialized');
            }
        }
    }, 100);
});

// Cleanup on page unload to prevent memory leaks
window.addEventListener('beforeunload', () => {
    if (formManager && typeof formManager.cleanup === 'function') {
        formManager.cleanup();
    }
});
