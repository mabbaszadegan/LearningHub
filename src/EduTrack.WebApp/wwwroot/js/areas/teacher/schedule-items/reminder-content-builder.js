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
            { type: 'audio', name: 'صوت' },
            { type: 'code', name: 'کد' }
        ];
        
        const selection = prompt('انتخاب نوع بلاک:\n1. متن\n2. تصویر\n3. ویدیو\n4. صوت\n5. کد\n\nلطفاً شماره مورد نظر را وارد کنید:');
        
        if (selection && selection >= 1 && selection <= 5) {
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
            { type: 'audio', name: 'صوت' },
            { type: 'code', name: 'کد' }
        ];
        
        const selection = prompt('انتخاب نوع بلاک:\n1. متن\n2. تصویر\n3. ویدیو\n4. صوت\n5. کد\n\nلطفاً شماره مورد نظر را وارد کنید:');
        
        if (selection && selection >= 1 && selection <= 5) {
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
        // Update both sidebar and modal preview
        window.reminderBlockManager.updatePreview();
        // Show modal
        window.reminderBlockManager.showPreviewModal();
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
        this.isLoadingExistingContent = false;
        console.log('Hidden field found:', this.hiddenField);
        console.log('Preview element found:', this.preview);
        
        // Setup global lazy loading for media elements
        this.setupGlobalLazyLoading();
        
        this.init();
    }
    
    setupGlobalLazyLoading() {
        // Setup global event delegation for lazy loading media elements
        document.addEventListener('play', (e) => {
            const target = e.target;
            if (target.tagName === 'AUDIO' || target.tagName === 'VIDEO') {
                const source = target.querySelector('source[data-src]');
                if (source && source.dataset.src && !source.src) {
                    source.src = source.dataset.src;
                    target.load(); // Reload the media element
                }
            }
        }, true); // Use capture phase to catch the event early
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
            // Find the button element (handle clicks on child elements like icons/text)
            const startBtn = e.target.closest('[data-action="start-recording"]');
            const stopBtn = e.target.closest('[data-action="stop-recording"]');
            const playBtn = e.target.closest('[data-action="play-recording"]');
            
            if (startBtn) {
                e.preventDefault();
                e.stopPropagation();
                this.startRecording(startBtn);
            } else if (stopBtn) {
                e.preventDefault();
                e.stopPropagation();
                this.stopRecording(stopBtn);
            } else if (playBtn) {
                e.preventDefault();
                e.stopPropagation();
                this.playRecording(playBtn);
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
        
        // File change buttons
        document.addEventListener('click', (e) => {
            if (e.target.matches('[data-action="change-image"]')) {
                this.handleChangeFile(e.target, 'image');
            } else if (e.target.matches('[data-action="change-video"]')) {
                this.handleChangeFile(e.target, 'video');
            } else if (e.target.matches('[data-action="change-audio"]')) {
                this.handleChangeFile(e.target, 'audio');
            } else if (e.target.matches('[data-action="reset-file"]')) {
                this.handleResetFile(e.target);
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
            { type: 'audio', name: 'صوت', icon: 'fas fa-microphone' },
            { type: 'code', name: 'کد', icon: 'fas fa-code' }
        ];
        
        const selection = prompt('انتخاب نوع بلاک:\n1. متن\n2. تصویر\n3. ویدیو\n4. صوت\n5. کد\n\nلطفاً شماره مورد نظر را وارد کنید:');
        
        if (selection && selection >= 1 && selection <= 5) {
            const selectedType = types[selection - 1].type;
            this.addBlock(selectedType);
        }
    }
    
    addBlock(type) {
        const blockId = `block-${this.nextBlockId++}`;
        const insertionIndex = this.insertionPoint !== undefined ? this.insertionPoint : this.blocks.length;
        
        const block = {
            id: blockId,
            type: type,
            order: insertionIndex,
            data: this.getDefaultBlockData(type)
        };
        
        // Insert block at the specified position
        this.blocks.splice(insertionIndex, 0, block);
        
        // Update order values for all blocks to ensure consistency
        this.updateBlockOrders();
        
        this.renderBlock(block);
        this.updateEmptyState();
        this.updateHiddenField();
        
        // Clear insertion point
        this.insertionPoint = undefined;
        
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
                    previewUrl: null,
                    pendingFile: null,
                    originalFileName: null,
                    fileSize: null,
                    mimeType: null,
                    size: 'medium',
                    position: 'center',
                    layout: 'standalone',
                    caption: '',
                    captionPosition: 'bottom'
                };
            case 'video':
                return {
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    previewUrl: null,
                    pendingFile: null,
                    originalFileName: null,
                    fileSize: null,
                    mimeType: null,
                    size: 'medium',
                    position: 'center',
                    layout: 'standalone',
                    caption: '',
                    captionPosition: 'bottom'
                };
            case 'audio':
                return {
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    previewUrl: null,
                    pendingFile: null,
                    originalFileName: null,
                    fileSize: null,
                    mimeType: null,
                    caption: '',
                    isRecorded: false,
                    duration: null
                };
            case 'code':
                return {
                    codeContent: '',
                    language: 'plaintext',
                    theme: 'default',
                    codeTitle: '',
                    size: 'medium',
                    showLineNumbers: true,
                    enableCopyButton: true
                };
            default:
                return {};
        }
    }
    
    renderBlock(block) {
        const template = document.querySelector(`#contentBlockTemplates .content-block-template[data-type="${block.type}"]`);
        if (!template) {
            console.error(`Template not found for block type: ${block.type}`);
            return;
        }
        
        const blockElement = template.cloneNode(true);
        blockElement.classList.add('content-block');
        blockElement.dataset.blockId = block.id;
        
        // Update block content based on data
        this.updateBlockContent(blockElement, block);
        
        // Setup event listeners for this block
        this.setupBlockEventListeners(blockElement);
        
        // Initialize file change buttons
        this.initializeFileChangeButtons(blockElement);
        
        // Setup lazy loading for audio/video elements
        this.setupLazyLoading(blockElement);
        
        // Insert at correct position
        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }
    }




    
    setupLazyLoading(blockElement) {
        // Setup lazy loading for audio elements
        const audioElements = blockElement.querySelectorAll('audio');
        audioElements.forEach(audio => {
            const source = audio.querySelector('source[data-src]');
            if (source) {
                // Add event listener for when user wants to play
                audio.addEventListener('play', () => {
                    if (source.dataset.src && !source.src) {
                        source.src = source.dataset.src;
                        audio.load(); // Reload the audio element
                    }
                }, { once: true }); // Only run once
            }
        });
        
        // Setup lazy loading for video elements
        const videoElements = blockElement.querySelectorAll('video');
        videoElements.forEach(video => {
            const source = video.querySelector('source[data-src]');
            if (source) {
                // Add event listener for when user wants to play
                video.addEventListener('play', () => {
                    if (source.dataset.src && !source.src) {
                        source.src = source.dataset.src;
                        video.load(); // Reload the video element
                    }
                }, { once: true }); // Only run once
            }
        });
    }

    setupBlockEventListeners(blockElement) {
        // Text editor events
        const textEditor = blockElement.querySelector('.rich-text-editor');
        if (textEditor) {
            textEditor.addEventListener('input', () => this.updateTextBlockContent(textEditor));
            textEditor.addEventListener('blur', () => this.updateTextBlockContent(textEditor));
        }
        
        // Toolbar button events - use event delegation
        const toolbar = blockElement.querySelector('.text-editor-toolbar');
        if (toolbar) {
            toolbar.addEventListener('click', (e) => {
                const button = e.target.closest('.toolbar-btn');
                if (button) {
                    e.preventDefault();
                    e.stopPropagation();
                    this.executeTextCommand(button);
                }
            });
        }
        
        // Block action events - use event delegation
        const blockActions = blockElement.querySelector('.block-actions');
        if (blockActions) {
            blockActions.addEventListener('click', (e) => {
                const button = e.target.closest('.btn-icon');
                if (button) {
                    e.preventDefault();
                    e.stopPropagation();
                    const action = button.dataset.action;
                    this.handleBlockAction(action, blockElement);
                }
            });
        }
        
        // Collapse icon event
        const collapseIcon = blockElement.querySelector('.collapse-icon');
        if (collapseIcon) {
            collapseIcon.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.toggleCollapse(blockElement);
            });
        }
        
        // Block header click for collapse
        const blockHeader = blockElement.querySelector('.block-header');
        if (blockHeader) {
            blockHeader.addEventListener('click', (e) => {
                // Only collapse if clicking on header, not on buttons, and not in fullscreen
                if (!e.target.closest('.block-actions') && 
                    !e.target.closest('.collapse-icon') && 
                    !blockElement.classList.contains('fullscreen')) {
                    this.toggleCollapse(blockElement);
                }
            });
        }
        
        // Caption events
        const captionTextarea = blockElement.querySelector('[data-caption="true"]');
        if (captionTextarea) {
            captionTextarea.addEventListener('input', () => this.updateBlockCaption(captionTextarea));
        }
        
        // File input events
        const fileInputs = blockElement.querySelectorAll('input[type="file"]');
        fileInputs.forEach(input => {
            input.addEventListener('change', (e) => {
                if (e.target.matches('input[data-action="image-upload"]')) {
                    this.handleImageUpload(e.target);
                } else if (e.target.matches('input[data-action="video-upload"]')) {
                    this.handleVideoUpload(e.target);
                } else if (e.target.matches('input[data-action="audio-upload"]')) {
                    this.handleAudioUpload(e.target);
                }
            });
        });
        
        // Settings events
        const settingsSelects = blockElement.querySelectorAll('[data-setting]');
        settingsSelects.forEach(select => {
            select.addEventListener('change', () => this.updateBlockSettings(blockElement));
        });
        
        // Code block events
        if (blockElement.dataset.type === 'code') {
            this.setupCodeBlockEventListeners(blockElement);
        }
    }
    
    setupCodeBlockEventListeners(blockElement) {
        // Code textarea events
        const codeTextarea = blockElement.querySelector('[data-code-content]');
        if (codeTextarea) {
            codeTextarea.addEventListener('input', (e) => {
                // Ensure textarea only contains plain text
                const plainText = e.target.value;
                if (e.target.value !== plainText) {
                    e.target.value = plainText;
                }
                this.updateCodePreview(blockElement);
                this.updateCodeBlockData(blockElement);
            });
            
            codeTextarea.addEventListener('keydown', (e) => {
                // Handle Tab key for indentation
                if (e.key === 'Tab') {
                    e.preventDefault();
                    const start = codeTextarea.selectionStart;
                    const end = codeTextarea.selectionEnd;
                    const value = codeTextarea.value;
                    
                    if (e.shiftKey) {
                        // Shift+Tab: Remove indentation
                        const lines = value.substring(0, start).split('\n');
                        const currentLine = lines[lines.length - 1];
                        if (currentLine.startsWith('    ')) {
                            const newValue = value.substring(0, start - 4) + value.substring(start);
                            codeTextarea.value = newValue;
                            codeTextarea.setSelectionRange(start - 4, end - 4);
                        }
                    } else {
                        // Tab: Add indentation
                        const newValue = value.substring(0, start) + '    ' + value.substring(end);
                        codeTextarea.value = newValue;
                        codeTextarea.setSelectionRange(start + 4, end + 4);
                    }
                    
                    this.updateCodePreview(blockElement);
                    this.updateCodeBlockData(blockElement);
                }
            });
        }
        
        // Code action events
        const codeActions = blockElement.querySelector('.code-actions');
        if (codeActions) {
            codeActions.addEventListener('click', (e) => {
                const button = e.target.closest('.btn-icon');
                if (button) {
                    e.preventDefault();
                    e.stopPropagation();
                    const action = button.dataset.action;
                    this.handleCodeAction(action, blockElement);
                }
            });
        }
        
        // Settings change events for code blocks
        const languageSelect = blockElement.querySelector('[data-setting="language"]');
        if (languageSelect) {
            languageSelect.addEventListener('change', () => {
                this.updateCodeBlockData(blockElement);
                this.updateCodePreview(blockElement);
                this.updateLanguageBadge(blockElement);
            });
        }
        
        const themeSelect = blockElement.querySelector('[data-setting="theme"]');
        if (themeSelect) {
            themeSelect.addEventListener('change', () => {
                this.updateCodeBlockData(blockElement);
                this.updateCodeTheme(blockElement);
            });
        }
        
        const sizeSelect = blockElement.querySelector('[data-setting="size"]');
        if (sizeSelect) {
            sizeSelect.addEventListener('change', () => {
                this.updateCodeBlockData(blockElement);
                this.updateCodeSize(blockElement);
            });
        }
        
        const titleInput = blockElement.querySelector('[data-setting="title"]');
        if (titleInput) {
            titleInput.addEventListener('input', () => {
                this.updateCodeBlockData(blockElement);
            });
        }
        
        const showLineNumbersCheckbox = blockElement.querySelector('[data-setting="showLineNumbers"]');
        if (showLineNumbersCheckbox) {
            showLineNumbersCheckbox.addEventListener('change', () => {
                this.updateCodeBlockData(blockElement);
            });
        }
        
        const enableCopyButtonCheckbox = blockElement.querySelector('[data-setting="enableCopyButton"]');
        if (enableCopyButtonCheckbox) {
            enableCopyButtonCheckbox.addEventListener('change', () => {
                this.updateCodeBlockData(blockElement);
            });
        }
    }
    
    handleCodeAction(action, blockElement) {
        switch (action) {
            case 'copy-code':
                this.copyCodeToClipboard(blockElement);
                break;
            case 'format-code':
                this.formatCode(blockElement);
                break;
        }
    }
    
    copyCodeToClipboard(blockElement) {
        const textarea = blockElement.querySelector('[data-code-content]');
        if (textarea) {
            navigator.clipboard.writeText(textarea.value).then(() => {
                const button = blockElement.querySelector('[data-action="copy-code"]');
                if (button) {
                    const originalIcon = button.innerHTML;
                    button.innerHTML = '<i class="fas fa-check"></i>';
                    button.classList.add('copied');
                    
                    setTimeout(() => {
                        button.innerHTML = originalIcon;
                        button.classList.remove('copied');
                    }, 2000);
                }
            }).catch(err => {
                console.error('Failed to copy code:', err);
                // Fallback for older browsers
                textarea.select();
                document.execCommand('copy');
            });
        }
    }
    
    formatCode(blockElement) {
        const textarea = blockElement.querySelector('[data-code-content]');
        const language = blockElement.querySelector('[data-setting="language"]')?.value || 'plaintext';
        
        if (!textarea) return;
        
        let formatted = textarea.value;
        
        // Basic formatting for different languages
        switch (language) {
            case 'javascript':
            case 'typescript':
                formatted = this.formatJavaScript(formatted);
                break;
            case 'json':
                formatted = this.formatJSON(formatted);
                break;
            case 'html':
                formatted = this.formatHTML(formatted);
                break;
            case 'css':
            case 'scss':
                formatted = this.formatCSS(formatted);
                break;
        }
        
        if (formatted !== textarea.value) {
            textarea.value = formatted;
            this.updateCodePreview(blockElement);
            this.updateCodeBlockData(blockElement);
        }
    }
    
    formatJavaScript(code) {
        // Basic JavaScript formatting
        return code
            .replace(/\s*{\s*/g, ' {\n    ')
            .replace(/;\s*/g, ';\n')
            .replace(/\s*}\s*/g, '\n}\n')
            .replace(/\n\s*\n/g, '\n')
            .trim();
    }
    
    formatJSON(code) {
        try {
            const parsed = JSON.parse(code);
            return JSON.stringify(parsed, null, 2);
        } catch (e) {
            return code; // Return original if not valid JSON
        }
    }
    
    formatHTML(code) {
        // Basic HTML formatting
        return code
            .replace(/></g, '>\n<')
            .replace(/\n\s*\n/g, '\n')
            .trim();
    }
    
    formatCSS(code) {
        // Basic CSS formatting
        return code
            .replace(/\s*{\s*/g, ' {\n    ')
            .replace(/;\s*/g, ';\n')
            .replace(/\s*}\s*/g, '\n}\n')
            .replace(/\n\s*\n/g, '\n')
            .trim();
    }
    
    updateCodeBlockData(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            const textarea = blockElement.querySelector('[data-code-content]');
            const languageSelect = blockElement.querySelector('[data-setting="language"]');
            const themeSelect = blockElement.querySelector('[data-setting="theme"]');
            const sizeSelect = blockElement.querySelector('[data-setting="size"]');
            const titleInput = blockElement.querySelector('[data-setting="title"]');
            const showLineNumbersCheckbox = blockElement.querySelector('[data-setting="showLineNumbers"]');
            const enableCopyButtonCheckbox = blockElement.querySelector('[data-setting="enableCopyButton"]');
            
            block.data.codeContent = textarea?.value || '';
            block.data.language = languageSelect?.value || 'plaintext';
            block.data.theme = themeSelect?.value || 'default';
            block.data.size = sizeSelect?.value || 'medium';
            block.data.codeTitle = titleInput?.value || '';
            block.data.showLineNumbers = showLineNumbersCheckbox?.checked || true;
            block.data.enableCopyButton = enableCopyButtonCheckbox?.checked || true;
            
            this.updateHiddenField();
        }
    }
    
    updateLanguageBadge(blockElement) {
        const languageSelect = blockElement.querySelector('[data-setting="language"]');
        const languageBadge = blockElement.querySelector('.language-name');
        
        if (languageSelect && languageBadge) {
            const languageNames = {
                'javascript': 'JavaScript',
                'python': 'Python',
                'csharp': 'C#',
                'java': 'Java',
                'cpp': 'C++',
                'c': 'C',
                'php': 'PHP',
                'ruby': 'Ruby',
                'go': 'Go',
                'rust': 'Rust',
                'swift': 'Swift',
                'kotlin': 'Kotlin',
                'typescript': 'TypeScript',
                'html': 'HTML',
                'css': 'CSS',
                'scss': 'SCSS',
                'sql': 'SQL',
                'json': 'JSON',
                'xml': 'XML',
                'yaml': 'YAML',
                'markdown': 'Markdown',
                'bash': 'Bash',
                'powershell': 'PowerShell',
                'plaintext': 'Plain Text'
            };
            languageBadge.textContent = languageNames[languageSelect.value] || 'Plain Text';
        }
    }
    
    updateCodeTheme(blockElement) {
        const themeSelect = blockElement.querySelector('[data-setting="theme"]');
        if (themeSelect) {
            blockElement.className = blockElement.className.replace(/code-block-theme-\w+/g, '');
            blockElement.classList.add(`code-block-theme-${themeSelect.value}`);
        }
    }
    
    updateCodeSize(blockElement) {
        const sizeSelect = blockElement.querySelector('[data-setting="size"]');
        if (sizeSelect) {
            blockElement.className = blockElement.className.replace(/code-block-size-\w+/g, '');
            blockElement.classList.add(`code-block-size-${sizeSelect.value}`);
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
                if (block.data.previewUrl || block.data.fileUrl || block.data.fileId) {
                    const uploadArea = element.querySelector('.image-upload-area');
                    const preview = element.querySelector('.image-preview');
                    const img = element.querySelector('.preview-image');
                    
                    uploadArea.style.display = 'none';
                    preview.style.display = 'flex';
                    
                    // Use previewUrl if available, otherwise construct from fileId
                    if (block.data.previewUrl) {
                        img.src = block.data.previewUrl;
                    } else if (block.data.fileUrl) {
                        img.src = block.data.fileUrl;
                    } else if (block.data.fileId) {
                        img.src = `/FileUpload/GetFile/${block.data.fileId}`;
                    }
                }
                break;
            case 'video':
                if (block.data.previewUrl || block.data.fileUrl || block.data.fileId) {
                    const uploadArea = element.querySelector('.video-upload-area');
                    const preview = element.querySelector('.video-preview');
                    const video = element.querySelector('.preview-video source');
                    
                    uploadArea.style.display = 'none';
                    preview.style.display = 'flex';
                    
                    // Use previewUrl if available, otherwise construct from fileId
                    if (block.data.previewUrl) {
                        video.src = block.data.previewUrl;
                    } else if (block.data.fileUrl) {
                        video.src = block.data.fileUrl;
                    } else if (block.data.fileId) {
                        video.src = `/FileUpload/GetFile/${block.data.fileId}`;
                    }
                }
                break;
            case 'audio':
                if (block.data.previewUrl || block.data.fileUrl || block.data.fileId) {
                    const uploadArea = element.querySelector('.audio-upload-area');
                    const preview = element.querySelector('.audio-preview');
                    const audio = element.querySelector('.preview-audio source');
                    
                    uploadArea.style.display = 'none';
                    preview.style.display = 'flex';
                    
                    // Use previewUrl if available, otherwise construct from fileId
                    if (block.data.previewUrl) {
                        audio.src = block.data.previewUrl;
                    } else if (block.data.fileUrl) {
                        audio.src = block.data.fileUrl;
                    } else if (block.data.fileId) {
                        audio.src = `/FileUpload/GetFile/${block.data.fileId}`;
                    }
                }
                break;
            case 'code':
                this.updateCodeBlockContent(element, block);
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
    
    updateCodeBlockContent(element, block) {
        // Update code content
        const textarea = element.querySelector('[data-code-content]');
        const preview = element.querySelector('[data-code-preview]');
        const languageBadge = element.querySelector('.language-name');
        
        if (textarea && block.data.codeContent) {
            textarea.value = block.data.codeContent;
            this.updateCodePreview(element);
        }
        
        // Update language badge
        if (languageBadge) {
            const languageNames = {
                'javascript': 'JavaScript',
                'python': 'Python',
                'csharp': 'C#',
                'java': 'Java',
                'cpp': 'C++',
                'c': 'C',
                'php': 'PHP',
                'ruby': 'Ruby',
                'go': 'Go',
                'rust': 'Rust',
                'swift': 'Swift',
                'kotlin': 'Kotlin',
                'typescript': 'TypeScript',
                'html': 'HTML',
                'css': 'CSS',
                'scss': 'SCSS',
                'sql': 'SQL',
                'json': 'JSON',
                'xml': 'XML',
                'yaml': 'YAML',
                'markdown': 'Markdown',
                'bash': 'Bash',
                'powershell': 'PowerShell',
                'plaintext': 'Plain Text'
            };
            languageBadge.textContent = languageNames[block.data.language] || 'Plain Text';
        }
        
        // Update theme classes
        element.className = element.className.replace(/code-block-theme-\w+/g, '');
        element.classList.add(`code-block-theme-${block.data.theme || 'default'}`);
        
        // Update size classes
        element.className = element.className.replace(/code-block-size-\w+/g, '');
        element.classList.add(`code-block-size-${block.data.size || 'medium'}`);
    }
    
    updateCodePreview(element) {
        const textarea = element.querySelector('[data-code-content]');
        const preview = element.querySelector('[data-code-preview]');
        const lineCountSpan = element.querySelector('[data-line-count]');
        const charCountSpan = element.querySelector('[data-char-count]');
        
        if (!textarea || !preview) return;
        
        const content = textarea.value;
        const lines = content.split('\n').length;
        const chars = content.length;
        
        // Update stats
        if (lineCountSpan) lineCountSpan.textContent = lines;
        if (charCountSpan) charCountSpan.textContent = chars;
        
        // Update preview with syntax highlighting
        const highlighted = this.highlightSyntax(content, element);
        preview.innerHTML = highlighted;
        
        // Ensure textarea only contains plain text (no HTML tags)
        if (textarea.value !== content) {
            textarea.value = content;
        }
    }
    
    highlightSyntax(code, element) {
        const language = element.querySelector('[data-setting="language"]')?.value || 'plaintext';
        
        // First escape any existing HTML to prevent XSS
        let escaped = this.escapeHtml(code);
        
        if (language === 'plaintext') {
            return escaped;
        }
        
        // Apply syntax highlighting - the methods add HTML tags
        let highlighted = escaped;
        
        switch (language) {
            case 'javascript':
            case 'typescript':
                highlighted = this.highlightJavaScript(highlighted);
                break;
            case 'python':
                highlighted = this.highlightPython(highlighted);
                break;
            case 'html':
                highlighted = this.highlightHTMLCode(highlighted);
                break;
            case 'css':
            case 'scss':
                highlighted = this.highlightCSS(highlighted);
                break;
            case 'json':
                highlighted = this.highlightJSON(highlighted);
                break;
            case 'sql':
                highlighted = this.highlightSQL(highlighted);
                break;
        }
        
        return highlighted;
    }
    
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
    
    highlightJavaScript(code) {
        return code
            .replace(/\b(function|const|let|var|if|else|for|while|return|class|import|export|from|async|await|try|catch|finally|throw|new|this|typeof|instanceof|in|of|true|false|null|undefined)\b/g, '<span class="keyword">$1</span>')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/\/\/.*$/gm, '<span class="comment">$&</span>')
            .replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>')
            .replace(/\b\d+\.?\d*\b/g, '<span class="number">$&</span>');
    }
    
    highlightPython(code) {
        return code
            .replace(/\b(def|class|if|elif|else|for|while|try|except|finally|with|import|from|return|yield|lambda|and|or|not|in|is|True|False|None)\b/g, '<span class="keyword">$1</span>')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/#.*$/gm, '<span class="comment">$&</span>')
            .replace(/\b\d+\.?\d*\b/g, '<span class="number">$&</span>');
    }
    
    highlightHTMLCode(code) {
        return code
            .replace(/&lt;(\/?[^&]+)&gt;/g, '<span class="keyword">&lt;$1&gt;</span>')
            .replace(/(["'])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/&lt;!--[\s\S]*?--&gt;/g, '<span class="comment">$&</span>');
    }
    
    highlightCSS(code) {
        return code
            .replace(/([.#]?[a-zA-Z-]+)\s*\{/g, '<span class="function">$1</span> {')
            .replace(/([a-zA-Z-]+)\s*:/g, '<span class="variable">$1</span>:')
            .replace(/(["'])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>')
            .replace(/\b\d+[a-zA-Z%]*\b/g, '<span class="number">$&</span>');
    }
    
    highlightJSON(code) {
        return code
            .replace(/("(?:[^"\\]|\\.)*")\s*:/g, '<span class="variable">$1</span>:')
            .replace(/: ("(?:[^"\\]|\\.)*")/g, ': <span class="string">$1</span>')
            .replace(/: (true|false|null)/g, ': <span class="keyword">$1</span>')
            .replace(/: (\d+\.?\d*)/g, ': <span class="number">$1</span>');
    }
    
    highlightSQL(code) {
        return code
            .replace(/\b(SELECT|FROM|WHERE|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP|TABLE|INDEX|PRIMARY|KEY|FOREIGN|REFERENCES|JOIN|INNER|LEFT|RIGHT|OUTER|ON|GROUP|BY|ORDER|HAVING|UNION|DISTINCT|COUNT|SUM|AVG|MIN|MAX|AS|AND|OR|NOT|IN|EXISTS|BETWEEN|LIKE|IS|NULL)\b/gi, '<span class="keyword">$1</span>')
            .replace(/(["'])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/--.*$/gm, '<span class="comment">$&</span>')
            .replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>');
    }
    
    updateBlockOrders() {
        // Update order values for all blocks to match their current positions
        this.blocks.forEach((block, index) => {
            block.order = index;
        });
        
        // Debug log to verify order is correct
        console.log('Block orders updated:', this.blocks.map(b => ({ id: b.id, order: b.order })));
    }
    
    updateEmptyState() {
        if (this.blocks.length === 0) {
            this.emptyState.style.display = 'flex';
        } else {
            this.emptyState.style.display = 'none';
        }
    }
    
    moveBlockUp(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex > 0) {
            // Swap blocks
            [this.blocks[blockIndex], this.blocks[blockIndex - 1]] = [this.blocks[blockIndex - 1], this.blocks[blockIndex]];
            
            // Update order values to match new positions
            this.updateBlockOrders();
            
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
    
    moveBlockDown(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex < this.blocks.length - 1) {
            // Swap blocks
            [this.blocks[blockIndex], this.blocks[blockIndex + 1]] = [this.blocks[blockIndex + 1], this.blocks[blockIndex]];
            
            // Update order values to match new positions
            this.updateBlockOrders();
            
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
    
    insertBlockAbove(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        this.showBlockTypeModal();
        // Store the insertion point for after modal selection
        this.insertionPoint = blockIndex;
    }
    
    deleteBlock(blockElement) {
        if (confirm('آیا از حذف این بلاک اطمینان دارید؟')) {
            const blockId = blockElement.dataset.blockId;
            
            // Remove from blocks array
            this.blocks = this.blocks.filter(b => b.id !== blockId);
            
            // Update order values for remaining blocks
            this.updateBlockOrders();
            
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
        
        // Store file for later upload
        block.data.pendingFile = file;
        block.data.originalFileName = file.name;
        block.data.fileSize = file.size;
        block.data.mimeType = file.type;
        
        // Create preview URL without storing base64 in JSON
        block.data.previewUrl = URL.createObjectURL(file);
        this.updateBlockContent(blockElement, block);
        this.updateHiddenField();
        
        // Clear the input to allow selecting the same file again
        input.value = '';
    }
    
    handleVideoUpload(input) {
        const file = input.files[0];
        if (!file) return;
        
        const blockElement = input.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        // Store file for later upload
        block.data.pendingFile = file;
        block.data.originalFileName = file.name;
        block.data.fileSize = file.size;
        block.data.mimeType = file.type;
        
        // Create preview URL without storing base64 in JSON
        block.data.previewUrl = URL.createObjectURL(file);
        this.updateBlockContent(blockElement, block);
        this.updateHiddenField();
        
        // Clear the input to allow selecting the same file again
        input.value = '';
    }
    
    handleAudioUpload(input) {
        const file = input.files[0];
        if (!file) return;
        
        const blockElement = input.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        // Store file for later upload
        block.data.pendingFile = file;
        block.data.originalFileName = file.name;
        block.data.fileSize = file.size;
        block.data.mimeType = file.type;
        block.data.isRecorded = false;
        
        // Create preview URL without storing base64 in JSON
        block.data.previewUrl = URL.createObjectURL(file);
        this.updateBlockContent(blockElement, block);
        this.updateHiddenField();
        
        // Clear the input to allow selecting the same file again
        input.value = '';
    }
    
    handleChangeFile(button, fileType) {
        const blockElement = button.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (!block) return;
        
        // Store original data for reset functionality
        if (!block.data.originalData) {
            block.data.originalData = JSON.parse(JSON.stringify(block.data));
        }
        
        // Create file input
        const fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.style.display = 'none';
        
        // Set accept attribute based on file type
        switch (fileType) {
            case 'image':
                fileInput.accept = 'image/*';
                fileInput.setAttribute('data-action', 'image-upload');
                break;
            case 'video':
                fileInput.accept = 'video/*';
                fileInput.setAttribute('data-action', 'video-upload');
                break;
            case 'audio':
                fileInput.accept = 'audio/*';
                fileInput.setAttribute('data-action', 'audio-upload');
                break;
        }
        
        // Add to DOM temporarily
        document.body.appendChild(fileInput);
        
        // Handle file selection
        fileInput.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file) {
                // Remove old file data
                delete block.data.pendingFile;
                delete block.data.previewUrl;
                delete block.data.originalFileName;
                delete block.data.fileSize;
                delete block.data.mimeType;
                delete block.data.isRecorded;
                
                // Process new file
                if (fileType === 'image') {
                    this.handleImageUpload(fileInput);
                } else if (fileType === 'video') {
                    this.handleVideoUpload(fileInput);
                } else if (fileType === 'audio') {
                    this.handleAudioUpload(fileInput);
                }
                
                // Update UI to show reset button
                this.updateFileChangeButtons(blockElement, true);
            }
            
            // Clean up
            document.body.removeChild(fileInput);
        });
        
        // Trigger file selection
        fileInput.click();
    }
    
    handleResetFile(button) {
        const blockElement = button.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (!block || !block.data.originalData) return;
        
        // Restore original data
        block.data = JSON.parse(JSON.stringify(block.data.originalData));
        delete block.data.originalData;
        
        // Update UI
        this.updateBlockContent(blockElement, block);
        this.updateFileChangeButtons(blockElement, false);
        this.updateHiddenField();
    }
    
    updateFileChangeButtons(blockElement, hasChanged) {
        const changeButton = blockElement.querySelector('[data-action^="change-"]');
        const resetButton = blockElement.querySelector('[data-action="reset-file"]');
        
        if (changeButton) {
            changeButton.style.display = hasChanged ? 'none' : 'inline-flex';
        }
        
        if (resetButton) {
            resetButton.style.display = hasChanged ? 'inline-flex' : 'none';
        } else if (hasChanged) {
            // Create reset button if it doesn't exist
            const resetBtn = document.createElement('button');
            resetBtn.type = 'button';
            resetBtn.className = 'btn-teacher btn-warning btn-sm';
            resetBtn.setAttribute('data-action', 'reset-file');
            resetBtn.innerHTML = '<i class="fas fa-undo"></i><span>بازگشت به حالت اولیه</span>';
            
            // Insert after change button
            if (changeButton) {
                changeButton.parentNode.insertBefore(resetBtn, changeButton.nextSibling);
            }
        }
    }
    
    initializeFileChangeButtons(blockElement) {
        // Initialize file change buttons for image, video, and audio blocks
        const changeButtons = blockElement.querySelectorAll('[data-action^="change-"]');
        changeButtons.forEach(button => {
            button.style.display = 'inline-flex';
        });
        
        // Remove any existing reset buttons
        const resetButtons = blockElement.querySelectorAll('[data-action="reset-file"]');
        resetButtons.forEach(button => {
            button.remove();
        });
    }
    
    showUploadProgress(blockElement, show) {
        const progressElement = blockElement.querySelector('.upload-progress');
        const placeholderElement = blockElement.querySelector('.upload-placeholder');
        
        if (progressElement) {
            progressElement.style.display = show ? 'block' : 'none';
        }
        
        if (placeholderElement) {
            placeholderElement.style.display = show ? 'none' : 'block';
        }
    }
    
    async uploadAllPendingFiles() {
        const uploadPromises = [];
        
        for (const block of this.blocks) {
            if (block.data.pendingFile) {
                const uploadPromise = this.uploadBlockFile(block);
                uploadPromises.push(uploadPromise);
            }
        }
        
        if (uploadPromises.length > 0) {
            try {
                await Promise.all(uploadPromises);
                console.log('All files uploaded successfully');
                return true;
            } catch (error) {
                console.error('Error uploading files:', error);
                throw error;
            }
        }
        
        return true;
    }
    
    getFileExtension(mimeType) {
        // Always return MP3 for recorded audio files
        return 'mp3';
    }
    
    async uploadBlockFile(block) {
        const file = block.data.pendingFile;
        const fileType = block.type;
        
        try {
            const formData = new FormData();
            
            // For audio files, ensure proper filename and MIME type
            if (fileType === 'audio' && file instanceof Blob) {
                const fileName = block.data.originalFileName || `recording_${Date.now()}.mp3`;
                // Create a new File object with MP3 MIME type
                const mp3File = new File([file], fileName, { type: 'audio/mpeg' });
                formData.append('file', mp3File);
            } else {
                formData.append('file', file);
            }
            
            formData.append('type', fileType);
            
            const response = await fetch('/FileUpload/UploadContentFile', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            });
            
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }
            
            const responseText = await response.text();
            if (!responseText) {
                throw new Error('Empty response from server');
            }
            
            const result = JSON.parse(responseText);
            
            if (result.success) {
                // Update block data with server response
                block.data.fileId = result.data.id;
                block.data.fileName = result.data.fileName;
                block.data.fileUrl = result.data.url;
                block.data.fileSize = result.data.size;
                block.data.mimeType = result.data.mimeType;
                
                // Remove pending file
                delete block.data.pendingFile;
                delete block.data.previewUrl;
                
                console.log(`File uploaded successfully for block ${block.id}`);
            } else {
                throw new Error(result.message || 'خطا در آپلود فایل');
            }
        } catch (error) {
            console.error(`Error uploading file for block ${block.id}:`, error);
            throw error;
        }
    }
    
    async startRecording(button) {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            
            // Use the best available format for recording
            const mimeTypes = [
                'audio/mp4; codecs=mp4a.40.2', // MP4 audio (widely supported)
                'audio/webm; codecs=opus',      // WebM with Opus (good quality)
                'audio/webm',                   // Basic WebM
                'audio/wav'                     // WAV fallback
            ];
            
            let selectedMimeType = null;
            for (const mimeType of mimeTypes) {
                if (MediaRecorder.isTypeSupported(mimeType)) {
                    selectedMimeType = mimeType;
                    break;
                }
            }
            
            if (!selectedMimeType) {
                throw new Error('No supported audio format found');
            }
            
            this.mediaRecorder = new MediaRecorder(stream, {
                mimeType: selectedMimeType
            });
            this.recordedChunks = [];
            
            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this.recordedChunks.push(event.data);
                }
            };
            
            this.mediaRecorder.onstop = () => {
                const blob = new Blob(this.recordedChunks, { type: this.mediaRecorder.mimeType });
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
            
            // Handle recorded data
            this.mediaRecorder.onstop = () => {
                const blob = new Blob(this.recordedChunks, { type: this.mediaRecorder.mimeType });
                const blockElement = button.closest('.content-block');
                const blockId = blockElement.dataset.blockId;
                const block = this.blocks.find(b => b.id === blockId);
                
                // Store recorded file for later upload
                block.data.pendingFile = blob;
                
                // Force MP3 format for all recordings
                block.data.originalFileName = `recording_${Date.now()}.mp3`;
                block.data.fileSize = blob.size;
                block.data.mimeType = 'audio/mpeg'; // Force MP3 MIME type
                block.data.isRecorded = true;
                block.data.duration = Math.floor((Date.now() - this.recordingStartTime) / 1000);
                
                // Create preview URL
                const url = URL.createObjectURL(blob);
                block.data.previewUrl = url;
                
                this.updateBlockContent(blockElement, block);
                this.updateHiddenField();
            };
        }
    }
    
    playRecording(button) {
        const blockElement = button.closest('.content-block');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        const audioUrl = block.data.previewUrl || block.data.fileUrl;
        if (audioUrl) {
            const audio = new Audio(audioUrl);
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
        const action = button.dataset.action;
        
        if (action === 'create-link') {
            this.showLinkDialog(blockElement, textEditor);
        } else if (action === 'text-formatting') {
            this.showTextFormattingDialog(blockElement, textEditor);
        } else if (action === 'insert-code') {
            this.showCodeBlockDialog(blockElement, textEditor);
        } else if (command) {
            document.execCommand(command, false, null);
            textEditor.focus();
            
            // Update block data
            this.updateTextBlockData(blockElement, textEditor);
        }
    }
    
    updateTextBlockData(blockElement, textEditor) {
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        if (block) {
            block.data.content = textEditor.innerHTML;
            block.data.textContent = textEditor.textContent || textEditor.innerText || '';
            this.updateHiddenField();
        }
    }

    showLinkDialog(blockElement, textEditor) {
        const linkModal = new bootstrap.Modal(document.getElementById('linkDialogModal'));
        const selectedText = window.getSelection().toString();
        
        // Pre-fill with selected text
        document.getElementById('linkText').value = selectedText || '';
        
        // Show modal
        linkModal.show();
        
        // Handle insert link
        document.getElementById('insertLinkBtn').onclick = () => {
            const linkText = document.getElementById('linkText').value;
            const linkUrl = document.getElementById('linkUrl').value;
            const linkTarget = document.getElementById('linkTarget').value;
            
            if (linkText && linkUrl) {
                const linkHTML = `<a href="${linkUrl}" target="${linkTarget}">${linkText}</a>`;
                
                if (selectedText) {
                    document.execCommand('insertHTML', false, linkHTML);
                } else {
                    textEditor.focus();
                    document.execCommand('insertHTML', false, linkHTML);
                }
                
                this.updateTextBlockData(blockElement, textEditor);
                linkModal.hide();
            }
        };
    }

    showTextFormattingDialog(blockElement, textEditor) {
        const formattingModal = new bootstrap.Modal(document.getElementById('textFormattingModal'));
        formattingModal.show();
        
        // Handle apply formatting
        document.getElementById('applyTextFormattingBtn').onclick = () => {
            const textSize = document.getElementById('textSize').value;
            const textColor = document.getElementById('textColor').value;
            const backgroundColor = document.getElementById('backgroundColor').value;
            const textAlign = document.getElementById('textAlign').value;
            
            const selectedText = window.getSelection().toString();
            
            if (selectedText) {
                const span = document.createElement('span');
                span.style.fontSize = textSize;
                span.style.color = textColor;
                span.style.backgroundColor = backgroundColor;
                span.style.textAlign = textAlign;
                span.textContent = selectedText;
                
                document.execCommand('insertHTML', false, span.outerHTML);
            } else {
                // Apply to entire block
                textEditor.style.fontSize = textSize;
                textEditor.style.color = textColor;
                textEditor.style.backgroundColor = backgroundColor;
                textEditor.style.textAlign = textAlign;
            }
            
            this.updateTextBlockData(blockElement, textEditor);
            formattingModal.hide();
        };
    }

    showCodeBlockDialog(blockElement, textEditor) {
        const codeModal = new bootstrap.Modal(document.getElementById('codeBlockModal'));
        codeModal.show();
        
        // Handle insert code block
        document.getElementById('insertCodeBlockBtn').onclick = () => {
            const language = document.getElementById('codeLanguage').value;
            const theme = document.getElementById('codeTheme').value;
            const title = document.getElementById('codeTitle').value;
            const content = document.getElementById('codeContent').value;
            
            if (content) {
                const codeHTML = `
                    <div class="code-block" data-language="${language}" data-theme="${theme}">
                        ${title ? `<div class="code-title">${title}</div>` : ''}
                        <pre><code class="language-${language}">${this.escapeHtml(content)}</code></pre>
                    </div>
                `;
                
                textEditor.focus();
                document.execCommand('insertHTML', false, codeHTML);
                this.updateTextBlockData(blockElement, textEditor);
                codeModal.hide();
            }
        };
    }

    toggleFullscreen(blockElement) {
        if (blockElement.classList.contains('fullscreen')) {
            this.exitFullscreen(blockElement);
        } else {
            this.enterFullscreen(blockElement);
        }
    }

    enterFullscreen(blockElement) {
        blockElement.classList.add('fullscreen');
        document.body.classList.add('block-fullscreen');
        
        // Store current fullscreen block
        this.currentFullscreenBlock = blockElement;
        
        // Create exit button
        const exitBtn = document.createElement('button');
        exitBtn.className = 'exit-fullscreen-btn';
        exitBtn.innerHTML = '<i class="fas fa-times"></i>';
        exitBtn.title = 'خروج از حالت تمام صفحه (Esc)';
        exitBtn.onclick = () => this.exitFullscreen(blockElement);
        document.body.appendChild(exitBtn);
        
        // Create navigation buttons
        this.createBlockNavigation(blockElement);
        
        // Add ESC key listener
        this.escKeyListener = (e) => {
            if (e.key === 'Escape') {
                this.exitFullscreen(blockElement);
            }
        };
        document.addEventListener('keydown', this.escKeyListener);
        
        // Disable other blocks' actions
        this.disableOtherBlocksActions(blockElement);
    }

    exitFullscreen(blockElement) {
        blockElement.classList.remove('fullscreen');
        document.body.classList.remove('block-fullscreen');
        
        // Clear current fullscreen block
        this.currentFullscreenBlock = null;
        
        // Remove exit button
        const exitBtn = document.querySelector('.exit-fullscreen-btn');
        if (exitBtn) {
            exitBtn.remove();
        }
        
        // Remove navigation
        const navigation = document.querySelector('.block-navigation');
        if (navigation) {
            navigation.remove();
        }
        
        // Remove ESC key listener
        if (this.escKeyListener) {
            document.removeEventListener('keydown', this.escKeyListener);
            this.escKeyListener = null;
        }
        
        // Enable other blocks' actions
        this.enableAllBlocksActions();
    }

    createBlockNavigation(currentBlock) {
        const navigation = document.createElement('div');
        navigation.className = 'block-navigation';
        
        const currentIndex = this.blocks.findIndex(b => b.id === currentBlock.dataset.blockId);
        const totalBlocks = this.blocks.length;
        
        // Previous button
        const prevBtn = document.createElement('button');
        prevBtn.className = 'nav-btn';
        prevBtn.innerHTML = '<i class="fas fa-chevron-left"></i>';
        prevBtn.title = 'بلاک قبلی';
        prevBtn.disabled = currentIndex === 0;
        prevBtn.onclick = () => this.navigateToBlock(currentIndex - 1);
        
        // Block counter
        const counter = document.createElement('div');
        counter.className = 'block-counter';
        counter.innerHTML = `<i class="fas fa-layer-group"></i> ${currentIndex + 1} از ${totalBlocks}`;
        
        // Next button
        const nextBtn = document.createElement('button');
        nextBtn.className = 'nav-btn';
        nextBtn.innerHTML = '<i class="fas fa-chevron-right"></i>';
        nextBtn.title = 'بلاک بعدی';
        nextBtn.disabled = currentIndex === totalBlocks - 1;
        nextBtn.onclick = () => this.navigateToBlock(currentIndex + 1);
        
        navigation.appendChild(prevBtn);
        navigation.appendChild(counter);
        navigation.appendChild(nextBtn);
        
        document.body.appendChild(navigation);
    }

    navigateToBlock(targetIndex) {
        if (targetIndex < 0 || targetIndex >= this.blocks.length) return;
        
        const targetBlockId = this.blocks[targetIndex].id;
        const targetBlockElement = document.querySelector(`[data-block-id="${targetBlockId}"]`);
        
        if (targetBlockElement) {
            // Exit current fullscreen
            this.exitFullscreen(this.currentFullscreenBlock);
            
            // Enter fullscreen for target block
            setTimeout(() => {
                this.enterFullscreen(targetBlockElement);
            }, 300);
        }
    }

    disableOtherBlocksActions(currentBlock) {
        const allBlocks = document.querySelectorAll('.content-block');
        allBlocks.forEach(block => {
            if (block !== currentBlock) {
                const actions = block.querySelector('.block-actions');
                if (actions) {
                    actions.style.opacity = '0.3';
                    actions.style.pointerEvents = 'none';
                }
            }
        });
        
        // Disable add block button
        const addBlockBtn = document.getElementById('addContentBlockBtn');
        if (addBlockBtn) {
            addBlockBtn.style.opacity = '0.3';
            addBlockBtn.style.pointerEvents = 'none';
        }
    }

    enableAllBlocksActions() {
        const allBlocks = document.querySelectorAll('.content-block');
        allBlocks.forEach(block => {
            const actions = block.querySelector('.block-actions');
            if (actions) {
                actions.style.opacity = '';
                actions.style.pointerEvents = '';
            }
        });
        
        // Enable add block button
        const addBlockBtn = document.getElementById('addContentBlockBtn');
        if (addBlockBtn) {
            addBlockBtn.style.opacity = '';
            addBlockBtn.style.pointerEvents = '';
        }
    }

    toggleCollapse(blockElement) {
        // Don't allow collapse in fullscreen mode
        if (blockElement.classList.contains('fullscreen')) {
            return;
        }
        
        blockElement.classList.toggle('collapsed');
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    handleBlockAction(action, blockElement) {
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (!block) return;
        
        switch (action) {
            case 'move-up':
                this.moveBlockUp(blockElement);
                break;
            case 'move-down':
                this.moveBlockDown(blockElement);
                break;
            case 'delete':
                this.deleteBlock(blockElement);
                break;
            case 'insert-above':
                this.insertBlockAbove(blockElement);
                break;
            case 'fullscreen':
                this.toggleFullscreen(blockElement);
                break;
            case 'toggle-collapse':
                this.toggleCollapse(blockElement);
                break;
            case 'edit':
                // Handle edit action if needed
                break;
        }
    }

    updateBlockSettings(blockElement) {
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (!block) return;
        
        // Update settings from form elements
        const settingsSelects = blockElement.querySelectorAll('[data-setting]');
        settingsSelects.forEach(select => {
            const setting = select.dataset.setting;
            block.data[setting] = select.value;
        });
        
        this.updateHiddenField();
    }

    showPreviewModal() {
        // Show the modal
        const previewModal = new bootstrap.Modal(document.getElementById('previewModal'));
        previewModal.show();
    }

    updateModalPreview() {
        const modalPreview = document.getElementById('modalReminderPreview');
        if (!modalPreview) {
            console.error('Modal preview element not found');
            return;
        }
        
        console.log('Updating modal preview, blocks:', this.blocks);
        
        let previewHTML = '<div class="reminder-card"><div class="reminder-icon"><i class="fas fa-bell"></i></div><div class="reminder-text">';
        
        if (this.blocks.length === 0) {
            previewHTML += '<p>محتوای یادآوری شما اینجا نمایش داده خواهد شد...</p>';
        } else {
            this.blocks.forEach(block => {
                previewHTML += this.generateBlockPreview(block);
            });
        }
        
        previewHTML += '</div></div>';
        modalPreview.innerHTML = previewHTML;
        console.log('Modal preview HTML updated:', previewHTML);
    }

    updatePreview() {
        console.log('updatePreview called');
        console.log('Blocks to preview:', this.blocks);
        
        // Update sidebar preview
        if (this.preview) {
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
            console.log('Sidebar preview HTML updated:', previewHTML);
        }
        
        // Update modal preview
        this.updateModalPreview();
    }
    
    generateBlockPreview(block) {
        let html = '';
        
        switch (block.type) {
            case 'text':
                html += `<div class="text-block">${block.data.content || ''}</div>`;
                break;
            case 'image':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const imageUrl = block.data.fileUrl || block.data.previewUrl;
                    const sizeClass = this.getSizeClass(block.data.size);
                    const positionClass = this.getPositionClass(block.data.position);
                    html += `<div class="image-block ${positionClass}">`;
                    if (block.data.caption && block.data.captionPosition === 'top') {
                        html += `<div class="caption caption-top">${block.data.caption}</div>`;
                    }
                    html += `<img src="${imageUrl}" alt="تصویر" class="${sizeClass}" />`;
                    if (block.data.caption && block.data.captionPosition === 'bottom') {
                        html += `<div class="caption caption-bottom">${block.data.caption}</div>`;
                    }
                    html += '</div>';
                }
                break;
            case 'video':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const videoUrl = block.data.fileUrl || block.data.previewUrl;
                    const sizeClass = this.getSizeClass(block.data.size);
                    const positionClass = this.getPositionClass(block.data.position);
                    html += `<div class="video-block ${positionClass}">`;
                    if (block.data.caption && block.data.captionPosition === 'top') {
                        html += `<div class="caption caption-top">${block.data.caption}</div>`;
                    }
                    html += `<video controls preload="none" class="${sizeClass}"><source data-src="${videoUrl}" type="video/mp4"></video>`;
                    if (block.data.caption && block.data.captionPosition === 'bottom') {
                        html += `<div class="caption caption-bottom">${block.data.caption}</div>`;
                    }
                    html += '</div>';
                }
                break;
            case 'audio':
                if (block.data.fileUrl || block.data.previewUrl) {
                    const audioUrl = block.data.fileUrl || block.data.previewUrl;
                    const mimeType = block.data.mimeType || 'audio/mpeg';
                    html += `<div class="audio-block">`;
                    if (block.data.caption) {
                        html += `<div class="caption">${block.data.caption}</div>`;
                    }
                    html += `<audio controls preload="none"><source data-src="${audioUrl}" type="${mimeType}"></audio>`;
                    html += '</div>';
                }
                break;
            case 'code':
                if (block.data.content) {
                    html += `<div class="code-block-preview">`;
                    if (block.data.title) {
                        html += `<div class="code-title">${block.data.title}</div>`;
                    }
                    html += `<pre><code class="language-${block.data.language || 'plaintext'}">${block.data.content}</code></pre>`;
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
        // Clean blocks data before saving - remove temporary data
        const cleanBlocks = this.blocks.map(block => {
            const cleanBlock = { ...block };
            const cleanData = { ...block.data };
            
            // Remove temporary data that shouldn't be saved
            delete cleanData.pendingFile;
            delete cleanData.previewUrl;
            delete cleanData.originalData;
            
            cleanBlock.data = cleanData;
            return cleanBlock;
        });
        
        const content = {
            type: 'reminder',
            blocks: cleanBlocks
        };
        
        const contentJson = JSON.stringify(content);
        
        // Update the reminder-specific field
        if (this.hiddenField) {
            this.hiddenField.value = contentJson;
        }
        
        // Also update the main contentJson field for form submission
        const mainContentField = document.getElementById('contentJson');
        if (mainContentField) {
            mainContentField.value = contentJson;
        }
        
        // Update preview (only if not loading existing content)
        if (!this.isLoadingExistingContent) {
            this.updatePreview();
        } else {
            console.log('Skipping preview update during loading');
        }
    }
    
    loadExistingContent() {
        console.log('=== loadExistingContent called ===');
        console.log('Hidden field:', this.hiddenField);
        console.log('Hidden field value:', this.hiddenField?.value);
        
        if (!this.hiddenField) {
            console.error('Hidden field not found');
            return;
        }
        
        const existingContent = this.hiddenField.value;
        console.log('Loading existing content:', existingContent);
        console.log('Content length:', existingContent?.length);
        console.log('Content trimmed:', existingContent?.trim());
        
        if (existingContent && existingContent.trim()) {
            try {
                // Set flag to prevent preview updates during loading
                this.isLoadingExistingContent = true;
                
                const data = JSON.parse(existingContent);
                console.log('Parsed data:', data);
                console.log('Data type:', typeof data);
                console.log('Data blocks:', data.blocks);
                console.log('Blocks is array:', Array.isArray(data.blocks));
                
                if (data.blocks && Array.isArray(data.blocks)) {
                    console.log('Blocks count:', data.blocks.length);
                    
                    // Clear existing blocks from DOM
                    const existingBlocks = this.blocksList.querySelectorAll('.content-block');
                    console.log('Existing blocks in DOM:', existingBlocks.length);
                    existingBlocks.forEach(block => block.remove());
                    
                    this.blocks = data.blocks;
                    if (this.blocks.length > 0) {
                        this.nextBlockId = Math.max(...this.blocks.map(b => parseInt(b.id.split('-')[1]) || 0)) + 1;
                    } else {
                        this.nextBlockId = 1;
                    }
                    
                    // Ensure order values are correct (in case they were corrupted)
                    this.updateBlockOrders();
                    
                    console.log('Loading blocks:', this.blocks);
                    console.log('Next block ID:', this.nextBlockId);
                    
                    // Render existing blocks
                    this.blocks.forEach((block, index) => {
                        console.log(`Rendering block ${index + 1}:`, block);
                        this.renderBlock(block);
                    });
                    
                    this.updateEmptyState();
                    console.log('Existing content loaded successfully');
                } else {
                    console.log('No blocks found in data');
                    console.log('Data structure:', data);
                }
                
                // Clear flag after loading
                this.isLoadingExistingContent = false;
                
                // Update preview after loading
                setTimeout(() => {
                    this.updatePreview();
                }, 100);
                
            } catch (error) {
                console.error('Error loading existing content:', error);
                console.error('Content that failed to parse:', existingContent);
                this.isLoadingExistingContent = false;
            }
        } else {
            console.log('No existing content to load');
            console.log('Content is empty or null');
        }
        
        console.log('=== loadExistingContent finished ===');
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
