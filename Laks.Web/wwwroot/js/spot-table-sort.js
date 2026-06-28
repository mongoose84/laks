(function () {
    'use strict';

    var table = document.getElementById('spotTable');
    if (!table) { return; }

    var headers = table.querySelectorAll('thead th');
    var tbody   = table.querySelector('tbody');
    var buttons = table.querySelectorAll('button.ed-sort-btn');

    // Build a map from sort-key -> column index
    var keyToIndex = {};
    buttons.forEach(function (btn) {
        var th      = btn.closest('th');
        var thIndex = Array.prototype.indexOf.call(th.parentNode.children, th);
        keyToIndex[btn.dataset.sortKey] = thIndex;
    });

    var currentKey = 'catches';
    var currentDir = 'desc'; // default initial sort

    function applySort(key, dir) {
        var colIndex = keyToIndex[key];
        var btn      = table.querySelector('[data-sort-key="' + key + '"]');
        var isNum    = btn && btn.dataset.sortType === 'number';

        var rows = Array.prototype.slice.call(tbody.querySelectorAll('tr'));
        rows.sort(function (a, b) {
            var av = a.children[colIndex] ? a.children[colIndex].dataset.sortValue : '';
            var bv = b.children[colIndex] ? b.children[colIndex].dataset.sortValue : '';
            var cmp;
            if (isNum) {
                var an = av === '' ? -Infinity : parseFloat(av);
                var bn = bv === '' ? -Infinity : parseFloat(bv);
                cmp = an - bn;
            } else {
                cmp = av.localeCompare(bv, 'da');
            }
            return dir === 'asc' ? cmp : -cmp;
        });

        rows.forEach(function (row) { tbody.appendChild(row); });

        // Update aria-sort on every th and aria-label on every sort button
        headers.forEach(function (th) {
            var thBtn = th.querySelector('button.ed-sort-btn');
            if (!thBtn) { return; }
            if (thBtn.dataset.sortKey === key) {
                th.setAttribute('aria-sort', dir === 'asc' ? 'ascending' : 'descending');
                var nextDirLabel = dir === 'asc' ? 'faldende' : 'stigende';
                thBtn.setAttribute('aria-label', 'Sortér efter ' + thBtn.textContent.trim() + ' ' + nextDirLabel);
            } else {
                th.setAttribute('aria-sort', 'none');
                thBtn.setAttribute('aria-label', 'Sortér efter ' + thBtn.textContent.trim() + ' stigende');
            }
        });
    }

    // Apply default sort on page load (aria attributes are already set in HTML;
    // this call re-applies them via JS so they reflect the live state)
    applySort(currentKey, currentDir);

    // Attach click handlers to all sort buttons
    buttons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            var key = btn.dataset.sortKey;
            var dir;
            if (key === currentKey) {
                dir = currentDir === 'asc' ? 'desc' : 'asc';
            } else {
                dir = 'asc';
            }
            currentKey = key;
            currentDir = dir;
            applySort(key, dir);
        });
    });
}());