/**
 * Unified Content Manager
 * Single manager for all schedule item types
 * Routes to appropriate handlers based on item type
 */

class UnifiedContentManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'contentPreview',
            hiddenFieldId: 'contentJson',
            modalId: 'blockTypeModal',
            contentType: 'unified'
        });

        this.itemType = null;
        this.blockConfig = null;
        this.handlers = [];

        this.init();
    }

    init() {
        super.init();
        this.detectItemType();
        this.setupItemTypeListener();
        this.initializeHandlers();
        this.setupButtonHandlers();
        this.setupModalCallback();
    }

    setupButtonHandlers() {
        // Setup add block button
        const addBlockBtn = document.getElementById('addContentBlockBtn');
        if (addBlockBtn) {
            addBlockBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.handleAddBlockClick();
            });
        }

        // Setup preview button
        const previewBtn = document.getElementById('previewContentBtn');
        if (previewBtn) {
            previewBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.showPreview();
            });
        }
    }

    setupModalCallback() {
        // Wait for modal to be initialized, then set up callback
        let retryCount = 0;
        const maxRetries = 50; // 5 seconds max wait
        
        const setupCallback = () => {
            // Try window.blockTypeSelectionModal first (preferred)
            if (window.blockTypeSelectionModal) {
                window.blockTypeSelectionModal.setOnTypeSelected((type) => {
                    console.log('UnifiedContentManager: Block type selected from modal:', type);
                    console.log('UnifiedContentManager: Current instance:', this);
                    console.log('UnifiedContentManager: blocksList available:', !!this.blocksList);
                    this.addBlock(type);
                });
                console.log('UnifiedContentManager: Modal callback set up successfully on window.blockTypeSelectionModal');
                return true;
            }
            
            // Also try SharedContentBlockManager's modal instance
            if (window.sharedContentBlockManager) {
                const modalManager = window.sharedContentBlockManager.blockManagers?.get('modal');
                if (modalManager) {
                    modalManager.setOnTypeSelected((type) => {
                        console.log('UnifiedContentManager: Block type selected via SharedContentBlockManager:', type);
                        console.log('UnifiedContentManager: Current instance:', this);
                        console.log('UnifiedContentManager: blocksList available:', !!this.blocksList);
                        this.addBlock(type);
                    });
                    console.log('UnifiedContentManager: Modal callback set up via SharedContentBlockManager');
                    return true;
                }
            }
            
            return false;
        };
        
        const trySetup = () => {
            const success = setupCallback();
            if (!success) {
                retryCount++;
                if (retryCount < maxRetries) {
                    setTimeout(trySetup, 100);
                } else {
                    console.warn('UnifiedContentManager: Could not set up modal callback after max retries');
                }
            }
        };
        
        // Try to set up immediately
        trySetup();
        
        // Also try after DOM is fully ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                setTimeout(trySetup, 200);
            });
        }
        
        // Also set up after a delay to ensure all scripts are loaded
        setTimeout(trySetup, 500);
    }

    handleAddBlockClick() {
        // Always use string type (never integer)
        let itemType = 'reminder';
        
        // First, try to use cached itemType (already converted to string)
        if (this.itemType) {
            itemType = this.itemType;
        } else {
            // Otherwise, get from select and convert
            const itemTypeSelect = document.getElementById('itemType');
            if (itemTypeSelect && itemTypeSelect.value) {
                const rawValue = itemTypeSelect.value;
                if (typeof getItemTypeString !== 'undefined') {
                    itemType = getItemTypeString(rawValue);
                } else {
                    // Fallback: try to get from data attribute
                    const selectedOption = itemTypeSelect.options[itemTypeSelect.selectedIndex];
                    if (selectedOption && selectedOption.dataset.typeString) {
                        itemType = selectedOption.dataset.typeString;
                    }
                }
            }
        }
        
        console.log('UnifiedContentManager: Opening block type modal for item type:', itemType);
        
        // Try window.blockTypeSelectionModal first
        if (window.blockTypeSelectionModal) {
            window.blockTypeSelectionModal.showModal(itemType);
            return;
        }
        
        // Try SharedContentBlockManager's modal instance
        if (window.sharedContentBlockManager) {
            const modalManager = window.sharedContentBlockManager.blockManagers?.get('modal');
            if (modalManager) {
                modalManager.showModal(itemType);
                return;
            }
        }
        
        console.error('Block type selection modal not available');
    }

    showPreview() {
        // Show preview of content
        // Implementation to be completed
        console.log('Preview functionality to be implemented');
    }

    detectItemType() {
        const itemTypeSelect = document.getElementById('itemType');
        if (itemTypeSelect && itemTypeSelect.value) {
            // Always convert to string using helper function
            const rawValue = itemTypeSelect.value;
            if (typeof getItemTypeString !== 'undefined') {
                this.itemType = getItemTypeString(rawValue);
            } else {
                // Fallback: try to get from data attribute
                const selectedOption = itemTypeSelect.options[itemTypeSelect.selectedIndex];
                if (selectedOption && selectedOption.dataset.typeString) {
                    this.itemType = selectedOption.dataset.typeString;
                } else {
                    this.itemType = 'reminder';
                }
            }
            console.log('UnifiedContentManager: Detected item type:', this.itemType, 'from raw value:', rawValue);
            this.updateBlockConfig();
        } else {
            // Default to reminder if no value selected
            this.itemType = 'reminder';
            this.updateBlockConfig();
        }
    }

    setupItemTypeListener() {
        const itemTypeSelect = document.getElementById('itemType');
        if (itemTypeSelect) {
            itemTypeSelect.addEventListener('change', () => {
                const rawValue = itemTypeSelect.value;
                if (!rawValue) {
                    this.itemType = 'reminder';
                } else if (typeof getItemTypeString !== 'undefined') {
                    this.itemType = getItemTypeString(rawValue);
                } else {
                    // Fallback: try to get from data attribute
                    const selectedOption = itemTypeSelect.options[itemTypeSelect.selectedIndex];
                    if (selectedOption && selectedOption.dataset.typeString) {
                        this.itemType = selectedOption.dataset.typeString;
                    } else {
                        this.itemType = 'reminder';
                    }
                }
                console.log('UnifiedContentManager: Item type changed to:', this.itemType, 'from raw value:', rawValue);
                this.updateBlockConfig();
            });
        }
    }

    updateBlockConfig() {
        if (typeof getBlockTypeConfig !== 'undefined') {
            // Ensure itemType is converted to string format
            const typeString = typeof getItemTypeString !== 'undefined' 
                ? getItemTypeString(this.itemType) 
                : (this.itemType || 'reminder');
            this.blockConfig = getBlockTypeConfig(typeString);
        } else {
            // Fallback to default
            this.blockConfig = {
                regularBlocks: [],
                questionBlocks: [],
                questionTypeBlocks: []
            };
        }
    }

    initializeHandlers() {
        this.handlers = [
            new RegularBlockHandler(this),
            new QuestionBlockHandler(this),
            new MultipleChoiceHandler(this),
            new GapFillHandler(this),
            new OrderingHandler(this)
        ];
    }

    getHandler(blockType) {
        for (const handler of this.handlers) {
            if (handler.canHandle && handler.canHandle(blockType)) {
                return handler;
            }
        }
        return null;
    }

    isBlockTypeAllowed(blockType) {
        if (!this.blockConfig) return true;

        if (this.isRegularBlock(blockType)) {
            return this.blockConfig.regularBlocks.includes(blockType);
        } else if (this.isQuestionBlock(blockType)) {
            return this.blockConfig.questionBlocks.includes(blockType);
        } else if (this.isQuestionTypeBlock(blockType)) {
            return this.blockConfig.questionTypeBlocks.includes(blockType);
        }

        return false;
    }

    isRegularBlock(type) {
        return ['text', 'image', 'video', 'audio', 'code'].includes(type);
    }

    isQuestionBlock(type) {
        return ['questionText', 'questionImage', 'questionVideo', 'questionAudio'].includes(type);
    }

    isQuestionTypeBlock(type) {
        return ['multipleChoice', 'gapFill', 'ordering', 'matching', 'errorFinding'].includes(type);
    }

    addBlock(type) {
        console.log('UnifiedContentManager.addBlock called:', {
            type: type,
            itemType: this.itemType,
            blockConfig: this.blockConfig,
            blocksList: !!this.blocksList
        });
        
        if (!this.blocksList) {
            console.error('UnifiedContentManager: blocksList not available');
            return;
        }

        if (!this.isBlockTypeAllowed(type)) {
            console.warn(`Block type "${type}" is not allowed for item type "${this.itemType}"`);
            return;
        }

        // For regular and question blocks, we don't need a handler
        // They use base class renderBlock directly
        const isQuestionTypeBlock = this.isQuestionTypeBlock(type);
        const handler = isQuestionTypeBlock ? this.getHandler(type) : null;
        
        if (isQuestionTypeBlock && !handler) {
            console.error(`No handler found for block type: ${type}`);
            return;
        }

        const blockId = `block-${this.nextBlockId++}`;
        const block = {
            id: blockId,
            type: type,
            order: this.blocks.length,
            data: this.getDefaultBlockDataForType(type)
        };

        console.log('UnifiedContentManager: Created block:', block);

        this.blocks.push(block);

        // Render block (which will add to DOM)
        const renderedElement = this.renderBlock(block);
        
        if (!renderedElement) {
            console.error('UnifiedContentManager: renderBlock returned null/undefined');
            // Remove from blocks array if rendering failed
            this.blocks.pop();
            return;
        }

        console.log('UnifiedContentManager: Block rendered successfully:', {
            blockId: blockId,
            element: !!renderedElement,
            inDOM: renderedElement && document.body.contains(renderedElement)
        });

        this.updateEmptyState();
        this.updateHiddenField();
        this.scrollToNewBlock(blockId);
    }

    getDefaultBlockDataForType(type) {
        const defaults = {
            multipleChoice: {
                questions: [],
                answerType: 'single',
                randomizeOptions: false
            },
            gapFill: {
                text: '',
                gaps: [],
                answerType: 'exact',
                caseSensitive: false
            },
            ordering: {
                items: [],
                correctOrder: [],
                allowDragDrop: true,
                direction: 'vertical',
                showNumbers: true
            },
            matching: {
                leftItems: [],
                rightItems: [],
                connections: []
            },
            errorFinding: {
                text: '',
                errors: [],
                showLineNumbers: true
            }
        };

        return defaults[type] || this.getDefaultBlockData(type);
    }

    renderBlock(block) {
        // For question type blocks (multipleChoice, gapFill, etc.), use handlers
        // For regular and question blocks, use base class directly to avoid infinite loop
        const isQuestionTypeBlock = this.isQuestionTypeBlock(block.type);
        
        if (isQuestionTypeBlock) {
            const handler = this.getHandler(block.type);
            if (handler && handler.render) {
                const blockElement = handler.render(block);
                if (blockElement) {
                    // Add to DOM
                    const emptyState = this.blocksList.querySelector('.empty-state');
                    if (emptyState) {
                        this.blocksList.insertBefore(blockElement, emptyState);
                    } else {
                        this.blocksList.appendChild(blockElement);
                    }
                    
                    // Initialize handlers if they have an initialize method
                    setTimeout(() => {
                        if (handler.initialize) {
                            handler.initialize(blockElement, block);
                        }
                    }, 100);
                }
                return blockElement;
            }
        }

        // For regular and question blocks, use base class directly (avoids infinite loop)
        return super.renderBlock(block);
    }

    loadExistingContent() {
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            return;
        }

        try {
            this.isLoadingExistingContent = true;
            let data = JSON.parse(hiddenFieldValue);

            // Migrate old structure if needed
            if (typeof ContentMigrator !== 'undefined') {
                data = ContentMigrator.migrate(data, this.itemType);
            }

            const blocks = data.blocks || [];
            if (!Array.isArray(blocks) || blocks.length === 0) {
                this.isLoadingExistingContent = false;
                return;
            }

            this.blocks = blocks;

            if (this.blocks.length > 0) {
                this.nextBlockId = Math.max(...this.blocks.map(b => {
                    const match = b.id.match(/\d+/);
                    return match ? parseInt(match[0]) : 0;
                })) + 1;
            }

            // Render all blocks
            this.blocks.forEach(block => {
                this.renderBlock(block);
            });

            this.updateEmptyState();
            this.isLoadingExistingContent = false;

        } catch (error) {
            console.error('UnifiedContentManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    getContent() {
        this.collectCurrentBlockData();
        return {
            itemType: this.itemType,
            blocks: this.blocks,
            settings: {}
        };
    }

    findBlockElement(blockId) {
        if (!this.blocksList) return null;
        return this.blocksList.querySelector(`[data-block-id="${blockId}"]`);
    }

    collectCurrentBlockData() {
        this.blocks.forEach(block => {
            const blockElement = this.findBlockElement(block.id);
            if (blockElement) {
                const handler = this.getHandler(block.type);
                if (handler && handler.collectData) {
                    block.data = handler.collectData(blockElement, block);
                } else {
                    // Fallback to base class collection
                    if (typeof QuestionBlockBase !== 'undefined') {
                        QuestionBlockBase.collectQuestionFields(blockElement, block);
                    }
                }
            }
        });
    }

    // Compatibility methods for form manager integration
    collectStep4Data() {
        return new Promise((resolve) => {
            this.collectCurrentBlockData();
            const content = this.getContent();
            resolve({
                ContentJson: JSON.stringify(content)
            });
        });
    }

    collectContentData() {
        this.collectCurrentBlockData();
        const content = this.getContent();
        return content;
    }

    loadStepData() {
        return new Promise((resolve) => {
            this.loadExistingContent();
            setTimeout(resolve, 500);
        });
    }

    updateStep4Content() {
        this.updateHiddenField();
    }
}

// Initialize
function initializeUnifiedContentManager() {
    if (window.unifiedContentManager) {
        return;
    }

    const requiredElements = [
        'contentBlocksList',
        'emptyBlocksState',
        'contentJson'
    ];

    const missingElements = requiredElements.filter(id => !document.getElementById(id));
    if (missingElements.length > 0) {
        console.warn('UnifiedContentManager: Missing elements:', missingElements);
        return false;
    }

    try {
        window.unifiedContentManager = new UnifiedContentManager();
        return true;
    } catch (error) {
        console.error('Error initializing UnifiedContentManager:', error);
        return false;
    }
}

window.initializeUnifiedContentManager = initializeUnifiedContentManager;
window.UnifiedContentManager = UnifiedContentManager;

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeUnifiedContentManager, 300);
    });
} else {
    setTimeout(initializeUnifiedContentManager, 300);
}
