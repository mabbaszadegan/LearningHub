/**
 * Schedule API Service
 * API calls for schedule-related operations (groups, subchapters)
 */

window.EduTrack = window.EduTrack || {};
window.EduTrack.API = window.EduTrack.API || {};

(function() {
    'use strict';

    const baseUrl = '/Teacher/Schedule';

    /**
     * Base fetch function
     * @private
     */
    async function fetchAPI(url, options = {}) {
        const csrfToken = (() => {
            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            return token ? token.value : null;
        })();

        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                ...(csrfToken && { 'RequestVerificationToken': csrfToken })
            },
            credentials: 'same-origin'
        };

        const mergedOptions = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...(options.headers || {})
            }
        };

        try {
            const response = await fetch(url, mergedOptions);
            
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
     * Schedule API Service
     */
    const ScheduleAPI = {
        /**
         * Get groups for teaching plan
         * @param {number} teachingPlanId - Teaching plan ID
         * @returns {Promise<Object>} API response
         */
        async getGroups(teachingPlanId) {
            const url = `${baseUrl}/GetGroups?teachingPlanId=${teachingPlanId}`;
            return await fetchAPI(url, { method: 'GET' });
        },

        /**
         * Get subchapters for teaching plan
         * @param {number} teachingPlanId - Teaching plan ID
         * @returns {Promise<Object>} API response
         */
        async getSubChapters(teachingPlanId) {
            const url = `${baseUrl}/GetSubChapters?teachingPlanId=${teachingPlanId}`;
            return await fetchAPI(url, { method: 'GET' });
        }
    };

    // Export API
    window.EduTrack.API.Schedule = ScheduleAPI;
})();

