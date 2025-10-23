/**
 * Content Builder Base Class
 * Manages common functionality for content blocks across different content types
 * Each block type has its own specific manager (text-block.js, image-block.js, etc.)
 */

class ContentBuilderBase {
    constructor(config = {}) {
        this.config = {
            containerId: config.containerId || 'contentBlocksList',
            emptyStateId: config.emptyStateId || 'emptyBlocksState',
            previewId: config.previewId || 'contentPreview',
            hiddenFieldId: config.hiddenFieldId || 'contentJson',
            modalId: config.modalId || 'blockTypeModal',
            contentType: config.contentType || 'content',
            ...config
        };

        this.blocks = [];
        this.nextBlockId = 1;
        this.isLoadingExistingContent = false;
        
        // Initialize shared managers
        this.fieldManager = new FieldManager();
        this.eventManager = new EventManager();
        this.previewManager = new PreviewManager();
        this.syncManager = new ContentSyncManager(this.fieldManager, this.eventManager);
        
        // DOM elements
        this.blocksList = document.getElementById(this.config.containerId);
        this.emptyState = document.getElementById(this.config.emptyStateId);
        this.preview = document.getElementById(this.config.previewId);
        this.hiddenField = document.getElementById(this.config.hiddenFieldId);
        
        this.init();
    }

    init() {
        if (!this.blocksList || !this.emptyState || !this.preview || !this.hiddenField) {
            console.error('Required elements not found!', {
                blocksList: !!this.blocksList,
                emptyState: !!this.emptyState,
                preview: !!this.preview,
                hiddenField: !!this.hiddenField
            });
            return;
        }
        
        this.setupFieldManager();
        this.setupEventListeners();
        this.setupSyncManager();
        this.loadExistingContent();
    }

    setupFieldManager() {
        // Register fields with validation
        this.fieldManager.registerField(this.config.hiddenFieldId, this.hiddenField, {
            required: false,
            validate: (value) => {
                try {
                    JSON.parse(value);
                    return { isValid: true };
                } catch (e) {
                    return { isValid: false, message: 'فرمت JSON نامعتبر است' };
                }
            }
        });
        
        this.fieldManager.registerField('contentJson', document.getElementById('contentJson'), {
            required: false,
            validate: (value) => {
                try {
                    JSON.parse(value);
                    return { isValid: true };
                } catch (e) {
                    return { isValid: false, message: 'فرمت JSON نامعتبر است' };
                }
            }
        });
    }

    setupSyncManager() {
        // Register sync callback for updating hidden fields
        this.syncManager.registerSyncCallback('updateHiddenFields', () => {
            this.updateHiddenField();
        });
        
        // Setup automatic sync
        this.syncManager.setupAutoSync();
    }

