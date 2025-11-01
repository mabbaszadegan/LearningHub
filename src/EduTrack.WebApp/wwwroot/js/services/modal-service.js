/**
 * Modal Service
 * Unified modal/dialog system to replace alert() and confirm()
 * Uses Bootstrap modals if available, falls back to native dialogs
 */

window.EduTrack = window.EduTrack || {};
window.EduTrack.Services = window.EduTrack.Services || {};

(function() {
    'use strict';

    const DOM = window.EduTrack.DOM || {};

    /**
     * Check if Bootstrap is available
     * @private
     */
    function isBootstrapAvailable() {
        return typeof bootstrap !== 'undefined' && bootstrap.Modal;
    }

    /**
     * Create Bootstrap modal HTML
     * @private
     */
    function createModalHTML(id, title, message, type = 'info') {
        const iconMap = {
            success: 'fa-check-circle text-success',
            error: 'fa-exclamation-circle text-danger',
            warning: 'fa-exclamation-triangle text-warning',
            info: 'fa-info-circle text-info',
            question: 'fa-question-circle text-primary'
        };

        const icon = iconMap[type] || iconMap.info;
        
        return `
            <div class="modal fade" id="${id}" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">
                                <i class="fas ${icon} me-2"></i>
                                ${title}
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            ${message}
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">انصراف</button>
                            <button type="button" class="btn btn-primary" data-modal-confirm="true">تأیید</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Modal Service
     */
    const ModalService = {
        /**
         * Show alert dialog (replaces alert())
         * @param {string} message - Message to display
         * @param {string} title - Dialog title (default: 'اطلاع')
         * @param {string} type - Alert type: success, error, warning, info (default: 'info')
         * @returns {Promise<void>} Promise that resolves when dialog is closed
         */
        async alert(message, title = 'اطلاع', type = 'info') {
            if (!isBootstrapAvailable()) {
                // Fallback to native alert
                window.alert((title ? title + ': ' : '') + message);
                return Promise.resolve();
            }

            return new Promise((resolve) => {
                const modalId = 'edutrack-alert-' + Date.now();
                const modalHTML = createModalHTML(modalId, title, message, type);
                
                // Remove confirm button for alert
                const htmlWithoutConfirm = modalHTML.replace(
                    '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">انصراف</button>\n                            <button type="button" class="btn btn-primary" data-modal-confirm="true">تأیید</button>',
                    '<button type="button" class="btn btn-primary" data-bs-dismiss="modal">تأیید</button>'
                );

                // Create and append modal
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = htmlWithoutConfirm;
                const modalElement = tempDiv.firstElementChild;
                document.body.appendChild(modalElement);

                // Initialize Bootstrap modal
                const modal = new bootstrap.Modal(modalElement);
                
                // Clean up on close
                modalElement.addEventListener('hidden.bs.modal', () => {
                    modal.dispose();
                    document.body.removeChild(modalElement);
                    resolve();
                }, { once: true });

                modal.show();
            });
        },

        /**
         * Show confirm dialog (replaces confirm())
         * @param {string} message - Message to display
         * @param {string} title - Dialog title (default: 'تأیید')
         * @param {string} confirmText - Confirm button text (default: 'تأیید')
         * @param {string} cancelText - Cancel button text (default: 'انصراف')
         * @returns {Promise<boolean>} Promise that resolves to true if confirmed, false if cancelled
         */
        async confirm(message, title = 'تأیید', confirmText = 'تأیید', cancelText = 'انصراف') {
            if (!isBootstrapAvailable()) {
                // Fallback to native confirm
                return Promise.resolve(window.confirm((title ? title + ': ' : '') + message));
            }

            return new Promise((resolve) => {
                const modalId = 'edutrack-confirm-' + Date.now();
                let modalHTML = createModalHTML(modalId, title, message, 'question');
                
                // Replace button texts
                modalHTML = modalHTML.replace('انصراف', cancelText);
                modalHTML = modalHTML.replace('تأیید', confirmText);

                // Create and append modal
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = modalHTML;
                const modalElement = tempDiv.firstElementChild;
                document.body.appendChild(modalElement);

                // Initialize Bootstrap modal
                const modal = new bootstrap.Modal(modalElement);
                
                // Handle confirm button
                const confirmBtn = modalElement.querySelector('[data-modal-confirm="true"]');
                confirmBtn.addEventListener('click', () => {
                    modal.hide();
                    resolve(true);
                });

                // Handle cancel/close
                const handleCancel = () => {
                    modal.hide();
                    resolve(false);
                };
                
                modalElement.querySelector('[data-bs-dismiss="modal"]').addEventListener('click', handleCancel);
                modalElement.querySelector('.btn-secondary').addEventListener('click', handleCancel);

                // Clean up on close
                modalElement.addEventListener('hidden.bs.modal', () => {
                    modal.dispose();
                    document.body.removeChild(modalElement);
                }, { once: true });

                modal.show();
            });
        },

        /**
         * Show custom modal
         * @param {string|HTMLElement} content - Modal content (HTML string or element)
         * @param {Object} options - Modal options
         * @returns {Promise<any>} Promise that resolves with result
         */
        async showModal(content, options = {}) {
            if (!isBootstrapAvailable()) {
                console.warn('Bootstrap not available for custom modal');
                return Promise.resolve(null);
            }

            const {
                title = '',
                size = '',
                backdrop = true,
                keyboard = true,
                focus = true
            } = options;

            return new Promise((resolve, reject) => {
                const modalId = 'edutrack-modal-' + Date.now();
                const sizeClass = size ? `modal-${size}` : '';
                
                const modalHTML = `
                    <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true" data-bs-backdrop="${backdrop}" data-bs-keyboard="${keyboard}">
                        <div class="modal-dialog modal-dialog-centered ${sizeClass}">
                            <div class="modal-content">
                                ${title ? `
                                    <div class="modal-header">
                                        <h5 class="modal-title">${title}</h5>
                                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                    </div>
                                ` : ''}
                                <div class="modal-body">
                                    ${typeof content === 'string' ? content : content.outerHTML}
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">بستن</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;

                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = modalHTML;
                const modalElement = tempDiv.firstElementChild;
                document.body.appendChild(modalElement);

                const modal = new bootstrap.Modal(modalElement, {
                    backdrop,
                    keyboard,
                    focus
                });

                modalElement.addEventListener('hidden.bs.modal', () => {
                    modal.dispose();
                    document.body.removeChild(modalElement);
                    resolve(null);
                }, { once: true });

                modal.show();
            });
        }
    };

    // Export service
    window.EduTrack.Services.Modal = ModalService;

    // Backward compatibility - provide global functions
    window.edutrackAlert = (message, title, type) => ModalService.alert(message, title, type);
    window.edutrackConfirm = (message, title, confirmText, cancelText) => 
        ModalService.confirm(message, title, confirmText, cancelText);
})();

