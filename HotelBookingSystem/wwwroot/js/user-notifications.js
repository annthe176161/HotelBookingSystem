// User Notifications System
class UserNotificationManager {
    constructor() {
        this.notifications = [];
        this.unreadCount = 0;
        this.userId = null;
        this.init();
    }

    init() {
        // Only initialize if user is authenticated
        if (!this.isUserAuthenticated()) {
            return;
        }
        
        this.getUserId();
        this.loadInitialNotifications();
        this.updateBadge();
        this.bindEvents();
        
        // In real implementation, this would connect to SignalR hub
        this.startDemoNotifications();
    }

    isUserAuthenticated() {
        // Check if notification dropdown exists (means user is logged in)
        return document.getElementById('userNotificationDropdown') !== null;
    }

    getUserId() {
        // In real implementation, this would get actual user ID
        // For now, simulate with random ID
        this.userId = 'user_' + Math.random().toString(36).substr(2, 9);
    }

    loadInitialNotifications() {
        // Load existing notifications from server
        // For demo, use the ones already in HTML
        this.countUnreadNotifications();
    }

    countUnreadNotifications() {
        const unreadItems = document.querySelectorAll('.notification-item.unread');
        this.unreadCount = unreadItems.length;
        this.updateBadge();
    }

    bindEvents() {
        // Mark notifications as read when dropdown is opened
        const notificationDropdown = document.getElementById('userNotificationDropdown');
        if (notificationDropdown) {
            notificationDropdown.addEventListener('shown.bs.dropdown', () => {
                this.markVisibleAsRead();
            });
        }

        // Handle notification item clicks
        document.addEventListener('click', (e) => {
            if (e.target.closest('.notification-item')) {
                const item = e.target.closest('.notification-item');
                this.markNotificationAsRead(item);
            }
        });
    }

    startDemoNotifications() {
        // Demo: Add new notifications periodically
        setTimeout(() => {
            this.addNotification({
                id: Date.now(),
                title: 'Xác nhận thanh toán',
                message: 'Thanh toán cho booking #BK2025001 đã được xử lý thành công',
                type: 'payment',
                icon: 'fas fa-credit-card',
                iconColor: 'bg-success',
                time: new Date(),
                link: '/Bookings'
            });
        }, 8000);

        setTimeout(() => {
            this.addNotification({
                id: Date.now() + 1,
                title: 'Khuyến mãi mới',
                message: 'Đặt phòng ngay để nhận ưu đãi giảm 25% cho kỳ nghỉ cuối tuần',
                type: 'promotion',
                icon: 'fas fa-percent',
                iconColor: 'bg-warning',
                time: new Date(),
                link: '/Rooms'
            });
        }, 15000);

        setTimeout(() => {
            this.addNotification({
                id: Date.now() + 2,
                title: 'Nhắc nhở check-out',
                message: 'Thời gian check-out là 12:00 PM ngày mai. Vui lòng chuẩn bị hành lý',
                type: 'reminder',
                icon: 'fas fa-clock',
                iconColor: 'bg-info',
                time: new Date(),
                link: '/Bookings'
            });
        }, 25000);
    }

    addNotification(notification) {
        // Add to notifications array
        this.notifications.unshift(notification);
        
        // Keep only latest 20 notifications
        if (this.notifications.length > 20) {
            this.notifications = this.notifications.slice(0, 20);
        }

        // Increment unread count
        this.unreadCount++;
        
        // Update UI
        this.updateBadge();
        this.updateDropdownContent(notification);
        this.showToastNotification(notification);
        this.playNotificationSound();

        // In real implementation, this would also update server
        // this.saveNotificationToServer(notification);
    }

    updateBadge() {
        const badge = document.getElementById('userNotificationBadge');
        if (badge) {
            if (this.unreadCount > 0) {
                badge.textContent = this.unreadCount > 99 ? '99+' : this.unreadCount;
                badge.style.display = 'flex';
            } else {
                badge.style.display = 'none';
            }
        }

        // Update header badge in dropdown
        const headerBadge = document.querySelector('.user-notification-dropdown .badge');
        if (headerBadge) {
            headerBadge.textContent = `${this.unreadCount} mới`;
        }
    }

    updateDropdownContent(newNotification) {
        const notificationItems = document.querySelector('.notification-items');
        if (!notificationItems) return;

        // Create new notification HTML
        const notificationHTML = `
            <a class="dropdown-item notification-item unread" href="${newNotification.link || '#'}" data-id="${newNotification.id}">
                <div class="notification-icon ${newNotification.iconColor}">
                    <i class="${newNotification.icon}"></i>
                </div>
                <div class="notification-content">
                    <div class="notification-title">${newNotification.title}</div>
                    <div class="notification-text">${newNotification.message}</div>
                    <div class="notification-time">${this.formatTime(newNotification.time)}</div>
                </div>
            </a>
        `;

        // Add to top of notifications
        notificationItems.insertAdjacentHTML('afterbegin', notificationHTML);

        // Remove oldest if more than 10 visible
        const visibleItems = notificationItems.querySelectorAll('.notification-item');
        if (visibleItems.length > 10) {
            visibleItems[visibleItems.length - 1].remove();
        }
    }

