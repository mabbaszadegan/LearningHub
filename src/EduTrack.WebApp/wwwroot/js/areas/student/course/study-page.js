/**
 * Study Page JavaScript
 * Manages sidebar, filtering, and interactions for the course study page
 */

class StudyPage {
    constructor() {
        this.config = window.studyPageConfig || {};
        this.scheduleItems = this.config.scheduleItems || [];
        this.chapters = this.config.chapters || [];
        this.selectedChapterId = null;
        this.selectedSubChapterId = null;
        this.selectedTypes = [];
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.initializeFilters();
        this.hideBottomMenu();
    }

    bindEvents() {
        // Hamburger menu toggle
        const hamburgerBtn = document.getElementById('studyHamburgerBtn');
        const sidebarClose = document.getElementById('studySidebarClose');
        const sidebarOverlay = document.getElementById('studySidebarOverlay');
        const sidebar = document.getElementById('studySidebar');

        if (hamburgerBtn) {
            hamburgerBtn.addEventListener('click', () => this.toggleSidebar());
        }

        if (sidebarClose) {
            sidebarClose.addEventListener('click', () => this.closeSidebar());
        }

        if (sidebarOverlay) {
            sidebarOverlay.addEventListener('click', () => this.closeSidebar());
        }

        // Chapter/SubChapter selection
        document.querySelectorAll('.study-chapter-header').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const chapterId = parseInt(btn.dataset.chapterId);
                this.toggleChapter(chapterId);
            });
        });

        document.querySelectorAll('.study-subchapter-item').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const subChapterId = parseInt(btn.dataset.subchapterId);
                const chapterId = parseInt(btn.dataset.chapterId);
                this.selectSubChapter(subChapterId, chapterId);
            });
        });

        // All topics filter
        document.querySelectorAll('.study-filter-item-all').forEach(btn => {
            btn.addEventListener('click', () => {
                this.clearFilters();
            });
        });

        // Type filter (in header)
        document.querySelectorAll('.study-type-filter-item-header').forEach(btn => {
            btn.addEventListener('click', () => {
                const type = btn.dataset.type;
                this.toggleTypeFilter(type);
            });
        });
    }

    toggleSidebar() {
        const sidebar = document.getElementById('studySidebar');
        const overlay = document.getElementById('studySidebarOverlay');
        
        if (sidebar && overlay) {
            const isActive = sidebar.classList.contains('active');
            sidebar.classList.toggle('active');
            overlay.classList.toggle('active');
            // Only prevent scrolling on body when sidebar is open on mobile
            if (window.innerWidth <= 768) {
                if (isActive) {
                    document.body.style.overflow = '';
                } else {
                    document.body.style.overflow = 'hidden';
                }
            }
        }
    }

    closeSidebar() {
        const sidebar = document.getElementById('studySidebar');
        const overlay = document.getElementById('studySidebarOverlay');
        
        if (sidebar && overlay) {
            sidebar.classList.remove('active');
            overlay.classList.remove('active');
            if (window.innerWidth <= 768) {
                document.body.style.overflow = '';
            }
        }
    }

    toggleChapter(chapterId) {
        const chapterItem = document.querySelector(`.study-chapter-item[data-chapter-id="${chapterId}"]`);
        const chapterHeader = document.querySelector(`.study-chapter-header[data-chapter-id="${chapterId}"]`);
        
        if (!chapterItem || !chapterHeader) return;

        const isActive = chapterItem.classList.contains('active');
        
        // Close all chapters
        document.querySelectorAll('.study-chapter-item').forEach(item => {
            item.classList.remove('active');
        });
        
        document.querySelectorAll('.study-chapter-header').forEach(header => {
            header.classList.remove('active');
        });

        // Toggle current chapter
        if (!isActive) {
            chapterItem.classList.add('active');
            chapterHeader.classList.add('active');
        }

        // Select chapter for filtering
        if (!isActive) {
            this.selectChapter(chapterId);
        } else {
            this.clearFilters();
        }
    }

    selectChapter(chapterId) {
        this.selectedChapterId = chapterId;
        this.selectedSubChapterId = null;
        this.updateFilterButtons();
        this.filterItems();
    }

    selectSubChapter(subChapterId, chapterId) {
        this.selectedSubChapterId = subChapterId;
        this.selectedChapterId = chapterId;
        this.updateFilterButtons();
        this.filterItems();
        
        // Close sidebar on mobile after selection
        if (window.innerWidth <= 768) {
            this.closeSidebar();
        }
    }

    clearFilters() {
        this.selectedChapterId = null;
        this.selectedSubChapterId = null;
        this.updateFilterButtons();
        this.filterItems();
    }

    toggleTypeFilter(type) {
        const index = this.selectedTypes.indexOf(type);
        if (index > -1) {
            this.selectedTypes.splice(index, 1);
        } else {
            this.selectedTypes.push(type);
        }
        this.updateFilterButtons();
        this.filterItems();
    }

    updateFilterButtons() {
        // Update "All" button
        document.querySelectorAll('.study-filter-item-all').forEach(btn => {
            if (!this.selectedChapterId && !this.selectedSubChapterId) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });

        // Update chapter buttons
        document.querySelectorAll('.study-chapter-header').forEach(btn => {
            const chapterId = parseInt(btn.dataset.chapterId);
            if (this.selectedChapterId === chapterId && !this.selectedSubChapterId) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });

        // Update subchapter buttons
        document.querySelectorAll('.study-subchapter-item').forEach(btn => {
            const subChapterId = parseInt(btn.dataset.subchapterId);
            if (this.selectedSubChapterId === subChapterId) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });

        // Update type filter buttons (in header)
        document.querySelectorAll('.study-type-filter-item-header').forEach(btn => {
            const type = btn.dataset.type;
            if (this.selectedTypes.includes(type)) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });
    }

    filterItems() {
        const items = document.querySelectorAll('.study-item-card');
        let visibleCount = 0;

        items.forEach(item => {
            let shouldShow = true;

            // Filter by chapter/subchapter
            if (this.selectedSubChapterId) {
                const subChapterIds = item.dataset.subchapterIds ? 
                    item.dataset.subchapterIds.split(',').map(id => parseInt(id.trim())) : [];
                shouldShow = shouldShow && subChapterIds.includes(this.selectedSubChapterId);
            } else if (this.selectedChapterId) {
                const chapter = this.chapters.find(c => c.id === this.selectedChapterId);
                if (chapter && chapter.subChapterIds) {
                    const subChapterIds = item.dataset.subchapterIds ? 
                        item.dataset.subchapterIds.split(',').map(id => parseInt(id.trim())) : [];
                    const chapterSubChapterIds = chapter.subChapterIds || [];
                    shouldShow = shouldShow && subChapterIds.some(id => chapterSubChapterIds.includes(id));
                } else {
                    shouldShow = false;
                }
            }

            // Filter by type - only apply if types are selected
            if (this.selectedTypes.length > 0) {
                const itemType = item.dataset.itemType;
                if (!itemType || !this.selectedTypes.includes(itemType)) {
                    shouldShow = false;
                }
            }

            if (shouldShow) {
                item.classList.remove('hidden');
                visibleCount++;
            } else {
                item.classList.add('hidden');
            }
        });

        // Show/hide empty state
        const emptyState = document.querySelector('.study-empty-state');
        const itemsGrid = document.getElementById('studyItemsGrid');
        
        if (visibleCount === 0 && items.length > 0) {
            if (!emptyState) {
                this.createEmptyState(itemsGrid);
            }
        } else {
            if (emptyState) {
                emptyState.remove();
            }
        }
    }

    createEmptyState(container) {
        const emptyState = document.createElement('div');
        emptyState.className = 'study-empty-state';
        emptyState.innerHTML = `
            <i class="fas fa-filter"></i>
            <h3>نتیجه‌ای یافت نشد</h3>
            <p>با فیلترهای انتخابی آیتمی پیدا نشد</p>
        `;
        container.appendChild(emptyState);
    }

    initializeFilters() {
        // Initialize chapter states
        this.chapters.forEach(chapter => {
            const chapterItem = document.querySelector(`.study-chapter-item[data-chapter-id="${chapter.id}"]`);
            if (chapterItem) {
                // Check if chapter has subchapters
                const hasSubChapters = chapter.subChapterIds && chapter.subChapterIds.length > 0;
                if (!hasSubChapters) {
                    // If no subchapters, make chapter clickable directly
                    const chapterHeader = chapterItem.querySelector('.study-chapter-header');
                    if (chapterHeader) {
                        chapterHeader.style.cursor = 'pointer';
                        chapterHeader.addEventListener('click', (e) => {
                            if (e.target.closest('.study-subchapter-item')) return;
                            this.selectChapter(chapter.id);
                        });
                    }
                }
            }
        });
    }

    hideBottomMenu() {
        // Add class to body to hide bottom menu and headers
        const body = document.body;
        if (!body.classList.contains('study-page-active')) {
            body.classList.add('study-page-active');
        }
        
        // Also hide directly using JavaScript as fallback
        const fixedFooter = document.querySelector('.fixed-footer');
        if (fixedFooter) {
            fixedFooter.style.display = 'none';
        }

        const fixedHeader = document.querySelector('.fixed-header');
        if (fixedHeader) {
            fixedHeader.style.display = 'none';
        }

        const desktopHeader = document.querySelector('.desktop-header');
        if (desktopHeader) {
            desktopHeader.style.display = 'none';
        }

        // Override main-content styles
        const mainContent = document.querySelector('.main-content');
        if (mainContent) {
            mainContent.style.position = 'static';
            mainContent.style.top = 'auto';
            mainContent.style.bottom = 'auto';
            mainContent.style.height = 'auto';
            mainContent.style.overflow = 'visible';
            mainContent.style.padding = '0';
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector('.study-page-container')) {
        new StudyPage();
        
        // Ensure overlay doesn't block interactions when sidebar is closed
        const overlay = document.getElementById('studySidebarOverlay');
        if (overlay && !overlay.classList.contains('active')) {
            overlay.style.pointerEvents = 'none';
        }
    }
});

// Handle window resize to close sidebar on mobile when switching to desktop
window.addEventListener('resize', () => {
    if (window.innerWidth > 768) {
        const studyPage = document.querySelector('.study-page-container');
        if (studyPage) {
            const sidebar = document.getElementById('studySidebar');
            const overlay = document.getElementById('studySidebarOverlay');
            if (sidebar) sidebar.classList.remove('active');
            if (overlay) {
                overlay.classList.remove('active');
                overlay.style.pointerEvents = 'none';
            }
            document.body.style.overflow = '';
        }
    }
});
