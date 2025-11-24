using FitVerse.Core.UnitOfWork;
using FitVerse.Core.ViewModels.Client;
using FitVerse.Core.ViewModels.Coach;
using FitVerse.Core.ViewModels.Package;
using FitVerse.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitVerse.Core.IService
{
    public interface ICoachService : IService
    {
        (bool Success, string Message) AddCoach(AddCoachVM model); //برتجع تابل tuble    }
        (bool Success, string Message) AddCoachByAdmin(Coach model); 
        List<AddCoachVM> GetAllCoaches();
        AddCoachVM GetCoachByIdGuid(string id);
        (bool Success, string Message) DeleteCoachById(string id);
        (bool Success, string Message) UpdateCoach(AddCoachVM model);
        (List<AddCoachVM> Data, int TotalItems) GetPagedEquipments(int page, int pageSize, string? search);
        CoachDashboardViewModel GetDashboardData(string coachId);
        List<PackageVM> GetPackagesByCoachId(string coachId);

        List<CoachWithPackagesVM> GetAllCoachesWithPackages();




        public List<Exercise> GetTodayExercisesByClient(string coachId);
        public List<ClientDashVM> GetClientsByCoachId(string coachId);
        public Coach GetCoachByClientId(string clientId);
        Task<bool> UpdateCoachProfessionalInfo(string userName, int? experienceYears, decimal? salary, string about, string certificates, string specialties);

    }
}
