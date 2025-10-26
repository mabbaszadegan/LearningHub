/**
 * Video Block Manager
 * Handles video block functionality including file upload and preview
 */

// Define VideoBlockManager class globally (with duplicate protection)
if (typeof window.VideoBlockManager === 'undefined') {
window.VideoBlockManager = class VideoBlockManager {
    constructor(options = {}) {
        this.isInitialized = false;
        this.maxFileSize = options.maxFileSize || 50 * 1024 * 1024; // 50MB default
        this.allowedTypes = options.allowedTypes || ['video/mp4', 'video/webm', 'video/quicktime'];
        this.uploadUrl = options.uploadUrl || '/api/files/upload';
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        // Only setup global listeners once
        if (!window._videoBlockListenersSetup) {
            this.setupEventListeners();
            this.setupDragAndDrop();
            window._videoBlockListenersSetup = true;
        }
        
        this.isInitialized = true;
    }

    setupEventListeners() {
        // Handle file input changes
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('file-input') && e.target.dataset.action === 'video-upload') {
                this.handleFileUpload(e.target);
            }
        });

        // Handle change video button clicks
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action="change-video"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="change-video"]');
                const blockElement = button.closest('.content-block-template, .content-block');
                if (blockElement) {
                    const fileInput = blockElement.querySelector('.file-input');
                    if (fileInput) {
                        fileInput.click();
                    }
                }
            }
        });

        // Handle settings changes
        document.addEventListener('change', (e) => {
            if (e.target.closest('.video-settings [data-setting]')) {
                const setting = e.target.dataset.setting;
                const value = e.target.value;
                const blockElement = e.target.closest('.content-block-template, .content-block');
                if (blockElement) {
                    this.updateVideoSettings(blockElement, setting, value);
                }
            }
        });

        // Handle caption changes
        document.addEventListener('input', (e) => {
            if (e.target.closest('.video-caption textarea[data-caption="true"]')) {
                const blockElement = e.target.closest('.content-block-template, .content-block');
                if (blockElement) {
                    this.updateVideoCaption(blockElement, e.target.value);
                }
            }
        });

        // Handle video events
        document.addEventListener('loadedmetadata', (e) => {
            if (e.target.classList.contains('preview-video')) {
                this.handleVideoLoaded(e.target);
            }
        });

        document.addEventListener('error', (e) => {
            if (e.target.classList.contains('preview-video')) {
                this.handleVideoError(e.target);
            }
        });

        // Handle populate block content events
        document.addEventListener('populateBlockContent', (e) => {
            // Handle both regular video blocks and question video blocks
            if (e.detail.blockType === 'video' || e.detail.blockType === 'questionVideo') {
                if (window.videoBlockManager && typeof window.videoBlockManager.populateVideoBlock === 'function') {
                    window.videoBlockManager.populateVideoBlock(e.detail.blockElement, e.detail.block.data);
                } else {
                    console.warn('VideoBlockManager not available, trying to initialize...');
                    // Try to initialize if not available
                    if (typeof initializeVideoBlocks === 'function') {
                        initializeVideoBlocks();
                        if (window.videoBlockManager && typeof window.videoBlockManager.populateVideoBlock === 'function') {
                            window.videoBlockManager.populateVideoBlock(e.detail.blockElement, e.detail.block.data);
                        }
                    }
                }
            }
        });
    }

    setupDragAndDrop() {
        document.addEventListener('dragover', (e) => {
            if (e.target.closest('.video-upload-area .upload-placeholder')) {
                e.preventDefault();
                e.target.closest('.video-upload-area .upload-placeholder').classList.add('drag-over');
            }
        });

        document.addEventListener('dragleave', (e) => {
            if (e.target.closest('.video-upload-area .upload-placeholder')) {
                e.target.closest('.video-upload-area .upload-placeholder').classList.remove('drag-over');
            }
        });

        document.addEventListener('drop', (e) => {
            if (e.target.closest('.video-upload-area .upload-placeholder')) {
                e.preventDefault();
                e.target.closest('.video-upload-area .upload-placeholder').classList.remove('drag-over');
                
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
            console.error('VideoBlockManager: Block element not found for file upload');
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
            isPending: true,
            fileId: null
        });
        
        // Store file in content builder's pendingFiles map
        const activeBuilder = this.findActiveContentBuilder(blockElement);
        if (activeBuilder && activeBuilder.pendingFiles) {
            activeBuilder.pendingFiles.set(blockId, file);
        }

        // Show preview
        this.showVideoPreview(blockElement, objectUrl);
        
        // Sync with content builder
        this.syncWithContentBuilder(blockElement);
        
        // Trigger change event
        this.triggerBlockChange(blockElement);
    }
    
    findActiveContentBuilder(blockElement) {
        const blockId = blockElement.dataset.blockId;
        if (window.reminderBlockManager && window.reminderBlockManager.pendingFiles) {
            if (window.reminderBlockManager.blocks.find(b => b.id === blockId)) {
                return window.reminderBlockManager;
            }
        }
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
            this.showError('فرمت فایل پشتیبانی نمی‌شود. لطفاً از فرمت‌های MP4، WebM یا MOV استفاده کنید.');
            return false;
        }

        // Check file size
        if (file.size > this.maxFileSize) {
            this.showError('حجم فایل بیش از حد مجاز است. حداکثر حجم مجاز 50 مگابایت است.');
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


    showVideoPreview(blockElement, videoUrl) {
        const previewContainer = blockElement.querySelector('.video-preview');
        const uploadPlaceholder = blockElement.querySelector('.upload-placeholder');
        const videoElement = blockElement.querySelector('.preview-video');
        
        if (previewContainer && videoElement) {
            videoElement.src = videoUrl;
            previewContainer.style.display = 'block';
        }
        
        if (uploadPlaceholder) {
            uploadPlaceholder.style.display = 'none';
        }
    }

    handleVideoLoaded(videoElement) {
        const blockElement = videoElement.closest('.content-block-template, .content-block');
        const blockData = this.getBlockData(blockElement);
        
        // Update block data with video metadata
        this.updateBlockData(blockElement, {
            ...blockData,
            duration: videoElement.duration,
            width: videoElement.videoWidth,
            height: videoElement.videoHeight
        });
        
        this.triggerBlockChange(blockElement);
    }

    handleVideoError(videoElement) {
        videoElement.classList.add('error');
        this.showError('خطا در بارگذاری ویدیو. لطفاً فایل را بررسی کنید.');
    }

    updateVideoSettings(blockElement, setting, value) {
        const previewContainer = blockElement.querySelector('.video-preview');
        
        if (!previewContainer) return;
        
        // Update the visual preview
        switch (setting) {
            case 'size':
                previewContainer.className = previewContainer.className.replace(/size-\w+/g, '');
                previewContainer.classList.add(`size-${value}`);
                break;
            case 'position':
                previewContainer.className = previewContainer.className.replace(/position-\w+/g, '');
                previewContainer.classList.add(`position-${value}`);
                break;
            case 'captionPosition':
                // Handle caption position if needed
                break;
        }
        
        // IMPORTANT: Update the block data with the new setting value
        this.updateBlockData(blockElement, { [setting]: value });
        
        // Trigger change event to notify content builder
        this.triggerBlockChange(blockElement);
        
        // Also directly sync with content builder
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

    updateVideoCaption(blockElement, caption) {
        // Update caption in block data
        this.updateBlockData(blockElement, { caption: caption });
        this.triggerBlockChange(blockElement);
        
        // Also directly sync with content builder
        this.syncWithContentBuilder(blockElement);
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

    // Public method to get video data
    getVideoData(blockElement) {
        return this.getBlockData(blockElement);
    }

    // Public method to set video data
    setVideoData(blockElement, data) {
        this.updateBlockData(blockElement, data);
        
        if (data.fileUrl) {
            this.showVideoPreview(blockElement, data.fileUrl);
        }
        
        if (data.size) {
            this.updateVideoSettings(blockElement, 'size', data.size);
        }
        
        if (data.position) {
            this.updateVideoSettings(blockElement, 'position', data.position);
        }
        
        if (data.caption) {
            const captionTextarea = blockElement.querySelector('.video-caption textarea');
            if (captionTextarea) {
                captionTextarea.value = data.caption;
            }
        }
    }

    // Public method to clear video
    clearVideo(blockElement) {
        const previewContainer = blockElement.querySelector('.video-preview');
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

    // Public method to play video
    playVideo(blockElement) {
        const videoElement = blockElement.querySelector('.preview-video');
        if (videoElement) {
            videoElement.play();
        }
    }

    // Public method to pause video
    pauseVideo(blockElement) {
        const videoElement = blockElement.querySelector('.preview-video');
        if (videoElement) {
            videoElement.pause();
        }
    }

    // Public method to populate video block content
    populateVideoBlock(blockElement, data) {
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
                this.showVideoPreview(blockElement, fileUrl);
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
        
        const positionSelect = blockElement.querySelector('select[data-setting="position"]');
        if (positionSelect) positionSelect.value = data.position || 'center';
        
        const captionPositionSelect = blockElement.querySelector('select[data-setting="captionPosition"]');
        if (captionPositionSelect) captionPositionSelect.value = data.captionPosition || 'bottom';
        
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
function initializeVideoBlocks() {
    if (!window.videoBlockManager) {
        window.videoBlockManager = new VideoBlockManager();
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeVideoBlocks();
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = VideoBlockManager;
}

} // End of VideoBlockManager class definition check
