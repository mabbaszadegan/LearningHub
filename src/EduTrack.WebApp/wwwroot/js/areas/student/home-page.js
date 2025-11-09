// Home Page JavaScript
(function() {
    'use strict';

    const DashboardLoader = {
        init() {
            if (window.bottomNavigation) {
                window.bottomNavigation.setActivePage('home');
            }

            this.observeSections();
        },

        observeSections() {
            const sections = document.querySelectorAll('[data-dashboard-endpoint]');
            if (!sections.length) {
                return;
            }

            if ('IntersectionObserver' in window) {
                const observer = new IntersectionObserver((entries, obs) => {
                    entries.forEach(entry => {
                        if (entry.isIntersecting) {
                            this.loadSection(entry.target);
                            obs.unobserve(entry.target);
                        }
                    });
                }, {
                    rootMargin: '200px 0px',
                    threshold: 0.1
                });

                sections.forEach(section => observer.observe(section));
            } else {
                sections.forEach(section => this.loadSection(section));
            }
        },

        async loadSection(section) {
            if (!section || section.dataset.dashboardLoaded === 'true') {
                return;
            }

            const endpoint = section.dataset.dashboardEndpoint;
            if (!endpoint) {
                return;
            }

            section.dataset.dashboardLoaded = 'loading';

            try {
                const response = await fetch(endpoint, {
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (!response.ok) {
                    throw new Error(`Request failed with status ${response.status}`);
                }

                const html = await response.text();
                section.innerHTML = html;
                section.dataset.dashboardLoaded = 'true';
                this.attachInteractions(section);
            } catch (error) {
                console.error('Failed to load dashboard section', error);
                this.renderError(section, 'بارگذاری این بخش با خطا مواجه شد. دوباره تلاش کنید.');
            }
        },

        renderError(section, message) {
            section.dataset.dashboardLoaded = 'false';
            section.innerHTML = `
                <div class="dashboard-section-message dashboard-section-error" data-dashboard-error>
                    <i class="fas fa-exclamation-circle"></i>
                    <span>${message}</span>
                    <button type="button" class="btn btn-link btn-sm" data-action="retry-section">تلاش مجدد</button>
                </div>
            `;

            if (!section.dataset.retryBound) {
                section.addEventListener('click', (event) => {
                    if (event.target && event.target.matches('[data-action="retry-section"]')) {
                        event.preventDefault();
                        this.loadSection(section);
                    }
                });
                section.dataset.retryBound = 'true';
            }
        },

        attachInteractions(section) {
            this.setupRecentCoursesScroll(section);
            this.setupEnrolledCoursesHover(section);
        },

        setupRecentCoursesScroll(section) {
            const list = section.querySelector('.recently-viewed-list');
            if (!list) {
                return;
            }

            list.style.cursor = 'grab';
            let isDown = false;
            let startX = 0;
            let scrollLeft = 0;

            list.addEventListener('mousedown', (event) => {
                isDown = true;
                list.style.cursor = 'grabbing';
                startX = event.pageX - list.offsetLeft;
                scrollLeft = list.scrollLeft;
            });

            list.addEventListener('mouseleave', () => {
                isDown = false;
                list.style.cursor = 'grab';
            });

            list.addEventListener('mouseup', () => {
                isDown = false;
                list.style.cursor = 'grab';
            });

            list.addEventListener('mousemove', (event) => {
                if (!isDown) {
                    return;
                }
                event.preventDefault();
                const x = event.pageX - list.offsetLeft;
                const walk = (x - startX) * 2;
                list.scrollLeft = scrollLeft - walk;
            });
        },

        setupEnrolledCoursesHover(section) {
            const rows = section.querySelectorAll('.enrolled-course-row');
            rows.forEach(row => {
                row.addEventListener('mouseenter', function() {
                    this.style.transform = 'translateX(-2px)';
                });
                row.addEventListener('mouseleave', function() {
                    this.style.transform = 'translateX(0)';
                });
            });
        }
    };

    document.addEventListener('DOMContentLoaded', () => DashboardLoader.init());
})();

