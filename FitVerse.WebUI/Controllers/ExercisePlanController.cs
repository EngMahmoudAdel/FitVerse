using AutoMapper;
using FitVerse.Core.IService;
using FitVerse.Core.ViewModels.Plan;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using FitVerse.Core.Models;

namespace FitVerse.Web.Controllers
{
    public class ExercisePlanController : Controller
    {
        private readonly IExercisePlanService _exercisePlanService;
        private readonly IExerciseService _exerciseService;
        private readonly IClientService clientService;
        private readonly IMapper _mapper;
        private readonly FitVerse.Web.Helpers.NotificationHelper _notificationHelper;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExercisePlanController(
            IExercisePlanService exercisePlanService,
            IExerciseService exerciseService,
            IClientService clientService,
            IMapper mapper,
            FitVerse.Web.Helpers.NotificationHelper notificationHelper,
            UserManager<ApplicationUser> userManager)
        {
            _exercisePlanService = exercisePlanService;
            _exerciseService = exerciseService;
            this.clientService = clientService;
            _mapper = mapper;
            _notificationHelper = notificationHelper;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var plans = _exercisePlanService.GetAllPlans();
            return View(plans);
        }
        [HttpGet]
        public IActionResult GetAllExercises()
        {
            try
            {
                var exercises = _exerciseService.GetAllExercises()
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        MuscleName = e.MuscleName,
                        EquipmentName = e.EquipmentName
                    })
                    .ToList();

