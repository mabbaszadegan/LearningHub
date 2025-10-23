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
        
        this.setupEventListeners();
        this.loadExistingContent();
    }

    setupEventListeners() {
        // Block type selection
        document.addEventListener('blockTypeSelected', (e) => {
            if (e.detail.type) {
                this.addBlock(e.detail.type);
            }
        });

        // Block actions (move up, move down, delete)
        document.addEventListener('click', (e) => {
            if (e.target.matches('[data-action="move-up"]')) {
                this.moveBlockUp(e.target);
            } else if (e.target.matches('[data-action="move-down"]')) {
                this.moveBlockDown(e.target);
            } else if (e.target.matches('[data-action="delete"]')) {
                this.deleteBlock(e.target);
            } else if (e.target.matches('[data-action="toggle-collapse"]')) {
                this.toggleCollapse(e.target.closest('.content-block-template'));
            } else if (e.target.matches('[data-action="fullscreen"]')) {
                this.toggleFullscreen(e.target.closest('.content-block-template'));
            } else if (e.target.matches('[data-action="insert-above"]')) {
                this.insertBlockAbove(e.target.closest('.content-block-template'));
            }
        });

        // Listen for block content changes from specific block managers
        document.addEventListener('blockContentChanged', (e) => {
            this.handleBlockContentChanged(e.detail);
        });

        // Settings changes
        document.addEventListener('change', (e) => {
            if (e.target.matches('[data-setting]')) {
                this.updateBlockSettings(e.target);
            }
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
        if (!template) return;
        
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = block.id;
        
        // Store initial block data
        blockElement.dataset.blockData = JSON.stringify(block.data);
        
        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }
    }

    updateEmptyState() {
        if (this.blocks.length === 0) {
            this.emptyState.style.display = 'flex';
        } else {
            this.emptyState.style.display = 'none';
        }
    }

    moveBlockUp(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex > 0) {
            [this.blocks[blockIndex], this.blocks[blockIndex - 1]] = [this.blocks[blockIndex - 1], this.blocks[blockIndex]];
            
            const prevBlock = blockElement.previousElementSibling;
            if (prevBlock && prevBlock.classList.contains('content-block')) {
                this.blocksList.insertBefore(blockElement, prevBlock);
            }
            
            this.updateHiddenField();
            this.scrollToBlock(blockId);
        }
    }

    moveBlockDown(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex < this.blocks.length - 1) {
            [this.blocks[blockIndex], this.blocks[blockIndex + 1]] = [this.blocks[blockIndex + 1], this.blocks[blockIndex]];
            
            const nextBlock = blockElement.nextElementSibling;
            if (nextBlock && nextBlock.classList.contains('content-block')) {
                this.blocksList.insertBefore(nextBlock, blockElement);
            }
            
            this.updateHiddenField();
            this.scrollToBlock(blockId);
        }
    }

    deleteBlock(blockElement) {
        if (confirm('آیا از حذف این بلاک اطمینان دارید؟')) {
            const blockId = blockElement.dataset.blockId;
            
            this.blocks = this.blocks.filter(b => b.id !== blockId);
            blockElement.remove();
            
            this.updateEmptyState();
            this.updateHiddenField();
        }
    }

    insertBlockAbove(blockElement) {
        // This will be handled by the specific content builder
        const event = new CustomEvent('insertBlockAbove', {
            detail: {
                blockElement: blockElement
            }
        });
        document.dispatchEvent(event);
    }

    toggleCollapse(blockElement) {
        if (blockElement.classList.contains('fullscreen')) {
            return;
        }
        blockElement.classList.toggle('collapsed');
    }

    toggleFullscreen(blockElement) {
        blockElement.classList.toggle('fullscreen');
        
        if (blockElement.classList.contains('fullscreen')) {
            blockElement.classList.remove('collapsed');
        }
    }

    handleBlockContentChanged(detail) {
        const blockElement = detail.blockElement;
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            // Update block data with new content
            if (detail.blockData) {
                block.data = { ...block.data, ...detail.blockData };
            }
            if (detail.content !== undefined) {
                block.data.content = detail.content;
            }
            if (detail.textContent !== undefined) {
                block.data.textContent = detail.textContent;
            }
            
            this.updateHiddenField();
        }
    }

    updateBlockSettings(select) {
        const blockElement = select.closest('.content-block-template');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            const setting = select.dataset.setting;
            block.data[setting] = select.value;
            this.updateHiddenField();
        }
    }

    updateHiddenField() {
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
        
        if (this.hiddenField) {
            this.hiddenField.value = contentJson;
        }
        
        const mainContentField = document.getElementById('contentJson');
        if (mainContentField) {
            mainContentField.value = contentJson;
        }
        
        if (!this.isLoadingExistingContent) {
            this.updatePreview();
        }
    }

    loadExistingContent() {
        if (!this.hiddenField) {
            console.error('Hidden field not found');
            return;
        }
        
        const existingContent = this.hiddenField.value;
        
        if (existingContent && existingContent.trim()) {
            try {
                this.isLoadingExistingContent = true;
                
                const data = JSON.parse(existingContent);
                
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
                console.error('Error loading existing content:', error);
                this.isLoadingExistingContent = false;
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

    // Abstract methods to be implemented by specific content builders
    updatePreview() {
        // To be implemented by specific content builders
        console.warn('updatePreview method should be implemented by specific content builder');
    }

    showPreviewModal() {
        // To be implemented by specific content builders
        console.warn('showPreviewModal method should be implemented by specific content builder');
    }

    generateBlockPreview(block) {
        // To be implemented by specific content builders
        console.warn('generateBlockPreview method should be implemented by specific content builder');
        return '';
    }
}

// Export for use in other files
if (typeof window !== 'undefined') {
    window.ContentBuilderBase = ContentBuilderBase;
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = ContentBuilderBase;
}
