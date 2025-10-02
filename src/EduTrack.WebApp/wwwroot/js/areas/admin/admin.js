// Admin Area JavaScript

// Admin Dashboard functionality
class AdminDashboard {
    constructor() {
        this.initializeCharts();
        this.initializeDataTables();
        this.initializeModals();
        this.initializeTooltips();
    }

    initializeCharts() {
        // User Registration Chart
        const userRegistrationCtx = document.getElementById('userRegistrationChart');
        if (userRegistrationCtx) {
            new Chart(userRegistrationCtx, {
                type: 'line',
                data: {
                    labels: window.userRegistrationData?.labels || [],
                    datasets: [{
                        label: 'ثبت نام کاربران',
                        data: window.userRegistrationData?.data || [],
                        borderColor: '#667eea',
                        backgroundColor: 'rgba(102, 126, 234, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        }

        // Class Activity Chart
        const classActivityCtx = document.getElementById('classActivityChart');
        if (classActivityCtx) {
            new Chart(classActivityCtx, {
                type: 'bar',
                data: {
                    labels: window.classActivityData?.labels || [],
                    datasets: [{
                        label: 'فعالیت کلاس‌ها',
                        data: window.classActivityData?.data || [],
                        backgroundColor: 'rgba(102, 126, 234, 0.8)',
                        borderColor: '#667eea',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        }
    }

    initializeDataTables() {
        // Initialize DataTables for admin tables
        $('.admin-data-table').DataTable({
            language: {
                url: '/lib/datatables/persian.json'
            },
            responsive: true,
            pageLength: 25,
            order: [[0, 'desc']]
        });
    }

    initializeModals() {
        // User management modals
        $('.btn-reset-password').on('click', function() {
            const userId = $(this).data('user-id');
            const userName = $(this).data('user-name');
            
            $('#resetPasswordModal').modal('show');
            $('#resetPasswordUserId').val(userId);
            $('#resetPasswordUserName').text(userName);
        });

        $('.btn-toggle-user-status').on('click', function() {
            const userId = $(this).data('user-id');
            const userName = $(this).data('user-name');
            const isActive = $(this).data('is-active');
            
            const action = isActive ? 'غیرفعال' : 'فعال';
            if (confirm(`آیا مطمئن هستید که می‌خواهید کاربر ${userName} را ${action} کنید؟`)) {
                AdminDashboard.toggleUserStatus(userId);
            }
        });
    }

    initializeTooltips() {
        // Initialize Bootstrap tooltips
        $('[data-bs-toggle="tooltip"]').tooltip();
    }

    static toggleUserStatus(userId) {
        $.ajax({
            url: '/Admin/Users/ToggleStatus',
            type: 'POST',
            data: {
                userId: userId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    AdminDashboard.showToast('success', response.message);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    AdminDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                AdminDashboard.showToast('error', 'خطا در ارتباط با سرور');
            }
        });
    }

    static resetPassword(userId, newPassword) {
        $.ajax({
            url: '/Admin/Users/ResetPassword',
            type: 'POST',
            data: {
                userId: userId,
                newPassword: newPassword,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    AdminDashboard.showToast('success', response.message);
                    $('#resetPasswordModal').modal('hide');
                } else {
                    AdminDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                AdminDashboard.showToast('error', 'خطا در ارتباط با سرور');
            }
        });
    }

    static logoutUser(userId) {
        $.ajax({
            url: '/Admin/Sessions/LogoutUser',
            type: 'POST',
            data: {
                userId: userId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    AdminDashboard.showToast('success', response.message);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    AdminDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                AdminDashboard.showToast('error', 'خطا در ارتباط با سرور');
            }
        });
    }

    static showToast(type, message) {
        const toast = $(`
            <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `);
        
        $('#toast-container').append(toast);
        toast.toast('show');
        
        setTimeout(() => toast.remove(), 5000);
    }
}

// Activity Logs functionality
class AdminActivityLogs {
    constructor() {
        this.initializeFilters();
        this.initializeExport();
    }

    initializeFilters() {
        $('#activityLogFilter').on('submit', function(e) {
            e.preventDefault();
            const formData = $(this).serialize();
            window.location.href = '/Admin/ActivityLogs?' + formData;
        });

        $('.filter-clear').on('click', function() {
            window.location.href = '/Admin/ActivityLogs';
        });
    }

    initializeExport() {
        $('.btn-export-logs').on('click', function() {
            const currentUrl = new URL(window.location);
            currentUrl.searchParams.set('export', 'true');
            window.location.href = currentUrl.toString();
        });
    }
}

// Course Management functionality
class AdminCourseManagement {
    constructor() {
        this.initializeCourseActions();
    }

    initializeCourseActions() {
        $('.btn-toggle-course-status').on('click', function() {
            const courseId = $(this).data('course-id');
            const courseTitle = $(this).data('course-title');
            const isActive = $(this).data('is-active');
            
            const action = isActive ? 'غیرفعال' : 'فعال';
            if (confirm(`آیا مطمئن هستید که می‌خواهید دوره "${courseTitle}" را ${action} کنید؟`)) {
                AdminCourseManagement.toggleCourseStatus(courseId);
            }
        });
    }

    static toggleCourseStatus(courseId) {
        $.ajax({
            url: '/Admin/Courses/ToggleStatus',
            type: 'POST',
            data: {
                courseId: courseId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    AdminDashboard.showToast('success', response.message);
                    setTimeout(() => location.reload(), 1500);
                } else {
                    AdminDashboard.showToast('error', response.message);
                }
            },
            error: function() {
                AdminDashboard.showToast('error', 'خطا در ارتباط با سرور');
            }
        });
    }
}

// Initialize admin functionality when document is ready
$(document).ready(function() {
    // Create toast container if it doesn't exist
    if ($('#toast-container').length === 0) {
        $('body').append('<div id="toast-container" class="toast-container position-fixed top-0 end-0 p-3"></div>');
    }

    // Initialize admin modules based on current page
    const currentPath = window.location.pathname;
    
    if (currentPath.includes('/Admin/Home') || currentPath === '/Admin') {
        new AdminDashboard();
    }
    
    if (currentPath.includes('/Admin/ActivityLogs')) {
        new AdminActivityLogs();
    }
    
    if (currentPath.includes('/Admin/Courses')) {
        new AdminCourseManagement();
    }

    // Global admin functionality
    $('.btn-confirm-action').on('click', function() {
        const message = $(this).data('confirm-message') || 'آیا مطمئن هستید؟';
        return confirm(message);
    });

    // Auto-hide alerts after 5 seconds
    $('.alert').delay(5000).fadeOut();
});

// Export functions for global use
window.AdminDashboard = AdminDashboard;
window.AdminActivityLogs = AdminActivityLogs;
window.AdminCourseManagement = AdminCourseManagement;
