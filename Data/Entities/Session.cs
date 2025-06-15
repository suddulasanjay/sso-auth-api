using System.ComponentModel.DataAnnotations;

namespace SSOAuthAPI.Data.Entities
{
    public class Session : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime LastAccessTime { get; set; }
    }

}
