/**
 * Content Builder Base Class
 * Manages common functionality for content blocks across different content types
 * Each block type has its own specific manager (text-block.js, image-block.js, etc.)
 */

class ContentBuilderBase {
    constructor(config = {}) {
        this.config = {
            containerId: config.containerId || 'contentBlocksList',
            emptyStateId: config.emptyStateId || 'emptyBlocksState',
            previewId: config.previewId || 'contentPreview',
            hiddenFieldId: config.hiddenFieldId || 'contentJson',
            modalId: config.modalId || 'blockTypeModal',
            contentType: config.contentType || 'content',
            ...config
        };

        this.blocks = [];
        this.nextBlockId = 1;
        this.isLoadingExistingContent = false;
        
        // Store pending files separately (File objects can't be JSON stringified)
        this.pendingFiles = new Map(); // blockId -> File object
        
        // Initialize shared managers
        this.fieldManager = new FieldManager();
        this.eventManager = new EventManager();
        this.previewManager = new PreviewManager();
        this.syncManager = new ContentSyncManager(this.fieldManager, this.eventManager);
        
        // DOM elements
        this.blocksList = document.getElementById(this.config.containerId);
        this.emptyState = document.getElementById(this.config.emptyStateId);
        this.preview = document.getElementById(this.config.previewId); // Optional - preview uses modal instead
        this.hiddenField = document.getElementById(this.config.hiddenFieldId);
        
        this.init();
    }

    init() {
        if (!this.blocksList || !this.emptyState || !this.hiddenField) {
            console.error('Required elements not found!', {
                blocksList: !!this.blocksList,
                emptyState: !!this.emptyState,
                preview: !!this.preview,
                hiddenField: !!this.hiddenField
            });
            return;
        }
        
        this.setupFieldManager();
        this.setupEventListeners();
        this.setupSyncManager();
        this.loadExistingContent();
    }
    
    // Setup global document listeners only once
    static setupGlobalDocumentListeners() {
        if (window._contentBuilderGlobalListenersSetup) {
            return;
        }
        
        window._contentBuilderGlobalListenersSetup = true;
        
        // Setup global blockContentChanged listener
        document.addEventListener('blockContentChanged', (e) => {
            // Validate event detail
            if (!e.detail) {
                return;
            }
            
            const blockElement = e.detail.blockElement;
            
            // Validate blockElement exists and has required properties
            if (!blockElement || !blockElement.dataset || !blockElement.dataset.blockId) {
                return;
            }
            
            const blockId = blockElement.dataset.blockId;
            
            // Forward to all active content builders that own this block
            if (window.reminderBlockManager && window.reminderBlockManager.blocks) {
                const block = window.reminderBlockManager.blocks.find(b => b.id === blockId);
                if (block) {
                    window.reminderBlockManager.handleBlockContentChanged(e.detail);
                }
            }
            if (window.writtenBlockManager && window.writtenBlockManager.blocks) {
                const block = window.writtenBlockManager.blocks.find(b => b.id === blockId);
                if (block) {
                    window.writtenBlockManager.handleBlockContentChanged(e.detail);
                }
            }
            if (window.gapFillContentManager && window.gapFillContentManager.blocks) {
                const block = window.gapFillContentManager.blocks.find(b => b.id === blockId);
                if (block) {
                    window.gapFillContentManager.handleBlockContentChanged(e.detail);
                }
            }
        });
    }

    setupFieldManager() {
        // Register fields with validation
        this.fieldManager.registerField(this.config.hiddenFieldId, this.hiddenField, {
            required: false,
            validate: (value) => {
                try {
                    JSON.parse(value);
                    return { isValid: true };
                } catch (e) {
                    return { isValid: false, message: 'فرمت JSON نامعتبر است' };
                }
            }
        });
        
        this.fieldManager.registerField('contentJson', document.getElementById('contentJson'), {
            required: false,
            validate: (value) => {
                try {
                    JSON.parse(value);
                    return { isValid: true };
                } catch (e) {
                    return { isValid: false, message: 'فرمت JSON نامعتبر است' };
                }
            }
        });
    }

    setupSyncManager() {
        // Register sync callback for updating hidden fields
        this.syncManager.registerSyncCallback('updateHiddenFields', () => {
            this.updateHiddenField();
        });
        
        // Setup automatic sync
        this.syncManager.setupAutoSync();
    }

