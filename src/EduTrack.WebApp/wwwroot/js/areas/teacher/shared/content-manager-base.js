/**
 * Content Manager Base Classes
 * Shared functionality for content management across different content types
 */

/**
 * Field Manager - Handles DOM field operations
 */
class FieldManager {
    constructor() {
        this.fields = new Map();
        this.fieldConfigs = new Map();
    }

    /**
     * Register a field with optional configuration
     * @param {string} id - Field ID
     * @param {HTMLElement} element - Field element
     * @param {Object} config - Field configuration
     */
    registerField(id, element, config = {}) {
        this.fields.set(id, element);
        this.fieldConfigs.set(id, {
            required: false,
            validate: null,
            transform: null,
            ...config
        });
    }

    /**
     * Get field element by ID
     * @param {string} id - Field ID
     * @returns {HTMLElement|null}
     */
    getField(id) {
        return this.fields.get(id) || document.getElementById(id);
    }

    /**
     * Get field value
     * @param {string} id - Field ID
     * @returns {string|null}
     */
    getFieldValue(id) {
        const field = this.getField(id);
        return field ? field.value : null;
    }

    /**
     * Set field value
     * @param {string} id - Field ID
     * @param {string} value - Value to set
     */
    setFieldValue(id, value) {
        const field = this.getField(id);
        if (field) {
            field.value = value;
        }
    }

    /**
     * Update field value with validation
     * @param {string} id - Field ID
     * @param {string} value - Value to set
     * @returns {boolean} - Success status
     */
    updateField(id, value) {
        const config = this.fieldConfigs.get(id);
        if (!config) {
            this.setFieldValue(id, value);
            return true;
        }

        // Apply transformation if exists
        let transformedValue = value;
        if (config.transform && typeof config.transform === 'function') {
            transformedValue = config.transform(value);
        }

        // Validate if validator exists
        if (config.validate && typeof config.validate === 'function') {
            const validationResult = config.validate(transformedValue);
            if (!validationResult.isValid) {
                this.showFieldError(id, validationResult.message);
                return false;
            }
        }

        this.setFieldValue(id, transformedValue);
        this.clearFieldError(id);
        return true;
    }

    /**
     * Sync values between fields
     * @param {string} sourceId - Source field ID
     * @param {string} targetId - Target field ID
     */
    syncFields(sourceId, targetId) {
        const sourceValue = this.getFieldValue(sourceId);
        if (sourceValue !== null) {
            this.updateField(targetId, sourceValue);
        }
    }

    /**
     * Show field error
     * @param {string} fieldId - Field ID
     * @param {string} message - Error message
     */
    showFieldError(fieldId, message) {
        const field = this.getField(fieldId);
        if (!field) return;

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

    /**
     * Clear field error
     * @param {string} fieldId - Field ID
     */
    clearFieldError(fieldId) {
        const field = this.getField(fieldId);
        if (!field) return;

        field.classList.remove('is-invalid');

        const existingError = field.parentNode.querySelector('.field-error');
        if (existingError) {
            existingError.remove();
        }
    }

    /**
     * Validate all registered fields
     * @returns {Object} - Validation result
     */
    validateAllFields() {
        const result = {
            isValid: true,
            errors: []
        };

        for (const [id, config] of this.fieldConfigs) {
            if (config.required) {
                const value = this.getFieldValue(id);
                if (!value || value.trim() === '') {
                    result.isValid = false;
                    result.errors.push({
                        fieldId: id,
                        message: `فیلد ${id} الزامی است`
                    });
                    this.showFieldError(id, `فیلد ${id} الزامی است`);
                }
            }
        }

        return result;
    }

    /**
     * Clear all field errors
     */
    clearAllErrors() {
        for (const [id] of this.fields) {
            this.clearFieldError(id);
        }
    }
}

/**
 * Event Manager - Handles custom events
 */
class EventManager {
    constructor() {
        this.listeners = new Map();
        this.eventHistory = [];
    }

    /**
     * Add event listener
     * @param {string} event - Event name
     * @param {Function} callback - Callback function
     * @param {Object} options - Event options
     */
    addListener(event, callback, options = {}) {
        if (!this.listeners.has(event)) {
            this.listeners.set(event, []);
        }

        const listenerInfo = {
            callback,
            options,
            id: `${event}_${Date.now()}_${Math.random()}`
        };

        this.listeners.get(event).push(listenerInfo);
        document.addEventListener(event, callback, options);

        // Track event history
        this.eventHistory.push({
            action: 'add',
            event,
            timestamp: Date.now(),
            listenerId: listenerInfo.id
        });
    }

