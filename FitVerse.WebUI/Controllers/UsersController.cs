using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.ViewModels.Admin.User;
using FitVerse.Core.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FitVerse.Core.Models;
using System.Threading.Tasks;
using FitVerse.Core.ViewModels.Profile;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;

namespace FitVerse.Web.Controllers
{
    [Route("Admin/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUnitOFWorkService unitOFWorkService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public UsersController(IUnitOFWorkService unitOFWorkService, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _roleManager = roleManager;
            this.unitOFWorkService = unitOFWorkService;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            List<GetAllUsersViewModel> data = await unitOFWorkService.UsersService.GetAllUsers();
            return Json(new { data });
        }

        [HttpGet("SearchBy")]
        public IActionResult SearchBy(string nameOrEmail)
        {
            List<GetAllUsersViewModel> data = unitOFWorkService.UsersService.SearchBy(nameOrEmail);
            return Json(new { data });
        }
        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser(AddUserByAdmin newUser)
        {
            ModelState.Remove("Id");
            if (!ModelState.IsValid)
            {
                ViewBag.ShowAddUserModal = true;
                return View("Index", newUser);
            }

            var (success, message) = await unitOFWorkService.UsersService.AddUserAsync(newUser);

            if (!success)
            {
                TempData["AddUserStatus"] = "error";
                TempData["AddUserMessage"] = message;
                ViewBag.ShowAddUserModal = true;
                return View("Index", newUser);
            }

            TempData["AddUserStatus"] = "success";
            TempData["AddUserMessage"] = "User added successfully!";
            return RedirectToAction("Index");
        }
        [HttpGet("profile/{UserName}")]
        public async Task<IActionResult> Profile(string UserName)
        {
            var res = await unitOFWorkService.UsersService.GetUserByUserNameAsync(UserName);
            string role = await unitOFWorkService.UsersService.showByRole(res.user);
            
            ProfileViewModel newUser = new ProfileViewModel();
            newUser.UserInfo = unitOFWorkService.UsersService.MapToGetAllUsersViewModel(res.user, role);
            
            // Initialize Coach Professional Info for Coaches
            if (role == "Coach")
            {
                // Initialize empty CoachViewModel to ensure form fields are generated
                newUser.coachProfessionalInfo = new CoachViewModel
                {
                    ExperienceYears = 0,
                    Salary = 0,
                    About = "",
                    Certificates = ""
                };
            }
            
            return View("profile", newUser);
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(ProfileViewModel userImage)
        {
            var image = userImage?.UserInfo?.Image;
            var userName = userImage?.UserInfo?.UserName;

            if (image == null || image.Length == 0 || string.IsNullOrWhiteSpace(userName))
            {
                return RedirectToAction("Profile", new { UserName = userName });
            }

            var uploads = Path.Combine(_env.ContentRootPath, "App_Data", "Uploads");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var extension = Path.GetExtension(image.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var oldImagePath = user.ImagePath;

                if (!string.IsNullOrEmpty(oldImagePath) && !oldImagePath.Contains("/"))
                {
                    var oldFilePath = Path.Combine(uploads, oldImagePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.ImagePath = fileName;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Profile", new { UserName = userName });
        }

        [HttpPost("UpdatePersonalInfo")]
        public async Task<IActionResult> UpdatePersonalInfo(GetAllUsersViewModel myUser)
        {
            // Remove validation for fields not needed for update
            var keysToKeep = new[]
            {
                "UserName",
                "FullName",
                "Email",
                "PhoneNumber",
                "Age",
                "Gender"
            };

            // Remove all other keys from ModelState
            foreach (var key in ModelState.Keys.Except(keysToKeep).ToList())
            {
                ModelState.Remove(key);
            }

            if(!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { Succeeded = false, Message = "Invalid data: " + string.Join(", ", errors) });
            }

            var res = await unitOFWorkService.UsersService.UpdatePersonalInfoAsync(myUser);

            return Json(new { Succeeded = res.Item1, Message = res.Item2 });
        }
        [HttpPost("ChangeUserRole")]
        public async Task<IActionResult> ChangeUserRole(ChangeUserRoleViewModel userWithRole)
        {
            var res = await unitOFWorkService.UsersService.ChangeUserRoleAsync(userWithRole);
            return Json(new { Succeeded = res.Success, Message = res.Message });
        }
    
        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await unitOFWorkService.UsersService.DeleteUserAsync(id);
            
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
        [HttpPost("ChangePasswordByAdmin")]

        public async Task<IActionResult> ChangePasswordByAdmin( ProfileViewModel userPass)
        {
            var keysToKeep = new[]
     {
                    "ChangePasswordByAdmin.Password",
                    "ChangePasswordByAdmin.ConfirmPassword",
                    "UserInfo.UserName"
                };

            // Remove all other keys from ModelState
            foreach (var key in ModelState.Keys.Except(keysToKeep).ToList())
            {
                ModelState.Remove(key);
            }
            (bool Success, string Message) res = (false, "Change Password Failed, Please try again.");

            if (ModelState.IsValid)
            {
                res = await unitOFWorkService.UsersService
                    .ChangePasswordByAdminAsync(userPass.UserInfo.UserName, userPass.ChangePasswordByAdmin);
            }
            else
            {
                res = (false, "Invalid input data. Please check your form.");
            }

            return Json(new { Success = res.Success, Message = res.Message });
        }
        [HttpPost("ChangePasswordByUser")]

        public async Task<IActionResult> ChangePasswordByUser(ProfileViewModel userPass)
        {
           

            var keysToKeep = new[]
                {
                    "ChangePasswordByUser.OldPassword",
                    "ChangePasswordByUser.Password",
                    "ChangePasswordByUser.ConfirmPassword",
                    "UserInfo.UserName"
                };

            // Remove all other keys from ModelState
            foreach (var key in ModelState.Keys.Except(keysToKeep).ToList())
            {
                ModelState.Remove(key);
            }
            (bool Success, string Message) res = (false, "Change Password Failed, Please try again.");

            if (ModelState.IsValid)
            {
                res = await unitOFWorkService.UsersService
                    .ChangePasswordByUserAsync(userPass.UserInfo.UserName, userPass.ChangePasswordByUser);
            }
           else
            {
                res = (false, "Invalid input data. Please check your form.");
            }

            return Json(new { Success = res.Success, Message = res.Message });
        }

        [HttpPost("UpdateCoachProfessionalInfo")]
        public async Task<IActionResult> UpdateCoachProfessionalInfo(string userName, int? experienceYears, decimal? salary, string about, string certificates, string specialties)
        {
            try
            {
                var result = await unitOFWorkService.CoachService.UpdateCoachProfessionalInfo(userName, experienceYears, salary, about, certificates, specialties);
                
                if (result)
                {
                    return Json(new { Succeeded = true, Message = "Professional information updated successfully." });
                }
                
                return Json(new { Succeeded = false, Message = "Failed to update professional information." });
            }
            catch (Exception ex)
            {
                return Json(new { Succeeded = false, Message = $"Error: {ex.Message}" });
            }
        }

        //todo: Update Client Goals
        [HttpPost("UpdateClientGoals")]
        public IActionResult UpdateClientGoals(string userName, double? height, double? startWeight, string goal)
        {
            var clientInfo = new ClientViewModel
            {
                Height = height,
                StartWeight = startWeight,
                Goal = goal
            };

            var result = unitOFWorkService.ClientService.UpdateClientGoals(userName, clientInfo);
            return Json(new { result });
        }

        [HttpGet("/profile-image/{fileName}")]
        public IActionResult GetProfileImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Redirect("/images/default.jpg");
            }

            var safeFileName = Path.GetFileName(fileName);
            var uploads = Path.Combine(_env.ContentRootPath, "App_Data", "Uploads");
            var filePath = Path.Combine(uploads, safeFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return Redirect("/images/default.jpg");
            }

            var extension = Path.GetExtension(safeFileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".avif" => "image/avif",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filePath, contentType);
        }

    }

}
