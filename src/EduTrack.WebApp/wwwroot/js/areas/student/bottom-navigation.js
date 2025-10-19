/**
 * Bottom Navigation JavaScript
 * Handles mobile bottom navigation functionality
 */

class BottomNavigation {
    constructor() {
        this.currentPage = this.detectCurrentPage();
        this.init();
    }

    init() {
        this.bindEvents();
        this.setActiveItem();
        this.handleResponsive();
    }

    detectCurrentPage() {
        const path = window.location.pathname.toLowerCase();
        
        if (path.includes('/student/course/scheduleitems')) {
            return 'my-courses';
        }
        if (path.includes('/student/course/catalog')) {
            return 'courses';
        }
        if (path.includes('/student/course') && !path.includes('/catalog')) {
            return 'my-courses';
        }
        if (path.includes('/student/home') || path.includes('/student')) {
            return 'home';
        }
        if (path.includes('/catalog') || path.includes('/courses')) {
            return 'courses';
        }
        if (path.includes('/progress')) {
            return 'progress';
        }
        if (path.includes('/profile')) {
            return 'profile';
        }
        
        return 'home'; // Default
    }

    bindEvents() {
        // Navigation item clicks
        document.querySelectorAll('.nav-item').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const page = item.dataset.page;
                this.navigateToPage(page);
            });
        });

        // Window resize handler
        window.addEventListener('resize', () => {
            this.handleResponsive();
        });
    }

    setActiveItem() {
        // Remove active class from all items
        document.querySelectorAll('.nav-item').forEach(item => {
            item.classList.remove('active');
        });

        // Add active class to current page
        const activeItem = document.querySelector(`[data-page="${this.currentPage}"]`);
        if (activeItem) {
            activeItem.classList.add('active');
        }
    }

    navigateToPage(page) {
        const routes = {
            'home': '/Student/Home',
            'courses': '/Student/Course/Catalog',
            'my-courses': '/Student/Course',
            'progress': '/Progress',
            'profile': '/Profile'
        };

        const route = routes[page];
        if (route && window.location.pathname !== route) {
            window.location.href = route;
        }
    }

    handleResponsive() {
        const bottomNav = document.querySelector('.bottom-nav');
        const fixedHeader = document.querySelector('.fixed-header');
        const fixedFooter = document.querySelector('.fixed-footer');
        
        if (!bottomNav || !fixedHeader || !fixedFooter) return;

        // Show/hide navigation based on screen size
        if (window.innerWidth >= 768) {
            fixedHeader.style.display = 'none';
            fixedFooter.style.display = 'none';
        } else {
            fixedHeader.style.display = 'flex';
            fixedFooter.style.display = 'flex';
        }
    }

    // Method to update active state programmatically
    setActivePage(page) {
        this.currentPage = page;
        this.setActiveItem();
    }

    // Method to show/hide navigation
    show() {
        const bottomNav = document.querySelector('.bottom-nav');
        if (bottomNav) {
            bottomNav.style.display = 'flex';
        }
    }

    hide() {
        const bottomNav = document.querySelector('.bottom-nav');
        if (bottomNav) {
            bottomNav.style.display = 'none';
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.bottomNavigation = new BottomNavigation();
    
        // Add course details functionality
        window.showCourseDetails = function(courseId) {
            // Navigate to course details page
            window.location.href = `/Student/Course/Details/${courseId}`;
        };
});

// Export for potential use in other scripts
window.BottomNavigation = BottomNavigation;
