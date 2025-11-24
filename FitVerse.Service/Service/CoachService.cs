using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitVerse.Core.IService;
using FitVerse.Core.viewModels;
using FitVerse.Core.ViewModels.Client;
using FitVerse.Core.ViewModels.Package;
using FitVerse.Data.UnitOfWork;
using global::FitVerse.Core.IService;
using global::FitVerse.Core.UnitOfWork;
using global::FitVerse.Core.ViewModels.Coach;
using global::FitVerse.Data.Models;

namespace FitVerse.Data.Service
{
    namespace FitVerse.Data.Service
    {
        public class CoachService : ICoachService
        {
            private readonly IUnitOfWork unitOfWork;
            private readonly IMapper mapper;
            private readonly IImageHandleService imageService;

            public CoachService(IUnitOfWork unitOfWork, IMapper mapper, IImageHandleService imageService)
            {
                this.unitOfWork = unitOfWork;
                this.mapper = mapper;
                this.imageService = imageService;
            }

            public (bool Success, string Message) AddCoach(AddCoachVM model)
            {
                
                try
                {
                    string? imagePath = imageService.SaveImage(model.CoachImageFile);

                    var coach = mapper.Map<Coach>(model);
                    //coach.ImagePath = imagePath ?? "/Images/default.jpg";

                    unitOfWork.Coaches.Add(coach);
                    unitOfWork.Complete();

                    return (true, "Coach added successfully.");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
            public (bool Success, string Message) AddCoachByAdmin(Coach model)
            {

                try
                {

                    unitOfWork.Coaches.Add(model);
                    unitOfWork.Complete();

                    return (true, "Coach added successfully.");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }

            public List<AddCoachVM> GetAllCoaches()
            {
                // نجيب الكوتشات ومعاهم التخصصات والـ User
                var coaches = unitOfWork.Coaches.GetAll(includeProperties: "User,CoachSpecialties.Specialty").ToList();

                // نعمل المابينج يدوياً عشان نضمن إن كل الداتا راجعة
                var coachVMs = coaches.Select(coach => new AddCoachVM
                {
                    Id = coach.Id,
                    Name = coach.User?.FullName ?? "Unknown",
                    Title = coach.User?.UserName ?? "Coach",
                    About = coach.About ?? "No description available",
                    ExperienceYears = coach.ExperienceYears ?? 0,
                    ImagePath = coach.User?.ImagePath,
                    IsActive = coach.User?.Status?.ToLower() == "active",
                    UserId = coach.UserId,
                    Specialties = coach.CoachSpecialties?
                        .Select(cs => cs.Specialty?.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList() ?? new List<string>()
                }).ToList();

                return coachVMs;
            }


            public AddCoachVM GetCoachByIdGuid(string id)
            {
                var coach = unitOfWork.Coaches.GetCoachByIdGuid(id);
                return mapper.Map<AddCoachVM>(coach);
            }


            (bool Success, string Message) ICoachService.DeleteCoachById(string id)
            {
                try
                {
                    var coach = unitOfWork.Coaches.Find(c => c.Id == id).FirstOrDefault();
                    if (coach == null)
                        return (false, "Coach not found");

                    // Delete related data in correct order
                    // 1. Delete Chats and Messages
                    var chats = unitOfWork.Chats.Find(c => c.CoachId == coach.Id.ToString()).ToList();
                    foreach (var chat in chats)
                    {
                        var messages = unitOfWork.Messages.Find(m => m.ChatId == chat.Id).ToList();
                        unitOfWork.Messages.RemoveRange(messages);
                        unitOfWork.Chats.Delete(chat);
                    }

                    // 2. Delete Coach Feedbacks
                    var feedbacks = unitOfWork.CoachFeedbacks.Find(f => f.CoachId == coach.Id).ToList();
                    foreach (var feedback in feedbacks)
                    {
                        unitOfWork.CoachFeedbacks.Delete(feedback);
                    }

                    // 3. Delete Exercise Plans
                    var exercisePlans = unitOfWork.ExercisePlans.Find(e => e.CoachId == coach.Id).ToList();
                    foreach (var plan in exercisePlans)
                    {
                        unitOfWork.ExercisePlans.Delete(plan);
                    }

                    // 4. Delete Diet Plans
                    var dietPlans = unitOfWork.DietPlans.Find(d => d.CoachId == coach.Id).ToList();
                    foreach (var plan in dietPlans)
                    {
                        unitOfWork.DietPlans.Delete(plan);
                    }

                    // 5. Delete Coach Packages
                    var coachPackages = unitOfWork.coachPackageRepository.Find(cp => cp.CoachId == coach.Id).ToList();
                    foreach (var package in coachPackages)
                    {
                        unitOfWork.coachPackageRepository.Delete(package);
                    }

                    // 6. Delete Coach Specialties
                    var specialties = unitOfWork.CoachSpecialties.Find(cs => cs.CoachId == coach.Id).ToList();
                    foreach (var specialty in specialties)
                    {
                        unitOfWork.CoachSpecialties.Delete(specialty);
                    }

                    // 7. Delete Coach record
                    unitOfWork.Coaches.Delete(coach);
                    
                    // Save all deletions
                    unitOfWork.Complete();

                    return (true, "Coach and all related data deleted successfully");
                }
                catch (Exception ex)
                {
                    return (false, $"Error deleting coach: {ex.Message}");
                }
            }

            public (bool Success, string Message) UpdateCoach(AddCoachVM model)
            {
                try
                {
                    var existingCoach = unitOfWork.Coaches.GetCoachByIdGuid(model.Id);
                    if (existingCoach == null)
                        return (false, "Coach not found.");

                    if (model.CoachImageFile != null && model.CoachImageFile.Length > 0)
                    {
                        string? imagePath = imageService.SaveImage(model.CoachImageFile);
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            //existingCoach.ImagePath = imagePath;
                        }
                    }
                    //existingCoach.Name = model.Name;
                    existingCoach.ExperienceYears = model.ExperienceYears;
                    existingCoach.About = model.About;
                    //existingCoach.IsActive = model.IsActive;

                    unitOfWork.Coaches.Update(existingCoach);
                    unitOfWork.Complete();
                    return (true, "Coach updated successfully.");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }



            public (List<AddCoachVM> Data, int TotalItems) GetPagedEquipments(int page, int pageSize, string? search)
                    {
                        var query = unitOfWork.Coaches.GetAll().AsQueryable();

                        if (!string.IsNullOrEmpty(search))
                            query = query.Where(e => e.User.UserName.ToLower().Contains(search.ToLower()));

                        var totalItems = query.Count();

                        // استخدام ProjectTo لتحويل الـ IQueryable مباشرة إلى الـ VM
                        var data = query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ProjectTo<AddCoachVM>(mapper.ConfigurationProvider)
                            .ToList();

                        return (data, totalItems);
                    }

            public CoachDashboardViewModel GetDashboardData(string coachId)
            {
                var dashboard = new CoachDashboardViewModel
                {
                    ActiveClientsCount = unitOfWork.Coaches.GetActiveClientsCount(),
                    TotalPlans = unitOfWork.Coaches.GetTotalPlans(coachId),
                    TotalExercises = unitOfWork.Coaches.GetTotalExercises(),
                    AverageRating = unitOfWork.Coaches.GetAverageRating(coachId),
                    RecentClients = unitOfWork.Coaches
                    .GetRecentClients(coachId)
                    .Select(c => mapper.Map <ClientDashVM >(c))
                    .ToList()
                };

                return dashboard;
            }

            public List<CoachWithPackagesVM> GetAllCoachesWithPackages()
            {
                var coaches = unitOfWork.Coaches.GetAll().ToList();

                var coachVMs = coaches.Select(c => new CoachWithPackagesVM
                {
                    Id = c.Id,
                    Name = c.User?.FullName ?? "Unknown",
                    ExperienceYears = c.ExperienceYears ?? 0,
                    Packages = c.CoachPackages.Select(cp => new PackagesVM
                    {
                        Id = cp.Package.Id,
                        Name = cp.Package.Name
                    }).ToList()
                }).ToList();

                return coachVMs;
            }

            public List<PackageVM> GetPackagesByCoachId(string coachId)
            {
                try
                {
                    if (string.IsNullOrEmpty(coachId))
                    {
                        return new List<PackageVM>();
                    }

                    var packages = unitOfWork.Packages
                        .GetAll(filter: p => p.CoachPackages.Any(cp => cp.CoachId == coachId), 
                               includeProperties: "CoachPackages,CoachPackages.Coach")
                        .ToList();

                    return mapper.Map<List<PackageVM>>(packages);
                }
                catch (Exception ex)
                {
                    // Log the exception if logger is available
                    return new List<PackageVM>();
                }
            }


            // يرجع كل العملاء للكوتش
            public List<ClientDashVM> GetClientsByCoachId(string coachId)
            {
                var clients = unitOfWork.Clients
                    .GetAll(filter: c => c.ClientSubscriptions.Any(cs => cs.CoachId == coachId),
                           includeProperties: "User")
                    .ToList();

                return mapper.Map<List<ClientDashVM>>(clients);
            }

            // تمارين اليوم للكوتش (لجميع عملائه اليوم)
            public List<Exercise> GetTodayExercisesByClient(string coachId)
            {
                var today = DateTime.UtcNow.Date;

                var exercises = unitOfWork.ExercisePlanDetails
                    .Find(epd => epd.ExercisePlan.CoachId == coachId
                                && epd.ExercisePlan.Date.Date == today)
                    .Select(epd => epd.Exercise)
                    .ToList();

                return exercises;
            }


            public Coach GetCoachByClientId(string clientId)
            {
                var client = unitOfWork.Clients
                    .GetAll(filter: c => c.Id == clientId, 
                           includeProperties: "ClientSubscriptions")
                    .FirstOrDefault();
                if (client == null) return null;

                var coachId = client.ClientSubscriptions.FirstOrDefault()?.CoachId;
                if (coachId == null) return null;

                return unitOfWork.Coaches
                    .GetAll(filter: c => c.Id == coachId, 
                           includeProperties: "User")
                    .FirstOrDefault();
            }

            public async Task<bool> UpdateCoachProfessionalInfo(string userName, int? experienceYears, decimal? salary, string about, string certificates, string specialties)
            {
                try
                {
                    // Get coach by username
                    var coach = unitOfWork.Coaches
                        .GetAll(filter: c => c.User.UserName == userName, includeProperties: "User")
                        .FirstOrDefault();

                    if (coach == null)
                        return false;

                    // Update coach properties
                    if (experienceYears.HasValue)
                        coach.ExperienceYears = experienceYears.Value;

                    if (salary.HasValue)
                        coach.Salary = salary.Value;

                    if (!string.IsNullOrWhiteSpace(about))
                        coach.About = about;

                    if (!string.IsNullOrWhiteSpace(certificates))
                        coach.Certificates = certificates;

                    // Update specialties if needed
                    // Note: You may need to update Specialties table separately if it's a many-to-many relationship
                    // For now, we're just updating the coach record

                    unitOfWork.Coaches.Update(coach);
                    unitOfWork.Complete();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

        }
    }
}

