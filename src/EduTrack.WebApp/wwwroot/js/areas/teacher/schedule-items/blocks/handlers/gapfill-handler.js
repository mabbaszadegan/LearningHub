/**
 * Gap Fill Block Handler
 * Handles gap fill question blocks
 */

class GapFillHandler {
    constructor(contentManager) {
        this.contentManager = contentManager;
        this.mediaRecorder = null;
        this.audioChunks = [];
        this.isRecording = false;
        this.currentRecordingBlock = null;
        this.uploadUrl = '/FileUpload/UploadContentFile';
    }

    canHandle(blockType) {
        return blockType === 'gapFill';
    }

    render(block) {
        const blockTemplatesContainer = document.getElementById('blockTemplatesContainer');
        if (!blockTemplatesContainer) {
            console.error('GapFillHandler: blockTemplatesContainer not found');
            return null;
        }

        const templatesContainer = blockTemplatesContainer.querySelector('#questionTypeBlockTemplates');
        if (!templatesContainer) {
            console.error('GapFillHandler: questionTypeBlockTemplates not found');
            return null;
        }

        const template = templatesContainer.querySelector('[data-type="gapFill"]');
        if (!template) {
            console.error('GapFillHandler: GapFill template not found');
            return null;
        }

        const blockElement = template.cloneNode(true);
        // Remove template class that hides the element
        blockElement.classList.remove('content-block-template');
        blockElement.classList.add('content-block', 'question-type-block');
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data || {});
        blockElement.style.display = ''; // Ensure it's visible

