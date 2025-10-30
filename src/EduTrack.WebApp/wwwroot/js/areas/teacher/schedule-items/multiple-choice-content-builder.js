/**
 * Multiple Choice Content Builder
 * - Optional single context block (text/image/video/audio)
 * - Multiple questions, each with options and single/multiple correct answers
 * - Produces ContentJson: { contextBlock?, questions: [ { question, options[], answerType, randomizeOptions } ] }
 */

(function () {
    if (window.multipleChoiceBuilderInitialized) return;
    window.multipleChoiceBuilderInitialized = true;

    const state = {
        contextBlock: null, // DOM element for context block
        questions: [], // { id, question, options: [{ index, text, isCorrect }], answerType, randomizeOptions, mediaBlock }
        pendingMediaQuestionId: null
    };

    const els = {
        contextContainer: document.getElementById('mcContextContainer'),
        contextEmpty: document.getElementById('mcContextEmptyState'),
        addContextBtn: document.getElementById('mcAddContextBlockBtn'),
        removeContextBtn: document.getElementById('mcRemoveContextBlockBtn'),
        addQuestionBtn: document.getElementById('mcAddQuestionBtn'),
        questionsContainer: document.getElementById('mcQuestionsContainer'),
        sidebar: document.getElementById('mcSidebar'),
        sidebarCount: document.getElementById('mcSidebarCount'),
        mainContentField: document.getElementById('contentJson'),
        selfHidden: document.getElementById('multipleChoiceContentJson')
    };

    function init() {
        if (!els.questionsContainer) return;
        bindEvents();
        loadExisting();
        renderAll();
        updateHidden();
        // Listen for content block updates to keep JSON in sync
        document.addEventListener('blockContentChanged', () => updateHidden());

        // Handle modal selection fallback when no active content builder
        document.addEventListener('blockTypeSelected', (e) => {
            if (!e.detail) return;
            // Only handle when we're awaiting media for a question
            if (!state.pendingMediaQuestionId) return;
            const type = (e.detail.type || '').toLowerCase();
            if (type !== 'image' && type !== 'audio') return;
            addQuestionMediaBlock(state.pendingMediaQuestionId, type);
            state.pendingMediaQuestionId = null;
        });
    }

    function bindEvents() {
        els.addContextBtn?.addEventListener('click', showBlockTypeModal);
        els.removeContextBtn?.addEventListener('click', removeContextBlock);
        els.addQuestionBtn?.addEventListener('click', () => addQuestion());
    }

    function loadExisting() {
        // Try to parse existing content
        try {
            const value = els.selfHidden?.value;
            if (value) {
                const obj = JSON.parse(value);
                if (obj && Array.isArray(obj.questions)) {
                    state.questions = obj.questions.map((q, i) => normalizeQuestion(q, i));
                }
            }
        } catch { /* ignore */ }
    }

    function normalizeQuestion(raw, idx) {
        return {
            id: raw.id ?? (idx + 1),
            question: raw.question ?? '',
            answerType: raw.answerType === 'multiple' ? 'multiple' : 'single',
            randomizeOptions: !!raw.randomizeOptions,
            options: Array.isArray(raw.options) ? raw.options.map((o, j) => ({
                index: typeof o.index === 'number' ? o.index : j,
                text: o.text ?? '',
                isCorrect: !!o.isCorrect
            })) : [],
            mediaBlock: raw.mediaBlock ?? null
        };
    }

    function showBlockTypeModal() {
        if (!window.sharedContentBlockManager) return;
        window.sharedContentBlockManager.showBlockTypeModal('blockTypeModal', 'multiplechoice', {
            onSelect: (block) => {
                addContextBlock(block);
            }
        });
    }

    function addContextBlock(block) {
        state.contextBlock = block;
        renderContext();
        updateHidden();
    }

    function removeContextBlock() {
        state.contextBlock = null;
        renderContext();
        updateHidden();
    }

    function addQuestion() {
        const id = (state.questions[state.questions.length - 1]?.id ?? 0) + 1;
        state.questions.push({
            id,
            question: '',
            answerType: 'single',
            randomizeOptions: false,
            options: [
                { index: 0, text: '', isCorrect: false },
                { index: 1, text: '', isCorrect: false }
            ]
        });
        renderAll();
        updateHidden();
    }

    function removeQuestion(id) {
        state.questions = state.questions.filter(q => q.id !== id);
        renderAll();
        updateHidden();
    }

    function addOption(id) {
        const q = state.questions.find(x => x.id === id);
        if (!q) return;
        const nextIndex = q.options.length;
        q.options.push({ index: nextIndex, text: '', isCorrect: false });
        renderQuestion(id);
        updateHidden();
    }

    function removeOption(id, optIndex) {
        const q = state.questions.find(x => x.id === id);
        if (!q) return;
        q.options = q.options.filter(o => o.index !== optIndex).map((o, i) => ({ index: i, text: o.text, isCorrect: o.isCorrect }));
        renderQuestion(id);
        updateHidden();
    }

    function setQuestionText(id, text) {
        const q = state.questions.find(x => x.id === id);
        if (!q) return;
        q.question = text;
        updateHidden();
    }

    function setAnswerType(id, type) {
        const q = state.questions.find(x => x.id === id);
        if (!q) return;
        q.answerType = type === 'multiple' ? 'multiple' : 'single';
        if (q.answerType === 'single') {
            // Ensure at most one correct
            let found = false;
            q.options = q.options.map(o => {
                if (o.isCorrect && !found) { found = true; return o; }
                return { ...o, isCorrect: false };
            });
        }
        renderQuestion(id);
        updateHidden();
    }

    function setRandomize(id, value) {
        const q = state.questions.find(x => x.id === id);
        if (!q) return;
        q.randomizeOptions = !!value;
        updateHidden();
    }

    function setOptionText(id, optIndex, text) {
        const q = state.questions.find(x => x.id === id);
        if (!q) return;
        const opt = q.options.find(o => o.index === optIndex);
        if (!opt) return;
        opt.text = text;
        updateHidden();
    }

    function setOptionCorrect(id, optIndex, isCorrect) {
        const q = state.questions.find(x => x.id === id);
        if (!q) return;
        if (q.answerType === 'single') {
            q.options = q.options.map(o => ({ ...o, isCorrect: o.index === optIndex }));
        } else {
            const opt = q.options.find(o => o.index === optIndex);
            if (opt) opt.isCorrect = !!isCorrect;
        }
        renderQuestion(id);
        updateHidden();
    }

    function renderContext() {
        if (!els.contextContainer) return;
        els.contextContainer.innerHTML = '';
        if (state.contextBlock) {
            const blockEl = window.sharedContentBlockManager?.renderBlock(state.contextBlock);
            if (blockEl) {
                els.contextContainer.appendChild(blockEl);
            }
            els.contextEmpty?.style && (els.contextEmpty.style.display = 'none');
            if (els.removeContextBtn) els.removeContextBtn.style.display = 'inline-flex';
        } else {
            els.contextEmpty?.style && (els.contextEmpty.style.display = 'block');
            if (els.removeContextBtn) els.removeContextBtn.style.display = 'none';
        }
    }

    function renderAll() {
        renderContext();
        renderQuestions();
        renderSidebar();
    }

    function renderQuestions() {
        els.questionsContainer.innerHTML = '';
        state.questions.forEach(q => {
            els.questionsContainer.appendChild(renderQuestionEditor(q));
        });
    }

    function renderQuestion(id) {
        const wrapper = document.getElementById(`mc-q-${id}`);
        if (!wrapper) return;
        const q = state.questions.find(x => x.id === id);
        if (!q) return;
        const newEditor = renderQuestionEditor(q);
        wrapper.replaceWith(newEditor);
        renderSidebar();
    }

    function renderQuestionEditor(q) {
        const wrapper = document.createElement('div');
        wrapper.className = 'mc-question';
        wrapper.id = `mc-q-${q.id}`;

        const header = document.createElement('div');
        header.className = 'mc-question-header';
        header.innerHTML = `
            <div class="mc-question-title">
                <span>سوال ${q.id}</span>
            </div>
            <div class="mc-question-actions">
                <button type="button" class="btn-teacher btn-danger btn-sm" data-action="remove-question">حذف</button>
            </div>
        `;
        header.querySelector('[data-action="remove-question"]').addEventListener('click', () => removeQuestion(q.id));

        const body = document.createElement('div');
        body.className = 'mc-question-body';
        body.innerHTML = `
            <div class="mb-3">
                <label class="form-label">صورت سوال</label>
                <textarea class="form-control" rows="2" data-role="stem"></textarea>
            </div>
            <div class="mc-settings">
                <div class="setting-item">
                    <label class="form-label">نوع پاسخ</label>
                    <select class="form-select" data-role="answer-type">
                        <option value="single">تک‌گزینه‌ای</option>
                        <option value="multiple">چندپاسخه</option>
                    </select>
                </div>
                <div class="setting-item">
                    <label class="form-label">به‌هم‌ریختن گزینه‌ها</label>
                    <div class="form-check form-switch">
                        <input class="form-check-input" type="checkbox" data-role="randomize">
                        <label class="form-check-label">فعال</label>
                    </div>
                </div>
            </div>
            <div class="mc-options">
                <div class="mc-options-header">
                    <div class="title">گزینه‌ها</div>
                    <button type="button" class="btn-teacher btn-secondary btn-sm" data-action="add-option">افزودن گزینه</button>
                </div>
                <div class="mc-options-list" data-role="options-list"></div>
            </div>
            <div class="mc-media">
                <div class="mc-media-header">
                    <div class="title">رسانه سوال (اختیاری)</div>
                    <div class="actions">
                        <button type="button" class="btn-teacher btn-secondary btn-sm" data-action="add-media">افزودن تصویر/صوت</button>
                        <button type="button" class="btn-teacher btn-danger btn-sm" data-action="remove-media" style="display:none;">حذف رسانه</button>
                    </div>
                </div>
                <div class="mc-media-container" data-role="media-container"></div>
            </div>
        `;

        // Bind question fields
        const stem = body.querySelector('[data-role="stem"]');
        stem.value = q.question;
        stem.addEventListener('input', (e) => setQuestionText(q.id, e.target.value));

        const answerType = body.querySelector('[data-role="answer-type"]');
        answerType.value = q.answerType;
        answerType.addEventListener('change', (e) => setAnswerType(q.id, e.target.value));

        const randomize = body.querySelector('[data-role="randomize"]');
        randomize.checked = q.randomizeOptions;
        randomize.addEventListener('change', (e) => setRandomize(q.id, e.target.checked));

        body.querySelector('[data-action="add-option"]').addEventListener('click', () => addOption(q.id));

        // Media handlers
        const addMediaBtn = body.querySelector('[data-action="add-media"]');
        const removeMediaBtn = body.querySelector('[data-action="remove-media"]');
        const mediaContainer = body.querySelector('[data-role="media-container"]');
        addMediaBtn.addEventListener('click', () => openMediaPicker(q.id));
        removeMediaBtn.addEventListener('click', () => removeQuestionMedia(q.id));

        // Render existing media if present
        if (q.mediaBlock) {
            renderMediaInto(q, mediaContainer);
            removeMediaBtn.style.display = 'inline-flex';
        }

        // Render options
        const list = body.querySelector('[data-role="options-list"]');
        list.innerHTML = '';
        q.options.forEach(opt => {
            const row = document.createElement('div');
            row.className = 'mc-option-row';
            row.innerHTML = `
                <div class="opt-correct">
                    ${q.answerType === 'single'
                        ? `<input type="radio" name="mc-correct-${q.id}" ${opt.isCorrect ? 'checked' : ''} />`
                        : `<input type="checkbox" ${opt.isCorrect ? 'checked' : ''} />`}
                </div>
                <div class="opt-text">
                    <input type="text" class="form-control" placeholder="گزینه ${opt.index + 1}" />
                </div>
                <div class="opt-actions">
                    <button type="button" class="btn-teacher btn-danger btn-sm">حذف</button>
                </div>
            `;
            // Bind
            const correctInput = row.querySelector('input[type="radio"], input[type="checkbox"]');
            correctInput.addEventListener('change', (e) => setOptionCorrect(q.id, opt.index, e.target.checked));
            const textInput = row.querySelector('input.form-control');
            textInput.value = opt.text;
            textInput.addEventListener('input', (e) => setOptionText(q.id, opt.index, e.target.value));
            const delBtn = row.querySelector('.btn-danger');
            delBtn.addEventListener('click', () => removeOption(q.id, opt.index));
            list.appendChild(row);
        });

        wrapper.appendChild(header);
        wrapper.appendChild(body);
        return wrapper;
    }

    function openMediaPicker(questionId) {
        state.pendingMediaQuestionId = questionId;
        if (window.sharedContentBlockManager && typeof window.sharedContentBlockManager.showBlockTypeModal === 'function') {
            // Use media-only config so teacher can pick image/audio
            window.sharedContentBlockManager.showBlockTypeModal('blockTypeModal', 'multiplechoice-media');
        } else if (typeof showBlockTypeModal === 'function') {
            showBlockTypeModal('multiplechoice-media');
        } else {
            // Fallback to image
            addQuestionMediaBlock(questionId, 'image');
            state.pendingMediaQuestionId = null;
        }
    }

    function addQuestionMediaBlock(questionId, type) {
        const q = state.questions.find(x => x.id === questionId);
        if (!q) return;

        // Only allow image or audio
        if (type !== 'image' && type !== 'audio') return;

        q.mediaBlock = { type, data: {} };
        renderQuestion(questionId);
        updateHidden();
    }

    function removeQuestionMedia(questionId) {
        const q = state.questions.find(x => x.id === questionId);
        if (!q) return;
        q.mediaBlock = null;
        renderQuestion(questionId);
        updateHidden();
    }

    function renderMediaInto(q, container) {
        const type = q.mediaBlock?.type;
        if (!type) return;
        const template = document.querySelector(`#mcContentBlockTemplates .content-block-template[data-type="${type}"]`);
        if (!template) return;
        container.innerHTML = '';
        const blockId = `mcmedia-${q.id}-${Date.now()}`;
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = blockId;
        blockElement.dataset.type = type;
        blockElement.dataset.blockData = JSON.stringify(q.mediaBlock.data || {});
        container.appendChild(blockElement);

        // Let specific block managers hydrate and bind
        const populateEvent = new CustomEvent('populateBlockContent', {
            detail: { blockElement, block: { id: blockId, type, data: q.mediaBlock.data || {} }, blockType: type }
        });
        document.dispatchEvent(populateEvent);
    }

    function renderSidebar() {
        if (!els.sidebar) return;
        els.sidebar.innerHTML = '';
        els.sidebarCount && (els.sidebarCount.textContent = `${state.questions.length}`);

        state.questions.forEach(q => {
            const item = document.createElement('div');
            item.className = 'mc-sidebar-item';
            item.innerHTML = `
                <div class="title">سوال ${q.id}</div>
                <div class="meta">${(q.question || '').slice(0, 40)}</div>
            `;
            item.addEventListener('click', () => {
                const el = document.getElementById(`mc-q-${q.id}`);
                if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            });
            els.sidebar.appendChild(item);
        });
    }

    function getContent() {
        const content = {
            type: 'multipleChoice',
            contextBlock: state.contextBlock || null,
            questions: state.questions.map(q => ({
                question: q.question,
                answerType: q.answerType,
                randomizeOptions: q.randomizeOptions,
                options: q.options.map(o => ({ index: o.index, text: o.text, isCorrect: o.isCorrect })),
                mediaBlock: serializeQuestionMedia(q)
            }))
        };
        return content;
    }

    function serializeQuestionMedia(q) {
        if (!q.mediaBlock) return null;
        // If there is a rendered block, try to read its latest data from DOM
        const container = document.querySelector(`#mc-q-${q.id} .mc-media-container`);
        const blockEl = container ? container.querySelector('.content-block-template, .content-block') : null;
        if (blockEl && window.sharedContentBlockManager && typeof window.sharedContentBlockManager.getBlockData === 'function') {
            const data = window.sharedContentBlockManager.getBlockData(blockEl);
            return { type: q.mediaBlock.type, data: data || {} };
        }
        return { type: q.mediaBlock.type, data: q.mediaBlock.data || {} };
    }

    function updateHidden() {
        const content = getContent();
        const json = JSON.stringify(content);
        if (els.selfHidden) els.selfHidden.value = json;
        if (els.mainContentField) els.mainContentField.value = json;
        // Trigger sync if available
        if (window.step4ContentManager && window.step4ContentManager.syncManager) {
            window.step4ContentManager.syncManager.trigger('syncContentWithMainField');
        }
    }

    // Expose API for Step4 manager
    window.multipleChoiceBuilder = {
        getContent
    };

    init();
})();


