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
        
        this.setupEventListeners();
        this.setupDragAndDrop();
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
                const blockElement = button.closest('.content-block-template');
                const fileInput = blockElement.querySelector('.file-input');
                if (fileInput) {
                    fileInput.click();
                }
            }
        });

        // Handle recording controls
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action="start-recording"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="start-recording"]');
                const blockElement = button.closest('.content-block-template');
                this.startRecording(blockElement);
            }
            
            if (e.target.closest('[data-action="stop-recording"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="stop-recording"]');
                const blockElement = button.closest('.content-block-template');
                this.stopRecording(blockElement);
            }
            
            if (e.target.closest('[data-action="play-recording"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="play-recording"]');
                const blockElement = button.closest('.content-block-template');
                this.playRecording(blockElement);
            }
        });

        // Handle caption changes
        document.addEventListener('input', (e) => {
            if (e.target.closest('.audio-caption textarea[data-caption="true"]')) {
                const blockElement = e.target.closest('.content-block-template');
                this.updateAudioCaption(blockElement, e.target.value);
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
            if (e.detail.blockType === 'audio') {
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
                
                const files = e.dataTransfer.files;
                if (files.length > 0) {
                    const fileInput = e.target.closest('.content-block-template').querySelector('.file-input');
                    if (fileInput) {
                        fileInput.files = files;
                        this.handleFileUpload(fileInput);
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

        const blockElement = fileInput.closest('.content-block-template');
        this.showUploadProgress(blockElement);
        
        // Simulate upload process (replace with actual upload)
        this.simulateUpload(file, blockElement);
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

    simulateUpload(file, blockElement) {
        // Simulate upload progress
        const progressFill = blockElement.querySelector('.progress-fill');
        let progress = 0;
        
        const progressInterval = setInterval(() => {
            progress += Math.random() * 20;
            if (progress > 100) progress = 100;
            
            if (progressFill) {
                progressFill.style.width = progress + '%';
            }
            
            if (progress >= 100) {
                clearInterval(progressInterval);
                this.handleUploadSuccess(file, blockElement);
            }
        }, 200);
    }

    handleUploadSuccess(file, blockElement) {
        // Create object URL for preview
        const objectUrl = URL.createObjectURL(file);
        
        // Update block data
        this.updateBlockData(blockElement, {
            fileId: `file_${Date.now()}`,
            fileName: file.name,
            fileUrl: objectUrl,
            fileSize: file.size,
            mimeType: file.type,
            isRecorded: false
        });

        // Show preview
        this.showAudioPreview(blockElement, objectUrl);
        
        // Hide progress
        this.hideUploadProgress(blockElement);
        
        // Trigger change event
        this.triggerBlockChange(blockElement);
    }

    showAudioPreview(blockElement, audioUrl) {
        const previewContainer = blockElement.querySelector('.audio-preview');
        const uploadPlaceholder = blockElement.querySelector('.upload-placeholder');
        const audioElement = blockElement.querySelector('.preview-audio');
        
        if (previewContainer && audioElement) {
            audioElement.src = audioUrl;
            previewContainer.style.display = 'block';
        }
        
        if (uploadPlaceholder) {
            uploadPlaceholder.style.display = 'none';
        }
    }

    handleAudioLoaded(audioElement) {
        const blockElement = audioElement.closest('.content-block-template');
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
                
                // Update block data
                this.updateBlockData(blockElement, {
                    fileId: `recording_${Date.now()}`,
                    fileName: `recording_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.wav`,
                    fileUrl: audioUrl,
                    fileSize: audioBlob.size,
                    mimeType: 'audio/wav',
                    isRecorded: true,
                    duration: (Date.now() - this.recordingStartTime) / 1000
                });
                
                // Show preview
                this.showAudioPreview(blockElement, audioUrl);
                
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
        const event = new CustomEvent('blockContentChanged', {
            detail: {
                blockElement: blockElement,
                blockData: this.getBlockData(blockElement)
            }
        });
        blockElement.dispatchEvent(event);
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
        // Populate file info if available
        if (data.fileId) {
            const fileIdInput = blockElement.querySelector('input[name="fileId"]');
            if (fileIdInput) fileIdInput.value = data.fileId;
            
            const fileNameSpan = blockElement.querySelector('.file-name');
            if (fileNameSpan && data.fileName) {
                fileNameSpan.textContent = data.fileName;
            }
            
            // Construct proper file URL for existing files
            let fileUrl = data.fileUrl;
            if (!fileUrl && data.fileId) {
                // If no URL is provided but we have a fileId, construct the URL
                fileUrl = `/FileUpload/GetFile/${data.fileId}`;
            }
            
            // Show preview if file URL exists
            if (fileUrl) {
                this.showAudioPreview(blockElement, fileUrl);
            }
        }
        
        // Populate caption
        const captionTextarea = blockElement.querySelector('textarea[data-caption="true"]');
        if (captionTextarea) {
            captionTextarea.value = data.caption || '';
        }
        
        // Populate settings
        const sizeSelect = blockElement.querySelector('select[data-setting="size"]');
        if (sizeSelect) sizeSelect.value = data.size || 'medium';
        
        // Update block data with proper file URL
        const updatedData = { ...data };
        if (data.fileId && !updatedData.fileUrl) {
            updatedData.fileUrl = `/FileUpload/GetFile/${data.fileId}`;
        }
        this.updateBlockData(blockElement, updatedData);
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
