/**
 * Step 4 Content Manager
 * Handles content builder, type selection and content preview for step 4
 */

class Step4ContentManager {
    constructor(formManager) {
        this.formManager = formManager;
        this.contentBuilder = null;
        this.selectedContentType = null;
        this.isSyncingContent = false; // Flag to prevent recursive syncing

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
        // Only sync on significant events, not on every content change
        // This prevents excessive calls to collectReminderContentData
        
        // Register sync callback for content synchronization with debouncing
        this.syncManager.registerSyncCallback('syncContentWithMainField', () => {
            // Debounce the sync to prevent excessive calls
            if (this.syncTimeout) {
                clearTimeout(this.syncTimeout);
            }
            this.syncTimeout = setTimeout(() => {
                this.syncContentWithMainField();
            }, 300); // Wait 300ms before syncing
        });

        // Setup automatic sync
        this.syncManager.setupAutoSync();
    }

    validateStep4() {

        // Clear all previous errors
        this.fieldManager.clearAllErrors();

        // Validate main content field
        const validationResult = this.fieldManager.validateAllFields();
        if (!validationResult.isValid) {
            return false;
        }

        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';


        // Additional validation based on content type
        if (selectedType === '0') {
            // Reminder content validation
            const reminderData = this.collectReminderContentData();

            if (!reminderData || !reminderData.blocks || reminderData.blocks.length === 0) {
                this.fieldManager.showFieldError('contentJson', 'حداقل یک بلاک محتوا برای یادآوری الزامی است');
                return false;
            }
        } else if (selectedType === '1') {
            // Written content validation
            const writtenData = this.collectWrittenContentData();

            if (!writtenData || !writtenData.questionBlocks || writtenData.questionBlocks.length === 0) {
                this.fieldManager.showFieldError('contentJson', 'حداقل یک سوال برای تمرین نوشتاری الزامی است');
                return false;
            }
        }

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
            // Remove any existing listeners first
            addBlockBtn.removeEventListener('click', this.handleAddBlockFromHeader);

            // Add new listener
            addBlockBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.handleAddBlockFromHeader();
            });

        } else {
            console.warn('Step4ContentManager: Add block button not found');
        }

        // Preview button in step header
        const previewBtn = document.getElementById('previewReminderBtn');

        if (previewBtn) {
            previewBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.handlePreviewFromHeader();
            });

        } else {
            console.warn('Step4ContentManager: Preview button not found');
        }
    }

    handleAddBlockFromHeader() {
        if (window.sharedContentBlockManager) {
            const itemTypeSelect = document.getElementById('itemType');
            const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';
            const itemTypeName = this.getItemTypeName(selectedType);
            
            window.sharedContentBlockManager.showBlockTypeModal('blockTypeModal', itemTypeName);
        }
    }

    getItemTypeName(typeValue) {
        const typeMap = {
            '0': 'reminder',      // Reminder
            '1': 'writing',       // Writing
            '2': 'audio',         // Audio
            '3': 'gapfill',       // Gap Fill
            '4': 'multiplechoice', // Multiple Choice
            '5': 'match',         // Match
            '6': 'errorfinding',  // Error Finding
            '7': 'codeexercise',  // Code Exercise
            '8': 'quiz'           // Quiz
        };
        
        return typeMap[typeValue] || 'reminder';
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

    // Refresh add button handlers after block deletion
    refreshAddButtonHandlers() {

        // Re-setup step header buttons
        this.setupStepHeaderButtons();

        // Ensure shared content block manager is available
        if (!window.sharedContentBlockManager) {
            console.warn('Step4ContentManager: SharedContentBlockManager not available, trying to reinitialize');
            if (typeof initializeContentBlocks === 'function') {
                initializeContentBlocks();
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

    // Force save all CKEditor content before collecting data
    forceSaveAllCKEditorContent() {
        // Find all CKEditor instances and force save their content
        const ckeditorElements = document.querySelectorAll('.ckeditor-editor');
        ckeditorElements.forEach(editorElement => {
            if (window.ckeditorManager) {
                const editorContent = window.ckeditorManager.getEditorContent(editorElement);
                if (editorContent) {

                    // Find the block element
                    const blockElement = editorElement.closest('.content-block');
                    if (blockElement) {
                        const blockId = blockElement.dataset.blockId;

                        // Update block data attribute
                        if (blockElement.dataset.blockData) {
                            try {
                                const blockData = JSON.parse(blockElement.dataset.blockData);
                                blockData.content = editorContent.html;
                                blockData.textContent = editorContent.text;
                                blockElement.dataset.blockData = JSON.stringify(blockData);

                                // Update the block in the content builder
                                this.updateBlockInContentBuilder(blockElement, blockData);
                            } catch (e) {
                                console.error('Step4ContentManager: Error updating block data:', e);
                            }
                        }
                    }
                }
            }
        });

        // Also force sync from reminder block manager
        if (window.reminderBlockManager && typeof window.reminderBlockManager.forceSyncWithMainField === 'function') {
            window.reminderBlockManager.forceSyncWithMainField();
        }

        // Force sync from written block manager
        if (window.writtenBlockManager && typeof window.writtenBlockManager.forceSyncWithMainField === 'function') {
            window.writtenBlockManager.forceSyncWithMainField();
        }
    }

    // Update block in content builder
    updateBlockInContentBuilder(blockElement, blockData) {
        const blockId = blockElement.dataset.blockId;

        // Try to find the content builder that owns this block
        let contentBuilder = null;

        // Method 1: Look for reminderBlockManager
        if (window.reminderBlockManager && window.reminderBlockManager.blocks) {
            const blockIndex = window.reminderBlockManager.blocks.findIndex(b => b.id === blockId);
            if (blockIndex !== -1) {
                contentBuilder = window.reminderBlockManager;
            }
        }

        // Method 2: Look for writtenBlockManager
        if (!contentBuilder && window.writtenBlockManager && window.writtenBlockManager.blocks) {
            const blockIndex = window.writtenBlockManager.blocks.findIndex(b => b.id === blockId);
            if (blockIndex !== -1) {
                contentBuilder = window.writtenBlockManager;
            }
        }

        if (contentBuilder && contentBuilder.blocks) {
            const blockIndex = contentBuilder.blocks.findIndex(b => b.id === blockId);
            if (blockIndex !== -1) {
                contentBuilder.blocks[blockIndex].data = { ...contentBuilder.blocks[blockIndex].data, ...blockData };

                // Force update hidden field
                if (typeof contentBuilder.updateHiddenField === 'function') {
                    contentBuilder.updateHiddenField();
                }
            }
        }
    }

    // Manual method to sync content with main field
    syncContentWithMainField() {
        // Skip if already syncing to prevent infinite loops
        if (this.isSyncingContent) return;
        
        this.isSyncingContent = true;
        try {
            // Force sync from reminder block manager if available
            if (window.reminderBlockManager && typeof window.reminderBlockManager.forceSyncWithMainField === 'function') {
                window.reminderBlockManager.forceSyncWithMainField();
            }

            // Force sync from written block manager if available
            if (window.writtenBlockManager && typeof window.writtenBlockManager.forceSyncWithMainField === 'function') {
                window.writtenBlockManager.forceSyncWithMainField();
            }

            const contentData = this.collectContentData();
            if (contentData) {
                const contentJson = typeof contentData === 'string' ? contentData : JSON.stringify(contentData);
                this.fieldManager.updateField('contentJson', contentJson);
            }
        } finally {
            this.isSyncingContent = false;
        }
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
        // First upload all pending files from both reminder and written managers
        const managers = [];
        if (window.reminderBlockManager) managers.push(window.reminderBlockManager);
        if (window.writtenBlockManager) managers.push(window.writtenBlockManager);
        
        for (const manager of managers) {
            if (manager && typeof manager.uploadAllPendingFiles === 'function') {
                try {
                    await manager.uploadAllPendingFiles();
                } catch (error) {
                    console.error('Error uploading files:', error);
                    throw new Error('خطا در آپلود فایل‌ها: ' + error.message);
                }
            }
        }

        // Force sync before collecting data to ensure all changes are captured
        this.syncContentWithMainField();

        // Force save all CKEditor content before collecting data
        this.forceSaveAllCKEditorContent();

        const contentData = this.collectContentData();

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
                }
            } else {
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
                } else {
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
