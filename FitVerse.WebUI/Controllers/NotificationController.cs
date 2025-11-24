using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FitVerse.Core.IService;
using System.Security.Claims;

namespace FitVerse.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get all notifications for the current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);

                var notificationData = notifications.Select(n => new
                {
                    Id = n.Id,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    RefId = n.RefId,
                    Type = (int)n.Type,
                    IsRead = n.IsRead,
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                }).ToList();

                return Json(new { success = true, data = notificationData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get unread notifications count
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var count = await _notificationService.GetUnreadCountAsync(userId);

                return Json(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get recent notifications (last 10)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecent(int count = 10)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var notifications = await _notificationService.GetRecentNotificationsAsync(userId, count);

                var notificationData = notifications.Select(n => new
                {
                    Id = n.Id,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    RefId = n.RefId,
                    Type = (int)n.Type,
                    IsRead = n.IsRead,
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                }).ToList();

                return Json(new { success = true, data = notificationData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _notificationService.MarkAsReadAsync(id, userId);

                if (success)
                {
                    var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                    return Json(new { success = true, unreadCount = unreadCount });
                }

                return Json(new { success = false, message = "Notification not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _notificationService.MarkAllAsReadAsync(userId);

                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _notificationService.DeleteAsync(id);

                if (success)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                    return Json(new { success = true, unreadCount = unreadCount });
                }

                return Json(new { success = false, message = "Notification not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Helper method to get time ago string
        /// </summary>
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)}mo ago";
            
            return $"{(int)(timeSpan.TotalDays / 365)}y ago";
        }
    }
}
