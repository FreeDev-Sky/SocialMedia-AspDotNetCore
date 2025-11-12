using System.ComponentModel.DataAnnotations;

namespace SocialMedia_AspDotNetCore.ViewModels
{
    public class UserProfileViewModel
    {
        // Profile identity
        public int? ProfileId { get; set; }

        // Associated user
        public string? UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [Display(Name = "Phone Number")]
        [MaxLength(32)]
        public string? PhoneNumber { get; set; }

        [MaxLength(256)]
        public string? Address { get; set; }
    }
}


