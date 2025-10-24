/**
 * Page Title Section JavaScript
 * Handles breadcrumb navigation and interactive features
 */

class PageTitleSection {
    constructor() {
        this.init();
    }

    init() {
        this.setupBreadcrumbNavigation();
        this.setupActionButtons();
        this.setupResponsiveBehavior();
        this.setupAccessibility();
    }

    /**
     * Setup breadcrumb navigation functionality
     */
    setupBreadcrumbNavigation() {
        const breadcrumbLinks = document.querySelectorAll('.breadcrumb-link');
        
        breadcrumbLinks.forEach((link, index) => {
            // Add click tracking for analytics
            link.addEventListener('click', (e) => {
                this.trackBreadcrumbClick(link, index);
            });

            // Add keyboard navigation support
            link.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    link.click();
                }
            });

            // Add hover effects
            link.addEventListener('mouseenter', () => {
                this.highlightBreadcrumbPath(link, index);
            });

            link.addEventListener('mouseleave', () => {
                this.clearBreadcrumbHighlight();
            });
        });
    }

    /**
     * Setup action buttons functionality
     */
    setupActionButtons() {
        const actionButtons = document.querySelectorAll('.title-actions .btn-teacher');
        
        actionButtons.forEach(button => {
            // Add ripple effect on click
            button.addEventListener('click', (e) => {
                this.createRippleEffect(e, button);
            });

            // Add loading state for async actions
            if (button.href && !button.href.startsWith('#')) {
                button.addEventListener('click', (e) => {
                    this.handleAsyncAction(e, button);
                });
            }
        });
    }

    /**
     * Setup responsive behavior
     */
    setupResponsiveBehavior() {
        const pageTitleSection = document.getElementById('pageTitleSection');
        if (!pageTitleSection) return;

        // Handle mobile menu toggle for breadcrumbs
        const handleResize = () => {
            const isMobile = window.innerWidth <= 768;
            pageTitleSection.classList.toggle('mobile-view', isMobile);
            
            if (isMobile) {
                this.setupMobileBreadcrumb();
            } else {
                this.setupDesktopBreadcrumb();
            }
        };

        window.addEventListener('resize', handleResize);
        handleResize(); // Initial call
    }

    /**
     * Setup accessibility features
     */
    setupAccessibility() {
        const pageTitleSection = document.getElementById('pageTitleSection');
        if (!pageTitleSection) return;

        // Add ARIA labels
        const breadcrumbNav = pageTitleSection.querySelector('.breadcrumb-modern');
        if (breadcrumbNav) {
            breadcrumbNav.setAttribute('aria-label', 'مسیر ناوبری');
        }

        // Add skip links for keyboard navigation
        this.addSkipLinks();
    }

    /**
     * Track breadcrumb click for analytics
     */
    trackBreadcrumbClick(link, index) {
        const breadcrumbText = link.querySelector('.breadcrumb-text')?.textContent || '';
        const breadcrumbUrl = link.href;
        
        // Send analytics event
        if (typeof gtag !== 'undefined') {
            gtag('event', 'breadcrumb_click', {
                'breadcrumb_text': breadcrumbText,
                'breadcrumb_index': index,
                'breadcrumb_url': breadcrumbUrl
            });
        }

        // Log for debugging
    }

    /**
     * Highlight breadcrumb path on hover
     */
    highlightBreadcrumbPath(link, index) {
        const breadcrumbItems = document.querySelectorAll('.breadcrumb-item');
        
        breadcrumbItems.forEach((item, i) => {
            if (i <= index) {
                item.classList.add('highlighted');
            }
        });
    }

    /**
     * Clear breadcrumb highlight
     */
    clearBreadcrumbHighlight() {
        const breadcrumbItems = document.querySelectorAll('.breadcrumb-item');
        breadcrumbItems.forEach(item => {
            item.classList.remove('highlighted');
        });
    }

    /**
     * Create ripple effect on button click
     */
    createRippleEffect(event, button) {
        const ripple = document.createElement('span');
        const rect = button.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;

        ripple.style.cssText = `
            position: absolute;
            width: ${size}px;
            height: ${size}px;
            left: ${x}px;
            top: ${y}px;
            background: rgba(0, 0, 0, 0.1);
            border-radius: 50%;
            transform: scale(0);
            animation: ripple 0.4s linear;
            pointer-events: none;
        `;

        button.style.position = 'relative';
        button.style.overflow = 'hidden';
        button.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 400);
    }

    /**
     * Handle async actions with loading state
     */
    handleAsyncAction(event, button) {
        const originalText = button.querySelector('span')?.textContent;
        const originalIcon = button.querySelector('i')?.className;
        
        // Show loading state using the standardized loading class
        button.classList.add('loading');
        button.disabled = true;
        
        if (button.querySelector('span')) {
            button.querySelector('span').textContent = 'در حال بارگذاری...';
        }
        
        if (button.querySelector('i')) {
            button.querySelector('i').className = 'fas fa-spinner fa-spin';
        }

        // Reset after a delay (in real implementation, this would be handled by the actual async operation)
        setTimeout(() => {
            button.classList.remove('loading');
            button.disabled = false;
            
            if (button.querySelector('span') && originalText) {
                button.querySelector('span').textContent = originalText;
            }
            
            if (button.querySelector('i') && originalIcon) {
                button.querySelector('i').className = originalIcon;
            }
        }, 2000);
    }

    /**
     * Setup mobile breadcrumb behavior
     */
    setupMobileBreadcrumb() {
        const breadcrumbList = document.querySelector('.breadcrumb-list');
        if (!breadcrumbList) return;

        // Add mobile-specific classes
        breadcrumbList.classList.add('mobile-breadcrumb');
        
        // Create mobile breadcrumb toggle
        this.createMobileBreadcrumbToggle();
    }

    /**
     * Setup desktop breadcrumb behavior
     */
    setupDesktopBreadcrumb() {
        const breadcrumbList = document.querySelector('.breadcrumb-list');
        if (!breadcrumbList) return;

        breadcrumbList.classList.remove('mobile-breadcrumb');
        
        // Remove mobile toggle if exists
        const mobileToggle = document.querySelector('.mobile-breadcrumb-toggle');
        if (mobileToggle) {
            mobileToggle.remove();
        }
    }

    /**
     * Create mobile breadcrumb toggle
     */
    createMobileBreadcrumbToggle() {
        const breadcrumbNav = document.querySelector('.breadcrumb-modern');
        if (!breadcrumbNav || document.querySelector('.mobile-breadcrumb-toggle')) return;

        const toggle = document.createElement('button');
        toggle.className = 'mobile-breadcrumb-toggle';
        toggle.innerHTML = '<i class="fas fa-ellipsis-h"></i>';
        toggle.setAttribute('aria-label', 'نمایش مسیر کامل');
        
        toggle.addEventListener('click', () => {
            const breadcrumbList = document.querySelector('.breadcrumb-list');
            if (breadcrumbList) {
                breadcrumbList.classList.toggle('expanded');
                toggle.setAttribute('aria-expanded', 
                    breadcrumbList.classList.contains('expanded'));
            }
        });

        breadcrumbNav.appendChild(toggle);
    }

    /**
     * Add skip links for accessibility
     */
    addSkipLinks() {
        const skipLink = document.createElement('a');
        skipLink.href = '#main-content';
        skipLink.textContent = 'رد شدن به محتوای اصلی';
        skipLink.className = 'skip-link';
        skipLink.style.cssText = `
            position: absolute;
            top: -40px;
            left: 6px;
            background: #000;
            color: #fff;
            padding: 8px;
            text-decoration: none;
            z-index: 1000;
            transition: top 0.3s;
        `;

        skipLink.addEventListener('focus', () => {
            skipLink.style.top = '6px';
        });

        skipLink.addEventListener('blur', () => {
            skipLink.style.top = '-40px';
        });

        document.body.insertBefore(skipLink, document.body.firstChild);
    }

    /**
     * Update page title dynamically
     */
    updateTitle(title, icon = null, description = null) {
        const titleElement = document.querySelector('.page-title .title-text');
        const iconElement = document.querySelector('.page-title i');
        const descriptionElement = document.querySelector('.page-description');

        if (titleElement) {
            titleElement.textContent = title;
        }

        if (iconElement && icon) {
            iconElement.className = icon;
        }

        if (descriptionElement && description) {
            descriptionElement.textContent = description;
        }

        // Update document title
        document.title = `${title} - پنل معلم EduTrack`;
    }

    /**
     * Add breadcrumb item dynamically
     */
    addBreadcrumbItem(text, url = null, icon = null, isActive = false) {
        const breadcrumbList = document.querySelector('.breadcrumb-list');
        if (!breadcrumbList) return;

        const item = document.createElement('li');
        item.className = `breadcrumb-item ${isActive ? 'active' : ''}`;

        if (url && !isActive) {
            item.innerHTML = `
                <a href="${url}" class="breadcrumb-link">
                    ${icon ? `<i class="${icon}"></i>` : ''}
                    <span class="breadcrumb-text">${text}</span>
                </a>
            `;
        } else {
            item.innerHTML = `
                <span class="breadcrumb-current">
                    ${icon ? `<i class="${icon}"></i>` : ''}
                    <span class="breadcrumb-text">${text}</span>
                </span>
            `;
        }

        breadcrumbList.appendChild(item);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new PageTitleSection();
});

// Add CSS animations
const pageTitleStyle = document.createElement('style');
pageTitleStyle.textContent = `
    @keyframes ripple {
        to {
            transform: scale(2);
            opacity: 0;
        }
    }
    
    .breadcrumb-item.highlighted {
        background: rgba(0, 0, 0, 0.05);
        border-radius: 0.25rem;
    }
    
    .mobile-breadcrumb-toggle {
        background: #e9ecef;
        border: none;
        color: #495057;
        padding: 0.25rem;
        border-radius: 0.25rem;
        cursor: pointer;
        margin-right: 0.25rem;
    }
    
    .mobile-breadcrumb.expanded {
        flex-wrap: wrap;
    }
    
    .skip-link:focus {
        outline: 2px solid #0d6efd;
        outline-offset: 2px;
    }
`;

// Check if style already exists before adding
if (!document.querySelector('#page-title-section-styles')) {
    pageTitleStyle.id = 'page-title-section-styles';
    document.head.appendChild(pageTitleStyle);
}
