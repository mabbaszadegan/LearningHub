/**
 * Step 4 Content Manager
 * Handles content builder, type selection and content preview for step 4
 */

class Step4ContentManager {
    constructor(formManager) {
        this.formManager = formManager;
        this.contentBuilder = null;
        this.selectedContentType = null;
        
        // Initialize shared managers
        this.fieldManager = new FieldManager();
        this.eventManager = new EventManager();
        this.previewManager = new PreviewManager();
        this.syncManager = new ContentSyncManager(this.fieldManager, this.eventManager);
        
        this.init();
    }

    init() {
        this.setupFieldManager();
        this.setupContentTypeListeners();
        this.setupContentBuilder();
        this.setupSyncManager();

        // Wait a bit for DOM to be ready
        setTimeout(async () => {
            this.updateStep4Content();
            await this.loadStepData();
        }, 500);
    }

    setupFieldManager() {
        // Register main content fields
        this.fieldManager.registerField('contentJson', document.getElementById('contentJson'), {
            required: true,
            validate: (value) => {
                if (!value || value.trim() === '' || value === '{}') {
                    return { isValid: false, message: 'محتوای آموزشی الزامی است' };
                }
                try {
                    JSON.parse(value);
                    return { isValid: true };
                } catch (e) {
                    return { isValid: false, message: 'فرمت JSON نامعتبر است' };
                }
            }
        });

        this.fieldManager.registerField('reminderContentJson', document.getElementById('reminderContentJson'));
        this.fieldManager.registerField('writtenContentJson', document.getElementById('writtenContentJson'));
        this.fieldManager.registerField('itemType', document.getElementById('itemType'));
    }

    setupSyncManager() {
        // Register sync callback for content synchronization
        this.syncManager.registerSyncCallback('syncContentWithMainField', () => {
            this.syncContentWithMainField();
        });
        
        // Setup automatic sync
        this.syncManager.setupAutoSync();
    }

    validateStep4() {
        console.log('Step4ContentManager: Validating step 4...');
        
        // Clear all previous errors
        this.fieldManager.clearAllErrors();
        
        // Validate main content field
        const validationResult = this.fieldManager.validateAllFields();
        if (!validationResult.isValid) {
            console.log('Step4ContentManager: Field validation failed:', validationResult.errors);
            return false;
        }

        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        console.log('Selected type:', selectedType);

        // Additional validation based on content type
        if (selectedType === '0') {
            // Reminder content validation
            const reminderData = this.collectReminderContentData();
            console.log('Reminder data for validation:', reminderData);
            
            if (!reminderData || !reminderData.blocks || reminderData.blocks.length === 0) {
                this.fieldManager.showFieldError('contentJson', 'حداقل یک بلاک محتوا برای یادآوری الزامی است');
                return false;
            }
        } else if (selectedType === '1') {
            // Written content validation
            const writtenData = this.collectWrittenContentData();
            console.log('Written data for validation:', writtenData);
            
            if (!writtenData || !writtenData.questionBlocks || writtenData.questionBlocks.length === 0) {
                this.fieldManager.showFieldError('contentJson', 'حداقل یک سوال برای تمرین نوشتاری الزامی است');
                return false;
            }
        }

        console.log('Step4ContentManager: Validation result: true');
        return true;
    }

    showFieldError(fieldName, message) {
        const field = document.querySelector(`[name="${fieldName}"]`);
        if (field) {
            field.classList.add('is-invalid');

            // Remove existing error message
            const existingError = field.parentNode.querySelector('.field-error');
            if (existingError) {
                existingError.remove();
            }

            // Add new error message
            const errorDiv = document.createElement('div');
            errorDiv.className = 'field-error text-danger mt-1';
            errorDiv.textContent = message;
            field.parentNode.appendChild(errorDiv);
        }
    }

    clearFieldError(fieldName) {
        const field = document.querySelector(`[name="${fieldName}"]`);
        if (field) {
            field.classList.remove('is-invalid');

            const existingError = field.parentNode.querySelector('.field-error');
            if (existingError) {
                existingError.remove();
            }
        }
    }

    setupContentTypeListeners() {
        const typeSelect = document.getElementById('itemType');
        if (typeSelect) {
            this.eventManager.addListener('change', (e) => {
                if (e.target === typeSelect) {
                    this.selectedContentType = e.target.value;
                    this.updateStep4Content();
                }
            });
        }
    }

