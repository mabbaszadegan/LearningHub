/**
 * Step 4 Content Manager
 * Handles content builder, type selection and content preview for step 4
 */

class Step4ContentManager {
    constructor(formManager) {
        this.formManager = formManager;
        this.contentBuilder = null;
        this.selectedContentType = null;
        this.init();
    }

    init() {
        this.setupContentTypeListeners();
        this.setupContentBuilder();

        // Wait a bit for DOM to be ready
        setTimeout(async () => {
            this.updateStep4Content();
            await this.loadStepData();
        }, 500);
    }

    validateStep4() {
        let isValid = true;

        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        // Validate Content JSON based on type
        const contentJsonInput = document.querySelector('input[name="ContentJson"]');
        if (!contentJsonInput || !contentJsonInput.value || contentJsonInput.value === '{}') {
            this.showFieldError('ContentJson', 'محتوای آموزشی الزامی است');
            isValid = false;
        } else {
            this.clearFieldError('ContentJson');

            // Additional validation based on content type
            if (selectedType === '0') {
                // Reminder content validation
                const reminderData = this.collectReminderContentData();
                if (!reminderData || !reminderData.blocks || reminderData.blocks.length === 0) {
                    this.showFieldError('ContentJson', 'حداقل یک بلاک محتوا برای یادآوری الزامی است');
                    isValid = false;
                }
            } else if (selectedType === '1') {
                // Written content validation
                const writtenData = this.collectWrittenContentData();
                if (!writtenData || !writtenData.questionBlocks || writtenData.questionBlocks.length === 0) {
                    this.showFieldError('ContentJson', 'حداقل یک سوال برای تمرین نوشتاری الزامی است');
                    isValid = false;
                }
            }
        }

        return isValid;
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
            typeSelect.addEventListener('change', (e) => {
                this.selectedContentType = e.target.value;
                this.updateStep4Content();
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

    setupContentBuilderEvents() {
        if (!this.contentBuilder) return;
        const saveBtn = document.getElementById('saveContentBtn');
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                this.saveContentBuilderData();
            });
        }
        const previewBtn = document.getElementById('previewContentBtn');
        if (previewBtn) {
            previewBtn.addEventListener('click', () => {
                this.previewContentBuilderData();
            });
        }

        // Setup step header buttons
        this.setupStepHeaderButtons();
    }

    setupStepHeaderButtons() {
        // Add block button in step header
        const addBlockBtn = document.getElementById('addContentBlockBtn');
        if (addBlockBtn) {
            addBlockBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.handleAddBlockFromHeader();
            });
        }

        // Preview button in step header
        const previewBtn = document.getElementById('previewReminderBtn');
        if (previewBtn) {
            previewBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.handlePreviewFromHeader();
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
        const contentField = document.getElementById('contentJson');
        if (contentField) {
            contentField.value = contentJson;
        }
        if (this.formManager && typeof this.formManager.showSuccess === 'function') {
            this.formManager.showSuccess('محتوای آموزشی با موفقیت ذخیره شد.');
        }
    }

    // Collect reminder content data
    collectReminderContentData() {
        if (window.reminderBlockManager && typeof window.reminderBlockManager.getContent === 'function') {
            return window.reminderBlockManager.getContent();
        }
        return null;
    }

    // Collect written content data
    collectWrittenContentData() {
        if (window.writtenBlockManager && typeof window.writtenBlockManager.getContent === 'function') {
            return window.writtenBlockManager.getContent();
        }
        return null;
    }

    // Debug method to check step 4 elements
    debugStep4Elements() {
        // Debug method - elements can be checked in browser dev tools
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
                        <audio controls preload="none">
                            <source data-src="/FileUpload/GetFile/${box.data.fileId}" type="${box.data.mimeType || 'audio/mpeg'}">
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

    collectContentData() {
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        if (selectedType === '0') {
            // Reminder type - collect from reminder editor
            const reminderData = this.collectReminderContentData();
            return reminderData;
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
        // First upload all pending files
        if (window.reminderBlockManager && typeof window.reminderBlockManager.uploadAllPendingFiles === 'function') {
            try {
                await window.reminderBlockManager.uploadAllPendingFiles();
            } catch (error) {
                console.error('Error uploading files:', error);
                throw new Error('خطا در آپلود فایل‌ها: ' + error.message);
            }
        }

        const contentData = this.collectContentData();
        return {
            ContentJson: typeof contentData === 'string' ? contentData : JSON.stringify(contentData)
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
                    const contentField = document.getElementById('contentJson');
                    const reminderField = document.getElementById('reminderContentJson');

                    if (contentField) {
                        contentField.value = existingData.contentJson;
                    }

                    if (reminderField) {
                        reminderField.value = existingData.contentJson;
                    }

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
}

window.Step4ContentManager = Step4ContentManager;


