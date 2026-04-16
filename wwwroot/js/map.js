const MAX_DISTANCE_KM = 20;
let currentSearchTerm = "";
let currentNearbyIds = "";
let userMarker = null;
let lastUserCoords = null;
let lastPromptedStationId = null;
let lastPromptTime = 0;

// 1. Initialize Map Immediately
const map = L.map('map', {
    zoomControl: false,
    attributionControl: false,
    maxZoom: 19
}).setView([-26.2041, 28.0473], 13);

const markers = L.markerClusterGroup({
    showCoverageOnHover: false,
    spiderfyOnMaxZoom: true,
    maxClusterRadius: 40
});
map.addLayer(markers);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap contributors'
}).addTo(map);

// 2. Helper Functions
function calculateDistance(lat1, lon1, lat2, lon2) {
    const R = 6371;
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLon = (lon2 - lon1) * Math.PI / 180;
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) + Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * Math.sin(dLon / 2) * Math.sin(dLon / 2);
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

function getBrandInfo(brand) {
    const b = (brand || '').toLowerCase();
    let logo = "/images/logos/fuel.png";
    let initial = (brand || '?').charAt(0);
    return { logo, class: 'brand-default', initial };
}

const zoomToStation = (id) => {
    const s = stations.find(s => s.id == id);
    if (s) map.flyTo([s.lat, s.lng], 16);
};

const showDetails = (id) => {
    currentStationId = id;
    fetch(`/Stations/Details/${id}?partial=true`).then(r => r.text()).then(h => {
        const content = document.getElementById('modalContent');
        if (content) content.innerHTML = h;
        const modal = new bootstrap.Modal(document.getElementById('stationModal'));
        modal.show();
    }).catch(err => console.error('Details load failed', err));
};

// 3. Main UI Update Logic
function updateNearbyStations(searchTerm = "", force = false) {
    if (!window.stations) return;
    
    let nearby;
    if (lastUserCoords) {
        nearby = stations.map(s => {
            s.distance = calculateDistance(lastUserCoords[0], lastUserCoords[1], s.lat, s.lng);
            return s;
        }).filter(s => {
            const matchesSearch = !searchTerm || 
                s.name.toLowerCase().includes(searchTerm) || 
                (s.brand && s.brand.toLowerCase().includes(searchTerm)) ||
                (s.suburb && s.suburb.toLowerCase().includes(searchTerm));
            return s.distance <= MAX_DISTANCE_KM && matchesSearch;
        }).sort((a, b) => a.distance - b.distance);
    } else {
        nearby = stations.filter(s => {
            return !searchTerm || 
                s.name.toLowerCase().includes(searchTerm) || 
                (s.brand && s.brand.toLowerCase().includes(searchTerm)) ||
                (s.suburb && s.suburb.toLowerCase().includes(searchTerm));
        });
        if (nearby.length > 50) nearby = nearby.slice(0, 50);
        nearby.forEach(s => s.distance = null);
    }

    const newIds = nearby.map(s => s.id).join(',');
    if (!force && newIds === currentNearbyIds) return;
    currentNearbyIds = newIds;

    const list = document.getElementById('nearbyList');
    if (list) {
        list.innerHTML = '';
        if (nearby.length === 0) {
            list.innerHTML = '<div class="text-center py-5 text-muted small">No stations found.</div>';
        }
    }
    
    markers.clearLayers();

    nearby.forEach(s => {
        addStationMarker(s);
        if (list) {
            const latest = s.LatestReport || s.latestReport;
            const priceLabel = (latest && latest.price) ? `R ${latest.price.toFixed(2)}` : "—";
            const distLabel = s.distance !== null ? `${s.distance.toFixed(1)} km` : "---";
            
            const item = document.createElement('div');
            item.className = 'list-group-item station-item list-group-item-action border-0 mb-2 rounded-3 shadow-sm';
            item.innerHTML = `
                <div class="d-flex w-100 justify-content-between align-items-center">
                    <h6 class='fw-bold mb-0'>${s.name}</h6>
                    <small class="text-primary fw-bold">${distLabel}</small>
                </div>
                <div class="d-flex justify-content-between align-items-center mt-1">
                    <small class="text-muted">Price: <span class="badge bg-light text-dark border">${priceLabel}</span></small>
                    <button onclick="showDetails(${s.id})" class="btn btn-outline-primary btn-sm rounded-pill px-3 py-0" style="font-size: 0.7rem;">Details</button>
                </div>
            `;
            item.onclick = (e) => { if(e.target.tagName !== 'BUTTON') zoomToStation(s.id); };
            list.appendChild(item);
        }
    });
    
    const countEl = document.getElementById('nearbyCount');
    if (countEl) countEl.innerText = nearby.length;
}

function addStationMarker(s) {
    const info = getBrandInfo(s.brand);
    const latest = s.LatestReport || s.latestReport;
    let statusClass = "marker-no-data";
    let dotClass = "dot-no-data";

    if (latest) {
        const diffHrs = (new Date() - new Date(latest.created_at)) / (1000 * 60 * 60);
        if (diffHrs <= 12) {
            statusClass = latest.available ? "marker-available" : "marker-unavailable";
            dotClass = latest.available ? "dot-available" : "dot-unavailable";
        }
    }

    // Use the logo path already provided by the backend or fallback
    const logoUrl = s.logo_path || info.logo;

    const icon = L.divIcon({
        className: '',
        html: `<div id="marker-${s.id}" class="brand-marker ${info.class} ${statusClass}">
                <img src="${logoUrl}" onerror="this.style.display='none';this.parentElement.innerText='${info.initial}'">
                <div class="status-dot ${dotClass}"></div>
               </div>`,
        iconSize: [48, 54], iconAnchor: [24, 54]
    });
    const marker = L.marker([s.lat, s.lng], { icon: icon });
    marker.stationId = s.id;
    marker.bindPopup(`<h5>${s.name}</h5><button onclick='showDetails(${s.id})' class='btn btn-primary btn-sm w-100'>Report</button>`);
    markers.addLayer(marker);
}

