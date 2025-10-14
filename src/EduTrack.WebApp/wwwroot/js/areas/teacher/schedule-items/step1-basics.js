/**
 * Step 1 Basics Manager
 * Handles title input, item type selection, rich text editor and basic form validation
 */

class Step1BasicsManager {
    constructor(formManager) {
        this.formManager = formManager;
        this.currentType = null;
        this.contentTypes = {};
        this.init();
    }

    init() {
        this.setupRichEditor();
        this.setupFormInputListeners();
    }

    setupFormInputListeners() {
        // Title character count
        const titleInput = document.getElementById('itemTitle');
        if (titleInput) {
            titleInput.addEventListener('input', (e) => {
                this.updateCharacterCount(e.target, 100);
                if (this.formManager && typeof this.formManager.validateField === 'function') {
                    this.formManager.validateField('title', e.target.value);
                }
            });
        }

        // Item type selection
        const itemTypeSelect = document.getElementById('itemType');
        if (itemTypeSelect) {
            itemTypeSelect.addEventListener('change', (e) => {
                this.changeItemType(parseInt(e.target.value));
                if (this.formManager && typeof this.formManager.validateField === 'function') {
                    this.formManager.validateField('type', e.target.value);
                }
            });
        }
    }

    setupRichEditor() {
        const editor = document.getElementById('descriptionEditor');
        if (!editor) return;

        // Set up basic editor functionality
        editor.addEventListener('paste', (e) => {
            e.preventDefault();
            const text = e.clipboardData.getData('text/plain');
            document.execCommand('insertText', false, text);
        });

        // Setup rich editor listeners
        this.setupRichEditorListeners();
    }

    setupRichEditorListeners() {
        const editor = document.getElementById('descriptionEditor');
        if (!editor) return;

        // Toolbar buttons
        document.querySelectorAll('.toolbar-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                this.executeEditorCommand(e.currentTarget.dataset.command);
            });
        });

        // Editor content changes
        editor.addEventListener('input', () => {
            this.updateHiddenDescription();
            if (this.formManager && typeof this.formManager.validateField === 'function') {
                this.formManager.validateField('description', editor.innerHTML);
            }
        });

        // Editor focus/blur
        editor.addEventListener('focus', () => {
            editor.parentElement.classList.add('focused');
        });

        editor.addEventListener('blur', () => {
            editor.parentElement.classList.remove('focused');
        });
    }

    executeEditorCommand(command) {
        document.execCommand(command, false, null);
        this.updateToolbarState();
    }

    updateToolbarState() {
        document.querySelectorAll('.toolbar-btn').forEach(btn => {
            const command = btn.dataset.command;
            btn.classList.toggle('active', document.queryCommandState(command));
        });
    }

    updateHiddenDescription() {
        const editor = document.getElementById('descriptionEditor');
        const hiddenField = document.getElementById('descriptionHidden');

        if (editor && hiddenField) {
            hiddenField.value = editor.innerHTML;
        }
    }

    changeItemType(typeId) {
        this.currentType = typeId;

        // Show type preview
        this.showTypePreview(typeId);

        // Update sidebar
        if (this.formManager && typeof this.formManager.updateStepIndicators === 'function') {
            this.formManager.updateStepIndicators();
        }
    }

    showTypePreview(typeId) {
        const preview = document.getElementById('typePreview');
        const previewContent = document.getElementById('typePreviewContent');

        if (!preview || !previewContent) return;

        const typeInfo = this.contentTypes[typeId];
        if (typeInfo) {
            previewContent.innerHTML = `
                <div class="type-info">
                    <h4>${typeInfo.name}</h4>
                    <p>${typeInfo.description}</p>
                    <div class="type-features">
                        <span class="feature-tag">تعاملی</span>
                        <span class="feature-tag">جذاب</span>
                        <span class="feature-tag">آموزشی</span>
                    </div>
                </div>
            `;
            preview.style.display = 'block';
        } else {
            preview.style.display = 'none';
        }
    }

    getTypeIcon(typeId) {
        const icons = {
            0: 'bell',
            1: 'pen',
            2: 'volume-up',
            3: 'edit',
            4: 'list-ul',
            5: 'link',
            6: 'bug',
            7: 'code',
            8: 'clipboard-check'
        };
        return icons[typeId] || 'file';
    }

    // Utility Methods
    updateCharacterCount(input, maxLength) {
        const charCount = input.parentElement.querySelector('.char-count');
        if (charCount) {
            const currentLength = input.value.length;
            charCount.textContent = `${currentLength}/${maxLength}`;
            charCount.classList.toggle('over-limit', currentLength > maxLength);
        }
    }

    // Data collection for step 1
    collectStep1Data() {
        return {
            Type: parseInt(document.getElementById('itemType')?.value),
            Title: document.getElementById('itemTitle')?.value,
            Description: document.getElementById('descriptionHidden')?.value
        };
    }

    // Load step 1 data from existing item
    async loadStepData() {
        if (this.formManager && typeof this.formManager.getExistingItemData === 'function') {
            const existingData = this.formManager.getExistingItemData();
            if (existingData) {
                this.populateStep1Data(existingData);
                console.log('Step 1 data loaded from existing item');
            }
        }
    }

    // Populate step 1 with existing data
    populateStep1Data(data) {
        if (data.title) {
            const itemTitle = document.getElementById('itemTitle');
            if (itemTitle) {
                itemTitle.value = data.title;
            }
        }
        if (data.type >= 0) {
            const itemType = document.getElementById('itemType');
            if (itemType) {
                itemType.value = data.type;
                // Trigger change event to update UI
                this.changeItemType(parseInt(data.type));
            }
        }
        if (data.description) {
            const descriptionEditor = document.getElementById('descriptionEditor');
            const descriptionHidden = document.getElementById('descriptionHidden');
            if (descriptionEditor) {
                descriptionEditor.innerHTML = data.description;
            }
            if (descriptionHidden) {
                descriptionHidden.value = data.description;
            }
        }
    }
}

window.Step1BasicsManager = Step1BasicsManager;
