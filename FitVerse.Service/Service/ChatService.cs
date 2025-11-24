using FitVerse.Core.IService;
using FitVerse.Core.UnitOfWork;
using FitVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Service.Service
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Chat> GetChatBetweenUsersAsync(string clientId, string coachId)
        {
            return await _unitOfWork.Chats.GetQueryable()
                .Where(c => (c.ClientId == clientId && c.CoachId == coachId) ||
                           (c.ClientId == coachId && c.CoachId == clientId))
                .Include(c => c.Messages)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Chat>> GetUserChatsAsync(string userId)
        {
            return await _unitOfWork.Chats.GetQueryable()
                .Where(c => c.Client.UserId == userId || c.Coach.UserId == userId)
                .Include(c => c.Messages)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl.User)
                .Include(c => c.Coach)
                    .ThenInclude(co => co.User)
                .OrderByDescending(c => c.Messages.Any() ? c.Messages.Max(m => m.SentAt) : DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<Chat> CreateChatAsync(string clientId, string coachId)
        {
            var existingChat = await GetChatBetweenUsersAsync(clientId, coachId);
            if (existingChat != null)
                return existingChat;

            var newChat = new Chat
            {
                ClientId = clientId,
                CoachId = coachId
            };

            _unitOfWork.Chats.Add(newChat);
            await _unitOfWork.SaveAsync();
            return newChat;
        }
    }
}
