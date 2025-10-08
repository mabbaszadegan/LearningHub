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

    // Form validation enhancement - only for forms without custom handlers
    $('form:not(.session-form)').on('submit', function(e) {
        const form = $(this);
        const submitBtn = form.find('button[type="submit"]');
        
        // Store original state for restoration
        if (!submitBtn.data('original-html')) {
            submitBtn.data('original-html', submitBtn.html());
        }
        
        // Show loading state
        submitBtn.prop('disabled', true);
        submitBtn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Processing...');
        
        // Set a timeout to restore button if form doesn't submit successfully
        setTimeout(function() {
            if (submitBtn.prop('disabled')) {
                restoreSubmitButton(submitBtn);
            }
        }, 10000); // 10 second timeout
    });

    // Function to restore submit button
    function restoreSubmitButton(submitBtn) {
        const originalHtml = submitBtn.data('original-html');
        submitBtn.prop('disabled', false);
        
        if (originalHtml) {
            submitBtn.html(originalHtml);
        } else {
            // Fallback based on button class
            if (submitBtn.hasClass('btn-submit')) {
                submitBtn.html('<i class="fas fa-save"></i><span>ایجاد جلسه</span>');
            } else {
                submitBtn.html('Submit');
            }
        }
    }

    // Restore all submit buttons on page load (handles validation errors)
    function restoreAllSubmitButtons() {
        $('form button[type="submit"]').each(function() {
            restoreSubmitButton($(this));
        });
    }

    // Call restoration function on document ready
    restoreAllSubmitButtons();

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
