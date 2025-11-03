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
        // Mark as initialized to prevent duplicate initialization
        if (blockElement.dataset.mcqInitialized !== 'true') {
            blockElement.dataset.mcqInitialized = 'true';
        }
        
        // Don't load data here - let populateBlockContent handle it
        // This ensures data is loaded after all blocks are rendered and data is properly set
        // Only ensure DOM is ready
        await new Promise(resolve => setTimeout(resolve, 50));
        
        console.log('MultipleChoiceHandler: initialize completed', {
            blockId: block.id,
            hasData: !!block.data,
            hasQuestions: !!(block.data && block.data.questions)
        });
    }

    async loadData(blockElement, block) {
        if (!blockElement) {
            console.warn('MultipleChoiceHandler: loadData called with invalid blockElement');
            return;
        }

        // Get block data from block parameter or from dataset
        let blockData = block?.data;
        if (!blockData && blockElement.dataset.blockData) {
            try {
                blockData = JSON.parse(blockElement.dataset.blockData);
                console.log('MultipleChoiceHandler: Loaded block data from dataset');
            } catch (e) {
                console.warn('MultipleChoiceHandler: Failed to parse block data from dataset', e);
            }
        }

        // If still no block, try to construct it from element
        if (!block && blockElement.dataset.blockId) {
            block = {
                id: blockElement.dataset.blockId,
                type: blockElement.dataset.type || 'multipleChoice',
                data: blockData
            };
        }

        console.log('MultipleChoiceHandler: loadData called', {
            blockId: block?.id || blockElement.dataset.blockId,
            blockType: block?.type || 'multipleChoice',
            hasData: !!blockData,
            hasQuestions: !!(blockData && blockData.questions),
            questionsCount: blockData?.questions?.length || 0,
            dataSource: block?.data ? 'parameter' : (blockElement.dataset.blockData ? 'dataset' : 'none')
        });

        // Wait for mcqManager to be available if not yet ready
        let retries = 0;
        while (!window.mcqManager && retries < 10) {
            await new Promise(resolve => setTimeout(resolve, 50));
            retries++;
        }
        
        // Load existing MCQ data if it exists
        if (blockData && blockData.questions && Array.isArray(blockData.questions) && blockData.questions.length > 0) {
            if (window.mcqManager) {
                try {
                    console.log('MultipleChoiceHandler: Loading MCQ data', {
                        questionsCount: blockData.questions.length,
                        firstQuestion: blockData.questions[0]
                    });
                    await window.mcqManager.loadMcqData(blockElement, blockData);
                    console.log('MultipleChoiceHandler: MCQ data loaded successfully');
                } catch (error) {
                    console.error('MultipleChoiceHandler: Error loading MCQ data:', error);
                }
            } else {
                console.warn('MultipleChoiceHandler: mcqManager not available after waiting');
            }
        } else {
            console.warn('MultipleChoiceHandler: No valid questions data to load', {
                hasBlockData: !!blockData,
                hasQuestions: !!(blockData && blockData.questions),
                isArray: !!(blockData && blockData.questions && Array.isArray(blockData.questions)),
                length: blockData?.questions?.length || 0
            });
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
