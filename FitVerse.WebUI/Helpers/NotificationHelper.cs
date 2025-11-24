using FitVerse.Core.Enums;
using FitVerse.Core.IService;
using FitVerse.Data.Models;
using Microsoft.AspNetCore.SignalR;
using FitVerse.Web.Hubs;

namespace FitVerse.Web.Helpers
{
    /// <summary>
    /// Helper class for creating and sending notifications
    /// Centralizes notification logic and SignalR delivery
    /// </summary>
    public class NotificationHelper
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<ChatHub> _hubContext;

        public NotificationHelper(INotificationService notificationService, IHubContext<ChatHub> hubContext)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Create and send a notification
        /// </summary>
        private async Task<Notification> CreateAndSendNotificationAsync(
            string receiverId, 
            string content, 
            NotificationType type, 
            int refId = 0)
        {
            Console.WriteLine($"[NotificationHelper] Creating notification for user: {receiverId}, Type: {type}, Content: {content}");
            
            // Create notification
            var notification = new Notification
            {
                ReciverId = receiverId,
                Content = content,
                Type = type,
                RefId = refId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationService.CreateAsync(notification);
            Console.WriteLine($"[NotificationHelper] Notification created in DB with ID: {notification.Id}");

            // Send via SignalR for real-time delivery
            try
            {
                var notificationData = new
                {
                    Id = notification.Id,
                    Content = notification.Content,
                    CreatedAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    RefId = notification.RefId,
                    Type = (int)notification.Type,
                    IsRead = false,
                    TimeAgo = "Just now"
                };

                Console.WriteLine($"[NotificationHelper] Sending notification via SignalR to user: {receiverId}");
                await _hubContext.Clients.User(receiverId).SendAsync("ReceiveNotification", notificationData);
                
                // Update notification count
                var unreadCount = await _notificationService.GetUnreadCountAsync(receiverId);
                await _hubContext.Clients.User(receiverId).SendAsync("UpdateNotificationCount", unreadCount);
                Console.WriteLine($"[NotificationHelper] Notification sent successfully. Unread count: {unreadCount}");
            }
            catch (Exception ex)
            {
                // Log error but don't fail the operation
                Console.WriteLine($"[NotificationHelper] ERROR sending notification via SignalR: {ex.Message}");
                Console.WriteLine($"[NotificationHelper] Stack trace: {ex.StackTrace}");
            }

            return notification;
        }

        /// <summary>
        /// Send notification when a message is received
        /// </summary>
        public async Task NotifyMessageReceivedAsync(string receiverUserId, string senderName, int messageId)
        {
            await CreateAndSendNotificationAsync(
                receiverUserId,
                $"New message from {senderName}",
                NotificationType.Message,
                messageId
            );
        }

        /// <summary>
        /// Send notification when new client is assigned to coach
        /// </summary>
        public async Task NotifyCoachNewClientAsync(string coachUserId, string clientName, int clientId)
        {
            await CreateAndSendNotificationAsync(
                coachUserId,
                $"New client assigned: {clientName}",
                NotificationType.NewClient,
                clientId
            );
        }

        /// <summary>
        /// Send notification when plan is assigned to client
        /// </summary>
        public async Task NotifyPlanAssignedAsync(string clientUserId, string planType, int planId)
        {
            await CreateAndSendNotificationAsync(
                clientUserId,
                $"New {planType} plan assigned to you",
                NotificationType.PlanAssigned,
                planId
            );
        }

        /// <summary>
        /// Send notification when payment is received
        /// </summary>
        public async Task NotifyPaymentReceivedAsync(string userId, decimal amount, int paymentId)
        {
            await CreateAndSendNotificationAsync(
                userId,
                $"Payment of ${amount:F2} received successfully",
                NotificationType.PaymentReceived,
                paymentId
            );
        }

        /// <summary>
        /// Send notification when subscription is expiring
        /// </summary>
        public async Task NotifySubscriptionExpiringAsync(string userId, int daysLeft, int subscriptionId)
        {
            await CreateAndSendNotificationAsync(
                userId,
                $"Your subscription expires in {daysLeft} days",
                NotificationType.SubscriptionExpiring,
                subscriptionId
            );
        }

        /// <summary>
        /// Send notification when workout is completed
        /// </summary>
        public async Task NotifyWorkoutCompletedAsync(string coachUserId, string clientName, int workoutId)
        {
            await CreateAndSendNotificationAsync(
                coachUserId,
                $"{clientName} completed a workout",
                NotificationType.WorkoutCompleted,
                workoutId
            );
        }

        /// <summary>
        /// Send notification when feedback is received
        /// </summary>
        public async Task NotifyFeedbackReceivedAsync(string userId, string fromName, int feedbackId)
        {
            await CreateAndSendNotificationAsync(
                userId,
                $"New feedback from {fromName}",
                NotificationType.FeedbackReceived,
                feedbackId
            );
        }

        /// <summary>
        /// Send system alert notification
        /// </summary>
        public async Task NotifySystemAlertAsync(string userId, string alertMessage, int refId = 0)
        {
            await CreateAndSendNotificationAsync(
                userId,
                alertMessage,
                NotificationType.SystemAlert,
                refId
            );
        }

        /// <summary>
        /// Send general notification
        /// </summary>
        public async Task NotifyGeneralAsync(string userId, string message, int refId = 0)
        {
            await CreateAndSendNotificationAsync(
                userId,
                message,
                NotificationType.General,
                refId
            );
        }

        /// <summary>
        /// Send notification when client submits a daily log
        /// </summary>
        public async Task NotifyDailyLogSubmittedAsync(string coachUserId, string clientName, int dailyLogId)
        {
            await CreateAndSendNotificationAsync(
                coachUserId,
                $"New Daily Log submitted by {clientName}",
                NotificationType.DailyLogSubmitted,
                dailyLogId
            );
        }

        /// <summary>
        /// Send notification when coach reviews/evaluates a daily log
        /// </summary>
        public async Task NotifyDailyLogReviewedAsync(string clientUserId, string coachName, int dailyLogId)
        {
            await CreateAndSendNotificationAsync(
                clientUserId,
                $"Your Daily Log has been reviewed by {coachName}",
                NotificationType.DailyLogReviewed,
                dailyLogId
            );
        }
    }
}
