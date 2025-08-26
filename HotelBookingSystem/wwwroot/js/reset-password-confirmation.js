// Reset Password Confirmation Page JavaScript
document.addEventListener('DOMContentLoaded', function () {
    // Add fade-in animation to content
    const content = document.querySelector('.login-form-wrapper');
    if (content) {
        content.style.opacity = '0';
        content.style.transform = 'translateY(20px)';

        setTimeout(() => {
            content.style.transition = 'all 0.6s ease-out';
            content.style.opacity = '1';
            content.style.transform = 'translateY(0)';
        }, 100);
    }

    // Add success confetti effect
    setTimeout(() => {
        createConfetti();
    }, 500);

    // Auto focus on login button after animation
    setTimeout(() => {
        const loginBtn = document.querySelector('.btn-primary');
        if (loginBtn) {
            loginBtn.focus();
        }
    }, 1000);
});

function createConfetti() {
    const colors = ['#ff6b6b', '#4ecdc4', '#45b7d1', '#f9ca24', '#6c5ce7'];
    const confettiCount = 50;

    for (let i = 0; i < confettiCount; i++) {
        createConfettiPiece(colors[Math.floor(Math.random() * colors.length)]);
    }
}

function createConfettiPiece(color) {
    const confetti = document.createElement('div');
    confetti.style.position = 'fixed';
    confetti.style.left = Math.random() * window.innerWidth + 'px';
    confetti.style.top = '-10px';
    confetti.style.width = '10px';
    confetti.style.height = '10px';
    confetti.style.backgroundColor = color;
    confetti.style.opacity = '0.8';
    confetti.style.borderRadius = '50%';
    confetti.style.pointerEvents = 'none';
    confetti.style.zIndex = '9999';
    confetti.style.transition = 'all 3s ease-out';

    document.body.appendChild(confetti);

    // Animate confetti falling
    setTimeout(() => {
        confetti.style.top = window.innerHeight + 'px';
        confetti.style.transform = `rotate(${Math.random() * 360}deg)`;
        confetti.style.opacity = '0';
    }, 50);

    // Remove confetti after animation
    setTimeout(() => {
        if (confetti.parentNode) {
            confetti.parentNode.removeChild(confetti);
        }
    }, 3000);
}
