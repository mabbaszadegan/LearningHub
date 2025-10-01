const CACHE_NAME = 'edutrack-v1';
const STATIC_CACHE_URLS = [
    '/',
    '/css/site.css',
    '/js/site.js',
    '/js/pwa.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/lib/jquery/dist/jquery.min.js'
];

const DYNAMIC_CACHE_URLS = [
    '/Catalog',
    '/Exam',
    '/Classroom',
    '/Progress'
];

// Install event - cache static resources
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('Caching static resources');
                return cache.addAll(STATIC_CACHE_URLS);
            })
            .then(() => {
                console.log('Static resources cached successfully');
                return self.skipWaiting();
            })
            .catch(error => {
                console.error('Failed to cache static resources:', error);
            })
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames.map(cacheName => {
                        if (cacheName !== CACHE_NAME) {
                            console.log('Deleting old cache:', cacheName);
                            return caches.delete(cacheName);
                        }
                    })
                );
            })
            .then(() => {
                console.log('Service worker activated');
                return self.clients.claim();
            })
    );
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', event => {
    // Skip non-GET requests
    if (event.request.method !== 'GET') {
        return;
    }

    // Skip requests to external domains
    if (!event.request.url.startsWith(self.location.origin)) {
        return;
    }

    event.respondWith(
        caches.match(event.request)
            .then(response => {
                if (response) {
                    console.log('Serving from cache:', event.request.url);
                    return response;
                }

                console.log('Fetching from network:', event.request.url);
                return fetch(event.request)
                    .then(response => {
                        // Don't cache non-successful responses
                        if (!response || response.status !== 200 || response.type !== 'basic') {
                            return response;
                        }

                        // Clone the response
                        const responseToCache = response.clone();

                        // Cache dynamic pages
                        if (DYNAMIC_CACHE_URLS.some(url => event.request.url.includes(url))) {
                            caches.open(CACHE_NAME)
                                .then(cache => {
                                    cache.put(event.request, responseToCache);
                                });
                        }

                        return response;
                    })
                    .catch(error => {
                        console.error('Fetch failed:', error);
                        
                        // Return offline page for navigation requests
                        if (event.request.mode === 'navigate') {
                            return caches.match('/');
                        }
                        
                        throw error;
                    });
            })
    );
});

// Background sync for offline form submissions
self.addEventListener('sync', event => {
    if (event.tag === 'exam-submission') {
        event.waitUntil(
            processOfflineSubmissions()
        );
    }
});

async function processOfflineSubmissions() {
    try {
        const submissions = await getOfflineSubmissions();
        
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
                    await removeOfflineSubmission(submission.id);
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

// IndexedDB operations for offline submissions
async function getOfflineSubmissions() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('EduTrackOffline', 1);
        
        request.onerror = () => reject(request.error);
        request.onsuccess = () => {
            const db = request.result;
            const transaction = db.transaction(['submissions'], 'readonly');
            const store = transaction.objectStore('submissions');
            const getAllRequest = store.getAll();
            
            getAllRequest.onsuccess = () => resolve(getAllRequest.result);
            getAllRequest.onerror = () => reject(getAllRequest.error);
        };
        
        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains('submissions')) {
                db.createObjectStore('submissions', { keyPath: 'id', autoIncrement: true });
            }
        };
    });
}

async function removeOfflineSubmission(id) {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('EduTrackOffline', 1);
        
        request.onerror = () => reject(request.error);
        request.onsuccess = () => {
            const db = request.result;
            const transaction = db.transaction(['submissions'], 'readwrite');
            const store = transaction.objectStore('submissions');
            const deleteRequest = store.delete(id);
            
            deleteRequest.onsuccess = () => resolve();
            deleteRequest.onerror = () => reject(deleteRequest.error);
        };
    });
}
