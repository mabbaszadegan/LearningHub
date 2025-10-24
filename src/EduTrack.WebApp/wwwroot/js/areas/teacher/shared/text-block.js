/**
 * Text Block Manager
 * Handles text block functionality including rich text editing
 */

// Define TextBlockManager class globally (with duplicate protection)
if (typeof window.TextBlockManager === 'undefined') {
window.TextBlockManager = class TextBlockManager {
    constructor(options = {}) {
        this.isInitialized = false;
        this.richTextEditors = new Map();
        this.toolbarButtons = new Map();
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        this.setupEventListeners();
        this.initializeRichTextEditors();
        this.isInitialized = true;
    }

    setupEventListeners() {
        // Handle toolbar button clicks
        document.addEventListener('click', (e) => {
            if (e.target.closest('.toolbar-btn[data-command]')) {
                e.preventDefault();
                const button = e.target.closest('.toolbar-btn[data-command]');
                const command = button.dataset.command;
                const editor = button.closest('.text-editor-toolbar').nextElementSibling;
                
                if (editor && editor.classList.contains('rich-text-editor')) {
                    this.executeCommand(command, editor);
                    this.updateToolbarState(editor);
                }
            }
        });

        // Handle rich text editor events
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('rich-text-editor')) {
                this.handleEditorInput(e.target);
            }
        });

        document.addEventListener('keyup', (e) => {
            if (e.target.classList.contains('rich-text-editor')) {
                this.updateToolbarState(e.target);
            }
        });

        document.addEventListener('selectionchange', () => {
            const activeEditor = document.activeElement;
            if (activeEditor && activeEditor.classList.contains('rich-text-editor')) {
                this.updateToolbarState(activeEditor);
            }
        });

        // Handle special toolbar actions
        document.addEventListener('click', (e) => {
            if (e.target.closest('.toolbar-btn[data-action="text-formatting"]')) {
                e.preventDefault();
                this.showTextFormattingModal();
            }
            
            if (e.target.closest('.toolbar-btn[data-action="create-link"]')) {
                e.preventDefault();
                this.showLinkDialog();
            }
            
            if (e.target.closest('.toolbar-btn[data-action="insert-code"]')) {
                e.preventDefault();
                this.showCodeBlockModal();
            }
        });

        // Handle populate block content events
        document.addEventListener('populateBlockContent', (e) => {      
            // Handle both regular text blocks and question text blocks
            if (e.detail.blockType === 'text' || e.detail.blockType === 'questionText') {
                if (window.textBlockManager && typeof window.textBlockManager.populateTextBlock === 'function') {
                    window.textBlockManager.populateTextBlock(e.detail.blockElement, e.detail.block.data);
                } else {
                    console.warn('TextBlockManager not available, trying to initialize...');
                    // Try to initialize if not available
                    if (typeof initializeTextBlocks === 'function') {
                        initializeTextBlocks();
                        if (window.textBlockManager && typeof window.textBlockManager.populateTextBlock === 'function') {
                            window.textBlockManager.populateTextBlock(e.detail.blockElement, e.detail.block.data);
                        }
                    }
                }
            }
        });
    }

    initializeRichTextEditors() {
        const editors = document.querySelectorAll('.rich-text-editor');
        editors.forEach(editor => {
            this.setupRichTextEditor(editor);
        });
    }

    setupRichTextEditor(editor) {
        // Store editor reference
        this.richTextEditors.set(editor, {
            element: editor,
            toolbar: editor.previousElementSibling,
            isActive: false
        });

        // Add placeholder behavior
        editor.addEventListener('focus', () => {
            if (editor.textContent.trim() === '') {
                editor.innerHTML = '';
            }
        });

        editor.addEventListener('blur', () => {
            if (editor.textContent.trim() === '') {
                editor.innerHTML = '';
            }
        });
    }

    executeCommand(command, editor) {
        editor.focus();
        
        switch (command) {
            case 'bold':
                document.execCommand('bold', false, null);
                break;
            case 'italic':
                document.execCommand('italic', false, null);
                break;
            case 'underline':
                document.execCommand('underline', false, null);
                break;
            case 'insertUnorderedList':
                document.execCommand('insertUnorderedList', false, null);
                break;
            case 'insertOrderedList':
                document.execCommand('insertOrderedList', false, null);
                break;
            default:
                console.warn(`Unknown command: ${command}`);
        }
    }

    updateToolbarState(editor) {
        const editorData = this.richTextEditors.get(editor);
        if (!editorData || !editorData.toolbar) return;

        const toolbar = editorData.toolbar;
        const buttons = toolbar.querySelectorAll('.toolbar-btn[data-command]');
        
        buttons.forEach(button => {
            const command = button.dataset.command;
            const isActive = document.queryCommandState(command);
            
            if (isActive) {
                button.classList.add('active');
            } else {
                button.classList.remove('active');
            }
        });
    }

    handleEditorInput(editor) {
        
        // Trigger change event for the block
        const blockElement = editor.closest('.content-block-template, .content-block');
        if (blockElement) {
            // Dispatch event on document instead of blockElement
            const event = new CustomEvent('blockContentChanged', {
                detail: {
                    blockElement: blockElement,
                    content: editor.innerHTML,
                    textContent: editor.textContent
                }
            });
            document.dispatchEvent(event);
            
        } else {
            console.warn('TextBlockManager: Block element not found for editor');
        }
    }

    showTextFormattingModal() {
        const modal = document.getElementById('textFormattingModal');
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } else {
            console.warn('Text formatting modal not found');
        }
    }

    showLinkDialog() {
        const modal = document.getElementById('linkDialogModal');
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } else {
            console.warn('Link dialog modal not found');
        }
    }

    showCodeBlockModal() {
        const modal = document.getElementById('codeBlockModal');
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } else {
            console.warn('Code block modal not found');
        }
    }

    // Public method to get editor content
    getEditorContent(editor) {
        return {
            html: editor.innerHTML,
            text: editor.textContent,
            isEmpty: editor.textContent.trim() === ''
        };
    }

    // Public method to set editor content
    setEditorContent(editor, content) {
        editor.innerHTML = content;
        this.updateToolbarState(editor);
    }

    // Public method to clear editor content
    clearEditorContent(editor) {
        editor.innerHTML = '';
        this.updateToolbarState(editor);
    }

    // Public method to insert text at cursor
    insertTextAtCursor(editor, text) {
        editor.focus();
        document.execCommand('insertText', false, text);
    }

    // Public method to insert HTML at cursor
    insertHtmlAtCursor(editor, html) {
        editor.focus();
        document.execCommand('insertHTML', false, html);
    }

    // Public method to populate text block content
    populateTextBlock(blockElement, data) {
        // Find rich text editor or textarea
        const editor = blockElement.querySelector('.rich-text-editor');
        const textarea = blockElement.querySelector('textarea');
        
        if (editor) {
            editor.innerHTML = data.content || '';
            this.updateToolbarState(editor);
        } else if (textarea) {
            textarea.value = data.content || '';
        }

        // Populate question-specific fields if this is a question block
        this.populateQuestionFields(blockElement, data);
    }

    // Helper method to populate question-specific fields
    populateQuestionFields(blockElement, data) {
        // Check if this is a question block by looking for question settings
        const questionSettings = blockElement.querySelector('.question-settings');
        if (!questionSettings) return;

        // Populate points field
        const pointsInput = questionSettings.querySelector('[data-setting="points"]');
        if (pointsInput && data.points !== undefined) {
            pointsInput.value = data.points;
        }

        // Populate difficulty field
        const difficultySelect = questionSettings.querySelector('[data-setting="difficulty"]');
        if (difficultySelect && data.difficulty !== undefined) {
            difficultySelect.value = data.difficulty;
        }

        // Populate required checkbox
        const requiredCheckbox = questionSettings.querySelector('[data-setting="isRequired"]');
        if (requiredCheckbox && data.isRequired !== undefined) {
            requiredCheckbox.checked = data.isRequired;
        }

        // Populate teacher guidance (hint)
        const hintTextarea = blockElement.querySelector('[data-hint="true"]');
        if (hintTextarea && data.teacherGuidance !== undefined) {
            hintTextarea.value = data.teacherGuidance;
        }

        // Populate question text content for different editor types
        this.populateQuestionText(blockElement, data);
    }

    // Helper method to populate question text content
    populateQuestionText(blockElement, data) {
        // Try CKEditor first (for text blocks)
        const ckEditor = blockElement.querySelector('.ckeditor-editor');
        if (ckEditor && data.content) {
            // For CKEditor, we need to wait for it to be initialized
            // The CKEditor manager will handle this
            return;
        }

        // Try rich text editor (for image/video/audio blocks)
        const richTextEditor = blockElement.querySelector('.rich-text-editor');
        if (richTextEditor && data.content) {
            richTextEditor.innerHTML = data.content;
            this.updateToolbarState(richTextEditor);
        }

        // Try textarea as fallback
        const textarea = blockElement.querySelector('textarea');
        if (textarea && data.content && !textarea.hasAttribute('data-hint')) {
            textarea.value = data.content;
        }
    }
}

// Global functions for backward compatibility
function initializeTextBlocks() {
    if (!window.textBlockManager) {
        window.textBlockManager = new TextBlockManager();
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeTextBlocks();
});

// Re-initialize when new content is added dynamically
document.addEventListener('DOMNodeInserted', function(e) {
    if (e.target.classList && e.target.classList.contains('rich-text-editor')) {
        if (window.textBlockManager) {
            window.textBlockManager.setupRichTextEditor(e.target);
        }
    }
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TextBlockManager;
}

} // End of TextBlockManager class definition check
