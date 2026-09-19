(function () {
    'use strict';

    function ready(fn) {
        if (document.readyState !== 'loading') { fn(); }
        else { document.addEventListener('DOMContentLoaded', fn); }
    }

    ready(function () {
        initNavToggle();
        initStickyHeader();
        initBanners();
        initLazyLoad();
        initSmoothScroll();
        highlightActiveNav();
        initMessageForms();
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

    function initStickyHeader() {
        var header = document.querySelector('.site-global-header.is-sticky, .site-global-header.is-fixed');
        if (!header) { return; }
        var scheduled = false;
        function update() {
            scheduled = false;
            header.classList.toggle('is-scrolled', window.scrollY > 24);
        }
        function schedule() {
            if (scheduled) { return; }
            scheduled = true;
            window.requestAnimationFrame(update);
        }
        update();
        window.addEventListener('scroll', schedule, { passive: true });
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

    function initMessageForms() {
        document.querySelectorAll('[data-message-form]').forEach(function (form) {
            var tip = form.querySelector('[data-message-tip]');
            var captchaImage = form.querySelector('[data-captcha-image]');
            var keyInput = form.querySelector('input[name="ValidateKey"]');

            function refreshCaptcha() {
                if (!captchaImage || !keyInput) { return; }
                var key = Date.now().toString() + '-' + Math.random().toString(36).slice(2);
                keyInput.value = key;
                captchaImage.src = '/Authorize/GetImg?key=' + encodeURIComponent(key) + '&v=' + Date.now();
            }

            if (captchaImage) { captchaImage.addEventListener('click', refreshCaptcha); }
            form.addEventListener('submit', function (event) {
                event.preventDefault();
                if (!form.reportValidity()) { return; }
                var button = form.querySelector('button[type="submit"]');
                if (button) { button.disabled = true; }
                if (tip) { tip.className = 'sb-contact-form-tip'; tip.textContent = '正在提交…'; }

                fetch(form.action || '/home/message', { method: 'POST', body: new FormData(form), credentials: 'same-origin' })
                    .then(function (response) {
                        if (!response.ok) { throw new Error('HTTP ' + response.status); }
                        return response.json();
                    })
                    .then(function (result) {
                        var success = Number(result.code) === 200;
                        if (tip) {
                            tip.classList.toggle('is-success', success);
                            tip.classList.toggle('is-error', !success);
                            tip.textContent = result.message || (success ? '提交成功' : '提交失败');
                        }
                        if (success) { form.reset(); }
                    })
                    .catch(function () {
                        if (tip) { tip.className = 'sb-contact-form-tip is-error'; tip.textContent = '提交失败，请稍后再试'; }
                    })
                    .finally(function () {
                        if (button) { button.disabled = false; }
                        refreshCaptcha();
                    });
            });
        });
    }
})();
