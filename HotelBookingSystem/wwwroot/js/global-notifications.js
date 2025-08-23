// Global Real-time Notifications for all pages
document.addEventListener("DOMContentLoaded", function () {
  // Check if user is authenticated
  if (!window.isUserAuthenticated) {
    return;
  }

  // Initialize notification system
  initializeGlobalNotifications();

  function initializeGlobalNotifications() {
    // Load notifications from localStorage
    loadStoredNotifications();

    // Initialize SignalR connection
    initializeSignalR();

    // Update notification badge
    updateNotificationBadge();
  }

  // SignalR Connection Setup
  let connection = null;

  function initializeSignalR() {
    connection = new signalR.HubConnectionBuilder()
      .withUrl("/notificationHub")
      .withAutomaticReconnect()
      .build();

    // Connection events
    connection.onreconnecting(() => {
      console.log("⏳ Đang kết nối lại SignalR...");
    });

    connection.onreconnected(() => {
      console.log("✅ Kết nối SignalR thành công");
    });

    connection.onclose(() => {
      console.log("❌ Kết nối SignalR đã ngắt");
    });

    // Listen for customer notifications
    connection.on("ReceiveNotification", function (notification) {
      handleIncomingNotification(notification);
    });

    // Start connection
    startConnection();
  }

  async function startConnection() {
    try {
      await connection.start();
      console.log("🔔 Hệ thống thông báo real-time đã sẵn sàng");
    } catch (err) {
      console.error("❌ Lỗi kết nối SignalR:", err);
      setTimeout(startConnection, 5000);
    }
  }

  // Notification Storage Management
  function getStoredNotifications() {
    try {
      return JSON.parse(localStorage.getItem("customerNotifications") || "[]");
    } catch (e) {
      console.warn("Lỗi đọc thông báo từ localStorage:", e);
      return [];
    }
  }

  function saveNotifications(notifications) {
    try {
      localStorage.setItem(
        "customerNotifications",
        JSON.stringify(notifications)
      );
    } catch (e) {
      console.warn("Lỗi lưu thông báo vào localStorage:", e);
    }
  }

  function loadStoredNotifications() {
    const notifications = getStoredNotifications();
    updateNotificationBadge();
    return notifications;
  }

  // Handle incoming real-time notifications
  function handleIncomingNotification(notification) {
    console.log("📨 Nhận thông báo mới:", notification);
    console.log("📨 Type:", notification.type);
    console.log("📨 Message:", notification.message);
    console.log("📨 Timestamp:", notification.timestamp);

    // Create formatted notification
    const newNotification = {
      id: Date.now() + Math.random(),
      type: notification.type || "booking_status",
      message: notification.message,
      timestamp: notification.timestamp || new Date().toISOString(),
      data: notification.data || {},
      isRead: false,
      isNew: true,
    };

    console.log("✅ Formatted notification:", newNotification);

    // Add to localStorage
    const notifications = getStoredNotifications();
    notifications.unshift(newNotification);

    // Keep only latest 50 notifications
    if (notifications.length > 50) {
      notifications.splice(50);
    }

    saveNotifications(notifications);
    console.log(
      "💾 Saved to localStorage, total notifications:",
      notifications.length
    );

    // Update badge
    updateNotificationBadge();

    // Show toast notification
    showToastNotification(newNotification);

    // Update notification page if currently open
    if (window.location.pathname.includes("/Notification")) {
      // Trigger page refresh or custom event
      if (typeof window.refreshNotifications === "function") {
        window.refreshNotifications();
      }
    }
  }

  // Notification Badge Management
  function updateNotificationBadge() {
    const badge = document.getElementById("notificationBadge");
    if (!badge) return;

    const notifications = getStoredNotifications();
    const unreadCount = notifications.filter((n) => !n.isRead).length;

    if (unreadCount > 0) {
      badge.textContent = unreadCount > 99 ? "99+" : unreadCount;
      badge.style.display = "inline-block";
      badge.classList.add("animate-pulse");
    } else {
      badge.style.display = "none";
      badge.classList.remove("animate-pulse");
    }
  }

  // Toast Notification Display
  function showToastNotification(notification) {
    const toastContainer = document.querySelector(".toast-container");
    if (!toastContainer) {
      console.warn("Toast container không tồn tại");
      return;
    }

    const toastId = `toast-${Date.now()}`;
    const toastElement = document.createElement("div");
    toastElement.id = toastId;
    toastElement.className = "toast";
    toastElement.setAttribute("role", "alert");
    toastElement.setAttribute("aria-live", "assertive");
    toastElement.setAttribute("aria-atomic", "true");

    const toastIcon = getNotificationIcon(notification.type);
    const toastColor = getNotificationColor(notification.type);

    toastElement.innerHTML = `
            <div class="toast-header bg-${toastColor} text-white">
                <i class="${toastIcon} me-2"></i>
                <strong class="me-auto">${getNotificationTitle(
                  notification.type
                )}</strong>
                <small class="text-white-50">Vừa xong</small>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${notification.message}
                <div class="mt-2">
                    <a href="/Notification" class="btn btn-sm btn-outline-primary">Xem chi tiết</a>
                </div>
            </div>
        `;

    toastContainer.appendChild(toastElement);

    // Initialize and show toast
    const toast = new bootstrap.Toast(toastElement, {
      autohide: true,
      delay: 8000, // Show for 8 seconds
    });

    toast.show();

    // Remove from DOM after hiding
    toastElement.addEventListener("hidden.bs.toast", function () {
      if (toastContainer.contains(toastElement)) {
        toastContainer.removeChild(toastElement);
      }
    });

    // Add click to mark as read
    toastElement.addEventListener("click", function () {
      markNotificationAsRead(notification.id);
    });
  }

  // Helper Functions
  function getNotificationIcon(type) {
    const icons = {
      booking_status: "fas fa-calendar-check",
      booking_confirmation: "fas fa-check-circle",
      booking: "fas fa-bed",
      payment: "fas fa-credit-card",
      cancellation: "fas fa-times-circle",
      default: "fas fa-bell",
    };
    return icons[type] || icons.default;
  }

  function getNotificationColor(type) {
    const colors = {
      booking_status: "success",
      booking_confirmation: "info",
      booking: "primary",
      payment: "warning",
      cancellation: "danger",
      default: "info",
    };
    return colors[type] || colors.default;
  }

  function getNotificationTitle(type) {
    const titles = {
      booking_status: "Cập nhật đặt phòng",
      booking_confirmation: "Xác nhận đặt phòng",
      booking: "Đặt phòng mới",
      payment: "Thanh toán",
      cancellation: "Hủy đặt phòng",
      default: "Thông báo",
    };
    return titles[type] || titles.default;
  }

  function markNotificationAsRead(notificationId) {
    const notifications = getStoredNotifications();
    const notification = notifications.find((n) => n.id == notificationId);
    if (notification) {
      notification.isRead = true;
      saveNotifications(notifications);
      updateNotificationBadge();
    }
  }

  // Global functions
  window.markAllNotificationsAsRead = function () {
    const notifications = getStoredNotifications();
    notifications.forEach((n) => (n.isRead = true));
    saveNotifications(notifications);
    updateNotificationBadge();
  };

  window.getUnreadNotificationCount = function () {
    const notifications = getStoredNotifications();
    return notifications.filter((n) => !n.isRead).length;
  };

  window.clearAllNotifications = function () {
    localStorage.removeItem("customerNotifications");
    updateNotificationBadge();
  };

  // Export for use in notification page
  window.globalNotificationManager = {
    getStoredNotifications,
    saveNotifications,
    updateNotificationBadge,
    markNotificationAsRead,
  };

  console.log("🌟 Global notification system initialized");
});
