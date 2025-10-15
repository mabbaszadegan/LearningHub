/**
 * Reminder Content Block Manager
 * Handles content block creation, editing, and management for reminder-type schedule items
 */

// Global cleanup function for modal backdrops
function cleanupModalBackdrops() {
    const backdrops = document.querySelectorAll('.modal-backdrop');
    backdrops.forEach(backdrop => {
        backdrop.remove();
    });
    document.body.classList.remove('modal-open');
    document.body.style.paddingRight = '';
    document.body.style.overflow = '';
    document.body.style.overflowX = '';
    document.body.style.overflowY = '';
    document.documentElement.style.overflow = '';
    document.documentElement.style.overflowX = '';
    document.documentElement.style.overflowY = '';
    console.log('Global modal backdrop cleanup completed');
}

// Global functions for onclick handlers - defined at the top to avoid timing issues
function showBlockTypeModal() {
    console.log('showBlockTypeModal called');
    
    // Check if modal element exists
    const modalElement = document.getElementById('blockTypeModal');
    if (!modalElement) {
        console.error('Modal element not found in DOM');
        // Use fallback immediately
        const types = [
            { type: 'text', name: 'متن' },
            { type: 'image', name: 'تصویر' },
            { type: 'video', name: 'ویدیو' },
            { type: 'audio', name: 'صوت' }
        ];
        
        const selection = prompt('انتخاب نوع بلاک:\n1. متن\n2. تصویر\n3. ویدیو\n4. صوت\n\nلطفاً شماره مورد نظر را وارد کنید:');
        
        if (selection && selection >= 1 && selection <= 4) {
            const selectedType = types[selection - 1].type;
            if (window.reminderBlockManager) {
                window.reminderBlockManager.addBlock(selectedType);
            } else {
                console.warn('Manager still not available after selection');
            }
        }
        return;
    }
    
    // Clean up any existing backdrops first
    cleanupModalBackdrops();
    
    if (window.reminderBlockManager) {
        console.log('Using manager to show modal');
        window.reminderBlockManager.showBlockTypeModal();
    } else {
        console.log('Manager not available, using fallback');
        // Fallback: show simple selection
        const types = [
            { type: 'text', name: 'متن' },
            { type: 'image', name: 'تصویر' },
            { type: 'video', name: 'ویدیو' },
            { type: 'audio', name: 'صوت' }
        ];
        
        const selection = prompt('انتخاب نوع بلاک:\n1. متن\n2. تصویر\n3. ویدیو\n4. صوت\n\nلطفاً شماره مورد نظر را وارد کنید:');
        
        if (selection && selection >= 1 && selection <= 4) {
            const selectedType = types[selection - 1].type;
            if (window.reminderBlockManager) {
                window.reminderBlockManager.addBlock(selectedType);
            } else {
                console.warn('Manager still not available after selection');
            }
        }
    }
}

