using System.ComponentModel.DataAnnotations;

namespace SSOAuthAPI.Data.Entities
{
    public abstract class BaseEntity
    {
        [Required]
        [StringLength(1)]
        public string Status { get; set; } = "E";
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedTime { get; set; } = DateTime.UtcNow;
    }
}
