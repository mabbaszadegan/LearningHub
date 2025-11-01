/**
 * Validation Utility Functions
 * Common validation functions used across the application
 */

window.EduTrack = window.EduTrack || {};
window.EduTrack.Validation = window.EduTrack.Validation || {};

(function() {
    'use strict';

    /**
     * Validate email address
     * @param {string} email - Email to validate
     * @returns {boolean} True if valid
     */
    function isValidEmail(email) {
        if (!email || typeof email !== 'string') return false;
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email.trim());
    }

    /**
     * Validate URL
     * @param {string} url - URL to validate
     * @returns {boolean} True if valid
     */
    function isValidUrl(url) {
        if (!url || typeof url !== 'string') return false;
        try {
            new URL(url);
            return true;
        } catch {
            return false;
        }
    }

    /**
     * Validate JSON string
     * @param {string} jsonString - JSON string to validate
     * @returns {Object} {isValid: boolean, error?: string, data?: object}
     */
    function isValidJson(jsonString) {
        if (!jsonString || typeof jsonString !== 'string') {
            return { isValid: false, error: 'JSON string is required' };
        }
        
        try {
            const parsed = JSON.parse(jsonString);
            return { isValid: true, data: parsed };
        } catch (error) {
            return { isValid: false, error: error.message };
        }
    }

    /**
     * Validate required field
     * @param {*} value - Value to validate
     * @param {string} fieldName - Field name for error message
     * @returns {Object} {isValid: boolean, error?: string}
     */
    function validateRequired(value, fieldName = 'فیلد') {
        if (value === null || value === undefined) {
            return { isValid: false, error: `${fieldName} الزامی است` };
        }
        
        if (typeof value === 'string' && value.trim() === '') {
            return { isValid: false, error: `${fieldName} الزامی است` };
        }
        
        if (Array.isArray(value) && value.length === 0) {
            return { isValid: false, error: `${fieldName} الزامی است` };
        }
        
        return { isValid: true };
    }

    /**
     * Validate minimum length
     * @param {string|Array} value - Value to validate
     * @param {number} minLength - Minimum length
     * @param {string} fieldName - Field name for error message
     * @returns {Object} {isValid: boolean, error?: string}
     */
    function validateMinLength(value, minLength, fieldName = 'فیلد') {
        const length = Array.isArray(value) ? value.length : (value || '').toString().length;
        if (length < minLength) {
            return { isValid: false, error: `${fieldName} باید حداقل ${minLength} کاراکتر باشد` };
        }
        return { isValid: true };
    }

    /**
     * Validate maximum length
     * @param {string|Array} value - Value to validate
     * @param {number} maxLength - Maximum length
     * @param {string} fieldName - Field name for error message
     * @returns {Object} {isValid: boolean, error?: string}
     */
    function validateMaxLength(value, maxLength, fieldName = 'فیلد') {
        const length = Array.isArray(value) ? value.length : (value || '').toString().length;
        if (length > maxLength) {
            return { isValid: false, error: `${fieldName} نمی‌تواند بیشتر از ${maxLength} کاراکتر باشد` };
        }
        return { isValid: true };
    }

    /**
     * Validate date range (start date should be before end date)
     * @param {Date|string} startDate - Start date
     * @param {Date|string} endDate - End date
     * @returns {Object} {isValid: boolean, error?: string}
     */
    function validateDateRange(startDate, endDate) {
        if (!startDate || !endDate) {
            return { isValid: true }; // Optional validation
        }
        
        const start = new Date(startDate);
        const end = new Date(endDate);
        
        if (isNaN(start.getTime()) || isNaN(end.getTime())) {
            return { isValid: false, error: 'تاریخ نامعتبر است' };
        }
        
        if (start > end) {
            return { isValid: false, error: 'تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد' };
        }
        
        return { isValid: true };
    }

    /**
     * Validate number range
     * @param {number} value - Number to validate
     * @param {number} min - Minimum value
     * @param {number} max - Maximum value
     * @param {string} fieldName - Field name for error message
     * @returns {Object} {isValid: boolean, error?: string}
     */
    function validateNumberRange(value, min, max, fieldName = 'فیلد') {
        const num = Number(value);
        if (isNaN(num)) {
            return { isValid: false, error: `${fieldName} باید یک عدد معتبر باشد` };
        }
        
        if (num < min || num > max) {
            return { isValid: false, error: `${fieldName} باید بین ${min} و ${max} باشد` };
        }
        
        return { isValid: true };
    }

    /**
     * Validate Persian date format (YYYY/MM/DD)
     * @param {string} dateString - Date string to validate
     * @returns {Object} {isValid: boolean, error?: string}
     */
    function validatePersianDate(dateString) {
        if (!dateString || typeof dateString !== 'string') {
            return { isValid: false, error: 'تاریخ معتبر نیست' };
        }
        
        const persianDateRegex = /^(\d{4})\/(\d{1,2})\/(\d{1,2})$/;
        const match = dateString.trim().match(persianDateRegex);
        
        if (!match) {
            return { isValid: false, error: 'فرمت تاریخ باید YYYY/MM/DD باشد' };
        }
        
        const [, year, month, day] = match;
        const yearNum = parseInt(year, 10);
        const monthNum = parseInt(month, 10);
        const dayNum = parseInt(day, 10);
        
        if (yearNum < 1300 || yearNum > 1500) {
            return { isValid: false, error: 'سال باید بین 1300 تا 1500 باشد' };
        }
        
        if (monthNum < 1 || monthNum > 12) {
            return { isValid: false, error: 'ماه باید بین 1 تا 12 باشد' };
        }
        
        if (dayNum < 1 || dayNum > 31) {
            return { isValid: false, error: 'روز باید بین 1 تا 31 باشد' };
        }
        
        return { isValid: true };
    }

    // Export functions
    window.EduTrack.Validation = {
        isValidEmail,
        isValidUrl,
        isValidJson,
        validateRequired,
        validateMinLength,
        validateMaxLength,
        validateDateRange,
        validateNumberRange,
        validatePersianDate
    };
})();

