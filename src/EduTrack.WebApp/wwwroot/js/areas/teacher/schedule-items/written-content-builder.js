/**
 * Written Content Question Block Manager
 * Handles question block creation, editing, and management for written-type schedule items
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
function showWrittenQuestionBlockTypeModal() {
    console.log('showWrittenQuestionBlockTypeModal called');
    
    // Check if manager is available first
    if (window.writtenBlockManager && window.writtenBlockManager.showBlockTypeModal) {
        console.log('Using manager to show modal');
        return window.writtenBlockManager.showBlockTypeModal();
    }
    
    // Fallback: show simple selection
    console.log('Manager not available, using fallback');
    const types = [
        { type: 'text', name: 'سوال متنی' },
        { type: 'image', name: 'سوال تصویری' },
        { type: 'video', name: 'سوال ویدیویی' },
        { type: 'audio', name: 'سوال صوتی' },
        { type: 'code', name: 'سوال کدی' }
    ];
    
    const selection = prompt('انتخاب نوع سوال:\n1. سوال متنی\n2. سوال تصویری\n3. سوال ویدیویی\n4. سوال صوتی\n5. سوال کدی\n\nلطفاً شماره مورد نظر را وارد کنید:');
    
    if (selection && selection >= 1 && selection <= 5) {
        const selectedType = types[selection - 1].type;
        if (window.writtenBlockManager && window.writtenBlockManager.addBlock) {
            window.writtenBlockManager.addBlock(selectedType);
        } else {
            console.warn('Written Block Manager not available');
            alert('سیستم مدیریت بلاک‌ها هنوز آماده نیست. لطفاً صفحه را رفرش کنید.');
        }
    }
}

function showQuestionTypeModal() {
    console.log('showQuestionTypeModal called');
    
    // Check if modal element exists
    const modalElement = document.getElementById('questionTypeModal');
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
        
        const selection = prompt('انتخاب نوع سوال:\n1. متن\n2. تصویر\n3. ویدیو\n4. صوت\n5. کد\n\nلطفاً شماره مورد نظر را وارد کنید:');
        
        if (selection && selection >= 1 && selection <= 5) {
            const selectedType = types[selection - 1].type;
            if (window.writtenBlockManager) {
                window.writtenBlockManager.addQuestionBlock(selectedType);
            } else {
                console.warn('Manager still not available after selection');
            }
        }
        return;
    }
    
    // Clean up any existing backdrops first
    cleanupModalBackdrops();
    
    if (window.writtenBlockManager) {
        console.log('Using manager to show modal');
        window.writtenBlockManager.showQuestionTypeModal();
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
        
        const selection = prompt('انتخاب نوع سوال:\n1. متن\n2. تصویر\n3. ویدیو\n4. صوت\n5. کد\n\nلطفاً شماره مورد نظر را وارد کنید:');
        
        if (selection && selection >= 1 && selection <= 5) {
            const selectedType = types[selection - 1].type;
            if (window.writtenBlockManager) {
                window.writtenBlockManager.addQuestionBlock(selectedType);
            } else {
                console.warn('Manager still not available after selection');
            }
        }
    }
}

function updateWrittenPreview() {
    console.log('updateWrittenPreview called');
    
    if (window.writtenBlockManager && window.writtenBlockManager.updatePreview) {
        console.log('Using manager to update preview');
        window.writtenBlockManager.updatePreview();
        if (window.writtenBlockManager.showPreviewModal) {
            window.writtenBlockManager.showPreviewModal();
        }
    } else {
        console.log('Manager not available for preview');
        alert('سیستم پیش‌نمایش هنوز آماده نیست. لطفاً صفحه را رفرش کنید.');
    }
}

class WrittenContentBlockManager {
    constructor() {
        this.questionBlocks = [];
        this.currentBlockId = 0;
        this.isInitialized = false;
        
        // Initialize when DOM is ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initialize());
        } else {
            this.initialize();
        }
    }

    initialize() {
        console.log('Initializing WrittenContentQuestionManager...');
        
        try {
            this.setupEventListeners();
            this.loadExistingContent();
            this.isInitialized = true;
            console.log('WrittenContentQuestionManager initialized successfully');
        } catch (error) {
            console.error('Error initializing WrittenContentQuestionManager:', error);
        }
    }

    setupEventListeners() {
        // Question type modal event listeners
        const questionTypeItems = document.querySelectorAll('.question-type-item');
        questionTypeItems.forEach(item => {
            item.addEventListener('click', () => {
                const type = item.dataset.type;
                this.addQuestionBlock(type);
                this.hideQuestionTypeModal();
            });
        });

        // Modal close events
        const questionTypeModal = document.getElementById('questionTypeModal');
        if (questionTypeModal) {
            questionTypeModal.addEventListener('hidden.bs.modal', () => {
                cleanupModalBackdrops();
            });
        }

        const previewModal = document.getElementById('writtenPreviewModal');
        if (previewModal) {
            previewModal.addEventListener('hidden.bs.modal', () => {
                cleanupModalBackdrops();
            });
        }
    }

    showBlockTypeModal() {
        const modal = document.getElementById('writtenQuestionBlockTypeModal');
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } else {
            console.error('Written question block type modal not found');
        }
    }

    showQuestionTypeModal() {
        const modal = document.getElementById('questionTypeModal');
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } else {
            console.error('Question type modal not found');
        }
    }

    hideQuestionTypeModal() {
        const modal = document.getElementById('questionTypeModal');
        if (modal) {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
        }
    }

    addBlock(type) {
        this.addQuestionBlock(type);
    }

    addQuestionBlock(type) {
        console.log(`Adding question block of type: ${type}`);
        
        const blockId = ++this.currentBlockId;
        const block = this.createQuestionBlock(type, blockId);
        
        this.questionBlocks.push(block);
        this.renderQuestionBlocks();
        this.updateContentJson();
        
        console.log(`Question block added with ID: ${blockId}`);
    }

    createQuestionBlock(type, blockId) {
        const block = {
            id: blockId,
            type: type,
            order: this.questionBlocks.length + 1,
            questionText: '',
            hint: '',
            points: 1,
            isRequired: true,
            questionData: this.getDefaultQuestionData(type)
        };

        return block;
    }

    getDefaultQuestionData(type) {
        const baseData = {
            size: 'medium',
            position: 'center',
            caption: '',
            captionPosition: 'bottom'
        };

        switch (type) {
            case 'text':
                return {
                    ...baseData,
                    textContent: ''
                };
            case 'image':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null
                };
            case 'video':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null
                };
            case 'audio':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null,
                    isRecorded: false,
                    duration: null
                };
            case 'code':
                return {
                    ...baseData,
                    codeContent: '',
                    language: 'plaintext',
                    theme: 'default',
                    codeTitle: '',
                    showLineNumbers: true,
                    enableCopyButton: true
                };
            default:
                return baseData;
        }
    }

    renderQuestionBlocks() {
        const container = document.getElementById('questionBlocksList');
        if (!container) return;

        // Clear existing blocks
        container.innerHTML = '';

        if (this.questionBlocks.length === 0) {
            container.innerHTML = `
                <div class="empty-state" id="emptyQuestionBlocksState">
                    <div class="empty-state-icon">
                        <i class="fas fa-question-circle"></i>
                    </div>
                    <h4>هنوز سوالی اضافه نشده</h4>
                    <p>برای شروع، روی دکمه "افزودن سوال" کلیک کنید</p>
                </div>
            `;
            return;
        }

        // Render each question block
        this.questionBlocks.forEach((block, index) => {
            const blockElement = this.createQuestionBlockElement(block, index);
            container.appendChild(blockElement);
        });

        this.setupBlockEventListeners();
    }

    createQuestionBlockElement(block, index) {
        const template = document.querySelector(`#questionBlockTemplates .question-block-template[data-type="${block.type}"]`);
        if (!template) return document.createElement('div');

        const clone = template.cloneNode(true);
        clone.id = `question-block-${block.id}`;
        clone.dataset.blockId = block.id;
        clone.dataset.blockIndex = index;

        // Update block content based on block data
        this.populateBlockContent(clone, block);

        return clone;
    }

    populateBlockContent(element, block) {
        // Update question text
        const questionEditor = element.querySelector('.rich-text-editor');
        if (questionEditor) {
            questionEditor.innerHTML = block.questionText || '';
        }

        // Update hint
        const hintTextarea = element.querySelector('[data-hint="true"]');
        if (hintTextarea) {
            hintTextarea.value = block.hint || '';
        }

        // Update points
        const pointsInput = element.querySelector('[data-setting="points"]');
        if (pointsInput) {
            pointsInput.value = block.points || 1;
        }

        // Update required checkbox
        const requiredCheckbox = element.querySelector('[data-setting="isRequired"]');
        if (requiredCheckbox) {
            requiredCheckbox.checked = block.isRequired !== false;
        }

        // Update question data based on type
        this.populateQuestionData(element, block);
    }

    populateQuestionData(element, block) {
        const data = block.questionData;

        switch (block.type) {
            case 'text':
                // Text content is already handled in questionText
                break;
            case 'image':
            case 'video':
            case 'audio':
                // Update file information if exists
                if (data.fileUrl) {
                    const preview = element.querySelector(`.${block.type}-preview`);
                    if (preview) {
                        preview.style.display = 'block';
                        const mediaElement = preview.querySelector(`.preview-${block.type}`);
                        if (mediaElement) {
                            if (block.type === 'image') {
                                mediaElement.src = data.fileUrl;
                            } else {
                                mediaElement.src = data.fileUrl;
                            }
                        }
                    }
                }
                break;
            case 'code':
                // Update code content
                const codeTextarea = element.querySelector('[data-code-content]');
                if (codeTextarea) {
                    codeTextarea.value = data.codeContent || '';
                }

                // Update language
                const languageSelect = element.querySelector('[data-setting="language"]');
                if (languageSelect) {
                    languageSelect.value = data.language || 'plaintext';
                }

                // Update theme
                const themeSelect = element.querySelector('[data-setting="theme"]');
                if (themeSelect) {
                    themeSelect.value = data.theme || 'default';
                }

                // Update code title
                const titleInput = element.querySelector('[data-setting="title"]');
                if (titleInput) {
                    titleInput.value = data.codeTitle || '';
                }

                // Update checkboxes
                const lineNumbersCheckbox = element.querySelector('[data-setting="showLineNumbers"]');
                if (lineNumbersCheckbox) {
                    lineNumbersCheckbox.checked = data.showLineNumbers !== false;
                }

                const copyButtonCheckbox = element.querySelector('[data-setting="enableCopyButton"]');
                if (copyButtonCheckbox) {
                    copyButtonCheckbox.checked = data.enableCopyButton !== false;
                }
                break;
        }
    }

    setupBlockEventListeners() {
        // Block action buttons
        document.querySelectorAll('.question-block-template [data-action]').forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                const action = button.dataset.action;
                const blockElement = button.closest('.question-block-template');
                const blockId = parseInt(blockElement.dataset.blockId);
                
                this.handleBlockAction(action, blockId, blockElement);
            });
        });

        // Content change events
        document.querySelectorAll('.question-block-template .rich-text-editor').forEach(editor => {
            editor.addEventListener('input', (e) => {
                const blockElement = e.target.closest('.question-block-template');
                const blockId = parseInt(blockElement.dataset.blockId);
                this.updateBlockContent(blockId, 'questionText', e.target.innerHTML);
            });
        });

        document.querySelectorAll('.question-block-template [data-hint="true"]').forEach(textarea => {
            textarea.addEventListener('input', (e) => {
                const blockElement = e.target.closest('.question-block-template');
                const blockId = parseInt(blockElement.dataset.blockId);
                this.updateBlockContent(blockId, 'hint', e.target.value);
            });
        });

        document.querySelectorAll('.question-block-template [data-setting="points"]').forEach(input => {
            input.addEventListener('input', (e) => {
                const blockElement = e.target.closest('.question-block-template');
                const blockId = parseInt(blockElement.dataset.blockId);
                this.updateBlockContent(blockId, 'points', parseInt(e.target.value) || 1);
            });
        });

        document.querySelectorAll('.question-block-template [data-setting="isRequired"]').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const blockElement = e.target.closest('.question-block-template');
                const blockId = parseInt(blockElement.dataset.blockId);
                this.updateBlockContent(blockId, 'isRequired', e.target.checked);
            });
        });

        // File upload events
        document.querySelectorAll('.question-block-template [data-action="image-upload"], .question-block-template [data-action="video-upload"], .question-block-template [data-action="audio-upload"]').forEach(input => {
            input.addEventListener('change', (e) => {
                const blockElement = e.target.closest('.question-block-template');
                const blockId = parseInt(blockElement.dataset.blockId);
                this.handleFileUpload(e.target, blockId, blockElement);
            });
        });

        // Settings change events
        document.querySelectorAll('.question-block-template [data-setting]').forEach(element => {
            element.addEventListener('change', (e) => {
                const blockElement = e.target.closest('.question-block-template');
                const blockId = parseInt(blockElement.dataset.blockId);
                const setting = e.target.dataset.setting;
                const value = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
                
                this.updateQuestionData(blockId, setting, value);
            });
        });
    }

    handleBlockAction(action, blockId, blockElement) {
        const blockIndex = this.questionBlocks.findIndex(b => b.id === blockId);
        if (blockIndex === -1) return;

        switch (action) {
            case 'move-up':
                if (blockIndex > 0) {
                    this.swapBlocks(blockIndex, blockIndex - 1);
                }
                break;
            case 'move-down':
                if (blockIndex < this.questionBlocks.length - 1) {
                    this.swapBlocks(blockIndex, blockIndex + 1);
                }
                break;
            case 'insert-above':
                this.showQuestionTypeModal();
                break;
            case 'delete':
                if (confirm('آیا مطمئن هستید که می‌خواهید این سوال را حذف کنید؟')) {
                    this.removeQuestionBlock(blockId);
                }
                break;
            case 'fullscreen':
                this.toggleFullscreen(blockElement);
                break;
            case 'toggle-collapse':
                this.toggleCollapse(blockElement);
                break;
        }
    }

    swapBlocks(index1, index2) {
        const temp = this.questionBlocks[index1];
        this.questionBlocks[index1] = this.questionBlocks[index2];
        this.questionBlocks[index2] = temp;
        
        // Update order
        this.questionBlocks.forEach((block, index) => {
            block.order = index + 1;
        });
        
        this.renderQuestionBlocks();
        this.updateContentJson();
    }

    removeQuestionBlock(blockId) {
        this.questionBlocks = this.questionBlocks.filter(b => b.id !== blockId);
        
        // Update order
        this.questionBlocks.forEach((block, index) => {
            block.order = index + 1;
        });
        
        this.renderQuestionBlocks();
        this.updateContentJson();
    }

    updateBlockContent(blockId, property, value) {
        const block = this.questionBlocks.find(b => b.id === blockId);
        if (block) {
            block[property] = value;
            this.updateContentJson();
        }
    }

    updateQuestionData(blockId, property, value) {
        const block = this.questionBlocks.find(b => b.id === blockId);
        if (block && block.questionData) {
            block.questionData[property] = value;
            this.updateContentJson();
        }
    }

    handleFileUpload(input, blockId, blockElement) {
        const file = input.files[0];
        if (!file) return;

        // Show upload progress
        const progressContainer = blockElement.querySelector('.upload-progress');
        if (progressContainer) {
            progressContainer.style.display = 'block';
        }

        // Simulate file upload (in real implementation, this would be an actual upload)
        setTimeout(() => {
            const block = this.questionBlocks.find(b => b.id === blockId);
            if (block) {
                block.questionData.fileId = `file_${Date.now()}`;
                block.questionData.fileName = file.name;
                block.questionData.fileUrl = URL.createObjectURL(file);
                block.questionData.fileSize = file.size;
                block.questionData.mimeType = file.type;

                // Show preview
                const preview = blockElement.querySelector(`.${block.type}-preview`);
                if (preview) {
                    preview.style.display = 'block';
                    const mediaElement = preview.querySelector(`.preview-${block.type}`);
                    if (mediaElement) {
                        mediaElement.src = block.questionData.fileUrl;
                    }
                }

                // Hide upload area
                const uploadArea = blockElement.querySelector('.upload-placeholder');
                if (uploadArea) {
                    uploadArea.style.display = 'none';
                }
            }

            // Hide progress
            if (progressContainer) {
                progressContainer.style.display = 'none';
            }

            this.updateContentJson();
        }, 1000);
    }

    toggleFullscreen(element) {
        // Implementation for fullscreen mode
        console.log('Toggle fullscreen for block:', element.dataset.blockId);
    }

    toggleCollapse(element) {
        const content = element.querySelector('.block-content');
        const icon = element.querySelector('.collapse-icon');
        
        if (content.style.display === 'none') {
            content.style.display = 'block';
            icon.classList.remove('fa-chevron-up');
            icon.classList.add('fa-chevron-down');
        } else {
            content.style.display = 'none';
            icon.classList.remove('fa-chevron-down');
            icon.classList.add('fa-chevron-up');
        }
    }

    updateContentJson() {
        const content = {
            title: '',
            description: '',
            questionBlocks: this.questionBlocks.map(block => ({
                id: block.id.toString(),
                order: block.order,
                questionText: block.questionText,
                questionType: block.type,
                questionData: block.questionData,
                points: block.points,
                isRequired: block.isRequired,
                hint: block.hint
            })),
            timeLimitMinutes: 0,
            allowLateSubmission: true,
            instructions: ''
        };

        const hiddenInput = document.getElementById('writtenContentJson');
        if (hiddenInput) {
            hiddenInput.value = JSON.stringify(content);
        }

        console.log('Written content JSON updated:', content);
    }

    loadExistingContent() {
        const hiddenInput = document.getElementById('writtenContentJson');
        if (hiddenInput && hiddenInput.value) {
            try {
                const content = JSON.parse(hiddenInput.value);
                if (content.questionBlocks && Array.isArray(content.questionBlocks)) {
                    this.questionBlocks = content.questionBlocks.map(block => ({
                        id: parseInt(block.id) || ++this.currentBlockId,
                        type: block.questionType || 'text',
                        order: block.order || 1,
                        questionText: block.questionText || '',
                        hint: block.hint || '',
                        points: block.points || 1,
                        isRequired: block.isRequired !== false,
                        questionData: block.questionData || this.getDefaultQuestionData(block.questionType || 'text')
                    }));
                    
                    this.currentBlockId = Math.max(...this.questionBlocks.map(b => b.id), 0);
                    this.renderQuestionBlocks();
                }
            } catch (error) {
                console.error('Error loading existing written content:', error);
            }
        }
    }

    updatePreview() {
        const previewContainer = document.getElementById('writtenPreview');
        if (!previewContainer) return;

        if (this.questionBlocks.length === 0) {
            previewContainer.innerHTML = `
                <div class="written-exercise-card">
                    <div class="exercise-icon">
                        <i class="fas fa-edit"></i>
                    </div>
                    <div class="exercise-text">
                        <p>هنوز سوالی اضافه نشده است</p>
                    </div>
                </div>
            `;
            return;
        }

        let previewHtml = '<div class="written-exercise-preview">';
        
        this.questionBlocks.forEach((block, index) => {
            previewHtml += `
                <div class="preview-question-block">
                    <div class="question-header">
                        <span class="question-number">سوال ${index + 1}</span>
                        <span class="question-points">(${block.points} امتیاز)</span>
                        ${block.isRequired ? '<span class="required-badge">اجباری</span>' : ''}
                    </div>
                    <div class="question-content">
                        ${this.renderQuestionPreview(block)}
                    </div>
                    ${block.hint ? `<div class="question-hint">راهنمایی: ${block.hint}</div>` : ''}
                </div>
            `;
        });
        
        previewHtml += '</div>';
        previewContainer.innerHTML = previewHtml;
    }

    renderQuestionPreview(block) {
        let content = `<div class="question-text">${block.questionText || 'متن سوال'}</div>`;
        
        switch (block.type) {
            case 'image':
                if (block.questionData.fileUrl) {
                    content += `<div class="question-image"><img src="${block.questionData.fileUrl}" alt="تصویر سوال" style="max-width: 100%; height: auto;"></div>`;
                }
                break;
            case 'video':
                if (block.questionData.fileUrl) {
                    content += `<div class="question-video"><video controls style="max-width: 100%;"><source src="${block.questionData.fileUrl}"></video></div>`;
                }
                break;
            case 'audio':
                if (block.questionData.fileUrl) {
                    content += `<div class="question-audio"><audio controls><source src="${block.questionData.fileUrl}"></audio></div>`;
                }
                break;
            case 'code':
                if (block.questionData.codeContent) {
                    content += `<div class="question-code"><pre><code>${block.questionData.codeContent}</code></pre></div>`;
                }
                break;
        }
        
        content += '<div class="answer-area"><textarea placeholder="پاسخ خود را اینجا بنویسید..." rows="4" style="width: 100%;"></textarea></div>';
        
        return content;
    }

    showPreviewModal() {
        const modal = document.getElementById('writtenPreviewModal');
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } else {
            console.error('Written preview modal not found');
        }
    }

    getContent() {
        return {
            title: '',
            description: '',
            questionBlocks: this.questionBlocks,
            timeLimitMinutes: 0,
            allowLateSubmission: true,
            instructions: ''
        };
    }
}

// Initialize the manager when DOM is ready
function initializeWrittenQuestionManager() {
    console.log('Initializing Written Question Manager...');
    
    if (window.writtenBlockManager) {
        console.log('Written Block Manager already exists');
        return;
    }
    
    try {
        window.writtenBlockManager = new WrittenContentBlockManager();
        console.log('Written Block Manager initialized successfully');
    } catch (error) {
        console.error('Error initializing Written Block Manager:', error);
    }
}

// Auto-initialize when script loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeWrittenQuestionManager);
} else {
    initializeWrittenQuestionManager();
}
