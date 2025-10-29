/**
 * Unified Content Loader
 * Handles loading existing content for all content types (written, reminder, regular)
 * Provides a single interface for content loading across different managers
 */

// Prevent duplicate declaration
if (typeof UnifiedContentLoader === 'undefined') {
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
        // Check for written content
        const hasWrittenContent = this.checkForWrittenContent();
        if (hasWrittenContent) {
            this.setupWrittenContentManager();
        }
        
        // Check for reminder content
        const hasReminderContent = this.checkForReminderContent();
        if (hasReminderContent) {
            this.setupReminderContentManager();
        }
        
        // Check for regular content
        const hasRegularContent = this.checkForRegularContent();
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
        
        if (window.writtenBlockManager) {
            return;
        }

        try {
            // Import and initialize WrittenContentBlockManager
            if (typeof WrittenContentBlockManager !== 'undefined') {
                window.writtenBlockManager = new WrittenContentBlockManager();
                this.managers.set('written', window.writtenBlockManager);
            } else {
                console.warn('UnifiedContentLoader: WrittenContentBlockManager class not found');
            }
        } catch (error) {
            console.error('UnifiedContentLoader: Error initializing WrittenBlockManager:', error);
        }
    }

    setupReminderContentManager() {
        
        if (window.reminderBlockManager) {
            return;
        }

        try {
            // Import and initialize ReminderContentBlockManager
            if (typeof ReminderContentBlockManager !== 'undefined') {
                window.reminderBlockManager = new ReminderContentBlockManager();
                this.managers.set('reminder', window.reminderBlockManager);
            } else {
                console.warn('UnifiedContentLoader: ReminderContentBlockManager class not found');
            }
        } catch (error) {
            console.error('UnifiedContentLoader: Error initializing ReminderBlockManager:', error);
        }
    }

    setupRegularContentManager() {
        
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
            } else {
                console.warn('UnifiedContentLoader: ContentBuilderBase class not found');
            }
        } catch (error) {
            console.error('UnifiedContentLoader: Error initializing regular content manager:', error);
        }
    }

    loadAllContent() {
        
        this.managers.forEach((manager, type) => {
            this.loadContentForManager(manager, type);
        });
        
        // Notify sidebar to refresh after all content is loaded
        setTimeout(() => {
            if (window.contentSidebarManager) {
                window.contentSidebarManager.forceRefresh();
            }
        }, 1000);
    }

    loadContentForManager(manager, type) {
        if (!manager || typeof manager.loadExistingContent !== 'function') {
            console.warn(`UnifiedContentLoader: Manager for ${type} does not have loadExistingContent method`);
            return;
        }

        
        try {
            manager.loadExistingContent();
        } catch (error) {
            console.error(`UnifiedContentLoader: Error loading content for ${type}:`, error);
        }
    }

    // Public method to force reload all content
    forceReloadAll() {
        
        this.managers.forEach((manager, type) => {
            if (manager && typeof manager.loadExistingContent === 'function') {
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

}

// Initialize unified content loader
function initializeUnifiedContentLoader() {
    try {
        if (window.unifiedContentLoader) {
            return;
        }

        window.unifiedContentLoader = new UnifiedContentLoader();
        
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
    }
};
window.forceReloadAllContent = () => {
    if (window.unifiedContentLoader) {
        window.unifiedContentLoader.forceReloadAll();
    } else {
    }
};

// Export for use in other files
if (typeof window !== 'undefined') {
    window.UnifiedContentLoader = UnifiedContentLoader;
    window.unifiedContentLoader = new UnifiedContentLoader();
}

} // End of if (typeof UnifiedContentLoader === 'undefined')
