/**
 * Block Type Selection Modal Manager
 * Handles the modal for selecting content block types
 */

// Define BlockTypeSelectionModal class globally (with duplicate protection)
if (typeof window.BlockTypeSelectionModal === 'undefined') {
window.BlockTypeSelectionModal = class BlockTypeSelectionModal {
    constructor(options = {}) {
        this.modalId = options.modalId || 'blockTypeModal';
        this.onTypeSelected = options.onTypeSelected || null;
        this.isInitialized = false;
        this.currentItemType = null;
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        this.setupEventListeners();
        this.isInitialized = true;
    }

    setupEventListeners() {
        const modal = document.getElementById(this.modalId);
        if (!modal) return;

        const blockTypeItems = modal.querySelectorAll('.block-type-option');
        blockTypeItems.forEach((item) => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const type = item.dataset.type;
                this.selectBlockType(type, item);
            });

            item.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    const type = item.dataset.type;
                    this.selectBlockType(type, item);
                }
            });

            item.setAttribute('tabindex', '0');
        });

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
                setTimeout(() => {
                    this.onTypeSelected(type);
                }, 100);
            } catch (error) {
                console.error('Error in onTypeSelected callback:', error);
            }
        }

        // Hide the modal
        setTimeout(() => {
            this.hideModal();
        }, 200);

        // Remove loading state
        setTimeout(() => {
            element.classList.remove('loading');
        }, 300);
    }

    showModal(scheduleItemType) {
        const modal = document.getElementById(this.modalId);
        if (modal) {
            this.loadBlockTypes(scheduleItemType || 'reminder');
            this.cleanupModalBackdrops();
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
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

    loadBlockTypes(itemType) {
        this.currentItemType = itemType;
        const blockTypeList = document.getElementById('blockTypeList');
        if (!blockTypeList) return;

        const config = this.getBlockTypeConfig(itemType);
        blockTypeList.innerHTML = '';
        this.loadBlockTypeOptions(config, blockTypeList);
    }

    getBlockTypeConfig(itemType) {
        const configs = {
            'reminder': {
                showRegularBlocks: true,
                showQuestionBlocks: false,
                itemType: 'reminder'
            },
            'writing': {
                showRegularBlocks: false,
                showQuestionBlocks: true,
                itemType: 'writing'
            },
            'quiz': {
                showRegularBlocks: true,
                showQuestionBlocks: true,
                itemType: 'quiz'
            },
            'multiplechoice': {
                showRegularBlocks: false,
                showQuestionBlocks: true,
                itemType: 'multiplechoice'
            },
            'gapfill': {
                showRegularBlocks: false,
                showQuestionBlocks: true,
                itemType: 'gapfill'
            },
            'match': {
                showRegularBlocks: false,
                showQuestionBlocks: true,
                itemType: 'match'
            },
            'errorfinding': {
                showRegularBlocks: false,
                showQuestionBlocks: true,
                itemType: 'errorfinding'
            },
            'codeexercise': {
                showRegularBlocks: false,
                showQuestionBlocks: true,
                itemType: 'codeexercise'
            }
        };

        return configs[itemType] || configs['reminder']; // Default to reminder
    }

    loadBlockTypeOptions(config, container) {
        const formData = new FormData();
        formData.append('itemType', config.itemType);
        formData.append('showRegularBlocks', config.showRegularBlocks);
        formData.append('showQuestionBlocks', config.showQuestionBlocks);

        fetch('/Teacher/ScheduleItem/GetBlockTypeOptions', {
            method: 'POST',
            body: formData
        })
        .then(response => response.text())
        .then(html => {
            container.innerHTML = html;
            this.setupEventListeners();
        })
        .catch(error => {
            console.error('Error loading block types:', error);
        });
    }
}

// Global function for backward compatibility
function showBlockTypeModal(itemType = null) {
    if (window.blockTypeSelectionModal) {
        window.blockTypeSelectionModal.showModal(itemType);
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