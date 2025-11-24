using FitVerse.Core.IService;
using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.ViewModels.Client;
using FitVerse.Core.ViewModels.Package;
using FitVerse.Data.Models;
using FitVerse.Service.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FitVerse.Web.Controllers
{
    public class ClientController : Controller
    {
        private readonly IUnitOFWorkService unitOFWorkService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IUnitOFWorkService unitOFWorkService, ILogger<ClientController> logger)
        {
            this.unitOFWorkService = unitOFWorkService;
            this._logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DashBoard()
        {
            return View();
        }

        public IActionResult GetAll()
        {
            var clients = unitOFWorkService.ClientService.GetAllClients();
            return Json(new { data = clients });
        }

      
        public IActionResult Add(AddClientVM model)
        {
            var result = unitOFWorkService.ClientService.AddClient(model);
            return Json(result);
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult Payment(int packageId, string coachId)
        {
            try
            {
                _logger.LogInformation($"Loading payment page for package {packageId} and coach {coachId}");

                if (packageId <= 0 || string.IsNullOrEmpty(coachId))
                {
                    TempData["Error"] = "Invalid package or coach selection";
                    return RedirectToAction("ClientCoaches", "Coach");
                }

                // Get current client ID
                var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(clientId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get package details
                var package = unitOFWorkService.PackageRepository.GetById(packageId);
                if (package == null)
                {
                    TempData["Error"] = "Package not found";
                    return RedirectToAction("ClientCoaches", "Coach");
                }

                // Get coach details
                var coach = unitOFWorkService.Coaches
                    .GetAll(filter: c => c.Id == coachId, includeProperties: "User")
                    .FirstOrDefault();
                
                if (coach == null)
                {
                    TempData["Error"] = "Coach not found";
                    return RedirectToAction("ClientCoaches", "Coach");
                }

                // Get client details
                var client = unitOFWorkService.Clients
                    .GetAll(filter: c => c.Id == clientId, includeProperties: "User")
                    .FirstOrDefault();

                // Calculate subscription duration (assuming 30 days per session)
                int durationDays = package.Sessions * 30;

                // Create view model
                var viewModel = new
                {
                    PackageId = package.Id,
                    PackageName = package.Name,
                    PackagePrice = package.Price,
                    PackageSessions = package.Sessions,
                    PackageDescription = package.Description,
                    CoachId = coach.Id,
                    CoachName = coach.User?.FullName ?? "Coach",
                    CoachImagePath = coach.User?.ImagePath ?? "/Images/default.jpg",
                    CoachExperience = coach.ExperienceYears ?? 0,
                    ClientId = clientId,
                    ClientName = client?.User?.FullName ?? "Client",
                    DurationDays = durationDays,
                    SubscriptionStartDate = DateTime.Now,
                    SubscriptionEndDate = DateTime.Now.AddDays(durationDays)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment page");
                TempData["Error"] = "An error occurred while loading the payment page";
                return RedirectToAction("ClientCoaches", "Coach");
            }
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int packageId, string coachId, string paymentMethod, string cardNumber = null)
        {
            try
            {
                _logger.LogInformation($"[PAYMENT START] Package: {packageId}, Coach: {coachId}, Method: {paymentMethod}");

                // Validate client authentication
                var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(clientId))
                {
                    _logger.LogWarning("[PAYMENT FAILED] User not authenticated");
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get package details
                var package = unitOFWorkService.PackageRepository.GetById(packageId);
                if (package == null)
                {
                    _logger.LogWarning($"[PAYMENT FAILED] Package {packageId} not found");
                    return Json(new { success = false, message = "Package not found" });
                }

                _logger.LogInformation($"[PAYMENT] Processing mock payment for ${package.Price}");

                // Mock Payment Processing (simplified - always succeeds for non-credit card)
                var paymentResult = await ProcessMockPayment(package.Price, paymentMethod, cardNumber);
                
                if (!paymentResult.Success)
                {
                    _logger.LogWarning($"[PAYMENT FAILED] {paymentResult.Message}");
                    return Json(new { success = false, message = paymentResult.Message });
                }

                _logger.LogInformation($"[PAYMENT] Mock payment successful. TXN: {paymentResult.TransactionId}");

                // Calculate subscription dates
                int durationDays = package.Sessions * 30;
                var startDate = DateTime.Now;
                var endDate = startDate.AddDays(durationDays);

                // Create payment record
                var payment = new Payment
                {
                    Amount = package.Price,
                    PaymentMethod = $"{paymentMethod} (TXN: {paymentResult.TransactionId})",
                    PaymentDate = paymentResult.ProcessedAt,
                    PaymentStatus = "Completed",
                    PackageId = packageId,
                    ClientId = clientId
                };

                _logger.LogInformation($"[PAYMENT] Adding payment record to database");
                unitOFWorkService.PaymentRepository.Add(payment);
                
                try
                {
                    unitOFWorkService.PaymentRepository.complete();
                    _logger.LogInformation($"[PAYMENT] Payment record saved successfully");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "[PAYMENT ERROR] Failed to save payment record");
                    return Json(new { success = false, message = "Failed to save payment record" });
                }

                // Create client subscription
                var subscription = new ClientSubscription
                {
                    ClientId = clientId,
                    CoachId = coachId,
                    PackageId = packageId,
                    StartDate = startDate,
                    EndDate = endDate,
                    PriceAtPurchase = (decimal)package.Price,
                    Status = "Active"
                };

                _logger.LogInformation($"[PAYMENT] Creating subscription record");

                // Add subscription directly without loading client
                try
                {
                    var client = unitOFWorkService.Clients.GetAll(filter: c => c.Id == clientId, includeProperties: "ClientSubscriptions").FirstOrDefault();
                    if (client == null)
                    {
                        _logger.LogWarning($"[PAYMENT FAILED] Client {clientId} not found");
                        return Json(new { success = false, message = "Client not found" });
                    }

                    if (client.ClientSubscriptions == null)
                    {
                        client.ClientSubscriptions = new List<ClientSubscription>();
                    }

                    client.ClientSubscriptions.Add(subscription);
                    unitOFWorkService.Clients.Update(client);
                    unitOFWorkService.Clients.complete();
                    
                    _logger.LogInformation($"[PAYMENT] Subscription saved successfully");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "[PAYMENT ERROR] Failed to save subscription");
                    return Json(new { success = false, message = "Failed to create subscription" });
                }

                _logger.LogInformation($"[PAYMENT SUCCESS] Client {clientId}, TXN: {paymentResult.TransactionId}");

                return Json(new { 
                    success = true, 
                    message = "Payment processed successfully!",
                    transactionId = paymentResult.TransactionId,
                    subscriptionEndDate = endDate.ToString("MMM dd, yyyy")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENT CRITICAL ERROR] Unexpected error during payment processing");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // Mock Payment Processing Method
        private async Task<MockPaymentResult> ProcessMockPayment(double amount, string paymentMethod, string cardNumber)
        {
            // Simulate minimal payment processing delay
            await Task.Delay(800);

            _logger.LogInformation($"[MOCK PAYMENT] Processing {paymentMethod} payment for ${amount}");

            // For demo purposes, always succeed regardless of payment method
            // In production, you would integrate with real payment gateway here
            
            var transactionId = $"TXN{Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()}";
            
            _logger.LogInformation($"[MOCK PAYMENT] Success - Transaction ID: {transactionId}");

            return new MockPaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                Message = $"Payment processed successfully via {paymentMethod}",
                ProcessedAt = DateTime.Now
            };
        }

        [HttpGet]
        public IActionResult GetClientUserId(string id)
        {
            try
            {
                // Get client by string Id using GetAll and FirstOrDefault
                var client = unitOFWorkService.ClientRepository.GetAll()
                    .FirstOrDefault(c => c.Id == id);
                
                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                if (string.IsNullOrEmpty(client.UserId))
                {
                    return Json(new { success = false, message = "Client has no associated user" });
                }
                
                return Json(new { success = true, userId = client.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting UserId for client {id}");
                return Json(new { success = false, message = "Error retrieving client information" });
            }
        }

        // Mock Payment Result Class
        private class MockPaymentResult
        {
            public bool Success { get; set; }
            public string TransactionId { get; set; }
            public string Message { get; set; }
            public DateTime ProcessedAt { get; set; } = DateTime.Now;
        }
    }
}