    setupContentBuilder() {
        this.contentBuilder = null;
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

    // This method is now handled by setupSyncManager()

    setupContentBuilderEvents() {
        if (!this.contentBuilder) return;
        
        const saveBtn = document.getElementById('saveContentBtn');
        if (saveBtn) {
            this.eventManager.addListener('click', (e) => {
                if (e.target === saveBtn) {
                    this.saveContentBuilderData();
                }
            });
        }
        
        const previewBtn = document.getElementById('previewContentBtn');
        if (previewBtn) {
            this.eventManager.addListener('click', (e) => {
                if (e.target === previewBtn) {
                    this.previewContentBuilderData();
                }
            });
        }

        // Setup step header buttons
        this.setupStepHeaderButtons();
    }

    setupStepHeaderButtons() {
        // Add block button in step header
        const addBlockBtn = document.getElementById('addContentBlockBtn');
        if (addBlockBtn) {
            this.eventManager.addListener('click', (e) => {
                if (e.target === addBlockBtn) {
                    e.preventDefault();
                    e.stopPropagation();
                    this.handleAddBlockFromHeader();
                }
            });
        }

        // Preview button in step header
        const previewBtn = document.getElementById('previewReminderBtn');
        if (previewBtn) {
            this.eventManager.addListener('click', (e) => {
                if (e.target === previewBtn) {
                    e.preventDefault();
                    e.stopPropagation();
                    this.handlePreviewFromHeader();
                }
            });
        }
    }

    handleAddBlockFromHeader() {
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        if (selectedType === '0') {
            // Reminder content
            if (window.sharedContentBlockManager) {
                window.sharedContentBlockManager.showBlockTypeModal('blockTypeModal');
            } else {
                console.warn('Shared Content Block Manager not available');
                alert('سیستم مدیریت بلاک‌ها هنوز آماده نیست. لطفاً صفحه را رفرش کنید.');
            }
        } else if (selectedType === '1') {
            // Written content
            if (window.sharedContentBlockManager) {
                window.sharedContentBlockManager.showBlockTypeModal('questionTypeModal');
            } else {
                console.warn('Shared Content Block Manager not available');
                alert('سیستم مدیریت بلاک‌ها هنوز آماده نیست. لطفاً صفحه را رفرش کنید.');
            }
        }
    }

    handlePreviewFromHeader() {
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        if (selectedType === '0') {
            // Reminder content
            if (window.reminderBlockManager) {
                window.reminderBlockManager.updatePreview();
                window.reminderBlockManager.showPreviewModal();
            } else {
                console.warn('Reminder Block Manager not available');
                alert('سیستم پیش‌نمایش هنوز آماده نیست');
            }
        } else if (selectedType === '1') {
            // Written content
            if (window.writtenBlockManager) {
                window.writtenBlockManager.updatePreview();
                window.writtenBlockManager.showPreviewModal();
            } else {
                console.warn('Written Block Manager not available');
                alert('سیستم پیش‌نمایش هنوز آماده نیست');
            }
        }
    }

    updateStep4Content() {
        const contentTypeSelector = document.getElementById('contentTypeSelector');
        const contentDesigner = document.getElementById('contentDesigner');
        const contentBuilder = document.getElementById('contentBuilder');
        const reminderContentBuilder = document.getElementById('reminderContentBuilder');
        const writtenContentBuilder = document.getElementById('writtenContentBuilder');
        const contentTemplates = document.getElementById('contentTemplates');

        // Hide all content builders first
        if (contentTypeSelector) contentTypeSelector.style.display = 'none';
        if (contentDesigner) contentDesigner.style.display = 'none';
        if (contentBuilder) contentBuilder.style.display = 'none';
        if (reminderContentBuilder) reminderContentBuilder.style.display = 'none';
        if (writtenContentBuilder) writtenContentBuilder.style.display = 'none';
        if (contentTemplates) contentTemplates.style.display = 'none';

        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        if (selectedType === '0') {
            // Reminder type
            if (reminderContentBuilder) {
                reminderContentBuilder.style.display = 'block';
            }
        } else if (selectedType === '1') {
            // Writing type
            if (writtenContentBuilder) {
                writtenContentBuilder.style.display = 'block';
            }
        } else {
            // Other types
            if (contentTypeSelector) {
                contentTypeSelector.style.display = 'block';
            }
        }
    }

    saveContentBuilderData() {
        if (!this.contentBuilder) return;
        const contentData = this.contentBuilder.getContentData();
        const contentJson = JSON.stringify(contentData);
        this.fieldManager.updateField('contentJson', contentJson);
        
        if (this.formManager && typeof this.formManager.showSuccess === 'function') {
            this.formManager.showSuccess('محتوای آموزشی با موفقیت ذخیره شد.');
        }
    }

    // Collect reminder content data
    collectReminderContentData() {
        if (window.reminderBlockManager && typeof window.reminderBlockManager.getContent === 'function') {
            const content = window.reminderBlockManager.getContent();
            // Ensure we return a valid object, not null
            return content || { type: 'reminder', blocks: [] };
        }
        
        // Fallback: try to get content from hidden field
        const hiddenField = document.getElementById('reminderContentJson');
        if (hiddenField && hiddenField.value) {
            try {
                const parsed = JSON.parse(hiddenField.value);
                return parsed || { type: 'reminder', blocks: [] };
            } catch (e) {
                console.warn('Failed to parse reminder content from hidden field:', e);
            }
        }
        
        // Return empty content structure
        return { type: 'reminder', blocks: [] };
    }

    // Collect written content data
    collectWrittenContentData() {
        if (window.writtenBlockManager && typeof window.writtenBlockManager.getContent === 'function') {
            const content = window.writtenBlockManager.getContent();
            // Ensure we return a valid object, not null
            return content || { questionBlocks: [] };
        }
        
        // Fallback: try to get content from hidden field
        const hiddenField = document.getElementById('writtenContentJson');
        if (hiddenField && hiddenField.value) {
            try {
                const parsed = JSON.parse(hiddenField.value);
                return parsed || { questionBlocks: [] };
            } catch (e) {
                console.warn('Failed to parse written content from hidden field:', e);
            }
        }
        
        // Return empty content structure
        return { questionBlocks: [] };
    }

    // Manual method to sync content with main field
    syncContentWithMainField() {
        const contentData = this.collectContentData();
        if (contentData) {
            const contentJson = typeof contentData === 'string' ? contentData : JSON.stringify(contentData);
            this.fieldManager.updateField('contentJson', contentJson);
            console.log('Step4ContentManager: Synced content with main field:', contentJson);
        }
    }

    // Debug method to check step 4 elements
    debugStep4Elements() {
        console.log('=== Step 4 Debug Info ===');
        console.log('Item Type:', document.getElementById('itemType')?.value);
        console.log('Main ContentJson field:', document.getElementById('contentJson')?.value);
        console.log('Reminder ContentJson field:', document.getElementById('reminderContentJson')?.value);
        console.log('Written ContentJson field:', document.getElementById('writtenContentJson')?.value);
        console.log('Reminder Block Manager available:', !!window.reminderBlockManager);
        console.log('Written Block Manager available:', !!window.writtenBlockManager);
        
        if (window.reminderBlockManager) {
            console.log('Reminder blocks:', window.reminderBlockManager.blocks?.length || 0);
        }
        if (window.writtenBlockManager) {
            console.log('Written question blocks:', window.writtenBlockManager.questionBlocks?.length || 0);
        }
        
        const contentData = this.collectContentData();
        console.log('Collected content data:', contentData);
        console.log('========================');
    }

    previewContentBuilderData() {
        if (!this.contentBuilder) return;
        const contentData = this.contentBuilder.getContentData();
        this.previewManager.showPreview(contentData);
    }

    // This method is now handled by previewManager.showPreview()

    collectContentData() {
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        if (selectedType === '0') {
            // Reminder type - collect from reminder editor
            const reminderData = this.collectReminderContentData();
            return reminderData;
        } else if (selectedType === '1') {
            // Written content type - collect from written editor
            const writtenData = this.collectWrittenContentData();
            return writtenData;
        } else {
            // Other types - collect from content designer
            const container = document.getElementById('contentDesignContainer');
            if (!container) return "{}";
            const contentData = {};
            container.querySelectorAll('input, textarea, select').forEach(input => {
                contentData[input.name || input.id] = input.value;
            });
            return JSON.stringify(contentData);
        }
    }

    // Collect step 4 data for saving
    async collectStep4Data() {
        console.log('Step4ContentManager: Starting to collect step 4 data...');
        
        // First upload all pending files
        if (window.reminderBlockManager && typeof window.reminderBlockManager.uploadAllPendingFiles === 'function') {
            try {
                console.log('Step4ContentManager: Uploading pending files...');
                await window.reminderBlockManager.uploadAllPendingFiles();
                console.log('Step4ContentManager: Files uploaded successfully');
            } catch (error) {
                console.error('Error uploading files:', error);
                throw new Error('خطا در آپلود فایل‌ها: ' + error.message);
            }
        }

        const contentData = this.collectContentData();
        console.log('Step4ContentManager: Collected content data:', contentData);
        
        // Handle null or undefined content data
        if (!contentData) {
            console.warn('Step4ContentManager: No content data collected for step 4');
            return {
                ContentJson: '{}'
            };
        }
        
        // Ensure we always return a valid JSON string
        let contentJson;
        if (typeof contentData === 'string') {
            contentJson = contentData;
        } else {
            contentJson = JSON.stringify(contentData);
        }
        
        // Validate that the JSON is not empty or invalid
        if (!contentJson || contentJson === 'null' || contentJson === 'undefined') {
            console.warn('Step4ContentManager: Invalid content JSON, using empty object');
            contentJson = '{}';
        }
        
        console.log('Step4ContentManager: Final content JSON:', contentJson);
        
        return {
            ContentJson: contentJson
        };
    }

    // Load step 4 data from existing item
    async loadStepData() {
        // Try to load data with retry mechanism
        let retryCount = 0;
        const maxRetries = 15;

        const tryLoadData = () => {
            if (this.formManager && typeof this.formManager.getExistingItemData === 'function') {
                const existingData = this.formManager.getExistingItemData();

                if (existingData && existingData.contentJson) {
                    // Use field manager to set values
                    this.fieldManager.updateField('contentJson', existingData.contentJson);
                    this.fieldManager.updateField('reminderContentJson', existingData.contentJson);

                    // Notify reminder block manager to reload content
                    const notifyReminderManager = () => {
                        if (window.reminderBlockManager && typeof window.reminderBlockManager.loadExistingContent === 'function') {
                            // Force reload the content
                            window.reminderBlockManager.loadExistingContent();
                            return true;
                        }
                        return false;
                    };

                    // Also notify written block manager if it exists
                    const notifyWrittenManager = () => {
                        if (window.writtenBlockManager && typeof window.writtenBlockManager.loadExistingContent === 'function') {
                            window.writtenBlockManager.loadExistingContent();
                            return true;
                        }
                        return false;
                    };

                    const reminderSuccess = notifyReminderManager();
                    const writtenSuccess = notifyWrittenManager();

                    if (!reminderSuccess && !writtenSuccess) {
                        return false;
                    }
                    return true;
                } else {
                    // No existing data found
                }
            } else {
                // FormManager not ready
            }
            return false;
        };

        // Try immediately
        if (tryLoadData()) {
            return;
        }

        // Retry with intervals
        const retryInterval = setInterval(() => {
            retryCount++;

            if (tryLoadData() || retryCount >= maxRetries) {
                clearInterval(retryInterval);
                if (retryCount >= maxRetries) {
                    console.warn('Step4ContentManager: Failed to load existing content after maximum retries');
                }
            }
        }, 300);
    }

    // Cleanup method
    destroy() {
        this.eventManager.removeAllListenersAll();
        this.syncManager = null;
        this.fieldManager = null;
        this.eventManager = null;
        this.previewManager = null;
    }
}

window.Step4ContentManager = Step4ContentManager;

// Global helper functions for debugging
window.syncStep4Content = function() {
    if (window.step4Manager && typeof window.step4Manager.syncContentWithMainField === 'function') {
        window.step4Manager.syncContentWithMainField();
    } else {
        console.warn('Step4 manager not available');
    }
};

window.debugStep4Content = function() {
    if (window.step4Manager && typeof window.step4Manager.debugStep4Elements === 'function') {
        window.step4Manager.debugStep4Elements();
    } else {
        console.warn('Step4 manager not available');
    }
};

// Test validation
window.testStep4Validation = function() {
    if (window.step4Manager && typeof window.step4Manager.validateStep4 === 'function') {
        console.log('=== Testing Step 4 Validation ===');
        const isValid = window.step4Manager.validateStep4();
        console.log('Validation result:', isValid);
        console.log('===============================');
        return isValid;
    } else {
        console.warn('Step4 manager not available');
        return false;
    }
};

// Test content sync
window.testContentSync = function() {
    console.log('=== Testing Content Sync ===');
    
    // Check current state
    const mainField = document.getElementById('contentJson');
    const reminderField = document.getElementById('reminderContentJson');
    const writtenField = document.getElementById('writtenContentJson');
    
    console.log('Main content field:', mainField?.value);
    console.log('Reminder field:', reminderField?.value);
    console.log('Written field:', writtenField?.value);
    
    // Force sync
    if (window.step4Manager && typeof window.step4Manager.syncContentWithMainField === 'function') {
        window.step4Manager.syncContentWithMainField();
        console.log('Sync completed');
    } else {
        console.warn('Step4 manager sync method not available');
    }
    
    console.log('============================');
};


