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
        this.selectedCategory = null;
        this.handleViewportResize = this.updateViewportHeightVar.bind(this);
        this.teardownHandler = this.restoreLayoutAdjustments.bind(this);
        this.layoutAdjusted = false;
        
        this.init();
    }

    init() {
        this.updateViewportHeightVar();
        window.addEventListener('resize', this.handleViewportResize);
        window.addEventListener('orientationchange', this.handleViewportResize);
        if (window.visualViewport) {
            window.visualViewport.addEventListener('resize', this.handleViewportResize);
            window.visualViewport.addEventListener('scroll', this.handleViewportResize);
        }
        window.addEventListener('beforeunload', this.teardownHandler);
        window.addEventListener('pagehide', this.teardownHandler);
        this.bindEvents();
        this.initializeCategoryTabs();
        this.initializeFilters();
        this.initializeSorting();
        this.hideBottomMenu();
        this.handleReturnFromStudy();
    }
    
    handleReturnFromStudy() {
        // Check if returning from study page
        const urlParams = new URLSearchParams(window.location.search);
        const itemId = urlParams.get('itemId');
        
        if (itemId) {
            // Remove itemId from URL
            urlParams.delete('itemId');
            const newUrl = window.location.pathname + (urlParams.toString() ? '?' + urlParams.toString() : '');
            window.history.replaceState({}, '', newUrl);
            
            // Scroll to and highlight the item
            setTimeout(() => {
                const itemCard = document.querySelector(`.study-item-card[data-item-id="${itemId}"]`);
                if (itemCard) {
                    itemCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    itemCard.classList.add('selected');
                    setTimeout(() => {
                        itemCard.classList.remove('selected');
                    }, 2000);
                }
                
                // Refresh statistics - page will reload with updated stats from server
            }, 100);
        }
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

        // Category tabs
        document.querySelectorAll('.study-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                const category = tab.dataset.category;
                if (category) {
                    this.selectCategory(category);
                }
            });
        });

        // All topics filter
        document.querySelectorAll('.study-filter-item-all').forEach(btn => {
            btn.addEventListener('click', () => {
                this.clearFilters();
                // Close sidebar on mobile when clicking "All topics"
                if (window.innerWidth <= 768) {
                    this.closeSidebar();
                }
            });
        });

        // Sort control
        const sortSelect = document.getElementById('sortBy');
        if (sortSelect) {
            sortSelect.addEventListener('change', (e) => {
                this.sortItems(e.target.value);
            });
        }
    }
    
    initializeSorting() {
        // Initial sort by last study
        this.sortItems('lastStudy');
    }
    
    sortItems(sortBy) {
        const grid = document.getElementById('studyItemsGrid');
        if (!grid) return;
        
        const cards = Array.from(grid.querySelectorAll('.study-item-card:not(.hidden)'));
        
        cards.sort((a, b) => {
            switch (sortBy) {
                case 'studyTime':
                    const timeA = parseInt(a.dataset.studyTime || '0');
                    const timeB = parseInt(b.dataset.studyTime || '0');
                    return timeB - timeA; // Descending
                    
                case 'lastStudy':
                    const lastA = parseInt(a.dataset.lastStudy || '0');
                    const lastB = parseInt(b.dataset.lastStudy || '0');
                    // Items with 0 (no study) should go to the end
                    if (lastA === 0 && lastB === 0) {
                        // If both have no study, sort by creation date
                        const createdA = parseInt(a.dataset.createdAt || '0');
                        const createdB = parseInt(b.dataset.createdAt || '0');
                        return createdB - createdA;
                    }
                    if (lastA === 0) return 1; // A goes to end
                    if (lastB === 0) return -1; // B goes to end
                    return lastB - lastA; // Descending (most recent first)
                    
                case 'mandatory':
                    const mandatoryA = parseInt(a.dataset.isMandatory || '0');
                    const mandatoryB = parseInt(b.dataset.isMandatory || '0');
                    if (mandatoryB !== mandatoryA) {
                        return mandatoryB - mandatoryA; // Mandatory first
                    }
                    // If both have same mandatory status, sort by start date
                    const createdAtA = parseInt(a.dataset.createdAt || '0');
                    const createdAtB = parseInt(b.dataset.createdAt || '0');
                    return createdAtB - createdAtA;
                    
                case 'startDate':
                default:
                    const createdA = parseInt(a.dataset.createdAt || '0');
                    const createdB = parseInt(b.dataset.createdAt || '0');
                    return createdB - createdA; // Descending (newest first)
            }
        });
        
        // Re-append sorted cards
        cards.forEach(card => grid.appendChild(card));
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
    }

    filterItems() {
        const items = document.querySelectorAll('.study-item-card');
        let visibleCount = 0;

        items.forEach(item => {
            let shouldShow = true;
            const itemCategory = item.dataset.category || '';

            // Filter by category tab
            if (this.selectedCategory) {
                shouldShow = shouldShow && itemCategory === this.selectedCategory;
            }

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

            if (shouldShow) {
                item.classList.remove('hidden');
                visibleCount++;
            } else {
                item.classList.add('hidden');
            }
        });

        // Re-apply current sort after filtering
        const sortSelect = document.getElementById('sortBy');
        if (sortSelect) {
            this.sortItems(sortSelect.value);
        }

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

    initializeCategoryTabs() {
        const tabs = Array.from(document.querySelectorAll('.study-tab'));
        if (tabs.length === 0) {
            this.selectedCategory = null;
            return;
        }

        let activeTab = tabs.find(tab => tab.classList.contains('active') || tab.getAttribute('aria-selected') === 'true');
        if (!activeTab) {
            activeTab = tabs[0];
        }

        this.selectedCategory = activeTab.dataset.category || null;
        this.updateTabStates();
        this.filterItems();
    }

    selectCategory(category) {
        if (!category || this.selectedCategory === category) {
            return;
        }
        this.selectedCategory = category;
        this.updateTabStates();
        this.filterItems();
    }

    updateTabStates() {
        const tabs = document.querySelectorAll('.study-tab');
        if (tabs.length === 0) {
            return;
        }

        tabs.forEach(tab => {
            const category = tab.dataset.category || null;
            const isActive = this.selectedCategory === category;
            if (isActive) {
                tab.classList.add('active');
                tab.setAttribute('aria-selected', 'true');
                tab.setAttribute('tabindex', '0');
            } else {
                tab.classList.remove('active');
                tab.setAttribute('aria-selected', 'false');
                tab.setAttribute('tabindex', '-1');
            }
        });
    }

    hideBottomMenu() {
        // Add class to body to hide bottom menu and headers
        const body = document.body;
        if (!body.classList.contains('study-page-active')) {
            body.classList.add('study-page-active');
            this.layoutAdjusted = true;
        }
    }

    updateViewportHeightVar() {
        const viewport = window.visualViewport;
        const vh = viewport ? Math.round(viewport.height) : window.innerHeight;
        if (vh > 0) {
            document.documentElement.style.setProperty('--study-page-viewport-height', `${vh}px`);
        }
    }

    restoreLayoutAdjustments() {
        window.removeEventListener('resize', this.handleViewportResize);
        window.removeEventListener('orientationchange', this.handleViewportResize);
        if (window.visualViewport) {
            window.visualViewport.removeEventListener('resize', this.handleViewportResize);
            window.visualViewport.removeEventListener('scroll', this.handleViewportResize);
        }
        window.removeEventListener('beforeunload', this.teardownHandler);
        window.removeEventListener('pagehide', this.teardownHandler);

        if (!this.layoutAdjusted) {
            return;
        }
        document.body.classList.remove('study-page-active');
        this.layoutAdjusted = false;
        // Remove inline fallback styles if they were set by other components
        const mainContent = document.querySelector('.main-content');
        if (mainContent) {
            mainContent.style.removeProperty('position');
            mainContent.style.removeProperty('top');
            mainContent.style.removeProperty('bottom');
            mainContent.style.removeProperty('height');
            mainContent.style.removeProperty('overflow');
            mainContent.style.removeProperty('padding');
        }
        const fixedFooter = document.querySelector('.fixed-footer');
        if (fixedFooter) {
            fixedFooter.style.removeProperty('display');
        }
        const fixedHeader = document.querySelector('.fixed-header');
        if (fixedHeader) {
            fixedHeader.style.removeProperty('display');
        }
        const desktopHeader = document.querySelector('.desktop-header');
        if (desktopHeader) {
            desktopHeader.style.removeProperty('display');
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
