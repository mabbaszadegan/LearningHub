/**
 * Common Utility Functions
 * Reusable utility functions used across the application
 */

window.EduTrack = window.EduTrack || {};
window.EduTrack.Utils = window.EduTrack.Utils || {};

(function() {
    'use strict';

    /**
     * Debounce function - limits the rate at which a function can fire
     * @param {Function} func - Function to debounce
     * @param {number} wait - Milliseconds to wait
     * @param {boolean} immediate - Execute immediately on first call
     * @returns {Function} Debounced function
     */
    function debounce(func, wait, immediate = false) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                timeout = null;
                if (!immediate) func.apply(this, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(this, args);
        };
    }

    /**
     * Throttle function - limits function execution to once per wait period
     * @param {Function} func - Function to throttle
     * @param {number} limit - Milliseconds to wait between executions
     * @returns {Function} Throttled function
     */
    function throttle(func, limit) {
        let inThrottle;
        return function executedFunction(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }

    /**
     * Deep clone an object
     * @param {*} obj - Object to clone
     * @returns {*} Cloned object
     */
    function deepClone(obj) {
        if (obj === null || typeof obj !== 'object') return obj;
        if (obj instanceof Date) return new Date(obj.getTime());
        if (obj instanceof Array) return obj.map(item => deepClone(item));
        if (obj instanceof Object) {
            const clonedObj = {};
            for (const key in obj) {
                if (obj.hasOwnProperty(key)) {
                    clonedObj[key] = deepClone(obj[key]);
                }
            }
            return clonedObj;
        }
    }

    /**
     * Check if two objects are deeply equal
     * @param {*} a - First object
     * @param {*} b - Second object
     * @returns {boolean} True if equal
     */
    function deepEqual(a, b) {
        if (a === b) return true;
        if (a == null || b == null) return false;
        if (typeof a !== typeof b) return false;
        
        if (typeof a === 'object') {
            const keysA = Object.keys(a);
            const keysB = Object.keys(b);
            
            if (keysA.length !== keysB.length) return false;
            
            for (const key of keysA) {
                if (!keysB.includes(key)) return false;
                if (!deepEqual(a[key], b[key])) return false;
            }
            
            return true;
        }
        
        return false;
    }

    /**
     * Parse query string from URL
     * @param {string} queryString - Query string (optional, defaults to window.location.search)
     * @returns {Object} Parsed query parameters
     */
    function parseQueryString(queryString = window.location.search) {
        const params = new URLSearchParams(queryString);
        const result = {};
        for (const [key, value] of params) {
            result[key] = value;
        }
        return result;
    }

    /**
     * Build query string from object
     * @param {Object} params - Parameters object
     * @returns {string} Query string
     */
    function buildQueryString(params) {
        const searchParams = new URLSearchParams();
        for (const [key, value] of Object.entries(params)) {
            if (value !== null && value !== undefined && value !== '') {
                searchParams.append(key, value);
            }
        }
        return searchParams.toString();
    }

    /**
     * Get CSRF token from page
     * @returns {string|null} CSRF token
     */
    function getCsrfToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
            return tokenInput.value;
        }
        
        // Try meta tag
        const metaTag = document.querySelector('meta[name="__RequestVerificationToken"]');
        if (metaTag) {
            return metaTag.content;
        }
        
        return null;
    }

    /**
     * Sleep/delay function
     * @param {number} ms - Milliseconds to wait
     * @returns {Promise} Promise that resolves after delay
     */
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    /**
     * Check if value is empty (null, undefined, empty string, empty array, empty object)
     * @param {*} value - Value to check
     * @returns {boolean} True if empty
     */
    function isEmpty(value) {
        if (value == null) return true;
        if (typeof value === 'string') return value.trim() === '';
        if (Array.isArray(value)) return value.length === 0;
        if (typeof value === 'object') return Object.keys(value).length === 0;
        return false;
    }

    /**
     * Format file size to human readable format
     * @param {number} bytes - File size in bytes
     * @returns {string} Formatted size
     */
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }

    // Export functions
    window.EduTrack.Utils = {
        debounce,
        throttle,
        deepClone,
        deepEqual,
        parseQueryString,
        buildQueryString,
        getCsrfToken,
        sleep,
        isEmpty,
        formatFileSize
    };
})();

