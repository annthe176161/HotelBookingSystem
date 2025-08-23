// Simple Global Notifications - Debug Version
console.log("🔍 Loading simple global notifications...");

document.addEventListener("DOMContentLoaded", function () {
  console.log("🔍 DOM Content Loaded");

  // Check if user is authenticated
  if (!window.isUserAuthenticated) {
    console.log("❌ User not authenticated, skipping notification setup");
    return;
  }

  console.log("✅ User is authenticated, setting up notifications...");

  // Simple notification storage
  let notifications = [];

  // Load existing notifications
  try {
    notifications = JSON.parse(
      localStorage.getItem("customerNotifications") || "[]"
    );
    console.log(`📚 Loaded ${notifications.length} existing notifications`);
  } catch (e) {
    console.warn("❌ Error loading notifications:", e);
    notifications = [];
  }

  // Initialize SignalR
  initializeSignalR();

  // Update badge initially
  updateBadge();

  function initializeSignalR() {
    console.log("🔌 Initializing SignalR...");

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/notificationHub")
      .withAutomaticReconnect()
      .build();

    // Listen for notifications
    connection.on("ReceiveNotification", function (notification) {
      console.log("📨 RAW notification received:", notification);
      handleNotification(notification);
    });

    // Connection events
    connection.onreconnecting(() => {
      console.log("🔄 SignalR Reconnecting...");
    });

    connection.onreconnected(() => {
      console.log("✅ SignalR Reconnected");
    });

    connection.onclose(() => {
      console.log("❌ SignalR Connection closed");
    });

    // Start connection
    startConnection(connection);
  }

  async function startConnection(connection) {
    try {
      await connection.start();
      console.log("🎉 SignalR Connected successfully!");
    } catch (err) {
      console.error("❌ SignalR Connection error:", err);
      setTimeout(() => startConnection(connection), 5000);
    }
  }

  function handleNotification(notification) {
    console.log("🔄 Processing notification:", notification);

    // Create notification object
    const newNotification = {
      id: Date.now() + Math.random(),
      type: notification.type || "info",
      message: notification.message || "No message",
      timestamp: notification.timestamp || new Date().toISOString(),
      data: notification.data || {},
      isRead: false,
      isNew: true,
    };

    console.log("✅ Formatted notification:", newNotification);

    // Add to array
    notifications.unshift(newNotification);

    // Keep only latest 50
    if (notifications.length > 50) {
      notifications.splice(50);
    }

    // Save to localStorage
    try {
      localStorage.setItem(
        "customerNotifications",
        JSON.stringify(notifications)
      );
      console.log("💾 Saved to localStorage, total:", notifications.length);
    } catch (e) {
      console.error("❌ Error saving to localStorage:", e);
    }

    // Update badge
    updateBadge();

    // Show toast
    showSimpleToast(newNotification);
  }

  function updateBadge() {
    const badge = document.getElementById("notificationBadge");
    if (!badge) {
      console.log("❌ Notification badge element not found");
      return;
    }

    const unreadCount = notifications.filter((n) => !n.isRead).length;
    console.log(`🔢 Updating badge: ${unreadCount} unread notifications`);

    if (unreadCount > 0) {
      badge.textContent = unreadCount > 99 ? "99+" : unreadCount;
      badge.style.display = "inline-block";
      badge.classList.add("animate-pulse");
      console.log("✅ Badge updated and shown");
    } else {
      badge.style.display = "none";
      badge.classList.remove("animate-pulse");
      console.log("✅ Badge hidden (no unread notifications)");
    }
  }

  function showSimpleToast(notification) {
    console.log("🍞 Showing toast for:", notification.message);

    const toastContainer = document.querySelector(".toast-container");
    if (!toastContainer) {
      console.log("❌ Toast container not found");
      return;
    }

    const toast = document.createElement("div");
    toast.className = "toast show";
    toast.innerHTML = `
            <div class="toast-header bg-primary text-white">
                <strong class="me-auto">Thông báo mới</strong>
                <button type="button" class="btn-close btn-close-white" onclick="this.parentElement.parentElement.remove()"></button>
            </div>
            <div class="toast-body">
                ${notification.message}
            </div>
        `;

    toastContainer.appendChild(toast);
    console.log("✅ Toast added to container");

    // Auto remove after 5 seconds
    setTimeout(() => {
      if (toast.parentNode) {
        toast.remove();
        console.log("🗑️ Toast auto-removed");
      }
    }, 5000);
  }

  // Global functions for external access
  window.simpleNotificationManager = {
    getNotifications: () => notifications,
    updateBadge: updateBadge,
    getUnreadCount: () => notifications.filter((n) => !n.isRead).length,
  };

  // Test function
  window.testNotification = function () {
    console.log("🧪 Testing notification...");
    handleNotification({
      type: "booking_status",
      message: "Test notification - " + new Date().toLocaleTimeString(),
      timestamp: new Date().toISOString(),
    });
  };

  console.log("🎉 Simple global notifications initialized!");
});
