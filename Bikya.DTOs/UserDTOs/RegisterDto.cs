using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Bikya.DTOs.UserDTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name must be less than 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// User type: 'User' for regular user or 'Admin' for administrator
        /// </summary>
        [Required(ErrorMessage = "User type is required.")]
        [RegularExpression("^(User|Admin)$", ErrorMessage = "User type must be 'User' or 'Admin'.")]
        public string UserType { get; set; } = "User";

        /// <summary>
        /// Admin registration code (optional)
        /// </summary>
        public string? AdminRegistrationCode { get; set; }
    }
}
