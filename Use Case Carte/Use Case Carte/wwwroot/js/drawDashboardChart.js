window.drawDashboardChart = function (labels, montants) {

    const canvas = document.getElementById("myChart");

    if (!canvas)
        return;

    const ctx = canvas.getContext("2d");

    // Évite d'empiler plusieurs graphiques
    if (window.dashboardChart) {
        window.dashboardChart.destroy();
    }

    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, "#FF0054");
    gradient.addColorStop(1, "#FA003F");

    const gradientFill = ctx.createLinearGradient(0, 0, 0, 400);
    gradientFill.addColorStop(0, "rgba(255,0,84,0.25)");
    gradientFill.addColorStop(1, "rgba(250,0,63,0.03)");

    window.dashboardChart = new Chart(ctx, {
        type: "line",
        data: {
            labels: labels,
            datasets: [{
                label: "Montant total",
                data: montants,
                backgroundColor: gradientFill,
                borderColor: gradient,
                borderWidth: 3,
                tension: 0.4,
                fill: true,
                pointRadius: 5,
                pointHoverRadius: 7,
                pointBackgroundColor: "#FA003F",
                pointBorderColor: "#fff",
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: "index",
                intersect: false
            },
            plugins: {
                legend: {
                    display: true,
                    labels: {
                        color: "#444",
                        font: {
                            size: 14
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.parsed.y.toLocaleString("fr-FR") + " XAF";
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    }
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toLocaleString("fr-FR");
                        }
                    }
                }
            }
        }
    });
};