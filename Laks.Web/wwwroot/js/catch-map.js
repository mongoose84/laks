(function () {
    "use strict";

    if (!window.L || !window.laksDashboard) {
        return;
    }

    var mapElement = document.getElementById("catchMap");
    if (!mapElement) {
        return;
    }

    var seasonButton = document.getElementById("mapScopeSeason");
    var allButton = document.getElementById("mapScopeAll");
    var empty = document.getElementById("catchMapEmpty");

    var map = L.map("catchMap").setView([59.186959, 9.993806], 16);
    L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png", {
        attribution: "&copy; OpenStreetMap contributors &copy; CARTO"
    }).addTo(map);

    var markersLayer = L.layerGroup().addTo(map);

    var seasonData = Array.isArray(window.laksDashboard.currentSeasonLocations)
        ? window.laksDashboard.currentSeasonLocations
        : [];
    var allData = Array.isArray(window.laksDashboard.allTimeLocations)
        ? window.laksDashboard.allTimeLocations
        : [];

    function markerColor(daysAgo) {
        if (daysAgo <= 1) {
            return "#d97757";
        }

        if (daysAgo <= 7) {
            return "#e8a989";
        }

        if (daysAgo <= 30) {
            return "#b35d3f";
        }

        return "#7a6f60";
    }

    function esc(value) {
        var div = document.createElement("div");
        div.textContent = String(value || "");
        return div.innerHTML;
    }

    function renderLocations(data) {
        markersLayer.clearLayers();

        if (!data || data.length === 0) {
            empty.classList.remove("d-none");
            return;
        }

        empty.classList.add("d-none");

        data.forEach(function (point) {
            if (!point.lat || !point.lng) {
                return;
            }

            var marker = L.circleMarker([point.lat, point.lng], {
                radius: Math.max(4, Math.min(14, Number(point.w) + 2)),
                color: markerColor(point.daysAgo),
                fillColor: markerColor(point.daysAgo),
                fillOpacity: 0.6,
                weight: 1
            }).addTo(markersLayer);

            var popup = "<strong>" + esc(point.angler) + "</strong><br/>"
                + esc(point.w) + " kg " + esc(point.type) + "<br/>"
                + esc(point.location) + "<br/>"
                + esc(new Date(point.date).toLocaleDateString("da-DK")) + "<br/>"
                + "Agn: " + esc(point.bait || "-");

            marker.bindPopup(popup);
        });
    }

    function setMode(mode) {
        if (mode === "all") {
            seasonButton.classList.remove("btn-primary", "is-active");
            seasonButton.classList.add("btn-outline-primary");
            allButton.classList.add("btn-primary", "is-active");
            allButton.classList.remove("btn-outline-primary");
            renderLocations(allData);
            return;
        }

        allButton.classList.remove("btn-primary", "is-active");
        allButton.classList.add("btn-outline-primary");
        seasonButton.classList.add("btn-primary", "is-active");
        seasonButton.classList.remove("btn-outline-primary");
        renderLocations(seasonData);
    }

    seasonButton.addEventListener("click", function () { setMode("season"); });
    allButton.addEventListener("click", function () { setMode("all"); });

    setMode(seasonData.length === 0 ? "all" : "season");
})();
