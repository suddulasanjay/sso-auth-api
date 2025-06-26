using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Utilities.Constants;
using System.Security.Claims;

namespace SSOAuthAPI.Filters
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly ISessionService _sessionService;

        public CustomCookieAuthenticationEvents(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;
            var authType = context.Principal?.Identity?.AuthenticationType;

            //Skip validation if coming from an external provider
            if (authType is "Google" or "Microsoft")
            {
                return;
            }
            var sessionId = userPrincipal?.FindFirstValue(CustomClaimTypes.SessionId);

            // Retrieve the session from the database
            var sessionValid = await _sessionService.ValidateSession(sessionId);

            if (!sessionValid)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
