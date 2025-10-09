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
            const step = $(e.target).data('step');
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

    populateAttendanceStep() {
        const container = $('#attendanceContainer');
        container.empty();

        console.log('Groups data:', this.groups);

        this.groups.forEach(group => {
            console.log(`Group ${group.name}:`, group);
            console.log(`Group members:`, group.members);
            const groupHtml = `
                <div class="group-attendance-section" data-group-id="${group.id}">
                    <div class="group-attendance-header">
                        <h4 class="group-attendance-title">${group.name}</h4>
                        <p class="group-attendance-subtitle">${group.memberCount} دانش‌آموز</p>
                    </div>
                    <div class="students-attendance-list">
                        ${group.members.map(member => `
                            <div class="student-attendance-item">
                                <div class="student-info">
                                    <span class="student-name">${member.studentName}</span>
                                </div>
                                <div class="attendance-controls">
                                    <select class="attendance-status" data-student-id="${member.studentId}">
                                        <option value="0">غایب</option>
                                        <option value="1" selected>حاضر</option>
                                        <option value="2">تأخیر</option>
                                        <option value="3">مرخصی</option>
                                    </select>
                                    <input type="number" class="participation-score" data-student-id="${member.studentId}" 
                                           min="0" max="100" value="100" placeholder="امتیاز مشارکت">
                                    <textarea class="attendance-comment" data-student-id="${member.studentId}" 
                                              placeholder="یادداشت (اختیاری)"></textarea>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
            `;
            container.append(groupHtml);
        });

        // Load existing data if available
        if (this.stepData[1]) {
            this.loadAttendanceData(this.stepData[1].completionData);
        }
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

    collectAttendanceData() {
        const attendanceData = {
            sessionId: this.sessionId,
            groupAttendances: []
        };

        this.groups.forEach(group => {
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
                    studentId: member.studentId,
                    studentName: member.studentName,
                    status: parseInt(status),
                    participationScore: participationScore ? parseFloat(participationScore) : null,
                    comment: comment || null
                });
            });

            attendanceData.groupAttendances.push(groupAttendance);
        });

        return attendanceData;
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
