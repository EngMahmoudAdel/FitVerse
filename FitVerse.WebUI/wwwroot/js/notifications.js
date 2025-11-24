// ========================================
// Notifications System with SignalR
// ========================================

let notificationConnection = null;
let isNotificationConnected = false;

// Initialize SignalR connection for notifications
function initializeNotifications() {
    if (notificationConnection) {
        console.log('Notification connection already exists');
        return;
    }

    // Create SignalR connection
    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    // Handle incoming notifications
    notificationConnection.on("ReceiveNotification", function (notification) {
        console.log('Received notification:', notification);
        
        // Add notification to dropdown
        addNotificationToDropdown(notification);
        
        // Update notification count
        updateNotificationCount();
        
        // Show toast notification
        showNotificationToast(notification);
    });

    // Handle notification count updates
    notificationConnection.on("UpdateNotificationCount", function (count) {
        console.log('Notification count updated:', count);
        updateNotificationBadge(count);
    });

    // Handle notification marked as read
    notificationConnection.on("NotificationMarkedAsRead", function (notificationId) {
        console.log('Notification marked as read:', notificationId);
        markNotificationAsReadInUI(notificationId);
    });

    // Handle all notifications marked as read
    notificationConnection.on("AllNotificationsMarkedAsRead", function () {
        console.log('All notifications marked as read');
        markAllNotificationsAsReadInUI();
    });

    // Handle connection state changes
    notificationConnection.onreconnecting(() => {
        console.log('Reconnecting to notification hub...');
        isNotificationConnected = false;
    });

    notificationConnection.onreconnected(() => {
        console.log('Reconnected to notification hub');
        isNotificationConnected = true;
        loadNotifications();
    });

    notificationConnection.onclose(() => {
        console.log('Notification connection closed');
        isNotificationConnected = false;
    });

    // Start the connection
    notificationConnection.start()
        .then(function () {
            console.log('Connected to notification hub');
            isNotificationConnected = true;
            loadNotifications();
        })
        .catch(function (err) {
            console.error('Error connecting to notification hub:', err);
            setTimeout(initializeNotifications, 5000); // Retry after 5 seconds
        });
}

// Load notifications from server
function loadNotifications() {
    $.ajax({
        url: '/Notification/GetRecent',
        method: 'GET',
        data: { count: 10 },
        success: function (response) {
            if (response.success) {
                displayNotifications(response.data);
                updateNotificationBadge(response.data.filter(n => !n.IsRead).length);
            }
        },
        error: function (error) {
            console.error('Error loading notifications:', error);
        }
    });
}

// Display notifications in dropdown
function displayNotifications(notifications) {
    const container = $('#notificationDropdown, #notificationList, .notification-list');
    
    if (container.length === 0) {
        console.warn('Notification container not found');
        return;
    }

    container.empty();

    if (notifications.length === 0) {
        container.append(`
            <div class="dropdown-item text-center text-muted py-4">
                <i class="bi bi-bell-slash fs-3 mb-2"></i>
                <p class="mb-0">No notifications</p>
            </div>
        `);
        return;
    }

    notifications.forEach(notification => {
        const notificationHtml = createNotificationHTML(notification);
        container.append(notificationHtml);
    });
}

// Create notification HTML
function createNotificationHTML(notification) {
    const isRead = notification.IsRead ? 'read' : 'unread';
    const icon = getNotificationIcon(notification.Type);
    const color = getNotificationColor(notification.Type);
    
    return `
        <div class="dropdown-item notification-item ${isRead}" data-id="${notification.Id}" onclick="handleNotificationClick(${notification.Id}, ${notification.RefId}, ${notification.Type})">
            <div class="d-flex align-items-start">
                <div class="notification-icon ${color} me-3">
                    <i class="${icon}"></i>
                </div>
                <div class="flex-grow-1">
                    <p class="notification-text mb-1">${notification.Content}</p>
                    <small class="text-muted">
                        <i class="bi bi-clock me-1"></i>${notification.TimeAgo}
                    </small>
                </div>
                ${!notification.IsRead ? '<span class="notification-badge"></span>' : ''}
            </div>
        </div>
    `;
}

