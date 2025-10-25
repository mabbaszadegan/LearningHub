/**
 * Unified Content Loader
 * Handles loading existing content for all content types (written, reminder, regular)
 * Provides a single interface for content loading across different managers
 */

class UnifiedContentLoader {
    constructor() {
        this.managers = new Map();
        this.isInitialized = false;
        this.loadAttempts = 0;
        this.maxLoadAttempts = 5;
        
        this.init();
    }

    init() {
        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                this.setupContentManagers();
            });
        } else {
            this.setupContentManagers();
        }
    }

    setupContentManagers() {
        console.log('UnifiedContentLoader: Setting up content managers...');
        
        // Check for written content
        const hasWrittenContent = this.checkForWrittenContent();
        console.log('UnifiedContentLoader: Has written content:', hasWrittenContent);
        if (hasWrittenContent) {
            this.setupWrittenContentManager();
        }
        
        // Check for reminder content
        const hasReminderContent = this.checkForReminderContent();
        console.log('UnifiedContentLoader: Has reminder content:', hasReminderContent);
        if (hasReminderContent) {
            this.setupReminderContentManager();
        }
        
        // Check for regular content
        const hasRegularContent = this.checkForRegularContent();
        console.log('UnifiedContentLoader: Has regular content:', hasRegularContent);
        if (hasRegularContent) {
            this.setupRegularContentManager();
        }
        
        this.isInitialized = true;
        
        // Start loading content after a short delay
        setTimeout(() => {
            this.loadAllContent();
        }, 300);
    }

    checkForWrittenContent() {
        const elements = [
            'contentBlocksList',
            'emptyBlocksState',
            'writtenPreview',
            'writtenContentJson',
            'questionBlockTemplates'
        ];
        
        return elements.every(id => document.getElementById(id) !== null);
    }

    checkForReminderContent() {
        const elements = [
            'contentBlocksList',
            'emptyBlocksState',
            'reminderPreview',
            'reminderContentJson',
            'contentBlockTemplates'
        ];
        
        return elements.every(id => document.getElementById(id) !== null);
    }

    checkForRegularContent() {
        const elements = [
            'contentBoxesContainer',
            'emptyState',
            'contentJson'
        ];
        
        // Check if at least the basic elements exist
        const hasBasicElements = elements.every(id => document.getElementById(id) !== null);
        
        // Also check if we're in a regular content context (not written or reminder)
        const isRegularContext = !this.checkForWrittenContent() && !this.checkForReminderContent();
        
        return hasBasicElements && isRegularContext;
    }

    setupWrittenContentManager() {
        console.log('UnifiedContentLoader: Setting up written content manager...');
        
        if (window.writtenBlockManager) {
            console.log('UnifiedContentLoader: WrittenBlockManager already exists');
            return;
        }

        try {
            // Import and initialize WrittenContentBlockManager
            if (typeof WrittenContentBlockManager !== 'undefined') {
                window.writtenBlockManager = new WrittenContentBlockManager();
                this.managers.set('written', window.writtenBlockManager);
                console.log('UnifiedContentLoader: WrittenBlockManager initialized');
            } else {
                console.warn('UnifiedContentLoader: WrittenContentBlockManager class not found');
            }
        } catch (error) {
            console.error('UnifiedContentLoader: Error initializing WrittenBlockManager:', error);
        }
    }

    setupReminderContentManager() {
        console.log('UnifiedContentLoader: Setting up reminder content manager...');
        
        if (window.reminderBlockManager) {
            console.log('UnifiedContentLoader: ReminderBlockManager already exists');
            return;
        }

        try {
            // Import and initialize ReminderContentBlockManager
            if (typeof ReminderContentBlockManager !== 'undefined') {
                window.reminderBlockManager = new ReminderContentBlockManager();
                this.managers.set('reminder', window.reminderBlockManager);
                console.log('UnifiedContentLoader: ReminderBlockManager initialized');
            } else {
                console.warn('UnifiedContentLoader: ReminderContentBlockManager class not found');
            }
        } catch (error) {
            console.error('UnifiedContentLoader: Error initializing ReminderBlockManager:', error);
        }
    }

    setupRegularContentManager() {
        console.log('UnifiedContentLoader: Setting up regular content manager...');
        
        // For regular content, we'll use ContentBuilderBase directly
        // since there's no specific ContentBlockManager class
        try {
            if (typeof ContentBuilderBase !== 'undefined') {
                // Create a generic content manager for regular content
                const regularManager = new ContentBuilderBase({
                    containerId: 'contentBoxesContainer',
                    emptyStateId: 'emptyState',
                    previewId: 'contentPreview',
                    hiddenFieldId: 'contentJson',
                    modalId: 'blockTypeModal',
                    contentType: 'regular'
                });
                
                window.contentBlockManager = regularManager;
                this.managers.set('regular', regularManager);
                console.log('UnifiedContentLoader: Regular content manager initialized');
            } else {
                console.warn('UnifiedContentLoader: ContentBuilderBase class not found');
            }
        } catch (error) {
            console.error('UnifiedContentLoader: Error initializing regular content manager:', error);
        }
    }

    loadAllContent() {
        console.log('UnifiedContentLoader: Loading all content...');
        
        this.managers.forEach((manager, type) => {
            this.loadContentForManager(manager, type);
        });
        
        // Notify sidebar to refresh after all content is loaded
        setTimeout(() => {
            if (window.contentSidebarManager) {
                console.log('UnifiedContentLoader: Refreshing sidebar...');
                window.contentSidebarManager.forceRefresh();
            }
        }, 1000);
    }

    loadContentForManager(manager, type) {
        if (!manager || typeof manager.loadExistingContent !== 'function') {
            console.warn(`UnifiedContentLoader: Manager for ${type} does not have loadExistingContent method`);
            return;
        }

        console.log(`UnifiedContentLoader: Loading content for ${type}...`);
        
        try {
            manager.loadExistingContent();
        } catch (error) {
            console.error(`UnifiedContentLoader: Error loading content for ${type}:`, error);
        }
    }

    // Public method to force reload all content
    forceReloadAll() {
        console.log('UnifiedContentLoader: Force reloading all content...');
        
        this.managers.forEach((manager, type) => {
            if (manager && typeof manager.loadExistingContent === 'function') {
                console.log(`UnifiedContentLoader: Force reloading ${type} content...`);
                manager.loadExistingContent();
            }
        });
        
        // Refresh sidebar
        setTimeout(() => {
            if (window.contentSidebarManager) {
                window.contentSidebarManager.forceRefresh();
            }
        }, 500);
    }

    // Public method to get manager by type
    getManager(type) {
        return this.managers.get(type);
    }

    // Public method to check if content is loaded
    isContentLoaded(type) {
        const manager = this.managers.get(type);
        if (!manager) return false;
        
        const blocks = document.querySelectorAll('.content-block, .question-block-template');
        return blocks.length > 0;
    }

    // Debug method
    debug() {
        console.log('UnifiedContentLoader Debug:', {
            isInitialized: this.isInitialized,
            managers: Array.from(this.managers.keys()),
            loadAttempts: this.loadAttempts,
            blocksInDOM: document.querySelectorAll('.content-block, .question-block-template').length
        });
        
        this.managers.forEach((manager, type) => {
            console.log(`${type} manager:`, {
                exists: !!manager,
                hasLoadMethod: !!(manager && typeof manager.loadExistingContent === 'function'),
                blocks: manager ? manager.blocks?.length || 0 : 0
            });
        });
    }
}

