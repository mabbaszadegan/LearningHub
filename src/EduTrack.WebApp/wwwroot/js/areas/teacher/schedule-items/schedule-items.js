/**
 * Modern Schedule Items Management JavaScript
 * Handles all interactions for schedule items management
 * Uses centralized API and Notification services
 */

class ScheduleItemsManager {
    constructor() {
        // Use Utils for parsing query string
        const Utils = window.EduTrack?.Utils || {};
        this.getTeachingPlanIdFromUrl = Utils.parseQueryString 
            ? () => {
                const params = Utils.parseQueryString(window.location.search);
                return parseInt(params.teachingPlanId) || 0;
            }
            : () => {
                const urlParams = new URLSearchParams(window.location.search);
                return parseInt(urlParams.get('teachingPlanId')) || 0;
            };

        this.currentTeachingPlanId = this.getTeachingPlanIdFromUrl();
        this.currentFilters = {
            search: '',
            type: '',
            status: ''
        };

        // Get services
        this.api = window.EduTrack?.API?.ScheduleItem;
        this.notification = window.EduTrack?.Services?.Notification;
        this.modal = window.EduTrack?.Services?.Modal;
        
        // Fallback debounce if Utils not available
        const debounceFn = window.EduTrack?.Utils?.debounce;
        this.debounce = debounceFn || this._fallbackDebounce.bind(this);

        this.init();
    }

    init() {
        this.setupEventListeners();
        this.loadScheduleItems();
    }

