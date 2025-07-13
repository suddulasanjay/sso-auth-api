namespace SSOAuthAPI.Models.Security
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public bool RequiresPasswordUpdate { get; internal set; }
    }
}
