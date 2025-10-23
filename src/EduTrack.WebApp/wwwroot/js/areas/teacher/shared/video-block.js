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
        
        this.setupEventListeners();
        this.setupDragAndDrop();
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
                const blockElement = button.closest('.content-block-template');
                const fileInput = blockElement.querySelector('.file-input');
                if (fileInput) {
                    fileInput.click();
                }
            }
        });

        // Handle settings changes
        document.addEventListener('change', (e) => {
            if (e.target.closest('.video-settings [data-setting]')) {
                const setting = e.target.dataset.setting;
                const value = e.target.value;
                const blockElement = e.target.closest('.content-block-template');
                this.updateVideoSettings(blockElement, setting, value);
            }
        });

        // Handle caption changes
        document.addEventListener('input', (e) => {
            if (e.target.closest('.video-caption textarea[data-caption="true"]')) {
                const blockElement = e.target.closest('.content-block-template');
                this.updateVideoCaption(blockElement, e.target.value);
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
            if (e.detail.blockType === 'video') {
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

    simulateUpload(file, blockElement) {
        // Simulate upload progress
        const progressFill = blockElement.querySelector('.progress-fill');
        let progress = 0;
        
        const progressInterval = setInterval(() => {
            progress += Math.random() * 15;
            if (progress > 100) progress = 100;
            
            if (progressFill) {
                progressFill.style.width = progress + '%';
            }
            
            if (progress >= 100) {
                clearInterval(progressInterval);
                this.handleUploadSuccess(file, blockElement);
            }
        }, 300);
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
            mimeType: file.type
        });

        // Show preview
        this.showVideoPreview(blockElement, objectUrl);
        
        // Hide progress
        this.hideUploadProgress(blockElement);
        
        // Trigger change event
        this.triggerBlockChange(blockElement);
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
        const blockElement = videoElement.closest('.content-block-template');
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
        
        this.triggerBlockChange(blockElement);
    }

    updateVideoCaption(blockElement, caption) {
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
        
        // Update block data with proper file URL
        const updatedData = { ...data };
        if (data.fileId && !updatedData.fileUrl) {
            updatedData.fileUrl = `/FileUpload/GetFile/${data.fileId}`;
        }
        this.updateBlockData(blockElement, updatedData);
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
