/**
 * Step 3 Assignment Module
 * Handles group selection, subchapter selection, and student management
 */

class Step3AssignmentManager {
    constructor(formManager) {
        this.formManager = formManager;
        this.selectedGroups = [];
        this.selectedSubChapters = [];
        this.selectedStudents = [];
        this.allStudents = [];

        // Event listener flags
        this.step3EventListenersSetup = false;
        this.step3ClickHandlerActive = false;

        // Processing flags to prevent multiple executions
        this.processingGroup = null;
        this.processingSubChapter = null;

        // Event handler reference for cleanup
        this.step3ClickHandler = null;

        this.init();
    }

    async init() {
        console.log('Step 3 Assignment Manager initialized');
    }

    // Initialize step 3 content when entering step 3
    async initializeStep3Content() {
        console.log('Initializing step 3 content...');

        // Wait for DOM to be ready
        await this.waitForDOMReady();

        // Check if required elements exist
        const requiredElements = {
            'groupBadgeGrid': document.getElementById('groupBadgeGrid'),
            'chapterList': document.getElementById('chapterList'),
            'studentSelectionContainer': document.getElementById('studentSelectionContainer'),
            'studentGrid': document.getElementById('studentGrid')
        };

        console.log('Required elements check:', requiredElements);

        // Check if all required elements exist
        const missingElements = Object.entries(requiredElements)
            .filter(([name, element]) => !element)
            .map(([name]) => name);

        if (missingElements.length > 0) {
            console.error('Missing required elements:', missingElements);
            return;
        }

        // Load groups and subchapters if not already loaded
        await this.loadGroupsAsBadges();
        await this.loadChaptersWithSubChapters();
        await this.loadAndSelectExistingAssignments();

        // Setup event listeners after content is loaded
        this.setupStep3EventListeners();

        // Update student selection visibility based on selected groups
        this.updateStudentSelectionVisibility();

        // Update assignment preview
        this.updateAssignmentPreview();
        
        // Also update the main form's assignment preview
        if (this.formManager && typeof this.formManager.updateAssignmentPreview === 'function') {
            this.formManager.updateAssignmentPreview();
        }

        console.log('Step 3 initialization completed');
    }

