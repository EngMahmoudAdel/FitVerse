using FitVerse.Core.IService;
using FitVerse.Core.UnitOfWork;
using FitVerse.Core.ViewModels.Coach;
using FitVerse.Core.ViewModels.Meuscle;
using FitVerse.Data.Models;
using FitVerse.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Service.Service
{
    public class ClientOnCoachesService : IClientOnCoachesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientOnCoachesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<ClientsVM> GetAllClients()
        {
          var clients = _unitOfWork.Clients.GetAll();

          return clients.Select(m => new ClientsVM
        {
        Id = m.Id,
        Name = m.User?.UserName ?? "Unknown",
        Age = 0,
        Height = m.Height ?? 0,
        StartWeight = m.StartWeight ?? 0,
        Goal = m.Goal ?? "Not specified",
        Gender = "Not specified",
        Image = "/images/default-user.jpg",
        JoinDate = DateTime.Now,
        IsActive = true,
        TotalWorkouts = m.ExercisePlans.Count,
        ProgressPercentage = 75,
    }).ToList();
        }

        public List<ClientsVM> GetClientsByCoachId(string coachId)
        {
            // Get all clients assigned to this coach through ClientSubscriptions
            var coach = _unitOfWork.Coaches
                .GetAll()
                .AsQueryable()
                .Include(c => c.ClientSubscriptions)
                    .ThenInclude(cs => cs.Client)
                        .ThenInclude(cl => cl.User)
                .Include(c => c.ClientSubscriptions)
                    .ThenInclude(cs => cs.Client)
                        .ThenInclude(cl => cl.ExercisePlans)
                .Include(c => c.ClientSubscriptions)
                    .ThenInclude(cs => cs.Client)
                        .ThenInclude(cl => cl.DietPlans)
                .FirstOrDefault(c => c.UserId == coachId);

            if (coach == null || coach.ClientSubscriptions == null)
            {
                return new List<ClientsVM>();
            }

            var clientsOnCoach = coach.ClientSubscriptions
                .Select(cs => cs.Client)
                .Where(c => c != null)
                .Distinct()
                .ToList();

            return clientsOnCoach.Select(client => new ClientsVM
            {
                Id = client.Id,
                Name = client.User?.FullName ?? client.User?.UserName ?? "Unknown",
                Image = client.User?.ImagePath ?? "/images/default-user.jpg",
                Age = client.User?.Age ?? 0,
                Height = client.Height ?? 0,
                StartWeight = client.StartWeight ?? 0,
                Goal = client.Goal ?? "Not specified",
                Gender = client.User?.Gender?.ToString() ?? "Not specified",
                JoinDate = DateTime.Now, // Client model doesn't have CreatedDate
                IsActive = client.User?.Status?.ToLower() == "active",
                TotalWorkouts = client.ExercisePlans?.Count ?? 0,
                ProgressPercentage = CalculateClientProgress(client),
                SubscriptionName = GetClientSubscriptionName(client)
            }).ToList();
        }

        private int CalculateClientProgress(Client client)
        {
            // Calculate progress based on total workouts
            var totalPlans = (client.ExercisePlans?.Count ?? 0) + (client.DietPlans?.Count ?? 0);
            if (totalPlans == 0) return 0;
            
            // Simple progress calculation - returns percentage based on number of plans
            // Can be enhanced later with actual completion tracking
            return Math.Min(totalPlans * 15, 100); // Each plan contributes 15% progress
        }

        private string GetClientSubscriptionName(Client client)
        {
            // Get the active subscription for the client
            var activeSubscription = _unitOfWork.Coaches
                .GetAll()
                .SelectMany(c => c.ClientSubscriptions)
                .Where(cs => cs.ClientId == client.Id && cs.Status.ToLower() == "active")
                .OrderByDescending(cs => cs.StartDate)
                .FirstOrDefault();

            if (activeSubscription?.Package != null)
            {
                return activeSubscription.Package.Name;
            }

            return "No Active Plan";
        }
    }
}
