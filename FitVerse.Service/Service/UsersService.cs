using AutoMapper;
using FitVerse.Core.Helpers;
using FitVerse.Core.IService;
using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.Models;
using FitVerse.Core.UnitOfWork;
using FitVerse.Core.ViewModels.Admin.User;
using FitVerse.Core.ViewModels.Profile;
using FitVerse.Core.ViewModels.User;
using FitVerse.Data.Models;
using FitVerse.Data.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace FitVerse.Service.Service
{
    public class UsersService : IUsersService
    {
        UserManager<ApplicationUser> userManager;
        IUnitOfWork UnitOfWork;
        IMapper mapper;
        public UsersService(UserManager<ApplicationUser> userManager, IMapper mapper ,IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.UnitOfWork = unitOfWork;
        }
        public async Task<List<GetAllUsersViewModel>> GetAllUsers()
        {
            // جلب كل المستخدمين مرة واحدة
            List<ApplicationUser> users = await userManager.Users.ToListAsync();


            var data = new List<GetAllUsersViewModel>();

            foreach (var user in users)
            {
                // جلب الـ role لكل مستخدم بشكل متتابع (مش متوازي)MO
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                var newUser = mapper.Map<GetAllUsersViewModel>(user);
                newUser.Id = user.Id;
                newUser.Role = role;
                newUser.PhoneNumber = user.PhoneNumber ?? "";

                data.Add(newUser);
            }

            return data;
        }

        public List<GetAllUsersViewModel> SearchBy(string nameOrEmail)
        {
            List<ApplicationUser> users = userManager.Users.Where(u => u.FullName.Contains(nameOrEmail) || u.Email.Contains(nameOrEmail)).ToList();
            return mapper.Map<List<GetAllUsersViewModel>>(users);

        }

        public async Task<String> showByRole(ApplicationUser user)
        {
            var res = await userManager.GetRolesAsync(user);
            return res.FirstOrDefault();

        }
        public async Task<(bool Success, string Message)> AddUserAsync(AddUserByAdmin newUser)
        {
            // Create the user object
            var user = new ApplicationUser
            {
                FullName = newUser.FirstName + " " + newUser.LastName,
                UserName = newUser.FirstName + newUser.LastName + Guid.NewGuid().ToString().Substring(0, 6),
                Email = newUser.Email,
                Status = newUser.Status,
                PhoneNumber = newUser.PhoneNumber,
                joinedDate = DateTime.Now,
                Age = newUser.Age,
                Gender = newUser.Gender
            };
            var oldUser = await userManager.FindByEmailAsync(newUser.Email);
            if (oldUser != null)
                return (false, "User Alreday created Before!");
            // Create user
            var result = await userManager.CreateAsync(user, newUser.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            // Assign role
            await userManager.AddToRoleAsync(user, newUser.Role);
            //
            if(Statics.CLIENT==newUser.Role)
            {
                Client client = new Client
                {
                    
                    UserId = user.Id
                    
                };

                UnitOfWork.Clients.Add(client);
            }
            else if(Statics.COACH==newUser.Role)
            {
                Coach coach = new Coach
                {
                    
                    UserId = user.Id
                    
                };
                UnitOfWork.Coaches.Add(coach);
            }
            UnitOfWork.Complete();
                return (true, "User created successfully!");
        }
        public async Task<(bool Success, string Message)> UpdateUserAsync(AddUserByAdmin myUser)
        {
            // Find the user by Id
            var user = await userManager.FindByIdAsync(myUser.Id.ToString());
            if (user == null)
                return (false, "User not found");

            // Update basic info
            user.FullName = myUser.FirstName.Trim() + ' ' + myUser.LastName.Trim();
            user.Email = myUser.Email;
            user.PhoneNumber = myUser.PhoneNumber;
            user.Status = myUser.Status;

            // Update user
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return (false, "Failed to update user, this data may be invalid or this Email used before.");

            // Update roles if changed
            var currentRoles = await userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(myUser.Role))
            {
                // Remove all old roles and add the new one
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                await userManager.AddToRoleAsync(user, myUser.Role);
            }



            return (true, "User updated successfully");
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return (false, "User not found");

            try
            {
                // Get user role to determine what related data to delete
                var roles = await userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault();

                // Handle Coach deletion
                if (userRole == "Coach")
                {
                    var coach = UnitOfWork.Coaches.Find(c => c.UserId == id).FirstOrDefault();
                    if (coach != null)
                    {
                        // Note: Skipping active subscription check as ClientSubscriptions repository is not available
                        // TODO: Add IClientSubscriptionRepository to UnitOfWork

                        // Delete related data in correct order
                        // 1. Delete Chats and Messages
                        var chats = UnitOfWork.Chats.Find(c => c.CoachId == coach.Id.ToString()).ToList();
                        foreach (var chat in chats)
                        {
                            var messages = UnitOfWork.Messages.Find(m => m.ChatId == chat.Id).ToList();
                            UnitOfWork.Messages.RemoveRange(messages);
                            UnitOfWork.Chats.Delete(chat);
                        }

                        // 2. Delete Coach Feedbacks
                        var feedbacks = UnitOfWork.CoachFeedbacks.Find(f => f.CoachId == coach.Id).ToList();
                        foreach (var feedback in feedbacks)
                        {
                            UnitOfWork.CoachFeedbacks.Delete(feedback);
                        }

                        // 3. Delete Exercise Plans
                        var exercisePlans = UnitOfWork.ExercisePlans.Find(e => e.CoachId == coach.Id).ToList();
                        foreach (var plan in exercisePlans)
                        {
                            UnitOfWork.ExercisePlans.Delete(plan);
                        }

                        // 4. Delete Diet Plans
                        var dietPlans = UnitOfWork.DietPlans.Find(d => d.CoachId == coach.Id).ToList();
                        foreach (var plan in dietPlans)
                        {
                            UnitOfWork.DietPlans.Delete(plan);
                        }

                        // Note: ClientSubscriptions repository needs to be added to UnitOfWork
                        // Skipping inactive subscriptions deletion for now

                        // 5. Delete Coach Packages
                        var coachPackages = UnitOfWork.coachPackageRepository.Find(cp => cp.CoachId == coach.Id).ToList();
                        foreach (var package in coachPackages)
                        {
                            UnitOfWork.coachPackageRepository.Delete(package);
                        }

                        // 6. Delete Coach Specialties
                        var specialties = UnitOfWork.CoachSpecialties.Find(cs => cs.CoachId == coach.Id).ToList();
                        foreach (var specialty in specialties)
                        {
                            UnitOfWork.CoachSpecialties.Delete(specialty);
                        }

                        // 7. Delete Coach record
                        UnitOfWork.Coaches.Delete(coach);
                    }
                }
                // Handle Client deletion
                else if (userRole == "Client")
                {
                    var client = UnitOfWork.Clients.Find(c => c.UserId == id).FirstOrDefault();
                    if (client != null)
                    {
                        // Note: Skipping active subscription check as ClientSubscriptions repository is not available
                        // TODO: Add IClientSubscriptionRepository to UnitOfWork

                        // Delete related data
                        // 1. Delete Chats and Messages
                        var chats = UnitOfWork.Chats.Find(c => c.ClientId == client.Id.ToString()).ToList();
                        foreach (var chat in chats)
                        {
                            var messages = UnitOfWork.Messages.Find(m => m.ChatId == chat.Id).ToList();
                            UnitOfWork.Messages.RemoveRange(messages);
                            UnitOfWork.Chats.Delete(chat);
                        }

                        // 2. Delete Daily Logs
                        var dailyLogs = UnitOfWork.DailyLogsRepository.Find(d => d.ClientId == client.Id).ToList();
                        foreach (var log in dailyLogs)
                        {
                            UnitOfWork.DailyLogsRepository.Delete(log);
                        }

                        // 3. Delete Coach Feedbacks
                        var feedbacks = UnitOfWork.CoachFeedbacks.Find(f => f.ClientId == client.Id).ToList();
                        foreach (var feedback in feedbacks)
                        {
                            UnitOfWork.CoachFeedbacks.Delete(feedback);
                        }

                        // Note: ClientSubscriptions repository needs to be added to UnitOfWork
                        // Skipping inactive subscriptions deletion for now

                        // 4. Delete Client record
                        UnitOfWork.Clients.Delete(client);
                    }
                }

                // Save all deletions
                UnitOfWork.Complete();

                // Finally delete the user
                var res = await userManager.DeleteAsync(user);
                if (res.Succeeded)
                {
                    return (true, "User and all related data deleted successfully");
                }

                return (false, string.Join(", ", res.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting user: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message, ApplicationUser user)> GetUserByEmailAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (success: false, Message: "User Not Found", user: null);
            }
            return (true, "User Found", user);
        }
        public async Task<(bool Success, string Message, ApplicationUser user)> GetUserByUserNameAsync(string UserName)
        {
            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                return (success: false, Message: "User Not Found", user: null);
            }
            return (true, "User Found", user);
        }
        public async Task<(bool Success, string Message, ApplicationUser user)> GetUserById(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return (success: false, Message: "User Not Found", user: null);
            }
            return (true, "User Found", user);
        }
        public GetAllUsersViewModel MapToGetAllUsersViewModel(ApplicationUser user, string role)
        {
            var viewModel = mapper.Map<GetAllUsersViewModel>(user);
            viewModel.Id = user.Id;
            viewModel.Role = role;
            viewModel.PhoneNumber = user.PhoneNumber;
            viewModel.Email = user.Email;
            viewModel.Age = user.Age;
            viewModel.Gender = user.Gender;

            return viewModel;
        }

        public async Task UpdateUserImagePathAsync(string userName, string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("User name is required", nameof(userName));
            }

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                throw new InvalidOperationException($"User not found: {userName}");
            }

            user.ImagePath = imageFileName;
            await userManager.UpdateAsync(user);
        }

        public async Task<(bool, string)> UpdatePersonalInfoAsync(GetAllUsersViewModel user)
        {
            var foundUser = await userManager.FindByEmailAsync(user.Email);
            var appUser = await userManager.FindByNameAsync(user.UserName);

            if (foundUser != null && (foundUser.UserName != appUser.UserName))
                return (Success: false, Message: "The Email is Already Used");

            var currentRoles = await userManager.GetRolesAsync(appUser);


            appUser.PhoneNumber = user.PhoneNumber;
            appUser.Email = user.Email;
            appUser.FullName = user.FullName;
            appUser.Age = user.Age;
            appUser.Gender = user.Gender;

            var result = await userManager.UpdateAsync(appUser);
            return (Success: true, Message: "Personal info updated successfully!");

        }

        public async Task<(bool Success, string Message)> ChangeUserRoleAsync(ChangeUserRoleViewModel userWithRole)
        {
            var user = await userManager.FindByNameAsync(userWithRole.UserName);
            if (user == null)
                return (false, "User not found");
            var currentRoles = await userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(userWithRole.Role))
            {
                // Remove all old roles and add the new one
                await userManager.AddToRoleAsync(user, userWithRole.Role);
                await userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            if (Statics.CLIENT == userWithRole.Role)
            {
                Client client = new Client
                {

                    UserId = user.Id

                };

                UnitOfWork.Clients.Add(client);
                UnitOfWork.Clients.DeleteByUserId(user.Id);
            }
            else if (Statics.COACH == userWithRole.Role)
            {
                Coach coach = new Coach
                {

                    UserId = user.Id

                };
                UnitOfWork.Coaches.Add(coach);
                UnitOfWork.Coaches.DeleteByUserId(user.Id);
            }
            else if (Statics.ADMIN == userWithRole.Role)
            {
                UnitOfWork.Coaches.DeleteByUserId(user.Id);
                UnitOfWork.Clients.DeleteByUserId(user.Id);

            }
            UnitOfWork.Complete();
            return (true, "User role updated successfully");

        }
        public async Task<(bool Success, string Message)> ChangePasswordByAdminAsync(string UserName, ChangePasswordByAdminViewModel passwords)
        {
            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
                return (false, "User not found");
            
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, passwords.Password);
               

            return (true, "User Password updated successfully");

        }
        public async Task<(bool Success, string Message)> ChangePasswordByUserAsync(string UserName, ChangePasswordByUserViewModel passwords)
        {
            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
                return (false, "User not found");
            
            var result= await userManager.ChangePasswordAsync(user, passwords.OldPassword, passwords.Password);
            if (!result.Succeeded)
            {
                return (false, "Updating password failed, please try again");

            }


            return (true, "User Password updated successfully");

        }

    }

}
