/**
 * Multiple Choice Content Manager
 * Manages multiple choice content type
 * Each block can have one or more multiple choice questions
 */

class MultipleChoiceContentManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'writtenPreview',
            hiddenFieldId: 'multipleChoiceContentJson',
            modalId: 'blockTypeModal',
            contentType: 'multipleChoice'
        });

        this.mcqQuestionCounter = 1;
        window.multipleChoiceMode = true;
    }

    init() {
        super.init();
        this.setupMultipleChoiceSpecificListeners();
    }

    setupMultipleChoiceSpecificListeners() {
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

    // Override renderBlock for multiple choice specific handling
    renderBlock(block) {
        const templateType = QuestionBlockBase.getTemplateType(block.type);

        if (!this.blocksList) {
            console.error('MultipleChoiceContentManager: blocksList not available');
            return null;
        }

        const templatesContainer = document.getElementById('questionBlockTemplates');
        if (!templatesContainer) {
            console.error('MultipleChoiceContentManager: questionBlockTemplates not found');
            return null;
        }

        let template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
        if (!template) {
            console.error('MultipleChoiceContentManager: Template not found for type:', templateType);
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

        // Attach MCQ questions editor
        setTimeout(() => {
            this.attachMcqQuestions(blockElement, block);
        }, 300);

        // Enhance question settings
        setTimeout(() => {
            QuestionBlockBase.enhanceQuestionSettings(blockElement, block);
        }, 300);

        return blockElement;
    }

    // Attach multiple choice questions to block
    attachMcqQuestions(blockElement, block) {
        // Check if already attached
        if (blockElement.querySelector('[data-role="mcq-container"]')) {
            return;
        }

        // Initialize MCQ questions array
        if (!Array.isArray(block.data.mcqQuestions)) {
            block.data.mcqQuestions = [];
        }

        // Load MCQ container from template
        this.loadMcqContainer(blockElement, block);
    }

    // Load MCQ container from partial view
    loadMcqContainer(blockElement, block) {
        const template = document.getElementById('mcqQuestionsContainerTemplate');
        if (!template) {
            console.error('MultipleChoiceContentManager: MCQ container template not found');
            return;
        }

        const mcqContainer = template.cloneNode(true);
        mcqContainer.id = '';
        mcqContainer.style.display = '';

        // Find insertion point (after question settings)
        const questionSettings = blockElement.querySelector('.question-settings');
        let insertPoint = questionSettings?.nextElementSibling || blockElement.querySelector('.block-content');
        if (insertPoint) {
            insertPoint.parentNode.insertBefore(mcqContainer, insertPoint);
        } else {
            blockElement.appendChild(mcqContainer);
        }

        // Bind add question button
        const addBtn = mcqContainer.querySelector('[data-action="mcq-add-question"]');
        if (addBtn) {
            addBtn.addEventListener('click', () => {
                this.addMcqQuestion(blockElement, block);
            });
        }

        // Render existing MCQ questions
        this.renderMcqQuestions(blockElement, block);
    }

    // Add new MCQ question
    addMcqQuestion(blockElement, block) {
        const questionId = `mcq-${block.id}-${this.mcqQuestionCounter++}`;
        const newQuestion = {
            id: questionId,
            stem: '',
            answerType: 'single',
            randomize: false,
            options: [
                { index: 0, text: '', correct: false },
                { index: 1, text: '', correct: false }
            ]
        };

        if (!Array.isArray(block.data.mcqQuestions)) {
            block.data.mcqQuestions = [];
        }
        block.data.mcqQuestions.push(newQuestion);
        this.renderMcqQuestions(blockElement, block);
        this.updateHiddenField();
    }

    // Render MCQ questions
    renderMcqQuestions(blockElement, block) {
        const listContainer = blockElement.querySelector('[data-role="mcq-list"]');
        if (!listContainer) return;

        // Clear empty state if exists
        const emptyState = listContainer.querySelector('.mcq-empty-state');
        if (emptyState) {
            emptyState.remove();
        }

        if (!block.data.mcqQuestions || block.data.mcqQuestions.length === 0) {
            const emptyStateDiv = document.createElement('div');
            emptyStateDiv.className = 'mcq-empty-state';
            emptyStateDiv.innerHTML = `
                <i class="fas fa-inbox"></i>
                <p>هنوز سوال چندگزینه‌ای تعریف نشده است</p>
            `;
            listContainer.appendChild(emptyStateDiv);
            return;
        }

        // Load question items from template
        const questionTemplate = document.getElementById('mcqQuestionItemTemplate');
        if (!questionTemplate) {
            console.error('MultipleChoiceContentManager: MCQ question template not found');
            return;
        }

        block.data.mcqQuestions.forEach((question, index) => {
            const questionElement = this.createMcqQuestionElement(question, blockElement, block, index, questionTemplate);
            listContainer.appendChild(questionElement);
        });
    }

    // Create MCQ question element from template
    createMcqQuestionElement(question, blockElement, block, index, template) {
        const questionDiv = template.cloneNode(true);
        questionDiv.id = '';
        questionDiv.style.display = '';
        questionDiv.dataset.questionId = question.id;
        questionDiv.querySelector('.mcq-question-number').textContent = `سوال ${index + 1}`;

        // Populate question data
        const stemInput = questionDiv.querySelector('[data-role="mcq-stem"]');
        if (stemInput) {
            stemInput.value = question.stem || '';
            stemInput.addEventListener('input', (e) => {
                question.stem = e.target.value;
                this.updateHiddenField();
            });
        }

        const answerTypeSelect = questionDiv.querySelector('[data-role="mcq-answer-type"]');
        if (answerTypeSelect) {
            answerTypeSelect.value = question.answerType || 'single';
            answerTypeSelect.addEventListener('change', (e) => {
                question.answerType = e.target.value;
                if (question.answerType === 'single') {
                    // Reset to single correct answer
                    question.options.forEach((opt, idx) => {
                        opt.correct = idx === 0 && question.options[0].correct;
                    });
                }
                this.renderMcqOptions(questionDiv, question);
                this.updateHiddenField();
            });
        }

        const randomizeCheckbox = questionDiv.querySelector('[data-role="mcq-randomize"]');
        if (randomizeCheckbox) {
            randomizeCheckbox.checked = question.randomize || false;
            randomizeCheckbox.addEventListener('change', (e) => {
                question.randomize = e.target.checked;
                this.updateHiddenField();
            });
        }

        // Bind remove button
        const removeBtn = questionDiv.querySelector('[data-action="mcq-remove"]');
        if (removeBtn) {
            removeBtn.addEventListener('click', () => {
                block.data.mcqQuestions = block.data.mcqQuestions.filter(q => q.id !== question.id);
                this.renderMcqQuestions(blockElement, block);
                this.updateHiddenField();
            });
        }

        // Bind add option button
        const addOptionBtn = questionDiv.querySelector('[data-action="mcq-add-option"]');
        if (addOptionBtn) {
            addOptionBtn.addEventListener('click', () => {
                const newIndex = question.options.length;
                question.options.push({ index: newIndex, text: '', correct: false });
                this.renderMcqOptions(questionDiv, question);
                this.updateHiddenField();
            });
        }

        // Render options
        this.renderMcqOptions(questionDiv, question);

        return questionDiv;
    }

    // Render MCQ options
    renderMcqOptions(questionDiv, question) {
        const optionsList = questionDiv.querySelector('[data-role="mcq-options"]');
        if (!optionsList) return;

        optionsList.innerHTML = '';

        if (!question.options || question.options.length === 0) {
            question.options = [
                { index: 0, text: '', correct: false },
                { index: 1, text: '', correct: false }
            ];
        }

        // Load option template
        const optionTemplate = document.getElementById('mcqOptionItemTemplate');
        if (!optionTemplate) {
            console.error('MultipleChoiceContentManager: MCQ option template not found');
            return;
        }

        const isSingle = question.answerType === 'single';
        const questionId = questionDiv.dataset.questionId || question.id;

        question.options.forEach((option, index) => {
            const optionElement = this.createMcqOptionElement(option, question, questionDiv, index, isSingle, questionId, optionTemplate);
            optionsList.appendChild(optionElement);
        });
    }

    // Create MCQ option element from template
    createMcqOptionElement(option, question, questionDiv, index, isSingle, questionId, template) {
        const optionDiv = template.cloneNode(true);
        optionDiv.id = '';
        optionDiv.style.display = '';
        optionDiv.dataset.optionIndex = option.index;

        // Update input type and name
        const correctInput = optionDiv.querySelector('[data-role="mcq-option-correct"]');
        if (correctInput) {
            correctInput.type = isSingle ? 'radio' : 'checkbox';
            correctInput.name = `mcq-${questionId}`;
            correctInput.checked = option.correct || false;
            correctInput.addEventListener('change', (e) => {
                if (isSingle) {
                    // Uncheck all other options
                    questionDiv.querySelectorAll(`input[name="mcq-${questionId}"]`).forEach(inp => {
                        inp.checked = false;
                    });
                    e.target.checked = true;
                    question.options.forEach(opt => {
                        opt.correct = opt.index === option.index;
                    });
                } else {
                    option.correct = e.target.checked;
                }
                this.updateHiddenField();
            });
        }

        const textInput = optionDiv.querySelector('[data-role="mcq-option-text"]');
        if (textInput) {
            textInput.value = option.text || '';
            textInput.addEventListener('input', (e) => {
                option.text = e.target.value;
                this.updateHiddenField();
            });
        }

        const removeBtn = optionDiv.querySelector('[data-action="remove-option"]');
        if (removeBtn) {
            if (question.options.length > 2) {
                removeBtn.addEventListener('click', () => {
                    question.options = question.options.filter(opt => opt.index !== option.index);
                    question.options.forEach((opt, idx) => {
                        opt.index = idx;
                    });
                    this.renderMcqOptions(questionDiv, question);
                    this.updateHiddenField();
                });
            } else {
                removeBtn.style.display = 'none';
            }
        }

        return optionDiv;
    }

    // Override getContent for multiple choice structure
    getContent() {
        this.collectCurrentBlockData();
        return {
            type: 'multipleChoice',
            blocks: this.blocks
        };
    }

    // Collect current data from DOM
    collectCurrentBlockData() {
        this.blocks.forEach(block => {
            const blockElement = this.findBlockElement(block.id);
            if (blockElement) {
                QuestionBlockBase.collectQuestionFields(blockElement, block);
                QuestionBlockBase.collectQuestionTextContent(blockElement, block);
                this.collectMcqQuestions(blockElement, block);
            }
        });
    }

    // Collect MCQ questions from DOM
    collectMcqQuestions(blockElement, block) {
        if (!block.data.mcqQuestions) {
            block.data.mcqQuestions = [];
        }

        const mcqList = blockElement.querySelector('[data-role="mcq-list"]');
        if (!mcqList) return;

        const questionItems = mcqList.querySelectorAll('.mcq-question-item');
        block.data.mcqQuestions = Array.from(questionItems).map((item, index) => {
            const questionId = item.dataset.questionId || `mcq-${block.id}-${index + 1}`;
            const stemInput = item.querySelector('[data-role="mcq-stem"]');
            const answerTypeSelect = item.querySelector('[data-role="mcq-answer-type"]');
            const randomizeCheckbox = item.querySelector('[data-role="mcq-randomize"]');
            const optionsList = item.querySelector('[data-role="mcq-options"]');
            
            const question = {
                id: questionId,
                stem: stemInput ? stemInput.value : '',
                answerType: answerTypeSelect ? answerTypeSelect.value : 'single',
                randomize: randomizeCheckbox ? randomizeCheckbox.checked : false,
                options: []
            };

            // Collect options
            if (optionsList) {
                const optionItems = optionsList.querySelectorAll('.mcq-option-item');
                question.options = Array.from(optionItems).map((optItem, optIndex) => {
                    const textInput = optItem.querySelector('[data-role="mcq-option-text"]');
                    const correctInput = optItem.querySelector('[data-role="mcq-option-correct"]');
                    return {
                        index: optIndex,
                        text: textInput ? textInput.value : '',
                        correct: correctInput ? correctInput.checked : false
                    };
                });
            }

            return question;
        });
    }

    // Override loadExistingContent
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

            this.blocks = blocks.map(block => ({
                ...block,
                data: {
                    ...block.data,
                    mcqQuestions: block.data.mcqQuestions || []
                }
            }));

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
            console.error('MultipleChoiceContentManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    // collectContentData is inherited from ContentBuilderBase
}

// Initialize when DOM is ready
function initializeMultipleChoiceContentManager() {
    if (window.multipleChoiceContentManager) {
        return;
    }

    const requiredElements = [
        'contentBlocksList',
        'emptyBlocksState',
        'writtenPreview',
        'multipleChoiceContentJson',
        'questionBlockTemplates'
    ];

    const missingElements = requiredElements.filter(id => !document.getElementById(id));
    if (missingElements.length > 0) {
        console.warn('MultipleChoiceContentManager: Missing elements:', missingElements);
        return false;
    }

    try {
        window.multipleChoiceContentManager = new MultipleChoiceContentManager();
        return true;
    } catch (error) {
        console.error('Error initializing MultipleChoiceContentManager:', error);
        return false;
    }
}

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeMultipleChoiceContentManager, 300);
    });
} else {
    setTimeout(initializeMultipleChoiceContentManager, 300);
}

// Global access
window.initializeMultipleChoiceContentManager = initializeMultipleChoiceContentManager;

// Export
if (typeof window !== 'undefined') {
    window.MultipleChoiceContentManager = MultipleChoiceContentManager;
}

