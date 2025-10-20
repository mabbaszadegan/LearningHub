/**
 * Study Content JavaScript - Enhanced with Auto Timer and Exit Confirmation
 * Handles study timer, session management, and exit confirmation for educational content
 */

let studySession = {
    isActive: false,
    startTime: null,
    elapsedTime: 0,
    sessionId: null,
    updateInterval: null,
    
    init() {
        this.sessionId = window.studyContentConfig.activeSessionId;
        this.bindEvents();
        this.startAutoTimer();
        this.updateStatistics();
    },
    
    bindEvents() {
        // Timer controls
        $('#timer-toggle').on('click', () => {
            if (this.isActive) {
                this.pauseTimer();
            } else {
                this.startTimer();
            }
        });
        
        // Prevent accidental page refresh/close
        window.addEventListener('beforeunload', (e) => {
            if (this.isActive && this.getElapsedTime() > 5) {
                e.preventDefault();
                e.returnValue = 'آیا مطمئن هستید که می‌خواهید صفحه را ترک کنید؟ زمان مطالعه ثبت نخواهد شد.';
                return e.returnValue;
            }
        });
        
        // Handle browser back button
        window.addEventListener('popstate', (e) => {
            if (this.isActive && this.getElapsedTime() > 5) {
                this.showExitConfirmation();
                history.pushState(null, null, window.location.href);
            }
        });
        
        // Add history state to prevent back button
        history.pushState(null, null, window.location.href);
        
        // Handle back button clicks
        $('.btn-back-compact').on('click', function(e) {
            e.preventDefault();
            
            if (studySession.isActive && studySession.getElapsedTime() > 5) {
                studySession.showExitConfirmation();
            } else {
                history.back();
            }
        });
    },
    
    startAutoTimer() {
        // Auto-start timer when page loads
        setTimeout(() => {
            this.startTimer();
        }, 1000); // Start after 1 second
    },
    
    startTimer() {
        if (!this.isActive) {
            this.isActive = true;
            this.startTime = Date.now() - this.elapsedTime;
            
            this.updateInterval = setInterval(() => {
                this.updateDisplay();
            }, 1000);
            
            this.updateTimerUI();
        }
    },
    
    pauseTimer() {
        if (this.isActive) {
            this.isActive = false;
            this.elapsedTime = this.getElapsedTime();
            clearInterval(this.updateInterval);
            this.updateTimerUI();
        }
    },
    
    getElapsedTime() {
        if (this.isActive && this.startTime) {
            return Math.floor((Date.now() - this.startTime) / 1000);
        }
        return this.elapsedTime;
    },
    
    formatTime(seconds) {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const secs = seconds % 60;
        
        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    },
    
    updateDisplay() {
        const elapsed = this.getElapsedTime();
        $('#timer-text').text(this.formatTime(elapsed));
    },
    
    updateTimerUI() {
        const toggleBtn = $('#timer-toggle');
        const icon = toggleBtn.find('i');
        
        if (this.isActive) {
            icon.removeClass('fa-play').addClass('fa-pause');
            toggleBtn.attr('title', 'توقف تایمر');
            $('.timer-icon-compact').removeClass('fa-play-circle').addClass('fa-pause-circle');
        } else {
            icon.removeClass('fa-pause').addClass('fa-play');
            toggleBtn.attr('title', 'شروع تایمر');
            $('.timer-icon-compact').removeClass('fa-pause-circle').addClass('fa-play-circle');
        }
    },
    
    updateStatistics() {
        // Update study statistics display
        // This would be implemented based on your backend API
        $('#total-study-time').text('00:00:00');
        $('#study-sessions-count').text('0');
    },
    
    showExitConfirmation() {
        const currentTime = this.getElapsedTime();
        
        // Create and show exit confirmation modal
        const modalHtml = `
            <div class="modal fade exit-confirmation-modal" id="exitConfirmationModal" tabindex="-1" aria-labelledby="exitConfirmationModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="exitConfirmationModalLabel">
                                <i class="fas fa-question-circle me-2"></i>تأیید خروج
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="exit-confirmation-content">
                                <div class="confirmation-icon">
                                    <i class="fas fa-clock fa-3x text-primary"></i>
                                </div>
                                <h6 class="confirmation-title">آیا می‌خواهید زمان مطالعه ثبت شود؟</h6>
                                <p class="confirmation-message">
                                    شما <span id="current-study-time">${this.formatTime(currentTime)}</span> روی این محتوا مطالعه کرده‌اید.
                                </p>
                                <div class="study-time-summary">
                                    <div class="time-summary-item">
                                        <span class="summary-label">زمان مطالعه فعلی:</span>
                                        <span class="summary-value" id="current-session-time">${this.formatTime(currentTime)}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                                <i class="fas fa-times me-1"></i>انصراف
                            </button>
                            <button type="button" class="btn btn-outline-secondary" id="exit-without-saving">
                                <i class="fas fa-sign-out-alt me-1"></i>خروج بدون ثبت
                            </button>
                            <button type="button" class="btn btn-primary" id="save-and-exit">
                                <i class="fas fa-save me-1"></i>ثبت و خروج
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        // Remove existing modal if any
        $('#exitConfirmationModal').remove();
        
        // Add new modal to body
        $('body').append(modalHtml);
        
        // Show modal
        $('#exitConfirmationModal').modal('show');
        
        // Bind events
        $('#exit-without-saving').on('click', () => {
            $('#exitConfirmationModal').modal('hide');
            this.exitWithoutSaving();
        });
        
        $('#save-and-exit').on('click', () => {
            $('#exitConfirmationModal').modal('hide');
            this.saveAndExit();
        });
        
        // Clean up modal when hidden
        $('#exitConfirmationModal').on('hidden.bs.modal', function() {
            $(this).remove();
        });
    },
    
    saveAndExit() {
        if (this.isActive) {
            this.pauseTimer();
        }
        
        // Show success message
        this.showToast('زمان مطالعه با موفقیت ثبت شد', 'success');
        
        // Navigate back after a short delay
        setTimeout(() => {
            history.back();
        }, 1000);
    },
    
    exitWithoutSaving() {
        if (this.isActive) {
            this.pauseTimer();
        }
        
        // Navigate back immediately
        history.back();
    },
    
    showToast(message, type) {
        const toastClass = type === 'error' ? 'alert-danger' : 'alert-success';
        const toastHtml = `
            <div class="alert ${toastClass} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999;" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        $('body').append(toastHtml);
        
        // Auto remove after 3 seconds
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 3000);
    }
};

