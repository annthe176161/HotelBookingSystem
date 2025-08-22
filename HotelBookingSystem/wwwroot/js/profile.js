document.addEventListener("DOMContentLoaded", function () {
  // Profile edit functionality
  const editProfileBtn = document.getElementById("edit-profile-btn");
  const cancelEditBtn = document.getElementById("cancel-edit-btn");
  const profileForm = document.getElementById("profile-form");
  const actionButtons = document.querySelector(".profile-action-buttons");

  if (editProfileBtn) {
    editProfileBtn.addEventListener("click", function () {
      // Enable all form fields except email
      const formInputs = profileForm.querySelectorAll("input, select");
      formInputs.forEach((input) => {
        // Don't enable email field as it should be readonly
        if (input.type !== "email") {
          input.disabled = false;
        }
      });

      // Show action buttons
      actionButtons.style.display = "flex";

      // Hide edit button
      editProfileBtn.style.display = "none";
    });
  }

  if (cancelEditBtn) {
    cancelEditBtn.addEventListener("click", function () {
      // Reload the page to reset all form values
      window.location.reload();
    });
  }

  // Profile form submission
  if (profileForm) {
    profileForm.addEventListener("submit", function (event) {
      // Don't preventDefault - let the form submit normally

      if (!profileForm.checkValidity()) {
        event.preventDefault();
        event.stopPropagation();
        profileForm.classList.add("was-validated");
        return;
      }

      // Form is valid, let it submit normally
      profileForm.classList.add("was-validated");
    });
  }

  // Avatar upload functionality
  const avatarUpload = document.getElementById("avatar-upload");
  if (avatarUpload) {
    avatarUpload.addEventListener("change", function (event) {
      const file = event.target.files[0];
      if (!file) return;

      // Validate file type
      const allowedTypes = [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
      ];
      if (!allowedTypes.includes(file.type)) {
        showToast("Vui lòng chọn file ảnh (JPEG, PNG, WebP, GIF)", "error");
        return;
      }

      // Validate file size (5MB max)
      const maxSize = 5 * 1024 * 1024; // 5MB
      if (file.size > maxSize) {
        showToast("Kích thước file không được vượt quá 5MB", "error");
        return;
      }

      // Show loading state
      const avatarContainer = document.querySelector(
        ".profile-avatar-container"
      );
      const originalContent = avatarContainer.innerHTML;
      avatarContainer.innerHTML = `
                <div class="rounded-circle d-flex align-items-center justify-content-center" style="width: 120px; height: 120px; background: #f8f9fa;">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Đang tải...</span>
                    </div>
                </div>
            `;

      // Create FormData
      const formData = new FormData();
      formData.append("avatar", file);

      // Get antiforgery token
      const token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
      ).value;

      // Upload avatar
      fetch("/Account/UploadAvatar", {
        method: "POST",
        headers: {
          RequestVerificationToken: token,
        },
        body: formData,
      })
        .then((response) => response.json())
        .then((data) => {
          if (data.success) {
            // Update avatar image
            avatarContainer.innerHTML = `
                        <img src="${data.avatarUrl}" alt="Avatar" class="profile-avatar rounded-circle" style="width: 120px; height: 120px; object-fit: cover;" />
                        <div class="profile-avatar-edit position-absolute" style="bottom: 5px; right: 5px;">
                            <label for="avatar-upload" class="btn btn-sm btn-primary rounded-circle p-2" style="width: 40px; height: 40px; cursor: pointer;">
                                <i class="fas fa-camera"></i>
                            </label>
                            <input type="file" id="avatar-upload" class="d-none" accept="image/*" />
                        </div>
                    `;

            // Re-bind the event listener for the new file input
            const newAvatarUpload = document.getElementById("avatar-upload");
            if (newAvatarUpload) {
              newAvatarUpload.addEventListener("change", arguments.callee);
            }

            showToast(data.message, "success");
          } else {
            // Restore original content on error
            avatarContainer.innerHTML = originalContent;
            showToast(data.message, "error");
          }
        })
        .catch((error) => {
          console.error("Error:", error);
          // Restore original content on error
          avatarContainer.innerHTML = originalContent;
          showToast("Có lỗi xảy ra khi upload ảnh", "error");
        });

      // Clear the input
      event.target.value = "";
    });
  }

  // Password visibility toggle
  const passwordToggles = document.querySelectorAll(".password-toggle");

  passwordToggles.forEach((toggle) => {
    toggle.addEventListener("click", function () {
      const passwordField = this.parentElement.querySelector("input");
      const type =
        passwordField.getAttribute("type") === "password" ? "text" : "password";
      passwordField.setAttribute("type", type);

      // Toggle icon
      const icon = this.querySelector("i");
      icon.className = type === "password" ? "far fa-eye" : "far fa-eye-slash";
    });
  });

  // Password strength meter
  const newPasswordField = document.getElementById("newPassword");
  const strengthMeter = document.getElementById("password-strength-meter");
  const strengthText = document.getElementById("password-strength-text");

  if (newPasswordField) {
    newPasswordField.addEventListener("input", function () {
      const password = this.value;
      let strength = 0;

      // Calculate strength
      if (password.length > 0) strength += 1;
      if (password.length >= 8) strength += 1;
      if (/[A-Z]/.test(password)) strength += 1;
      if (/[a-z]/.test(password)) strength += 1;
      if (/[0-9]/.test(password)) strength += 1;
      if (/[^A-Za-z0-9]/.test(password)) strength += 1;

      // Update strength meter
      let strengthPercentage = (strength / 6) * 100;
      let strengthClass = "bg-danger";
      let strengthLabel = "Rất yếu";

      if (strength === 0) {
        strengthPercentage = 0;
        strengthLabel = "";
      } else if (strength <= 2) {
        strengthLabel = "Yếu";
        strengthClass = "bg-danger";
      } else if (strength <= 4) {
        strengthLabel = "Trung bình";
        strengthClass = "bg-warning";
      } else if (strength <= 5) {
        strengthLabel = "Mạnh";
        strengthClass = "bg-info";
      } else {
        strengthLabel = "Rất mạnh";
        strengthClass = "bg-success";
      }

      strengthMeter.style.width = strengthPercentage + "%";
      strengthMeter.className = `progress-bar ${strengthClass}`;
      strengthText.textContent = strengthLabel;
    });
  }

  // Password confirmation validation
  const confirmPasswordField = document.getElementById("confirmPassword");

  if (confirmPasswordField && newPasswordField) {
    confirmPasswordField.addEventListener("input", function () {
      if (this.value !== newPasswordField.value) {
        this.setCustomValidity("Mật khẩu xác nhận không khớp");
      } else {
        this.setCustomValidity("");
      }
    });
  }

  // Change password form submission
  const changePasswordForm = document.getElementById("change-password-form");

  if (changePasswordForm) {
    changePasswordForm.addEventListener("submit", function (event) {
      event.preventDefault();

      if (!changePasswordForm.checkValidity()) {
        event.stopPropagation();
        changePasswordForm.classList.add("was-validated");
        return;
      }

      // Collect form data
      const passwordData = {
        currentPassword: document.getElementById("currentPassword").value,
        newPassword: document.getElementById("newPassword").value,
        confirmPassword: document.getElementById("confirmPassword").value,
      };

      // Normally you would send this data to your server via AJAX
      // For now, let's just simulate a successful update

      // Reset form
      changePasswordForm.reset();

      // Show success message
      showToast("Mật khẩu đã được thay đổi thành công!", "success");
    });
  }

  // Handle booking filters
  const statusFilter = document.getElementById("booking-status-filter");
  const dateFromFilter = document.getElementById("booking-date-from");
  const dateToFilter = document.getElementById("booking-date-to");

  if (statusFilter && dateFromFilter && dateToFilter) {
    const filterFunction = function () {
      const status = statusFilter.value;
      const dateFrom = dateFromFilter.value
        ? new Date(dateFromFilter.value)
        : null;
      const dateTo = dateToFilter.value ? new Date(dateToFilter.value) : null;

      const bookingItems = document.querySelectorAll(".booking-item");

      bookingItems.forEach((item) => {
        let showItem = true;

        // Filter by status
        if (status !== "all" && !item.classList.contains(status)) {
          showItem = false;
        }

        // TODO: Add date range filtering if needed

        item.style.display = showItem ? "block" : "none";
      });
    };

    statusFilter.addEventListener("change", filterFunction);
    dateFromFilter.addEventListener("change", filterFunction);
    dateToFilter.addEventListener("change", filterFunction);
  }

  // Handle cancel booking button
  const cancelBookingButtons = document.querySelectorAll(".cancel-booking-btn");

  cancelBookingButtons.forEach((button) => {
    button.addEventListener("click", function () {
      const bookingId = this.getAttribute("data-id");

      // Show confirmation modal
      const confirmModal = document.getElementById("confirm-modal");
      const confirmMessage = document.getElementById("confirm-message");
      const confirmAction = document.getElementById("confirm-action");

      confirmMessage.textContent = "Bạn có chắc chắn muốn hủy đặt phòng này?";

      const bsModal = new bootstrap.Modal(confirmModal);
      bsModal.show();

      confirmAction.onclick = function () {
        // Normally you would send this to your server via AJAX
        // For now, let's just simulate a successful cancellation
        const bookingItem = document.querySelector(
          `.booking-item[data-id="${bookingId}"]`
        );
        if (bookingItem) {
          // Update status badge
          const statusBadge = bookingItem.querySelector(".status-badge");
          statusBadge.textContent = "Đã hủy";
          statusBadge.className = "status-badge cancelled";

          // Remove cancel button
          button.remove();
        }

        // Hide modal
        bsModal.hide();

        // Show success message
        showToast("Đặt phòng đã được hủy thành công!", "success");
      };
    });
  });

  // Handle add review button
  const addReviewButtons = document.querySelectorAll(".add-review-btn");
  const reviewModal = document.getElementById("add-review-modal");

  addReviewButtons.forEach((button) => {
    button.addEventListener("click", function () {
      const bookingId = this.getAttribute("data-id");
      const roomName = this.getAttribute("data-room");

      // Fill modal with booking info
      document.getElementById("booking-id").value = bookingId;
      document.getElementById("review-room-name").value = roomName;

      // Reset rating and comment
      document.querySelectorAll(".rating-input i").forEach((star) => {
        star.className = "far fa-star";
      });
      document.getElementById("rating-value").value = "";
      document.getElementById("review-comment").value = "";

      // Show modal
      const bsModal = new bootstrap.Modal(reviewModal);
      bsModal.show();
    });
  });

  // Handle star rating
  const ratingStars = document.querySelectorAll(".rating-input i");

  ratingStars.forEach((star) => {
    star.addEventListener("mouseover", function () {
      const rating = this.getAttribute("data-rating");
      highlightStars(rating);
    });

    star.addEventListener("mouseout", function () {
      const currentRating = document.getElementById("rating-value").value;
      highlightStars(currentRating);
    });

    star.addEventListener("click", function () {
      const rating = this.getAttribute("data-rating");
      document.getElementById("rating-value").value = rating;
      highlightStars(rating);
    });
  });

  function highlightStars(rating) {
    ratingStars.forEach((star) => {
      const starRating = star.getAttribute("data-rating");
      star.className = starRating <= rating ? "fas fa-star" : "far fa-star";
    });
  }

  // Handle submit review
  const submitReviewButton = document.getElementById("submit-review");

  if (submitReviewButton) {
    submitReviewButton.addEventListener("click", function () {
      const bookingId = document.getElementById("booking-id").value;
      const rating = document.getElementById("rating-value").value;
      const comment = document.getElementById("review-comment").value;

      if (!rating) {
        alert("Vui lòng chọn số sao đánh giá!");
        return;
      }

      if (!comment) {
        alert("Vui lòng nhập nhận xét của bạn!");
        return;
      }

      // Normally you would send this to your server via AJAX
      // For now, let's just simulate a successful review submission

      // Hide modal
      bootstrap.Modal.getInstance(reviewModal).hide();

      // Mark booking as reviewed
      const reviewButton = document.querySelector(
        `.add-review-btn[data-id="${bookingId}"]`
      );
      if (reviewButton) {
        reviewButton.remove();
      }

      // Show success message
      showToast("Cảm ơn bạn đã gửi đánh giá!", "success");
    });
  }

  // Handle delete review button
  const deleteReviewButtons = document.querySelectorAll(".delete-review-btn");

  deleteReviewButtons.forEach((button) => {
    button.addEventListener("click", function () {
      const reviewId = this.getAttribute("data-id");

      // Show confirmation modal
      const confirmModal = document.getElementById("confirm-modal");
      const confirmMessage = document.getElementById("confirm-message");
      const confirmAction = document.getElementById("confirm-action");

      confirmMessage.textContent = "Bạn có chắc chắn muốn xóa đánh giá này?";

      const bsModal = new bootstrap.Modal(confirmModal);
      bsModal.show();

      confirmAction.onclick = function () {
        // Normally you would send this to your server via AJAX
        // For now, let's just simulate a successful deletion
        const reviewItem = button.closest(".review-item");
        if (reviewItem) {
          reviewItem.remove();
        }

        // Hide modal
        bsModal.hide();

        // Show success message
        showToast("Đánh giá đã được xóa thành công!", "success");
      };
    });
  });

  // Handle remove favorite button
  const removeFavoriteButtons = document.querySelectorAll(
    ".remove-favorite-btn"
  );

  removeFavoriteButtons.forEach((button) => {
    button.addEventListener("click", function () {
      const roomId = this.getAttribute("data-id");

      // Normally you would send this to your server via AJAX
      // For now, let's just simulate a successful removal
      const favoriteItem = button.closest(".col-md-6");
      if (favoriteItem) {
        favoriteItem.style.opacity = "0";
        setTimeout(() => {
          favoriteItem.remove();

          // Check if there are any favorites left
          const favoriteItems = document.querySelectorAll(
            ".favorite-room-item"
          );
          if (favoriteItems.length === 0) {
            const favoritesList = document.querySelector(".favorites-list");
            if (favoritesList) {
              favoritesList.innerHTML = `
                                <div class="empty-state">
                                    <i class="fas fa-heart empty-icon"></i>
                                    <p>Bạn chưa có phòng yêu thích nào</p>
                                    <a href="/Home/Rooms" class="btn btn-primary mt-3">Khám phá phòng ngay</a>
                                </div>
                            `;
            }
          }
        }, 300);
      }

      // Show success message
      showToast("Phòng đã được xóa khỏi danh sách yêu thích!", "success");
    });
  });

  // Handle account settings switches
  const twoFactorSwitch = document.getElementById("two-factor-switch");
  const loginNotifySwitch = document.getElementById("login-notify-switch");

  if (twoFactorSwitch) {
    twoFactorSwitch.addEventListener("change", function () {
      // Normally you would send this to your server via AJAX
      // For now, let's just show a message
      const status = this.checked ? "bật" : "tắt";
      showToast(`Xác thực hai yếu tố đã được ${status}!`, "success");
    });
  }

  if (loginNotifySwitch) {
    loginNotifySwitch.addEventListener("change", function () {
      // Normally you would send this to your server via AJAX
      // For now, let's just show a message
      const status = this.checked ? "bật" : "tắt";
      showToast(`Thông báo đăng nhập đã được ${status}!`, "success");
    });
  }

  // Handle disable account button
  const disableAccountButton = document.getElementById("disable-account-btn");

  if (disableAccountButton) {
    disableAccountButton.addEventListener("click", function () {
      // Show confirmation modal
      const confirmModal = document.getElementById("confirm-modal");
      const confirmMessage = document.getElementById("confirm-message");
      const confirmAction = document.getElementById("confirm-action");

      confirmMessage.textContent =
        "Bạn có chắc chắn muốn vô hiệu hóa tài khoản? Bạn sẽ không thể đăng nhập cho đến khi kích hoạt lại tài khoản.";

      const bsModal = new bootstrap.Modal(confirmModal);
      bsModal.show();

      confirmAction.onclick = function () {
        // Normally you would send this to your server via AJAX
        // For now, let's just show a message

        // Hide modal
        bsModal.hide();

        // Show success message
        showToast(
          "Tài khoản đã được vô hiệu hóa. Bạn sẽ bị đăng xuất trong giây lát.",
          "success"
        );

        // Simulate logout
        setTimeout(() => {
          window.location.href = "/";
        }, 3000);
      };
    });
  }

  // Handle delete account button
  const deleteAccountButton = document.getElementById("delete-account-btn");

  if (deleteAccountButton) {
    deleteAccountButton.addEventListener("click", function () {
      // Show confirmation modal
      const confirmModal = document.getElementById("confirm-modal");
      const confirmMessage = document.getElementById("confirm-message");
      const confirmAction = document.getElementById("confirm-action");

      confirmMessage.textContent =
        "Bạn có chắc chắn muốn xóa vĩnh viễn tài khoản? Hành động này không thể hoàn tác và tất cả dữ liệu của bạn sẽ bị xóa.";

      const bsModal = new bootstrap.Modal(confirmModal);
      bsModal.show();

      confirmAction.onclick = function () {
        // Normally you would send this to your server via AJAX
        // For now, let's just show a message

        // Hide modal
        bsModal.hide();

        // Show success message
        showToast(
          "Tài khoản đã được xóa vĩnh viễn. Bạn sẽ bị đăng xuất trong giây lát.",
          "success"
        );

        // Simulate logout
        setTimeout(() => {
          window.location.href = "/";
        }, 3000);
      };
    });
  }

  // Toast notification helper
  function showToast(message, type = "info") {
    // Create toast container if it doesn't exist
    let toastContainer = document.querySelector(".toast-container");
    if (!toastContainer) {
      toastContainer = document.createElement("div");
      toastContainer.className =
        "toast-container position-fixed bottom-0 end-0 p-3";
      document.body.appendChild(toastContainer);
    }

    // Create toast element
    const toastId = "toast-" + Date.now();
    const toast = document.createElement("div");
    toast.className = `toast align-items-center text-white bg-${
      type === "success"
        ? "success"
        : type === "warning"
        ? "warning"
        : type === "error"
        ? "danger"
        : "primary"
    } border-0`;
    toast.id = toastId;
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");

    toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

    // Add toast to container
    toastContainer.appendChild(toast);

    // Initialize and show toast
    const bsToast = new bootstrap.Toast(toast, {
      autohide: true,
      delay: 3000,
    });
    bsToast.show();

    // Remove toast element after it's hidden
    toast.addEventListener("hidden.bs.toast", function () {
      toast.remove();
    });
  }
});
