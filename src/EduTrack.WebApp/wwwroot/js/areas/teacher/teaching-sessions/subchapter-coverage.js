/**
 * SubChapter Coverage Management
 * Manages subchapter coverage tracking for teaching sessions
 */

class SubChapterCoverageManager {
    constructor() {
        this.sessionId = null;
        this.groups = [];
        this.chapters = [];
        this.currentGroupId = null;
        this.coverageData = {};
        this.isInitialized = false;
        
        this.initEventListeners();
    }

    init(sessionId, groups, chapters) {
        this.sessionId = sessionId;
        this.groups = groups;
        this.chapters = chapters;
        this.isInitialized = true;
        
        console.log('SubChapterCoverageManager initialized:', {
            sessionId: this.sessionId,
            groupsCount: this.groups.length,
            chaptersCount: this.chapters.length
        });
        
        this.loadExistingData();
        this.setupGroupSelection();
    }

    initEventListeners() {
        // Group selection
        document.addEventListener('click', (e) => {
            if (e.target.closest('.group-item')) {
                const groupItem = e.target.closest('.group-item');
                const groupId = parseInt(groupItem.dataset.groupId);
                this.selectGroup(groupId);
            }
        });

        // Chapter toggle
        document.addEventListener('click', (e) => {
            if (e.target.closest('.chapter-header')) {
                const chapterHeader = e.target.closest('.chapter-header');
                const chapterId = parseInt(chapterHeader.dataset.chapterId);
                this.toggleChapter(chapterId);
            }
        });

        // Coverage checkbox
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('coverage-checkbox')) {
                const groupId = parseInt(e.target.dataset.groupId);
                const subChapterId = parseInt(e.target.dataset.subchapterId);
                this.updateCoverage(groupId, subChapterId, 'wasCovered', e.target.checked);
                this.updateChapterProgress(groupId, chapterId);
            }
        });

        // Percentage slider
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('percentage-slider')) {
                const groupId = parseInt(e.target.dataset.groupId);
                const subChapterId = parseInt(e.target.dataset.subchapterId);
                const percentage = parseInt(e.target.value);
                
                this.updateCoverage(groupId, subChapterId, 'coveragePercentage', percentage);
                this.updatePercentageDisplay(groupId, subChapterId, percentage);
                this.updateChapterProgress(groupId, this.getChapterIdBySubChapterId(subChapterId));
            }
        });

        // Status select
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('status-select')) {
                const groupId = parseInt(e.target.dataset.groupId);
                const subChapterId = parseInt(e.target.dataset.subchapterId);
                const status = parseInt(e.target.value);
                
                this.updateCoverage(groupId, subChapterId, 'coverageStatus', status);
            }
        });

        // Text areas
        document.addEventListener('input', (e) => {
            if (e.target.classList.contains('notes-textarea') || 
                e.target.classList.contains('challenges-textarea') ||
                e.target.classList.contains('general-notes-textarea') ||
                e.target.classList.contains('general-challenges-textarea') ||
                e.target.classList.contains('recommendations-textarea')) {
                
                const groupId = parseInt(e.target.dataset.groupId);
                const subChapterId = e.target.dataset.subchapterId ? parseInt(e.target.dataset.subchapterId) : null;
                
                if (subChapterId) {
                    const field = e.target.classList.contains('notes-textarea') ? 'teacherNotes' : 'challenges';
                    this.updateCoverage(groupId, subChapterId, field, e.target.value);
                } else {
                    const field = e.target.classList.contains('general-notes-textarea') ? 'generalNotes' :
                                 e.target.classList.contains('general-challenges-textarea') ? 'challenges' : 'recommendations';
                    this.updateGroupData(groupId, field, e.target.value);
                }
            }
        });

        // Save button
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-save-group')) {
                const button = e.target.closest('.btn-save-group');
                const groupId = parseInt(button.dataset.groupId);
                this.saveGroupData(groupId);
            }
        });
    }

    setupGroupSelection() {
        if (this.groups.length > 0) {
            this.selectGroup(this.groups[0].id);
        }
    }

    selectGroup(groupId) {
        // Update UI
        document.querySelectorAll('.group-item').forEach(item => {
            item.classList.remove('active');
        });
        
        const selectedGroupItem = document.querySelector(`[data-group-id="${groupId}"]`);
        if (selectedGroupItem) {
            selectedGroupItem.classList.add('active');
        }

        // Show/hide tabs
        document.querySelectorAll('.coverage-tab').forEach(tab => {
            tab.style.display = 'none';
        });
        
        const selectedTab = document.querySelector(`.coverage-tab[data-group-id="${groupId}"]`);
        if (selectedTab) {
            selectedTab.style.display = 'block';
        }

        this.currentGroupId = groupId;
        this.updateGroupStatus(groupId);
    }

    toggleChapter(chapterId) {
        const chapterHeader = document.querySelector(`[data-chapter-id="${chapterId}"]`);
        const subchaptersContainer = chapterHeader.nextElementSibling;
        const toggleIcon = chapterHeader.querySelector('.chapter-toggle');
        
        if (subchaptersContainer.style.display === 'none') {
            subchaptersContainer.style.display = 'block';
            toggleIcon.classList.remove('fa-chevron-down');
            toggleIcon.classList.add('fa-chevron-up');
        } else {
            subchaptersContainer.style.display = 'none';
            toggleIcon.classList.remove('fa-chevron-up');
            toggleIcon.classList.add('fa-chevron-down');
        }
    }

    updateCoverage(groupId, subChapterId, field, value) {
        if (!this.coverageData[groupId]) {
            this.coverageData[groupId] = {
                groupId: groupId,
                subChapterCoverages: [],
                generalNotes: '',
                challenges: '',
                recommendations: ''
            };
        }

        let coverage = this.coverageData[groupId].subChapterCoverages.find(c => c.subChapterId === subChapterId);
        if (!coverage) {
            coverage = {
                subChapterId: subChapterId,
                wasPlanned: false,
                wasCovered: false,
                coveragePercentage: 0,
                coverageStatus: 0,
                teacherNotes: '',
                challenges: ''
            };
            this.coverageData[groupId].subChapterCoverages.push(coverage);
        }

        coverage[field] = value;
        
        // Auto-update related fields
        if (field === 'wasCovered' && value) {
            coverage.coveragePercentage = Math.max(coverage.coveragePercentage, 50);
            this.updatePercentageDisplay(groupId, subChapterId, coverage.coveragePercentage);
            const slider = document.querySelector(`.percentage-slider[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
            if (slider) {
                slider.value = coverage.coveragePercentage;
            }
        }
        
        if (field === 'coveragePercentage') {
            coverage.wasCovered = value > 0;
            const checkbox = document.querySelector(`.coverage-checkbox[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
            if (checkbox) {
                checkbox.checked = coverage.wasCovered;
            }
        }
    }

    updateGroupData(groupId, field, value) {
        if (!this.coverageData[groupId]) {
            this.coverageData[groupId] = {
                groupId: groupId,
                subChapterCoverages: [],
                generalNotes: '',
                challenges: '',
                recommendations: ''
            };
        }

        this.coverageData[groupId][field] = value;
    }

    updatePercentageDisplay(groupId, subChapterId, percentage) {
        const valueDisplay = document.querySelector(`.percentage-value[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        if (valueDisplay) {
            valueDisplay.textContent = `${percentage}%`;
        }
    }

    updateChapterProgress(groupId, chapterId) {
        if (!chapterId) return;
        
        const chapter = this.chapters.find(c => c.id === chapterId);
        if (!chapter) return;

        const subChapterIds = chapter.subChapters.map(sc => sc.id);
        const coverages = this.coverageData[groupId]?.subChapterCoverages || [];
        
        const chapterCoverages = coverages.filter(c => subChapterIds.includes(c.subChapterId));
        const totalPercentage = chapterCoverages.reduce((sum, c) => sum + c.coveragePercentage, 0);
        const averagePercentage = chapterCoverages.length > 0 ? totalPercentage / chapterCoverages.length : 0;
        
        const progressBar = document.querySelector(`[data-chapter-id="${chapterId}"] .progress-fill`);
        const progressText = document.querySelector(`[data-chapter-id="${chapterId}"] .progress-text`);
        
        if (progressBar) {
            progressBar.style.width = `${averagePercentage}%`;
        }
        if (progressText) {
            progressText.textContent = `${Math.round(averagePercentage)}%`;
        }
    }

    getChapterIdBySubChapterId(subChapterId) {
        for (const chapter of this.chapters) {
            if (chapter.subChapters.some(sc => sc.id === subChapterId)) {
                return chapter.id;
            }
        }
        return null;
    }

    updateGroupStatus(groupId) {
        const statusIndicator = document.querySelector(`.status-indicator[data-group-id="${groupId}"]`);
        if (!statusIndicator) return;

        const hasData = this.coverageData[groupId] && 
                       (this.coverageData[groupId].subChapterCoverages.length > 0 || 
                        this.coverageData[groupId].generalNotes || 
                        this.coverageData[groupId].challenges || 
                        this.coverageData[groupId].recommendations);

        const icon = statusIndicator.querySelector('i');
        if (hasData) {
            icon.classList.remove('status-pending');
            icon.classList.add('status-completed');
            statusIndicator.title = 'اطلاعات ثبت شده';
        } else {
            icon.classList.remove('status-completed');
            icon.classList.add('status-pending');
            statusIndicator.title = 'در انتظار ثبت اطلاعات';
        }
    }

    async loadExistingData() {
        try {
            const response = await fetch(`/Teacher/TeachingSessions/GetSubChapterCoverageStepData?sessionId=${this.sessionId}`);
            const result = await response.json();
            
            if (result.success && result.data) {
                this.coverageData = {};
                
                result.data.groupCoverages.forEach(groupCoverage => {
                    this.coverageData[groupCoverage.groupId] = {
                        groupId: groupCoverage.groupId,
                        subChapterCoverages: groupCoverage.subChapterCoverages.map(sc => ({
                            subChapterId: sc.subChapterId,
                            wasPlanned: sc.wasPlanned,
                            wasCovered: sc.wasCovered,
                            coveragePercentage: sc.coveragePercentage,
                            coverageStatus: sc.coverageStatus,
                            teacherNotes: sc.teacherNotes || '',
                            challenges: sc.challenges || ''
                        })),
                        generalNotes: groupCoverage.generalNotes || '',
                        challenges: groupCoverage.challenges || '',
                        recommendations: groupCoverage.recommendations || ''
                    };
                });
                
                this.populateUI();
            }
        } catch (error) {
            console.error('Error loading existing data:', error);
        }
    }

    populateUI() {
        // Populate form fields with existing data
        Object.keys(this.coverageData).forEach(groupId => {
            const groupData = this.coverageData[groupId];
            
            // Populate subchapter coverages
            groupData.subChapterCoverages.forEach(coverage => {
                // Checkbox
                const checkbox = document.querySelector(`.coverage-checkbox[data-group-id="${groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                if (checkbox) {
                    checkbox.checked = coverage.wasCovered;
                }
                
                // Percentage slider
                const slider = document.querySelector(`.percentage-slider[data-group-id="${groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                if (slider) {
                    slider.value = coverage.coveragePercentage;
                }
                
                // Percentage display
                this.updatePercentageDisplay(groupId, coverage.subChapterId, coverage.coveragePercentage);
                
                // Status select
                const statusSelect = document.querySelector(`.status-select[data-group-id="${groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                if (statusSelect) {
                    statusSelect.value = coverage.coverageStatus;
                }
                
                // Notes textarea
                const notesTextarea = document.querySelector(`.notes-textarea[data-group-id="${groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                if (notesTextarea) {
                    notesTextarea.value = coverage.teacherNotes || '';
                }
                
                // Challenges textarea
                const challengesTextarea = document.querySelector(`.challenges-textarea[data-group-id="${groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                if (challengesTextarea) {
                    challengesTextarea.value = coverage.challenges || '';
                }
                
                // Update chapter progress
                const chapterId = this.getChapterIdBySubChapterId(coverage.subChapterId);
                if (chapterId) {
                    this.updateChapterProgress(groupId, chapterId);
                }
            });
            
            // Populate general fields
            const generalNotesTextarea = document.querySelector(`.general-notes-textarea[data-group-id="${groupId}"]`);
            if (generalNotesTextarea) {
                generalNotesTextarea.value = groupData.generalNotes || '';
            }
            
            const generalChallengesTextarea = document.querySelector(`.general-challenges-textarea[data-group-id="${groupId}"]`);
            if (generalChallengesTextarea) {
                generalChallengesTextarea.value = groupData.challenges || '';
            }
            
            const recommendationsTextarea = document.querySelector(`.recommendations-textarea[data-group-id="${groupId}"]`);
            if (recommendationsTextarea) {
                recommendationsTextarea.value = groupData.recommendations || '';
            }
            
            this.updateGroupStatus(groupId);
        });
    }

    async saveGroupData(groupId) {
        if (!this.coverageData[groupId]) {
            this.showMessage('هیچ اطلاعاتی برای ذخیره وجود ندارد.', 'warning');
            return;
        }

        const saveButton = document.querySelector(`.btn-save-group[data-group-id="${groupId}"]`);
        if (saveButton) {
            saveButton.disabled = true;
            saveButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> در حال ذخیره...';
        }

        try {
            const data = {
                sessionId: this.sessionId,
                groupCoverages: [this.coverageData[groupId]]
            };

            const response = await fetch('/Teacher/TeachingSessions/SaveSubChapterCoverageStep', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                this.showMessage('اطلاعات گروه با موفقیت ذخیره شد.', 'success');
                this.updateGroupStatus(groupId);
            } else {
                this.showMessage(result.message || 'خطا در ذخیره اطلاعات.', 'error');
            }
        } catch (error) {
            console.error('Error saving group data:', error);
            this.showMessage('خطا در ارتباط با سرور.', 'error');
        } finally {
            if (saveButton) {
                saveButton.disabled = false;
                saveButton.innerHTML = '<i class="fas fa-save"></i> ذخیره اطلاعات گروه';
            }
        }
    }

    showMessage(message, type = 'info') {
        // Create toast notification
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
                <span>${message}</span>
            </div>
        `;
        
        document.body.appendChild(toast);
        
        // Show toast
        setTimeout(() => toast.classList.add('show'), 100);
        
        // Hide toast
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => document.body.removeChild(toast), 300);
        }, 3000);
    }
}

// Initialize global instance
const SubChapterCoverageManager = new SubChapterCoverageManager();
