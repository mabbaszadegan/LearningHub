/**
 * Gap Fill Content Manager
 * Manages gap fill content with minimal design
 * Each block can have one or more multiple choice questions
 */

class GapFillContentManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'gapFillPreview',
            hiddenFieldId: 'gapFillContentJson',
            modalId: 'blockTypeModal',
            contentType: 'gapfill'
        });

        // Gap Fill only has gaps, not MCQ questions
    }

    init() {
        super.init();
        this.setupGapFillSpecificListeners();
        window.gapFillMode = true;
    }

    setupGapFillSpecificListeners() {
        this.eventManager.addListener('insertBlockAbove', (e) => {
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    // Override addBlock to only allow questionText for gap fill
    addBlock(type) {
        // Gap fill only supports text blocks
        if (type !== 'text' && type !== 'questionText') {
            type = 'questionText';
        }
        super.addBlock('questionText');
    }

    handleInsertBlockAbove(blockElement) {
        this.insertAboveBlock = blockElement;
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId, 'gapfill');
        }
    }

    // Override renderBlock for gap fill specific handling
    renderBlock(block) {
        if (!this.blocksList) {
            console.error('GapFillContentManager: blocksList not available');
            return null;
        }

        const templatesContainer = document.getElementById('questionBlockTemplates');
        if (!templatesContainer) {
            console.error('GapFillContentManager: questionBlockTemplates not found');
            return null;
        }

        // Gap fill only uses text blocks
        let template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="text"]`);
        if (!template) {
            console.error('GapFillContentManager: Text template not found');
            return null;
        }

        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block', 'gapfill-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data);
        blockElement.dataset.type = 'questionText';
        blockElement.dataset.templateType = 'text';

        // Configure as question block
        this.configureQuestionBlock(blockElement, block);

        this.addDirectEventListeners(blockElement);

        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }

        // Initialize CKEditor
        QuestionBlockBase.initializeCKEditorForBlock(blockElement);

        // Attach gap fill editor
        setTimeout(() => {
            this.attachGapFillEditor(blockElement, block);
            this.setupCKEditorSync(blockElement);
        }, 400);

        // Dispatch populate event
        QuestionBlockBase.dispatchPopulateEvent(this.eventManager, blockElement, block);

        // Enhance question settings
        setTimeout(() => {
            QuestionBlockBase.enhanceQuestionSettings(blockElement, block);
        }, 200);

        return blockElement;
    }

    // Attach gap fill editor to block
    attachGapFillEditor(blockElement, block) {
        if ((block.type || '').toLowerCase() !== 'questiontext') return;

        // Check if already attached
        if (blockElement.querySelector('[data-role="gf-container"]')) {
            return;
        }

        // Load gap fill editor from template
        this.loadGapFillEditor(blockElement, block);
    }

    // Load gap fill editor from partial view template
    loadGapFillEditor(blockElement, block) {
        const template = document.getElementById('gapFillEditorTemplate');
        if (!template) {
            console.error('GapFillContentManager: Gap fill editor template not found');
            return;
        }

        const gfContainer = template.cloneNode(true);
        gfContainer.id = '';
        gfContainer.style.display = '';

        // Find insertion point (after question settings)
        const questionSettings = blockElement.querySelector('.question-settings');
        let insertPoint = questionSettings?.nextElementSibling || blockElement.querySelector('.block-content');
        if (insertPoint) {
            insertPoint.parentNode.insertBefore(gfContainer, insertPoint);
        } else {
            blockElement.appendChild(gfContainer);
        }

        // Initialize gap fill data
        if (!Array.isArray(block.data.gaps)) {
            block.data.gaps = [];
        }
        if (!block.data.answerType) {
            block.data.answerType = 'exact';
        }
        if (typeof block.data.caseSensitive !== 'boolean') {
            block.data.caseSensitive = false;
        }

        // Bind events
        const insertBlankBtn = gfContainer.querySelector('[data-action="gf-insert-blank"]');
        if (insertBlankBtn) {
            insertBlankBtn.addEventListener('click', () => this.insertGapBlankToken(blockElement));
        }

        const answerTypeSelect = gfContainer.querySelector('[data-role="gf-answer-type"]');
        if (answerTypeSelect) {
            answerTypeSelect.value = block.data.answerType;
            answerTypeSelect.addEventListener('change', (e) => {
                block.data.answerType = e.target.value;
                this.updateHiddenField();
            });
        }

        const caseCheckbox = gfContainer.querySelector('[data-role="gf-case"]');
        if (caseCheckbox) {
            caseCheckbox.checked = block.data.caseSensitive;
            caseCheckbox.addEventListener('change', (e) => {
                block.data.caseSensitive = e.target.checked;
                this.updateHiddenField();
            });
        }

        // Render gaps
        this.renderGapList(blockElement, block);
    }

    // REMOVED: attachMcqQuestions - Gap Fill should only have gaps, not MCQ questions


    // Setup CKEditor sync for gap fill
    setupCKEditorSync(blockElement) {
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (!editorEl || !window.ckeditorManager) return;

        // Wait for editor to be ready
        const waitForEditor = () => {
            const editor = window.ckeditorManager.editors.get(editorEl);
            if (editor) {
                // Listen for content changes
                editor.model.document.on('change:data', () => {
                    this.syncGapsFromContent(blockElement);
                });
            } else {
                setTimeout(waitForEditor, 100);
            }
        };
        setTimeout(waitForEditor, 200);
    }

    // Insert gap blank token in CKEditor
    insertGapBlankToken(blockElement) {
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (editorEl && window.ckeditorManager) {
            const editor = window.ckeditorManager.editors.get(editorEl);
            const index = this.nextGapIndex(blockElement);
            const token = ` [[blank${index}]] `;
            if (editor) {
                editor.model.change(writer => {
                    const pos = editor.model.document.selection.getFirstPosition();
                    writer.insertText(token, pos);
                });
                editor.editing.view.focus();
                // Sync will happen automatically via change:data listener
            }
        }
    }

    // Get next gap index
    nextGapIndex(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const block = this.findBlock(blockId);
        if (!block || !Array.isArray(block.data.gaps)) {
            return 1;
        }
        const used = new Set(block.data.gaps.map(g => g.index));
        let i = 1;
        while (used.has(i)) i++;
        return i;
    }

    // Sync gaps from content text
    syncGapsFromContent(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const block = this.findBlock(blockId);
        if (!block) return;

        const editorEl = blockElement.querySelector('.ckeditor-editor');
        let content = '';
        if (editorEl && window.ckeditorManager) {
            const editorContent = window.ckeditorManager.getEditorContent(editorEl);
            content = editorContent ? editorContent.html : '';
        } else {
            const textarea = blockElement.querySelector('textarea');
            content = textarea ? textarea.value : '';
        }

        // Extract gap indices from content
        const tokens = [...String(content).matchAll(/\[\[blank(\d+)\]\]/gi)]
            .map(m => parseInt(m[1], 10))
            .filter(n => !isNaN(n));
        const uniqueIndices = Array.from(new Set(tokens)).sort((a, b) => a - b);

        // Update gaps array
        const existingGaps = new Map((block.data.gaps || []).map(g => [g.index, g]));
        block.data.gaps = uniqueIndices.map(index => {
            return existingGaps.get(index) || {
                index,
                correctAnswer: '',
                alternativeAnswers: [],
                hint: ''
            };
        });

        this.renderGapList(blockElement, block);
        this.updateHiddenField();
    }

    // Render gap list
    renderGapList(blockElement, block) {
        const gapsList = blockElement.querySelector('[data-role="gf-gaps"]');
        if (!gapsList) return;

        // Clear empty state if exists
        const emptyState = gapsList.querySelector('.gaps-empty-state');
        if (emptyState) {
            emptyState.remove();
        }

        gapsList.innerHTML = '';

        if (!block.data.gaps || block.data.gaps.length === 0) {
            // Empty state is already in the template, just show message
            const emptyStateDiv = document.createElement('div');
            emptyStateDiv.className = 'gaps-empty-state';
            emptyStateDiv.innerHTML = '<i class="fas fa-info-circle"></i><p>برای این بلاک هنوز جای‌خالی تعریف نشده است</p><small>از دکمه "درج جای‌خالی" استفاده کنید</small>';
            gapsList.appendChild(emptyStateDiv);
            return;
        }

        // Load gap item template
        const gapTemplate = document.getElementById('gapItemTemplate');
        if (!gapTemplate) {
            console.error('GapFillContentManager: Gap item template not found');
            return;
        }

        block.data.gaps.forEach(gap => {
            const gapElement = this.createGapElement(gap, blockElement, block, gapTemplate);
            gapsList.appendChild(gapElement);
        });
    }

    // Create gap element from template
    createGapElement(gap, blockElement, block, template) {
        const gapDiv = template.cloneNode(true);
        gapDiv.id = '';
        gapDiv.style.display = '';
        gapDiv.dataset.gapIndex = gap.index;

        // Update gap number
        const gapNumber = gapDiv.querySelector('.gap-item-number');
        if (gapNumber) {
            gapNumber.textContent = `جای‌خالی ${gap.index}`;
        }

        // Populate gap data
        const correctInput = gapDiv.querySelector('[data-role="gf-correct"]');
        if (correctInput) {
            correctInput.value = gap.correctAnswer || '';
            correctInput.addEventListener('input', (e) => {
                gap.correctAnswer = e.target.value;
                this.updateHiddenField();
            });
        }

        const altsInput = gapDiv.querySelector('[data-role="gf-alts"]');
        if (altsInput) {
            altsInput.value = (gap.alternativeAnswers || []).join('، ');
            altsInput.addEventListener('input', (e) => {
                const value = e.target.value.trim();
                gap.alternativeAnswers = value ? value.split(/[،,]/).map(s => s.trim()).filter(s => s) : [];
                this.updateHiddenField();
            });
        }

        const hintInput = gapDiv.querySelector('[data-role="gf-hint"]');
        if (hintInput) {
            hintInput.value = gap.hint || '';
            hintInput.addEventListener('input', (e) => {
                gap.hint = e.target.value;
                this.updateHiddenField();
            });
        }

        return gapDiv;
    }

    // Escape HTML
    escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str || '';
        return div.innerHTML;
    }

    // Override getContent for gap fill structure
    getContent() {
        this.collectCurrentBlockData();
        return {
            type: 'gapfill',
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
                // Sync gaps from content and collect MCQ questions
                this.syncGapsFromContent(blockElement);
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
            const stemInput = item.querySelector('.mcq-stem-input');
            const answerTypeSelect = item.querySelector('.mcq-answer-type-select');
            const randomizeCheckbox = item.querySelector('.mcq-randomize-checkbox');
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
                    const textInput = optItem.querySelector('.mcq-option-text');
                    const correctInput = optItem.querySelector('.mcq-option-correct');
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
                type: 'questionText', // Force questionText type
                data: {
                    ...block.data,
                    gaps: block.data.gaps || [],
                    mcqQuestions: block.data.mcqQuestions || [],
                    answerType: block.data.answerType || 'exact',
                    caseSensitive: block.data.caseSensitive || false
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

            this.isLoadingExistingContent = false;

            setTimeout(() => {
                this.updatePreview();
            }, 800);

        } catch (error) {
            console.error('GapFillContentManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    // collectContentData is inherited from ContentBuilderBase
}

// Initialize when DOM is ready
function initializeGapFillContentManager() {
    if (window.gapFillContentManager) {
        return;
    }

    const requiredElements = [
        'contentBlocksList',
        'emptyBlocksState',
        'gapFillPreview',
        'gapFillContentJson',
        'questionBlockTemplates'
    ];

    const missingElements = requiredElements.filter(id => !document.getElementById(id));
    if (missingElements.length > 0) {
        console.warn('GapFillContentManager: Missing elements:', missingElements);
        return false;
    }

    try {
        window.gapFillContentManager = new GapFillContentManager();
        return true;
    } catch (error) {
        console.error('Error initializing GapFillContentManager:', error);
        return false;
    }
}

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeGapFillContentManager, 300);
    });
} else {
    setTimeout(initializeGapFillContentManager, 300);
}

// Global access
window.initializeGapFillContentManager = initializeGapFillContentManager;

// Export
if (typeof window !== 'undefined') {
    window.GapFillContentManager = GapFillContentManager;
}

