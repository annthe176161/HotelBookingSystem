// Customer Notifications Page - Integrated with Global Notification System
document.addEventListener("DOMContentLoaded", function () {
  const notificationsList = document.getElementById("notificationsList");
  const notificationCounter = document.getElementById("notificationCounter");
  const emptyState = document.getElementById("emptyState");
  const connectionStatus = document.getElementById("connectionStatus");
  const statusText = document.getElementById("statusText");

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
    // Get notifications from global manager or localStorage fallback
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

  function saveNotifications() {
    if (window.globalNotificationManager) {
      window.globalNotificationManager.saveNotifications(notifications);
    } else {
      localStorage.setItem(
        "customerNotifications",
        JSON.stringify(notifications)
      );
    }
  }

  function markAllAsRead() {
    notifications.forEach((notification) => {
      notification.isRead = true;
    });
    saveNotifications();
    renderNotifications();

    if (window.globalNotificationManager) {
      window.globalNotificationManager.updateNotificationBadge();
    }
  }

  function clearAllNotifications() {
    if (confirm("Bạn có chắc chắn muốn xóa tất cả thông báo?")) {
      notifications = [];
      saveNotifications();
      renderNotifications();

      if (window.globalNotificationManager) {
        window.globalNotificationManager.updateNotificationBadge();
      }
    }
  }

  function deleteNotification(notificationId) {
    if (confirm("Bạn có chắc chắn muốn xóa thông báo này?")) {
      notifications = notifications.filter((n) => n.id != notificationId);
      saveNotifications();
      renderNotifications();

      if (window.globalNotificationManager) {
        window.globalNotificationManager.updateNotificationBadge();
      }
    }
  }

  // Global functions
  window.markAsRead = markAsRead;
  window.deleteNotification = deleteNotification;
  window.viewDetails = viewDetails;

  function viewDetails(notificationId) {
    const notification = notifications.find((n) => n.id == notificationId);
    if (!notification) return;

    const modal = document.getElementById("notificationDetailModal");
    const modalTitle = document.getElementById("modalTitle");
    const modalBody = document.getElementById("modalBody");

    modalTitle.textContent = getTypeLabel(notification.type);

    modalBody.innerHTML = `
            <div class="notification-detail">
                <p><strong>Thông báo:</strong> ${notification.message}</p>
                <p><strong>Thời gian:</strong> ${formatTime(
                  notification.timestamp
                )}</p>
                <p><strong>Loại:</strong> ${getTypeLabel(notification.type)}</p>
                ${
                  notification.data
                    ? `
                    <p><strong>Chi tiết:</strong></p>
                    <pre class="bg-light p-2 rounded">${JSON.stringify(
                      notification.data,
                      null,
                      2
                    )}</pre>
                `
                    : ""
                }
            </div>
        `;

    const bootstrapModal = new bootstrap.Modal(modal);
    bootstrapModal.show();

    // Mark as read when viewed
    markAsRead(notificationId);
  }

  function filterNotifications() {
    currentPage = 1;
    renderNotifications();
  }

  function showEmptyState() {
    emptyState.style.display = "block";
    notificationsList.style.display = "none";
    updateCounter(0);
  }

  function hideEmptyState() {
    emptyState.style.display = "none";
    notificationsList.style.display = "block";
  }

  function updateCounter(count) {
    if (notificationCounter) {
      notificationCounter.textContent = count;
    }
  }

  function updateConnectionStatus(status) {
    if (!connectionStatus || !statusText) return;

    connectionStatus.className = "alert alert-info";

    switch (status) {
      case "connected":
        connectionStatus.className = "alert alert-success";
        statusText.innerHTML =
          '<i class="fas fa-check-circle me-2"></i>Kết nối thành công';
        break;
      case "connecting":
        connectionStatus.className = "alert alert-warning";
        statusText.innerHTML =
          '<i class="fas fa-spinner fa-spin me-2"></i>Đang kết nối...';
        break;
      case "disconnected":
        connectionStatus.className = "alert alert-danger";
        statusText.innerHTML =
          '<i class="fas fa-exclamation-triangle me-2"></i>Mất kết nối';
        break;
      case "error":
        connectionStatus.className = "alert alert-danger";
        statusText.innerHTML =
          '<i class="fas fa-times-circle me-2"></i>Lỗi kết nối';
        break;
    }
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
    paginationList.addEventListener("click", function (e) {
      e.preventDefault();
      if (
        e.target.tagName === "A" &&
        !e.target.parentElement.classList.contains("disabled")
      ) {
        const page = parseInt(e.target.dataset.page);
        if (page && page !== currentPage) {
          currentPage = page;
          renderNotifications();
        }
      }
    });
  }

  // Helper functions
  function getNotificationIcon(type) {
    const icons = {
      booking_status: "fas fa-calendar-check",
      booking_confirmation: "fas fa-check-circle",
      booking: "fas fa-bed",
      payment: "fas fa-credit-card",
      default: "fas fa-bell",
    };
    return icons[type] || icons.default;
  }

  function getTypeLabel(type) {
    const labels = {
      booking_status: "Trạng thái đặt phòng",
      booking_confirmation: "Xác nhận đặt phòng",
      booking: "Đặt phòng",
      payment: "Thanh toán",
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

  console.log(
    "📱 Customer Notifications Page initialized with global integration"
  );
});
