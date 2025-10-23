/**
 * Block Type Selection Modal Manager
 * Handles the modal for selecting content block types
 */

// Check if BlockTypeSelectionModal is already defined to prevent duplicate declarations
if (typeof BlockTypeSelectionModal === 'undefined') {
class BlockTypeSelectionModal {
    constructor(options = {}) {
        this.modalId = options.modalId || 'blockTypeModal';
        this.onTypeSelected = options.onTypeSelected || null;
        this.isInitialized = false;
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        this.setupEventListeners();
        this.isInitialized = true;
    }

    setupEventListeners() {
        const modal = document.getElementById(this.modalId);
        if (!modal) {
            console.error(`Modal with ID '${this.modalId}' not found`);
            return;
        }

        // Handle block type selection
        const blockTypeItems = modal.querySelectorAll('.block-type-option');
        blockTypeItems.forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const type = item.dataset.type;
                this.selectBlockType(type, item);
            });

            // Add keyboard support
            item.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    const type = item.dataset.type;
                    this.selectBlockType(type, item);
                }
            });

            // Make items focusable
            item.setAttribute('tabindex', '0');
        });

        // Handle modal events
        modal.addEventListener('hidden.bs.modal', () => {
            this.cleanupModalBackdrops();
        });

        modal.addEventListener('show.bs.modal', () => {
            this.cleanupModalBackdrops();
        });
    }

    selectBlockType(type, element) {
        
        // Add visual feedback
        element.classList.add('loading');
        
        // Call the callback if provided
        if (this.onTypeSelected && typeof this.onTypeSelected === 'function') {
            try {
                this.onTypeSelected(type);
            } catch (error) {
                console.error('Error in onTypeSelected callback:', error);
            }
        }

        // Hide the modal
        this.hideModal();

        // Remove loading state after a short delay
        setTimeout(() => {
            element.classList.remove('loading');
        }, 300);
    }

    showModal() {
        const modal = document.getElementById(this.modalId);
        if (modal) {
            // Clean up any existing backdrops first
            this.cleanupModalBackdrops();
            
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } else {
            console.error(`Modal with ID '${this.modalId}' not found`);
        }
    }

    hideModal() {
        const modal = document.getElementById(this.modalId);
        if (modal) {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
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

    // Public method to update the callback
    setOnTypeSelected(callback) {
        this.onTypeSelected = callback;
    }

    // Public method to get available block types
    getAvailableTypes() {
        const modal = document.getElementById(this.modalId);
        if (!modal) return [];
        
        const items = modal.querySelectorAll('.block-type-option');
        return Array.from(items).map(item => item.dataset.type);
    }

    // Public method to enable/disable specific block types
    setTypeEnabled(type, enabled) {
        const modal = document.getElementById(this.modalId);
        if (!modal) return;
        
        const item = modal.querySelector(`[data-type="${type}"]`);
        if (item) {
            if (enabled) {
                item.style.display = '';
                item.removeAttribute('disabled');
            } else {
                item.style.display = 'none';
                item.setAttribute('disabled', 'true');
            }
        }
    }
}

// Global function for backward compatibility
function showBlockTypeModal() {
    if (window.blockTypeSelectionModal) {
        window.blockTypeSelectionModal.showModal();
    } else {
        console.warn('Block Type Selection Modal not initialized');
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize the modal if it exists
    const modal = document.getElementById('blockTypeModal');
    if (modal && !window.blockTypeSelectionModal) {
        window.blockTypeSelectionModal = new BlockTypeSelectionModal({
            modalId: 'blockTypeModal',
            onTypeSelected: function(type) {
                // This can be overridden by specific implementations
            }
        });
    }
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = BlockTypeSelectionModal;
}

} // End of BlockTypeSelectionModal class definition check
