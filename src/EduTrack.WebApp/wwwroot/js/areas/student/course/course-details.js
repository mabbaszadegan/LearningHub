/**
 * Student Course Details JavaScript
 * Handles course enrollment/unenrollment and UI interactions
 */

class StudentCourseDetails {
    constructor() {
        this.init();
    }

    init() {
        this.bindEvents();
        this.initializeCollapsibleSections();
        this.animateProgressBars();
    }

    bindEvents() {
        // Course enrollment/unenrollment
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-action="enroll"]')) {
                e.preventDefault();
                const courseId = e.target.closest('[data-action="enroll"]').dataset.courseId;
                this.enrollInCourse(courseId);
            }
            
            if (e.target.closest('[data-action="unenroll"]')) {
                e.preventDefault();
                const courseId = e.target.closest('[data-action="unenroll"]').dataset.courseId;
                this.unenrollFromCourse(courseId);
            }
        });

        // Chapter toggle
        document.addEventListener('click', (e) => {
            if (e.target.closest('.chapter-header')) {
                e.preventDefault();
                const chapterHeader = e.target.closest('.chapter-header');
                this.toggleChapter(chapterHeader);
            }
        });

        // Description toggle
        document.addEventListener('click', (e) => {
            if (e.target.closest('.course-description-header')) {
                e.preventDefault();
                const descriptionHeader = e.target.closest('.course-description-header');
                this.toggleDescription(descriptionHeader);
            }
        });
    }

    initializeCollapsibleSections() {
        // Initialize all chapters as collapsed
        document.querySelectorAll('.chapter-content').forEach(content => {
            content.style.maxHeight = '0';
        });

        // Initialize description as collapsed
        const descriptionContent = document.querySelector('.course-description-content');
        if (descriptionContent) {
            const preview = descriptionContent.querySelector('.course-description-preview');
            const full = descriptionContent.querySelector('.course-description-full');
            
            if (preview && full) {
                full.style.display = 'none';
                preview.style.display = 'block';
            }
        }
    }

    toggleChapter(chapterHeader) {
        const chapterContent = chapterHeader.nextElementSibling;
        const toggle = chapterHeader.querySelector('.chapter-toggle');
        
        if (chapterContent.classList.contains('expanded')) {
            // Collapse
            chapterContent.classList.remove('expanded');
            chapterContent.style.maxHeight = '0';
            toggle.classList.remove('expanded');
        } else {
            // Expand
            chapterContent.classList.add('expanded');
            chapterContent.style.maxHeight = chapterContent.scrollHeight + 'px';
            toggle.classList.add('expanded');
        }
    }

    toggleDescription(descriptionHeader) {
        const descriptionContent = descriptionHeader.nextElementSibling;
        const toggle = descriptionHeader.querySelector('.course-description-toggle');
        const preview = descriptionContent.querySelector('.course-description-preview');
        const full = descriptionContent.querySelector('.course-description-full');
        
        if (full.classList.contains('show')) {
            // Collapse
            full.classList.remove('show');
            full.style.display = 'none';
            preview.style.display = 'block';
            toggle.classList.remove('expanded');
            toggle.textContent = 'مشاهده بیشتر';
        } else {
            // Expand
            full.classList.add('show');
            full.style.display = 'block';
            preview.style.display = 'none';
            toggle.classList.add('expanded');
            toggle.textContent = 'مشاهده کمتر';
        }
    }

    enrollInCourse(courseId) {
        if (!this.showConfirmationDialog('آیا مطمئن هستید که می‌خواهید در این دوره ثبت‌نام کنید؟')) {
            return;
        }

        const button = document.querySelector(`[data-action="enroll"][data-course-id="${courseId}"]`);
        if (button) {
            button.classList.add('loading');
            button.disabled = true;
        }

        this.makeEnrollRequest(courseId)
            .then(response => {
                if (response.success) {
                    this.showSuccessMessage(response.message);
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    this.showErrorMessage(response.error || 'خطا در ثبت‌نام');
                }
            })
            .catch(error => {
                console.error('Enrollment error:', error);
                this.showErrorMessage('خطا در ارتباط با سرور');
            })
            .finally(() => {
                if (button) {
                    button.classList.remove('loading');
                    button.disabled = false;
                }
            });
    }

    unenrollFromCourse(courseId) {
        if (!this.showConfirmationDialog('آیا مطمئن هستید که می‌خواهید از این دوره خارج شوید؟\n\nاین عمل قابل بازگشت نیست.')) {
            return;
        }

        const button = document.querySelector(`[data-action="unenroll"][data-course-id="${courseId}"]`);
        if (button) {
            button.classList.add('loading');
            button.disabled = true;
        }

        this.makeUnenrollRequest(courseId)
            .then(response => {
                if (response.success) {
                    this.showSuccessMessage(response.message);
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    this.showErrorMessage(response.error || 'خطا در خارج شدن از دوره');
                }
            })
            .catch(error => {
                console.error('Unenrollment error:', error);
                this.showErrorMessage('خطا در ارتباط با سرور');
            })
            .finally(() => {
                if (button) {
                    button.classList.remove('loading');
                    button.disabled = false;
                }
            });
    }

    showConfirmationDialog(message) {
        return confirm(message);
    }

    makeEnrollRequest(courseId) {
        return fetch('/Student/Course/Enroll', {
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

    animateProgressBars() {
        // Animate progress bars on page load
        document.querySelectorAll('.progress-bar-fill').forEach(bar => {
            const width = bar.getAttribute('data-width') || bar.style.width;
            bar.style.width = '0%';
            
            setTimeout(() => {
                bar.style.width = width;
            }, 500);
        });
    }

    // Utility method to format numbers
    formatNumber(num) {
        return new Intl.NumberFormat('fa-IR').format(num);
    }

    // Utility method to format percentages
    formatPercentage(num) {
        return `${this.formatNumber(num)}%`;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new StudentCourseDetails();
});

// Export for potential use in other scripts
window.StudentCourseDetails = StudentCourseDetails;