function updatePreview() {
    console.log('updatePreview called');
    
    if (window.reminderBlockManager) {
        console.log('Using manager to update preview');
        window.reminderBlockManager.updatePreview();
    } else {
        console.log('Manager not available for preview');
        alert('سیستم پیش‌نمایش هنوز آماده نیست');
    }
}

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
        console.log('ReminderContentBlockManager: Initializing...');
        
        // Check if required elements exist
        const addBlockBtn = document.getElementById('addContentBlockBtn');
        const previewBtn = document.getElementById('previewReminderBtn');
        
        console.log('Add block button found:', !!addBlockBtn);
        console.log('Preview button found:', !!previewBtn);
        
        if (!addBlockBtn) {
            console.error('Add block button not found!');
            return;
        }
        
        if (!previewBtn) {
            console.error('Preview button not found!');
            return;
        }
        
        this.setupEventListeners();
        this.loadExistingContent();
        
        // Ensure scroll is enabled on initialization
        this.ensureScrollEnabled();
        
        console.log('ReminderContentBlockManager: Initialization complete');
    }
    
    setupEventListeners() {
        // Add block button
        const addBlockBtn = document.getElementById('addContentBlockBtn');
        if (addBlockBtn) {
            // Remove any existing listeners to prevent duplicates
            addBlockBtn.removeEventListener('click', this.handleAddBlockClick);
            this.handleAddBlockClick = (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('Add block button clicked');
                this.showBlockTypeModal();
            };
            addBlockBtn.addEventListener('click', this.handleAddBlockClick);
        } else {
            console.warn('Add block button not found');
        }
        
        // Preview button
        const previewBtn = document.getElementById('previewReminderBtn');
        if (previewBtn) {
            // Remove any existing listeners to prevent duplicates
            previewBtn.removeEventListener('click', this.handlePreviewClick);
            this.handlePreviewClick = (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('Preview button clicked');
                this.updatePreview();
            };
            previewBtn.addEventListener('click', this.handlePreviewClick);
        } else {
            console.warn('Preview button not found');
        }
        
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
        const modalElement = document.getElementById('blockTypeModal');
        if (!modalElement) {
            console.warn('Modal element not found, using fallback');
            // Fallback: show block type selection directly
            this.showBlockTypeSelection();
            return;
        }
        
        // Clean up any existing backdrops first
        this.cleanupModalBackdrop();
        
        // Try to use Bootstrap Modal if available
        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            try {
                // Dispose any existing modal instance
                const existingModal = bootstrap.Modal.getInstance(modalElement);
                if (existingModal) {
                    existingModal.dispose();
                }
                
                // Ensure the modal element is still in the DOM
                if (!document.body.contains(modalElement)) {
                    console.error('Modal element not in DOM, using fallback');
                    this.showBlockTypeSelection();
                    return;
                }
                
                // Create new modal instance
                const modal = new bootstrap.Modal(modalElement, {
                    backdrop: true,
                    keyboard: true,
                    focus: true
                });
                
                // Add event listener for when modal is hidden
                modalElement.addEventListener('hidden.bs.modal', () => {
                    this.cleanupModalBackdrop();
                }, { once: true });
                
                modal.show();
            } catch (error) {
                console.error('Error showing Bootstrap modal:', error);
                // Fallback to manual modal
                this.showModalManually(modalElement);
            }
        } else {
            // Fallback: show modal manually
            this.showModalManually(modalElement);
        }
    }
    
    showModalManually(modalElement) {
        if (!modalElement || !document.body.contains(modalElement)) {
            console.error('Modal element not available for manual display');
            this.showBlockTypeSelection();
            return;
        }
        
        modalElement.style.display = 'block';
        modalElement.classList.add('show');
        document.body.classList.add('modal-open');
        
        // Add backdrop
        const backdrop = document.createElement('div');
        backdrop.className = 'modal-backdrop fade show';
        backdrop.id = 'blockTypeModalBackdrop';
        document.body.appendChild(backdrop);
        
        // Close modal when backdrop is clicked
        backdrop.addEventListener('click', () => {
            this.hideBlockTypeModal();
        });
    }
    
    hideBlockTypeModal() {
        const modalElement = document.getElementById('blockTypeModal');
        if (!modalElement) {
            console.warn('Modal element not found when trying to hide');
            this.cleanupModalBackdrop();
            return;
        }
        
        // Try to use Bootstrap Modal if available
        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            try {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) {
                    modal.hide();
                    
                    // Ensure backdrop is removed after Bootstrap hides the modal
                    setTimeout(() => {
                        this.cleanupModalBackdrop();
                    }, 300); // Wait for Bootstrap's animation to complete
                } else {
                    // No existing instance, try to create one and hide it
                    if (document.body.contains(modalElement)) {
                        const newModal = new bootstrap.Modal(modalElement);
                        newModal.hide();
                        
                        setTimeout(() => {
                            this.cleanupModalBackdrop();
                        }, 300);
                    } else {
                        console.warn('Modal element not in DOM, cleaning up manually');
                        this.cleanupModalBackdrop();
                    }
                }
            } catch (error) {
                console.error('Error hiding Bootstrap modal:', error);
                // Fallback to manual hide
                this.hideModalManually(modalElement);
            }
        } else {
            // Fallback: hide modal manually
            this.hideModalManually(modalElement);
        }
    }
    
    hideModalManually(modalElement) {
        if (!modalElement || !document.body.contains(modalElement)) {
            console.warn('Modal element not available for manual hide');
            this.cleanupModalBackdrop();
            return;
        }
        
        modalElement.style.display = 'none';
        modalElement.classList.remove('show');
        document.body.classList.remove('modal-open');
        
        // Remove backdrop
        this.cleanupModalBackdrop();
    }
    
    cleanupModalBackdrop() {
        // Remove any existing modal backdrops
        const backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach(backdrop => {
            backdrop.remove();
        });
        
        // Remove modal-open class from body
        document.body.classList.remove('modal-open');
        
        // Reset body padding and overflow if they were modified
        document.body.style.paddingRight = '';
        document.body.style.overflow = '';
        document.body.style.overflowX = '';
        document.body.style.overflowY = '';
        
        // Ensure html element is also clean
        document.documentElement.style.overflow = '';
        document.documentElement.style.overflowX = '';
        document.documentElement.style.overflowY = '';
        
        console.log('Modal backdrop cleanup completed');
    }
    
    showBlockTypeSelection() {
        // Fallback: show a simple selection dialog
        const types = [
            { type: 'text', name: 'متن', icon: 'fas fa-font' },
            { type: 'image', name: 'تصویر', icon: 'fas fa-image' },
            { type: 'video', name: 'ویدیو', icon: 'fas fa-video' },
            { type: 'audio', name: 'صوت', icon: 'fas fa-microphone' }
        ];
        
        const selection = prompt('انتخاب نوع بلاک:\n1. متن\n2. تصویر\n3. ویدیو\n4. صوت\n\nلطفاً شماره مورد نظر را وارد کنید:');
        
        if (selection && selection >= 1 && selection <= 4) {
            const selectedType = types[selection - 1].type;
            this.addBlock(selectedType);
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
        
        // Ensure scroll is enabled
        this.ensureScrollEnabled();
        
        // Auto-scroll to the new block
        this.scrollToNewBlock(blockId);
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
            
            // Scroll to the moved block
            this.scrollToBlock(blockId);
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
            
            // Scroll to the moved block
            this.scrollToBlock(blockId);
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
    
    scrollToNewBlock(blockId) {
        // Wait a bit for the DOM to update
        setTimeout(() => {
            const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
            if (blockElement) {
                // Smooth scroll to the new block
                blockElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center',
                    inline: 'nearest'
                });
                
                // Add highlight class for better animation
                blockElement.classList.add('highlight');
                
                // Remove highlight after 2 seconds
                setTimeout(() => {
                    blockElement.classList.remove('highlight');
                }, 2000);
                
                console.log('Scrolled to new block:', blockId);
            }
        }, 100);
    }
    
    scrollToBlock(blockId) {
        // Wait a bit for the DOM to update
        setTimeout(() => {
            const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
            if (blockElement) {
                // Smooth scroll to the block
                blockElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center',
                    inline: 'nearest'
                });
                
                console.log('Scrolled to block:', blockId);
            }
        }, 50);
    }
    
    ensureScrollEnabled() {
        // Ensure body and html can scroll
        document.body.style.overflow = '';
        document.body.style.overflowX = '';
        document.body.style.overflowY = '';
        document.documentElement.style.overflow = '';
        document.documentElement.style.overflowX = '';
        document.documentElement.style.overflowY = '';
        
        // Remove any classes that might disable scroll
        document.body.classList.remove('modal-open');
        
        console.log('Scroll enabled');
    }
    
    getContent() {
        return {
            type: 'reminder',
            blocks: this.blocks
        };
    }
}


