using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Bikya.Data.Response;
using Bikya.DTOs.AuthDTOs;
using Bikya.DTOs.UserDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        // تعريف ثوابت لرسائل الأخطاء
        private const string UserNotFoundMessage = "User not found.";
        private const string PasswordChangeFailedMessage = "Incorrect password or invalid new password.";
        private const string ProfileUpdateFailedMessage = "Failed to update profile.";

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Retrieves a user by ID and throws a standardized error response if not found.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>ApplicationUser instance or null</returns>
        private async Task<ApplicationUser?> GetUserOrErrorAsync(int userId)
        {
            return await _userRepository.FindByIdAsync(userId);
        }

        /// <summary>
        /// Changes the password for a user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">ChangePasswordDto containing current and new password</param>
        /// <returns>ApiResponse indicating success or failure</returns>
        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await GetUserOrErrorAsync(userId);
            if (user == null)
                return ApiResponse<bool>.ErrorResponse(UserNotFoundMessage, 404);

            var result = await _userRepository.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<bool>.ErrorResponse(PasswordChangeFailedMessage, 400, errors);
            }

            return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully.");
        }

        /// <summary>
        /// Deactivates a user account (soft delete).
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>ApiResponse indicating success or failure</returns>
        public async Task<ApiResponse<bool>> DeactivateAccountAsync(int userId)
        {
            var user = await GetUserOrErrorAsync(userId);
            if (user == null)
                return ApiResponse<bool>.ErrorResponse(UserNotFoundMessage, 404);

            await _userRepository.SoftDeleteUserAsync(userId);
            return ApiResponse<bool>.SuccessResponse(true, "Account deactivated successfully.");
        }

        /// <summary>
        /// Gets a user profile by user ID.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>ApiResponse containing UserProfileDto or error</returns>
        public async Task<ApiResponse<UserProfileDto>> GetByIdAsync(int id)
        {
            var user = await GetUserOrErrorAsync(id);
            if (user == null)
                return ApiResponse<UserProfileDto>.ErrorResponse(UserNotFoundMessage, 404);

            var roles = await _userRepository.GetUserRolesAsync(user);

            return ApiResponse<UserProfileDto>.SuccessResponse(new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                SellerRating = user.SellerRating,
                ProfileImageUrl = user.ProfileImageUrl,
                IsVerified = user.IsVerified,
                Roles = roles.ToList()
            });
        }

        /// <summary>
        /// Updates a user profile.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">UpdateProfileDto containing new profile data</param>
        /// <returns>ApiResponse indicating success or failure</returns>
        public async Task<ApiResponse<bool>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await GetUserOrErrorAsync(userId);
            if (user == null)
                return ApiResponse<bool>.ErrorResponse(UserNotFoundMessage, 404);

            user.FullName = dto.FullName ?? user.FullName;
            user.ProfileImageUrl = dto.ProfileImageUrl ?? user.ProfileImageUrl;
            user.Address = dto.Address ?? user.Address;
            user.PhoneNumber =dto.PhoneNumber ?? user.PhoneNumber;

            var result = await _userRepository.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<bool>.ErrorResponse(ProfileUpdateFailedMessage, 400, errors);
            }

            return ApiResponse<bool>.SuccessResponse(true, "Profile updated successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateProfileImageAsync(int userId, string imageUrl)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.ErrorResponse("User not found.", 404);

            user.ProfileImageUrl = imageUrl;
            var result = await _userRepository.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<bool>.ErrorResponse("Failed to update profile image.", 400, errors);
            }

            return ApiResponse<bool>.SuccessResponse(true, "Profile image updated successfully.");
        }
    }
}