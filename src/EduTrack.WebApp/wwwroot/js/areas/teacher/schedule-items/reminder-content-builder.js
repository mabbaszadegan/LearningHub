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
                console.log('ReminderContentBlockManager: Force loading existing content...');
                this.loadExistingContent();
            }
        }, 500);
    }

    setupReminderSpecificEventListeners() {
        // Listen for insert-above events
        this.eventManager.addListener('insertBlockAbove', (e) => {
            console.log('ReminderContentBlockManager: insertBlockAbove event received', e.detail);
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    handleInsertBlockAbove(blockElement) {
        console.log('ReminderContentBlockManager: handleInsertBlockAbove called for block:', blockElement.dataset.blockId);
        
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
        console.log('ReminderContentBlockManager: loadExistingContent called');
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        
        console.log('ReminderContentBlockManager: Hidden field value:', hiddenFieldValue);
        
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            console.log('ReminderContentBlockManager: No hidden field value found');
            return;
        }
        
        try {
            this.isLoadingExistingContent = true;
            
            const data = JSON.parse(hiddenFieldValue);
            
            // Handle blocks for reminder content
            if (data.blocks && Array.isArray(data.blocks)) {
                console.log('ReminderContentBlockManager: Found', data.blocks.length, 'blocks');
                
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
                
                console.log('ReminderContentBlockManager: Rendering', this.blocks.length, 'blocks');
                this.blocks.forEach((block, index) => {
                    console.log('ReminderContentBlockManager: Rendering block', block.id, 'of type', block.type);
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

// Initialize when DOM is loaded
function initializeReminderBlockManager() {
    try {
        if (window.reminderBlockManager) {
            console.log('ReminderContentBlockManager: Already initialized');
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
            console.log(`ReminderContentBlockManager: Checking element ${id}:`, !!element);
            if (!element) {
                missingElements.push(id);
            }
        });
        
        if (missingElements.length > 0) {
            console.warn('ReminderContentBlockManager: Missing required elements:', missingElements);
            return;
        }
        
        console.log('ReminderContentBlockManager: All required elements found, creating manager...');
        window.reminderBlockManager = new ReminderContentBlockManager();
        console.log('ReminderContentBlockManager: Successfully initialized', window.reminderBlockManager);
        
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
        console.log('Force loading reminder content...');
        window.reminderBlockManager.loadExistingContent();
    } else {
        console.log('ReminderBlockManager not available, trying to initialize...');
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
    console.log('=== Checking Reminder Content Setup ===');
    console.log('Required elements:');
    const requiredElements = [
        'contentBlocksList',
        'emptyBlocksState', 
        'reminderPreview',
        'reminderContentJson',
        'contentBlockTemplates'
    ];
    
    requiredElements.forEach(id => {
        const element = document.getElementById(id);
        console.log(`- ${id}:`, element ? '✓' : '✗');
    });
    
    console.log('Manager status:', {
        exists: !!window.reminderBlockManager,
        hasBlocks: window.reminderBlockManager ? window.reminderBlockManager.blocks?.length || 0 : 0
    });
    
    return {
        elements: requiredElements.map(id => ({
            id,
            exists: !!document.getElementById(id)
        })),
        manager: !!window.reminderBlockManager
    };
};
