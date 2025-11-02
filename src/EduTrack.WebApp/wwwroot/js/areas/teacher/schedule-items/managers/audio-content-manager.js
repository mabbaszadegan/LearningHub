/**
 * Audio Content Manager
 * Manages audio content type (similar to written content)
 * Extends ContentBuilderBase for block management
 */

class AudioContentManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'writtenPreview', // Uses same preview structure
            hiddenFieldId: 'writtenContentJson', // Uses same hidden field
            modalId: 'blockTypeModal',
            contentType: 'audio'
        });
    }

    init() {
        super.init();
        this.setupAudioSpecificListeners();
    }

    setupAudioSpecificListeners() {
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
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId, 'written');
        }
    }

    // Override renderBlock for audio-specific handling
    renderBlock(block) {
        const templateType = QuestionBlockBase.getTemplateType(block.type);

        if (!this.blocksList) {
            console.error('AudioContentManager: blocksList not available');
            return null;
        }

        const templatesContainer = document.getElementById('questionBlockTemplates');
        if (!templatesContainer) {
            console.error('AudioContentManager: questionBlockTemplates not found');
            return null;
        }

        let template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
        if (!template) {
            console.error('AudioContentManager: Template not found for type:', templateType);
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

        // Enhance question settings
        setTimeout(() => {
            QuestionBlockBase.enhanceQuestionSettings(blockElement, block);
        }, 150);

        return blockElement;
    }

    // Override getContent for audio content structure
    getContent() {
        this.collectCurrentBlockData();
        return {
            type: 'audio',
            questionBlocks: this.blocks
        };
    }

    // Collect current data from DOM
    collectCurrentBlockData() {
        this.blocks.forEach(block => {
            const blockElement = this.findBlockElement(block.id);
            if (blockElement) {
                QuestionBlockBase.collectQuestionFields(blockElement, block);
                QuestionBlockBase.collectQuestionTextContent(blockElement, block);
            }
        });
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
            console.error('AudioContentManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    // collectContentData is inherited from ContentBuilderBase
}

// Initialize when DOM is ready
function initializeAudioContentManager() {
    if (window.audioContentManager) {
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
        console.warn('AudioContentManager: Missing elements:', missingElements);
        return false;
    }

    try {
        window.audioContentManager = new AudioContentManager();
        return true;
    } catch (error) {
        console.error('Error initializing AudioContentManager:', error);
        return false;
    }
}

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeAudioContentManager, 300);
    });
} else {
    setTimeout(initializeAudioContentManager, 300);
}

// Global access
window.initializeAudioContentManager = initializeAudioContentManager;

// Export
if (typeof window !== 'undefined') {
    window.AudioContentManager = AudioContentManager;
}

