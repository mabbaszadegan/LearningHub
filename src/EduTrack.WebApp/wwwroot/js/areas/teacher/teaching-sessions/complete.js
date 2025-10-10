// Step-by-Step Session Completion JavaScript

// Safe JSON parsing utility
function safeJsonParse(data, fallback = []) {
    if (!data || data === 'null' || data === 'undefined' || data === '' || data === '[]' || data === 'null') {
        return fallback;
    }

    try {
        return JSON.parse(data);
    } catch (error) {
        console.warn('JSON parse error:', error, 'Data:', data);
        return fallback;
    }
}

// Step-by-Step Session Completion Manager
class StepCompletionManager {
    constructor(options = {}) {
        this.sessionId = options.sessionId;
        this.hasPlan = options.hasPlan || false;
        this.groups = options.groups || [];
        this.availableSubTopics = options.availableSubTopics || [];
        this.availableLessons = options.availableLessons || [];
        this.plannedItems = options.plannedItems || [];
        this.currentStep = 1;
        this.completionProgress = null;
        this.stepData = {};
        this.currentActiveTab = 0; // Index of currently active tab
        this.tabData = {}; // Store data for each tab
    }

    async init() {
        await this.loadCompletionProgress();
        this.setupEventListeners();
        this.initializeStepIndicators();
        this.showCurrentStep();
        this.populateStepContent();
    }

    async loadCompletionProgress() {
        try {
            const response = await fetch(`/Teacher/TeachingSessions/GetCompletionProgress?sessionId=${this.sessionId}`);
            const result = await response.json();

            if (result.success) {
                this.completionProgress = result.data;
                this.currentStep = this.completionProgress.currentStep;

                await this.loadStepData(this.currentStep);
            }
        } catch (error) {
            console.error('Error loading completion progress:', error);
        }
    }

    async loadStepData(stepNumber) {
        try {
            const response = await fetch(`/Teacher/TeachingSessions/GetStepData?sessionId=${this.sessionId}&stepNumber=${stepNumber}`);
            const result = await response.json();

            if (result.success) {
                this.stepData[stepNumber] = result.data;
            }
        } catch (error) {
            console.error(`Error loading step ${stepNumber} data:`, error);
        }
    }

    setupEventListeners() {
        // Step navigation
        $('.btn-next-step').on('click', (e) => {
            const nextStep = $(e.target).data('next-step');
            this.goToStep(nextStep);
        });

        $('.btn-prev-step').on('click', (e) => {
            const prevStep = $(e.target).data('prev-step');
            this.goToStep(prevStep);
        });

        // Step saving
        $('.btn-save-step').on('click', (e) => {
            const step = $(e.target).data('step');
            this.saveStep(step);
        });

        // Complete session
        $('#completeSession').on('click', () => {
            this.completeSession();
        });

        // Step indicator clicks
        $(document).on('click', '.step-indicator', (e) => {
            const step = $(e.currentTarget).data('step');
            if (this.canNavigateToStep(step)) {
                this.goToStep(step);
            }
        });
    }

    initializeStepIndicators() {
        const indicatorsContainer = $('#stepIndicators');
        indicatorsContainer.empty();

        const steps = [
            { number: 1, title: 'حضور و غیاب', icon: 'fas fa-user-check' },
            { number: 2, title: 'بازخورد', icon: 'fas fa-comment-alt' },
            { number: 3, title: 'پوشش زیرمباحث', icon: 'fas fa-book-open' }
        ];

        steps.forEach(step => {
            const isCompleted = this.completionProgress?.steps[step.number - 1]?.isCompleted || false;
            const isCurrent = step.number === this.currentStep;
            const canNavigate = this.canNavigateToStep(step.number);

            const indicatorHtml = `
                <div class="step-indicator ${isCompleted ? 'completed' : ''} ${isCurrent ? 'current' : ''} ${canNavigate ? 'clickable' : ''}" 
                     data-step="${step.number}">
                    <div class="step-indicator-icon">
                        <i class="${step.icon}"></i>
                    </div>
                    <div class="step-indicator-content">
                        <span class="step-indicator-number">${step.number}</span>
                        <span class="step-indicator-title">${step.title}</span>
                    </div>
                </div>
            `;

            indicatorsContainer.append(indicatorHtml);
        });

        this.updateProgressBar();
    }

    canNavigateToStep(stepNumber) {
        if (stepNumber === 1) return true;

        // Can navigate to step if previous steps are completed or it's the current step
        for (let i = 1; i < stepNumber; i++) {
            if (!this.completionProgress?.steps[i - 1]?.isCompleted) {
                return false;
            }
        }

        return true;
    }

    updateProgressBar() {
        const completedSteps = this.completionProgress?.steps.filter(s => s.isCompleted).length || 0;
        const progressPercentage = (completedSteps / 3) * 100;
        $('#stepProgressFill').css('width', `${progressPercentage}%`);
    }

    showCurrentStep() {
        $('.step-panel').hide();
        $(`#step-${this.currentStep}`).show();
    }

    goToStep(stepNumber) {
        if (!this.canNavigateToStep(stepNumber)) {
            return;
        }

        this.currentStep = stepNumber;
        this.showCurrentStep();
        this.populateStepContent();
        this.initializeStepIndicators();
    }

    populateStepContent() {
        switch (this.currentStep) {
            case 1:
                this.populateAttendanceStep();
                break;
            case 2:
                this.populateFeedbackStep();
                break;
            case 3:
                this.populateSubChapterCoverageStep();
                break;
        }
    }

    async populateAttendanceStep() {
        const navContainer = $('#attendanceTabsNav');
        const contentContainer = $('#attendanceTabsContent');
        
        navContainer.empty();
        contentContainer.empty();

        console.log('Groups data:', this.groups);
        
        // Debug: Log existing attendance data
        this.groups.forEach((group, groupIndex) => {
            console.log(`Group ${groupIndex}: ${group.name}`);
            group.members.forEach((member, memberIndex) => {
                console.log(`  Member ${memberIndex}: ${member.studentName} (${member.studentId})`);
                console.log(`    ExistingAttendance:`, member.existingAttendance);
            });
        });

        if (this.groups.length === 0) {
            contentContainer.html(`
                <div class="attendance-loading">
                    <div class="loading-spinner">
                        <i class="fas fa-exclamation-circle"></i>
                    </div>
                    <p class="loading-text">هیچ گروهی یافت نشد</p>
                </div>
            `);
            return;
        }

        // Create tab navigation
        this.groups.forEach((group, index) => {
            const completionStatus = this.calculateGroupCompletionStatus(group);
            const tabNavHtml = `
                <div class="attendance-tab-nav-item ${index === 0 ? 'active' : ''}" 
                     data-tab-index="${index}" data-group-id="${group.id}">
                    ${completionStatus.isCompleted ? '<div class="completion-badge"><i class="fas fa-check"></i></div>' : ''}
                    <div class="attendance-tab-icon">
                        <i class="fas fa-users"></i>
                    </div>
                    <div class="attendance-tab-info">
                        <div class="attendance-tab-title">${group.name} (${group.memberCount})</div>
                    </div>
                    <div class="attendance-tab-status ${completionStatus.isCompleted ? 'completed' : 'pending'}">
                        ${completionStatus.text}
                    </div>
                </div>
            `;
            navContainer.append(tabNavHtml);
        });

        // Create tab content panels
        this.groups.forEach((group, index) => {
            const tabContentHtml = `
                <div class="attendance-tab-panel ${index === 0 ? 'active' : ''}" 
                     data-tab-index="${index}" data-group-id="${group.id}">
                    <div class="group-attendance-section" data-group-id="${group.id}">
                        <div class="group-attendance-header">
                            <h4 class="group-attendance-title">
                                <i class="fas fa-users"></i>
                                ${group.name}
                            </h4>
                            <p class="group-attendance-subtitle">${group.memberCount} دانش‌آموز</p>
                            <div class="group-progress">
                                <div class="progress-bar">
                                    <div class="progress-fill" style="width: 0%"></div>
                                </div>
                                <span class="progress-text">0/${group.memberCount} تکمیل شده</span>
                            </div>
                        </div>
                        <div class="students-attendance-list">
                            ${group.members.map(member => {
                                const hasExistingData = member.existingAttendance;
                                const statusValue = hasExistingData ? parseInt(member.existingAttendance.status) : 1;
                                const participationValue = hasExistingData ? member.existingAttendance.participationScore || 100 : 100;
                                const commentValue = hasExistingData ? member.existingAttendance.comment || '' : '';
                                
                                return `
                                <div class="student-attendance-item ${hasExistingData ? 'has-existing-data' : ''}" data-student-id="${member.studentId}">
                                    <div class="student-info">
                                        <div class="student-avatar">
                                            <i class="fas fa-user"></i>
                                            ${hasExistingData ? '<div class="existing-data-indicator"><i class="fas fa-check-circle"></i></div>' : ''}
                                        </div>
                                        <div class="student-details">
                                            <span class="student-name">${member.studentName}</span>
                                            <span class="student-email">${member.studentEmail || ''}</span>
                                            ${hasExistingData ? '<span class="existing-data-label">داده‌های موجود</span>' : ''}
                                        </div>
                                    </div>
                                    <div class="attendance-controls">
                                        <div class="control-group">
                                            <label class="control-label">وضعیت حضور</label>
                                            <select class="attendance-status" data-student-id="${member.studentId}">
                                                <option value="0" ${statusValue === 0 ? 'selected' : ''}>حاضر</option>
                                                <option value="1" ${statusValue === 1 ? 'selected' : ''}>غایب</option>
                                                <option value="2" ${statusValue === 2 ? 'selected' : ''}>تأخیر</option>
                                                <option value="3" ${statusValue === 3 ? 'selected' : ''}>مرخصی</option>
                                            </select>
                                        </div>
                                        <div class="control-group">
                                            <label class="control-label">میزان مشارکت</label>
                                            <div class="participation-slider-container">
                                                <input type="range" class="participation-score" data-student-id="${member.studentId}" 
                                                       min="0" max="100" value="${participationValue}" step="5">
                                                <span class="participation-display">${participationValue}%</span>
                                            </div>
                                        </div>
                                        <div class="control-group">
                                            <label class="control-label">یادداشت</label>
                                            <input type="text" class="attendance-comment" data-student-id="${member.studentId}" 
                                                   value="${commentValue}" placeholder="یادداشت (اختیاری)" maxlength="200">
                                        </div>
                                    </div>
                                </div>
                            `;
                            }).join('')}
                        </div>
                        <div class="group-actions">
                            <button class="btn btn-primary btn-save-tab" data-group-id="${group.id}" data-tab-index="${index}">
                                <i class="fas fa-save"></i>
                                ذخیره حضور و غیاب ${group.name}
                            </button>
                        </div>
                    </div>
                </div>
            `;
            contentContainer.append(tabContentHtml);
        });

        // Setup event listeners
        this.setupTabEventListeners();
        this.setupAttendanceEventListeners();

        // Load existing data if available
        if (this.stepData[1]) {
            this.loadAttendanceData(this.stepData[1].completionData);
        }

        // Initialize first tab
        this.currentActiveTab = 0;
        this.updateTabProgress(0);
    }


