﻿using AutoMapper;
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
                    coach.UserId ="6A29B02B-7643-48C3-9B47-6ECF12F4B9F9";
                    coach.ImagePath = imagePath ?? "/Images/default.jpg";

                    unitOfWork.Coaches.Add(coach);
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
                // نجيب الكوتشات ومعاهم التخصصات
                var coaches = unitOfWork.Coaches.GetAll(includeProperties: "CoachSpecialties.Specialty");

                // نعمل المابينج
                var coachVMs = mapper.Map<List<AddCoachVM>>(coaches);

                // نضيف التخصصات جوه الـ ViewModel
                foreach (var coachVm in coachVMs)
                {
                    var coachEntity = coaches.First(c => c.Id == coachVm.Id);
                    coachVm.Specialties = coachEntity.CoachSpecialties
                        .Select(cs => cs.Specialty?.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();
                }

                return coachVMs;
            }


            public AddCoachVM GetCoachByIdGuid(string id)
            {
                var coach = unitOfWork.Coaches.GetCoachByIdGuid(id);
                return mapper.Map<AddCoachVM>(coach);
            }



            (bool Success, string Message) ICoachService.DeleteCoachById(string id)
            {
                var coaches = unitOfWork.Coaches.DeleteCoachById(id);

                return coaches;
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
                            existingCoach.ImagePath = imagePath;
                        }
                    }
                    existingCoach.Name = model.Name;
                    existingCoach.ExperienceYears = model.ExperienceYears;
                    existingCoach.About = model.About;
                    existingCoach.IsActive = model.IsActive;

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
                query = query.Where(e => e.Name.ToLower().Contains(search.ToLower()));

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
            public List<PackageVM> GetPackagesByCoachId(string coachId)
            {
                var packages = unitOfWork.Packages
                    .GetAll(p => p.CoachPackages.Any(cp => cp.CoachId == coachId), includeProperties: "CoachPackages");

                
                return mapper.Map<List<PackageVM>>(packages);
            }





        }
}
}

