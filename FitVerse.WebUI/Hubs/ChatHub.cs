using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using FitVerse.Core.IService;
using FitVerse.Data.Models;
using Microsoft.AspNetCore.Identity;
using FitVerse.Core.Models;

namespace FitVerse.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly INotificationService _notificationService;
        private readonly FitVerse.Web.Helpers.NotificationHelper _notificationHelper;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly Dictionary<string, string> _userConnections = new(); // userId → connectionId

        public ChatHub(IChatService chatService, IMessageService messageService, INotificationService notificationService, FitVerse.Web.Helpers.NotificationHelper notificationHelper, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _messageService = messageService;
            _notificationService = notificationService;
            _notificationHelper = notificationHelper;
            _userManager = userManager;
        }

        public async Task SendMessage(string chatId, string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderId))
                return;

            // إنشاء الرسالة وحفظها
            var newMessage = new Message
            {
                ChatId = int.Parse(chatId),
                SenderId = senderId,
                ReciverId = receiverId,
                Content = message,
                SentAt = DateTime.Now,
                IsRead = false
            };

            await _messageService.CreateAsync(newMessage);

            // Send notification to receiver
            try
            {
                var sender = await _userManager.FindByIdAsync(senderId);
                var senderName = sender?.FullName ?? sender?.UserName ?? "Someone";
                
                Console.WriteLine($"[ChatHub] Sending notification to {receiverId} from {senderName}");
                
                await _notificationHelper.NotifyMessageReceivedAsync(
                    receiverId,
                    senderName,
                    newMessage.Id
                );
                
                Console.WriteLine($"[ChatHub] Notification sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] Error sending notification: {ex.Message}");
            }

            var messageData = new
            {
                Id = newMessage.Id,
                ChatId = chatId,
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                SentAt = newMessage.SentAt.ToString("HH:mm"),
                IsRead = false
            };

            // إرسال الرسالة للمستقبل لو متصل
            if (_userConnections.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.User(receiverId).SendAsync("ReceiveMessage", messageData);
            }

            // إرسال نسخة للمرسل لتحديث الـ UI
            if (_userConnections.TryGetValue(senderId, out var senderConnectionId))
            {
                await Clients.User(senderId).SendAsync("ReceiveMessage", messageData);
            }
        }

        public async Task MarkMessageAsRead(string messageId)
        {
            var userId = Context.UserIdentifier;
            var message = await _messageService.GetByIdAsync(int.Parse(messageId));

            if (message != null && message.ReciverId == userId)
            {
                message.IsRead = true;
                await _messageService.UpdateAsync(message);

                if (_userConnections.TryGetValue(message.SenderId, out var senderConnectionId))
                {
                    await Clients.Client(senderConnectionId).SendAsync("MessageRead", messageId);
                }
            }
        }

        public async Task MarkChatAsRead(string chatId, string userId)
        {
            try
            {
                Console.WriteLine($"[ChatHub] MarkChatAsRead called - ChatId: {chatId}, UserId: {userId}");
                
                // Mark all messages in this chat as read for this user
                await _messageService.MarkMessagesAsReadAsync(int.Parse(chatId), userId);
                
                // Notify the current user to update their UI
                await Clients.User(userId).SendAsync("ChatMarkedAsRead", chatId);
                
                Console.WriteLine($"[ChatHub] Chat {chatId} marked as read for user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] Error in MarkChatAsRead: {ex.Message}");
            }
        }

        public async Task UserTyping(string receiverId)
        {
            var userId = Context.UserIdentifier;
            if (_userConnections.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("UserTyping", userId);
            }
        }

        public async Task UserStoppedTyping(string receiverId)
        {
            var userId = Context.UserIdentifier;
            if (_userConnections.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("UserStoppedTyping", userId);
            }
        }

        public async Task SendNotification(string receiverId, string content, int refId, int notificationType)
        {
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderId))
                return;

            // إنشاء الإشعار وحفظه
            var notification = new Notification
            {
                ReciverId = receiverId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                RefId = refId,
                Type = (FitVerse.Core.Enums.NotificationType)notificationType,
                IsRead = false
            };

            await _notificationService.CreateAsync(notification);

            var notificationData = new
            {
                Id = notification.Id,
                Content = content,
                CreatedAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                RefId = refId,
                Type = notificationType,
                IsRead = false
            };

            // إرسال الإشعار للمستقبل لو متصل
            if (_userConnections.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.User(receiverId).SendAsync("ReceiveNotification", notificationData);
                
                // تحديث عداد الإشعارات
                var unreadCount = await _notificationService.GetUnreadCountAsync(receiverId);
                await Clients.User(receiverId).SendAsync("UpdateNotificationCount", unreadCount);
            }
        }

        public async Task MarkNotificationAsRead(string notificationId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                return;

            var success = await _notificationService.MarkAsReadAsync(int.Parse(notificationId), userId);
            if (success)
            {
                // تحديث عداد الإشعارات
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                await Clients.User(userId).SendAsync("UpdateNotificationCount", unreadCount);
                await Clients.User(userId).SendAsync("NotificationMarkedAsRead", notificationId);
            }
        }

        public async Task MarkAllNotificationsAsRead()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                return;

            var success = await _notificationService.MarkAllAsReadAsync(userId);
            if (success)
            {
                await Clients.User(userId).SendAsync("UpdateNotificationCount", 0);
                await Clients.User(userId).SendAsync("AllNotificationsMarkedAsRead");
            }
        }

        public async Task GetNotifications()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                return;

            var notifications = await _notificationService.GetRecentNotificationsAsync(userId, 10);
            var notificationData = notifications.Select(n => new
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                RefId = n.RefId,
                Type = (int)n.Type,
                IsRead = n.IsRead
            }).ToList();

            await Clients.User(userId).SendAsync("LoadNotifications", notificationData);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
                await Clients.All.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }

}
