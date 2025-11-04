// Home Page JavaScript
(function() {
    'use strict';

    document.addEventListener('DOMContentLoaded', function() {
        // Initialize bottom navigation
        if (window.bottomNavigation) {
            window.bottomNavigation.setActivePage('home');
        }

        // Add smooth scroll behavior for recently viewed courses
        const recentlyViewedList = document.querySelector('.recently-viewed-list');
        if (recentlyViewedList) {
            // Enable smooth scrolling with touch support
            let isDown = false;
            let startX;
            let scrollLeft;

            recentlyViewedList.addEventListener('mousedown', (e) => {
                isDown = true;
                recentlyViewedList.style.cursor = 'grabbing';
                startX = e.pageX - recentlyViewedList.offsetLeft;
                scrollLeft = recentlyViewedList.scrollLeft;
            });

            recentlyViewedList.addEventListener('mouseleave', () => {
                isDown = false;
                recentlyViewedList.style.cursor = 'grab';
            });

            recentlyViewedList.addEventListener('mouseup', () => {
                isDown = false;
                recentlyViewedList.style.cursor = 'grab';
            });

            recentlyViewedList.addEventListener('mousemove', (e) => {
                if (!isDown) return;
                e.preventDefault();
                const x = e.pageX - recentlyViewedList.offsetLeft;
                const walk = (x - startX) * 2;
                recentlyViewedList.scrollLeft = scrollLeft - walk;
            });
        }

        // Add hover effects for enrolled course rows
        const enrolledCourseRows = document.querySelectorAll('.enrolled-course-row');
        enrolledCourseRows.forEach(row => {
            row.addEventListener('mouseenter', function() {
                this.style.transform = 'translateX(-2px)';
            });

            row.addEventListener('mouseleave', function() {
                this.style.transform = 'translateX(0)';
            });
        });

        // Prevent default link behavior on empty states
        const emptyStateLinks = document.querySelectorAll('.empty-state-minimal a');
        emptyStateLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                // Allow normal link navigation
                return true;
            });
        });
    });
})();

