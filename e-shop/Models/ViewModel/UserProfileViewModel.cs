using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace e_shop.Models.ViewModel
{
    public class UserProfileViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }

        public IFormFile? ImageFile { get; set; }  // ✅ Nullable
        public string ImagePath { get; set; }

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }

}
