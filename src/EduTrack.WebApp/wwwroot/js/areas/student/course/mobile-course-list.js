// Mobile Course List - Search and Filter Functionality

(function () {
    'use strict';

    const searchInput = document.getElementById('courseSearchInput');
    const searchClearBtn = document.getElementById('searchClearBtn');
    const courseGroupsContainer = document.getElementById('courseGroupsContainer');
    const emptySearchState = document.getElementById('emptySearchState');
    const courseTiles = document.querySelectorAll('.course-tile');

    if (!searchInput || !courseGroupsContainer) {
        return; // Elements not found, likely on desktop
    }

    // Search functionality
    searchInput.addEventListener('input', function () {
        const searchTerm = this.value.trim().toLowerCase();
        
        if (searchTerm.length === 0) {
            // Show all courses and groups
            courseTiles.forEach(tile => {
                tile.classList.remove('hidden');
            });
            
            document.querySelectorAll('.course-group').forEach(group => {
                const visibleTiles = group.querySelectorAll('.course-tile:not(.hidden)');
                if (visibleTiles.length === 0) {
                    group.classList.add('hidden');
                } else {
                    group.classList.remove('hidden');
                }
            });

            searchClearBtn.style.display = 'none';
            emptySearchState.style.display = 'none';
            courseGroupsContainer.style.display = 'flex';
        } else {
            // Filter courses
            let hasVisibleCourses = false;

            courseTiles.forEach(tile => {
                const courseTitle = tile.getAttribute('data-course-title') || '';
                if (courseTitle.includes(searchTerm)) {
                    tile.classList.remove('hidden');
                    hasVisibleCourses = true;
                } else {
                    tile.classList.add('hidden');
                }
            });

            // Hide/show groups based on visible courses
            document.querySelectorAll('.course-group').forEach(group => {
                const visibleTiles = group.querySelectorAll('.course-tile:not(.hidden)');
                if (visibleTiles.length === 0) {
                    group.classList.add('hidden');
                } else {
                    group.classList.remove('hidden');
                    hasVisibleCourses = true;
                }
            });

            // Show/hide empty state
            if (!hasVisibleCourses) {
                emptySearchState.style.display = 'block';
                courseGroupsContainer.style.display = 'none';
            } else {
                emptySearchState.style.display = 'none';
                courseGroupsContainer.style.display = 'flex';
            }

            searchClearBtn.style.display = 'flex';
        }
    });

    // Clear search
    if (searchClearBtn) {
        searchClearBtn.addEventListener('click', function () {
            searchInput.value = '';
            searchInput.dispatchEvent(new Event('input'));
            searchInput.focus();
        });
    }

    // Smooth scroll for course tiles on mobile
    const courseTilesScrolls = document.querySelectorAll('.course-tiles-scroll');
    courseTilesScrolls.forEach(scrollContainer => {
        let isDown = false;
        let startX;
        let scrollLeft;

        scrollContainer.addEventListener('mousedown', (e) => {
            isDown = true;
            scrollContainer.style.cursor = 'grabbing';
            startX = e.pageX - scrollContainer.offsetLeft;
            scrollLeft = scrollContainer.scrollLeft;
        });

        scrollContainer.addEventListener('mouseleave', () => {
            isDown = false;
            scrollContainer.style.cursor = 'grab';
        });

        scrollContainer.addEventListener('mouseup', () => {
            isDown = false;
            scrollContainer.style.cursor = 'grab';
        });

        scrollContainer.addEventListener('mousemove', (e) => {
            if (!isDown) return;
            e.preventDefault();
            const x = e.pageX - scrollContainer.offsetLeft;
            const walk = (x - startX) * 2;
            scrollContainer.scrollLeft = scrollLeft - walk;
        });
    });

    // Prevent link click when scrolling on mobile
    let touchStartX = 0;
    let touchStartY = 0;

    courseTiles.forEach(tile => {
        const link = tile.querySelector('.tile-link');
        
        tile.addEventListener('touchstart', (e) => {
            touchStartX = e.touches[0].clientX;
            touchStartY = e.touches[0].clientY;
        });

        tile.addEventListener('touchend', (e) => {
            const touchEndX = e.changedTouches[0].clientX;
            const touchEndY = e.changedTouches[0].clientY;
            const diffX = Math.abs(touchStartX - touchEndX);
            const diffY = Math.abs(touchStartY - touchEndY);

            // If horizontal scroll is significant, prevent link click
            if (diffX > 10 && diffX > diffY) {
                e.preventDefault();
                return false;
            }
        }, { passive: false });
    });

    // Handle enrollment actions for catalog page
    const enrollButtons = document.querySelectorAll('[data-action="enroll"]');
    enrollButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.stopPropagation();
            const courseId = this.getAttribute('data-course-id');
            if (courseId && confirm('آیا می‌خواهید در این دوره ثبت‌نام کنید؟')) {
                enrollInCourse(courseId);
            }
        });
    });

    function enrollInCourse(courseId) {
        fetch('/Student/Course/Enroll', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: `courseId=${courseId}`
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                if (typeof toastr !== 'undefined') {
                    toastr.success('با موفقیت در دوره ثبت‌نام کردید');
                } else {
                    alert('با موفقیت در دوره ثبت‌نام کردید');
                }
                setTimeout(() => {
                    location.reload();
                }, 1000);
            } else {
                if (typeof toastr !== 'undefined') {
                    toastr.error(data.error || 'خطا در ثبت‌نام');
                } else {
                    alert(data.error || 'خطا در ثبت‌نام');
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            if (typeof toastr !== 'undefined') {
                toastr.error('خطا در ثبت‌نام');
            } else {
                alert('خطا در ثبت‌نام');
            }
        });
    }
})();