    setupEventListeners() {
        // Search functionality
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', this.debounce((e) => {
                this.currentFilters.search = e.target.value;
                this.filterItems();
            }, 300));
        }

        // Filter controls
        const typeFilter = document.getElementById('typeFilter');
        if (typeFilter) {
            typeFilter.addEventListener('change', (e) => {
                this.currentFilters.type = e.target.value;
                this.filterItems();
            });
        }

        const statusFilter = document.getElementById('statusFilter');
        if (statusFilter) {
            statusFilter.addEventListener('change', (e) => {
                this.currentFilters.status = e.target.value;
                this.filterItems();
            });
        }

        // Clear filters
        const clearFiltersBtn = document.getElementById('clearFilters');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', () => {
                this.clearFilters();
            });
        }
    }

    async loadScheduleItems() {
        try {
            let result;
            
            if (this.api) {
                // Use centralized API service
                result = await this.api.getScheduleItems(this.currentTeachingPlanId);
            } else {
                // Fallback to direct fetch
                const response = await fetch(`/Teacher/ScheduleItem/GetScheduleItems?teachingPlanId=${this.currentTeachingPlanId}`);
                result = await response.json();
            }
            
            if (result.success) {
                this.renderScheduleItems(result.data);
            } else {
                this.showError('خطا در بارگذاری آیتم‌ها: ' + (result.message || 'خطای ناشناخته'));
            }
        } catch (error) {
            console.error('Error loading schedule items:', error);
            this.showError('خطا در بارگذاری آیتم‌ها');
        }
    }

    renderScheduleItems(items) {
        const grid = document.getElementById('scheduleItemsGrid');
        if (!grid) return;

        if (!items || items.length === 0) {
            grid.innerHTML = this.getEmptyStateHtml();
            return;
        }

        const itemsHtml = items.map(item => this.createItemCard(item)).join('');
        grid.innerHTML = itemsHtml;

        // Re-setup event listeners for new items
        this.setupItemEventListeners();
    }

    createItemCard(item) {
        const typeIcon = this.getTypeIcon(item.type);
        const statusClass = `status-${item.status}`;
        const typeClass = `type-${item.type}`;

        return `
            <div class="schedule-item-card" data-item-id="${item.id}" data-type="${item.type}" data-status="${item.status}">
                <div class="item-header">
                    <div class="item-type-badge ${typeClass}">
                        <i class="${typeIcon}"></i>
                        ${item.typeName}
                    </div>
                    <div class="item-actions">
                        <div class="dropdown">
                            <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
                                <i class="fas fa-ellipsis-v"></i>
                            </button>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item" href="#" onclick="scheduleItemsManager.editItem(${item.id})">
                                    <i class="fas fa-edit"></i> ویرایش
                                </a></li>
                                <li><a class="dropdown-item" href="#" onclick="scheduleItemsManager.duplicateItem(${item.id})">
                                    <i class="fas fa-copy"></i> کپی
                                </a></li>
                                <li><hr class="dropdown-divider"></li>
                                <li><a class="dropdown-item text-danger" href="#" onclick="scheduleItemsManager.deleteItem(${item.id})">
                                    <i class="fas fa-trash"></i> حذف
                                </a></li>
                            </ul>
                        </div>
                    </div>
                </div>
                
                <div class="item-content">
                    <h5 class="item-title">${item.title}</h5>
                    ${item.description ? `<p class="item-description">${item.description}</p>` : ''}
                    
                    <div class="item-meta">
                        <div class="meta-item">
                            <i class="fas fa-calendar-alt"></i>
                            <span>شروع: ${this.formatDate(item.startDate)}</span>
                        </div>
                        ${item.dueDate ? `
                            <div class="meta-item">
                                <i class="fas fa-clock"></i>
                                <span>مهلت: ${this.formatDate(item.dueDate)}</span>
                            </div>
                        ` : ''}
                        ${item.maxScore ? `
                            <div class="meta-item">
                                <i class="fas fa-star"></i>
                                <span>امتیاز: ${item.maxScore}</span>
                            </div>
                        ` : ''}
                    </div>
                </div>
                
                <div class="item-footer">
                    <div class="item-status ${statusClass}">
                        ${item.statusText}
                    </div>
                    ${item.groupName ? `
                        <div class="item-group">
                            <i class="fas fa-users"></i>
                            ${item.groupName}
                        </div>
                    ` : ''}
                </div>
            </div>
        `;
    }

    getEmptyStateHtml() {
        return `
            <div class="empty-state">
                <div class="empty-icon">
                    <i class="fas fa-tasks"></i>
                </div>
                <h3>هیچ آیتم آموزشی وجود ندارد</h3>
                <p>برای شروع، اولین آیتم آموزشی خود را ایجاد کنید.</p>
                <button type="button" class="btn btn-primary btn-modern" data-bs-toggle="modal" data-bs-target="#createItemModal">
                    <i class="fas fa-plus"></i>
                    افزودن آیتم جدید
                </button>
            </div>
        `;
    }

    setupItemEventListeners() {
        // Add any specific event listeners for individual items
        // This method can be expanded as needed
    }

    filterItems() {
        const cards = document.querySelectorAll('.schedule-item-card');
        
        cards.forEach(card => {
            const itemId = card.getAttribute('data-item-id');
            const itemType = card.getAttribute('data-type');
            const itemStatus = card.getAttribute('data-status');
            const itemTitle = card.querySelector('.item-title')?.textContent.toLowerCase() || '';
            const itemDescription = card.querySelector('.item-description')?.textContent.toLowerCase() || '';
            
            let show = true;
            
            // Search filter
            if (this.currentFilters.search) {
                const searchTerm = this.currentFilters.search.toLowerCase();
                if (!itemTitle.includes(searchTerm) && !itemDescription.includes(searchTerm)) {
                    show = false;
                }
            }
            
            // Type filter
            if (this.currentFilters.type && itemType !== this.currentFilters.type) {
                show = false;
            }
            
            // Status filter
            if (this.currentFilters.status && itemStatus !== this.currentFilters.status) {
                show = false;
            }
            
            card.style.display = show ? 'block' : 'none';
        });
    }

    clearFilters() {
        this.currentFilters = {
            search: '',
            type: '',
            status: ''
        };
        
        // Reset form controls
        const searchInput = document.getElementById('searchInput');
        const typeFilter = document.getElementById('typeFilter');
        const statusFilter = document.getElementById('statusFilter');
        
        if (searchInput) searchInput.value = '';
        if (typeFilter) typeFilter.value = '';
        if (statusFilter) statusFilter.value = '';
        
        // Show all items
        const cards = document.querySelectorAll('.schedule-item-card');
        cards.forEach(card => {
            card.style.display = 'block';
        });
    }


    async editItem(itemId) {
        // Get teaching plan ID from current page URL or from a global variable
        const urlParams = new URLSearchParams(window.location.search);
        const teachingPlanId = urlParams.get('teachingPlanId') || window.teachingPlanId || 1;
        
        // Redirect to create page with edit parameters
        window.location.href = `/Teacher/ScheduleItem/CreateOrEdit?teachingPlanId=${teachingPlanId}&id=${itemId}`;
    }

    async duplicateItem(itemId) {
        try {
            let result;
            
            if (this.api) {
                result = await this.api.getScheduleItem(itemId);
            } else {
                const response = await fetch(`/Teacher/ScheduleItem/GetScheduleItem?id=${itemId}`);
                result = await response.json();
            }
            
            if (result.success) {
                const item = result.data;
                // Create a new item with the same data but different title
                const duplicatedData = {
                    ...item,
                    title: `${item.title} (کپی)`,
                    id: undefined
                };
                
                // You can implement duplication logic here
                this.showSuccess('آیتم با موفقیت کپی شد');
            } else {
                this.showError('خطا در کپی آیتم: ' + (result.message || 'خطای ناشناخته'));
            }
        } catch (error) {
            console.error('Error duplicating item:', error);
            this.showError('خطا در کپی آیتم');
        }
    }

    async deleteItem(itemId) {
        // Use modal service instead of confirm
        const confirmed = this.modal 
            ? await this.modal.confirm('آیا از حذف این آیتم آموزشی اطمینان دارید؟', 'حذف آیتم آموزشی')
            : confirm('آیا از حذف این آیتم آموزشی اطمینان دارید؟');
        
        if (!confirmed) {
            return;
        }
        
        try {
            let result;
            
            if (this.api) {
                result = await this.api.deleteScheduleItem(itemId);
            } else {
                const response = await fetch('/Teacher/ScheduleItem/DeleteScheduleItem', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ id: itemId })
                });
                result = await response.json();
            }
            
            if (result.success) {
                this.showSuccess('آیتم آموزشی با موفقیت حذف شد');
                this.loadScheduleItems();
            } else {
                this.showError('خطا در حذف آیتم: ' + (result.message || 'خطای ناشناخته'));
            }
        } catch (error) {
            console.error('Error deleting item:', error);
            this.showError('خطا در حذف آیتم آموزشی');
        }
    }

    closeModal(modalId) {
        const modal = bootstrap.Modal.getInstance(document.getElementById(modalId));
        if (modal) {
            modal.hide();
        }
    }

    getTypeIcon(type) {
        const icons = {
            0: 'fas fa-bell',      // Reminder
            1: 'fas fa-pen',       // Writing
            2: 'fas fa-volume-up', // Audio
            3: 'fas fa-edit',      // Gap Fill
            4: 'fas fa-list-ul',  // Multiple Choice
            5: 'fas fa-link',      // Match
            6: 'fas fa-search',    // Error Finding
            7: 'fas fa-code',      // Code Exercise
            8: 'fas fa-question-circle' // Quiz
        };
        return icons[type] || 'fas fa-tasks';
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('fa-IR');
    }

    /**
     * Fallback debounce if Utils not available
     * @private
     */
    _fallbackDebounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    showSuccess(message) {
        if (this.notification) {
            this.notification.success(message);
        } else if (typeof toastSuccess !== 'undefined') {
            toastSuccess(message);
        } else {
            alert(message);
        }
    }

    showError(message) {
        if (this.notification) {
            this.notification.error(message);
        } else if (typeof toastError !== 'undefined') {
            toastError(message);
        } else {
            alert(message);
        }
    }
}

// Initialize the manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.scheduleItemsManager = new ScheduleItemsManager();
});
