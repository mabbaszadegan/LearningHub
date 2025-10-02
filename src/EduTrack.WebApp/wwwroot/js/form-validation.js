// Real-time Form Validation
class FormValidator {
    constructor() {
        this.validators = {
            required: (value) => value && value.trim().length > 0,
            email: (value) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value),
            minLength: (value, min) => value && value.length >= min,
            maxLength: (value, max) => value && value.length <= max,
            number: (value) => !isNaN(value) && value !== '',
            positiveNumber: (value) => !isNaN(value) && parseFloat(value) > 0,
            persianDate: (value) => this.isValidPersianDate(value),
            futureDate: (value) => this.isFutureDate(value)
        };

        this.messages = {
            required: 'این فیلد الزامی است',
            email: 'فرمت ایمیل صحیح نیست',
            minLength: 'حداقل {min} کاراکتر وارد کنید',
            maxLength: 'حداکثر {max} کاراکتر مجاز است',
            number: 'لطفا عدد وارد کنید',
            positiveNumber: 'لطفا عدد مثبت وارد کنید',
            persianDate: 'فرمت تاریخ صحیح نیست (مثال: 1403/01/15)',
            futureDate: 'تاریخ باید در آینده باشد',
            dateRange: 'تاریخ پایان باید بعد از تاریخ شروع باشد'
        };
    }

    isValidPersianDate(dateString) {
        if (!dateString) return false;
        
        const parts = dateString.split('/');
        if (parts.length !== 3) return false;
        
        const year = parseInt(parts[0]);
        const month = parseInt(parts[1]);
        const day = parseInt(parts[2]);
        
        if (isNaN(year) || isNaN(month) || isNaN(day)) return false;
        if (year < 1300 || year > 1500) return false;
        if (month < 1 || month > 12) return false;
        if (day < 1 || day > 31) return false;
        
        // Check days in month
        if (month <= 6 && day > 31) return false;
        if (month > 6 && month <= 11 && day > 30) return false;
        if (month === 12 && day > 29) return false;
        
        return true;
    }

    isFutureDate(persianDateString) {
        if (!this.isValidPersianDate(persianDateString)) return false;
        
        const today = window.persianDate.getTodayPersian();
        const parsed = window.persianDate.parsePersian(persianDateString);
        
        if (parsed.year > today.year) return true;
        if (parsed.year === today.year && parsed.month > today.month) return true;
        if (parsed.year === today.year && parsed.month === today.month && parsed.day >= today.day) return true;
        
        return false;
    }

    validateField(field, rules) {
        const value = field.value.trim();
        const errors = [];

        for (const rule of rules) {
            const [ruleName, ...params] = rule.split(':');
            
            if (ruleName === 'required' && !this.validators.required(value)) {
                errors.push(this.messages.required);
                break; // If required fails, don't check other rules
            }
            
            if (value && this.validators[ruleName]) {
                if (params.length > 0) {
                    if (!this.validators[ruleName](value, ...params)) {
                        let message = this.messages[ruleName];
                        params.forEach((param, index) => {
                            message = message.replace(`{${Object.keys(this.messages)[index] || 'param'}}`, param);
                        });
                        errors.push(message);
                    }
                } else {
                    if (!this.validators[ruleName](value)) {
                        errors.push(this.messages[ruleName]);
                    }
                }
            }
        }

        return errors;
    }

    showFieldError(field, errors) {
        const errorContainer = field.parentElement.querySelector('.field-error');
        const validationSpan = field.parentElement.querySelector('.text-danger');
        
        if (errors.length > 0) {
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
            
            if (validationSpan) {
                validationSpan.textContent = errors[0];
                validationSpan.style.display = 'block';
            }
        } else {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
            
            if (validationSpan) {
                validationSpan.textContent = '';
                validationSpan.style.display = 'none';
            }
        }
    }

    validateDateRange(startField, endField) {
        const startValue = startField.value.trim();
        const endValue = endField.value.trim();
        
        if (!startValue || !endValue) return true;
        
        if (!this.isValidPersianDate(startValue) || !this.isValidPersianDate(endValue)) {
            return true; // Let individual field validation handle invalid dates
        }
        
        const startParsed = window.persianDate.parsePersian(startValue);
        const endParsed = window.persianDate.parsePersian(endValue);
        
        if (endParsed.year < startParsed.year) return false;
        if (endParsed.year === startParsed.year && endParsed.month < startParsed.month) return false;
        if (endParsed.year === startParsed.year && endParsed.month === startParsed.month && endParsed.day <= startParsed.day) return false;
        
        return true;
    }

    initializeForm(formSelector) {
        const form = document.querySelector(formSelector);
        if (!form) return;

        // Add validation rules to fields
        const validationRules = {
            'input[name="Name"]': ['required', 'maxLength:200'],
            'select[name="CourseId"]': ['required'],
            'select[name="TeacherId"]': ['required'],
            'input[name="StartDate"]': ['required', 'persianDate', 'futureDate'],
            'input[name="EndDate"]': ['persianDate']
        };

        // Initialize real-time validation
        Object.keys(validationRules).forEach(selector => {
            const field = form.querySelector(selector);
            if (field) {
                const rules = validationRules[selector];
                
                // Add input event listener for real-time validation
                field.addEventListener('input', () => {
                    const errors = this.validateField(field, rules);
                    this.showFieldError(field, errors);
                    
                    // Special handling for date range validation
                    if (selector.includes('StartDate') || selector.includes('EndDate')) {
                        this.validateDateRangeFields(form);
                    }
                });

                // Add blur event for more thorough validation
                field.addEventListener('blur', () => {
                    const errors = this.validateField(field, rules);
                    this.showFieldError(field, errors);
                });
            }
        });

        // Form submission validation
        form.addEventListener('submit', (e) => {
            let hasErrors = false;
            
            Object.keys(validationRules).forEach(selector => {
                const field = form.querySelector(selector);
                if (field) {
                    const rules = validationRules[selector];
                    const errors = this.validateField(field, rules);
                    this.showFieldError(field, errors);
                    
                    if (errors.length > 0) {
                        hasErrors = true;
                    }
                }
            });

            // Validate date range
            const startField = form.querySelector('input[name="StartDate"]');
            const endField = form.querySelector('input[name="EndDate"]');
            
            if (startField && endField && !this.validateDateRange(startField, endField)) {
                this.showFieldError(endField, [this.messages.dateRange]);
                hasErrors = true;
            }

            if (hasErrors) {
                e.preventDefault();
                
                // Scroll to first error
                const firstError = form.querySelector('.is-invalid');
                if (firstError) {
                    firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    firstError.focus();
                }
            }
        });
    }

    validateDateRangeFields(form) {
        const startField = form.querySelector('input[name="StartDate"]');
        const endField = form.querySelector('input[name="EndDate"]');
        
        if (startField && endField && startField.value && endField.value) {
            if (!this.validateDateRange(startField, endField)) {
                this.showFieldError(endField, [this.messages.dateRange]);
            } else {
                // Clear date range error if it was the only error
                const endErrors = this.validateField(endField, ['persianDate']);
                this.showFieldError(endField, endErrors);
            }
        }
    }
}

// Initialize form validator
window.formValidator = new FormValidator();