// Add notification to dropdown (for real-time)
function addNotificationToDropdown(notification) {
    const container = $('#notificationDropdown, #notificationList, .notification-list');
    
    if (container.length === 0) {
        return;
    }

    // Remove "no notifications" message if exists
    container.find('.text-muted').parent().remove();

    // Add new notification at the top
    const notificationHtml = createNotificationHTML(notification);
    container.prepend(notificationHtml);

    // Keep only last 10 notifications
    container.find('.notification-item').slice(10).remove();
}

// Get notification icon based on type
function getNotificationIcon(type) {
    const icons = {
        1: 'bi bi-chat-dots',           // Message
        2: 'bi bi-person-plus',         // NewClient
        3: 'bi bi-person-badge',        // NewCoach
        4: 'bi bi-clipboard-check',     // PlanAssigned
        5: 'bi bi-cash-coin',           // PaymentReceived
        6: 'bi bi-exclamation-triangle',// SubscriptionExpiring
        7: 'bi bi-trophy',              // WorkoutCompleted
        8: 'bi bi-star',                // FeedbackReceived
        9: 'bi bi-info-circle',         // SystemAlert
        10: 'bi bi-bell',               // General
        11: 'bi bi-journal-plus',       // DailyLogSubmitted
        12: 'bi bi-journal-check'       // DailyLogReviewed
    };
    return icons[type] || 'bi bi-bell';
}

// Get notification color based on type
function getNotificationColor(type) {
    const colors = {
        1: 'bg-primary',        // Message
        2: 'bg-success',        // NewClient
        3: 'bg-info',           // NewCoach
        4: 'bg-warning',        // PlanAssigned
        5: 'bg-success',        // PaymentReceived
        6: 'bg-danger',         // SubscriptionExpiring
        7: 'bg-success',        // WorkoutCompleted
        8: 'bg-warning',        // FeedbackReceived
        9: 'bg-danger',         // SystemAlert
        10: 'bg-secondary',     // General
        11: 'bg-info',          // DailyLogSubmitted
        12: 'bg-success'        // DailyLogReviewed
    };
    return colors[type] || 'bg-secondary';
}

// Update notification badge count
function updateNotificationBadge(count) {
    const badge = $('#notificationCount');
    
    if (count > 0) {
        badge.text(count > 99 ? '99+' : count).show();
    } else {
        badge.hide();
    }
}

// Update notification count (fetch from server)
function updateNotificationCount() {
    $.ajax({
        url: '/Notification/GetUnreadCount',
        method: 'GET',
        success: function (response) {
            if (response.success) {
                updateNotificationBadge(response.count);
            }
        },
        error: function (error) {
            console.error('Error getting notification count:', error);
        }
    });
}

// Handle notification click
function handleNotificationClick(notificationId, refId, type) {
    // Mark as read
    markNotificationAsRead(notificationId);
    
    // Navigate based on type
    navigateToNotificationTarget(refId, type);
}

// Mark notification as read
function markNotificationAsRead(notificationId) {
    $.ajax({
        url: '/Notification/MarkAsRead',
        method: 'POST',
        data: { id: notificationId },
        success: function (response) {
            if (response.success) {
                markNotificationAsReadInUI(notificationId);
                updateNotificationBadge(response.unreadCount);
            }
        },
        error: function (error) {
            console.error('Error marking notification as read:', error);
        }
    });
}

// Mark notification as read in UI
function markNotificationAsReadInUI(notificationId) {
    $(`.notification-item[data-id="${notificationId}"]`)
        .removeClass('unread')
        .addClass('read')
        .find('.notification-badge')
        .remove();
}

