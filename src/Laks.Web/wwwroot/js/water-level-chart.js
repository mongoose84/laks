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
    var isRising = trend === "rising";

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
                borderColor: isRising ? "#34d399" : "#fbbf24",
                backgroundColor: isRising ? "rgba(52,211,153,0.15)" : "rgba(251,191,36,0.15)",
                pointRadius: 0,
                borderWidth: 2,
                fill: true,
                tension: 0.25
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: "#1c2a35",
                    titleColor: "#e8edf2",
                    bodyColor: "#8899a8",
                    borderColor: "#243442",
                    borderWidth: 1
                }
            },
            scales: {
                y: {
                    ticks: {
                        color: "#8899a8",
                        callback: function (value) { return Number(value).toFixed(2) + " m"; }
                    },
                    grid: { color: "#243442" }
                },
                x: {
                    ticks: { color: "#8899a8" },
                    grid: { color: "#243442" }
                }
            }
        }
    });
})();
