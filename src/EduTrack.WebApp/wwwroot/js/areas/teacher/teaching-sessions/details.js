// Teaching Session Details Manager
class TeachingSessionDetailsManager {
    constructor(options = {}) {
        this.sessionId = options.sessionId;
        this.isEditing = false;
        this.currentEditingCard = null;

        this.init();
    }

    init() {
        this.setupEventListeners();
    }

    setupEventListeners() {
        // Tab navigation
        $(document).on('click', '.tab-btn', (e) => {
            e.preventDefault();
            const tabName = $(e.currentTarget).data('tab');
            this.switchTab(tabName);
        });

        // Edit button clicks
        $(document).on('click', '.btn-edit-attendance', (e) => {
            e.preventDefault();
            const card = $(e.target).closest('.student-card');
            this.startEdit(card);
        });

        // Cancel button clicks
        $(document).on('click', '.btn-cancel', (e) => {
            e.preventDefault();
            const card = $(e.target).closest('.student-card');
            this.cancelEdit(card);
        });

        // Form submission
        $(document).on('submit', '.attendance-edit-form', (e) => {
            e.preventDefault();
            const form = $(e.target);
            this.saveAttendance(form);
        });

        // Close edit mode when clicking outside
        $(document).on('click', (e) => {
            if (this.isEditing && !$(e.target).closest('.student-card').length) {
                this.cancelEdit(this.currentEditingCard);
            }
        });
    }

    switchTab(tabName) {
        // Remove active class from all tabs and panels
        $('.tab-btn').removeClass('active');
        $('.tab-panel').removeClass('active');

        // Add active class to clicked tab and corresponding panel
        $(`.tab-btn[data-tab="${tabName}"]`).addClass('active');
        $(`#${tabName}`).addClass('active');
    }

    startEdit(card) {
        if (this.isEditing) {
            this.cancelEdit(this.currentEditingCard);
        }

        this.isEditing = true;
        this.currentEditingCard = card;

        // Hide card content and show edit form
        card.find('.card-content').hide();
        card.find('.edit-form').show();

        // Add editing class
        card.addClass('editing');

        // Focus on first input
        card.find('.form-control').first().focus();
    }

    cancelEdit(card) {
        if (!this.isEditing) return;

        this.isEditing = false;
        this.currentEditingCard = null;

        // Show card content and hide edit form
        card.find('.card-content').show();
        card.find('.edit-form').hide();

        // Remove editing class
        card.removeClass('editing');

        // Reset form to original values
        this.resetForm(card);
    }

    resetForm(card) {
        const form = card.find('.attendance-edit-form');
        const studentId = form.data('student-id');

        // Reset form to original values
        // This would need to be implemented based on the original data
        // For now, we'll just hide the form
    }

    async saveAttendance(form) {
        const card = form.closest('.student-card');
        const studentId = form.data('student-id');
        const attendanceId = form.data('attendance-id');

        // Show loading state
        const saveBtn = form.find('.btn-save');
        const originalText = saveBtn.html();
        saveBtn.html('<i class="fas fa-spinner fa-spin"></i> در حال ذخیره...').prop('disabled', true);

        try {
            // Collect form data
            const formData = {
                studentId: studentId, // This is now a GUID string
                attendanceId: attendanceId,
                sessionId: this.sessionId,
                status: parseInt(form.find('[name="status"]').val()),
                participationScore: form.find('[name="participationScore"]').val() ? parseFloat(form.find('[name="participationScore"]').val()) : null,
                comment: form.find('[name="comment"]').val() || null
            };

            // Send to server
            const response = await fetch('/Teacher/TeachingSessions/UpdateAttendance', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (result.success) {
                // Update the card with new data
                this.updateCardData(card, formData);

                // Show success message
                this.showMessage('حضور و غیاب با موفقیت ذخیره شد', 'success');

                // Exit edit mode
                this.cancelEdit(card);
            } else {
                throw new Error(result.message || 'خطا در ذخیره حضور و غیاب');
            }
        } catch (error) {
            console.error('Error saving attendance:', error);
            this.showMessage(error.message || 'خطا در ذخیره حضور و غیاب', 'error');
        } finally {
            // Restore button state
            saveBtn.html(originalText).prop('disabled', false);
        }
    }

    updateCardData(card, data) {
        // Update status badge
        const statusTexts = {
            0: 'غایب',
            1: 'حاضر',
            2: 'تأخیر',
            3: 'معذور'
        };

        const statusClasses = {
            0: 'bg-danger',
            1: 'bg-success',
            2: 'bg-warning',
            3: 'bg-info'
        };

        const statusBadge = card.find('.status-badge');
        statusBadge.removeClass('bg-success bg-danger bg-warning bg-info')
            .addClass(statusClasses[data.status])
            .text(statusTexts[data.status]);

        // Update participation score
        const scoreElement = card.find('.score-value');
        if (data.participationScore !== null) {
            scoreElement.text(data.participationScore.toFixed(1));
        } else {
            scoreElement.text('-').addClass('no-value');
        }

        // Update comment
        const commentElement = card.find('.comment-text');
        if (data.comment) {
            commentElement.text(data.comment).removeClass('no-value');
        } else {
            commentElement.text('-').addClass('no-value');
        }

        // Update completion indicator
        const completionIndicator = card.find('.completion-indicator');
        completionIndicator.html(`
            <i class="fas fa-check-circle text-success"></i>
            <span class="text-success">ثبت شده</span>
        `);

        // Add has-record class if not already present
        card.addClass('has-record');
    }

    showMessage(message, type = 'info') {
        // Create toast notification
        const toastClass = type === 'success' ? 'alert-success' :
            type === 'error' ? 'alert-danger' : 'alert-info';

        const toast = $(`
            <div class="alert ${toastClass} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);

        $('body').append(toast);

        // Auto remove after 5 seconds
        setTimeout(() => {
            toast.alert('close');
        }, 5000);
    }
}

// Initialize when document is ready
$(document).ready(function () {
    // Get session ID from data attribute or URL
    const sessionId = $('body').data('session-id') ||
        window.location.pathname.split('/').pop();

    if (sessionId) {
        window.teachingSessionDetailsManager = new TeachingSessionDetailsManager({
            sessionId: sessionId
        });
    }
});