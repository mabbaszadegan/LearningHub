// PWA functionality
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/js/sw.js')
            .then(registration => {
                console.log('SW registered: ', registration);
            })
            .catch(registrationError => {
                console.log('SW registration failed: ', registrationError);
            });
    });
}

// Install prompt
let deferredPrompt;
let installButton;

window.addEventListener('beforeinstallprompt', (e) => {
    console.log('PWA install prompt triggered');
    e.preventDefault();
    deferredPrompt = e;
    
    // Show install button
    installButton = document.getElementById('install-button');
    if (installButton) {
        installButton.style.display = 'inline-block';
        installButton.addEventListener('click', async () => {
            if (deferredPrompt) {
                deferredPrompt.prompt();
                const { outcome } = await deferredPrompt.userChoice;
                console.log(`User ${outcome} the install prompt`);
                deferredPrompt = null;
                installButton.style.display = 'none';
            }
        });
    }
});

// Handle app installed event
window.addEventListener('appinstalled', () => {
    console.log('PWA was installed');
    if (installButton) {
        installButton.style.display = 'none';
    }
});

// Offline submission queue
class OfflineSubmissionQueue {
    constructor() {
        this.dbName = 'EduTrackOffline';
        this.dbVersion = 1;
        this.initDB();
    }

    async initDB() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, this.dbVersion);
            
            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                this.db = request.result;
                resolve();
            };
            
            request.onupgradeneeded = () => {
                const db = request.result;
                if (!db.objectStoreNames.contains('submissions')) {
                    db.createObjectStore('submissions', { keyPath: 'id', autoIncrement: true });
                }
            };
        });
    }

    async queueSubmission(url, data, csrfToken) {
        const submission = {
            url: url,
            data: data,
            csrfToken: csrfToken,
            timestamp: new Date().toISOString()
        };

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['submissions'], 'readwrite');
            const store = transaction.objectStore('submissions');
            const request = store.add(submission);
            
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async processQueue() {
        try {
            const submissions = await this.getSubmissions();
            
            for (const submission of submissions) {
                try {
                    const response = await fetch(submission.url, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-CSRF-TOKEN': submission.csrfToken
                        },
                        body: JSON.stringify(submission.data)
                    });

                    if (response.ok) {
                        await this.removeSubmission(submission.id);
                        console.log('Offline submission processed:', submission.id);
                    }
                } catch (error) {
                    console.error('Failed to process offline submission:', error);
                }
            }
        } catch (error) {
            console.error('Error processing offline submissions:', error);
        }
    }

    async getSubmissions() {
        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['submissions'], 'readonly');
            const store = transaction.objectStore('submissions');
            const request = store.getAll();
            
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async removeSubmission(id) {
        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['submissions'], 'readwrite');
            const store = transaction.objectStore('submissions');
            const request = store.delete(id);
            
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    }
}

// Initialize offline submission queue
const offlineQueue = new OfflineSubmissionQueue();

// Process queue when back online
window.addEventListener('online', () => {
    console.log('Back online, processing offline submissions...');
    offlineQueue.processQueue();
});

// Exam timer functionality
class ExamTimer {
    constructor(durationMinutes, onTimeUp) {
        this.durationMinutes = durationMinutes;
        this.onTimeUp = onTimeUp;
        this.startTime = Date.now();
        this.timer = null;
        this.isRunning = false;
    }

    start() {
        if (this.isRunning) return;
        
        this.isRunning = true;
        this.timer = setInterval(() => {
            const elapsed = Date.now() - this.startTime;
            const remaining = (this.durationMinutes * 60 * 1000) - elapsed;
            
            if (remaining <= 0) {
                this.stop();
                this.onTimeUp();
                return;
            }
            
            this.updateDisplay(remaining);
        }, 1000);
    }

    stop() {
        if (this.timer) {
            clearInterval(this.timer);
            this.timer = null;
        }
        this.isRunning = false;
    }

    updateDisplay(remainingMs) {
        const minutes = Math.floor(remainingMs / 60000);
        const seconds = Math.floor((remainingMs % 60000) / 1000);
        
        const display = document.getElementById('exam-timer');
        if (display) {
            display.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
            
            // Change color when time is running low
            if (minutes < 5) {
                display.classList.add('text-danger');
            } else if (minutes < 10) {
                display.classList.add('text-warning');
            }
        }
    }
}

// Export for global use
window.OfflineSubmissionQueue = OfflineSubmissionQueue;
window.ExamTimer = ExamTimer;
window.offlineQueue = offlineQueue;