    setupTabEventListeners() {
        // Tab navigation clicks
        $(document).on('click', '.attendance-tab-nav-item', (e) => {
            const tabIndex = parseInt($(e.currentTarget).data('tab-index'));
            this.switchTab(tabIndex);
        });

        // Individual tab save button
        $(document).on('click', '.btn-save-tab', (e) => {
            e.preventDefault();
            const tabIndex = parseInt($(e.target).data('tab-index'));
            const groupId = $(e.target).data('group-id');
            this.saveTabData(tabIndex, groupId);
        });
    }

    setupFeedbackTabEventListeners() {
        // Feedback tab navigation click events
        $(document).on('click', '.feedback-tab-nav-item', (e) => {
            const tabIndex = parseInt($(e.currentTarget).data('tab-index'));
            this.switchFeedbackTab(tabIndex);
        });

        // Save feedback button click events
        $(document).on('click', '.btn-save-feedback', (e) => {
            e.preventDefault();
            const groupId = parseInt($(e.target).data('group-id'));
            this.saveGroupFeedback(groupId);
        });
    }

    setupAttendanceEventListeners() {
        // Participation score slider updates
        $(document).on('input', '.participation-score', (e) => {
            const value = $(e.target).val();
            $(e.target).siblings('.participation-display').text(`${value}%`);
            const tabIndex = parseInt($(e.target).closest('.attendance-tab-panel').data('tab-index'));
            this.updateTabProgress(tabIndex);
        });

        // Attendance status changes
        $(document).on('change', '.attendance-status', (e) => {
            const tabIndex = parseInt($(e.target).closest('.attendance-tab-panel').data('tab-index'));
            this.updateTabProgress(tabIndex);
        });

        // Comment changes
        $(document).on('input', '.attendance-comment', (e) => {
            const tabIndex = parseInt($(e.target).closest('.attendance-tab-panel').data('tab-index'));
            this.updateTabProgress(tabIndex);
        });
    }

    switchTab(tabIndex) {
        if (tabIndex === this.currentActiveTab) return;

        // Update navigation
        $('.attendance-tab-nav-item').removeClass('active');
        $(`.attendance-tab-nav-item[data-tab-index="${tabIndex}"]`).addClass('active');

        // Update content
        $('.attendance-tab-panel').removeClass('active');
        $(`.attendance-tab-panel[data-tab-index="${tabIndex}"]`).addClass('active');

        this.currentActiveTab = tabIndex;
    }

    switchFeedbackTab(tabIndex) {
        // Remove active class from all feedback tabs and panels
        $('.feedback-tab-nav-item').removeClass('active');
        $('.feedback-tab-panel').removeClass('active');

        // Add active class to selected tab and panel
        $(`.feedback-tab-nav-item[data-tab-index="${tabIndex}"]`).addClass('active');
        $(`.feedback-tab-panel[data-tab-index="${tabIndex}"]`).addClass('active');
    }

    updateFeedbackTabStatus(groupId) {
        const group = this.groups.find(g => g.id === groupId);
        if (!group) return;

        const hasExistingFeedback = group.existingFeedback && 
            (group.existingFeedback.understandingLevel || 
             group.existingFeedback.participationLevel || 
             group.existingFeedback.teacherSatisfaction || 
             group.existingFeedback.groupFeedback || 
             group.existingFeedback.challenges || 
             group.existingFeedback.nextSessionRecommendations);

        // Find the tab index for this group
        const tabIndex = this.groups.findIndex(g => g.id === groupId);
        if (tabIndex === -1) return;

        // Update tab status
        const tabNavItem = $(`.feedback-tab-nav-item[data-tab-index="${tabIndex}"]`);
        const statusElement = tabNavItem.find('.feedback-tab-status');
        
        if (hasExistingFeedback) {
            statusElement.removeClass('pending').addClass('completed').text('تکمیل شده');
            
            // Add completion badge if not exists
            if (tabNavItem.find('.completion-badge').length === 0) {
                tabNavItem.append('<div class="completion-badge"><i class="fas fa-check"></i></div>');
            }
        } else {
            statusElement.removeClass('completed').addClass('pending').text('در انتظار');
            tabNavItem.find('.completion-badge').remove();
        }
    }

    async saveGroupFeedback(groupId) {
        try {
            const group = this.groups.find(g => g.id === groupId);
            if (!group) {
                this.showNotification('گروه یافت نشد', 'error');
                return;
            }

            // Collect feedback data for this group
            const understandingLevel = $(`.understanding-level[data-group-id="${groupId}"]`).val();
            const participationLevel = $(`.participation-level[data-group-id="${groupId}"]`).val();
            const teacherSatisfaction = $(`.teacher-satisfaction[data-group-id="${groupId}"]`).val();
            const groupFeedback = $(`.group-feedback[data-group-id="${groupId}"]`).val();
            const challenges = $(`.challenges[data-group-id="${groupId}"]`).val();
            const nextSessionRecommendations = $(`.next-session-recommendations[data-group-id="${groupId}"]`).val();

            const feedbackData = {
                sessionId: this.sessionId,
                groupFeedbacks: [{
                    groupId: groupId,
                    groupName: group.name,
                    understandingLevel: parseInt(understandingLevel),
                    participationLevel: parseInt(participationLevel),
                    teacherSatisfaction: parseInt(teacherSatisfaction),
                    groupFeedback: groupFeedback || null,
                    challenges: challenges || null,
                    nextSessionRecommendations: nextSessionRecommendations || null
                }]
            };

            // Show loading state
            const saveButton = $(`.btn-save-feedback[data-group-id="${groupId}"]`);
            const originalText = saveButton.html();
            saveButton.html('<i class="fas fa-spinner fa-spin"></i> در حال ذخیره...').prop('disabled', true);

            // Send request
            const response = await fetch('/Teacher/TeachingSessions/SaveFeedbackStep', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(feedbackData)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.showNotification(`بازخورد گروه ${group.name} با موفقیت ذخیره شد`, 'success');
                
                // Update button state
                saveButton.html('<i class="fas fa-check"></i> ذخیره شده').removeClass('btn-primary').addClass('btn-success');
                
                // Update group data
                if (!group.existingFeedback) {
                    group.existingFeedback = {};
                }
                group.existingFeedback.understandingLevel = parseInt(understandingLevel);
                group.existingFeedback.participationLevel = parseInt(participationLevel);
                group.existingFeedback.teacherSatisfaction = parseInt(teacherSatisfaction);
                group.existingFeedback.groupFeedback = groupFeedback;
                group.existingFeedback.challenges = challenges;
                group.existingFeedback.nextSessionRecommendations = nextSessionRecommendations;

                // Update tab status
                this.updateFeedbackTabStatus(groupId);
            } else {
                this.showNotification(result.message || 'خطا در ذخیره بازخورد', 'error');
                saveButton.html(originalText).prop('disabled', false);
            }
        } catch (error) {
            console.error('Error saving group feedback:', error);
            this.showNotification('خطا در ذخیره بازخورد', 'error');
            
            // Reset button state
            const saveButton = $(`.btn-save-feedback[data-group-id="${groupId}"]`);
            saveButton.html('<i class="fas fa-save"></i> ذخیره بازخورد').prop('disabled', false);
        }
    }

    updateTabProgress(tabIndex) {
        const tabPanel = $(`.attendance-tab-panel[data-tab-index="${tabIndex}"]`);
        const groupSection = tabPanel.find('.group-attendance-section');
        const students = groupSection.find('.student-attendance-item');
        let completedCount = 0;

        students.each((index, studentElement) => {
            const $student = $(studentElement);
            const status = $student.find('.attendance-status').val();
            const participation = $student.find('.participation-score').val();
            const comment = $student.find('.attendance-comment').val().trim();

            // Consider completed if status is not absent (0) or has any data
            if (status !== '0' || participation !== '0' || comment !== '') {
                completedCount++;
            }
        });

        const totalStudents = students.length;
        const progressPercentage = totalStudents > 0 ? (completedCount / totalStudents) * 100 : 0;
        
        // Update progress bar instantly without animation
        groupSection.find('.progress-fill').css('width', `${progressPercentage}%`);
        groupSection.find('.progress-text').text(`${completedCount}/${totalStudents} تکمیل شده`);

        // Update tab navigation status
        const tabNavItem = $(`.attendance-tab-nav-item[data-tab-index="${tabIndex}"]`);
        const statusElement = tabNavItem.find('.attendance-tab-status');
        
        if (completedCount === totalStudents) {
            statusElement.text('تکمیل شده').removeClass('pending').addClass('completed');
            tabNavItem.addClass('completed');
            groupSection.addClass('completed');
            
            // Add completion badge if not exists
            if (!tabNavItem.find('.completion-badge').length) {
                tabNavItem.prepend('<div class="completion-badge"><i class="fas fa-check"></i></div>');
            }
        } else if (completedCount > 0) {
            statusElement.text(`${completedCount}/${totalStudents} تکمیل شده`).removeClass('pending completed');
            tabNavItem.removeClass('completed');
            groupSection.removeClass('completed');
            
            // Remove completion badge
            tabNavItem.find('.completion-badge').remove();
        } else {
            statusElement.text('در انتظار').removeClass('completed').addClass('pending');
            tabNavItem.removeClass('completed');
            groupSection.removeClass('completed');
            
            // Remove completion badge
            tabNavItem.find('.completion-badge').remove();
        }
    }

