/**
 * Written Content Question Block Manager
 * Handles question block creation, editing, and management for written-type schedule items
 * Uses specific block managers (text-block.js, image-block.js, etc.) for individual block functionality
 */

// Global functions for onclick handlers
function showWrittenQuestionBlockTypeModal() {
    
    if (window.sharedContentBlockManager) {
        window.sharedContentBlockManager.showBlockTypeModal('questionTypeModal');
    } else {
        console.warn('Shared Content Block Manager not available');
        alert('سیستم مدیریت بلاک‌ها هنوز آماده نیست. لطفاً صفحه را رفرش کنید.');
    }
}

function showQuestionTypeModal() {
    
    if (window.sharedContentBlockManager) {
        window.sharedContentBlockManager.showBlockTypeModal('questionTypeModal');
    } else {
        console.warn('Shared Content Block Manager not available');
        alert('سیستم مدیریت بلاک‌ها هنوز آماده نیست. لطفاً صفحه را رفرش کنید.');
    }
}

function updateWrittenPreview() {
    
    if (window.writtenBlockManager && window.writtenBlockManager.updatePreview) {
        window.writtenBlockManager.updatePreview();
        if (window.writtenBlockManager.showPreviewModal) {
            window.writtenBlockManager.showPreviewModal();
        }
    } else {
        alert('سیستم پیش‌نمایش هنوز آماده نیست. لطفاً صفحه را رفرش کنید.');
    }
}

class WrittenContentBlockManager extends ContentBuilderBase {
    constructor() {
        super({
            containerId: 'questionBlocksList',
            emptyStateId: 'emptyQuestionBlocksState',
            previewId: 'writtenPreview',
            hiddenFieldId: 'writtenContentJson',
            modalId: 'questionTypeModal',
            contentType: 'written'
        });

        this.questionBlocks = this.blocks; // Alias for backward compatibility
        this.currentBlockId = this.nextBlockId - 1; // Alias for backward compatibility
        this.isInitialized = false;
        
        // Initialize when DOM is ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initialize());
        } else {
            this.initialize();
        }
    }

    initialize() {
        
        try {
            this.setupWrittenSpecificEventListeners();
            this.isInitialized = true;
        } catch (error) {
            console.error('Error initializing WrittenContentQuestionManager:', error);
        }
    }

    setupWrittenSpecificEventListeners() {
        // Listen for block type selection from shared manager
        document.addEventListener('blockTypeSelected', (e) => {
            if (e.detail.type) {
                this.addQuestionBlock(e.detail.type);
            }
        });

        // Listen for insert block above events
        document.addEventListener('insertBlockAbove', (e) => {
            this.showBlockTypeModal();
        });

        // Modal close events
        const previewModal = document.getElementById('writtenPreviewModal');
        if (previewModal) {
            previewModal.addEventListener('hidden.bs.modal', () => {
                this.cleanupModalBackdrops();
            });
        }

        // Question-specific settings
        document.addEventListener('change', (e) => {
            if (e.target.matches('[data-setting="points"]')) {
                this.updateQuestionPoints(e.target);
            } else if (e.target.matches('[data-setting="isRequired"]')) {
                this.updateQuestionRequired(e.target);
            }
        });

        // Question hint changes
        document.addEventListener('input', (e) => {
            if (e.target.matches('[data-hint="true"]')) {
                this.updateQuestionHint(e.target);
            }
        });
    }

    addQuestionBlock(type) {
        
        const blockId = ++this.currentBlockId;
        const block = this.createQuestionBlock(type, blockId);
        
        this.questionBlocks.push(block);
        this.renderQuestionBlocks();
        this.updateContentJson();
        
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

        // Render each question block using shared templates
        this.questionBlocks.forEach((block, index) => {
            const blockElement = this.createQuestionBlockElement(block, index);
            container.appendChild(blockElement);
        });
    }

    createQuestionBlockElement(block, index) {
        const template = document.querySelector(`#questionBlockTemplates .content-block-template[data-type="${block.type}"]`);
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
                            mediaElement.src = data.fileUrl;
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

    updateQuestionPoints(input) {
        const blockElement = input.closest('.content-block-template');
        const blockId = blockElement.dataset.blockId;
        const block = this.questionBlocks.find(b => b.id === blockId);
        
        if (block) {
            block.points = parseInt(input.value) || 1;
            this.updateContentJson();
        }
    }

    updateQuestionRequired(checkbox) {
        const blockElement = checkbox.closest('.content-block-template');
        const blockId = blockElement.dataset.blockId;
        const block = this.questionBlocks.find(b => b.id === blockId);
        
        if (block) {
            block.isRequired = checkbox.checked;
            this.updateContentJson();
        }
    }

    updateQuestionHint(textarea) {
        const blockElement = textarea.closest('.content-block-template');
        const blockId = blockElement.dataset.blockId;
        const block = this.questionBlocks.find(b => b.id === blockId);
        
        if (block) {
            block.hint = textarea.value;
            this.updateContentJson();
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
                    
                    // Populate content fields after rendering
                    setTimeout(() => {
                        this.populateAllQuestionBlocks();
                    }, 200);
                }
            } catch (error) {
                console.error('Error loading existing written content:', error);
            }
        }
    }

    populateAllQuestionBlocks() {
        this.questionBlocks.forEach(block => {
            const blockElement = document.querySelector(`[data-question-id="${block.id}"]`);
            if (blockElement) {
                this.populateBlockContent(blockElement, block);
            }
        });
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

    cleanupModalBackdrops() {
        const backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach(backdrop => {
            backdrop.remove();
        });
        
        // Reset body styles
        document.body.classList.remove('modal-open');
        document.body.style.paddingRight = '';
        document.body.style.overflow = '';
        document.body.style.overflowX = '';
        document.body.style.overflowY = '';
        document.documentElement.style.overflow = '';
        document.documentElement.style.overflowX = '';
        document.documentElement.style.overflowY = '';
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
    if (window.writtenBlockManager) {
        return;
    }
    
    try {
        window.writtenBlockManager = new WrittenContentBlockManager();
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
