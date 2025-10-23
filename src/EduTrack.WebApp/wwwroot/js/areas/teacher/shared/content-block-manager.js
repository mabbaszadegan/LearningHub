/**
 * Shared Content Block Manager
 * Coordinates between different content builders (reminder, written, etc.)
 * and manages the block type selection modal
 */

class SharedContentBlockManager {
    constructor() {
        this.currentModalId = null;
        this.isInitialized = false;
        this.modalInstances = new Map();
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        this.setupEventListeners();
        this.initializeModals();
        this.isInitialized = true;
    }

    setupEventListeners() {
        // Listen for block type selection events
        document.addEventListener('blockTypeSelected', (e) => {
        });

        // Clean up modal backdrops on page unload
        window.addEventListener('beforeunload', () => {
            this.cleanupModalBackdrops();
        });

        // Clean up on page visibility change
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this.cleanupModalBackdrops();
            }
        });
    }

    initializeModals() {
        // Find all block type selection modals
        const modals = document.querySelectorAll('[id$="TypeModal"], [id$="typeModal"]');
        
        modals.forEach(modal => {
            const modalId = modal.id;
            
            // Create modal instance
            const modalInstance = new BlockTypeSelectionModal({
                modalId: modalId,
                onTypeSelected: (type) => {
                    this.handleBlockTypeSelection(type, modalId);
                }
            });
            
            this.modalInstances.set(modalId, modalInstance);
        });
    }

    handleBlockTypeSelection(type, modalId) {
        
        // Dispatch custom event for other components to listen
        const event = new CustomEvent('blockTypeSelected', {
            detail: {
                type: type,
                modalId: modalId
            }
        });
        
        document.dispatchEvent(event);
    }

    showBlockTypeModal(modalId) {
        
        if (!modalId) {
            console.error('Modal ID is required');
            return;
        }

        const modalInstance = this.modalInstances.get(modalId);
        if (modalInstance) {
            this.currentModalId = modalId;
            modalInstance.showModal();
        } else {
            console.error(`Modal instance not found for ID: ${modalId}`);
            
            // Try to initialize the modal if it exists in DOM
            const modal = document.getElementById(modalId);
            if (modal) {
                const newInstance = new BlockTypeSelectionModal({
                    modalId: modalId,
                    onTypeSelected: (type) => {
                        this.handleBlockTypeSelection(type, modalId);
                    }
                });
                
                this.modalInstances.set(modalId, newInstance);
                this.currentModalId = modalId;
                newInstance.showModal();
            } else {
                console.error(`Modal element not found in DOM: ${modalId}`);
                alert('مودال انتخاب نوع بلاک یافت نشد. لطفاً صفحه را رفرش کنید.');
            }
        }
    }

    hideBlockTypeModal(modalId) {
        if (!modalId) {
            modalId = this.currentModalId;
        }
        
        if (modalId) {
            const modalInstance = this.modalInstances.get(modalId);
            if (modalInstance) {
                modalInstance.hideModal();
            }
        }
    }

    cleanupModalBackdrops() {
        
        const backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach(backdrop => {
            backdrop.remove();
        });
        
        // Reset body styles
        document.body.classList.remove('modal-open');
        document.body.style.paddingRight = '';
        document.body.style.overflow = '';
        document.body.style.overflowX = '';
        document.body.style.overflowY = '';
        document.documentElement.style.overflow = '';
        document.documentElement.style.overflowX = '';
        document.documentElement.style.overflowY = '';
    }

    // Public method to get available block types for a specific modal
    getAvailableTypes(modalId) {
        const modalInstance = this.modalInstances.get(modalId);
        if (modalInstance) {
            return modalInstance.getAvailableTypes();
        }
        return [];
    }

    // Public method to enable/disable specific block types
    setTypeEnabled(modalId, type, enabled) {
        const modalInstance = this.modalInstances.get(modalId);
        if (modalInstance) {
            modalInstance.setTypeEnabled(type, enabled);
        }
    }

    // Public method to check if a modal is currently open
    isModalOpen(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            return modal.classList.contains('show');
        }
        return false;
    }

    // Public method to get current modal ID
    getCurrentModalId() {
        return this.currentModalId;
    }
}

// Global functions for backward compatibility
function showBlockTypeModal(modalId) {
    if (window.sharedContentBlockManager) {
        window.sharedContentBlockManager.showBlockTypeModal(modalId);
    } else {
        console.warn('Shared Content Block Manager not available');
        alert('سیستم مدیریت بلاک‌ها هنوز آماده نیست. لطفاً صفحه را رفرش کنید.');
    }
}

function showWrittenQuestionBlockTypeModal() {
    showBlockTypeModal('questionTypeModal');
}

function showQuestionTypeModal() {
    showBlockTypeModal('questionTypeModal');
}

// Initialize the shared manager when DOM is ready
function initializeSharedContentBlockManager() {
    
    if (window.sharedContentBlockManager) {
        return;
    }
    
    try {
        window.sharedContentBlockManager = new SharedContentBlockManager();
    } catch (error) {
        console.error('Error initializing Shared Content Block Manager:', error);
    }
}

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeSharedContentBlockManager);
} else {
    initializeSharedContentBlockManager();
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SharedContentBlockManager;
}
