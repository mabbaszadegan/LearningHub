/**
 * Audio Block Manager
 * Handles audio block functionality including file upload, recording, and preview
 */

// Define AudioBlockManager class globally (with duplicate protection)
if (typeof window.AudioBlockManager === 'undefined') {
window.AudioBlockManager = class AudioBlockManager {
    constructor(options = {}) {
        this.isInitialized = false;
        this.maxFileSize = options.maxFileSize || 10 * 1024 * 1024; // 10MB default
        this.allowedTypes = options.allowedTypes || ['audio/mpeg', 'audio/wav', 'audio/ogg'];
        this.uploadUrl = options.uploadUrl || '/api/files/upload';
        
        // Recording properties
        this.mediaRecorder = null;
        this.audioChunks = [];
        this.isRecording = false;
        this.recordingTimer = null;
        this.recordingStartTime = null;
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        // Only setup global listeners once
        if (!window._audioBlockListenersSetup) {
            this.setupEventListeners();
            this.setupDragAndDrop();
            window._audioBlockListenersSetup = true;
        }
        
        this.isInitialized = true;
    }

    setupEventListeners() {
        // Handle file input changes
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('file-input') && e.target.dataset.action === 'audio-upload') {
                this.handleFileUpload(e.target);
            }
        });

        // Handle change audio button clicks
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action="change-audio"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="change-audio"]');
                const blockElement = button.closest('.content-block-template, .content-block');
                if (blockElement) {
                    const fileInput = blockElement.querySelector('.file-input');
                    if (fileInput) {
                        fileInput.click();
                    }
                }
            }

            // Handle upload audio icon button clicks (next to recorder controls)
            if (e.target.closest('[data-action="upload-audio"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="upload-audio"]');
                const blockElement = button.closest('.content-block-template, .content-block');
                if (blockElement) {
                    const fileInput = blockElement.querySelector('.file-input');
                    if (fileInput) {
                        fileInput.click();
                    }
                }
            }
        });

        // Handle recording controls
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action="start-recording"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="start-recording"]');
                const blockElement = button.closest('.content-block-template, .content-block');
                if (blockElement) {
                    this.startRecording(blockElement);
                }
            }
            
            if (e.target.closest('[data-action="stop-recording"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="stop-recording"]');
                const blockElement = button.closest('.content-block-template, .content-block');
                if (blockElement) {
                    this.stopRecording(blockElement);
                }
            }
            
            if (e.target.closest('[data-action="play-recording"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="play-recording"]');
                const blockElement = button.closest('.content-block-template, .content-block');
                if (blockElement) {
                    this.playRecording(blockElement);
                }
            }
        });

        // Handle caption changes
        document.addEventListener('input', (e) => {
            if (e.target.closest('.audio-caption textarea[data-caption="true"]')) {
                const blockElement = e.target.closest('.content-block-template, .content-block');
                if (blockElement) {
                    this.updateAudioCaption(blockElement, e.target.value);
                }
            }
        });

        // Handle settings changes
        document.addEventListener('change', (e) => {
            if (e.target.closest('.audio-settings [data-setting]')) {
                const setting = e.target.dataset.setting;
                const value = e.target.value;
                const blockElement = e.target.closest('.content-block-template, .content-block');
                if (blockElement) {
                    this.updateAudioSettings(blockElement, setting, value);
                }
            }
        });

        // Handle audio events
        document.addEventListener('loadedmetadata', (e) => {
            if (e.target.classList.contains('preview-audio')) {
                this.handleAudioLoaded(e.target);
            }
        });

        document.addEventListener('error', (e) => {
            if (e.target.classList.contains('preview-audio')) {
                this.handleAudioError(e.target);
            }
        });

        // Handle populate block content events
        document.addEventListener('populateBlockContent', (e) => {
            // Handle both regular audio blocks and question audio blocks
            if (e.detail.blockType === 'audio' || e.detail.blockType === 'questionAudio') {
                if (window.audioBlockManager && typeof window.audioBlockManager.populateAudioBlock === 'function') {
                    window.audioBlockManager.populateAudioBlock(e.detail.blockElement, e.detail.block.data);
                } else {
                    console.warn('AudioBlockManager not available, trying to initialize...');
                    // Try to initialize if not available
                    if (typeof initializeAudioBlocks === 'function') {
                        initializeAudioBlocks();
                        if (window.audioBlockManager && typeof window.audioBlockManager.populateAudioBlock === 'function') {
                            window.audioBlockManager.populateAudioBlock(e.detail.blockElement, e.detail.block.data);
                        }
                    }
                }
            }
        });
    }

    setupDragAndDrop() {
        document.addEventListener('dragover', (e) => {
            if (e.target.closest('.audio-upload-area .upload-placeholder')) {
                e.preventDefault();
                e.target.closest('.audio-upload-area .upload-placeholder').classList.add('drag-over');
            }
        });

        document.addEventListener('dragleave', (e) => {
            if (e.target.closest('.audio-upload-area .upload-placeholder')) {
                e.target.closest('.audio-upload-area .upload-placeholder').classList.remove('drag-over');
            }
        });

        document.addEventListener('drop', (e) => {
            if (e.target.closest('.audio-upload-area .upload-placeholder')) {
                e.preventDefault();
                e.target.closest('.audio-upload-area .upload-placeholder').classList.remove('drag-over');
                
                const blockElement = e.target.closest('.content-block-template, .content-block');
                if (blockElement) {
                    const files = e.dataTransfer.files;
                    if (files.length > 0) {
                        const fileInput = blockElement.querySelector('.file-input');
                        if (fileInput) {
                            fileInput.files = files;
                            this.handleFileUpload(fileInput);
                        }
                    }
                }
            }
        });
    }

    handleFileUpload(fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

        // Validate file
        if (!this.validateFile(file)) {
            return;
        }

        const blockElement = fileInput.closest('.content-block-template, .content-block');
        if (!blockElement) {
            console.error('AudioBlockManager: Block element not found for file upload');
            return;
        }
        
        // Store file locally and show preview (will be uploaded later when save is clicked)
        this.handleLocalFile(file, blockElement);
    }
    
    handleLocalFile(file, blockElement) {
        // Create local object URL for preview
        const objectUrl = URL.createObjectURL(file);
        const blockId = blockElement.dataset.blockId;
        
        // Update block data (without localFile - will be stored separately)
        this.updateBlockData(blockElement, {
            fileName: file.name,
            fileUrl: objectUrl,
            fileSize: file.size,
            mimeType: file.type,
            isRecorded: false,
            isPending: true,
            fileId: null
        });
        
        // Store file in content builder's pendingFiles map
        const activeBuilder = this.findActiveContentBuilder(blockElement);
        if (activeBuilder && activeBuilder.pendingFiles) {
            activeBuilder.pendingFiles.set(blockId, file);
        }

        // Show preview
        this.showAudioPreview(blockElement, objectUrl);
        
        // Sync with content builder
        this.syncWithContentBuilder(blockElement);
        
        // Trigger change event
        this.triggerBlockChange(blockElement);
    }
    
    findActiveContentBuilder(blockElement) {
        const blockId = blockElement.dataset.blockId;
        // Try unifiedContentManager first (new unified system)
        if (window.unifiedContentManager && window.unifiedContentManager.pendingFiles) {
            if (window.unifiedContentManager.blocks && window.unifiedContentManager.blocks.find(b => b.id === blockId)) {
                return window.unifiedContentManager;
            }
        }
        // Try reminderBlockManager
        if (window.reminderBlockManager && window.reminderBlockManager.pendingFiles) {
            if (window.reminderBlockManager.blocks.find(b => b.id === blockId)) {
                return window.reminderBlockManager;
            }
        }
        // Try writtenBlockManager
        if (window.writtenBlockManager && window.writtenBlockManager.pendingFiles) {
            if (window.writtenBlockManager.blocks.find(b => b.id === blockId)) {
                return window.writtenBlockManager;
            }
        }
        return null;
    }
    
    validateFile(file) {
        // Check file type
        if (!this.allowedTypes.includes(file.type)) {
            this.showError('فرمت فایل پشتیبانی نمی‌شود. لطفاً از فرمت‌های MP3، WAV یا OGG استفاده کنید.');
            return false;
        }

        // Check file size
        if (file.size > this.maxFileSize) {
            this.showError('حجم فایل بیش از حد مجاز است. حداکثر حجم مجاز 10 مگابایت است.');
            return false;
        }

        return true;
    }

    showUploadProgress(blockElement) {
        const progressContainer = blockElement.querySelector('.upload-progress');
        const uploadPlaceholder = blockElement.querySelector('.upload-placeholder');
        
        if (progressContainer) {
            progressContainer.style.display = 'flex';
        }
        
        if (uploadPlaceholder) {
            uploadPlaceholder.style.display = 'none';
        }
    }

    hideUploadProgress(blockElement) {
        const progressContainer = blockElement.querySelector('.upload-progress');
        
        if (progressContainer) {
            progressContainer.style.display = 'none';
        }
    }


    showAudioPreview(blockElement, audioUrl) {
        if (!audioUrl) return;
        
        const previewContainer = blockElement.querySelector('.audio-preview');
        const uploadPlaceholder = blockElement.querySelector('.upload-placeholder');
        const audioElement = blockElement.querySelector('.preview-audio');
        const sourceElement = audioElement ? audioElement.querySelector('source') : null;

        if (audioElement && audioUrl) {
            // Set both <audio src> and <source src> for maximum compatibility
            if (sourceElement) {
                sourceElement.src = audioUrl;
                sourceElement.type = audioUrl.match(/\.mp3/i) ? 'audio/mpeg' : 
                                   audioUrl.match(/\.wav/i) ? 'audio/wav' : 
                                   audioUrl.match(/\.ogg/i) ? 'audio/ogg' : 'audio/mpeg';
            }
            audioElement.src = audioUrl;
            
            // Force the browser to re-evaluate sources
            if (typeof audioElement.load === 'function') {
                audioElement.load();
            }
        }

        if (previewContainer) {
            previewContainer.style.display = 'flex';
        }

        if (uploadPlaceholder) {
            uploadPlaceholder.style.display = 'none';
        }
    }

    handleAudioLoaded(audioElement) {
        const blockElement = audioElement.closest('.content-block-template, .content-block');
        const blockData = this.getBlockData(blockElement);
        
        // Update block data with audio metadata
        this.updateBlockData(blockElement, {
            ...blockData,
            duration: audioElement.duration
        });
        
        this.triggerBlockChange(blockElement);
    }

    handleAudioError(audioElement) {
        audioElement.classList.add('error');
        this.showError('خطا در بارگذاری فایل صوتی. لطفاً فایل را بررسی کنید.');
    }

    async startRecording(blockElement) {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            
            this.mediaRecorder = new MediaRecorder(stream);
            this.audioChunks = [];
            this.isRecording = true;
            this.recordingStartTime = Date.now();
            
            this.mediaRecorder.ondataavailable = (event) => {
                this.audioChunks.push(event.data);
            };
            
            this.mediaRecorder.onstop = () => {
                const audioBlob = new Blob(this.audioChunks, { type: 'audio/wav' });
                const audioUrl = URL.createObjectURL(audioBlob);
                
                // Create a File object from the blob
                const recordingFile = new File([audioBlob], `recording_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.wav`, { type: 'audio/wav' });
                
                const blockId = blockElement.dataset.blockId;
                
                // Update block data (without localFile - will be stored separately)
                this.updateBlockData(blockElement, {
                    fileName: `recording_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.wav`,
                    fileUrl: audioUrl,
                    fileSize: audioBlob.size,
                    mimeType: 'audio/wav',
                    isRecorded: true,
                    isPending: true,
                    fileId: null,
                    duration: (Date.now() - this.recordingStartTime) / 1000
                });
                
                // Store file in content builder's pendingFiles map
                const activeBuilder = this.findActiveContentBuilder(blockElement);
                if (activeBuilder && activeBuilder.pendingFiles) {
                    activeBuilder.pendingFiles.set(blockId, recordingFile);
                }
                
                // Show preview
                this.showAudioPreview(blockElement, audioUrl);
                
                // Sync with content builder
                this.syncWithContentBuilder(blockElement);
                
                // Trigger change event
                this.triggerBlockChange(blockElement);
                
                // Stop all tracks
                stream.getTracks().forEach(track => track.stop());
            };
            
            this.mediaRecorder.start();
            this.updateRecordingUI(blockElement, true);
            this.startRecordingTimer(blockElement);
            
        } catch (error) {
            console.error('Error starting recording:', error);
            this.showError('خطا در شروع ضبط صدا. لطفاً دسترسی میکروفون را بررسی کنید.');
        }
    }

    stopRecording(blockElement) {
        if (this.mediaRecorder && this.isRecording) {
            this.mediaRecorder.stop();
            this.isRecording = false;
            this.updateRecordingUI(blockElement, false);
            this.stopRecordingTimer();
        }
    }

    playRecording(blockElement) {
        const audioElement = blockElement.querySelector('.preview-audio');
        if (audioElement && audioElement.src) {
            audioElement.play();
        }
    }

    updateRecordingUI(blockElement, isRecording) {
        const startBtn = blockElement.querySelector('[data-action="start-recording"]');
        const stopBtn = blockElement.querySelector('[data-action="stop-recording"]');
        const playBtn = blockElement.querySelector('[data-action="play-recording"]');
        const status = blockElement.querySelector('.recording-status');
        const timer = blockElement.querySelector('.recording-timer');
        
        if (isRecording) {
            startBtn.disabled = true;
            stopBtn.disabled = false;
            playBtn.disabled = true;
            status.textContent = 'در حال ضبط...';
            timer.style.display = 'block';
            startBtn.classList.add('recording');
        } else {
            startBtn.disabled = false;
            stopBtn.disabled = true;
            playBtn.disabled = false;
            status.textContent = 'آماده برای ضبط';
            timer.style.display = 'none';
            startBtn.classList.remove('recording');
        }
    }

    startRecordingTimer(blockElement) {
        const timerDisplay = blockElement.querySelector('.timer-display');
        this.recordingTimer = setInterval(() => {
            const elapsed = Math.floor((Date.now() - this.recordingStartTime) / 1000);
            const minutes = Math.floor(elapsed / 60);
            const seconds = elapsed % 60;
            timerDisplay.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        }, 1000);
    }

    stopRecordingTimer() {
        if (this.recordingTimer) {
            clearInterval(this.recordingTimer);
            this.recordingTimer = null;
        }
    }

    updateAudioCaption(blockElement, caption) {
        // Update caption in block data
        this.updateBlockData(blockElement, { caption: caption });
        this.triggerBlockChange(blockElement);
        
        // Also directly sync with content builder
        this.syncWithContentBuilder(blockElement);
    }

    updateAudioSettings(blockElement, setting, value) {
        // Update setting in block data
        this.updateBlockData(blockElement, { [setting]: value });
        
        // If display mode changed, show/hide attachment mode setting
        if (setting === 'displayMode') {
            const attachmentModeSetting = blockElement.querySelector('#attachment-mode-setting');
            if (attachmentModeSetting) {
                if (value === 'icon') {
                    attachmentModeSetting.style.display = 'block';
                } else {
                    attachmentModeSetting.style.display = 'none';
                }
            }
        }
        
        this.triggerBlockChange(blockElement);
        this.syncWithContentBuilder(blockElement);
    }
    
    syncWithContentBuilder(blockElement) {
        // Find the active content builder (reminderBlockManager or writtenBlockManager)
        const blockId = blockElement.dataset.blockId;
        const blockData = this.getBlockData(blockElement);
        
        // Try reminderBlockManager first
        if (window.reminderBlockManager && window.reminderBlockManager.blocks) {
            const blockIndex = window.reminderBlockManager.blocks.findIndex(b => b.id === blockId);
            if (blockIndex !== -1) {
                window.reminderBlockManager.blocks[blockIndex].data = {
                    ...window.reminderBlockManager.blocks[blockIndex].data,
                    ...blockData
                };
                window.reminderBlockManager.updateHiddenField();
                return;
            }
        }
        
        // Try writtenBlockManager
        if (window.writtenBlockManager && window.writtenBlockManager.blocks) {
            const blockIndex = window.writtenBlockManager.blocks.findIndex(b => b.id === blockId);
            if (blockIndex !== -1) {
                window.writtenBlockManager.blocks[blockIndex].data = {
                    ...window.writtenBlockManager.blocks[blockIndex].data,
                    ...blockData
                };
                window.writtenBlockManager.updateHiddenField();
                return;
            }
        }
    }

    updateBlockData(blockElement, data) {
        // Store data in block element for later retrieval
        if (!blockElement.dataset.blockData) {
            blockElement.dataset.blockData = '{}';
        }
        
        const currentData = JSON.parse(blockElement.dataset.blockData);
        const updatedData = { ...currentData, ...data };
        blockElement.dataset.blockData = JSON.stringify(updatedData);
    }

    getBlockData(blockElement) {
        if (!blockElement.dataset.blockData) {
            return {};
        }
        
        try {
            return JSON.parse(blockElement.dataset.blockData);
        } catch (error) {
            console.error('Error parsing block data:', error);
            return {};
        }
    }

    triggerBlockChange(blockElement) {
        const blockData = this.getBlockData(blockElement);
        const event = new CustomEvent('blockContentChanged', {
            detail: {
                blockElement: blockElement,
                blockData: blockData
            }
        });
        // Dispatch on both element and document so all listeners can catch it
        blockElement.dispatchEvent(event);
        document.dispatchEvent(event);
    }

    showError(message) {
        // Create a simple error notification
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger alert-dismissible fade show';
        errorDiv.style.position = 'fixed';
        errorDiv.style.top = '20px';
        errorDiv.style.right = '20px';
        errorDiv.style.zIndex = '9999';
        errorDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(errorDiv);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (errorDiv.parentNode) {
                errorDiv.parentNode.removeChild(errorDiv);
            }
        }, 5000);
    }

    // Public method to get audio data
    getAudioData(blockElement) {
        return this.getBlockData(blockElement);
    }

    // Public method to set audio data
    setAudioData(blockElement, data) {
        this.updateBlockData(blockElement, data);
        
        if (data.fileUrl) {
            this.showAudioPreview(blockElement, data.fileUrl);
        }
        
        if (data.caption) {
            const captionTextarea = blockElement.querySelector('.audio-caption textarea');
            if (captionTextarea) {
                captionTextarea.value = data.caption;
            }
        }
    }

    // Public method to clear audio
    clearAudio(blockElement) {
        const previewContainer = blockElement.querySelector('.audio-preview');
        const uploadPlaceholder = blockElement.querySelector('.upload-placeholder');
        const fileInput = blockElement.querySelector('.file-input');
        
        if (previewContainer) {
            previewContainer.style.display = 'none';
        }
        
        if (uploadPlaceholder) {
            uploadPlaceholder.style.display = 'flex';
        }
        
        if (fileInput) {
            fileInput.value = '';
        }
        
        this.updateBlockData(blockElement, {});
        this.triggerBlockChange(blockElement);
    }

    // Public method to play audio
    playAudio(blockElement) {
        const audioElement = blockElement.querySelector('.preview-audio');
        if (audioElement) {
            audioElement.play();
        }
    }

    // Public method to pause audio
    pauseAudio(blockElement) {
        const audioElement = blockElement.querySelector('.preview-audio');
        if (audioElement) {
            audioElement.pause();
        }
    }

    // Public method to populate audio block content (for edit mode)
    populateAudioBlock(blockElement, data) {
        // Populate file info if available (check both fileId and fileUrl)
        let fileUrl = data.fileUrl;
        
        if (data.fileId) {
            const fileIdInput = blockElement.querySelector('input[name="fileId"]');
            if (fileIdInput) fileIdInput.value = data.fileId;
            
            const fileNameSpan = blockElement.querySelector('.file-name');
            if (fileNameSpan && data.fileName) {
                fileNameSpan.textContent = data.fileName;
            }
            
            // Construct proper file URL for existing files if not provided
            if (!fileUrl && data.fileId) {
                fileUrl = `/FileUpload/GetFile/${data.fileId}`;
            }
        }
        
        // Show preview if file URL exists (regardless of whether we have fileId)
        if (fileUrl) {
            // Use setTimeout to ensure DOM is ready
            setTimeout(() => {
                this.showAudioPreview(blockElement, fileUrl);
            }, 50);
        }
        
        // Populate caption
        const captionTextarea = blockElement.querySelector('textarea[data-caption="true"]');
        if (captionTextarea) {
            captionTextarea.value = data.caption || '';
        }
        
        // Populate settings
        const sizeSelect = blockElement.querySelector('select[data-setting="size"]');
        if (sizeSelect) sizeSelect.value = data.size || 'medium';
        
        // Populate display mode setting
        const displayModeSelect = blockElement.querySelector('select[data-setting="displayMode"]');
        if (displayModeSelect) {
            displayModeSelect.value = data.displayMode || 'player';
            // Trigger change to show/hide attachment mode setting
            if (displayModeSelect.value === 'icon') {
                const attachmentModeSetting = blockElement.querySelector('#attachment-mode-setting');
                if (attachmentModeSetting) {
                    attachmentModeSetting.style.display = 'block';
                }
            }
        }
        
        // Populate attachment mode setting
        const attachmentModeSelect = blockElement.querySelector('select[data-setting="attachmentMode"]');
        if (attachmentModeSelect) {
            attachmentModeSelect.value = data.attachmentMode || 'independent';
        }
        
        // Populate question-specific fields if this is a question block
        this.populateQuestionFields(blockElement, data);
        
        // Update block data with proper file URL
        const updatedData = { ...data };
        if (data.fileId && !updatedData.fileUrl) {
            updatedData.fileUrl = `/FileUpload/GetFile/${data.fileId}`;
        }
        this.updateBlockData(blockElement, updatedData);
    }

    // Helper method to populate question-specific fields
    populateQuestionFields(blockElement, data) {
        // Check if this is a question block by looking for question settings
        const questionSettings = blockElement.querySelector('.question-settings');
        if (!questionSettings) return;

        // Populate points field
        const pointsInput = questionSettings.querySelector('[data-setting="points"]');
        if (pointsInput && data.points !== undefined) {
            pointsInput.value = data.points;
        }

        // Populate difficulty field
        const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
        if (difficultySelect && data.difficulty !== undefined) {
            difficultySelect.value = data.difficulty;
        }

        // Populate required checkbox
        const requiredCheckbox = questionSettings.querySelector('[data-setting="isRequired"]');
        if (requiredCheckbox && data.isRequired !== undefined) {
            requiredCheckbox.checked = data.isRequired;
        }

        // Populate teacher guidance (hint)
        const hintTextarea = blockElement.querySelector('[data-hint="true"]');
        if (hintTextarea && data.teacherGuidance !== undefined) {
            hintTextarea.value = data.teacherGuidance;
        }

        // Populate question text content
        this.populateQuestionText(blockElement, data);
    }

    // Helper method to populate question text content
    populateQuestionText(blockElement, data) {
        // Try rich text editor (for image/video/audio blocks)
        const richTextEditor = blockElement.querySelector('.rich-text-editor');
        if (richTextEditor && data.content) {
            richTextEditor.innerHTML = data.content;
            // Update toolbar state if text block manager is available
            if (window.textBlockManager && typeof window.textBlockManager.updateToolbarState === 'function') {
                window.textBlockManager.updateToolbarState(richTextEditor);
            }
        }
    }
}

// Global functions for backward compatibility
function initializeAudioBlocks() {
    if (!window.audioBlockManager) {
        window.audioBlockManager = new AudioBlockManager();
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeAudioBlocks();
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AudioBlockManager;
}

} // End of AudioBlockManager class definition check
