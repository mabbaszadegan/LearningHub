/**
 * Gap Fill Content Builder
 * - Optional single context block (text/image/video/audio) using existing block templates/managers
 * - Gap text editor with [[blankN]] placeholders and per-blank answers (correct + alternatives + hint)
 * - Produces ContentJson in shape of GapFillContent { text, gaps[], answerType, caseSensitive }
 */

(function () {
    if (window.gapFillBuilderInitialized) return;
    window.gapFillBuilderInitialized = true;

    const state = {
        contextBlock: null, // DOM element for context block
        gaps: [], // { index: number, correctAnswer: string, alternativeAnswers: string[], hint: string }
        answerType: 'exact',
        caseSensitive: false
    };

    const els = {
        contextContainer: document.getElementById('gapFillContextContainer'),
        contextEmpty: document.getElementById('contextEmptyState'),
        addContextBtn: document.getElementById('addContextBlockBtn'),
        removeContextBtn: document.getElementById('removeContextBlockBtn'),
        gapText: document.getElementById('gapText'),
        insertBlankBtn: document.getElementById('insertBlankBtn'),
        previewBtn: document.getElementById('previewGapFillBtn'),
        gapsList: document.getElementById('gapsList'),
        answerType: document.getElementById('answerType'),
        caseSensitive: document.getElementById('caseSensitive'),
        sidebar: document.getElementById('gapFillSidebar'),
        contentJson: document.getElementById('contentJson'),
        selfHidden: document.getElementById('gapFillContentJson')
    };

    function init() {
        if (!els.contextContainer || !els.gapText) return;

        bindEvents();
        loadExisting();
        renderSidebar();
        updateHidden();
    }

    function bindEvents() {
        els.addContextBtn?.addEventListener('click', showBlockTypeModal);
        els.removeContextBtn?.addEventListener('click', removeContextBlock);
        els.insertBlankBtn?.addEventListener('click', insertBlankAtCursor);
        els.answerType?.addEventListener('change', () => { state.answerType = els.answerType.value; updateHidden(); });
        els.caseSensitive?.addEventListener('change', () => { state.caseSensitive = !!els.caseSensitive.checked; updateHidden(); });
        els.gapText?.addEventListener('input', handleGapTextChange);

        // Listen for populate from content blocks
        document.addEventListener('blockContentChanged', updateHidden);

        // If shared modal manager exists use it to pick block type
        document.addEventListener('blockTypeSelected', (e) => {
            if (!e.detail || !e.detail.scope || e.detail.scope !== 'gapfill') return;
            const type = e.detail.type;
            addContextBlock(type);
        });
    }

    function showBlockTypeModal() {
        // Limit to text/image/video/audio
        if (window.sharedContentBlockManager && typeof window.sharedContentBlockManager.showBlockTypeModal === 'function') {
            window.sharedContentBlockManager.showBlockTypeModal('blockTypeModal', 'gapfill', {
                allowed: ['text', 'image', 'video', 'audio']
            });
        } else {
            // Fallback: default to text if modal not available
            addContextBlock('text');
        }
    }

    function addContextBlock(type) {
        if (state.contextBlock) {
            state.contextBlock.remove();
            state.contextBlock = null;
        }

        const template = document.querySelector(`#contentBlockTemplates .content-block-template[data-type="${type}"]`);
        if (!template) return;

        const blockId = `ctx-${Date.now()}`;
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = blockId;
        blockElement.dataset.type = type;
        blockElement.dataset.blockData = JSON.stringify({});

        els.contextContainer.appendChild(blockElement);
        state.contextBlock = blockElement;

        if (els.contextEmpty) els.contextEmpty.style.display = 'none';
        if (els.removeContextBtn) els.removeContextBtn.style.display = 'inline-flex';

        // Dispatch populate event so specific managers (text/image/video/audio) can hydrate
        const populateEvent = new CustomEvent('populateBlockContent', {
            detail: { blockElement, block: { id: blockId, type, data: {} }, blockType: type }
        });
        document.dispatchEvent(populateEvent);

        renderSidebar();
        updateHidden();
    }

    function removeContextBlock() {
        if (state.contextBlock) {
            state.contextBlock.remove();
            state.contextBlock = null;
        }
        if (els.contextEmpty) els.contextEmpty.style.display = '';
        if (els.removeContextBtn) els.removeContextBtn.style.display = 'none';
        renderSidebar();
        updateHidden();
    }

    function insertBlankAtCursor() {
        const ta = els.gapText;
        if (!ta) return;
        const nextIndex = getNextBlankIndex();
        const token = `[[blank${nextIndex}]]`;
        const start = ta.selectionStart || 0;
        const end = ta.selectionEnd || 0;
        const value = ta.value || '';
        ta.value = value.substring(0, start) + token + value.substring(end);
        ta.selectionStart = ta.selectionEnd = start + token.length;
        ta.focus();
        handleGapTextChange();
    }

    function getNextBlankIndex() {
        const used = new Set(state.gaps.map(g => g.index));
        let i = 1;
        while (used.has(i)) i++;
        return i;
    }

    function handleGapTextChange() {
        const text = els.gapText.value || '';
        const tokens = [...text.matchAll(/\[\[blank(\d+)\]\]/gi)].map(m => parseInt(m[1], 10)).filter(n => !isNaN(n));
        const unique = Array.from(new Set(tokens)).sort((a, b) => a - b);

        // Sync state.gaps with detected tokens
        const existingByIndex = new Map(state.gaps.map(g => [g.index, g]));
        state.gaps = unique.map(idx => existingByIndex.get(idx) || ({ index: idx, correctAnswer: '', alternativeAnswers: [], hint: '' }));

        renderGapsList();
        renderSidebar();
        updateHidden();
    }

    function renderGapsList() {
        if (!els.gapsList) return;
        els.gapsList.innerHTML = '';
        if (state.gaps.length === 0) {
            const empty = document.createElement('div');
            empty.className = 'empty-state';
            empty.innerHTML = '<p>هنوز جای‌خالی‌ای در متن تعریف نشده است. از دکمه "درج جای‌خالی" استفاده کنید.</p>';
            els.gapsList.appendChild(empty);
            return;
        }

        state.gaps.forEach(g => {
            const item = document.createElement('div');
            item.className = 'gap-item';
            item.innerHTML = `
                <div class="gap-item-header">
                    <div class="gap-item-title"><i class="fas fa-square"></i> جای‌خالی ${g.index}</div>
                </div>
                <div class="gap-item-body">
                    <div class="row g-2">
                        <div class="col-12 col-md-6">
                            <label class="form-label">پاسخ صحیح</label>
                            <input type="text" class="form-control" data-role="correct" data-index="${g.index}" value="${escapeHtml(g.correctAnswer)}" />
                        </div>
                        <div class="col-12 col-md-6">
                            <label class="form-label">پاسخ‌های جایگزین (با ویرگول جدا کنید)</label>
                            <input type="text" class="form-control" data-role="alternatives" data-index="${g.index}" value="${escapeHtml(g.alternativeAnswers.join(', '))}" />
                        </div>
                        <div class="col-12">
                            <label class="form-label">راهنما (اختیاری)</label>
                            <input type="text" class="form-control" data-role="hint" data-index="${g.index}" value="${escapeHtml(g.hint)}" />
                        </div>
                    </div>
                </div>`;

            item.querySelectorAll('input').forEach(inp => {
                inp.addEventListener('input', onGapFieldChange);
            });

            els.gapsList.appendChild(item);
        });
    }

    function onGapFieldChange(e) {
        const role = e.target.dataset.role;
        const idx = parseInt(e.target.dataset.index, 10);
        const gap = state.gaps.find(g => g.index === idx);
        if (!gap) return;
        const val = e.target.value || '';
        if (role === 'correct') gap.correctAnswer = val;
        if (role === 'alternatives') gap.alternativeAnswers = val.split(',').map(s => s.trim()).filter(Boolean);
        if (role === 'hint') gap.hint = val;
        updateHidden();
    }

    function renderSidebar() {
        if (!els.sidebar) return;
        const hasContext = !!state.contextBlock;
        const blanks = state.gaps.map(g => `جای‌خالی ${g.index}`).join('، ');
        els.sidebar.innerHTML = `
            <div class="sidebar-section">
                <div class="sidebar-row"><i class="fas fa-layer-group"></i><span>بلاک زمینه: ${hasContext ? 'دارد' : 'ندارد'}</span></div>
                <div class="sidebar-row"><i class="fas fa-square"></i><span>تعداد جای‌خالی: ${state.gaps.length}</span></div>
                ${state.gaps.length ? `<div class="sidebar-plain">${blanks}</div>` : ''}
            </div>`;
    }

    function buildContentJson() {
        const content = {
            Text: els.gapText.value || '',
            Gaps: state.gaps.map(g => ({ Index: g.index, CorrectAnswer: g.correctAnswer || '', AlternativeAnswers: g.alternativeAnswers || [], Hint: g.hint || '' })),
            AnswerType: state.answerType,
            CaseSensitive: !!state.caseSensitive
        };

        // If context exists, embed as first content block to enable student rendering reuse
        if (state.contextBlock) {
            const data = getBlockData(state.contextBlock);
            // Store minimal, student-side renderer already supports these types
            content.ContextBlock = {
                Type: state.contextBlock.dataset.type,
                Data: data || {}
            };
        }
        return content;
    }

    function getBlockData(blockEl) {
        try {
            if (!blockEl) return {};
            const raw = blockEl.dataset.blockData || '{}';
            return JSON.parse(raw);
        } catch {
            return {};
        }
    }

    function updateHidden() {
        const json = JSON.stringify(buildContentJson());
        if (els.selfHidden) els.selfHidden.value = json;
        if (els.contentJson) els.contentJson.value = json;
    }

    function loadExisting() {
        const raw = els.selfHidden?.value || '';
        if (!raw) return;
        try {
            const data = JSON.parse(raw);
            els.gapText.value = data.Text || '';
            state.answerType = data.AnswerType || 'exact';
            state.caseSensitive = !!data.CaseSensitive;
            if (els.answerType) els.answerType.value = state.answerType;
            if (els.caseSensitive) els.caseSensitive.checked = state.caseSensitive;

            if (Array.isArray(data.Gaps)) {
                state.gaps = data.Gaps.map(g => ({ index: g.Index, correctAnswer: g.CorrectAnswer || '', alternativeAnswers: g.AlternativeAnswers || [], hint: g.Hint || '' }));
                renderGapsList();
            }

            if (data.ContextBlock && data.ContextBlock.Type) {
                addContextBlock((data.ContextBlock.Type || '').toLowerCase());
                // After creation, merge data
                if (state.contextBlock) {
                    state.contextBlock.dataset.blockData = JSON.stringify(data.ContextBlock.Data || {});
                    // Let specific managers hydrate preview
                    const populateEvent = new CustomEvent('populateBlockContent', {
                        detail: { blockElement: state.contextBlock, block: { id: state.contextBlock.dataset.blockId, type: state.contextBlock.dataset.type, data: data.ContextBlock.Data || {} }, blockType: state.contextBlock.dataset.type }
                    });
                    document.dispatchEvent(populateEvent);
                }
            }
        } catch {}
        renderSidebar();
        updateHidden();
    }

    function escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str || '';
        return div.innerHTML;
    }

    // Kickoff after DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();