$(document).ready(function() {
    // Initialize study session
    studySession.init();
});
                e.returnValue = 'آیا مطمئن هستید که می‌خواهید صفحه را ترک کنید؟ زمان مطالعه ثبت نخواهد شد.';
                return e.returnValue;
            }
        });
        
        // Handle browser back button
        window.addEventListener('popstate', (e) => {
            if (this.isRunning && this.getElapsedTime() > 5) {
                this.showExitConfirmation();
                history.pushState(null, null, window.location.href);
            }
        });
        
        // Add history state to prevent back button
        history.pushState(null, null, window.location.href);
    },
    
    startTimer() {
        if (!this.isRunning) {
            this.isRunning = true;
            this.startTime = Date.now() - this.elapsedTime;
            
            // Start a new session if none exists
            if (this.sessionId === 0) {
                this.createNewSession();
            }
            
            this.updateInterval = setInterval(() => {
                this.updateDisplay();
                this.updateSessionDuration();
            }, 1000);
            
            this.updateTimerUI();
        }
    },
    
    pauseTimer() {
        if (this.isRunning) {
            this.isRunning = false;
            this.elapsedTime = this.getElapsedTime();
            clearInterval(this.updateInterval);
            this.updateTimerUI();
        }
    },
    
    resetTimer() {
        this.isRunning = false;
        this.elapsedTime = 0;
        this.startTime = null;
        clearInterval(this.updateInterval);
        this.updateDisplay();
        this.updateTimerUI();
    },
    
    getElapsedTime() {
        if (this.isRunning && this.startTime) {
            return Math.floor((Date.now() - this.startTime) / 1000);
        }
        return this.elapsedTime;
    },
    
    formatTime(seconds) {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const secs = seconds % 60;
        
        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    },
    
    updateDisplay() {
        const elapsed = this.getElapsedTime();
        $('#timer-text').text(this.formatTime(elapsed));
    },
    
    updateTimerUI() {
        const toggleBtn = $('#timer-toggle');
        const icon = toggleBtn.find('i');
        
        if (this.isRunning) {
            icon.removeClass('fa-play').addClass('fa-pause');
            toggleBtn.attr('title', 'توقف تایمر');
            $('.timer-icon').removeClass('fa-play-circle').addClass('fa-pause-circle');
        } else {
            icon.removeClass('fa-pause').addClass('fa-play');
            toggleBtn.attr('title', 'شروع تایمر');
            $('.timer-icon').removeClass('fa-pause-circle').addClass('fa-play-circle');
        }
    },
    
    async createNewSession() {
        try {
            const response = await $.ajax({
                url: window.studyContentConfig.startSessionUrl,
                type: 'POST',
                data: {
                    educationalContentId: window.studyContentConfig.educationalContentId
                },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });
            
            if (response.success) {
                this.sessionId = response.sessionId;
                window.studyContentConfig.activeSessionId = response.sessionId;
            } else {
                console.error('Failed to create study session:', response.error);
            }
        } catch (error) {
            console.error('Error creating study session:', error);
        }
    },
    
    async updateSessionDuration() {
        if (this.sessionId > 0) {
            try {
                await $.ajax({
                    url: window.studyContentConfig.updateDurationUrl,
                    type: 'POST',
                    data: {
                        sessionId: this.sessionId,
                        durationSeconds: this.getElapsedTime()
                    },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    }
                });
            } catch (error) {
                console.error('Error updating session duration:', error);
            }
        }
    },
    
    async completeSession() {
        if (this.sessionId > 0) {
            try {
                const response = await $.ajax({
                    url: window.studyContentConfig.completeSessionUrl,
                    type: 'POST',
                    data: {
                        sessionId: this.sessionId,
                        durationSeconds: this.getElapsedTime()
                    },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    }
                });
                
                if (response.success) {
                    this.sessionId = 0;
                    window.studyContentConfig.activeSessionId = 0;
                    this.updateStatistics();
                } else {
                    console.error('Failed to complete study session:', response.error);
                }
            } catch (error) {
                console.error('Error completing study session:', error);
            }
        }
    },
    
    async updateStatistics() {
        try {
            const response = await $.ajax({
                url: window.studyContentConfig.getStatisticsUrl,
                type: 'GET',
                data: {
                    educationalContentId: window.studyContentConfig.educationalContentId
                }
            });
            
            if (response.success) {
                const stats = response.statistics;
                $('#total-study-time').text(this.formatTime(stats.totalStudyTimeSeconds));
                $('#study-sessions-count').text(stats.studySessionsCount);
                $('#last-study-date').text(stats.lastStudyDate ? 
                    new Date(stats.lastStudyDate).toLocaleDateString('fa-IR') : 'هیچ‌گاه');
            }
        } catch (error) {
            console.error('Error updating statistics:', error);
        }
    },
    
    showExitConfirmation() {
        const currentTime = this.getElapsedTime();
        $('#current-study-time').text(this.formatTime(currentTime));
        $('#current-session-time').text(this.formatTime(currentTime));
        
        $('#exitConfirmationModal').modal('show');
    },
    
    async saveAndExit() {
        $('#exitConfirmationModal').modal('hide');
        
        if (this.isRunning) {
            this.pauseTimer();
        }
        
        await this.completeSession();
        
        // Show success message
        this.showToast('زمان مطالعه با موفقیت ثبت شد', 'success');
        
        // Navigate back after a short delay
        setTimeout(() => {
            history.back();
        }, 1000);
    },
    
    exitWithoutSaving() {
        $('#exitConfirmationModal').modal('hide');
        
        if (this.isRunning) {
            this.pauseTimer();
        }
        
        // Navigate back immediately
        history.back();
    },
    
    showToast(message, type) {
        const toastClass = type === 'error' ? 'alert-danger' : 'alert-success';
        const toastHtml = `
            <div class="alert ${toastClass} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999;" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        $('body').append(toastHtml);
        
        // Auto remove after 3 seconds
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 3000);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    studyTimer.init();
    
    // Handle back button clicks
    $('.btn-back').on('click', function(e) {
        e.preventDefault();
        
        if (studyTimer.isRunning && studyTimer.getElapsedTime() > 5) {
            studyTimer.showExitConfirmation();
        } else {
            history.back();
        }
    });
    
    // Handle browser back button
    window.addEventListener('popstate', function(e) {
        if (studyTimer.isRunning && studyTimer.getElapsedTime() > 5) {
            studyTimer.showExitConfirmation();
            history.pushState(null, null, window.location.href);
        }
    });
});
