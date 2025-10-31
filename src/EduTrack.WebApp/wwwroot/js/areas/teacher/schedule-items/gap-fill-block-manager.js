/**
 * Gap-Fill Content Block Manager
 * Block-based editor similar to Written/MC: each block is a question (text/image/video/audio)
 * For text questions, teachers can insert [[blankN]] tokens and configure answers per blank.
 * Serialized shape: { type: 'gapfill', blocks: [ { id, type: 'questionText'|'questionImage'|..., data: { content, textContent, points, difficulty, isRequired, teacherGuidance, gaps: [{ index, correctAnswer, alternativeAnswers, hint }], answerType, caseSensitive } } ] }
 */

(function () {
    if (window.gapFillBlockManagerInitialized) return;
    window.gapFillBlockManagerInitialized = true;

    class GapFillBlockManager extends ContentBuilderBase {
        constructor() {
            super({
                containerId: 'contentBlocksList',
                emptyStateId: 'emptyBlocksState',
                previewId: 'gapFillPreview',
                hiddenFieldId: 'gapFillContentJson',
                modalId: 'blockTypeModal',
                contentType: 'gapfill'
            });
        }

        init() {
            // Ensure base initialization (fields, events, sync, load existing)
            if (typeof super.init === 'function') {
                super.init();
            }
            if (!this.blocksList || !this.emptyState || !this.preview || !this.hiddenField) {
                return;
            }
            this.setupGapFillListeners();
        }

        // Override addBlock to convert regular block types to question types
        addBlock(type) {
            console.log('GapFillBlockManager: addBlock called with type:', type);
            
            // Convert regular block types to question types
            // This ensures that when user selects 'text', 'image', etc. from modal,
            // they become 'questionText', 'questionImage', etc.
            let questionType = type;
            
            if (!type.startsWith('question')) {
                // Map regular types to question types
                const typeMap = {
                    'text': 'questionText',
                    'image': 'questionImage',
                    'video': 'questionVideo',
                    'audio': 'questionAudio'
                };
                
                questionType = typeMap[type.toLowerCase()] || type;
                console.log('GapFillBlockManager: Converted block type from', type, 'to', questionType);
            }
            
            // Call parent addBlock with the converted type
            if (typeof ContentBuilderBase.prototype.addBlock === 'function') {
                ContentBuilderBase.prototype.addBlock.call(this, questionType);
            } else {
                console.error('GapFillBlockManager: Parent addBlock method not available');
            }
        }

        setupGapFillListeners() {
            // Listen to content population to attach GF UI per question block
            // Note: We handle this in renderBlock directly, but keep this as backup
            this.eventManager.addListener('populateBlockContent', (e) => {
                const { blockElement, block } = e.detail || {};
                if (!blockElement || !block || !block.type?.startsWith('question')) return;
                // Check if gap fill UI already exists to avoid duplicates
                const existingGfContainer = blockElement.querySelector('[data-role="gf-container"]');
                if (!existingGfContainer) {
                    this.ensureGapFillUi(blockElement, block);
                }
            });

            // Listen to CKEditor changes routed by base to keep gaps in sync
            document.addEventListener('blockContentChanged', (e) => {
                // Only process if this is related to our blocks
                if (!e.detail || !e.detail.blockElement) return;
                
                const el = e.detail.blockElement;
                
                // Validate element has required properties
                if (!el.dataset || !el.dataset.blockId) return;
                
                // Check if this block belongs to this manager
                const blockId = el.dataset.blockId;
                const block = this.blocks?.find(b => b.id === blockId);
                if (!block) return; // Block doesn't belong to this manager
                
                const type = (el.dataset.type || '').toLowerCase();
                if (type === 'questiontext') {
                    this.syncGapsFromContent(el);
                }
            });
        }

        // Override renderBlock to use questionBlockTemplates instead of contentBlockTemplates
        renderBlock(block) {
            console.log('GapFillBlockManager: Rendering block:', block);
            
            // Determine the base template type for question blocks
            let templateType = block.type;
            if (block.type.startsWith('question')) {
                templateType = block.type.replace('question', '').toLowerCase();
            }
            
            console.log('GapFillBlockManager: Looking for template type:', templateType);
            
            if (!this.blocksList) {
                console.error('GapFillBlockManager: blocksList not available!');
                return null;
            }
            
            // Check if questionBlockTemplates exists
            const templatesContainer = document.getElementById('questionBlockTemplates');
            if (!templatesContainer) {
                console.error('GapFillBlockManager: questionBlockTemplates container not found!');
                return null;
            }
            
            // Look for template in questionBlockTemplates (for gap fill content)
            let template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
            
            console.log('GapFillBlockManager: Template found:', !!template);
            
            if (!template) {
                console.error('GapFillBlockManager: Template not found for type:', templateType);
                const allTemplates = document.querySelectorAll('#questionBlockTemplates .content-block-template');
                console.log('Available templates:', Array.from(allTemplates).map(t => ({
                    type: t.dataset.type,
                    element: t
                })));
                return null;
            }
            
            const blockElement = template.cloneNode(true);
            blockElement.classList.add('content-block');
            blockElement.dataset.blockId = block.id;
            blockElement.dataset.blockData = JSON.stringify(block.data);
            blockElement.dataset.type = block.type; // Keep original type (e.g., questionText)
            blockElement.dataset.templateType = templateType; // Store template type (e.g., text)
            
            // Configure template for question blocks
            if (block.type.startsWith('question')) {
                this.configureQuestionBlock(blockElement, block);
            }
            
            // Add direct event listeners to this specific block
            this.addDirectEventListeners(blockElement);
            
            // Find empty state - make sure it's a direct child of blocksList
            const emptyState = this.blocksList.querySelector(':scope > .empty-state, :scope > #emptyBlocksState.empty-state');
            
            // Verify emptyState is actually a child of blocksList before inserting
            if (emptyState && emptyState.parentNode === this.blocksList) {
                try {
                    this.blocksList.insertBefore(blockElement, emptyState);
                } catch (error) {
                    console.warn('GapFillBlockManager: Error inserting before emptyState, using appendChild instead:', error);
                    this.blocksList.appendChild(blockElement);
                }
            } else {
                this.blocksList.appendChild(blockElement);
            }
            
            // Initialize CKEditor for text blocks
            if (block.type === 'text' || block.type === 'questionText') {
                // Initialize CKEditor with a delay to ensure DOM is ready
                setTimeout(() => {
                    const editorElement = blockElement.querySelector('.ckeditor-editor');
                    if (editorElement && window.ckeditorManager) {
                        window.ckeditorManager.initializeEditor(editorElement);
                    }
                }, 100);
            }
            
            // Dispatch populate event for specific block managers
            const populateEvent = new CustomEvent('populateBlockContent', {
                detail: {
                    blockElement: blockElement,
                    block: block,
                    blockType: block.type
                }
            });
            document.dispatchEvent(populateEvent);
            
            // Attach gap fill UI for questionText blocks
            if (block.type.toLowerCase() === 'questiontext') {
                setTimeout(() => {
                    try {
                        this.ensureGapFillUi(blockElement, block);
                    } catch (error) {
                        console.error('Error attaching gap fill UI:', error);
                    }
                }, 150);
            }
            
            this.updateEmptyState();
            
            console.log('GapFillBlockManager: Block rendering completed for', block.id);
            return blockElement;
        }

        ensureGapFillUi(blockElement, block) {
            // Only for questionText blocks we add GF controls below text editor
            if ((block.type || '').toLowerCase() !== 'questiontext') return;

            let gfContainer = blockElement.querySelector('[data-role="gf-container"]');
            if (!gfContainer) {
                gfContainer = document.createElement('div');
                gfContainer.dataset.role = 'gf-container';
                gfContainer.className = 'gf-editor';
                gfContainer.innerHTML = `
                    <div class="section-header">
                        <div class="section-title">
                            <i class="fas fa-square"></i>
                            <span>تنظیمات جای‌خالی این سوال</span>
                        </div>
                        <div class="section-actions">
                            <button type="button" class="btn-teacher btn-secondary" data-action="gf-insert-blank">
                                <i class="fas fa-plus-square"></i>
                                <span>درج جای‌خالی</span>
                            </button>
                        </div>
                    </div>
                    <div class="gf-settings">
                        <div class="setting-item">
                            <label class="form-label">نوع تصحیح</label>
                            <select class="form-select form-select-sm" data-role="gf-answer-type">
                                <option value="exact">دقیق</option>
                                <option value="similar">مشابه</option>
                                <option value="keyword">کلیدواژه</option>
                            </select>
                        </div>
                        <div class="setting-item">
                            <label class="form-label">حساس به حروف</label>
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" data-role="gf-case">
                                <label class="form-check-label">فعال</label>
                            </div>
                        </div>
                    </div>
                    <div class="gaps-list" data-role="gf-gaps"></div>
                `;
                const qHint = blockElement.querySelector('.question-hint');
                if (qHint) {
                    qHint.parentNode.insertBefore(gfContainer, qHint);
                } else {
                    blockElement.querySelector('.block-content')?.appendChild(gfContainer);
                }

                // Bind insert blank
                gfContainer.querySelector('[data-action="gf-insert-blank"]').addEventListener('click', (ev) => {
                    ev.preventDefault(); ev.stopPropagation();
                    this.insertBlankToken(blockElement);
                });

                // Bind settings
                gfContainer.querySelector('[data-role="gf-answer-type"]').addEventListener('change', (e) => {
                    block.data.answerType = e.target.value;
                    this.updateHiddenField();
                });
                gfContainer.querySelector('[data-role="gf-case"]').addEventListener('change', (e) => {
                    block.data.caseSensitive = !!e.target.checked;
                    this.updateHiddenField();
                });
            }

            // Initialize defaults if missing
            if (!Array.isArray(block.data.gaps)) block.data.gaps = [];
            if (!block.data.answerType) block.data.answerType = 'exact';
            if (typeof block.data.caseSensitive !== 'boolean') block.data.caseSensitive = false;

            // Apply current values
            gfContainer.querySelector('[data-role="gf-answer-type"]').value = block.data.answerType;
            gfContainer.querySelector('[data-role="gf-case"]').checked = !!block.data.caseSensitive;

            // Render gaps list
            this.renderGaps(blockElement);
            // Ensure content->gaps sync at start
            this.syncGapsFromContent(blockElement);
        }

        insertBlankToken(blockElement) {
            const editorEl = blockElement.querySelector('.ckeditor-editor');
            if (editorEl && window.ckeditorManager) {
                const editor = window.ckeditorManager.editors.get(editorEl);
                const index = this.getNextGapIndex(blockElement);
                const token = ` [[blank${index}]] `;
                if (editor) {
                    editor.model.change(writer => {
                        const pos = editor.model.document.selection.getFirstPosition();
                        writer.insertText(token, pos);
                    });
                    editor.editing.view.focus();
                }
            }
        }

        getNextGapIndex(blockElement) {
            const blockId = blockElement.dataset.blockId;
            const block = this.blocks.find(b => b.id === blockId);
            const used = new Set((block?.data?.gaps || []).map(g => g.index));
            let i = 1; while (used.has(i)) i++; return i;
        }

        syncGapsFromContent(blockElement) {
            const blockId = blockElement.dataset.blockId;
            const block = this.blocks.find(b => b.id === blockId);
            if (!block) return;

            const contentHtml = block.data?.content || '';
            const text = contentHtml;
            const tokens = [...String(text).matchAll(/\[\[blank(\d+)\]\]/gi)].map(m => parseInt(m[1], 10)).filter(n => !isNaN(n));
            const unique = Array.from(new Set(tokens)).sort((a, b) => a - b);

            const existingByIndex = new Map((block.data.gaps || []).map(g => [g.index, g]));
            block.data.gaps = unique.map(idx => existingByIndex.get(idx) || ({ index: idx, correctAnswer: '', alternativeAnswers: [], hint: '' }));

            this.renderGaps(blockElement);
            this.updateHiddenField();
        }

        renderGaps(blockElement) {
            const blockId = blockElement.dataset.blockId;
            const block = this.blocks.find(b => b.id === blockId);
            if (!block) return;
            const container = blockElement.querySelector('[data-role="gf-gaps"]');
            if (!container) return;
            container.innerHTML = '';

            if (!block.data.gaps || block.data.gaps.length === 0) {
                const empty = document.createElement('div');
                empty.className = 'empty-state';
                empty.innerHTML = '<p>برای این سوال هنوز جای‌خالی تعریف نشده است. از دکمه "درج جای‌خالی" استفاده کنید.</p>';
                container.appendChild(empty);
                return;
            }

            block.data.gaps.forEach(g => {
                const row = document.createElement('div');
                row.className = 'gap-item';
                row.innerHTML = `
                    <div class="gap-item-header">
                        <div class="gap-item-title"><i class=\"fas fa-square\"></i> جای‌خالی ${g.index}</div>
                    </div>
                    <div class="gap-item-body">
                        <div class="row g-2">
                            <div class="col-12 col-md-6">
                                <label class="form-label">پاسخ صحیح</label>
                                <input type="text" class="form-control" data-role="gf-correct" data-index="${g.index}" value="${this.escapeHtml(g.correctAnswer)}" />
                            </div>
                            <div class="col-12 col-md-6">
                                <label class="form-label">پاسخ‌های جایگزین (با ویرگول جدا کنید)</label>
                                <input type="text" class="form-control" data-role="gf-alts" data-index="${g.index}" value="${this.escapeHtml((g.alternativeAnswers||[]).join(', '))}" />
                            </div>
                            <div class="col-12">
                                <label class="form-label">راهنما (اختیاری)</label>
                                <input type="text" class="form-control" data-role="gf-hint" data-index="${g.index}" value="${this.escapeHtml(g.hint||'')}" />
                            </div>
                        </div>
                    </div>`;

                row.querySelectorAll('input').forEach(inp => {
                    inp.addEventListener('input', (e) => this.onGapFieldChange(e, block));
                });
                container.appendChild(row);
            });
        }

        onGapFieldChange(e, block) {
            const role = e.target.dataset.role;
            const idx = parseInt(e.target.dataset.index, 10);
            const gap = (block.data.gaps || []).find(x => x.index === idx);
            if (!gap) return;
            const val = e.target.value || '';
            if (role === 'gf-correct') gap.correctAnswer = val;
            if (role === 'gf-alts') gap.alternativeAnswers = val.split(',').map(s => s.trim()).filter(Boolean);
            if (role === 'gf-hint') gap.hint = val;
            this.updateHiddenField();
        }

        escapeHtml(str) {
            const div = document.createElement('div');
            div.textContent = str || '';
            return div.innerHTML;
        }

        // Collect current data from DOM elements before returning content
        collectCurrentBlockData() {
            this.blocks.forEach(block => {
                const blockElement = document.querySelector(`[data-block-id="${block.id}"]`);
                if (blockElement) {
                    // Collect question-specific fields (points, difficulty, etc.)
                    this.collectQuestionFields(blockElement, block);
                    
                    // Collect gap fill specific data
                    this.collectGapFillFields(blockElement, block);
                }
            });
        }

        // Collect question-specific fields from DOM
        collectQuestionFields(blockElement, block) {
            const questionSettings = blockElement.querySelector('.question-settings');
            if (!questionSettings) return;

            // Collect points
            const pointsInput = questionSettings.querySelector('[data-setting="points"]');
            if (pointsInput) {
                block.data.points = parseFloat(pointsInput.value) || 1;
            }

            // Collect difficulty
            const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
            if (difficultySelect) {
                block.data.difficulty = difficultySelect.value || 'medium';
            }

            // Collect isRequired
            const isRequiredCheckbox = questionSettings.querySelector('[data-setting="isRequired"]');
            if (isRequiredCheckbox) {
                block.data.isRequired = !!isRequiredCheckbox.checked;
            }

            // Collect teacher guidance
            const teacherGuidanceInput = questionSettings.querySelector('[data-hint="true"]');
            if (teacherGuidanceInput) {
                block.data.teacherGuidance = teacherGuidanceInput.value || '';
            }
        }

        // Collect gap fill specific fields from DOM
        collectGapFillFields(blockElement, block) {
            // Only collect for questionText blocks
            if ((block.type || '').toLowerCase() !== 'questiontext') return;

            // Collect answer type
            const answerTypeSelect = blockElement.querySelector('[data-role="gf-answer-type"]');
            if (answerTypeSelect) {
                block.data.answerType = answerTypeSelect.value || 'exact';
            }

            // Collect case sensitive
            const caseCheckbox = blockElement.querySelector('[data-role="gf-case"]');
            if (caseCheckbox) {
                block.data.caseSensitive = !!caseCheckbox.checked;
            }

            // Collect gap answers from inputs
            const gapInputs = blockElement.querySelectorAll('[data-role^="gf-"]');
            if (gapInputs.length > 0 && block.data.gaps) {
                gapInputs.forEach(input => {
                    const index = parseInt(input.dataset.index, 10);
                    if (isNaN(index)) return;

                    const gap = block.data.gaps.find(g => g.index === index);
                    if (!gap) return;

                    const role = input.dataset.role;
                    if (role === 'gf-correct') {
                        gap.correctAnswer = input.value || '';
                    } else if (role === 'gf-alts') {
                        gap.alternativeAnswers = input.value.split(',').map(s => s.trim()).filter(Boolean);
                    } else if (role === 'gf-hint') {
                        gap.hint = input.value || '';
                    }
                });
            }
        }

        // Override to serialize points/difficulty/etc already handled by base; gaps added here
        getContent() {
            this.collectCurrentBlockData();
            return { type: this.config.contentType, blocks: this.blocks };
        }
    }

    function initializeGapFillBlockManager() {
        try {
            if (window.gapFillBlockManager) return;
            const required = ['contentBlocksList', 'emptyBlocksState', 'gapFillPreview', 'gapFillContentJson', 'questionBlockTemplates'];
            const missing = required.filter(id => !document.getElementById(id));
            if (missing.length) return;
            window.gapFillBlockManager = new GapFillBlockManager();
            setTimeout(() => {
                if (window.gapFillBlockManager && typeof window.gapFillBlockManager.loadExistingContent === 'function') {
                    window.gapFillBlockManager.loadExistingContent();
                }
            }, 300);
        } catch (e) {
            console.error('Error initializing GapFillBlockManager:', e);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => setTimeout(initializeGapFillBlockManager, 100));
    } else {
        setTimeout(initializeGapFillBlockManager, 100);
    }

    window.initializeGapFillBlockManager = initializeGapFillBlockManager;
})();


