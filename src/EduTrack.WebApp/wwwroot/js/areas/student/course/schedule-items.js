/**
 * Student Schedule Items JavaScript
 * Handles filtering, searching, and interactive functionality
 */

class ScheduleItemsManager {
    constructor() {
        this.scheduleItems = [];
        this.filteredItems = [];
        this.currentFilters = {
            type: '',
            status: '',
            search: ''
        };
        
        this.init();
    }

    init() {
        this.collectScheduleItems();
        this.bindEvents();
        this.initializeTooltips();
        this.setupIntersectionObserver();
        this.updateStatistics();
    }

    collectScheduleItems() {
        const itemCards = document.querySelectorAll('.schedule-item-card');
        this.scheduleItems = Array.from(itemCards).map(card => ({
            element: card,
            type: card.dataset.type,
            status: card.dataset.status,
            title: card.dataset.title,
            description: card.dataset.description
        }));
        
        this.filteredItems = [...this.scheduleItems];
    }

    bindEvents() {
        // Type filter
        const typeFilter = document.getElementById('typeFilter');
        if (typeFilter) {
            typeFilter.addEventListener('change', (e) => {
                this.currentFilters.type = e.target.value;
                this.applyFilters();
            });
        }

        // Status filter
        const statusFilter = document.getElementById('statusFilter');
        if (statusFilter) {
            statusFilter.addEventListener('change', (e) => {
                this.currentFilters.status = e.target.value;
                this.applyFilters();
            });
        }

        // Search input
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.currentFilters.search = e.target.value.toLowerCase();
                this.applyFilters();
            });
        }

        // Clear filters button
        const clearFiltersBtn = document.getElementById('clearFilters');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', () => {
                this.clearFilters();
            });
        }

        // Schedule item actions
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-start')) {
                e.preventDefault();
                const itemId = e.target.closest('.btn-start').onclick?.toString().match(/startScheduleItem\((\d+)\)/)?.[1];
                if (itemId) {
                    this.startScheduleItem(parseInt(itemId));
                }
            }
            
            if (e.target.closest('.btn-details')) {
                e.preventDefault();
                const itemId = e.target.closest('.btn-details').onclick?.toString().match(/showItemDetails\((\d+)\)/)?.[1];
                if (itemId) {
                    this.showItemDetails(parseInt(itemId));
                }
            }
        });

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + F to focus search
            if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
                e.preventDefault();
                const searchInput = document.getElementById('searchInput');
                if (searchInput) {
                    searchInput.focus();
                    searchInput.select();
                }
            }
            
            // Escape to clear filters
            if (e.key === 'Escape') {
                this.clearFilters();
            }
        });
    }

    initializeTooltips() {
        // Initialize Bootstrap tooltips if available
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    }

    setupIntersectionObserver() {
        // Lazy loading for schedule item cards
        if ('IntersectionObserver' in window) {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('animate-in');
                        observer.unobserve(entry.target);
                    }
                });
            }, {
                threshold: 0.1,
                rootMargin: '50px'
            });

            document.querySelectorAll('.schedule-item-card').forEach(card => {
                observer.observe(card);
            });
        }
    }

    applyFilters() {
        this.filteredItems = this.scheduleItems.filter(item => {
            // Type filter
            if (this.currentFilters.type && item.type !== this.currentFilters.type) {
                return false;
            }

            // Status filter
            if (this.currentFilters.status && item.status !== this.currentFilters.status) {
                return false;
            }

            // Search filter
            if (this.currentFilters.search) {
                const searchTerm = this.currentFilters.search;
                const matchesTitle = item.title.includes(searchTerm);
                const matchesDescription = item.description.includes(searchTerm);
                
                if (!matchesTitle && !matchesDescription) {
                    return false;
                }
            }

            return true;
        });

        this.updateDisplay();
        this.updateStatistics();
        this.updateFilterCounts();
    }

    updateDisplay() {
        // Hide all items first
        this.scheduleItems.forEach(item => {
            item.element.classList.add('filtered-out');
            item.element.classList.remove('filtered-in');
        });

        // Show filtered items with animation
        setTimeout(() => {
            this.filteredItems.forEach((item, index) => {
                setTimeout(() => {
                    item.element.classList.remove('filtered-out');
                    item.element.classList.add('filtered-in');
                }, index * 50); // Staggered animation
            });
        }, 100);

        // Show empty state if no items match
        this.checkEmptyState();
    }

    updateStatistics() {
        const totalItems = this.filteredItems.length;
        const completedItems = this.filteredItems.filter(item => item.status === 'Completed').length;
        const activeItems = this.filteredItems.filter(item => item.status === 'Active').length;
        const overdueItems = this.filteredItems.filter(item => item.status === 'Overdue').length;

        // Update statistics with animation
        this.animateNumber('totalItems', totalItems);
        this.animateNumber('completedItems', completedItems);
        this.animateNumber('activeItems', activeItems);
        this.animateNumber('overdueItems', overdueItems);
    }

    animateNumber(elementId, targetValue) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const startValue = parseInt(element.textContent) || 0;
        const duration = 500;
        const startTime = performance.now();

        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            
            // Easing function for smooth animation
            const easeOutCubic = 1 - Math.pow(1 - progress, 3);
            const currentValue = Math.round(startValue + (targetValue - startValue) * easeOutCubic);
            
            element.textContent = currentValue;

            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        };

        requestAnimationFrame(animate);
    }

    updateFilterCounts() {
        // Update filter dropdown counts (if needed)
        const typeFilter = document.getElementById('typeFilter');
        if (typeFilter) {
            const options = typeFilter.querySelectorAll('option');
            options.forEach(option => {
                if (option.value) {
                    const count = this.scheduleItems.filter(item => item.type === option.value).length;
                    const originalText = option.textContent.split(' (')[0];
                    option.textContent = `${originalText} (${count})`;
                }
            });
        }
    }

    clearFilters() {
        this.currentFilters = {
            type: '',
            status: '',
            search: ''
        };

        // Reset form elements
        const typeFilter = document.getElementById('typeFilter');
        const statusFilter = document.getElementById('statusFilter');
        const searchInput = document.getElementById('searchInput');

        if (typeFilter) typeFilter.value = '';
        if (statusFilter) statusFilter.value = '';
        if (searchInput) searchInput.value = '';

        // Apply filters to show all items
        this.applyFilters();
        
        // Show success message
        this.showToast('فیلترها پاک شدند', 'success');
    }

    checkEmptyState() {
        const grid = document.getElementById('scheduleItemsGrid');
        if (!grid) return;

        if (this.filteredItems.length === 0) {
            if (!grid.querySelector('.empty-filter-state')) {
                const emptyState = document.createElement('div');
                emptyState.className = 'empty-filter-state';
                emptyState.innerHTML = `
                    <div class="text-center py-5">
                        <div class="empty-state">
                            <i class="fas fa-search"></i>
                            <h4>آیتمی یافت نشد</h4>
                            <p>با فیلترهای انتخاب شده آیتمی پیدا نشد. لطفاً فیلترها را تغییر دهید.</p>
                            <button type="button" class="btn btn-modern-primary" onclick="scheduleItemsManager.clearFilters()">
                                <i class="fas fa-times me-2"></i>پاک کردن فیلترها
                            </button>
                        </div>
                    </div>
                `;
                grid.appendChild(emptyState);
            }
        } else {
            const emptyState = grid.querySelector('.empty-filter-state');
            if (emptyState) {
                emptyState.remove();
            }
        }
    }

    startScheduleItem(itemId) {
        // Find the item card
        const itemCard = document.querySelector(`[onclick*="startScheduleItem(${itemId})"]`)?.closest('.schedule-item-card');
        if (itemCard) {
            itemCard.classList.add('loading');
        }

        // Redirect to Study page
        window.location.href = `/Student/ScheduleItem/Study/${itemId}`;
    }

    showItemDetails(itemId) {
        // Find the item card
        const itemCard = document.querySelector(`[onclick*="showItemDetails(${itemId})"]`)?.closest('.schedule-item-card');
        if (itemCard) {
            itemCard.classList.add('loading');
        }

        // Show loading message
        this.showToast('در حال بارگذاری جزئیات...', 'info');

        // Simulate API call (replace with actual implementation)
        setTimeout(() => {
            if (itemCard) {
                itemCard.classList.remove('loading');
            }
            
            // For now, just show a message
            this.showToast('جزئیات آیتم بارگذاری شد', 'success');
            
            // In a real implementation, you would show a modal or redirect
            // this.showItemDetailsModal(itemId);
        }, 1000);
    }

    showToast(message, type = 'info') {
        // Create toast element
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <i class="fas ${this.getToastIcon(type)}"></i>
                <span>${message}</span>
            </div>
        `;

        // Add styles
        Object.assign(toast.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            background: this.getToastColor(type),
            color: 'white',
            padding: '12px 20px',
            borderRadius: '12px',
            boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
            zIndex: '9999',
            transform: 'translateX(100%)',
            transition: 'transform 0.3s ease',
            maxWidth: '300px',
            fontSize: '14px',
            fontWeight: '500',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
        });

        // Add to page
        document.body.appendChild(toast);

        // Animate in
        setTimeout(() => {
            toast.style.transform = 'translateX(0)';
        }, 100);

        // Remove after delay
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, 3000);
    }

    getToastIcon(type) {
        switch (type) {
            case 'success': return 'fa-check-circle';
            case 'error': return 'fa-exclamation-circle';
            case 'warning': return 'fa-exclamation-triangle';
            case 'info': return 'fa-info-circle';
            default: return 'fa-info-circle';
        }
    }

    getToastColor(type) {
        switch (type) {
            case 'success': return '#28a745';
            case 'error': return '#dc3545';
            case 'warning': return '#ffc107';
            case 'info': return '#17a2b8';
            default: return '#6c757d';
        }
    }

    // Utility method to format numbers
    formatNumber(num) {
        return new Intl.NumberFormat('fa-IR').format(num);
    }

    // Method to handle responsive behavior
    handleResize() {
        const grid = document.getElementById('scheduleItemsGrid');
        if (!grid) return;

        // Adjust grid columns based on screen size
        if (window.innerWidth < 768) {
            grid.style.gridTemplateColumns = '1fr';
        } else if (window.innerWidth < 1200) {
            grid.style.gridTemplateColumns = 'repeat(auto-fill, minmax(300px, 1fr))';
        } else {
            grid.style.gridTemplateColumns = 'repeat(auto-fill, minmax(350px, 1fr))';
        }
    }
}

// Global functions for onclick handlers
function startScheduleItem(itemId) {
    if (window.scheduleItemsManager) {
        window.scheduleItemsManager.startScheduleItem(itemId);
    }
}

function showItemDetails(itemId) {
    if (window.scheduleItemsManager) {
        window.scheduleItemsManager.showItemDetails(itemId);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.scheduleItemsManager = new ScheduleItemsManager();
    
    // Handle window resize
    window.addEventListener('resize', () => {
        if (window.scheduleItemsManager) {
            window.scheduleItemsManager.handleResize();
        }
    });
});

// Export for potential use in other scripts
window.ScheduleItemsManager = ScheduleItemsManager;
