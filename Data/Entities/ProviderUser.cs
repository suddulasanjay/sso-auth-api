using System.ComponentModel.DataAnnotations;

namespace SSOAuthAPI.Data.Entities
{
    public class ProviderUser : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int ProviderId { get; set; }
        public Provider Provider { get; set; } = default!;

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public string? SubjectId { get; set; }
    }

}
