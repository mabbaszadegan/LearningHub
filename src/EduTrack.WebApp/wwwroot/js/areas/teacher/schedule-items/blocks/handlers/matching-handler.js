/**
 * Matching Block Handler
 * Handles matching question blocks
 */

class MatchingHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
    }

    canHandle(blockType) {
        return blockType === 'matching';
    }

    render(block) {
        const blockTemplatesContainer = document.getElementById('blockTemplatesContainer');
        if (!blockTemplatesContainer) {
            console.error('MatchingHandler: blockTemplatesContainer not found');
            return null;
        }

        const templatesContainer = blockTemplatesContainer.querySelector('#questionTypeBlockTemplates');
        if (!templatesContainer) {
            console.error('MatchingHandler: questionTypeBlockTemplates not found');
            return null;
        }

        const template = templatesContainer.querySelector('[data-type="matching"]');
        if (!template) {
            console.error('MatchingHandler: Matching template not found');
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

        // Initialize matching items if they exist
        if (block.data) {
            // Support both old structure (leftItems/rightItems) and new structure (items)
            if (block.data.items && Array.isArray(block.data.items)) {
                this.renderItems(blockElement, block);
            } else if ((block.data.leftItems && block.data.leftItems.length > 0) || 
                       (block.data.rightItems && block.data.rightItems.length > 0)) {
                // Migrate old structure to new structure
                this.migrateOldStructure(block);
                this.renderItems(blockElement, block);
            }
        }

        // Setup event listeners
        const addItemBtn = blockElement.querySelector('[data-action="add-matching-item"]');
        if (addItemBtn) {
            addItemBtn.addEventListener('click', () => this.addItem(blockElement, block));
        }

        // Settings change listeners
        const settingsInputs = blockElement.querySelectorAll('[data-setting]');
        settingsInputs.forEach(input => {
            const eventType = input.tagName === 'TEXTAREA' || input.type === 'text' ? 'input' : 'change';
            input.addEventListener(eventType, () => {
                const key = input.getAttribute('data-setting');
                let value;
                if (input.type === 'checkbox') {
                    value = input.checked;
                } else {
                    const rawValue = input.value;
                    value = typeof rawValue === 'string' ? rawValue.trim() : rawValue;
                }
                block.data = block.data || {};
                block.data[key] = value;
                if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                    this.contentManager.updateHiddenField();
                }
            });
        });
    }

    addItem(blockElement, block) {
        const itemsList = blockElement.querySelector('[data-role="matching-items"]');
        if (!itemsList) return;

        const newItem = {
            id: this._generateItemId(block),
            leftType: 'text',
            leftText: '',
            rightType: 'text',
            rightText: ''
        };
        
        block.data = block.data || {};
        block.data.items = block.data.items || [];
        block.data.items.push(newItem);

        const itemEl = this._createItemElement(newItem, block);
        itemsList.appendChild(itemEl);

        // Focus first input if it's text type
        const leftInput = itemEl.querySelector('[data-field="left-text"]');
        if (leftInput) leftInput.focus();

        if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
            this.contentManager.updateHiddenField();
        }
    }

    renderItems(blockElement, block) {
        const itemsList = blockElement.querySelector('[data-role="matching-items"]');
        if (!itemsList) return;
        itemsList.innerHTML = '';

        (block.data.items || []).forEach(item => {
            const itemEl = this._createItemElement(item, block);
            itemsList.appendChild(itemEl);
        });
    }

    collectData(blockElement, block) {
        const itemsList = blockElement.querySelector('[data-role="matching-items"]');

        const items = Array.from(itemsList.querySelectorAll('.matching-item')).map(el => {
            const id = el.getAttribute('data-item-id');
            
            // Left side
            const leftTypeSelect = el.querySelector('[data-field="left-type"]');
            const leftType = leftTypeSelect ? leftTypeSelect.value : 'text';
            
            let leftData = { type: leftType };
            if (leftType === 'text') {
                const leftInput = el.querySelector('[data-field="left-text"]');
                leftData.text = leftInput ? leftInput.value.trim() : '';
            } else {
                leftData.fileId = el.getAttribute('data-left-file-id') ? parseInt(el.getAttribute('data-left-file-id'), 10) : null;
                leftData.fileName = el.getAttribute('data-left-file-name') || null;
                leftData.fileUrl = el.getAttribute('data-left-file-url') || null;
                leftData.mimeType = el.getAttribute('data-left-mime') || null;
            }
            
            // Right side
            const rightTypeSelect = el.querySelector('[data-field="right-type"]');
            const rightType = rightTypeSelect ? rightTypeSelect.value : 'text';
            
            let rightData = { type: rightType };
            if (rightType === 'text') {
                const rightInput = el.querySelector('[data-field="right-text"]');
                rightData.text = rightInput ? rightInput.value.trim() : '';
            } else {
                rightData.fileId = el.getAttribute('data-right-file-id') ? parseInt(el.getAttribute('data-right-file-id'), 10) : null;
                rightData.fileName = el.getAttribute('data-right-file-name') || null;
                rightData.fileUrl = el.getAttribute('data-right-file-url') || null;
                rightData.mimeType = el.getAttribute('data-right-mime') || null;
            }
            
            return { 
                id, 
                leftType: leftData.type,
                leftText: leftData.text || '',
                leftFileId: leftData.fileId,
                leftFileName: leftData.fileName,
                leftFileUrl: leftData.fileUrl,
                leftMimeType: leftData.mimeType,
                rightType: rightData.type,
                rightText: rightData.text || '',
                rightFileId: rightData.fileId,
                rightFileName: rightData.fileName,
                rightFileUrl: rightData.fileUrl,
                rightMimeType: rightData.mimeType
            };
        });

        const settings = this._readSettings(blockElement);

        // Maintain backward compatibility with old structure (for text only)
        const leftItems = items.map((item, index) => ({
            Index: index,
            Text: item.leftType === 'text' ? item.leftText : (item.leftType === 'image' ? 'تصویر' : 'صوت')
        }));
        
        const rightItems = items.map((item, index) => ({
            Index: index,
            Text: item.rightType === 'text' ? item.rightText : (item.rightType === 'image' ? 'تصویر' : 'صوت')
        }));

        const connections = items.map((item, index) => ({
            LeftIndex: index,
            RightIndex: index
        }));

        block.data = Object.assign({}, block.data, settings, {
            items,
            leftItems,
            rightItems,
            connections
        });

        return block.data;
    }

    migrateOldStructure(block) {
        // Convert old structure (leftItems/rightItems) to new structure (items)
        if (!block.data.leftItems || !block.data.rightItems) return;
        
        const items = [];
        const maxLength = Math.max(block.data.leftItems.length, block.data.rightItems.length);
        
        for (let i = 0; i < maxLength; i++) {
            items.push({
                id: this._generateItemId(block),
                leftType: 'text',
                leftText: block.data.leftItems[i]?.Text || '',
                rightType: 'text',
                rightText: block.data.rightItems[i]?.Text || ''
            });
        }
        
        block.data.items = items;
    }

    // Helpers
    _generateItemId(block) {
        const base = (block && block.id) ? block.id : 'block';
        const ts = Date.now();
        const rnd = Math.floor(Math.random() * 100000);
        return `${base}-item-${ts}-${rnd}`;
    }

    _createItemElement(item, block) {
        const div = document.createElement('div');
        div.className = 'matching-item';
        div.setAttribute('data-item-id', item.id);

        const leftType = item.leftType || 'text';
        const rightType = item.rightType || 'text';

        div.innerHTML = `
            <div class="matching-item-inner">
                <div class="matching-item-controls" style="display: flex; gap: 1rem; align-items: flex-start; width: 100%; margin-bottom: 0.5rem;">
                    <div class="matching-side matching-side-left" style="flex: 1;">
                        <label style="display: block; font-size: 12px; margin-bottom: 4px; color: #6b7280; font-weight: 500;">سمت چپ:</label>
                        <select data-field="left-type" class="form-select form-select-sm" style="width: 100%; margin-bottom: 6px;">
                            <option value="text" ${leftType === 'text' ? 'selected' : ''}>متن</option>
                            <option value="image" ${leftType === 'image' ? 'selected' : ''}>تصویر</option>
                            <option value="audio" ${leftType === 'audio' ? 'selected' : ''}>صوت</option>
                        </select>
                        <div class="matching-left-inputs" style="margin-top: 6px;"></div>
                    </div>
                    <div style="flex: 0 0 auto; padding-top: 28px;">
                        <i class="fas fa-arrow-left" style="color: #9ca3af;"></i>
                    </div>
                    <div class="matching-side matching-side-right" style="flex: 1;">
                        <label style="display: block; font-size: 12px; margin-bottom: 4px; color: #6b7280; font-weight: 500;">سمت راست:</label>
                        <select data-field="right-type" class="form-select form-select-sm" style="width: 100%; margin-bottom: 6px;">
                            <option value="text" ${rightType === 'text' ? 'selected' : ''}>متن</option>
                            <option value="image" ${rightType === 'image' ? 'selected' : ''}>تصویر</option>
                            <option value="audio" ${rightType === 'audio' ? 'selected' : ''}>صوت</option>
                        </select>
                        <div class="matching-right-inputs" style="margin-top: 6px;"></div>
                    </div>
                    <div style="flex: 0 0 auto; padding-top: 28px;">
                        <button type="button" class="btn btn-sm btn-outline-danger" data-action="remove-item" title="حذف">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
                <div class="matching-item-preview" style="display: flex; gap: 1rem; margin-top: 0.5rem;">
                    <div class="matching-left-preview" style="flex: 1;"></div>
                    <div style="flex: 0 0 auto; width: 30px;"></div>
                    <div class="matching-right-preview" style="flex: 1;"></div>
                </div>
            </div>
        `;

        // Set file data attributes if they exist
        if (item.leftFileId) {
            div.setAttribute('data-left-file-id', String(item.leftFileId));
            if (item.leftFileName) div.setAttribute('data-left-file-name', item.leftFileName);
            if (item.leftFileUrl) div.setAttribute('data-left-file-url', item.leftFileUrl);
            if (item.leftMimeType) div.setAttribute('data-left-mime', item.leftMimeType);
        }
        if (item.rightFileId) {
            div.setAttribute('data-right-file-id', String(item.rightFileId));
            if (item.rightFileName) div.setAttribute('data-right-file-name', item.rightFileName);
            if (item.rightFileUrl) div.setAttribute('data-right-file-url', item.rightFileUrl);
            if (item.rightMimeType) div.setAttribute('data-right-mime', item.rightMimeType);
        }

        // Build inputs and bind events
        const leftTypeSelect = div.querySelector('[data-field="left-type"]');
        const rightTypeSelect = div.querySelector('[data-field="right-type"]');
        const leftInputsHost = div.querySelector('.matching-left-inputs');
        const rightInputsHost = div.querySelector('.matching-right-inputs');
        const removeBtn = div.querySelector('[data-action="remove-item"]');

        const buildInputsForSide = (side, currentType, inputsHost) => {
            inputsHost.innerHTML = '';
            if (currentType === 'text') {
                const input = document.createElement('input');
                input.type = 'text';
                input.className = 'form-control form-control-sm';
                input.setAttribute('data-field', `${side}-text`);
                input.placeholder = `متن ${side === 'left' ? 'سمت چپ' : 'سمت راست'}...`;
                const textValue = side === 'left' ? (item.leftText || '') : (item.rightText || '');
                input.value = textValue;
                inputsHost.appendChild(input);
                input.addEventListener('input', () => {
                    updateItemModel();
                    // Update preview when text changes
                    this._updatePreview(div, side, 'text', input.value.trim());
                });
            } else if (currentType === 'image') {
                const uploadBtn = document.createElement('button');
                uploadBtn.type = 'button';
                uploadBtn.className = 'btn btn-sm btn-outline-primary';
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
                    await this._handleItemFileUpload(div, block, item, fileInput.files[0], 'image', side);
                });
                // Show existing file if available
                const existingFileUrl = side === 'left' 
                    ? (item.leftFileUrl || div.getAttribute('data-left-file-url'))
                    : (item.rightFileUrl || div.getAttribute('data-right-file-url'));
                if (existingFileUrl && (side === 'left' ? (item.leftType === 'image') : (item.rightType === 'image'))) {
                    this._updatePreview(div, side, 'image', existingFileUrl);
                }
            } else if (currentType === 'audio') {
                const uploadBtn = document.createElement('button');
                uploadBtn.type = 'button';
                uploadBtn.className = 'btn btn-sm btn-outline-primary';
                uploadBtn.innerHTML = '<i class="fas fa-upload"></i> آپلود';
                uploadBtn.style.marginRight = '4px';
                const fileInput = document.createElement('input');
                fileInput.type = 'file';
                fileInput.accept = 'audio/*';
                fileInput.style.display = 'none';
                const recordBtn = document.createElement('button');
                recordBtn.type = 'button';
                recordBtn.className = 'btn btn-sm btn-outline-secondary';
                recordBtn.innerHTML = '<i class="fas fa-microphone"></i> ضبط';
                recordBtn.style.marginRight = '4px';
                const stopBtn = document.createElement('button');
                stopBtn.type = 'button';
                stopBtn.className = 'btn btn-sm btn-outline-danger';
                stopBtn.innerHTML = '<i class="fas fa-stop"></i>';
                stopBtn.disabled = true;
                inputsHost.appendChild(uploadBtn);
                inputsHost.appendChild(fileInput);
                inputsHost.appendChild(recordBtn);
                inputsHost.appendChild(stopBtn);
                uploadBtn.addEventListener('click', () => fileInput.click());
                fileInput.addEventListener('change', async () => {
                    if (!fileInput.files || !fileInput.files[0]) return;
                    await this._handleItemFileUpload(div, block, item, fileInput.files[0], 'audio', side);
                });
                let mediaRecorder = null;
                let chunks = [];
                let stream = null;
                recordBtn.addEventListener('click', async () => {
                    try {
                        stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                        mediaRecorder = new MediaRecorder(stream);
                        chunks = [];
                        mediaRecorder.ondataavailable = e => chunks.push(e.data);
                        mediaRecorder.onstop = async () => {
                            const blob = new Blob(chunks, { type: 'audio/wav' });
                            const file = new File([blob], `recording_${Date.now()}.wav`, { type: 'audio/wav' });
                            stream.getTracks().forEach(t => t.stop());
                            await this._handleItemFileUpload(div, block, item, file, 'audio', side);
                        };
                        mediaRecorder.start();
                        recordBtn.disabled = true;
                        stopBtn.disabled = false;
                    } catch (err) {
                        console.error('MatchingHandler recording error:', err);
                        alert('خطا در دسترسی به میکروفون');
                    }
                });
                stopBtn.addEventListener('click', () => {
                    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
                        mediaRecorder.stop();
                        recordBtn.disabled = false;
                        stopBtn.disabled = true;
                    }
                });
                // Show existing file if available
                const existingFileUrl = side === 'left' 
                    ? (item.leftFileUrl || div.getAttribute('data-left-file-url'))
                    : (item.rightFileUrl || div.getAttribute('data-right-file-url'));
                if (existingFileUrl && (side === 'left' ? (item.leftType === 'audio') : (item.rightType === 'audio'))) {
                    this._updatePreview(div, side, 'audio', existingFileUrl);
                }
            }
        };

        // Build initial inputs
        buildInputsForSide('left', leftType, leftInputsHost);
        buildInputsForSide('right', rightType, rightInputsHost);

        // Update previews with correct values
        const leftPreviewValue = leftType === 'text' 
            ? (item.leftText || '') 
            : (item.leftFileUrl || item.leftFileName || '');
        const rightPreviewValue = rightType === 'text' 
            ? (item.rightText || '') 
            : (item.rightFileUrl || item.rightFileName || '');
        this._updatePreview(div, 'left', leftType, leftPreviewValue);
        this._updatePreview(div, 'right', rightType, rightPreviewValue);

        const updateItemModel = () => {
            // Update in-memory data
            if (block && block.data && Array.isArray(block.data.items)) {
                const idx = block.data.items.findIndex(x => x.id === item.id);
                if (idx >= 0) {
                    block.data.items[idx].leftType = leftTypeSelect.value;
                    block.data.items[idx].rightType = rightTypeSelect.value;
                    if (leftTypeSelect.value === 'text') {
                        const leftInput = div.querySelector('[data-field="left-text"]');
                        block.data.items[idx].leftText = leftInput ? leftInput.value.trim() : '';
                    }
                    if (rightTypeSelect.value === 'text') {
                        const rightInput = div.querySelector('[data-field="right-text"]');
                        block.data.items[idx].rightText = rightInput ? rightInput.value.trim() : '';
                    }
                }
            }
            // Persist to hidden content
            if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                this.contentManager.updateHiddenField();
            }
        };

        leftTypeSelect.addEventListener('change', () => {
            buildInputsForSide('left', leftTypeSelect.value, leftInputsHost);
            updateItemModel();
            // Update preview - use existing file if available, otherwise use text input
            if (leftTypeSelect.value === 'text') {
                const leftInput = div.querySelector('[data-field="left-text"]');
                this._updatePreview(div, 'left', 'text', leftInput ? leftInput.value.trim() : '');
            } else {
                const fileUrl = div.getAttribute('data-left-file-url');
                this._updatePreview(div, 'left', leftTypeSelect.value, fileUrl || '');
            }
        });

        rightTypeSelect.addEventListener('change', () => {
            buildInputsForSide('right', rightTypeSelect.value, rightInputsHost);
            updateItemModel();
            // Update preview - use existing file if available, otherwise use text input
            if (rightTypeSelect.value === 'text') {
                const rightInput = div.querySelector('[data-field="right-text"]');
                this._updatePreview(div, 'right', 'text', rightInput ? rightInput.value.trim() : '');
            } else {
                const fileUrl = div.getAttribute('data-right-file-url');
                this._updatePreview(div, 'right', rightTypeSelect.value, fileUrl || '');
            }
        });

        if (removeBtn) {
            removeBtn.addEventListener('click', () => {
                // Remove from data
                if (block && block.data && Array.isArray(block.data.items)) {
                    block.data.items = block.data.items.filter(x => x.id !== item.id);
                }
                div.remove();
                if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                    this.contentManager.updateHiddenField();
                }
            });
        }

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
                const rawValue = input.value;
                settings[key] = typeof rawValue === 'string' ? rawValue.trim() : rawValue;
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

    _updatePreview(containerEl, side, type, value) {
        const preview = containerEl.querySelector(`.matching-${side}-preview`);
        if (!preview) return;
        preview.innerHTML = '';
        if (type === 'text') {
            const span = document.createElement('span');
            span.textContent = value || '—';
            span.style.fontSize = '12px';
            span.style.color = '#6b7280';
            preview.appendChild(span);
        } else if (type === 'image') {
            if (value) {
                const img = document.createElement('img');
                img.src = value;
                img.alt = 'matching image';
                img.style.maxWidth = '150px';
                img.style.maxHeight = '100px';
                img.style.borderRadius = '4px';
                img.style.border = '1px solid #e5e7eb';
                preview.appendChild(img);
            }
        } else if (type === 'audio') {
            if (value) {
                const audio = document.createElement('audio');
                audio.controls = true;
                audio.src = value;
                audio.style.maxWidth = '100%';
                preview.appendChild(audio);
            }
        }
    }

    async _handleItemFileUpload(itemElement, block, item, file, kind, side) {
        // Basic validation
        const mime = (file.type || '').toLowerCase();
        if (kind === 'image' && !mime.startsWith('image/')) return;
        if (kind === 'audio' && !mime.startsWith('audio/') && file.name && !/\.(mp3|wav|ogg)$/i.test(file.name)) return;

        // Show progress in preview
        const preview = itemElement.querySelector(`.matching-${side}-preview`);
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
            const fileId = data.id;
            const fileName = data.fileName || data.originalFileName || file.name;
            const fileUrl = data.url || `/FileUpload/GetFile/${data.id}`;
            const mimeType = data.mimeType || file.type || '';

            // Update item data
            if (side === 'left') {
                item.leftType = kind;
                item.leftFileId = fileId;
                item.leftFileName = fileName;
                item.leftFileUrl = fileUrl;
                item.leftMimeType = mimeType;
                delete item.leftText;
                
                // Update DOM attributes
                itemElement.setAttribute('data-left-file-id', String(fileId));
                if (fileName) itemElement.setAttribute('data-left-file-name', fileName);
                if (fileUrl) itemElement.setAttribute('data-left-file-url', fileUrl);
                if (mimeType) itemElement.setAttribute('data-left-mime', mimeType);
            } else {
                item.rightType = kind;
                item.rightFileId = fileId;
                item.rightFileName = fileName;
                item.rightFileUrl = fileUrl;
                item.rightMimeType = mimeType;
                delete item.rightText;
                
                // Update DOM attributes
                itemElement.setAttribute('data-right-file-id', String(fileId));
                if (fileName) itemElement.setAttribute('data-right-file-name', fileName);
                if (fileUrl) itemElement.setAttribute('data-right-file-url', fileUrl);
                if (mimeType) itemElement.setAttribute('data-right-mime', mimeType);
            }

            // Reflect in block data array
            if (block && block.data && Array.isArray(block.data.items)) {
                const idx = block.data.items.findIndex(x => x.id === item.id);
                if (idx >= 0) {
                    block.data.items[idx] = { ...block.data.items[idx], ...item };
                }
            }

            // Update preview
            this._updatePreview(itemElement, side, kind, fileUrl);

            if (this.contentManager && typeof this.contentManager.updateHiddenField === 'function') {
                this.contentManager.updateHiddenField();
            }
        } catch (err) {
            console.error('MatchingHandler upload error:', err);
            if (preview) {
                preview.innerHTML = '<div class="text-danger" style="font-size:12px;">خطا در آپلود</div>';
            }
        }
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
    window.MatchingHandler = MatchingHandler;
}

