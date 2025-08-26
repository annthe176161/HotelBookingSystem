// Forgot Password Page Enhancements
document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('.login-form');
    const submitBtn = form.querySelector('button[type="submit"]');
    const emailInput = form.querySelector('input[name="Email"]');

    // Add loading state to submit button
    form.addEventListener('submit', function (e) {
        if (form.checkValidity()) {
            // Prevent double submission
            if (submitBtn.disabled) {
                e.preventDefault();
                return false;
            }

            submitBtn.disabled = true;

            // Store original content
            const originalContent = submitBtn.innerHTML;

            // Replace content with single spinner
            submitBtn.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Đang gửi...
            `;

            // Re-enable button after 30 seconds (in case of network issues)
            setTimeout(() => {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalContent;
            }, 30000);
        }
    });

    // Email validation feedback
    emailInput.addEventListener('blur', function () {
        const email = this.value.trim();
        if (email && !isValidEmail(email)) {
            showValidationMessage(this, 'Vui lòng nhập địa chỉ email hợp lệ');
        } else {
            clearValidationMessage(this);
        }
    });

    // Real-time email validation
    emailInput.addEventListener('input', function () {
        clearValidationMessage(this);
    });

    // Helper functions
    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    function showValidationMessage(input, message) {
        clearValidationMessage(input);
        const errorSpan = input.parentNode.querySelector('.text-danger');
        if (errorSpan) {
            errorSpan.textContent = message;
            errorSpan.style.display = 'block';
        }
        input.classList.add('is-invalid');
    }

    function clearValidationMessage(input) {
        const errorSpan = input.parentNode.querySelector('.text-danger');
        if (errorSpan) {
            errorSpan.textContent = '';
            errorSpan.style.display = 'none';
        }
        input.classList.remove('is-invalid');
    }

    // Add smooth animations
    const formWrapper = document.querySelector('.login-form-wrapper');
    if (formWrapper) {
        formWrapper.style.opacity = '0';
        formWrapper.style.transform = 'translateY(20px)';

        setTimeout(() => {
            formWrapper.style.transition = 'all 0.5s ease';
            formWrapper.style.opacity = '1';
            formWrapper.style.transform = 'translateY(0)';
        }, 100);
    }

    // Add focus enhancement
    const inputs = form.querySelectorAll('input[type="email"], input[type="password"]');
    inputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.parentNode.classList.add('focused');
        });

        input.addEventListener('blur', function () {
            if (!this.value) {
                this.parentNode.classList.remove('focused');
            }
        });
    });
});

// Password toggle functionality
function togglePassword(fieldName) {
    const passwordField = document.getElementById(fieldName);
    const eyeIcon = document.getElementById(fieldName + '-eye-icon');

    if (passwordField && eyeIcon) {
        if (passwordField.type === 'password') {
            passwordField.type = 'text';
            eyeIcon.classList.remove('fa-eye');
            eyeIcon.classList.add('fa-eye-slash');
        } else {
            passwordField.type = 'password';
            eyeIcon.classList.remove('fa-eye-slash');
            eyeIcon.classList.add('fa-eye');
        }
    }
}

// Add some helpful tooltips
document.addEventListener('DOMContentLoaded', function () {
    // Initialize Bootstrap tooltips if available
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        tooltips.forEach(tooltip => {
            new bootstrap.Tooltip(tooltip);
        });
    }

    // Override HTML5 validation messages for Vietnamese
    const inputs = document.querySelectorAll('input[required]');
    inputs.forEach(input => {
        input.addEventListener('invalid', function (e) {
            if (this.validity.valueMissing) {
                if (this.type === 'email') {
                    this.setCustomValidity('Vui lòng nhập địa chỉ email.');
                } else if (this.type === 'password') {
                    this.setCustomValidity('Vui lòng nhập mật khẩu.');
                } else {
                    this.setCustomValidity('Vui lòng nhập thông tin này.');
                }
            } else if (this.validity.typeMismatch && this.type === 'email') {
                this.setCustomValidity('Vui lòng nhập địa chỉ email hợp lệ.');
            }
        });

        input.addEventListener('input', function () {
            this.setCustomValidity('');
        });
    });
});
