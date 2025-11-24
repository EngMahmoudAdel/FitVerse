using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.ViewModels.DietPlan;
using FitVerse.Data.Models;
using FitVerse.Service.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using FitVerse.Core.Models;

namespace FitVerse.Web.Controllers
{
    [Authorize(Roles = "Coach")]
    public class DietPlanController : Controller
    {
        private readonly IUnitOFWorkService _unitOfWorkService;
        private readonly ILogger<DietPlanController> _logger;
        private readonly FitVerse.Web.Helpers.NotificationHelper _notificationHelper;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public DietPlanController(IUnitOFWorkService unitOfWorkService, ILogger<DietPlanController> logger, FitVerse.Web.Helpers.NotificationHelper notificationHelper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWorkService = unitOfWorkService;
            _logger = logger;
            _notificationHelper = notificationHelper;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var model = new DietPlanDashboardVM
            {
                DietPlanCount = _unitOfWorkService.DietPlanService.GetCount(),
                ClientFollowingCount = _unitOfWorkService.DietPlanService.GetClientFollowing()

            };
            return View(model);
        }
        [HttpGet]
        public IActionResult GetAll(string? search)
        {
            // ✅ جلب CoachId للمستخدم الحالي
            var currentCoachId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var plans = _unitOfWorkService.DietPlanService.GetAll()
                .Where(p => p.CoachId == currentCoachId); // ✅ فلترة حسب الكوتش الحالي

            if (!string.IsNullOrWhiteSpace(search))
            {
                plans = plans.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return Json(new { data = plans });
        }

        public IActionResult GetById(int id)
        {
            var plan = _unitOfWorkService.DietPlanService.GetById(id);
            if (plan == null)
                return Json(null);

            return Json(plan); // دلوقتي res في JS هتبقى الخطة مباشرة
        }
        [HttpPost]
        public IActionResult Add([FromBody] DietPlanVM plan)
        {
            try
            {
                // ✅ تعيين CoachId من المستخدم الحالي
                var currentCoachId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentCoachId))
                {
                    return Json(new { success = false, message = "Coach not authenticated" });
                }
                
                plan.CoachId = currentCoachId;
                
                _unitOfWorkService.DietPlanService.Add(plan);
                return Json(new { success = true, message = "Diet Plan added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stack = ex.StackTrace });
            }
        }

        public IActionResult Update([FromBody] DietPlanVM plan)
        {
            try
            {
                if (plan == null || plan.Id == 0)
                    return Json(new { success = false, message = "Invalid diet plan data." });

                _unitOfWorkService.DietPlanService.Update(plan);
                return Json(new { success = true, message = "Diet Plan updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Delete(int id)
        {
            try
            {
                _unitOfWorkService.DietPlanService.Delete(id);
                return Json(new { success = true, message = "Diet Plan deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting diet plan {id}");
                return Json(new { success = false, message = "Error deleting diet plan." });
            }
        }

        [HttpGet]
        public IActionResult GetCoachClients()
        {
            try
            {
                var currentCoachId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentCoachId))
                {
                    return Json(new { success = false, message = "Coach not authenticated" });
                }

                // Get clients assigned to this coach
                var clients = _unitOfWorkService.ClientRepository
                    .Find(c => c.ClientSubscriptions.Any(cs => cs.CoachId == currentCoachId))
                    .Select(c => new
                    {
                        Id = c.Id,
                        Name = c.User.FullName,
                        Email = c.User.Email,
                        ImagePath = c.User.ImagePath
                    })
                    .ToList();

                return Json(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coach clients");
                return Json(new { success = false, message = "Error loading clients" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignPlan([FromBody] AssignDietPlanRequest request)
        {
            try
            {
                if (request == null || request.PlanId <= 0 || string.IsNullOrEmpty(request.ClientId))
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                var currentCoachId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentCoachId))
                {
                    return Json(new { success = false, message = "Coach not authenticated" });
                }

                // Verify the client belongs to the current coach
                var client = _unitOfWorkService.ClientRepository
                    .Find(c => c.Id == request.ClientId && c.ClientSubscriptions.Any(cs => cs.CoachId == currentCoachId))
                    .FirstOrDefault();
                
                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found or not assigned to you" });
                }

                // Get the plan using repository to avoid tracking conflicts
                var dietPlanEntity = _unitOfWorkService.DietPlanRepository
                    .Find(p => p.Id == request.PlanId)
                    .FirstOrDefault();
                    
                if (dietPlanEntity == null)
                {
                    return Json(new { success = false, message = "Diet plan not found" });
                }

                // Update both ClientId and CoachId to assign the plan
                dietPlanEntity.ClientId = request.ClientId;
                dietPlanEntity.CoachId = currentCoachId; // ✅ تحديث CoachId أيضاً
                
                // Save changes through repository
                _unitOfWorkService.DietPlanRepository.Update(dietPlanEntity);
                _unitOfWorkService.DietPlanRepository.complete(); // Commit the changes

                _logger.LogInformation($"Diet plan {request.PlanId} assigned to client {request.ClientId} by coach {currentCoachId}");
                
                // Send notification to client
                try
                {
                    await _notificationHelper.NotifyPlanAssignedAsync(
                        request.ClientId,
                        "Diet",
                        request.PlanId
                    );
                }
                catch (Exception notifEx)
                {
                    _logger.LogError(notifEx, $"Error sending diet plan assignment notification to client {request.ClientId}");
                }
                
                return Json(new { success = true, message = "Diet plan assigned successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning diet plan {request?.PlanId} to client {request?.ClientId}");
                return Json(new { success = false, message = "Error assigning diet plan" });
            }
        }
    }

    public class AssignDietPlanRequest
    {
        public int PlanId { get; set; }
        public string ClientId { get; set; }
    }
}
