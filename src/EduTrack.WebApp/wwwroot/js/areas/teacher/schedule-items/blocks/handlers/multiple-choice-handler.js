/**
 * Multiple Choice Block Handler
 * Handles multiple choice question blocks
 */

class MultipleChoiceHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
    }

    canHandle(blockType) {
        return blockType === 'multipleChoice';
    }

    render(block) {
        const blockTemplatesContainer = document.getElementById('blockTemplatesContainer');
        if (!blockTemplatesContainer) {
            console.error('MultipleChoiceHandler: blockTemplatesContainer not found');
            return null;
        }

        const templatesContainer = blockTemplatesContainer.querySelector('#questionTypeBlockTemplates');
        if (!templatesContainer) {
            console.error('MultipleChoiceHandler: questionTypeBlockTemplates not found');
            return null;
        }

        const template = templatesContainer.querySelector('[data-type="multipleChoice"]');
        if (!template) {
            console.error('MultipleChoiceHandler: MultipleChoice template not found');
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

    async initialize(blockElement, block) {
        // Load existing MCQ data if it exists
        if (block.data && block.data.questions && window.mcqManager) {
            try {
                await window.mcqManager.loadMcqData(blockElement, block.data);
            } catch (error) {
                console.error('Error loading MCQ data:', error);
            }
        }
    }

    collectData(blockElement, block) {
        // Collect MCQ questions data using MCQ Manager
        if (window.mcqManager) {
            const mcqData = window.mcqManager.collectMcqData(blockElement);
            return {
                ...block.data,
                ...mcqData
            };
        }
        
        return block.data || {};
    }
}

if (typeof window !== 'undefined') {
    window.MultipleChoiceHandler = MultipleChoiceHandler;
}
