/**
 * Content Sidebar Manager
 * Manages the sidebar navigation for content blocks in Step 4
 * Provides navigation between blocks and visual feedback
 */

// Prevent duplicate class declaration
if (typeof window !== 'undefined' && !window.ContentSidebarManager) {
    window.ContentSidebarManager = class ContentSidebarManager {
    constructor() {
        this.sidebar = document.getElementById('blocksNavigation');
        this.sidebarCount = document.getElementById('sidebarBlockCount');
        this.mainContentArea = document.querySelector('.main-content-area');
        this.blocks = [];
        this.activeBlockId = null;
        this.scrollObserver = null;
        this.checkInterval = null;
        
        this.init();
    }

    init() {
        if (!this.sidebar || !this.sidebarCount) {
            console.warn('ContentSidebarManager: Required elements not found');
            return;
        }

        this.setupScrollObserver();
        this.setupEventListeners();
        
        // Wait a bit before initial update to ensure content is loaded
        setTimeout(() => {
            this.updateSidebar();
            // Debug after initial update
            setTimeout(() => {
                this.debugSidebarState();
            }, 1000);
        }, 500);
        
        // Set up periodic check for blocks (only for first 30 seconds)
        this.checkInterval = setInterval(() => {
            this.checkAndRefresh();
        }, 2000);
        
        // Stop checking after 30 seconds
        setTimeout(() => {
            if (this.checkInterval) {
                clearInterval(this.checkInterval);
                this.checkInterval = null;
            }
        }, 30000);
        
        // Make debug method available globally for testing
        window.debugSidebar = () => this.debugSidebarState();
        window.forceRefreshSidebar = () => this.forceRefresh();
    }

    setupScrollObserver() {
        // Create intersection observer to track which block is currently visible
        this.scrollObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const blockId = entry.target.dataset.blockId;
                    if (blockId) {
                        this.setActiveBlock(blockId);
                    }
                }
            });
        }, {
            root: this.mainContentArea,
            rootMargin: '-20% 0px -20% 0px',
            threshold: 0.1
        });
    }

    setupEventListeners() {
        // Listen for block changes from content builders
        document.addEventListener('blockAdded', (e) => {
            this.addBlockToSidebar(e.detail);
        });

        document.addEventListener('blockDeleted', (e) => {
            this.removeBlockFromSidebar(e.detail.blockId);
        });

        document.addEventListener('blockMoved', (e) => {
            this.updateSidebar();
        });

        document.addEventListener('blockContentChanged', (e) => {
            this.updateBlockInSidebar(e.detail.blockId);
        });
        
        // Listen for content loaded events
        document.addEventListener('contentLoaded', (e) => {
            setTimeout(() => {
                this.updateSidebar();
            }, 200);
        });

        // Listen for sidebar navigation clicks
        this.sidebar.addEventListener('click', (e) => {
            const navItem = e.target.closest('.block-nav-item');
            if (navItem) {
                const blockId = navItem.dataset.blockId;
                if (blockId) {
                    this.scrollToBlock(blockId);
                }
            }

            // Handle sidebar action buttons
            const actionBtn = e.target.closest('.block-nav-action');
            if (actionBtn) {
                const action = actionBtn.dataset.action;
                const blockId = actionBtn.closest('.block-nav-item').dataset.blockId;
                
                switch (action) {
                    case 'delete':
                        this.deleteBlock(blockId);
                        break;
                    case 'edit':
                        this.editBlock(blockId);
                        break;
                }
            }
        });
    }

    addBlockToSidebar(blockDetail) {
        const blockId = blockDetail.blockId;
        const blockType = blockDetail.blockType;
        
        // Check if block is already in sidebar to prevent duplicates
        const existingItem = this.sidebar.querySelector(`[data-block-id="${blockId}"]`);
        if (existingItem) {
            return;
        }
        
        // Find the actual block element to get more details
        const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
        if (!blockElement) return;

        const blockData = this.extractBlockData(blockElement, blockType);
        
        // Add to blocks array
        this.blocks.push({
            id: blockId,
            type: blockType,
            ...blockData
        });

        // Create sidebar item
        const navItem = this.createSidebarItem(blockId, blockType, blockData);
        
        // Find where to insert the block to maintain order
        if (blockElement) {
            // Find the position of this block relative to other blocks
            const allBlocks = Array.from(document.querySelectorAll('.content-block, .question-block-template'));
            const currentIndex = allBlocks.indexOf(blockElement);
            
            // Find the next block in sidebar to insert before
            let insertBefore = null;
            for (let i = currentIndex + 1; i < allBlocks.length; i++) {
                const nextBlockId = allBlocks[i].dataset.blockId;
                const nextSidebarItem = this.sidebar.querySelector(`[data-block-id="${nextBlockId}"]`);
                if (nextSidebarItem) {
                    insertBefore = nextSidebarItem;
                    break;
                }
            }
            
            if (insertBefore) {
                this.sidebar.insertBefore(navItem, insertBefore);
            } else {
                this.sidebar.appendChild(navItem);
            }
        } else {
            // If we can't find the block element, just append
            this.sidebar.appendChild(navItem);
        }

        // Start observing the block for scroll tracking
        this.scrollObserver.observe(blockElement);

        this.updateSidebarCount();
    }

    removeBlockFromSidebar(blockId) {
        // Remove from blocks array
        this.blocks = this.blocks.filter(block => block.id !== blockId);

        // Remove sidebar item
        const navItem = this.sidebar.querySelector(`[data-block-id="${blockId}"]`);
        if (navItem) {
            navItem.remove();
        }

        // Stop observing the block
        const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
        if (blockElement) {
            this.scrollObserver.unobserve(blockElement);
        }

        this.updateSidebarCount();
    }

    updateBlockInSidebar(blockId) {
        const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
        if (!blockElement) return;

        const blockType = blockElement.dataset.type;
        const blockData = this.extractBlockData(blockElement, blockType);

        // Update blocks array
        const blockIndex = this.blocks.findIndex(block => block.id === blockId);
        if (blockIndex !== -1) {
            this.blocks[blockIndex] = {
                ...this.blocks[blockIndex],
                ...blockData
            };
        }

        // Update sidebar item
        const navItem = this.sidebar.querySelector(`[data-block-id="${blockId}"]`);
        if (navItem) {
            this.updateSidebarItem(navItem, blockType, blockData);
        }
    }

    createSidebarItem(blockId, blockType, blockData) {
        const navItem = document.createElement('div');
        navItem.className = 'block-nav-item';
        navItem.dataset.blockId = blockId;

        const iconClass = this.getBlockIcon(blockType);
        const typeName = this.getBlockTypeName(blockType);
        const preview = this.getBlockPreview(blockData);

        navItem.innerHTML = `
            <div class="block-nav-icon">
                <i class="${iconClass}"></i>
            </div>
            <div class="block-nav-content">
                <div class="block-nav-title">${typeName}</div>
                <div class="block-nav-type">${blockType}</div>
                <div class="block-nav-preview">${preview}</div>
            </div>
            <div class="block-nav-actions">
                <div class="block-nav-action" data-action="edit" title="ویرایش">
                    <i class="fas fa-edit"></i>
                </div>
                <div class="block-nav-action" data-action="delete" title="حذف">
                    <i class="fas fa-trash"></i>
                </div>
            </div>
        `;

        return navItem;
    }

    updateSidebarItem(navItem, blockType, blockData) {
        const iconClass = this.getBlockIcon(blockType);
        const typeName = this.getBlockTypeName(blockType);
        const preview = this.getBlockPreview(blockData);

        const iconElement = navItem.querySelector('.block-nav-icon i');
        const titleElement = navItem.querySelector('.block-nav-title');
        const previewElement = navItem.querySelector('.block-nav-preview');

        if (iconElement) iconElement.className = iconClass;
        if (titleElement) titleElement.textContent = typeName;
        if (previewElement) previewElement.textContent = preview;
    }

    extractBlockData(blockElement, blockType) {
        const blockData = {
            title: this.getBlockTypeName(blockType),
            preview: this.getBlockPreviewFromElement(blockElement, blockType)
        };

        return blockData;
    }

    getBlockPreviewFromElement(blockElement, blockType) {
        switch (blockType) {
            case 'text':
            case 'questionText':
                const textEditor = blockElement.querySelector('.ckeditor-editor, .rich-text-editor');
                if (textEditor) {
                    const text = textEditor.textContent || textEditor.innerText || '';
                    return text.substring(0, 50) + (text.length > 50 ? '...' : '');
                }
                return 'متن خالی';
            
            case 'image':
            case 'questionImage':
                const imageElement = blockElement.querySelector('img');
                if (imageElement && imageElement.src) {
                    return 'تصویر آپلود شده';
                }
                return 'تصویر خالی';
            
            case 'video':
            case 'questionVideo':
                const videoElement = blockElement.querySelector('video');
                if (videoElement) {
                    return 'ویدیو آپلود شده';
                }
                return 'ویدیو خالی';
            
            case 'audio':
            case 'questionAudio':
                const audioElement = blockElement.querySelector('audio');
                if (audioElement) {
                    return 'فایل صوتی آپلود شده';
                }
                return 'فایل صوتی خالی';
            
            case 'code':
                const codeElement = blockElement.querySelector('pre code');
                if (codeElement) {
                    const code = codeElement.textContent || '';
                    return code.substring(0, 30) + (code.length > 30 ? '...' : '');
                }
                return 'کد خالی';
            
            default:
                return 'محتوای نامشخص';
        }
    }

    getBlockIcon(blockType) {
        const iconMap = {
            'text': 'fas fa-font',
            'questionText': 'fas fa-font',
            'image': 'fas fa-image',
            'questionImage': 'fas fa-image',
            'video': 'fas fa-video',
            'questionVideo': 'fas fa-video',
            'audio': 'fas fa-microphone',
            'questionAudio': 'fas fa-microphone',
            'code': 'fas fa-code',
            'questionCode': 'fas fa-code'
        };
        return iconMap[blockType] || 'fas fa-square';
    }

    getBlockTypeName(blockType) {
        const nameMap = {
            'text': 'متن',
            'questionText': 'سوال متنی',
            'image': 'تصویر',
            'questionImage': 'سوال تصویری',
            'video': 'ویدیو',
            'questionVideo': 'سوال ویدیویی',
            'audio': 'صوت',
            'questionAudio': 'سوال صوتی',
            'code': 'کد',
            'questionCode': 'سوال کدی'
        };
        return nameMap[blockType] || 'نامشخص';
    }

    getBlockPreview(blockData) {
        return blockData.preview || 'محتوای خالی';
    }

    setActiveBlock(blockId) {
        if (this.activeBlockId === blockId) return;

        // Remove active class from previous item
        const prevActive = this.sidebar.querySelector('.block-nav-item.active');
        if (prevActive) {
            prevActive.classList.remove('active');
        }

        // Add active class to new item
        const newActive = this.sidebar.querySelector(`[data-block-id="${blockId}"]`);
        if (newActive) {
            newActive.classList.add('active');
            this.activeBlockId = blockId;
        }
    }

    scrollToBlock(blockId) {
        const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
        if (!blockElement) return;

        // Get the main content area (scrollable container)
        const mainContentArea = this.mainContentArea || document.querySelector('.main-content-area');
        if (!mainContentArea) {
            // Fallback to window scroll if main content area not found
            blockElement.scrollIntoView({
                behavior: 'smooth',
                block: 'center',
                inline: 'nearest'
            });
            return;
        }

        // Calculate the position of the block relative to the main content area
        const blockRect = blockElement.getBoundingClientRect();
        const containerRect = mainContentArea.getBoundingClientRect();
        
        // Calculate the scroll position to center the block
        const scrollTop = mainContentArea.scrollTop + (blockRect.top - containerRect.top) - (containerRect.height / 2) + (blockRect.height / 2);
        
        // Smooth scroll within the main content area
        mainContentArea.scrollTo({
            top: scrollTop,
            behavior: 'smooth'
        });

        // Highlight the block briefly
        blockElement.classList.add('highlight');
        setTimeout(() => {
            blockElement.classList.remove('highlight');
        }, 2000);
    }

    deleteBlock(blockId) {
        // Find the block element and trigger delete
        const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
        if (blockElement) {
            const deleteBtn = blockElement.querySelector('[data-action="delete"]');
            if (deleteBtn) {
                deleteBtn.click();
            }
        }
    }

    editBlock(blockId) {
        // Scroll to block and focus on it
        this.scrollToBlock(blockId);
        
        // Try to focus on the first input/textarea in the block
        setTimeout(() => {
            const blockElement = document.querySelector(`[data-block-id="${blockId}"]`);
            if (blockElement) {
                const firstInput = blockElement.querySelector('input, textarea, .ckeditor-editor, .rich-text-editor');
                if (firstInput) {
                    firstInput.focus();
                }
            }
        }, 500);
    }

    updateSidebarCount() {
        if (this.sidebarCount) {
            this.sidebarCount.textContent = this.blocks.length;
        }
    }

    updateSidebar() {
        // Clear existing sidebar items and blocks array
        this.sidebar.innerHTML = '';
        this.blocks = [];

        // Rebuild sidebar from current blocks
        const blockElements = document.querySelectorAll('.content-block, .question-block-template');
        
        if (blockElements.length === 0) {
            this.showEmptyState();
            this.updateSidebarCount();
            return;
        }

        // Sort blocks by their order in the DOM
        const sortedBlocks = Array.from(blockElements).sort((a, b) => {
            const aRect = a.getBoundingClientRect();
            const bRect = b.getBoundingClientRect();
            return aRect.top - bRect.top;
        });

        let addedCount = 0;
        sortedBlocks.forEach((blockElement, index) => {
            const blockId = blockElement.dataset.blockId;
            const blockType = blockElement.dataset.type;
            
            if (blockId && blockType) {
                const blockData = this.extractBlockData(blockElement, blockType);
                const navItem = this.createSidebarItem(blockId, blockType, blockData);
                // Append in order to maintain the same order as main content
                this.sidebar.appendChild(navItem);
                
                // Add to blocks array
                this.blocks.push({
                    id: blockId,
                    type: blockType,
                    ...blockData
                });
                
                addedCount++;
                
                // Start observing the block
                this.scrollObserver.observe(blockElement);
            }
        });

        this.updateSidebarCount();
    }

    showEmptyState() {
        this.sidebar.innerHTML = `
            <div class="empty-sidebar-state">
                <i class="fas fa-list"></i>
                <h4>هیچ بلاکی وجود ندارد</h4>
                <p>برای شروع، بلاک جدیدی اضافه کنید</p>
            </div>
        `;
    }

    // Public method to refresh sidebar (called from content builders)
    refresh() {
        // Add a small delay to ensure DOM is ready
        setTimeout(() => {
            this.updateSidebar();
        }, 100);
    }
    
    // Force refresh with longer delay (for loading existing content)
    forceRefresh() {
        setTimeout(() => {
            this.updateSidebar();
        }, 1500);
    }
    
    // Check if blocks exist and refresh if needed
    checkAndRefresh() {
        const blockElements = document.querySelectorAll('.content-block, .question-block-template');
        const sidebarItems = this.sidebar.querySelectorAll('.block-nav-item');
        
        if (blockElements.length > 0 && sidebarItems.length === 0) {
            this.updateSidebar();
        } else if (blockElements.length > 0 && sidebarItems.length !== blockElements.length) {
            this.updateSidebar();
        }
    }
    
    // Debug method to check sidebar state
    debugSidebarState() {
        const blockElements = document.querySelectorAll('.content-block, .question-block-template');
        const sidebarItems = this.sidebar.querySelectorAll('.block-nav-item');
     
        
        if (blockElements.length > 0) {
        }
        
        if (sidebarItems.length > 0) {
        }
        
    }

    // Cleanup method
    destroy() {
        if (this.scrollObserver) {
            this.scrollObserver.disconnect();
        }
        if (this.checkInterval) {
            clearInterval(this.checkInterval);
        }
    }
    };
}

// Initialize sidebar manager when DOM is loaded
function initializeContentSidebar() {
    try {
        if (window.contentSidebarManager) {
            return;
        }

        const sidebar = document.getElementById('blocksNavigation');
        if (!sidebar) {
            console.warn('ContentSidebarManager: Sidebar element not found');
            return;
        }

        window.contentSidebarManager = new ContentSidebarManager();
        
    } catch (error) {
        console.error('Error initializing ContentSidebarManager:', error);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(initializeContentSidebar, 100);
});

// Also try to initialize immediately if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeContentSidebar, 100);
    });
} else {
    setTimeout(initializeContentSidebar, 100);
}