    setupEventListeners() {
        // Block actions (move up, move down, delete)
        this.eventManager.addListener('click', (e) => {
            console.log('ContentBuilderBase: Click event detected on:', e.target);
            console.log('ContentBuilderBase: Event target matches delete:', e.target.matches('[data-action="delete"]'));
            
            if (e.target.matches('[data-action="move-up"]')) {
                console.log('ContentBuilderBase: Move up action detected');
                this.moveBlockUp(e.target.closest('.content-block-template, .content-block'));
            } else if (e.target.matches('[data-action="move-down"]')) {
                console.log('ContentBuilderBase: Move down action detected');
                this.moveBlockDown(e.target.closest('.content-block-template, .content-block'));
            } else if (e.target.matches('[data-action="delete"]')) {
                console.log('ContentBuilderBase: Delete action detected');
                const blockElement = e.target.closest('.content-block-template, .content-block');
                console.log('ContentBuilderBase: Delete button clicked, blockElement:', blockElement);
                this.deleteBlock(blockElement);
            } else if (e.target.matches('[data-action="toggle-collapse"]')) {
                console.log('ContentBuilderBase: Toggle collapse action detected');
                this.toggleCollapse(e.target.closest('.content-block-template, .content-block'));
            } else if (e.target.matches('[data-action="fullscreen"]')) {
                console.log('ContentBuilderBase: Fullscreen action detected');
                this.toggleFullscreen(e.target.closest('.content-block-template, .content-block'));
            } else if (e.target.matches('[data-action="insert-above"]')) {
                console.log('ContentBuilderBase: Insert above action detected');
                this.insertBlockAbove(e.target.closest('.content-block-template, .content-block'));
            }
        });

        // Listen for block content changes from specific block managers
        this.eventManager.addListener('blockContentChanged', (e) => {
            this.handleBlockContentChanged(e.detail);
        });
        
        // Also listen on document for blockContentChanged events
        document.addEventListener('blockContentChanged', (e) => {
            console.log('ContentBuilderBase: Document blockContentChanged event received:', e.detail);
            this.handleBlockContentChanged(e.detail);
        });
        
        // Direct input listener for rich text editors
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('rich-text-editor')) {
                console.log('ContentBuilderBase: Rich text editor input detected');
                this.handleRichTextInput(e.target);
            }
        });

        // Settings changes
        this.eventManager.addListener('change', (e) => {
            if (e.target.matches('[data-setting]')) {
                this.updateBlockSettings(e.target);
            }
        });

        // Caption changes
        this.eventManager.addListener('input', (e) => {
            if (e.target.matches('[data-caption="true"]')) {
                this.updateBlockCaption(e.target);
            }
        });

        // Handle insert block above event
        this.eventManager.addListener('insertBlockAbove', (e) => {
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    addBlock(type) {
        const blockId = `block-${this.nextBlockId++}`;
        const block = {
            id: blockId,
            type: type,
            order: this.blocks.length,
            data: this.getDefaultBlockData(type)
        };
        
        this.blocks.push(block);
        this.renderBlock(block);
        this.updateEmptyState();
        this.updateHiddenField();
        this.scrollToNewBlock(blockId);
        
        // Dispatch custom event
        this.eventManager.dispatch('blockAdded', {
            blockId: blockId, 
            blockType: type, 
            contentType: this.config.contentType
        });
    }

    getDefaultBlockData(type) {
        const baseData = {
            size: 'medium',
            position: 'center',
            caption: '',
            captionPosition: 'bottom'
        };

        switch (type) {
            case 'text':
                return {
                    content: '',
                    textContent: ''
                };
            case 'image':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null
                };
            case 'video':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null
                };
            case 'audio':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null,
                    isRecorded: false,
                    duration: null
                };
            case 'code':
                return {
                    ...baseData,
                    codeContent: '',
                    language: 'plaintext',
                    theme: 'default',
                    codeTitle: '',
                    showLineNumbers: true,
                    enableCopyButton: true
                };
            default:
                return baseData;
        }
    }

    renderBlock(block) {
        const template = document.querySelector(`#contentBlockTemplates .content-block-template[data-type="${block.type}"]`);
        if (!template) {
            console.error('ContentBuilderBase: Template not found for type:', block.type);
            return;
        }
        
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data);
        blockElement.dataset.type = block.type;
        
        // Add direct event listeners to this specific block
        this.addDirectEventListeners(blockElement);
        
        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }
        
        // Initialize TextBlockManager for text blocks
        if (block.type === 'text') {
            console.log('ContentBuilderBase: Initializing TextBlockManager for text block');
            if (window.TextBlockManager && !window.textBlockManager) {
                window.textBlockManager = new window.TextBlockManager();
            }
            
            // Setup rich text editor for this specific block
            const editor = blockElement.querySelector('.rich-text-editor');
            if (editor && window.textBlockManager) {
                console.log('ContentBuilderBase: Setting up rich text editor');
                window.textBlockManager.setupRichTextEditor(editor);
            }
            
            // Add direct input handler for immediate saving
            const textEditor = blockElement.querySelector('.rich-text-editor');
            const textarea = blockElement.querySelector('textarea');
            
            if (textEditor) {
                console.log('ContentBuilderBase: Adding direct input handler for rich text editor');
                textEditor.addEventListener('input', (e) => {
                    console.log('ContentBuilderBase: Direct input detected on rich text editor');
                    this.saveTextContentImmediately(blockElement, textEditor);
                });
                
                textEditor.addEventListener('blur', (e) => {
                    console.log('ContentBuilderBase: Blur detected on rich text editor');
                    this.saveTextContentImmediately(blockElement, textEditor);
                });
            }
            
            if (textarea) {
                console.log('ContentBuilderBase: Adding direct input handler for textarea');
                textarea.addEventListener('input', (e) => {
                    console.log('ContentBuilderBase: Direct input detected on textarea');
                    this.saveTextContentImmediately(blockElement, textarea);
                });
                
                textarea.addEventListener('blur', (e) => {
                    console.log('ContentBuilderBase: Blur detected on textarea');
                    this.saveTextContentImmediately(blockElement, textarea);
                });
            }
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

    addDirectEventListeners(blockElement) {
        // Add direct event listeners to action buttons
        const actionButtons = blockElement.querySelectorAll('[data-action]');
        actionButtons.forEach(button => {
            const action = button.dataset.action;
            button.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                
                console.log('ContentBuilderBase: Direct event listener triggered for action:', action);
                
                switch (action) {
                    case 'move-up':
                        this.moveBlockUp(blockElement);
                        break;
                    case 'move-down':
                        this.moveBlockDown(blockElement);
                        break;
                    case 'delete':
                        console.log('ContentBuilderBase: Direct delete action triggered');
                        this.deleteBlock(blockElement);
                        break;
                    case 'toggle-collapse':
                        this.toggleCollapse(blockElement);
                        break;
                    case 'fullscreen':
                        this.toggleFullscreen(blockElement);
                        break;
                    case 'insert-above':
                        this.insertBlockAbove(blockElement);
                        break;
                }
            });
        });
    }

    updateEmptyState() {
        if (this.blocks.length === 0) {
            this.emptyState.style.display = 'flex';
        } else {
            this.emptyState.style.display = 'none';
        }
    }

    moveBlockUp(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: moveBlockUp called with null blockElement');
            return;
        }
        
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex > 0) {
            [this.blocks[blockIndex], this.blocks[blockIndex - 1]] = [this.blocks[blockIndex - 1], this.blocks[blockIndex]];
            
            const prevBlock = blockElement.previousElementSibling;
            if (prevBlock && (prevBlock.classList.contains('content-block') || prevBlock.classList.contains('content-block-template'))) {
                this.blocksList.insertBefore(blockElement, prevBlock);
            }
            
            this.updateHiddenField();
            this.scrollToBlock(blockId);
            
            // Dispatch custom event
            this.eventManager.dispatch('blockMoved', {
                blockId: blockId, 
                direction: 'up', 
                contentType: this.config.contentType
            });
        }
    }

    moveBlockDown(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: moveBlockDown called with null blockElement');
            return;
        }
        
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex < this.blocks.length - 1) {
            [this.blocks[blockIndex], this.blocks[blockIndex + 1]] = [this.blocks[blockIndex + 1], this.blocks[blockIndex]];
            
            const nextBlock = blockElement.nextElementSibling;
            if (nextBlock && (nextBlock.classList.contains('content-block') || nextBlock.classList.contains('content-block-template'))) {
                this.blocksList.insertBefore(nextBlock, blockElement);
            }
            
            this.updateHiddenField();
            this.scrollToBlock(blockId);
            
            // Dispatch custom event
            this.eventManager.dispatch('blockMoved', {
                blockId: blockId, 
                direction: 'down', 
                contentType: this.config.contentType
            });
        }
    }

    deleteBlock(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: deleteBlock called with null blockElement');
            return;
        }
        
        if (confirm('آیا از حذف این بلاک اطمینان دارید؟')) {
            const blockId = blockElement.dataset.blockId;
            
            if (!blockId) {
                console.error('ContentBuilderBase: Block element has no blockId');
                return;
            }
            
            console.log('ContentBuilderBase: Deleting block with ID:', blockId);
            console.log('ContentBuilderBase: Blocks before deletion:', this.blocks.length);
            
            // Remove from blocks array
            this.blocks = this.blocks.filter(b => b.id !== blockId);
            
            console.log('ContentBuilderBase: Blocks after deletion:', this.blocks.length);
            
            // Remove from DOM
            blockElement.remove();
            
            // Update UI
            this.updateEmptyState();
            
            // Force immediate update of hidden fields
            this.updateHiddenField();
            
            // Also trigger sync manager to ensure all fields are updated
            if (this.syncManager) {
                this.syncManager.sync('blockDeleted');
            }
            
            // Dispatch custom event
            this.eventManager.dispatch('blockDeleted', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
            
            // Notify step4Manager to refresh add button handlers
            if (window.step4Manager && typeof window.step4Manager.refreshAddButtonHandlers === 'function') {
                console.log('ContentBuilderBase: Notifying step4Manager to refresh add button handlers');
                window.step4Manager.refreshAddButtonHandlers();
            }
            
            console.log('ContentBuilderBase: Block deletion completed');
        }
    }

    insertBlockAbove(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: insertBlockAbove called with null blockElement');
            return;
        }
        
        // This will be handled by the specific content builder
        this.eventManager.dispatch('insertBlockAbove', {
            blockElement: blockElement
        });
    }

    handleInsertBlockAbove(blockElement) {
        // Show block type selection modal for inserting above
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId);
        }
    }

    updateBlockCaption(textarea) {
        const blockElement = textarea.closest('.content-block-template, .content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            block.data.caption = textarea.value;
            this.updateHiddenField();
            
            // Dispatch custom event
            this.eventManager.dispatch('blockContentChanged', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
        }
    }

    toggleCollapse(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: toggleCollapse called with null blockElement');
            return;
        }
        
        if (blockElement.classList.contains('fullscreen')) {
            return;
        }
        blockElement.classList.toggle('collapsed');
    }

    toggleFullscreen(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: toggleFullscreen called with null blockElement');
            return;
        }
        
        blockElement.classList.toggle('fullscreen');
        
        if (blockElement.classList.contains('fullscreen')) {
            blockElement.classList.remove('collapsed');
        }
    }

    handleBlockContentChanged(detail) {
        console.log('ContentBuilderBase: Block content changed event received:', detail);
        
        const blockElement = detail.blockElement;
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        console.log('ContentBuilderBase: Block ID:', blockId);
        console.log('ContentBuilderBase: Block found:', !!block);
        
        if (block) {
            // Update block data with new content
            if (detail.blockData) {
                block.data = { ...block.data, ...detail.blockData };
                console.log('ContentBuilderBase: Updated block data from blockData');
            }
            if (detail.content !== undefined) {
                block.data.content = detail.content;
                console.log('ContentBuilderBase: Updated block content:', detail.content);
            }
            if (detail.textContent !== undefined) {
                block.data.textContent = detail.textContent;
                console.log('ContentBuilderBase: Updated block textContent:', detail.textContent);
            }
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            console.log('ContentBuilderBase: Updated block data:', block.data);
            
            this.updateHiddenField();
            
            // Dispatch custom event
            this.eventManager.dispatch('blockContentChanged', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
        } else {
            console.warn('ContentBuilderBase: Block not found for ID:', blockId);
        }
    }

    handleRichTextInput(editor) {
        console.log('ContentBuilderBase: Handling rich text input');
        
        const blockElement = editor.closest('.content-block');
        if (!blockElement) {
            console.warn('ContentBuilderBase: No block element found for editor');
            return;
        }
        
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        console.log('ContentBuilderBase: Rich text input - Block ID:', blockId);
        console.log('ContentBuilderBase: Rich text input - Block found:', !!block);
        
        if (block) {
            // Update block data with editor content
            block.data.content = editor.innerHTML;
            block.data.textContent = editor.textContent;
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            console.log('ContentBuilderBase: Rich text input - Updated block data:', block.data);
            
            this.updateHiddenField();
            
            // Dispatch custom event
            this.eventManager.dispatch('blockContentChanged', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
            
            console.log('ContentBuilderBase: Rich text input - Content saved successfully');
        } else {
            console.warn('ContentBuilderBase: Rich text input - Block not found for ID:', blockId);
        }
    }

    saveTextContentImmediately(blockElement, editor) {
        console.log('ContentBuilderBase: Saving text content immediately');
        
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        console.log('ContentBuilderBase: Immediate save - Block ID:', blockId);
        console.log('ContentBuilderBase: Immediate save - Block found:', !!block);
        
        if (block) {
            // Get content from editor
            let content = '';
            let textContent = '';
            
            if (editor.classList.contains('rich-text-editor')) {
                content = editor.innerHTML;
                textContent = editor.textContent;
            } else if (editor.tagName === 'TEXTAREA') {
                content = editor.value;
                textContent = editor.value;
            }
            
            console.log('ContentBuilderBase: Immediate save - Editor content:', content);
            console.log('ContentBuilderBase: Immediate save - Editor textContent:', textContent);
            
            // Update block data
            block.data.content = content;
            block.data.textContent = textContent;
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            console.log('ContentBuilderBase: Immediate save - Updated block data:', block.data);
            
            // Update hidden field immediately
            this.updateHiddenField();
            
            console.log('ContentBuilderBase: Immediate save - Content saved successfully');
        } else {
            console.warn('ContentBuilderBase: Immediate save - Block not found for ID:', blockId);
        }
    }

    updateBlockSettings(select) {
        const blockElement = select.closest('.content-block-template, .content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            const setting = select.dataset.setting;
            block.data[setting] = select.value;
            this.updateHiddenField();
        }
    }

    updateHiddenField() {
        console.log('ContentBuilderBase: Updating hidden field with', this.blocks.length, 'blocks');
        
        const cleanBlocks = this.blocks.map(block => {
            const cleanBlock = { ...block };
            const cleanData = { ...block.data };
            
            // Remove temporary data
            delete cleanData.pendingFile;
            delete cleanData.previewUrl;
            delete cleanData.originalData;
            
            cleanBlock.data = cleanData;
            return cleanBlock;
        });
        
        const content = { type: this.config.contentType, blocks: cleanBlocks };
        const contentJson = JSON.stringify(content);
        
        console.log('ContentBuilderBase: Generated content JSON:', contentJson);
        
        // Use field manager to update fields
        const hiddenFieldUpdated = this.fieldManager.updateField(this.config.hiddenFieldId, contentJson);
        const mainFieldUpdated = this.fieldManager.updateField('contentJson', contentJson);
        
        console.log('ContentBuilderBase: Field update results - Hidden field:', hiddenFieldUpdated, 'Main field:', mainFieldUpdated);
        
        // Also directly update the fields as a fallback
        const hiddenField = document.getElementById(this.config.hiddenFieldId);
        const mainField = document.getElementById('contentJson');
        
        if (hiddenField) {
            hiddenField.value = contentJson;
            console.log('ContentBuilderBase: Directly updated hidden field:', this.config.hiddenFieldId);
        }
        
        if (mainField) {
            mainField.value = contentJson;
            console.log('ContentBuilderBase: Directly updated main field: contentJson');
        }
        
        if (!this.isLoadingExistingContent) {
            this.updatePreview();
        }
    }

    loadExistingContent() {
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            return;
        }
        
        try {
            this.isLoadingExistingContent = true;
            
            const data = JSON.parse(hiddenFieldValue);
            
            if (data.blocks && Array.isArray(data.blocks)) {
                const existingBlocks = this.blocksList.querySelectorAll('.content-block');
                existingBlocks.forEach(block => block.remove());
                
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
            console.error('ContentBuilderBase: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    // Force reload existing content (called from step4-content.js)
    forceReloadExistingContent() {
        this.loadExistingContent();
    }

    // Force sync content with main field (called from step4-content.js)
    forceSyncWithMainField() {
        console.log('ContentBuilderBase: Force syncing with main field');
        this.updateHiddenField();
        
        // Also trigger sync manager
        if (this.syncManager) {
            this.syncManager.sync('forceSync');
        }
    }

    populateBlockContent() {
        this.blocks.forEach(block => {
            const blockElement = document.querySelector(`[data-block-id="${block.id}"]`);
            if (!blockElement) {
                console.warn(`ContentBuilderBase: Block element not found for block ${block.id}`);
                return;
            }
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            // Use block-specific populate methods
            this.populateBlockByType(blockElement, block);
        });
    }

    populateBlockByType(blockElement, block) {
        // Dispatch custom event for block-specific managers to handle
        this.eventManager.dispatch('populateBlockContent', {
            blockElement: blockElement,
            block: block,
            blockType: block.type
        });
    }

    scrollToNewBlock(blockId) {
        setTimeout(() => {
            const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
            if (blockElement) {
                blockElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center',
                    inline: 'nearest'
                });
                
                blockElement.classList.add('highlight');
                
                setTimeout(() => {
                    blockElement.classList.remove('highlight');
                }, 2000);
            }
        }, 100);
    }

    scrollToBlock(blockId) {
        setTimeout(() => {
            const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
            if (blockElement) {
                blockElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center',
                    inline: 'nearest'
                });
            }
        }, 50);
    }

    getContent() {
        return {
            type: this.config.contentType,
            blocks: this.blocks
        };
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
        this.previewManager.showPreview(this.getContent());
    }

    // Abstract methods to be implemented by specific content builders
    updatePreview() {
        // To be implemented by specific content builders
        console.warn('ContentBuilderBase: updatePreview method should be implemented by specific content builder');
    }

    generateBlockPreview(block) {
        // Use shared preview manager
        return this.previewManager.generateBlockPreview(block);
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

// Export for use in other files
if (typeof window !== 'undefined') {
    window.ContentBuilderBase = ContentBuilderBase;
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = ContentBuilderBase;
}