    showToastNotification(notification) {
        // Remove existing toast if any
        const existingToast = document.querySelector('.user-notification-toast');
        if (existingToast) {
            existingToast.remove();
        }

        // Create new toast
        const toast = document.createElement('div');
        toast.className = 'user-notification-toast';
        toast.innerHTML = `
            <button class="toast-close" onclick="this.parentElement.remove()">&times;</button>
            <div class="toast-content">
                <div class="toast-icon ${notification.iconColor}">
                    <i class="${notification.icon}"></i>
                </div>
                <div class="toast-text">
                    <div class="toast-title">${notification.title}</div>
                    <div class="toast-message">${notification.message}</div>
                </div>
            </div>
        `;

        // Add to page
        document.body.appendChild(toast);

        // Show with animation
        setTimeout(() => {
            toast.classList.add('show');
        }, 100);

        // Auto remove after 6 seconds
        setTimeout(() => {
            if (toast.parentElement) {
                toast.classList.remove('show');
                setTimeout(() => {
                    if (toast.parentElement) {
                        toast.remove();
                    }
                }, 300);
            }
        }, 6000);

        // Make toast clickable
        toast.addEventListener('click', () => {
            if (notification.link) {
                window.location.href = notification.link;
            }
        });
    }

    playNotificationSound() {
        try {
            // Create a gentle notification sound
            const audioContext = new (window.AudioContext || window.webkitAudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            // Create a pleasant two-tone notification sound
            oscillator.frequency.setValueAtTime(600, audioContext.currentTime);
            oscillator.frequency.setValueAtTime(800, audioContext.currentTime + 0.1);
            
            gainNode.gain.setValueAtTime(0.05, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.3);

            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.3);
        } catch (error) {
            // Fallback: no sound if Web Audio API is not supported
            console.log('Notification sound not supported');
        }
    }

    markVisibleAsRead() {
        const unreadItems = document.querySelectorAll('.notification-item.unread');
        unreadItems.forEach(item => {
            item.classList.remove('unread');
        });
        
        this.unreadCount = 0;
        this.updateBadge();

        // In real implementation, update server
        // this.markNotificationsAsReadOnServer();
    }

    markNotificationAsRead(item) {
        if (item.classList.contains('unread')) {
            item.classList.remove('unread');
            this.unreadCount = Math.max(0, this.unreadCount - 1);
            this.updateBadge();

            // In real implementation, update server
            const notificationId = item.dataset.id;
            // this.markNotificationAsReadOnServer(notificationId);
        }
    }

    formatTime(date) {
        const now = new Date();
        const diff = now - date;
        const minutes = Math.floor(diff / 60000);
        const hours = Math.floor(diff / 3600000);
        const days = Math.floor(diff / 86400000);

        if (minutes < 1) return 'Vừa xong';
        if (minutes < 60) return `${minutes} phút trước`;
        if (hours < 24) return `${hours} giờ trước`;
        if (days < 7) return `${days} ngày trước`;
        
        return date.toLocaleDateString('vi-VN');
    }

    // Methods for SignalR integration (to be implemented later)
    connectToHub() {
        // Connect to SignalR notification hub
        // const connection = new signalR.HubConnectionBuilder()
        //     .withUrl("/notificationHub")
        //     .build();
        
        // connection.start().then(() => {
        //     console.log("Connected to notification hub");
        //     this.registerUserConnection();
        // });

        // connection.on("ReceiveNotification", (notification) => {
        //     this.addNotification(notification);
        // });
    }

    registerUserConnection() {
        // Register user connection with hub
        // connection.invoke("JoinUserGroup", this.userId);
    }

    // API methods (to be implemented later)
    async fetchNotificationsFromServer() {
        try {
            // const response = await fetch('/api/user-notifications');
            // return await response.json();
            return [];
        } catch (error) {
            console.error('Error fetching notifications:', error);
            return [];
        }
    }

    async markNotificationAsReadOnServer(notificationId) {
        try {
            // await fetch(`/api/user-notifications/${notificationId}/read`, {
            //     method: 'POST'
            // });
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }
}

// Initialize user notification manager when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Initialize for all pages (will check if user is authenticated internally)
    window.userNotificationManager = new UserNotificationManager();
});

// Export for potential use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UserNotificationManager;
}
