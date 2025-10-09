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
            { number: 3, title: 'پوشش موضوعات', icon: 'fas fa-check-square' }
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
                this.populateTopicCoverageStep();
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
                                                <option value="0" ${statusValue === 0 ? 'selected' : ''}>غایب</option>
                                                <option value="1" ${statusValue === 1 ? 'selected' : ''}>حاضر</option>
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
        const container = $('#feedbackContainer');
        container.empty();

        this.groups.forEach(group => {
            const groupHtml = `
                <div class="group-feedback-section" data-group-id="${group.id}">
                    <div class="group-feedback-header">
                        <h4 class="group-feedback-title">${group.name}</h4>
                    </div>
                    <div class="feedback-controls">
                        <div class="rating-group">
                            <label class="rating-label">سطح درک مطلب</label>
                            <input type="range" class="rating-slider understanding-level" min="1" max="5" value="3" data-group-id="${group.id}">
                            <span class="rating-display">3</span>
                        </div>
                        <div class="rating-group">
                            <label class="rating-label">سطح مشارکت</label>
                            <input type="range" class="rating-slider participation-level" min="1" max="5" value="3" data-group-id="${group.id}">
                            <span class="rating-display">3</span>
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
                    </div>
                </div>
            `;
            container.append(groupHtml);
        });

        // Setup rating slider events
        $('.rating-slider').on('input', (e) => {
            const value = $(e.target).val();
            $(e.target).siblings('.rating-display').text(value);
        });

        // Load existing data if available
        if (this.stepData[2]) {
            this.loadFeedbackData(this.stepData[2].completionData);
        }
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
                    stepData = this.collectTopicCoverageData();
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
            const groupFeedback = $(`.group-feedback[data-group-id="${group.id}"]`).val();
            const challenges = $(`.challenges[data-group-id="${group.id}"]`).val();
            const nextSessionRecommendations = $(`.next-session-recommendations[data-group-id="${group.id}"]`).val();

            feedbackData.groupFeedbacks.push({
                groupId: group.id,
                groupName: group.name,
                understandingLevel: parseInt(understandingLevel),
                participationLevel: parseInt(participationLevel),
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
                    stepName: 'topic-coverage',
                    completionData: JSON.stringify(this.collectTopicCoverageData()),
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
            3: 'topic-coverage'
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
        // Load existing feedback data
        if (data) {
            const feedbackData = JSON.parse(data);
            feedbackData.groupFeedbacks.forEach(groupFeedback => {
                $(`.understanding-level[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.understandingLevel);
                $(`.participation-level[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.participationLevel);
                $(`.group-feedback[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.groupFeedback || '');
                $(`.challenges[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.challenges || '');
                $(`.next-session-recommendations[data-group-id="${groupFeedback.groupId}"]`).val(groupFeedback.nextSessionRecommendations || '');
            });
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