// Initialize when DOM is loaded
function initializeReminderBlockManager() {
    try {
        // Check if already initialized
        if (window.reminderBlockManager) {
            return;
        }
        
        // Check if required elements exist
        const requiredElements = [
            'contentBlocksList',
            'emptyBlocksState', 
            'reminderPreview',
            'reminderContentJson'
        ];
        
        let missingElements = [];
        requiredElements.forEach(id => {
            if (!document.getElementById(id)) {
                missingElements.push(id);
            }
        });
        
        if (missingElements.length > 0) {
            console.warn('ReminderContentBlockManager: Missing required elements:', missingElements);
            return;
        }
        
        window.reminderBlockManager = new ReminderContentBlockManager();
        console.log('ReminderContentBlockManager initialized successfully');
        
    } catch (error) {
        console.error('Error initializing ReminderContentBlockManager:', error);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Add a small delay to ensure all elements are ready
    setTimeout(initializeReminderBlockManager, 100);
});

// Also try to initialize immediately if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeReminderBlockManager, 100);
    });
} else {
    // DOM is already loaded
    setTimeout(initializeReminderBlockManager, 100);
}

// Clean up modal backdrops when page is about to unload
window.addEventListener('beforeunload', () => {
    cleanupModalBackdrops();
});

// Also clean up on page visibility change (when user switches tabs)
document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
        cleanupModalBackdrops();
    }
});
