// User Notifications System
class UserNotificationManager {
  constructor() {
    this.notifications = [];
    this.unreadCount = 0;
    this.init();
  }

  init() {
    this.bindEvents();
    this.loadNotifications();
    this.updateBadge();
  }

  bindEvents() {
    // Mark all as read
    const markAllReadBtn = document.getElementById("markAllRead");
    if (markAllReadBtn) {
      markAllReadBtn.addEventListener("click", () => {
        this.markAllAsRead();
      });
    }

    // Individual notification clicks
    document.addEventListener("click", (e) => {
      if (e.target.closest(".notification-item")) {
        const notificationItem = e.target.closest(".notification-item");
        this.markAsRead(notificationItem);
      }
    });

    // View all notifications
    const viewAllBtn = document.getElementById("viewAllNotifications");
    if (viewAllBtn) {
      viewAllBtn.addEventListener("click", (e) => {
        e.preventDefault();
        this.viewAllNotifications();
      });
    }
  }

  loadNotifications() {
    // Load real notifications from SignalR
    // This will be populated by SignalR notifications only
    this.notifications = [];

    this.renderNotifications();
    this.updateBadge();
  }

  renderNotifications() {
    const notificationList = document.getElementById("notificationList");
    if (!notificationList) return;

    notificationList.innerHTML = "";

    this.notifications.forEach((notification) => {
      const notificationElement = this.createNotificationElement(notification);
      notificationList.appendChild(notificationElement);
    });
  }

  createNotificationElement(notification) {
    const li = document.createElement("li");
    li.className = `notification-item ${!notification.isRead ? "unread" : ""}`;
    li.dataset.notificationId = notification.id;

    li.innerHTML = `
            <div class="notification-content">
                <div class="notification-icon-wrapper ${notification.type}">
                    <i class="${notification.icon}"></i>
                </div>
                <div class="notification-text">
                    <div class="notification-title">${notification.title}</div>
                    <div class="notification-message">${notification.message}</div>
                    <div class="notification-time">${notification.time}</div>
                </div>
            </div>
        `;

    return li;
  }

  markAsRead(notificationElement) {
    const notificationId = parseInt(notificationElement.dataset.notificationId);
    const notification = this.notifications.find(
      (n) => n.id === notificationId
    );

    if (notification && !notification.isRead) {
      notification.isRead = true;
      notificationElement.classList.remove("unread");
      this.updateBadge();

      // Play notification sound
      this.playNotificationSound("read");

      // In real implementation, send AJAX request to mark as read on server
      // this.markAsReadOnServer(notificationId);
    }
  }

  markAllAsRead() {
    this.notifications.forEach((notification) => {
      notification.isRead = true;
    });

    document.querySelectorAll(".notification-item.unread").forEach((item) => {
      item.classList.remove("unread");
    });

    this.updateBadge();
    this.playNotificationSound("read");

    // Show success message
    this.showToast("Đã đánh dấu tất cả thông báo là đã đọc", "success");
  }

  updateBadge() {
    this.unreadCount = this.notifications.filter((n) => !n.isRead).length;
    const badge = document.getElementById("notificationBadge");

    if (badge) {
      if (this.unreadCount > 0) {
        badge.textContent = this.unreadCount > 99 ? "99+" : this.unreadCount;
        badge.style.display = "flex";
      } else {
        badge.style.display = "none";
      }
    }
  }

  addNotification(notification) {
    // Add new notification to the beginning of the array
    notification.id = Date.now();
    notification.isRead = false;
    this.notifications.unshift(notification);

    // Keep only last 50 notifications
    if (this.notifications.length > 50) {
      this.notifications = this.notifications.slice(0, 50);
    }

    this.renderNotifications();
    this.updateBadge();
    this.playNotificationSound("new");

    // Show toast notification
    this.showToast(notification.title, "info");
  }

  playNotificationSound(type) {
    // Simple beep sound using Web Audio API
    if (typeof Audio !== "undefined") {
      try {
        const audioContext = new (window.AudioContext ||
          window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        oscillator.frequency.value = type === "new" ? 800 : 600;
        oscillator.type = "sine";

        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(
          0.01,
          audioContext.currentTime + 0.1
        );

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.1);
      } catch (e) {
        // Fallback for browsers that don't support Web Audio API
        console.log("Notification sound not available");
      }
    }
  }

  showToast(message, type = "info") {
    // Create toast element
    const toast = document.createElement("div");
    toast.className = `toast-notification toast-${type}`;
    toast.innerHTML = `
            <div class="toast-content">
                <i class="fas fa-${
                  type === "success"
                    ? "check-circle"
                    : type === "error"
                    ? "exclamation-circle"
                    : "info-circle"
                }"></i>
                <span>${message}</span>
            </div>
        `;

    // Style the toast
    Object.assign(toast.style, {
      position: "fixed",
      top: "20px",
      right: "20px",
      background:
        type === "success"
          ? "#10b981"
          : type === "error"
          ? "#ef4444"
          : "#3b82f6",
      color: "white",
      padding: "12px 20px",
      borderRadius: "8px",
      zIndex: "9999",
      transform: "translateX(100%)",
      transition: "transform 0.3s ease",
      maxWidth: "300px",
      boxShadow: "0 4px 12px rgba(0, 0, 0, 0.15)",
    });

    // Add to DOM
    document.body.appendChild(toast);

    // Show toast
    setTimeout(() => {
      toast.style.transform = "translateX(0)";
    }, 100);

    // Hide and remove toast
    setTimeout(() => {
      toast.style.transform = "translateX(100%)";
      setTimeout(() => {
        if (toast.parentNode) {
          toast.parentNode.removeChild(toast);
        }
      }, 300);
    }, 3000);
  }

  viewAllNotifications() {
    // In real implementation, this would redirect to a full notifications page
    this.showToast(
      "Chức năng xem tất cả thông báo sẽ được phát triển thêm!",
      "info"
    );
  }

  // Method to receive notifications from SignalR
  receiveNotification(notification) {
    this.addNotification(notification);
  }
}

// Initialize notification system when DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  const notificationManager = new UserNotificationManager();

  // Expose to global scope for SignalR integration
  window.notificationManager = notificationManager;
});

// CSS for toast notifications
const style = document.createElement("style");
style.textContent = `
    .toast-notification {
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        font-size: 14px;
        font-weight: 500;
    }
    
    .toast-content {
        display: flex;
        align-items: center;
        gap: 8px;
    }
    
    .toast-content i {
        font-size: 16px;
    }
    
    @media (max-width: 576px) {
        .toast-notification {
            right: 10px !important;
            left: 10px !important;
            max-width: none !important;
        }
    }
`;
document.head.appendChild(style);
