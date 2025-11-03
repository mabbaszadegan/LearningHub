/**
 * MCQ Manager
 * Handles multiple choice questions with text, image, and audio options
 */

class McqManager {
    constructor() {
        this.mediaRecorder = null;
        this.audioChunks = [];
        this.isRecording = false;
        this.recordingStartTime = null;
        this.recordingTimer = null;
        this.currentRecordingOption = null;
        this.uploadUrl = '/FileUpload/UploadContentFile';
        
        this.init();
    }

    init() {
        // Delegate events using event delegation
        document.addEventListener('click', (e) => this.handleClick(e));
        document.addEventListener('change', (e) => this.handleChange(e));
        document.addEventListener('input', (e) => this.handleInput(e));
    }

    handleInput(e) {
        const target = e.target;
        
        // Handle stem text input changes - check for textarea with the role attribute
        if (target.matches('textarea[data-role="mcq-stem-text"]')) {
            this.onStemTextChange(target.closest('.mcq-question-item'));
        }
        
        // Handle option text input changes
        if (target.matches('input[data-role="mcq-option-text"], .mcq-option-text-input')) {
            this.onOptionTextChange(target.closest('.mcq-option-item'));
        }
    }

    onStemTextChange(questionItem) {
        if (!questionItem) return;
        
        // Trigger content update for the parent block
        const blockElement = questionItem.closest('.content-block[data-type="multipleChoice"]');
        this.triggerContentUpdate(blockElement);
    }

    onOptionTextChange(optionItem) {
        if (!optionItem) return;
        
        // Trigger content update for the parent block
        const blockElement = optionItem.closest('.content-block[data-type="multipleChoice"]');
        this.triggerContentUpdate(blockElement);
    }

    triggerContentUpdate(blockElement) {
        if (!blockElement) return;
        
        // Find the content manager and trigger update
        const blockId = blockElement.dataset.blockId;
        if (blockId) {
            // Trigger block content changed event
            const event = new CustomEvent('blockContentChanged', {
                detail: {
                    blockElement: blockElement,
                    blockId: blockId
                }
            });
            document.dispatchEvent(event);
            
            // Also try to update via content builder if available
            if (window.step4ContentManager && typeof window.step4ContentManager.updateHiddenField === 'function') {
                // Get the handler and collect fresh data
                if (window.sharedContentBlockManager) {
                    const handler = window.sharedContentBlockManager.getBlockManager('multipleChoice');
                    if (handler && typeof handler.collectData === 'function') {
                        const block = window.step4ContentManager.blocks.find(b => b.id === blockId);
                        if (block) {
                            const newData = handler.collectData(blockElement, block);
                            if (newData) {
                                block.data = { ...block.data, ...newData };
                                blockElement.dataset.blockData = JSON.stringify(block.data);
                            }
                        }
                    }
                }
                window.step4ContentManager.updateHiddenField();
            }
        }
    }

    handleClick(e) {
        const target = e.target.closest('[data-action]');
        if (!target) return;

        const action = target.getAttribute('data-action');
        const optionItem = target.closest('.mcq-option-item');
        const questionItem = target.closest('.mcq-question-item');
        const blockElement = target.closest('.content-block[data-type="multipleChoice"]');

        switch (action) {
            case 'mcq-add-question':
                e.preventDefault();
                this.addQuestion(blockElement);
                break;
            case 'mcq-remove':
                e.preventDefault();
                this.removeQuestion(questionItem);
                break;
            case 'mcq-add-option':
                e.preventDefault();
                this.addOption(questionItem);
                break;
            case 'remove-option':
                e.preventDefault();
                this.removeOption(optionItem);
                break;
            case 'upload-image':
                e.preventDefault();
                this.triggerImageUpload(optionItem);
                break;
            case 'upload-audio':
                e.preventDefault();
                this.triggerAudioUpload(optionItem);
                break;
            case 'record-audio':
                e.preventDefault();
                this.startRecording(optionItem);
                break;
            case 'stop-recording':
                e.preventDefault();
                this.stopRecording();
                break;
            case 'remove-audio':
                e.preventDefault();
                this.removeAudio(optionItem);
                break;
            case 'upload-stem-image':
                e.preventDefault();
                this.triggerStemImageUpload(questionItem);
                break;
            case 'upload-stem-audio':
                e.preventDefault();
                this.triggerStemAudioUpload(questionItem);
                break;
            case 'record-stem-audio':
                e.preventDefault();
                this.startStemRecording(questionItem);
                break;
            case 'stop-stem-recording':
                e.preventDefault();
                this.stopStemRecording();
                break;
            case 'remove-stem-audio':
                e.preventDefault();
                this.removeStemAudio(questionItem);
                break;
            case 'remove-stem-media':
                e.preventDefault();
                const mediaType = target.getAttribute('data-media-type');
                this.removeStemMedia(questionItem, mediaType);
                break;
            case 'upload-stem-video':
                e.preventDefault();
                this.triggerStemVideoUpload(questionItem);
                break;
        }
    }

    handleChange(e) {
        const target = e.target;
        
        if (target.matches('.mcq-option-type-select')) {
            this.changeOptionType(target.closest('.mcq-option-item'), target.value);
        } else if (target.matches('.mcq-option-image-file')) {
            this.handleImageFileSelect(target);
        } else if (target.matches('.mcq-option-audio-file')) {
            this.handleAudioFileSelect(target);
        } else if (target.matches('.mcq-answer-type-select')) {
            this.updateAnswerType(target.closest('.mcq-question-item'), target.value);
            this.triggerContentUpdate(target.closest('.content-block[data-type="multipleChoice"]'));
        } else if (target.matches('.mcq-stem-media-type-select')) {
            this.changeStemMediaType(target.closest('.mcq-question-item'), target.value);
            this.triggerContentUpdate(target.closest('.content-block[data-type="multipleChoice"]'));
        } else if (target.matches('.mcq-option-correct')) {
            this.triggerContentUpdate(target.closest('.content-block[data-type="multipleChoice"]'));
        } else if (target.matches('.mcq-randomize-checkbox')) {
            this.triggerContentUpdate(target.closest('.content-block[data-type="multipleChoice"]'));
        } else if (target.matches('.mcq-stem-image-file')) {
            this.handleStemImageFileSelect(target);
        } else if (target.matches('.mcq-stem-audio-file')) {
            this.handleStemAudioFileSelect(target);
        } else if (target.matches('.mcq-stem-video-file')) {
            this.handleStemVideoFileSelect(target);
        }
    }

