using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Bikya.DTOs.UserDTOs
{
    public class UpdateProfileDto
    {
        [StringLength(100, ErrorMessage = "Full name must be less than 100 characters.")]
        public string? FullName { get; set; }

        [StringLength(255, ErrorMessage = "Address must be less than 255 characters.")]
        public string? Address { get; set; }

        [StringLength(500, ErrorMessage = "Profile image URL must be less than 500 characters.")]
        [Url(ErrorMessage = "Invalid profile image URL.")]
        public string? ProfileImageUrl { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
