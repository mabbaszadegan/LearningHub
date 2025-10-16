/**
 * Global Lazy Loading for Audio and Video Elements
 * This prevents multiple media elements from loading simultaneously
 */

(function() {
    'use strict';
    
    // Setup global event delegation for lazy loading media elements
    document.addEventListener('play', (e) => {
        const target = e.target;
        if (target.tagName === 'AUDIO' || target.tagName === 'VIDEO') {
            const source = target.querySelector('source[data-src]');
            if (source && source.dataset.src && !source.src) {
                console.log('Lazy loading media:', source.dataset.src);
                source.src = source.dataset.src;
                target.load(); // Reload the media element
            }
        }
    }, true); // Use capture phase to catch the event early
    
    // Also handle click events on play buttons for better compatibility
    document.addEventListener('click', (e) => {
        const target = e.target;
        if (target.tagName === 'AUDIO' || target.tagName === 'VIDEO') {
            const source = target.querySelector('source[data-src]');
            if (source && source.dataset.src && !source.src) {
                console.log('Lazy loading media on click:', source.dataset.src);
                source.src = source.dataset.src;
                target.load(); // Reload the media element
            }
        }
    }, true);
    
    console.log('Global lazy loading initialized');
})();
