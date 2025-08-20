document.addEventListener("DOMContentLoaded", function () {
  // Enhanced password visibility toggle
  const passwordField = document.getElementById("Password");

  if (passwordField) {
    // Create and append password toggle button with better styling
    const toggleButton = document.createElement("button");
    toggleButton.type = "button";
    toggleButton.className = "password-toggle";
    toggleButton.innerHTML = '<i class="far fa-eye"></i>';
    toggleButton.setAttribute("aria-label", "Hiện/ẩn mật khẩu");

    // Enhanced styling
    Object.assign(toggleButton.style, {
      position: "absolute",
      right: "12px",
      top: "50%",
      transform: "translateY(-50%)",
      border: "none",
      background: "transparent",
      color: "#6c757d",
      cursor: "pointer",
      zIndex: "10",
      padding: "4px",
      borderRadius: "4px",
      transition: "all 0.2s ease",
    });

    // Append to parent element (with position relative)
    const parentElement = passwordField.parentElement;
    parentElement.style.position = "relative";
    parentElement.appendChild(toggleButton);

    // Enhanced hover effect
    toggleButton.addEventListener("mouseenter", function () {
      this.style.color = "#495057";
      this.style.backgroundColor = "rgba(0,0,0,0.05)";
    });

    toggleButton.addEventListener("mouseleave", function () {
      this.style.color = "#6c757d";
      this.style.backgroundColor = "transparent";
    });

    // Toggle password visibility with animation
    toggleButton.addEventListener("click", function () {
      const type =
        passwordField.getAttribute("type") === "password" ? "text" : "password";
      passwordField.setAttribute("type", type);

      // Toggle icon with smooth transition
      const icon = toggleButton.querySelector("i");
      icon.style.transform = "scale(0.8)";
      setTimeout(() => {
        icon.className =
          type === "password" ? "far fa-eye" : "far fa-eye-slash";
        icon.style.transform = "scale(1)";
      }, 100);
    });
  }

  // Enhanced form validation and UX improvements
  const loginForm = document.querySelector(".login-form");

  if (loginForm) {
    const submitButton = loginForm.querySelector('button[type="submit"]');

    // Add loading state functionality
    loginForm.addEventListener("submit", function (event) {
      const emailInput = document.getElementById("Email");
      const passwordInput = document.getElementById("Password");

      let isValid = true;

      // Enhanced email validation
      if (!validateEmail(emailInput.value)) {
        showError(emailInput, "Vui lòng nhập email hợp lệ");
        isValid = false;
      } else {
        clearError(emailInput);
      }

      // Enhanced password validation
      if (passwordInput.value.length < 6) {
        showError(passwordInput, "Mật khẩu phải có ít nhất 6 ký tự");
        isValid = false;
      } else {
        clearError(passwordInput);
      }

      if (!isValid) {
        event.preventDefault();
        // Shake animation for invalid form
        loginForm.style.animation = "shake 0.5s ease-in-out";
        setTimeout(() => {
          loginForm.style.animation = "";
        }, 500);
      } else if (submitButton) {
        // Add loading state
        submitButton.disabled = true;
        submitButton.innerHTML =
          '<i class="fas fa-spinner fa-spin"></i> Đang đăng nhập...';

        // Remove loading state after timeout (fallback)
        setTimeout(() => {
          submitButton.disabled = false;
          submitButton.innerHTML = "Đăng nhập";
        }, 5000);
      }
    });

    // Real-time validation
    const inputs = loginForm.querySelectorAll(
      'input[type="email"], input[type="password"]'
    );
    inputs.forEach((input) => {
      input.addEventListener("input", function () {
        clearError(this);
      });

      // Enhanced focus effects
      input.addEventListener("focus", function () {
        this.closest(".form-floating").style.transform = "scale(1.02)";
      });

      input.addEventListener("blur", function () {
        this.closest(".form-floating").style.transform = "scale(1)";
      });
    });
  }

  // Enhanced helper functions
  function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(String(email).toLowerCase());
  }

  function showError(input, message) {
    const formGroup = input.closest(".form-floating");
    let errorElement = formGroup.querySelector(".text-danger");

    if (!errorElement) {
      errorElement = document.createElement("span");
      errorElement.className = "text-danger";
      errorElement.style.fontSize = "0.875rem";
      errorElement.style.marginTop = "0.25rem";
      errorElement.style.display = "block";
      formGroup.appendChild(errorElement);
    }

    errorElement.textContent = message;
    input.classList.add("is-invalid");

    // Add shake animation to input
    input.style.animation = "inputShake 0.3s ease-in-out";
    setTimeout(() => {
      input.style.animation = "";
    }, 300);
  }

  function clearError(input) {
    const formGroup = input.closest(".form-floating");
    const errorElement = formGroup.querySelector(".text-danger");

    if (errorElement) {
      errorElement.textContent = "";
    }

    input.classList.remove("is-invalid");
  }

  // Auto-focus first input
  const firstInput = document.querySelector(
    ".form-floating input:first-of-type"
  );
  if (firstInput) {
    setTimeout(() => {
      firstInput.focus();
    }, 300);
  }

  // Add CSS animations
  const style = document.createElement("style");
  style.textContent = `
    @keyframes shake {
      0%, 100% { transform: translateX(0); }
      20%, 60% { transform: translateX(-5px); }
      40%, 80% { transform: translateX(5px); }
    }
    
    @keyframes inputShake {
      0%, 100% { transform: translateX(0); }
      25%, 75% { transform: translateX(-3px); }
      50% { transform: translateX(3px); }
    }
    
    .form-floating {
      transition: transform 0.2s ease;
    }
    
    .password-toggle i {
      transition: transform 0.1s ease;
    }
    
    .btn[type="submit"] {
      transition: all 0.3s ease;
    }
    
    .btn[type="submit"]:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }
  `;
  document.head.appendChild(style);
});
