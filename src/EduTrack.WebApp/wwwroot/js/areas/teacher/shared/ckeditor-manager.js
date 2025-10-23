/**
 * CKEditor Manager
 * Handles CKEditor rich text editor initialization and management
 */

if (typeof window.CKEditorManager === 'undefined') {
    window.CKEditorManager = class CKEditorManager {
    constructor() {
        this.editors = new Map();
        this.isInitialized = false;
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        console.log('CKEditorManager: Starting initialization...');
        console.log('CKEditorManager: ClassicEditor available:', typeof ClassicEditor !== 'undefined');
        
        // Wait for CKEditor to be loaded
        if (typeof ClassicEditor === 'undefined') {
            console.warn('CKEditor not loaded yet, retrying...');
            setTimeout(() => this.init(), 1000);
            return;
        }

        this.setupEventListeners();
        this.isInitialized = true;
        console.log('CKEditorManager initialized successfully');
    }

    setupEventListeners() {
        // Listen for new text blocks being added
        document.addEventListener('DOMNodeInserted', (e) => {
            if (e.target.classList && e.target.classList.contains('ckeditor-editor')) {
                this.initializeEditor(e.target);
            }
        });

        // Listen for populate block content events
        document.addEventListener('populateBlockContent', (e) => {
            if (e.detail.blockType === 'text') {
                this.populateTextBlock(e.detail.blockElement, e.detail.block.data);
            }
        });

        // Listen for block content changes
        document.addEventListener('blockContentChanged', (e) => {
            console.log('CKEditorManager: Received blockContentChanged event', e.detail);
        });
    }

    async initializeEditor(editorElement) {
        if (!editorElement || this.editors.has(editorElement)) {
            console.log('CKEditorManager: Editor already initialized or element not found');
            return;
        }

        console.log('CKEditorManager: Initializing editor for:', editorElement);

        const blockElement = editorElement.closest('.content-block');
        const blockId = blockElement ? blockElement.dataset.blockId : 'unknown';

        // Check if ClassicEditor is available
        if (typeof ClassicEditor === 'undefined') {
            console.error('CKEditorManager: ClassicEditor not available');
            return;
        }

        try {
            const editor = await ClassicEditor.create(editorElement, {
                toolbar: {
                    items: [
                        'heading', '|',
                        'bold', 'italic', 'underline', 'strikethrough', '|',
                        'fontSize', 'fontFamily', 'fontColor', 'fontBackgroundColor', '|',
                        'alignment', '|',
                        'numberedList', 'bulletedList', '|',
                        'outdent', 'indent', '|',
                        'link', 'insertImage', 'insertTable', '|',
                        'codeBlock', 'blockQuote', '|',
                        'undo', 'redo', '|',
                        'findAndReplace', 'selectAll', '|',
                        'fullScreen'
                    ],
                    shouldNotGroupWhenFull: true
                },
                language: 'fa',
                placeholder: editorElement.dataset.placeholder || 'متن خود را اینجا تایپ کنید...',
                heading: {
                    options: [
                        { model: 'paragraph', title: 'پاراگراف', class: 'ck-heading_paragraph' },
                        { model: 'heading1', view: 'h1', title: 'سرتیتر 1', class: 'ck-heading_heading1' },
                        { model: 'heading2', view: 'h2', title: 'سرتیتر 2', class: 'ck-heading_heading2' },
                        { model: 'heading3', view: 'h3', title: 'سرتیتر 3', class: 'ck-heading_heading3' },
                        { model: 'heading4', view: 'h4', title: 'سرتیتر 4', class: 'ck-heading_heading4' }
                    ]
                },
                fontSize: {
                    options: [
                        9, 11, 13, 'default', 17, 19, 21
                    ],
                    supportAllValues: true
                },
                fontFamily: {
                    options: [
                        'default',
                        'Arial, Helvetica, sans-serif',
                        'Courier New, Courier, monospace',
                        'Georgia, serif',
                        'Lucida Sans Unicode, Lucida Grande, sans-serif',
                        'Tahoma, Geneva, sans-serif',
                        'Times New Roman, Times, serif',
                        'Trebuchet MS, Helvetica, sans-serif',
                        'Verdana, Geneva, sans-serif',
                        'Tahoma, Arial, sans-serif'
                    ],
                    supportAllValues: true
                },
                fontColor: {
                    colors: [
                        { color: 'hsl(0, 0%, 0%)', label: 'سیاه' },
                        { color: 'hsl(0, 0%, 30%)', label: 'خاکستری تیره' },
                        { color: 'hsl(0, 0%, 60%)', label: 'خاکستری' },
                        { color: 'hsl(0, 0%, 90%)', label: 'خاکستری روشن' },
                        { color: 'hsl(0, 0%, 100%)', label: 'سفید', hasBorder: true },
                        { color: 'hsl(0, 75%, 60%)', label: 'قرمز' },
                        { color: 'hsl(30, 75%, 60%)', label: 'نارنجی' },
                        { color: 'hsl(60, 75%, 60%)', label: 'زرد' },
                        { color: 'hsl(90, 75%, 60%)', label: 'سبز روشن' },
                        { color: 'hsl(120, 75%, 60%)', label: 'سبز' },
                        { color: 'hsl(150, 75%, 60%)', label: 'سبز آبی' },
                        { color: 'hsl(180, 75%, 60%)', label: 'آبی روشن' },
                        { color: 'hsl(210, 75%, 60%)', label: 'آبی' },
                        { color: 'hsl(240, 75%, 60%)', label: 'بنفش' },
                        { color: 'hsl(270, 75%, 60%)', label: 'بنفش روشن' }
                    ]
                },
                fontBackgroundColor: {
                    colors: [
                        { color: 'hsl(0, 75%, 60%)', label: 'قرمز' },
                        { color: 'hsl(30, 75%, 60%)', label: 'نارنجی' },
                        { color: 'hsl(60, 75%, 60%)', label: 'زرد' },
                        { color: 'hsl(90, 75%, 60%)', label: 'سبز روشن' },
                        { color: 'hsl(120, 75%, 60%)', label: 'سبز' },
                        { color: 'hsl(150, 75%, 60%)', label: 'سبز آبی' },
                        { color: 'hsl(180, 75%, 60%)', label: 'آبی روشن' },
                        { color: 'hsl(210, 75%, 60%)', label: 'آبی' },
                        { color: 'hsl(240, 75%, 60%)', label: 'بنفش' },
                        { color: 'hsl(270, 75%, 60%)', label: 'بنفش روشن' }
                    ]
                },
                alignment: {
                    options: ['left', 'center', 'right', 'justify']
                },
                link: {
                    addTargetToExternalLinks: true,
                    defaultProtocol: 'https://'
                },
                table: {
                    contentToolbar: [
                        'tableColumn',
                        'tableRow',
                        'mergeTableCells'
                    ]
                }
            });

            // Store editor reference
            this.editors.set(editorElement, editor);

            // Add event listeners
            editor.model.document.on('change:data', () => {
                console.log('CKEditorManager: Content changed in block:', blockId);
                this.handleContentChange(editor, blockElement);
            });

            // Also listen for input events
            editor.editing.view.document.on('input', () => {
                console.log('CKEditorManager: Input event in block:', blockId);
                this.handleContentChange(editor, blockElement);
            });

            // Listen for blur events
            editor.editing.view.document.on('blur', () => {
                console.log('CKEditorManager: Blur event in block:', blockId);
                this.handleContentChange(editor, blockElement);
            });

            // Listen for keyup events
            editor.editing.view.document.on('keyup', () => {
                console.log('CKEditorManager: Keyup event in block:', blockId);
                this.handleContentChange(editor, blockElement);
            });

            // Add periodic content check as fallback
            const intervalId = setInterval(() => {
                const currentContent = editor.getData();
                const lastContent = editorElement.dataset.lastContent || '';
                
                if (currentContent !== lastContent) {
                    console.log('CKEditorManager: Periodic check detected content change in block:', blockId);
                    editorElement.dataset.lastContent = currentContent;
                    this.handleContentChange(editor, blockElement);
                }
            }, 2000); // Check every 2 seconds

            // Store interval ID for cleanup
            editorElement.dataset.intervalId = intervalId;

            console.log('CKEditorManager: Editor initialized for block:', blockId);

        } catch (error) {
            console.error('CKEditorManager: Error initializing editor:', error);
        }
    }

    handleContentChange(editor, blockElement) {
        if (!blockElement) {
            console.warn('CKEditorManager: No block element found');
            return;
        }

        const blockId = blockElement.dataset.blockId;
        const content = editor.getData();
        const textContent = editor.getData().replace(/<[^>]*>/g, ''); // Strip HTML tags

        console.log('CKEditorManager: Handling content change for block:', blockId);
        console.log('CKEditorManager: Content:', content);
        console.log('CKEditorManager: Text content:', textContent);

        // Update block data attribute immediately
        if (blockElement.dataset.blockData) {
            try {
                const blockData = JSON.parse(blockElement.dataset.blockData);
                blockData.content = content;
                blockData.textContent = textContent;
                blockElement.dataset.blockData = JSON.stringify(blockData);
                console.log('CKEditorManager: Updated block data attribute:', blockData);
                
                // Update the actual block object in the content builder
                this.updateBlockInContentBuilder(blockElement, blockData);
                
                // Force update hidden field immediately
                this.forceUpdateHiddenField(blockElement, blockData);
                
                // Also update the main content field
                this.updateMainContentField();
                
            } catch (e) {
                console.error('CKEditorManager: Error updating block data:', e);
            }
        }

        // Dispatch custom event for content change
        const event = new CustomEvent('blockContentChanged', {
            detail: {
                blockElement: blockElement,
                content: content,
                textContent: textContent
            }
        });
        document.dispatchEvent(event);
        
        console.log('CKEditorManager: Dispatched blockContentChanged event');
    }

    updateBlockInContentBuilder(blockElement, blockData) {
        console.log('CKEditorManager: Updating block in content builder');
        
        // Find the content builder that owns this block
        const blocksList = blockElement.closest('.content-blocks-list');
        if (!blocksList) {
            console.warn('CKEditorManager: No blocks list found');
            return;
        }
        
        // Find the content builder instance
        const blockId = blockElement.dataset.blockId;
        
        // Try to find the content builder through various methods
        let contentBuilder = null;
        
        // Method 1: Look for reminderBlockManager
        if (window.reminderBlockManager) {
            contentBuilder = window.reminderBlockManager;
            console.log('CKEditorManager: Found reminderBlockManager');
        }
        
        // Method 2: Look for writtenBlockManager
        if (!contentBuilder && window.writtenBlockManager) {
            contentBuilder = window.writtenBlockManager;
            console.log('CKEditorManager: Found writtenBlockManager');
        }
        
        // Method 3: Look for step4Manager
        if (!contentBuilder && window.step4Manager) {
            // Check if this is a reminder or written block
            const isReminderBlock = blocksList.closest('#reminderContent');
            const isWrittenBlock = blocksList.closest('#writtenContent');
            
            if (isReminderBlock && window.step4Manager.reminderBlockManager) {
                contentBuilder = window.step4Manager.reminderBlockManager;
                console.log('CKEditorManager: Found reminderBlockManager through step4Manager');
            } else if (isWrittenBlock && window.step4Manager.writtenBlockManager) {
                contentBuilder = window.step4Manager.writtenBlockManager;
                console.log('CKEditorManager: Found writtenBlockManager through step4Manager');
            }
        }
        
        if (contentBuilder && contentBuilder.blocks) {
            console.log('CKEditorManager: Updating block in content builder blocks array');
            
            // Find and update the block in the blocks array
            const blockIndex = contentBuilder.blocks.findIndex(b => b.id === blockId);
            if (blockIndex !== -1) {
                contentBuilder.blocks[blockIndex].data = { ...contentBuilder.blocks[blockIndex].data, ...blockData };
                console.log('CKEditorManager: Updated block in content builder:', contentBuilder.blocks[blockIndex]);
                
                // Force update hidden field through content builder
                if (typeof contentBuilder.updateHiddenField === 'function') {
                    contentBuilder.updateHiddenField();
                    console.log('CKEditorManager: Called content builder updateHiddenField');
                }
            } else {
                console.warn('CKEditorManager: Block not found in content builder blocks array');
            }
        } else {
            console.warn('CKEditorManager: Content builder not found or has no blocks array');
        }
    }

    forceUpdateHiddenField(blockElement, blockData) {
        console.log('CKEditorManager: Force updating hidden field');
        
        // Find the content builder that owns this block
        const blocksList = blockElement.closest('.content-blocks-list');
        if (!blocksList) {
            console.warn('CKEditorManager: No blocks list found');
            return;
        }
        
        // Find the hidden field
        const hiddenField = blocksList.closest('form')?.querySelector('input[type="hidden"][id$="ContentJson"]');
        if (!hiddenField) {
            console.warn('CKEditorManager: No hidden field found');
            return;
        }
        
        console.log('CKEditorManager: Found hidden field:', hiddenField);
        
        try {
            // Parse current data
            const currentData = JSON.parse(hiddenField.value || '{}');
            const blocks = currentData.blocks || [];
            
            // Find and update the block
            const blockId = blockElement.dataset.blockId;
            const blockIndex = blocks.findIndex(b => b.id === blockId);
            
            if (blockIndex !== -1) {
                blocks[blockIndex].data = { ...blocks[blockIndex].data, ...blockData };
                currentData.blocks = blocks;
                hiddenField.value = JSON.stringify(currentData);
                
                console.log('CKEditorManager: Updated hidden field:', currentData);
            } else {
                console.warn('CKEditorManager: Block not found in hidden field data');
            }
        } catch (e) {
            console.error('CKEditorManager: Error updating hidden field:', e);
        }
    }

    updateMainContentField() {
        console.log('CKEditorManager: Updating main content field');
        
        // Find the main content field
        const mainContentField = document.getElementById('contentJson');
        if (!mainContentField) {
            console.warn('CKEditorManager: Main content field not found');
            return;
        }
        
        // Try to get content from reminder or written content managers
        let contentData = null;
        
        if (window.reminderBlockManager && typeof window.reminderBlockManager.getContent === 'function') {
            contentData = window.reminderBlockManager.getContent();
            console.log('CKEditorManager: Got content from reminderBlockManager:', contentData);
        } else if (window.writtenBlockManager && typeof window.writtenBlockManager.getContent === 'function') {
            contentData = window.writtenBlockManager.getContent();
            console.log('CKEditorManager: Got content from writtenBlockManager:', contentData);
        }
        
        if (contentData) {
            const contentJson = JSON.stringify(contentData);
            mainContentField.value = contentJson;
            console.log('CKEditorManager: Updated main content field with:', contentJson);
        }
    }

    populateTextBlock(blockElement, data) {
        const editorElement = blockElement.querySelector('.ckeditor-editor');
        if (!editorElement) {
            console.warn('CKEditorManager: No editor element found in block element');
            return;
        }

        console.log('CKEditorManager: Attempting to populate text block with data:', data);

        const editor = this.editors.get(editorElement);
        if (editor) {
            console.log('CKEditorManager: Editor found, setting data:', data.content || '');
            editor.setData(data.content || '');
        } else {
            console.warn('CKEditorManager: Editor not found for editor element, waiting for initialization...');
            
            // Wait for editor to be initialized
            const checkEditor = () => {
                const editor = this.editors.get(editorElement);
                if (editor) {
                    console.log('CKEditorManager: Editor now available, setting data:', data.content || '');
                    editor.setData(data.content || '');
                } else {
                    // Try again after a short delay
                    setTimeout(checkEditor, 100);
                }
            };
            
            // Start checking after a short delay
            setTimeout(checkEditor, 100);
        }
    }

    getEditorContent(editorElement) {
        const editor = this.editors.get(editorElement);
        if (editor) {
            return {
                html: editor.getData(),
                text: editor.getData().replace(/<[^>]*>/g, ''),
                isEmpty: editor.getData().replace(/<[^>]*>/g, '').trim() === ''
            };
        }
        return null;
    }

    setEditorContent(editorElement, content) {
        const editor = this.editors.get(editorElement);
        if (editor) {
            editor.setData(content);
        }
    }

    destroyEditor(editorElement) {
        const editor = this.editors.get(editorElement);
        if (editor) {
            // Clear interval if exists
            const intervalId = editorElement.dataset.intervalId;
            if (intervalId) {
                clearInterval(intervalId);
                delete editorElement.dataset.intervalId;
            }
            
            editor.destroy();
            this.editors.delete(editorElement);
        }
    }

    destroyAllEditors() {
        this.editors.forEach((editor, editorElement) => {
            editor.destroy();
        });
        this.editors.clear();
    }
    }

    // Global instance
    window.ckeditorManager = new window.CKEditorManager();

    // Export for module systems
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = window.CKEditorManager;
    }
}
