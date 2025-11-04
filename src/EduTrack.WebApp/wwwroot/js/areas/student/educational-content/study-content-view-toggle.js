/**
 * Study Content View Toggle
 * Toggles between unified view (all blocks visible) and step-by-step view (one block at a time)
 */

class StudyContentViewToggle {
    constructor() {
        this.container = document.getElementById('reminder-content');
        this.currentViewMode = 'unified'; // 'unified' or 'step-by-step'
        this.currentBlockIndex = 0;
        this.blocks = [];
        
        if (!this.container) {
            console.warn('Reminder content container not found');
            return;
        }
        
        this.init();
    }
    
    init() {
        this.collectBlocks();
        this.createToggleButton();
        this.bindEvents();
        this.updateView();
    }
    
    collectBlocks() {
        // Get all content blocks (both content and question blocks)
        this.blocks = Array.from(this.container.querySelectorAll('.content-block'));
        
        // Sort by order attribute
        this.blocks.sort((a, b) => {
            const orderA = parseInt(a.dataset.blockOrder || '0');
            const orderB = parseInt(b.dataset.blockOrder || '0');
            return orderA - orderB;
        });
    }
    
    createToggleButton() {
        // Find the header actions area
        const headerActions = document.querySelector('.header-actions-minimal');
        if (!headerActions) {
            console.warn('Header actions not found, cannot add toggle button');
            return;
        }
        
        // Create toggle button
        const toggleButton = document.createElement('button');
        toggleButton.className = 'btn-icon-minimal btn-view-toggle';
        toggleButton.id = 'view-toggle-btn';
        toggleButton.setAttribute('title', 'تغییر نمایش');
        toggleButton.innerHTML = '<i class="fas fa-th"></i>';
        
        // Insert before the session toggle button
        const sessionToggle = document.getElementById('study-session-toggle');
        if (sessionToggle && sessionToggle.parentNode) {
            sessionToggle.parentNode.insertBefore(toggleButton, sessionToggle);
        } else {
            headerActions.appendChild(toggleButton);
        }
    }
    
    bindEvents() {
        const toggleButton = document.getElementById('view-toggle-btn');
        if (toggleButton) {
            toggleButton.addEventListener('click', () => this.toggleView());
        }
        
        // Navigation buttons for step-by-step view
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-block-next')) {
                e.preventDefault();
                this.nextBlock();
            }
            if (e.target.closest('.btn-block-prev')) {
                e.preventDefault();
                this.prevBlock();
            }
        });
    }
    
    toggleView() {
        this.currentViewMode = this.currentViewMode === 'unified' ? 'step-by-step' : 'unified';
        this.container.setAttribute('data-view-mode', this.currentViewMode);
        this.updateView();
        this.updateToggleButton();
    }
    
    updateView() {
        if (this.currentViewMode === 'unified') {
            this.showUnifiedView();
        } else {
            this.showStepByStepView();
        }
    }
    
    showUnifiedView() {
        // Show all blocks
        this.blocks.forEach((block, index) => {
            block.style.display = '';
            block.classList.remove('block-hidden');
            block.classList.add('block-visible');
        });
        
        // Remove navigation buttons
        this.removeNavigationButtons();
    }
    
    showStepByStepView() {
        // Hide all blocks except current one
        this.blocks.forEach((block, index) => {
            if (index === this.currentBlockIndex) {
                block.style.display = '';
                block.classList.remove('block-hidden');
                block.classList.add('block-visible', 'block-active');
            } else {
                block.style.display = 'none';
                block.classList.add('block-hidden');
                block.classList.remove('block-visible', 'block-active');
            }
        });
        
        // Add navigation buttons
        this.addNavigationButtons();
    }
    
    addNavigationButtons() {
        // Remove existing navigation if any
        this.removeNavigationButtons();
        
        // Create navigation container
        const navContainer = document.createElement('div');
        navContainer.className = 'block-navigation';
        
        // Previous button
        const prevButton = document.createElement('button');
        prevButton.className = 'btn-block-nav btn-block-prev';
        prevButton.disabled = this.currentBlockIndex === 0;
        prevButton.innerHTML = '<i class="fas fa-arrow-right"></i> قبلی';
        prevButton.addEventListener('click', () => this.prevBlock());
        
        // Block counter
        const counter = document.createElement('div');
        counter.className = 'block-counter';
        counter.textContent = `${this.currentBlockIndex + 1} / ${this.blocks.length}`;
        
        // Next button
        const nextButton = document.createElement('button');
        nextButton.className = 'btn-block-nav btn-block-next';
        nextButton.disabled = this.currentBlockIndex === this.blocks.length - 1;
        nextButton.innerHTML = 'بعدی <i class="fas fa-arrow-left"></i>';
        nextButton.addEventListener('click', () => this.nextBlock());
        
        navContainer.appendChild(prevButton);
        navContainer.appendChild(counter);
        navContainer.appendChild(nextButton);
        
        // Insert after the container
        this.container.parentNode.insertBefore(navContainer, this.container.nextSibling);
    }
    
    removeNavigationButtons() {
        const navContainer = this.container.parentNode.querySelector('.block-navigation');
        if (navContainer) {
            navContainer.remove();
        }
    }
    
    nextBlock() {
        if (this.currentBlockIndex < this.blocks.length - 1) {
            this.currentBlockIndex++;
            this.showStepByStepView();
        }
    }
    
    prevBlock() {
        if (this.currentBlockIndex > 0) {
            this.currentBlockIndex--;
            this.showStepByStepView();
        }
    }
    
    updateToggleButton() {
        const toggleButton = document.getElementById('view-toggle-btn');
        if (!toggleButton) return;
        
        const icon = toggleButton.querySelector('i');
        if (this.currentViewMode === 'unified') {
            icon.className = 'fas fa-th';
            toggleButton.setAttribute('title', 'نمایش مرحله‌ای');
        } else {
            icon.className = 'fas fa-list';
            toggleButton.setAttribute('title', 'نمایش یکپارچه');
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('reminder-content')) {
        window.studyContentViewToggle = new StudyContentViewToggle();
    }
});

