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

    async init() {
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
            
            // If we're already on step 3, initialize step 3 content
            if (this.currentStep === 3) {
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

        //if (submitBtn) {
        //    submitBtn.addEventListener('click', (e) => this.handleFormSubmit(e));
        //}

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

    updateAssignmentPreview() {
        if (!this.step3Manager) return;
        
        const targetGroupsElement = document.getElementById('targetGroups');
        const relatedSubChaptersElement = document.getElementById('relatedSubChapters');
        
        const selectedGroups = this.step3Manager.getSelectedGroups();
        const selectedSubChapters = this.step3Manager.getSelectedSubChapters();

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
        console.log('Success:', message);
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
            console.warn(`Invalid step: ${step}. Must be between 1 and ${this.totalSteps}`);
            return;
        }

        // Prevent rapid step changes
        if (this.isNavigating) {
            console.warn('Step navigation already in progress');
            return;
        }

        this.isNavigating = true;

        // Validate current step before moving forward
        if (step > this.currentStep && !this.validateCurrentStep()) {
            console.warn('Current step validation failed');
            this.isNavigating = false;
            return;
        }

        const previousStep = this.currentStep;
        this.currentStep = step;
        
        console.log(`Navigating from step ${previousStep} to step ${step}`);
        
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
        console.log(`Initializing step ${step} content and loading data`);
        console.log('Existing item data available:', !!this.existingItemData);
        
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
                console.error('Step3AssignmentManager is not available. Make sure step3-assignment.js is loaded before schedule-item-form.js');
                return;
            }
            console.log('Initializing Step3AssignmentManager...');
            this.step3Manager = new window.Step3AssignmentManager(this);
            console.log('Step3AssignmentManager initialized:', this.step3Manager);
        }
    }

    async initializeStep3() {
        // Prevent double initialization
        if (this.step3Initialized) {
            console.log('Step 3 already initialized, skipping...');
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
            console.log('Step 3 initialization completed');
        }
    }


    // Cleanup method for memory management
    cleanup() {
        console.log('Cleaning up event listeners...');
        
        // Cleanup step 3 manager
        if (this.step3Manager && typeof this.step3Manager.cleanup === 'function') {
            this.step3Manager.cleanup();
        }
        
        console.log('Cleanup completed');
    }

    async goToNextStep() {
        if (this.currentStep < this.totalSteps) {
            const nextBtn = document.getElementById('nextStepBtn');
            
            try {
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
                        console.log('About to save current step before moving to next step');
                        await this.saveCurrentStep();
                        console.log('Save current step completed');
                        
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
                console.error('Error saving step:', error);
                
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
                console.error('Error navigating to previous step:', error);
                
                // Re-enable button on error
                this.setButtonLoadingState(prevBtn, false, 'مرحله قبلی');
                
                this.showErrorMessage('خطا در انتقال به مرحله قبلی');
            }
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
                nextBtn.innerHTML = `
                    <span>ثبت ${currentStepName} و ادامه</span>
                    <i class="fas fa-chevron-left"></i>
                `;
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
        console.log(`Step ${step} marked as changed`);
    }

    // Mark step as saved (no changes)
    markStepAsSaved(step) {
        this.stepChanges[step] = false;
        // Update original data after save
        this.originalStepData[step] = this.collectCurrentStepData();
        console.log(`Step ${step} marked as saved`);
    }

    // Clear step cache to force reload from server
    clearStepCache(step) {
        // Clear any cached data for the step
        if (this.originalStepData[step]) {
            this.originalStepData[step] = null;
        }
        // Reset change tracking
        this.stepChanges[step] = false;
        console.log(`Step ${step} cache cleared`);
    }

    // Setup change detection for form inputs
    setupChangeDetection() {
        // Use event delegation for better performance and coverage
        document.addEventListener('input', (e) => {
            const target = e.target;
            if (target.matches('input, select, textarea')) {
                console.log('Input change detected:', target.name || target.id);
                this.markStepAsChanged(this.currentStep);
            }
        }, true);

        // Listen for changes on checkboxes and radio buttons
        document.addEventListener('change', (e) => {
            const target = e.target;
            if (target.matches('input[type="checkbox"], input[type="radio"], select')) {
                console.log('Change detected:', target.name || target.id);
                this.markStepAsChanged(this.currentStep);
            }
        }, true);

        // Listen for changes on rich text editors (contenteditable)
        document.addEventListener('input', (e) => {
            const target = e.target;
            if (target.matches('[contenteditable="true"]')) {
                console.log('Rich text editor change detected:', target.id);
                this.markStepAsChanged(this.currentStep);
            }
        }, true);

        // Listen for changes on content builder (Step 4)
        document.addEventListener('contentChanged', (e) => {
            console.log('Content changed event detected');
            if (this.currentStep === 4) {
                this.markStepAsChanged(4);
            }
        });

        // Listen for changes on assignment selection (Step 3)
        document.addEventListener('assignmentChanged', (e) => {
            console.log('Assignment changed event detected');
            if (this.currentStep === 3) {
                this.markStepAsChanged(3);
            }
        });

        // Also listen for click events on buttons that might change data
        document.addEventListener('click', (e) => {
            const target = e.target;
            if (target.matches('.date-preset-btn, .score-preset, .subchapter-badge, .student-badge, .group-badge, .toolbar-btn')) {
                console.log('Button click detected:', target.className);
                this.markStepAsChanged(this.currentStep);
            }
        }, true);
    }

    // Check if we should show confirmation dialog
    shouldShowConfirmation() {
        const hasChanges = this.hasStepChanges(this.currentStep);
        console.log(`Should show confirmation for step ${this.currentStep}:`, hasChanges);
        console.log('Step changes status:', this.stepChanges);
        return hasChanges;
    }

    // Force mark current step as changed (for testing)
    forceMarkCurrentStepAsChanged() {
        this.markStepAsChanged(this.currentStep);
        console.log(`Forced step ${this.currentStep} as changed`);
    }

    // Debug method to check current state
    debugStepChanges() {
        console.log('=== Step Changes Debug ===');
        console.log('Current step:', this.currentStep);
        console.log('Step changes:', this.stepChanges);
        console.log('Should show confirmation:', this.shouldShowConfirmation());
        console.log('========================');
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
        if (this.step3Manager) {
            const selectedGroups = this.step3Manager.getSelectedGroups();
            const selectedSubChapters = this.step3Manager.getSelectedSubChapters();
            
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
            console.error('Error submitting form:', error);
            
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
            console.log('About to save final step before completion');
            await this.saveCurrentStep();
            console.log('Final step save completed');

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
        console.log('saveCurrentStep called for step:', this.currentStep);
        
        // Prevent multiple simultaneous save operations
        if (this.isSaving) {
            console.warn('Save operation already in progress, isSaving:', this.isSaving);
            return;
        }

        console.log('Setting isSaving to true');
        this.isSaving = true;
        
        try {
            // Collect step data from the appropriate step manager
            const stepData = await this.collectCurrentStepData();
            console.log('Collected step data:', stepData);

            const requestData = {
                Id: this.currentItemId,
                TeachingPlanId: parseInt(document.querySelector('[name="TeachingPlanId"]').value),
                Step: this.currentStep,
                ...stepData
            };

            console.log('Saving step data:', requestData);

            // For step 3, ensure we have proper data structure
            if (this.currentStep === 3) {
                if (!requestData.GroupIds) requestData.GroupIds = [];
                if (!requestData.SubChapterIds) requestData.SubChapterIds = [];
                if (!requestData.StudentIds) requestData.StudentIds = [];
                
                console.log('Step 3 data prepared:', {
                    groups: requestData.GroupIds,
                    subChapters: requestData.SubChapterIds,
                    students: requestData.StudentIds
                });
            }

            console.log('Making fetch request to SaveStep...');
            const response = await fetch('/Teacher/ScheduleItem/SaveStep', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });
            console.log('Fetch response received:', response.status, response.statusText);

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
            console.log('Resetting isSaving to false due to error');
            this.isSaving = false; // Reset flag on error
            throw error;
        } finally {
            console.log('Finally block: Resetting isSaving to false');
            this.isSaving = false; // Ensure flag is always reset
        }
    }

    // Method to manually reset isSaving flag (for debugging)
    resetSavingFlag() {
        console.log('Manually resetting isSaving flag');
        this.isSaving = false;
    }

    async collectCurrentStepData() {
        console.log(`Collecting data for current step: ${this.currentStep}`);
        console.log('Available step managers:', {
            step1: !!window.step1Manager,
            step2: !!window.step2Manager,
            step3: !!window.step3Manager,
            step4: !!window.step4Manager
        });
        
        switch (this.currentStep) {
            case 1:
                if (window.step1Manager && typeof window.step1Manager.collectStep1Data === 'function') {
                    return window.step1Manager.collectStep1Data();
                }
                break;
                
            case 2:
                if (window.step2Manager && typeof window.step2Manager.collectStep2Data === 'function') {
                    return window.step2Manager.collectStep2Data();
                }
                break;
                
            case 3:
                // Make sure step3Manager is available
                if (this.step3Manager && !window.step3Manager) {
                    window.step3Manager = this.step3Manager;
                }
                if (window.step3Manager && typeof window.step3Manager.collectStep3Data === 'function') {
                    return window.step3Manager.collectStep3Data();
                }
                break;
                
            case 4:
                if (window.step4Manager && typeof window.step4Manager.collectStep4Data === 'function') {
                    return window.step4Manager.collectStep4Data();
                }
                break;
        }

        return {};
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
                // Store the loaded data for step managers to use
                this.existingItemData = result.data;
                
                // Update current step if available
                if (result.data.currentStep) {
                    this.currentStep = result.data.currentStep;
                    this.updateStepVisibility();
                    this.updateStepIndicators();
                    this.updateProgress();
                    this.updateNavigationButtons();
                    
                    // Initialize step content for the current step
                    setTimeout(() => {
                        this.initializeStepContent(this.currentStep);
                    }, 300);
                }

                // Update form state indicators
                this.updateFormStateIndicators();
                
                console.log('Existing item data loaded and stored for step managers');
            }
        } catch (error) {
            console.error('Error loading existing item:', error);
        }
    }

    // Get existing item data for step managers
    getExistingItemData() {
        return this.existingItemData || null;
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
                            console.error('Error saving step:', error);
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
    
    // Wait a bit for step3Manager to be initialized by the form manager
    setTimeout(() => {
        window.step3Manager = formManager.step3Manager;
    }, 100);
});

// Cleanup on page unload to prevent memory leaks
window.addEventListener('beforeunload', () => {
    if (formManager && typeof formManager.cleanup === 'function') {
        formManager.cleanup();
    }
});