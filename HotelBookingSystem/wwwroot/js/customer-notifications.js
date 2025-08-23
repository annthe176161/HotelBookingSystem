// Customer Notifications Management - Updated to work with Global Notifications
document.addEventListener("DOMContentLoaded", function () {
  const notificationsList = document.getElementById("notificationsList");
  const notificationCounter = document.getElementById("notificationCounter");
  const emptyState = document.getElementById("emptyState");
  const connectionStatus = document.getElementById("connectionStatus");
  const statusText = document.getElementById("statusText");

  // Use global notification manager if available
  let notifications = [];
  let currentPage = 1;
  const itemsPerPage = 10;

  // Initialize page
  initializePage();
  setupEventListeners();

  function initializePage() {
    loadNotifications();
    renderNotifications();
    updateConnectionStatus("connected"); // Global manager handles connection
  }

  function loadNotifications() {
    // Get notifications from global manager or localStorage
    if (window.globalNotificationManager) {
      notifications = window.globalNotificationManager.getStoredNotifications();
    } else {
      notifications = JSON.parse(
        localStorage.getItem("customerNotifications") || "[]"
      );
    }
  }

  // Global function to refresh notifications from external sources
  window.refreshNotifications = function () {
    loadNotifications();
    renderNotifications();
  };

  function setupEventListeners() {
    // Mark all as read
    document
      .getElementById("markAllAsRead")
      ?.addEventListener("click", markAllAsRead);

    // Clear all notifications
    document
      .getElementById("clearNotifications")
      ?.addEventListener("click", clearAllNotifications);

    // Filters
    document
      .getElementById("notificationTypeFilter")
      ?.addEventListener("change", filterNotifications);
    document
      .getElementById("dateFilter")
      ?.addEventListener("change", filterNotifications);
  }

  function addNewNotification(notification) {
    const newNotification = {
      id: Date.now() + Math.random(),
      type: notification.type || "info",
      message: notification.message,
      timestamp: notification.timestamp || new Date().toISOString(),
      data: notification.data || {},
      isRead: false,
      isNew: true,
    };

    notifications.unshift(newNotification);
    saveNotifications();
    renderNotifications();

    // Show toast
    showToast(notification.message);

    // Play notification sound (optional)
    playNotificationSound();
  }

  function renderNotifications() {
    const filteredNotifications = getFilteredNotifications();
    const paginatedNotifications = getPaginatedNotifications(
      filteredNotifications
    );

    if (filteredNotifications.length === 0) {
      showEmptyState();
      return;
    }

    hideEmptyState();

    notificationsList.innerHTML = "";

    paginatedNotifications.forEach((notification) => {
      const element = createNotificationElement(notification);
      notificationsList.appendChild(element);
    });

    updateCounter(filteredNotifications.length);
    renderPagination(filteredNotifications.length);
  }

  function createNotificationElement(notification) {
    const div = document.createElement("div");
    div.className = `notification-item d-flex ${
      !notification.isRead ? "unread" : ""
    } ${notification.isNew ? "new" : ""}`;
    div.dataset.id = notification.id;

    const iconClass = getNotificationIcon(notification.type);
    const timeFormatted = formatTime(notification.timestamp);
    const typeLabel = getTypeLabel(notification.type);

    div.innerHTML = `
            <div class="notification-icon ${notification.type}">
                <i class="${iconClass}"></i>
            </div>
            <div class="notification-content">
                <div class="notification-header">
                    <span class="notification-type ${notification.type}">${typeLabel}</span>
                    <span class="notification-time">${timeFormatted}</span>
                </div>
                <div class="notification-message">${notification.message}</div>
                <div class="notification-actions">
                    <button class="btn btn-sm btn-outline-primary me-2" onclick="markAsRead('${notification.id}')">
                        <i class="fas fa-check"></i> Đánh dấu đã đọc
                    </button>
                    <button class="btn btn-sm btn-outline-info me-2" onclick="viewDetails('${notification.id}')">
                        <i class="fas fa-eye"></i> Chi tiết
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteNotification('${notification.id}')">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </div>
            </div>
        `;

    // Remove new class after animation
    setTimeout(() => {
      div.classList.remove("new");
      notification.isNew = false;
    }, 500);

    return div;
  }

  function getFilteredNotifications() {
    const typeFilter =
      document.getElementById("notificationTypeFilter")?.value || "";
    const dateFilter = document.getElementById("dateFilter")?.value || "";

    return notifications.filter((notification) => {
      // Type filter
      if (typeFilter && notification.type !== typeFilter) {
        return false;
      }

      // Date filter
      if (dateFilter) {
        const notificationDate = new Date(notification.timestamp);
        const now = new Date();

        switch (dateFilter) {
          case "today":
            if (notificationDate.toDateString() !== now.toDateString()) {
              return false;
            }
            break;
          case "week":
            const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
            if (notificationDate < weekAgo) {
              return false;
            }
            break;
          case "month":
            const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
            if (notificationDate < monthAgo) {
              return false;
            }
            break;
        }
      }

      return true;
    });
  }

  function getPaginatedNotifications(filteredNotifications) {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    return filteredNotifications.slice(startIndex, endIndex);
  }

  function markAsRead(notificationId) {
    const notification = notifications.find((n) => n.id == notificationId);
    if (notification) {
      notification.isRead = true;
      saveNotifications();
      renderNotifications();

      // Update global badge
      if (window.globalNotificationManager) {
        window.globalNotificationManager.updateNotificationBadge();
      }
    }
  }

  function markAllAsRead() {
    notifications.forEach((n) => (n.isRead = true));
    saveNotifications();
    renderNotifications();
    showToast("Đã đánh dấu tất cả thông báo là đã đọc");
  }

  function deleteNotification(notificationId) {
    if (confirm("Bạn có chắc muốn xóa thông báo này?")) {
      notifications = notifications.filter((n) => n.id != notificationId);
      saveNotifications();
      renderNotifications();
      showToast("Đã xóa thông báo");
    }
  }

  function clearAllNotifications() {
    if (confirm("Bạn có chắc muốn xóa tất cả thông báo?")) {
      notifications = [];
      saveNotifications();
      renderNotifications();
      showToast("Đã xóa tất cả thông báo");
    }
  }

  function viewDetails(notificationId) {
    const notification = notifications.find((n) => n.id == notificationId);
    if (!notification) return;

    const modal = document.getElementById("notificationDetailModal");
    const modalTitle = document.getElementById("modalTitle");
    const modalBody = document.getElementById("modalBody");
    const actionButton = document.getElementById("actionButton");

    modalTitle.textContent = getTypeLabel(notification.type);

    modalBody.innerHTML = `
            <div class="mb-3">
                <strong>Thời gian:</strong> ${formatFullTime(
                  notification.timestamp
                )}
            </div>
            <div class="mb-3">
                <strong>Nội dung:</strong><br>
                ${notification.message}
            </div>
            ${
              notification.data && notification.data.bookingId
                ? `<div class="mb-3">
                    <strong>Mã đặt phòng:</strong> #${notification.data.bookingId}
                </div>`
                : ""
            }
            ${
              notification.data && notification.data.status
                ? `<div class="mb-3">
                    <strong>Trạng thái:</strong> ${notification.data.status}
                </div>`
                : ""
            }
        `;

    // Show action button if it's a booking-related notification
    if (notification.data && notification.data.bookingId) {
      actionButton.style.display = "inline-block";
      actionButton.onclick = () => {
        window.location.href = `/Bookings/Index`;
      };
    } else {
      actionButton.style.display = "none";
    }

    const bootstrapModal = new bootstrap.Modal(modal);
    bootstrapModal.show();

    // Mark as read when viewed
    markAsRead(notificationId);
  }

  function filterNotifications() {
    currentPage = 1;
    renderNotifications();
  }

  function renderPagination(totalItems) {
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const paginationContainer = document.getElementById("paginationContainer");
    const paginationList = document.getElementById("paginationList");

    if (totalPages <= 1) {
      paginationContainer.style.display = "none";
      return;
    }

    paginationContainer.style.display = "block";
    paginationList.innerHTML = "";

    // Previous button
    const prevLi = document.createElement("li");
    prevLi.className = `page-item ${currentPage === 1 ? "disabled" : ""}`;
    prevLi.innerHTML = `<a class="page-link" href="#" data-page="${
      currentPage - 1
    }">‹</a>`;
    paginationList.appendChild(prevLi);

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
      const li = document.createElement("li");
      li.className = `page-item ${i === currentPage ? "active" : ""}`;
      li.innerHTML = `<a class="page-link" href="#" data-page="${i}">${i}</a>`;
      paginationList.appendChild(li);
    }

    // Next button
    const nextLi = document.createElement("li");
    nextLi.className = `page-item ${
      currentPage === totalPages ? "disabled" : ""
    }`;
    nextLi.innerHTML = `<a class="page-link" href="#" data-page="${
      currentPage + 1
    }">›</a>`;
    paginationList.appendChild(nextLi);

    // Add click events
    paginationList.addEventListener("click", (e) => {
      e.preventDefault();
      if (
        e.target.tagName === "A" &&
        !e.target.parentElement.classList.contains("disabled")
      ) {
        currentPage = parseInt(e.target.dataset.page);
        renderNotifications();
      }
    });
  }

  // Utility functions
  function getNotificationIcon(type) {
    const icons = {
      booking: "fas fa-calendar-check",
      booking_status: "fas fa-info-circle",
      payment: "fas fa-credit-card",
      booking_confirmation: "fas fa-check-circle",
      default: "fas fa-bell",
    };
    return icons[type] || icons.default;
  }

  function getTypeLabel(type) {
    const labels = {
      booking: "Đặt phòng",
      booking_status: "Trạng thái đặt phòng",
      payment: "Thanh toán",
      booking_confirmation: "Xác nhận đặt phòng",
      default: "Thông báo",
    };
    return labels[type] || labels.default;
  }

  function formatTime(timestamp) {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return "Vừa xong";
    if (diffMins < 60) return `${diffMins} phút trước`;
    if (diffHours < 24) return `${diffHours} giờ trước`;
    if (diffDays < 7) return `${diffDays} ngày trước`;

    return date.toLocaleDateString("vi-VN");
  }

  function formatFullTime(timestamp) {
    const date = new Date(timestamp);
    return date.toLocaleString("vi-VN");
  }

  function updateConnectionStatus(status) {
    const statusMessages = {
      connecting: { text: "Đang kết nối...", class: "" },
      connected: {
        text: "Kết nối thành công - Nhận thông báo real-time",
        class: "connected",
      },
      reconnecting: { text: "Đang kết nối lại...", class: "" },
      disconnected: {
        text: "Mất kết nối - Thông báo có thể bị trễ",
        class: "disconnected",
      },
      error: { text: "Lỗi kết nối - Đang thử lại...", class: "disconnected" },
      not_authenticated: {
        text: "Vui lòng đăng nhập để nhận thông báo",
        class: "disconnected",
      },
    };

    const config = statusMessages[status] || statusMessages.connecting;
    statusText.textContent = config.text;
    connectionStatus.className = `alert alert-info ${config.class}`;
  }

  function showEmptyState() {
    emptyState.style.display = "block";
    document.getElementById("paginationContainer").style.display = "none";
  }

  function hideEmptyState() {
    emptyState.style.display = "none";
  }

  function updateCounter(count) {
    notificationCounter.textContent = count;
    notificationCounter.className = `badge ${
      count > 0 ? "bg-primary" : "bg-secondary"
    } ms-2`;
  }

  function saveNotifications() {
    if (window.globalNotificationManager) {
      window.globalNotificationManager.saveNotifications(notifications);
      window.globalNotificationManager.updateNotificationBadge();
    } else {
      localStorage.setItem(
        "customerNotifications",
        JSON.stringify(notifications)
      );
    }
  }

  function showToast(message) {
    // Use existing toast system if available
    if (window.notificationManager && window.notificationManager.showToast) {
      window.notificationManager.showToast(message);
    } else {
      // Fallback toast
      const toast = document.createElement("div");
      toast.className = "toast-notification";
      toast.textContent = message;
      toast.style.cssText = `
                position: fixed; top: 20px; right: 20px; z-index: 9999;
                background: #007bff; color: white; padding: 10px 15px;
                border-radius: 5px; animation: slideIn 0.3s ease;
            `;
      document.body.appendChild(toast);
      setTimeout(() => document.body.removeChild(toast), 3000);
    }
  }

  function playNotificationSound() {
    try {
      const audio = new Audio("/sounds/notification.mp3");
      audio.volume = 0.3;
      audio.play().catch(() => {
        // Ignore audio play errors (browser restrictions)
      });
    } catch (e) {
      // Ignore audio errors
    }
  }

  // Expose functions globally for onclick handlers
  window.markAsRead = markAsRead;
  window.viewDetails = viewDetails;
  window.deleteNotification = deleteNotification;
});
