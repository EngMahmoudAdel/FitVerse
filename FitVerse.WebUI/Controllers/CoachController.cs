using AutoMapper;
using FitVerse.Core.IService;
using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.UnitOfWork;
using FitVerse.Core.ViewModels.Client;
using FitVerse.Core.ViewModels.Coach;
using FitVerse.Core.ViewModels.Equipment;
using FitVerse.Core.ViewModels.Package;
using FitVerse.Data.Models;
using FitVerse.Data.Service.FitVerse.Data.Service;
using FitVerse.Data.UnitOfWork;
using FitVerse.Service.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FitVerse.Web.Controllers
{
    public class CoachController : Controller
    {
        private readonly IUnitOFWorkService unitOFWorkService;
        private readonly IMapper mapper;
        private readonly ILogger<CoachController> _logger;

        //private readonly ICoachService coachService;

        public CoachController(IUnitOFWorkService unitOFWorkService, ILogger<CoachController> logger)
        {
            this.unitOFWorkService = unitOFWorkService;
            this._logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MyClients()
        {
            return View();
        }
        public IActionResult Dashboard()
        {
            var coachId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var coachName = User.FindFirstValue(ClaimTypes.Name);
            ViewBag.CoachName = coachName;


            var model = unitOFWorkService.CoachService.GetDashboardData(coachId);
            return View("Dashboard", model);
        }
        public IActionResult ClientCoaches()
        {
            return View("ClientCoaches");
        }


        [HttpGet]
        public IActionResult GetAllCoaches()
        {
            var coaches = unitOFWorkService.CoachService.GetAllCoaches();



            if (coaches == null || !coaches.Any())
            {
                return Json(new { success = false, message = "No coaches found." });
            }
            return Json(new { success = true, data = coaches });
        }

        [HttpPost]
        public IActionResult AddCoach([FromForm] AddCoachVM model)
        {
            var result = unitOFWorkService.CoachService.AddCoach(model);

            if (result.Success)
                return Json(new { success = true, message = result.Message });
            else
                return Json(new { success = false, message = result.Message });
        }
        [HttpGet]
        public IActionResult GetCoachById(string Id)
        {


            var coach = unitOFWorkService.CoachService.GetCoachByIdGuid(Id);


            if (coach == null)
                return Json(new { success = false, message = "Coach not found." });

            return Json(new { success = true, data = coach });
        }


        [HttpPost]
        public IActionResult DeleteCoach(string Id)
        {
            var result = unitOFWorkService.CoachService.DeleteCoachById(Id);
            
            if (result.Success)
            {
                return Json(new { 
                    success = true, 
                    message = result.Message 
                });
            }
            
            return Json(new { 
                success = false, 
                message = result.Message 
            });
        }


        [HttpPost]
        public IActionResult UpdateCoach([FromForm] AddCoachVM model)
        {
            var result = unitOFWorkService.CoachService.UpdateCoach(model);
            if (result.Success)
                return Json(new { success = true, message = result.Message });
            else
                return Json(new { success = false, message = result.Message });
        }



        [HttpGet]
        public IActionResult GetPaged(int page = 1, int pageSize = 5, string? search = null)
        {
            var (data, totalItems) = unitOFWorkService.CoachService.GetPagedEquipments(page, pageSize, search);

            return Json(new
            {
                data = data,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            });
        }
        [HttpGet]
        public IActionResult GetAllClients()
        {
            var coachId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var clients = unitOFWorkService.CoachRepository.GetAllClientsByCoach(coachId.ToString());

            return Json(new { data = clients });
        }

        [HttpGet]
        public IActionResult GetMyClients()
        {
            try
            {
                var coachId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(coachId))
                {
                    return Json(new { success = false, message = "Coach not authenticated" });
                }
                
                var clients = unitOFWorkService.clientOnCoachesService.GetClientsByCoachId(coachId);
                return Json(new { success = true, clients });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coach clients");
                return Json(new { success = false, message = "Error loading clients" });
            }
        }
        [HttpGet]
        public IActionResult GetPackagesByCoachId(string coachId)
        {
            try
            {
                _logger.LogInformation($"Getting packages for coach ID: {coachId}");
                
                if (string.IsNullOrEmpty(coachId))
                {
                    _logger.LogWarning("Coach ID is null or empty");
                    return Json(new { success = false, message = "Coach ID is required", data = new List<PackageVM>() });
                }

                var packages = unitOFWorkService.CoachService.GetPackagesByCoachId(coachId);
                
                _logger.LogInformation($"Found {packages?.Count ?? 0} packages for coach {coachId}");
                
                return Json(new { success = true, data = packages ?? new List<PackageVM>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting packages for coach {coachId}");
                return Json(new { success = false, message = "Error loading packages", data = new List<PackageVM>() });
            }
        }

    }

}

