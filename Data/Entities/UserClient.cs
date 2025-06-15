using OpenIddict.EntityFrameworkCore.Models;
using System.ComponentModel.DataAnnotations;

namespace SSOAuthAPI.Data.Entities
{
    public class UserClient : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        [Required]
        public string AppId { get; set; } = default!; // ClientId from OpenIddict

        public string? Scope { get; set; }

        public OpenIddictEntityFrameworkCoreApplication<string> App { get; set; } = default!;
    }
}
