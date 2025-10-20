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
                                <div class="text-center py-4">
                                    <div class="spinner-border text-primary" role="status">
                                        <span class="visually-hidden">در حال بارگذاری...</span>
                                    </div>
                                    <p class="mt-2">در حال بارگذاری جزئیات دوره...</p>
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
                        <div class="text-center py-4">
                            <i class="fas fa-exclamation-triangle text-warning fa-3x mb-3"></i>
                            <h5>خطا در بارگذاری</h5>
                            <p>متأسفانه خطایی در بارگذاری جزئیات دوره رخ داد.</p>
                            <button type="button" class="btn btn-primary" onclick="showCourseDetails(${courseId})">
                                تلاش مجدد
                            </button>
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
            
            // Handle description toggle
            if (typeof toggleDescriptionMinimal === 'function') {
                // Re-bind the toggle function if it exists
                const toggleBtn = document.getElementById('descriptionToggleMinimal');
                if (toggleBtn) {
                    toggleBtn.onclick = toggleDescriptionMinimal;
                }
            }
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
            
            if (full.style.display === 'none') {
                preview.style.display = 'none';
                full.style.display = 'block';
                toggle.classList.remove('fa-chevron-down');
                toggle.classList.add('fa-chevron-up');
            } else {
                preview.style.display = 'block';
                full.style.display = 'none';
                toggle.classList.remove('fa-chevron-up');
                toggle.classList.add('fa-chevron-down');
            }
        }
});

// Export for potential use in other scripts
window.BottomNavigation = BottomNavigation;
