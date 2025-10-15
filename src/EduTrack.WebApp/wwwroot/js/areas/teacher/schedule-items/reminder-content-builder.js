/**
 * Reminder Content Block Manager
 * Handles content block creation, editing, and management for reminder-type schedule items
 */

class ReminderContentBlockManager {
    constructor() {
        this.blocks = [];
        this.nextBlockId = 1;
        this.mediaRecorder = null;
        this.recordedChunks = [];
        this.recordingTimer = null;
        this.recordingStartTime = null;
        
        this.blocksList = document.getElementById('contentBlocksList');
        this.emptyState = document.getElementById('emptyBlocksState');
        this.preview = document.getElementById('reminderPreview');
        this.hiddenField = document.getElementById('reminderContentJson');
        
        this.init();
    }
    
    init() {
        this.setupEventListeners();
        this.loadExistingContent();
    }
    
    setupEventListeners() {
        // Add block button
        document.getElementById('addContentBlockBtn').addEventListener('click', () => {
            this.showBlockTypeModal();
        });
        
        // Preview button
        document.getElementById('previewReminderBtn').addEventListener('click', () => {
            this.updatePreview();
        });
        
        // Block type selection
        document.querySelectorAll('.block-type-item').forEach(item => {
            item.addEventListener('click', (e) => {
                const type = e.currentTarget.dataset.type;
                this.addBlock(type);
                this.hideBlockTypeModal();
            });
        });
        
        // File upload handlers
        this.setupFileUploadHandlers();
    }
    
    setupFileUploadHandlers() {
        // Image upload
        document.addEventListener('change', (e) => {
            if (e.target.matches('input[data-action="image-upload"]')) {
                this.handleImageUpload(e.target);
            } else if (e.target.matches('input[data-action="video-upload"]')) {
                this.handleVideoUpload(e.target);
            } else if (e.target.matches('input[data-action="audio-upload"]')) {
                this.handleAudioUpload(e.target);
            }
        });
        
        // Audio recording
        document.addEventListener('click', (e) => {
            if (e.target.matches('[data-action="start-recording"]')) {
                this.startRecording(e.target);
            } else if (e.target.matches('[data-action="stop-recording"]')) {
                this.stopRecording(e.target);
            } else if (e.target.matches('[data-action="play-recording"]')) {
                this.playRecording(e.target);
            }
        });
        
        // Block actions
        document.addEventListener('click', (e) => {
            if (e.target.matches('[data-action="move-up"]')) {
                this.moveBlockUp(e.target);
            } else if (e.target.matches('[data-action="move-down"]')) {
                this.moveBlockDown(e.target);
            } else if (e.target.matches('[data-action="insert-above"]')) {
                this.insertBlockAbove(e.target);
            } else if (e.target.matches('[data-action="edit"]')) {
                this.editBlock(e.target);
            } else if (e.target.matches('[data-action="delete"]')) {
                this.deleteBlock(e.target);
            }
        });
        
        // Settings changes
        document.addEventListener('change', (e) => {
            if (e.target.matches('[data-setting]')) {
                this.updateBlockSettings(e.target);
            }
        });
        
        // Caption changes
        document.addEventListener('input', (e) => {
            if (e.target.matches('[data-caption="true"]')) {
                this.updateBlockCaption(e.target);
            }
        });
        
        // Text editor toolbar
        document.addEventListener('click', (e) => {
            if (e.target.matches('.toolbar-btn')) {
                e.preventDefault();
                this.executeTextCommand(e.target);
            }
        });
        
        // Text editor content changes
        document.addEventListener('input', (e) => {
            if (e.target.matches('.rich-text-editor')) {
                this.updateTextBlockContent(e.target);
            }
        });
    }
    
    showBlockTypeModal() {
        const modal = new bootstrap.Modal(document.getElementById('blockTypeModal'));
        modal.show();
    }
    
    hideBlockTypeModal() {
        const modal = bootstrap.Modal.getInstance(document.getElementById('blockTypeModal'));
        if (modal) {
            modal.hide();
        }
    }
    
    addBlock(type) {
        const blockId = `block-${this.nextBlockId++}`;
        const block = {
            id: blockId,
            type: type,
            order: this.blocks.length,
            data: this.getDefaultBlockData(type)
        };
        
        this.blocks.push(block);
        this.renderBlock(block);
        this.updateEmptyState();
        this.updateHiddenField();
    }
    
