/**
 * Step 4 Content Manager
 * Main manager for step 4 content editing
 * Coordinates between different content type managers
 */

class Step4ContentManager {
    constructor(formManager) {
        this.formManager = formManager;
        this.currentContentManager = null;
        this.contentManagers = new Map();

        // Initialize shared managers
        this.fieldManager = new FieldManager();
        this.eventManager = new EventManager();

        this.init();
    }

    init() {
        this.setupFieldManager();
        this.registerContentManagers();
        this.setupEventListeners();
        
        setTimeout(() => {
            this.updateContentType();
            this.loadStepData();
        }, 500);
    }

    setupFieldManager() {
        // Register all content JSON fields
        ['contentJson', 'reminderContentJson', 'writtenContentJson', 
         'gapFillContentJson', 'multipleChoiceContentJson'].forEach(fieldId => {
            const element = document.getElementById(fieldId);
            if (element) {
                this.fieldManager.registerField(fieldId, element);
            }
        });

        // Register item type field
        const itemTypeField = document.getElementById('itemType');
        if (itemTypeField) {
            this.fieldManager.registerField('itemType', itemTypeField);
        }
    }

    registerContentManagers() {
        // Register available content managers
        this.contentManagers.set('0', 'reminder'); // Reminder
        this.contentManagers.set('1', 'written');  // Writing
        this.contentManagers.set('2', 'audio');    // Audio
        this.contentManagers.set('3', 'gapfill');  // GapFill
        this.contentManagers.set('4', 'multiplechoice');  // MultipleChoice
    }

    setupEventListeners() {
        const itemTypeSelect = document.getElementById('itemType');
        if (itemTypeSelect) {
            itemTypeSelect.addEventListener('change', () => {
                this.updateContentType();
            });
        }

        // Setup add block button handler
        this.refreshAddButtonHandlers();
    }