    updateGroupProgress(groupSection) {
        const groupId = groupSection.data('group-id');
        const students = groupSection.find('.student-attendance-item');
        let completedCount = 0;

        students.each((index, studentElement) => {
            const $student = $(studentElement);
            const status = $student.find('.attendance-status').val();
            const participation = $student.find('.participation-score').val();
            const comment = $student.find('.attendance-comment').val().trim();

            // Consider completed if status is not absent (0) or has any data
            if (status !== '0' || participation !== '0' || comment !== '') {
                completedCount++;
            }
        });

        const totalStudents = students.length;
        const progressPercentage = totalStudents > 0 ? (completedCount / totalStudents) * 100 : 0;
        
        // Update progress bar instantly without animation
        groupSection.find('.progress-fill').css('width', `${progressPercentage}%`);
        
        groupSection.find('.progress-text').text(`${completedCount}/${totalStudents} تکمیل شده`);

        // Update visual state
        if (completedCount === totalStudents) {
            groupSection.addClass('completed');
            groupSection.find('.btn-complete-group').addClass('btn-success').removeClass('btn-primary');
            groupSection.find('.btn-complete-group').html('<i class="fas fa-check"></i> تکمیل شده');
        } else {
            groupSection.removeClass('completed');
            groupSection.find('.btn-complete-group').removeClass('btn-success').addClass('btn-primary');
            groupSection.find('.btn-complete-group').html(`<i class="fas fa-check"></i> تکمیل حضور و غیاب ${groupSection.find('.group-attendance-title').text().replace('👥 ', '')}`);
        }
    }

    completeGroupAttendance(groupId) {
        const groupSection = $(`.group-attendance-section[data-group-id="${groupId}"]`);
        const groupName = groupSection.find('.group-attendance-title').text().replace('👥 ', '');
        
        // Show completion animation
        groupSection.addClass('completing');
        
        // Simulate completion process
        setTimeout(() => {
            groupSection.removeClass('completing').addClass('completed');
            groupSection.find('.btn-complete-group').addClass('btn-success').removeClass('btn-primary');
            groupSection.find('.btn-complete-group').html('<i class="fas fa-check"></i> تکمیل شده');
            
            // Show success message
            this.showNotification(`حضور و غیاب گروه ${groupName} با موفقیت تکمیل شد`, 'success');
            
            // Check if all groups are completed
            this.checkAllGroupsCompleted();
        }, 1000);
    }

    checkAllGroupsCompleted() {
        const allGroups = $('.group-attendance-section');
        const completedGroups = $('.group-attendance-section.completed');
        
        if (allGroups.length === completedGroups.length) {
            // All groups completed
            this.showNotification('همه گروه‌ها تکمیل شدند! می‌توانید به مرحله بعد بروید.', 'success');
            
            // Enable next step button
            $('.btn-next-step').prop('disabled', false).removeClass('disabled');
        }
    }

    showNotification(message, type = 'info') {
        const notification = $(`
            <div class="notification notification-${type}">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'info-circle'}"></i>
                <span>${message}</span>
            </div>
        `);
        
        $('body').append(notification);
        
        // Animate in
        notification.css({ opacity: 0, transform: 'translateY(-20px)' });
        notification.animate({ opacity: 1 }, 300).css('transform', 'translateY(0)');
        
        // Auto remove after 3 seconds
        setTimeout(() => {
            notification.animate({ opacity: 0 }, 300, () => {
                notification.remove();
            });
        }, 3000);
    }

