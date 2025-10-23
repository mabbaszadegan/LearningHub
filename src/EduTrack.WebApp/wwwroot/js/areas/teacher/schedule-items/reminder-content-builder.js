/**
 * Reminder Content Block Manager
 * Handles reminder content creation and management for reminder-type schedule items
 * Uses specific block managers (text-block.js, image-block.js, etc.) for individual block functionality
 */

// Global functions

function updatePreview() {
    if (window.reminderBlockManager) {
        window.reminderBlockManager.updatePreview();
        window.reminderBlockManager.showPreviewModal();
    } else {
        alert('سیستم پیش‌نمایش هنوز آماده نیست');
    }
}

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
            console.error('Required elements not found!');
            return;
        }
        
        this.setupReminderSpecificEventListeners();
    }

    setupReminderSpecificEventListeners() {
        // Reminder-specific event listeners can be added here if needed
        // Most common functionality is now handled by the base class
    }

    updatePreview() {
        if (this.preview) {
            let previewHTML = '<div class="reminder-card"><div class="reminder-icon"><i class="fas fa-bell"></i></div><div class="reminder-text">';
            
            if (this.blocks.length === 0) {
                previewHTML += '<p>محتوای یادآوری شما اینجا نمایش داده خواهد شد...</p>';
            } else {
                this.blocks.forEach(block => {
                    previewHTML += this.generateBlockPreview(block);
                });
            }
            
            previewHTML += '</div></div>';
            this.preview.innerHTML = previewHTML;
        }
        
        this.updateModalPreview();
    }

    updateModalPreview() {
        const modalPreview = document.getElementById('modalReminderPreview');
        if (!modalPreview) return;
        
        let previewHTML = '<div class="reminder-card"><div class="reminder-icon"><i class="fas fa-bell"></i></div><div class="reminder-text">';
        
        if (this.blocks.length === 0) {
            previewHTML += '<p>محتوای یادآوری شما اینجا نمایش داده خواهد شد...</p>';
        } else {
            this.blocks.forEach(block => {
                previewHTML += this.generateBlockPreview(block);
            });
        }
        
        previewHTML += '</div></div>';
        modalPreview.innerHTML = previewHTML;
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
        }
        
        return html;
    }
}

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
            'reminderContentJson'
        ];
        
        let missingElements = [];
        requiredElements.forEach(id => {
            if (!document.getElementById(id)) {
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
