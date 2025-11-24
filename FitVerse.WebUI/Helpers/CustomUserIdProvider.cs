using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FitVerse.Web.Helpers
{
    /// <summary>
    /// Custom UserIdProvider for SignalR to map connections to authenticated users
    /// This is CRITICAL for targeted notifications using Clients.User(userId)
    /// </summary>
    public class CustomUserIdProvider : IUserIdProvider
    {
        /// <summary>
        /// Gets the user identifier from the connection context
        /// Uses ClaimTypes.NameIdentifier which contains the AspNetUsers.Id
        /// </summary>
        public string? GetUserId(HubConnectionContext connection)
        {
            // Extract the user ID from the NameIdentifier claim
            var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Log for debugging
            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"[SignalR] User connected with ID: {userId}");
            }
            else
            {
                Console.WriteLine($"[SignalR] WARNING: User connected but no NameIdentifier claim found!");
            }
            
            return userId;
        }
    }
}
