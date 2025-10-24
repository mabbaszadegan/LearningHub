// Global DatePicker Initialization
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all Persian date pickers
    initializeAllDatePickers();
});

function initializeAllDatePickers() {
    // Find all elements with persian-datepicker class
    const dateInputs = document.querySelectorAll('.persian-datepicker');
    
    dateInputs.forEach(input => {
        // Skip if already initialized
        if (input.hasAttribute('data-datepicker-initialized')) {
            return;
        }
        
        // Get options from data attributes
        const options = {
            placeholder: input.getAttribute('placeholder') || 'انتخاب تاریخ',
            minDate: input.dataset.minDate,
            maxDate: input.dataset.maxDate,
            initialDate: input.value
        };
        
        // Find associated hidden field
        const hiddenField = findHiddenField(input);
        
        if (hiddenField) {
            options.onSelect = function(date, dateString) {
                // Convert Persian date to Gregorian and update hidden field
                let gregorianDate;
                
                // Use jalaali-js for accurate conversion
                if (typeof window.jalaali !== 'undefined') {
                    const parts = dateString.split('/');
                    if (parts.length === 3) {
                        const jy = parseInt(parts[0]);
                        const jm = parseInt(parts[1]);
                        const jd = parseInt(parts[2]);
                        const gregorian = window.jalaali.toGregorian(jy, jm, jd);
                        gregorianDate = new Date(gregorian.gy, gregorian.gm - 1, gregorian.gd);
                    }
                } else {
                    // Fallback to old method
                    gregorianDate = window.persianDate.persianStringToGregorianDate(dateString);
                }
                
                if (gregorianDate) {
                    hiddenField.value = gregorianDate.toISOString();
                }
                
                // Trigger change event on hidden field
                const changeEvent = new Event('change', { bubbles: true });
                hiddenField.dispatchEvent(changeEvent);
            };
        }
        
        // Initialize the datepicker
        new PersianDatePicker(input, options);
        
        // Mark as initialized
        input.setAttribute('data-datepicker-initialized', 'true');
        
        // Initialize with existing value if any
        if (hiddenField && hiddenField.value) {
            try {
                const existingDate = new Date(hiddenField.value);
                let persianDate;
                // Use jalaali-js for accurate conversion
                if (typeof window.jalaali !== 'undefined') {
                    const jalaali = window.jalaali.toJalaali(existingDate);
                    persianDate = `${jalaali.jy}/${jalaali.jm.toString().padStart(2, '0')}/${jalaali.jd.toString().padStart(2, '0')}`;
                } else {
                    // Fallback to old method
                    persianDate = window.persianDate.gregorianDateToPersianString(existingDate);
                }
                
                input.value = persianDate;
                
                // Update the datepicker's selected date
                if (input.datePicker) {
                    input.datePicker.setDate(persianDate);
                } else {
                    console.warn('datepicker-init: DatePicker not found on input');
                }
            } catch (e) {
                console.warn('Could not parse existing date:', hiddenField.value, e);
            }
        } else {
        }
    });
}

function findHiddenField(input) {
    // Try to find hidden field by various methods
    
    // Method 1: Look for data-target attribute (most reliable)
    const targetName = input.dataset.target;
    if (targetName) {
        const hiddenField = document.querySelector(`input[name="${targetName}"][type="hidden"]`);
        if (hiddenField) return hiddenField;
        
        // Also try by ID
        const hiddenFieldById = document.getElementById(targetName);
        if (hiddenFieldById && hiddenFieldById.type === 'hidden') return hiddenFieldById;
    }
    
    // Method 2: Look for hidden field with similar name
    const inputName = input.name || input.id;
    if (inputName) {
        // Remove "Persian" prefix if exists
        const baseName = inputName.replace(/^Persian/, '');
        const hiddenField = document.querySelector(`input[name="${baseName}"][type="hidden"]`);
        if (hiddenField) return hiddenField;
    }
    
    // Method 3: Look in the same parent container
    const container = input.closest('.mb-3, .form-group, .col-md-6, .col-sm-6, .date-input-wrapper');
    if (container) {
        const hiddenField = container.querySelector('input[type="hidden"]');
        if (hiddenField) return hiddenField;
    }
    
    return null;
}

// Utility function to set minimum date for a datepicker
function setDatePickerMinDate(inputId, minDate) {
    const input = document.getElementById(inputId);
    if (input && input.datePicker) {
        input.datePicker.options.minDate = minDate;
    }
}

// Utility function to set maximum date for a datepicker
function setDatePickerMaxDate(inputId, maxDate) {
    const input = document.getElementById(inputId);
    if (input && input.datePicker) {
        input.datePicker.options.maxDate = maxDate;
    }
}

// Utility function to get Persian date from datepicker
function getDatePickerValue(inputId) {
    const input = document.getElementById(inputId);
    return input ? input.value : null;
}

// Utility function to set Persian date for datepicker
function setDatePickerValue(inputId, persianDate) {
    const input = document.getElementById(inputId);
    if (input && input.datePicker) {
        input.datePicker.setDate(persianDate);
    }
}

// Export functions for global use
window.DatePickerUtils = {
    initializeAllDatePickers,
    setDatePickerMinDate,
    setDatePickerMaxDate,
    getDatePickerValue,
    setDatePickerValue
};