    getDefaultBlockData(type) {
        switch (type) {
            case 'text':
                return {
                    content: '',
                    textContent: ''
                };
            case 'image':
                return {
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    size: 'medium',
                    position: 'center',
                    caption: '',
                    captionPosition: 'bottom'
                };
            case 'video':
                return {
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    size: 'medium',
                    position: 'center',
                    caption: '',
                    captionPosition: 'bottom'
                };
            case 'audio':
                return {
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    caption: '',
                    isRecorded: false,
                    duration: null
                };
            default:
                return {};
        }
    }
    
    renderBlock(block) {
        const template = document.querySelector(`#contentBlockTemplates .content-block-template[data-type="${block.type}"]`);
        if (!template) return;
        
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = block.id;
        
        // Update block content based on data
        this.updateBlockContent(blockElement, block);
        
        // Insert at correct position
        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }
    }
    
    updateBlockContent(element, block) {
        switch (block.type) {
            case 'text':
                const textEditor = element.querySelector('.rich-text-editor');
                if (textEditor && block.data.content) {
                    textEditor.innerHTML = block.data.content;
                }
                break;
            case 'image':
                if (block.data.fileUrl) {
                    const uploadArea = element.querySelector('.image-upload-area');
                    const preview = element.querySelector('.image-preview');
                    const img = element.querySelector('.preview-image');
                    
                    uploadArea.style.display = 'none';
                    preview.style.display = 'flex';
                    img.src = block.data.fileUrl;
                }
                break;
            case 'video':
                if (block.data.fileUrl) {
                    const uploadArea = element.querySelector('.video-upload-area');
                    const preview = element.querySelector('.video-preview');
                    const video = element.querySelector('.preview-video source');
                    
                    uploadArea.style.display = 'none';
                    preview.style.display = 'flex';
                    video.src = block.data.fileUrl;
                }
                break;
            case 'audio':
                if (block.data.fileUrl) {
                    const uploadArea = element.querySelector('.audio-upload-area');
                    const preview = element.querySelector('.audio-preview');
                    const audio = element.querySelector('.preview-audio source');
                    
                    uploadArea.style.display = 'none';
                    preview.style.display = 'flex';
                    audio.src = block.data.fileUrl;
                }
                break;
        }
        
        // Update settings
        element.querySelectorAll('[data-setting]').forEach(select => {
            const setting = select.dataset.setting;
            if (block.data[setting]) {
                select.value = block.data[setting];
            }
        });
        
        // Update captions
        element.querySelectorAll('[data-caption="true"]').forEach(textarea => {
            if (block.data.caption) {
                textarea.value = block.data.caption;
            }
        });
    }
    
    updateEmptyState() {
        if (this.blocks.length === 0) {
            this.emptyState.style.display = 'flex';
        } else {
            this.emptyState.style.display = 'none';
        }
    }
    
    moveBlockUp(button) {
        const blockElement = button.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex > 0) {
            // Swap blocks
            [this.blocks[blockIndex], this.blocks[blockIndex - 1]] = [this.blocks[blockIndex - 1], this.blocks[blockIndex]];
            
            // Update DOM
            const prevBlock = blockElement.previousElementSibling;
            if (prevBlock && prevBlock.classList.contains('content-block')) {
                this.blocksList.insertBefore(blockElement, prevBlock);
            }
            
            this.updateHiddenField();
        }
    }
    
    moveBlockDown(button) {
        const blockElement = button.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex < this.blocks.length - 1) {
            // Swap blocks
            [this.blocks[blockIndex], this.blocks[blockIndex + 1]] = [this.blocks[blockIndex + 1], this.blocks[blockIndex]];
            
            // Update DOM
            const nextBlock = blockElement.nextElementSibling;
            if (nextBlock && nextBlock.classList.contains('content-block')) {
                this.blocksList.insertBefore(nextBlock, blockElement);
            }
            
            this.updateHiddenField();
        }
    }
    
    insertBlockAbove(button) {
        const blockElement = button.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        this.showBlockTypeModal();
        // Store the insertion point for after modal selection
        this.insertionPoint = blockIndex;
    }
    
    deleteBlock(button) {
        if (confirm('آیا از حذف این بلاک اطمینان دارید؟')) {
            const blockElement = button.closest('.content-block');
            const blockId = blockElement.dataset.blockId;
            
            // Remove from blocks array
            this.blocks = this.blocks.filter(b => b.id !== blockId);
            
            // Remove from DOM
            blockElement.remove();
            
            this.updateEmptyState();
            this.updateHiddenField();
        }
    }
    
    editBlock(button) {
        const blockElement = button.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block.type === 'text') {
            const textEditor = blockElement.querySelector('.rich-text-editor');
            textEditor.focus();
        }
    }
    
    handleImageUpload(input) {
        const file = input.files[0];
        if (!file) return;
        
        const blockElement = input.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        // Simulate upload (in real implementation, upload to server)
        const reader = new FileReader();
        reader.onload = (e) => {
            block.data.fileUrl = e.target.result;
            block.data.fileName = file.name;
            block.data.fileSize = file.size;
            block.data.mimeType = file.type;
            
            this.updateBlockContent(blockElement, block);
            this.updateHiddenField();
        };
        reader.readAsDataURL(file);
    }
    
    handleVideoUpload(input) {
        const file = input.files[0];
        if (!file) return;
        
        const blockElement = input.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        // Simulate upload
        const reader = new FileReader();
        reader.onload = (e) => {
            block.data.fileUrl = e.target.result;
            block.data.fileName = file.name;
            block.data.fileSize = file.size;
            block.data.mimeType = file.type;
            
            this.updateBlockContent(blockElement, block);
            this.updateHiddenField();
        };
        reader.readAsDataURL(file);
    }
    
    handleAudioUpload(input) {
        const file = input.files[0];
        if (!file) return;
        
        const blockElement = input.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        // Simulate upload
        const reader = new FileReader();
        reader.onload = (e) => {
            block.data.fileUrl = e.target.result;
            block.data.fileName = file.name;
            block.data.fileSize = file.size;
            block.data.mimeType = file.type;
            block.data.isRecorded = false;
            
            this.updateBlockContent(blockElement, block);
            this.updateHiddenField();
        };
        reader.readAsDataURL(file);
    }
    
    async startRecording(button) {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            this.mediaRecorder = new MediaRecorder(stream);
            this.recordedChunks = [];
            
            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this.recordedChunks.push(event.data);
                }
            };
            
            this.mediaRecorder.onstop = () => {
                const blob = new Blob(this.recordedChunks, { type: 'audio/wav' });
                const url = URL.createObjectURL(blob);
                
                const blockElement = button.closest('.content-block');
                const blockId = blockElement.dataset.blockId;
                const block = this.blocks.find(b => b.id === blockId);
                
                block.data.fileUrl = url;
                block.data.isRecorded = true;
                block.data.duration = Math.floor((Date.now() - this.recordingStartTime) / 1000);
                
                this.updateBlockContent(blockElement, block);
                this.updateHiddenField();
            };
            
            this.mediaRecorder.start();
            this.recordingStartTime = Date.now();
            
            // Update UI
            button.disabled = true;
            button.nextElementSibling.disabled = false;
            button.nextElementSibling.nextElementSibling.disabled = false;
            
            const status = button.closest('.audio-recorder').querySelector('.recording-status');
            status.textContent = 'در حال ضبط...';
            
            const timer = button.closest('.audio-recorder').querySelector('.timer-display');
            const timerContainer = button.closest('.audio-recorder').querySelector('.recording-timer');
            timerContainer.style.display = 'block';
            
            this.recordingTimer = setInterval(() => {
                const elapsed = Math.floor((Date.now() - this.recordingStartTime) / 1000);
                const minutes = Math.floor(elapsed / 60);
                const seconds = elapsed % 60;
                timer.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
            }, 1000);
            
        } catch (error) {
            alert('خطا در شروع ضبط صدا. لطفاً مجوز دسترسی به میکروفون را بررسی کنید.');
        }
    }
    
    stopRecording(button) {
        if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
            this.mediaRecorder.stop();
            this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
            
            // Update UI
            button.disabled = true;
            button.previousElementSibling.disabled = false;
            
            const status = button.closest('.audio-recorder').querySelector('.recording-status');
            status.textContent = 'ضبط تکمیل شد';
            
            const timerContainer = button.closest('.audio-recorder').querySelector('.recording-timer');
            timerContainer.style.display = 'none';
            
            if (this.recordingTimer) {
                clearInterval(this.recordingTimer);
                this.recordingTimer = null;
            }
        }
    }
    
    playRecording(button) {
        const blockElement = button.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block.data.fileUrl) {
            const audio = new Audio(block.data.fileUrl);
            audio.play();
        }
    }
    
    updateBlockSettings(select) {
        const blockElement = select.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        const setting = select.dataset.setting;
        block.data[setting] = select.value;
        
        this.updateHiddenField();
    }
    
    updateBlockCaption(textarea) {
        const blockElement = textarea.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        block.data.caption = textarea.value;
        this.updateHiddenField();
    }
    
    updateTextBlockContent(textEditor) {
        const blockElement = textEditor.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            block.data.content = textEditor.innerHTML;
            block.data.textContent = textEditor.textContent || textEditor.innerText || '';
            this.updateHiddenField();
        }
    }
    
    executeTextCommand(button) {
        const blockElement = button.closest('.content-block');
        const textEditor = blockElement.querySelector('.rich-text-editor');
        const command = button.dataset.command;
        
        document.execCommand(command, false, null);
        textEditor.focus();
        
        // Update block data
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        block.data.content = textEditor.innerHTML;
        block.data.textContent = textEditor.textContent || textEditor.innerText || '';
        
        this.updateHiddenField();
    }
    
    updatePreview() {
        let previewHTML = '<div class="reminder-card"><div class="reminder-icon"><i class="fas fa-bell"></i></div><div class="reminder-text">';
        
        if (this.blocks.length === 0) {
            previewHTML += '<p>محتوای یادآوری شما اینجا نمایش داده خواهد شد...</p>';
        } else {
            this.blocks.forEach(block => {
                previewHTML += this.generateBlockPreview(block);
            });
        }
        
        previewHTML += '</div></div>';
        this.preview.innerHTML = previewHTML;
    }
    
    generateBlockPreview(block) {
        let html = '';
        
        switch (block.type) {
            case 'text':
                html += `<div class="text-block">${block.data.content || ''}</div>`;
                break;
            case 'image':
                if (block.data.fileUrl) {
                    const sizeClass = this.getSizeClass(block.data.size);
                    const positionClass = this.getPositionClass(block.data.position);
                    html += `<div class="image-block ${positionClass}">`;
                    if (block.data.caption && block.data.captionPosition === 'top') {
                        html += `<div class="caption caption-top">${block.data.caption}</div>`;
                    }
                    html += `<img src="${block.data.fileUrl}" alt="تصویر" class="${sizeClass}" />`;
                    if (block.data.caption && block.data.captionPosition === 'bottom') {
                        html += `<div class="caption caption-bottom">${block.data.caption}</div>`;
                    }
                    html += '</div>';
                }
                break;
            case 'video':
                if (block.data.fileUrl) {
                    const sizeClass = this.getSizeClass(block.data.size);
                    const positionClass = this.getPositionClass(block.data.position);
                    html += `<div class="video-block ${positionClass}">`;
                    if (block.data.caption && block.data.captionPosition === 'top') {
                        html += `<div class="caption caption-top">${block.data.caption}</div>`;
                    }
                    html += `<video controls class="${sizeClass}"><source src="${block.data.fileUrl}" type="video/mp4"></video>`;
                    if (block.data.caption && block.data.captionPosition === 'bottom') {
                        html += `<div class="caption caption-bottom">${block.data.caption}</div>`;
                    }
                    html += '</div>';
                }
                break;
            case 'audio':
                if (block.data.fileUrl) {
                    html += `<div class="audio-block">`;
                    if (block.data.caption) {
                        html += `<div class="caption">${block.data.caption}</div>`;
                    }
                    html += `<audio controls><source src="${block.data.fileUrl}" type="audio/mpeg"></audio>`;
                    html += '</div>';
                }
                break;
        }
        
        return html;
    }
    
    getSizeClass(size) {
        switch (size) {
            case 'small': return 'size-small';
            case 'medium': return 'size-medium';
            case 'large': return 'size-large';
            case 'full': return 'size-full';
            default: return 'size-medium';
        }
    }
    
    getPositionClass(position) {
        switch (position) {
            case 'left': return 'position-left';
            case 'center': return 'position-center';
            case 'right': return 'position-right';
            default: return 'position-center';
        }
    }
    
    updateHiddenField() {
        const content = {
            type: 'reminder',
            blocks: this.blocks
        };
        
        this.hiddenField.value = JSON.stringify(content);
    }
    
    loadExistingContent() {
        const existingContent = this.hiddenField.value;
        if (existingContent) {
            try {
                const data = JSON.parse(existingContent);
                if (data.blocks && Array.isArray(data.blocks)) {
                    this.blocks = data.blocks;
                    this.nextBlockId = Math.max(...this.blocks.map(b => parseInt(b.id.split('-')[1]) || 0)) + 1;
                    
                    // Render existing blocks
                    this.blocks.forEach(block => {
                        this.renderBlock(block);
                    });
                    
                    this.updateEmptyState();
                }
            } catch (error) {
                // Error loading existing content - ignore silently
            }
        }
    }
    
    getContent() {
        return {
            type: 'reminder',
            blocks: this.blocks
        };
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.reminderBlockManager = new ReminderContentBlockManager();
});
