using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Bikya.DTOs.UserDTOs
{
    public class UpdateUserDto
    {
        [StringLength(100, ErrorMessage = "Full name must be less than 100 characters.")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [RegularExpression("^(User|Admin)$", ErrorMessage = "Role must be 'User' or 'Admin'.")]
        public string? Role { get; set; }
    }
}
