/**
 * Question Block Base Functions
 * Shared functions for question-based content managers (Written, Audio)
 */

const QuestionBlockBase = {
    /**
     * Convert regular block type to question type
     */
    convertToQuestionType(type) {
        if (type.startsWith('question')) {
            return type;
        }
        const typeMap = {
            'text': 'questionText',
            'image': 'questionImage',
            'video': 'questionVideo',
            'audio': 'questionAudio'
        };
        return typeMap[type.toLowerCase()] || type;
    },

    /**
     * Get template type from block type
     */
    getTemplateType(blockType) {
        if (blockType.startsWith('question')) {
            return blockType.replace('question', '').toLowerCase();
        }
        return blockType.toLowerCase();
    },

    /**
     * Initialize CKEditor for a text block
     */
    initializeCKEditorForBlock(blockElement) {
        let attempts = 0;
        const maxAttempts = 5;
        const initEditor = () => {
            attempts++;
            const editorElement = blockElement.querySelector('.ckeditor-editor');
            if (editorElement && window.ckeditorManager) {
                try {
                    window.ckeditorManager.initializeEditor(editorElement);
                } catch (error) {
                    console.error('Error initializing CKEditor:', error);
                }
            } else if (attempts < maxAttempts) {
                setTimeout(initEditor, 100 * attempts);
            }
        };
        setTimeout(initEditor, 150);
    },

    /**
     * Dispatch populate event for block
     */
    dispatchPopulateEvent(eventManager, blockElement, block) {
        const dispatchPopulate = () => {
            eventManager.dispatch('populateBlockContent', {
                blockElement,
                block,
                blockType: block.type
            });
        };

        // For text blocks, dispatch multiple times to ensure CKEditor is ready
        if (block.type === 'text' || block.type === 'questionText') {
            dispatchPopulate(); // Immediate
            setTimeout(dispatchPopulate, 300); // After delay
        } else {
            dispatchPopulate();
        }
    },

    /**
     * Enhance question settings
     */
    enhanceQuestionSettings(blockElement, block) {
        const questionSettings = blockElement.querySelector('.question-settings');
        if (!questionSettings || !window.enhanceQuestionSettings) return;

        if (block.type.startsWith('question')) {
            const pointsInput = questionSettings.querySelector('[data-setting="points"]');
            const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
            if (pointsInput && block.data && block.data.points !== undefined) {
                pointsInput.value = block.data.points;
            }
            if (difficultySelect && block.data && block.data.difficulty) {
                difficultySelect.value = block.data.difficulty;
            }
        }
        window.enhanceQuestionSettings(questionSettings);
    },

    /**
     * Collect question fields from DOM
     */
    collectQuestionFields(blockElement, block) {
        const questionSettings = blockElement.querySelector('.question-settings');
        if (!questionSettings) return;

        const pointsInput = questionSettings.querySelector('[data-setting="points"]');
        if (pointsInput) {
            block.data.points = parseFloat(pointsInput.value) || 1;
        }

        const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
        if (difficultySelect) {
            block.data.difficulty = difficultySelect.value || 'medium';
        }

        const requiredCheckbox = questionSettings.querySelector('[data-setting="isRequired"]');
        if (requiredCheckbox) {
            block.data.isRequired = requiredCheckbox.checked;
        }

        const hintTextarea = blockElement.querySelector('[data-hint="true"]');
        if (hintTextarea) {
            block.data.teacherGuidance = hintTextarea.value || '';
        }
    },

    /**
     * Collect question text content from different editor types
     */
    collectQuestionTextContent(blockElement, block) {
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
};

// Export
if (typeof window !== 'undefined') {
    window.QuestionBlockBase = QuestionBlockBase;
}

