using AutoMapper;
using FitVerse.Core.Models;
using FitVerse.Core.ViewModels.Admin.User;
using FitVerse.Core.ViewModels.Profile;
using FitVerse.Core.ViewModels.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Core.IService
{
    public interface IUsersService:IService
    {
        public  Task<List<GetAllUsersViewModel>> GetAllUsers();
        public  List<GetAllUsersViewModel> SearchBy(string nameOrEmail);
        public Task<String> showByRole(ApplicationUser user);
        Task<(bool Success, string Message)> AddUserAsync(AddUserByAdmin newUser);

        public Task<(bool Success, string Message)> UpdateUserAsync(AddUserByAdmin myUser);
        public Task<(bool Success, string Message)> DeleteUserAsync(string Id);
        public Task<(bool Success, string Message, ApplicationUser user)> GetUserByEmailAsync(string email);
        public Task<(bool Success, string Message, ApplicationUser user)> GetUserByUserNameAsync(string UserName)
;
        public Task<(bool Success, string Message, ApplicationUser user)> GetUserById(string id);
        public GetAllUsersViewModel MapToGetAllUsersViewModel(ApplicationUser user, string role);
        public Task UpdateUserImagePathAsync(string userName, string imageFileName);
        public  Task<(bool, string)> UpdatePersonalInfoAsync(GetAllUsersViewModel user);

        public Task<(bool Success, string Message)> ChangeUserRoleAsync(ChangeUserRoleViewModel userWithRole);
        public Task<(bool Success, string Message)> ChangePasswordByAdminAsync(string UserName, ChangePasswordByAdminViewModel passwords);
        public Task<(bool Success, string Message)> ChangePasswordByUserAsync(string UserName, ChangePasswordByUserViewModel passwords);




    }
}
