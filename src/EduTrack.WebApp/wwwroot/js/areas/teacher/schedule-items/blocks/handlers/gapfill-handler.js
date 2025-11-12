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

    generateOptionId() {
        if (window.crypto && typeof window.crypto.randomUUID === 'function') {
            return window.crypto.randomUUID();
        }

        return `gf-opt-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;
    }

    generateResponseGroupName() {
        if (window.crypto && typeof window.crypto.randomUUID === 'function') {
            return `gf-response-${window.crypto.randomUUID()}`;
        }

        return `gf-response-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
    }

    normalizeOptionArray(options) {
        if (!Array.isArray(options)) {
            return [];
        }

        return options
            .map(option => {
                if (!option) {
                    return null;
                }

                if (typeof option === 'string') {
                    const value = option.trim();
                    if (!value) {
                        return null;
                    }
                    return {
                        id: this.generateOptionId(),
                        value: value,
                        displayText: value
                    };
                }

                const value = (option.value || option.Value || option.text || option.Text || option.label || option.Label || '').toString().trim();
                if (!value) {
                    return null;
                }

                return {
                    id: option.id || option.Id || this.generateOptionId(),
                    value: value,
                    displayText: option.displayText || option.DisplayText || option.label || option.Label || value
                };
            })
            .filter(option => option !== null);
    }

    parseOptionsInput(rawInput, previousOptions = []) {
        if (!rawInput) {
            return [];
        }

        const values = rawInput
            .split(/\r?\n/)
            .map(line => line.trim())
            .filter(line => line.length > 0);

        const previous = this.normalizeOptionArray(previousOptions);
        const previousLookup = new Map();
        previous.forEach(option => {
            if (!option || !option.value) {
                return;
            }
            previousLookup.set(option.value.toLowerCase(), option);
        });

        const seen = new Set();
        const options = [];

        values.forEach(value => {
            const key = value.toLowerCase();
            if (seen.has(key)) {
                return;
            }

            seen.add(key);
            const existing = previousLookup.get(key);

            if (existing) {
                options.push({
                    id: existing.id || this.generateOptionId(),
                    value: existing.value || value,
                    displayText: existing.displayText || existing.value || value
                });
            } else {
                options.push({
                    id: this.generateOptionId(),
                    value: value,
                    displayText: value
                });
            }
        });

        return options;
    }

    serializeOptions(options) {
        if (!Array.isArray(options)) {
            return '';
        }

        const normalized = this.normalizeOptionArray(options);
        const seen = new Set();
        const values = [];

        normalized.forEach(option => {
            if (!option || !option.value) {
                return;
            }
            const key = option.value.toLowerCase();
            if (seen.has(key)) {
                return;
            }
            seen.add(key);
            values.push(option.value);
        });

        return values.join('\n');
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

        // Note: Data loading is handled by loadData method which is called by populateBlockContent
        // We don't load data here in initialize to avoid duplicate loading
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
            const handleShowOptionsChange = () => {
                this.toggleGlobalOptions(blockElement, showOptionsCheckbox.checked);
                this.triggerContentUpdate(blockElement);
            };

            showOptionsCheckbox.addEventListener('change', handleShowOptionsChange);
            // Apply initial state
            handleShowOptionsChange();
        } else {
            this.toggleGlobalOptions(blockElement, true);
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

        const globalOptionsInput = blockElement.querySelector('[data-role="gf-global-options"]');
        if (globalOptionsInput) {
            globalOptionsInput.addEventListener('input', () => {
                this.triggerContentUpdate(blockElement);
            });
        }
    }

    toggleGlobalOptions(blockElement, isEnabled) {
        const container = blockElement.querySelector('[data-role="gf-global-options-container"]');
        if (container) {
            container.style.display = isEnabled ? '' : 'none';
            const textarea = container.querySelector('[data-role="gf-global-options"]');
            if (textarea) {
                textarea.disabled = !isEnabled;
            }
        }

        const gapItems = blockElement.querySelectorAll('.gap-item');
        gapItems.forEach(gapItem => {
            ['global', 'custom'].forEach(mode => {
                const radio = gapItem.querySelector(`[data-role="gf-response-mode"][value="${mode}"]`);
                if (!radio) {
                    return;
                }

                radio.disabled = !isEnabled;
                if (!isEnabled && radio.checked) {
                    this.setResponseMode(gapItem, 'manual', blockElement, true);
                }
            });

            if (!isEnabled) {
                this.applyResponseModeState(gapItem, 'manual');
            } else {
                const currentMode = this.getSelectedResponseMode(gapItem);
                this.applyResponseModeState(gapItem, currentMode);
            }
        });
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

    async loadData(blockElement, block) {
        if (!blockElement) {
            console.warn('GapFillHandler: loadData called with invalid blockElement');
            return;
        }

        // Get block data from block parameter or from dataset
        let blockData = block?.data;
        if (!blockData && blockElement.dataset.blockData) {
            try {
                blockData = JSON.parse(blockElement.dataset.blockData);
                console.log('GapFillHandler: Loaded block data from dataset');
            } catch (e) {
                console.warn('GapFillHandler: Failed to parse block data from dataset', e);
            }
        }

        // If still no block, try to construct it from element
        if (!block && blockElement.dataset.blockId) {
            block = {
                id: blockElement.dataset.blockId,
                type: blockElement.dataset.type || 'gapFill',
                data: blockData
            };
        }

        console.log('GapFillHandler: loadData called', {
            blockId: block?.id || blockElement.dataset.blockId,
            blockType: block?.type || 'gapFill',
            hasData: !!blockData,
            hasContent: !!(blockData && blockData.content),
            hasGaps: !!(blockData && blockData.gaps),
            gapsCount: blockData?.gaps?.length || 0
        });

        if (!blockData) {
            console.warn('GapFillHandler: No block data to load');
            return;
        }

        // Load CKEditor content
        const editorEl = blockElement.querySelector('.ckeditor-editor');
        if (editorEl && blockData.content) {
            // Wait for CKEditor to be initialized
            let attempts = 0;
            const maxAttempts = 30;
            const loadContent = async () => {
                attempts++;
                if (window.ckeditorManager) {
                    const editor = window.ckeditorManager.editors.get(editorEl);
                    if (editor) {
                        try {
                            editor.setData(blockData.content || '');
                            console.log('GapFillHandler: CKEditor content loaded');
                            
                            // Update gaps list after content is loaded
                            setTimeout(() => {
                                this.updateGapsList(blockElement);
                                this.populateGapsData(blockElement, blockData);
                            }, 300);
                        } catch (error) {
                            console.error('GapFillHandler: Error setting CKEditor data:', error);
                        }
                    } else if (attempts < maxAttempts) {
                        setTimeout(loadContent, 100);
                    }
                } else if (attempts < maxAttempts) {
                    setTimeout(loadContent, 100);
                }
            };
            
            loadContent();
        }

        // Load media
        if (blockData.fileUrl || blockData.fileId) {
            this.loadMedia(blockElement, blockData);
        }

        // Load settings
        this.loadSettings(blockElement, blockData);

        // Load gaps data (will be populated after gaps list is updated)
        if (blockData.gaps && Array.isArray(blockData.gaps) && blockData.gaps.length > 0) {
            // This will be called after updateGapsList
            setTimeout(() => {
                this.populateGapsData(blockElement, blockData);
            }, 500);
        }
    }

    loadSettings(blockElement, data) {
        // Load answer type
        const answerTypeSelect = blockElement.querySelector('[data-role="gf-answer-type"]');
        if (answerTypeSelect && data.answerType) {
            answerTypeSelect.value = data.answerType;
        }

        // Load case sensitive
        const caseCheckbox = blockElement.querySelector('[data-role="gf-case"]');
        if (caseCheckbox) {
            caseCheckbox.checked = data.caseSensitive === true;
        }

        // Load show options
        const showOptionsCheckbox = blockElement.querySelector('[data-role="gf-show-options"]');
        if (showOptionsCheckbox) {
            showOptionsCheckbox.checked = data.showOptions !== false; // Default to true
            this.toggleGlobalOptions(blockElement, showOptionsCheckbox.checked);
        } else {
            this.toggleGlobalOptions(blockElement, data.showOptions !== false);
        }

        // Load points
        const pointsSelect = blockElement.querySelector('[data-setting="points"]');
        if (pointsSelect && data.points) {
            pointsSelect.value = String(data.points);
        }

        // Load isRequired
        const isRequiredCheckbox = blockElement.querySelector('[data-setting="isRequired"]');
        if (isRequiredCheckbox) {
            isRequiredCheckbox.checked = data.isRequired !== false; // Default to true
        }
    }

    populateGapsData(blockElement, blockData) {
        if (!blockData) {
            return;
        }

        const blanksArray = Array.isArray(blockData.blanks) ? blockData.blanks : [];
        const gapsArray = Array.isArray(blockData.gaps) ? blockData.gaps : [];

        const blanksByIndex = new Map();
        blanksArray.forEach(blank => {
            if (!blank) {
                return;
            }

            let index = parseInt(blank.index ?? blank.order ?? blank.number ?? blank.blankIndex, 10);
            if (!Number.isFinite(index) || index <= 0) {
                const idDigits = (blank.id || blank.Id || '').toString().match(/\d+/);
                if (idDigits && idDigits.length > 0) {
                    index = parseInt(idDigits[0], 10);
                }
            }

            if (!Number.isFinite(index) || index <= 0) {
                return;
            }

            blanksByIndex.set(index, blank);
        });

        const gapsByIndex = new Map();
        gapsArray.forEach(gap => {
            if (!gap) {
                return;
            }
            const index = parseInt(gap.index ?? gap.order ?? gap.number, 10);
            if (Number.isFinite(index) && index > 0) {
                gapsByIndex.set(index, gap);
            }
        });

        const showOptionsCheckbox = blockElement.querySelector('[data-role="gf-show-options"]');
        const showOptionsDefault = showOptionsCheckbox ? showOptionsCheckbox.checked : !!blockData.showOptions;

        blanksByIndex.forEach((blank, index) => {
            const gapItem = blockElement.querySelector(`.gap-item[data-gap-index="${index}"]`);
            if (gapItem) {
                const correctInput = gapItem.querySelector('[data-role="gf-correct"]');
                const altsInput = gapItem.querySelector('[data-role="gf-alts"]');
                const hintInput = gapItem.querySelector('[data-role="gf-hint"]');

                const legacyGap = gapsByIndex.get(index);

                const correctAnswer = blank.correctAnswer ?? legacyGap?.correctAnswer ?? '';
                if (correctInput) {
                    correctInput.value = correctAnswer || '';
                }

                const altAnswers = Array.isArray(blank.alternativeAnswers)
                    ? blank.alternativeAnswers
                    : Array.isArray(legacyGap?.alternativeAnswers)
                        ? legacyGap.alternativeAnswers
                        : typeof legacyGap?.alternativeAnswers === 'string'
                            ? legacyGap.alternativeAnswers.split('،')
                            : [];

                if (altsInput) {
                    altsInput.value = Array.isArray(altAnswers) && altAnswers.length > 0
                        ? altAnswers.join('، ')
                        : '';
                }

                if (hintInput) {
                    const hint = blank.hint ?? legacyGap?.hint ?? '';
                    hintInput.value = hint || '';
                }

                const blankOptionsInput = gapItem.querySelector('[data-role="gf-blank-options"]');

                const normalizedOptions = this.normalizeOptionArray(blank.options || blank.Options || []);
                if (blankOptionsInput) {
                    blankOptionsInput.value = this.serializeOptions(normalizedOptions);
                }

                const responseMode = this.resolveResponseMode(blank, showOptionsDefault, normalizedOptions);
                this.setResponseMode(gapItem, responseMode, blockElement, true);

                if (blank.id || blank.Id) {
                    gapItem.dataset.gapId = blank.id || blank.Id;
                }
            }
        });

        gapsByIndex.forEach((gap, index) => {
            if (blanksByIndex.has(index)) {
                return;
            }

            const gapItem = blockElement.querySelector(`.gap-item[data-gap-index="${index}"]`);
            if (!gapItem) {
                return;
            }

            const correctInput = gapItem.querySelector('[data-role="gf-correct"]');
            const altsInput = gapItem.querySelector('[data-role="gf-alts"]');
            const hintInput = gapItem.querySelector('[data-role="gf-hint"]');
            const blankOptionsInput = gapItem.querySelector('[data-role="gf-blank-options"]');
            if (correctInput && gap.correctAnswer) {
                correctInput.value = gap.correctAnswer;
            }
            if (altsInput && gap.alternativeAnswers) {
                const altsValue = Array.isArray(gap.alternativeAnswers)
                    ? gap.alternativeAnswers.join('، ')
                    : gap.alternativeAnswers;
                altsInput.value = altsValue;
            }
            if (hintInput && gap.hint) {
                hintInput.value = gap.hint;
            }

            const legacyOptions = gap.options || gap.Options;
            if (blankOptionsInput && legacyOptions) {
                const normalizedLegacyOptions = this.normalizeOptionArray(legacyOptions);
                blankOptionsInput.value = this.serializeOptions(normalizedLegacyOptions);
            }

            this.setResponseMode(gapItem, 'manual', blockElement, true);
        });

        const globalOptionsInput = blockElement.querySelector('[data-role="gf-global-options"]');
        if (globalOptionsInput) {
            const globalOptions = this.normalizeOptionArray(blockData.globalOptions || blockData.options || []);
            globalOptionsInput.value = this.serializeOptions(globalOptions);
        }
    }

    renderGaps(blockElement, block) {
        // This method is called when loading existing data
        // First, update gaps list from content (to show all gaps found in text)
        this.updateGapsList(blockElement);
        
        // Then, if we have saved gap data, populate the input fields
        if (block && block.data && block.data.gaps && Array.isArray(block.data.gaps)) {
            setTimeout(() => {
                this.populateGapsData(blockElement, block.data);
            }, 500);
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

            const showOptionsCheckbox = blockElement.querySelector('[data-role="gf-show-options"]');

            // Handle empty state
            if (!blankMatches || blankMatches.length === 0) {
                gapsList.innerHTML = `
                    <div class="gaps-empty-state">
                        <i class="fas fa-info-circle"></i>
                        <p>هنوز هیچ جای‌خالی‌ای ایجاد نشده است.</p>
                        <small>از دکمه «جای‌خالی جدید» برای شروع استفاده کنید.</small>
                    </div>
                `;
                return;
            }

            const existingItems = new Map();
            gapsList.querySelectorAll('.gap-item').forEach(item => {
                const itemIndex = parseInt(item.dataset.gapIndex, 10);
                if (Number.isFinite(itemIndex)) {
                    existingItems.set(itemIndex, item);
                }
            });

            // Extract unique gap indices and sort them
            const gapIndices = new Set();
            blankMatches.forEach(match => {
                const numMatch = match.match(/\d+/);
                if (numMatch) {
                    gapIndices.add(parseInt(numMatch[0], 10));
                }
            });

            const sortedIndices = Array.from(gapIndices).sort((a, b) => a - b);
            const fragment = document.createDocumentFragment();

            sortedIndices.forEach(index => {
                let gapItem = existingItems.get(index);

                if (gapItem) {
                    existingItems.delete(index);
                } else {
                    gapItem = this.createGapItem(index);
                    this.setupGapItemListeners(blockElement, gapItem);
                }

                gapItem.dataset.gapIndex = index;
                const numberEl = gapItem.querySelector('.gap-item-number');
                if (numberEl) {
                    numberEl.textContent = `جای‌خالی ${index}`;
                }
                const tokenEl = gapItem.querySelector('.gap-item-token');
                if (tokenEl) {
                    tokenEl.textContent = `[[blank${index}]]`;
                }

                fragment.appendChild(gapItem);
            });

            // Remove leftover items not referenced anymore
            existingItems.forEach(item => item.remove());

            gapsList.innerHTML = '';
            gapsList.appendChild(fragment);

            // Ensure listeners exist for all items (existing listeners are guarded)
            this.setupGapItemListeners(blockElement);
            const allowOptions = showOptionsCheckbox ? showOptionsCheckbox.checked : true;
            this.toggleGlobalOptions(blockElement, allowOptions);

        } catch (error) {
            console.error('GapFillHandler: Error updating gaps list:', error);
        }
    }

    createGapItem(index) {
        const gapItem = document.createElement('div');
        gapItem.className = 'gap-item';
        gapItem.dataset.gapIndex = index;
        gapItem.dataset.gapId = `blank${index}`;

        const responseGroup = this.generateResponseGroupName();

        gapItem.innerHTML = `
            <div class="gap-item-header">
                <span class="gap-item-number">جای‌خالی ${index}</span>
                <span class="gap-item-token">[[blank${index}]]</span>
            </div>
            <div class="gap-item-body">
                <div class="gap-field-row">
                    <div class="gap-field">
                        <label>پاسخ صحیح</label>
                        <input type="text" class="gap-correct-input" placeholder="پاسخ صحیح را وارد کنید" data-role="gf-correct" data-index="${index}">
                    </div>
                    <div class="gap-field">
                        <label>پاسخ‌های جایگزین (با کاما جدا کنید)</label>
                        <input type="text" class="gap-alts-input" placeholder="پاسخ۱، پاسخ۲، ..." data-role="gf-alts" data-index="${index}">
                    </div>
                </div>
                <div class="gap-field">
                    <label>راهنما (اختیاری)</label>
                    <input type="text" class="gap-hint-input" placeholder="راهنمایی برای دانش‌آموز" data-role="gf-hint" data-index="${index}">
                </div>
                <div class="gap-response-section" data-role="gf-response-section">
                    <span class="gap-response-title">نحوه پاسخ‌دهی</span>
                    <div class="gap-response-options">
                        <label class="gap-radio">
                            <input type="radio" name="${responseGroup}" value="manual" data-role="gf-response-mode" checked>
                            <span>پاسخ تایپی (بدون گزینه)</span>
                        </label>
                        <label class="gap-radio">
                            <input type="radio" name="${responseGroup}" value="global" data-role="gf-response-mode">
                            <span>انتخاب از گزینه‌های عمومی</span>
                        </label>
                        <label class="gap-radio">
                            <input type="radio" name="${responseGroup}" value="custom" data-role="gf-response-mode">
                            <span>انتخاب از پیشنهادهای اختصاصی</span>
                        </label>
                    </div>
                </div>
                <div class="gap-blank-options" data-role="gf-blank-options-container" hidden>
                    <label>پیشنهادهای اختصاصی برای این جای‌خالی (هر خط یک گزینه)</label>
                    <textarea class="gap-blank-options-input" data-role="gf-blank-options" rows="3" placeholder="گزینه۱&#10;گزینه۲"></textarea>
                    <small class="gap-blank-options-hint">این گزینه‌ها فقط در صورت انتخاب «پیشنهادهای اختصاصی» نمایش داده می‌شوند.</small>
                </div>
            </div>
        `;

        return gapItem;
    }

    setupGapItemListeners(blockElement, specificGapItem = null) {
        const scope = specificGapItem || blockElement;

        const textInputs = scope.querySelectorAll('[data-role="gf-correct"], [data-role="gf-alts"], [data-role="gf-hint"], [data-role="gf-blank-options"]');
        textInputs.forEach(input => {
            if (!input.dataset.listenerSetup) {
                input.dataset.listenerSetup = 'true';
                input.addEventListener('input', () => {
                    this.triggerContentUpdate(blockElement);
                });
            }
        });

        const responseRadios = scope.querySelectorAll('[data-role="gf-response-mode"]');
        responseRadios.forEach(radio => {
            if (!radio.dataset.listenerSetup) {
                radio.dataset.listenerSetup = 'true';
                radio.addEventListener('change', () => {
                    if (!radio.checked) {
                        return;
                    }

                    const gapItem = radio.closest('.gap-item');
                    if (gapItem) {
                        this.handleResponseModeChange(blockElement, gapItem, radio.value);
                    }
                });
            }
        });

        const initializeState = (gapItem) => {
            const mode = this.getSelectedResponseMode(gapItem);
            this.setResponseMode(gapItem, mode, blockElement, true);
        };

        if (specificGapItem) {
            initializeState(specificGapItem);
        } else {
            blockElement.querySelectorAll('.gap-item').forEach(item => initializeState(item));
        }
    }

    getSelectedResponseMode(gapItem) {
        if (!gapItem) {
            return 'manual';
        }

        const checkedRadio = gapItem.querySelector('[data-role="gf-response-mode"]:checked');
        if (checkedRadio && !checkedRadio.disabled) {
            return checkedRadio.value;
        }

        return gapItem.dataset.responseMode || 'manual';
    }

    applyResponseModeState(gapItem, mode) {
        if (!gapItem) {
            return;
        }

        gapItem.dataset.responseMode = mode;

        const optionsContainer = gapItem.querySelector('[data-role="gf-blank-options-container"]');
        if (optionsContainer) {
            const textarea = optionsContainer.querySelector('[data-role="gf-blank-options"]');
            if (mode === 'custom') {
                optionsContainer.hidden = false;
                if (textarea) {
                    textarea.disabled = false;
                }
            } else {
                optionsContainer.hidden = true;
                if (textarea) {
                    textarea.disabled = true;
                }
            }
        }
    }

    setResponseMode(gapItem, mode, blockElement, suppressUpdate = false) {
        if (!gapItem) {
            return;
        }

        let resolvedMode = mode;
        const targetRadio = gapItem.querySelector(`[data-role="gf-response-mode"][value="${mode}"]`);
        if (!targetRadio || targetRadio.disabled) {
            resolvedMode = 'manual';
        }

        const radios = gapItem.querySelectorAll('[data-role="gf-response-mode"]');
        radios.forEach(radio => {
            radio.checked = radio.value === resolvedMode;
        });

        this.applyResponseModeState(gapItem, resolvedMode);

        if (!suppressUpdate) {
            this.triggerContentUpdate(blockElement);
        }
    }

    handleResponseModeChange(blockElement, gapItem, mode) {
        this.setResponseMode(gapItem, mode, blockElement, false);
    }

    resolveResponseMode(blank, showOptionsEnabled, normalizedOptions) {
        const hasOptions = Array.isArray(normalizedOptions) && normalizedOptions.length > 0;
        const allowManual = typeof blank?.allowManualInput === 'boolean' ? blank.allowManualInput : true;
        const allowGlobal = typeof blank?.allowGlobalOptions === 'boolean'
            ? blank.allowGlobalOptions
            : showOptionsEnabled;
        const allowCustom = typeof blank?.allowBlankOptions === 'boolean'
            ? blank.allowBlankOptions
            : hasOptions;

        if (!showOptionsEnabled) {
            return allowManual ? 'manual' : 'manual';
        }

        if (allowCustom && hasOptions) {
            return 'custom';
        }

        if (allowGlobal) {
            return 'global';
        }

        return allowManual ? 'manual' : 'manual';
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
        const showOptions = showOptionsCheckbox ? showOptionsCheckbox.checked : !!data.showOptions;
        data.showOptions = showOptions;
        data.showGlobalOptions = showOptions;

        const globalOptionsInput = blockElement.querySelector('[data-role="gf-global-options"]');
        const previousGlobalOptions = Array.isArray(data.globalOptions) ? data.globalOptions : [];
        data.globalOptions = this.parseOptionsInput(globalOptionsInput ? globalOptionsInput.value : '', previousGlobalOptions);

        const previousBlanks = Array.isArray(data.blanks) ? data.blanks : [];
        const previousBlanksByIndex = new Map();
        previousBlanks.forEach(blank => {
            if (!blank) {
                return;
            }

            let index = parseInt(blank.index ?? blank.order ?? blank.number ?? blank.blankIndex, 10);
            if (!Number.isFinite(index) || index <= 0) {
                const idDigits = (blank.id || blank.Id || '').toString().match(/\d+/);
                if (idDigits && idDigits.length > 0) {
                    index = parseInt(idDigits[0], 10);
                }
            }

            if (!Number.isFinite(index) || index <= 0) {
                return;
            }

            previousBlanksByIndex.set(index, blank);
        });

        const blanks = [];
        const gapsLegacy = [];
        const gapItems = blockElement.querySelectorAll('.gap-item');
        
        gapItems.forEach(gapItem => {
            const gapIndex = parseInt(gapItem.dataset.gapIndex) || 0;
            const correctInput = gapItem.querySelector('[data-role="gf-correct"]');
            const altsInput = gapItem.querySelector('[data-role="gf-alts"]');
            const hintInput = gapItem.querySelector('[data-role="gf-hint"]');
            const blankOptionsInput = gapItem.querySelector('[data-role="gf-blank-options"]');
            
            const correctAnswer = correctInput ? correctInput.value.trim() : '';
            const altsText = altsInput ? altsInput.value.trim() : '';
            const hint = hintInput ? hintInput.value.trim() : '';
            
            // Parse alternative answers (comma-separated)
            const alternativeAnswers = altsText
                ? altsText.split(/[،,]/).map(a => a.trim()).filter(a => a.length > 0)
                : [];
            const previousBlank = previousBlanksByIndex.get(gapIndex) || {};
            const previousOptions = previousBlank.options || previousBlank.Options || [];
            const blankOptions = this.parseOptionsInput(blankOptionsInput ? blankOptionsInput.value : '', previousOptions);
            const responseMode = this.getSelectedResponseMode(gapItem);
            const allowManual = responseMode === 'manual';
            const allowGlobal = responseMode === 'global';
            const allowBlank = responseMode === 'custom';
            const blankId = gapItem.dataset.gapId || previousBlank.id || previousBlank.Id || `blank${gapIndex}`;
            
            if (gapIndex > 0) {
                blanks.push({
                    id: blankId,
                    index: gapIndex,
                    correctAnswer: correctAnswer,
                    alternativeAnswers: alternativeAnswers,
                    hint: hint,
                    allowManualInput: allowManual,
                    allowGlobalOptions: allowGlobal,
                    allowBlankOptions: allowBlank,
                    options: blankOptions,
                    correctOptionId: previousBlank.correctOptionId || previousBlank.CorrectOptionId || null,
                    alternativeOptionIds: Array.isArray(previousBlank.alternativeOptionIds || previousBlank.AlternativeOptionIds)
                        ? (previousBlank.alternativeOptionIds || previousBlank.AlternativeOptionIds)
                        : []
                });

                gapsLegacy.push({
                    index: gapIndex,
                    correctAnswer: correctAnswer,
                    alternativeAnswers: alternativeAnswers,
                    hint: hint
                });
            }
        });
        
        blanks.sort((a, b) => a.index - b.index);
        gapsLegacy.sort((a, b) => a.index - b.index);

        data.blanks = blanks;
        data.gaps = gapsLegacy;

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
