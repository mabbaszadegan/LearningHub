/**
 * Unified Course Content JavaScript
 * Handles interactions, filtering, and navigation for unified course content view
 */

let currentScheduleFilter = 'all';
let allScheduleItems = [];
let filteredScheduleItems = [];

$(document).ready(function() {
    // Initialize schedule items filter if items exist
    if (typeof scheduleItems !== 'undefined' && scheduleItems.length > 0) {
        allScheduleItems = scheduleItems;
        filteredScheduleItems = scheduleItems;
        initializeScheduleItemsFilter();
    }
    
    // Handle tab switching
    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function(e) {
        const targetTab = $(e.target).data('bs-target');
        if (targetTab === '#schedule-items-content') {
            // Refresh schedule items when switching to schedule items tab
            updateScheduleItemsFilter();
        }
    });
    
    // Handle refresh button
    $('#refresh-schedule-items').on('click', function() {
        loadScheduleItems();
    });
    
    // Smooth scroll for chapter navigation
    $('.chapter-header').on('click', function() {
        const chapterCard = $(this).closest('.chapter-card');
        if (chapterCard.length) {
            setTimeout(() => {
                chapterCard[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            }, 300);
        }
    });
});

/**
 * Initialize schedule items filter badges
 */
function initializeScheduleItemsFilter() {
    if (!allScheduleItems || allScheduleItems.length === 0) {
        return;
    }
    
    // Group items by type
    const typeGroups = {};
    allScheduleItems.forEach(function(item) {
        const type = item.type || item.Type || 'Unknown';
        if (!typeGroups[type]) {
            typeGroups[type] = 0;
        }
        typeGroups[type]++;
    });
    
    // Generate filter badges
    const badgesContainer = $('#schedule-items-type-badges');
    let badgesHtml = '';
    
    // Always show "All" filter
    badgesHtml += `
        <button class="content-type-badge-compact ${currentScheduleFilter === 'all' ? 'active' : ''}" 
                data-type="all" 
                title="همه">
            <i class="fas fa-th-large"></i>
        </button>
    `;
    
    // Add badges for each type
    Object.keys(typeGroups).sort().forEach(function(type) {
        const count = typeGroups[type];
        const typeIcon = getTypeIcon(type);
        const typeTitle = getTypeTitle(type);
        
        badgesHtml += `
            <button class="content-type-badge-compact ${currentScheduleFilter === type ? 'active' : ''}" 
                    data-type="${type}" 
                    title="${typeTitle} (${count})">
                <i class="${typeIcon}"></i>
            </button>
        `;
    });
    
    badgesContainer.html(badgesHtml);
    
    // Bind click events
    $('.content-type-badge-compact').off('click').on('click', function() {
        $('.content-type-badge-compact').removeClass('active');
        $(this).addClass('active');
        
        currentScheduleFilter = $(this).data('type');
        updateScheduleItemsFilter();
    });
    
    // Update count
    $('#schedule-items-count').text(allScheduleItems.length);
}

/**
 * Update schedule items filter
 */
function updateScheduleItemsFilter() {
    if (!allScheduleItems || allScheduleItems.length === 0) {
        return;
    }
    
    // Filter items
    if (currentScheduleFilter === 'all') {
        filteredScheduleItems = allScheduleItems;
    } else {
        filteredScheduleItems = allScheduleItems.filter(function(item) {
            return (item.type || item.Type || '') === currentScheduleFilter;
        });
    }
    
    // Update display
    displayFilteredScheduleItems();
}

/**
 * Display filtered schedule items
 */
function displayFilteredScheduleItems() {
    const container = $('#schedule-items-list');
    
    if (filteredScheduleItems.length === 0) {
        container.html(`
            <div class="empty-state">
                <i class="fas fa-filter fa-3x"></i>
                <h4>آیتمی یافت نشد</h4>
                <p>هیچ آیتمی با فیلتر انتخابی یافت نشد</p>
            </div>
        `);
        return;
    }
    
    let html = '';
    filteredScheduleItems.forEach(function(item) {
        const statusClass = getItemStatusClass(item);
        const statusIcon = getItemStatusIcon(item);
        const typeIcon = getTypeIcon(item.type || item.Type || '');
        const dueDateText = item.dueDate ? formatDate(item.dueDate) : '';
        
        html += generateScheduleItemHTML(item, statusClass, statusIcon, typeIcon, dueDateText);
    });
    
    container.html(html);
    
    // Update count
    $('#schedule-items-count').text(filteredScheduleItems.length);
}

