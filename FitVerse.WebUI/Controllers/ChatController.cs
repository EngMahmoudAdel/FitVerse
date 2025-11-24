using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FitVerse.Core.Models;
using FitVerse.Core.IService;
using FitVerse.Data.Models;
using FitVerse.Core.Helpers;
using FitVerse.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitVerse.Web.Controllers
{
    public class CreateChatRequest
    {
        public string OtherUserId { get; set; }
    }

    public class SendMessageRequest
    {
        public int ChatId { get; set; }
        public string Content { get; set; }
    }

    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICoachService _coachService;
        private readonly IClientService _clientService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FitVerse.Web.Helpers.NotificationHelper _notificationHelper;

        public ChatController(IChatService chatService, IMessageService messageService, UserManager<ApplicationUser> userManager, ICoachService coachService, IClientService clientService, IUnitOfWork unitOfWork, FitVerse.Web.Helpers.NotificationHelper notificationHelper)
        {
            _chatService = chatService;
            _messageService = messageService;
            _userManager = userManager;
            _coachService = coachService;
            _clientService = clientService;
            _unitOfWork = unitOfWork;
            _notificationHelper = notificationHelper;
        }

        public async Task<IActionResult> ClientChat()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chats = await _chatService.GetUserChatsAsync(userId);
            
            // Get all coaches for client to start new chats - we need actual Coach entities with UserId
            var coachEntities = await GetAllCoachEntitiesAsync();
            var coaches = coachEntities;
            
            ViewBag.UserId = userId;
            ViewBag.Chats = chats;
            ViewBag.AvailableCoaches = coaches;
            return View();
        }

        public async Task<IActionResult> CoachChat()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chats = await _chatService.GetUserChatsAsync(userId);
            
            // Get all clients for coach to start new chats - we need actual Client entities with UserId
            // Since ClientDashVM doesn't have UserId, let's get the actual Client entities
            var clientEntities = await GetAllClientEntitiesAsync();
            var clients = clientEntities;
            
            ViewBag.UserId = userId;
            ViewBag.Chats = chats;
            ViewBag.AvailableClients = clients;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                
                // Debug logging
                Console.WriteLine($"Current User ID: {currentUserId}");
                Console.WriteLine($"Other User ID: {request?.OtherUserId}");
                
                if (request == null || string.IsNullOrEmpty(request.OtherUserId))
                {
                    return Json(new { success = false, message = "Invalid request: OtherUserId is required" });
                }
                
                var otherUser = await _userManager.FindByIdAsync(request.OtherUserId);
                
                if (otherUser == null)
                {
                    return Json(new { success = false, message = $"User not found with ID: {request.OtherUserId}" });
                }

                // Determine who is coach and who is client, and get their entity IDs
                string clientEntityId, coachEntityId;
                if (await _userManager.IsInRoleAsync(currentUser, RoleConstants.Coach))
                {
                    // Current user is coach, other user is client
                    var coachEntity = await _unitOfWork.Coaches.GetQueryable()
                        .FirstOrDefaultAsync(c => c.UserId == currentUserId);
                    var clientEntity = await _unitOfWork.Clients.GetQueryable()
                        .FirstOrDefaultAsync(c => c.UserId == request.OtherUserId);

                    if (coachEntity == null || clientEntity == null)
                    {
                        return Json(new { success = false, message = "Coach or Client entity not found" });
                    }

                    coachEntityId = coachEntity.Id;
                    clientEntityId = clientEntity.Id;
                }
                else
                {
                    // Current user is client, other user is coach
                    var clientEntity = await _unitOfWork.Clients.GetQueryable()
                        .FirstOrDefaultAsync(c => c.UserId == currentUserId);
                    var coachEntity = await _unitOfWork.Coaches.GetQueryable()
                        .FirstOrDefaultAsync(c => c.UserId == request.OtherUserId);

                    if (coachEntity == null || clientEntity == null)
                    {
                        return Json(new { success = false, message = "Coach or Client entity not found" });
                    }

                    clientEntityId = clientEntity.Id;
                    coachEntityId = coachEntity.Id;
                }

                var chat = await _chatService.CreateChatAsync(clientEntityId, coachEntityId);
                
                return Json(new { 
                    success = true, 
                    chatId = chat.Id, 
                    otherUserName = otherUser.UserName 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChatMessages(int chatId)
        {
            var messages = await _messageService.GetChatMessagesAsync(chatId);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var messageData = messages.Select(m => new
            {
                Id = m.Id,
                Content = m.Content,
                SentAt = m.SentAt.ToString("HH:mm"),
                SenderId = m.SenderId,
                SenderName = m.Sender?.UserName ?? "Unknown",
                IsCurrentUser = m.SenderId == currentUserId,
                IsRead = m.IsRead
            });

            return Json(messageData);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (request == null || request.ChatId <= 0 || string.IsNullOrWhiteSpace(request.Content))
                {
                    return Json(new { success = false, message = "Invalid message data" });
                }

                // Get the chat to determine the receiver
                var chat = await _unitOfWork.Chats.GetQueryable()
                    .Where(c => c.Id == request.ChatId)
                    .FirstOrDefaultAsync();

                if (chat == null)
                {
                    return Json(new { success = false, message = "Chat not found" });
                }

                // Determine receiver ID
                var receiverId = chat.ClientId == senderId ? chat.CoachId : chat.ClientId;

                var message = new Message
                {
                    ChatId = request.ChatId,
                    SenderId = senderId,
                    ReciverId = receiverId,
                    Content = request.Content,
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                await _messageService.CreateAsync(message);

                // Get sender info for response
                var sender = await _userManager.FindByIdAsync(senderId);

                // Send notification to receiver
                try
                {
                    var senderName = sender?.FullName ?? sender?.UserName ?? "Someone";
                    await _notificationHelper.NotifyMessageReceivedAsync(
                        receiverId,
                        senderName,
                        message.Id
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ChatController] Error sending message notification: {ex.Message}");
                }

                return Json(new
                {
                    success = true,
                    message = new
                    {
                        Id = message.Id,
                        Content = message.Content,
                        SentAt = message.SentAt.ToString("HH:mm"),
                        SenderId = message.SenderId,
                        SenderName = sender?.UserName ?? "Unknown",
                        IsCurrentUser = true,
                        IsRead = message.IsRead
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int chatId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get unread count before marking as read
            var chat = await _unitOfWork.Chats.GetQueryable()
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == chatId);
            
            if (chat == null)
            {
                return Json(new { success = false, message = "Chat not found" });
            }
            
            var unreadCount = chat.Messages?.Count(m => m.ReciverId == userId && !m.IsRead) ?? 0;
            
            // Mark messages as read
            await _messageService.MarkMessagesAsReadAsync(chatId, userId);
            
            return Json(new { 
                success = true, 
                unreadCount = 0,  // After marking as read, unread count is 0
                previousUnreadCount = unreadCount 
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chats = await _chatService.GetUserChatsAsync(userId);
            
            var chatData = new List<object>();
            
            foreach (var chat in chats)
            {
                ApplicationUser otherUser;
                if (chat.Client?.UserId == userId)
                {
                    otherUser = chat.Coach?.User;
                }
                else
                {
                    otherUser = chat.Client?.User;
                }
                
                var latestMessage = await _messageService.GetLatestMessageInChatAsync(chat.Id);
                var unreadCount = chat.Messages?.Count(m => m.ReciverId == userId && !m.IsRead) ?? 0;

                string otherUserAvatar;
                if (!string.IsNullOrEmpty(otherUser?.ImagePath))
                {
                    var imgPath = otherUser.ImagePath;
                    if (imgPath.StartsWith("/"))
                    {
                        otherUserAvatar = imgPath;
                    }
                    else
                    {
                        otherUserAvatar = $"/profile-image/{imgPath}";
                    }
                }
                else
                {
                    var nameForAvatar = otherUser?.FullName ?? otherUser?.UserName ?? "User";
                    otherUserAvatar = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(nameForAvatar)}&background=10b981&color=fff";
                }

                chatData.Add(new
                {
                    Id = chat.Id,
                    OtherUserId = otherUser?.Id,
                    OtherUserName = otherUser?.FullName ?? "Unknown",
                    OtherUserAvatar = otherUserAvatar,
                    LatestMessage = latestMessage?.Content ?? "No messages yet",
                    LatestMessageTime = latestMessage?.SentAt.ToString("HH:mm") ?? "",
                    UnreadCount = unreadCount,
                    IsOnline = false // You can implement online status tracking
                });
            }
            
            return Json(chatData);
        }

        private async Task<IEnumerable<Coach>> GetAllCoachEntitiesAsync()
        {
            return await _unitOfWork.Coaches.GetQueryable()
                .Where(c => c.User.Status=="Active" && c.UserId != null)
                .Include(c => c.User)
                .ToListAsync();
        }

        private async Task<IEnumerable<Client>> GetAllClientEntitiesAsync()
        {
            return await _unitOfWork.Clients.GetQueryable()
                .Where(c => c.User.Status == "Active" && c.UserId != null)
                .Include(c => c.User)
                .ToListAsync();
        }
    }
}
