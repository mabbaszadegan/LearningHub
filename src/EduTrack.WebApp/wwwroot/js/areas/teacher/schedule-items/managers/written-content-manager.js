/**
 * Written Content Manager
 * Manages written content type (Writing, Audio, MultipleChoice)
 * Extends ContentBuilderBase for block management
 */

class WrittenContentManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'writtenPreview',
            hiddenFieldId: 'writtenContentJson',
            modalId: 'blockTypeModal',
            contentType: 'written'
        });
    }

    init() {
        super.init();
        this.setupWrittenSpecificListeners();
    }

    setupWrittenSpecificListeners() {
        this.eventManager.addListener('insertBlockAbove', (e) => {
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    // Override addBlock to convert regular types to question types
    addBlock(type) {
        const questionType = QuestionBlockBase.convertToQuestionType(type);
        super.addBlock(questionType);
    }

    handleInsertBlockAbove(blockElement) {
        this.insertAboveBlock = blockElement;
        const itemTypeName = this.getItemTypeName();
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId, itemTypeName);
        }
    }

    getItemTypeName() {
        if (window.gapFillMode || this.config.contentType === 'gapfill') {
            return 'gapfill';
        } else if (window.multipleChoiceMode || this.config.contentType === 'multipleChoice') {
            return 'multiplechoice';
        }
        return 'written';
    }

    // Override renderBlock for written-specific handling
    renderBlock(block) {
        const templateType = QuestionBlockBase.getTemplateType(block.type);

        if (!this.blocksList) {
            console.error('WrittenContentManager: blocksList not available');
            return null;
        }

        const templatesContainer = document.getElementById('questionBlockTemplates');
        if (!templatesContainer) {
            console.error('WrittenContentManager: questionBlockTemplates not found');
            return null;
        }

        let template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
        if (!template) {
            console.error('WrittenContentManager: Template not found for type:', templateType);
            return null;
        }

        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data);
        blockElement.dataset.type = block.type;
        blockElement.dataset.templateType = templateType;

        if (block.type.startsWith('question')) {
            this.configureQuestionBlock(blockElement, block);
        }

        this.addDirectEventListeners(blockElement);

        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }

        // Initialize CKEditor for text blocks
        if (block.type === 'text' || block.type === 'questionText') {
            QuestionBlockBase.initializeCKEditorForBlock(blockElement);
        }

        // Dispatch populate event
        QuestionBlockBase.dispatchPopulateEvent(this.eventManager, blockElement, block);

        // Handle mode-specific attachments
        if (window.multipleChoiceMode) {
            this.attachMcqEditor(blockElement, block);
        }
        if (window.gapFillMode) {
            setTimeout(() => {
                this.attachGapFillEditor(blockElement, block);
            }, 150);
        }

        // Enhance question settings
        setTimeout(() => {
            QuestionBlockBase.enhanceQuestionSettings(blockElement, block);
        }, 150);

        return blockElement;
    }


    // Override getContent for written content structure
    getContent() {
        this.collectCurrentBlockData();

        if (window.gapFillMode || this.config.contentType === 'gapfill') {
            return { type: 'gapfill', blocks: this.blocks };
        }
        if (window.multipleChoiceMode || this.config.contentType === 'multipleChoice') {
            return { type: 'multipleChoice', blocks: this.blocks };
        }
        return {
            type: this.config.contentType || 'written',
            questionBlocks: this.blocks
        };
    }

    // Override loadExistingContent for questionBlocks structure
    loadExistingContent() {
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            return;
        }

        try {
            this.isLoadingExistingContent = true;
            const data = JSON.parse(hiddenFieldValue);

            // Handle questionBlocks structure
            const blocks = data.questionBlocks || data.blocks || [];
            if (!Array.isArray(blocks) || blocks.length === 0) {
                this.isLoadingExistingContent = false;
                return;
            }

            // Clear existing blocks
            if (this.blocksList) {
                const existingBlocks = this.blocksList.querySelectorAll('.content-block');
                existingBlocks.forEach(block => block.remove());
            }

            this.blocks = blocks;
            if (this.blocks.length > 0) {
                this.nextBlockId = Math.max(...this.blocks.map(b => parseInt(b.id.split('-')[1]) || 0)) + 1;
            }

            // Render blocks
            this.blocks.forEach(block => {
                this.renderBlock(block);
            });

            this.updateEmptyState();

            // Populate content after rendering
            setTimeout(() => {
                this.populateBlockContent();
            }, 1000);

            setTimeout(() => {
                this.populateBlockContent();
            }, 2000);

            this.isLoadingExistingContent = false;

            setTimeout(() => {
                this.updatePreview();
            }, 800);

        } catch (error) {
            console.error('WrittenContentManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    collectCurrentBlockData() {
        this.blocks.forEach(block => {
            const blockElement = this.findBlockElement(block.id);
            if (blockElement) {
                this.collectQuestionFields(blockElement, block);
            }
        });
    }

    collectQuestionFields(blockElement, block) {
        QuestionBlockBase.collectQuestionFields(blockElement, block);
        QuestionBlockBase.collectQuestionTextContent(blockElement, block);
    }

    // Multiple Choice specific methods
    attachMcqEditor(blockElement, block) {
        // Implementation from original file
        if (!Array.isArray(block.data.mcQuestions)) {
            block.data.mcQuestions = [];
        }
        // ... (rest of MCQ editor attachment code)
    }

    // Gap Fill specific methods
    attachGapFillEditor(blockElement, block) {
        if ((block.type || '').toLowerCase() !== 'questiontext') return;
        // ... (rest of gap fill editor attachment code)
    }
}

// Initialize when DOM is ready
function initializeWrittenContentManager() {
    if (window.writtenContentManager) {
        return;
    }

    const requiredElements = [
        'contentBlocksList',
        'emptyBlocksState',
        'writtenPreview',
        'writtenContentJson',
        'questionBlockTemplates'
    ];

    const missingElements = requiredElements.filter(id => !document.getElementById(id));
    if (missingElements.length > 0) {
        console.warn('WrittenContentManager: Missing elements:', missingElements);
        return false;
    }

    try {
        window.writtenContentManager = new WrittenContentManager();
        return true;
    } catch (error) {
        console.error('Error initializing WrittenContentManager:', error);
        return false;
    }
}

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeWrittenContentManager, 300);
    });
} else {
    setTimeout(initializeWrittenContentManager, 300);
}

// Global access
window.initializeWrittenContentManager = initializeWrittenContentManager;

// Export
if (typeof window !== 'undefined') {
    window.WrittenContentManager = WrittenContentManager;
}