    refreshAddButtonHandlers() {
        const addBlockBtn = document.getElementById('addContentBlockBtn');
        if (addBlockBtn) {
            // Remove existing listeners by cloning the element
            const newBtn = addBlockBtn.cloneNode(true);
            addBlockBtn.parentNode.replaceChild(newBtn, addBlockBtn);

            // Add click handler
            newBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.handleAddBlockClick();
            });
        }
    }

    handleAddBlockClick() {
        // Get current item type to determine which modal type to show
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : null;
        const managerType = selectedType ? this.contentManagers.get(selectedType) : null;

        // Map manager types to item types for modal
        const itemTypeMap = {
            'reminder': 'reminder',
            'written': 'writing',
            'audio': 'writing',
            'gapfill': 'gapfill',
            'multiplechoice': 'multiplechoice'
        };

        const itemTypeForModal = managerType ? (itemTypeMap[managerType] || 'reminder') : 'reminder';

        // Show the block type selection modal
        if (window.sharedContentBlockManager) {
            window.sharedContentBlockManager.showBlockTypeModal('blockTypeModal', itemTypeForModal);
        } else if (window.blockTypeSelectionModal) {
            window.blockTypeSelectionModal.showModal(itemTypeForModal);
        } else {
            console.error('Step4ContentManager: Block type modal not available');
        }
    }

    updateContentType() {
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : null;
        
        if (!selectedType) return;

        // Hide all content builders
        this.hideAllContentBuilders();

        // Show and initialize appropriate content manager
        const managerType = this.contentManagers.get(selectedType);
        if (!managerType) {
            console.warn('Step4ContentManager: Unknown content type:', selectedType);
            return;
        }

        this.currentContentManager = managerType;
        this.showContentBuilder(managerType, selectedType);
        
        // Refresh button handlers after content type changes
        setTimeout(() => {
            this.refreshAddButtonHandlers();
        }, 100);
    }

    hideAllContentBuilders() {
        const builders = [
            'reminderContentBuilder',
            'writtenContentBuilder',
            'gapFillContentBuilder',
            'multipleChoiceContentBuilder',
            'contentBuilder'
        ];

        builders.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.style.display = 'none';
            }
        });

        // Reset global modes
        window.multipleChoiceMode = false;
        window.gapFillMode = false;
    }

    showContentBuilder(managerType, selectedType) {
        let builderId = '';
        
        switch (managerType) {
            case 'reminder':
                builderId = 'reminderContentBuilder';
                break;
            case 'written':
                builderId = 'writtenContentBuilder';
                break;
            case 'audio':
                builderId = 'writtenContentBuilder'; // Audio uses same builder structure
                break;
            case 'gapfill':
                builderId = 'gapFillContentBuilder';
                window.gapFillMode = true;
                break;
            case 'multiplechoice':
                builderId = 'multipleChoiceContentBuilder';
                window.multipleChoiceMode = true;
                break;
        }

        const builder = document.getElementById(builderId);
        if (builder) {
            builder.style.display = 'block';
            this.initializeContentManager(managerType);
        }
    }

    initializeContentManager(managerType) {
        // Initialize the appropriate manager
        switch (managerType) {
            case 'reminder':
                if (typeof window.initializeReminderContentManager === 'function') {
                    window.initializeReminderContentManager();
                }
                break;
            case 'written':
                if (typeof window.initializeWrittenContentManager === 'function') {
                    window.initializeWrittenContentManager();
                }
                break;
            case 'audio':
                if (typeof window.initializeAudioContentManager === 'function') {
                    window.initializeAudioContentManager();
                }
                break;
            case 'gapfill':
                if (typeof window.initializeGapFillContentManager === 'function') {
                    window.initializeGapFillContentManager();
                }
                break;
            case 'multiplechoice':
                if (typeof window.initializeMultipleChoiceContentManager === 'function') {
                    window.initializeMultipleChoiceContentManager();
                }
                break;
        }
    }

    async loadStepData() {
        if (!this.formManager || typeof this.formManager.getExistingItemData !== 'function') {
            return;
        }

        const existingData = this.formManager.getExistingItemData();
        if (!existingData || !existingData.contentJson) {
            return;
        }

        // Update all content JSON fields with existing data
        ['contentJson', 'reminderContentJson', 'writtenContentJson', 
         'gapFillContentJson', 'multipleChoiceContentJson'].forEach(fieldId => {
            this.fieldManager.updateField(fieldId, existingData.contentJson);
        });

        // Determine item type
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        // Set modes based on type
        if (selectedType === '3') {
            window.gapFillMode = true;
            window.multipleChoiceMode = false;
        } else if (selectedType === '4') {
            window.multipleChoiceMode = true;
            window.gapFillMode = false;
        } else {
            window.multipleChoiceMode = false;
            window.gapFillMode = false;
        }

        // Load content for active manager
        setTimeout(() => {
            this.loadContentForManager(selectedType);
        }, 500);
    }

    loadContentForManager(selectedType) {
        const managerType = this.contentManagers.get(selectedType);
        if (!managerType) return;

        switch (managerType) {
            case 'reminder':
                if (window.reminderContentManager && 
                    typeof window.reminderContentManager.loadExistingContent === 'function') {
                    window.reminderContentManager.loadExistingContent();
                }
                break;
            case 'written':
                if (window.writtenContentManager && 
                    typeof window.writtenContentManager.loadExistingContent === 'function') {
                    window.writtenContentManager.loadExistingContent();
                }
                break;
            case 'audio':
                if (window.audioContentManager && 
                    typeof window.audioContentManager.loadExistingContent === 'function') {
                    window.audioContentManager.loadExistingContent();
                }
                break;
            case 'gapfill':
                if (window.gapFillContentManager && 
                    typeof window.gapFillContentManager.loadExistingContent === 'function') {
                    window.gapFillContentManager.loadExistingContent();
                }
                break;
            case 'multiplechoice':
                if (window.multipleChoiceContentManager && 
                    typeof window.multipleChoiceContentManager.loadExistingContent === 'function') {
                    window.multipleChoiceContentManager.loadExistingContent();
                }
                break;
        }
    }

    validateStep4() {
        this.fieldManager.clearAllErrors();
        
        const validationResult = this.fieldManager.validateAllFields();
        if (!validationResult.isValid) {
            return false;
        }

        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';
        const managerType = this.contentManagers.get(selectedType);

        // Type-specific validation
        if (managerType === 'written' && selectedType === '1') {
            // Writing type validation
            const writtenManager = window.writtenContentManager;
            if (writtenManager && typeof writtenManager.validateContent === 'function') {
                return writtenManager.validateContent();
            }
        } else if (managerType === 'gapfill') {
            // GapFill validation - ensure at least one block exists
            const gapFillManager = window.gapFillContentManager;
            if (gapFillManager && (!gapFillManager.blocks || gapFillManager.blocks.length === 0)) {
                this.fieldManager.showFieldError('contentJson', 'حداقل یک بلاک برای تمرین جای‌خالی الزامی است');
                return false;
            }
        }

        // Check if content JSON is empty
        const contentJson = this.fieldManager.getFieldValue('contentJson');
        if (!contentJson || contentJson.trim() === '' || contentJson === '{}') {
            this.fieldManager.showFieldError('contentJson', 'محتوای آموزشی الزامی است');
            return false;
        }

        return true;
    }

    async collectStep4Data() {
        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';
        const managerType = this.contentManagers.get(selectedType);

        // Map manager types to their window objects
        const managerMap = {
            'reminder': window.reminderContentManager,
            'written': window.writtenContentManager,
            'audio': window.audioContentManager,
            'gapfill': window.gapFillContentManager,
            'multiplechoice': window.multipleChoiceContentManager
        };

        let contentJson = '{}';
        const activeManager = managerMap[managerType];

        // Unified collection - all managers use collectContentData from ContentBuilderBase
        if (activeManager && typeof activeManager.collectContentData === 'function') {
            try {
                const content = activeManager.collectContentData();
                contentJson = JSON.stringify(content);
            } catch (error) {
                console.error(`Step4ContentManager: Error collecting content from ${managerType}:`, error);
            }
        }

        // Fallback to hidden field value
        if (!contentJson || contentJson === '{}') {
            const hiddenField = document.getElementById('contentJson');
            if (hiddenField && hiddenField.value) {
                contentJson = hiddenField.value;
            }
        }

        return {
            ContentJson: contentJson
        };
    }

    updateStep4Content() {
        this.updateContentType();
    }
}

// Export
if (typeof window !== 'undefined') {
    window.Step4ContentManager = Step4ContentManager;
}

