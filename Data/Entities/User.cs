using System.ComponentModel.DataAnnotations;

namespace SSOAuthAPI.Data.Entities
{
    public class User : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = default!;

        public string? LastName { get; set; }

        [Required]
        public string Email { get; set; } = default!;

        public string? PasswordHash { get; set; }
        public DateTime? PasswordModifiedTime { get; set; }

        public bool Verified { get; set; } = false;
        public bool IsAdmin { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }

        public int? ProviderId { get; set; }
        public Provider? Provider { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public ICollection<UserClient> UserClients { get; set; } = new List<UserClient>();
    }

}
