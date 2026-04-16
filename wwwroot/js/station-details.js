document.addEventListener('DOMContentLoaded', function () {
    const ctx = document.getElementById('queueChart');
    if (!ctx || !window.chartData) return;

    const labels = window.chartData.map(r => r.time);
    const data = window.chartData.map(r => r.queue);

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Queue Length (mins)',
                data: data,
                borderColor: '#0d6efd',
                backgroundColor: 'rgba(13, 110, 253, 0.1)',
                fill: true,
                tension: 0.4,
                pointRadius: 4,
                pointBackgroundColor: '#0d6efd'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Minutes'
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
});
