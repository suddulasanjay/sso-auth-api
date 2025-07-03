using OpenIddict.Abstractions;
using OpenIddict.Validation;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Utilities.Constants;
using System.Security.Claims;
using static OpenIddict.Validation.OpenIddictValidationEvents;

namespace SSOAuthAPI.Filters
{
    public class CustomOpenIdDictValidationHandlers :
        IOpenIddictValidationHandler<ValidateTokenContext>
    {
        private readonly ISessionService _sessionService;

        public CustomOpenIdDictValidationHandlers(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }


        public async ValueTask HandleAsync(ValidateTokenContext context)
        {
            var principal = context.Principal;

            await ValidatePrincipal(context, principal);
        }


        private async Task ValidatePrincipal(BaseValidatingContext context, ClaimsPrincipal? principal)
        {
            var session = principal?.FindFirstValue(CustomClaimTypes.SessionId);

            if (!string.IsNullOrEmpty(session))
            {
                var isSessionValid = await _sessionService.ValidateSession(session);
                if (!isSessionValid)
                {
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidToken,
                        description: "The session is no longer valid."
                    );
                }
            }
        }
    }
}
