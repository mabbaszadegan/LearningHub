/**
 * Ordering Block Handler
 * Handles ordering question blocks
 */

class OrderingHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
    }

    canHandle(blockType) {
        return blockType === 'ordering';
    }

    render(block) {
        const blockTemplatesContainer = document.getElementById('blockTemplatesContainer');
        if (!blockTemplatesContainer) {
            console.error('OrderingHandler: blockTemplatesContainer not found');
            return null;
        }

        const templatesContainer = blockTemplatesContainer.querySelector('#questionTypeBlockTemplates');
        if (!templatesContainer) {
            console.error('OrderingHandler: questionTypeBlockTemplates not found');
            return null;
        }

        const template = templatesContainer.querySelector('[data-type="ordering"]');
        if (!template) {
            console.error('OrderingHandler: Ordering template not found');
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

        // Initialize ordering items if they exist
        if (block.data && Array.isArray(block.data.items)) {
            this.renderItems(blockElement, block);
        }

        // Setup event listeners
        const addItemBtn = blockElement.querySelector('[data-action="add-ordering-item"]');
        if (addItemBtn) {
            addItemBtn.addEventListener('click', () => this.addItem(blockElement, block));
        }

        const saveOrderBtn = blockElement.querySelector('[data-action="save-correct-order"]');
        if (saveOrderBtn) {
            saveOrderBtn.addEventListener('click', () => this.saveCorrectOrder(blockElement, block));
        }

        // Settings change listeners
        const settingsInputs = blockElement.querySelectorAll('[data-setting]');
        settingsInputs.forEach(input => {
            input.addEventListener('change', () => {
                const key = input.getAttribute('data-setting');
                let value;
                if (input.type === 'checkbox') {
                    value = input.checked;
                } else if (input.type === 'radio') {
                    // For radio buttons, get the checked one's value
                    const checkedRadio = blockElement.querySelector(`[data-setting="${key}"]:checked`);
                    value = checkedRadio ? checkedRadio.value : input.value;
                } else {
                    value = input.value;
                }
                block.data = block.data || {};
                block.data[key] = value;
                if (key === 'allowDragDrop') {
                    this.initializeDragDrop(blockElement);
                }
                if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                    this.contentManager.updateHiddenField();
                }
            });
        });

        // Initialize drag & drop if enabled
        if (block.data && block.data.allowDragDrop) {
            this.initializeDragDrop(blockElement);
        }
    }

    addItem(blockElement, block) {
        const itemsList = blockElement.querySelector('[data-role="ordering-items"]');
        if (!itemsList) return;

        const newItem = {
            id: this._generateItemId(block),
            type: 'text',
            value: '',
            include: true
        };
        block.data = block.data || {};
        block.data.items = block.data.items || [];
        block.data.items.push(newItem);

        const itemEl = this._createItemElement(newItem, block);
        itemsList.appendChild(itemEl);

        // Focus first input
        const input = itemEl.querySelector('input[type="text"]');
        if (input) input.focus();

        if (block.data.allowDragDrop) {
            this.initializeDragDrop(blockElement);
        }

        if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
            this.contentManager.updateHiddenField();
        }
    }

    renderItems(blockElement, block) {
        const itemsList = blockElement.querySelector('[data-role="ordering-items"]');
        if (!itemsList) return;
        itemsList.innerHTML = '';

        (block.data.items || []).forEach(item => {
            const itemEl = this._createItemElement(item, block);
            itemsList.appendChild(itemEl);
        });

        // Render saved correct order if exists
        this._renderCorrectOrder(blockElement, block);
    }

    saveCorrectOrder(blockElement, block) {
        const itemsList = blockElement.querySelector('[data-role="ordering-items"]');
        if (!itemsList) return;

        const idsInOrder = Array.from(itemsList.querySelectorAll('.ordering-item'))
            .filter(el => {
                const inc = el.querySelector('[data-field="include"]');
                return inc ? inc.checked : true;
            })
            .map(el => el.getAttribute('data-item-id'));
        block.data = block.data || {};
        block.data.correctOrder = idsInOrder;

        this._renderCorrectOrder(blockElement, block);

        if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
            this.contentManager.updateHiddenField();
        }
    }

    initializeDragDrop(blockElement) {
        const itemsList = blockElement.querySelector('[data-role="ordering-items"]');
        if (!itemsList) return;

        const draggable = blockElement.querySelector('[data-setting="allowDragDrop"]').checked;
        Array.from(itemsList.children).forEach(child => {
            child.setAttribute('draggable', draggable ? 'true' : 'false');
            child.classList.toggle('draggable', draggable);
        });

        // Avoid re-binding multiple times
        if (itemsList._orderingDnDBound) return;
        itemsList._orderingDnDBound = true;

        let dragSrcEl = null;

        itemsList.addEventListener('dragstart', (e) => {
            const li = e.target.closest('.ordering-item');
            if (!li) return;
            dragSrcEl = li;
            li.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
            try { e.dataTransfer.setData('text/plain', li.dataset.itemId); } catch (_) { }
        });

        itemsList.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            const li = e.target.closest('.ordering-item');
            if (!li || li === dragSrcEl) return;
            const rect = li.getBoundingClientRect();
            const next = (e.clientY - rect.top) / rect.height > 0.5;
            itemsList.insertBefore(dragSrcEl, next ? li.nextSibling : li);
        });

        itemsList.addEventListener('drop', (e) => {
            e.preventDefault();
        });

        itemsList.addEventListener('dragend', () => {
            if (dragSrcEl) dragSrcEl.classList.remove('dragging');
        });
    }

    collectData(blockElement, block) {
        const itemsList = blockElement.querySelector('[data-role="ordering-items"]');
        const correctList = blockElement.querySelector('[data-role="correct-order"]');

        const items = Array.from(itemsList.querySelectorAll('.ordering-item')).map(el => {
            const id = el.getAttribute('data-item-id');
            const typeSelect = el.querySelector('[data-field="type"]');
            const type = typeSelect ? typeSelect.value : 'text';
            const includeCheckbox = el.querySelector('[data-field="include"]');
            const include = includeCheckbox ? !!includeCheckbox.checked : true;
            if (type === 'text') {
                const valueInput = el.querySelector('[data-field="value"]');
                const value = valueInput ? valueInput.value.trim() : '';
                return { id, type, value, include };
            }
            // media item
            const fileId = el.getAttribute('data-file-id');
            const fileName = el.getAttribute('data-file-name');
            const fileUrl = el.getAttribute('data-file-url');
            const mimeType = el.getAttribute('data-mime');
            return { id, type, include, fileId: fileId ? parseInt(fileId, 10) : null, fileName, fileUrl, mimeType };
        });

        const correctOrder = Array.from(correctList.querySelectorAll('[data-item-id]')).map(el => el.getAttribute('data-item-id'));

        const settings = this._readSettings(blockElement);

        block.data = Object.assign({}, block.data, settings, {
            items,
            correctOrder
        });

        return block.data;
    }

    // Helpers
    _generateItemId(block) {
        const base = (block && block.id) ? block.id : 'block';
        const ts = Date.now();
        const rnd = Math.floor(Math.random() * 100000);
        return `${base}-item-${ts}-${rnd}`;
    }

    _createItemElement(item, block) {
        const li = document.createElement('div');
        li.className = 'ordering-item';
        li.setAttribute('data-item-id', item.id);

        const direction = (block.data && block.data.direction) || 'vertical';
        li.style.display = direction === 'horizontal' ? 'inline-block' : 'block';

        li.innerHTML = `
            <div class="ordering-item-inner">
                <div class="ordering-item-controls" style="display:flex; gap:8px; align-items:center;">
                    <span class="drag-handle" title="جابجایی">⋮⋮</span>
                    <select data-field="type" class="form-select form-select-sm" style="width: 140px;">
                        <option value="text" ${item.type === 'text' ? 'selected' : ''}>متن</option>
                        <option value="image" ${item.type === 'image' ? 'selected' : ''}>تصویر</option>
                        <option value="audio" ${item.type === 'audio' ? 'selected' : ''}>صوت</option>
                    </select>
                    <label class="form-check-label" style="display:flex; align-items:center; gap:4px;">
                        <input type="checkbox" class="form-check-input" data-field="include" ${item.include === false ? '' : 'checked'} />
                        <span style="font-size:12px;">جزء پاسخ</span>
                    </label>
                    <div class="ordering-item-inputs" style="flex:1; display:flex; gap:6px; align-items:center;"></div>
                    <button type="button" class="btn btn-sm btn-outline-danger" data-action="remove-item" title="حذف">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
                <div class="ordering-item-preview" style="margin-top:6px;"></div>
            </div>
        `;

        // Bind events
        const typeSelect = li.querySelector('[data-field="type"]');
        const removeBtn = li.querySelector('[data-action="remove-item"]');
        const inputsHost = li.querySelector('.ordering-item-inputs');
        const includeCheckbox = li.querySelector('[data-field="include"]');

        const buildInputsForType = (currentType) => {
            inputsHost.innerHTML = '';
            if (currentType === 'text') {
                const input = document.createElement('input');
                input.type = 'text';
                input.className = 'form-control form-control-sm';
                input.setAttribute('data-field', 'value');
                input.placeholder = 'عبارت...';
                input.value = item.value && typeof item.value === 'string' ? item.value : '';
                inputsHost.appendChild(input);
                input.addEventListener('input', () => updateItemModel());
            } else if (currentType === 'image') {
                // Upload button + hidden input
                const uploadBtn = document.createElement('button');
                uploadBtn.type = 'button';
                uploadBtn.className = 'btn btn-sm btn-outline-primary';
                uploadBtn.setAttribute('data-action', 'upload-image');
                uploadBtn.innerHTML = '<i class="fas fa-upload"></i> آپلود تصویر';
                const fileInput = document.createElement('input');
                fileInput.type = 'file';
                fileInput.accept = 'image/*';
                fileInput.style.display = 'none';
                inputsHost.appendChild(uploadBtn);
                inputsHost.appendChild(fileInput);
                uploadBtn.addEventListener('click', () => fileInput.click());
                fileInput.addEventListener('change', async () => {
                    if (!fileInput.files || !fileInput.files[0]) return;
                    await this._handleItemFileUpload(li, block, item, fileInput.files[0], 'image');
                });
            } else if (currentType === 'audio') {
                // Upload + Record
                const uploadBtn = document.createElement('button');
                uploadBtn.type = 'button';
                uploadBtn.className = 'btn btn-sm btn-outline-primary';
                uploadBtn.setAttribute('data-action', 'upload-audio');
                uploadBtn.innerHTML = '<i class="fas fa-upload"></i> آپلود صوت';
                const fileInput = document.createElement('input');
                fileInput.type = 'file';
                fileInput.accept = 'audio/*';
                fileInput.style.display = 'none';
                const recordBtn = document.createElement('button');
                recordBtn.type = 'button';
                recordBtn.className = 'btn btn-sm btn-outline-secondary';
                recordBtn.setAttribute('data-action', 'start-record');
                recordBtn.innerHTML = '<i class="fas fa-microphone"></i> ضبط';
                const stopBtn = document.createElement('button');
                stopBtn.type = 'button';
                stopBtn.className = 'btn btn-sm btn-outline-danger';
                stopBtn.setAttribute('data-action', 'stop-record');
                stopBtn.innerHTML = '<i class="fas fa-stop"></i>';
                stopBtn.disabled = true;
                inputsHost.appendChild(uploadBtn);
                inputsHost.appendChild(fileInput);
                inputsHost.appendChild(recordBtn);
                inputsHost.appendChild(stopBtn);
                uploadBtn.addEventListener('click', () => fileInput.click());
                fileInput.addEventListener('change', async () => {
                    if (!fileInput.files || !fileInput.files[0]) return;
                    await this._handleItemFileUpload(li, block, item, fileInput.files[0], 'audio');
                });
                let mediaRecorder = null;
                let chunks = [];
                recordBtn.addEventListener('click', async () => {
                    try {
                        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                        mediaRecorder = new MediaRecorder(stream);
                        chunks = [];
                        mediaRecorder.ondataavailable = e => chunks.push(e.data);
                        mediaRecorder.onstop = async () => {
                            const blob = new Blob(chunks, { type: 'audio/wav' });
                            const file = new File([blob], `recording_${Date.now()}.wav`, { type: 'audio/wav' });
                            // stop tracks
                            stream.getTracks().forEach(t => t.stop());
                            await this._handleItemFileUpload(li, block, item, file, 'audio');
                        };
                        mediaRecorder.start();
                        recordBtn.disabled = true;
                        stopBtn.disabled = false;
                    } catch (err) {
                        console.error('OrderingHandler recording error:', err);
                    }
                });
                stopBtn.addEventListener('click', () => {
                    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
                        mediaRecorder.stop();
                        recordBtn.disabled = false;
                        stopBtn.disabled = true;
                    }
                });
            }
        };

        buildInputsForType(item.type);

        const updateItemModel = () => {
            // Update in-memory data
            if (block && block.data && Array.isArray(block.data.items)) {
                const idx = block.data.items.findIndex(x => x.id === item.id);
                if (idx >= 0) {
                    block.data.items[idx].type = typeSelect.value;
                    block.data.items[idx].include = includeCheckbox ? !!includeCheckbox.checked : true;
                    // For text type, read input value
                    if (typeSelect.value === 'text') {
                        const textInput = li.querySelector('[data-field="value"]');
                        block.data.items[idx].value = textInput ? textInput.value.trim() : '';
                        // Clean media fields
                        delete block.data.items[idx].fileId;
                        delete block.data.items[idx].fileName;
                        delete block.data.items[idx].fileUrl;
                        delete block.data.items[idx].mimeType;
                        // Also clean DOM attributes
                        li.removeAttribute('data-file-id');
                        li.removeAttribute('data-file-name');
                        li.removeAttribute('data-file-url');
                        li.removeAttribute('data-mime');
                    }
                }
            }
            // Update preview
            const textInput = li.querySelector('[data-field="value"]');
            this._updatePreview(li, typeSelect.value, textInput ? textInput.value.trim() : (item.fileUrl || ''));
            // Persist to hidden content
            if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                this.contentManager.updateHiddenField();
            }
        };

        typeSelect.addEventListener('change', () => {
            // Rebuild inputs on type change
            buildInputsForType(typeSelect.value);
            updateItemModel();
        });

        if (includeCheckbox) {
            includeCheckbox.addEventListener('change', updateItemModel);
        }

        removeBtn.addEventListener('click', () => {
            // Remove from data
            if (block && block.data && Array.isArray(block.data.items)) {
                block.data.items = block.data.items.filter(x => x.id !== item.id);
            }
            li.remove();
            if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                this.contentManager.updateHiddenField();
            }
        });

        // Initial preview render
        this._updatePreview(li, item.type, (item.type === 'text') ? (item.value || '') : (item.fileUrl || ''));

        // Set file data attributes if item already has file information (for existing items)
        if (item.type !== 'text' && item.fileId) {
            li.setAttribute('data-file-id', String(item.fileId));
            if (item.fileName) li.setAttribute('data-file-name', item.fileName);
            if (item.fileUrl) li.setAttribute('data-file-url', item.fileUrl);
            if (item.mimeType) li.setAttribute('data-mime', item.mimeType);
        }

        return li;
    }

    _updatePreview(containerEl, type, value) {
        const preview = containerEl.querySelector('.ordering-item-preview');
        if (!preview) return;
        preview.innerHTML = '';
        if (type === 'text') {
            const span = document.createElement('span');
            span.textContent = value || '—';
            preview.appendChild(span);
        } else if (type === 'image') {
            if (value) {
                const img = document.createElement('img');
                img.src = value;
                img.alt = 'ordering image';
                img.style.maxWidth = '200px';
                img.style.maxHeight = '120px';
                preview.appendChild(img);
            }
        } else if (type === 'audio') {
            if (value) {
                const audio = document.createElement('audio');
                audio.controls = true;
                audio.src = value;
                preview.appendChild(audio);
            }
        }
    }

    async _handleItemFileUpload(itemElement, block, item, file, kind) {
        // Basic validation
        const mime = (file.type || '').toLowerCase();
        if (kind === 'image' && !mime.startsWith('image/')) return;
        if (kind === 'audio' && !mime.startsWith('audio/') && file.name && !/\.(mp3|wav|ogg)$/i.test(file.name)) return;

        // Minimal inline progress (optional: can be enhanced to match shared UI)
        const preview = itemElement.querySelector('.ordering-item-preview');
        if (preview) {
            preview.innerHTML = '<div class="text-muted" style="font-size:12px;">در حال آپلود...</div>';
        }

        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', kind);

            const response = await fetch('/FileUpload/UploadContentFile', {
                method: 'POST',
                body: formData
            });
            const result = await response.json();
            if (!result || !result.success) throw new Error(result?.message || 'Upload failed');

            const data = result.data || {};
            // Update item with file info
            item.type = kind;
            item.fileId = data.id;
            item.fileName = data.fileName || data.originalFileName || file.name;
            item.fileUrl = data.url || `/FileUpload/GetFile/${data.id}`;
            item.mimeType = data.mimeType || file.type || '';
            // Clear text value
            delete item.value;

            // Mirror on DOM element for collection
            itemElement.setAttribute('data-file-id', String(item.fileId));
            if (item.fileName) itemElement.setAttribute('data-file-name', item.fileName); else itemElement.removeAttribute('data-file-name');
            if (item.fileUrl) itemElement.setAttribute('data-file-url', item.fileUrl); else itemElement.removeAttribute('data-file-url');
            if (item.mimeType) itemElement.setAttribute('data-mime', item.mimeType); else itemElement.removeAttribute('data-mime');

            // Reflect in block data array
            if (block && block.data && Array.isArray(block.data.items)) {
                const idx = block.data.items.findIndex(x => x.id === item.id);
                if (idx >= 0) {
                    block.data.items[idx] = { ...block.data.items[idx], ...item };
                }
            }

            // Update preview
            this._updatePreview(itemElement, kind, item.fileUrl);

            if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                this.contentManager.updateHiddenField();
            }
        } catch (err) {
            console.error('OrderingHandler upload error:', err);
            if (preview) {
                preview.innerHTML = '<div class="text-danger" style="font-size:12px;">خطا در آپلود</div>';
            }
        }
    }

    _renderCorrectOrder(blockElement, block) {
        const correctList = blockElement.querySelector('[data-role="correct-order"]');
        const itemsList = blockElement.querySelector('[data-role="ordering-items"]');
        if (!correctList || !itemsList) return;
        correctList.innerHTML = '';

        const ids = (block.data && Array.isArray(block.data.correctOrder) && block.data.correctOrder.length)
            ? block.data.correctOrder
            : Array.from(itemsList.querySelectorAll('.ordering-item'))
                .filter(el => {
                    const inc = el.querySelector('[data-field="include"]');
                    return inc ? inc.checked : true;
                })
                .map(el => el.getAttribute('data-item-id'));

        ids.forEach(id => {
            const src = itemsList.querySelector(`[data-item-id="${id}"]`);
            if (!src) return;
            const type = src.querySelector('[data-field="type"]').value;
            const valueInput = src.querySelector('[data-field="value"]');
            const value = valueInput ? valueInput.value.trim() : '';
            const pill = document.createElement('div');
            pill.className = 'correct-order-item';
            pill.setAttribute('data-item-id', id);
            pill.style.display = 'inline-block';
            pill.style.margin = '4px';
            pill.style.padding = '4px 8px';
            pill.style.border = '1px solid #ddd';
            pill.style.borderRadius = '6px';
            pill.style.background = '#f8f9fa';
            pill.textContent = type === 'text' ? (value || '—') : (type === 'image' ? 'تصویر' : 'صوت');
            correctList.appendChild(pill);
        });

        // Enable drag-drop reordering for correct list
        this._initializeCorrectListDragDrop(correctList, block);
    }

    _readSettings(blockElement) {
        const settings = {};
        const inputs = blockElement.querySelectorAll('[data-setting]');
        const processedKeys = new Set();
        
        inputs.forEach(input => {
            const key = input.getAttribute('data-setting');
            if (!key || processedKeys.has(key)) return;
            
            if (input.type === 'checkbox') {
                settings[key] = input.checked;
                processedKeys.add(key);
            } else if (input.type === 'radio') {
                // For radio buttons, only read the checked one
                const checkedRadio = blockElement.querySelector(`[data-setting="${key}"]:checked`);
                if (checkedRadio) {
                    settings[key] = checkedRadio.value;
                    processedKeys.add(key);
                }
            } else {
                settings[key] = input.value;
                processedKeys.add(key);
            }
        });
        return settings;
    }

    _initializeCorrectListDragDrop(correctList, block) {
        if (!correctList || correctList._orderingDnDBound) return;
        correctList._orderingDnDBound = true;
        let dragSrcEl = null;
        correctList.addEventListener('dragstart', (e) => {
            const el = e.target.closest('.correct-order-item');
            if (!el) return;
            dragSrcEl = el;
            el.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
            try { e.dataTransfer.setData('text/plain', el.dataset.itemId); } catch (_) { }
        });
        // Set draggable on children
        Array.from(correctList.children).forEach(ch => ch.setAttribute('draggable', 'true'));
        correctList.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            const el = e.target.closest('.correct-order-item');
            if (!el || el === dragSrcEl) return;
            const rect = el.getBoundingClientRect();
            const next = (e.clientX - rect.left) / rect.width > 0.5; // horizontal
            correctList.insertBefore(dragSrcEl, next ? el.nextSibling : el);
        });
        correctList.addEventListener('drop', (e) => { e.preventDefault(); });
        correctList.addEventListener('dragend', () => {
            if (dragSrcEl) dragSrcEl.classList.remove('dragging');
            // Update block.data.correctOrder to reflect new order
            const ids = Array.from(correctList.querySelectorAll('.correct-order-item'))
                .map(el => el.getAttribute('data-item-id'));
            block.data = block.data || {};
            block.data.correctOrder = ids;
            if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                this.contentManager.updateHiddenField();
            }
        });
    }

    applySettingsToUi(blockElement, block) {
        if (!block || !block.data) return;
        const inputs = blockElement.querySelectorAll('[data-setting]');
        const processedKeys = new Set();
        
        // Handle backward compatibility for direction
        if (block.data.direction === 'horizontal') {
            block.data.direction = 'horizontal-ltr';
        }
        
        inputs.forEach(input => {
            const key = input.getAttribute('data-setting');
            if (!key || processedKeys.has(key)) return;
            if (block.data[key] === undefined) return;
            
            if (input.type === 'checkbox') {
                input.checked = !!block.data[key];
                processedKeys.add(key);
            } else if (input.type === 'radio') {
                // For radio buttons, check the one matching the value
                let value = block.data[key];
                const matchingRadio = blockElement.querySelector(`[data-setting="${key}"][value="${value}"]`);
                if (matchingRadio) {
                    matchingRadio.checked = true;
                    processedKeys.add(key);
                }
            } else {
                input.value = block.data[key];
                processedKeys.add(key);
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
    window.OrderingHandler = OrderingHandler;
}
