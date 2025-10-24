/**
 * Written Content Block Manager
 * Handles written content creation and management for written-type schedule items
 * Uses specific block managers (text-block.js, image-block.js, etc.) for individual block functionality
 */

// Global functions

class WrittenContentBlockManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'writtenPreview',
            hiddenFieldId: 'writtenContentJson',
            modalId: 'blockTypeModal',
            contentType: 'written'
        });
        
        this.init();
    }
    
    init() {
        if (!this.blocksList || !this.emptyState || !this.preview || !this.hiddenField) {
            console.error('WrittenContentBlockManager: Required elements not found!', {
                blocksList: !!this.blocksList,
                emptyState: !!this.emptyState,
                preview: !!this.preview,
                hiddenField: !!this.hiddenField
            });
            return;
        }
        
        this.setupWrittenSpecificEventListeners();
    }

    setupWrittenSpecificEventListeners() {
        // Written-specific event listeners can be added here if needed
        // Most common functionality is now handled by the base class
    }

    renderBlock(block) {
        // Determine the base template type for question blocks
        let templateType = block.type;
        if (block.type.startsWith('question')) {
            templateType = block.type.replace('question', '').toLowerCase();
        }
        
        // Look for template in questionBlockTemplates (for written content)
        let template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
        
        if (!template) {
            console.error('WrittenContentBlockManager: Template not found for type:', templateType);
            return;
        }
        
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data);
        blockElement.dataset.type = block.type; // Keep original type (e.g., questionText)
        blockElement.dataset.templateType = templateType; // Store template type (e.g., text)
        
        // Configure template for question blocks
        if (block.type.startsWith('question')) {
            this.configureQuestionBlock(blockElement, block);
        }
        
        // Add direct event listeners to this specific block
        this.addDirectEventListeners(blockElement);
        
        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }
        
        // Initialize CKEditor only for text blocks
        if (block.type === 'text' || block.type === 'questionText') {
            
            // Initialize CKEditor with a delay to ensure DOM is ready
            setTimeout(() => {
                const editorElement = blockElement.querySelector('.ckeditor-editor');
                if (editorElement && window.ckeditorManager) {
                    window.ckeditorManager.initializeEditor(editorElement);
                } else {
                    console.warn('WrittenContentBlockManager: CKEditor element or manager not found', {
                        editorElement: !!editorElement,
                        ckeditorManager: !!window.ckeditorManager
                    });
                }
            }, 100);
        }
        
        // Dispatch populate event for specific block managers
        const populateEvent = new CustomEvent('populateBlockContent', {
            detail: {
                blockElement: blockElement,
                block: block,
                blockType: block.type
            }
        });
        document.dispatchEvent(populateEvent);
        
        return blockElement;
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
function initializeWrittenBlockManager() {
    try {
        if (window.writtenBlockManager) {
            return;
        }
        
        const requiredElements = [
            'contentBlocksList',
            'emptyBlocksState', 
            'writtenPreview',
            'writtenContentJson'
        ];
        
        let missingElements = [];
        requiredElements.forEach(id => {
            if (!document.getElementById(id)) {
                missingElements.push(id);
            }
        });
        
        if (missingElements.length > 0) {
            console.warn('WrittenContentBlockManager: Missing required elements:', missingElements);
            return;
        }
        
        window.writtenBlockManager = new WrittenContentBlockManager();
        
    } catch (error) {
        console.error('Error initializing WrittenContentBlockManager:', error);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(initializeWrittenBlockManager, 100);
});

// Also try to initialize immediately if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeWrittenBlockManager, 100);
    });
} else {
    setTimeout(initializeWrittenBlockManager, 100);
}
