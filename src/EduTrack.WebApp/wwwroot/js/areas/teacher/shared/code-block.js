/**
 * Code Block Manager
 * Handles code block functionality including syntax highlighting and formatting
 */

// Define CodeBlockManager class globally (with duplicate protection)
if (typeof window.CodeBlockManager === 'undefined') {
window.CodeBlockManager = class CodeBlockManager {
    constructor(options = {}) {
        this.isInitialized = false;
        this.supportedLanguages = options.supportedLanguages || [
            'javascript', 'python', 'csharp', 'java', 'cpp', 'c', 'php', 'ruby',
            'go', 'rust', 'swift', 'kotlin', 'typescript', 'html', 'css', 'scss',
            'sql', 'json', 'xml', 'yaml', 'markdown', 'bash', 'powershell', 'plaintext'
        ];
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        this.setupEventListeners();
        this.initializeCodeEditors();
        this.isInitialized = true;
    }

    setupEventListeners() {
        // Handle settings changes
        document.addEventListener('change', (e) => {
            if (e.target.closest('.code-block-settings [data-setting]')) {
                const setting = e.target.dataset.setting;
                const value = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
                const blockElement = e.target.closest('.content-block-template');
                this.updateCodeSettings(blockElement, setting, value);
            }
        });

        // Handle code content changes
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('code-textarea')) {
                const blockElement = e.target.closest('.content-block-template');
                this.handleCodeContentChange(blockElement, e.target.value);
            }
        });

        // Handle code actions
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action="copy-code"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="copy-code"]');
                const blockElement = button.closest('.content-block-template');
                this.copyCode(blockElement);
            }
            
            if (e.target.closest('[data-action="format-code"]')) {
                e.preventDefault();
                const button = e.target.closest('[data-action="format-code"]');
                const blockElement = button.closest('.content-block-template');
                this.formatCode(blockElement);
            }
        });

        // Handle language badge clicks
        document.addEventListener('click', (e) => {
            if (e.target.closest('.code-language-badge')) {
                const blockElement = e.target.closest('.content-block-template');
                const languageSelect = blockElement.querySelector('[data-setting="language"]');
                if (languageSelect) {
                    languageSelect.focus();
                }
            }
        });

        // Handle populate block content events
        document.addEventListener('populateBlockContent', (e) => {
            if (e.detail.blockType === 'code') {
                if (window.codeBlockManager && typeof window.codeBlockManager.populateCodeBlock === 'function') {
                    window.codeBlockManager.populateCodeBlock(e.detail.blockElement, e.detail.block.data);
                } else {
                    console.warn('CodeBlockManager not available, trying to initialize...');
                    // Try to initialize if not available
                    if (typeof initializeCodeBlocks === 'function') {
                        initializeCodeBlocks();
                        if (window.codeBlockManager && typeof window.codeBlockManager.populateCodeBlock === 'function') {
                            window.codeBlockManager.populateCodeBlock(e.detail.blockElement, e.detail.block.data);
                        }
                    }
                }
            }
        });
    }

    initializeCodeEditors() {
        const codeEditors = document.querySelectorAll('.code-editor-container');
        codeEditors.forEach(editor => {
            this.setupCodeEditor(editor);
        });
    }

    setupCodeEditor(editorContainer) {
        const textarea = editorContainer.querySelector('.code-textarea');
        const preview = editorContainer.querySelector('.code-preview');
        const languageSelect = editorContainer.closest('.content-block-template').querySelector('[data-setting="language"]');
        
        if (!textarea || !preview) return;
        
        // Set initial values
        const language = languageSelect ? languageSelect.value : 'plaintext';
        this.updateLanguageBadge(editorContainer, language);
        this.updateCodePreview(textarea, preview, language);
        this.updateCodeStats(textarea);
        
        // Store editor reference
        editorContainer.dataset.editorInitialized = 'true';
    }

    updateCodeSettings(blockElement, setting, value) {
        const editorContainer = blockElement.querySelector('.code-editor-container');
        
        if (!editorContainer) return;
        
        switch (setting) {
            case 'language':
                this.updateLanguageBadge(editorContainer, value);
                this.updateCodePreview(
                    editorContainer.querySelector('.code-textarea'),
                    editorContainer.querySelector('.code-preview'),
                    value
                );
                break;
            case 'theme':
                this.updateCodeTheme(editorContainer, value);
                break;
            case 'size':
                this.updateCodeSize(editorContainer, value);
                break;
            case 'title':
                // Handle title if needed
                break;
            case 'showLineNumbers':
                this.updateLineNumbers(editorContainer, value);
                break;
            case 'enableCopyButton':
                this.updateCopyButton(editorContainer, value);
                break;
        }
        
        this.updateBlockData(blockElement, { [setting]: value });
        this.triggerBlockChange(blockElement);
    }

    updateLanguageBadge(editorContainer, language) {
        const badge = editorContainer.querySelector('.language-name');
        if (badge) {
            badge.textContent = this.getLanguageDisplayName(language);
        }
    }

    getLanguageDisplayName(language) {
        const languageNames = {
            'javascript': 'JavaScript',
            'python': 'Python',
            'csharp': 'C#',
            'java': 'Java',
            'cpp': 'C++',
            'c': 'C',
            'php': 'PHP',
            'ruby': 'Ruby',
            'go': 'Go',
            'rust': 'Rust',
            'swift': 'Swift',
            'kotlin': 'Kotlin',
            'typescript': 'TypeScript',
            'html': 'HTML',
            'css': 'CSS',
            'scss': 'SCSS',
            'sql': 'SQL',
            'json': 'JSON',
            'xml': 'XML',
            'yaml': 'YAML',
            'markdown': 'Markdown',
            'bash': 'Bash',
            'powershell': 'PowerShell',
            'plaintext': 'Plain Text'
        };
        
        return languageNames[language] || language;
    }

    updateCodeTheme(editorContainer, theme) {
        // Remove existing theme classes
        editorContainer.className = editorContainer.className.replace(/theme-\w+/g, '');
        
        // Add new theme class
        if (theme !== 'default') {
            editorContainer.classList.add(`theme-${theme}`);
        }
    }

    updateCodeSize(editorContainer, size) {
        // Remove existing size classes
        editorContainer.className = editorContainer.className.replace(/size-\w+/g, '');
        
        // Add new size class
        if (size !== 'medium') {
            editorContainer.classList.add(`size-${size}`);
        }
    }

    updateLineNumbers(editorContainer, showLineNumbers) {
        if (showLineNumbers) {
            editorContainer.classList.add('show-line-numbers');
        } else {
            editorContainer.classList.remove('show-line-numbers');
        }
        
        this.updateLineNumbersDisplay(editorContainer);
    }

    updateCopyButton(editorContainer, enableCopyButton) {
        if (enableCopyButton) {
            editorContainer.classList.add('enable-copy-button');
        } else {
            editorContainer.classList.remove('enable-copy-button');
        }
    }

    updateCodePreview(textarea, preview, language) {
        if (!textarea || !preview) return;
        
        const content = textarea.value;
        preview.textContent = content;
        
        // Apply basic syntax highlighting
        this.applySyntaxHighlighting(preview, language);
        
        // Update line numbers if enabled
        const editorContainer = textarea.closest('.code-editor-container');
        if (editorContainer && editorContainer.classList.contains('show-line-numbers')) {
            this.updateLineNumbersDisplay(editorContainer);
        }
    }

    applySyntaxHighlighting(preview, language) {
        // Basic syntax highlighting implementation
        const content = preview.textContent;
        
        if (language === 'javascript' || language === 'typescript') {
            preview.innerHTML = this.highlightJavaScript(content);
        } else if (language === 'python') {
            preview.innerHTML = this.highlightPython(content);
        } else if (language === 'html') {
            preview.innerHTML = this.highlightHTML(content);
        } else if (language === 'css') {
            preview.innerHTML = this.highlightCSS(content);
        } else if (language === 'json') {
            preview.innerHTML = this.highlightJSON(content);
        } else {
            preview.innerHTML = content;
        }
    }

    highlightJavaScript(content) {
        return content
            .replace(/\b(function|const|let|var|if|else|for|while|return|class|import|export)\b/g, '<span class="keyword">$1</span>')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/\/\/.*$/gm, '<span class="comment">$&</span>')
            .replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>')
            .replace(/\b\d+(\.\d+)?\b/g, '<span class="number">$&</span>');
    }

    highlightPython(content) {
        return content
            .replace(/\b(def|class|if|else|elif|for|while|return|import|from|try|except|finally|with|as)\b/g, '<span class="keyword">$1</span>')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/#.*$/gm, '<span class="comment">$&</span>')
            .replace(/\b\d+(\.\d+)?\b/g, '<span class="number">$&</span>');
    }

    highlightHTML(content) {
        return content
            .replace(/&lt;(\/?[^&]+)&gt;/g, '<span class="keyword">&lt;$1&gt;</span>')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/<!--[\s\S]*?-->/g, '<span class="comment">$&</span>');
    }

    highlightCSS(content) {
        return content
            .replace(/([.#]?[a-zA-Z-]+)\s*\{/g, '<span class="keyword">$1</span> {')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>')
            .replace(/\b\d+(\.\d+)?(px|em|rem|%|vh|vw)\b/g, '<span class="number">$&</span>');
    }

    highlightJSON(content) {
        return content
            .replace(/("(?:[^"\\]|\\.)*")\s*:/g, '<span class="keyword">$1</span>:')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/\b(true|false|null)\b/g, '<span class="keyword">$1</span>')
            .replace(/\b\d+(\.\d+)?\b/g, '<span class="number">$&</span>');
    }

    updateLineNumbersDisplay(editorContainer) {
        const textarea = editorContainer.querySelector('.code-textarea');
        const preview = editorContainer.querySelector('.code-preview');
        
        if (!textarea || !preview) return;
        
        const lines = textarea.value.split('\n');
        const lineNumbers = lines.map((_, index) => (index + 1).toString().padStart(3, ' ')).join('\n');
        preview.dataset.lineNumbers = lineNumbers;
    }

    handleCodeContentChange(blockElement, content) {
        const editorContainer = blockElement.querySelector('.code-editor-container');
        const textarea = editorContainer.querySelector('.code-textarea');
        const preview = editorContainer.querySelector('.code-preview');
        const languageSelect = blockElement.querySelector('[data-setting="language"]');
        
        if (textarea && preview) {
            const language = languageSelect ? languageSelect.value : 'plaintext';
            this.updateCodePreview(textarea, preview, language);
            this.updateCodeStats(textarea);
        }
        
        this.updateBlockData(blockElement, { codeContent: content });
        this.triggerBlockChange(blockElement);
    }

    updateCodeStats(textarea) {
        const editorContainer = textarea.closest('.code-editor-container');
        const lineCount = editorContainer.querySelector('[data-line-count]');
        const charCount = editorContainer.querySelector('[data-char-count]');
        
        if (lineCount) {
            const lines = textarea.value.split('\n').length;
            lineCount.textContent = lines;
        }
        
        if (charCount) {
            charCount.textContent = textarea.value.length;
        }
    }

    async copyCode(blockElement) {
        const textarea = blockElement.querySelector('.code-textarea');
        const copyButton = blockElement.querySelector('[data-action="copy-code"]');
        
        if (!textarea) return;
        
        try {
            await navigator.clipboard.writeText(textarea.value);
            
            // Visual feedback
            if (copyButton) {
                copyButton.classList.add('copied');
                setTimeout(() => {
                    copyButton.classList.remove('copied');
                }, 2000);
            }
            
            this.showSuccess('کد کپی شد');
        } catch (error) {
            console.error('Error copying code:', error);
            this.showError('خطا در کپی کردن کد');
        }
    }

    formatCode(blockElement) {
        const textarea = blockElement.querySelector('.code-textarea');
        const languageSelect = blockElement.querySelector('[data-setting="language"]');
        
        if (!textarea) return;
        
        const language = languageSelect ? languageSelect.value : 'plaintext';
        const content = textarea.value;
        
        // Basic formatting based on language
        let formattedContent = content;
        
        switch (language) {
            case 'javascript':
            case 'typescript':
                formattedContent = this.formatJavaScript(content);
                break;
            case 'python':
                formattedContent = this.formatPython(content);
                break;
            case 'json':
                formattedContent = this.formatJSON(content);
                break;
            default:
                formattedContent = this.formatGeneric(content);
        }
        
        textarea.value = formattedContent;
        this.handleCodeContentChange(blockElement, formattedContent);
        this.showSuccess('کد فرمت شد');
    }

    formatJavaScript(content) {
        // Basic JavaScript formatting
        return content
            .replace(/\{/g, ' {\n')
            .replace(/\}/g, '\n}\n')
            .replace(/;/g, ';\n')
            .replace(/\n\s*\n/g, '\n')
            .trim();
    }

    formatPython(content) {
        // Basic Python formatting
        return content
            .replace(/:/g, ':\n')
            .replace(/\n\s*\n/g, '\n')
            .trim();
    }

    formatJSON(content) {
        try {
            const parsed = JSON.parse(content);
            return JSON.stringify(parsed, null, 2);
        } catch (error) {
            return content;
        }
    }

    formatGeneric(content) {
        // Basic generic formatting
        return content
            .replace(/\n\s*\n/g, '\n')
            .trim();
    }

    updateBlockData(blockElement, data) {
        // Store data in block element for later retrieval
        if (!blockElement.dataset.blockData) {
            blockElement.dataset.blockData = '{}';
        }
        
        const currentData = JSON.parse(blockElement.dataset.blockData);
        const updatedData = { ...currentData, ...data };
        blockElement.dataset.blockData = JSON.stringify(updatedData);
    }

    getBlockData(blockElement) {
        if (!blockElement.dataset.blockData) {
            return {};
        }
        
        try {
            return JSON.parse(blockElement.dataset.blockData);
        } catch (error) {
            console.error('Error parsing block data:', error);
            return {};
        }
    }

    triggerBlockChange(blockElement) {
        const event = new CustomEvent('blockContentChanged', {
            detail: {
                blockElement: blockElement,
                blockData: this.getBlockData(blockElement)
            }
        });
        blockElement.dispatchEvent(event);
    }

    showSuccess(message) {
        this.showNotification(message, 'success');
    }

    showError(message) {
        this.showNotification(message, 'error');
    }

    showNotification(message, type) {
        const notificationDiv = document.createElement('div');
        notificationDiv.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`;
        notificationDiv.style.position = 'fixed';
        notificationDiv.style.top = '20px';
        notificationDiv.style.right = '20px';
        notificationDiv.style.zIndex = '9999';
        notificationDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notificationDiv);
        
        // Auto-remove after 3 seconds
        setTimeout(() => {
            if (notificationDiv.parentNode) {
                notificationDiv.parentNode.removeChild(notificationDiv);
            }
        }, 3000);
    }

    // Public method to get code data
    getCodeData(blockElement) {
        return this.getBlockData(blockElement);
    }

    // Public method to set code data
    setCodeData(blockElement, data) {
        this.updateBlockData(blockElement, data);
        
        if (data.codeContent) {
            const textarea = blockElement.querySelector('.code-textarea');
            if (textarea) {
                textarea.value = data.codeContent;
                this.handleCodeContentChange(blockElement, data.codeContent);
            }
        }
        
        if (data.language) {
            const languageSelect = blockElement.querySelector('[data-setting="language"]');
            if (languageSelect) {
                languageSelect.value = data.language;
                this.updateCodeSettings(blockElement, 'language', data.language);
            }
        }
        
        if (data.theme) {
            this.updateCodeSettings(blockElement, 'theme', data.theme);
        }
        
        if (data.size) {
            this.updateCodeSettings(blockElement, 'size', data.size);
        }
        
        if (data.title) {
            const titleInput = blockElement.querySelector('[data-setting="title"]');
            if (titleInput) {
                titleInput.value = data.title;
            }
        }
        
        if (data.showLineNumbers !== undefined) {
            this.updateCodeSettings(blockElement, 'showLineNumbers', data.showLineNumbers);
        }
        
        if (data.enableCopyButton !== undefined) {
            this.updateCodeSettings(blockElement, 'enableCopyButton', data.enableCopyButton);
        }
    }

    // Public method to clear code
    clearCode(blockElement) {
        const textarea = blockElement.querySelector('.code-textarea');
        const preview = blockElement.querySelector('.code-preview');
        
        if (textarea) {
            textarea.value = '';
        }
        
        if (preview) {
            preview.textContent = '';
        }
        
        this.updateCodeStats(textarea);
        this.updateBlockData(blockElement, {});
        this.triggerBlockChange(blockElement);
    }

    // Public method to populate code block content
    populateCodeBlock(blockElement, data) {
        // Populate code content
        const codeTextarea = blockElement.querySelector('textarea[name="codeContent"]');
        if (codeTextarea) {
            codeTextarea.value = data.codeContent || '';
        }
        
        // Populate language
        const languageSelect = blockElement.querySelector('select[name="language"]');
        if (languageSelect) languageSelect.value = data.language || 'plaintext';
        
        // Populate title
        const titleInput = blockElement.querySelector('input[name="codeTitle"]');
        if (titleInput) titleInput.value = data.codeTitle || '';
        
        // Populate theme
        const themeSelect = blockElement.querySelector('select[name="theme"]');
        if (themeSelect) themeSelect.value = data.theme || 'default';
    }
}

// Global functions for backward compatibility
function initializeCodeBlocks() {
    if (!window.codeBlockManager) {
        window.codeBlockManager = new CodeBlockManager();
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeCodeBlocks();
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CodeBlockManager;
}

} // End of CodeBlockManager class definition check
