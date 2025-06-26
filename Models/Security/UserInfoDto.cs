namespace SSOAuthAPI.Models.Security
{
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Verified { get; set; }
        public bool IsActive { get; set; }
    }
}