    setupEventListeners() {
        // Setup global document listeners once
        ContentBuilderBase.setupGlobalDocumentListeners();
        
        // Block actions (move up, move down, delete)
        this.eventManager.addListener('click', (e) => {
            
            if (e.target.matches('[data-action="move-up"]')) {
                this.moveBlockUp(e.target.closest('.content-block-template, .content-block, .question-block-template'));
            } else if (e.target.matches('[data-action="move-down"]')) {
                this.moveBlockDown(e.target.closest('.content-block-template, .content-block, .question-block-template'));
            } else if (e.target.matches('[data-action="delete"]')) {
                const blockElement = e.target.closest('.content-block-template, .content-block, .question-block-template');
                this.deleteBlock(blockElement);
            } else if (e.target.matches('[data-action="toggle-collapse"]')) {
                this.toggleCollapse(e.target.closest('.content-block-template, .content-block, .question-block-template'));
            } else if (e.target.matches('[data-action="fullscreen"]')) {
                this.toggleFullscreen(e.target.closest('.content-block-template, .content-block, .question-block-template'));
            } else if (e.target.matches('[data-action="insert-above"]')) {
                this.insertBlockAbove(e.target.closest('.content-block-template, .content-block, .question-block-template'));
            }
        });

        // Listen for block content changes from specific block managers
        this.eventManager.addListener('blockContentChanged', (e) => {
            this.handleBlockContentChanged(e.detail);
        });

        // Settings changes (change and input to support sliders)
        this.eventManager.addListener('change', (e) => {
            if (e.target.matches('[data-setting]')) {
                this.updateBlockSettings(e.target);
            }
        });
        this.eventManager.addListener('input', (e) => {
            if (e.target.matches('[data-setting]')) {
                this.updateBlockSettings(e.target);
            }
        });

        // Caption changes
        this.eventManager.addListener('input', (e) => {
            if (e.target.matches('[data-caption="true"]')) {
                this.updateBlockCaption(e.target);
            }
        });

        // Question hint changes
        this.eventManager.addListener('input', (e) => {
            if (e.target.matches('[data-hint="true"]')) {
                this.updateQuestionHint(e.target);
            }
        });

        // Handle insert block above event
        this.eventManager.addListener('insertBlockAbove', (e) => {
            this.handleInsertBlockAbove(e.detail.blockElement);
        });
    }

    addBlock(type) {
        console.log('ContentBuilderBase: addBlock called with type:', type, 'for contentType:', this.config.contentType);
        console.log('ContentBuilderBase: blocksList available:', !!this.blocksList);
        
        if (!this.blocksList) {
            console.error('ContentBuilderBase: blocksList not available, cannot add block');
            return;
        }
        
        const blockId = `block-${this.nextBlockId++}`;
        const block = {
            id: blockId,
            type: type,
            order: this.blocks.length,
            data: this.getDefaultBlockData(type)
        };
        
        console.log('ContentBuilderBase: Created block:', block);
        
        this.blocks.push(block);
        
        const renderResult = this.renderBlock(block);
        if (!renderResult) {
            console.error('ContentBuilderBase: renderBlock returned nothing, block may not have been rendered');
        }
        
        this.updateEmptyState();
        this.updateHiddenField();
        this.scrollToNewBlock(blockId);
        
        console.log('ContentBuilderBase: Block added successfully, total blocks:', this.blocks.length);
        
        // Dispatch custom event for sidebar (event-driven approach - no direct call needed)
        this.eventManager.dispatch('blockAdded', {
            blockId: blockId, 
            blockType: type, 
            contentType: this.config.contentType
        });
    }

