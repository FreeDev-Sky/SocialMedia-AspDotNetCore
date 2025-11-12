using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMedia_AspDotNetCore.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public DateTime? Birthday { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(32)]
        public string? PhoneNumber { get; set; }

        [MaxLength(256)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}




