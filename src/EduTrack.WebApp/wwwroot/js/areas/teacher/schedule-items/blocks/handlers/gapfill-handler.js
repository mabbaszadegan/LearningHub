/**
 * Gap Fill Block Handler
 * Handles gap fill question blocks
 */

class GapFillHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
    }

    canHandle(blockType) {
        return blockType === 'gapFill';
    }

    render(block) {
        const blockTemplatesContainer = document.getElementById('blockTemplatesContainer');
        if (!blockTemplatesContainer) {
            console.error('GapFillHandler: blockTemplatesContainer not found');
            return null;
        }

        const templatesContainer = blockTemplatesContainer.querySelector('#questionTypeBlockTemplates');
        if (!templatesContainer) {
            console.error('GapFillHandler: questionTypeBlockTemplates not found');
            return null;
        }

        const template = templatesContainer.querySelector('[data-type="gapFill"]');
        if (!template) {
            console.error('GapFillHandler: GapFill template not found');
            return null;
        }

        const blockElement = template.cloneNode(true);
        // Remove template class that hides the element
        blockElement.classList.remove('content-block-template');
        blockElement.classList.add('content-block', 'question-type-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data || {});
        blockElement.style.display = ''; // Ensure it's visible

        return blockElement;
    }

    initialize(blockElement, block) {
        // Initialize CKEditor for text editor
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (editorEl && typeof QuestionBlockBase !== 'undefined') {
            QuestionBlockBase.initializeCKEditorForBlock(blockElement);
        }

        // Setup gap fill editor
        const insertBlankBtn = blockElement.querySelector('[data-action="gf-insert-blank"]');
        if (insertBlankBtn) {
            insertBlankBtn.addEventListener('click', () => this.insertBlank(blockElement));
        }

        // Render existing gaps
        if (block.data && block.data.gaps) {
            this.renderGaps(blockElement, block);
        }
    }

    insertBlank(blockElement) {
        // Insert blank token in CKEditor
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (editorEl && window.ckeditorManager) {
            const editor = window.ckeditorManager.editors.get(editorEl);
            if (editor) {
                const index = this.getNextGapIndex(blockElement);
                const token = ` [[blank${index}]] `;
                editor.model.change(writer => {
                    const pos = editor.model.document.selection.getFirstPosition();
                    writer.insertText(token, pos);
                });
            }
        }
    }

    getNextGapIndex(blockElement) {
        // Get next available gap index
        return 1; // Simplified - implement proper logic
    }

    renderGaps(blockElement, block) {
        // Render gap items
        // Implementation to be completed
    }

    collectData(blockElement, block) {
        // Collect gap fill data
        // Implementation to be completed
        return block.data;
    }
}

if (typeof window !== 'undefined') {
    window.GapFillHandler = GapFillHandler;
}