    populateFeedbackStep() {
        const navContainer = $('#feedbackTabsNav');
        const contentContainer = $('#feedbackTabsContent');
        
        navContainer.empty();
        contentContainer.empty();

        if (this.groups.length === 0) {
            contentContainer.html(`
                <div class="feedback-loading">
                    <div class="loading-spinner">
                        <i class="fas fa-exclamation-circle"></i>
                        <p>هیچ گروهی یافت نشد</p>
                    </div>
                </div>
            `);
            return;
        }

        // Create tab navigation
        this.groups.forEach((group, index) => {
            const hasExistingFeedback = group.existingFeedback && 
                (group.existingFeedback.understandingLevel || 
                 group.existingFeedback.participationLevel || 
                 group.existingFeedback.teacherSatisfaction || 
                 group.existingFeedback.groupFeedback || 
                 group.existingFeedback.challenges || 
                 group.existingFeedback.nextSessionRecommendations);
            
            const tabNavHtml = `
                <div class="feedback-tab-nav-item ${index === 0 ? 'active' : ''}"
                     data-tab-index="${index}" data-group-id="${group.id}">
                    <div class="feedback-tab-icon">
                        <i class="fas fa-comments"></i>
                    </div>
                    <div class="feedback-tab-info">
                        <div class="feedback-tab-title">${group.name} (${group.memberCount})</div>
                    </div>
                    <div class="feedback-tab-status ${hasExistingFeedback ? 'completed' : 'pending'}">
                        ${hasExistingFeedback ? 'تکمیل شده' : 'در انتظار'}
                    </div>
                    ${hasExistingFeedback ? '<div class="completion-badge"><i class="fas fa-check"></i></div>' : ''}
                </div>
            `;
            navContainer.append(tabNavHtml);
        });

        // Create tab content panels
        this.groups.forEach((group, index) => {
            const tabContentHtml = `
                <div class="feedback-tab-panel ${index === 0 ? 'active' : ''}" 
                     data-tab-index="${index}" data-group-id="${group.id}">
                    <div class="group-feedback-section" data-group-id="${group.id}">
                        <div class="group-feedback-header">
                            <h4 class="group-feedback-title">
                                <i class="fas fa-comments"></i>
                                ${group.name}
                            </h4>
                            <p class="group-feedback-subtitle">${group.memberCount} دانش‌آموز</p>
                        </div>
                        <div class="feedback-controls">
                            <div class="rating-groups-container">
                                <div class="rating-group">
                                    <label class="rating-label">سطح درک مطلب</label>
                                    <div class="rating-control">
                                        <input type="range" class="rating-slider understanding-level" min="1" max="5" value="3" data-group-id="${group.id}">
                                        <span class="rating-display">3</span>
                                    </div>
                                </div>
                                <div class="rating-group">
                                    <label class="rating-label">سطح مشارکت</label>
                                    <div class="rating-control">
                                        <input type="range" class="rating-slider participation-level" min="1" max="5" value="3" data-group-id="${group.id}">
                                        <span class="rating-display">3</span>
                                    </div>
                                </div>
                                <div class="rating-group">
                                    <label class="rating-label">میزان رضایت معلم</label>
                                    <div class="rating-control">
                                        <input type="range" class="rating-slider teacher-satisfaction" min="1" max="5" value="3" data-group-id="${group.id}">
                                        <span class="rating-display">3</span>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="form-label">بازخورد کلی</label>
                                <textarea class="form-control group-feedback" data-group-id="${group.id}" 
                                          placeholder="بازخورد کلی درباره عملکرد گروه..."></textarea>
                            </div>
                            <div class="form-group">
                                <label class="form-label">چالش‌ها</label>
                                <textarea class="form-control challenges" data-group-id="${group.id}" 
                                          placeholder="چالش‌ها و مشکلات پیش آمده..."></textarea>
                            </div>
                            <div class="form-group">
                                <label class="form-label">پیشنهادات جلسه بعد</label>
                                <textarea class="form-control next-session-recommendations" data-group-id="${group.id}" 
                                          placeholder="پیشنهادات برای جلسه بعد..."></textarea>
                            </div>
                            <div class="group-actions">
                                <button type="button" class="btn btn-primary btn-save-feedback" data-group-id="${group.id}">
                                    <i class="fas fa-save"></i>
                                    ذخیره بازخورد
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            contentContainer.append(tabContentHtml);
        });

        // Setup tab event listeners
        this.setupFeedbackTabEventListeners();

        // Setup rating slider events
        $('.rating-slider').on('input', (e) => {
            const value = $(e.target).val();
            $(e.target).siblings('.rating-display').text(value);
        });

        // Load existing feedback data
        this.loadFeedbackData();
    }

    populateTopicCoverageStep() {
        const container = $('#topicCoverageContainer');
        container.empty();

        this.groups.forEach(group => {
            const groupHtml = `
                <div class="group-topic-coverage-section" data-group-id="${group.id}">
                    <div class="group-topic-coverage-header">
                        <h4 class="group-topic-coverage-title">${group.name}</h4>
                    </div>
                    <div class="topic-coverage-content">
                        <div class="subtopics-coverage">
                            <h5>زیرمباحث</h5>
                            <div class="topic-coverage-list">
                                ${this.availableSubTopics.map(subTopic => `
                                    <div class="topic-coverage-item" data-topic-type="SubTopic" data-topic-id="${subTopic.id}">
                                        <div class="topic-info">
                                            <span class="topic-title">${subTopic.title}</span>
                                        </div>
                                        <div class="coverage-controls">
                                            <label class="coverage-checkbox">
                                                <input type="checkbox" class="coverage-check" data-group-id="${group.id}" data-topic-type="SubTopic" data-topic-id="${subTopic.id}">
                                                <span>پوشش داده شد</span>
                                            </label>
                                            <input type="range" class="coverage-percentage" min="0" max="100" value="0" 
                                                   data-group-id="${group.id}" data-topic-type="SubTopic" data-topic-id="${subTopic.id}">
                                            <span class="coverage-display">0%</span>
                                        </div>
                                    </div>
                                `).join('')}
                            </div>
                        </div>
                        <div class="lessons-coverage">
                            <h5>درس‌ها</h5>
                            <div class="topic-coverage-list">
                                ${this.availableLessons.map(lesson => `
                                    <div class="topic-coverage-item" data-topic-type="Lesson" data-topic-id="${lesson.id}">
                                        <div class="topic-info">
                                            <span class="topic-title">${lesson.title}</span>
                                        </div>
                                        <div class="coverage-controls">
                                            <label class="coverage-checkbox">
                                                <input type="checkbox" class="coverage-check" data-group-id="${group.id}" data-topic-type="Lesson" data-topic-id="${lesson.id}">
                                                <span>پوشش داده شد</span>
                                            </label>
                                            <input type="range" class="coverage-percentage" min="0" max="100" value="0" 
                                                   data-group-id="${group.id}" data-topic-type="Lesson" data-topic-id="${lesson.id}">
                                            <span class="coverage-display">0%</span>
                                        </div>
                                    </div>
                                `).join('')}
                            </div>
                        </div>
                    </div>
                </div>
            `;
            container.append(groupHtml);
        });

        // Setup coverage percentage events
        $('.coverage-percentage').on('input', (e) => {
            const value = $(e.target).val();
            $(e.target).siblings('.coverage-display').text(value + '%');
        });

        // Load existing data if available
        if (this.stepData[3]) {
            this.loadTopicCoverageData(this.stepData[3].completionData);
        }
    }

    async populateSubChapterCoverageStep() {
        const navContainer = $('#subchapterCoverageTabsNav');
        const contentContainer = $('#subchapterCoverageTabsContent');
        
        navContainer.empty();
        contentContainer.empty();

        if (this.groups.length === 0) {
            contentContainer.html(`
                <div class="subchapter-coverage-loading">
                    <div class="loading-spinner">
                        <i class="fas fa-exclamation-circle"></i>
                    </div>
                    <p class="loading-text">هیچ گروهی یافت نشد</p>
                </div>
            `);
            return;
        }

        // Create tab navigation (vertical layout like attendance and feedback)
        this.groups.forEach((group, index) => {
            const completionStatus = this.calculateSubChapterGroupCompletionStatus(group);
            const tabNavHtml = `
                <div class="subchapter-coverage-tab-nav-item ${index === 0 ? 'active' : ''}" 
                     data-tab-index="${index}" data-group-id="${group.id}">
                    ${completionStatus.isCompleted ? '<div class="completion-badge"><i class="fas fa-check"></i></div>' : ''}
                    <div class="subchapter-coverage-tab-icon">
                        <i class="fas fa-users"></i>
                    </div>
                    <div class="subchapter-coverage-tab-info">
                        <div class="subchapter-coverage-tab-title">${group.name} (${group.memberCount})</div>
                    </div>
                    <div class="subchapter-coverage-tab-status ${completionStatus.isCompleted ? 'completed' : 'pending'}">
                        ${completionStatus.text}
                    </div>
                </div>
            `;
            navContainer.append(tabNavHtml);
        });

        // Create tab content panels
        this.groups.forEach((group, index) => {
            const tabContentHtml = `
                <div class="subchapter-coverage-tab-panel ${index === 0 ? 'active' : ''}" 
                     data-tab-index="${index}" data-group-id="${group.id}">
                    <div class="group-subchapter-coverage-section" data-group-id="${group.id}">
                        <div class="group-subchapter-coverage-header">
                            <h4 class="group-subchapter-coverage-title">
                                <i class="fas fa-book-open"></i>
                                ${group.name}
                            </h4>
                            <p class="group-subchapter-coverage-subtitle">${group.memberCount} دانش‌آموز</p>
                            <div class="group-progress">
                                <div class="progress-bar">
                                    <div class="progress-fill" style="width: 0%"></div>
                                </div>
                                <span class="progress-text">0% تکمیل شده</span>
                            </div>
                        </div>
                        <div class="chapters-container" id="chaptersContainer_${group.id}">
                            <!-- Chapters will be populated by JavaScript -->
                        </div>
                        <div class="group-actions">
                            <button class="btn btn-primary btn-save-tab" data-group-id="${group.id}" data-tab-index="${index}">
                                <i class="fas fa-save"></i>
                                ذخیره پوشش زیرمباحث ${group.name}
                            </button>
                        </div>
                    </div>
                </div>
            `;
            contentContainer.append(tabContentHtml);
        });

        // Setup event listeners
        this.setupSubChapterCoverageTabEventListeners();
        this.setupSubChapterCoverageEventListeners();

        // Load chapters data and existing coverage data
        await this.loadChaptersData();

        // Also load step completion data if available (for backward compatibility)
        if (this.stepData[3]) {
            this.loadSubChapterCoverageData(this.stepData[3].completionData);
        }

        // Initialize first tab
        this.currentActiveTab = 0;
        this.updateSubChapterCoverageTabProgress(0);
        
        // Ensure all subchapters are closed by default
        this.closeAllSubchapters();
    }

    closeAllSubchapters() {
        // Close all subchapters and reset toggle icons
        $('.subchapters-container').hide();
        $('.chapter-toggle').removeClass('fa-chevron-up').addClass('fa-chevron-down');
        $('.subchapter-card-body').hide();
        $('.subchapter-toggle').removeClass('fa-chevron-up').addClass('fa-chevron-down');
    }

    closeAllSubChapterCards() {
        // Close all subchapter card bodies but keep headers visible
        $('.subchapter-card-body').slideUp();
        $('.subchapter-toggle').removeClass('fa-chevron-up').addClass('fa-chevron-down');
    }

    async saveTabData(tabIndex, groupId) {
        try {
            const group = this.groups[tabIndex];
            if (!group) {
                this.showErrorMessage('گروه یافت نشد');
                return;
            }


            const tabData = this.collectTabAttendanceData(tabIndex, groupId);
            
            console.log('Saving tab data:', {
                sessionId: this.sessionId,
                groupId: groupId,
                tabIndex: tabIndex,
                tabData: tabData
            });
            
            const response = await fetch(`/Teacher/TeachingSessions/SaveTabCompletion`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    sessionId: this.sessionId,
                    groupId: groupId,
                    tabIndex: tabIndex,
                    completionData: JSON.stringify(tabData),
                    isCompleted: true
                })
            });

            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('Save tab response:', result);

            if (result.success) {
                this.showNotification(`حضور و غیاب گروه ${group.name} با موفقیت ذخیره شد`, 'success');
                
                // Update tab status
                const tabNavItem = $(`.attendance-tab-nav-item[data-tab-index="${tabIndex}"]`);
                tabNavItem.addClass('completed');
                tabNavItem.find('.attendance-tab-status').text('ذخیره شده').addClass('completed');
                
                // Store tab data
                this.tabData[tabIndex] = tabData;
                
                // Check if all tabs are completed
                this.checkAllTabsCompleted();
            } else {
                this.showErrorMessage(result.message);
            }
        } catch (error) {
            console.error('Error saving tab data:', error);
            this.showErrorMessage('خطا در ذخیره داده‌های تب: ' + error.message);
        }
    }

    async saveStep(stepNumber) {
        try {
            let stepData;

            switch (stepNumber) {
                case 1:
                    stepData = this.collectAttendanceData();
                    break;
                case 2:
                    stepData = this.collectFeedbackData();
                    break;
                case 3:
                    stepData = this.collectSubChapterCoverageData();
                    break;
            }

            const response = await fetch(`/Teacher/TeachingSessions/SaveStepCompletion`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    sessionId: this.sessionId,
                    stepNumber: stepNumber,
                    stepName: this.getStepName(stepNumber),
                    completionData: JSON.stringify(stepData),
                    isCompleted: true
                })
            });

            const result = await response.json();

            if (result.success) {
                this.showSuccessMessage(result.message);
                await this.loadCompletionProgress();
                this.initializeStepIndicators();
            } else {
                this.showErrorMessage(result.message);
            }
        } catch (error) {
            console.error('Error saving step:', error);
            this.showErrorMessage('خطا در ذخیره مرحله');
        }
    }

    collectTabAttendanceData(tabIndex, groupId) {
        const group = this.groups[tabIndex];
        if (!group) return null;

        const groupAttendance = {
            groupId: group.id,
            groupName: group.name,
            students: []
        };

        group.members.forEach(member => {
            const status = $(`.attendance-status[data-student-id="${member.studentId}"]`).val();
            const participationScore = $(`.participation-score[data-student-id="${member.studentId}"]`).val();
            const comment = $(`.attendance-comment[data-student-id="${member.studentId}"]`).val();

            groupAttendance.students.push({
                studentId: member.studentId, // Use GroupMember.StudentId (GUID) for proper relationship
                studentName: member.studentName,
                status: parseInt(status),
                participationScore: participationScore ? parseFloat(participationScore) : null,
                comment: comment || null
            });
        });

        // Return in AttendanceStepDataDto format
        return {
            sessionId: this.sessionId,
            groupAttendances: [groupAttendance]
        };
    }

    collectAttendanceData() {
        const attendanceData = {
            sessionId: this.sessionId,
            groupAttendances: []
        };

        this.groups.forEach((group, index) => {
            // Use saved tab data if available, otherwise collect current data
            if (this.tabData[index]) {
                // tabData[index] is already in AttendanceStepDataDto format
                attendanceData.groupAttendances.push(...this.tabData[index].groupAttendances);
            } else {
                const tabData = this.collectTabAttendanceData(index, group.id);
                if (tabData) {
                    attendanceData.groupAttendances.push(...tabData.groupAttendances);
                }
            }
        });

        return attendanceData;
    }

    checkAllTabsCompleted() {
        const allTabs = $('.attendance-tab-nav-item');
        const completedTabs = $('.attendance-tab-nav-item.completed');
        
        if (allTabs.length === completedTabs.length) {
            // All tabs completed
            this.showNotification('همه گروه‌ها تکمیل شدند! می‌توانید به مرحله بعد بروید.', 'success');
            
            // Enable next step button
            $('.btn-next-step').prop('disabled', false).removeClass('disabled');
        }
    }

    collectFeedbackData() {
        const feedbackData = {
            sessionId: this.sessionId,
            groupFeedbacks: []
        };

        this.groups.forEach(group => {
            const understandingLevel = $(`.understanding-level[data-group-id="${group.id}"]`).val();
            const participationLevel = $(`.participation-level[data-group-id="${group.id}"]`).val();
            const teacherSatisfaction = $(`.teacher-satisfaction[data-group-id="${group.id}"]`).val();
            const groupFeedback = $(`.group-feedback[data-group-id="${group.id}"]`).val();
            const challenges = $(`.challenges[data-group-id="${group.id}"]`).val();
            const nextSessionRecommendations = $(`.next-session-recommendations[data-group-id="${group.id}"]`).val();

            feedbackData.groupFeedbacks.push({
                groupId: group.id,
                groupName: group.name,
                understandingLevel: parseInt(understandingLevel),
                participationLevel: parseInt(participationLevel),
                teacherSatisfaction: parseInt(teacherSatisfaction),
                groupFeedback: groupFeedback || null,
                challenges: challenges || null,
                nextSessionRecommendations: nextSessionRecommendations || null
            });
        });

        return feedbackData;
    }

    collectTopicCoverageData() {
        const topicCoverageData = {
            sessionId: this.sessionId,
            groupTopicCoverages: []
        };

        this.groups.forEach(group => {
            const groupTopicCoverage = {
                groupId: group.id,
                groupName: group.name,
                subTopicCoverages: [],
                lessonCoverages: []
            };

            // Collect subtopic coverages
            this.availableSubTopics.forEach(subTopic => {
                const wasCovered = $(`.coverage-check[data-group-id="${group.id}"][data-topic-type="SubTopic"][data-topic-id="${subTopic.id}"]`).is(':checked');
                const coveragePercentage = $(`.coverage-percentage[data-group-id="${group.id}"][data-topic-type="SubTopic"][data-topic-id="${subTopic.id}"]`).val();

                groupTopicCoverage.subTopicCoverages.push({
                    topicId: subTopic.id,
                    topicTitle: subTopic.title,
                    wasPlanned: false, // TODO: Check if was planned
                    wasCovered: wasCovered,
                    coveragePercentage: parseInt(coveragePercentage),
                    teacherNotes: null,
                    challenges: null
                });
            });

            // Collect lesson coverages
            this.availableLessons.forEach(lesson => {
                const wasCovered = $(`.coverage-check[data-group-id="${group.id}"][data-topic-type="Lesson"][data-topic-id="${lesson.id}"]`).is(':checked');
                const coveragePercentage = $(`.coverage-percentage[data-group-id="${group.id}"][data-topic-type="Lesson"][data-topic-id="${lesson.id}"]`).val();

                groupTopicCoverage.lessonCoverages.push({
                    topicId: lesson.id,
                    topicTitle: lesson.title,
                    wasPlanned: false, // TODO: Check if was planned
                    wasCovered: wasCovered,
                    coveragePercentage: parseInt(coveragePercentage),
                    teacherNotes: null,
                    challenges: null
                });
            });

            topicCoverageData.groupTopicCoverages.push(groupTopicCoverage);
        });

        return topicCoverageData;
    }

    async completeSession() {
        try {
            // Save the final step first
            await this.saveStep(3);

            // Mark session as completed
            const response = await fetch(`/Teacher/TeachingSessions/SaveStepCompletion`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    sessionId: this.sessionId,
                    stepNumber: 3,
                    stepName: 'subchapter-coverage',
                    completionData: JSON.stringify(this.collectSubChapterCoverageData()),
                    isCompleted: true
                })
            });

            const result = await response.json();

            if (result.success) {
                this.showCompletionSummary();
            } else {
                this.showErrorMessage(result.message);
            }
        } catch (error) {
            console.error('Error completing session:', error);
            this.showErrorMessage('خطا در تکمیل گزارش');
        }
    }

    showCompletionSummary() {
        $('.step-content').hide();
        $('#completionSummary').show();
    }

    getStepName(stepNumber) {
        const stepNames = {
            1: 'attendance',
            2: 'feedback',
            3: 'subchapter-coverage'
        };
        return stepNames[stepNumber] || 'unknown';
    }

    calculateGroupCompletionStatus(group) {
        if (!group.members || group.members.length === 0) {
            return { isCompleted: false, text: 'در انتظار', completedCount: 0, totalCount: 0 };
        }

        const totalCount = group.members.length;
        const completedCount = group.members.filter(member => member.existingAttendance).length;
        
        if (completedCount === 0) {
            return { isCompleted: false, text: 'در انتظار', completedCount, totalCount };
        } else if (completedCount === totalCount) {
            return { isCompleted: true, text: 'تکمیل شده', completedCount, totalCount };
        } else {
            return { isCompleted: false, text: `${completedCount}/${totalCount} تکمیل شده`, completedCount, totalCount };
        }
    }

    loadAttendanceData(data) {
        // Load existing attendance data
        if (data) {
            const attendanceData = JSON.parse(data);
            attendanceData.groupAttendances.forEach(groupAttendance => {
                groupAttendance.students.forEach(student => {
                    $(`.attendance-status[data-student-id="${student.studentId}"]`).val(student.status);
                    $(`.participation-score[data-student-id="${student.studentId}"]`).val(student.participationScore || '');
                    $(`.attendance-comment[data-student-id="${student.studentId}"]`).val(student.comment || '');
                });
            });
        }
    }

    loadFeedbackData(data) {
        // Load existing feedback data from groups (database)
        if (this.groups && this.groups.length > 0) {
            this.groups.forEach(group => {
                if (group.existingFeedback) {
                    const feedback = group.existingFeedback;
                    
                    // Update rating sliders and displays
                    $(`.understanding-level[data-group-id="${group.id}"]`).val(feedback.understandingLevel);
                    $(`.understanding-level[data-group-id="${group.id}"]`).siblings('.rating-display').text(feedback.understandingLevel);
                    
                    $(`.participation-level[data-group-id="${group.id}"]`).val(feedback.participationLevel);
                    $(`.participation-level[data-group-id="${group.id}"]`).siblings('.rating-display').text(feedback.participationLevel);
                    
                    $(`.teacher-satisfaction[data-group-id="${group.id}"]`).val(feedback.teacherSatisfaction || 3);
                    $(`.teacher-satisfaction[data-group-id="${group.id}"]`).siblings('.rating-display').text(feedback.teacherSatisfaction || 3);
                    
                    // Update text areas
                    $(`.group-feedback[data-group-id="${group.id}"]`).val(feedback.groupFeedback || '');
                    $(`.challenges[data-group-id="${group.id}"]`).val(feedback.challenges || '');
                    $(`.next-session-recommendations[data-group-id="${group.id}"]`).val(feedback.nextSessionRecommendations || '');
                }
            });
        }
        
        // Also try to load from JSON data (fallback)
        if (data) {
            try {
                const feedbackData = JSON.parse(data);
                if (feedbackData.groupFeedbacks) {
                    feedbackData.groupFeedbacks.forEach(groupFeedback => {
                        // Update rating sliders and displays
                        $(`.understanding-level[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.understandingLevel);
                        $(`.understanding-level[data-group-id="${groupFeedback.groupId}"]`).siblings('.rating-display').text(groupFeedback.understandingLevel);
                        
                        $(`.participation-level[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.participationLevel);
                        $(`.participation-level[data-group-id="${groupFeedback.groupId}"]`).siblings('.rating-display').text(groupFeedback.participationLevel);
                        
                        $(`.teacher-satisfaction[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.teacherSatisfaction || 3);
                        $(`.teacher-satisfaction[data-group-id="${groupFeedback.groupId}"]`).siblings('.rating-display').text(groupFeedback.teacherSatisfaction || 3);
                        
                        // Update text areas
                        $(`.group-feedback[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.groupFeedback || '');
                        $(`.challenges[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.challenges || '');
                        $(`.next-session-recommendations[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.nextSessionRecommendations || '');
                    });
                }
            } catch (e) {
                console.log('Error parsing feedback data:', e);
            }
        }
    }

    loadTopicCoverageData(data) {
        // Load existing topic coverage data
        if (data) {
            const topicCoverageData = JSON.parse(data);
            topicCoverageData.groupTopicCoverages.forEach(groupCoverage => {
                groupCoverage.subTopicCoverages.forEach(coverage => {
                    $(`.coverage-check[data-group-id="${groupCoverage.groupId}"][data-topic-type="SubTopic"][data-topic-id="${coverage.topicId}"]`).prop('checked', coverage.wasCovered);
                    $(`.coverage-percentage[data-group-id="${groupCoverage.groupId}"][data-topic-type="SubTopic"][data-topic-id="${coverage.topicId}"]`).val(coverage.coveragePercentage);
                });

                groupCoverage.lessonCoverages.forEach(coverage => {
                    $(`.coverage-check[data-group-id="${groupCoverage.groupId}"][data-topic-type="Lesson"][data-topic-id="${coverage.topicId}"]`).prop('checked', coverage.wasCovered);
                    $(`.coverage-percentage[data-group-id="${groupCoverage.groupId}"][data-topic-type="Lesson"][data-topic-id="${coverage.topicId}"]`).val(coverage.coveragePercentage);
                });
            });
        }
    }

    showSuccessMessage(message) {
        // Show success message (you can implement a toast notification here)
        alert(message);
    }

    showErrorMessage(message) {
        // Show error message (you can implement a toast notification here)
        alert('خطا: ' + message);
    }

    // SubChapter Coverage helper methods
    calculateSubChapterGroupCompletionStatus(group) {
        if (!this.chaptersData || this.chaptersData.length === 0) {
            return { isCompleted: false, text: 'در انتظار', completedCount: 0, totalCount: 0 };
        }

        // Count total subchapters for this group
        let totalSubChapters = 0;
        let completedSubChapters = 0;

        this.chaptersData.forEach(chapter => {
            chapter.subChapters.forEach(subChapter => {
                totalSubChapters++;
                
                // Check if this subchapter has coverage data for this group
                const checkbox = $(`.coverage-checkbox[data-group-id="${group.id}"][data-subchapter-id="${subChapter.id}"]`);
                if (checkbox.length && checkbox.is(':checked')) {
                    completedSubChapters++;
                }
            });
        });

        if (totalSubChapters === 0) {
            return { isCompleted: false, text: 'در انتظار', completedCount: 0, totalCount: 0 };
        }

        if (completedSubChapters === 0) {
            return { isCompleted: false, text: 'در انتظار', completedCount: 0, totalCount: totalSubChapters };
        } else if (completedSubChapters === totalSubChapters) {
            return { isCompleted: true, text: 'تکمیل شده', completedCount: completedSubChapters, totalCount: totalSubChapters };
        } else {
            return { isCompleted: false, text: `${completedSubChapters}/${totalSubChapters} تکمیل شده`, completedCount: completedSubChapters, totalCount: totalSubChapters };
        }
    }

    async loadChaptersData() {
        try {
            const response = await fetch(`/Teacher/TeachingSessions/GetSubChapterCoverageData?sessionId=${this.sessionId}`);
            const result = await response.json();
            
            if (result.success && result.data) {
                this.chaptersData = result.data.chapters;
                this.existingCoverages = result.data.existingCoverages || [];
                this.groupCoverageStats = result.data.groupCoverageStats || [];
                this.populateChaptersForGroups();
                this.loadExistingCoverageData();
            }
        } catch (error) {
            console.error('Error loading chapters data:', error);
        }
    }

    loadExistingCoverageData() {
        if (!this.existingCoverages || this.existingCoverages.length === 0) return;

        this.existingCoverages.forEach(coverage => {
            // Checkbox
            const checkbox = $(`.coverage-checkbox[data-group-id="${coverage.studentGroupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
            if (checkbox.length) {
                checkbox.prop('checked', coverage.wasCovered);
            }
            
            // Percentage slider
            const slider = $(`.percentage-slider[data-group-id="${coverage.studentGroupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
            if (slider.length) {
                slider.val(coverage.coveragePercentage);
                slider.siblings('.percentage-value').text(coverage.coveragePercentage + '%');
            }
            
            // Notes textarea
            const notesTextarea = $(`.notes-textarea[data-group-id="${coverage.studentGroupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
            if (notesTextarea.length) {
                notesTextarea.val(coverage.teacherNotes || '');
            }
            
            // Challenges textarea
            const challengesTextarea = $(`.challenges-textarea[data-group-id="${coverage.studentGroupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
            if (challengesTextarea.length) {
                challengesTextarea.val(coverage.challenges || '');
            }
            
            // Show existing notes and challenges in card header
            this.showExistingNotesInHeader(coverage.studentGroupId, coverage.subChapterId, coverage.teacherNotes, coverage.challenges);
            
            // Update delete button visibility and checkbox
            this.updateDeleteButtonVisibility(coverage.studentGroupId, coverage.subChapterId);
            this.updateCheckboxBasedOnData(coverage.studentGroupId, coverage.subChapterId);
        });

        // Update all tab statuses after loading existing data
        this.groups.forEach(group => {
            this.updateSubChapterCoverageTabStatus(group.id);
        });
    }

    populateChaptersForGroups() {
        if (!this.chaptersData) return;

        this.groups.forEach(group => {
            const container = $(`#chaptersContainer_${group.id}`);
            if (container.length === 0) return;

            container.empty();

            this.chaptersData.forEach(chapter => {
                // Get group-specific stats for this chapter
                const groupStats = this.groupCoverageStats?.find(gs => gs.groupId === group.id);
                const chapterGroupStats = groupStats?.chapterStats?.find(cs => cs.chapterId === chapter.id);

                const chapterHtml = `
                    <div class="chapter-section">
                        <div class="chapter-header" data-chapter-id="${chapter.id}">
                            <div class="chapter-info">
                                <i class="fas fa-chevron-down chapter-toggle"></i>
                                <h5 class="chapter-title">${chapter.title}</h5>
                                <span class="chapter-subchapters-count">${chapter.subChapters.length} زیرمبحث</span>
                                <div class="chapter-coverage-stats">
                                    <span class="coverage-count">${chapterGroupStats?.totalCoverageCount || 0} بار پوشش داده شده</span>
                                    <span class="average-progress">میانگین: ${Math.round(chapterGroupStats?.averageProgressPercentage || 0)}%</span>
                                </div>
                            </div>
                            <div class="chapter-progress">
                                <div class="progress-bar">
                                    <div class="progress-fill" style="width: ${chapterGroupStats?.averageProgressPercentage || 0}%"></div>
                                </div>
                                <span class="progress-text">${Math.round(chapterGroupStats?.averageProgressPercentage || 0)}%</span>
                            </div>
                        </div>
                        <div class="subchapters-container" style="display: none;">
                            ${chapter.subChapters.map(subChapter => {
                                const subChapterGroupStats = chapterGroupStats?.subChapterStats?.find(ss => ss.subChapterId === subChapter.id);
                                return `
                                <div class="subchapter-card" data-subchapter-id="${subChapter.id}">
                                    <div class="subchapter-card-header" 
                                         style="display: flex; align-items: flex-start;"
                                         data-subchapter-id="${subChapter.id}">
                                        <div class="subchapter-card-info">
                                            <i class="fas fa-chevron-down subchapter-toggle"></i>
                                            <div class="subchapter-text-content">
                                                <h6 class="subchapter-title">${subChapter.title}</h6>
                                                ${subChapter.description ? `<p class="subchapter-description">${subChapter.description}</p>` : ''}
                                                <div class="subchapter-coverage-stats">
                                                    <span class="coverage-count">${subChapterGroupStats?.coverageCount || 0} بار پوشش داده شده</span>
                                                    <span class="average-progress">میانگین: ${Math.round(subChapterGroupStats?.averageProgressPercentage || 0)}%</span>
                                                </div>
                                                <div class="subchapter-existing-notes" id="existing-notes-${group.id}-${subChapter.id}" style="display: none;">
                                                    <div class="existing-notes-content">
                                                        <div class="existing-notes-section" id="existing-teacher-notes-${group.id}-${subChapter.id}" style="display: none;">
                                                            <i class="fas fa-sticky-note"></i>
                                                            <span class="notes-label">یادداشت:</span>
                                                            <span class="notes-text"></span>
                                                        </div>
                                                        <div class="existing-challenges-section" id="existing-challenges-${group.id}-${subChapter.id}" style="display: none;">
                                                            <i class="fas fa-exclamation-triangle"></i>
                                                            <span class="challenges-label">چالش:</span>
                                                            <span class="challenges-text"></span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="subchapter-card-controls">
                                            <div class="coverage-toggle">
                                                <input type="checkbox" 
                                                       class="coverage-checkbox" 
                                                       data-group-id="${group.id}" 
                                                       data-subchapter-id="${subChapter.id}"
                                                       id="coverage_${group.id}_${subChapter.id}">
                                                <label for="coverage_${group.id}_${subChapter.id}">
                                                    <span class="toggle-text">پوشش داده شد</span>
                                                </label>
                                            </div>
                                            <div class="subchapter-header-actions" id="header-actions-${group.id}-${subChapter.id}" style="display: none;">
                                                <button type="button" class="btn-delete-coverage-minimal" 
                                                        data-group-id="${group.id}" 
                                                        data-subchapter-id="${subChapter.id}"
                                                        title="حذف پوشش">
                                                    <i class="fas fa-times"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="subchapter-card-body" style="display: none;">
                                    <div class="coverage-percentage-full">
                                                <label>درصد پوشش:</label>
                                                <div class="percentage-input-full">
                                                    <input type="range" 
                                                           class="percentage-slider" 
                                                           data-group-id="${group.id}" 
                                                           data-subchapter-id="${subChapter.id}"
                                                           min="0" max="100" value="0">
                                                    <span class="percentage-value">0%</span>
                                                </div>
                                            </div>
                                        <div class="subchapter-details">                                            
                                            <div class="teacher-notes">
                                                <label>یادداشت‌های معلم:</label>
                                                <textarea class="notes-textarea" 
                                                          data-group-id="${group.id}" 
                                                          data-subchapter-id="${subChapter.id}"
                                                          placeholder="یادداشت‌های خود را وارد کنید..."></textarea>
                                            </div>
                                            <div class="challenges">
                                                <label>مشکلات و چالش‌ها:</label>
                                                <textarea class="challenges-textarea" 
                                                          data-group-id="${group.id}" 
                                                          data-subchapter-id="${subChapter.id}"
                                                          placeholder="مشکلات و چالش‌های موجود را شرح دهید..."></textarea>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            `;
                            }).join('')}
                        </div>
                    </div>
                `;
                container.append(chapterHtml);
            });

            // Ensure all subchapters are closed by default after populating
            container.find('.subchapters-container').hide();
            container.find('.chapter-toggle').removeClass('fa-chevron-up').addClass('fa-chevron-down');
            container.find('.subchapter-card-body').hide();
            container.find('.subchapter-toggle').removeClass('fa-chevron-up').addClass('fa-chevron-down');
        });
    }

    setupSubChapterCoverageTabEventListeners() {
        // Tab navigation clicks
        $(document).on('click', '.subchapter-coverage-tab-nav-item', (e) => {
            const tabIndex = parseInt($(e.currentTarget).data('tab-index'));
            this.switchSubChapterCoverageTab(tabIndex);
        });

        // Individual tab save button
        $(document).on('click', '.btn-save-tab', (e) => {
            e.preventDefault();
            const tabIndex = parseInt($(e.target).data('tab-index'));
            const groupId = $(e.target).data('group-id');
            this.saveSubChapterCoverageTabData(tabIndex, groupId);
        });
    }

    setupSubChapterCoverageEventListeners() {
        // Chapter toggle
        $(document).on('click', '.chapter-header', (e) => {
            const chapterHeader = $(e.currentTarget);
            const subchaptersContainer = chapterHeader.next('.subchapters-container');
            const toggleIcon = chapterHeader.find('.chapter-toggle');
            
            if (subchaptersContainer.is(':hidden')) {
                subchaptersContainer.slideDown();
                toggleIcon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
                
                // Ensure all subchapter cards are closed when chapter opens
                subchaptersContainer.find('.subchapter-card-body').hide();
                subchaptersContainer.find('.subchapter-toggle').removeClass('fa-chevron-up').addClass('fa-chevron-down');
            } else {
                subchaptersContainer.slideUp();
                toggleIcon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
            }
        });

        // SubChapter card toggle
        $(document).on('click', '.subchapter-card-header', (e) => {
            // Don't toggle if clicking on checkbox or its label
            if ($(e.target).is('input[type="checkbox"]') || $(e.target).is('label') || $(e.target).closest('label').length > 0) {
                return;
            }
            
            const subchapterHeader = $(e.currentTarget);
            const subchapterBody = subchapterHeader.next('.subchapter-card-body');
            const toggleIcon = subchapterHeader.find('.subchapter-toggle');
            
            if (subchapterBody.is(':hidden')) {
                subchapterBody.slideDown();
                toggleIcon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
            } else {
                subchapterBody.slideUp();
                toggleIcon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
            }
        });

        // Ensure all subchapters are closed by default
        $('.subchapters-container').hide();
        $('.chapter-toggle').removeClass('fa-chevron-up').addClass('fa-chevron-down');
        $('.subchapter-card-body').hide();
        $('.subchapter-toggle').removeClass('fa-chevron-up').addClass('fa-chevron-down');

        // Coverage checkbox
        $(document).on('change', '.coverage-checkbox', (e) => {
            const groupId = parseInt($(e.target).data('group-id'));
            const subChapterId = parseInt($(e.target).data('subchapter-id'));
            const isChecked = $(e.target).is(':checked');
            
            // Auto-set percentage if checked
            if (isChecked) {
                const slider = $(`.percentage-slider[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
                if (slider.val() === '0') {
                    slider.val('50');
                    slider.siblings('.percentage-value').text('50%');
                }
            }
            
            this.updateSubChapterCoverageTabProgress(this.currentActiveTab);
            this.updateSubChapterCoverageTabStatus(groupId);
            this.updateDeleteButtonVisibility(groupId, subChapterId);
            this.updateCheckboxBasedOnData(groupId, subChapterId);
        });

        // Prevent event bubbling for checkbox and label clicks
        $(document).on('click', '.coverage-checkbox, .coverage-toggle label', (e) => {
            e.stopPropagation();
        });

        // Percentage slider
        $(document).on('input', '.percentage-slider', (e) => {
            const value = $(e.target).val();
            $(e.target).siblings('.percentage-value').text(value + '%');
            
            // Auto-check checkbox if percentage > 0
            const checkbox = $(e.target).siblings('.coverage-checkbox');
            if (parseInt(value) > 0) {
                checkbox.prop('checked', true);
            }
            
            this.updateSubChapterCoverageTabProgress(this.currentActiveTab);
            
            // Update delete button visibility and checkbox
            const groupId = parseInt($(e.target).data('group-id'));
            const subChapterId = parseInt($(e.target).data('subchapter-id'));
            this.updateDeleteButtonVisibility(groupId, subChapterId);
            this.updateCheckboxBasedOnData(groupId, subChapterId);
        });


        // Text areas
        $(document).on('input', '.notes-textarea, .challenges-textarea', (e) => {
            this.updateSubChapterCoverageTabProgress(this.currentActiveTab);
            
            // Update existing notes display in header
            const groupId = parseInt($(e.target).data('group-id'));
            const subChapterId = parseInt($(e.target).data('subchapter-id'));
            const isNotesTextarea = $(e.target).hasClass('notes-textarea');
            const isChallengesTextarea = $(e.target).hasClass('challenges-textarea');
            
            if (isNotesTextarea || isChallengesTextarea) {
                const notesTextarea = $(`.notes-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
                const challengesTextarea = $(`.challenges-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
                
                this.showExistingNotesInHeader(
                    groupId, 
                    subChapterId, 
                    notesTextarea.val(), 
                    challengesTextarea.val()
                );
            }
            
            // Update delete button visibility and checkbox
            this.updateDeleteButtonVisibility(groupId, subChapterId);
            this.updateCheckboxBasedOnData(groupId, subChapterId);
        });

        // Delete coverage button
        $(document).on('click', '.btn-delete-coverage-minimal', (e) => {
            e.preventDefault();
            e.stopPropagation(); // Prevent card toggle
            const groupId = parseInt($(e.target).closest('.btn-delete-coverage-minimal').data('group-id'));
            const subChapterId = parseInt($(e.target).closest('.btn-delete-coverage-minimal').data('subchapter-id'));
            
            if (confirm('آیا مطمئن هستید که می‌خواهید پوشش این زیرمبحث را حذف کنید؟')) {
                this.deleteSubChapterCoverage(groupId, subChapterId);
            }
        });
    }

    switchSubChapterCoverageTab(tabIndex) {
        if (tabIndex === this.currentActiveTab) return;

        // Update navigation
        $('.subchapter-coverage-tab-nav-item').removeClass('active');
        $(`.subchapter-coverage-tab-nav-item[data-tab-index="${tabIndex}"]`).addClass('active');

        // Update content
        $('.subchapter-coverage-tab-panel').removeClass('active');
        $(`.subchapter-coverage-tab-panel[data-tab-index="${tabIndex}"]`).addClass('active');

        this.currentActiveTab = tabIndex;
        
        // Ensure all subchapters are closed when switching tabs
        this.closeAllSubchapters();
    }

    updateSubChapterCoverageTabProgress(tabIndex) {
        const tabPanel = $(`.subchapter-coverage-tab-panel[data-tab-index="${tabIndex}"]`);
        const groupSection = tabPanel.find('.group-subchapter-coverage-section');
        const subChapters = groupSection.find('.subchapter-card');
        let completedCount = 0;

        subChapters.each((index, subChapterElement) => {
            const $subChapter = $(subChapterElement);
            const isChecked = $subChapter.find('.coverage-checkbox').is(':checked');
            const percentage = parseInt($subChapter.find('.percentage-slider').val());
            const status = parseInt($subChapter.find('.status-select').val());
            const notes = $subChapter.find('.notes-textarea').val().trim();
            const challenges = $subChapter.find('.challenges-textarea').val().trim();

            // Consider completed if checked or has any data
            if (isChecked || percentage > 0 || status > 0 || notes !== '' || challenges !== '') {
                completedCount++;
            }
        });

        const totalSubChapters = subChapters.length;
        const progressPercentage = totalSubChapters > 0 ? (completedCount / totalSubChapters) * 100 : 0;
        
        // Update progress bar
        groupSection.find('.progress-fill').css('width', `${progressPercentage}%`);
        groupSection.find('.progress-text').text(`${Math.round(progressPercentage)}% تکمیل شده`);

        // Update tab navigation status
        const tabNavItem = $(`.subchapter-coverage-tab-nav-item[data-tab-index="${tabIndex}"]`);
        const statusElement = tabNavItem.find('.subchapter-coverage-tab-status');
        
        if (completedCount === totalSubChapters && totalSubChapters > 0) {
            statusElement.text('تکمیل شده').removeClass('pending').addClass('completed');
            tabNavItem.addClass('completed');
            
            // Add completion badge if not exists
            if (!tabNavItem.find('.completion-badge').length) {
                tabNavItem.prepend('<div class="completion-badge"><i class="fas fa-check"></i></div>');
            }
        } else if (completedCount > 0) {
            statusElement.text(`${completedCount}/${totalSubChapters} تکمیل شده`).removeClass('pending completed');
            tabNavItem.removeClass('completed');
            
            // Remove completion badge
            tabNavItem.find('.completion-badge').remove();
        } else {
            statusElement.text('در انتظار').removeClass('completed').addClass('pending');
            tabNavItem.removeClass('completed');
            
            // Remove completion badge
            tabNavItem.find('.completion-badge').remove();
        }
    }

    async saveSubChapterCoverageTabData(tabIndex, groupId) {
        try {
            const group = this.groups[tabIndex];
            if (!group) {
                this.showNotification('گروه یافت نشد', 'error');
                return;
            }

            const tabData = this.collectSubChapterCoverageTabData(tabIndex, groupId);
            
            const response = await fetch(`/Teacher/TeachingSessions/SaveSubChapterCoverageStep`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(tabData)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();

            if (result.success) {
                this.showNotification(`پوشش زیرمباحث گروه ${group.name} با موفقیت ذخیره شد`, 'success');
                
                // Update tab status
                const tabNavItem = $(`.subchapter-coverage-tab-nav-item[data-tab-index="${tabIndex}"]`);
                tabNavItem.addClass('completed');
                tabNavItem.find('.subchapter-coverage-tab-status').text('ذخیره شده').addClass('completed');
                
                // Store tab data
                this.tabData[tabIndex] = tabData;
                
                // Check if all tabs are completed
                this.checkAllSubChapterCoverageTabsCompleted();
            } else {
                this.showNotification(result.message || 'خطا در ذخیره پوشش زیرمباحث', 'error');
            }
        } catch (error) {
            console.error('Error saving subchapter coverage tab data:', error);
            this.showNotification('خطا در ذخیره داده‌های تب: ' + error.message, 'error');
        }
    }

    collectSubChapterCoverageTabData(tabIndex, groupId) {
        const group = this.groups[tabIndex];
        if (!group) return null;

        const groupCoverage = {
            groupId: group.id,
            groupName: group.name,
            subChapterCoverages: [],
            generalNotes: '',
            challenges: '',
            recommendations: ''
        };

        // Collect subchapter coverages
        if (this.chaptersData) {
            this.chaptersData.forEach(chapter => {
                chapter.subChapters.forEach(subChapter => {
                    const checkbox = $(`.coverage-checkbox[data-group-id="${groupId}"][data-subchapter-id="${subChapter.id}"]`);
                    const slider = $(`.percentage-slider[data-group-id="${groupId}"][data-subchapter-id="${subChapter.id}"]`);
                    const notesTextarea = $(`.notes-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapter.id}"]`);
                    const challengesTextarea = $(`.challenges-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapter.id}"]`);

                    groupCoverage.subChapterCoverages.push({
                        subChapterId: subChapter.id,
                        subChapterTitle: subChapter.title,
                        chapterTitle: chapter.title,
                        wasPlanned: false, // TODO: Check if was planned
                        wasCovered: checkbox.is(':checked'),
                        coveragePercentage: parseInt(slider.val() || '0'),
                        teacherNotes: notesTextarea.val() || null,
                        challenges: challengesTextarea.val() || null
                    });
                });
            });
        }


        // Return in SubChapterCoverageStepDataDto format
        return {
            sessionId: this.sessionId,
            groupCoverages: [groupCoverage]
        };
    }

    collectSubChapterCoverageData() {
        const coverageData = {
            sessionId: this.sessionId,
            groupCoverages: []
        };

        this.groups.forEach((group, index) => {
            // Use saved tab data if available, otherwise collect current data
            if (this.tabData[index]) {
                coverageData.groupCoverages.push(...this.tabData[index].groupCoverages);
            } else {
                const tabData = this.collectSubChapterCoverageTabData(index, group.id);
                if (tabData) {
                    coverageData.groupCoverages.push(...tabData.groupCoverages);
                }
            }
        });

        return coverageData;
    }

    checkAllSubChapterCoverageTabsCompleted() {
        const allTabs = $('.subchapter-coverage-tab-nav-item');
        const completedTabs = $('.subchapter-coverage-tab-nav-item.completed');
        
        if (allTabs.length === completedTabs.length) {
            // All tabs completed
            this.showNotification('همه گروه‌ها تکمیل شدند! می‌توانید گزارش را تکمیل کنید.', 'success');
            
            // Enable complete session button
            $('.btn-complete-session').prop('disabled', false).removeClass('disabled');
        }
    }

    loadSubChapterCoverageData(data) {
        // Load existing subchapter coverage data
        if (data) {
            const coverageData = JSON.parse(data);
            coverageData.groupCoverages.forEach(groupCoverage => {
                        groupCoverage.subChapterCoverages.forEach(coverage => {
                    // Checkbox
                    const checkbox = $(`.coverage-checkbox[data-group-id="${groupCoverage.groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                    if (checkbox.length) {
                        checkbox.prop('checked', coverage.wasCovered);
                    }
                    
                    // Percentage slider
                    const slider = $(`.percentage-slider[data-group-id="${groupCoverage.groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                    if (slider.length) {
                        slider.val(coverage.coveragePercentage);
                        slider.siblings('.percentage-value').text(coverage.coveragePercentage + '%');
                    }
                    
                    
                    // Notes textarea
                    const notesTextarea = $(`.notes-textarea[data-group-id="${groupCoverage.groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                    if (notesTextarea.length) {
                        notesTextarea.val(coverage.teacherNotes || '');
                    }
                    
                    // Challenges textarea
                    const challengesTextarea = $(`.challenges-textarea[data-group-id="${groupCoverage.groupId}"][data-subchapter-id="${coverage.subChapterId}"]`);
                    if (challengesTextarea.length) {
                        challengesTextarea.val(coverage.challenges || '');
                    }
                    
                    // Show existing notes and challenges in card header
                    this.showExistingNotesInHeader(groupCoverage.groupId, coverage.subChapterId, coverage.teacherNotes, coverage.challenges);
                });
                
                // Update tab progress and status
                const tabIndex = this.groups.findIndex(g => g.id === groupCoverage.groupId);
                if (tabIndex !== -1) {
                    this.updateSubChapterCoverageTabProgress(tabIndex);
                    this.updateSubChapterCoverageTabStatus(groupCoverage.groupId);
                }
            });
            
            // Ensure all subchapters remain closed after loading data
            this.closeAllSubchapters();
        }
    }

    showExistingNotesInHeader(groupId, subChapterId, teacherNotes, challenges) {
        const existingNotesContainer = $(`#existing-notes-${groupId}-${subChapterId}`);
        const teacherNotesSection = $(`#existing-teacher-notes-${groupId}-${subChapterId}`);
        const challengesSection = $(`#existing-challenges-${groupId}-${subChapterId}`);
        const headerActions = $(`#header-actions-${groupId}-${subChapterId}`);
        
        if (existingNotesContainer.length === 0) return;
        
        let hasContent = false;
        
        // Show teacher notes if exists
        if (teacherNotes && teacherNotes.trim()) {
            teacherNotesSection.find('.notes-text').text(teacherNotes);
            teacherNotesSection.show();
            hasContent = true;
        } else {
            teacherNotesSection.hide();
        }
        
        // Show challenges if exists
        if (challenges && challenges.trim()) {
            challengesSection.find('.challenges-text').text(challenges);
            challengesSection.show();
            hasContent = true;
        } else {
            challengesSection.hide();
        }
        
        // Show container if there's any content
        if (hasContent) {
            existingNotesContainer.show();
        } else {
            existingNotesContainer.hide();
        }
        
        // Show delete button if there's any coverage data
        this.updateDeleteButtonVisibility(groupId, subChapterId);
    }

    updateDeleteButtonVisibility(groupId, subChapterId) {
        const checkbox = $(`.coverage-checkbox[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const slider = $(`.percentage-slider[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const notesTextarea = $(`.notes-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const challengesTextarea = $(`.challenges-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const headerActions = $(`#header-actions-${groupId}-${subChapterId}`);
        
        // Check if there's any coverage data
        const hasCoverage = checkbox.is(':checked') || 
                           parseInt(slider.val() || '0') > 0 || 
                           (notesTextarea.val() && notesTextarea.val().trim()) ||
                           (challengesTextarea.val() && challengesTextarea.val().trim());
        
        if (hasCoverage) {
            headerActions.show();
        } else {
            headerActions.hide();
        }
    }

    updateCheckboxBasedOnData(groupId, subChapterId) {
        const checkbox = $(`.coverage-checkbox[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const slider = $(`.percentage-slider[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const notesTextarea = $(`.notes-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const challengesTextarea = $(`.challenges-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        
        // Check if there's any coverage data
        const hasCoverage = parseInt(slider.val() || '0') > 0 || 
                           (notesTextarea.val() && notesTextarea.val().trim()) ||
                           (challengesTextarea.val() && challengesTextarea.val().trim());
        
        // Auto-check checkbox if there's coverage data
        if (hasCoverage && !checkbox.is(':checked')) {
            checkbox.prop('checked', true);
        }
    }

    updateSubChapterCoverageTabStatus(groupId) {
        const group = this.groups.find(g => g.id === groupId);
        if (!group) return;

        const completionStatus = this.calculateSubChapterGroupCompletionStatus(group);
        const tabNavItem = $(`.subchapter-coverage-tab-nav-item[data-group-id="${groupId}"]`);
        
        if (tabNavItem.length === 0) return;

        // Update completion badge
        const completionBadge = tabNavItem.find('.completion-badge');
        if (completionStatus.isCompleted) {
            if (completionBadge.length === 0) {
                tabNavItem.prepend('<div class="completion-badge"><i class="fas fa-check"></i></div>');
            }
        } else {
            completionBadge.remove();
        }

        // Update status text and class
        const statusElement = tabNavItem.find('.subchapter-coverage-tab-status');
        statusElement.removeClass('completed pending')
                   .addClass(completionStatus.isCompleted ? 'completed' : 'pending')
                   .text(completionStatus.text);
    }

    deleteSubChapterCoverage(groupId, subChapterId) {
        // Clear all form fields for this subchapter
        const checkbox = $(`.coverage-checkbox[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const slider = $(`.percentage-slider[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const notesTextarea = $(`.notes-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        const challengesTextarea = $(`.challenges-textarea[data-group-id="${groupId}"][data-subchapter-id="${subChapterId}"]`);
        
        checkbox.prop('checked', false);
        slider.val(0);
        slider.siblings('.percentage-value').text('0%');
        notesTextarea.val('');
        challengesTextarea.val('');
        
        // Hide existing notes display and delete button
        this.showExistingNotesInHeader(groupId, subChapterId, '', '');
        this.updateDeleteButtonVisibility(groupId, subChapterId);
        
        // Update tab status
        this.updateSubChapterCoverageTabStatus(groupId);
        this.updateSubChapterCoverageTabProgress(this.currentActiveTab);
        
        this.showNotification('پوشش زیرمبحث حذف شد', 'success');
    }
}

// Initialize when document is ready
$(document).ready(async function () {
    // Check if we're on the step completion page
    if ($('.step-completion-container').length > 0) {
        const sessionId = $('#sessionId').val();
        const hasPlan = $('#hasPlan').val() === 'true';
        const groupsData = safeJsonParse($('#groupsData').val());
        const subTopicsData = safeJsonParse($('#subTopicsData').val());
        const lessonsData = safeJsonParse($('#lessonsData').val());
        const plannedItemsData = safeJsonParse($('#plannedItemsData').val());

        // Initialize step completion manager
        const stepCompletionManager = new StepCompletionManager({
            sessionId: sessionId,
            hasPlan: hasPlan,
            groups: groupsData,
            availableSubTopics: subTopicsData,
            availableLessons: lessonsData,
            plannedItems: plannedItemsData
        });
        await stepCompletionManager.init();
    }
});
