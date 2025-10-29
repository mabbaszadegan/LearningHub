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

    validateStep1() {
        let isValid = true;
        
        // Validate Title
        const titleInput = document.querySelector('input[name="Title"]');
        if (!titleInput || !titleInput.value || titleInput.value.trim() === '') {
            this.showFieldError('Title', 'عنوان آیتم آموزشی الزامی است');
            isValid = false;
        } else {
            this.clearFieldError('Title');
        }
        
        // Validate Type
        const typeSelect = document.querySelector('select[name="Type"]');
        if (!typeSelect || !typeSelect.value) {
            this.showFieldError('Type', 'نوع آیتم آموزشی الزامی است');
            isValid = false;
        } else {
            this.clearFieldError('Type');
        }
        
        // Validate Description (optional but if provided, check length)
        const descriptionInput = document.querySelector('textarea[name="Description"]');
        if (descriptionInput && descriptionInput.value && descriptionInput.value.length > 1000) {
            this.showFieldError('Description', 'توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد');
            isValid = false;
        } else {
            this.clearFieldError('Description');
        }
        
        return isValid;
    }

    showFieldError(fieldName, message) {
        const field = document.querySelector(`[name="${fieldName}"]`);
        if (field) {
            field.classList.add('is-invalid');
            
            // Remove existing error message
            const existingError = field.parentNode.querySelector('.field-error');
            if (existingError) {
                existingError.remove();
            }
            
            // Add new error message
            const errorDiv = document.createElement('div');
            errorDiv.className = 'field-error text-danger mt-1';
            errorDiv.textContent = message;
            field.parentNode.appendChild(errorDiv);
        }
    }

    clearFieldError(fieldName) {
        const field = document.querySelector(`[name="${fieldName}"]`);
        if (field) {
            field.classList.remove('is-invalid');
            
            const existingError = field.parentNode.querySelector('.field-error');
            if (existingError) {
                existingError.remove();
            }
        }
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
        // CKEditor 5 is initialized in CreateOrEdit.cshtml as a module
        // Wait for it to be available
        if (window.descriptionEditor) {
            // Editor is already initialized
            this.setupCKEditorListeners();
        } else {
            // Wait for editor to be initialized
            const checkEditor = setInterval(() => {
                if (window.descriptionEditor) {
                    clearInterval(checkEditor);
                    this.setupCKEditorListeners();
                }
            }, 100);

            // Stop checking after 5 seconds
            setTimeout(() => clearInterval(checkEditor), 5000);
        }
    }

    setupCKEditorListeners() {
        const editor = window.descriptionEditor;
        if (!editor) return;

        // Editor content changes are handled by CreateOrEdit.cshtml module
        // which syncs to descriptionHidden. We can validate here if needed.
        editor.model.document.on('change:data', () => {
            if (this.formManager && typeof this.formManager.validateField === 'function') {
                const content = editor.getData();
                this.formManager.validateField('description', content);
            }
        });
    }

    updateHiddenDescription() {
        // This is now handled automatically by the CKEditor module in CreateOrEdit.cshtml
        // But we can still update it here if needed for validation
        const editor = window.descriptionEditor;
        const hiddenField = document.getElementById('descriptionHidden');

        if (editor && hiddenField) {
            hiddenField.value = editor.getData();
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
            const descriptionHidden = document.getElementById('descriptionHidden');
            if (descriptionHidden) {
                descriptionHidden.value = data.description;
            }
            
            // Set CKEditor content if available
            if (window.descriptionEditor) {
                window.descriptionEditor.setData(data.description);
            } else {
                // Wait for editor to be initialized
                const checkEditor = setInterval(() => {
                    if (window.descriptionEditor) {
                        clearInterval(checkEditor);
                        window.descriptionEditor.setData(data.description);
                    }
                }, 100);
                
                // Stop checking after 5 seconds
                setTimeout(() => clearInterval(checkEditor), 5000);
            }
        }
    }
}

window.Step1BasicsManager = Step1BasicsManager;
