/**
 * Study Content JavaScript - Hidden Timer with Exit Confirmation
 * Handles hidden timer and elegant exit confirmation for ScheduleItem study
 */

let studySession = {
    isActive: false,
    startTime: null,
    elapsedTime: 0,
    sessionId: null,
    updateInterval: null,
    
    init() {
        this.sessionId = window.studyContentConfig?.activeSessionId || 0;
        this.bindEvents();
        this.startHiddenTimer();
        console.log('Study session initialized with sessionId:', this.sessionId);
    },
    
    bindEvents() {
        const self = this;
        
        // Prevent accidental page refresh/close
        window.addEventListener('beforeunload', (e) => {
            if (self.isActive && self.getElapsedTime() > 5) {
                e.preventDefault();
                e.returnValue = 'آیا مطمئن هستید که می‌خواهید صفحه را ترک کنید؟';
                return e.returnValue;
            }
        });
        
        // Handle browser back button
        window.addEventListener('popstate', (e) => {
            if (self.isActive && self.getElapsedTime() > 5) {
                self.showExitConfirmation();
                history.pushState(null, null, window.location.href);
            }
        });
        
        // Add history state to prevent back button
        history.pushState(null, null, window.location.href);
        
        // Wait for DOM to be fully loaded
        setTimeout(() => {
            // Handle ALL clicks on the page
            document.addEventListener('click', function(e) {
                const target = e.target;
                
                console.log('Click detected on:', target.tagName, target.className, target.id, 'timer active:', self.isActive, 'time:', self.getElapsedTime());
                
                // Check if timer is active and has enough time
                if (self.isActive && self.getElapsedTime() > 5) {
                    
                    // Skip if clicking inside the modal
                    if (target.closest('#exitConfirmationModal')) {
                        console.log('Click inside modal, ignoring');
                        return;
                    }
                    
                    // Check if it's a link that navigates away
                    if (target.tagName === 'A') {
                        const href = target.getAttribute('href');
                        if (href && !href.startsWith('#') && !href.startsWith('javascript:')) {
                            console.log('External link clicked:', href);
                            e.preventDefault();
                            e.stopPropagation();
                            self.showExitConfirmation();
                            return false;
                        }
                    }
                    
                    // Check for navbar/menu clicks - ANY click inside navbar
                    if (target.closest('.navbar') || target.closest('.nav-link') || target.closest('.navbar-nav') || 
                        target.closest('.navbar-brand') || target.closest('.navbar-toggler') || 
                        target.closest('.navbar-collapse') || target.closest('.navbar-nav')) {
                        console.log('Navbar/menu clicked:', target);
                        e.preventDefault();
                        e.stopPropagation();
                        self.showExitConfirmation();
                        return false;
                    }
                    
                    // Check for any clickable element that might navigate
                    if (target.closest('a')) {
                        const parentLink = target.closest('a');
                        const parentHref = parentLink.getAttribute('href');
                        if (parentHref && !parentHref.startsWith('#') && !parentHref.startsWith('javascript:')) {
                            console.log('Parent link clicked:', parentHref);
                            e.preventDefault();
                            e.stopPropagation();
                            self.showExitConfirmation();
                            return false;
                        }
                    }
                    
                    // Check for any button that might navigate (but not modal buttons)
                    if ((target.tagName === 'BUTTON' || target.closest('button')) && 
                        !target.closest('#exitConfirmationModal')) {
                        console.log('Button clicked:', target);
                        e.preventDefault();
                        e.stopPropagation();
                        self.showExitConfirmation();
                        return false;
                    }
                    
                    // Check for any element that might cause navigation (breadcrumb, menu items, etc.)
                    if (target.closest('.breadcrumb') || target.closest('.dropdown') || target.closest('.dropdown-menu') ||
                        target.closest('.nav-item') || target.closest('.dropdown-item')) {
                        console.log('Navigation element clicked:', target);
                        e.preventDefault();
                        e.stopPropagation();
                        self.showExitConfirmation();
                        return false;
                    }
                }
            }, true); // Use capture phase
            
            console.log('Events bound successfully');
        }, 1000); // Wait 1 second for DOM to be ready
    },
    
    startHiddenTimer() {
        // Auto-start hidden timer when page loads
        setTimeout(() => {
            this.startTimer();
        }, 500); // Start after 0.5 second
    },
    
    startTimer() {
        if (!this.isActive) {
            this.isActive = true;
            this.startTime = Date.now() - this.elapsedTime;
            
            // Timer runs in background, no UI updates needed
            this.updateInterval = setInterval(() => {
                // Timer runs silently in background
            }, 1000);
            
            console.log('Timer started');
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
    
    showExitConfirmation() {
        const currentTime = this.getElapsedTime();
        console.log('Showing exit confirmation, time:', currentTime);
        
        // Remove any existing modal first
        const existingModal = document.getElementById('exitConfirmationModal');
        if (existingModal) {
            existingModal.remove();
        }
        
        // Create simple modal HTML
        const modalHtml = `
            <div id="exitConfirmationModal" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 9999; display: flex; align-items: center; justify-content: center; padding: 1rem;">
                <div style="background: white; padding: 2rem; border-radius: 16px; max-width: 450px; width: 100%; text-align: center; box-shadow: 0 20px 40px rgba(0,0,0,0.15);">
                    <div style="margin-bottom: 1rem;">
                        <i class="fas fa-clock" style="font-size: 3rem; color: #667eea;"></i>
                    </div>
                    <h5 style="margin-bottom: 1rem; color: #1e293b; font-weight: 600;">زمان مطالعه شما</h5>
                    <div style="margin: 1.5rem 0;">
                        <div style="width: 120px; height: 120px; border-radius: 50%; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); display: flex; align-items: center; justify-content: center; margin: 0 auto; box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);">
                            <span style="color: white; font-family: 'Courier New', monospace; font-size: 1.2rem; font-weight: bold;">${this.formatTime(currentTime)}</span>
                        </div>
                    </div>
                    <p style="margin-bottom: 1.5rem; color: #64748b; font-size: 1rem;">آیا می‌خواهید این زمان مطالعه ثبت شود؟</p>
                    <!-- Action buttons row -->
                    <div style="display: flex; gap: 0.75rem; justify-content: center; align-items: center; margin-bottom: 1rem;">
                        <button id="exit-without-saving" style="
                            padding: 0.75rem 1.5rem; 
                            border: 1px solid #fecaca; 
                            background: #ffffff; 
                            color: #dc2626; 
                            border-radius: 12px; 
                            cursor: pointer; 
                            font-weight: 500; 
                            font-size: 0.9rem;
                            transition: all 0.2s ease;
                            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
                            flex: 1;
                            min-width: 120px;
                        " onmouseover="this.style.background='#fef2f2'; this.style.borderColor='#fca5a5'; this.style.transform='translateY(-1px)'; this.style.boxShadow='0 2px 6px rgba(0,0,0,0.15)'" 
                           onmouseout="this.style.background='#ffffff'; this.style.borderColor='#fecaca'; this.style.transform='translateY(0)'; this.style.boxShadow='0 1px 3px rgba(0,0,0,0.1)'">
                            <i class="fas fa-sign-out-alt" style="margin-left: 0.5rem;"></i> خروج بدون ثبت
                        </button>
                        <button id="save-and-exit" style="
                            padding: 0.75rem 1.5rem; 
                            border: none; 
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                            color: white; 
                            border-radius: 12px; 
                            cursor: pointer; 
                            font-weight: 600; 
                            font-size: 0.9rem;
                            transition: all 0.2s ease;
                            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
                            flex: 1;
                            min-width: 120px;
                        " onmouseover="this.style.transform='translateY(-2px)'; this.style.boxShadow='0 6px 20px rgba(102, 126, 234, 0.5)'" 
                           onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 4px 15px rgba(102, 126, 234, 0.4)'">
                            <i class="fas fa-save" style="margin-left: 0.5rem;"></i> ثبت و خروج
                        </button>
                    </div>
                    
                    <!-- Cancel button - full width -->
                    <div style="width: 100%;">
                        <button id="cancel-exit" style="
                            width: 100%;
                            padding: 0.75rem 1.5rem; 
                            border: 1px solid #e2e8f0; 
                            background: #ffffff; 
                            color: #64748b; 
                            border-radius: 12px; 
                            cursor: pointer; 
                            font-weight: 500; 
                            font-size: 0.9rem;
                            transition: all 0.2s ease;
                            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
                        " onmouseover="this.style.background='#f8fafc'; this.style.borderColor='#cbd5e1'; this.style.transform='translateY(-1px)'; this.style.boxShadow='0 2px 6px rgba(0,0,0,0.15)'" 
                           onmouseout="this.style.background='#ffffff'; this.style.borderColor='#e2e8f0'; this.style.transform='translateY(0)'; this.style.boxShadow='0 1px 3px rgba(0,0,0,0.1)'">
                            <i class="fas fa-times" style="margin-left: 0.5rem;"></i> انصراف و ادامه مطالعه
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        // Add modal to body
        document.body.insertAdjacentHTML('beforeend', modalHtml);
        
        // Bind events
        document.getElementById('cancel-exit').onclick = () => {
            console.log('Cancel clicked');
            this.hideModal();
        };
        
        document.getElementById('exit-without-saving').onclick = () => {
            console.log('Exit without saving clicked');
            this.hideModal();
            this.exitWithoutSaving();
        };
        
        document.getElementById('save-and-exit').onclick = () => {
            console.log('Save and exit clicked');
            this.hideModal();
            this.saveAndExit();
        };
        
        console.log('Modal created and shown');
        
        // Prevent body scroll
        document.body.style.overflow = 'hidden';
    },
    
    showModal() {
        const modal = document.getElementById('exitConfirmationModal');
        if (modal) {
            // Remove any existing backdrop first
            const existingBackdrop = document.getElementById('modalBackdrop');
            if (existingBackdrop) {
                existingBackdrop.remove();
            }
            
            // Add modal backdrop
            const backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop fade show';
            backdrop.id = 'modalBackdrop';
            document.body.appendChild(backdrop);
            
            // Show modal with proper classes
            modal.classList.add('show');
            modal.classList.remove('fade');
            modal.style.display = 'block';
            modal.setAttribute('aria-hidden', 'false');
            
            // Prevent body scroll and add modal-open class
            document.body.style.overflow = 'hidden';
            document.body.classList.add('modal-open');
            
            console.log('Modal shown successfully');
        } else {
            console.error('Modal element not found!');
        }
    },
    
    hideModal() {
        console.log('Hiding modal');
        const modal = document.getElementById('exitConfirmationModal');
        if (modal) {
            modal.remove();
            console.log('Modal removed');
        }
        
        // Restore body scroll
        document.body.style.overflow = '';
    },
    
    saveAndExit() {
        if (this.isActive) {
            this.isActive = false;
            clearInterval(this.updateInterval);
        }
        
        // Save study session to database
        this.saveStudySession().then(() => {
            // Show success message
            this.showToast('زمان مطالعه با موفقیت ثبت شد', 'success');
            
            // Navigate back after a short delay
            setTimeout(() => {
                history.back();
            }, 1000);
        }).catch((error) => {
            console.error('Error saving study session:', error);
            this.showToast('خطا در ثبت زمان مطالعه', 'error');
            
            // Still navigate back even if save failed
            setTimeout(() => {
                history.back();
            }, 2000);
        });
    },
    
    async saveStudySession() {
        const studyData = {
            sessionId: this.sessionId || 0,
            durationSeconds: this.getElapsedTime()
        };
        
        console.log('Saving study session:', studyData);
        
        try {
            const response = await fetch(window.studyContentConfig.completeSessionUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify(studyData)
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const result = await response.json();
            console.log('Study session saved:', result);
            return result;
        } catch (error) {
            console.error('Failed to save study session:', error);
            throw error;
        }
    },
    
    exitWithoutSaving() {
        // Don't stop the timer, just navigate back
        // The timer should continue running for future attempts
        
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
        document.body.insertAdjacentHTML('beforeend', toastHtml);
        
        // Auto remove after 3 seconds
        setTimeout(() => {
            const alert = document.querySelector('.alert');
            if (alert) {
                alert.remove();
            }
        }, 3000);
    }
};

$(document).ready(function() {
    console.log('Document ready, initializing study session');
    // Initialize study session
    studySession.init();
});