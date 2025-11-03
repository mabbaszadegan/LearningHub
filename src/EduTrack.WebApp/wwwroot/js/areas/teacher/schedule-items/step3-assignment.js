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
    }

    validateStep3() {
        let isValid = true;
        
        // Validate SubChapter selection (mandatory)
        const selectedSubChapters = this.getSelectedSubChapters();
        if (selectedSubChapters.length === 0) {
            this.showErrorMessage('انتخاب حداقل یک زیرمبحث اجباری است');
            isValid = false;
        }
        
        return isValid;
    }

    showErrorMessage(message) {
        const notification = window.EduTrack?.Services?.Notification;
        if (notification) {
            notification.error(message);
        } else if (typeof toastError !== 'undefined') {
            toastError(message);
        } else {
            try { alert(message); } catch (_) {}
        }
    }

    // Initialize step 3 content when entering step 3
    async initializeStep3Content() {

        // Wait for DOM to be ready
        await this.waitForDOMReady();

        // Check if required elements exist
        const requiredElements = {
            'groupBadgeGrid': document.getElementById('groupBadgeGrid'),
            'chapterList': document.getElementById('chapterList'),
            'studentSelectionContainer': document.getElementById('studentSelectionContainer'),
            'studentGrid': document.getElementById('studentGrid')
        };


        // Check if all required elements exist
        const missingElements = Object.entries(requiredElements)
            .filter(([name, element]) => !element)
            .map(([name]) => name);

        if (missingElements.length > 0) {
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
        this.step3EventListenersSetup = true;
    }

    // Cleanup method for memory management
    cleanup() {

        // Reset flags
        this.step3EventListenersSetup = false;
        this.step3ClickHandlerActive = false;
        this.processingGroup = null;
        this.processingSubChapter = null;
        this.step3ClickHandler = null;

    }

    // Group Management Methods
    async loadGroupsAsBadges() {
        try {
            const teachingPlanId = this.getTeachingPlanId();
            
            if (!teachingPlanId) {
                return;
            }
            
            let groups;
            
            // Use API service if available
            if (window.EduTrack?.API?.Schedule) {
                groups = await window.EduTrack.API.Schedule.getGroups(teachingPlanId);
            } else {
                const response = await fetch(`/Teacher/Schedule/GetGroups?teachingPlanId=${teachingPlanId}`);
                if (!response.ok) {
                    return;
                }
                groups = await response.json();
            }

            if (groups.success) {
                this.renderGroupBadges(groups.data);
            }
        } catch (error) {
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
                this.toggleGroupSelection(groupId);
            });
        });
    }

    toggleGroupSelection(groupId) {
        
        // Prevent multiple executions for the same group
        if (this.processingGroup === groupId) {
            return;
        }

        this.processingGroup = groupId;

        const badge = document.querySelector(`[data-group-id="${groupId}"]`);

        if (!badge) {
            this.processingGroup = null;
            return;
        }

        const isSelected = badge.classList.contains('selected');

        if (isSelected) {
            badge.classList.remove('selected');
            this.selectedGroups = this.selectedGroups.filter(g => g.id !== groupId);
        } else {
            badge.classList.add('selected');
            const group = { id: groupId, name: badge.querySelector('.group-badge-name').textContent };
            this.selectedGroups.push(group);
        }
        

        this.updateGroupSelectionSummary();
        this.updateAssignmentPreview();
        this.updateHiddenInputs();
        this.updateStudentSelectionVisibility(); // Update student list when groups change
        
        // Dispatch assignment changed event
        document.dispatchEvent(new CustomEvent('assignmentChanged', {
            detail: { type: 'group', groupId: groupId, selected: !isSelected }
        }));
        
        // Also update the main form's assignment preview
        if (this.formManager && typeof this.formManager.updateAssignmentPreview === 'function') {
            this.formManager.updateAssignmentPreview();
        }

        // Reset processing flag after a short delay
        setTimeout(() => {
            this.processingGroup = null;
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
            
            if (!teachingPlanId) {
                return;
            }
            
            let subChapters;
            
            // Use API service if available
            if (window.EduTrack?.API?.Schedule) {
                subChapters = await window.EduTrack.API.Schedule.getSubChapters(teachingPlanId);
            } else {
                const response = await fetch(`/Teacher/Schedule/GetSubChapters?teachingPlanId=${teachingPlanId}`);
                if (!response.ok) {
                    return;
                }
                subChapters = await response.json();
            }

            if (subChapters.success) {
                this.renderChapterHierarchy(subChapters.data);
            }
        } catch (error) {
        }
    }

    renderChapterHierarchy(subChapters) {
        const container = document.getElementById('chapterList');
        if (!container) {
            return;
        }


        // Group subchapters by chapter
        const chaptersMap = new Map();
        subChapters.forEach(subChapter => {
            const chapterTitle = subChapter.chapterTitle;
            if (!chaptersMap.has(chapterTitle)) {
                chaptersMap.set(chapterTitle, []);
            }
            chaptersMap.get(chapterTitle).push(subChapter);
        });


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


        // Add event listeners directly to each subchapter badge
        container.querySelectorAll('.subchapter-badge').forEach(badge => {
            badge.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                const subChapterId = parseInt(badge.dataset.subchapterId);
                this.toggleSubChapterSelection(subChapterId);
            });
        });

        // Add event listeners directly to each chapter header
        container.querySelectorAll('.chapter-header').forEach(header => {
            header.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                const chapterItem = header.closest('.chapter-item');
                this.toggleChapterExpansion(chapterItem);
            });
        });
    }

    toggleChapterExpansion(chapterItem) {
        chapterItem.classList.toggle('expanded');
    }

    toggleSubChapterSelection(subChapterId) {
        
        // Prevent multiple executions for the same subchapter
        if (this.processingSubChapter === subChapterId) {
            return;
        }

        this.processingSubChapter = subChapterId;

        const badge = document.querySelector(`[data-subchapter-id="${subChapterId}"]`);

        if (!badge) {
            this.processingSubChapter = null;
            return;
        }

        const isSelected = badge.classList.contains('selected');

        if (isSelected) {
            badge.classList.remove('selected');
            this.selectedSubChapters = this.selectedSubChapters.filter(sc => sc.id !== subChapterId);
        } else {
            badge.classList.add('selected');
            const titleElement = badge.querySelector('.subchapter-title');
            const title = titleElement ? titleElement.textContent : `زیرمبحث ${subChapterId}`;
            const subChapter = {
                id: subChapterId,
                title: title
            };
            this.selectedSubChapters.push(subChapter);
        }
        

        this.updateSubChapterSelectionSummary();
        this.updateAssignmentPreview();
        this.updateHiddenInputs();
        this.validateSubChapterSelection();
        
        // Dispatch assignment changed event
        document.dispatchEvent(new CustomEvent('assignmentChanged', {
            detail: { type: 'subchapter', subChapterId: subChapterId, selected: !isSelected }
        }));
        
        // Also update the main form's assignment preview
        if (this.formManager && typeof this.formManager.updateAssignmentPreview === 'function') {
            this.formManager.updateAssignmentPreview();
        }

        // Reset processing flag after a short delay
        setTimeout(() => {
            this.processingSubChapter = null;
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
            this.allStudents = [];

            // Load students for each selected group
            for (const group of this.selectedGroups) {
                let students;
                
                // Use API service if available
                if (window.EduTrack?.API?.StudentGroup) {
                    const result = await window.EduTrack.API.StudentGroup.getStudents(group.id);
                    // Normalize both wrapped { success, data } and plain array responses
                    if (Array.isArray(result)) {
                        students = result;
                    } else if (result && Array.isArray(result.data)) {
                        students = result.data;
                    } else if (result && result.success && result.data) {
                        students = result.data;
                    } else {
                        students = [];
                    }
                } else {
                    const response = await fetch(`/Teacher/StudentGroup/GetStudents?groupId=${group.id}`);
                    if (response.ok) {
                        const result = await response.json();
                        // Controller currently returns a plain array; support both shapes
                        if (Array.isArray(result)) {
                            students = result;
                        } else if (result && Array.isArray(result.data)) {
                            students = result.data;
                        } else if (result && result.success && result.data) {
                            students = result.data;
                        } else {
                            students = [];
                        }
                    } else {
                        students = [];
                    }
                }
                
                if (students && students.length > 0) {
                    // Add group info to each student
                    const studentsWithGroup = students.map(student => ({
                        ...student,
                        groupId: group.id,
                        groupName: group.name
                    }));
                    this.allStudents.push(...studentsWithGroup);
                } else {
                }
            }

            // Remove duplicates based on student ID
            this.allStudents = this.allStudents.filter((student, index, self) =>
                index === self.findIndex(s => s.id === student.id)
            );


            // Update selected students with full details from loaded students
            this.updateSelectedStudentsWithDetails();

            this.renderStudents();
            this.updateStudentCount();
        } catch (error) {
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

    }

    renderStudents() {
        const studentGrid = document.getElementById('studentGrid');
        if (!studentGrid) return;

        studentGrid.innerHTML = '';


        this.allStudents.forEach(student => {
            const studentBadge = document.createElement('div');
            studentBadge.className = 'student-badge';
            studentBadge.dataset.studentId = student.id;

            const isSelected = this.selectedStudents.some(s => s.id === student.id);

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
        
        const badge = document.querySelector(`[data-student-id="${student.id}"]`);
        if (!badge) {
            return;
        }
        
        const existingIndex = this.selectedStudents.findIndex(s => s.id === student.id);
        const isSelected = existingIndex > -1;

        if (isSelected) {
            badge.classList.remove('selected');
            this.selectedStudents.splice(existingIndex, 1);
        } else {
            badge.classList.add('selected');
            this.selectedStudents.push(student);
        }
        
        this.updateSelectedStudentsSummary();
        
        // Dispatch assignment changed event
        document.dispatchEvent(new CustomEvent('assignmentChanged', {
            detail: { type: 'student', studentId: student.id, selected: !isSelected }
        }));
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

    // Collect step 3 data for saving
    collectStep3Data() {
        const stepData = {
            GroupId: parseInt(document.getElementById('groupId')?.value) || null
        };
        
        // Add selected groups and subchapters for badge-based selection
        const selectedGroups = this.getSelectedGroups();
        const selectedSubChapters = this.getSelectedSubChapters();
        const selectedStudents = this.getSelectedStudents();
        
        // Always include arrays, even if empty, to ensure proper handling
        stepData.GroupIds = selectedGroups.length > 0 ? selectedGroups.map(g => g.id) : [];
        stepData.SubChapterIds = selectedSubChapters.length > 0 ? selectedSubChapters.map(sc => sc.id) : [];
        stepData.StudentIds = selectedStudents.length > 0 ? selectedStudents.map(s => s.id) : [];

        return stepData;
    }

    // Load step 3 data from existing item
    async loadStepData() {
        if (this.formManager && typeof this.formManager.getExistingItemData === 'function') {
            const existingData = this.formManager.getExistingItemData();
            if (existingData) {
                
                // Load selected groups and subchapters for badge-based selection
                if (existingData.groupIds && existingData.groupIds.length > 0) {
                    const groups = existingData.groupIds.map(id => ({ id: id, name: '' })); // Name will be loaded later
                    this.setSelectedGroups(groups);
                }
                if (existingData.subChapterIds && existingData.subChapterIds.length > 0) {
                    const subChapters = existingData.subChapterIds.map(id => ({ id: id, title: '' })); // Title will be loaded later
                    this.setSelectedSubChapters(subChapters);
                }
                if (existingData.studentIds && existingData.studentIds.length > 0) {
                    const students = existingData.studentIds.map(id => ({ id: id, firstName: '', lastName: '' })); // Details will be loaded later
                    this.setSelectedStudents(students);
                }
                
                // Update UI to show selected items
                this.updateGroupSelectionSummary();
                this.updateSubChapterSelectionSummary();
                this.updateSelectedStudentsSummary();
                this.updateAssignmentPreview();
                this.updateHiddenInputs();
                
                // Load and select existing assignments in UI
                await this.loadAndSelectExistingAssignments();
                
            } else {
            }
        } else {
        }
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
                } else {
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
                } else {
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
        }
    }


}

// Export for use in main form
window.Step3AssignmentManager = Step3AssignmentManager;
