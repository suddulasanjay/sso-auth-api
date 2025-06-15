namespace SSOAuthAPI.Models.Application
{
    public class ClientDto
    {
        public string ClientId { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? ClientSecret { get; set; }
        public List<string> RedirectUris { get; set; } = new();
        public List<string> PostLogoutRedirectUris { get; set; } = new();
        public bool EnableClientCredentials { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    public class ClientAppResponse
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }

}
