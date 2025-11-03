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

        // Clear any empty state that might have been cloned
        const container = blockElement.querySelector('[data-role="mcq-container"]');
        if (container) {
            const questionsList = container.querySelector('[data-role="mcq-list"]');
            if (questionsList) {
                // Remove empty state if exists (it might be in template)
                const emptyState = questionsList.querySelector('.mcq-empty-state');
                if (emptyState) {
                    emptyState.remove();
                }
            }
        }

        return blockElement;
    }

    async initialize(blockElement, block) {
        // Check if already initialized to prevent duplicate loading
        if (blockElement.dataset.mcqInitialized === 'true') {
            return;
        }
        
        // Mark as initialized
        blockElement.dataset.mcqInitialized = 'true';
        
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
            // Return only fresh collected data, don't merge with old data
            // This prevents stale data from corrupting the saved content
            if (mcqData && mcqData.questions !== undefined) {
                return {
                    questions: mcqData.questions
                };
            }
        }
        
        // Return empty structure if no valid data collected
        return {
            questions: []
        };
    }
}

if (typeof window !== 'undefined') {
    window.MultipleChoiceHandler = MultipleChoiceHandler;
}
