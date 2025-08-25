// Change Password Page Enhancements
document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('.login-form');
    const submitBtn = form.querySelector('button[type="submit"]');
    const currentPasswordInput = form.querySelector('input[name="CurrentPassword"]');
    const newPasswordInput = form.querySelector('input[name="NewPassword"]');
    const confirmPasswordInput = form.querySelector('input[name="ConfirmPassword"]');
    const strengthMeter = document.querySelector('.password-strength');
    const strengthFill = document.querySelector('.strength-meter-fill');
    const strengthText = document.querySelector('.strength-level');
    const passwordRequirements = document.querySelector('.password-requirements');

    // Add loading state to submit button
    form.addEventListener('submit', function (e) {
        if (form.checkValidity()) {
            submitBtn.classList.add('btn-loading');
            submitBtn.disabled = true;

            // Store original text
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Đang cập nhật...';

            // Re-enable button after 30 seconds (in case of network issues)
            setTimeout(() => {
                submitBtn.classList.remove('btn-loading');
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }, 30000);
        }
    });

    // Password strength checker for new password
    if (newPasswordInput && strengthMeter) {
        newPasswordInput.addEventListener('input', function () {
            const password = this.value;
            if (password.length > 0) {
                strengthMeter.style.display = 'block';
                if (passwordRequirements) {
                    passwordRequirements.style.display = 'block';
                }
                checkPasswordStrength(password);
                updatePasswordRequirements(password);
            } else {
                strengthMeter.style.display = 'none';
                if (passwordRequirements) {
                    passwordRequirements.style.display = 'none';
                }
            }
        });

        newPasswordInput.addEventListener('focus', function () {
            if (passwordRequirements) {
                passwordRequirements.style.display = 'block';
            }
        });
    }

    // Password confirmation checker
    if (confirmPasswordInput && newPasswordInput) {
        confirmPasswordInput.addEventListener('input', function () {
            checkPasswordMatch();
        });

        newPasswordInput.addEventListener('input', function () {
            if (confirmPasswordInput.value.length > 0) {
                checkPasswordMatch();
            }
        });
    }

    // Password strength checker function
    function checkPasswordStrength(password) {
        let score = 0;
        let feedback = 'Yếu';
        let className = 'strength-weak';

        // Length check
        if (password.length >= 8) score += 1;
        if (password.length >= 12) score += 1;

        // Character variety checks
        if (/[a-z]/.test(password)) score += 1; // lowercase
        if (/[A-Z]/.test(password)) score += 1; // uppercase
        if (/[0-9]/.test(password)) score += 1; // numbers
        if (/[^A-Za-z0-9]/.test(password)) score += 1; // special characters

        // Common patterns check (reduce score for common patterns)
        if (/(.)\1{2,}/.test(password)) score -= 1; // repeated characters
        if (/123|abc|qwe/i.test(password)) score -= 1; // common sequences

        // Determine strength level
        if (score <= 2) {
            feedback = 'Yếu';
            className = 'strength-weak';
        } else if (score <= 4) {
            feedback = 'Trung bình';
            className = 'strength-fair';
        } else if (score <= 5) {
            feedback = 'Tốt';
            className = 'strength-good';
        } else {
            feedback = 'Mạnh';
            className = 'strength-strong';
        }

        // Update UI
        strengthMeter.className = 'password-strength ' + className;
        strengthText.textContent = feedback;
    }

    // Update password requirements indicators
    function updatePasswordRequirements(password) {
        if (!passwordRequirements) return;

        const requirements = {
            'req-length': password.length >= 8,
            'req-uppercase': /[A-Z]/.test(password),
            'req-lowercase': /[a-z]/.test(password),
            'req-number': /[0-9]/.test(password),
            'req-special': /[^A-Za-z0-9]/.test(password)
        };

        Object.keys(requirements).forEach(reqId => {
            const element = document.getElementById(reqId);
            if (element) {
                if (requirements[reqId]) {
                    element.classList.add('valid');
                } else {
                    element.classList.remove('valid');
                }
            }
        });
    }

    // Password match checker function
    function checkPasswordMatch() {
        const newPassword = newPasswordInput.value;
        const confirmPassword = confirmPasswordInput.value;

        // Remove existing match indicator
        const existingIndicator = confirmPasswordInput.parentNode.querySelector('.password-match');
        if (existingIndicator) {
            existingIndicator.remove();
        }

        if (confirmPassword.length > 0) {
            const matchIndicator = document.createElement('div');
            matchIndicator.className = 'password-match show';

            if (newPassword === confirmPassword) {
                matchIndicator.className += ' match';
                matchIndicator.innerHTML = '<i class="fas fa-check me-1"></i>Mật khẩu khớp';
                confirmPasswordInput.classList.remove('is-invalid');
                confirmPasswordInput.classList.add('is-valid');
            } else {
                matchIndicator.className += ' no-match';
                matchIndicator.innerHTML = '<i class="fas fa-times me-1"></i>Mật khẩu không khớp';
                confirmPasswordInput.classList.remove('is-valid');
                confirmPasswordInput.classList.add('is-invalid');
            }

            confirmPasswordInput.parentNode.appendChild(matchIndicator);
        } else {
            confirmPasswordInput.classList.remove('is-valid', 'is-invalid');
        }
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

    // Enhanced focus states
    const inputs = form.querySelectorAll('input[type="password"]');
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

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s ease';
            alert.style.opacity = '0';
            setTimeout(() => {
                alert.style.display = 'none';
            }, 500);
        }, 5000);
    });
});

// Password toggle function (global scope for onclick)
function togglePassword(toggleIcon) {
    const input = toggleIcon.closest('.form-floating').querySelector('input');
    const type = input.getAttribute('type') === 'password' ? 'text' : 'password';

    input.setAttribute('type', type);

    // Toggle icon
    if (type === 'password') {
        toggleIcon.classList.remove('fa-eye-slash');
        toggleIcon.classList.add('fa-eye');
    } else {
        toggleIcon.classList.remove('fa-eye');
        toggleIcon.classList.add('fa-eye-slash');
    }
}

// Password generation suggestion (optional feature)
function generateStrongPassword() {
    const length = 12;
    const charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
    let password = "";

    // Ensure at least one character from each required category
    password += "abcdefghijklmnopqrstuvwxyz"[Math.floor(Math.random() * 26)]; // lowercase
    password += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[Math.floor(Math.random() * 26)]; // uppercase
    password += "0123456789"[Math.floor(Math.random() * 10)]; // number
    password += "!@#$%^&*"[Math.floor(Math.random() * 8)]; // special

    // Fill the rest randomly
    for (let i = password.length; i < length; i++) {
        password += charset[Math.floor(Math.random() * charset.length)];
    }

    // Shuffle the password
    return password.split('').sort(() => Math.random() - 0.5).join('');
}
