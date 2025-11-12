using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMedia_AspDotNetCore.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // For private messages - if null, it's a global message
        public string? ReceiverId { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public User? Receiver { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Helper property to check if message is private
        public bool IsPrivate => !string.IsNullOrEmpty(ReceiverId);
    }
}

