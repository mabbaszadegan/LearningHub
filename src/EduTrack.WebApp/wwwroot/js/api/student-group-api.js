/**
 * Student Group API Service
 * API calls for student group operations
 */

window.EduTrack = window.EduTrack || {};
window.EduTrack.API = window.EduTrack.API || {};

(function() {
    'use strict';

    const baseUrl = '/Teacher/StudentGroup';

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
     * Student Group API Service
     */
    const StudentGroupAPI = {
        /**
         * Get students in a group
         * @param {number} groupId - Group ID
         * @returns {Promise<Object>} API response
         */
        async getStudents(groupId) {
            const url = `${baseUrl}/GetStudents?groupId=${groupId}`;
            return await fetchAPI(url, { method: 'GET' });
        }
    };

    // Export API
    window.EduTrack.API.StudentGroup = StudentGroupAPI;
})();

