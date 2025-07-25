﻿namespace SSOAuthAPI.Models.Security
{
    public class UserInfoDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public bool Verified { get; set; }
        public bool IsActive { get; set; }
    }
}
