// Scroll Reveal Animation System
document.addEventListener("DOMContentLoaded", function () {
    const revealElements = document.querySelectorAll(".reveal, .reveal-left, .reveal-right, .reveal-fade");

    // Initialize Intersection Observer with subtle viewport offset
    const revealObserver = new IntersectionObserver(
        (entries, observer) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("active");
                    // Unobserve to keep active state after animating once
                    observer.unobserve(entry.target);
                }
            });
        },
        {
            root: null, // Viewport
            threshold: 0.1, // Trigger when 10% of element is visible
            rootMargin: "0px 0px -50px 0px" // Trigger slightly before entering fully
        }
    );

    // Observe each target element
    revealElements.forEach((element) => {
        revealObserver.observe(element);
    });
});