/**
 * Generate HTML for a schedule item
 */
function generateScheduleItemHTML(item, statusClass, statusIcon, typeIcon, dueDateText) {
    const isCompleted = item.status === 'Completed' || item.status === '3';
    const isUpcoming = item.isUpcoming || false;
    
    let actionButton = '';
    if (isCompleted) {
        actionButton = `
            <button class="btn-action-unified success" disabled>
                <i class="fas fa-check"></i>
                <span>تکمیل شده</span>
            </button>
        `;
    } else if (isUpcoming) {
        actionButton = `
            <button class="btn-action-unified warning" disabled>
                <i class="fas fa-clock"></i>
                <span>شروع در آینده</span>
            </button>
        `;
    } else {
        actionButton = `
            <button class="btn-action-unified primary" onclick="openScheduleItem(${item.id})">
                <i class="fas fa-play"></i>
                <span>شروع</span>
            </button>
        `;
    }
    
    return `
        <div class="schedule-item-card-unified" data-type="${item.type || item.Type || ''}" data-status="${statusClass}">
            <div class="schedule-item-icon-unified">
                <i class="${typeIcon}"></i>
            </div>
            <div class="schedule-item-content-unified">
                <div class="schedule-item-header-unified">
                    <h4 class="schedule-item-title-unified">${escapeHtml(item.title || '')}</h4>
                    <span class="status-badge-unified status-${statusClass}">
                        <i class="${statusIcon}"></i>
                        ${escapeHtml(item.statusText || 'فعال')}
                    </span>
                </div>
                ${item.description ? `
                    <p class="schedule-item-description-unified">${escapeHtml(item.description)}</p>
                ` : ''}
                <div class="schedule-item-meta-unified">
                    <div class="meta-item-unified">
                        <i class="fas fa-calendar-alt"></i>
                        <span>شروع: ${formatDate(item.startDate || item.StartDate)}</span>
                    </div>
                    ${dueDateText ? `
                        <div class="meta-item-unified">
                            <i class="fas fa-clock"></i>
                            <span>مهلت: ${dueDateText}</span>
                        </div>
                    ` : ''}
                    ${item.maxScore ? `
                        <div class="meta-item-unified">
                            <i class="fas fa-star"></i>
                            <span>حداکثر نمره: ${item.maxScore}</span>
                        </div>
                    ` : ''}
                    ${item.isMandatory ? `
                        <div class="meta-item-unified mandatory">
                            <i class="fas fa-exclamation-circle"></i>
                            <span>اجباری</span>
                        </div>
                    ` : ''}
                </div>
            </div>
            <div class="schedule-item-actions-unified">
                ${actionButton}
                <button class="btn-action-unified secondary" onclick="viewScheduleItemDetails(${item.id})" title="جزئیات">
                    <i class="fas fa-info-circle"></i>
                </button>
            </div>
        </div>
    `;
}

/**
 * Get item status class
 */
function getItemStatusClass(item) {
    const status = item.status || item.Status || '';
    const statusText = item.statusText || item.StatusText || '';
    
    if (status === 'Completed' || status === '3' || statusText.includes('تکمیل')) {
        return 'completed';
    }
    if (item.isOverdue || statusText.includes('تأخیر')) {
        return 'overdue';
    }
    if (item.isUpcoming || statusText.includes('آینده')) {
        return 'upcoming';
    }
    return 'active';
}

/**
 * Get item status icon
 */
function getItemStatusIcon(item) {
    const statusClass = getItemStatusClass(item);
    
    switch (statusClass) {
        case 'completed':
            return 'fas fa-check-circle';
        case 'overdue':
            return 'fas fa-exclamation-triangle';
        case 'upcoming':
            return 'fas fa-clock';
        default:
            return 'fas fa-play-circle';
    }
}

/**
 * Get type icon
 */
