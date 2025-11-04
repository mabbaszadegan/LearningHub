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
        this.handleBrowserHistory();
    }

    handleBrowserHistory() {
        // Handle browser back/forward buttons
        window.addEventListener('popstate', (event) => {
            if (event.state && event.state.page) {
                this.loadPageContent(event.state.page);
            } else {
                // Fallback: reload the page
                window.location.reload();
            }
        });
    }

    detectCurrentPage() {
        const path = window.location.pathname.toLowerCase();
        
        // Check for specific routes first
        if (path.includes('/student/statistics')) {
            return 'statistics';
        }
        if (path.includes('/student/course/catalog')) {
            return 'courses';
        }
        if (path.includes('/student/home') || (path === '/student' || path === '/student/')) {
            return 'home';
        }
        
        // Fallback checks
        if (path.includes('/catalog')) {
            return 'courses';
        }
        if (path.includes('/statistics')) {
            return 'statistics';
        }
        
        // Default to home if in student area
        if (path.includes('/student')) {
            return 'home';
        }
        
        return 'home'; // Default
    }

    bindEvents() {
        // Navigation item clicks
        document.querySelectorAll('.nav-item').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                const page = item.dataset.page;
                
                // Update active state immediately for better UX
                this.setActivePage(page);
                
                // Navigate to page
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
            'statistics': '/Student/Statistics'
        };

        const route = routes[page];
        if (!route) return;

        const currentPath = window.location.pathname.toLowerCase();
        const targetPath = route.toLowerCase();
        
        // Only navigate if we're not already on the target page
        if (currentPath.includes(targetPath.replace('/student/', '').replace('/student', ''))) {
            return;
        }

        // Load content via AJAX
        this.loadPageContent(route);
    }

    async loadPageContent(url) {
        const mainContent = document.querySelector('.main-content');
        if (!mainContent) return;

        // Show loading state
        const originalContent = mainContent.innerHTML;
        mainContent.innerHTML = `
            <div class="page-loading-state">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">در حال بارگذاری...</span>
                </div>
                <p>در حال بارگذاری...</p>
            </div>
        `;

        try {
            // Fetch the page content
            const response = await fetch(url, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Accept': 'text/html'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const html = await response.text();
            
            // Parse the HTML response
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            
            // Extract main content
            const newMainContent = doc.querySelector('.main-content');
            const newContent = newMainContent ? newMainContent.innerHTML : html;

            // Update main content
            mainContent.innerHTML = newContent;

            // Extract and update title
            const newTitle = doc.querySelector('title');
            if (newTitle) {
                document.title = newTitle.textContent;
            }

            // Extract and load stylesheets
            const stylesheets = doc.querySelectorAll('link[rel="stylesheet"]');
            stylesheets.forEach(link => {
                const href = link.getAttribute('href');
                if (href && !document.querySelector(`link[href="${href}"]`)) {
                    const newLink = document.createElement('link');
                    newLink.rel = 'stylesheet';
                    newLink.href = href;
                    document.head.appendChild(newLink);
                }
            });

            // Extract and execute scripts
            const scripts = doc.querySelectorAll('script');
            scripts.forEach(script => {
                if (script.src) {
                    // External script
                    if (!document.querySelector(`script[src="${script.src}"]`)) {
                        const newScript = document.createElement('script');
                        newScript.src = script.src;
                        if (script.hasAttribute('asp-append-version')) {
                            newScript.setAttribute('asp-append-version', 'true');
                        }
                        document.body.appendChild(newScript);
                    }
                } else {
                    // Inline script
                    const newScript = document.createElement('script');
                    newScript.textContent = script.textContent;
                    document.body.appendChild(newScript);
                }
            });

            // Update URL without reload
            window.history.pushState({ page: url }, '', url);

            // Scroll to top
            mainContent.scrollTop = 0;

            // Reinitialize any necessary components
            this.reinitializePage();

        } catch (error) {
            console.error('Error loading page:', error);
            mainContent.innerHTML = `
                <div class="page-error-state">
                    <i class="fas fa-exclamation-triangle text-warning"></i>
                    <h5>خطا در بارگذاری صفحه</h5>
                    <p>متأسفانه خطایی در بارگذاری صفحه رخ داد.</p>
                    <button class="btn btn-primary" onclick="location.reload()">تلاش مجدد</button>
                </div>
            `;
        }
    }

    reinitializePage() {
        // Reinitialize bottom navigation
        this.currentPage = this.detectCurrentPage();
        this.setActiveItem();

        // Reinitialize toastr if needed
        if (typeof toastr !== 'undefined') {
            // Toastr will handle its own initialization
        }

        // Reinitialize any other components that need it
        // This can be extended based on page-specific needs
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
            // Show loading state
            const loadingHtml = `
                <div class="modal fade" id="courseDetailsModal" tabindex="-1" aria-labelledby="courseDetailsModalLabel" aria-hidden="true">
                    <div class="modal-dialog modal-lg modal-dialog-scrollable">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title" id="courseDetailsModalLabel">جزئیات دوره</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <div class="course-loading-state">
                                    <div class="course-loading-spinner">
                                        <div class="spinner-border text-primary" role="status">
                                            <span class="visually-hidden">در حال بارگذاری...</span>
                                        </div>
                                    </div>
                                    <div class="course-loading-text">
                                        <p>در حال بارگذاری جزئیات دوره...</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            // Remove existing modal if any
            const existingModal = document.getElementById('courseDetailsModal');
            if (existingModal) {
                existingModal.remove();
            }
            
            // Add modal to body
            document.body.insertAdjacentHTML('beforeend', loadingHtml);
            
            // Show modal
            const modal = new bootstrap.Modal(document.getElementById('courseDetailsModal'));
            modal.show();
            
            // Load course details via AJAX
            fetch(`/Student/Course/GetCourseDetails/${courseId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('خطا در بارگذاری جزئیات دوره');
                    }
                    return response.text();
                })
                .then(html => {
                    // Update modal body with course details
                    const modalBody = document.querySelector('#courseDetailsModal .modal-body');
                    modalBody.innerHTML = html;
                    
                    // Initialize event handlers for the new content
                    initializeCourseDetailsHandlers();
                })
                .catch(error => {
                    console.error('Error loading course details:', error);
                    const modalBody = document.querySelector('#courseDetailsModal .modal-body');
                    modalBody.innerHTML = `
                        <div class="course-error-state">
                            <div class="course-error-icon">
                                <i class="fas fa-exclamation-triangle text-warning"></i>
                            </div>
                            <div class="course-error-content">
                                <h5>خطا در بارگذاری</h5>
                                <p>متأسفانه خطایی در بارگذاری جزئیات دوره رخ داد.</p>
                                <button type="button" class="btn btn-primary btn-sm" onclick="showCourseDetails(${courseId})">
                                    تلاش مجدد
                                </button>
                            </div>
                        </div>
                    `;
                });
        };
        
        // Initialize course details handlers
        function initializeCourseDetailsHandlers() {
            // Handle enrollment actions
            $('[data-action="enroll"]').off('click').on('click', function() {
                const courseId = $(this).data('course-id');
                enrollInCourse(courseId);
            });
            
            // Handle unenrollment actions
            $('[data-action="unenroll"]').off('click').on('click', function() {
                const courseId = $(this).data('course-id');
                unenrollFromCourse(courseId);
            });
            
            // Handle description toggle - bind directly to the header click
            $('.course-description-header-minimal').off('click').on('click', function() {
                toggleDescriptionMinimal();
            });
        }
        
        // Course enrollment functions
        function enrollInCourse(courseId) {
            if (confirm('آیا می‌خواهید در این دوره ثبت‌نام کنید؟')) {
                $.ajax({
                    url: '/Student/Course/Enroll',
                    type: 'POST',
                    data: { courseId: courseId },
                    success: function(response) {
                        if (response.success) {
                            toastr.success('با موفقیت در دوره ثبت‌نام کردید');
                            // Close modal and reload page
                            const modal = bootstrap.Modal.getInstance(document.getElementById('courseDetailsModal'));
                            if (modal) {
                                modal.hide();
                            }
                            location.reload();
                        } else {
                            toastr.error(response.error || 'خطا در ثبت‌نام');
                        }
                    },
                    error: function() {
                        toastr.error('خطا در ثبت‌نام');
                    }
                });
            }
        }
        
        function unenrollFromCourse(courseId) {
            if (confirm('آیا می‌خواهید از این دوره خارج شوید؟')) {
                $.ajax({
                    url: '/Student/Course/Unenroll',
                    type: 'POST',
                    data: { courseId: courseId },
                    success: function(response) {
                        if (response.success) {
                            toastr.success('با موفقیت از دوره خارج شدید');
                            // Close modal and reload page
                            const modal = bootstrap.Modal.getInstance(document.getElementById('courseDetailsModal'));
                            if (modal) {
                                modal.hide();
                            }
                            location.reload();
                        } else {
                            toastr.error(response.error || 'خطا در خروج از دوره');
                        }
                    },
                    error: function() {
                        toastr.error('خطا در خروج از دوره');
                    }
                });
            }
        }
        
        // Description toggle function
        function toggleDescriptionMinimal() {
            const preview = document.getElementById('descriptionPreviewMinimal');
            const full = document.getElementById('descriptionFullMinimal');
            const toggle = document.getElementById('descriptionToggleMinimal');
            
            if (!preview || !full || !toggle) {
                console.error('Required elements not found');
                return;
            }
            
            // Check if full description is currently visible
            const isFullVisible = full.style.display === 'block' || 
                                 window.getComputedStyle(full).display === 'block';
                     
            
            if (isFullVisible) {
                // Currently showing full, switch to preview
                preview.style.display = 'block';
                full.style.display = 'none';
                toggle.classList.remove('fa-chevron-up');
                toggle.classList.add('fa-chevron-down');
            } else {
                // Currently showing preview, switch to full
                preview.style.display = 'none';
                full.style.display = 'block';
                toggle.classList.remove('fa-chevron-down');
                toggle.classList.add('fa-chevron-up');
            }
        }
        
        // Make function globally available
        window.toggleDescriptionMinimal = toggleDescriptionMinimal;
});

// Export for potential use in other scripts
window.BottomNavigation = BottomNavigation;
