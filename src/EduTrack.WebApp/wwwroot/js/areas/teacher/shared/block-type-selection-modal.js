/**
 * Ultra-Modern Block Type Selection Modal Manager
 * Handles the modal for selecting content block types with advanced animations
 */

// Define BlockTypeSelectionModal class globally (with duplicate protection)
if (typeof window.BlockTypeSelectionModal === 'undefined') {
window.BlockTypeSelectionModal = class BlockTypeSelectionModal {
    constructor(options = {}) {
        this.modalId = options.modalId || 'blockTypeModal';
        this.onTypeSelected = options.onTypeSelected || null;
        this.isInitialized = false;
        this.animationDuration = options.animationDuration || 300;
        this.enableHapticFeedback = options.enableHapticFeedback !== false;
        this.enableSoundEffects = options.enableSoundEffects || false;
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        this.setupEventListeners();
        this.setupAdvancedInteractions();
        this.isInitialized = true;
    }

    setupEventListeners() {
        const modal = document.getElementById(this.modalId);
        if (!modal) {
            console.error(`Modal with ID '${this.modalId}' not found`);
            return;
        }

        // Handle block type selection with enhanced animations
        const blockTypeItems = modal.querySelectorAll('.block-type-option');
        blockTypeItems.forEach((item, index) => {
            // Add staggered animation delay
            item.style.animationDelay = `${index * 0.1}s`;
            
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const type = item.dataset.type;
                this.selectBlockType(type, item);
            });

            // Enhanced keyboard support
            item.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    const type = item.dataset.type;
                    this.selectBlockType(type, item);
                }
            });

            // Add hover sound effect
            if (this.enableSoundEffects) {
                item.addEventListener('mouseenter', () => {
                    this.playHoverSound();
                });
            }

            // Make items focusable
            item.setAttribute('tabindex', '0');
        });

        // Enhanced modal events
        modal.addEventListener('hidden.bs.modal', () => {
            this.cleanupModalBackdrops();
            this.resetAnimations();
        });

        modal.addEventListener('show.bs.modal', () => {
            this.cleanupModalBackdrops();
            this.prepareAnimations();
        });

        modal.addEventListener('shown.bs.modal', () => {
            this.startEntranceAnimations();
        });
    }

    setupAdvancedInteractions() {
        const modal = document.getElementById(this.modalId);
        if (!modal) return;

        // Add ripple effect on click
        const blockTypeItems = modal.querySelectorAll('.block-type-option');
        blockTypeItems.forEach(item => {
            item.addEventListener('click', (e) => {
                this.createRippleEffect(e, item);
            });
        });

        // Add magnetic effect on mouse move
        blockTypeItems.forEach(item => {
            item.addEventListener('mousemove', (e) => {
                this.addMagneticEffect(e, item);
            });

            item.addEventListener('mouseleave', () => {
                this.removeMagneticEffect(item);
            });
        });
    }

    createRippleEffect(event, element) {
        const ripple = document.createElement('span');
        const rect = element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;
        
        ripple.style.cssText = `
            position: absolute;
            width: ${size}px;
            height: ${size}px;
            left: ${x}px;
            top: ${y}px;
            background: rgba(102, 126, 234, 0.3);
            border-radius: 50%;
            transform: scale(0);
            animation: ripple 0.6s ease-out;
            pointer-events: none;
            z-index: 1;
        `;
        
        element.style.position = 'relative';
        element.style.overflow = 'hidden';
        element.appendChild(ripple);
        
        setTimeout(() => {
            ripple.remove();
        }, 600);
    }

    addMagneticEffect(event, element) {
        const rect = element.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;
        const deltaX = (event.clientX - centerX) * 0.1;
        const deltaY = (event.clientY - centerY) * 0.1;
        
        element.style.transform = `translate(${deltaX}px, ${deltaY}px)`;
    }

    removeMagneticEffect(element) {
        element.style.transform = '';
    }

    prepareAnimations() {
        const modal = document.getElementById(this.modalId);
        if (!modal) return;

        const blockTypeItems = modal.querySelectorAll('.block-type-option');
        blockTypeItems.forEach(item => {
            item.style.opacity = '0';
            item.style.transform = 'translateY(20px) scale(0.9)';
        });
    }

    startEntranceAnimations() {
        const modal = document.getElementById(this.modalId);
        if (!modal) return;

        const blockTypeItems = modal.querySelectorAll('.block-type-option');
        blockTypeItems.forEach((item, index) => {
            setTimeout(() => {
                item.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
                item.style.opacity = '1';
                item.style.transform = 'translateY(0) scale(1)';
            }, index * 100);
        });
    }

    resetAnimations() {
        const modal = document.getElementById(this.modalId);
        if (!modal) return;

        const blockTypeItems = modal.querySelectorAll('.block-type-option');
        blockTypeItems.forEach(item => {
            item.style.transition = '';
            item.style.opacity = '';
            item.style.transform = '';
        });
    }

    selectBlockType(type, element) {
        // Add haptic feedback if supported
        if (this.enableHapticFeedback && 'vibrate' in navigator) {
            navigator.vibrate(50);
        }

        // Play selection sound
        if (this.enableSoundEffects) {
            this.playSelectionSound();
        }

        // Add visual feedback with enhanced animation
        element.classList.add('loading');
        
        // Create success animation
        this.createSuccessAnimation(element);
        
        // Call the callback if provided
        if (this.onTypeSelected && typeof this.onTypeSelected === 'function') {
            try {
                // Add slight delay for better UX
                setTimeout(() => {
                    this.onTypeSelected(type);
                }, 200);
            } catch (error) {
                console.error('Error in onTypeSelected callback:', error);
            }
        }

        // Hide the modal with delay
        setTimeout(() => {
            this.hideModal();
        }, 300);

        // Remove loading state after animation
        setTimeout(() => {
            element.classList.remove('loading');
        }, 600);
    }

    createSuccessAnimation(element) {
        const successIcon = document.createElement('div');
        successIcon.innerHTML = '<i class="fas fa-check"></i>';
        successIcon.style.cssText = `
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%) scale(0);
            background: linear-gradient(135deg, #10b981, #059669);
            color: white;
            width: 40px;
            height: 40px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.2rem;
            z-index: 10;
            animation: successPop 0.6s cubic-bezier(0.68, -0.55, 0.265, 1.55);
        `;
        
        element.style.position = 'relative';
        element.appendChild(successIcon);
        
        setTimeout(() => {
            successIcon.remove();
        }, 600);
    }

    playHoverSound() {
        // Create a subtle hover sound using Web Audio API
        if (typeof AudioContext !== 'undefined' || typeof webkitAudioContext !== 'undefined') {
            const audioContext = new (AudioContext || webkitAudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();
            
            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);
            
            oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
            oscillator.frequency.exponentialRampToValueAtTime(1000, audioContext.currentTime + 0.1);
            
            gainNode.gain.setValueAtTime(0, audioContext.currentTime);
            gainNode.gain.linearRampToValueAtTime(0.1, audioContext.currentTime + 0.01);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1);
            
            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.1);
        }
    }

    playSelectionSound() {
        // Create a success sound
        if (typeof AudioContext !== 'undefined' || typeof webkitAudioContext !== 'undefined') {
            const audioContext = new (AudioContext || webkitAudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();
            
            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);
            
            oscillator.frequency.setValueAtTime(523, audioContext.currentTime); // C5
            oscillator.frequency.setValueAtTime(659, audioContext.currentTime + 0.1); // E5
            oscillator.frequency.setValueAtTime(784, audioContext.currentTime + 0.2); // G5
            
            gainNode.gain.setValueAtTime(0, audioContext.currentTime);
            gainNode.gain.linearRampToValueAtTime(0.2, audioContext.currentTime + 0.01);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.3);
            
            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.3);
        }
    }

    showModal(scheduleItemType) {
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
