using System.ComponentModel.DataAnnotations;

namespace SSOAuthAPI.Data.Entities
{
    public class Provider : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string ClientId { get; set; }
    }
}