    // Wait for DOM to be ready
    async waitForDOMReady() {
        return new Promise((resolve) => {
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', resolve);
            } else {
                resolve();
            }
        });
    }

    setupStep3EventListeners() {
        // Event listeners are now added directly to each badge in render methods
        // This method is kept for compatibility but doesn't do anything
        console.log('Step 3 event listeners setup completed (direct listeners used)');
        this.step3EventListenersSetup = true;
    }

    // Cleanup method for memory management
    cleanup() {
        console.log('Cleaning up step 3 event listeners...');

        // Reset flags
        this.step3EventListenersSetup = false;
        this.step3ClickHandlerActive = false;
        this.processingGroup = null;
        this.processingSubChapter = null;
        this.step3ClickHandler = null;

        console.log('Step 3 cleanup completed');
    }

    // Group Management Methods
    async loadGroupsAsBadges() {
        try {
            const teachingPlanId = this.getTeachingPlanId();
            console.log('Loading groups for teaching plan ID:', teachingPlanId);
            
            if (!teachingPlanId) {
                console.error('TeachingPlanId is null or undefined');
                return;
            }
            
            const response = await fetch(`/Teacher/Schedule/GetGroups?teachingPlanId=${teachingPlanId}`);
            console.log('Groups response status:', response.status);
            
            if (!response.ok) {
                console.error('Failed to fetch groups:', response.statusText);
                return;
            }
            
            const groups = await response.json();
            console.log('Groups response:', groups);

            if (groups.success) {
                this.renderGroupBadges(groups.data);
                console.log('Groups rendered successfully');
            } else {
                console.error('Failed to load groups:', groups.message);
            }
        } catch (error) {
            console.error('Error loading groups:', error);
        }
    }

    renderGroupBadges(groups) {
        const container = document.getElementById('groupBadgeGrid');
        if (!container) return;

        container.innerHTML = groups.map(group => `
            <div class="group-badge" data-group-id="${group.id}">
                <div class="group-badge-content">
                    <div class="group-badge-icon">
                        <i class="fas fa-users"></i>
                    </div>
                    <div class="group-badge-info">
                        <div class="group-badge-name">${group.name}</div>
                        <div class="group-badge-count">گروه دانشجویی</div>
                    </div>
                </div>
                <div class="group-badge-check"></div>
            </div>
        `).join('');

        // Add event listeners directly to each badge
        container.querySelectorAll('.group-badge').forEach(badge => {
            badge.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                const groupId = parseInt(badge.dataset.groupId);
                console.log('Group badge clicked directly:', groupId);
                this.toggleGroupSelection(groupId);
            });
        });
    }

    toggleGroupSelection(groupId) {
        console.log('toggleGroupSelection called for groupId:', groupId);
        console.log('Current processingGroup:', this.processingGroup);
        
        // Prevent multiple executions for the same group
        if (this.processingGroup === groupId) {
            console.log('Group is already being processed, skipping...');
            return;
        }

        this.processingGroup = groupId;

        const badge = document.querySelector(`[data-group-id="${groupId}"]`);

        if (!badge) {
            console.error('Group badge not found for ID:', groupId);
            this.processingGroup = null;
            return;
        }

        const isSelected = badge.classList.contains('selected');
        console.log('Group isSelected:', isSelected);

        if (isSelected) {
            console.log('Removing selected class from group');
            badge.classList.remove('selected');
            this.selectedGroups = this.selectedGroups.filter(g => g.id !== groupId);
        } else {
            console.log('Adding selected class to group');
            badge.classList.add('selected');
            const group = { id: groupId, name: badge.querySelector('.group-badge-name').textContent };
            this.selectedGroups.push(group);
        }
        
        console.log('Selected groups after toggle:', this.selectedGroups);

        this.updateGroupSelectionSummary();
        this.updateAssignmentPreview();
        this.updateHiddenInputs();
        this.updateStudentSelectionVisibility(); // Update student list when groups change
        
        // Also update the main form's assignment preview
        if (this.formManager && typeof this.formManager.updateAssignmentPreview === 'function') {
            this.formManager.updateAssignmentPreview();
        }

        // Reset processing flag after a short delay
        setTimeout(() => {
            this.processingGroup = null;
            console.log('Processing flag reset for group:', groupId);
        }, 300);
    }

    updateGroupSelectionSummary() {
        const summary = document.getElementById('groupSelectionSummary');
        if (!summary) return;

        const summaryText = summary.querySelector('.summary-text');
        if (this.selectedGroups.length === 0) {
            summaryText.textContent = 'همه گروه‌ها';
        } else {
            const groupNames = this.selectedGroups.map(g => g.name).join('، ');
            summaryText.textContent = `گروه‌های انتخاب شده: ${groupNames}`;
        }
    }

    // SubChapter Management Methods
    async loadChaptersWithSubChapters() {
        try {
            const teachingPlanId = this.getTeachingPlanId();
            console.log('Loading subchapters for teaching plan ID:', teachingPlanId);
            
            if (!teachingPlanId) {
                console.error('TeachingPlanId is null or undefined');
                return;
            }
            
            const response = await fetch(`/Teacher/Schedule/GetSubChapters?teachingPlanId=${teachingPlanId}`);
            console.log('Subchapters response status:', response.status);
            
            if (!response.ok) {
                console.error('Failed to fetch subchapters:', response.statusText);
                return;
            }
            
            const subChapters = await response.json();
            console.log('Subchapters response:', subChapters);

            if (subChapters.success) {
                this.renderChapterHierarchy(subChapters.data);
                console.log('Chapter hierarchy rendered successfully');
            } else {
                console.error('Failed to load subchapters:', subChapters.message);
            }
        } catch (error) {
            console.error('Error loading chapters:', error);
        }
    }

    renderChapterHierarchy(subChapters) {
        const container = document.getElementById('chapterList');
        if (!container) {
            console.error('Chapter list container not found');
            return;
        }

        console.log('Rendering chapter hierarchy with subchapters:', subChapters);

        // Group subchapters by chapter
        const chaptersMap = new Map();
        subChapters.forEach(subChapter => {
            const chapterTitle = subChapter.chapterTitle;
            if (!chaptersMap.has(chapterTitle)) {
                chaptersMap.set(chapterTitle, []);
            }
            chaptersMap.get(chapterTitle).push(subChapter);
        });

        console.log('Grouped chapters:', chaptersMap);

        container.innerHTML = Array.from(chaptersMap.entries()).map(([chapterTitle, chapterSubChapters]) => `
            <div class="chapter-item">
                <div class="chapter-header">
                    <div class="chapter-icon">
                        <i class="fas fa-book"></i>
                    </div>
                    <div class="chapter-info">
                        <div class="chapter-title">${chapterTitle}</div>
                        <div class="chapter-description">${chapterSubChapters.length} زیرمبحث</div>
                    </div>
                    <div class="chapter-toggle">
                        <i class="fas fa-chevron-down"></i>
                    </div>
                </div>
                <div class="subchapter-grid">
                    ${chapterSubChapters.map(subChapter => `
                        <div class="subchapter-badge" data-subchapter-id="${subChapter.id}">
                            <div class="subchapter-content">
                                <div class="subchapter-icon">
                                    <i class="fas fa-list"></i>
                                </div>
                                <div class="subchapter-info">
                                    <div class="subchapter-title">${subChapter.title}</div>
                                    <div class="subchapter-meta">زیرمبحث</div>
                                </div>
                            </div>
                            <div class="subchapter-check"></div>
                        </div>
                    `).join('')}
                </div>
            </div>
        `).join('');

        console.log('Chapter hierarchy rendered. Container HTML:', container.innerHTML);

        // Add event listeners directly to each subchapter badge
        container.querySelectorAll('.subchapter-badge').forEach(badge => {
            badge.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                const subChapterId = parseInt(badge.dataset.subchapterId);
                console.log('Subchapter badge clicked directly:', subChapterId);
                this.toggleSubChapterSelection(subChapterId);
            });
        });

        // Add event listeners directly to each chapter header
        container.querySelectorAll('.chapter-header').forEach(header => {
            header.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                const chapterItem = header.closest('.chapter-item');
                console.log('Chapter header clicked directly');
                this.toggleChapterExpansion(chapterItem);
            });
        });
    }

    toggleChapterExpansion(chapterItem) {
        console.log('toggleChapterExpansion called for:', chapterItem);
        chapterItem.classList.toggle('expanded');
        console.log('Chapter expanded state:', chapterItem.classList.contains('expanded'));
    }

    toggleSubChapterSelection(subChapterId) {
        console.log('toggleSubChapterSelection called for subChapterId:', subChapterId);
        console.log('Current processingSubChapter:', this.processingSubChapter);
        
        // Prevent multiple executions for the same subchapter
        if (this.processingSubChapter === subChapterId) {
            console.log('SubChapter is already being processed, skipping...');
            return;
        }

        this.processingSubChapter = subChapterId;

        const badge = document.querySelector(`[data-subchapter-id="${subChapterId}"]`);

        if (!badge) {
            console.error('Subchapter badge not found for ID:', subChapterId);
            this.processingSubChapter = null;
            return;
        }

        const isSelected = badge.classList.contains('selected');
        console.log('SubChapter isSelected:', isSelected);

        if (isSelected) {
            console.log('Removing selected class from subchapter');
            badge.classList.remove('selected');
            this.selectedSubChapters = this.selectedSubChapters.filter(sc => sc.id !== subChapterId);
        } else {
            console.log('Adding selected class to subchapter');
            badge.classList.add('selected');
            const titleElement = badge.querySelector('.subchapter-title');
            const title = titleElement ? titleElement.textContent : `زیرمبحث ${subChapterId}`;
            const subChapter = {
                id: subChapterId,
                title: title
            };
            this.selectedSubChapters.push(subChapter);
        }
        
        console.log('Selected subchapters after toggle:', this.selectedSubChapters);

        this.updateSubChapterSelectionSummary();
        this.updateAssignmentPreview();
        this.updateHiddenInputs();
        this.validateSubChapterSelection();
        
        // Also update the main form's assignment preview
        if (this.formManager && typeof this.formManager.updateAssignmentPreview === 'function') {
            this.formManager.updateAssignmentPreview();
        }

        // Reset processing flag after a short delay
        setTimeout(() => {
            this.processingSubChapter = null;
            console.log('Processing flag reset for subchapter:', subChapterId);
        }, 300);
    }

    updateSubChapterSelectionSummary() {
        const summary = document.getElementById('subChapterSelectionSummary');
        if (!summary) return;

        const summaryText = summary.querySelector('.summary-text');
        if (this.selectedSubChapters.length === 0) {
            summaryText.textContent = 'هیچ زیرمبحثی انتخاب نشده';
        } else {
            const subChapterNames = this.selectedSubChapters.map(sc => sc.title).join('، ');
            summaryText.textContent = `زیرمباحث انتخاب شده: ${subChapterNames}`;
        }
    }

    validateSubChapterSelection() {
        const errorElement = document.getElementById('subChapterValidationError');
        if (!errorElement) return;

        if (this.selectedSubChapters.length === 0) {
            errorElement.style.display = 'block';
        } else {
            errorElement.style.display = 'none';
        }
    }

    // Student Management Methods
    updateStudentSelectionVisibility() {
        const studentSelectionContainer = document.getElementById('studentSelectionContainer');
        const studentListContainer = document.getElementById('studentListContainer');

        if (studentSelectionContainer && studentListContainer) {
            if (this.selectedGroups.length > 0) {
                // Show student selection for selected groups
                studentSelectionContainer.style.display = 'block';
                studentListContainer.style.display = 'block';

                // Load students for all selected groups
                this.loadStudentsForSelectedGroups();
            } else {
                // Hide student selection for no groups
                studentSelectionContainer.style.display = 'none';
                this.allStudents = [];
                this.renderStudents();
            }
        }
    }

    async loadStudentsForSelectedGroups() {
        try {
            console.log('Loading students for selected groups:', this.selectedGroups);
            this.allStudents = [];

            // Load students for each selected group
            for (const group of this.selectedGroups) {
                console.log('Loading students for group:', group);
                const response = await fetch(`/Teacher/StudentGroup/GetStudents?groupId=${group.id}`);
                console.log('Student response status for group', group.id, ':', response.status);
                if (response.ok) {
                    const students = await response.json();
                    console.log('Students loaded for group', group.id, ':', students);
                    // Add group info to each student
                    const studentsWithGroup = students.map(student => ({
                        ...student,
                        groupId: group.id,
                        groupName: group.name
                    }));
                    this.allStudents.push(...studentsWithGroup);
                } else {
                    console.error('Failed to load students for group', group.id, ':', response.statusText);
                }
            }

            // Remove duplicates based on student ID
            this.allStudents = this.allStudents.filter((student, index, self) =>
                index === self.findIndex(s => s.id === student.id)
            );

            console.log('Total students loaded:', this.allStudents.length);

            // Update selected students with full details from loaded students
            this.updateSelectedStudentsWithDetails();

            this.renderStudents();
            this.updateStudentCount();
        } catch (error) {
            console.error('Error loading students:', error);
        }
    }

    updateSelectedStudentsWithDetails() {
        // Update selected students with full details from loaded students
        this.selectedStudents = this.selectedStudents.map(selectedStudent => {
            const fullStudentDetails = this.allStudents.find(student => student.id === selectedStudent.id);
            if (fullStudentDetails) {
                return {
                    ...fullStudentDetails,
                    // Keep the original selected state
                    isSelected: true
                };
            }
            return selectedStudent;
        });

        // Remove any selected students that are no longer in the allStudents list
        this.selectedStudents = this.selectedStudents.filter(selectedStudent =>
            this.allStudents.some(student => student.id === selectedStudent.id)
        );

        console.log('Updated selected students:', this.selectedStudents);
        console.log('All students:', this.allStudents);
    }

    renderStudents() {
        const studentGrid = document.getElementById('studentGrid');
        if (!studentGrid) return;

        studentGrid.innerHTML = '';

        console.log('Rendering students. All students:', this.allStudents.length);
        console.log('Selected students:', this.selectedStudents.length);

        this.allStudents.forEach(student => {
            const studentBadge = document.createElement('div');
            studentBadge.className = 'student-badge';
            studentBadge.dataset.studentId = student.id;

            const isSelected = this.selectedStudents.some(s => s.id === student.id);
            console.log(`Student ${student.id} (${student.firstName} ${student.lastName}) isSelected:`, isSelected);

            if (isSelected) {
                studentBadge.classList.add('selected');
            }

            studentBadge.innerHTML = `
                <div class="student-avatar">
                    ${student.firstName ? student.firstName.charAt(0) : '?'}
                </div>
                <div class="student-info">
                    <div class="student-name">${student.firstName} ${student.lastName}</div>
                    <div class="student-group">${student.groupName || 'گروه نامشخص'}</div>
                </div>
                <div class="student-check"></div>
            `;

            studentBadge.addEventListener('click', () => {
                this.toggleStudentSelection(student);
            });

            studentGrid.appendChild(studentBadge);
        });
    }

    toggleStudentSelection(student) {
        console.log('toggleStudentSelection called for student:', student);
        
        const badge = document.querySelector(`[data-student-id="${student.id}"]`);
        if (!badge) {
            console.error('Student badge not found for ID:', student.id);
            return;
        }
        
        const existingIndex = this.selectedStudents.findIndex(s => s.id === student.id);
        const isSelected = existingIndex > -1;

        if (isSelected) {
            console.log('Removing selected class from student');
            badge.classList.remove('selected');
            this.selectedStudents.splice(existingIndex, 1);
        } else {
            console.log('Adding selected class to student');
            badge.classList.add('selected');
            this.selectedStudents.push(student);
        }
        
        console.log('Selected students after toggle:', this.selectedStudents);
        this.updateSelectedStudentsSummary();
    }

    selectAllStudents() {
        this.selectedStudents = [...this.allStudents];
        this.renderStudents();
        this.updateSelectedStudentsSummary();
    }

    clearAllStudents() {
        this.selectedStudents = [];
        this.renderStudents();
        this.updateSelectedStudentsSummary();
    }

    updateStudentCount() {
        const studentCount = document.getElementById('studentCount');
        if (studentCount) {
            studentCount.textContent = `${this.allStudents.length} دانش‌آموز`;
        }
    }

    updateSelectedStudentsSummary() {
        const selectedStudentsText = document.getElementById('selectedStudentsText');
        if (!selectedStudentsText) return;

        if (this.selectedStudents.length === 0) {
            selectedStudentsText.textContent = 'هیچ دانش‌آموزی انتخاب نشده - برای همه دانش‌آموزان گروه‌های انتخاب شده';
        } else if (this.selectedStudents.length === this.allStudents.length) {
            selectedStudentsText.textContent = 'همه دانش‌آموزان گروه‌های انتخاب شده انتخاب شده‌اند';
        } else {
            selectedStudentsText.textContent = `${this.selectedStudents.length} دانش‌آموز انتخاب شده`;
        }
    }

    // Assignment Preview Methods
    updateAssignmentPreview() {
        this.updateTargetGroupsPreview();
        this.updateRelatedSubChaptersPreview();
    }

    updateTargetGroupsPreview() {
        const targetGroupsElement = document.getElementById('targetGroups');
        if (targetGroupsElement) {
            if (this.selectedGroups.length === 0) {
                targetGroupsElement.textContent = 'همه گروه‌ها';
            } else {
                targetGroupsElement.textContent = this.selectedGroups.map(g => g.name).join(', ');
            }
        }
    }

    updateRelatedSubChaptersPreview() {
        const relatedSubChaptersElement = document.getElementById('relatedSubChapters');
        if (relatedSubChaptersElement) {
            if (this.selectedSubChapters.length === 0) {
                relatedSubChaptersElement.textContent = 'انتخاب نشده';
            } else {
                relatedSubChaptersElement.textContent = this.selectedSubChapters.map(sc => sc.title).join(', ');
            }
        }
    }

    // Utility Methods
    getTeachingPlanId() {
        const teachingPlanIdInput = document.querySelector('input[name="TeachingPlanId"]');
        return teachingPlanIdInput ? teachingPlanIdInput.value : null;
    }

    updateHiddenInputs() {
        // Update hidden inputs for form submission
        const groupIdsInput = document.getElementById('selectedGroupIds');
        const subChapterIdsInput = document.getElementById('selectedSubChapterIds');
        const studentIdsInput = document.getElementById('selectedStudentIds');

        if (groupIdsInput) {
            groupIdsInput.value = this.selectedGroups.map(g => g.id).join(',');
        }

        if (subChapterIdsInput) {
            subChapterIdsInput.value = this.selectedSubChapters.map(sc => sc.id).join(',');
        }

        if (studentIdsInput) {
            studentIdsInput.value = this.selectedStudents.map(s => s.id).join(',');
        }
    }

    // Data Export Methods for main form
    getSelectedGroups() {
        return this.selectedGroups;
    }

    getSelectedSubChapters() {
        return this.selectedSubChapters;
    }

    getSelectedStudents() {
        return this.selectedStudents;
    }

    // Data Import Methods for existing data
    setSelectedGroups(groups) {
        this.selectedGroups = groups || [];
    }

    setSelectedSubChapters(subChapters) {
        this.selectedSubChapters = subChapters || [];
    }

    setSelectedStudents(students) {
        this.selectedStudents = students || [];
    }

    // Load existing assignments
    async loadAndSelectExistingAssignments() {
        // Select existing groups and subchapters (content already loaded in initializeStep3Content)
        try {
            // Select existing groups
            this.selectedGroups.forEach(group => {
                const badge = document.querySelector(`[data-group-id="${group.id}"]`);
                if (badge) {
                    badge.classList.add('selected');
                    // Update the group name if we have it
                    const nameElement = badge.querySelector('.group-badge-name');
                    if (nameElement) {
                        group.name = nameElement.textContent;
                    }
                }
            });

            // Select existing subchapters and expand parent chapters
            this.selectedSubChapters.forEach(subChapter => {
                const badge = document.querySelector(`[data-subchapter-id="${subChapter.id}"]`);
                if (badge) {
                    badge.classList.add('selected');
                    // Update the subchapter title if we have it
                    const titleElement = badge.querySelector('.subchapter-title');
                    if (titleElement) {
                        subChapter.title = titleElement.textContent;
                    }

                    // Expand the parent chapter if it contains selected subchapters
                    const chapterItem = badge.closest('.chapter-item');
                    if (chapterItem && !chapterItem.classList.contains('expanded')) {
                        chapterItem.classList.add('expanded');
                    }
                }
            });

            // Update summaries and preview
            this.updateGroupSelectionSummary();
            this.updateSubChapterSelectionSummary();
            this.updateAssignmentPreview();
            this.updateHiddenInputs();
            
            // Also update the main form's assignment preview
            if (this.formManager && typeof this.formManager.updateAssignmentPreview === 'function') {
                this.formManager.updateAssignmentPreview();
            }

            // Load students for selected groups
            if (this.selectedGroups.length > 0) {
                this.updateStudentSelectionVisibility();
            }

        } catch (error) {
            console.error('Error loading existing assignments:', error);
        }
    }


}

// Export for use in main form
window.Step3AssignmentManager = Step3AssignmentManager;
