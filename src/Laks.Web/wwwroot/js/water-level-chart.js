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
        return new Date(p.t).toLocaleTimeString("nb-NO", { hour: "2-digit", minute: "2-digit" });
    });

    var values = points.map(function (p) { return p.v; });

    new Chart(canvas, {
        type: "line",
        data: {
            labels: labels,
            datasets: [{
                data: values,
                borderColor: isRising ? "#1f7a3f" : "#bf5f00",
                backgroundColor: isRising ? "rgba(31,122,63,0.15)" : "rgba(191,95,0,0.15)",
                pointRadius: 0,
                borderWidth: 2,
                fill: true,
                tension: 0.25
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    ticks: {
                        callback: function (value) { return value + " m"; }
                    }
                }
            }
        }
    });
})();
