// Shared reporting state
let reportData = {};
let currentStationId = 0;

// Authentication state
window.isUserAuthenticated = window.isUserAuthenticated || false;

// Global showTutorial handler
function showTutorial() {
    if (window.location.pathname.toLowerCase().includes('/stations/map')) {
        // If on map page, call the real one implemented in map.js
        if (typeof window.showTutorialInternal === 'function') {
            window.showTutorialInternal();
        }
    } else {
        // If not on map page, redirect to map page with tutorial flag
        window.location.href = '/Stations/Map?showTutorial=true';
    }
}

function reportStep(step, data) {
    if (data) {
        reportData = { ...reportData, ...data };
    }
    
    // Hide all steps
    document.querySelectorAll('.report-step').forEach(el => el.classList.add('d-none'));
    
    // Show current step
    const stepEl = document.getElementById(`step${step}`);
    if (stepEl) {
        stepEl.classList.remove('d-none');
    }
}

function toggleCustomQueue() {
    const el = document.getElementById('customQueueInput');
    el.classList.toggle('d-none');
    if (!el.classList.contains('d-none')) {
        document.getElementById('customMins').focus();
    }
}

function submitCustomQueue() {
    const mins = document.getElementById('customMins').value;
    if (mins && mins >= 0) {
        reportStep(4, { queue_mins: parseInt(mins) });
    } else {
        alert('Please enter a valid number of minutes.');
    }
}

function togglePriceInput() {
    const el = document.getElementById('priceInputArea');
    el.classList.toggle('d-none');
    if (!el.classList.contains('d-none')) {
        document.getElementById('fuelPrice').focus();
    }
}

function submitPrice() {
    const price = document.getElementById('fuelPrice').value;
    if (price && price > 0) {
        submitQuickReport({ price: parseFloat(price) });
    } else {
        submitQuickReport({ price: null }); // Skip if invalid/empty
    }
}

function getAntiForgeryToken() {
    const form = document.getElementById('antiForgeryForm');
    if (form) {
        return form.querySelector('input[name="__RequestVerificationToken"]').value;
    }
    // Try to find it in any other form
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

function submitQuickReport(finalData) {
    reportData = { ...reportData, ...finalData, station_id: currentStationId };
    
    // Show loading or just proceed to submit
    const formData = new URLSearchParams();
    for (const key in reportData) {
        formData.append(key, reportData[key]);
    }
    
    const token = getAntiForgeryToken();
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    fetch('/Reports/CreateAjax', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: formData.toString()
    }).then(response => {
        if (response.ok) {
            response.json().then(reportData => {
                reportStep('Success');
                
                // Dispatch a custom event for the UI to respond to in real-time
                const event = new CustomEvent('reportSubmitted', { detail: reportData });
                document.dispatchEvent(event);

                // Auto-close modal after success if on map
                if (window.location.pathname.includes('Map')) {
                    setTimeout(() => {
                        const modalEl = document.getElementById('stationModal');
                        if (modalEl) {
                            const modal = bootstrap.Modal.getInstance(modalEl);
                            if (modal) modal.hide();
                        }
                    }, 1500);
                }
            });
        } else if (response.status === 401) {
            alert('Please login to submit a report.');
            window.location.href = '/Account/Login';
        } else {
            alert('Error submitting report. Please try again.');
        }
    });
}

function vote(reportId, isUpvote, stationId) {
    const token = getAntiForgeryToken();
    fetch('/Reports/Vote', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `reportId=${reportId}&isUpvote=${isUpvote}&__RequestVerificationToken=${token}`
    })
    .then(response => {
        if (response.ok) return response.json();
        if (response.status === 401) {
            alert('Please login to vote.');
            window.location.href = '/Account/Login';
            throw new Error('Unauthorized');
        }
        throw new Error('Vote failed');
    })
    .then(data => {
        if (data.isHidden) {
            // If hidden, refresh the whole list
            if (window.showDetails) showDetails(stationId);
            else location.reload();
        } else {
            // Update counts instantly
            const upEl = document.getElementById(`up-${reportId}`);
            const downEl = document.getElementById(`down-${reportId}`);
            const fullUpEl = document.getElementById(`full-up-${reportId}`);
            const fullDownEl = document.getElementById(`full-down-${reportId}`);
            
            if (upEl) upEl.innerText = data.upvotes;
            if (downEl) downEl.innerText = data.downvotes;
            if (fullUpEl) fullUpEl.innerText = data.upvotes;
            if (fullDownEl) fullDownEl.innerText = data.downvotes;
        }
    })
    .catch(err => {
        if (err.message !== 'Unauthorized') console.error(err);
    });
}