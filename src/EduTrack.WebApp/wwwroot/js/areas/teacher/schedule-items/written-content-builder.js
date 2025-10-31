/**
 * Written Content Block Manager
 * Handles written content creation and management for written-type schedule items
 * Uses specific block managers (text-block.js, image-block.js, etc.) for individual block functionality
 */

// Global functions

class WrittenContentBlockManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'contentBlocksList',
            emptyStateId: 'emptyBlocksState',
            previewId: 'writtenPreview',
            hiddenFieldId: 'writtenContentJson',
            modalId: 'blockTypeModal',
            contentType: 'written'
        });
        
        this.init();
    }
    
    init() {
        console.log('WrittenContentBlockManager: Initializing...', {
            blocksList: !!this.blocksList,
            blocksListId: this.config.containerId,
            emptyState: !!this.emptyState,
            emptyStateId: this.config.emptyStateId,
            preview: !!this.preview,
            previewId: this.config.previewId,
            hiddenField: !!this.hiddenField,
            hiddenFieldId: this.config.hiddenFieldId
        });
        
        if (!this.blocksList || !this.emptyState || !this.preview || !this.hiddenField) {
            console.error('WrittenContentBlockManager: Required elements not found!', {
                blocksList: !!this.blocksList,
                blocksListElement: this.blocksList ? this.blocksList.id : 'missing',
                emptyState: !!this.emptyState,
                preview: !!this.preview,
                hiddenField: !!this.hiddenField
            });
            // Log which elements are missing
            if (!this.blocksList) {
                const blocksListElement = document.getElementById(this.config.containerId);
                console.error('WrittenContentBlockManager: blocksList element not found. Looking for:', this.config.containerId, 'Found:', !!blocksListElement);
            }
            if (!this.emptyState) {
                const emptyStateElement = document.getElementById(this.config.emptyStateId);
                console.error('WrittenContentBlockManager: emptyState element not found. Looking for:', this.config.emptyStateId, 'Found:', !!emptyStateElement);
            }
            return;
        }
        
        console.log('WrittenContentBlockManager: All required elements found, initialization successful');
        
        // Adjust content type based on global mode
        if (window.multipleChoiceMode) {
            this.config.contentType = 'multipleChoice';
            // Force an initial hidden field update so ContentJson mirrors type
            setTimeout(() => this.updateHiddenField(), 0);
        } else if (window.gapFillMode) {
            this.config.contentType = 'gapfill';
            // Force an initial hidden field update so ContentJson mirrors type
            setTimeout(() => this.updateHiddenField(), 0);
        }

        this.setupWrittenSpecificEventListeners();
    }

    setupWrittenSpecificEventListeners() {
        // Listen for insert-above events
        this.eventManager.addListener('insertBlockAbove', (e) => {
            console.log('WrittenContentBlockManager: insertBlockAbove event received', e.detail);
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    // Override addBlock to convert regular block types to question types
    addBlock(type) {
        console.log('WrittenContentBlockManager: addBlock called with type:', type, 'contentType:', this.config.contentType, 'gapFillMode:', window.gapFillMode);
        
        // Convert regular block types to question types for written/gapfill/multipleChoice content
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
            console.log('WrittenContentBlockManager: Converted block type from', type, 'to', questionType);
        }
        
        // Call parent addBlock with the converted type
        if (typeof ContentBuilderBase.prototype.addBlock === 'function') {
            ContentBuilderBase.prototype.addBlock.call(this, questionType);
        } else {
            console.error('WrittenContentBlockManager: Parent addBlock method not available');
        }
    }

    handleInsertBlockAbove(blockElement) {
        console.log('WrittenContentBlockManager: handleInsertBlockAbove called for block:', blockElement.dataset.blockId);
        
        // Store the reference to the block above which we want to insert
        this.insertAboveBlock = blockElement;
        
        // Determine the item type name based on current mode
        let itemTypeName = 'written';
        if (window.gapFillMode || this.config.contentType === 'gapfill') {
            itemTypeName = 'gapfill';
        } else if (window.multipleChoiceMode || this.config.contentType === 'multipleChoice') {
            itemTypeName = 'multiplechoice';
        }
        
        // Show block type selection modal for inserting above
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId, itemTypeName);
        }
    }

    renderBlock(block) {
        console.log('WrittenContentBlockManager: Rendering block:', block);
        
        // Determine the base template type for question blocks
        let templateType = block.type;
        if (block.type.startsWith('question')) {
            templateType = block.type.replace('question', '').toLowerCase();
        }
        
        console.log('WrittenContentBlockManager: Looking for template type:', templateType);
        console.log('WrittenContentBlockManager: blocksList available:', !!this.blocksList);
        console.log('WrittenContentBlockManager: GapFillMode:', window.gapFillMode);
        
        if (!this.blocksList) {
            console.error('WrittenContentBlockManager: blocksList not available!');
            return null;
        }
        
        // Check if questionBlockTemplates exists
        const templatesContainer = document.getElementById('questionBlockTemplates');
        if (!templatesContainer) {
            console.error('WrittenContentBlockManager: questionBlockTemplates container not found!');
            return null;
        }
        
        // Look for template in questionBlockTemplates (for written content)
        let template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
        
        console.log('WrittenContentBlockManager: Template found:', !!template);
        
        if (!template) {
            console.error('WrittenContentBlockManager: Template not found for type:', templateType);
            const allTemplates = document.querySelectorAll('#questionBlockTemplates .content-block-template');
            console.log('Available templates:', Array.from(allTemplates).map(t => ({
                type: t.dataset.type,
                element: t
            })));
            console.log('Looking for:', `#questionBlockTemplates .content-block-template[data-type="${templateType}"]`);
            return null;
        }
        
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data);
        blockElement.dataset.type = block.type; // Keep original type (e.g., questionText)
        blockElement.dataset.templateType = templateType; // Store template type (e.g., text)
        
        console.log('WrittenContentBlockManager: Created block element:', blockElement);
        
        // Configure template for question blocks
        if (block.type.startsWith('question')) {
            this.configureQuestionBlock(blockElement, block);
        }
        
        // Add direct event listeners to this specific block
        console.log('WrittenContentBlockManager: Adding direct event listeners...');
        this.addDirectEventListeners(blockElement);
        
        const emptyState = this.blocksList.querySelector('.empty-state');
        console.log('WrittenContentBlockManager: emptyState found:', !!emptyState);
        console.log('WrittenContentBlockManager: blocksList children before:', this.blocksList.children.length);
        
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
            console.log('WrittenContentBlockManager: Block inserted before emptyState');
        } else {
            this.blocksList.appendChild(blockElement);
            console.log('WrittenContentBlockManager: Block appended to blocksList');
        }
        
        console.log('WrittenContentBlockManager: blocksList children after:', this.blocksList.children.length);
        console.log('WrittenContentBlockManager: Block added to DOM, element:', blockElement);
        
        // Initialize CKEditor only for text blocks
        if (block.type === 'text' || block.type === 'questionText') {
            // Initialize CKEditor with a delay to ensure DOM is ready
            // Use longer delay and retry mechanism to ensure editor element is available
            let attempts = 0;
            const maxAttempts = 5;
            const initEditor = () => {
                attempts++;
                const editorElement = blockElement.querySelector('.ckeditor-editor');
                if (editorElement && window.ckeditorManager) {
                    try {
                        window.ckeditorManager.initializeEditor(editorElement);
                        console.log('WrittenContentBlockManager: CKEditor initialized for block', block.id);
                    } catch (error) {
                        console.error('WrittenContentBlockManager: Error initializing CKEditor:', error);
                    }
                } else {
                    if (attempts < maxAttempts) {
                        // Retry with exponential backoff
                        setTimeout(initEditor, 100 * attempts);
                    } else {
                        console.warn('WrittenContentBlockManager: CKEditor element or manager not found after', maxAttempts, 'attempts', {
                            editorElement: !!editorElement,
                            ckeditorManager: !!window.ckeditorManager,
                            blockElement: !!blockElement,
                            blockType: block.type
                        });
                    }
                }
            };
            setTimeout(initEditor, 150);
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

        // If in Multiple Choice mode, attach MCQ editor to this block
        if (window.multipleChoiceMode) {
            this.attachMcqEditor(blockElement, block);
        }
        // If in Gap Fill mode, attach per-question gap editor (for questionText)
        // Use setTimeout to ensure DOM is fully ready
        if (window.gapFillMode) {
            setTimeout(() => {
                try {
                    this.attachGapFillEditor(blockElement, block);
                } catch (error) {
                    console.error('Error attaching gap fill editor:', error);
                    // Don't fail block creation if gap fill editor attachment fails
                }
            }, 150);
        }
        
        // Enhance question settings if present (modern controls)
        // Wait longer to ensure values are set and DOM is ready
        setTimeout(() => {
            const questionSettings = blockElement.querySelector('.question-settings');
            if (questionSettings && typeof window.enhanceQuestionSettings === 'function') {
                // Make sure values are set before enhancing
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
            }
        }, 150);
        
        console.log('WrittenContentBlockManager: Block rendering completed for', block.id);
        return blockElement;
    }

    // Mirror hidden field to gap fill when in gapFillMode
    updateHiddenField() {
        // Call base implementation via ContentBuilderBase prototype
        if (typeof ContentBuilderBase.prototype.updateHiddenField === 'function') {
            ContentBuilderBase.prototype.updateHiddenField.call(this);
        }
        if (window.gapFillMode) {
            try {
                const content = { type: 'gapfill', blocks: this.blocks };
                const json = JSON.stringify(content);
                const gf = document.getElementById('gapFillContentJson');
                if (gf) gf.value = json;
                const main = document.getElementById('contentJson');
                if (main) main.value = json;
            } catch (e) {
                console.warn('GapFill mirror update failed:', e);
            }
        }
    }

    attachMcqEditor(blockElement, block) {
        // Create MCQ container
        const mcqContainer = document.createElement('div');
        mcqContainer.className = 'mcq-editor-section';
        mcqContainer.innerHTML = `
            <div class="mcq-header">
                <div class="title">سوالات چندگزینه‌ای این بلاک</div>
                <div class="actions">
                    <button type="button" class="btn-teacher btn-secondary btn-sm" data-action="mcq-add-q">افزودن سوال</button>
                </div>
            </div>
            <div class="mcq-list" data-role="mcq-list"></div>
        `;
        blockElement.appendChild(mcqContainer);

        // Ensure data structure
        if (!Array.isArray(block.data.mcQuestions)) {
            block.data.mcQuestions = [];
        }

        const list = mcqContainer.querySelector('[data-role="mcq-list"]');
        const addBtn = mcqContainer.querySelector('[data-action="mcq-add-q"]');
        addBtn.addEventListener('click', () => {
            const qId = (block.data.mcQuestions[block.data.mcQuestions.length - 1]?.id || 0) + 1;
            block.data.mcQuestions.push({ id: qId, stem: '', answerType: 'single', randomize: false, options: [
                { index: 0, text: '', correct: false }, { index: 1, text: '', correct: false }
            ]});
            this.renderMcqList(list, block);
            this.updateHiddenField();
        });

        this.renderMcqList(list, block);
    }

    attachGapFillEditor(blockElement, block) {
        if ((block.type || '').toLowerCase() !== 'questiontext') return;
        
        // Ensure block element is in DOM
        if (!blockElement || !document.contains(blockElement)) {
            console.warn('WrittenContentBlockManager: Block element not in DOM, skipping gap fill editor attachment');
            return;
        }

        // Create UI if not present
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
            
            // Try to find insertion point
            let insertPoint = blockElement.querySelector('.question-hint');
            if (insertPoint && insertPoint.parentNode) {
                insertPoint.parentNode.insertBefore(gfContainer, insertPoint);
            } else {
                const blockContent = blockElement.querySelector('.block-content');
                if (blockContent) {
                    blockContent.appendChild(gfContainer);
                } else {
                    // Fallback: append to block element itself
                    console.warn('WrittenContentBlockManager: Could not find suitable insertion point for gap fill editor');
                    blockElement.appendChild(gfContainer);
                }
            }

            // Bind insert blank
            const insertBlankBtn = gfContainer.querySelector('[data-action="gf-insert-blank"]');
            if (insertBlankBtn) {
                insertBlankBtn.addEventListener('click', (ev) => {
                    ev.preventDefault(); ev.stopPropagation();
                    this.insertGapBlankToken(blockElement);
                });
            }

            // Bind settings
            const answerTypeSelect = gfContainer.querySelector('[data-role="gf-answer-type"]');
            if (answerTypeSelect) {
                answerTypeSelect.addEventListener('change', (e) => {
                    block.data.answerType = e.target.value;
                    this.updateHiddenField();
                });
            }
            
            const caseCheckbox = gfContainer.querySelector('[data-role="gf-case"]');
            if (caseCheckbox) {
                caseCheckbox.addEventListener('change', (e) => {
                    block.data.caseSensitive = !!e.target.checked;
                    this.updateHiddenField();
                });
            }
        }

        if (!Array.isArray(block.data.gaps)) block.data.gaps = [];
        if (!block.data.answerType) block.data.answerType = 'exact';
        if (typeof block.data.caseSensitive !== 'boolean') block.data.caseSensitive = false;

        // Apply current
        const sel = blockElement.querySelector('[data-role="gf-answer-type"]');
        if (sel) sel.value = block.data.answerType;
        const chk = blockElement.querySelector('[data-role="gf-case"]');
        if (chk) chk.checked = !!block.data.caseSensitive;

        // Render gaps and sync tokens
        this.renderGapList(blockElement, block);
        this.syncGapsFromQuestionContent(blockElement, block);
    }

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
            }
        }
    }

    nextGapIndex(blockElement) {
        const id = blockElement.dataset.blockId;
        const blk = this.blocks.find(b => b.id === id);
        const used = new Set((blk?.data?.gaps || []).map(g => g.index));
        let i = 1; while (used.has(i)) i++; return i;
    }

    syncGapsFromQuestionContent(blockElement, block) {
        const html = block.data?.content || '';
        const tokens = [...String(html).matchAll(/\[\[blank(\d+)\]\]/gi)]
            .map(m => parseInt(m[1], 10)).filter(n => !isNaN(n));
        const unique = Array.from(new Set(tokens)).sort((a,b)=>a-b);
        const map = new Map((block.data.gaps||[]).map(g => [g.index, g]));
        block.data.gaps = unique.map(i => map.get(i) || ({ index: i, correctAnswer: '', alternativeAnswers: [], hint: '' }));
        this.renderGapList(blockElement, block);
        this.updateHiddenField();
    }

    renderGapList(blockElement, block) {
        const container = blockElement.querySelector('[data-role="gf-gaps"]');
        if (!container) return;
        container.innerHTML = '';
        if (!block.data.gaps || !block.data.gaps.length) {
            const empty = document.createElement('div');
            empty.className = 'empty-state';
            empty.innerHTML = '<p>برای این سوال هنوز جای‌خالی تعریف نشده است.</p>';
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

    renderMcqList(container, block) {
        container.innerHTML = '';
        block.data.mcQuestions.forEach(q => {
            const wrapper = document.createElement('div');
            wrapper.className = 'mcq-item';
            wrapper.innerHTML = `
                <div class="mcq-item-header">
                    <div class="title">سوال ${q.id}</div>
                    <div class="actions">
                        <button type="button" class="btn-teacher btn-danger btn-sm" data-action="remove-q">حذف</button>
                    </div>
                </div>
                <div class="mcq-item-body">
                    <div class="mb-2">
                        <label class="form-label">صورت سوال</label>
                        <textarea class="form-control" rows="2" data-role="stem"></textarea>
                    </div>
                    <div class="mcq-settings">
                        <div class="setting-item">
                            <label class="form-label">نوع پاسخ</label>
                            <select class="form-select form-select-sm" data-role="atype">
                                <option value="single">تک‌گزینه‌ای</option>
                                <option value="multiple">چندپاسخه</option>
                            </select>
                        </div>
                        <div class="setting-item">
                            <label class="form-label">به‌هم‌ریختن گزینه‌ها</label>
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" data-role="rand">
                                <label class="form-check-label">فعال</label>
                            </div>
                        </div>
                    </div>
                    <div class="mcq-options">
                        <div class="mcq-options-header">
                            <div class="title">گزینه‌ها</div>
                            <button type="button" class="btn-teacher btn-secondary btn-sm" data-action="add-opt">افزودن گزینه</button>
                        </div>
                        <div class="mcq-options-list" data-role="opts"></div>
                    </div>
                </div>
            `;

            // Bind controls
            const stem = wrapper.querySelector('[data-role="stem"]');
            stem.value = q.stem || '';
            stem.addEventListener('input', (e) => { q.stem = e.target.value; this.updateHiddenField(); });

            const atype = wrapper.querySelector('[data-role="atype"]');
            atype.value = q.answerType || 'single';
            atype.addEventListener('change', (e) => {
                q.answerType = e.target.value === 'multiple' ? 'multiple' : 'single';
                if (q.answerType === 'single') {
                    let found = false; q.options = q.options.map(o => { if (o.correct && !found) { found = true; return o; } return { ...o, correct: false }; });
                }
                this.renderMcqOptions(wrapper.querySelector('[data-role="opts"]'), q);
                this.updateHiddenField();
            });

            const rand = wrapper.querySelector('[data-role="rand"]');
            rand.checked = !!q.randomize;
            rand.addEventListener('change', (e) => { q.randomize = !!e.target.checked; this.updateHiddenField(); });

            const removeQ = wrapper.querySelector('[data-action="remove-q"]');
            removeQ.addEventListener('click', () => {
                block.data.mcQuestions = block.data.mcQuestions.filter(x => x.id !== q.id);
                this.renderMcqList(container, block);
                this.updateHiddenField();
            });

            const optsList = wrapper.querySelector('[data-role="opts"]');
            const addOpt = wrapper.querySelector('[data-action="add-opt"]');
            addOpt.addEventListener('click', () => {
                const idx = q.options.length;
                q.options.push({ index: idx, text: '', correct: false });
                this.renderMcqOptions(optsList, q);
                this.updateHiddenField();
            });

            this.renderMcqOptions(optsList, q);
            container.appendChild(wrapper);
        });
    }

    renderMcqOptions(container, q) {
        container.innerHTML = '';
        q.options.forEach(opt => {
            const row = document.createElement('div');
            row.className = 'mcq-option-row';
            row.innerHTML = `
                <div class="opt-correct">${q.answerType === 'single' ? '<input type="radio" />' : '<input type="checkbox" />'}</div>
                <div class="opt-text"><input type="text" class="form-control form-control-sm" /></div>
                <div class="opt-actions"><button type="button" class="btn-teacher btn-danger btn-sm">حذف</button></div>
            `;
            const correctInput = row.querySelector('input[type="radio"], input[type="checkbox"]');
            correctInput.checked = !!opt.correct;
            correctInput.addEventListener('change', (e) => {
                if (q.answerType === 'single') {
                    q.options = q.options.map(o => ({ ...o, correct: o.index === opt.index }));
                } else {
                    opt.correct = !!e.target.checked;
                }
                this.updateHiddenField();
            });
            const textInput = row.querySelector('input.form-control');
            textInput.value = opt.text || '';
            textInput.addEventListener('input', (e) => { opt.text = e.target.value; this.updateHiddenField(); });
            const delBtn = row.querySelector('.btn-danger');
            delBtn.addEventListener('click', () => {
                q.options = q.options.filter(o => o.index !== opt.index).map((o, i) => ({ index: i, text: o.text, correct: o.correct }));
                this.renderMcqOptions(container, q);
                this.updateHiddenField();
            });
            container.appendChild(row);
        });
    }

    generateBlockPreview(block) {
        let html = '';
        
        switch (block.type) {
            case 'text':
                html += `<div class="text-block">${block.data.content || ''}</div>`;
                break;
            case 'image':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const imageUrl = block.data.fileUrl || block.data.previewUrl;
                    const sizeClass = this.getSizeClass(block.data.size);
                    const positionClass = this.getPositionClass(block.data.position);
                    html += `<div class="image-block ${positionClass}">`;
                    if (block.data.caption && block.data.captionPosition === 'top') {
                        html += `<div class="caption caption-top">${block.data.caption}</div>`;
                    }
                    html += `<img src="${imageUrl}" alt="تصویر" class="${sizeClass}" />`;
                    if (block.data.caption && block.data.captionPosition === 'bottom') {
                        html += `<div class="caption caption-bottom">${block.data.caption}</div>`;
                    }
                    html += '</div>';
                }
                break;
            case 'video':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const videoUrl = block.data.fileUrl || block.data.previewUrl;
                    const sizeClass = this.getSizeClass(block.data.size);
                    const positionClass = this.getPositionClass(block.data.position);
                    html += `<div class="video-block ${positionClass}">`;
                    if (block.data.caption && block.data.captionPosition === 'top') {
                        html += `<div class="caption caption-top">${block.data.caption}</div>`;
                    }
                    html += `<video controls preload="none" class="${sizeClass}"><source data-src="${videoUrl}" type="video/mp4"></video>`;
                    if (block.data.caption && block.data.captionPosition === 'bottom') {
                        html += `<div class="caption caption-bottom">${block.data.caption}</div>`;
                    }
                    html += '</div>';
                }
                break;
            case 'audio':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const audioUrl = block.data.fileUrl || block.data.previewUrl;
                    const mimeType = block.data.mimeType || 'audio/mpeg';
                    html += `<div class="audio-block">`;
                    if (block.data.caption) {
                        html += `<div class="caption">${block.data.caption}</div>`;
                    }
                    html += `<audio controls preload="none"><source data-src="${audioUrl}" type="${mimeType}"></audio>`;
                    html += '</div>';
                }
                break;
            case 'code':
                if (block.data.codeContent) {
                    html += `<div class="code-block-preview">`;
                    if (block.data.codeTitle) {
                        html += `<div class="code-title">${block.data.codeTitle}</div>`;
                    }
                    html += `<pre><code class="language-${block.data.language || 'plaintext'}">${block.data.codeContent}</code></pre>`;
                    html += '</div>';
                }
                break;
        }
        
        return html;
    }

    // Override getContent to return questionBlocks instead of blocks
    getContent() {
        // Collect current data from DOM before returning
        this.collectCurrentBlockData();
        
        // For gapFill mode, return blocks instead of questionBlocks
        if (window.gapFillMode || this.config.contentType === 'gapfill') {
            return {
                type: 'gapfill',
                blocks: this.blocks
            };
        }
        
        // For multiple choice, return blocks
        if (window.multipleChoiceMode || this.config.contentType === 'multipleChoice') {
            return {
                type: 'multipleChoice',
                blocks: this.blocks
            };
        }
        
        // For written content, return questionBlocks
        return {
            type: this.config.contentType || 'written',
            questionBlocks: this.blocks // Return as questionBlocks for written content
        };
    }

    // Collect current data from DOM elements
    collectCurrentBlockData() {
        this.blocks.forEach(block => {
            const blockElement = document.querySelector(`[data-block-id="${block.id}"]`);
            if (blockElement) {
                // Collect question-specific fields
                this.collectQuestionFields(blockElement, block);
            }
        });
    }

    // Collect question-specific fields from DOM
    collectQuestionFields(blockElement, block) {
        // Check if this is a question block
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

        // Collect required status
        const requiredCheckbox = questionSettings.querySelector('[data-setting="isRequired"]');
        if (requiredCheckbox) {
            block.data.isRequired = requiredCheckbox.checked;
        }

        // Collect teacher guidance
        const hintTextarea = blockElement.querySelector('[data-hint="true"]');
        if (hintTextarea) {
            block.data.teacherGuidance = hintTextarea.value || '';
        }

        // Collect question text content
        this.collectQuestionTextContent(blockElement, block);
    }

    // Collect question text content from different editor types
    collectQuestionTextContent(blockElement, block) {
        // Try CKEditor first (for text blocks)
        const ckEditor = blockElement.querySelector('.ckeditor-editor');
        if (ckEditor && window.ckeditorManager) {
            const editorContent = window.ckeditorManager.getEditorContent(ckEditor);
            if (editorContent) {
                block.data.content = editorContent.html;
                block.data.textContent = editorContent.text;
            }
            return;
        }

        // Try rich text editor (for image/video/audio blocks)
        const richTextEditor = blockElement.querySelector('.rich-text-editor');
        if (richTextEditor) {
            block.data.content = richTextEditor.innerHTML;
            block.data.textContent = richTextEditor.textContent;
            return;
        }

        // Try textarea as fallback
        const textarea = blockElement.querySelector('textarea');
        if (textarea && !textarea.hasAttribute('data-hint')) {
            block.data.content = textarea.value;
            block.data.textContent = textarea.value;
        }
    }

    // Override loadExistingContent to handle questionBlocks
    loadExistingContent() {
        console.log('WrittenContentBlockManager: loadExistingContent called');
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        
        console.log('WrittenContentBlockManager: Hidden field value:', hiddenFieldValue);
        
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            console.log('WrittenContentBlockManager: No hidden field value found');
            return;
        }
        
        try {
            this.isLoadingExistingContent = true;
            
            const data = JSON.parse(hiddenFieldValue);
            
            // Handle questionBlocks for written content
            if (data.questionBlocks && Array.isArray(data.questionBlocks)) {
                console.log('WrittenContentBlockManager: Found', data.questionBlocks.length, 'question blocks');
                
                if (this.blocksList) {
                    const existingBlocks = this.blocksList.querySelectorAll('.content-block, .question-block-template');
                    existingBlocks.forEach(block => block.remove());
                }
                
                this.blocks = data.questionBlocks;
                if (this.blocks.length > 0) {
                    this.nextBlockId = Math.max(...this.blocks.map(b => parseInt(b.id.split('-')[1]) || 0)) + 1;
                } else {
                    this.nextBlockId = 1;
                }
                
                console.log('WrittenContentBlockManager: Rendering', this.blocks.length, 'blocks');
                this.blocks.forEach((block, index) => {
                    console.log('WrittenContentBlockManager: Rendering block', block.id, 'of type', block.type);
                    this.renderBlock(block);
                });
                
                this.updateEmptyState();
                
                // Populate content fields after rendering with longer delay
                setTimeout(() => {
                    this.populateBlockContent();
                }, 500);
                
                // Notify sidebar manager to refresh
                setTimeout(() => {
                    if (window.contentSidebarManager) {
                        window.contentSidebarManager.forceRefresh();
                    }
                }, 1000);
            } else {
                console.warn('WrittenContentBlockManager: No questionBlocks found in data');
            }
            
            this.isLoadingExistingContent = false;
            
            setTimeout(() => {
                this.updatePreview();
                
                // Dispatch content loaded event for sidebar
                document.dispatchEvent(new CustomEvent('contentLoaded', {
                    detail: { contentType: this.config.contentType }
                }));
            }, 800);
            
        } catch (error) {
            console.error('WrittenContentBlockManager: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    // Implement updatePreview method
    updatePreview() {
        if (!this.preview) {
            return;
        }
        
        try {
            // Get current content
            const content = this.getContent();
            
            // Update preview using previewManager
            if (this.previewManager && typeof this.previewManager.generatePreviewHTML === 'function') {
                const previewHTML = this.previewManager.generatePreviewHTML(content);
                this.preview.innerHTML = previewHTML;
            } else {
                // Fallback: simple preview update
                if (content && content.blocks && Array.isArray(content.blocks)) {
                    let html = '<div class="content-preview">';
                    content.blocks.forEach(block => {
                        html += this.generateBlockPreview(block);
                    });
                    html += '</div>';
                    this.preview.innerHTML = html;
                } else if (content && content.questionBlocks && Array.isArray(content.questionBlocks)) {
                    let html = '<div class="content-preview">';
                    content.questionBlocks.forEach(question => {
                        html += this.generateBlockPreview(question);
                    });
                    html += '</div>';
                    this.preview.innerHTML = html;
                } else {
                    this.preview.innerHTML = '<div class="empty-content">هیچ محتوایی برای نمایش وجود ندارد.</div>';
                }
            }
        } catch (error) {
            console.error('WrittenContentBlockManager: Error updating preview:', error);
        }
    }
}

// Initialize when DOM is loaded
function initializeWrittenBlockManager() {
    try {
        console.log('WrittenContentBlockManager: Attempting to initialize...');
        
        if (window.writtenBlockManager) {
            console.log('WrittenContentBlockManager: Already initialized');
            return;
        }
        
        const requiredElements = [
            'contentBlocksList',
            'emptyBlocksState', 
            'writtenPreview',
            'writtenContentJson',
            'questionBlockTemplates'
        ];
        
        let missingElements = [];
        requiredElements.forEach(id => {
            const element = document.getElementById(id);
            console.log(`WrittenContentBlockManager: Checking element ${id}:`, !!element);
            if (!element) {
                missingElements.push(id);
            }
        });
        
        if (missingElements.length > 0) {
            console.warn('WrittenContentBlockManager: Missing required elements:', missingElements);
            console.warn('WrittenContentBlockManager: This usually means the writtenContentBuilder section is not loaded yet');
            // Return false to indicate failure instead of just returning
            return false;
        }
        
        console.log('WrittenContentBlockManager: All required elements found, creating manager...');
        try {
            window.writtenBlockManager = new WrittenContentBlockManager();
            console.log('WrittenContentBlockManager: Successfully initialized', window.writtenBlockManager);
            return true;
        } catch (error) {
            console.error('WrittenContentBlockManager: Error creating manager:', error);
            return false;
        }
        
        // Force load existing content after initialization
        setTimeout(() => {
            if (window.writtenBlockManager && typeof window.writtenBlockManager.loadExistingContent === 'function') {
                console.log('WrittenContentBlockManager: Force loading existing content...');
                window.writtenBlockManager.loadExistingContent();
            }
        }, 500);
        
        // Also try to load content after a longer delay
        setTimeout(() => {
            if (window.writtenBlockManager && typeof window.writtenBlockManager.loadExistingContent === 'function') {
                console.log('WrittenContentBlockManager: Second attempt to load existing content...');
                window.writtenBlockManager.loadExistingContent();
            }
        }, 2000);
        
    } catch (error) {
        console.error('Error initializing WrittenContentBlockManager:', error);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(initializeWrittenBlockManager, 100);
});

// Also try to initialize immediately if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeWrittenBlockManager, 300);
    });
} else {
    // Try multiple times with increasing delays to ensure elements are loaded
    setTimeout(initializeWrittenBlockManager, 300);
    setTimeout(initializeWrittenBlockManager, 1000);
    setTimeout(initializeWrittenBlockManager, 2000);
}

// Make initialization function available globally for manual triggering
window.initializeWrittenBlockManager = initializeWrittenBlockManager;

// Also make force load function available
window.forceLoadWrittenContent = () => {
    if (window.writtenBlockManager && typeof window.writtenBlockManager.loadExistingContent === 'function') {
        console.log('Force loading written content...');
        window.writtenBlockManager.loadExistingContent();
    } else {
        console.log('WrittenBlockManager not available, trying to initialize...');
        initializeWrittenBlockManager();
    }
};