    /**
     * Remove event listener
     * @param {string} event - Event name
     * @param {Function} callback - Callback function
     */
    removeListener(event, callback) {
        const listeners = this.listeners.get(event);
        if (!listeners) return;

        const index = listeners.findIndex(listener => listener.callback === callback);
        if (index > -1) {
            const listenerInfo = listeners[index];
            listeners.splice(index, 1);
            document.removeEventListener(event, callback);

            // Track event history
            this.eventHistory.push({
                action: 'remove',
                event,
                timestamp: Date.now(),
                listenerId: listenerInfo.id
            });
        }
    }

    /**
     * Dispatch custom event
     * @param {string} event - Event name
     * @param {Object} detail - Event detail
     */
    dispatch(event, detail = {}) {
        const customEvent = new CustomEvent(event, { detail });
        document.dispatchEvent(customEvent);

        // Track event history
        this.eventHistory.push({
            action: 'dispatch',
            event,
            timestamp: Date.now(),
            detail
        });
    }

    /**
     * Remove all listeners for an event
     * @param {string} event - Event name
     */
    removeAllListeners(event) {
        const listeners = this.listeners.get(event);
        if (!listeners) return;

        listeners.forEach(listener => {
            document.removeEventListener(event, listener.callback);
        });

        this.listeners.delete(event);
    }

    /**
     * Remove all listeners
     */
    removeAllListenersAll() {
        for (const [event, listeners] of this.listeners) {
            listeners.forEach(listener => {
                document.removeEventListener(event, listener.callback);
            });
        }
        this.listeners.clear();
    }

    /**
     * Get event history
     * @returns {Array} - Event history
     */
    getEventHistory() {
        return [...this.eventHistory];
    }
}

/**
 * Preview Manager - Handles content preview
 */
class PreviewManager {
    constructor(config = {}) {
        this.config = {
            modalId: 'previewModal',
            previewContentId: 'previewContent',
            ...config
        };
    }

    /**
     * Show preview modal
     * @param {Object} content - Content to preview
     */
    showPreview(content) {
        const modal = document.getElementById(this.config.modalId);
        const previewContent = document.getElementById(this.config.previewContentId);

        if (!modal || !previewContent) {
            console.error('Preview modal elements not found', {
                modal: !!modal,
                previewContent: !!previewContent
            });
            return;
        }

        previewContent.innerHTML = this.generatePreviewHTML(content);
        
        try {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } catch (error) {
            console.error('Error showing preview modal:', error);
            // Fallback: show modal without Bootstrap
            modal.style.display = 'block';
        }
    }

    /**
     * Generate preview HTML
     * @param {Object} content - Content data
     * @returns {string} - HTML string
     */
    generatePreviewHTML(content) {
        if (!content) {
            return '<div class="empty-content">هیچ محتوایی برای نمایش وجود ندارد.</div>';
        }

        let html = '<div class="content-preview">';

        if (content.blocks && Array.isArray(content.blocks)) {
            content.blocks.forEach(block => {
                html += this.generateBlockPreview(block);
            });
        } else if (content.questionBlocks && Array.isArray(content.questionBlocks)) {
            content.questionBlocks.forEach(question => {
                html += this.generateQuestionPreview(question);
            });
        } else {
            html += '<div class="empty-content">هیچ محتوایی اضافه نشده است.</div>';
        }

        html += '</div>';
        return html;
    }

