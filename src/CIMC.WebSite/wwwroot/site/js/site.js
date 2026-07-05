(function () {
    'use strict';

    function ready(fn) {
        if (document.readyState !== 'loading') { fn(); }
        else { document.addEventListener('DOMContentLoaded', fn); }
    }

    ready(function () {
        initNavToggle();
        initBanners();
        initLazyLoad();
        initSmoothScroll();
        highlightActiveNav();
    });

    function initNavToggle() {
        var toggle = document.querySelector('[data-nav-toggle]');
        var menu = document.querySelector('[data-nav-menu]');
        if (!toggle || !menu) { return; }
        toggle.addEventListener('click', function () {
            menu.classList.toggle('open');
        });
        document.addEventListener('click', function (e) {
            if (!menu.contains(e.target) && !toggle.contains(e.target)) {
                menu.classList.remove('open');
            }
        });
    }

    function initBanners() {
        var banners = document.querySelectorAll('[data-banner]');
        banners.forEach(function (banner) {
            var slides = banner.querySelectorAll('.banner-slide');
            if (slides.length <= 1) { return; }

            var autoplay = banner.getAttribute('data-autoplay') === 'true';
            var interval = parseInt(banner.getAttribute('data-interval') || '5000', 10);
            var current = 0;
            var timer = null;
            var dots = banner.querySelectorAll('[data-banner-dots] .banner-dot');
            var prevBtn = banner.querySelector('[data-banner-prev]');
            var nextBtn = banner.querySelector('[data-banner-next]');

            function show(idx) {
                if (idx < 0) { idx = slides.length - 1; }
                if (idx >= slides.length) { idx = 0; }
                slides[current].classList.remove('active');
                if (dots[current]) { dots[current].classList.remove('active'); }
                current = idx;
                slides[current].classList.add('active');
                if (dots[current]) { dots[current].classList.add('active'); }
            }

            function next() { show(current + 1); }
            function prev() { show(current - 1); }

            function startAuto() {
                if (!autoplay) { return; }
                stopAuto();
                timer = setInterval(next, interval);
            }
            function stopAuto() {
                if (timer) { clearInterval(timer); timer = null; }
            }

            if (prevBtn) { prevBtn.addEventListener('click', function () { prev(); startAuto(); }); }
            if (nextBtn) { nextBtn.addEventListener('click', function () { next(); startAuto(); }); }
            dots.forEach(function (dot, i) {
                dot.addEventListener('click', function () { show(i); startAuto(); });
            });

            banner.addEventListener('mouseenter', stopAuto);
            banner.addEventListener('mouseleave', startAuto);

            startAuto();
        });
    }

    function initLazyLoad() {
        if (!('IntersectionObserver' in window)) { return; }
        var imgs = document.querySelectorAll('img[loading="lazy"]');
        if (!imgs.length) { return; }
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    var img = entry.target;
                    observer.unobserve(img);
                }
            });
        }, { rootMargin: '50px' });
        imgs.forEach(function (img) { observer.observe(img); });
    }

    function initSmoothScroll() {
        document.querySelectorAll('a[href^="#"]').forEach(function (a) {
            a.addEventListener('click', function (e) {
                var href = a.getAttribute('href');
                if (href === '#' || href === '#!') { return; }
                var target = document.querySelector(href);
                if (!target) { return; }
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            });
        });
    }

    function highlightActiveNav() {
        var wrapper = document.querySelector('.page-wrapper');
        if (!wrapper) { return; }
        var currentPath = wrapper.getAttribute('data-current-path') || '/';
        var navLinks = document.querySelectorAll('.main-nav .nav-item > a');
        navLinks.forEach(function (link) {
            var href = link.getAttribute('href');
            if (!href || href === '#') { return; }
            if (href === '/' && currentPath === '/') {
                link.parentElement.classList.add('active');
            } else if (href !== '/' && currentPath.indexOf(href) === 0) {
                link.parentElement.classList.add('active');
            }
        });
    }
})();
