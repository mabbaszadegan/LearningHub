/**
 * Reminder Content Manager
 * Manages reminder content type
 * Extends ContentBuilderBase for block management
 */

class ReminderContentManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'reminderPreview',
            hiddenFieldId: 'reminderContentJson',
            modalId: 'blockTypeModal',
            contentType: 'reminder'
        });
    }

    init() {
        super.init();
        this.setupReminderSpecificListeners();
    }

    setupReminderSpecificListeners() {
        this.eventManager.addListener('insertBlockAbove', (e) => {
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    handleInsertBlockAbove(blockElement) {
        this.insertAboveBlock = blockElement;
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId, 'reminder');
        }
    }

    // Override getContent for reminder content structure
    getContent() {
        this.collectCurrentBlockData();
        return {
            type: 'reminder',
            blocks: this.blocks
        };
    }

    // Collect current data from DOM
    collectCurrentBlockData() {
        this.blocks.forEach(block => {
            const blockElement = this.findBlockElement(block.id);
            if (blockElement) {
                this.collectBlockData(blockElement, block);
            }
        });
    }

    collectBlockData(blockElement, block) {
        // Collect content based on block type
        const ckEditor = blockElement.querySelector('.ckeditor-editor');
        if (ckEditor && window.ckeditorManager) {
            const editorContent = window.ckeditorManager.getEditorContent(ckEditor);
            if (editorContent) {
                block.data.content = editorContent.html;
                block.data.textContent = editorContent.text;
            }
            return;
        }

        const richTextEditor = blockElement.querySelector('.rich-text-editor');
        if (richTextEditor) {
            block.data.content = richTextEditor.innerHTML;
            block.data.textContent = richTextEditor.textContent;
            return;
        }

        const textarea = blockElement.querySelector('textarea');
        if (textarea && !textarea.hasAttribute('data-hint')) {
            block.data.content = textarea.value;
            block.data.textContent = textarea.value;
        }
    }

    // Override loadExistingContent for reminder structure
    loadExistingContent() {
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            return;
        }

        try {
            this.isLoadingExistingContent = true;
            const data = JSON.parse(hiddenFieldValue);

            const blocks = data.blocks || [];
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
            }, 500);

            this.isLoadingExistingContent = false;

            setTimeout(() => {
                this.updatePreview();
            }, 800);

        } catch (error) {
            console.error('ReminderContentManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    // collectContentData is inherited from ContentBuilderBase
}

// Initialize when DOM is ready
function initializeReminderContentManager() {
    if (window.reminderContentManager) {
        return;
    }

    const requiredElements = [
        'contentBlocksList',
        'emptyBlocksState',
        'reminderPreview',
        'reminderContentJson',
        'contentBlockTemplates'
    ];

    const missingElements = requiredElements.filter(id => !document.getElementById(id));
    if (missingElements.length > 0) {
        console.warn('ReminderContentManager: Missing elements:', missingElements);
        return false;
    }

    try {
        window.reminderContentManager = new ReminderContentManager();
        return true;
    } catch (error) {
        console.error('Error initializing ReminderContentManager:', error);
        return false;
    }
}

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeReminderContentManager, 300);
    });
} else {
    setTimeout(initializeReminderContentManager, 300);
}

// Global access
window.initializeReminderContentManager = initializeReminderContentManager;

// Export
if (typeof window !== 'undefined') {
    window.ReminderContentManager = ReminderContentManager;
}

