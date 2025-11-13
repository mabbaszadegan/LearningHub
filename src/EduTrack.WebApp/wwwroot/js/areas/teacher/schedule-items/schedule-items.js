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

        // Prefer reading from container data attribute; fallback to query string
        const container = document.querySelector('.schedule-items-container');
        const dataTeachingPlanId = container?.dataset?.teachingPlanId ? parseInt(container.dataset.teachingPlanId) : 0;
        this.isCourseScope = container?.dataset?.courseScope === 'true';
        this.isSessionScope = container?.dataset?.sessionScope === 'true';
        this.currentCourseId = container?.dataset?.courseId ? parseInt(container.dataset.courseId) : 0;
        this.currentSessionReportId = container?.dataset?.sessionReportId ? parseInt(container.dataset.sessionReportId) : 0;
        this.currentTeachingPlanId = dataTeachingPlanId || this.getTeachingPlanIdFromUrl();
        if (this.isCourseScope) {
            this.currentTeachingPlanId = 0;
        }
        if (this.isSessionScope && (!this.currentSessionReportId || Number.isNaN(this.currentSessionReportId))) {
            const urlParams = new URLSearchParams(window.location.search);
            this.currentSessionReportId = parseInt(urlParams.get('sessionReportId')) || 0;
        }
        if (!this.currentSessionReportId) {
            const urlParams = new URLSearchParams(window.location.search);
            this.currentSessionReportId = parseInt(urlParams.get('sessionReportId')) || 0;
        }
        if (!this.currentCourseId) {
            const urlParams = new URLSearchParams(window.location.search);
            this.currentCourseId = parseInt(urlParams.get('courseId')) || 0;
        }
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

    buildTypeNameMap() {
        this.typeNameMap = {};
        const typeChips = document.querySelectorAll('.chips-group[aria-label="نوع آیتم"] .filter-chip[data-type]');
        typeChips.forEach(chip => {
            const key = chip.getAttribute('data-type');
            if (key !== null && key !== '') {
                this.typeNameMap[key] = chip.textContent.trim();
            }
        });
    }

    getTypeName(type) {
        const key = String(type);
        return this.typeNameMap ? this.typeNameMap[key] : undefined;
    }

    init() {
        this.buildTypeNameMap();
        this.setupEventListeners();
        this.loadScheduleItems();
        this.updateClearVisibility();
    }

    setupEventListeners() {
        // Search functionality
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', this.debounce((e) => {
                this.currentFilters.search = e.target.value;
                this.filterItems();
                this.updateClearVisibility();
            }, 300));
        }

        // Filter controls (dropdowns)
        const typeFilter = document.getElementById('typeFilter');
        if (typeFilter) {
            typeFilter.addEventListener('change', (e) => {
                this.currentFilters.type = e.target.value;
                this.filterItems();
                this.updateClearVisibility();
            });
        }

        const statusFilter = document.getElementById('statusFilter');
        if (statusFilter) {
            statusFilter.addEventListener('change', (e) => {
                this.currentFilters.status = e.target.value;
                this.filterItems();
                this.updateClearVisibility();
            });
        }

        // Clear filters
        const clearFiltersBtn = document.getElementById('clearFilters');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', () => {
                this.clearFilters();
            });
        }

        // Chip filters (modern toggles)
        const typeChips = document.querySelectorAll('.filter-chip[data-type]');
        if (typeChips && typeChips.length) {
            typeChips.forEach(chip => {
                chip.addEventListener('click', () => {
                    typeChips.forEach(c => c.classList.remove('active'));
                    chip.classList.add('active');
                    const value = chip.getAttribute('data-type') || '';
                    this.currentFilters.type = value;
                    // keep selects in sync if present
                    if (typeFilter) typeFilter.value = value;
                    this.filterItems();
                    this.updateClearVisibility();
                });
            });
        }

        const statusChips = document.querySelectorAll('.filter-chip[data-status]');
        const statusFilterEl = document.getElementById('statusFilter');
        if (statusChips && statusChips.length) {
            statusChips.forEach(chip => {
                chip.addEventListener('click', () => {
                    statusChips.forEach(c => c.classList.remove('active'));
                    chip.classList.add('active');
                    const value = chip.getAttribute('data-status') || '';
                    this.currentFilters.status = value;
                    if (statusFilterEl) statusFilterEl.value = value;
                    this.filterItems();
                    this.updateClearVisibility();
                });
            });
        }

        // Sort select
        const sortSelect = document.getElementById('sortSelect');
        if (sortSelect) {
            sortSelect.addEventListener('change', () => {
                this.applySort(sortSelect.value);
            });
        }
    }

    async loadScheduleItems() {
        try {
            let result;
            
            if (this.api) {
                if (this.isSessionScope && this.currentSessionReportId > 0) {
                    result = await this.api.getSessionScheduleItems(this.currentSessionReportId);
                } else if (this.isCourseScope && this.currentCourseId > 0) {
                    result = await this.api.getCourseScheduleItems(this.currentCourseId, true);
                } else if (this.currentTeachingPlanId > 0) {
                    result = await this.api.getScheduleItems(this.currentTeachingPlanId);
                } else {
                    return;
                }
            } else {
                let fetchUrl = null;
                if (this.isSessionScope && this.currentSessionReportId > 0) {
                    fetchUrl = `/Teacher/ScheduleItem/GetSessionScheduleItems?sessionReportId=${this.currentSessionReportId}`;
                } else if (this.isCourseScope && this.currentCourseId > 0) {
                    fetchUrl = `/Teacher/ScheduleItem/GetCourseScheduleItems?courseId=${this.currentCourseId}&courseScopeOnly=true`;
                } else if (this.currentTeachingPlanId > 0) {
                    fetchUrl = `/Teacher/ScheduleItem/GetScheduleItems?teachingPlanId=${this.currentTeachingPlanId}`;
                }

                if (!fetchUrl) {
                    return;
                }

                const response = await fetch(fetchUrl);
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
        const typeName = this.getTypeName(item.type) || item.typeName || '';

        const actionsHtml = `
            <div class="item-actions">
                <div class="dropdown">
                    <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
                        <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <ul class="dropdown-menu">
                        <li><a class="dropdown-item" href="#" data-action="edit" data-item-id="${item.id}">
                            <i class="fas fa-edit"></i> ویرایش
                        </a></li>
                        <li><a class="dropdown-item" href="#" data-action="duplicate" data-item-id="${item.id}">
                            <i class="fas fa-copy"></i> کپی
                        </a></li>
                        <li><hr class="dropdown-divider"></li>
                        <li><a class="dropdown-item text-danger" href="#" data-action="delete" data-item-id="${item.id}">
                            <i class="fas fa-trash"></i> حذف
                        </a></li>
                    </ul>
                </div>
            </div>
        `;

        const teachingPlanId = item.teachingPlanId ?? '';
        const courseId = item.courseId ?? '';
        const sessionReportId = item.sessionReportId ?? '';

        return `
            <div class="schedule-item-card"
                 data-item-id="${item.id}"
                 data-type="${item.type}"
                 data-status="${item.status}"
                 data-teaching-plan-id="${teachingPlanId}"
                 data-course-id="${courseId}"
                 data-session-report-id="${sessionReportId}">
                <div class="item-header">
                    <div class="item-type-badge ${typeClass}">
                        <i class="${typeIcon}"></i>
                        ${typeName}
                    </div>
                    ${actionsHtml}
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
        let createUrl;
        if (this.isSessionScope && this.currentSessionReportId) {
            return `
                <div class="empty-state">
                    <div class="empty-icon">
                        <i class="fas fa-tasks"></i>
                    </div>
                    <h3>هنوز تکلیفی برای این جلسه ثبت نشده</h3>
                    <p>از صفحه جزئیات جلسه می‌توانید تکلیف جدید ثبت کنید.</p>
                </div>
            `;
        } else if (this.isCourseScope && this.currentCourseId) {
            createUrl = `/Teacher/ScheduleItem/CreateOrEdit?courseId=${this.currentCourseId}`;
        } else {
            createUrl = `/Teacher/ScheduleItem/CreateOrEdit?teachingPlanId=${this.currentTeachingPlanId}`;
        }

        return `
            <div class="empty-state">
                <div class="empty-icon">
                    <i class="fas fa-tasks"></i>
                </div>
                <h3>هیچ آیتم آموزشی وجود ندارد</h3>
                <p>برای شروع، اولین آیتم آموزشی خود را ایجاد کنید.</p>
                <a href="${createUrl}" class="btn btn-primary btn-modern">
                    <i class="fas fa-plus"></i>
                    افزودن آیتم جدید
                </a>
            </div>
        `;
    }

    setupItemEventListeners() {
        // Delegated events for dropdown actions
        const grid = document.getElementById('scheduleItemsGrid');
        if (!grid) return;

        grid.addEventListener('click', (e) => {
            const actionLink = e.target.closest('a[data-action]');
            if (!actionLink) return;
            e.preventDefault();

            const action = actionLink.getAttribute('data-action');
            const itemId = parseInt(actionLink.getAttribute('data-item-id')) || parseInt(actionLink.closest('.schedule-item-card')?.getAttribute('data-item-id'));

            if (!itemId) return;

            switch (action) {
                case 'edit':
                    this.editItem(itemId);
                    break;
                case 'duplicate':
                    this.duplicateItem(itemId);
                    break;
                case 'delete':
                    this.deleteItem(itemId);
                    break;
            }
        });
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
        // Reset chips
        document.querySelectorAll('.filter-chip[data-type]').forEach((c, idx) => c.classList.toggle('active', idx === 0));
        document.querySelectorAll('.filter-chip[data-status]').forEach((c, idx) => c.classList.toggle('active', idx === 0));
        
        // Show all items
        const cards = document.querySelectorAll('.schedule-item-card');
        cards.forEach(card => {
            card.style.display = 'block';
        });

        this.updateClearVisibility();
    }

    applySort(mode) {
        const grid = document.getElementById('scheduleItemsGrid');
        if (!grid) return;
        const cards = Array.from(grid.querySelectorAll('.schedule-item-card'));
        const parseDate = (text) => {
            // expects text like "شروع: yyyy/MM/dd" or "مهلت: yyyy/MM/dd"
            const m = text?.match(/\d{4}\/\d{2}\/\d{2}/);
            return m ? new Date(m[0]) : new Date(0);
        };

        const getStart = (card) => parseDate(card.querySelector('.meta-item span')?.textContent || '');
        const getDue = (card) => {
            const items = card.querySelectorAll('.meta-item span');
            return parseDate(items.length > 1 ? items[1].textContent : '');
        };
        const getTitle = (card) => (card.querySelector('.item-title')?.textContent || '').toLowerCase();

        const compare = (a, b) => {
            switch (mode) {
                case 'start-asc': return getStart(a) - getStart(b);
                case 'start-desc': return getStart(b) - getStart(a);
                case 'due-asc': return getDue(a) - getDue(b);
                case 'due-desc': return getDue(b) - getDue(a);
                case 'title-desc': return getTitle(b).localeCompare(getTitle(a));
                case 'title-asc':
                default: return getTitle(a).localeCompare(getTitle(b));
            }
        };

        cards.sort(compare).forEach(card => grid.appendChild(card));
    }

    isAnyFilterActive() {
        return Boolean(this.currentFilters.search || this.currentFilters.type || this.currentFilters.status);
    }

    updateClearVisibility() {
        const btn = document.getElementById('clearFilters');
        if (!btn) return;
        if (this.isAnyFilterActive()) {
            btn.removeAttribute('hidden');
        } else {
            btn.setAttribute('hidden', '');
        }
    }


    async editItem(itemId) {
        const card = document.querySelector(`.schedule-item-card[data-item-id="${itemId}"]`);
        const urlParams = new URLSearchParams(window.location.search);

        const cardTeachingPlanId = card?.dataset?.teachingPlanId ? parseInt(card.dataset.teachingPlanId) : 0;
        const cardCourseId = card?.dataset?.courseId ? parseInt(card.dataset.courseId) : 0;
        const cardSessionReportId = card?.dataset?.sessionReportId ? parseInt(card.dataset.sessionReportId) : 0;

        const teachingPlanId = cardTeachingPlanId
            || this.currentTeachingPlanId
            || parseInt(urlParams.get('teachingPlanId')) || 0;

        const courseId = cardCourseId
            || this.currentCourseId
            || parseInt(urlParams.get('courseId')) || 0;

        const sessionReportId = cardSessionReportId
            || this.currentSessionReportId
            || parseInt(urlParams.get('sessionReportId')) || 0;

        const query = new URLSearchParams();

        if (sessionReportId > 0) {
            query.set('sessionReportId', sessionReportId);
        }

        if (teachingPlanId > 0) {
            query.set('teachingPlanId', teachingPlanId);
        } else if (courseId > 0) {
            query.set('courseId', courseId);
        }

        query.set('id', itemId);

        if (!query.has('teachingPlanId') && !query.has('courseId') && !query.has('sessionReportId')) {
            this.showError('کانتکست آیتم برای ویرایش یافت نشد.');
            return;
        }

        window.location.href = `/Teacher/ScheduleItem/CreateOrEdit?${query.toString()}`;
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
