(function () {
    var header = document.querySelector("[data-header]");
    var nav = document.querySelector("[data-nav]");
    var navToggle = document.querySelector("[data-nav-toggle]");

    function updateHeader() {
        if (!header) return;
        header.classList.toggle("is-scrolled", window.scrollY > 24);
    }

    updateHeader();
    window.addEventListener("scroll", updateHeader, { passive: true });

    if (navToggle && nav) {
        navToggle.addEventListener("click", function () {
            nav.classList.toggle("is-open");
            navToggle.classList.toggle("is-open");
        });

        document.addEventListener("click", function (e) {
            if (window.innerWidth > 1024) return;
            if (!nav.contains(e.target) && !navToggle.contains(e.target)) {
                nav.classList.remove("is-open");
                navToggle.classList.remove("is-open");
            }
        });

        var allNavLinks = Array.prototype.slice.call(nav.querySelectorAll("a"));
        allNavLinks.forEach(function (link) {
            link.addEventListener("click", function () {
                if (window.innerWidth > 1024) return;
                nav.classList.remove("is-open");
                navToggle.classList.remove("is-open");
            });
        });
    }

    var hero = document.querySelector("[data-hero]");
    if (hero) {
        var slides = Array.prototype.slice.call(hero.querySelectorAll(".hero__slide"));
        var dots = Array.prototype.slice.call(hero.querySelectorAll("[data-hero-pager] button"));
        var index = 0;

        function setSlide(next) {
            index = next % slides.length;
            slides.forEach(function (slide, i) { slide.classList.toggle("is-active", i === index); });
            dots.forEach(function (dot, i) { dot.classList.toggle("is-active", i === index); });
        }

        dots.forEach(function (dot, i) {
            dot.addEventListener("click", function () { setSlide(i); });
        });

        window.setInterval(function () {
            setSlide(index + 1);
        }, 5200);
    }

    Array.prototype.slice.call(document.querySelectorAll("[data-page-hero]")).forEach(function (pageHero) {
        var pageSlides = Array.prototype.slice.call(pageHero.querySelectorAll(".page-hero__slide"));
        if (pageSlides.length <= 1) {
            return;
        }

        var current = 0;
        window.setInterval(function () {
            pageSlides[current].classList.remove("is-active");
            current = (current + 1) % pageSlides.length;
            pageSlides[current].classList.add("is-active");
        }, 5200);
    });

    var revealItems = Array.prototype.slice.call(document.querySelectorAll(".reveal"));
    var capArticles = Array.prototype.slice.call(document.querySelectorAll(".capability-grid article"));
    if ("IntersectionObserver" in window) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.16 });

        revealItems.forEach(function (item) { observer.observe(item); });

        capArticles.forEach(function (article, i) {
            article.style.transitionDelay = (i * 0.15) + "s";
            observer.observe(article);
        });
    } else {
        revealItems.forEach(function (item) { item.classList.add("is-visible"); });
        capArticles.forEach(function (article) { article.classList.add("is-visible"); });
    }

    var metricPanel = document.querySelector(".metric-panel");
    if (metricPanel && "IntersectionObserver" in window) {
        var metricStarted = false;
        var metricObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting && !metricStarted) {
                    metricStarted = true;
                    metricObserver.unobserve(entry.target);
                    startMetricCounters(metricPanel);
                }
            });
        }, { threshold: 0.3 });
        metricObserver.observe(metricPanel);
    } else if (metricPanel) {
        startMetricCounters(metricPanel);
    }

    function startMetricCounters(panel) {
        var strongs = Array.prototype.slice.call(panel.querySelectorAll("strong"));
        strongs.forEach(function (el) { animateMetric(el); });
    }

    function animateMetric(el) {
        var text = el.textContent;
        var match = text.match(/^([\d.]+)/);
        if (!match) return;
        var target = parseFloat(match[1]);
        var suffix = text.substring(match[1].length);
        var duration = 1800;
        var startTime = null;

        function step(timestamp) {
            if (!startTime) startTime = timestamp;
            var elapsed = timestamp - startTime;
            var progress = Math.min(elapsed / duration, 1);
            var eased = 1 - Math.pow(1 - progress, 3);
            var current = (target * eased).toFixed(1);
            if (progress === 1) {
                el.textContent = text;
            } else {
                el.textContent = current.replace(/\.0$/, "") + suffix;
            }
            if (progress < 1) {
                requestAnimationFrame(step);
            }
        }

        requestAnimationFrame(step);
    }

    Array.prototype.slice.call(document.querySelectorAll("[data-accordion]")).forEach(function (button, index) {
        var item = button.closest(".job-item");
        if (index === 0 && item) {
            item.classList.add("is-open");
            button.querySelector("em").textContent = "收起";
        }

        button.addEventListener("click", function () {
            if (!item) return;
            var open = item.classList.toggle("is-open");
            var label = button.querySelector("em");
            if (label) {
                label.textContent = open ? "收起" : "展开";
            }
        });
    });

    Array.prototype.slice.call(document.querySelectorAll("[data-cert-carousel]")).forEach(function (carousel) {
        var track = carousel.querySelector(".cert-track");
        var cards = Array.prototype.slice.call(carousel.querySelectorAll(".cert-card"));
        var prev = carousel.querySelector(".cert-arrow-left");
        var next = carousel.querySelector(".cert-arrow-right");
        var index = 0;

        function perPage() {
            if (window.innerWidth <= 720) return 1;
            if (window.innerWidth <= 1024) return 2;
            return 4;
        }

        function update() {
            if (!track || cards.length === 0) return;
            var max = Math.max(cards.length - perPage(), 0);
            index = Math.max(0, Math.min(index, max));
            var cardWidth = cards[0].getBoundingClientRect().width + 24;
            track.style.transform = "translateX(-" + (index * cardWidth) + "px)";
        }

        if (prev) {
            prev.addEventListener("click", function () {
                index -= perPage();
                update();
            });
        }

        if (next) {
            next.addEventListener("click", function () {
                index += perPage();
                update();
            });
        }

        window.addEventListener("resize", update);
        update();
    });

    Array.prototype.slice.call(document.querySelectorAll("[data-message-form]")).forEach(function (form) {
        var tip = form.querySelector("[data-message-tip]");
        var captchaImg = form.querySelector(".captcha-img");
        var validateKeyInput = form.querySelector("input[name='ValidateKey']");

        function refreshCaptcha() {
            if (!captchaImg || !validateKeyInput) return;
            var newKey = new Date().getTime().toString();
            validateKeyInput.value = newKey;
            captchaImg.src = "/Authorize/GetImg?key=" + newKey + "&v=" + newKey;
        }

        if (captchaImg) {
            captchaImg.addEventListener("click", refreshCaptcha);
        }

        form.addEventListener("submit", function (event) {
            event.preventDefault();
            var button = form.querySelector("button[type='submit']");
            var formData = new FormData(form);
            if (button) button.disabled = true;
            if (tip) tip.textContent = "正在提交...";

            fetch("/home/message", {
                method: "POST",
                body: formData
            }).then(function (response) {
                return response.json();
            }).then(function (res) {
                if (tip) tip.textContent = res.message || "提交成功";
                if (res.code === 200) {
                    form.reset();
                }
                refreshCaptcha();
            }).catch(function () {
                if (tip) tip.textContent = "提交失败，请稍后再试";
                refreshCaptcha();
            }).finally(function () {
                if (button) button.disabled = false;
            });
        });
    });
})();