    getDefaultBlockData(type) {
        const baseData = {
            size: 'medium',
            position: 'center',
            caption: '',
            captionPosition: 'bottom'
        };

        switch (type) {
            case 'text':
                return {
                    content: '',
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
            // Question block types
            case 'questionText':
                const questionTextData = {
                    content: '',
                    textContent: '',
                    points: 1,
                    difficulty: 'medium',
                    isRequired: true,
                    teacherGuidance: ''
                };
                // Add gap fill specific data if in gapFillMode
                if (window.gapFillMode) {
                    questionTextData.gaps = [];
                    questionTextData.answerType = 'exact';
                    questionTextData.caseSensitive = false;
                }
                return questionTextData;
            case 'questionImage':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null,
                    points: 1,
                    difficulty: 'medium',
                    isRequired: true,
                    teacherGuidance: ''
                };
            case 'questionVideo':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null,
                    points: 1,
                    difficulty: 'medium',
                    isRequired: true,
                    teacherGuidance: ''
                };
            case 'questionAudio':
                return {
                    ...baseData,
                    fileId: null,
                    fileName: null,
                    fileUrl: null,
                    fileSize: null,
                    mimeType: null,
                    isRecorded: false,
                    duration: null,
                    points: 1,
                    difficulty: 'medium',
                    isRequired: true,
                    teacherGuidance: ''
                };
            default:
                return baseData;
        }
    }

    renderBlock(block) {
        // Determine the base template type for question blocks
        let templateType = block.type;
        if (block.type.startsWith('question')) {
            templateType = block.type.replace('question', '').toLowerCase();
        }
        
        // Look for template in blockTemplatesContainer
        const blockTemplatesContainer = document.getElementById('blockTemplatesContainer');
        let template = null;
        
        if (blockTemplatesContainer) {
            // Check in appropriate template container based on block type
            if (block.type.startsWith('question')) {
                const questionTemplates = blockTemplatesContainer.querySelector('#questionBlockTemplates');
                if (questionTemplates) {
                    template = questionTemplates.querySelector(`.content-block-template[data-type="${block.type}"]`);
                }
            } else {
                const regularTemplates = blockTemplatesContainer.querySelector('#contentBlockTemplates');
                if (regularTemplates) {
                    template = regularTemplates.querySelector(`.content-block-template[data-type="${templateType}"]`);
                }
            }
        }
        
        // Fallback to old selector for backward compatibility
        if (!template) {
            template = document.querySelector(`#contentBlockTemplates .content-block-template[data-type="${templateType}"]`);
        }
        
        if (!template) {
            console.error('ContentBuilderBase: Template not found for type:', templateType);
            return;
        }
        
        const blockElement = template.cloneNode(true);
        // Remove template class that hides the element
        blockElement.classList.remove('content-block-template');
        blockElement.classList.add('content-block');
        blockElement.style.display = ''; // Ensure it's visible
        blockElement.dataset.blockId = block.id;
        blockElement.dataset.blockData = JSON.stringify(block.data);
        blockElement.dataset.type = block.type; // Keep original type (e.g., questionText)
        blockElement.dataset.templateType = templateType; // Store template type (e.g., text)
        
        // Configure template for question blocks
        if (block.type.startsWith('question')) {
            this.configureQuestionBlock(blockElement, block);
        }
        
        // Add direct event listeners to this specific block
        this.addDirectEventListeners(blockElement);
        
        const emptyState = this.blocksList.querySelector('.empty-state');
        if (emptyState) {
            this.blocksList.insertBefore(blockElement, emptyState);
        } else {
            this.blocksList.appendChild(blockElement);
        }
        
        // Initialize CKEditor for text blocks
        if (block.type === 'text' || block.type === 'questionText') {
            
            // Initialize CKEditor with a delay to ensure DOM is ready
            setTimeout(() => {
                const editorElement = blockElement.querySelector('.ckeditor-editor');
                if (editorElement && window.ckeditorManager) {
                    window.ckeditorManager.initializeEditor(editorElement);
                } else {
                }
            }, 100);
        }
        
        // Dispatch populate event for specific block managers
        const populateEvent = new CustomEvent('populateBlockContent', {
            detail: {
                blockElement: blockElement,
                block: block,
                blockType: block.type
            }
        });
        document.dispatchEvent(populateEvent);
        
        // Enhance question settings if present (modern controls)
        // Wait longer to ensure values are set and DOM is ready
        setTimeout(() => {
            const questionSettings = blockElement.querySelector('.question-settings');
            if (questionSettings && typeof window.enhanceQuestionSettings === 'function') {
                // Make sure values are set before enhancing
                if (block.type.startsWith('question')) {
                    const pointsInput = questionSettings.querySelector('[data-setting="points"]');
                    const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
                    if (pointsInput && block.data && block.data.points !== undefined) {
                        // Set value and trigger input event to ensure it's recognized
                        pointsInput.value = String(block.data.points);
                        pointsInput.dispatchEvent(new Event('input', { bubbles: true }));
                    }
                    if (difficultySelect && block.data && block.data.difficulty) {
                        difficultySelect.value = block.data.difficulty;
                        difficultySelect.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                }
                
                // Enhance the settings
                window.enhanceQuestionSettings(questionSettings);
                
                // After enhancement, update the bubble position if slider was created
                // This ensures the bubble shows the correct value after page reload
                setTimeout(() => {
                    if (block.type.startsWith('question') && block.data && block.data.points !== undefined) {
                        const pointsInput = questionSettings.querySelector('[data-setting="points"]');
                        if (pointsInput) {
                            const slider = questionSettings.querySelector('.points-range');
                            const bubble = questionSettings.querySelector('.points-bubble');
                            if (slider && bubble) {
                                const value = parseInt(block.data.points, 10);
                                if (!isNaN(value)) {
                                    slider.value = String(value);
                                    pointsInput.value = String(value);
                                    // Update bubble using the enhance function's updateBubble if available
                                    // Trigger input event on slider to update bubble
                                    slider.dispatchEvent(new Event('input', { bubbles: true }));
                                }
                            }
                        }
                    }
                }, 50);
            }
        }, 150);
        
        return blockElement;
    }

    configureQuestionBlock(blockElement, block) {
        // Update block title to show it's a question
        const blockTypeElement = blockElement.querySelector('.block-type span');
        if (blockTypeElement) {
            const originalText = blockTypeElement.textContent;
            blockTypeElement.textContent = `${originalText}`;
        }

        // Show question settings if they exist
        const questionSettings = blockElement.querySelector('.question-settings');
        if (questionSettings) {
            questionSettings.style.display = 'block';
            
            // Set points value
            const pointsInput = questionSettings.querySelector('[data-setting="points"]');
            if (pointsInput) {
                pointsInput.value = block.data.points || 1;
            }
            
            // Set difficulty value
            const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
            if (difficultySelect) {
                difficultySelect.value = block.data.difficulty || 'medium';
                difficultySelect.dispatchEvent(new Event('change', { bubbles: true }));
            }
            
            // Set required checkbox
            const requiredCheckbox = questionSettings.querySelector('[data-setting="isRequired"]');
            if (requiredCheckbox) {
                requiredCheckbox.checked = block.data.isRequired !== false;
            }
        }

        // Show question text editor if it exists
        const questionTextEditor = blockElement.querySelector('.question-text-editor');
        if (questionTextEditor) {
            questionTextEditor.style.display = 'block';
            
            // Set question text content for rich-text-editor
            const textEditor = questionTextEditor.querySelector('.rich-text-editor');
            if (textEditor && block.data.content) {
                textEditor.innerHTML = block.data.content;
            }
        }

        // Show question hint if it exists
        const questionHint = blockElement.querySelector('.question-hint');
        if (questionHint) {
            questionHint.style.display = 'block';
            
            // Set teacher guidance
            const hintTextarea = questionHint.querySelector('[data-hint="true"]');
            if (hintTextarea && block.data.teacherGuidance) {
                hintTextarea.value = block.data.teacherGuidance;
            }
        }

        // Hide regular content editor for question blocks (but not question text editor)
        const regularEditor = blockElement.querySelector('.ckeditor-container:not(.question-text-editor .ckeditor-container)');
        if (regularEditor) {
            regularEditor.style.display = 'none';
        }
    }

    addDirectEventListeners(blockElement) {
        // Add direct event listeners to action buttons
        const actionButtons = blockElement.querySelectorAll('[data-action]');
        // Define general block actions that ContentBuilderBase should handle
        const generalBlockActions = [
            'move-up', 'move-down', 'delete', 'toggle-collapse', 'fullscreen', 'insert-above'
        ];
        
        actionButtons.forEach((button, index) => {
            const action = button.dataset.action;
            // Only handle general block actions, skip block-specific actions
            if (!generalBlockActions.includes(action)) {
                return;
            }
            
            button.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                
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
                    case 'toggle-collapse':
                        this.toggleCollapse(blockElement);
                        break;
                    case 'fullscreen':
                        this.toggleFullscreen(blockElement);
                        break;
                    case 'insert-above':
                        this.insertBlockAbove(blockElement);
                        break;
                }
            });
        });
        
    }

    updateEmptyState() {
        if (this.blocks.length === 0) {
            this.emptyState.style.display = 'flex';
        } else {
            this.emptyState.style.display = 'none';
        }
    }

    moveBlockUp(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: moveBlockUp called with null blockElement');
            return;
        }
        
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex > 0) {
            [this.blocks[blockIndex], this.blocks[blockIndex - 1]] = [this.blocks[blockIndex - 1], this.blocks[blockIndex]];
            
            const prevBlock = blockElement.previousElementSibling;
            if (prevBlock && (prevBlock.classList.contains('content-block') || prevBlock.classList.contains('content-block-template') || prevBlock.classList.contains('question-block-template'))) {
                this.blocksList.insertBefore(blockElement, prevBlock);
            }
            
            this.updateHiddenField();
            this.scrollToBlock(blockId);
            
            // Dispatch custom event
            this.eventManager.dispatch('blockMoved', {
                blockId: blockId, 
                direction: 'up', 
                contentType: this.config.contentType
            });
        }
    }

    moveBlockDown(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: moveBlockDown called with null blockElement');
            return;
        }
        
        const blockId = blockElement.dataset.blockId;
        const blockIndex = this.blocks.findIndex(b => b.id === blockId);
        
        if (blockIndex < this.blocks.length - 1) {
            [this.blocks[blockIndex], this.blocks[blockIndex + 1]] = [this.blocks[blockIndex + 1], this.blocks[blockIndex]];
            
            const nextBlock = blockElement.nextElementSibling;
            if (nextBlock && (nextBlock.classList.contains('content-block') || nextBlock.classList.contains('content-block-template') || nextBlock.classList.contains('question-block-template'))) {
                this.blocksList.insertBefore(nextBlock, blockElement);
            }
            
            this.updateHiddenField();
            this.scrollToBlock(blockId);
            
            // Dispatch custom event
            this.eventManager.dispatch('blockMoved', {
                blockId: blockId, 
                direction: 'down', 
                contentType: this.config.contentType
            });
        }
    }

    async deleteBlock(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: deleteBlock called with null blockElement');
            return;
        }
        
        // Use modal service instead of confirm
        const modal = window.EduTrack?.Services?.Modal;
        const confirmed = modal 
            ? await modal.confirm('آیا از حذف این بلاک اطمینان دارید؟', 'حذف بلاک')
            : confirm('آیا از حذف این بلاک اطمینان دارید؟');
        
        if (confirmed) {
            const blockId = blockElement.dataset.blockId;
            
            if (!blockId) {
                console.error('ContentBuilderBase: Block element has no blockId');
                return;
            }
            
            // Remove from blocks array
            this.blocks = this.blocks.filter(b => b.id !== blockId);
            
            
            // Remove from DOM
            blockElement.remove();
            
            // Update UI
            this.updateEmptyState();
            
            // Force immediate update of hidden fields
            this.updateHiddenField();
            
            // Also trigger sync manager to ensure all fields are updated
            if (this.syncManager) {
                this.syncManager.sync('blockDeleted');
            }
            
            // Dispatch custom event
            this.eventManager.dispatch('blockDeleted', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
            
            // Notify sidebar manager
            if (window.contentSidebarManager) {
                window.contentSidebarManager.removeBlockFromSidebar(blockId);
            }
            
            // Notify step4Manager to refresh add button handlers
            if (window.step4Manager && typeof window.step4Manager.refreshAddButtonHandlers === 'function') {
                window.step4Manager.refreshAddButtonHandlers();
            }
            
        }
    }

    insertBlockAbove(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: insertBlockAbove called with null blockElement');
            return;
        }
        
        // This will be handled by the specific content builder
        this.eventManager.dispatch('insertBlockAbove', {
            blockElement: blockElement
        });
    }

    handleInsertBlockAbove(blockElement) {
        // Show block type selection modal for inserting above
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal(this.config.modalId);
        }
    }

    updateBlockCaption(textarea) {
        const blockElement = textarea.closest('.content-block-template, .content-block, .question-block-template');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            block.data.caption = textarea.value;
            this.updateHiddenField();
            
            // Dispatch custom event
            this.eventManager.dispatch('blockContentChanged', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
        }
    }

    updateQuestionHint(textarea) {
        const blockElement = textarea.closest('.content-block-template, .content-block, .question-block-template');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            block.data.teacherGuidance = textarea.value;
            this.updateHiddenField();
            
            // Dispatch custom event
            this.eventManager.dispatch('blockContentChanged', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
        }
    }

    toggleCollapse(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: toggleCollapse called with null blockElement');
            return;
        }
        
        if (blockElement.classList.contains('fullscreen')) {
            return;
        }
        blockElement.classList.toggle('collapsed');
    }

    toggleFullscreen(blockElement) {
        if (!blockElement) {
            console.error('ContentBuilderBase: toggleFullscreen called with null blockElement');
            return;
        }
        
        blockElement.classList.toggle('fullscreen');
        
        if (blockElement.classList.contains('fullscreen')) {
            blockElement.classList.remove('collapsed');
        }
    }

    handleBlockContentChanged(detail) {
        // Validate detail and blockElement
        if (!detail || !detail.blockElement) {
            console.warn('ContentBuilderBase: handleBlockContentChanged called without valid blockElement');
            return;
        }
        
        const blockElement = detail.blockElement;
        
        // Check if blockElement has dataset
        if (!blockElement.dataset) {
            console.warn('ContentBuilderBase: blockElement does not have dataset');
            return;
        }
        
        const blockId = blockElement.dataset.blockId;
        
        if (!blockId) {
            console.warn('ContentBuilderBase: blockElement does not have blockId');
            return;
        }
        
        // Check if this block belongs to this manager
        const block = this.blocks.find(b => b.id === blockId);
        
        if (!block) {
            // Block doesn't belong to this manager, silently return
            return;
        }
        
        // Update block data with new content
        if (detail.blockData) {
            block.data = { ...block.data, ...detail.blockData };
        }
        if (detail.content !== undefined) {
            block.data.content = detail.content;
        }
        if (detail.textContent !== undefined) {
            block.data.textContent = detail.textContent;
        }
        
        // Update block data attribute
        try {
            blockElement.dataset.blockData = JSON.stringify(block.data);
        } catch (error) {
            console.error('ContentBuilderBase: Error updating blockData attribute:', error);
        }
        
        this.updateHiddenField();
        
        // Dispatch custom event (but don't pass blockElement to avoid recursion issues)
        this.eventManager.dispatch('blockContentChanged', {
            blockId: blockId, 
            contentType: this.config.contentType
        });
        
        // Notify sidebar manager
        if (window.contentSidebarManager) {
            window.contentSidebarManager.updateBlockInSidebar(blockId);
        }
    }

    handleRichTextInput(editor) {
        const blockElement = editor.closest('.content-block');
        if (!blockElement) {
            console.warn('ContentBuilderBase: No block element found for editor');
            return;
        }
        
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            // Update block data with editor content
            block.data.content = editor.innerHTML;
            block.data.textContent = editor.textContent;
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            this.updateHiddenField();
            
            // Dispatch custom event
            this.eventManager.dispatch('blockContentChanged', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
            
        } else {
            console.warn('ContentBuilderBase: Rich text input - Block not found for ID:', blockId);
        }
    }

    handleCKEditorInput(editor) {
        const blockElement = editor.closest('.content-block');
        if (!blockElement) {
            console.warn('ContentBuilderBase: No block element found for editor');
            return;
        }
        
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            // Get content from CKEditor
            let content = '';
            let textContent = '';
            
            if (window.ckeditorManager) {
                const editorContent = window.ckeditorManager.getEditorContent(editor);
                content = editorContent.html;
                textContent = editorContent.text;
            } else {
                // Fallback if CKEditor manager is not available
                content = editor.innerHTML;
                textContent = editor.textContent;
            }
            
            // Update block data with editor content
            block.data.content = content;
            block.data.textContent = textContent;
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            this.updateHiddenField();
            
            // Dispatch custom event
            this.eventManager.dispatch('blockContentChanged', {
                blockId: blockId, 
                contentType: this.config.contentType
            });
            
        } else {
            console.warn('ContentBuilderBase: CKEditor input - Block not found for ID:', blockId);
        }
    }

    saveTextContentImmediately(blockElement, editor) {
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            // Get content from CKEditor
            const editorElement = blockElement.querySelector('.ckeditor-editor');
            let content = '';
            let textContent = '';
            
            if (editorElement && window.ckeditorManager) {
                const editorContent = window.ckeditorManager.getEditorContent(editorElement);
                if (editorContent) {
                    content = editorContent.html;
                    textContent = editorContent.text;
                }
            } else if (editor.classList.contains('rich-text-editor')) {
                content = editor.innerHTML;
                textContent = editor.textContent;
            } else if (editor.tagName === 'TEXTAREA') {
                content = editor.value;
                textContent = editor.value;
            }
            
            // Update block data
            block.data.content = content;
            block.data.textContent = textContent;
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            // Update hidden field immediately
            this.updateHiddenField();
            
        } else {
            console.warn('ContentBuilderBase: Immediate save - Block not found for ID:', blockId);
        }
    }

    updateBlockSettings(select) {
        const blockElement = select.closest('.content-block-template, .content-block, .question-block-template');
        const blockId = blockElement.dataset.blockId;
        const block = this.blocks.find(b => b.id === blockId);
        
        if (block) {
            const setting = select.dataset.setting;
            
            // Handle different input types
            if (select.type === 'checkbox') {
                block.data[setting] = select.checked;
            } else {
                block.data[setting] = select.value;
            }
            
            this.updateHiddenField();
        }
    }

    updateHiddenField() {
        const cleanBlocks = this.blocks.map(block => {
            const cleanBlock = { ...block };
            const cleanData = { ...block.data };
            
            // Remove temporary data
            delete cleanData.pendingFile;
            delete cleanData.previewUrl;
            delete cleanData.originalData;
            
            cleanBlock.data = cleanData;
            return cleanBlock;
        });
        
        const content = { type: this.config.contentType, blocks: cleanBlocks };
        const contentJson = JSON.stringify(content);
        
        // Use field manager to update fields
        const hiddenFieldUpdated = this.fieldManager.updateField(this.config.hiddenFieldId, contentJson);
        const mainFieldUpdated = this.fieldManager.updateField('contentJson', contentJson);
        
        // Also directly update the fields as a fallback
        const hiddenField = document.getElementById(this.config.hiddenFieldId);
        const mainField = document.getElementById('contentJson');
        
        if (hiddenField) {
            hiddenField.value = contentJson;
        }
        
        if (mainField) {
            mainField.value = contentJson;
        }
        
        if (!this.isLoadingExistingContent) {
            this.updatePreview();
        }
    }

    loadExistingContent() {
        
        const hiddenFieldValue = this.fieldManager.getFieldValue(this.config.hiddenFieldId);
        
        if (!hiddenFieldValue || !hiddenFieldValue.trim()) {
            return;
        }
        
        try {
            this.isLoadingExistingContent = true;
            
            const data = JSON.parse(hiddenFieldValue);
            
            if (data.blocks && Array.isArray(data.blocks)) {
                
                if (this.blocksList) {
                    const existingBlocks = this.blocksList.querySelectorAll('.content-block, .question-block-template');
                    existingBlocks.forEach(block => block.remove());
                }
                
                this.blocks = data.blocks;
                if (this.blocks.length > 0) {
                    this.nextBlockId = Math.max(...this.blocks.map(b => parseInt(b.id.split('-')[1]) || 0)) + 1;
                } else {
                    this.nextBlockId = 1;
                }
                
                this.blocks.forEach((block, index) => {
                    this.renderBlock(block);
                });
                
                this.updateEmptyState();
                
                // Populate content fields after rendering with longer delay
                setTimeout(() => {
                    this.populateBlockContent();
                }, 500); // Increased delay
                
                // Notify sidebar manager to refresh
                setTimeout(() => {
                    if (window.contentSidebarManager) {
                        window.contentSidebarManager.forceRefresh();
                    }
                }, 1000);
            } else {
            }
            
            this.isLoadingExistingContent = false;
            
            setTimeout(() => {
                this.updatePreview();
                
                // Dispatch content loaded event for sidebar
                document.dispatchEvent(new CustomEvent('contentLoaded', {
                    detail: { contentType: this.config.contentType }
                }));
            }, 800); // Increased delay
            
        } catch (error) {
            console.error('ContentBuilderBase: Error loading existing content:', error);
            this.isLoadingExistingContent = false;
        }
    }

    // Force reload existing content (called from step4-content.js)
    forceReloadExistingContent() {
        this.loadExistingContent();
    }

    // Force sync content with main field (called from step4-content.js)
    forceSyncWithMainField() {
        
        // Force save all CKEditor content first
        if (window.ckeditorManager) {
            const ckeditorElements = document.querySelectorAll('.ckeditor-editor');
            ckeditorElements.forEach(editorElement => {
                const editorContent = window.ckeditorManager.getEditorContent(editorElement);
                if (editorContent) {
                    const blockElement = editorElement.closest('.content-block');
                    if (blockElement) {
                        const blockId = blockElement.dataset.blockId;
                        const block = this.blocks.find(b => b.id === blockId);
                        if (block) {
                            block.data.content = editorContent.html;
                            block.data.textContent = editorContent.text;
                            blockElement.dataset.blockData = JSON.stringify(block.data);
                        }
                    }
                }
            });
        }
        
        this.updateHiddenField();
        
        // Also trigger sync manager
        if (this.syncManager) {
            this.syncManager.sync('forceSync');
        }
        
    }

    populateBlockContent() {
        
        this.blocks.forEach((block, index) => {
            
            const blockElement = document.querySelector(`[data-block-id="${block.id}"]`);
            if (!blockElement) {
                console.warn(`ContentBuilderBase: Block element not found for block ${block.id}`);
                return;
            }
            
            
            // Update block data attribute
            blockElement.dataset.blockData = JSON.stringify(block.data);
            
            // Configure question blocks with their settings
            if (block.type.startsWith('question')) {
                this.configureQuestionBlock(blockElement, block);
            }
            
            // Use block-specific populate methods
            this.populateBlockByType(blockElement, block);
        });
        
        // After populating, update enhanced question settings (sliders, etc.)
        setTimeout(() => {
            this.blocks.forEach((block) => {
                if (block.type.startsWith('question')) {
                    const blockElement = document.querySelector(`[data-block-id="${block.id}"]`);
                    if (blockElement) {
                        const questionSettings = blockElement.querySelector('.question-settings');
                        if (questionSettings) {
                            // Update points slider
                            if (block.data && block.data.points !== undefined) {
                                const slider = questionSettings.querySelector('.points-range');
                                const bubble = questionSettings.querySelector('.points-bubble');
                                const pointsInput = questionSettings.querySelector('[data-setting="points"]');
                                
                                if (slider && bubble && pointsInput) {
                                    const value = parseInt(block.data.points, 10);
                                    if (!isNaN(value)) {
                                        slider.value = String(value);
                                        pointsInput.value = String(value);
                                        slider.dispatchEvent(new Event('input', { bubbles: true }));
                                    }
                                }
                            }
                            
                            // Update difficulty slider
                            if (block.data && block.data.difficulty) {
                                const difficultySlider = questionSettings.querySelector('.difficulty-range');
                                const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
                                
                                if (difficultySelect) {
                                    const options = [
                                        { value: 'easy', label: 'آسان', index: 0 },
                                        { value: 'medium', label: 'متوسط', index: 1 },
                                        { value: 'hard', label: 'سخت', index: 2 }
                                    ];
                                    const selectedOption = options.find(opt => opt.value === block.data.difficulty) || options[1];
                                    
                                    difficultySelect.value = selectedOption.value;
                                    difficultySelect.dispatchEvent(new Event('change', { bubbles: true }));
                                    
                                    if (difficultySlider) {
                                        difficultySlider.value = String(selectedOption.index);
                                        difficultySlider.dispatchEvent(new Event('input', { bubbles: true }));
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }, 200);
    }

    populateBlockByType(blockElement, block) {
        // Dispatch custom event for block-specific managers to handle
        this.eventManager.dispatch('populateBlockContent', {
            blockElement: blockElement,
            block: block,
            blockType: block.type
        });
    }

    scrollToNewBlock(blockId) {
        setTimeout(() => {
            const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
            if (blockElement) {
                blockElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center',
                    inline: 'nearest'
                });
                
                blockElement.classList.add('highlight');
                
                setTimeout(() => {
                    blockElement.classList.remove('highlight');
                }, 2000);
            }
        }, 100);
    }

    scrollToBlock(blockId) {
        setTimeout(() => {
            const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
            if (blockElement) {
                blockElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center',
                    inline: 'nearest'
                });
            }
        }, 50);
    }

    getContent() {
        return {
            type: this.config.contentType,
            blocks: this.blocks
        };
    }

    // Unified method to collect content data - can be overridden by child classes
    collectContentData() {
        return this.getContent();
    }

    getSizeClass(size) {
        const sizes = { 'small': 'size-small', 'medium': 'size-medium', 'large': 'size-large', 'full': 'size-full' };
        return sizes[size] || 'size-medium';
    }
    
    getPositionClass(position) {
        const positions = { 'left': 'position-left', 'center': 'position-center', 'right': 'position-right' };
        return positions[position] || 'position-center';
    }

    showPreviewModal() {
        this.previewManager.showPreview(this.getContent());
    }

    // Abstract methods to be implemented by specific content builders
    updatePreview() {
        // To be implemented by specific content builders
        console.warn('ContentBuilderBase: updatePreview method should be implemented by specific content builder');
    }

    generateBlockPreview(block) {
        // Use shared preview manager
        return this.previewManager.generateBlockPreview(block);
    }

    // Upload all pending files to server (works for all content types)
    async uploadAllPendingFiles() {
        const pendingBlocks = [];
        
        // Find all blocks with pending files
        for (const block of this.blocks) {
            const blockElement = document.querySelector(`[data-block-id="${block.id}"]`);
            if (blockElement) {
                try {
                    const blockData = JSON.parse(blockElement.dataset.blockData || '{}');
                    // Check if we have a pending file for this block
                    if (this.pendingFiles.has(block.id) || blockData.isPending) {
                        pendingBlocks.push({ blockElement, blockData, block });
                    }
                } catch (e) {
                    console.error('ContentBuilderBase: Error parsing block data:', e);
                }
            }
        }
        
        if (pendingBlocks.length === 0) {
            return; // No pending files
        }
        
        // Upload all files
        for (const { blockElement, blockData, block } of pendingBlocks) {
            try {
                const fileId = await this.uploadBlockFile(blockElement, blockData, block.id);
                if (fileId) {
                    block.data.fileId = fileId;
                    block.data.isPending = false;
                    // Remove from pending files map
                    this.pendingFiles.delete(block.id);
                }
            } catch (error) {
                console.error('ContentBuilderBase: Error uploading file for block', block.id, error);
                throw new Error(`خطا در آپلود فایل بلاک ${block.id}: ${error.message}`);
            }
        }
        
        // Update hidden field after all uploads
        this.updateHiddenField();
    }
    
    // Helper method to upload a single block's file
    async uploadBlockFile(blockElement, blockData, blockId) {
        // Get the file from pending files map
        let fileToUpload = this.pendingFiles.get(blockId);
        
        if (!fileToUpload && blockData.isPending) {
            console.warn(`ContentBuilderBase: No file found for pending block ${blockId}`);
            return null;
        }
        
        if (!fileToUpload) return null;
        
        this.showUploadProgress(blockElement);
        
        try {
            const formData = new FormData();
            formData.append('file', fileToUpload);
            
            // Determine file type robustly
            let fileType = 'image';
            const blockType = (blockElement.dataset.type || '').toLowerCase();
            const templateType = (blockElement.dataset.templateType || '').toLowerCase();
            const mime = (fileToUpload.type || '').toLowerCase();
            if (mime.startsWith('audio/') || blockType.includes('audio') || templateType.includes('audio')) {
                fileType = 'audio';
            } else if (mime.startsWith('video/') || blockType.includes('video') || templateType.includes('video')) {
                fileType = 'video';
            }
            formData.append('type', fileType);
            
            const progressFill = blockElement.querySelector('.progress-fill');
            
            const response = await new Promise((resolve, reject) => {
                const xhr = new XMLHttpRequest();
                
                xhr.upload.addEventListener('progress', (e) => {
                    if (e.lengthComputable && progressFill) {
                        const percentComplete = (e.loaded / e.total) * 100;
                        progressFill.style.width = percentComplete + '%';
                    }
                });
                
                xhr.onload = () => {
                    if (xhr.status === 200) {
                        try {
                            const result = JSON.parse(xhr.responseText);
                            resolve(result);
                        } catch (e) {
                            reject(e);
                        }
                    } else {
                        reject(new Error('Upload failed'));
                    }
                };
                
                xhr.onerror = () => reject(new Error('Network error'));
                
                xhr.open('POST', '/FileUpload/UploadContentFile');
                xhr.send(formData);
            });
            
            if (response.success) {
                // Update block data
                const updatedData = {
                    fileId: response.data.id,
                    fileName: response.data.fileName || response.data.originalFileName,
                    fileUrl: response.data.url,
                    fileSize: response.data.size,
                    mimeType: response.data.mimeType,
                    isPending: false
                };
                
                // Clean old data and update with new data
                const oldData = JSON.parse(blockElement.dataset.blockData || '{}');
                delete oldData.localFile;
                delete oldData.pendingUpload;
                
                blockElement.dataset.blockData = JSON.stringify({ ...oldData, ...updatedData });
                
                const blockIndex = this.blocks.findIndex(b => b.id === blockElement.dataset.blockId);
                if (blockIndex !== -1) {
                    delete this.blocks[blockIndex].data.localFile;
                    delete this.blocks[blockIndex].data.pendingUpload;
                    this.blocks[blockIndex].data = { ...this.blocks[blockIndex].data, ...updatedData };
                }
                
                this.hideUploadProgress(blockElement);
                return response.data.id;
            } else {
                throw new Error(response.message || 'Upload failed');
            }
        } catch (error) {
            this.hideUploadProgress(blockElement);
            throw error;
        }
    }
    
    // Helper methods for upload progress
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

    // Cleanup method
    destroy() {
        this.eventManager.removeAllListenersAll();
        this.syncManager = null;
        this.fieldManager = null;
        this.eventManager = null;
        this.previewManager = null;
    }
}

// Export for use in other files
if (typeof window !== 'undefined') {
    window.ContentBuilderBase = ContentBuilderBase;
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = ContentBuilderBase;
}
