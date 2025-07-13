using System.ComponentModel.DataAnnotations;

namespace SSOAuthAPI.Models.Security
{
    public class CustomLoginDto
    {
        /// <summary>
        /// Email address of the user
        /// </summary>
        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; }
        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        public string Password { get; set; }
        //public string? ReturnUrl { get; set; }

        public string? AuthId { get; set; }
        public string? ClientId { get; set; }
    }
}
