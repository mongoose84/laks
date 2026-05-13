(function () {
    "use strict";

    if (!window.Chart || !window.laksDashboard) {
        return;
    }

    var canvas = document.getElementById("waterLevelChart");
    if (!canvas) {
        return;
    }

    var points = Array.isArray(window.laksDashboard.waterLevelSeries)
        ? window.laksDashboard.waterLevelSeries
        : [];

    if (points.length === 0) {
        return;
    }

    var trend = window.laksDashboard.trend || "stable";

    var labels = points.map(function (p, index) {
        if (index % 6 !== 0 && index !== points.length - 1) {
            return "";
        }
        return new Date(p.t).toLocaleTimeString("da-DK", { hour: "2-digit", minute: "2-digit" });
    });

    var values = points.map(function (p) { return p.v; });

    new Chart(canvas, {
        type: "line",
        data: {
            labels: labels,
            datasets: [{
                data: values,
                borderColor: "#d97757",
                backgroundColor: "rgba(217,119,87,0.12)",
                pointRadius: 0,
                pointHoverRadius: 5,
                pointHoverBackgroundColor: "#d97757",
                borderWidth: 2,
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: "#1c1814",
                    titleColor: "#d97757",
                    bodyColor: "#f5ede0",
                    borderColor: "#3a3128",
                    borderWidth: 1,
                    callbacks: {
                        label: function (ctx) {
                            return Number(ctx.parsed.y).toFixed(2).replace(".", ",") + " m";
                        }
                    }
                }
            },
            scales: {
                y: {
                    ticks: {
                        color: "#7a6f60",
                        font: { size: 10 },
                        callback: function (value) {
                            return Number(value).toFixed(2).replace(".", ",") + " m";
                        }
                    },
                    grid: { color: "rgba(217,119,87,0.04)" },
                    border: { color: "#3a3128" }
                },
                x: {
                    ticks: { color: "#7a6f60", font: { size: 10 }, maxTicksLimit: 8 },
                    grid: { color: "rgba(217,119,87,0.04)" },
                    border: { color: "#3a3128" }
                }
            }
        }
    });
})();
