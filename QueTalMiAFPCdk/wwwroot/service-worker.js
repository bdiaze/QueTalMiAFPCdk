const CACHE_NAME = 'static-v1';

const STATIC_EXTENSIONS = [
    '.css',
    '.js',
    '.png',
    '.jpg',
    '.jpeg',
    '.webp',
    '.svg',
    '.ico',
    '.woff',
    '.woff2',
    '.ttf'
];

self.addEventListener('install', () => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(
        Promise.all([
            self.clients.claim(),
            caches.keys().then(keys =>
                Promise.all(
                    keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k))
                )
            )
        ])
    );
});

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;

    const url = new URL(event.request.url);

    if (url.origin !== self.location.origin) return;

    const isStaticAsset = STATIC_EXTENSIONS.some(ext =>
        url.pathname.toLowerCase().endsWith(ext)
    );

    if (!isStaticAsset) return;

    event.respondWith(
        caches.open(CACHE_NAME).then(cache =>
            cache.match(event.request).then(cached => {
                if (cached) return cached;

                return fetch(event.request).then(response => {
                    if (response.ok && response.type === 'basic') {
                        cache.put(event.request, response.clone());
                    }
                    return response;
                });
            })
        )
    );
});