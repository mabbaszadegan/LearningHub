/**
 * Modern Content Builder for Educational Content
 * Handles text, image, video, and audio content boxes with drag & drop
 */

class ContentBuilder {
    constructor() {
        this.contentBoxes = [];
        this.currentBoxId = 0;
        this.mediaRecorder = null;
        this.recordingTimer = null;
        this.recordingStartTime = null;
        this.isRecording = false;
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.setupDragAndDrop();
        this.setupTextEditor();
        this.setupFileUploads();
        this.setupAudioRecording();
    }

    setupEventListeners() {
        // Add content box button
        const addBtn = document.getElementById('addContentBoxBtn');
        if (addBtn) {
            addBtn.addEventListener('click', () => this.showContentBoxTypeModal());
        }

        // Preview content button
        const previewBtn = document.getElementById('previewContentBtn');
        if (previewBtn) {
            previewBtn.addEventListener('click', () => this.previewContent());
        }

        // Save content button
        const saveBtn = document.getElementById('saveContentBtn');
        if (saveBtn) {
            saveBtn.addEventListener('click', () => this.saveContent());
        }

        // Quick template buttons
        document.addEventListener('click', (e) => {
            if (e.target.closest('.template-btn')) {
                const template = e.target.closest('.template-btn').dataset.template;
                this.addQuickTemplate(template);
            }
        });

        // Content box type selection
        document.addEventListener('click', (e) => {
            if (e.target.closest('.box-type-item')) {
                const type = e.target.closest('.box-type-item').dataset.type;
                this.addContentBox(type);
                this.hideContentBoxTypeModal();
            }
        });

        // Box actions
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-icon')) {
                const btn = e.target.closest('.btn-icon');
                const action = btn.dataset.action;
                const box = btn.closest('.content-box-template');
                
                if (action === 'delete') {
                    this.removeContentBox(box);
                } else if (action === 'settings') {
                    this.toggleBoxSettings(box);
                } else if (action === 'move-up') {
                    this.moveBoxUp(box);
                } else if (action === 'move-down') {
                    this.moveBoxDown(box);
                }
            }
        });

        // File input changes
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('image-file-input')) {
                this.handleImageUpload(e.target);
            } else if (e.target.classList.contains('video-file-input')) {
                this.handleVideoUpload(e.target);
            } else if (e.target.classList.contains('audio-file-input')) {
                this.handleAudioUpload(e.target);
            }
        });

        // Overlay actions
        document.addEventListener('click', (e) => {
            if (e.target.closest('.overlay-btn')) {
                const btn = e.target.closest('.overlay-btn');
                const action = btn.dataset.action;
                const box = btn.closest('.content-box-template');
                
                if (action === 'change') {
                    this.changeFile(box);
                } else if (action === 'remove') {
                    this.removeFile(box);
                }
            }
        });

        // Settings changes
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('image-size-select') ||
                e.target.classList.contains('image-align-select') ||
                e.target.classList.contains('video-size-select') ||
                e.target.classList.contains('video-align-select') ||
                e.target.classList.contains('caption-position-select')) {
                this.updateBoxSettings(e.target);
            }
        });

        // Caption input changes
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('image-caption-input') ||
                e.target.classList.contains('video-caption-input') ||
                e.target.classList.contains('audio-caption-input')) {
                this.updateBoxSettings(e.target);
            }
        });

        // Text editor content changes
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('text-editor-content')) {
                this.updateTextContent(e.target);
            }
        });
    }

    setupDragAndDrop() {
        const container = document.getElementById('contentBoxesList');
        if (!container) return;

        // Make container droppable
        container.addEventListener('dragover', (e) => {
            e.preventDefault();
            container.classList.add('drag-over');
        });

        container.addEventListener('dragleave', (e) => {
            if (!container.contains(e.relatedTarget)) {
                container.classList.remove('drag-over');
            }
        });

        container.addEventListener('drop', (e) => {
            e.preventDefault();
            container.classList.remove('drag-over');
            
            const draggedBox = document.querySelector('.content-box-template.dragging');
            if (draggedBox) {
                container.appendChild(draggedBox);
                draggedBox.classList.remove('dragging');
                this.updateBoxOrder();
            }
        });

        // Make boxes draggable
        container.addEventListener('dragstart', (e) => {
            if (e.target.closest('.content-box-template')) {
                const box = e.target.closest('.content-box-template');
                box.classList.add('dragging');
                e.dataTransfer.effectAllowed = 'move';
            }
        });

        container.addEventListener('dragend', (e) => {
            const box = e.target.closest('.content-box-template');
            if (box) {
                box.classList.remove('dragging');
            }
        });
    }

    setupTextEditor() {
        // Text editor toolbar
        document.addEventListener('click', (e) => {
            if (e.target.closest('.toolbar-btn')) {
                const btn = e.target.closest('.toolbar-btn');
                const command = btn.dataset.command;
                const editor = btn.closest('.text-editor-container').querySelector('.text-editor-content');
                
                this.executeTextCommand(editor, command);
            }
        });

        // Font size and color changes
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('font-size-select')) {
                const editor = e.target.closest('.text-editor-container').querySelector('.text-editor-content');
                this.executeTextCommand(editor, 'fontSize', e.target.value);
            } else if (e.target.classList.contains('color-picker')) {
                const editor = e.target.closest('.text-editor-container').querySelector('.text-editor-content');
                this.executeTextCommand(editor, 'foreColor', e.target.value);
            }
        });
    }

    setupFileUploads() {
        // Drag and drop for file uploads
        document.addEventListener('dragover', (e) => {
            if (e.target.closest('.image-upload-area, .video-upload-area, .audio-upload-area')) {
                e.preventDefault();
            }
        });

        document.addEventListener('drop', (e) => {
            const uploadArea = e.target.closest('.image-upload-area, .video-upload-area, .audio-upload-area');
            if (uploadArea) {
                e.preventDefault();
                const files = e.dataTransfer.files;
                if (files.length > 0) {
                    this.handleFileDrop(uploadArea, files[0]);
                }
            }
        });

        // Click to upload
        document.addEventListener('click', (e) => {
            if (e.target.closest('.image-upload-area, .video-upload-area, .audio-upload-area')) {
                const uploadArea = e.target.closest('.image-upload-area, .video-upload-area, .audio-upload-area');
                const fileInput = uploadArea.querySelector('input[type="file"]');
                if (fileInput) {
                    fileInput.click();
                }
            }
        });
    }

    setupAudioRecording() {
        // Try both possible IDs
        const recordBtn = document.getElementById('recordBtn') || document.getElementById('startRecording');
        const stopBtn = document.getElementById('stopRecordBtn') || document.getElementById('stopRecording');
        const timer = document.getElementById('recordingTimer');
        const controls = document.getElementById('recordingControls');

        if (recordBtn) {
            recordBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.startRecording();
            });
        }

        if (stopBtn) {
            stopBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.stopRecording();
            });
        }
    }

    showContentBoxTypeModal() {
        const modal = document.getElementById('contentBoxTypeModal');
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        }
    }

    hideContentBoxTypeModal() {
        const modal = document.getElementById('contentBoxTypeModal');
        if (modal) {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
        }
    }

    addContentBox(type) {
        const container = document.getElementById('contentBoxesContainer');
        const templates = document.getElementById('contentBoxTemplates');
        
        if (!container || !templates) return;

        const template = templates.querySelector(`[data-type="${type}"]`);
        if (!template) return;

        // Hide empty state
        const emptyState = document.getElementById('emptyState');
        if (emptyState) {
            emptyState.style.display = 'none';
        }

        const newBox = template.cloneNode(true);
        newBox.id = `content-box-${++this.currentBoxId}`;
        newBox.dataset.boxId = this.currentBoxId;
        newBox.dataset.type = type;
        newBox.style.display = 'block';
        newBox.draggable = true;

        // Clear template content
        this.clearTemplateContent(newBox, type);

        // Add to container
        container.appendChild(newBox);

        // Add to data structure
        this.contentBoxes.push({
            id: this.currentBoxId,
            type: type,
            data: this.getDefaultDataForType(type)
        });

        // Update order
        this.updateBoxOrder();

        // Scroll to new box
        newBox.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    clearTemplateContent(box, type) {
        switch (type) {
            case 'text':
                const textEditor = box.querySelector('.text-editor-content');
                if (textEditor) {
                    textEditor.innerHTML = '';
                }
                break;
            case 'image':
                const imagePreview = box.querySelector('.image-preview');
                const imageUpload = box.querySelector('.image-upload-area');
                if (imagePreview) imagePreview.style.display = 'none';
                if (imageUpload) imageUpload.style.display = 'block';
                break;
            case 'video':
                const videoPreview = box.querySelector('.video-preview');
                const videoUpload = box.querySelector('.video-upload-area');
                if (videoPreview) videoPreview.style.display = 'none';
                if (videoUpload) videoUpload.style.display = 'block';
                break;
            case 'audio':
                const audioPreview = box.querySelector('.audio-preview');
                const audioUpload = box.querySelector('.audio-upload-area');
                if (audioPreview) audioPreview.style.display = 'none';
                if (audioUpload) audioUpload.style.display = 'block';
                break;
        }
    }

    getDefaultDataForType(type) {
        switch (type) {
            case 'text':
                return { content: '', style: {} };
            case 'image':
                return { fileId: null, size: 'medium', align: 'center', caption: '', captionPosition: 'bottom' };
            case 'video':
                return { fileId: null, size: 'medium', align: 'center', caption: '', captionPosition: 'bottom' };
            case 'audio':
                return { fileId: null, caption: '' };
            default:
                return {};
        }
    }

    removeContentBox(box) {
        if (!box) return;

        const boxId = parseInt(box.dataset.boxId);
        
        // Remove from DOM
        box.remove();

        // Remove from data structure
        this.contentBoxes = this.contentBoxes.filter(b => b.id !== boxId);

        // Show empty state if no boxes left
        if (this.contentBoxes.length === 0) {
            const emptyState = document.getElementById('emptyState');
            if (emptyState) {
                emptyState.style.display = 'block';
            }
        }

        // Update order
        this.updateBoxOrder();
    }

    toggleBoxSettings(box) {
        const settings = box.querySelector('.image-settings, .video-settings, .audio-settings');
        if (settings) {
            settings.style.display = settings.style.display === 'none' ? 'block' : 'none';
        }
    }

    updateBoxSettings(element) {
        const box = element.closest('.content-box-template');
        if (!box) return;

        const boxId = parseInt(box.dataset.boxId);
        const boxData = this.contentBoxes.find(b => b.id === boxId);
        if (!boxData) return;

        const type = boxData.type;
        const name = element.className.split(' ')[0];

        switch (name) {
            case 'image-size-select':
            case 'video-size-select':
                boxData.data.size = element.value;
                break;
            case 'image-align-select':
            case 'video-align-select':
                boxData.data.align = element.value;
                break;
            case 'caption-position-select':
                boxData.data.captionPosition = element.value;
                break;
            case 'image-caption-input':
            case 'video-caption-input':
            case 'audio-caption-input':
                boxData.data.caption = element.value;
                break;
        }
    }

    updateTextContent(element) {
        const box = element.closest('.content-box-template');
        if (!box) return;

        const boxId = parseInt(box.dataset.boxId);
        const boxData = this.contentBoxes.find(b => b.id === boxId);
        if (!boxData) return;

        boxData.data.content = element.innerHTML;
    }

    updateBoxOrder() {
        const container = document.getElementById('contentBoxesList');
        if (!container) return;

        const boxes = container.querySelectorAll('.content-box-template');
        boxes.forEach((box, index) => {
            const boxId = parseInt(box.dataset.boxId);
            const boxData = this.contentBoxes.find(b => b.id === boxId);
            if (boxData) {
                boxData.order = index;
            }
        });
    }

    executeTextCommand(editor, command, value = null) {
        if (!editor) return;

        editor.focus();
        
        switch (command) {
            case 'bold':
            case 'italic':
            case 'underline':
                document.execCommand(command, false, null);
                break;
            case 'insertUnorderedList':
            case 'insertOrderedList':
                document.execCommand(command, false, null);
                break;
            case 'fontSize':
                if (value) {
                    document.execCommand('fontSize', false, '7');
                    const fontElements = editor.querySelectorAll('font[size="7"]');
                    fontElements.forEach(el => {
                        el.removeAttribute('size');
                        el.style.fontSize = value;
                    });
                }
                break;
            case 'foreColor':
                if (value) {
                    document.execCommand('foreColor', false, value);
                }
                break;
        }

        // Update button states
        this.updateToolbarStates(editor);
    }

    updateToolbarStates(editor) {
        const toolbar = editor.closest('.text-editor-container').querySelector('.text-editor-toolbar');
        if (!toolbar) return;

        const buttons = toolbar.querySelectorAll('.toolbar-btn');
        buttons.forEach(btn => {
            const command = btn.dataset.command;
            if (command && ['bold', 'italic', 'underline'].includes(command)) {
                btn.classList.toggle('active', document.queryCommandState(command));
            }
        });
    }

    handleImageUpload(input) {
        const file = input.files[0];
        if (!file) return;

        if (!file.type.startsWith('image/')) {
            this.showError('لطفاً یک فایل تصویری انتخاب کنید.');
            return;
        }

        this.uploadFile(file, 'image', input);
    }

    handleVideoUpload(input) {
        const file = input.files[0];
        if (!file) return;

        if (!file.type.startsWith('video/')) {
            this.showError('لطفاً یک فایل ویدیویی انتخاب کنید.');
            return;
        }

        this.uploadFile(file, 'video', input);
    }

    handleAudioUpload(input) {
        const file = input.files[0];
        if (!file) return;

        if (!file.type.startsWith('audio/')) {
            this.showError('لطفاً یک فایل صوتی انتخاب کنید.');
            return;
        }

        this.uploadFile(file, 'audio', input);
    }

    handleFileDrop(uploadArea, file) {
        const box = uploadArea.closest('.content-box-template');
        if (!box) return;

        const type = box.dataset.type;
        
        if (type === 'image' && !file.type.startsWith('image/')) {
            this.showError('لطفاً یک فایل تصویری انتخاب کنید.');
            return;
        }
        
        if (type === 'video' && !file.type.startsWith('video/')) {
            this.showError('لطفاً یک فایل ویدیویی انتخاب کنید.');
            return;
        }
        
        if (type === 'audio' && !file.type.startsWith('audio/')) {
            this.showError('لطفاً یک فایل صوتی انتخاب کنید.');
            return;
        }

        this.uploadFile(file, type, uploadArea);
    }

    async uploadFile(file, type, element) {
        const box = element.closest('.content-box-template');
        if (!box) return;

        // Show loading state
        box.classList.add('loading');

        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', type);

            const response = await fetch('/FileUpload/UploadContentFile', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });

            if (!response.ok) {
                throw new Error('خطا در آپلود فایل');
            }

            const result = await response.json();
            
            if (result.success) {
                this.showFilePreview(box, result.data, type);
                this.updateBoxData(box, 'fileId', result.data.id);
            } else {
                throw new Error(result.message || 'خطا در آپلود فایل');
            }
        } catch (error) {
            this.showError(error.message);
        } finally {
            box.classList.remove('loading');
        }
    }

    showFilePreview(box, fileData, type) {
        const uploadArea = box.querySelector(`.${type}-upload-area`);
        const preview = box.querySelector(`.${type}-preview`);
        
        if (uploadArea) uploadArea.style.display = 'none';
        if (preview) {
            preview.style.display = 'block';
            
            if (type === 'image') {
                const img = preview.querySelector('.preview-image');
                if (img) img.src = fileData.url || '/uploads/' + fileData.fileName;
            } else if (type === 'video') {
                const video = preview.querySelector('.preview-video');
                if (video) {
                    const source = video.querySelector('source');
                    if (source) source.src = fileData.url || '/uploads/' + fileData.fileName;
                    video.load();
                }
            } else if (type === 'audio') {
                const audio = preview.querySelector('.preview-audio');
                if (audio) {
                    const source = audio.querySelector('source');
                    if (source) source.src = fileData.url || '/uploads/' + fileData.fileName;
                    audio.load();
                }
            }
        }
    }

    updateBoxData(box, key, value) {
        const boxId = parseInt(box.dataset.boxId);
        const boxData = this.contentBoxes.find(b => b.id === boxId);
        if (boxData) {
            boxData.data[key] = value;
        }
    }

    changeFile(box) {
        const type = box.dataset.type;
        const fileInput = box.querySelector(`.${type}-file-input`);
        if (fileInput) {
            fileInput.click();
        }
    }

    removeFile(box) {
        const type = box.dataset.type;
        const uploadArea = box.querySelector(`.${type}-upload-area`);
        const preview = box.querySelector(`.${type}-preview`);
        
        if (uploadArea) uploadArea.style.display = 'block';
        if (preview) preview.style.display = 'none';
        
        this.updateBoxData(box, 'fileId', null);
    }

    async startRecording() {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            this.mediaRecorder = new MediaRecorder(stream);
            
            const chunks = [];
            this.mediaRecorder.ondataavailable = (e) => chunks.push(e.data);
            
            this.mediaRecorder.onstop = () => {
                const blob = new Blob(chunks, { type: 'audio/webm' });
                this.handleRecordedAudio(blob);
                stream.getTracks().forEach(track => track.stop());
            };
            
            this.mediaRecorder.start();
            this.isRecording = true;
            this.recordingStartTime = Date.now();
            
            // Update UI
            document.getElementById('recordBtn').style.display = 'none';
            document.getElementById('recordingControls').style.display = 'flex';
            
            // Start timer
            this.recordingTimer = setInterval(() => {
                const elapsed = Date.now() - this.recordingStartTime;
                const minutes = Math.floor(elapsed / 60000);
                const seconds = Math.floor((elapsed % 60000) / 1000);
                document.getElementById('recordingTimer').textContent = 
                    `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
            }, 1000);
            
        } catch (error) {
            this.showError('خطا در دسترسی به میکروفون');
        }
    }

    stopRecording() {
        if (this.mediaRecorder && this.isRecording) {
            this.mediaRecorder.stop();
            this.isRecording = false;
            
            if (this.recordingTimer) {
                clearInterval(this.recordingTimer);
                this.recordingTimer = null;
            }
            
            // Update UI
            document.getElementById('recordBtn').style.display = 'flex';
            document.getElementById('recordingControls').style.display = 'none';
        }
    }

    async handleRecordedAudio(blob) {
        // Convert blob to file
        const file = new File([blob], 'recording.webm', { type: 'audio/webm' });
        
        // Find the current audio box
        const audioBox = document.querySelector('.content-box-template[data-type="audio"]:last-child');
        if (audioBox) {
            await this.uploadFile(file, 'audio', audioBox);
        }
    }

    getContentData() {
        return {
            type: 'reminder',
            boxes: this.contentBoxes.map(box => ({
                id: box.id,
                type: box.type,
                order: box.order || 0,
                data: box.data
            }))
        };
    }

    loadContentData(data) {
        if (!data || !data.boxes) return;

        this.contentBoxes = [];
        this.currentBoxId = 0;
        
        const container = document.getElementById('contentBoxesList');
        if (container) {
            container.innerHTML = '';
        }

        data.boxes.forEach(boxData => {
            this.addContentBox(boxData.type);
            const box = document.querySelector(`[data-box-id="${boxData.id}"]`);
            if (box) {
                this.loadBoxData(box, boxData.data);
            }
        });
    }

    loadBoxData(box, data) {
        const type = box.dataset.type;
        
        switch (type) {
            case 'text':
                const textEditor = box.querySelector('.text-editor-content');
                if (textEditor && data.content) {
                    textEditor.innerHTML = data.content;
                }
                break;
            case 'image':
            case 'video':
            case 'audio':
                if (data.fileId) {
                    // Load file preview
                    this.loadFilePreview(box, data.fileId, type);
                }
                
                // Load settings
                if (data.size) {
                    const sizeSelect = box.querySelector(`.${type}-size-select`);
                    if (sizeSelect) sizeSelect.value = data.size;
                }
                
                if (data.align) {
                    const alignSelect = box.querySelector(`.${type}-align-select`);
                    if (alignSelect) alignSelect.value = data.align;
                }
                
                if (data.caption) {
                    const captionInput = box.querySelector(`.${type}-caption-input`);
                    if (captionInput) captionInput.value = data.caption;
                }
                
                if (data.captionPosition) {
                    const positionSelect = box.querySelector('.caption-position-select');
                    if (positionSelect) positionSelect.value = data.captionPosition;
                }
                break;
        }
    }

    //async loadFilePreview(box, fileId, type) {
    //    try {
    //        const response = await fetch(`./FileUpload/GetFile/${fileId}`);
    //        if (response.ok) {
    //            const fileData = await response.json();
    //            this.showFilePreview(box, fileData, type);
    //            this.updateBoxData(box, 'fileId', fileId);
    //        }
    //    } catch (error) {
    //    }
    //}

    addQuickTemplate(template) {
        const emptyState = document.getElementById('emptyState');
        if (emptyState) {
            emptyState.style.display = 'none';
        }

        switch (template) {
            case 'text':
                this.addContentBox('text');
                break;
            case 'image-text':
                this.addContentBox('image');
                setTimeout(() => this.addContentBox('text'), 100);
                break;
            case 'video-text':
                this.addContentBox('video');
                setTimeout(() => this.addContentBox('text'), 100);
                break;
            default:
                this.addContentBox('text');
        }
    }

    moveBoxUp(box) {
        const container = document.getElementById('contentBoxesContainer');
        const previousSibling = box.previousElementSibling;
        
        if (previousSibling && container) {
            container.insertBefore(box, previousSibling);
            this.updateBoxOrder();
        }
    }

    moveBoxDown(box) {
        const container = document.getElementById('contentBoxesContainer');
        const nextSibling = box.nextElementSibling;
        
        if (nextSibling && container) {
            container.insertBefore(nextSibling, box);
            this.updateBoxOrder();
        }
    }

    previewContent() {
        const contentData = this.getContentData();
        this.showContentPreview(contentData);
    }

    saveContent() {
        const contentData = this.getContentData();
        const contentJson = JSON.stringify(contentData);
        
        // Update the hidden content field
        const contentField = document.getElementById('contentJson');
        if (contentField) {
            contentField.value = contentJson;
        }

        this.showSuccess('محتوای آموزشی با موفقیت ذخیره شد.');
    }

    showContentPreview(contentData) {
        const modal = document.getElementById('previewModal');
        const previewContent = document.getElementById('previewContent');
        
        if (!modal || !previewContent) return;

        // Generate preview HTML
        let previewHTML = '<div class="content-preview">';
        
        contentData.boxes.forEach(box => {
            switch (box.type) {
                case 'text':
                    previewHTML += `<div class="preview-text-box">${box.data.content || ''}</div>`;
                    break;
                case 'image':
                    if (box.data.imageUrl) {
                        previewHTML += `<div class="preview-image-box">
                            <img src="${box.data.imageUrl}" alt="${box.data.caption || ''}" style="max-width: 100%; height: auto;">
                            ${box.data.caption ? `<p class="image-caption">${box.data.caption}</p>` : ''}
                        </div>`;
                    }
                    break;
                case 'video':
                    if (box.data.videoUrl) {
                        previewHTML += `<div class="preview-video-box">
                            <video controls style="max-width: 100%; height: auto;">
                                <source src="${box.data.videoUrl}" type="video/mp4">
                            </video>
                            ${box.data.caption ? `<p class="video-caption">${box.data.caption}</p>` : ''}
                        </div>`;
                    }
                    break;
                case 'audio':
                    if (box.data.audioUrl) {
                        previewHTML += `<div class="preview-audio-box">
                            <audio controls style="width: 100%;">
                                <source src="${box.data.audioUrl}" type="audio/mpeg">
                            </audio>
                            ${box.data.caption ? `<p class="audio-caption">${box.data.caption}</p>` : ''}
                        </div>`;
                    }
                    break;
            }
        });
        
        previewHTML += '</div>';
        previewContent.innerHTML = previewHTML;

        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }

    showError(message) {
        // You can implement a toast notification system here
        alert(message);
    }

    showSuccess(message) {
        // You can implement a toast notification system here
        // Show a simple success message
        const successDiv = document.createElement('div');
        successDiv.className = 'alert alert-success alert-dismissible fade show';
        successDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        const container = document.querySelector('.content-builder');
        if (container) {
            container.insertBefore(successDiv, container.firstChild);
            
            // Auto remove after 3 seconds
            setTimeout(() => {
                if (successDiv.parentNode) {
                    successDiv.remove();
                }
            }, 3000);
        }
    }
}

// Initialize content builder when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.contentBuilder = new ContentBuilder();
});
