/**
 * Student Course List JavaScript
 * Handles course enrollment/unenrollment and UI interactions
 */

class StudentCourseList {
    constructor() {
        this.init();
    }

    init() {
        this.bindEvents();
        this.initializeTooltips();
        this.setupIntersectionObserver();
    }

    bindEvents() {
        // Unenroll course functionality
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action="unenroll"]')) {
                e.preventDefault();
                const courseId = e.target.closest('[data-action="unenroll"]').dataset.courseId;
                this.unenrollCourse(courseId);
            }
        });

        // Course card hover effects
        document.querySelectorAll('.course-card').forEach(card => {
            card.addEventListener('mouseenter', this.handleCardHover.bind(this));
            card.addEventListener('mouseleave', this.handleCardLeave.bind(this));
        });

        // Mobile touch events
        if ('ontouchstart' in window) {
            document.querySelectorAll('.course-card').forEach(card => {
                card.addEventListener('touchstart', this.handleTouchStart.bind(this));
                card.addEventListener('touchend', this.handleTouchEnd.bind(this));
            });
        }
    }

    initializeTooltips() {
        // Initialize Bootstrap tooltips if available
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    }

    setupIntersectionObserver() {
        // Lazy loading for course cards
        if ('IntersectionObserver' in window) {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('animate-in');
                        observer.unobserve(entry.target);
                    }
                });
            }, {
                threshold: 0.1,
                rootMargin: '50px'
            });

            document.querySelectorAll('.course-card').forEach(card => {
                observer.observe(card);
            });
        }
    }

    handleCardHover(e) {
        const card = e.currentTarget;
        card.style.transform = 'translateY(-8px)';
        card.style.boxShadow = '0 12px 40px rgba(0, 0, 0, 0.15)';
    }

    handleCardLeave(e) {
        const card = e.currentTarget;
        card.style.transform = 'translateY(0)';
        card.style.boxShadow = '0 4px 20px rgba(0, 0, 0, 0.08)';
    }

    handleTouchStart(e) {
        const card = e.currentTarget;
        card.style.transform = 'scale(0.98)';
    }

    handleTouchEnd(e) {
        const card = e.currentTarget;
        card.style.transform = 'scale(1)';
    }

    unenrollCourse(courseId) {
        // Show confirmation dialog
        if (!this.showConfirmationDialog()) {
            return;
        }

        // Find the course card and show loading state
        const courseCard = document.querySelector(`[data-course-id="${courseId}"]`);
        if (courseCard) {
            courseCard.classList.add('loading');
        }

        // Make AJAX request
        this.makeUnenrollRequest(courseId)
            .then(response => {
                if (response.success) {
                    this.showSuccessMessage(response.message);
                    this.removeCourseCard(courseId);
                } else {
                    this.showErrorMessage(response.error || 'خطا در خارج شدن از دوره');
                }
            })
            .catch(error => {
                console.error('Unenroll error:', error);
                this.showErrorMessage('خطا در ارتباط با سرور');
            })
            .finally(() => {
                if (courseCard) {
                    courseCard.classList.remove('loading');
                }
            });
    }

    showConfirmationDialog() {
        return confirm('آیا مطمئن هستید که می‌خواهید از این دوره خارج شوید؟\n\nاین عمل قابل بازگشت نیست.');
    }

    makeUnenrollRequest(courseId) {
        return fetch('/Student/Course/Unenroll', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: `courseId=${courseId}`
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        });
    }

    showSuccessMessage(message) {
        this.showToast(message, 'success');
    }

    showErrorMessage(message) {
        this.showToast(message, 'error');
    }

    showToast(message, type = 'info') {
        // Create toast element
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle'}"></i>
                <span>${message}</span>
            </div>
        `;

        // Add styles
        Object.assign(toast.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            background: type === 'success' ? '#28a745' : '#dc3545',
            color: 'white',
            padding: '12px 20px',
            borderRadius: '8px',
            boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
            zIndex: '9999',
            transform: 'translateX(100%)',
            transition: 'transform 0.3s ease',
            maxWidth: '300px',
            fontSize: '14px',
            fontWeight: '500'
        });

        // Add to page
        document.body.appendChild(toast);

        // Animate in
        setTimeout(() => {
            toast.style.transform = 'translateX(0)';
        }, 100);

        // Remove after delay
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, 3000);
    }

    removeCourseCard(courseId) {
        const courseCard = document.querySelector(`[data-course-id="${courseId}"]`);
        if (courseCard) {
            courseCard.style.transform = 'scale(0.8)';
            courseCard.style.opacity = '0';
            
            setTimeout(() => {
                courseCard.remove();
                this.checkEmptyState();
            }, 300);
        }
    }

    checkEmptyState() {
        const courseCards = document.querySelectorAll('.course-card');
        if (courseCards.length === 0) {
            this.showEmptyState();
        }
    }

    showEmptyState() {
        const container = document.querySelector('.course-list-container .row');
        if (container) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="text-center py-5">
                        <div class="empty-state">
                            <i class="fas fa-book fa-4x text-muted mb-4"></i>
                            <h4 class="text-muted mb-3">هنوز در دوره‌ای ثبت‌نام نکرده‌اید</h4>
                            <p class="text-muted mb-4">برای شروع یادگیری، در یکی از دوره‌های موجود ثبت‌نام کنید</p>
                            <a href="/Student/Course/Catalog" class="btn btn-modern-primary">
                                <i class="fas fa-search me-2"></i>مشاهده کاتالوگ دوره‌ها
                            </a>
                        </div>
                    </div>
                </div>
            `;
        }
    }

    // Utility method to format numbers
    formatNumber(num) {
        return new Intl.NumberFormat('fa-IR').format(num);
    }

    // Utility method to format percentages
    formatPercentage(num) {
        return `${this.formatNumber(num)}%`;
    }

    // Method to update progress bar animation
    animateProgressBars() {
        document.querySelectorAll('.progress-bar').forEach(bar => {
            const width = bar.getAttribute('aria-valuenow');
            bar.style.width = '0%';
            
            setTimeout(() => {
                bar.style.width = `${width}%`;
            }, 100);
        });
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new StudentCourseList();
});

// Export for potential use in other scripts
window.StudentCourseList = StudentCourseList;
