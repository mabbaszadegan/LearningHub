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
        // Listen for insert-above events
        this.eventManager.addListener('insertBlockAbove', (e) => {
            console.log('WrittenContentBlockManager: insertBlockAbove event received', e.detail);
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    handleInsertBlockAbove(blockElement) {
        console.log('WrittenContentBlockManager: handleInsertBlockAbove called for block:', blockElement.dataset.blockId);
        
        // Store the reference to the block above which we want to insert
        this.insertAboveBlock = blockElement;
        
        // Show block type selection modal for inserting above
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId, 'written');
        }
    }

    renderBlock(block) {
        console.log('WrittenContentBlockManager: Rendering block:', block);
        
        // Determine the base template type for question blocks
        let templateType = block.type;
        if (block.type.startsWith('question')) {
            templateType = block.type.replace('question', '').toLowerCase();
        }
        
        console.log('WrittenContentBlockManager: Looking for template type:', templateType);
        
        // Look for template in questionBlockTemplates (for written content)
        let template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
        
        console.log('WrittenContentBlockManager: Template found:', !!template);
        
        if (!template) {
            console.error('WrittenContentBlockManager: Template not found for type:', templateType);
            console.log('Available templates:', document.querySelectorAll('#questionBlockTemplates .content-block-template'));
            console.log('Looking for:', `#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
            return;
        }
        
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data);
        blockElement.dataset.type = block.type; // Keep original type (e.g., questionText)
        blockElement.dataset.templateType = templateType; // Store template type (e.g., text)
        
        console.log('WrittenContentBlockManager: Created block element:', blockElement);
        
        // Configure template for question blocks
        if (block.type.startsWith('question')) {
            this.configureQuestionBlock(blockElement, block);
        }
        
        // Add direct event listeners to this specific block
        console.log('WrittenContentBlockManager: Adding direct event listeners...');
        this.addDirectEventListeners(blockElement);
        
        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }
        
        console.log('WrittenContentBlockManager: Block added to DOM');
        
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
        
        // Enhance question settings if present (modern controls)
        // Wait longer to ensure values are set and DOM is ready
        setTimeout(() => {
            const questionSettings = blockElement.querySelector('.question-settings');
            if (questionSettings && typeof window.enhanceQuestionSettings === 'function') {
                // Make sure values are set before enhancing
                if (block.type.startsWith('question')) {
                    const pointsInput = questionSettings.querySelector('[data-setting="points"]');
                    const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
                    if (pointsInput && block.data && block.data.points !== undefined) {
                        pointsInput.value = block.data.points;
                    }
                    if (difficultySelect && block.data && block.data.difficulty) {
                        difficultySelect.value = block.data.difficulty;
                    }
                }
                window.enhanceQuestionSettings(questionSettings);
            }
        }, 150);
        
        console.log('WrittenContentBlockManager: Block rendering completed for', block.id);
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

    // Override getContent to return questionBlocks instead of blocks
    getContent() {
        // Collect current data from DOM before returning
        this.collectCurrentBlockData();
        
        return {
            type: this.config.contentType,
            questionBlocks: this.blocks // Return as questionBlocks for written content
        };
    }

    // Collect current data from DOM elements
    collectCurrentBlockData() {
        this.blocks.forEach(block => {
            const blockElement = document.querySelector(`[data-block-id="${block.id}"]`);
            if (blockElement) {
                // Collect question-specific fields
                this.collectQuestionFields(blockElement, block);
            }
        });
    }

    // Collect question-specific fields from DOM
    collectQuestionFields(blockElement, block) {
        // Check if this is a question block
        const questionSettings = blockElement.querySelector('.question-settings');
        if (!questionSettings) return;

        // Collect points
        const pointsInput = questionSettings.querySelector('[data-setting="points"]');
        if (pointsInput) {
            block.data.points = parseFloat(pointsInput.value) || 1;
        }

        // Collect difficulty
        const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
        if (difficultySelect) {
            block.data.difficulty = difficultySelect.value || 'medium';
        }

        // Collect required status
        const requiredCheckbox = questionSettings.querySelector('[data-setting="isRequired"]');
        if (requiredCheckbox) {
            block.data.isRequired = requiredCheckbox.checked;
        }

        // Collect teacher guidance
        const hintTextarea = blockElement.querySelector('[data-hint="true"]');
        if (hintTextarea) {
            block.data.teacherGuidance = hintTextarea.value || '';
        }

        // Collect question text content
        this.collectQuestionTextContent(blockElement, block);
    }

    // Collect question text content from different editor types
    collectQuestionTextContent(blockElement, block) {
        // Try CKEditor first (for text blocks)
        const ckEditor = blockElement.querySelector('.ckeditor-editor');
        if (ckEditor && window.ckeditorManager) {
            const editorContent = window.ckeditorManager.getEditorContent(ckEditor);
            if (editorContent) {
                block.data.content = editorContent.html;
                block.data.textContent = editorContent.text;
            }
            return;
        }

        // Try rich text editor (for image/video/audio blocks)
        const richTextEditor = blockElement.querySelector('.rich-text-editor');
        if (richTextEditor) {
            block.data.content = richTextEditor.innerHTML;
            block.data.textContent = richTextEditor.textContent;
            return;
        }

        // Try textarea as fallback
        const textarea = blockElement.querySelector('textarea');
        if (textarea && !textarea.hasAttribute('data-hint')) {
            block.data.content = textarea.value;
            block.data.textContent = textarea.value;
        }
    }

    // Override loadExistingContent to handle questionBlocks
    loadExistingContent() {
        console.log('WrittenContentBlockManager: loadExistingContent called');
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        
        console.log('WrittenContentBlockManager: Hidden field value:', hiddenFieldValue);
        
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            console.log('WrittenContentBlockManager: No hidden field value found');
            return;
        }
        
        try {
            this.isLoadingExistingContent = true;
            
            const data = JSON.parse(hiddenFieldValue);
            
            // Handle questionBlocks for written content
            if (data.questionBlocks && Array.isArray(data.questionBlocks)) {
                console.log('WrittenContentBlockManager: Found', data.questionBlocks.length, 'question blocks');
                
                if (this.blocksList) {
                    const existingBlocks = this.blocksList.querySelectorAll('.content-block, .question-block-template');
                    existingBlocks.forEach(block => block.remove());
                }
                
                this.blocks = data.questionBlocks;
                if (this.blocks.length > 0) {
                    this.nextBlockId = Math.max(...this.blocks.map(b => parseInt(b.id.split('-')[1]) || 0)) + 1;
                } else {
                    this.nextBlockId = 1;
                }
                
                console.log('WrittenContentBlockManager: Rendering', this.blocks.length, 'blocks');
                this.blocks.forEach((block, index) => {
                    console.log('WrittenContentBlockManager: Rendering block', block.id, 'of type', block.type);
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
                console.warn('WrittenContentBlockManager: No questionBlocks found in data');
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
            console.error('WrittenContentBlockManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }
}

// Initialize when DOM is loaded
function initializeWrittenBlockManager() {
    try {
        console.log('WrittenContentBlockManager: Attempting to initialize...');
        
        if (window.writtenBlockManager) {
            console.log('WrittenContentBlockManager: Already initialized');
            return;
        }
        
        const requiredElements = [
            'contentBlocksList',
            'emptyBlocksState', 
            'writtenPreview',
            'writtenContentJson',
            'questionBlockTemplates'
        ];
        
        let missingElements = [];
        requiredElements.forEach(id => {
            const element = document.getElementById(id);
            console.log(`WrittenContentBlockManager: Checking element ${id}:`, !!element);
            if (!element) {
                missingElements.push(id);
            }
        });
        
        if (missingElements.length > 0) {
            console.warn('WrittenContentBlockManager: Missing required elements:', missingElements);
            return;
        }
        
        console.log('WrittenContentBlockManager: All required elements found, creating manager...');
        window.writtenBlockManager = new WrittenContentBlockManager();
        console.log('WrittenContentBlockManager: Successfully initialized', window.writtenBlockManager);
        
        // Force load existing content after initialization
        setTimeout(() => {
            if (window.writtenBlockManager && typeof window.writtenBlockManager.loadExistingContent === 'function') {
                console.log('WrittenContentBlockManager: Force loading existing content...');
                window.writtenBlockManager.loadExistingContent();
            }
        }, 500);
        
        // Also try to load content after a longer delay
        setTimeout(() => {
            if (window.writtenBlockManager && typeof window.writtenBlockManager.loadExistingContent === 'function') {
                console.log('WrittenContentBlockManager: Second attempt to load existing content...');
                window.writtenBlockManager.loadExistingContent();
            }
        }, 2000);
        
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

// Make initialization function available globally for manual triggering
window.initializeWrittenBlockManager = initializeWrittenBlockManager;

// Also make force load function available
window.forceLoadWrittenContent = () => {
    if (window.writtenBlockManager && typeof window.writtenBlockManager.loadExistingContent === 'function') {
        console.log('Force loading written content...');
        window.writtenBlockManager.loadExistingContent();
    } else {
        console.log('WrittenBlockManager not available, trying to initialize...');
        initializeWrittenBlockManager();
    }
};
