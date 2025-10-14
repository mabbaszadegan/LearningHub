/**
 * Step 4 Content Manager
 * Handles content builder, type selection and content preview for step 4
 */

class Step4ContentManager {
    constructor(formManager) {
        this.formManager = formManager;
        this.contentBuilder = null;
        this.selectedContentType = null;
        this.init();
    }

    init() {
        this.setupContentTypeListeners();
        this.setupContentBuilder();
        this.updateStep4Content();
    }

    setupContentTypeListeners() {
        const typeSelect = document.getElementById('itemType');
        if (typeSelect) {
            typeSelect.addEventListener('change', (e) => {
                this.selectedContentType = e.target.value;
                this.updateStep4Content();
            });
        }
    }

    setupContentBuilder() {
        this.contentBuilder = null;
        const checkContentBuilder = () => {
            if (window.contentBuilder) {
                this.contentBuilder = window.contentBuilder;
                this.setupContentBuilderEvents();
            } else {
                setTimeout(checkContentBuilder, 100);
            }
        };
        checkContentBuilder();
    }

    setupContentBuilderEvents() {
        if (!this.contentBuilder) return;
        const saveBtn = document.getElementById('saveContentBtn');
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                this.saveContentBuilderData();
            });
        }
        const previewBtn = document.getElementById('previewContentBtn');
        if (previewBtn) {
            previewBtn.addEventListener('click', () => {
                this.previewContentBuilderData();
            });
        }
    }

    updateStep4Content() {
        const contentTypeSelector = document.getElementById('contentTypeSelector');
        const contentDesigner = document.getElementById('contentDesigner');
        const contentBuilder = document.getElementById('contentBuilder');
        const contentTemplates = document.getElementById('contentTemplates');

        if (contentTypeSelector) contentTypeSelector.style.display = 'none';
        if (contentDesigner) contentDesigner.style.display = 'none';
        if (contentBuilder) contentBuilder.style.display = 'none';
        if (contentTemplates) contentTemplates.style.display = 'none';

        const itemTypeSelect = document.getElementById('itemType');
        const selectedType = itemTypeSelect ? itemTypeSelect.value : '0';

        if (selectedType === '0') {
            if (contentBuilder) {
                contentBuilder.style.display = 'block';
                if (!this.contentBuilder) {
                    this.setupContentBuilder();
                }
            }
        } else {
            if (contentTypeSelector) {
                contentTypeSelector.style.display = 'block';
            }
        }
    }

    saveContentBuilderData() {
        if (!this.contentBuilder) return;
        const contentData = this.contentBuilder.getContentData();
        const contentJson = JSON.stringify(contentData);
        const contentField = document.getElementById('contentJson');
        if (contentField) {
            contentField.value = contentJson;
        }
        if (this.formManager && typeof this.formManager.showSuccess === 'function') {
            this.formManager.showSuccess('محتوای آموزشی با موفقیت ذخیره شد.');
        }
    }

    previewContentBuilderData() {
        if (!this.contentBuilder) return;
        const contentData = this.contentBuilder.getContentData();
        this.showContentPreview(contentData);
    }

    showContentPreview(contentData) {
        const modal = document.getElementById('previewModal');
        const previewContent = document.getElementById('previewContent');
        if (!modal || !previewContent) return;
        let previewHTML = '<div class="content-preview">';
        if (contentData.boxes && contentData.boxes.length > 0) {
            contentData.boxes.forEach(box => {
                previewHTML += this.generateBoxPreview(box);
            });
        } else {
            previewHTML += '<div class="empty-content">هیچ محتوایی اضافه نشده است.</div>';
        }
        previewHTML += '</div>';
        previewContent.innerHTML = previewHTML;
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }

    generateBoxPreview(box) {
        let html = `<div class="preview-box preview-box-${box.type}">`;
        switch (box.type) {
            case 'text':
                html += `<div class="text-content">${box.data.content || ''}</div>`;
                break;
            case 'image':
                if (box.data.fileId) {
                    html += `<div class="image-content" style="text-align: ${box.data.align || 'center'};">
                        <img src="/uploads/${box.data.fileId}" alt="تصویر" style="max-width: ${this.getImageSize(box.data.size)};" />
                        ${box.data.caption ? `<div class="image-caption">${box.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
            case 'video':
                if (box.data.fileId) {
                    html += `<div class="video-content" style="text-align: ${box.data.align || 'center'};">
                        <video controls style="max-width: ${this.getVideoSize(box.data.size)};">
                            <source src="/uploads/${box.data.fileId}" type="video/mp4">
                        </video>
                        ${box.data.caption ? `<div class="video-caption">${box.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
            case 'audio':
                if (box.data.fileId) {
                    html += `<div class="audio-content">
                        <audio controls>
                            <source src="/uploads/${box.data.fileId}" type="audio/mpeg">
                        </audio>
                        ${box.data.caption ? `<div class="audio-caption">${box.data.caption}</div>` : ''}
                    </div>`;
                }
                break;
        }
        html += '</div>';
        return html;
    }

    getImageSize(size) {
        switch (size) {
            case 'small': return '200px';
            case 'medium': return '400px';
            case 'large': return '600px';
            case 'full': return '100%';
            default: return '400px';
        }
    }

    getVideoSize(size) {
        switch (size) {
            case 'small': return '400px';
            case 'medium': return '600px';
            case 'large': return '800px';
            case 'full': return '100%';
            default: return '600px';
        }
    }

    collectContentData() {
        const container = document.getElementById('contentDesignContainer');
        if (!container) return "{}";
        const contentData = {};
        container.querySelectorAll('input, textarea, select').forEach(input => {
            contentData[input.name || input.id] = input.value;
        });
        return JSON.stringify(contentData);
    }

    // Collect step 4 data for saving
    collectStep4Data() {
        return {
            ContentJson: this.collectContentData()
        };
    }

    // Load step 4 data from existing item
    async loadStepData() {
        if (this.formManager && typeof this.formManager.getExistingItemData === 'function') {
            const existingData = this.formManager.getExistingItemData();
            if (existingData && existingData.contentJson) {
                const contentField = document.getElementById('contentJson');
                if (contentField) {
                    contentField.value = existingData.contentJson;
                }
                console.log('Step 4 data loaded from existing item');
            }
        }
    }
}

window.Step4ContentManager = Step4ContentManager;


