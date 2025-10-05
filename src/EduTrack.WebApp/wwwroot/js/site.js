// Site-specific JavaScript functionality
$(document).ready(function() {
    // Initialize Toastr if available
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": true,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut",
            "rtl": true
        };
    }

    // Auto-dismiss alerts after 5 seconds
    $('.alert').each(function() {
        const alert = $(this);
        setTimeout(function() {
            alert.fadeOut();
        }, 5000);
    });

    // Confirm delete actions
    $('.btn-danger[data-confirm]').on('click', function(e) {
        const message = $(this).data('confirm');
        if (!confirm(message)) {
            e.preventDefault();
        }
    });

    // Form validation enhancement
    $('form').on('submit', function() {
        const submitBtn = $(this).find('button[type="submit"]');
        submitBtn.prop('disabled', true);
        submitBtn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Processing...');
    });

    // Exam timer initialization
    if (typeof window.examTimerData !== 'undefined') {
        const timer = new ExamTimer(window.examTimerData.durationMinutes, function() {
            alert('Time is up! Your exam will be submitted automatically.');
            // Auto-submit the exam
            const form = document.getElementById('exam-form');
            if (form) {
                form.submit();
            }
        });
        timer.start();
    }

    // Offline form submission
    $('form[data-offline]').on('submit', function(e) {
        if (!navigator.onLine) {
            e.preventDefault();
            
            const form = $(this);
            const url = form.attr('action');
            const data = form.serialize();
            const csrfToken = form.find('input[name="__RequestVerificationToken"]').val();
            
            offlineQueue.queueSubmission(url, data, csrfToken)
                .then(() => {
                    alert('Your submission has been queued for when you\'re back online.');
                })
                .catch(error => {
                    console.error('Failed to queue offline submission:', error);
                    alert('Failed to queue your submission. Please try again when online.');
                });
        }
    });

    // Progress tracking
    $('.progress-item').on('click', function() {
        const itemId = $(this).data('item-id');
        const itemType = $(this).data('item-type');
        
        // Mark as started if not already started
        if (!$(this).hasClass('started')) {
            $(this).addClass('started');
            // Could send AJAX request to update progress
        }
    });

    // Resource download tracking
    $('a[data-resource-id]').on('click', function() {
        const resourceId = $(this).data('resource-id');
        // Track resource access
        console.log('Resource accessed:', resourceId);
    });
});
