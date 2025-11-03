/**
 * Error Finding Block Handler
 * Handles error finding question blocks
 */

class ErrorFindingHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
    }

    canHandle(blockType) {
        return blockType === 'errorFinding';
    }

    render(block) {
        const blockTemplatesContainer = document.getElementById('blockTemplatesContainer');
        if (!blockTemplatesContainer) {
            console.error('ErrorFindingHandler: blockTemplatesContainer not found');
            return null;
        }

        const templatesContainer = blockTemplatesContainer.querySelector('#questionTypeBlockTemplates');
        if (!templatesContainer) {
            console.error('ErrorFindingHandler: questionTypeBlockTemplates not found');
            return null;
        }

        const template = templatesContainer.querySelector('[data-type="errorFinding"]');
        if (!template) {
            console.error('ErrorFindingHandler: ErrorFinding template not found');
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
        // Initialize settings from data
        this.applySettingsToUi(blockElement, block);

        // Initialize CKEditor for error text editor
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (editorEl && typeof QuestionBlockBase !== 'undefined') {
            QuestionBlockBase.initializeCKEditorForBlock(blockElement);
        }

        // Setup error settings listeners
        const settingsInputs = blockElement.querySelectorAll('[data-setting]');
        settingsInputs.forEach(input => {
            input.addEventListener('change', () => {
                const key = input.getAttribute('data-setting');
                let value;
                if (input.type === 'checkbox') {
                    value = input.checked;
                } else {
                    value = input.value;
                }
                block.data = block.data || {};
                block.data[key] = value;
                if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                    this.contentManager.updateHiddenField();
                }
            });
        });

        // Setup event listeners for adding errors
        const addErrorBtn = blockElement.querySelector('[data-action="add-error"]');
        if (addErrorBtn) {
            addErrorBtn.addEventListener('click', () => this.addError(blockElement, block));
        }

        // Initialize existing errors if they exist
        if (block.data && Array.isArray(block.data.errors)) {
            this.renderErrors(blockElement, block);
        }
    }

    addError(blockElement, block) {
        const errorsList = blockElement.querySelector('[data-role="errors-list"]');
        if (!errorsList) return;

        const newError = {
            id: this._generateErrorId(block),
            lineNumber: 1,
            errorText: '',
            explanation: ''
        };
        block.data = block.data || {};
        block.data.errors = block.data.errors || [];
        block.data.errors.push(newError);

        const errorEl = this._createErrorElement(newError, block);
        errorsList.appendChild(errorEl);

        // Focus first input
        const input = errorEl.querySelector('input[type="number"]');
        if (input) input.focus();

        if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
            this.contentManager.updateHiddenField();
        }
    }

    renderErrors(blockElement, block) {
        const errorsList = blockElement.querySelector('[data-role="errors-list"]');
        if (!errorsList) return;
        errorsList.innerHTML = '';

        (block.data.errors || []).forEach(error => {
            const errorEl = this._createErrorElement(error, block);
            errorsList.appendChild(errorEl);
        });
    }

    collectData(blockElement, block) {
        // Collect error text from CKEditor
        if (typeof QuestionBlockBase !== 'undefined') {
            QuestionBlockBase.collectQuestionTextContent(blockElement, block);
        }

        // Collect errors list
        const errorsList = blockElement.querySelector('[data-role="errors-list"]');

        const errors = Array.from(errorsList.querySelectorAll('.error-item')).map(el => {
            const id = el.getAttribute('data-error-id');
            const lineNumberInput = el.querySelector('[data-field="line-number"]');
            const errorTextInput = el.querySelector('[data-field="error-text"]');
            const explanationInput = el.querySelector('[data-field="explanation"]');

            return {
                id: id,
                lineNumber: lineNumberInput ? parseInt(lineNumberInput.value, 10) || 1 : 1,
                errorText: errorTextInput ? errorTextInput.value.trim() : '',
                explanation: explanationInput ? explanationInput.value.trim() : ''
            };
        });

        // Collect settings
        const settings = this._readSettings(blockElement);

        block.data = Object.assign({}, block.data, settings, {
            errors
        });

        return block.data;
    }

    // Helpers
    _generateErrorId(block) {
        const base = (block && block.id) ? block.id : 'block';
        const ts = Date.now();
        const rnd = Math.floor(Math.random() * 100000);
        return `${base}-error-${ts}-${rnd}`;
    }

    _createErrorElement(error, block) {
        const div = document.createElement('div');
        div.className = 'error-item';
        div.setAttribute('data-error-id', error.id);

        div.innerHTML = `
            <div class="error-item-header">
                <div class="error-item-fields">
                    <div class="form-group">
                        <label>شماره خط:</label>
                        <input type="number" class="form-control form-control-sm" 
                               data-field="line-number" 
                               value="${error.lineNumber || 1}" 
                               min="1">
                    </div>
                    <div class="form-group" style="flex: 1;">
                        <label>متن خطا:</label>
                        <input type="text" class="form-control form-control-sm" 
                               data-field="error-text" 
                               value="${this._escape(error.errorText || '')}" 
                               placeholder="متن خطا...">
                    </div>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger" data-action="remove-error" title="حذف">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
            <div class="form-group">
                <label>توضیح:</label>
                <textarea class="form-control form-control-sm" 
                          data-field="explanation" 
                          rows="2" 
                          placeholder="توضیح خطا...">${this._escape(error.explanation || '')}</textarea>
            </div>
        `;

        // Bind events
        const removeBtn = div.querySelector('[data-action="remove-error"]');
        const inputs = div.querySelectorAll('input, textarea');

        inputs.forEach(input => {
            input.addEventListener('input', () => {
                if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                    this.contentManager.updateHiddenField();
                }
            });
        });

        removeBtn.addEventListener('click', () => {
            // Remove from data
            if (block && block.data && Array.isArray(block.data.errors)) {
                block.data.errors = block.data.errors.filter(x => x.id !== error.id);
            }
            div.remove();
            if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                this.contentManager.updateHiddenField();
            }
        });

        return div;
    }

    _readSettings(blockElement) {
        const settings = {};
        const inputs = blockElement.querySelectorAll('[data-setting]');
        inputs.forEach(input => {
            const key = input.getAttribute('data-setting');
            if (!key) return;
            if (input.type === 'checkbox') {
                settings[key] = input.checked;
            } else {
                settings[key] = input.value;
            }
        });
        return settings;
    }

    applySettingsToUi(blockElement, block) {
        if (!block || !block.data) return;
        const inputs = blockElement.querySelectorAll('[data-setting]');
        inputs.forEach(input => {
            const key = input.getAttribute('data-setting');
            if (!key) return;
            if (block.data[key] === undefined) return;
            if (input.type === 'checkbox') {
                input.checked = !!block.data[key];
            } else {
                input.value = block.data[key];
            }
        });
    }

    _escape(text) {
        return String(text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
}

if (typeof window !== 'undefined') {
    window.ErrorFindingHandler = ErrorFindingHandler;
}