    /**
     * Generate block preview HTML
     * @param {Object} block - Block data
     * @returns {string} - HTML string
     */
    generateBlockPreview(block) {
        let html = `<div class="preview-block preview-block-${block.type}">`;

        switch (block.type) {
            case 'text':
                html += `<div class="text-content">${block.data.content || block.data.textContent || ''}</div>`;
                break;
            case 'image':
                if (block.data.fileId || block.data.fileUrl) {
                    const imageUrl = block.data.fileUrl || `/uploads/${block.data.fileId}`;
                    html += `<div class="image-content" style="text-align: ${block.data.position || 'center'};">
                        <img src="${imageUrl}" alt="تصویر" style="max-width: ${this.getImageSize(block.data.size)};" />
                        ${block.data.caption ? `<div class="image-caption">${block.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
            case 'video':
                if (block.data.fileId || block.data.fileUrl) {
                    const videoUrl = block.data.fileUrl || `/uploads/${block.data.fileId}`;
                    html += `<div class="video-content" style="text-align: ${block.data.position || 'center'};">
                        <video controls style="max-width: ${this.getVideoSize(block.data.size)};">
                            <source src="${videoUrl}" type="${block.data.mimeType || 'video/mp4'}">
                        </video>
                        ${block.data.caption ? `<div class="video-caption">${block.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
            case 'audio':
                if (block.data.fileId || block.data.fileUrl) {
                    const audioUrl = block.data.fileUrl || `/FileUpload/GetFile/${block.data.fileId}`;
                    html += `<div class="audio-content">
                        <audio controls preload="none">
                            <source src="${audioUrl}" type="${block.data.mimeType || 'audio/mpeg'}">
                        </audio>
                        ${block.data.caption ? `<div class="audio-caption">${block.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
            case 'code':
                html += `<div class="code-content">
                    ${block.data.codeTitle ? `<div class="code-title">${block.data.codeTitle}</div>` : ''}
                    <pre><code class="language-${block.data.language || 'plaintext'}">${block.data.codeContent || ''}</code></pre>
                </div>`;
                break;
            default:
                html += `<div class="unknown-content">نوع بلاک ناشناخته: ${block.type}</div>`;
        }

        html += '</div>';
        return html;
    }

    /**
     * Generate question preview HTML
     * @param {Object} question - Question data
     * @returns {string} - HTML string
     */
    generateQuestionPreview(question) {
        let html = `<div class="preview-question">`;
        html += `<div class="question-title">${question.title || 'سوال'}</div>`;
        html += `<div class="question-content">${question.content || ''}</div>`;
        
        if (question.options && Array.isArray(question.options)) {
            html += `<div class="question-options">`;
            question.options.forEach((option, index) => {
                html += `<div class="option">${String.fromCharCode(65 + index)}. ${option}</div>`;
            });
            html += `</div>`;
        }
        
        html += `</div>`;
        return html;
    }

    /**
     * Get image size CSS
     * @param {string} size - Size name
     * @returns {string} - CSS size
     */
    getImageSize(size) {
        const sizes = {
            'small': '200px',
            'medium': '400px',
            'large': '600px',
            'full': '100%'
        };
        return sizes[size] || '400px';
    }

    /**
     * Get video size CSS
     * @param {string} size - Size name
     * @returns {string} - CSS size
     */
    getVideoSize(size) {
        const sizes = {
            'small': '400px',
            'medium': '600px',
            'large': '800px',
            'full': '100%'
        };
        return sizes[size] || '600px';
    }
}

/**
 * Content Sync Manager - Handles content synchronization
 */
class ContentSyncManager {
    constructor(fieldManager, eventManager) {
        this.fieldManager = fieldManager;
        this.eventManager = eventManager;
        this.syncCallbacks = new Map();
        this.isSyncing = false;
    }

    /**
     * Register sync callback
     * @param {string} key - Sync key
     * @param {Function} callback - Sync callback
     */
    registerSyncCallback(key, callback) {
        this.syncCallbacks.set(key, callback);
    }

    /**
     * Trigger content sync
     * @param {string} source - Source of sync
     */
    sync(source = 'manual') {
        if (this.isSyncing) {
            console.warn('Sync already in progress, skipping...');
            return;
        }

        this.isSyncing = true;
        console.log(`ContentSyncManager: Syncing content from ${source}`);

        try {
            // Execute all registered sync callbacks
            for (const [key, callback] of this.syncCallbacks) {
                if (typeof callback === 'function') {
                    callback();
                }
            }

            // Dispatch sync event
            this.eventManager.dispatch('contentSynced', { source });
        } catch (error) {
            console.error('Error during content sync:', error);
        } finally {
            this.isSyncing = false;
        }
    }

    /**
     * Setup automatic sync listeners
     */
    setupAutoSync() {
        // Listen for block events
        this.eventManager.addListener('blockContentChanged', () => this.sync('blockContentChanged'));
        this.eventManager.addListener('blockDeleted', () => this.sync('blockDeleted'));
        this.eventManager.addListener('blockAdded', () => this.sync('blockAdded'));
        this.eventManager.addListener('blockMoved', () => this.sync('blockMoved'));

        // Setup MutationObserver for DOM changes
        this.setupMutationObserver();
    }

    /**
     * Setup MutationObserver for automatic sync
     */
    setupMutationObserver() {
        const contentBlocksList = document.getElementById('contentBlocksList');
        if (!contentBlocksList) return;

        const observer = new MutationObserver((mutations) => {
            let shouldSync = false;
            mutations.forEach((mutation) => {
                if (mutation.type === 'childList') {
                    shouldSync = true;
                } else if (mutation.type === 'attributes' && 
                          (mutation.attributeName === 'data-block-id' || 
                           mutation.attributeName === 'class')) {
                    shouldSync = true;
                }
            });
            
            if (shouldSync) {
                setTimeout(() => this.sync('mutationObserver'), 100);
            }
        });
        
        observer.observe(contentBlocksList, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['data-block-id', 'class']
        });
    }
}

// Export classes for use in other files
if (typeof window !== 'undefined') {
    window.FieldManager = FieldManager;
    window.EventManager = EventManager;
    window.PreviewManager = PreviewManager;
    window.ContentSyncManager = ContentSyncManager;
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        FieldManager,
        EventManager,
        PreviewManager,
        ContentSyncManager
    };
}
