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
    
    init() {
        this.sessionId = window.studyContentConfig?.activeSessionId || 0;
        this.bindEvents();
        this.startHiddenTimer();
        this.startStudySession();
        
        // Initialize reminder content if needed
        if (window.studyContentConfig?.contentType === 'Reminder') {
            this.initializeReminderContent();
        }
        
        console.log('Study session initialized with sessionId:', this.sessionId);
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
        
        console.log('Reminder content rendered successfully');
    },
    
    createReminderBody(content) {
        const body = document.createElement('div');
        body.className = 'reminder-body';
        
        // Add main message if exists
        const mainMessage = content.message || content.Message;
        if (mainMessage && mainMessage.trim()) {
            const messageDiv = document.createElement('div');
            messageDiv.className = 'reminder-message';
            messageDiv.innerHTML = this.formatTextContent(mainMessage);
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
        blockElement.className = 'content-block';
        
        switch ((block.type || block.Type)?.toLowerCase()) {
            case 'text':
            case 0: // Text
                const textContent = block.data?.content || 
                                  block.data?.textContent || 
                                  block.data?.Content || 
                                  block.data?.TextContent || 
                                  '';
                blockElement.innerHTML = `
                    <div class="content-block-text">
                        ${this.formatTextContent(textContent)}
                    </div>
                `;
                break;
                
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
        console.log('Study session tracking started locally (no database record yet)');
        this.sessionId = 0; // No database session yet
    },
    
    bindEvents() {
        const self = this;
        
        // Handle browser back button
        window.addEventListener('popstate', (e) => {
            if (self.isActive && self.getElapsedTime() > 5) {
                self.showExitConfirmation();
                history.pushState(null, null, window.location.href);
            }
        });
        
        // Add history state to prevent back button
        history.pushState(null, null, window.location.href);
        
        // Wait for DOM to be fully loaded
        setTimeout(() => {
            // Handle specific navigation clicks only
            const handleNavigationClick = function(e) {
                const target = e.target;
                
                console.log('Navigation click detected on:', target.tagName, target.className, target.id, 'timer active:', self.isActive, 'time:', self.getElapsedTime());
                
                // Check if timer is active and has enough time
                if (self.isActive && self.getElapsedTime() > 5) {
                    
                    // Skip if clicking inside the modal
                    if (target.closest('#exitConfirmationModal')) {
                        console.log('Click inside modal, ignoring');
                        return;
                    }
                    
                    // Skip if clicking on image action buttons
                    if (target.closest('.image-action-btn') || target.closest('.image-overlay')) {
                        console.log('Image action button clicked, ignoring');
                        return;
                    }
                    
                    // Skip if clicking on audio/video controls
                    if (target.closest('.speaker-button') || target.closest('.play-button') || 
                        target.closest('.progress-bar') || target.closest('.audio-controls') ||
                        target.closest('.video-controls')) {
                        console.log('Media control clicked, ignoring');
                        return;
                    }
                    
                    // Check if it's a link that navigates away
                    if (target.tagName === 'A') {
                        const href = target.getAttribute('href');
                        if (href && !href.startsWith('#') && !href.startsWith('javascript:')) {
                            console.log('External link clicked:', href);
                            e.preventDefault();
                            e.stopPropagation();
                            self.showExitConfirmation();
                            return false;
                        }
                    }
                    
                    // Check for navbar/menu clicks - ONLY specific navigation elements
                    if (target.closest('.navbar-brand') || target.closest('.nav-link') || 
                        target.closest('.navbar-toggler') || target.closest('.dropdown-item')) {
                        console.log('Navigation element clicked:', target);
                        e.preventDefault();
                        e.stopPropagation();
                        self.showExitConfirmation();
                        return false;
                    }
                    
                    // Check for breadcrumb clicks
                    if (target.closest('.breadcrumb') || target.closest('.breadcrumb-item')) {
                        console.log('Breadcrumb clicked:', target);
                        e.preventDefault();
                        e.stopPropagation();
                        self.showExitConfirmation();
                        return false;
                    }
                    
                    // Check for bottom navigation clicks - handle nested elements
                    if (target.closest('.bottom-nav') || target.closest('.nav-item') || target.closest('.fixed-footer')) {
                        console.log('Bottom navigation clicked:', target);
                        e.preventDefault();
                        e.stopPropagation();
                        self.showExitConfirmation();
                        return false;
                    }
                    
                    // Check for any clickable element that might navigate
                    if (target.closest('a')) {
                        const parentLink = target.closest('a');
                        const parentHref = parentLink.getAttribute('href');
                        if (parentHref && !parentHref.startsWith('#') && !parentHref.startsWith('javascript:')) {
                            console.log('Parent link clicked:', parentHref);
                            e.preventDefault();
                            e.stopPropagation();
                            self.showExitConfirmation();
                            return false;
                        }
                    }
                }
            };
            
            // Bind event to document but only handle navigation elements
            document.addEventListener('click', handleNavigationClick, true);
            
            console.log('Navigation events bound successfully to document');
        }, 1000); // Wait 1 second for DOM to be ready
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
            }
            
            console.log('Timer started at:', new Date(this.startTime).toLocaleTimeString());
            
            // Timer runs in background, no UI updates needed
            this.updateInterval = setInterval(() => {
                // Timer runs silently in background
                this.elapsedTime = Math.floor((Date.now() - this.startTime) / 1000);
            }, 1000);
            
            console.log('Timer started');
        }
    },
    
    stopTimer() {
        if (this.isActive) {
            this.isActive = false;
            if (this.updateInterval) {
                clearInterval(this.updateInterval);
                this.updateInterval = null;
            }
            console.log('Timer stopped');
        }
    },
    
    getElapsedTime() {
        if (this.isActive && this.startTime) {
            const elapsed = Math.floor((Date.now() - this.startTime) / 1000);
            console.log('Elapsed time calculated:', elapsed, 'seconds');
            return elapsed;
        }
        console.log('Timer not active, returning elapsedTime:', this.elapsedTime);
        return this.elapsedTime;
    },
    
    getActualSessionDuration() {
        // If we have a fixed end time (modal is open), use it
        if (this.fixedEndTime) {
            const duration = Math.floor((this.fixedEndTime - this.startTime) / 1000);
            console.log('Using fixed end time, duration:', duration, 'seconds');
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
    
    showExitConfirmation() {
        // Stop the timer immediately when modal opens
        this.stopTimer();
        
        // Fix the end time to prevent further changes
        this.fixedEndTime = Date.now();
        
        const currentTime = this.getActualSessionDuration();
        console.log('Showing exit confirmation, fixed session duration:', currentTime);
        
        // Remove any existing modal first
        const existingModal = document.getElementById('exitConfirmationModal');
        if (existingModal) {
            existingModal.remove();
        }
        
        // Create simple modal HTML
        const modalHtml = `
            <div id="exitConfirmationModal" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 9999; display: flex; align-items: center; justify-content: center; padding: 1rem;">
                <div style="background: white; padding: 2rem; border-radius: 16px; max-width: 450px; width: 100%; text-align: center; box-shadow: 0 20px 40px rgba(0,0,0,0.15);">
                    <div style="margin-bottom: 1rem;">
                        <i class="fas fa-clock" style="font-size: 3rem; color: #667eea;"></i>
                    </div>
                    <h5 style="margin-bottom: 1rem; color: #1e293b; font-weight: 600;">زمان مطالعه شما</h5>
                    <div style="margin: 1.5rem 0;">
                        <div style="width: 120px; height: 120px; border-radius: 50%; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); display: flex; align-items: center; justify-content: center; margin: 0 auto; box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);">
                            <span style="color: white; font-family: 'Courier New', monospace; font-size: 1.2rem; font-weight: bold;">${this.formatTime(currentTime)}</span>
                        </div>
                    </div>
                    <p style="margin-bottom: 1.5rem; color: #64748b; font-size: 1rem;">آیا می‌خواهید این زمان مطالعه ثبت شود؟</p>
                    <!-- Action buttons row -->
                    <div style="display: flex; gap: 0.75rem; justify-content: center; align-items: center; margin-bottom: 1rem;">
                        <button id="exit-without-saving" style="
                            padding: 0.75rem 1.5rem; 
                            border: 1px solid #fecaca; 
                            background: #ffffff; 
                            color: #dc2626; 
                            border-radius: 12px; 
                            cursor: pointer; 
                            font-weight: 500; 
                            font-size: 0.9rem;
                            transition: all 0.2s ease;
                            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
                            flex: 1;
                            min-width: 120px;
                        " onmouseover="this.style.background='#fef2f2'; this.style.borderColor='#fca5a5'; this.style.transform='translateY(-1px)'; this.style.boxShadow='0 2px 6px rgba(0,0,0,0.15)'" 
                           onmouseout="this.style.background='#ffffff'; this.style.borderColor='#fecaca'; this.style.transform='translateY(0)'; this.style.boxShadow='0 1px 3px rgba(0,0,0,0.1)'">
                            <i class="fas fa-sign-out-alt" style="margin-left: 0.5rem;"></i> خروج بدون ثبت
                        </button>
                        <button id="save-and-exit" style="
                            padding: 0.75rem 1.5rem; 
                            border: none; 
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                            color: white; 
                            border-radius: 12px; 
                            cursor: pointer; 
                            font-weight: 600; 
                            font-size: 0.9rem;
                            transition: all 0.2s ease;
                            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
                            flex: 1;
                            min-width: 120px;
                        " onmouseover="this.style.transform='translateY(-2px)'; this.style.boxShadow='0 6px 20px rgba(102, 126, 234, 0.5)'" 
                           onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 4px 15px rgba(102, 126, 234, 0.4)'">
                            <i class="fas fa-save" style="margin-left: 0.5rem;"></i> ثبت و خروج
                        </button>
                    </div>
                    
                    <!-- Cancel button - full width -->
                    <div style="width: 100%;">
                        <button id="cancel-exit" style="
                            width: 100%;
                            padding: 0.75rem 1.5rem; 
                            border: 1px solid #e2e8f0; 
                            background: #ffffff; 
                            color: #64748b; 
                            border-radius: 12px; 
                            cursor: pointer; 
                            font-weight: 500; 
                            font-size: 0.9rem;
                            transition: all 0.2s ease;
                            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
                        " onmouseover="this.style.background='#f8fafc'; this.style.borderColor='#cbd5e1'; this.style.transform='translateY(-1px)'; this.style.boxShadow='0 2px 6px rgba(0,0,0,0.15)'" 
                           onmouseout="this.style.background='#ffffff'; this.style.borderColor='#e2e8f0'; this.style.transform='translateY(0)'; this.style.boxShadow='0 1px 3px rgba(0,0,0,0.1)'">
                            <i class="fas fa-times" style="margin-left: 0.5rem;"></i> انصراف و ادامه مطالعه
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        // Add modal to body
        document.body.insertAdjacentHTML('beforeend', modalHtml);
        
        // Bind events
        document.getElementById('cancel-exit').onclick = () => {
            console.log('Cancel clicked');
            this.hideModal();
            this.exitWithoutSaving();
        };
        
        document.getElementById('exit-without-saving').onclick = () => {
            console.log('Exit without saving clicked');
            this.hideModal();
            this.exitWithoutSaving();
        };
        
        document.getElementById('save-and-exit').onclick = () => {
            console.log('Save and exit clicked');
            this.hideModal();
            this.saveAndExit();
        };
        
        console.log('Modal created and shown');
        
        // Prevent body scroll
        document.body.style.overflow = 'hidden';
    },
    
    showModal() {
        const modal = document.getElementById('exitConfirmationModal');
        if (modal) {
            // Remove any existing backdrop first
            const existingBackdrop = document.getElementById('modalBackdrop');
            if (existingBackdrop) {
                existingBackdrop.remove();
            }
            
            // Add modal backdrop
            const backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop fade show';
            backdrop.id = 'modalBackdrop';
            document.body.appendChild(backdrop);
            
            // Show modal with proper classes
            modal.classList.add('show');
            modal.classList.remove('fade');
            modal.style.display = 'block';
            modal.setAttribute('aria-hidden', 'false');
            
            // Prevent body scroll and add modal-open class
            document.body.style.overflow = 'hidden';
            document.body.classList.add('modal-open');
            
            console.log('Modal shown successfully');
        } else {
            console.error('Modal element not found!');
        }
    },
    
    hideModal() {
        console.log('Hiding modal');
        const modal = document.getElementById('exitConfirmationModal');
        if (modal) {
            modal.remove();
            console.log('Modal removed');
        }
        
        // Restore body scroll
        document.body.style.overflow = '';
    },
    
    saveAndExit() {
        if (this.isActive) {
            this.isActive = false;
            clearInterval(this.updateInterval);
        }
        
        // Save study session to database
        this.saveStudySession().then(() => {
            // Show success message
            this.showToast('زمان مطالعه با موفقیت ثبت شد', 'success');
            
            // Navigate to course schedule items list after a short delay
            setTimeout(() => {
                this.navigateToCourseScheduleItems();
            }, 1000);
        }).catch((error) => {
            console.error('Error saving study session:', error);
            this.showToast('خطا در ثبت زمان مطالعه', 'error');
            
            // Still navigate back even if save failed
            setTimeout(() => {
                this.navigateToCourseScheduleItems();
            }, 2000);
        });
    },
    
    async saveStudySession() {
        console.log('=== SAVING STUDY SESSION ===');
        console.log('Timer active:', this.isActive);
        console.log('Start time:', this.startTime ? new Date(this.startTime).toLocaleTimeString() : 'null');
        console.log('Fixed end time:', this.fixedEndTime ? new Date(this.fixedEndTime).toLocaleTimeString() : 'null');
        console.log('Current time:', new Date().toLocaleTimeString());
        console.log('============================');
        
        try {
            // Create and complete the session in one operation
            const createAndCompleteData = {
                ScheduleItemId: window.studyContentConfig.educationalContentId,
                StartedAt: new Date(this.startTime).toISOString(),
                EndedAt: new Date(this.fixedEndTime || Date.now()).toISOString()
            };
            
            console.log('Creating and completing session with data:', createAndCompleteData);
            
            const response = await fetch('/Student/EducationalContent/CreateAndCompleteStudySession', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify(createAndCompleteData)
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const result = await response.json();
            console.log('Study session created and completed:', result);
            
            if (!result.success) {
                throw new Error(result.error || 'خطا در ثبت جلسه مطالعه');
            }
            
            return result;
        } catch (error) {
            console.error('Failed to save study session:', error);
            throw error;
        }
    },
    
    async exitWithoutSaving() {
        // Clear the fixed end time since user cancelled
        this.fixedEndTime = null;
        
        // Make sure timer is active and restart it
        this.isActive = true;
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
        }
        this.updateInterval = setInterval(() => {
            this.elapsedTime = Math.floor((Date.now() - this.startTime) / 1000);
        }, 1000);
        
        console.log('User cancelled exit, continuing study on same page');
        console.log('Timer is now active:', this.isActive);
        console.log('Elapsed time:', this.elapsedTime);
    },
    
    navigateToCourseScheduleItems() {
        // Get the course ID from config or URL
        const courseId = window.studyContentConfig?.courseId || this.getCourseIdFromUrl();
        if (courseId && courseId > 0) {
            window.location.href = `/Student/Course/Study/${courseId}`;
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
        const toastClass = type === 'error' ? 'alert-danger' : 'alert-success';
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
    console.log('Document ready, initializing study session');
    // Initialize study session
    studySession.init();
});