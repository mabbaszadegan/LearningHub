/**
 * Shared Content Block Manager
 * Initializes and manages all content block types
 */

class SharedContentBlockManager {
    constructor(options = {}) {
        this.isInitialized = false;
        this.blockManagers = new Map();
        this.options = options;
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        this.initializeBlockManagers();
        this.setupGlobalEventListeners();
        this.isInitialized = true;
    }

    initializeBlockManagers() {
        // Initialize text block manager
        if (typeof TextBlockManager !== 'undefined') {
            this.blockManagers.set('text', new TextBlockManager());
        }

        // Initialize image block manager
        if (typeof ImageBlockManager !== 'undefined') {
            this.blockManagers.set('image', new ImageBlockManager({
                maxFileSize: this.options.imageMaxSize || 5 * 1024 * 1024,
                allowedTypes: this.options.imageTypes || ['image/jpeg', 'image/jpg', 'image/png', 'image/gif']
            }));
        }

        // Initialize video block manager
        if (typeof VideoBlockManager !== 'undefined') {
            this.blockManagers.set('video', new VideoBlockManager({
                maxFileSize: this.options.videoMaxSize || 50 * 1024 * 1024,
                allowedTypes: this.options.videoTypes || ['video/mp4', 'video/webm', 'video/quicktime']
            }));
        }

        // Initialize audio block manager
        if (typeof AudioBlockManager !== 'undefined') {
            this.blockManagers.set('audio', new AudioBlockManager({
                maxFileSize: this.options.audioMaxSize || 10 * 1024 * 1024,
                allowedTypes: this.options.audioTypes || ['audio/mpeg', 'audio/wav', 'audio/ogg']
            }));
        }

        // Initialize code block manager
        if (typeof CodeBlockManager !== 'undefined') {
            this.blockManagers.set('code', new CodeBlockManager({
                supportedLanguages: this.options.supportedLanguages || [
                    'javascript', 'python', 'csharp', 'java', 'cpp', 'c', 'php', 'ruby',
                    'go', 'rust', 'swift', 'kotlin', 'typescript', 'html', 'css', 'scss',
                    'sql', 'json', 'xml', 'yaml', 'markdown', 'bash', 'powershell', 'plaintext'
                ]
            }));
        }

        // Initialize block type selection modal
        if (typeof BlockTypeSelectionModal !== 'undefined') {
            this.blockManagers.set('modal', new BlockTypeSelectionModal({
                modalId: this.options.modalId || 'blockTypeModal',
                onTypeSelected: this.handleBlockTypeSelected.bind(this)
            }));
        }
    }

    setupGlobalEventListeners() {
        // Handle block content changes
        document.addEventListener('blockContentChanged', (e) => {
            this.handleBlockContentChanged(e.detail);
        });

        // Handle block actions
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action]')) {
                this.handleBlockAction(e);
            }
        });

        // Handle dynamic content loading
        document.addEventListener('DOMNodeInserted', (e) => {
            if (e.target.classList && e.target.classList.contains('content-block-template')) {
                this.initializeNewBlock(e.target);
            }
        });
    }

    handleBlockTypeSelected(type) {
        
        // Trigger custom event for specific implementations
        const event = new CustomEvent('blockTypeSelected', {
            detail: { type: type }
        });
        document.dispatchEvent(event);
    }

    handleBlockContentChanged(detail) {
        
        // Update any global state or trigger callbacks
        if (this.options.onContentChanged) {
            this.options.onContentChanged(detail);
        }
    }

    handleBlockAction(e) {
        const action = e.target.closest('[data-action]').dataset.action;
        const blockElement = e.target.closest('.content-block-template, .question-block-template');
        
        if (!blockElement) return;

        const blockType = blockElement.dataset.type;
        const manager = this.blockManagers.get(blockType);
        
        if (manager && typeof manager.handleAction === 'function') {
            manager.handleAction(action, blockElement);
        }
    }

    initializeNewBlock(blockElement) {
        const blockType = blockElement.dataset.type;
        const manager = this.blockManagers.get(blockType);
        
        if (manager && typeof manager.initializeBlock === 'function') {
            manager.initializeBlock(blockElement);
        }
    }

    // Public methods
    getBlockManager(type) {
        return this.blockManagers.get(type);
    }

    getAllBlockData() {
        const allData = {};
        const blocks = document.querySelectorAll('.content-block-template, .question-block-template');
        
        blocks.forEach((block, index) => {
            const type = block.dataset.type;
            const manager = this.blockManagers.get(type);
            
            if (manager && typeof manager.getBlockData === 'function') {
                allData[`block_${index}`] = {
                    type: type,
                    data: manager.getBlockData(block)
                };
            }
        });
        
        return allData;
    }

    setBlockData(blockElement, data) {
        const blockType = blockElement.dataset.type;
        const manager = this.blockManagers.get(blockType);
        
        if (manager && typeof manager.setBlockData === 'function') {
            manager.setBlockData(blockElement, data);
        }
    }

    clearBlock(blockElement) {
        const blockType = blockElement.dataset.type;
        const manager = this.blockManagers.get(blockType);
        
        if (manager && typeof manager.clearBlock === 'function') {
            manager.clearBlock(blockElement);
        }
    }

    showBlockTypeModal(modalId) {
        const modalManager = this.blockManagers.get('modal');
        if (modalManager) {
            modalManager.modalId = modalId || modalManager.modalId;
            modalManager.showModal();
        }
    }

    hideBlockTypeModal() {
        const modalManager = this.blockManagers.get('modal');
        if (modalManager) {
            modalManager.hideModal();
        }
    }

    // Utility methods
    validateBlockData(blockElement) {
        const blockType = blockElement.dataset.type;
        const manager = this.blockManagers.get(blockType);
        
        if (manager && typeof manager.validateData === 'function') {
            return manager.validateData(blockElement);
        }
        
        return true; // Default to valid if no validation method
    }

    exportBlockData() {
        return JSON.stringify(this.getAllBlockData(), null, 2);
    }

    importBlockData(jsonData) {
        try {
            const data = JSON.parse(jsonData);
            Object.values(data).forEach(blockInfo => {
                // Implementation would depend on specific requirements
            });
        } catch (error) {
            console.error('Error importing block data:', error);
        }
    }
}

// Global functions for backward compatibility
function initializeContentBlocks(options = {}) {
    if (!window.sharedContentBlockManager) {
        window.sharedContentBlockManager = new SharedContentBlockManager(options);
    }
    return window.sharedContentBlockManager;
}

function showBlockTypeModal(modalId) {
    if (window.sharedContentBlockManager) {
        window.sharedContentBlockManager.showBlockTypeModal(modalId);
    }
}

function hideBlockTypeModal() {
    if (window.sharedContentBlockManager) {
        window.sharedContentBlockManager.hideBlockTypeModal();
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize with default options
    initializeContentBlocks({
        imageMaxSize: 5 * 1024 * 1024, // 5MB
        videoMaxSize: 50 * 1024 * 1024, // 50MB
        audioMaxSize: 10 * 1024 * 1024, // 10MB
        modalId: 'blockTypeModal'
    });
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SharedContentBlockManager;
}
