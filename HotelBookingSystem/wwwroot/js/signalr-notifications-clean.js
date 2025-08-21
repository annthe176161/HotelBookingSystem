// SignalR Real-time Notifications - Production Version
document.addEventListener("DOMContentLoaded", function () {
  // Check if user is authenticated
  if (!window.isUserAuthenticated) {
    return;
  }

  // Initialize SignalR connection
  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

  // Start connection
  startConnection();

  async function startConnection() {
    try {
      await connection.start();
    } catch (err) {
      console.error("SignalR Connection Error:", err);
      setTimeout(startConnection, 5000);
    }
  }

  // Listen for notifications
  connection.on("ReceiveNotification", function (notification) {
    addNotificationToList(notification);
    updateNotificationCount();
    showToastNotification(notification);
  });

  // Listen for admin notifications
  connection.on("ReceiveAdminNotification", function (notification) {
    addNotificationToList(notification);
    updateNotificationCount();
    showToastNotification(notification);
  });

  // Listen for customer confirmation notifications
  connection.on("ReceiveCustomerConfirmation", function (notification) {
    addNotificationToList(notification);
    updateNotificationCount();
    showToastNotification(notification);
  });

  function addNotificationToList(notification) {
    const notificationList = document.getElementById("notificationList");
    if (!notificationList) return;

    const notificationElement = document.createElement("div");
    notificationElement.className = "dropdown-item notification-item";
    notificationElement.innerHTML = `
            <div class="notification-icon ${getNotificationIconClass(
              notification.type
            )}">
                <i class="${getNotificationIcon(notification.type)}"></i>
            </div>
            <div class="notification-content">
                <div class="notification-text">${notification.message}</div>
                <div class="notification-time">${formatTime(
                  notification.timestamp
                )}</div>
            </div>
        `;

    // Add to beginning of list
    if (notificationList.children.length > 0) {
      notificationList.insertBefore(
        notificationElement,
        notificationList.firstChild
      );
    } else {
      notificationList.appendChild(notificationElement);
    }

    // Limit to 10 notifications
    while (notificationList.children.length > 10) {
      notificationList.removeChild(notificationList.lastChild);
    }
  }

  function updateNotificationCount() {
    const badge = document.getElementById("notificationBadge");
    const countElement = document.getElementById("notificationCount");

    if (!badge) return;

    const notificationList = document.getElementById("notificationList");
    const unreadCount = notificationList ? notificationList.children.length : 0;

    if (unreadCount > 0) {
      badge.textContent = unreadCount;
      badge.style.display = "inline";
      badge.className = "notification-badge animate-pulse";

      if (countElement) {
        countElement.textContent = `${unreadCount} mới`;
      }
    } else {
      badge.style.display = "none";
      if (countElement) {
        countElement.textContent = "0 mới";
      }
    }
  }

  function showToastNotification(notification) {
    const toastContainer = document.querySelector(".toast-container");
    if (!toastContainer) return;

    const toastElement = document.createElement("div");
    toastElement.className = "toast";
    toastElement.setAttribute("role", "alert");
    toastElement.setAttribute("aria-live", "assertive");
    toastElement.setAttribute("aria-atomic", "true");

    toastElement.innerHTML = `
            <div class="toast-header">
                <i class="fas fa-bell text-primary me-2"></i>
                <strong class="me-auto">Thông báo mới</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${notification.message}
            </div>
        `;

    toastContainer.appendChild(toastElement);

    // Initialize and show toast
    const toast = new bootstrap.Toast(toastElement, {
      autohide: true,
      delay: 5000,
    });
    toast.show();

    // Remove from DOM after hiding
    toastElement.addEventListener("hidden.bs.toast", function () {
      toastContainer.removeChild(toastElement);
    });
  }

  function getNotificationIcon(type) {
    const icons = {
      booking: "fas fa-calendar-check",
      confirmation: "fas fa-check-circle",
      cancellation: "fas fa-times-circle",
      payment: "fas fa-credit-card",
      default: "fas fa-bell",
    };
    return icons[type] || icons.default;
  }

  function getNotificationIconClass(type) {
    const classes = {
      booking: "bg-primary",
      confirmation: "bg-success",
      cancellation: "bg-danger",
      payment: "bg-warning",
      default: "bg-info",
    };
    return classes[type] || classes.default;
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

  // Handle connection events for UI updates
  connection.onreconnecting(() => {
    console.log("Đang kết nối lại...");
  });

  connection.onreconnected(() => {
    console.log("Đã kết nối lại thành công");
  });

  connection.onclose(() => {
    console.log("Kết nối đã ngắt");
  });
});
