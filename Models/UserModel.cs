using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URLShortenerAPI.Enums;

namespace URLShortenerAPI.Models
{
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryDate { get; set; }
    }
}
