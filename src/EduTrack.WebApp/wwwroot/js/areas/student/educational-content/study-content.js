/**
 * Study Content JavaScript - Hidden Timer with Exit Confirmation
 * Handles hidden timer and elegant exit confirmation for ScheduleItem study
 * Includes beautiful reminder content rendering
 */

let studySession = {
    isActive: false,
    startTime: null,
    elapsedTime: 0,
    sessionId: null,
    updateInterval: null,
    fixedEndTime: null, // Fixed end time when modal is open
    isSaving: false, // Flag to prevent duplicate saves
    lastSavedStartTime: null, // Track the start time of the last saved session
    
    init() {
        this.sessionId = window.studyContentConfig?.activeSessionId || 0;
        this.bindEvents();
        // Start study session automatically (which starts the timer)
        this.startStudySession();
        
        // Initialize reminder content if needed (only if not already rendered server-side)
        if (window.studyContentConfig?.contentType === 'Reminder') {
            const container = document.getElementById('reminder-content');
            // Only initialize if container is empty (no server-side rendering)
            if (container && (!container.querySelector('.content-block') && !container.querySelector('.content-empty-state'))) {
                this.initializeReminderContent();
            }
        }
        
        // Apply automatic code highlighting to all code blocks on page load
        this.applyAutomaticCodeHighlighting();
        
    },
    
    applyAutomaticCodeHighlighting() {
        // Wait for DOM to be fully loaded
        setTimeout(() => {
            
            // Find all code blocks in the page
            const codeBlocks = document.querySelectorAll('pre code, .text-content code, .content-description code');
            
            codeBlocks.forEach((codeElement, index) => {
                // Skip if already highlighted (has syntax highlighting classes)
                if (codeElement.classList.contains('highlighted') || 
                    codeElement.querySelector('.keyword') || 
                    codeElement.querySelector('.string')) {
                    return;
                }
                
                const codeContent = codeElement.textContent || codeElement.innerText;
                if (!codeContent || codeContent.trim().length === 0) {
                    return;
                }
                
                // Detect language from class or content
                let language = 'plaintext';
                const classList = Array.from(codeElement.classList);
                
                // Check for language class (language-javascript, language-python, etc.)
                const langClass = classList.find(cls => cls.startsWith('language-'));
                if (langClass) {
                    language = langClass.replace('language-', '');
                } else {
                    // Auto-detect language from content
                    language = this.detectLanguageFromContent(codeContent);
                }
                
                // Apply syntax highlighting
                const highlightedCode = this.highlightCodeSyntax(codeContent, language);
                
                // Create wrapper for the code block if it's in a pre tag
                const isInPre = codeElement.parentElement.tagName === 'PRE';
                if (isInPre) {
                    // Create a proper code block container
                    const codeId = `auto-code-${Date.now()}-${index}`;
                    const wrapper = document.createElement('div');
                    wrapper.className = 'content-block-code auto-highlighted';
                    wrapper.innerHTML = `
                        <div class="code-header">
                            <div class="code-language">
                                <i class="fas fa-code"></i>
                                <span>${this.getLanguageDisplayName(language)}</span>
                            </div>
                            <div class="code-actions">
                                <button type="button" class="copy-btn" onclick="studySession.copyCodeToClipboard('${codeId}'); return false;" title="کپی کد">
                                    <i class="fas fa-copy"></i>
                                    <span>کپی</span>
                                </button>
                            </div>
                        </div>
                        <div class="code-content">
                            <pre><code id="${codeId}" class="language-${language} theme-default" data-code-content="${this.escapeHtml(codeContent)}">${highlightedCode}</code></pre>
                        </div>
                    `;
                    
                    // Replace the original pre element with the new wrapper
                    codeElement.parentElement.parentElement.replaceChild(wrapper, codeElement.parentElement);
                } else {
                    // For inline code, just apply highlighting
                    codeElement.innerHTML = highlightedCode;
                    codeElement.classList.add('highlighted');
                }
                
            });
            
        }, 100); // Small delay to ensure DOM is ready
    },
    
    detectLanguageFromContent(codeContent) {
        const content = codeContent.toLowerCase();
        
        // JavaScript/TypeScript detection
        if (content.includes('function') && (content.includes('var ') || content.includes('let ') || content.includes('const '))) {
            return 'javascript';
        }
        
        // Python detection
        if (content.includes('def ') || content.includes('import ') || content.includes('from ') || content.includes('print(')) {
            return 'python';
        }
        
        // HTML detection
        if (content.includes('<html') || content.includes('<div') || content.includes('<span') || content.includes('<p>')) {
            return 'html';
        }
        
        // CSS detection
        if (content.includes('{') && content.includes('}') && (content.includes('color:') || content.includes('margin:') || content.includes('padding:'))) {
            return 'css';
        }
        
        // JSON detection
        if (content.trim().startsWith('{') && content.trim().endsWith('}')) {
            return 'json';
        }
        
        // SQL detection
        if (content.includes('select ') || content.includes('insert ') || content.includes('update ') || content.includes('delete ')) {
            return 'sql';
        }
        
        // C# detection
        if (content.includes('using ') || content.includes('namespace ') || content.includes('public class')) {
            return 'csharp';
        }
        
        return 'plaintext';
    },
    
    initializeReminderContent() {
        const contentJson = window.studyContentConfig?.contentJson;
        if (!contentJson) {
            console.warn('No content JSON found for reminder');
            this.showReminderError('محتوای یادآوری یافت نشد');
            return;
        }
        
        try {
            const reminderContent = typeof contentJson === 'string' ? JSON.parse(contentJson) : contentJson;
            this.renderReminderContent(reminderContent);
        } catch (error) {
            console.error('Error parsing reminder content:', error);
            this.showReminderError('خطا در بارگذاری محتوای یادآوری');
        }
    },
    
    renderReminderContent(content) {
        const container = document.getElementById('reminder-content');
        if (!container) {
            console.error('Reminder content container not found');
            return;
        }
        
        // Clear loading state
        container.innerHTML = '';
        
        // Create body
        const body = this.createReminderBody(content);
        container.appendChild(body);
        
    },
    
    createReminderBody(content) {
        const body = document.createElement('div');
        body.className = 'reminder-body';
        
        // Add main message if exists
        const mainMessage = content.message || content.Message;
        if (mainMessage && mainMessage.trim()) {
            const messageDiv = document.createElement('div');
            messageDiv.className = 'reminder-message';
            // Check if message is HTML or plain text
            const isHtml = /<[a-z][\s\S]*>/i.test(mainMessage);
            messageDiv.innerHTML = isHtml ? mainMessage : this.formatTextContent(mainMessage);
            body.appendChild(messageDiv);
        }
        
        // Add content blocks
        const blocks = content.blocks || content.Blocks;
        if (blocks && blocks.length > 0) {
            const blocksContainer = document.createElement('div');
            blocksContainer.className = 'content-blocks';
            
            // Sort blocks by order
            const sortedBlocks = blocks.sort((a, b) => (a.order || a.Order || 0) - (b.order || b.Order || 0));
            
            sortedBlocks.forEach(block => {
                const blockElement = this.createContentBlock(block);
                if (blockElement) {
                    blocksContainer.appendChild(blockElement);
                }
            });
            
            body.appendChild(blocksContainer);
        }
        
        return body;
    },
    
    createContentBlock(block) {
        const blockElement = document.createElement('div');
        const blockType = (block.type || block.Type || '').toString().toLowerCase();
        const blockTypeNum = typeof (block.type || block.Type) === 'number' ? block.type || block.Type : null;
        
        // Add base class and type-specific class to match server-side rendering
        const typeClass = blockTypeNum === 0 ? 'text' : (blockType || 'unknown');
        blockElement.className = `content-block content-block-${typeClass}`;
        
        // Add data attributes to match server-side rendering
        if (block.id || block.Id) {
            blockElement.setAttribute('data-block-id', block.id || block.Id);
        }
        if (block.order !== undefined || block.Order !== undefined) {
            blockElement.setAttribute('data-block-order', block.order || block.Order);
        }
        blockElement.setAttribute('data-block-type', 'content');
        
        // Handle text blocks (both string 'text' and number 0)
        if (blockType === 'text' || blockTypeNum === 0) {
            // Prefer Content (HTML) over TextContent (plain text)
            const htmlContent = block.data?.content || block.data?.Content || '';
            const plainTextContent = block.data?.textContent || block.data?.TextContent || '';
            const contentToDisplay = htmlContent || plainTextContent;
            
            // If content is HTML (has tags), display it directly without formatting
            // Otherwise, format plain text
            const isHtml = /<[a-z][\s\S]*>/i.test(contentToDisplay);
            const displayContent = isHtml ? contentToDisplay : this.formatTextContent(contentToDisplay);
            
            // Add inner wrapper to match server-side structure
            const innerWrapper = document.createElement('div');
            innerWrapper.className = 'content-block-inner';
            
            const textDiv = document.createElement('div');
            textDiv.className = 'content-block-text';
            textDiv.innerHTML = displayContent;
            
            innerWrapper.appendChild(textDiv);
            blockElement.appendChild(innerWrapper);
        } else {
            // Handle other block types with switch
            switch (blockType) {
                
            case 'image':
            case 1: // Image
                const imageUrl = block.data?.fileUrl || block.data?.FileUrl;
                const imageCaption = block.data?.caption || block.data?.Caption;
                if (imageUrl) {
                    const imageId = `image-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
                    
                    blockElement.innerHTML = `
                        <div class="content-block-image">
                            <img src="${imageUrl}" 
                                 alt="${imageCaption || 'تصویر'}" 
                                 loading="lazy" 
                                 id="${imageId}" 
                                 data-image-url="${imageUrl}" 
                                 data-image-caption="${imageCaption || ''}">
                            <div class="image-overlay">
                                <button type="button" class="image-action-btn" onclick="studySession.openImageModal('${imageId}'); return false;" title="بزرگنمایی">
                                    <i class="fas fa-search-plus"></i>
                                </button>
                                <button type="button" class="image-action-btn" onclick="studySession.downloadImage('${imageUrl}', '${imageCaption || 'تصویر'}'); return false;" title="دانلود">
                                    <i class="fas fa-download"></i>
                                </button>
                            </div>
                            ${imageCaption ? `<div class="image-caption">${imageCaption}</div>` : ''}
                        </div>
                    `;
                } else {
                    console.warn('Image block without file URL:', block);
                    return null;
                }
                break;
                
            case 'video':
            case 2: // Video
                const videoUrl = block.data?.fileUrl || block.data?.FileUrl;
                const videoMimeType = block.data?.mimeType || block.data?.MimeType || 'video/mp4';
                const videoCaption = block.data?.caption || block.data?.Caption;
                if (videoUrl) {
                    blockElement.innerHTML = `
                        <div class="content-block-video">
                            <video controls preload="metadata">
                                <source src="${videoUrl}" type="${videoMimeType}">
                                مرورگر شما از پخش ویدیو پشتیبانی نمی‌کند.
                            </video>
                            ${videoCaption ? `<div class="image-caption">${videoCaption}</div>` : ''}
                        </div>
                    `;
                } else {
                    console.warn('Video block without file URL:', block);
                    return null;
                }
                break;
                
            case 'audio':
            case 3: // Audio
                const audioUrl = block.data?.fileUrl || block.data?.FileUrl;
                const audioMimeType = block.data?.mimeType || block.data?.MimeType || 'audio/mpeg';
                const audioDuration = block.data?.duration || block.data?.Duration;
                const audioCaption = block.data?.caption || block.data?.Caption;
                if (audioUrl) {
                    const duration = audioDuration ? this.formatDuration(audioDuration) : '';
                    const audioId = `audio-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
                    
                    blockElement.innerHTML = `
                        <div class="content-block-audio">
                            <div class="audio-player-container">
                                <div class="custom-audio-player">
                                    <div class="speaker-button" id="speaker-${audioId}" data-audio-id="${audioId}">
                                        <i class="fas fa-volume-up speaker-icon"></i>
                                    </div>
                                    <div class="audio-title">فایل صوتی</div>
                                    <div class="audio-info">
                                        ${duration ? `<span class="audio-duration">${duration}</span>` : ''}
                                        <span class="audio-status" id="status-${audioId}">آماده پخش</span>
                                    </div>
                                    <div class="progress-container">
                                        <div class="progress-bar" id="progress-${audioId}">
                                            <div class="progress-fill" id="progress-fill-${audioId}"></div>
                                        </div>
                                        <div class="time-display">
                                            <span id="current-time-${audioId}">00:00</span>
                                            <span id="total-time-${audioId}">${duration || '00:00'}</span>
                                        </div>
                                    </div>
                                </div>
                                <audio class="hidden-audio" id="${audioId}" preload="metadata">
                                    <source src="${audioUrl}" type="${audioMimeType}">
                                    مرورگر شما از پخش صدا پشتیبانی نمی‌کند.
                                </audio>
                                ${audioCaption ? `<div class="audio-caption">${audioCaption}</div>` : ''}
                            </div>
                        </div>
                    `;
                    
                    // Initialize custom audio player after DOM is ready
                    setTimeout(() => {
                        this.initializeCustomAudioPlayer(audioId, audioUrl);
                    }, 100);
                } else {
                    console.warn('Audio block without file URL:', block);
                    return null;
                }
                break;
                
            case 'code':
            case 4: // Code
                const codeContent = block.data?.codeContent || block.data?.CodeContent || '';
                const language = block.data?.language || block.data?.Language || 'plaintext';
                const theme = block.data?.theme || block.data?.Theme || 'default';
                const codeTitle = block.data?.codeTitle || block.data?.CodeTitle || '';
                const showLineNumbers = block.data?.showLineNumbers !== false;
                const enableCopyButton = block.data?.enableCopyButton !== false;
                
                if (codeContent) {
                    const codeId = `code-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
                    
                    blockElement.innerHTML = `
                        <div class="content-block-code">
                            <div class="code-header">
                                ${codeTitle ? `<div class="code-title">${codeTitle}</div>` : ''}
                                <div class="code-language">
                                    <i class="fas fa-code"></i>
                                    <span>${this.getLanguageDisplayName(language)}</span>
                                </div>
                                ${enableCopyButton ? `
                                    <div class="code-actions">
                                        <button type="button" class="copy-btn" onclick="studySession.copyCodeToClipboard('${codeId}'); return false;" title="کپی کد">
                                            <i class="fas fa-copy"></i>
                                            <span>کپی</span>
                                        </button>
                                    </div>
                                ` : ''}
                            </div>
                            <div class="code-content">
                                <pre><code id="${codeId}" class="language-${language} theme-${theme}" data-code-content="${this.escapeHtml(codeContent)}">${this.highlightCodeSyntax(codeContent, language)}</code></pre>
                            </div>
                        </div>
                    `;
                } else {
                    console.warn('Code block without content:', block);
                    return null;
                }
                break;
                
            default:
                console.warn('Unknown content block type:', block.type);
                return null;
        }
        
        return blockElement;
    },
    
    formatTextContent(text) {
        if (!text) return '';
        
        // Convert line breaks to HTML
        let formatted = text.replace(/\n/g, '<br>');
        
        // Convert markdown-style formatting
        formatted = formatted.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
        formatted = formatted.replace(/\*(.*?)\*/g, '<em>$1</em>');
        formatted = formatted.replace(/`(.*?)`/g, '<code>$1</code>');
        
        // Convert URLs to links
        formatted = formatted.replace(/(https?:\/\/[^\s]+)/g, '<a href="$1" target="_blank" rel="noopener noreferrer">$1</a>');
        
        return formatted;
    },
    
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
        return languageNames[language] || 'Plain Text';
    },
    
    highlightCodeSyntax(code, language) {
        if (language === 'plaintext') {
            return this.escapeHtml(code);
        }
        
        let highlighted = this.escapeHtml(code);
        
        switch (language) {
            case 'javascript':
            case 'typescript':
                highlighted = this.highlightJavaScript(highlighted);
                break;
            case 'python':
                highlighted = this.highlightPython(highlighted);
                break;
            case 'html':
                highlighted = this.highlightHTML(highlighted);
                break;
            case 'css':
            case 'scss':
                highlighted = this.highlightCSS(highlighted);
                break;
            case 'json':
                highlighted = this.highlightJSON(highlighted);
                break;
            case 'sql':
                highlighted = this.highlightSQL(highlighted);
                break;
        }
        
        return highlighted;
    },
    
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },
    
    highlightJavaScript(code) {
        return code
            .replace(/\b(function|const|let|var|if|else|for|while|return|class|import|export|from|async|await|try|catch|finally|throw|new|this|typeof|instanceof|in|of|true|false|null|undefined)\b/g, '<span class="keyword">$1</span>')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/\/\/.*$/gm, '<span class="comment">$&</span>')
            .replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>')
            .replace(/\b\d+\.?\d*\b/g, '<span class="number">$&</span>');
    },
    
    highlightPython(code) {
        return code
            .replace(/\b(def|class|if|elif|else|for|while|try|except|finally|with|import|from|return|yield|lambda|and|or|not|in|is|True|False|None)\b/g, '<span class="keyword">$1</span>')
            .replace(/(["'`])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/#.*$/gm, '<span class="comment">$&</span>')
            .replace(/\b\d+\.?\d*\b/g, '<span class="number">$&</span>');
    },
    
    highlightHTML(code) {
        return code
            .replace(/&lt;(\/?[^&]+)&gt;/g, '<span class="keyword">&lt;$1&gt;</span>')
            .replace(/(["'])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/&lt;!--[\s\S]*?--&gt;/g, '<span class="comment">$&</span>');
    },
    
    highlightCSS(code) {
        return code
            .replace(/([.#]?[a-zA-Z-]+)\s*\{/g, '<span class="function">$1</span> {')
            .replace(/([a-zA-Z-]+)\s*:/g, '<span class="variable">$1</span>:')
            .replace(/(["'])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>')
            .replace(/\b\d+[a-zA-Z%]*\b/g, '<span class="number">$&</span>');
    },
    
    highlightJSON(code) {
        return code
            .replace(/("(?:[^"\\]|\\.)*")\s*:/g, '<span class="variable">$1</span>:')
            .replace(/: ("(?:[^"\\]|\\.)*")/g, ': <span class="string">$1</span>')
            .replace(/: (true|false|null)/g, ': <span class="keyword">$1</span>')
            .replace(/: (\d+\.?\d*)/g, ': <span class="number">$1</span>');
    },
    
    highlightSQL(code) {
        return code
            .replace(/\b(SELECT|FROM|WHERE|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP|TABLE|INDEX|PRIMARY|KEY|FOREIGN|REFERENCES|JOIN|INNER|LEFT|RIGHT|OUTER|ON|GROUP|BY|ORDER|HAVING|UNION|DISTINCT|COUNT|SUM|AVG|MIN|MAX|AS|AND|OR|NOT|IN|EXISTS|BETWEEN|LIKE|IS|NULL)\b/gi, '<span class="keyword">$1</span>')
            .replace(/(["'])((?:\\.|(?!\1)[^\\])*?)\1/g, '<span class="string">$1$2$1</span>')
            .replace(/--.*$/gm, '<span class="comment">$&</span>')
            .replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>');
    },
    
    copyCodeToClipboard(codeId) {
        const codeElement = document.getElementById(codeId);
        if (codeElement) {
            const codeContent = codeElement.dataset.codeContent || codeElement.textContent;
            navigator.clipboard.writeText(codeContent).then(() => {
                const button = codeElement.closest('.content-block-code').querySelector('.copy-btn');
                if (button) {
                    const originalContent = button.innerHTML;
                    button.innerHTML = '<i class="fas fa-check"></i><span>کپی شد!</span>';
                    button.classList.add('copied');
                    
                    setTimeout(() => {
                        button.innerHTML = originalContent;
                        button.classList.remove('copied');
                    }, 2000);
                }
            }).catch(err => {
                console.error('Failed to copy code:', err);
                // Fallback for older browsers
                const textArea = document.createElement('textarea');
                textArea.value = codeContent;
                document.body.appendChild(textArea);
                textArea.select();
                document.execCommand('copy');
                document.body.removeChild(textArea);
                
                // Show success message for fallback
                const button = codeElement.closest('.content-block-code').querySelector('.copy-btn');
                if (button) {
                    const originalContent = button.innerHTML;
                    button.innerHTML = '<i class="fas fa-check"></i><span>کپی شد!</span>';
                    button.classList.add('copied');
                    
                    setTimeout(() => {
                        button.innerHTML = originalContent;
                        button.classList.remove('copied');
                    }, 2000);
                }
            });
        }
    },
    
    formatDuration(seconds) {
        if (!seconds || seconds < 0) return '00:00';
        
        // Convert to integer to remove decimals
        const totalSeconds = Math.floor(seconds);
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const secs = totalSeconds % 60;
        
        if (hours > 0) {
            return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
        } else {
            return `${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
        }
    },
    
    getPriorityText(priority) {
        const priorityTexts = {
            'high': 'اولویت بالا',
            'normal': 'اولویت عادی',
            'low': 'اولویت پایین'
        };
        return priorityTexts[priority] || 'اولویت عادی';
    },
    
    initializeCustomAudioPlayer(audioId, audioUrl) {
        const audio = document.getElementById(audioId);
        const speakerButton = document.getElementById(`speaker-${audioId}`);
        const statusElement = document.getElementById(`status-${audioId}`);
        const progressBar = document.getElementById(`progress-${audioId}`);
        const progressFill = document.getElementById(`progress-fill-${audioId}`);
        const currentTimeElement = document.getElementById(`current-time-${audioId}`);
        const totalTimeElement = document.getElementById(`total-time-${audioId}`);
        
        // Check if all elements exist
        if (!audio || !speakerButton || !statusElement || !progressBar || !progressFill || !currentTimeElement || !totalTimeElement) {
            console.warn('Audio player elements not found for:', audioId);
            return;
        }
        
        let isPlaying = false;
        let isDragging = false;
        
        // Speaker button click handler
        speakerButton.addEventListener('click', () => {
            if (isPlaying) {
                this.pauseAudio(audioId);
            } else {
                this.playAudio(audioId);
            }
        });
        
        // Progress bar click handler
        progressBar.addEventListener('click', (e) => {
            if (audio.duration) {
                const rect = progressBar.getBoundingClientRect();
                const clickX = e.clientX - rect.left;
                const percentage = clickX / rect.width;
                const newTime = percentage * audio.duration;
                audio.currentTime = newTime;
            }
        });
        
        // Audio event listeners
        audio.addEventListener('loadedmetadata', () => {
            const duration = this.formatDuration(audio.duration);
            totalTimeElement.textContent = duration;
        });
        
        audio.addEventListener('timeupdate', () => {
            if (!isDragging) {
                const percentage = (audio.currentTime / audio.duration) * 100;
                progressFill.style.width = `${percentage}%`;
                currentTimeElement.textContent = this.formatDuration(audio.currentTime);
            }
        });
        
        audio.addEventListener('play', () => {
            isPlaying = true;
            speakerButton.classList.add('playing');
            statusElement.textContent = 'در حال پخش';
            speakerButton.querySelector('.speaker-icon').className = 'fas fa-volume-mute speaker-icon';
        });
        
        audio.addEventListener('pause', () => {
            isPlaying = false;
            speakerButton.classList.remove('playing');
            statusElement.textContent = 'متوقف شده';
            speakerButton.querySelector('.speaker-icon').className = 'fas fa-volume-up speaker-icon';
        });
        
        audio.addEventListener('ended', () => {
            isPlaying = false;
            speakerButton.classList.remove('playing');
            statusElement.textContent = 'پایان یافت';
            speakerButton.querySelector('.speaker-icon').className = 'fas fa-volume-up speaker-icon';
            progressFill.style.width = '0%';
            currentTimeElement.textContent = '00:00';
        });
        
        audio.addEventListener('error', () => {
            statusElement.textContent = 'خطا در بارگذاری';
            console.error('Audio loading error:', audio.error);
        });
        
        // Touch events for mobile
        progressBar.addEventListener('touchstart', (e) => {
            isDragging = true;
            e.preventDefault();
        });
        
        progressBar.addEventListener('touchmove', (e) => {
            if (isDragging && audio.duration) {
                const rect = progressBar.getBoundingClientRect();
                const touchX = e.touches[0].clientX - rect.left;
                const percentage = Math.max(0, Math.min(1, touchX / rect.width));
                const newTime = percentage * audio.duration;
                audio.currentTime = newTime;
                progressFill.style.width = `${percentage * 100}%`;
                currentTimeElement.textContent = this.formatDuration(newTime);
            }
            e.preventDefault();
        });
        
        progressBar.addEventListener('touchend', () => {
            isDragging = false;
        });
    },
    
    playAudio(audioId) {
        const audio = document.getElementById(audioId);
        if (audio) {
            audio.play().catch(error => {
                console.error('Error playing audio:', error);
                const statusElement = document.getElementById(`status-${audioId}`);
                statusElement.textContent = 'خطا در پخش';
            });
        }
    },
    
    pauseAudio(audioId) {
        const audio = document.getElementById(audioId);
        if (audio) {
            audio.pause();
        }
    },
    
    openImageModal(imageId) {
        const imageElement = document.getElementById(imageId);
        if (!imageElement) return;
        
        const imageUrl = imageElement.dataset.imageUrl;
        const imageCaption = imageElement.dataset.imageCaption;
        
        // Create modal if it doesn't exist
        let modal = document.getElementById('imageModal');
        if (!modal) {
            modal = document.createElement('div');
            modal.id = 'imageModal';
            modal.className = 'image-modal';
            modal.innerHTML = `
                <div class="image-modal-content">
                    <div class="image-modal-header">
                        <div class="image-modal-title" id="modalImageTitle">تصویر</div>
                        <div class="image-modal-actions">
                            <button type="button" class="image-modal-btn" id="modalDownloadBtn">
                                <i class="fas fa-download"></i>
                                دانلود
                            </button>
                        </div>
                    </div>
                    <img id="modalImage" src="" alt="">
                    <button type="button" class="image-modal-close" onclick="studySession.closeImageModal()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            `;
            document.body.appendChild(modal);
        }
        
        // Set image data
        document.getElementById('modalImage').src = imageUrl;
        document.getElementById('modalImageTitle').textContent = imageCaption || 'تصویر';
        
        // Set download button
        const downloadBtn = document.getElementById('modalDownloadBtn');
        downloadBtn.onclick = (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.downloadImage(imageUrl, imageCaption || 'تصویر');
        };
        
        // Show modal
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
        
        // Close modal on background click
        modal.onclick = (e) => {
            if (e.target === modal) {
                this.closeImageModal();
            }
        };
        
        // Close modal on Escape key
        document.addEventListener('keydown', this.handleModalKeydown);
    },
    
    closeImageModal() {
        const modal = document.getElementById('imageModal');
        if (modal) {
            modal.style.display = 'none';
            document.body.style.overflow = 'auto';
            document.removeEventListener('keydown', this.handleModalKeydown);
        }
    },
    
    handleModalKeydown(e) {
        if (e.key === 'Escape') {
            this.closeImageModal();
        }
    },
    
    downloadImage(imageUrl, filename) {
        try {
            // Prevent form submission
            event.preventDefault();
            event.stopPropagation();
            
            // Create a temporary link element
            const link = document.createElement('a');
            link.href = imageUrl;
            link.download = filename || 'تصویر';
            link.target = '_blank';
            
            // Add to DOM, click, and remove
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        } catch (error) {
            console.error('Error downloading image:', error);
            // Fallback: open image in new tab
            window.open(imageUrl, '_blank');
        }
    },
    
    showReminderError(message) {
        const container = document.getElementById('reminder-content');
        if (!container) return;
        
        container.innerHTML = `
            <div class="reminder-error" style="
                padding: 3rem;
                text-align: center;
                color: #dc2626;
                background: #fef2f2;
                border-radius: 16px;
                border: 1px solid #fecaca;
            ">
                <i class="fas fa-exclamation-triangle" style="font-size: 3rem; margin-bottom: 1rem;"></i>
                <h3 style="margin-bottom: 1rem;">خطا در بارگذاری محتوا</h3>
                <p>${message}</p>
            </div>
        `;
    },
    
    async startStudySession() {
        // Don't create session in database until user completes study
        // Just track locally for now
        this.sessionId = 0; // No database session yet
        
        // Start timer automatically when page loads (hidden, for tracking only)
        this.startTimer();
        
        // Load and display total study time
        this.loadTotalStudyTime();
    },
    
    async loadTotalStudyTime() {
        // Get total study time from data attribute (set by server)
        const timeDisplay = document.getElementById('study-time-display');
        if (!timeDisplay) return;
        
        const totalSeconds = parseInt(timeDisplay.dataset.totalSeconds || '0');
        this.updateTotalTimeDisplay(totalSeconds);
    },
    
    async updateTotalTimeDisplay(totalSeconds) {
        const timeDisplay = document.getElementById('study-time-display');
        if (!timeDisplay) return;
        
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;
        
        const formattedTime = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        timeDisplay.textContent = formattedTime;
        timeDisplay.dataset.totalSeconds = totalSeconds;
    },
    
    bindEvents() {
        const self = this;
        
        // Auto-save study session when page unloads (refresh, close, navigate away)
        window.addEventListener('beforeunload', function(e) {
            // Save study session silently
            if (self.isActive && self.getElapsedTime() > 0) {
                // Use sendBeacon for reliable delivery during page unload
                self.saveStudySessionOnUnload();
            }
        });
        
        // Save on page unload (fallback for browsers that support it)
        window.addEventListener('pagehide', function() {
            if (self.isActive && self.getElapsedTime() > 0) {
                self.saveStudySessionOnUnload();
            }
        });
        
        // Save periodically (every 30 seconds) to prevent data loss
        setInterval(function() {
            if (self.isActive && self.getElapsedTime() > 30) {
                // Only save if at least 30 seconds have passed
                self.saveStudySessionSilently();
            }
        }, 30000); // Every 30 seconds
    },
    
    saveStudySessionOnUnload() {
        // Use sendBeacon for reliable delivery during page unload
        try {
            // Prevent duplicate saves - if already saving or already saved this session, skip
            if (this.isSaving || (this.lastSavedStartTime && this.lastSavedStartTime === this.startTime)) {
                console.log('Skipping duplicate save on unload');
                return;
            }
            
            const scheduleItemId = document.getElementById('schedule-item-content')?.dataset?.itemId || 
                                  window.studyContentConfig?.scheduleItemId;
            
            if (!scheduleItemId || !this.startTime) {
                return;
            }
            
            // Mark as saving to prevent duplicate calls
            this.isSaving = true;
            this.lastSavedStartTime = this.startTime;
            
            const endTime = Date.now();
            const data = {
                ScheduleItemId: parseInt(scheduleItemId),
                StartedAt: new Date(this.startTime).toISOString(),
                EndedAt: new Date(endTime).toISOString()
            };
            
            const blob = new Blob([JSON.stringify(data)], { type: 'application/json' });
            navigator.sendBeacon('/Student/ScheduleItem/CreateAndCompleteStudySession', blob);
        } catch (error) {
            console.error('Error saving study session on unload:', error);
            // Reset flag on error so it can be retried
            this.isSaving = false;
        }
    },
    
    async saveStudySessionSilently() {
        // Save without showing any UI feedback
        try {
            // Prevent duplicate saves - if already saving or already saved this session, skip
            if (this.isSaving || (this.lastSavedStartTime && this.lastSavedStartTime === this.startTime)) {
                console.log('Skipping duplicate silent save');
                return;
            }
            await this.saveStudySession();
        } catch (error) {
            console.error('Error saving study session silently:', error);
            // Reset flag on error so it can be retried
            this.isSaving = false;
        }
    },
    
    startHiddenTimer() {
        // Auto-start hidden timer when page loads
        setTimeout(() => {
            this.startTimer();
        }, 500); // Start after 0.5 second
    },
    
    startTimer() {
        if (!this.isActive) {
            this.isActive = true;
            // Don't reset startTime if it already exists (for resume after cancel)
            if (!this.startTime) {
                this.startTime = Date.now();
                this.elapsedTime = 0;
                // Reset save flags when starting a new session
                this.isSaving = false;
                this.lastSavedStartTime = null;
            }
            
            // Timer runs silently in background (no UI updates needed)
            this.updateInterval = setInterval(() => {
                this.elapsedTime = Math.floor((Date.now() - this.startTime) / 1000);
            }, 1000);
        }
    },
    
    stopTimer() {
        if (this.isActive) {
            this.isActive = false;
            if (this.updateInterval) {
                clearInterval(this.updateInterval);
                this.updateInterval = null;
            }
        }
    },
    
    getElapsedTime() {
        if (this.isActive && this.startTime) {
            const elapsed = Math.floor((Date.now() - this.startTime) / 1000);
            return elapsed;
        }
        return this.elapsedTime;
    },
    
    getActualSessionDuration() {
        // If we have a fixed end time (modal is open), use it
        if (this.fixedEndTime) {
            const duration = Math.floor((this.fixedEndTime - this.startTime) / 1000);
            return duration;
        }
        
        // If we have an active session, calculate duration from StartedAt and current time
        // This matches the backend calculation logic
        if (this.sessionId && this.sessionId > 0) {
            // For active sessions, calculate from start time to now
            if (this.isActive && this.startTime) {
                const duration = Math.floor((Date.now() - this.startTime) / 1000);
                return duration;
            }
            // For completed sessions, we should get the actual duration from backend
            return this.elapsedTime;
        }
        return 0;
    },
    
    formatTime(seconds) {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const secs = Math.floor(seconds % 60);
        
        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    },
    
    
    async saveStudySession() {
        try {
            // Prevent duplicate saves - if already saving or already saved this session, skip
            if (this.isSaving) {
                console.log('Already saving study session, skipping duplicate call');
                return;
            }
            
            // Check if this session has already been saved
            if (this.lastSavedStartTime && this.lastSavedStartTime === this.startTime) {
                console.log('This session has already been saved, skipping duplicate save');
                return;
            }
            
            // Mark as saving to prevent duplicate calls
            this.isSaving = true;
            
            // Get schedule item ID from data attribute or config
            const scheduleItemId = document.getElementById('schedule-item-content')?.dataset?.itemId || 
                                  window.studyContentConfig?.scheduleItemId;
            
            console.log('Schedule Item ID sources:', {
                fromDataAttribute: document.getElementById('schedule-item-content')?.dataset?.itemId,
                fromConfig: window.studyContentConfig?.scheduleItemId,
                final: scheduleItemId
            });
            
            if (!scheduleItemId) {
                this.isSaving = false;
                throw new Error('Schedule item ID not found');
            }
            
            // Ensure we have valid start time
            if (!this.startTime) {
                console.warn('No start time found, using current time');
                this.startTime = Date.now() - (this.elapsedTime * 1000);
            }
            
            // Use current time as end time
            const endTime = Date.now();
            
            // Create and complete the session in one operation
            const createAndCompleteData = {
                ScheduleItemId: parseInt(scheduleItemId),
                StartedAt: new Date(this.startTime).toISOString(),
                EndedAt: new Date(endTime).toISOString()
            };
            
            console.log('Saving study session with data:', createAndCompleteData);
            console.log('Elapsed time:', this.elapsedTime, 'seconds');
            console.log('Start time:', new Date(this.startTime).toISOString());
            console.log('End time:', new Date(endTime).toISOString());
            
            // Get anti-forgery token
            const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            const token = tokenElement ? tokenElement.value : '';
            console.log('Anti-forgery token found:', !!token);
            
            // Build headers
            const headers = {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            };
            
            // Add CSRF token if available (ASP.NET Core uses X-CSRF-TOKEN header)
            if (token) {
                headers['X-CSRF-TOKEN'] = token;
            }
            
            const response = await fetch('/Student/ScheduleItem/CreateAndCompleteStudySession', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(createAndCompleteData)
            });
            
            console.log('Response status:', response.status);
            console.log('Response ok:', response.ok);
            
            if (!response.ok) {
                this.isSaving = false;
                let errorText = '';
                try {
                    errorText = await response.text();
                    console.error('Response error text:', errorText);
                    
                    // Try to parse as JSON
                    try {
                        const errorJson = JSON.parse(errorText);
                        console.error('Response error JSON:', errorJson);
                        throw new Error(errorJson.error || errorJson.message || `HTTP error! status: ${response.status}`);
                    } catch (parseError) {
                        // Not JSON, use text
                        throw new Error(errorText || `HTTP error! status: ${response.status}`);
                    }
                } catch (textError) {
                    throw new Error(`HTTP error! status: ${response.status}, message: ${textError.message}`);
                }
            }
            
            const result = await response.json();
            console.log('Response result:', result);
            
            if (!result.success) {
                this.isSaving = false;
                throw new Error(result.error || 'خطا در ثبت جلسه مطالعه');
            }
            
            // Mark this session as saved to prevent duplicate saves
            this.lastSavedStartTime = this.startTime;
            this.isSaving = false;
            
            // Update total study time display after successful save
            if (result.totalStudyTimeSeconds !== undefined) {
                this.updateTotalTimeDisplay(result.totalStudyTimeSeconds);
            } else {
                // Reload total time from server
                this.loadTotalStudyTime();
            }
            
            return result;
        } catch (error) {
            this.isSaving = false;
            console.error('Failed to save study session:', error);
            console.error('Error stack:', error.stack);
            throw error;
        }
    },
    
    navigateToCourseScheduleItems() {
        // Get the course ID from config or URL
        const courseId = window.studyContentConfig?.courseId || this.getCourseIdFromUrl();
        const scheduleItemId = document.getElementById('schedule-item-content')?.dataset?.itemId || 
                              window.studyContentConfig?.scheduleItemId;
        
        if (courseId && courseId > 0) {
            // Add itemId to URL to highlight it when returning
            const url = scheduleItemId 
                ? `/Student/Course/Study/${courseId}?itemId=${scheduleItemId}`
                : `/Student/Course/Study/${courseId}`;
            window.location.href = url;
        } else {
            // Fallback to history.back() if we can't determine the course ID
            console.warn('Course ID not found, using history.back()');
            history.back();
        }
    },
    
    getCourseIdFromUrl() {
        // Try to extract course ID from current URL or referrer
        const currentUrl = window.location.href;
        const referrer = document.referrer;
        
        // Look for course ID in referrer URL (coming from course study page)
        if (referrer) {
            const courseMatch = referrer.match(/\/Student\/Course\/Study\/(\d+)/);
            if (courseMatch) {
                return courseMatch[1];
            }
        }
        
        // Look for course ID in current URL if we're in a nested route
        const currentMatch = currentUrl.match(/\/Student\/Course\/Study\/(\d+)/);
        if (currentMatch) {
            return currentMatch[1];
        }
        
        return null;
    },
    
    showToast(message, type) {
        let toastClass;
        switch (type) {
            case 'error':
                toastClass = 'alert-danger';
                break;
            case 'info':
                toastClass = 'alert-info';
                break;
            case 'warning':
                toastClass = 'alert-warning';
                break;
            default:
                toastClass = 'alert-success';
                break;
        }
        
        const toastHtml = `
            <div class="alert ${toastClass} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999;" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        document.body.insertAdjacentHTML('beforeend', toastHtml);
        
        // Auto remove after 3 seconds
        setTimeout(() => {
            const alert = document.querySelector('.alert');
            if (alert) {
                alert.remove();
            }
        }, 3000);
    }
};

$(document).ready(function() {
    // Initialize study session
    studySession.init();
});