// 4. Geolocation Logic
function updateUserMarker(lat, lng) {
    lastUserCoords = [lat, lng];
    if (userMarker) {
        userMarker.setLatLng(lastUserCoords);
    } else {
        const userIcon = L.divIcon({
            className: '',
            html: '<div class="user-location-marker"></div>',
            iconSize: [20, 20],
            iconAnchor: [10, 10]
        });
        userMarker = L.marker(lastUserCoords, { icon: userIcon, zIndexOffset: 2000 }).addTo(map);
    }
    updateNearbyStations(currentSearchTerm);
    checkProximityPrompt(lat, lng);
}

function handleGeoError(err) {
    console.warn(`GeoError(${err.code}): ${err.message}`);
    if (!lastUserCoords) updateNearbyStations(currentSearchTerm, true);
}

function initGeolocation() {
    if (!navigator.geolocation) {
        updateNearbyStations(currentSearchTerm, true);
        return;
    }

    const options = { enableHighAccuracy: true, timeout: 10000, maximumAge: 30000 };
    
    if (window.isUserAuthenticated) {
        document.getElementById('trackingStatus')?.classList.remove('d-none');
        navigator.geolocation.watchPosition(pos => {
            updateUserMarker(pos.coords.latitude, pos.coords.longitude);
        }, handleGeoError, options);
    } else {
        navigator.geolocation.getCurrentPosition(pos => {
            updateUserMarker(pos.coords.latitude, pos.coords.longitude);
        }, handleGeoError, options);
    }
}

// 5. User Controls
function centerOnUser() {
    if (lastUserCoords) map.flyTo(lastUserCoords, 16);
    else { alert("Detecting location... Please ensure GPS is enabled."); initGeolocation(); }
}

function resetView() { map.flyTo([-26.2041, 28.0473], 13); }

function findNearest() {
    if (!lastUserCoords) return alert("Enable location to find nearest station.");
    const sorted = stations.map(s => ({ ...s, d: calculateDistance(lastUserCoords[0], lastUserCoords[1], s.lat, s.lng) }))
                          .sort((a, b) => a.d - b.d);
    if (sorted.length > 0) zoomToStation(sorted[0].id);
}

function findNearestAvailable() {
    if (!lastUserCoords) return alert("Enable location to find nearest available station.");
    const available = stations.filter(s => (s.LatestReport || s.latestReport)?.available)
                              .map(s => ({ ...s, d: calculateDistance(lastUserCoords[0], lastUserCoords[1], s.lat, s.lng) }))
                              .sort((a, b) => a.d - b.d);
    if (available.length > 0) zoomToStation(available[0].id);
    else alert("No available stations found nearby.");
}

function toggleSidebar() {
    document.getElementById('sidebar').classList.toggle('sidebar-hidden');
    document.getElementById('sidebarToggleBtn').classList.toggle('d-none');
}

function showTutorialInternal() {
    alert("How it works:\n1. Enable location to see nearby stations.\n2. Green = Available, Red = Dry.\n3. Click a station to report fuel status!");
}

// 6. Proximity & Reporting
function checkProximityPrompt(lat, lng) {
    const now = Date.now();
    if (now - lastPromptTime < 30 * 60 * 1000) return;

    const nearby = stations.find(s => calculateDistance(lat, lng, s.lat, s.lng) <= 0.15);
    if (nearby && nearby.id !== lastPromptedStationId) {
        lastPromptedStationId = nearby.id;
        lastPromptTime = now;
        currentStationId = nearby.id;
        document.getElementById('proxStationName').innerText = nearby.name;
        new bootstrap.Modal(document.getElementById('proximityModal')).show();
    }
}

function submitProximityReport(isAvailable) {
    const modal = bootstrap.Modal.getInstance(document.getElementById('proximityModal'));
    if (modal) modal.hide();
    if (typeof submitQuickReport === 'function') {
        submitQuickReport({ station_id: currentStationId, available: isAvailable, fuel_type: "Petrol/Diesel", queue_mins: 5, notes: "Proximity auto-report" });
    }
}

// 7. Event Listeners & Initialization
document.addEventListener('reportSubmitted', (e) => {
    const report = e.detail;
    const station = stations.find(s => s.id == report.station_id);
    if (station) {
        station.LatestReport = report;
        updateNearbyStations(currentSearchTerm, true);
    }
});

const searchInp = document.getElementById('stationSearch');
if (searchInp) {
    searchInp.addEventListener('input', (e) => {
        currentSearchTerm = e.target.value.toLowerCase();
        updateNearbyStations(currentSearchTerm);
    });
}

// Start everything
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js').catch(err => console.log('SW Fail', err));
}

// Load stations and then trigger geolocation prompt
updateNearbyStations("", true);
initGeolocation();

// Check for tutorial flag
if (new URLSearchParams(window.location.search).get('showTutorial') === 'true') {
    setTimeout(() => {
        showTutorialInternal();
        // Clean up URL to prevent tutorial re-showing on refresh
        const url = new URL(window.location);
        url.searchParams.delete('showTutorial');
        window.history.replaceState({}, '', url);
    }, 1000); // Small delay to let map initialize
}
