/**
 * EduTrack Core
 * Main initialization file that loads all core utilities and services
 * This should be loaded before any feature-specific JavaScript files
 */

(function() {
    'use strict';

    // Initialize EduTrack namespace
    window.EduTrack = window.EduTrack || {};
    window.EduTrack.isInitialized = false;

    /**
     * Initialize EduTrack core services
     */
    function initializeCore() {
        if (window.EduTrack.isInitialized) {
            return;
        }

        // Core is initialized when all required files are loaded
        // This will be set to true after loading utilities and services
        console.log('EduTrack Core: Initializing...');
        
        // Check if required services are available
        const hasUtils = window.EduTrack?.Utils;
        const hasServices = window.EduTrack?.Services;
        const hasAPI = window.EduTrack?.API;

        if (hasUtils && hasServices && hasAPI) {
            window.EduTrack.isInitialized = true;
            console.log('EduTrack Core: Initialized successfully');
            
            // Dispatch custom event for other scripts to listen
            window.dispatchEvent(new CustomEvent('edutrack:core:ready', {
                detail: {
                    Utils: window.EduTrack.Utils,
                    Services: window.EduTrack.Services,
                    API: window.EduTrack.API
                }
            }));
        } else {
            console.warn('EduTrack Core: Some services not loaded yet', {
                hasUtils,
                hasServices,
                hasAPI
            });
        }
    }

    // Try to initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeCore);
    } else {
        // DOM already loaded, initialize immediately
        initializeCore();
    }

    // Also try after a short delay to catch late-loading scripts
    setTimeout(initializeCore, 100);
    setTimeout(initializeCore, 500);
})();

