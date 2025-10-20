/**
 * Study Page JavaScript
 * Handles schedule items loading, filtering, and display
 */

let currentFilter = 'all';
let isLoading = false;
let allScheduleItems = [];
let availableTypes = [];

$(document).ready(function() {
    // Load initial schedule items
    loadScheduleItems();
    
    // Handle refresh button
    $('#refresh-content').on('click', function() {
        loadScheduleItems();
    });

    // Update last accessed time
    updateLastAccessed();
});

function loadScheduleItems() {
    isLoading = true;
    $('#schedule-items-list').html(`
        <div class="loading-placeholder">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">در حال بارگذاری...</span>
            </div>
            <p class="loading-text">در حال بارگذاری محتوای آموزشی...</p>
        </div>
    `);

    $.ajax({
        url: window.studyPageConfig.getScheduleItemsUrl,
        type: 'GET',
        data: {
            filter: currentFilter
        },
        success: function(response) {
            if (response.success) {
                allScheduleItems = response.data;
                availableTypes = response.availableTypes;
                
                // Update filter badges based on available types
                updateFilterBadges();
                
                // Display schedule items
                displayScheduleItems(allScheduleItems);
            } else {
                $('#schedule-items-list').html(`
                    <div class="error-placeholder">
                        <i class="fas fa-exclamation-triangle fa-2x text-warning mb-3"></i>
                        <p class="text-muted">${response.error || 'خطا در بارگذاری محتوای آموزشی'}</p>
                        <button class="btn btn-modern-primary" onclick="loadScheduleItems()">
                            <i class="fas fa-redo me-1"></i>تلاش مجدد
                        </button>
                    </div>
                `);
            }
        },
        error: function() {
            $('#schedule-items-list').html(`
                <div class="error-placeholder">
                    <i class="fas fa-exclamation-triangle fa-2x text-warning mb-3"></i>
                    <p class="text-muted">خطا در بارگذاری محتوای آموزشی</p>
                    <button class="btn btn-modern-primary" onclick="loadScheduleItems()">
                        <i class="fas fa-redo me-1"></i>تلاش مجدد
                    </div>
                `);
        },
        complete: function() {
            isLoading = false;
        }
    });
}

function updateFilterBadges() {
    var badgesContainer = $('#content-type-badges');
    var badgesHtml = '';
    
    // Always show "All" filter
    badgesHtml += `
        <button class="content-type-badge-compact ${currentFilter === 'all' ? 'active' : ''}" data-type="all" title="همه">
            <i class="fas fa-th-large"></i>
        </button>
    `;
    
    // Add badges for available types
    if (availableTypes && availableTypes.length > 0) {
        availableTypes.forEach(function(typeInfo) {
            var typeIcon = getTypeIcon(typeInfo.type);
            var typeTitle = getTypeTitle(typeInfo.type);
            
            badgesHtml += `
                <button class="content-type-badge-compact ${currentFilter === typeInfo.type ? 'active' : ''}" 
                        data-type="${typeInfo.type}" 
                        title="${typeTitle} (${typeInfo.count})">
                    <i class="${typeIcon}"></i>
                </button>
            `;
        });
    } else {
        // Fallback: show all possible types if availableTypes is empty
        var allTypes = ['Reminder', 'Writing', 'Audio', 'GapFill', 'MultipleChoice', 'Match', 'ErrorFinding', 'CodeExercise', 'Quiz'];
        allTypes.forEach(function(type) {
            var typeIcon = getTypeIcon(type);
            var typeTitle = getTypeTitle(type);
            
            badgesHtml += `
                <button class="content-type-badge-compact ${currentFilter === type ? 'active' : ''}" 
                        data-type="${type}" 
                        title="${typeTitle}">
                    <i class="${typeIcon}"></i>
                </button>
            `;
        });
    }
    
    badgesContainer.html(badgesHtml);
    
    // Re-bind click events
    $('.content-type-badge-compact').off('click').on('click', function() {
        $('.content-type-badge-compact').removeClass('active');
        $(this).addClass('active');
        
        currentFilter = $(this).data('type');
        loadScheduleItems();
    });
}

function displayScheduleItems(data) {
    if (data.length === 0) {
        $('#schedule-items-list').html(`
            <div class="empty-placeholder">
                <i class="fas fa-calendar-times fa-2x text-muted mb-3"></i>
                <p class="text-muted">محتوای آموزشی یافت نشد</p>
            </div>
        `);
        $('#content-count').text('0');
        return;
    }

    var html = generateScheduleItemsHTML(data);
    $('#schedule-items-list').html(html);
    $('#content-count').text(data.length);
}