    async addQuestion(blockElement, skipDefaultOptions = false) {
        const container = blockElement?.querySelector('[data-role="mcq-container"]');
        if (!container) return Promise.resolve();

        const questionsList = container.querySelector('[data-role="mcq-list"]');
        if (!questionsList) return Promise.resolve();

        // Remove empty state if exists
        const emptyState = questionsList.querySelector('.mcq-empty-state');
        if (emptyState) {
            emptyState.remove();
        }

        const questionId = `mcq-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const questionCount = questionsList.querySelectorAll('.mcq-question-item').length;

        try {
            const response = await fetch('/Teacher/ScheduleItem/GetMcqQuestionTemplate', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            });

            if (!response.ok) {
                throw new Error('Failed to load question template');
            }

            const html = await response.text();
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;

            let questionElement = tempDiv.querySelector('.mcq-question-item');
            if (!questionElement) {
                // Fallback: create question manually
                questionElement = this.createQuestionElement(questionId, questionCount);
            } else {
                questionElement = questionElement.cloneNode(true);
                questionElement.dataset.questionId = questionId;
                
                // Update question number
                const questionNumber = questionElement.querySelector('.mcq-question-number');
                if (questionNumber) {
                    questionNumber.textContent = `سوال ${questionCount + 1}`;
                }
            }

            questionsList.appendChild(questionElement);
            if (!skipDefaultOptions) {
                this.initializeQuestion(questionElement);
            }
            this.updateEmptyState(container);
        } catch (error) {
            console.error('Error adding question:', error);
            // Fallback: create question manually
            const questionElement = this.createQuestionElement(questionId, questionCount);
            questionsList.appendChild(questionElement);
            if (!skipDefaultOptions) {
                this.initializeQuestion(questionElement);
            }
            this.updateEmptyState(container);
        }
        
        return Promise.resolve();
    }

    createQuestionElement(questionId, index) {
        const div = document.createElement('div');
        div.className = 'mcq-question-item';
        div.dataset.questionId = questionId;
        div.innerHTML = `
            <div class="mcq-question-header">
                <div class="mcq-question-number">سوال ${index + 1}</div>
                <button type="button" class="btn-mcq-remove" data-action="mcq-remove">
                    <i class="fas fa-times"></i>
                </button>
            </div>
            <div class="mcq-question-body">
                <div class="mcq-stem-field">
                    <div class="mcq-stem-header">
                        <label>صورت سوال</label>
                        <div class="mcq-stem-type-selector">
                            <select class="mcq-stem-type-select" data-role="mcq-stem-type">
                                <option value="text">متن</option>
                                <option value="image">تصویر</option>
                                <option value="audio">صوت</option>
                                <option value="video">ویدیو</option>
                            </select>
                        </div>
                    </div>
                    
                    <!-- Text Stem - Always Visible -->
                    <div class="mcq-stem-content mcq-stem-text-content" data-content-type="text">
                        <textarea class="mcq-stem-input" rows="2" placeholder="صورت سوال را وارد کنید..." data-role="mcq-stem-text"></textarea>
                    </div>
                    
                    <!-- Media Section - Optional -->
                    <div class="mcq-stem-media-section">
                        <label class="mcq-stem-media-label">افزودن رسانه (اختیاری)</label>
                        <div class="mcq-stem-media-selector">
                            <select class="mcq-stem-media-type-select" data-role="mcq-stem-media-type">
                                <option value="">بدون رسانه</option>
                                <option value="image">تصویر</option>
                                <option value="audio">صوت</option>
                                <option value="video">ویدیو</option>
                            </select>
                        </div>
                        
                        <!-- Image Media -->
                        <div class="mcq-stem-media-content mcq-stem-image-content" data-media-type="image" style="display: none;">
                            <div class="mcq-stem-image-upload">
                                <input type="file" class="mcq-stem-image-file" accept="image/*" data-role="mcq-stem-image-file" style="display: none;">
                                <div class="mcq-stem-image-preview" data-role="mcq-stem-image-preview">
                                    <div class="mcq-stem-image-placeholder">
                                        <i class="fas fa-image"></i>
                                        <span>برای آپلود تصویر کلیک کنید</span>
                                    </div>
                                </div>
                                <button type="button" class="btn-mcq-upload-stem-image" data-action="upload-stem-image">
                                    <i class="fas fa-upload"></i>
                                    آپلود تصویر
                                </button>
                                <button type="button" class="btn-mcq-remove-stem-media" data-action="remove-stem-media" data-media-type="image" style="display: none;">
                                    <i class="fas fa-times"></i>
                                    حذف تصویر
                                </button>
                                <input type="hidden" class="mcq-stem-image-url" data-role="mcq-stem-image-url">
                                <input type="hidden" class="mcq-stem-image-fileid" data-role="mcq-stem-image-fileid">
                            </div>
                        </div>
                        
                        <!-- Audio Media -->
                        <div class="mcq-stem-media-content mcq-stem-audio-content" data-media-type="audio" style="display: none;">
                        <div class="mcq-stem-audio-upload">
                            <div class="mcq-stem-audio-controls">
                                <button type="button" class="btn-mcq-upload-stem-audio" data-action="upload-stem-audio">
                                    <i class="fas fa-upload"></i>
                                    آپلود
                                </button>
                                <input type="file" class="mcq-stem-audio-file" accept="audio/*" data-role="mcq-stem-audio-file" style="display: none;">
                                <button type="button" class="btn-mcq-record-stem-audio" data-action="record-stem-audio">
                                    <i class="fas fa-microphone"></i>
                                    ضبط
                                </button>
                                <button type="button" class="btn-mcq-stop-stem-recording" data-action="stop-stem-recording" disabled>
                                    <i class="fas fa-stop"></i>
                                </button>
                            </div>
                            <div class="mcq-stem-audio-preview" data-role="mcq-stem-audio-preview" style="display: none;">
                                <audio controls class="mcq-stem-audio-player"></audio>
                                <div class="mcq-stem-audio-info">
                                    <span class="mcq-stem-audio-name"></span>
                                    <button type="button" class="btn-mcq-remove-stem-media" data-action="remove-stem-media" data-media-type="audio">
                                        <i class="fas fa-times"></i>
                                        حذف
                                    </button>
                                </div>
                            </div>
                            <input type="hidden" class="mcq-stem-audio-url" data-role="mcq-stem-audio-url">
                            <input type="hidden" class="mcq-stem-audio-fileid" data-role="mcq-stem-audio-fileid">
                            <input type="hidden" class="mcq-stem-audio-recorded" data-role="mcq-stem-audio-recorded" value="false">
                        </div>
                    </div>
                    
                    <!-- Video Media -->
                    <div class="mcq-stem-media-content mcq-stem-video-content" data-media-type="video" style="display: none;">
                        <div class="mcq-stem-video-upload">
                            <input type="file" class="mcq-stem-video-file" accept="video/*" data-role="mcq-stem-video-file" style="display: none;">
                            <div class="mcq-stem-video-preview" data-role="mcq-stem-video-preview">
                                <div class="mcq-stem-video-placeholder">
                                    <i class="fas fa-video"></i>
                                    <span>برای آپلود ویدیو کلیک کنید</span>
                                </div>
                            </div>
                            <button type="button" class="btn-mcq-upload-stem-video" data-action="upload-stem-video">
                                <i class="fas fa-upload"></i>
                                آپلود ویدیو
                            </button>
                            <button type="button" class="btn-mcq-remove-stem-media" data-action="remove-stem-media" data-media-type="video" style="display: none;">
                                <i class="fas fa-times"></i>
                                حذف ویدیو
                            </button>
                            <input type="hidden" class="mcq-stem-video-url" data-role="mcq-stem-video-url">
                            <input type="hidden" class="mcq-stem-video-fileid" data-role="mcq-stem-video-fileid">
                        </div>
                    </div>
                </div>
                </div>
                <div class="mcq-settings-row">
                    <div class="mcq-setting">
                        <label>نوع پاسخ</label>
                        <select class="mcq-answer-type-select" data-role="mcq-answer-type">
                            <option value="single">تک‌گزینه‌ای</option>
                            <option value="multiple">چندپاسخه</option>
                        </select>
                    </div>
                    <div class="mcq-setting">
                        <label class="mcq-checkbox-label">
                            <input type="checkbox" class="mcq-randomize-checkbox" data-role="mcq-randomize">
                            <span>به‌هم‌ریختن گزینه‌ها</span>
                        </label>
                    </div>
                </div>
                <div class="mcq-options-section">
                    <div class="mcq-options-header">
                        <span>گزینه‌ها</span>
                        <button type="button" class="btn-mcq-add-option" data-action="mcq-add-option">
                            <i class="fas fa-plus"></i>
                            افزودن گزینه
                        </button>
                    </div>
                    <div class="mcq-options-list" data-role="mcq-options"></div>
                </div>
            </div>
        `;
        return div;
    }

    initializeQuestion(questionElement) {
        const optionsList = questionElement.querySelector('[data-role="mcq-options"]');
        if (optionsList && optionsList.children.length === 0) {
            // Add at least 2 default options
            this.addOption(questionElement);
            this.addOption(questionElement);
        }
    }

    removeQuestion(questionItem) {
        if (!questionItem) return;
        
        if (confirm('آیا مطمئن هستید که می‌خواهید این سوال را حذف کنید؟')) {
            const container = questionItem.closest('[data-role="mcq-container"]');
            questionItem.remove();
            this.updateQuestionNumbers(container);
            this.updateEmptyState(container);
        }
    }

    updateQuestionNumbers(container) {
        const questions = container?.querySelectorAll('.mcq-question-item') || [];
        questions.forEach((q, index) => {
            const numberElement = q.querySelector('.mcq-question-number');
            if (numberElement) {
                numberElement.textContent = `سوال ${index + 1}`;
            }
        });
    }

    updateEmptyState(container) {
        const questionsList = container?.querySelector('[data-role="mcq-list"]');
        if (!questionsList) return;

        const hasQuestions = questionsList.querySelectorAll('.mcq-question-item').length > 0;

        if (!hasQuestions && !questionsList.querySelector('.mcq-empty-state')) {
            const emptyState = document.createElement('div');
            emptyState.className = 'mcq-empty-state';
            emptyState.innerHTML = `
                <i class="fas fa-inbox"></i>
                <p>هنوز سوال چندگزینه‌ای تعریف نشده است</p>
            `;
            questionsList.appendChild(emptyState);
        }
    }

    async addOption(questionItem) {
        if (!questionItem) return Promise.resolve();

        const optionsList = questionItem.querySelector('[data-role="mcq-options"]');
        if (!optionsList) return Promise.resolve();

        const questionId = questionItem.dataset.questionId || `q-${Date.now()}`;
        const optionCount = optionsList.querySelectorAll('.mcq-option-item').length;
        const isSingle = questionItem.querySelector('.mcq-answer-type-select')?.value === 'single';
        const inputName = `mcq-${questionId}`;
        const inputType = isSingle ? 'radio' : 'checkbox';

        try {
            const response = await fetch('/Teacher/ScheduleItem/GetMcqOptionTemplate', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            });

            if (!response.ok) {
                throw new Error('Failed to load option template');
            }

            const html = await response.text();
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;

            let optionElement = tempDiv.querySelector('.mcq-option-item');
            if (!optionElement) {
                optionElement = this.createOptionElement(optionCount, questionId, inputName, inputType);
            } else {
                optionElement = optionElement.cloneNode(true);
                optionElement.dataset.optionIndex = optionCount;
                
                // Update input name and type
                const correctInput = optionElement.querySelector('.mcq-option-correct');
                if (correctInput) {
                    correctInput.name = inputName;
                    correctInput.type = inputType;
                }

                const numberSpan = optionElement.querySelector('.mcq-option-number');
                if (numberSpan) {
                    numberSpan.textContent = optionCount + 1;
                }
            }

            optionsList.appendChild(optionElement);
        } catch (error) {
            console.error('Error adding option:', error);
            // Fallback: create option manually
            const optionElement = this.createOptionElement(optionCount, questionId, inputName, inputType);
            optionsList.appendChild(optionElement);
        }
        
        return Promise.resolve();
    }

    createOptionElement(index, questionId, inputName, inputType) {
        const div = document.createElement('div');
        div.className = 'mcq-option-item';
        div.dataset.optionIndex = index;
        div.dataset.optionType = 'text';
        
        // Import the option template HTML structure (simplified version)
        div.innerHTML = `
            <div class="mcq-option-header">
                <label class="mcq-option-label">
                    <input type="${inputType}" name="${inputName}" class="mcq-option-correct" data-role="mcq-option-correct">
                    <span class="mcq-option-number">${index + 1}</span>
                </label>
                <div class="mcq-option-type-selector">
                    <select class="mcq-option-type-select" data-role="mcq-option-type">
                        <option value="text">متن</option>
                        <option value="image">تصویر</option>
                        <option value="audio">صوت</option>
                    </select>
                </div>
                <button type="button" class="btn-mcq-remove-option" data-action="remove-option" title="حذف گزینه">
                    <i class="fas fa-times"></i>
                </button>
            </div>
            <div class="mcq-option-content mcq-option-text-content" data-content-type="text">
                <input type="text" class="mcq-option-text-input form-control" placeholder="متن گزینه را وارد کنید..." data-role="mcq-option-text">
            </div>
            <div class="mcq-option-content mcq-option-image-content" data-content-type="image" style="display: none;">
                <div class="mcq-option-image-upload">
                    <input type="file" class="mcq-option-image-file" accept="image/*" data-role="mcq-option-image-file" style="display: none;">
                    <div class="mcq-option-image-preview" data-role="mcq-option-image-preview">
                        <div class="mcq-option-image-placeholder">
                            <i class="fas fa-image"></i>
                            <span>برای آپلود تصویر کلیک کنید</span>
                        </div>
                    </div>
                    <button type="button" class="btn-mcq-upload-image" data-action="upload-image">
                        <i class="fas fa-upload"></i>
                        آپلود تصویر
                    </button>
                    <input type="hidden" class="mcq-option-image-url" data-role="mcq-option-image-url">
                    <input type="hidden" class="mcq-option-image-fileid" data-role="mcq-option-image-fileid">
                </div>
            </div>
            <div class="mcq-option-content mcq-option-audio-content" data-content-type="audio" style="display: none;">
                <div class="mcq-option-audio-upload">
                    <div class="mcq-option-audio-controls">
                        <button type="button" class="btn-mcq-upload-audio" data-action="upload-audio">
                            <i class="fas fa-upload"></i>
                            آپلود
                        </button>
                        <input type="file" class="mcq-option-audio-file" accept="audio/*" data-role="mcq-option-audio-file" style="display: none;">
                        <button type="button" class="btn-mcq-record-audio" data-action="record-audio">
                            <i class="fas fa-microphone"></i>
                            ضبط
                        </button>
                        <button type="button" class="btn-mcq-stop-recording" data-action="stop-recording" disabled>
                            <i class="fas fa-stop"></i>
                        </button>
                    </div>
                    <div class="mcq-option-audio-preview" data-role="mcq-option-audio-preview" style="display: none;">
                        <audio controls class="mcq-option-audio-player"></audio>
                        <div class="mcq-option-audio-info">
                            <span class="mcq-option-audio-name"></span>
                            <button type="button" class="btn-mcq-remove-audio" data-action="remove-audio">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                    </div>
                    <input type="hidden" class="mcq-option-audio-url" data-role="mcq-option-audio-url">
                    <input type="hidden" class="mcq-option-audio-fileid" data-role="mcq-option-audio-fileid">
                    <input type="hidden" class="mcq-option-audio-recorded" data-role="mcq-option-audio-recorded" value="false">
                </div>
            </div>
        `;
        
        return div;
    }

    removeOption(optionItem) {
        if (!optionItem) return;
        
        const optionsList = optionItem.parentElement;
        optionItem.remove();
        
        // Update option numbers
        if (optionsList) {
            const options = optionsList.querySelectorAll('.mcq-option-item');
            options.forEach((opt, index) => {
                opt.dataset.optionIndex = index;
                const numberSpan = opt.querySelector('.mcq-option-number');
                if (numberSpan) {
                    numberSpan.textContent = index + 1;
                }
            });
        }
    }

    changeOptionType(optionItem, type) {
        if (!optionItem) return;

        optionItem.dataset.optionType = type;

        // Hide all content types
        const contents = optionItem.querySelectorAll('.mcq-option-content');
        contents.forEach(content => {
            content.style.display = 'none';
        });

        // Show selected content type
        const selectedContent = optionItem.querySelector(`.mcq-option-${type}-content`);
        if (selectedContent) {
            selectedContent.style.display = 'block';
        }
    }

    updateAnswerType(questionItem, type) {
        if (!questionItem) return;

        const questionId = questionItem.dataset.questionId || `q-${Date.now()}`;
        const inputName = `mcq-${questionId}`;
        const inputType = type === 'single' ? 'radio' : 'checkbox';

        const correctInputs = questionItem.querySelectorAll('.mcq-option-correct');
        correctInputs.forEach(input => {
            input.name = inputName;
            input.type = inputType;
        });
    }

    triggerImageUpload(optionItem) {
        const fileInput = optionItem?.querySelector('.mcq-option-image-file');
        if (fileInput) {
            fileInput.click();
        }
    }

    async handleImageFileSelect(fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

        const optionItem = fileInput.closest('.mcq-option-item');
        if (!optionItem) return;

        // Validate file type
        if (!file.type.startsWith('image/')) {
            alert('لطفاً یک فایل تصویری انتخاب کنید');
            return;
        }

        // Validate file size (10MB limit)
        if (file.size > 10 * 1024 * 1024) {
            alert('حجم فایل نباید بیش از 10 مگابایت باشد');
            return;
        }

        try {
            // Show preview
            const preview = optionItem.querySelector('[data-role="mcq-option-image-preview"]');
            const reader = new FileReader();
            
            reader.onload = (e) => {
                if (preview) {
                    preview.innerHTML = `<img src="${e.target.result}" alt="Preview" style="max-width: 100%; max-height: 200px; border-radius: 4px;">`;
                }
            };
            
            reader.readAsDataURL(file);

            // Upload file
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', 'image');

            const response = await fetch(this.uploadUrl, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: formData
            });

            const result = await response.json();

            if (result.success && result.data) {
                const urlInput = optionItem.querySelector('[data-role="mcq-option-image-url"]');
                const fileIdInput = optionItem.querySelector('[data-role="mcq-option-image-fileid"]');
                
                const fileUrl = result.data.url || result.url || '';
                const fileId = result.data.id || result.fileId || '';
                const fileName = result.data.fileName || result.data.originalFileName || file.name;
                
                if (urlInput) urlInput.value = fileUrl;
                if (fileIdInput) fileIdInput.value = String(fileId);
                
                // Store file info in dataset
                optionItem.dataset.pendingImageFile = fileName;
                optionItem.dataset.imageFileId = String(fileId);
                optionItem.dataset.imageFileUrl = fileUrl;
                
                // Update preview
                const preview = optionItem.querySelector('[data-role="mcq-option-image-preview"]');
                if (preview && fileUrl) {
                    preview.innerHTML = `<img src="${fileUrl}" alt="Preview" style="max-width: 100%; max-height: 200px; border-radius: 4px;">`;
                }
            } else {
                alert(result.message || 'خطا در آپلود تصویر');
            }
        } catch (error) {
            console.error('Error uploading image:', error);
            alert('خطا در آپلود تصویر');
        }
    }

    triggerAudioUpload(optionItem) {
        const fileInput = optionItem?.querySelector('.mcq-option-audio-file');
        if (fileInput) {
            fileInput.click();
        }
    }

    async handleAudioFileSelect(fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

        const optionItem = fileInput.closest('.mcq-option-item');
        if (!optionItem) return;

        // Validate file type
        if (!file.type.startsWith('audio/') && !/\.(mp3|wav|ogg)$/i.test(file.name)) {
            alert('لطفاً یک فایل صوتی انتخاب کنید');
            return;
        }

        // Validate file size (10MB limit)
        if (file.size > 10 * 1024 * 1024) {
            alert('حجم فایل نباید بیش از 10 مگابایت باشد');
            return;
        }

        await this._handleAudioFileUpload(optionItem, file, false);
    }

    async startRecording(optionItem) {
        if (this.isRecording) {
            alert('در حال ضبط صدا هستید. لطفاً ابتدا ضبط قبلی را متوقف کنید.');
            return;
        }

        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            const mediaRecorder = new MediaRecorder(stream);
            const chunks = [];
            
            this.isRecording = true;
            this.currentRecordingOption = optionItem;

            // Store recorder and stream on the option item
            optionItem._mediaRecorder = mediaRecorder;
            optionItem._recordingStream = stream;
            optionItem._recordingChunks = chunks;

            mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    chunks.push(event.data);
                }
            };

            mediaRecorder.onstop = async () => {
                const blob = new Blob(chunks, { type: 'audio/wav' });
                const file = new File([blob], `recording_${Date.now()}.wav`, { type: 'audio/wav' });
                
                // Stop stream tracks
                stream.getTracks().forEach(t => t.stop());
                
                // Upload the recorded file
                await this._handleAudioFileUpload(optionItem, file, true);
                
                // Cleanup
                this.isRecording = false;
                this.currentRecordingOption = null;
                delete optionItem._mediaRecorder;
                delete optionItem._recordingStream;
                delete optionItem._recordingChunks;
                
                // Update UI
                this.updateRecordingUI(optionItem, false);
            };

            mediaRecorder.start();
            this.updateRecordingUI(optionItem, true);

        } catch (error) {
            console.error('Error starting recording:', error);
            alert('خطا در شروع ضبط صدا. لطفاً دسترسی میکروفون را بررسی کنید.');
            this.isRecording = false;
            this.currentRecordingOption = null;
        }
    }

    stopRecording() {
        if (!this.isRecording || !this.currentRecordingOption) return;

        const optionItem = this.currentRecordingOption;
        const mediaRecorder = optionItem._mediaRecorder;
        
        if (mediaRecorder && mediaRecorder.state !== 'inactive') {
            mediaRecorder.stop();
        }
    }

    updateRecordingUI(optionItem, isRecording) {
        if (!optionItem) return;

        const recordBtn = optionItem.querySelector('[data-action="record-audio"]');
        const stopBtn = optionItem.querySelector('[data-action="stop-recording"]');
        const uploadBtn = optionItem.querySelector('[data-action="upload-audio"]');

        if (isRecording) {
            if (recordBtn) recordBtn.disabled = true;
            if (stopBtn) stopBtn.disabled = false;
            if (uploadBtn) uploadBtn.disabled = true;
        } else {
            if (recordBtn) recordBtn.disabled = false;
            if (stopBtn) stopBtn.disabled = true;
            if (uploadBtn) uploadBtn.disabled = false;
        }
    }

    async _handleAudioFileUpload(optionItem, file, isRecorded = false) {
        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', 'audio');

            const response = await fetch(this.uploadUrl, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: formData
            });

            const result = await response.json();

            if (result.success && result.data) {
                const fileUrl = result.data.url || result.url || '';
                const fileId = result.data.id || result.fileId || '';
                const fileName = result.data.fileName || result.data.originalFileName || file.name || '';
                
                this.showAudioPreview(optionItem, fileUrl, fileName, isRecorded);
                
                const urlInput = optionItem.querySelector('[data-role="mcq-option-audio-url"]');
                const fileIdInput = optionItem.querySelector('[data-role="mcq-option-audio-fileid"]');
                const recordedInput = optionItem.querySelector('[data-role="mcq-option-audio-recorded"]');
                
                if (urlInput) urlInput.value = fileUrl;
                if (fileIdInput) fileIdInput.value = String(fileId || '');
                if (recordedInput) recordedInput.value = isRecorded ? 'true' : 'false';
                
                // Store in dataset for easy access
                optionItem.dataset.audioFileId = String(fileId || '');
                optionItem.dataset.audioFileUrl = fileUrl;
            } else {
                alert(result.message || 'خطا در آپلود فایل صوتی');
            }
        } catch (error) {
            console.error('Error uploading audio:', error);
            alert('خطا در آپلود فایل صوتی');
        }
    }

    showAudioPreview(optionItem, url, fileName, isRecorded) {
        if (!optionItem) return;

        const preview = optionItem.querySelector('[data-role="mcq-option-audio-preview"]');
        const controls = optionItem.querySelector('.mcq-option-audio-controls');

        if (preview && url) {
            const player = preview.querySelector('.mcq-option-audio-player');
            const nameSpan = preview.querySelector('.mcq-option-audio-name');

            if (player) {
                player.src = url;
                player.style.display = 'block';
            }
            if (nameSpan) {
                nameSpan.textContent = fileName || (isRecorded ? 'ضبط صوتی' : 'فایل صوتی');
            }

            preview.style.display = 'block';
        }

        if (controls) {
            controls.style.display = 'flex';
        }
    }

    removeAudio(optionItem) {
        if (!optionItem) return;

        const preview = optionItem.querySelector('[data-role="mcq-option-audio-preview"]');
        const controls = optionItem.querySelector('.mcq-option-audio-controls');
        const urlInput = optionItem.querySelector('[data-role="mcq-option-audio-url"]');
        const fileIdInput = optionItem.querySelector('[data-role="mcq-option-audio-fileid"]');
        const recordedInput = optionItem.querySelector('[data-role="mcq-option-audio-recorded"]');
        const fileInput = optionItem.querySelector('.mcq-option-audio-file');

        if (preview) preview.style.display = 'none';
        if (controls) controls.style.display = 'flex';
        if (urlInput) urlInput.value = '';
        if (fileIdInput) fileIdInput.value = '';
        if (recordedInput) recordedInput.value = 'false';
        if (fileInput) fileInput.value = '';
    }

    // Stem media handling methods
    changeStemMediaType(questionItem, mediaType) {
        if (!questionItem) return;

        // Hide all media content
        const mediaContents = questionItem.querySelectorAll('.mcq-stem-media-content');
        mediaContents.forEach(content => {
            content.style.display = 'none';
        });

        // Show selected media type (if any)
        if (mediaType) {
            const selectedMediaContent = questionItem.querySelector(`.mcq-stem-media-content[data-media-type="${mediaType}"]`);
            if (selectedMediaContent) {
                selectedMediaContent.style.display = 'block';
            }
            questionItem.dataset.stemMediaType = mediaType;
        } else {
            delete questionItem.dataset.stemMediaType;
        }
    }

    triggerStemImageUpload(questionItem) {
        const fileInput = questionItem?.querySelector('.mcq-stem-image-file');
        if (fileInput) {
            fileInput.click();
        }
    }

    async handleStemImageFileSelect(fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

        const questionItem = fileInput.closest('.mcq-question-item');
        if (!questionItem) return;

        // Validate file type
        if (!file.type.startsWith('image/')) {
            alert('لطفاً یک فایل تصویری انتخاب کنید');
            return;
        }

        // Validate file size (10MB limit)
        if (file.size > 10 * 1024 * 1024) {
            alert('حجم فایل نباید بیش از 10 مگابایت باشد');
            return;
        }

        try {
            // Show preview
            const preview = questionItem.querySelector('[data-role="mcq-stem-image-preview"]');
            const reader = new FileReader();
            
            reader.onload = (e) => {
                if (preview) {
                    preview.innerHTML = `<img src="${e.target.result}" alt="Preview" style="max-width: 100%; max-height: 200px; border-radius: 4px;">`;
                }
            };
            
            reader.readAsDataURL(file);

            // Upload file
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', 'image');

            const response = await fetch(this.uploadUrl, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: formData
            });

            const result = await response.json();

            if (result.success && result.data) {
                const urlInput = questionItem.querySelector('[data-role="mcq-stem-image-url"]');
                const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-image-fileid"]');
                
                const fileUrl = result.data.url || result.url || '';
                const fileId = result.data.id || result.fileId || '';
                const fileName = result.data.fileName || result.data.originalFileName || file.name;
                
                if (urlInput) urlInput.value = fileUrl;
                if (fileIdInput) fileIdInput.value = String(fileId);
                
                // Store file info in dataset
                questionItem.dataset.pendingStemImageFile = fileName;
                questionItem.dataset.stemImageFileId = String(fileId);
                questionItem.dataset.stemImageFileUrl = fileUrl;
                
                // Update preview
                const preview = questionItem.querySelector('[data-role="mcq-stem-image-preview"]');
                const removeBtn = questionItem.querySelector('[data-action="remove-stem-media"][data-media-type="image"]');
                if (preview && fileUrl) {
                    preview.innerHTML = `<img src="${fileUrl}" alt="Preview" style="max-width: 100%; max-height: 200px; border-radius: 4px;">`;
                    if (removeBtn) removeBtn.style.display = 'inline-block';
                }
                
                // Trigger content update
                this.triggerContentUpdate(questionItem.closest('.content-block[data-type="multipleChoice"]'));
            } else {
                alert(result.message || 'خطا در آپلود تصویر');
            }
        } catch (error) {
            console.error('Error uploading stem image:', error);
            alert('خطا در آپلود تصویر');
        }
    }

    triggerStemAudioUpload(questionItem) {
        const fileInput = questionItem?.querySelector('.mcq-stem-audio-file');
        if (fileInput) {
            fileInput.click();
        }
    }

    async handleStemAudioFileSelect(fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

        const questionItem = fileInput.closest('.mcq-question-item');
        if (!questionItem) return;

        // Validate file type
        if (!file.type.startsWith('audio/') && !/\.(mp3|wav|ogg)$/i.test(file.name)) {
            alert('لطفاً یک فایل صوتی انتخاب کنید');
            return;
        }

        // Validate file size (10MB limit)
        if (file.size > 10 * 1024 * 1024) {
            alert('حجم فایل نباید بیش از 10 مگابایت باشد');
            return;
        }

        await this._handleStemAudioFileUpload(questionItem, file, false);
    }

    async startStemRecording(questionItem) {
        if (this.isRecording) {
            alert('در حال ضبط صدا هستید. لطفاً ابتدا ضبط قبلی را متوقف کنید.');
            return;
        }

        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            const mediaRecorder = new MediaRecorder(stream);
            const chunks = [];
            
            this.isRecording = true;
            this.currentRecordingOption = questionItem;

            // Store recorder and stream on the question item
            questionItem._mediaRecorder = mediaRecorder;
            questionItem._recordingStream = stream;
            questionItem._recordingChunks = chunks;

            mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    chunks.push(event.data);
                }
            };

            mediaRecorder.onstop = async () => {
                const blob = new Blob(chunks, { type: 'audio/wav' });
                const file = new File([blob], `recording_${Date.now()}.wav`, { type: 'audio/wav' });
                
                // Stop stream tracks
                stream.getTracks().forEach(t => t.stop());
                
                // Upload the recorded file
                await this._handleStemAudioFileUpload(questionItem, file, true);
                
                // Cleanup
                this.isRecording = false;
                this.currentRecordingOption = null;
                delete questionItem._mediaRecorder;
                delete questionItem._recordingStream;
                delete questionItem._recordingChunks;
                
                // Update UI
                this.updateStemRecordingUI(questionItem, false);
            };

            mediaRecorder.start();
            this.updateStemRecordingUI(questionItem, true);

        } catch (error) {
            console.error('Error starting stem recording:', error);
            alert('خطا در شروع ضبط صدا. لطفاً دسترسی میکروفون را بررسی کنید.');
            this.isRecording = false;
            this.currentRecordingOption = null;
        }
    }

    stopStemRecording() {
        if (!this.isRecording || !this.currentRecordingOption) return;

        const questionItem = this.currentRecordingOption;
        const mediaRecorder = questionItem._mediaRecorder;
        
        if (mediaRecorder && mediaRecorder.state !== 'inactive') {
            mediaRecorder.stop();
        }
    }

    updateStemRecordingUI(questionItem, isRecording) {
        if (!questionItem) return;

        const recordBtn = questionItem.querySelector('[data-action="record-stem-audio"]');
        const stopBtn = questionItem.querySelector('[data-action="stop-stem-recording"]');
        const uploadBtn = questionItem.querySelector('[data-action="upload-stem-audio"]');

        if (isRecording) {
            if (recordBtn) recordBtn.disabled = true;
            if (stopBtn) stopBtn.disabled = false;
            if (uploadBtn) uploadBtn.disabled = true;
        } else {
            if (recordBtn) recordBtn.disabled = false;
            if (stopBtn) stopBtn.disabled = true;
            if (uploadBtn) uploadBtn.disabled = false;
        }
    }

    async _handleStemAudioFileUpload(questionItem, file, isRecorded = false) {
        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', 'audio');

            const response = await fetch(this.uploadUrl, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: formData
            });

            const result = await response.json();

            if (result.success && result.data) {
                const fileUrl = result.data.url || result.url || '';
                const fileId = result.data.id || result.fileId || '';
                const fileName = result.data.fileName || result.data.originalFileName || file.name || '';
                
                this.showStemAudioPreview(questionItem, fileUrl, fileName, isRecorded);
                
                const urlInput = questionItem.querySelector('[data-role="mcq-stem-audio-url"]');
                const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-audio-fileid"]');
                const recordedInput = questionItem.querySelector('[data-role="mcq-stem-audio-recorded"]');
                
                if (urlInput) urlInput.value = fileUrl;
                if (fileIdInput) fileIdInput.value = String(fileId || '');
                if (recordedInput) recordedInput.value = isRecorded ? 'true' : 'false';
                
                // Store in dataset for easy access
                questionItem.dataset.stemAudioFileId = String(fileId || '');
                questionItem.dataset.stemAudioFileUrl = fileUrl;
                
                // Trigger content update
                this.triggerContentUpdate(questionItem.closest('.content-block[data-type="multipleChoice"]'));
            } else {
                alert(result.message || 'خطا در آپلود فایل صوتی');
            }
        } catch (error) {
            console.error('Error uploading stem audio:', error);
            alert('خطا در آپلود فایل صوتی');
        }
    }

    showStemAudioPreview(questionItem, url, fileName, isRecorded) {
        if (!questionItem) return;

        const preview = questionItem.querySelector('[data-role="mcq-stem-audio-preview"]');
        const controls = questionItem.querySelector('.mcq-stem-audio-controls');

        if (preview && url) {
            const player = preview.querySelector('.mcq-stem-audio-player');
            const nameSpan = preview.querySelector('.mcq-stem-audio-name');

            if (player) {
                player.src = url;
                player.style.display = 'block';
            }
            if (nameSpan) {
                nameSpan.textContent = fileName || (isRecorded ? 'ضبط صوتی' : 'فایل صوتی');
            }

            preview.style.display = 'block';
        }

        if (controls) {
            controls.style.display = 'flex';
        }
    }

    removeStemAudio(questionItem) {
        if (!questionItem) return;
        this.removeStemMedia(questionItem, 'audio');
    }

    removeStemMedia(questionItem, mediaType) {
        if (!questionItem || !mediaType) return;

        if (mediaType === 'image') {
            const preview = questionItem.querySelector('[data-role="mcq-stem-image-preview"]');
            const urlInput = questionItem.querySelector('[data-role="mcq-stem-image-url"]');
            const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-image-fileid"]');
            const fileInput = questionItem.querySelector('.mcq-stem-image-file');
            const removeBtn = questionItem.querySelector('[data-action="remove-stem-media"][data-media-type="image"]');

            if (preview) {
                preview.innerHTML = '<div class="mcq-stem-image-placeholder"><i class="fas fa-image"></i><span>برای آپلود تصویر کلیک کنید</span></div>';
            }
            if (urlInput) urlInput.value = '';
            if (fileIdInput) fileIdInput.value = '';
            if (fileInput) fileInput.value = '';
            if (removeBtn) removeBtn.style.display = 'none';
            delete questionItem.dataset.stemImageFileId;
            delete questionItem.dataset.stemImageFileUrl;
            delete questionItem.dataset.pendingStemImageFile;
        } else if (mediaType === 'audio') {
            const preview = questionItem.querySelector('[data-role="mcq-stem-audio-preview"]');
            const controls = questionItem.querySelector('.mcq-stem-audio-controls');
            const urlInput = questionItem.querySelector('[data-role="mcq-stem-audio-url"]');
            const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-audio-fileid"]');
            const recordedInput = questionItem.querySelector('[data-role="mcq-stem-audio-recorded"]');
            const fileInput = questionItem.querySelector('.mcq-stem-audio-file');

            if (preview) preview.style.display = 'none';
            if (controls) controls.style.display = 'flex';
            if (urlInput) urlInput.value = '';
            if (fileIdInput) fileIdInput.value = '';
            if (recordedInput) recordedInput.value = 'false';
            if (fileInput) fileInput.value = '';
            delete questionItem.dataset.stemAudioFileId;
            delete questionItem.dataset.stemAudioFileUrl;
        } else if (mediaType === 'video') {
            const preview = questionItem.querySelector('[data-role="mcq-stem-video-preview"]');
            const urlInput = questionItem.querySelector('[data-role="mcq-stem-video-url"]');
            const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-video-fileid"]');
            const fileInput = questionItem.querySelector('.mcq-stem-video-file');
            const removeBtn = questionItem.querySelector('[data-action="remove-stem-media"][data-media-type="video"]');

            if (preview) {
                preview.innerHTML = '<div class="mcq-stem-video-placeholder"><i class="fas fa-video"></i><span>برای آپلود ویدیو کلیک کنید</span></div>';
            }
            if (urlInput) urlInput.value = '';
            if (fileIdInput) fileIdInput.value = '';
            if (fileInput) fileInput.value = '';
            if (removeBtn) removeBtn.style.display = 'none';
            delete questionItem.dataset.stemVideoFileId;
            delete questionItem.dataset.stemVideoFileUrl;
            delete questionItem.dataset.pendingStemVideoFile;
        }

        // Reset media type selector
        const mediaTypeSelect = questionItem.querySelector('[data-role="mcq-stem-media-type"]');
        if (mediaTypeSelect) {
            mediaTypeSelect.value = '';
            this.changeStemMediaType(questionItem, '');
        }

        // Trigger content update
        this.triggerContentUpdate(questionItem.closest('.content-block[data-type="multipleChoice"]'));
    }

    triggerStemVideoUpload(questionItem) {
        const fileInput = questionItem?.querySelector('.mcq-stem-video-file');
        if (fileInput) {
            fileInput.click();
        }
    }

    async handleStemVideoFileSelect(fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

        const questionItem = fileInput.closest('.mcq-question-item');
        if (!questionItem) return;

        // Validate file type
        if (!file.type.startsWith('video/')) {
            alert('لطفاً یک فایل ویدیویی انتخاب کنید');
            return;
        }

        // Validate file size (50MB limit for video)
        if (file.size > 50 * 1024 * 1024) {
            alert('حجم فایل نباید بیش از 50 مگابایت باشد');
            return;
        }

        try {
            // Show preview
            const preview = questionItem.querySelector('[data-role="mcq-stem-video-preview"]');
            const reader = new FileReader();
            
            reader.onload = (e) => {
                if (preview) {
                    preview.innerHTML = `<video src="${e.target.result}" controls style="max-width: 100%; max-height: 300px; border-radius: 4px;"></video>`;
                }
            };
            
            reader.readAsDataURL(file);

            // Upload file
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', 'video');

            const response = await fetch(this.uploadUrl, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: formData
            });

            const result = await response.json();

            if (result.success && result.data) {
                const urlInput = questionItem.querySelector('[data-role="mcq-stem-video-url"]');
                const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-video-fileid"]');
                
                const fileUrl = result.data.url || result.url || '';
                const fileId = result.data.id || result.fileId || '';
                const fileName = result.data.fileName || result.data.originalFileName || file.name;
                
                if (urlInput) urlInput.value = fileUrl;
                if (fileIdInput) fileIdInput.value = String(fileId);
                
                // Store file info in dataset
                questionItem.dataset.pendingStemVideoFile = fileName;
                questionItem.dataset.stemVideoFileId = String(fileId);
                questionItem.dataset.stemVideoFileUrl = fileUrl;
                
                // Update preview
                const preview = questionItem.querySelector('[data-role="mcq-stem-video-preview"]');
                const removeBtn = questionItem.querySelector('[data-action="remove-stem-media"][data-media-type="video"]');
                if (preview && fileUrl) {
                    preview.innerHTML = `<video src="${fileUrl}" controls style="max-width: 100%; max-height: 300px; border-radius: 4px;"></video>`;
                    if (removeBtn) removeBtn.style.display = 'inline-block';
                }
                
                // Trigger content update
                this.triggerContentUpdate(questionItem.closest('.content-block[data-type="multipleChoice"]'));
            } else {
                alert(result.message || 'خطا در آپلود ویدیو');
            }
        } catch (error) {
            console.error('Error uploading stem video:', error);
            alert('خطا در آپلود ویدیو');
        }
    }

    // Collect MCQ data from block
    collectMcqData(blockElement) {
        const container = blockElement?.querySelector('[data-role="mcq-container"]');
        if (!container) return null;

        const questions = [];
        const questionItems = container.querySelectorAll('.mcq-question-item');

        questionItems.forEach((questionItem, qIndex) => {
            // Skip if this is a template or empty state element
            if (questionItem.closest('.content-block-template') || 
                questionItem.classList.contains('mcq-empty-state')) {
                return;
            }

            // Collect stem data - text is always required, media is optional
            const stem = questionItem.querySelector('[data-role="mcq-stem-text"]')?.value?.trim() || '';
            const stemMediaType = questionItem.querySelector('[data-role="mcq-stem-media-type"]')?.value || questionItem.dataset.stemMediaType || '';
            const stemData = {
                text: stem,
                mediaType: stemMediaType || null
            };

            // Collect media data if media type is selected
            if (stemMediaType === 'image') {
                stemData.imageUrl = questionItem.querySelector('[data-role="mcq-stem-image-url"]')?.value || questionItem.dataset.stemImageFileUrl || '';
                stemData.imageFileId = questionItem.querySelector('[data-role="mcq-stem-image-fileid"]')?.value || questionItem.dataset.stemImageFileId || '';
                stemData.imageFileName = questionItem.dataset.pendingStemImageFile || '';
            } else if (stemMediaType === 'audio') {
                stemData.audioUrl = questionItem.querySelector('[data-role="mcq-stem-audio-url"]')?.value || questionItem.dataset.stemAudioFileUrl || '';
                stemData.audioFileId = questionItem.querySelector('[data-role="mcq-stem-audio-fileid"]')?.value || questionItem.dataset.stemAudioFileId || '';
                stemData.isRecorded = questionItem.querySelector('[data-role="mcq-stem-audio-recorded"]')?.value === 'true';
                const audioName = questionItem.querySelector('.mcq-stem-audio-name')?.textContent || '';
                stemData.audioFileName = audioName || '';
            } else if (stemMediaType === 'video') {
                stemData.videoUrl = questionItem.querySelector('[data-role="mcq-stem-video-url"]')?.value || questionItem.dataset.stemVideoFileUrl || '';
                stemData.videoFileId = questionItem.querySelector('[data-role="mcq-stem-video-fileid"]')?.value || questionItem.dataset.stemVideoFileId || '';
                stemData.videoFileName = questionItem.dataset.pendingStemVideoFile || '';
            }

            const answerType = questionItem.querySelector('[data-role="mcq-answer-type"]')?.value || 'single';
            const randomize = questionItem.querySelector('[data-role="mcq-randomize"]')?.checked || false;

            const options = [];
            const optionItems = questionItem.querySelectorAll('.mcq-option-item');

            optionItems.forEach((optionItem, oIndex) => {
                // Skip if option item is not valid
                if (!optionItem || !optionItem.closest('.mcq-options-list')) {
                    return;
                }

                const optionType = optionItem.dataset.optionType || 'text';
                const isCorrect = optionItem.querySelector('[data-role="mcq-option-correct"]')?.checked || false;

                const option = {
                    index: oIndex,
                    optionType: optionType,
                    isCorrect: isCorrect
                };

                if (optionType === 'text') {
                    const textValue = optionItem.querySelector('[data-role="mcq-option-text"]')?.value?.trim() || '';
                    option.text = textValue || '';
                    // Note: We keep empty text options to allow users to save incomplete questions
                } else if (optionType === 'image') {
                    option.imageUrl = optionItem.querySelector('[data-role="mcq-option-image-url"]')?.value || optionItem.dataset.imageFileUrl || '';
                    const fileIdValue = optionItem.querySelector('[data-role="mcq-option-image-fileid"]')?.value || optionItem.dataset.imageFileId || '';
                    option.imageFileId = fileIdValue ? String(fileIdValue) : '';
                    option.imageFileName = optionItem.dataset.pendingImageFile || '';
                    
                    // Only add option if it has an image
                    if (!option.imageUrl && !option.imageFileId) {
                        return; // Skip options without image
                    }
                } else if (optionType === 'audio') {
                    option.audioUrl = optionItem.querySelector('[data-role="mcq-option-audio-url"]')?.value || optionItem.dataset.audioFileUrl || '';
                    const audioFileIdValue = optionItem.querySelector('[data-role="mcq-option-audio-fileid"]')?.value || optionItem.dataset.audioFileId || '';
                    option.audioFileId = audioFileIdValue ? String(audioFileIdValue) : '';
                    option.isRecorded = optionItem.querySelector('[data-role="mcq-option-audio-recorded"]')?.value === 'true';
                    const audioName = optionItem.querySelector('.mcq-option-audio-name')?.textContent || '';
                    option.audioFileName = audioName || '';
                    
                    // Only add option if it has audio
                    if (!option.audioUrl && !option.audioFileId) {
                        return; // Skip options without audio
                    }
                }

                options.push(option);
            });

            // Only add question if it has at least one valid option or has a stem (text)
            if (stem || options.length > 0) {
                questions.push({
                    id: questionItem.dataset.questionId || `q-${Date.now()}-${qIndex}`,
                    stem: stem,
                    stemData: stemData,
                    answerType: answerType,
                    randomizeOptions: randomize,
                    options: options
                });
            }
        });

        return {
            questions: questions
        };
    }

    // Load MCQ data into block
    async loadMcqData(blockElement, data) {
        if (!blockElement || !data || !data.questions) return;
        
        // Check if already loading to prevent duplicate loads
        if (blockElement.dataset.mcqLoading === 'true') {
            return;
        }
        
        blockElement.dataset.mcqLoading = 'true';

        const container = blockElement.querySelector('[data-role="mcq-container"]');
        if (!container) {
            delete blockElement.dataset.mcqLoading;
            return;
        }

        // Clear existing questions completely
        const questionsList = container.querySelector('[data-role="mcq-list"]');
        if (questionsList) {
            questionsList.innerHTML = '';
        }

        // Load questions - skip default options since we're loading existing data
        for (let qIndex = 0; qIndex < data.questions.length; qIndex++) {
            const question = data.questions[qIndex];
            await this.addQuestion(blockElement, true);
            
            const questionItems = container.querySelectorAll('.mcq-question-item');
            const questionItem = questionItems[questionItems.length - 1];

            if (questionItem) {
                // Set question stem text (always required)
                const stemTextInput = questionItem.querySelector('[data-role="mcq-stem-text"]');
                if (stemTextInput) {
                    const stemValue = question.stem || (question.stemData?.text || '');
                    stemTextInput.value = stemValue;
                }
                
                // Set stem media (optional)
                const stemData = question.stemData || {};
                const stemMediaType = stemData.mediaType || '';
                const stemMediaTypeSelect = questionItem.querySelector('[data-role="mcq-stem-media-type"]');
                
                if (stemMediaTypeSelect) {
                    stemMediaTypeSelect.value = stemMediaType || '';
                    if (stemMediaType) {
                        this.changeStemMediaType(questionItem, stemMediaType);
                    }
                }
                
                // Set stem media content based on type
                if (stemMediaType === 'image' && stemData) {
                    const urlInput = questionItem.querySelector('[data-role="mcq-stem-image-url"]');
                    const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-image-fileid"]');
                    
                    const imageUrl = stemData.imageUrl || '';
                    const imageFileId = stemData.imageFileId ? String(stemData.imageFileId) : '';
                    const imageFileName = stemData.imageFileName || '';
                    
                    if (urlInput) urlInput.value = imageUrl;
                    if (fileIdInput) fileIdInput.value = imageFileId;
                    
                    // Store in dataset
                    if (imageFileId) questionItem.dataset.stemImageFileId = imageFileId;
                    if (imageUrl) questionItem.dataset.stemImageFileUrl = imageUrl;
                    if (imageFileName) questionItem.dataset.pendingStemImageFile = imageFileName;
                    
                    // Show preview
                    if (imageUrl) {
                        const preview = questionItem.querySelector('[data-role="mcq-stem-image-preview"]');
                        const removeBtn = questionItem.querySelector('[data-action="remove-stem-media"][data-media-type="image"]');
                        if (preview) {
                            preview.innerHTML = `<img src="${imageUrl}" alt="Preview" style="max-width: 100%; max-height: 200px; border-radius: 4px;">`;
                            if (removeBtn) removeBtn.style.display = 'inline-block';
                        }
                    }
                } else if (stemMediaType === 'audio' && stemData) {
                    const urlInput = questionItem.querySelector('[data-role="mcq-stem-audio-url"]');
                    const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-audio-fileid"]');
                    const recordedInput = questionItem.querySelector('[data-role="mcq-stem-audio-recorded"]');
                    
                    const audioUrl = stemData.audioUrl || '';
                    const audioFileId = stemData.audioFileId ? String(stemData.audioFileId) : '';
                    const audioFileName = stemData.audioFileName || '';
                    
                    if (urlInput) urlInput.value = audioUrl;
                    if (fileIdInput) fileIdInput.value = audioFileId;
                    if (recordedInput) recordedInput.value = stemData.isRecorded ? 'true' : 'false';
                    
                    // Store in dataset
                    if (audioFileId) questionItem.dataset.stemAudioFileId = audioFileId;
                    if (audioUrl) questionItem.dataset.stemAudioFileUrl = audioUrl;
                    
                    // Show preview
                    if (audioUrl) {
                        this.showStemAudioPreview(questionItem, audioUrl, audioFileName, stemData.isRecorded || false);
                    }
                } else if (stemMediaType === 'video' && stemData) {
                    const urlInput = questionItem.querySelector('[data-role="mcq-stem-video-url"]');
                    const fileIdInput = questionItem.querySelector('[data-role="mcq-stem-video-fileid"]');
                    
                    const videoUrl = stemData.videoUrl || '';
                    const videoFileId = stemData.videoFileId ? String(stemData.videoFileId) : '';
                    const videoFileName = stemData.videoFileName || '';
                    
                    if (urlInput) urlInput.value = videoUrl;
                    if (fileIdInput) fileIdInput.value = videoFileId;
                    
                    // Store in dataset
                    if (videoFileId) questionItem.dataset.stemVideoFileId = videoFileId;
                    if (videoUrl) questionItem.dataset.stemVideoFileUrl = videoUrl;
                    if (videoFileName) questionItem.dataset.pendingStemVideoFile = videoFileName;
                    
                    // Show preview
                    if (videoUrl) {
                        const preview = questionItem.querySelector('[data-role="mcq-stem-video-preview"]');
                        if (preview) {
                            preview.innerHTML = `<video src="${videoUrl}" controls style="max-width: 100%; max-height: 300px; border-radius: 4px;"></video>`;
                        }
                    }
                }
                
                // Set answer type and randomize
                const answerTypeSelect = questionItem.querySelector('[data-role="mcq-answer-type"]');
                const randomizeCheckbox = questionItem.querySelector('[data-role="mcq-randomize"]');

                if (answerTypeSelect) answerTypeSelect.value = question.answerType || 'single';
                if (randomizeCheckbox) randomizeCheckbox.checked = question.randomizeOptions || false;

                // Update answer type
                this.updateAnswerType(questionItem, question.answerType || 'single');

                // Load options
                const optionsList = questionItem.querySelector('[data-role="mcq-options"]');
                if (optionsList) {
                    optionsList.innerHTML = '';

                    for (let oIndex = 0; oIndex < (question.options?.length || 0); oIndex++) {
                        const optionData = question.options[oIndex];
                        await this.addOption(questionItem);
                        
                        const optionItems = optionsList.querySelectorAll('.mcq-option-item');
                        const optionItem = optionItems[optionItems.length - 1];

                        if (optionItem) {
                            // Set option type first
                            const typeSelect = optionItem.querySelector('[data-role="mcq-option-type"]');
                            if (typeSelect) {
                                typeSelect.value = optionData.optionType || 'text';
                                // Change option type and wait a bit for UI to update
                                this.changeOptionType(optionItem, optionData.optionType || 'text');
                                // Small delay to ensure UI is updated
                                await new Promise(resolve => setTimeout(resolve, 10));
                            }

                            // Set option data based on type
                            if (optionData.optionType === 'text') {
                                const textInput = optionItem.querySelector('[data-role="mcq-option-text"]');
                                const correctInput = optionItem.querySelector('[data-role="mcq-option-correct"]');
                                if (textInput) textInput.value = optionData.text || '';
                                if (correctInput) correctInput.checked = optionData.isCorrect || false;
                            } else if (optionData.optionType === 'image') {
                                const urlInput = optionItem.querySelector('[data-role="mcq-option-image-url"]');
                                const fileIdInput = optionItem.querySelector('[data-role="mcq-option-image-fileid"]');
                                const correctInput = optionItem.querySelector('[data-role="mcq-option-correct"]');
                                
                                const imageUrl = optionData.imageUrl || '';
                                const imageFileId = optionData.imageFileId ? String(optionData.imageFileId) : '';
                                const imageFileName = optionData.imageFileName || '';
                                
                                if (urlInput) urlInput.value = imageUrl;
                                if (fileIdInput) fileIdInput.value = imageFileId;
                                if (correctInput) correctInput.checked = optionData.isCorrect || false;
                                
                                // Store in dataset
                                if (imageFileId) optionItem.dataset.imageFileId = imageFileId;
                                if (imageUrl) optionItem.dataset.imageFileUrl = imageUrl;
                                if (imageFileName) optionItem.dataset.pendingImageFile = imageFileName;

                                // Show preview
                                if (imageUrl) {
                                    const preview = optionItem.querySelector('[data-role="mcq-option-image-preview"]');
                                    if (preview) {
                                        preview.innerHTML = `<img src="${imageUrl}" alt="Preview" style="max-width: 100%; max-height: 200px; border-radius: 4px;">`;
                                    }
                                }
                            } else if (optionData.optionType === 'audio') {
                                const urlInput = optionItem.querySelector('[data-role="mcq-option-audio-url"]');
                                const fileIdInput = optionItem.querySelector('[data-role="mcq-option-audio-fileid"]');
                                const recordedInput = optionItem.querySelector('[data-role="mcq-option-audio-recorded"]');
                                const correctInput = optionItem.querySelector('[data-role="mcq-option-correct"]');
                                
                                const audioUrl = optionData.audioUrl || '';
                                const audioFileId = optionData.audioFileId ? String(optionData.audioFileId) : '';
                                const audioFileName = optionData.audioFileName || '';
                                
                                if (urlInput) urlInput.value = audioUrl;
                                if (fileIdInput) fileIdInput.value = audioFileId;
                                if (recordedInput) recordedInput.value = optionData.isRecorded ? 'true' : 'false';
                                if (correctInput) correctInput.checked = optionData.isCorrect || false;
                                
                                // Store in dataset
                                if (audioFileId) optionItem.dataset.audioFileId = audioFileId;
                                if (audioUrl) optionItem.dataset.audioFileUrl = audioUrl;

                                // Show preview
                                if (audioUrl) {
                                    this.showAudioPreview(optionItem, audioUrl, audioFileName, optionData.isRecorded || false);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // Mark loading as complete
        delete blockElement.dataset.mcqLoading;
    }
}

// Initialize MCQ Manager
if (typeof window !== 'undefined') {
    window.mcqManager = new McqManager();
    window.McqManager = McqManager;
}
