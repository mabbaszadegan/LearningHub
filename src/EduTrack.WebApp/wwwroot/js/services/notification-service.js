/**
 * Notification Service
 * Unified notification system using toastr with fallback to alerts
 * This replaces all showSuccess/showError/showMessage methods across the app
 */

window.EduTrack = window.EduTrack || {};
window.EduTrack.Services = window.EduTrack.Services || {};

(function() {
    'use strict';

    /**
     * Ensure toastr is configured with defaults
     * @private
     */
    function ensureToastrDefaults() {
        if (typeof toastr === 'undefined') return;
        
        toastr.options = Object.assign({
            closeButton: true,
            progressBar: true,
            newestOnTop: true,
            positionClass: 'toast-top-right',
            timeOut: 4000,
            extendedTimeOut: 2000,
            preventDuplicates: true,
            showDuration: 300,
            hideDuration: 300
        }, toastr.options || {});
    }

    /**
     * Fallback alert for when toastr is not available
     * @private
     */
    function fallbackAlert(prefix, message) {
        try {
            alert((prefix ? prefix + ': ' : '') + message);
        } catch (error) {
            console.error('Error showing alert:', error);
        }
    }

    /**
     * Notification Service
     */
    const NotificationService = {
        /**
         * Show success notification
         * @param {string} message - Success message
         * @param {string} title - Optional title (default: 'موفقیت')
         * @param {Object} options - Toastr options
         */
        success(message, title = 'موفقیت', options = {}) {
            ensureToastrDefaults();
            if (typeof toastr !== 'undefined') {
                toastr.success(message, title, options);
            } else {
                fallbackAlert('موفق', message);
            }
        },

        /**
         * Show error notification
         * @param {string} message - Error message
         * @param {string} title - Optional title (default: 'خطا')
         * @param {Object} options - Toastr options
         */
        error(message, title = 'خطا', options = {}) {
            ensureToastrDefaults();
            if (typeof toastr !== 'undefined') {
                toastr.error(message, title, {
                    timeOut: 5000,
                    ...options
                });
            } else {
                fallbackAlert('خطا', message);
            }
        },

        /**
         * Show info notification
         * @param {string} message - Info message
         * @param {string} title - Optional title (default: 'اطلاع')
         * @param {Object} options - Toastr options
         */
        info(message, title = 'اطلاع', options = {}) {
            ensureToastrDefaults();
            if (typeof toastr !== 'undefined') {
                toastr.info(message, title, options);
            } else {
                fallbackAlert('', message);
            }
        },

        /**
         * Show warning notification
         * @param {string} message - Warning message
         * @param {string} title - Optional title (default: 'هشدار')
         * @param {Object} options - Toastr options
         */
        warning(message, title = 'هشدار', options = {}) {
            ensureToastrDefaults();
            if (typeof toastr !== 'undefined') {
                toastr.warning(message, title, options);
            } else {
                fallbackAlert('هشدار', message);
            }
        },

        /**
         * Clear all notifications
         */
        clear() {
            if (typeof toastr !== 'undefined') {
                toastr.clear();
            }
        },

        /**
         * Remove specific notification
         * @param {jQuery|HTMLElement} toast - Toast element
         */
        remove(toast) {
            if (typeof toastr !== 'undefined' && toastr.remove) {
                toastr.remove(toast);
            }
        }
    };

    // Export service
    window.EduTrack.Services.Notification = NotificationService;

    // Backward compatibility - also expose as global functions for existing code
    window.toastSuccess = (message, title, options) => NotificationService.success(message, title, options);
    window.toastError = (message, title, options) => NotificationService.error(message, title, options);
    window.toastInfo = (message, title, options) => NotificationService.info(message, title, options);
    window.toastWarning = (message, title, options) => NotificationService.warning(message, title, options);
})();