                return Json(exercises);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load exercises: " + ex.Message });
            }
        }

        public IActionResult Details(int id)
        {
            var plan = _exercisePlanService.GetPlanById(id);
            if (plan == null)
                return NotFound();

            return View(plan);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Exercises = _exerciseService.GetAllExercises().ToList();
            return View();
        }
        [HttpPost]
        public IActionResult CreatePlan([FromBody] ExercisePlanVM vm)
        {
            try
            {
                // Validate input
                if (vm == null)
                {
                    return BadRequest(new { message = "Plan data is required." });
                }

                if (string.IsNullOrEmpty(vm.Name))
                {
                    return BadRequest(new { message = "Plan name is required." });
                }

                if (string.IsNullOrEmpty(vm.Notes))
                {
                    return BadRequest(new { message = "Plan description is required." });
                }

                if (vm.DurationWeeks <= 0)
                {
                    return BadRequest(new { message = "Duration must be greater than 0 weeks." });
                }

                if (vm.SelectedExerciseIds == null || !vm.SelectedExerciseIds.Any())
                {
                    return BadRequest(new { message = "At least one exercise must be selected." });
                }

                if (string.IsNullOrEmpty(vm.ClientId))
                {
                    return BadRequest(new { message = "A client must be selected for this plan." });
                }

                // Get current logged-in coach ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                // Set the coach ID from current user
                vm.CoachId = currentUserId;

                var success = _exercisePlanService.CreatePlan(vm);
                if (success)
                {
                    return Ok(new { message = "Plan created successfully!" });
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to create plan. Please try again." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework)
                return StatusCode(500, new { message = "An error occurred while creating the plan: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var plan = _exercisePlanService.GetPlanById(id);
            if (plan == null)
                return NotFound();

            var vm = _mapper.Map<ExercisePlanVM>(plan);
            ViewBag.Exercises = _exerciseService.GetAllExercises().ToList();
            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(int id, ExercisePlanVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Exercises = _exerciseService.GetAllExercises().ToList();
                return View(vm);
            }

            var success = _exercisePlanService.UpdatePlan(id, vm);
            if (success)
                return RedirectToAction(nameof(Index));

            TempData["Error"] = "something went wrong ";
            return View(vm);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var plan = _exercisePlanService.GetPlanById(id);
            if (plan == null)
                return NotFound();

            return View(plan);
        }

        [HttpPost, ActionName("DeleteConfirmed")]

        public IActionResult DeleteConfirmed(int id)
        {
            var success = _exercisePlanService.DeletePlan(id);
            if (success)
                return RedirectToAction(nameof(Index));

            TempData["Error"] = "something went wrong ";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetAllPlans()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var plans = _exercisePlanService.GetAllPlans()
                    .Where(p => p.CoachId == currentUserId) // Only show plans for current coach
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        durationWeeks = p.DurationWeeks,
                        notes = p.Notes,
                        clientCount = string.IsNullOrEmpty(p.ClientId) ? 0 : 1, // Simplified for now
                        exerciseCount = p.ExercisePlanDetails?.Count ?? 0,
                        createdDate = p.Date
                    })
                    .ToList();

                return Json(plans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load plans: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetCoachClients()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var clients = clientService.GetClientsByCoachId(currentUserId)
                    .Select(c => new
                    {
                         c.Id,
                         c.Name,
                         c.IsActive
                    })
                    .ToList();

                return Json(clients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load clients: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetPlanDetails(int id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var plan = _exercisePlanService.GetPlanById(id);
                
                if (plan == null)
                {
                    return NotFound(new { message = "Plan not found." });
                }

                // Ensure the plan belongs to the current coach
                if (plan.CoachId != currentUserId)
                {
                    return Forbid();
                }

                var planDetails = new
                {
                    id = plan.Id,
                    name = plan.Name,
                    notes = plan.Notes,
                    durationWeeks = plan.DurationWeeks,
                    clientId = plan.ClientId,
                    coachId = plan.CoachId,
                    createdDate = plan.Date,
                    exercises = plan.ExercisePlanDetails?.Select(epd => new
                    {
                        id = epd.Exercise?.Id,
                        name = epd.Exercise?.Name,
                        sets = epd.NumOfSets,
                        reps = epd.NumOfRepeats,
                        muscleName = epd.Exercise?.Muscle?.Name,
                        equipmentName = epd.Exercise?.Equipment?.Name
                    }).ToList()
                };

                return Json(planDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load plan details: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdatePlan([FromBody] ExercisePlanVM vm)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Validate input
                if (vm == null || vm.Id <= 0)
                {
                    return BadRequest(new { message = "Invalid plan data." });
                }

                // Ensure the plan belongs to the current coach
                var existingPlan = _exercisePlanService.GetPlanById(vm.Id);
                if (existingPlan == null)
                {
                    return NotFound(new { message = "Plan not found." });
                }

                if (existingPlan.CoachId != currentUserId)
                {
                    return Forbid();
                }

                // Validate required fields
                if (string.IsNullOrEmpty(vm.Name))
                {
                    return BadRequest(new { message = "Plan name is required." });
                }

                if (string.IsNullOrEmpty(vm.ClientId))
                {
                    return BadRequest(new { message = "A client must be selected for this plan." });
                }

                if (vm.DurationWeeks <= 0)
                {
                    return BadRequest(new { message = "Duration must be greater than 0 weeks." });
                }

                if (vm.SelectedExerciseIds == null || !vm.SelectedExerciseIds.Any())
                {
                    return BadRequest(new { message = "At least one exercise must be selected." });
                }

                // Set the coach ID from current user
                vm.CoachId = currentUserId;

                var success = _exercisePlanService.UpdatePlan(vm.Id, vm);
                if (success)
                {
                    return Ok(new { message = "Plan updated successfully!" });
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to update plan. Please try again." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the plan: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignPlan([FromBody] AssignPlanRequest request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (request == null || request.PlanId <= 0 || string.IsNullOrEmpty(request.ClientId))
                {
                    return BadRequest(new { message = "Invalid assignment data." });
                }

                // Get the plan and ensure it belongs to the current coach
                var plan = _exercisePlanService.GetPlanById(request.PlanId);
                if (plan == null)
                {
                    return NotFound(new { message = "Plan not found." });
                }

                if (plan.CoachId != currentUserId)
                {
                    return Forbid();
                }

                // Update the plan's client assignment
                var planVM = new ExercisePlanVM
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    Notes = plan.Notes,
                    DurationWeeks = plan.DurationWeeks,
                    CoachId = plan.CoachId,
                    ClientId = request.ClientId,
                    SelectedExerciseIds = plan.ExercisePlanDetails?.Select(epd => epd.ExerciseId).ToList()
                };

                var success = _exercisePlanService.UpdatePlan(request.PlanId, planVM);
                if (success)
                {
                    // Send notification to client
                    try
                    {
                        await _notificationHelper.NotifyPlanAssignedAsync(
                            request.ClientId,
                            "Exercise",
                            request.PlanId
                        );
                    }
                    catch (Exception notifEx)
                    {
                        Console.WriteLine($"Error sending plan assignment notification: {notifEx.Message}");
                    }
                    
                    return Ok(new { message = "Plan assigned successfully!" });
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to assign plan. Please try again." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning the plan: " + ex.Message });
            }
        }

        public class AssignPlanRequest
        {
            public int PlanId { get; set; }
            public string ClientId { get; set; }
        }


    }
}
