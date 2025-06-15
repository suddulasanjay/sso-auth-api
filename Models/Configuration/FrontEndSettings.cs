namespace SSOAuthAPI.Models.Configuration
{
    public class FrontEndSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string LoginPath { get; set; } = string.Empty;
        public string AccessDeniedPath { get; set; } = string.Empty;
        public string RedirectAfterLogin { get; set; } = string.Empty;
    }

}
