$(document).ready(function () {
    // Store original button content
    const originalButtonContent = $('#createSessionBtn').html();

    // Fix for loading state issue when validation errors occur
    function restoreSubmitButton() {
        const submitBtn = $('#createSessionBtn');
        if (submitBtn.length > 0) {
            submitBtn.prop('disabled', false);
            submitBtn.html(originalButtonContent);
            console.log('Submit button restored');
        }
    }

    // Override the global form submit handler for this specific form
    $('.session-form').off('submit').on('submit', function (e) {
        const submitBtn = $('#createSessionBtn');

        // Show loading state
        submitBtn.prop('disabled', true);
        submitBtn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ÏÑ ÍÇá ÑÏÇÒÔ...');

        // Set timeout to restore button if form doesn't submit
        setTimeout(function () {
            if (submitBtn.prop('disabled')) {
                restoreSubmitButton();
            }
        }, 5000);
    });

    // Force restore button state immediately on page load
    restoreSubmitButton();

    // Check if there are validation errors and restore button state
    if ($('.text-danger:visible, .field-validation-error:visible, .input-validation-error').length > 0) {
        restoreSubmitButton();
    }

    // Monitor for validation errors and restore button
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === 'childList') {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1 && (node.classList.contains('text-danger') || node.classList.contains('field-validation-error'))) {
                        restoreSubmitButton();
                    }
                });
            }
        });
    });

    // Start observing
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
    // Auto-resize textareas
    $('.modern-textarea').on('input', function () {
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    });

    // Form validation feedback
    $('.modern-input, .modern-select, .modern-textarea').on('blur', function () {
        if ($(this).hasClass('input-validation-error')) {
            $(this).addClass('error-state');
        } else {
            $(this).removeClass('error-state');
        }
    });

    // Set current time first
    const now = new Date();
    const currentTime = now.toTimeString().slice(0, 5);
    $('#SessionTime').val(currentTime);

    // Initialize Persian date picker (it will set current date automatically)
    setTimeout(function () {
        initializeAllDatePickers();

        // Update the hidden field after initialization
        setTimeout(function () {
            updateSessionDateTime();
            console.log('Persian date picker initialized with current date');
        }, 50);
    }, 100);

    // Update hidden field when time changes
    $('#SessionTime').on('change', function () {
        updateSessionDateTime();
    });

    // Update hidden field when Persian date changes
    $('#PersianSessionDate').on('change', function () {
        updateSessionDateTime();
    });


    function updateSessionDateTime() {
        const persianDate = $('#PersianSessionDate').val();
        const time = $('#SessionTime').val();

        if (persianDate && time) {
            try {
                let gregorianDate;

                if (window.persianDate && window.persianDate.persianStringToGregorianDate) {
                    // Use the existing Persian date library
                    gregorianDate = window.persianDate.persianStringToGregorianDate(persianDate);
                } else {
                    // Fallback: manual conversion
                    gregorianDate = convertPersianToGregorian(persianDate);
                }

                if (gregorianDate) {
                    // Set the time
                    const [hours, minutes] = time.split(':');
                    gregorianDate.setHours(parseInt(hours), parseInt(minutes), 0, 0);

                    // Update hidden field
                    $('#SessionDate').val(gregorianDate.toISOString());
                }
            } catch (e) {
                console.warn('Error converting Persian date:', e);
            }
        }
    }

    // Fallback Persian to Gregorian conversion
    function convertPersianToGregorian(persianDateString) {
        try {
            const parts = persianDateString.split('/');
            if (parts.length !== 3) return null;

            const pYear = parseInt(parts[0]);
            const pMonth = parseInt(parts[1]);
            const pDay = parseInt(parts[2]);

            // Simple conversion (approximate)
            let gYear = pYear + 621;
            let gMonth = pMonth;
            let gDay = pDay;

            // Adjust for Persian calendar offset
            if (pMonth <= 6) {
                gMonth = pMonth + 6;
            } else {
                gMonth = pMonth - 6;
                gYear = pYear + 622;
            }

            return new Date(gYear, gMonth - 1, gDay);
        } catch (e) {
            console.warn('Error in fallback conversion:', e);
            return null;
        }
    }
});
