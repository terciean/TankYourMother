const CACHE_NAME = 'wheregass-v3';
const STATIC_ASSETS = [
  '/css/site.css',
  '/css/map.css',
  '/js/site.js',
  '/js/map.js',
  '/js/station-details.js',
  '/lib/bootstrap/dist/css/bootstrap.min.css',
  '/favicon.ico',
  '/images/logos/fuel.png'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(STATIC_ASSETS))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys => Promise.all(
      keys.filter(key => key !== CACHE_NAME).map(key => caches.delete(key))
    )).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);
    
    // 0. Skip non-http(s) requests (e.g., chrome-extension://)
    if (!url.protocol.startsWith('http')) return;

    // 1. Skip caching for Admin, Account, and API-like POST requests
    if (url.pathname.startsWith('/Admin') || 
        url.pathname.startsWith('/Account') || 
        event.request.method !== 'GET') {
        return; 
    }

    // 2. Network-First Strategy for Navigation/Pages (Home, Map, Details)
    if (event.request.mode === 'navigate' || url.pathname.startsWith('/Stations')) {
        event.respondWith(
            fetch(event.request)
                .catch(() => caches.match(event.request))
                .then(response => {
                    if (!response) return fetch(event.request); // Fallback to network if cache also fails
                    return response;
                })
        );
        return;
    }

    // 3. Stale-While-Revalidate for Static Assets
    event.respondWith(
        caches.open(CACHE_NAME).then(cache => {
            return cache.match(event.request).then(response => {
                const fetchPromise = fetch(event.request).then(networkResponse => {
                    // Only cache successful HTTP responses
                    if (networkResponse && networkResponse.status === 200 && networkResponse.type === 'basic') {
                        cache.put(event.request, networkResponse.clone());
                    }
                    return networkResponse;
                }).catch(() => response); // If fetch fails, return the cached response
                return response || fetchPromise;
            });
        })
    );
});

// Listener for system-level notifications
self.addEventListener('notificationclick', event => {
  event.notification.close();
  const action = event.action;
  const stationId = event.notification.data.stationId;

  if (action === 'yes' || action === 'no') {
    event.waitUntil(
      clients.openWindow(`/Stations/Map?stationId=${stationId}&report=${action}`)
    );
  } else {
    event.waitUntil(
      clients.openWindow(`/Stations/Map`)
    );
  }
});
