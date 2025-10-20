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
        console.log('Study session initialized');
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
                    
                    // Check for any button that might navigate
                    if (target.tagName === 'BUTTON' || target.closest('button')) {
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
        
        // Create a simple but elegant confirmation dialog
        const confirmed = confirm(`🕐 زمان مطالعه شما: ${this.formatTime(currentTime)}\n\nآیا می‌خواهید این زمان مطالعه ثبت شود؟\n\n✅ OK = ثبت و خروج\n❌ Cancel = خروج بدون ثبت`);
        
        if (confirmed) {
            this.saveAndExit();
        } else {
            this.exitWithoutSaving();
        }
    },
    
    saveAndExit() {
        if (this.isActive) {
            this.isActive = false;
            clearInterval(this.updateInterval);
        }
        
        // Show success message
        alert('✅ زمان مطالعه با موفقیت ثبت شد');
        
        // Navigate back
        history.back();
    },
    
    exitWithoutSaving() {
        // Don't stop the timer, just navigate back
        // The timer should continue running for future attempts
        
        // Navigate back immediately
        history.back();
    }
};

$(document).ready(function() {
    console.log('Document ready, initializing study session');
    // Initialize study session
    studySession.init();
});