function generateScheduleItemsHTML(items) {
    var html = '';
    items.forEach(function(item) {
        var statusClass = getStatusClass(item.statusText || 'Active');
        var statusIcon = getStatusIcon(item.statusText || 'Active');
        var typeIcon = getTypeIcon(item.type || item.Type || '');
        var dueDateText = item.dueDate ? formatDate(item.dueDate) : 'بدون مهلت';
        
        html += `
            <div class="schedule-item-card-minimal" data-type="${item.type || item.Type || ''}">
                <div class="schedule-item-icon-minimal">
                    <i class="${typeIcon}"></i>
                </div>
                <div class="schedule-item-content-minimal">
                    <div class="schedule-item-header-minimal">
                        <h6 class="schedule-item-title-minimal">${item.title}</h6>
                        <span class="status-badge-minimal ${statusClass}">
                            <i class="${statusIcon}"></i>
                        </span>
                    </div>
                    <p class="schedule-item-description-minimal">${item.description || ''}</p>
                    <div class="schedule-item-meta-minimal">
                        <span class="meta-item-minimal">
                            <i class="fas fa-calendar-alt"></i>
                            ${dueDateText}
                        </span>
                        ${item.maxScore ? `
                            <span class="meta-item-minimal">
                                <i class="fas fa-star"></i>
                                ${item.maxScore}
                            </span>
                        ` : ''}
                        ${item.isMandatory ? `
                            <span class="meta-item-minimal mandatory">
                                <i class="fas fa-exclamation-circle"></i>
                                اجباری
                            </span>
                        ` : ''}
                    </div>
                </div>
                <div class="schedule-item-actions-minimal">
                    <button class="btn-action-minimal primary" onclick="openScheduleItem(${item.id})" title="شروع">
                        <i class="fas fa-play"></i>
                    </button>
                    <button class="btn-action-minimal secondary" onclick="viewScheduleItemDetails(${item.id})" title="جزئیات">
                        <i class="fas fa-info-circle"></i>
                    </button>
                </div>
            </div>
        `;
    });
    return html;
}

function getStatusClass(status) {
    switch (status) {
        case 'Active': return 'status-active';
        case 'Completed': return 'status-completed';
        case 'Expired': return 'status-expired';
        case 'Draft': return 'status-draft';
        default: return 'status-active';
    }
}

function getStatusIcon(status) {
    switch (status) {
        case 'Active': return 'fas fa-play-circle';
        case 'Completed': return 'fas fa-check-circle';
        case 'Expired': return 'fas fa-clock';
        case 'Draft': return 'fas fa-edit';
        default: return 'fas fa-circle';
    }
}

function getTypeIcon(type) {
    // Handle both string and numeric enum values
    var typeStr = type.toString();
    
    switch (typeStr) {
        case '0':
        case 'Reminder': return 'fas fa-sticky-note';
        case '1':
        case 'Writing': return 'fas fa-pen';
        case '2':
        case 'Audio': return 'fas fa-microphone';
        case '3':
        case 'GapFill': return 'fas fa-edit';
        case '4':
        case 'MultipleChoice': return 'fas fa-list-ul';
        case '5':
        case 'Match': return 'fas fa-link';
        case '6':
        case 'ErrorFinding': return 'fas fa-search';
        case '7':
        case 'CodeExercise': return 'fas fa-code';
        case '8':
        case 'Quiz': return 'fas fa-question-circle';
        default: return 'fas fa-file';
    }
}

function getTypeTitle(type) {
    // Handle both string and numeric enum values
    var typeStr = type.toString();
    
    switch (typeStr) {
        case '0':
        case 'Reminder': return 'یادآوری';
        case '1':
        case 'Writing': return 'نوشتاری';
        case '2':
        case 'Audio': return 'صوتی';
        case '3':
        case 'GapFill': return 'جای خالی';
        case '4':
        case 'MultipleChoice': return 'چند گزینه‌ای';
        case '5':
        case 'Match': return 'تطبیق';
        case '6':
        case 'ErrorFinding': return 'پیدا کردن خطا';
        case '7':
        case 'CodeExercise': return 'کدنویسی';
        case '8':
        case 'Quiz': return 'کویز';
        default: return 'فایل';
    }
}

function formatDate(date) {
    return new Date(date).toLocaleDateString('fa-IR');
}

function openScheduleItem(itemId) {
    // Placeholder for opening schedule item
    showToast('شروع تمرین...', 'info');
}

function viewScheduleItemDetails(itemId) {
    // Placeholder for viewing schedule item details
    showToast('نمایش جزئیات...', 'info');
}

function updateLastAccessed() {
    // Update last accessed time in progress modal
    var now = new Date();
    var timeString = now.toLocaleTimeString('fa-IR');
    $('#last-accessed-time').text(timeString);
}

function showToast(message, type) {
    // Simple toast notification
    var toastClass = type === 'error' ? 'alert-danger' : 'alert-info';
    var toastHtml = `
        <div class="alert ${toastClass} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999;" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    $('body').append(toastHtml);
    
    // Auto remove after 3 seconds
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 3000);
}
