/**
 * Reminder Content Block Manager
 * Handles reminder content creation and management for reminder-type schedule items
 * Uses specific block managers (text-block.js, image-block.js, etc.) for individual block functionality
 */

// Global functions


class ReminderContentBlockManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'reminderPreview',
            hiddenFieldId: 'reminderContentJson',
            modalId: 'blockTypeModal',
            contentType: 'reminder'
        });
        
        this.init();
    }
    
    init() {
        if (!this.blocksList || !this.emptyState || !this.preview || !this.hiddenField) {
            console.error('ReminderContentBlockManager: Required elements not found!');
            return;
        }
        
        this.setupReminderSpecificEventListeners();
        
        // Force load existing content after initialization
        setTimeout(() => {
            if (typeof this.loadExistingContent === 'function') {
                this.loadExistingContent();
            }
        }, 500);
    }

    setupReminderSpecificEventListeners() {
        // Listen for insert-above events
        this.eventManager.addListener('insertBlockAbove', (e) => {
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    handleInsertBlockAbove(blockElement) {
        // Store the reference to the block above which we want to insert
        this.insertAboveBlock = blockElement;
        
        // Show block type selection modal for inserting above
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId, 'reminder');
        }
    }

    generateBlockPreview(block) {
        let html = '';
        
        switch (block.type) {
            case 'text':
                html += `<div class="text-block">${block.data.content || ''}</div>`;
                break;
            case 'image':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const imageUrl = block.data.fileUrl || block.data.previewUrl;
                    const sizeClass = this.getSizeClass(block.data.size);
                    const positionClass = this.getPositionClass(block.data.position);
                    html += `<div class="image-block ${positionClass}">`;
                    if (block.data.caption && block.data.captionPosition === 'top') {
                        html += `<div class="caption caption-top">${block.data.caption}</div>`;
                    }
                    html += `<img src="${imageUrl}" alt="تصویر" class="${sizeClass}" />`;
                    if (block.data.caption && block.data.captionPosition === 'bottom') {
                        html += `<div class="caption caption-bottom">${block.data.caption}</div>`;
                    }
                    html += '</div>';
                }
                break;
            case 'video':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const videoUrl = block.data.fileUrl || block.data.previewUrl;
                    const sizeClass = this.getSizeClass(block.data.size);
                    const positionClass = this.getPositionClass(block.data.position);
                    html += `<div class="video-block ${positionClass}">`;
                    if (block.data.caption && block.data.captionPosition === 'top') {
                        html += `<div class="caption caption-top">${block.data.caption}</div>`;
                    }
                    html += `<video controls preload="none" class="${sizeClass}"><source data-src="${videoUrl}" type="video/mp4"></video>`;
                    if (block.data.caption && block.data.captionPosition === 'bottom') {
                        html += `<div class="caption caption-bottom">${block.data.caption}</div>`;
                    }
                    html += '</div>';
                }
                break;
            case 'audio':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const audioUrl = block.data.fileUrl || block.data.previewUrl;
                    const mimeType = block.data.mimeType || 'audio/mpeg';
                    html += `<div class="audio-block">`;
                    if (block.data.caption) {
                        html += `<div class="caption">${block.data.caption}</div>`;
                    }
                    html += `<audio controls preload="none"><source data-src="${audioUrl}" type="${mimeType}"></audio>`;
                    html += '</div>';
                }
                break;
            case 'code':
                if (block.data.codeContent) {
                    html += `<div class="code-block-preview">`;
                    if (block.data.codeTitle) {
                        html += `<div class="code-title">${block.data.codeTitle}</div>`;
                    }
                    html += `<pre><code class="language-${block.data.language || 'plaintext'}">${block.data.codeContent}</code></pre>`;
                    html += '</div>';
                }
                break;
            // Question block types - reuse existing block previews with question styling
            case 'questionText':
                html += `<div class="question-block question-text">`;
                html += `<div class="question-header">`;
                html += `<span class="question-type">سوال متنی</span>`;
                html += `<span class="question-points">${block.data.points || 1} نمره</span>`;
                html += `</div>`;
                html += `<div class="question-content">${block.data.content || ''}</div>`;
                if (block.data.teacherGuidance) {
                    html += `<div class="teacher-guidance">راهنمایی معلم: ${block.data.teacherGuidance}</div>`;
                }
                html += `<div class="answer-field">پاسخ دانش آموز: [فیلد متنی]</div>`;
                html += '</div>';
                break;
            case 'questionImage':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const imageUrl = block.data.fileUrl || block.data.previewUrl;
                    const sizeClass = this.getSizeClass(block.data.size);
                    html += `<div class="question-block question-image">`;
                    html += `<div class="question-header">`;
                    html += `<span class="question-type">سوال تصویری</span>`;
                    html += `<span class="question-points">${block.data.points || 1} نمره</span>`;
                    html += `</div>`;
                    html += `<div class="question-content">`;
                    html += `<img src="${imageUrl}" alt="تصویر سوال" class="${sizeClass}" />`;
                    if (block.data.caption) {
                        html += `<div class="caption">${block.data.caption}</div>`;
                    }
                    html += `</div>`;
                    if (block.data.teacherGuidance) {
                        html += `<div class="teacher-guidance">راهنمایی معلم: ${block.data.teacherGuidance}</div>`;
                    }
                    html += `<div class="answer-field">پاسخ دانش آموز: [فیلد متنی]</div>`;
                    html += '</div>';
                }
                break;
            case 'questionVideo':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const videoUrl = block.data.fileUrl || block.data.previewUrl;
                    const sizeClass = this.getSizeClass(block.data.size);
                    html += `<div class="question-block question-video">`;
                    html += `<div class="question-header">`;
                    html += `<span class="question-type">سوال ویدیویی</span>`;
                    html += `<span class="question-points">${block.data.points || 1} نمره</span>`;
                    html += `</div>`;
                    html += `<div class="question-content">`;
                    html += `<video controls preload="none" class="${sizeClass}"><source data-src="${videoUrl}" type="video/mp4"></video>`;
                    if (block.data.caption) {
                        html += `<div class="caption">${block.data.caption}</div>`;
                    }
                    html += `</div>`;
                    if (block.data.teacherGuidance) {
                        html += `<div class="teacher-guidance">راهنمایی معلم: ${block.data.teacherGuidance}</div>`;
                    }
                    html += `<div class="answer-field">پاسخ دانش آموز: [فیلد متنی]</div>`;
                    html += '</div>';
                }
                break;
            case 'questionAudio':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const audioUrl = block.data.fileUrl || block.data.previewUrl;
                    const mimeType = block.data.mimeType || 'audio/mpeg';
                    html += `<div class="question-block question-audio">`;
                    html += `<div class="question-header">`;
                    html += `<span class="question-type">سوال صوتی</span>`;
                    html += `<span class="question-points">${block.data.points || 1} نمره</span>`;
                    html += `</div>`;
                    html += `<div class="question-content">`;
                    html += `<audio controls preload="none"><source data-src="${audioUrl}" type="${mimeType}"></audio>`;
                    if (block.data.caption) {
                        html += `<div class="caption">${block.data.caption}</div>`;
                    }
                    html += `</div>`;
                    if (block.data.teacherGuidance) {
                        html += `<div class="teacher-guidance">راهنمایی معلم: ${block.data.teacherGuidance}</div>`;
                    }
                    html += `<div class="answer-field">پاسخ دانش آموز: [فیلد متنی]</div>`;
                    html += '</div>';
                }
                break;
        }
        
        return html;
    }
    
    // Override loadExistingContent to add debugging
    loadExistingContent() {
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        
        
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            return;
        }
        
        try {
            this.isLoadingExistingContent = true;
            
            const data = JSON.parse(hiddenFieldValue);
            
            // Handle blocks for reminder content
            if (data.blocks && Array.isArray(data.blocks)) {
                
                if (this.blocksList) {
                    const existingBlocks = this.blocksList.querySelectorAll('.content-block, .question-block-template');
                    existingBlocks.forEach(block => block.remove());
                }
                
                this.blocks = data.blocks;
                if (this.blocks.length > 0) {
                    this.nextBlockId = Math.max(...this.blocks.map(b => parseInt(b.id.split('-')[1]) || 0)) + 1;
                } else {
                    this.nextBlockId = 1;
                }
                
                this.blocks.forEach((block, index) => {
                    this.renderBlock(block);
                });
                
                this.updateEmptyState();
                
                // Populate content fields after rendering with longer delay
                setTimeout(() => {
                    this.populateBlockContent();
                }, 500);
                
                // Notify sidebar manager to refresh
                setTimeout(() => {
                    if (window.contentSidebarManager) {
                        window.contentSidebarManager.forceRefresh();
                    }
                }, 1000);
            } else {
                console.warn('ReminderContentBlockManager: No blocks found in data');
            }
            
            this.isLoadingExistingContent = false;
            
            setTimeout(() => {
                this.updatePreview();
                
                // Dispatch content loaded event for sidebar
                document.dispatchEvent(new CustomEvent('contentLoaded', {
                    detail: { contentType: this.config.contentType }
                }));
            }, 800);
            
        } catch (error) {
            console.error('ReminderContentBlockManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }
}

// Export to window for global access
window.ReminderContentBlockManager = ReminderContentBlockManager;

// Initialize when DOM is loaded
function initializeReminderBlockManager() {
    try {
        if (window.reminderBlockManager) {
            return;
        }
        
        const requiredElements = [
            'contentBlocksList',
            'emptyBlocksState', 
            'reminderPreview',
            'reminderContentJson',
            'contentBlockTemplates'
        ];
        
        let missingElements = [];
        requiredElements.forEach(id => {
            const element = document.getElementById(id);
            if (!element) {
                missingElements.push(id);
            }
        });
        
        if (missingElements.length > 0) {
            console.warn('ReminderContentBlockManager: Missing required elements:', missingElements);
            return;
        }
        
        window.reminderBlockManager = new ReminderContentBlockManager();
        
    } catch (error) {
        console.error('Error initializing ReminderContentBlockManager:', error);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(initializeReminderBlockManager, 100);
});

// Also try to initialize immediately if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeReminderBlockManager, 100);
    });
} else {
    setTimeout(initializeReminderBlockManager, 100);
}

// Make initialization function available globally for manual triggering
window.initializeReminderBlockManager = initializeReminderBlockManager;

// Also make force load function available
window.forceLoadReminderContent = () => {
    if (window.reminderBlockManager && typeof window.reminderBlockManager.loadExistingContent === 'function') {
        window.reminderBlockManager.loadExistingContent();
    } else {
        initializeReminderBlockManager();
        
        // Try to load content after initialization
        setTimeout(() => {
            if (window.reminderBlockManager && typeof window.reminderBlockManager.loadExistingContent === 'function') {
                window.reminderBlockManager.loadExistingContent();
            }
        }, 500);
    }
};

// Check initialization status
window.checkReminderContentSetup = () => {
    const requiredElements = [
        'contentBlocksList',
        'emptyBlocksState', 
        'reminderPreview',
        'reminderContentJson',
        'contentBlockTemplates'
    ];
    
    requiredElements.forEach(id => {
        const element = document.getElementById(id);
    });
    
    
    return {
        elements: requiredElements.map(id => ({
            id,
            exists: !!document.getElementById(id)
        })),
        manager: !!window.reminderBlockManager
    };
};
