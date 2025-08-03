using System.ComponentModel.DataAnnotations;

namespace e_shop.Models.ViewModel
{
    public class AdminProfileViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }


}
