(() => {
    const threshold = 100;
    let pending = false;

    const update = () => {
        pending = false;

        const root = document.documentElement;
        const canScroll = root.scrollHeight - window.innerHeight > threshold;

        root.classList.toggle("nm-scrolled", canScroll && window.scrollY > threshold);
    };

    const schedule = () => {
        if (pending)
            return;

        pending = true;
        requestAnimationFrame(update);
    };

    window.addEventListener("scroll", schedule, { passive: true });
    window.addEventListener("resize", schedule);
    new ResizeObserver(schedule).observe(document.body);

    schedule();
})();