        return blockElement;
    }

    async initialize(blockElement, block) {
        // Wait a bit to ensure DOM is ready
        await new Promise(resolve => setTimeout(resolve, 50));

        // Initialize CKEditor for text editor
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (editorEl && typeof QuestionBlockBase !== 'undefined') {
            QuestionBlockBase.initializeCKEditorForBlock(blockElement);
            
            // Wait for CKEditor to be initialized and then setup content change listener
            setTimeout(() => {
                this.setupCKEditorChangeListener(blockElement, editorEl);
            }, 500);
        }

        // Setup gap fill editor
        const insertBlankBtn = blockElement.querySelector('[data-action="gf-insert-blank"]');
        if (insertBlankBtn) {
            insertBlankBtn.addEventListener('click', () => this.insertBlank(blockElement));
        }

        // Setup media handlers
        this.setupMediaHandlers(blockElement);

        // Setup settings handlers
        this.setupSettingsHandlers(blockElement);

        // Render existing gaps after a delay to ensure CKEditor is ready
        setTimeout(() => {
        if (block.data && block.data.gaps) {
            this.renderGaps(blockElement, block);
            } else {
                // Try to extract gaps from content
                this.updateGapsList(blockElement);
            }
        }, 600);

        // Load media if exists
        if (block.data) {
            this.loadMedia(blockElement, block.data);
        }
    }

    setupCKEditorChangeListener(blockElement, editorEl) {
        if (!window.ckeditorManager) {
            return;
        }

        const editor = window.ckeditorManager.editors.get(editorEl);
        if (!editor) {
            // Try again after a delay
            setTimeout(() => this.setupCKEditorChangeListener(blockElement, editorEl), 200);
            return;
        }

        // Listen for content changes in CKEditor
        editor.model.document.on('change:data', () => {
            // Debounce the update to avoid too many updates
            if (this.updateGapsTimeout) {
                clearTimeout(this.updateGapsTimeout);
            }
            
            this.updateGapsTimeout = setTimeout(() => {
                this.updateGapsList(blockElement);
            }, 300);
        });
    }

    setupMediaHandlers(blockElement) {
        // Media type selector
        const mediaTypeSelect = blockElement.querySelector('[data-role="gf-media-type"]');
        if (mediaTypeSelect) {
            mediaTypeSelect.addEventListener('change', (e) => {
                this.changeMediaType(blockElement, e.target.value);
            });
        }

        // Image upload
        const uploadImageBtn = blockElement.querySelector('[data-action="gf-upload-image"]');
        if (uploadImageBtn) {
            uploadImageBtn.addEventListener('click', () => {
                const fileInput = blockElement.querySelector('[data-role="gf-image-file"]');
                if (fileInput) fileInput.click();
            });
        }

        const imageFileInput = blockElement.querySelector('[data-role="gf-image-file"]');
        if (imageFileInput) {
            imageFileInput.addEventListener('change', (e) => {
                this.handleImageFileSelect(blockElement, e.target);
            });
        }

        // Audio upload
        const uploadAudioBtn = blockElement.querySelector('[data-action="gf-upload-audio"]');
        if (uploadAudioBtn) {
            uploadAudioBtn.addEventListener('click', () => {
                const fileInput = blockElement.querySelector('[data-role="gf-audio-file"]');
                if (fileInput) fileInput.click();
            });
        }

        const audioFileInput = blockElement.querySelector('[data-role="gf-audio-file"]');
        if (audioFileInput) {
            audioFileInput.addEventListener('change', (e) => {
                this.handleAudioFileSelect(blockElement, e.target);
            });
        }

        // Audio record
        const recordAudioBtn = blockElement.querySelector('[data-action="gf-record-audio"]');
        if (recordAudioBtn) {
            recordAudioBtn.addEventListener('click', () => {
                this.startRecording(blockElement);
            });
        }

        const stopRecordingBtn = blockElement.querySelector('[data-action="gf-stop-recording"]');
        if (stopRecordingBtn) {
            stopRecordingBtn.addEventListener('click', () => {
                this.stopRecording();
            });
        }

        // Video upload
        const uploadVideoBtn = blockElement.querySelector('[data-action="gf-upload-video"]');
        if (uploadVideoBtn) {
            uploadVideoBtn.addEventListener('click', () => {
                const fileInput = blockElement.querySelector('[data-role="gf-video-file"]');
                if (fileInput) fileInput.click();
            });
        }

        const videoFileInput = blockElement.querySelector('[data-role="gf-video-file"]');
        if (videoFileInput) {
            videoFileInput.addEventListener('change', (e) => {
                this.handleVideoFileSelect(blockElement, e.target);
            });
        }

        // Remove media buttons
        const removeMediaBtns = blockElement.querySelectorAll('[data-action="gf-remove-media"]');
        removeMediaBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const mediaType = btn.getAttribute('data-media-type');
                this.removeMedia(blockElement, mediaType);
            });
        });
    }

    setupSettingsHandlers(blockElement) {
        // Show options checkbox
        const showOptionsCheckbox = blockElement.querySelector('[data-role="gf-show-options"]');
        if (showOptionsCheckbox) {
            showOptionsCheckbox.addEventListener('change', () => {
                this.triggerContentUpdate(blockElement);
            });
        }

        // Answer type select
        const answerTypeSelect = blockElement.querySelector('[data-role="gf-answer-type"]');
        if (answerTypeSelect) {
            answerTypeSelect.addEventListener('change', () => {
                this.triggerContentUpdate(blockElement);
            });
        }

        // Case sensitive checkbox
        const caseCheckbox = blockElement.querySelector('[data-role="gf-case"]');
        if (caseCheckbox) {
            caseCheckbox.addEventListener('change', () => {
                this.triggerContentUpdate(blockElement);
            });
        }
    }

    triggerContentUpdate(blockElement) {
        if (!blockElement) return;
        
        const blockId = blockElement.dataset.blockId;
        if (blockId) {
            const event = new CustomEvent('blockContentChanged', {
                detail: {
                    blockElement: blockElement,
                    blockId: blockId
                }
            });
            document.dispatchEvent(event);
        }
    }

    changeMediaType(blockElement, mediaType) {
        // Hide all media contents
        const allMediaContents = blockElement.querySelectorAll('.gapfill-media-content');
        allMediaContents.forEach(content => {
            content.style.display = 'none';
        });

        // Show selected media type
        if (mediaType) {
            const selectedContent = blockElement.querySelector(`.gapfill-media-content[data-media-type="${mediaType}"]`);
            if (selectedContent) {
                selectedContent.style.display = 'block';
            }
        }

        this.triggerContentUpdate(blockElement);
    }

    insertBlank(blockElement) {
        // Insert blank token in CKEditor
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (!editorEl) {
            console.error('GapFillHandler: Editor element not found');
            alert('ادیتور پیدا نشد. لطفاً صفحه را رفرش کنید.');
            return;
        }

        if (!window.ckeditorManager) {
            console.error('GapFillHandler: ckeditorManager not available');
            alert('CKEditor Manager در دسترس نیست. لطفاً صفحه را رفرش کنید.');
            return;
        }

        // Wait for editor to be ready
        const waitForEditor = (attempts = 0) => {
            const maxAttempts = 10;
            const editor = window.ckeditorManager.editors.get(editorEl);
            
            if (editor) {
                try {
                const index = this.getNextGapIndex(blockElement);
                const token = ` [[blank${index}]] `;
                    
                editor.model.change(writer => {
                        // Get current selection position
                        const selection = editor.model.document.selection;
                        const pos = selection.getFirstPosition();
                        
                        // Insert the token
                    writer.insertText(token, pos);
                        
                        // Move cursor to the end of inserted text
                        const newPos = pos.getShiftedBy(token.length);
                        writer.setSelection(writer.createRange(newPos));
                    });
                    
                    // Update gaps list after insertion
                    setTimeout(() => {
                        this.updateGapsList(blockElement);
                    }, 100);
                    
                    // Focus editor after insertion to allow continued typing
                    setTimeout(() => {
                        editor.editing.view.focus();
                    }, 0);
                } catch (error) {
                    console.error('GapFillHandler: Error inserting blank:', error);
                    alert('خطا در درج جای خالی: ' + error.message);
                }
            } else if (attempts < maxAttempts) {
                // Editor not ready yet, wait and try again
                setTimeout(() => waitForEditor(attempts + 1), 100);
            } else {
                console.error('GapFillHandler: Editor not found after waiting');
                alert('ادیتور آماده نشد. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.');
            }
        };
        
        waitForEditor();
    }

    getNextGapIndex(blockElement) {
        // Get next available gap index by counting existing gaps in the text
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (!editorEl || !window.ckeditorManager) {
            return 1;
        }

        const editor = window.ckeditorManager.editors.get(editorEl);
        if (!editor) {
            return 1;
        }

        try {
            const content = editor.getData();
            // Find all existing blank tokens like [[blank1]], [[blank2]], etc.
            const blankMatches = content.match(/\[\[blank(\d+)\]\]/g);
            if (!blankMatches || blankMatches.length === 0) {
                return 1;
            }

            // Extract all numbers and find the maximum
            const indices = blankMatches.map(match => {
                const numMatch = match.match(/\d+/);
                return numMatch ? parseInt(numMatch[0]) : 0;
            });

            const maxIndex = Math.max(...indices);
            return maxIndex + 1;
        } catch (error) {
            console.error('GapFillHandler: Error getting next gap index:', error);
            return 1;
        }
    }

    renderGaps(blockElement, block) {
        // This method is called when loading existing data
        // First, update gaps list from content (to show all gaps found in text)
        this.updateGapsList(blockElement);
        
        // Then, if we have saved gap data, populate the input fields
        if (block && block.data && block.data.gaps && Array.isArray(block.data.gaps)) {
            setTimeout(() => {
                block.data.gaps.forEach(gap => {
                    const gapItem = blockElement.querySelector(`.gap-item[data-gap-index="${gap.index}"]`);
                    if (gapItem) {
                        const correctInput = gapItem.querySelector('[data-role="gf-correct"]');
                        const altsInput = gapItem.querySelector('[data-role="gf-alts"]');
                        const hintInput = gapItem.querySelector('[data-role="gf-hint"]');
                        
                        if (correctInput && gap.correctAnswer) {
                            correctInput.value = gap.correctAnswer;
                        }
                        if (altsInput && gap.alternativeAnswers && gap.alternativeAnswers.length > 0) {
                            altsInput.value = gap.alternativeAnswers.join('، ');
                        }
                        if (hintInput && gap.hint) {
                            hintInput.value = gap.hint;
                        }
                    }
                });
            }, 200);
        }
    }

    updateGapsList(blockElement) {
        // Extract gaps from CKEditor content
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (!editorEl) {
            console.warn('GapFillHandler: Editor element not found in updateGapsList');
            return;
        }

        if (!window.ckeditorManager) {
            console.warn('GapFillHandler: ckeditorManager not available');
            return;
        }

        const editor = window.ckeditorManager.editors.get(editorEl);
        if (!editor) {
            // Editor not ready yet, try again after a delay
            setTimeout(() => {
                this.updateGapsList(blockElement);
            }, 200);
            return;
        }

        try {
            const content = editor.getData();
            // Find all blank tokens like [[blank1]], [[blank2]], etc.
            const blankMatches = content.match(/\[\[blank(\d+)\]\]/g);
            
            const gapsList = blockElement.querySelector('[data-role="gf-gaps"]');
            if (!gapsList) {
                return;
            }

            // Remove empty state if gaps exist
            const emptyState = gapsList.querySelector('.gaps-empty-state');
            
            if (!blankMatches || blankMatches.length === 0) {
                // No gaps, show empty state
                if (!emptyState) {
                    gapsList.innerHTML = `
                        <div class="gaps-empty-state">
                            <i class="fas fa-info-circle"></i>
                            <p>برای این بلاک هنوز جای‌خالی تعریف نشده است</p>
                            <small>از دکمه "درج جای‌خالی" استفاده کنید</small>
                        </div>
                    `;
                } else {
                    emptyState.style.display = 'block';
                }
                return;
            }

            // Hide empty state
            if (emptyState) {
                emptyState.style.display = 'none';
            }

            // Extract unique gap indices
            const gapIndices = new Set();
            blankMatches.forEach(match => {
                const numMatch = match.match(/\d+/);
                if (numMatch) {
                    gapIndices.add(parseInt(numMatch[0]));
                }
            });

            // Sort indices
            const sortedIndices = Array.from(gapIndices).sort((a, b) => a - b);

            // Remove existing gap items
            const existingGaps = gapsList.querySelectorAll('.gap-item');
            existingGaps.forEach(gap => gap.remove());

            // Create gap items for each index
            sortedIndices.forEach(index => {
                const gapItem = this.createGapItem(index);
                gapsList.appendChild(gapItem);
            });

            // Setup event listeners for gap items
            this.setupGapItemListeners(blockElement);

        } catch (error) {
            console.error('GapFillHandler: Error updating gaps list:', error);
        }
    }

    createGapItem(index) {
        const gapItem = document.createElement('div');
        gapItem.className = 'gap-item';
        gapItem.dataset.gapIndex = index;
        
        gapItem.innerHTML = `
            <div class="gap-item-header">
                <span class="gap-item-number">جای‌خالی ${index}</span>
            </div>
            <div class="gap-item-body">
                <div class="gap-field-row">
                    <div class="gap-field">
                        <label>پاسخ صحیح</label>
                        <input type="text" class="gap-correct-input form-control" placeholder="پاسخ صحیح را وارد کنید" data-role="gf-correct" data-index="${index}">
                    </div>
                    <div class="gap-field">
                        <label>پاسخ‌های جایگزین (با کاما جدا کنید)</label>
                        <input type="text" class="gap-alts-input form-control" placeholder="پاسخ1، پاسخ2، ..." data-role="gf-alts" data-index="${index}">
                    </div>
                </div>
                <div class="gap-field">
                    <label>راهنما (اختیاری)</label>
                    <input type="text" class="gap-hint-input form-control" placeholder="راهنمایی برای دانش‌آموز" data-role="gf-hint" data-index="${index}">
                </div>
            </div>
        `;
        
        return gapItem;
    }

    setupGapItemListeners(blockElement) {
        // Setup input listeners for gap fields (only for inputs that don't have listeners yet)
        const gapInputs = blockElement.querySelectorAll('[data-role="gf-correct"], [data-role="gf-alts"], [data-role="gf-hint"]');
        gapInputs.forEach(input => {
            if (!input.dataset.listenerSetup) {
                input.dataset.listenerSetup = 'true';
                input.addEventListener('input', () => {
                    this.triggerContentUpdate(blockElement);
                });
            }
        });
    }

    async handleImageFileSelect(blockElement, fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

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
            const preview = blockElement.querySelector('[data-role="gf-image-preview"]');
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

            const response = await fetch('/FileUpload/UploadContentFile', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: formData
            });

            const result = await response.json();

            if (result.success && result.data) {
                const urlInput = blockElement.querySelector('[data-role="gf-image-url"]');
                const fileIdInput = blockElement.querySelector('[data-role="gf-image-fileid"]');
                
                const fileUrl = result.data.url || result.url || '';
                const fileId = result.data.id || result.fileId || '';
                const fileName = result.data.fileName || result.data.originalFileName || file.name;
                
                if (urlInput) urlInput.value = fileUrl;
                if (fileIdInput) fileIdInput.value = String(fileId);
                
                // Update preview
                if (preview && fileUrl) {
                    preview.innerHTML = `<img src="${fileUrl}" alt="Preview" style="max-width: 100%; max-height: 200px; border-radius: 4px;">`;
                }

                // Show remove button
                const removeBtn = blockElement.querySelector('[data-action="gf-remove-media"][data-media-type="image"]');
                if (removeBtn) removeBtn.style.display = 'inline-block';
                
                this.triggerContentUpdate(blockElement);
            } else {
                alert(result.message || 'خطا در آپلود تصویر');
            }
        } catch (error) {
            console.error('Error uploading image:', error);
            alert('خطا در آپلود تصویر');
        }
    }

    async handleAudioFileSelect(blockElement, fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

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

        await this._handleAudioFileUpload(blockElement, file, false);
    }

    async handleVideoFileSelect(blockElement, fileInput) {
        const file = fileInput.files[0];
        if (!file) return;

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
            const preview = blockElement.querySelector('[data-role="gf-video-preview"]');
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

            const response = await fetch('/FileUpload/UploadContentFile', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: formData
            });

            const result = await response.json();

            if (result.success && result.data) {
                const urlInput = blockElement.querySelector('[data-role="gf-video-url"]');
                const fileIdInput = blockElement.querySelector('[data-role="gf-video-fileid"]');
                
                const fileUrl = result.data.url || result.url || '';
                const fileId = result.data.id || result.fileId || '';
                const fileName = result.data.fileName || result.data.originalFileName || file.name;
                
                if (urlInput) urlInput.value = fileUrl;
                if (fileIdInput) fileIdInput.value = String(fileId);
                
                // Update preview
                if (preview && fileUrl) {
                    preview.innerHTML = `<video src="${fileUrl}" controls style="max-width: 100%; max-height: 300px; border-radius: 4px;"></video>`;
                }

                // Show remove button
                const removeBtn = blockElement.querySelector('[data-action="gf-remove-media"][data-media-type="video"]');
                if (removeBtn) removeBtn.style.display = 'inline-block';
                
                this.triggerContentUpdate(blockElement);
            } else {
                alert(result.message || 'خطا در آپلود ویدیو');
            }
        } catch (error) {
            console.error('Error uploading video:', error);
            alert('خطا در آپلود ویدیو');
        }
    }

    async _handleAudioFileUpload(blockElement, file, isRecorded = false) {
        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('type', 'audio');

            const response = await fetch('/FileUpload/UploadContentFile', {
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
                
                this.showAudioPreview(blockElement, fileUrl, fileName, isRecorded);
                
                const urlInput = blockElement.querySelector('[data-role="gf-audio-url"]');
                const fileIdInput = blockElement.querySelector('[data-role="gf-audio-fileid"]');
                const recordedInput = blockElement.querySelector('[data-role="gf-audio-recorded"]');
                
                if (urlInput) urlInput.value = fileUrl;
                if (fileIdInput) fileIdInput.value = String(fileId || '');
                if (recordedInput) recordedInput.value = isRecorded ? 'true' : 'false';
                
                this.triggerContentUpdate(blockElement);
            } else {
                alert(result.message || 'خطا در آپلود صوت');
            }
        } catch (error) {
            console.error('Error uploading audio:', error);
            alert('خطا در آپلود صوت');
        }
    }

    showAudioPreview(blockElement, fileUrl, fileName, isRecorded) {
        const preview = blockElement.querySelector('[data-role="gf-audio-preview"]');
        const audioPlayer = blockElement.querySelector('.gapfill-audio-player');
        const audioName = blockElement.querySelector('.gapfill-audio-name');
        
        if (preview) {
            preview.style.display = 'block';
        }
        
        if (audioPlayer) {
            audioPlayer.src = fileUrl;
        }
        
        if (audioName) {
            audioName.textContent = fileName || 'فایل صوتی';
        }
    }

    removeMedia(blockElement, mediaType) {
        // Reset media type selector
        const mediaTypeSelect = blockElement.querySelector('[data-role="gf-media-type"]');
        if (mediaTypeSelect) {
            mediaTypeSelect.value = '';
        }

        // Hide media content
        const mediaContent = blockElement.querySelector(`.gapfill-media-content[data-media-type="${mediaType}"]`);
        if (mediaContent) {
            mediaContent.style.display = 'none';
        }

        // Clear inputs and previews
        if (mediaType === 'image') {
            const urlInput = blockElement.querySelector('[data-role="gf-image-url"]');
            const fileIdInput = blockElement.querySelector('[data-role="gf-image-fileid"]');
            const preview = blockElement.querySelector('[data-role="gf-image-preview"]');
            const removeBtn = blockElement.querySelector('[data-action="gf-remove-media"][data-media-type="image"]');
            
            if (urlInput) urlInput.value = '';
            if (fileIdInput) fileIdInput.value = '';
            if (preview) {
                preview.innerHTML = '<div class="gapfill-image-placeholder"><i class="fas fa-image"></i><span>برای آپلود تصویر کلیک کنید</span></div>';
            }
            if (removeBtn) removeBtn.style.display = 'none';
        } else if (mediaType === 'audio') {
            const urlInput = blockElement.querySelector('[data-role="gf-audio-url"]');
            const fileIdInput = blockElement.querySelector('[data-role="gf-audio-fileid"]');
            const recordedInput = blockElement.querySelector('[data-role="gf-audio-recorded"]');
            const preview = blockElement.querySelector('[data-role="gf-audio-preview"]');
            const audioPlayer = blockElement.querySelector('.gapfill-audio-player');
            
            if (urlInput) urlInput.value = '';
            if (fileIdInput) fileIdInput.value = '';
            if (recordedInput) recordedInput.value = 'false';
            if (preview) preview.style.display = 'none';
            if (audioPlayer) audioPlayer.src = '';
        } else if (mediaType === 'video') {
            const urlInput = blockElement.querySelector('[data-role="gf-video-url"]');
            const fileIdInput = blockElement.querySelector('[data-role="gf-video-fileid"]');
            const preview = blockElement.querySelector('[data-role="gf-video-preview"]');
            const removeBtn = blockElement.querySelector('[data-action="gf-remove-media"][data-media-type="video"]');
            
            if (urlInput) urlInput.value = '';
            if (fileIdInput) fileIdInput.value = '';
            if (preview) {
                preview.innerHTML = '<div class="gapfill-video-placeholder"><i class="fas fa-video"></i><span>برای آپلود ویدیو کلیک کنید</span></div>';
            }
            if (removeBtn) removeBtn.style.display = 'none';
        }

        this.triggerContentUpdate(blockElement);
    }

    loadMedia(blockElement, data) {
        // Determine media type from mimeType or fileUrl
        let mediaType = '';
        if (data.fileUrl) {
            if (data.mimeType) {
                if (data.mimeType.startsWith('image/')) mediaType = 'image';
                else if (data.mimeType.startsWith('audio/')) mediaType = 'audio';
                else if (data.mimeType.startsWith('video/')) mediaType = 'video';
            } else {
                // Try to guess from file extension
                const ext = data.fileUrl.split('.').pop().toLowerCase();
                if (['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext)) mediaType = 'image';
                else if (['mp3', 'wav', 'ogg', 'm4a'].includes(ext)) mediaType = 'audio';
                else if (['mp4', 'webm', 'ogg'].includes(ext)) mediaType = 'video';
            }
        }

        if (mediaType) {
            // Set media type selector
            const mediaTypeSelect = blockElement.querySelector('[data-role="gf-media-type"]');
            if (mediaTypeSelect) {
                mediaTypeSelect.value = mediaType;
                this.changeMediaType(blockElement, mediaType);
            }

            // Load media data
            if (mediaType === 'image') {
                const urlInput = blockElement.querySelector('[data-role="gf-image-url"]');
                const fileIdInput = blockElement.querySelector('[data-role="gf-image-fileid"]');
                const preview = blockElement.querySelector('[data-role="gf-image-preview"]');
                const removeBtn = blockElement.querySelector('[data-action="gf-remove-media"][data-media-type="image"]');
                
                if (urlInput && data.fileUrl) urlInput.value = data.fileUrl;
                if (fileIdInput && data.fileId) fileIdInput.value = String(data.fileId);
                if (preview && data.fileUrl) {
                    preview.innerHTML = `<img src="${data.fileUrl}" alt="Preview" style="max-width: 100%; max-height: 200px; border-radius: 4px;">`;
                }
                if (removeBtn) removeBtn.style.display = 'inline-block';
            } else if (mediaType === 'audio') {
                const urlInput = blockElement.querySelector('[data-role="gf-audio-url"]');
                const fileIdInput = blockElement.querySelector('[data-role="gf-audio-fileid"]');
                const recordedInput = blockElement.querySelector('[data-role="gf-audio-recorded"]');
                
                if (urlInput && data.fileUrl) urlInput.value = data.fileUrl;
                if (fileIdInput && data.fileId) fileIdInput.value = String(data.fileId);
                if (recordedInput) recordedInput.value = data.isRecorded ? 'true' : 'false';
                
                this.showAudioPreview(blockElement, data.fileUrl, data.fileName, data.isRecorded);
            } else if (mediaType === 'video') {
                const urlInput = blockElement.querySelector('[data-role="gf-video-url"]');
                const fileIdInput = blockElement.querySelector('[data-role="gf-video-fileid"]');
                const preview = blockElement.querySelector('[data-role="gf-video-preview"]');
                const removeBtn = blockElement.querySelector('[data-action="gf-remove-media"][data-media-type="video"]');
                
                if (urlInput && data.fileUrl) urlInput.value = data.fileUrl;
                if (fileIdInput && data.fileId) fileIdInput.value = String(data.fileId);
                if (preview && data.fileUrl) {
                    preview.innerHTML = `<video src="${data.fileUrl}" controls style="max-width: 100%; max-height: 300px; border-radius: 4px;"></video>`;
                }
                if (removeBtn) removeBtn.style.display = 'inline-block';
            }
        }

        // Load showOptions setting
        const showOptionsCheckbox = blockElement.querySelector('[data-role="gf-show-options"]');
        if (showOptionsCheckbox && data.showOptions !== undefined) {
            showOptionsCheckbox.checked = data.showOptions;
        }
    }

    collectData(blockElement, block) {
        const data = block.data || {};
        
        // Collect text content from CKEditor
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (editorEl && window.ckeditorManager) {
            const editor = window.ckeditorManager.editors.get(editorEl);
            if (editor) {
                const content = editor.getData();
                data.content = content;
                data.textContent = content.replace(/<[^>]*>/g, ''); // Strip HTML tags
            }
        }

        // Collect media data
        const mediaTypeSelect = blockElement.querySelector('[data-role="gf-media-type"]');
        const mediaType = mediaTypeSelect ? mediaTypeSelect.value : '';
        
        if (mediaType === 'image') {
            const urlInput = blockElement.querySelector('[data-role="gf-image-url"]');
            const fileIdInput = blockElement.querySelector('[data-role="gf-image-fileid"]');
            
            if (urlInput && urlInput.value) {
                data.fileUrl = urlInput.value;
                data.mimeType = 'image/' + urlInput.value.split('.').pop().toLowerCase();
            }
            if (fileIdInput && fileIdInput.value) {
                data.fileId = fileIdInput.value;
            }
        } else if (mediaType === 'audio') {
            const urlInput = blockElement.querySelector('[data-role="gf-audio-url"]');
            const fileIdInput = blockElement.querySelector('[data-role="gf-audio-fileid"]');
            const recordedInput = blockElement.querySelector('[data-role="gf-audio-recorded"]');
            
            if (urlInput && urlInput.value) {
                data.fileUrl = urlInput.value;
                data.mimeType = 'audio/' + urlInput.value.split('.').pop().toLowerCase();
            }
            if (fileIdInput && fileIdInput.value) {
                data.fileId = fileIdInput.value;
            }
            if (recordedInput) {
                data.isRecorded = recordedInput.value === 'true';
            }
        } else if (mediaType === 'video') {
            const urlInput = blockElement.querySelector('[data-role="gf-video-url"]');
            const fileIdInput = blockElement.querySelector('[data-role="gf-video-fileid"]');
            
            if (urlInput && urlInput.value) {
                data.fileUrl = urlInput.value;
                data.mimeType = 'video/' + urlInput.value.split('.').pop().toLowerCase();
            }
            if (fileIdInput && fileIdInput.value) {
                data.fileId = fileIdInput.value;
            }
        } else {
            // Clear media data if no media selected
            delete data.fileId;
            delete data.fileUrl;
            delete data.mimeType;
            delete data.isRecorded;
        }

        // Collect settings
        const answerTypeSelect = blockElement.querySelector('[data-role="gf-answer-type"]');
        if (answerTypeSelect) {
            data.answerType = answerTypeSelect.value;
        }

        const caseCheckbox = blockElement.querySelector('[data-role="gf-case"]');
        if (caseCheckbox) {
            data.caseSensitive = caseCheckbox.checked;
        }

        const showOptionsCheckbox = blockElement.querySelector('[data-role="gf-show-options"]');
        if (showOptionsCheckbox) {
            data.showOptions = showOptionsCheckbox.checked;
        }

        // Collect gaps from input fields
        const gaps = [];
        const gapItems = blockElement.querySelectorAll('.gap-item');
        
        gapItems.forEach(gapItem => {
            const gapIndex = parseInt(gapItem.dataset.gapIndex) || 0;
            const correctInput = gapItem.querySelector('[data-role="gf-correct"]');
            const altsInput = gapItem.querySelector('[data-role="gf-alts"]');
            const hintInput = gapItem.querySelector('[data-role="gf-hint"]');
            
            const correctAnswer = correctInput ? correctInput.value.trim() : '';
            const altsText = altsInput ? altsInput.value.trim() : '';
            const hint = hintInput ? hintInput.value.trim() : '';
            
            // Parse alternative answers (comma-separated)
            const alternativeAnswers = altsText
                ? altsText.split(',').map(a => a.trim()).filter(a => a.length > 0)
                : [];
            
            if (gapIndex > 0) {
                gaps.push({
                    index: gapIndex,
                    correctAnswer: correctAnswer,
                    alternativeAnswers: alternativeAnswers,
                    hint: hint
                });
            }
        });
        
        // Sort gaps by index
        gaps.sort((a, b) => a.index - b.index);
        data.gaps = gaps;

        return data;
    }

    async startRecording(blockElement) {
        if (this.isRecording) {
            alert('در حال ضبط صدا هستید. لطفاً ابتدا ضبط قبلی را متوقف کنید.');
            return;
        }

        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            const mediaRecorder = new MediaRecorder(stream);
            const chunks = [];
            
            this.isRecording = true;
            this.currentRecordingBlock = blockElement;
            this.mediaRecorder = mediaRecorder;
            this.audioChunks = chunks;

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
                await this._handleAudioFileUpload(blockElement, file, true);
                
                // Cleanup
                this.isRecording = false;
                this.currentRecordingBlock = null;
                this.mediaRecorder = null;
                this.audioChunks = [];
                
                // Update UI
                this.updateRecordingUI(blockElement, false);
            };

            mediaRecorder.start();
            this.updateRecordingUI(blockElement, true);

        } catch (error) {
            console.error('Error starting recording:', error);
            alert('خطا در شروع ضبط صدا. لطفاً دسترسی میکروفون را بررسی کنید.');
            this.isRecording = false;
            this.currentRecordingBlock = null;
        }
    }

    stopRecording() {
        if (!this.isRecording || !this.mediaRecorder) return;

        if (this.mediaRecorder.state !== 'inactive') {
            this.mediaRecorder.stop();
        }
    }

    updateRecordingUI(blockElement, isRecording) {
        if (!blockElement) return;

        const recordBtn = blockElement.querySelector('[data-action="gf-record-audio"]');
        const stopBtn = blockElement.querySelector('[data-action="gf-stop-recording"]');
        const uploadBtn = blockElement.querySelector('[data-action="gf-upload-audio"]');

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
}

if (typeof window !== 'undefined') {
    window.GapFillHandler = GapFillHandler;
}
