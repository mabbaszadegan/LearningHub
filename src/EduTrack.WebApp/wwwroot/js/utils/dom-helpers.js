/**
 * DOM Helper Functions
 * Utility functions for DOM manipulation
 */

window.EduTrack = window.EduTrack || {};
window.EduTrack.DOM = window.EduTrack.DOM || {};

(function() {
    'use strict';

    /**
     * Wait for element to appear in DOM
     * @param {string} selector - CSS selector
     * @param {number} timeout - Timeout in milliseconds
     * @returns {Promise<HTMLElement>} Element promise
     */
    function waitForElement(selector, timeout = 5000) {
        return new Promise((resolve, reject) => {
            const element = document.querySelector(selector);
            if (element) {
                resolve(element);
                return;
            }

            const observer = new MutationObserver((mutations, obs) => {
                const element = document.querySelector(selector);
                if (element) {
                    obs.disconnect();
                    resolve(element);
                }
            });

            observer.observe(document.body, {
                childList: true,
                subtree: true
            });

            setTimeout(() => {
                observer.disconnect();
                reject(new Error(`Element ${selector} not found within ${timeout}ms`));
            }, timeout);
        });
    }

    /**
     * Get all elements matching selector (including dynamic ones)
     * @param {string} selector - CSS selector
     * @returns {NodeList} Elements
     */
    function querySelectorAll(selector) {
        return document.querySelectorAll(selector);
    }

    /**
     * Safe querySelector that returns null instead of throwing
     * @param {string} selector - CSS selector
     * @returns {HTMLElement|null} Element or null
     */
    function querySelector(selector) {
        try {
            return document.querySelector(selector);
        } catch (error) {
            console.warn('Invalid selector:', selector, error);
            return null;
        }
    }

    /**
     * Safe getElementById
     * @param {string} id - Element ID
     * @returns {HTMLElement|null} Element or null
     */
    function getElementById(id) {
        try {
            return document.getElementById(id);
        } catch (error) {
            console.warn('Invalid ID:', id, error);
            return null;
        }
    }

    /**
     * Add event listener with automatic cleanup tracking
     * @param {HTMLElement|string} element - Element or selector
     * @param {string} event - Event name
     * @param {Function} handler - Event handler
     * @param {Object} options - Event options
     * @returns {Function} Cleanup function
     */
    function addEventListener(element, event, handler, options = {}) {
        const el = typeof element === 'string' ? querySelector(element) : element;
        if (!el) {
            console.warn('Element not found for event listener:', element);
            return () => {};
        }

        el.addEventListener(event, handler, options);
        
        // Return cleanup function
        return () => {
            el.removeEventListener(event, handler, options);
        };
    }

    /**
     * Toggle class on element
     * @param {HTMLElement|string} element - Element or selector
     * @param {string} className - Class name
     * @param {boolean} force - Force add/remove
     * @returns {boolean} True if class was added
     */
    function toggleClass(element, className, force) {
        const el = typeof element === 'string' ? querySelector(element) : element;
        if (!el) return false;
        return el.classList.toggle(className, force);
    }

    /**
     * Add class to element
     * @param {HTMLElement|string} element - Element or selector
     * @param {string} className - Class name
     */
    function addClass(element, className) {
        const el = typeof element === 'string' ? querySelector(element) : element;
        if (el) el.classList.add(className);
    }

    /**
     * Remove class from element
     * @param {HTMLElement|string} element - Element or selector
     * @param {string} className - Class name
     */
    function removeClass(element, className) {
        const el = typeof element === 'string' ? querySelector(element) : element;
        if (el) el.classList.remove(className);
    }

    /**
     * Check if element has class
     * @param {HTMLElement|string} element - Element or selector
     * @param {string} className - Class name
     * @returns {boolean} True if has class
     */
    function hasClass(element, className) {
        const el = typeof element === 'string' ? querySelector(element) : element;
        return el ? el.classList.contains(className) : false;
    }

    /**
     * Set element attributes
     * @param {HTMLElement|string} element - Element or selector
     * @param {Object} attributes - Attributes object
     */
    function setAttributes(element, attributes) {
        const el = typeof element === 'string' ? querySelector(element) : element;
        if (!el) return;
        
        for (const [key, value] of Object.entries(attributes)) {
            if (value === null || value === undefined) {
                el.removeAttribute(key);
            } else {
                el.setAttribute(key, value);
            }
        }
    }

    /**
     * Get element data attributes
     * @param {HTMLElement} element - Element
     * @returns {Object} Data attributes object
     */
    function getDataAttributes(element) {
        if (!element || !element.dataset) return {};
        return { ...element.dataset };
    }

    /**
     * Create element with attributes and children
     * @param {string} tag - HTML tag name
     * @param {Object} attributes - Attributes object
     * @param {Array<HTMLElement|string>} children - Child elements or text
     * @returns {HTMLElement} Created element
     */
    function createElement(tag, attributes = {}, children = []) {
        const element = document.createElement(tag);
        
        // Set attributes
        for (const [key, value] of Object.entries(attributes)) {
            if (key === 'textContent' || key === 'innerHTML') {
                element[key] = value;
            } else if (key.startsWith('data-')) {
                element.setAttribute(key, value);
            } else {
                element[key] = value;
            }
        }
        
        // Append children
        for (const child of children) {
            if (typeof child === 'string') {
                element.appendChild(document.createTextNode(child));
            } else if (child instanceof HTMLElement) {
                element.appendChild(child);
            }
        }
        
        return element;
    }

    /**
     * Remove element from DOM
     * @param {HTMLElement|string} element - Element or selector
     */
    function removeElement(element) {
        const el = typeof element === 'string' ? querySelector(element) : element;
        if (el && el.parentNode) {
            el.parentNode.removeChild(el);
        }
    }

    /**
     * Scroll element into view
     * @param {HTMLElement|string} element - Element or selector
     * @param {Object} options - Scroll options
     */
    function scrollIntoView(element, options = {}) {
        const el = typeof element === 'string' ? querySelector(element) : element;
        if (el) {
            el.scrollIntoView({
                behavior: 'smooth',
                block: 'center',
                ...options
            });
        }
    }

    // Export functions
    window.EduTrack.DOM = {
        waitForElement,
        querySelectorAll,
        querySelector,
        getElementById,
        addEventListener,
        toggleClass,
        addClass,
        removeClass,
        hasClass,
        setAttributes,
        getDataAttributes,
        createElement,
        removeElement,
        scrollIntoView
    };
})();

