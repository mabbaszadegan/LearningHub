/**
 * Desktop Header JavaScript
 * Handles modern desktop header functionality for student panel
 */

class DesktopHeader {
    constructor() {
        this.currentPage = this.detectCurrentPage();
        this.init();
    }

    init() {
        this.bindEvents();
        this.setActiveNavigation();
        this.handleResponsive();
        this.loadUserInfo();
    }

    detectCurrentPage() {
        const path = window.location.pathname.toLowerCase();
        
        // Student area detection
        if (path.includes('/student/course/scheduleitems')) {
            return 'my-courses';
        }
        if (path.includes('/student/course/catalog')) {
            return 'courses';
        }
        if (path.includes('/student/course/study')) {
            return 'my-courses';
        }
        if (path.includes('/student/course') && !path.includes('/catalog') && !path.includes('/study')) {
            return 'my-courses';
        }
        if (path.includes('/progress')) {
            return 'progress';
        }
        if (path.includes('/profile')) {
            return 'profile';
        }
        
        return 'courses'; // Default to courses instead of home
    }

    bindEvents() {
        // Navigation clicks
        document.querySelectorAll('.desktop-nav-link').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const href = link.getAttribute('href');
                if (href && href !== '#') {
                    this.navigateToPage(href);
                }
            });
        });

        // Dropdown navigation
        document.querySelectorAll('.desktop-nav-dropdown-item').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const href = item.getAttribute('href');
                if (href && href !== '#') {
                    this.navigateToPage(href);
                }
            });
        });

        // User menu actions
        document.querySelectorAll('.desktop-user-menu-item').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const action = item.dataset.action;
                if (action) {
                    this.handleUserAction(action);
                } else {
                    const href = item.getAttribute('href');
                    if (href && href !== '#') {
                        this.navigateToPage(href);
                    }
                }
            });
        });

        // Header button actions
        document.querySelectorAll('.desktop-header-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const action = btn.dataset.action;
                if (action) {
                    this.handleHeaderAction(action);
                }
            });
        });

        // Window resize handler
        window.addEventListener('resize', () => {
            this.handleResponsive();
        });

    }

    setActiveNavigation() {
        // Remove active class from all navigation items
        document.querySelectorAll('.desktop-nav-link').forEach(link => {
            link.classList.remove('active');
        });

        // Add active class to current page
        const activeLink = document.querySelector(`[data-page="${this.currentPage}"]`);
        if (activeLink) {
            activeLink.classList.add('active');
        }
    }

    navigateToPage(url) {
        if (url && url !== '#' && url !== window.location.pathname) {
            window.location.href = url;
        }
    }

    handleUserAction(action) {
        switch (action) {
            case 'profile':
                this.navigateToPage('/Profile');
                break;
            case 'settings':
                this.navigateToPage('/Settings');
                break;
            case 'logout':
                this.handleLogout();
                break;
            case 'help':
                this.showHelp();
                break;
            default:
                console.log('Unknown user action:', action);
        }
    }

    handleHeaderAction(action) {
        switch (action) {
            case 'notifications':
                this.showNotifications();
                break;
            case 'messages':
                this.showMessages();
                break;
            case 'help':
                this.showHelp();
                break;
            case 'fullscreen':
                this.toggleFullscreen();
                break;
            default:
                console.log('Unknown header action:', action);
        }
    }


    showNotifications() {
        // Implement notifications panel
        console.log('Showing notifications');
        // You can create a modal or dropdown for notifications
        this.showNotificationPanel();
    }

    showNotificationPanel() {
        // Create and show notification panel
        const panel = document.createElement('div');
        panel.className = 'desktop-notification-panel';
        panel.innerHTML = `
            <div class="notification-panel-header">
                <h6>اعلانات</h6>
                <button class="btn-close-notifications">&times;</button>
            </div>
            <div class="notification-panel-body">
                <div class="notification-item">
                    <i class="fas fa-bell text-primary"></i>
                    <div class="notification-content">
                        <div class="notification-title">اعلان جدید</div>
                        <div class="notification-time">2 دقیقه پیش</div>
                    </div>
                </div>
                <div class="notification-item">
                    <i class="fas fa-graduation-cap text-success"></i>
                    <div class="notification-content">
                        <div class="notification-title">دوره جدید اضافه شد</div>
                        <div class="notification-time">1 ساعت پیش</div>
                    </div>
                </div>
            </div>
        `;

        // Add styles
        panel.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            width: 350px;
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
            border: 1px solid rgba(0, 0, 0, 0.1);
            z-index: 1002;
            max-height: 400px;
            overflow-y: auto;
        `;

        document.body.appendChild(panel);

        // Close button functionality
        panel.querySelector('.btn-close-notifications').addEventListener('click', () => {
            panel.remove();
        });

        // Auto close after 5 seconds
        setTimeout(() => {
            if (panel.parentNode) {
                panel.remove();
            }
        }, 5000);
    }

    showMessages() {
        console.log('Showing messages');
        // Implement messages functionality
    }

    showHelp() {
        console.log('Showing help');
        // Implement help functionality
        window.open('/Help', '_blank');
    }

    toggleFullscreen() {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
        } else {
            document.exitFullscreen();
        }
    }

    handleLogout() {
        if (confirm('آیا می‌خواهید از حساب کاربری خود خارج شوید؟')) {
            // Implement logout functionality
            window.location.href = '/Account/Logout';
        }
    }

    handleResponsive() {
        const desktopHeader = document.querySelector('.desktop-header');
        const fixedHeader = document.querySelector('.fixed-header');
        const fixedFooter = document.querySelector('.fixed-footer');
        
        if (window.innerWidth >= 768) {
            if (desktopHeader) {
                desktopHeader.style.display = 'flex';
            }
            if (fixedHeader) {
                fixedHeader.style.display = 'none';
            }
            if (fixedFooter) {
                fixedFooter.style.display = 'none';
            }
        } else {
            if (desktopHeader) {
                desktopHeader.style.display = 'none';
            }
            if (fixedHeader) {
                fixedHeader.style.display = 'flex';
            }
            if (fixedFooter) {
                fixedFooter.style.display = 'flex';
            }
        }
    }


    loadUserInfo() {
        // Load user information for the header
        // This could be done via AJAX if needed
        const userAvatar = document.querySelector('.desktop-user-avatar');
        const userName = document.querySelector('.desktop-user-name');
        const userRole = document.querySelector('.desktop-user-role');

        if (userAvatar && !userAvatar.textContent.trim()) {
            // Set default avatar if no content
            userAvatar.textContent = 'U';
        }

        if (userName && !userName.textContent.trim()) {
            // Set default name if no content
            userName.textContent = 'کاربر';
        }

        if (userRole && !userRole.textContent.trim()) {
            // Set default role if no content
            userRole.textContent = 'دانش‌آموز';
        }
    }

    // Method to update active state programmatically
    setActivePage(page) {
        this.currentPage = page;
        this.setActiveNavigation();
    }

    // Method to show/hide header
    show() {
        const desktopHeader = document.querySelector('.desktop-header');
        if (desktopHeader) {
            desktopHeader.style.display = 'flex';
        }
    }

    hide() {
        const desktopHeader = document.querySelector('.desktop-header');
        if (desktopHeader) {
            desktopHeader.style.display = 'none';
        }
    }

    // Method to update notification badge
    updateNotificationBadge(count) {
        const badge = document.querySelector('.desktop-notification-badge');
        if (badge) {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.style.display = 'flex';
            } else {
                badge.style.display = 'none';
            }
        }
    }

    // Method to show loading state
    showLoading() {
        const header = document.querySelector('.desktop-header');
        if (header) {
            header.classList.add('desktop-header-loading');
        }
    }

    hideLoading() {
        const header = document.querySelector('.desktop-header');
        if (header) {
            header.classList.remove('desktop-header-loading');
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Only initialize on desktop
    if (window.innerWidth >= 768) {
        window.desktopHeader = new DesktopHeader();
    }
});

// Export for potential use in other scripts
window.DesktopHeader = DesktopHeader;
