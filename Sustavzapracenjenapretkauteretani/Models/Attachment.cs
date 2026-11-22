using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Teretana.Models
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Vjezba")]
        public int VjezbaId { get; set; }

        public virtual Vjezba? Vjezba { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