function getTypeIcon(type) {
    const typeStr = type.toString();
    
    switch (typeStr) {
        case '0':
        case 'Reminder':
            return 'fas fa-bell';
        case '1':
        case 'Writing':
            return 'fas fa-pen';
        case '2':
        case 'Audio':
            return 'fas fa-microphone';
        case '3':
        case 'GapFill':
            return 'fas fa-edit';
        case '4':
        case 'MultipleChoice':
            return 'fas fa-list-ul';
        case '5':
        case 'Match':
            return 'fas fa-link';
        case '6':
        case 'ErrorFinding':
            return 'fas fa-search';
        case '7':
        case 'CodeExercise':
            return 'fas fa-code';
        case '8':
        case 'Quiz':
            return 'fas fa-question-circle';
        default:
            return 'fas fa-file';
    }
}

/**
 * Get type title
 */
function getTypeTitle(type) {
    const typeStr = type.toString();
    
    switch (typeStr) {
        case '0':
        case 'Reminder':
            return 'یادآوری';
        case '1':
        case 'Writing':
            return 'نوشتاری';
        case '2':
        case 'Audio':
            return 'صوتی';
        case '3':
        case 'GapFill':
            return 'جای خالی';
        case '4':
        case 'MultipleChoice':
            return 'چند گزینه‌ای';
        case '5':
        case 'Match':
            return 'تطبیق';
        case '6':
        case 'ErrorFinding':
            return 'پیدا کردن خطا';
        case '7':
        case 'CodeExercise':
            return 'کدنویسی';
        case '8':
        case 'Quiz':
            return 'کویز';
        default:
            return 'فایل';
    }
}

/**
 * Format date
 */
function formatDate(date) {
    if (!date) return '';
    
    try {
        const dateObj = new Date(date);
        return dateObj.toLocaleDateString('fa-IR');
    } catch (e) {
        return date;
    }
}

/**
 * Escape HTML
 */
function escapeHtml(text) {
    if (!text) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.toString().replace(/[&<>"']/g, m => map[m]);
}

/**
 * Open schedule item
 */
function openScheduleItem(itemId) {
    if (!itemId) {
        showToast('خطا: شناسه آیتم نامعتبر است', 'error');
        return;
    }
    
    const studyUrl = `/Student/ScheduleItem/Study/${itemId}`;
    window.location.href = studyUrl;
}

/**
 * View schedule item details
 */
function viewScheduleItemDetails(itemId) {
    // Placeholder for viewing schedule item details
    // You can implement a modal or navigate to details page
    showToast('نمایش جزئیات آیتم...', 'info');
}

/**
 * Load schedule items from server
 */
function loadScheduleItems() {
    if (!window.studyPageConfig || !window.studyPageConfig.getScheduleItemsUrl) {
        return;
    }
    
    const container = $('#schedule-items-list');
    container.html(`
        <div class="loading-placeholder">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">در حال بارگذاری...</span>
            </div>
            <p class="loading-text">در حال بارگذاری آیتم‌های آموزشی...</p>
        </div>
    `);
    
    $.ajax({
        url: window.studyPageConfig.getScheduleItemsUrl,
        type: 'GET',
        data: {
            filter: currentScheduleFilter
        },
        success: function(response) {
            if (response.success) {
                allScheduleItems = response.data || [];
                filteredScheduleItems = allScheduleItems;
                
                // Update filter badges
                initializeScheduleItemsFilter();
                
                // Display items
                displayFilteredScheduleItems();
            } else {
                container.html(`
                    <div class="empty-state">
                        <i class="fas fa-exclamation-triangle fa-3x"></i>
                        <h4>خطا در بارگذاری</h4>
                        <p>${response.error || 'خطا در بارگذاری آیتم‌های آموزشی'}</p>
                    </div>
                `);
            }
        },
        error: function() {
            container.html(`
                <div class="empty-state">
                    <i class="fas fa-exclamation-triangle fa-3x"></i>
                    <h4>خطا در بارگذاری</h4>
                    <p>خطا در ارتباط با سرور</p>
                </div>
            `);
        }
    });
}

/**
 * Show toast notification
 */
function showToast(message, type) {
    const toastClass = type === 'error' ? 'alert-danger' : type === 'success' ? 'alert-success' : 'alert-info';
    const toastHtml = `
        <div class="alert ${toastClass} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;" role="alert">
            ${escapeHtml(message)}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    $('body').append(toastHtml);
    
    // Auto remove after 3 seconds
    setTimeout(function() {
        $('.alert').fadeOut(function() {
            $(this).remove();
        });
    }, 3000);
}
