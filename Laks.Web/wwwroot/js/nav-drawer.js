(function () {
    document.documentElement.classList.add('js');
    var btn = document.getElementById('edBurger');
    var drawer = document.getElementById('edNavDrawer');
    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        var open = drawer.classList.toggle('is-open');
        btn.setAttribute('aria-expanded', String(open));
        btn.setAttribute('aria-label', open ? 'Luk navigation' : 'Åbn navigation');
    });
    document.addEventListener('click', function (e) {
        if (!drawer.contains(e.target) && !btn.contains(e.target)) {
            drawer.classList.remove('is-open');
            btn.setAttribute('aria-expanded', 'false');
            btn.setAttribute('aria-label', 'Åbn navigation');
        }
    });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            drawer.classList.remove('is-open');
            btn.setAttribute('aria-expanded', 'false');
            btn.setAttribute('aria-label', 'Åbn navigation');
            btn.focus();
        }
    });
}());
