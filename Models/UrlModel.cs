using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace URLShortenerAPI.Models
{
    public class UrlModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string OriginalUrl { get; set; }

        public string? ShortenedUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int CreatedBy { get; set; } 
    }
}
