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
        // Caption changes
        document.addEventListener('input', (e) => {
            if (e.target.matches('[data-caption="true"]')) {
                this.updateBlockCaption(e.target);
            }
        });

        // Handle insert block above event
        document.addEventListener('insertBlockAbove', (e) => {
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    handleInsertBlockAbove(blockElement) {
        // Show block type selection modal for inserting above
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal('blockTypeModal');
        }
    }

    updateBlockCaption(textarea) {
        const blockElement = textarea.closest('.content-block-template');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            block.data.caption = textarea.value;
            this.updateHiddenField();
        }
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
    
    getSizeClass(size) {
        const sizes = { 'small': 'size-small', 'medium': 'size-medium', 'large': 'size-large', 'full': 'size-full' };
        return sizes[size] || 'size-medium';
    }
    
    getPositionClass(position) {
        const positions = { 'left': 'position-left', 'center': 'position-center', 'right': 'position-right' };
        return positions[position] || 'position-center';
    }

    showPreviewModal() {
        const previewModal = new bootstrap.Modal(document.getElementById('previewModal'));
        previewModal.show();
    }

    getContent() {
        return {
            type: 'reminder',
            blocks: this.blocks
        };
    }

    // Force reload existing content (called from step4-content.js)
    loadExistingContent() {
        if (this.hiddenField) {
            const existingContent = this.hiddenField.value;
            
            if (existingContent && existingContent.trim()) {
                try {
                    this.isLoadingExistingContent = true;
                    
                    const data = JSON.parse(existingContent);
                    
                    if (data.blocks && Array.isArray(data.blocks)) {
                        // Clear existing blocks
                        const existingBlocks = this.blocksList.querySelectorAll('.content-block');
                        existingBlocks.forEach(block => block.remove());
                        
                        this.blocks = data.blocks;
                        if (this.blocks.length > 0) {
                            this.nextBlockId = Math.max(...this.blocks.map(b => parseInt(b.id.split('-')[1]) || 0)) + 1;
                        } else {
                            this.nextBlockId = 1;
                        }
                        
                        // Render blocks
                        this.blocks.forEach((block, index) => {
                            this.renderBlock(block);
                        });
                        
                        this.updateEmptyState();
                        
                        // Populate content fields after rendering
                        setTimeout(() => {
                            this.populateBlockContent();
                        }, 200);
                    }
                    
                    this.isLoadingExistingContent = false;
                    
                    setTimeout(() => {
                        this.updatePreview();
                    }, 300);
                    
                } catch (error) {
                    console.error('Error loading existing content in ReminderContentBlockManager:', error);
                    this.isLoadingExistingContent = false;
                }
            }
        }
    }

    populateBlockContent() {
        this.blocks.forEach(block => {
            const blockElement = document.querySelector(`[data-block-id="${block.id}"]`);
            if (!blockElement) return;
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            // Use block-specific populate methods
            this.populateBlockByType(blockElement, block);
        });
    }

    populateBlockByType(blockElement, block) {
        // Dispatch custom event for block-specific managers to handle
        const event = new CustomEvent('populateBlockContent', {
            detail: {
                blockElement: blockElement,
                block: block,
                blockType: block.type
            }
        });
        document.dispatchEvent(event);
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