// Mark all notifications as read
function markAllNotificationsAsRead() {
    $.ajax({
        url: '/Notification/MarkAllAsRead',
        method: 'POST',
        success: function (response) {
            if (response.success) {
                markAllNotificationsAsReadInUI();
                updateNotificationBadge(0);
                
                Swal.fire({
                    icon: 'success',
                    title: 'All notifications marked as read',
                    timer: 1500,
                    showConfirmButton: false
                });
            }
        },
        error: function (error) {
            console.error('Error marking all notifications as read:', error);
        }
    });
}

// Mark all notifications as read in UI
function markAllNotificationsAsReadInUI() {
    $('.notification-item')
        .removeClass('unread')
        .addClass('read')
        .find('.notification-badge')
        .remove();
}

// Navigate to notification target
function navigateToNotificationTarget(refId, type) {
    switch (type) {
        case 1: // Message
            // Detect user role and navigate to appropriate chat
            if (window.location.pathname.includes('/Coach/') || window.location.pathname.includes('/CoachDashboard/')) {
                window.location.href = '/Chat/CoachChat';
            } else if (window.location.pathname.includes('/Client/') || window.location.pathname.includes('/ClientDashboard/')) {
                window.location.href = '/Chat/ClientChat';
            } else {
                // Default fallback - try to detect from current page
                window.location.href = '/Chat/ClientChat';
            }
            break;
        case 2: // NewClient
            window.location.href = '/Coach/MyClients';
            break;
        case 3: // NewCoach
            window.location.href = '/Admin/Coaches';
            break;
        case 4: // PlanAssigned
            window.location.href = '/Client/MyPlans';
            break;
        case 5: // PaymentReceived
            window.location.href = '/Admin/Payments';
            break;
        case 11: // DailyLogSubmitted
            window.location.href = '/DailyLog/CoachLogs';
            break;
        case 12: // DailyLogReviewed
            window.location.href = '/DailyLog/ClientLogs';
            break;
        default:
            console.log('No navigation defined for notification type:', type);
    }
}

// Show toast notification
function showNotificationToast(notification) {
    const icon = getNotificationIcon(notification.Type);
    
    // Using SweetAlert2 for toast
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 5000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer)
            toast.addEventListener('mouseleave', Swal.resumeTimer)
        }
    });

    Toast.fire({
        icon: 'info',
        title: notification.Content,
        html: `<small class="text-muted"><i class="${icon} me-1"></i>Just now</small>`
    });
}

// Initialize on document ready
$(document).ready(function () {
    // Initialize notifications
    initializeNotifications();
    
    // Refresh notifications every 30 seconds (fallback)
    setInterval(function () {
        if (!isNotificationConnected) {
            updateNotificationCount();
        }
    }, 30000);
    
    // Handle mark all as read button
    $(document).on('click', '#markAllAsRead, .mark-all-read', function (e) {
        e.preventDefault();
        markAllNotificationsAsRead();
    });
});

// ========================================
// Notification Styles (Add to your CSS)
// ========================================
/*
.notification-item {
    padding: 12px 16px;
    border-bottom: 1px solid #f0f0f0;
    cursor: pointer;
    transition: background-color 0.2s;
}

.notification-item:hover {
    background-color: #f8f9fa;
}

.notification-item.unread {
    background-color: #f0f7ff;
}

.notification-icon {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
}

.notification-text {
    font-size: 14px;
    color: #333;
    margin: 0;
}

.notification-badge {
    width: 8px;
    height: 8px;
    background-color: #ef4444;
    border-radius: 50%;
    display: inline-block;
}

.notification-badge-count {
    position: absolute;
    top: -5px;
    right: -5px;
    background-color: #ef4444;
    color: white;
    border-radius: 10px;
    padding: 2px 6px;
    font-size: 11px;
    font-weight: bold;
    min-width: 18px;
    text-align: center;
}
*/