// Initialize unified content loader
function initializeUnifiedContentLoader() {
    try {
        if (window.unifiedContentLoader) {
            console.log('UnifiedContentLoader: Already initialized');
            return;
        }

        console.log('UnifiedContentLoader: Initializing...');
        window.unifiedContentLoader = new UnifiedContentLoader();
        console.log('UnifiedContentLoader: Successfully initialized');
        
    } catch (error) {
        console.error('Error initializing UnifiedContentLoader:', error);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(initializeUnifiedContentLoader, 100);
});

// Also try to initialize immediately if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initializeUnifiedContentLoader, 100);
    });
} else {
    setTimeout(initializeUnifiedContentLoader, 100);
}

// Make functions available globally for debugging
window.initializeUnifiedContentLoader = initializeUnifiedContentLoader;
window.debugUnifiedLoader = () => {
    if (window.unifiedContentLoader) {
        window.unifiedContentLoader.debug();
    } else {
        console.log('UnifiedContentLoader not initialized');
    }
};
window.forceReloadAllContent = () => {
    if (window.unifiedContentLoader) {
        window.unifiedContentLoader.forceReloadAll();
    } else {
        console.log('UnifiedContentLoader not initialized');
    }
};

// Test function to check all content types
window.testAllContentTypes = () => {
    console.log('=== Testing All Content Types ===');
    
    // Check written content
    console.log('Written Content:');
    console.log('- Manager exists:', !!window.writtenBlockManager);
    console.log('- Blocks in DOM:', document.querySelectorAll('#contentBlocksList .content-block, #contentBlocksList .question-block-template').length);
    console.log('- Hidden field:', document.getElementById('writtenContentJson')?.value ? 'Has data' : 'Empty');
    
    // Check reminder content
    console.log('Reminder Content:');
    console.log('- Manager exists:', !!window.reminderBlockManager);
    console.log('- Blocks in DOM:', document.querySelectorAll('#contentBlocksList .content-block, #contentBlocksList .question-block-template').length);
    console.log('- Hidden field:', document.getElementById('reminderContentJson')?.value ? 'Has data' : 'Empty');
    
    // Check regular content
    console.log('Regular Content:');
    console.log('- Manager exists:', !!window.contentBlockManager);
    console.log('- Blocks in DOM:', document.querySelectorAll('#contentBoxesContainer .content-block').length);
    console.log('- Hidden field:', document.getElementById('contentJson')?.value ? 'Has data' : 'Empty');
    
    // Check sidebar
    console.log('Sidebar:');
    console.log('- Manager exists:', !!window.contentSidebarManager);
    console.log('- Sidebar items:', document.querySelectorAll('#blocksNavigation .block-nav-item').length);
    
    // Check unified loader
    console.log('Unified Loader:');
    console.log('- Loader exists:', !!window.unifiedContentLoader);
    if (window.unifiedContentLoader) {
        window.unifiedContentLoader.debug();
    }
};

// Export for use in other files
if (typeof window !== 'undefined') {
    window.UnifiedContentLoader = UnifiedContentLoader;
}
