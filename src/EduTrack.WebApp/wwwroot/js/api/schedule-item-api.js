/**
 * Schedule Item API Service
 * Centralized API calls for schedule items
 */

window.EduTrack = window.EduTrack || {};
window.EduTrack.API = window.EduTrack.API || {};

(function() {
    'use strict';

    const Utils = window.EduTrack.Utils || {};
    const baseUrl = '/Teacher/ScheduleItem';

    /**
     * Base fetch function with error handling
     * @private
     */
    async function fetchAPI(url, options = {}) {
        // Validate URL before making request
        if (!url || typeof url !== 'string' || url.trim() === '') {
            throw new Error(`Invalid URL provided to fetchAPI: ${url}`);
        }

        // Trim and clean URL
        let trimmedUrl = url.trim();
        
        // Check for common invalid patterns
        if (trimmedUrl.includes('undefined') || trimmedUrl.includes('null') || trimmedUrl.includes('NaN')) {
            throw new Error(`Invalid URL contains undefined/null/NaN: ${trimmedUrl}`);
        }
        
        // Validate it's a proper relative or absolute URL format
        try {
            // For relative URLs, create a test URL using window.location as base
            if (typeof window !== 'undefined' && window.location) {
                if (trimmedUrl.startsWith('/')) {
                    new URL(trimmedUrl, window.location.origin);
                } else if (trimmedUrl.startsWith('http://') || trimmedUrl.startsWith('https://')) {
                    new URL(trimmedUrl);
                } else {
                    // Relative path without leading slash - use current origin
                    new URL(trimmedUrl, window.location.origin);
                }
            }
        } catch (urlError) {
            throw new Error(`Invalid URL format provided to fetchAPI: "${trimmedUrl}". ${urlError.message}`);
        }

        // Get CSRF token properly
        let csrfToken = null;
        if (Utils && typeof Utils.getCsrfToken === 'function') {
            csrfToken = Utils.getCsrfToken();
        } else {
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            csrfToken = tokenInput ? tokenInput.value : null;
        }

        // Build headers object, filtering out invalid values
        const headers = new Headers();
        headers.set('Content-Type', 'application/json');
        
        // Only add CSRF token if it's a valid string
        if (csrfToken && typeof csrfToken === 'string' && csrfToken.trim() !== '') {
            headers.set('RequestVerificationToken', csrfToken.trim());
        }

        // Merge with provided headers
        if (options.headers) {
            if (options.headers instanceof Headers) {
                options.headers.forEach((value, key) => {
                    if (value != null && typeof value === 'string' && value.trim() !== '') {
                        headers.set(key, value.trim());
                    }
                });
            } else if (typeof options.headers === 'object') {
                for (const [key, value] of Object.entries(options.headers)) {
                    if (value != null && typeof value === 'string' && value.trim() !== '') {
                        headers.set(key, value.trim());
                    }
                }
            }
        }

        // Build final options object
        const mergedOptions = {
            method: options.method || 'GET',
            headers: headers,
            credentials: options.credentials || 'same-origin'
        };

        // Only add body if it exists and is not empty
        if (options.body !== undefined && options.body !== null) {
            if (typeof options.body === 'string' || options.body instanceof FormData || options.body instanceof Blob) {
                mergedOptions.body = options.body;
            } else if (typeof options.body === 'object') {
                mergedOptions.body = JSON.stringify(options.body);
            }
        }

        try {
            const response = await fetch(trimmedUrl, mergedOptions);
            
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            
            return await response.text();
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    /**
     * Schedule Item API Service
     */
    const ScheduleItemAPI = {
        /**
         * Get schedule items by teaching plan
         * @param {number} teachingPlanId - Teaching plan ID
         * @returns {Promise<Object>} API response
         */
        async getScheduleItems(teachingPlanId) {
            const url = `${baseUrl}/GetScheduleItems?teachingPlanId=${teachingPlanId}`;
            return await fetchAPI(url, { method: 'GET' });
        },

        /**
         * Get single schedule item by ID
         * @param {number} id - Schedule item ID
         * @returns {Promise<Object>} API response
         */
        async getScheduleItem(id) {
            // Validate ID is a valid positive number
            const parsedId = parseInt(id);
            if (!id || isNaN(parsedId) || parsedId <= 0) {
                throw new Error(`Invalid schedule item ID: ${id}`);
            }
            const url = `${baseUrl}/GetScheduleItem?id=${parsedId}`;
            return await fetchAPI(url, { method: 'GET' });
        },

        /**
         * Get schedule item by ID (alternative endpoint)
         * @param {number} id - Schedule item ID
         * @returns {Promise<Object>} API response
         */
        async getById(id) {
            // Validate ID is a valid positive number
            if (id === null || id === undefined || id === '') {
                throw new Error(`Invalid schedule item ID: ${id}`);
            }
            const parsedId = parseInt(id, 10);
            if (isNaN(parsedId) || parsedId <= 0) {
                throw new Error(`Invalid schedule item ID: ${id} (parsed as: ${parsedId})`);
            }
            // Ensure baseUrl is defined and valid
            if (!baseUrl || typeof baseUrl !== 'string') {
                throw new Error(`Invalid baseUrl: ${baseUrl}`);
            }
            // Build URL ensuring proper formatting
            const cleanBaseUrl = baseUrl.trim().replace(/\/+$/, ''); // Remove trailing slashes
            const url = `${cleanBaseUrl}/GetById/${parsedId}`;
            
            // Debug logging (remove in production if needed)
            if (url.includes('undefined') || url.includes('null') || url.includes('NaN')) {
                console.error('Invalid URL constructed in getById:', { id, parsedId, url, baseUrl });
                throw new Error(`Invalid URL constructed: ${url}. Original ID: ${id}`);
            }
            
            return await fetchAPI(url, { method: 'GET' });
        },

        /**
         * Create new schedule item
         * @param {Object} data - Schedule item data
         * @returns {Promise<Object>} API response
         */
        async createScheduleItem(data) {
            const url = `${baseUrl}/CreateScheduleItem`;
            return await fetchAPI(url, {
                method: 'POST',
                body: JSON.stringify(data)
            });
        },

        /**
         * Update existing schedule item
         * @param {Object} data - Schedule item data
         * @returns {Promise<Object>} API response
         */
        async updateScheduleItem(data) {
            const url = `${baseUrl}/UpdateScheduleItem`;
            return await fetchAPI(url, {
                method: 'POST',
                body: JSON.stringify(data)
            });
        },

        /**
         * Delete schedule item
         * @param {number} id - Schedule item ID
         * @returns {Promise<Object>} API response
         */
        async deleteScheduleItem(id) {
            // Validate ID is a valid positive number
            const parsedId = parseInt(id);
            if (!id || isNaN(parsedId) || parsedId <= 0) {
                throw new Error(`Invalid schedule item ID: ${id}`);
            }
            const url = `${baseUrl}/DeleteScheduleItem`;
            return await fetchAPI(url, {
                method: 'POST',
                body: JSON.stringify({ id: parsedId })
            });
        },

        /**
         * Save schedule item step
         * @param {Object} data - Step data
         * @returns {Promise<Object>} API response
         */
        async saveStep(data) {
            debugger;
            if (!data || typeof data !== 'object') {
                console.error('ScheduleItemAPI.saveStep called with invalid payload:', data);
                throw new Error('داده مرحله ارسال نشده است');
            }
            if (Object.keys(data).length === 0) {
                console.error('ScheduleItemAPI.saveStep called with empty payload object');
                throw new Error('داده مرحله خالی است');
            }

            const url = `${baseUrl}/SaveStep`;
            return await fetchAPI(url, {
                method: 'POST',
                body: JSON.stringify(data)
            });
        },

        /**
         * Complete schedule item
         * @param {number} id - Schedule item ID
         * @returns {Promise<Object>} API response
         */
        async complete(id) {
            const url = `${baseUrl}/Complete`;
            return await fetchAPI(url, {
                method: 'POST',
                body: JSON.stringify({ id })
            });
        },

        /**
         * Get schedule item statistics
         * @param {number} teachingPlanId - Teaching plan ID
         * @returns {Promise<Object>} API response
         */
        async getStats(teachingPlanId) {
            const url = `${baseUrl}/GetStats?teachingPlanId=${teachingPlanId}`;
            return await fetchAPI(url, { method: 'GET' });
        }
    };

    // Export API
    window.EduTrack.API.ScheduleItem = ScheduleItemAPI;
})();

