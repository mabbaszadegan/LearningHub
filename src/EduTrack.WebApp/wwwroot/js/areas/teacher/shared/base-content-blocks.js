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

    // Method to reinitialize if block managers are not available
    reinitialize() {
        this.isInitialized = false;
        this.blockManagers.clear();
        this.init();
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

        // Question blocks reuse existing managers - no need for separate managers

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

        // Handle block actions - use capture phase to avoid conflicts with direct listeners
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action]')) {
                const actionButton = e.target.closest('[data-action]');
                const action = actionButton.dataset.action;
                
                // Skip block-specific actions - let individual block managers handle them
                const blockSpecificActions = [
                    // Audio block actions
                    'start-recording', 'stop-recording', 'play-recording', 
                    'change-audio', 'audio-upload',
                    // Image block actions
                    'change-image', 'image-upload',
                    // Video block actions
                    'change-video', 'video-upload',
                    // Text block actions
                    'text-formatting', 'create-link', 'insert-code'
                ];
                
                if (blockSpecificActions.includes(action)) {
                    console.log('SharedContentBlockManager: Skipping block-specific action:', action);
                    return;
                }
                
                // Check if there's an active content builder that should handle this
                const activeBuilder = this.getActiveContentBuilder();
                if (activeBuilder) {
                    console.log('SharedContentBlockManager: Active content builder found, delegating to direct listeners');
                    // Let the active content builder handle it through its direct listeners
                    return;
                }
                
                console.log('SharedContentBlockManager: No active content builder, handling action directly');
                // Only handle if no active content builder is present
                this.handleBlockAction(e);
            }
        }, true); // Use capture phase

        // Handle dynamic content loading
        document.addEventListener('DOMNodeInserted', (e) => {
            if (e.target.classList && (e.target.classList.contains('content-block-template') || e.target.classList.contains('question-block-template') || e.target.classList.contains('content-block'))) {
                this.initializeNewBlock(e.target);
            }
        });
    }

    getActiveContentBuilder() {
        console.log('SharedContentBlockManager: Checking for active content builder...');
        
        // Check for reminder content builder
        const reminderBuilder = document.getElementById('reminderContentBuilder');
        const reminderVisible = reminderBuilder && reminderBuilder.style.display !== 'none';
        console.log('Reminder builder check:', {
            element: !!reminderBuilder,
            visible: reminderVisible,
            manager: !!window.reminderBlockManager
        });
        
        if (window.reminderBlockManager && reminderVisible) {
            console.log('SharedContentBlockManager: Found active reminder builder');
            return window.reminderBlockManager;
        }
        
        // Check for gap-fill content builder (must check before written since gap fill has its own manager)
        const gapFillBuilder = document.getElementById('gapFillContentBuilder');
        const gapFillVisible = gapFillBuilder && gapFillBuilder.style.display !== 'none' && 
                               gapFillBuilder.offsetParent !== null; // Also check if actually visible
        console.log('GapFill builder check:', {
            element: !!gapFillBuilder,
            visible: gapFillVisible,
            display: gapFillBuilder ? gapFillBuilder.style.display : 'none',
            manager: !!window.gapFillContentManager,
            blocksList: window.gapFillContentManager?.blocksList ? !!window.gapFillContentManager.blocksList : false
        });
        if (window.gapFillContentManager && gapFillVisible) {
            // Ensure blocksList is available - scope to the visible builder
            if (!window.gapFillContentManager.blocksList) {
                const containerId = window.gapFillContentManager.config?.containerId || 'contentBlocksList';
                // Try to find blocksList within the visible gapFillContentBuilder first
                const blocksListElement = gapFillBuilder.querySelector(`#${containerId}`) || 
                                         document.getElementById(containerId);
                if (blocksListElement) {
                    window.gapFillContentManager.blocksList = blocksListElement;
                    console.log('SharedContentBlockManager: Found blocksList element, assigned to gapFillContentManager');
                }
            }
            
            if (window.gapFillContentManager.blocksList) {
                console.log('SharedContentBlockManager: Found active gap-fill builder');
                return window.gapFillContentManager;
            } else {
                console.warn('SharedContentBlockManager: gapFillContentManager found but blocksList not available');
            }
        }
        
        // Check for written content builder (used for written and multiple choice)
        const writtenBuilder = document.getElementById('writtenContentBuilder');
        const writtenVisible = writtenBuilder && writtenBuilder.style.display !== 'none';
        console.log('Written builder check:', {
            element: !!writtenBuilder,
            visible: writtenVisible,
            manager: !!window.writtenBlockManager,
            multipleChoiceMode: !!window.multipleChoiceMode,
            contentType: window.writtenBlockManager?.config?.contentType,
            blocksList: window.writtenBlockManager?.blocksList ? !!window.writtenBlockManager.blocksList : false
        });
        
        // For multiple choice, multipleChoiceMode is set and writtenContentBuilder is shown
        // For written, writtenContentBuilder is shown
        if (window.writtenBlockManager && writtenVisible) {
            // Additional check: ensure blocksList is available
            if (!window.writtenBlockManager.blocksList) {
                console.warn('SharedContentBlockManager: writtenBlockManager found but blocksList not available');
                // Try to get blocksList from DOM
                const blocksListElement = document.getElementById(window.writtenBlockManager.config?.containerId || 'contentBlocksList');
                if (blocksListElement) {
                    window.writtenBlockManager.blocksList = blocksListElement;
                    console.log('SharedContentBlockManager: Found blocksList element, assigned to manager');
                } else {
                    console.warn('SharedContentBlockManager: Could not find blocksList element');
                }
            }
            
            // If blocksList is still not available, try to initialize
            if (!window.writtenBlockManager.blocksList && typeof window.initializeWrittenBlockManager === 'function') {
                console.log('SharedContentBlockManager: Attempting to reinitialize writtenBlockManager...');
                window.initializeWrittenBlockManager();
            }
            
            if (window.writtenBlockManager.blocksList) {
                console.log('SharedContentBlockManager: Found active written builder');
                return window.writtenBlockManager;
            } else {
                console.warn('SharedContentBlockManager: writtenBlockManager found but blocksList still not available');
            }
        }
        
        // Check for other content builders
        const contentBuilder = document.getElementById('contentBuilder');
        const contentVisible = contentBuilder && contentBuilder.style.display !== 'none';
        console.log('Content builder check:', {
            element: !!contentBuilder,
            visible: contentVisible,
            manager: !!window.contentBlockManager
        });
        
        if (window.contentBlockManager && contentVisible) {
            console.log('SharedContentBlockManager: Found active content builder');
            return window.contentBlockManager;
        }
        
        console.log('SharedContentBlockManager: No active content builder found');
        return null;
    }

    handleBlockTypeSelected(type) {
        console.log('SharedContentBlockManager: handleBlockTypeSelected called with type:', type);
        
        // Find the active content builder
        const activeBuilder = this.getActiveContentBuilder();
        
        console.log('SharedContentBlockManager: Active builder found:', {
            builder: !!activeBuilder,
            builderType: activeBuilder ? activeBuilder.constructor.name : 'none',
            hasAddBlock: activeBuilder && typeof activeBuilder.addBlock === 'function',
            blocksList: activeBuilder && activeBuilder.blocksList ? !!activeBuilder.blocksList : false
        });
        
        if (activeBuilder && typeof activeBuilder.addBlock === 'function') {
            console.log('SharedContentBlockManager: Calling addBlock with type:', type);
            try {
                activeBuilder.addBlock(type);
                console.log('SharedContentBlockManager: addBlock completed successfully');
            } catch (error) {
                console.error('SharedContentBlockManager: Error in addBlock:', error);
            }
        } else {
            console.warn('SharedContentBlockManager: No active content builder found or addBlock not available', {
                activeBuilder: !!activeBuilder,
                hasAddBlock: activeBuilder && typeof activeBuilder.addBlock === 'function'
            });
            // Fallback: trigger custom event for specific implementations
            const event = new CustomEvent('blockTypeSelected', {
                detail: { type: type }
            });
            document.dispatchEvent(event);
        }
    }

    handleBlockContentChanged(detail) {
        
        // Update any global state or trigger callbacks
        if (this.options.onContentChanged) {
            this.options.onContentChanged(detail);
        }
    }

    handleBlockAction(e) {
        const actionButton = e.target.closest('[data-action]');
        if (!actionButton) return;
        const action = actionButton.dataset.action;
        
        // Skip block-specific actions - let individual block managers handle them
        const blockSpecificActions = [
            // Audio block actions
            'start-recording', 'stop-recording', 'play-recording', 
            'change-audio', 'audio-upload',
            // Image block actions
            'change-image', 'image-upload',
            // Video block actions
            'change-video', 'video-upload',
            // Text block actions
            'text-formatting', 'create-link', 'insert-code'
        ];
        
        if (blockSpecificActions.includes(action)) {
            console.log('SharedContentBlockManager: Skipping block-specific action in handleBlockAction:', action);
            return;
        }
        
        const blockElement = actionButton.closest('.content-block-template, .question-block-template, .content-block');
        
        if (!blockElement) return;

        // First, try to delegate to the active content builder
        const activeBuilder = this.getActiveContentBuilder();
        if (activeBuilder) {
            // Use the content builder's method for proper data management
            switch (action) {
                case 'move-up':
                    if (typeof activeBuilder.moveBlockUp === 'function') {
                        activeBuilder.moveBlockUp(blockElement);
                        return;
                    }
                    break;
                case 'move-down':
                    if (typeof activeBuilder.moveBlockDown === 'function') {
                        activeBuilder.moveBlockDown(blockElement);
                        return;
                    }
                    break;
                case 'delete':
                    if (typeof activeBuilder.deleteBlock === 'function') {
                        activeBuilder.deleteBlock(blockElement);
                        return;
                    }
                    break;
                case 'toggle-collapse':
                    if (typeof activeBuilder.toggleCollapse === 'function') {
                        activeBuilder.toggleCollapse(blockElement);
                        return;
                    }
                    break;
                case 'fullscreen':
                    if (typeof activeBuilder.toggleFullscreen === 'function') {
                        activeBuilder.toggleFullscreen(blockElement);
                        return;
                    }
                    break;
                case 'insert-above':
                    if (typeof activeBuilder.insertBlockAbove === 'function') {
                        activeBuilder.insertBlockAbove(blockElement);
                        return;
                    }
                    break;
            }
        }

        const blockType = blockElement.dataset.type;
        // For question blocks, use the base template type for manager lookup
        const managerType = blockType.startsWith('question') ? blockType.replace('question', '').toLowerCase() : blockType;
        const manager = this.blockManagers.get(managerType);
        
        // Try to delegate to specific block manager first
        if (manager && typeof manager.handleAction === 'function') {
            manager.handleAction(action, blockElement);
            return;
        }
        
        // Handle common block actions directly as fallback
        this.handleCommonBlockAction(action, blockElement);
    }

    handleCommonBlockAction(action, blockElement) {
        switch (action) {
            case 'move-up':
                this.moveBlockUp(blockElement);
                break;
            case 'move-down':
                this.moveBlockDown(blockElement);
                break;
            case 'delete':
                this.deleteBlock(blockElement);
                break;
            case 'fullscreen':
                this.toggleFullscreen(blockElement);
                break;
            case 'toggle-collapse':
                this.toggleCollapse(blockElement);
                break;
            case 'insert-above':
                this.insertBlockAbove(blockElement);
                break;
            default:
                console.warn(`Unknown block action: ${action}`);
        }
    }

    moveBlockUp(blockElement) {
        const previousBlock = blockElement.previousElementSibling;
        if (previousBlock && (previousBlock.classList.contains('content-block-template') || previousBlock.classList.contains('question-block-template') || previousBlock.classList.contains('content-block'))) {
            blockElement.parentNode.insertBefore(blockElement, previousBlock);
            this.triggerBlockReorder();
        }
    }

    moveBlockDown(blockElement) {
        const nextBlock = blockElement.nextElementSibling;
        if (nextBlock && (nextBlock.classList.contains('content-block-template') || nextBlock.classList.contains('question-block-template') || nextBlock.classList.contains('content-block'))) {
            blockElement.parentNode.insertBefore(nextBlock, blockElement);
            this.triggerBlockReorder();
        }
    }

    deleteBlock(blockElement) {
        if (confirm('آیا از حذف این بلاک اطمینان دارید؟')) {
            blockElement.remove();
            this.triggerBlockReorder();
        }
    }

    toggleFullscreen(blockElement) {
        blockElement.classList.toggle('fullscreen');
        
        if (blockElement.classList.contains('fullscreen')) {
            blockElement.classList.remove('collapsed');
        }
    }

    toggleCollapse(blockElement) {
        if (blockElement.classList.contains('fullscreen')) {
            return;
        }
        blockElement.classList.toggle('collapsed');
    }

    insertBlockAbove(blockElement) {
        // Trigger event for specific content builders to handle
        const event = new CustomEvent('insertBlockAbove', {
            detail: {
                blockElement: blockElement
            }
        });
        document.dispatchEvent(event);
    }

    triggerBlockReorder() {
        // Trigger event to notify content builders about reordering
        const event = new CustomEvent('blockReorder', {
            detail: {
                timestamp: Date.now()
            }
        });
        document.dispatchEvent(event);
    }

    initializeNewBlock(blockElement) {
        const blockType = blockElement.dataset.type;
        // For question blocks, use the base template type for manager lookup
        const managerType = blockType.startsWith('question') ? blockType.replace('question', '').toLowerCase() : blockType;
        const manager = this.blockManagers.get(managerType);
        
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
        const blocks = document.querySelectorAll('.content-block-template, .question-block-template, .content-block');
        
        blocks.forEach((block, index) => {
            const type = block.dataset.type;
            // For question blocks, use the base template type for manager lookup
            const managerType = type.startsWith('question') ? type.replace('question', '').toLowerCase() : type;
            const manager = this.blockManagers.get(managerType);
            
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
        // For question blocks, use the base template type for manager lookup
        const managerType = blockType.startsWith('question') ? blockType.replace('question', '').toLowerCase() : blockType;
        const manager = this.blockManagers.get(managerType);
        
        if (manager && typeof manager.setBlockData === 'function') {
            manager.setBlockData(blockElement, data);
        }
    }

    clearBlock(blockElement) {
        const blockType = blockElement.dataset.type;
        // For question blocks, use the base template type for manager lookup
        const managerType = blockType.startsWith('question') ? blockType.replace('question', '').toLowerCase() : blockType;
        const manager = this.blockManagers.get(managerType);
        
        if (manager && typeof manager.clearBlock === 'function') {
            manager.clearBlock(blockElement);
        }
    }

    showBlockTypeModal(modalId, itemType = null) {
        const modalManager = this.blockManagers.get('modal');
        if (modalManager) {
            modalManager.modalId = modalId || modalManager.modalId;
            modalManager.showModal(itemType);
        } else {
            // Try to reinitialize if modal manager is not found
            this.reinitialize();
            const retryModalManager = this.blockManagers.get('modal');
            if (retryModalManager) {
                retryModalManager.modalId = modalId || retryModalManager.modalId;
                retryModalManager.showModal(itemType);
            }
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
        // For question blocks, use the base template type for manager lookup
        const managerType = blockType.startsWith('question') ? blockType.replace('question', '').toLowerCase() : blockType;
        const manager = this.blockManagers.get(managerType);
        
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

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Wait a bit for all scripts to load
    setTimeout(() => {
        // Initialize with default options
        initializeContentBlocks({
            imageMaxSize: 5 * 1024 * 1024, // 5MB
            videoMaxSize: 50 * 1024 * 1024, // 50MB
            audioMaxSize: 10 * 1024 * 1024, // 10MB
            modalId: 'blockTypeModal'
        });
    }, 100);
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SharedContentBlockManager;
}
