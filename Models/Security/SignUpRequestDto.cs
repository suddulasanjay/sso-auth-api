using System.ComponentModel.DataAnnotations;

namespace SSOAuthAPI.Models.Security
{
    public class SignupRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = default!;

        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = default!;
    }
}
