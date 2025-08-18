document.addEventListener('DOMContentLoaded', function () {
    // Hero Slideshow
    initHeroSlideshow();
    
    // Smooth scrolling for anchor links
    initSmoothScrolling();
    
    // Newsletter form
    initNewsletterForm();
    
    // Animations on scroll
    initScrollAnimations();
    
    // Counter animation for stats
    initStatsCounter();
});

// Hero Slideshow Function
function initHeroSlideshow() {
    const slides = document.querySelectorAll('.hero-slide');
    const navBtns = document.querySelectorAll('.hero-nav-btn');
    let currentSlide = 0;
    
    if (slides.length === 0) return;
    
    // Auto slideshow
    const slideInterval = setInterval(() => {
        nextSlide();
    }, 5000);
    
    // Navigation buttons
    navBtns.forEach((btn, index) => {
        btn.addEventListener('click', () => {
            goToSlide(index);
            // Reset interval when user manually changes slide
            clearInterval(slideInterval);
            setTimeout(() => {
                setInterval(() => {
                    nextSlide();
                }, 5000);
            }, 5000);
        });
    });
    
    function nextSlide() {
        currentSlide = (currentSlide + 1) % slides.length;
        goToSlide(currentSlide);
    }
    
    function goToSlide(index) {
        // Remove active class from all slides and nav buttons
        slides.forEach(slide => slide.classList.remove('active'));
        navBtns.forEach(btn => btn.classList.remove('active'));
        
        // Add active class to current slide and nav button
        if (slides[index]) {
            slides[index].classList.add('active');
        }
        if (navBtns[index]) {
            navBtns[index].classList.add('active');
        }
        
        currentSlide = index;
    }
}

// Smooth scrolling for anchor links
function initSmoothScrolling() {
    const anchors = document.querySelectorAll('a[href^="#"]');
    
    anchors.forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            
            const targetId = this.getAttribute('href');
            const targetElement = document.querySelector(targetId);
            
            if (targetElement) {
                const offsetTop = targetElement.offsetTop - 80; // Account for fixed header
                
                window.scrollTo({
                    top: offsetTop,
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Newsletter form
function initNewsletterForm() {
    const newsletterForm = document.getElementById('newsletterForm');
    
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', function (e) {
            e.preventDefault();
            
            const emailInput = this.querySelector('input[type="email"]');
            const submitBtn = this.querySelector('button[type="submit"]');
            
            if (emailInput && emailInput.value) {
                // Simulate form submission
                const originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
                submitBtn.disabled = true;
                
                setTimeout(() => {
                    submitBtn.innerHTML = '<i class="fas fa-check"></i> Đã đăng ký!';
                    submitBtn.classList.add('btn-success');
                    emailInput.value = '';
                    
                    setTimeout(() => {
                        submitBtn.innerHTML = originalText;
                        submitBtn.classList.remove('btn-success');
                        submitBtn.disabled = false;
                    }, 3000);
                }, 2000);
            }
        });
    }
}

// Scroll animations
function initScrollAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
            }
        });
    }, observerOptions);
    
    // Observe elements for animation
    const animateElements = document.querySelectorAll('.room-card, .service-item, .testimonial-item, .gallery-item, .offer-card');
    animateElements.forEach(el => {
        observer.observe(el);
    });
    
    // Initial setup for animation
    animateElements.forEach(element => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(20px)';
        element.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
    });
}

// Counter animation for stats
function initStatsCounter() {
    const statsSection = document.getElementById('stats');
    if (!statsSection) return;
    
    const statsObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounters();
                statsObserver.disconnect();
            }
        });
    }, { threshold: 0.5 });
    
    statsObserver.observe(statsSection);
}

function animateCounters() {
    const counters = document.querySelectorAll('.stat-number');
    
    counters.forEach(counter => {
        const target = parseInt(counter.textContent.replace(/[^\d]/g, ''));
        const duration = 2000;
        const step = target / (duration / 16);
        let current = 0;
        
        const timer = setInterval(() => {
            current += step;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            
            const suffix = counter.textContent.replace(/[\d,]/g, '');
            counter.textContent = Math.floor(current) + suffix;
        }, 16);
    });
}

// Add CSS class for animate-in effect
document.head.insertAdjacentHTML('beforeend', `
<style>
.animate-in {
    opacity: 1 !important;
    transform: translateY(0) !important;
}
</style>
`);