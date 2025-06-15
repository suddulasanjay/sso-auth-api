using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Models.Configuration;
using SSOAuthAPI.Utilities.Constants;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SSOAuthAPI.Controllers.OpenID
{
    [Route("api/connect")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserClientService _userClientService;
        private readonly IProviderUserService _providerUserService;
        private readonly FrontEndSettings _frontEndSettings;
        public AuthorizationController(IUserService userService, IUserClientService userClientService, IProviderUserService providerUserService, IOptions<FrontEndSettings> frontEndSettings)
        {
            _userService = userService;
            _providerUserService = providerUserService;
            _userClientService = userClientService;
            _frontEndSettings = frontEndSettings.Value;
        }

        [HttpGet("authorize")]
        [ProducesResponseType(302)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Authorize(
        [FromQuery(Name = "client_id"), Required] string clientId,
        [FromQuery(Name = "redirect_uri"), Required] string redirectUri,
        [FromQuery(Name = "response_type"), Required] string responseType,
        [FromQuery(Name = "scope"), Required] string scope,
        [FromQuery(Name = "state")] string? state,
        [FromQuery(Name = "prompt")] string? prompt)
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("Invalid OpenID Connect request");

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var authId = Guid.NewGuid().ToString();

            if (!result.Succeeded)
            {
                if (prompt == "none")
                {
                    return Content("<script>window.parent.postMessage({ type: 'session_not_found' }, '*');</script>", "text/html");
                }

                var qs = new QueryString()
                    .Add("auth_id", authId)
                    .Add("client_id", clientId)
                    .Add("redirect_uri", redirectUri)
                    .Add("response_type", responseType)
                    .Add("scope", scope)
                    .Add("state", state ?? "");

                return Redirect(_frontEndSettings.BaseUrl + _frontEndSettings.LoginPath + qs.ToString());
            }

            // If already signed in, directly issue authorization code.
            return await SignInUser(result.Principal, clientId, scope);
        }

        [HttpPost("token")]
        [ProducesResponseType(typeof(OpenIddictResponse), 200)]
        [ProducesResponseType(typeof(OpenIddictResponse), 400)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("Invalid token request");

            if (request.IsAuthorizationCodeGrantType())
            {
                var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal
                    ?? throw new InvalidOperationException("Invalid principal");

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new { error = "unsupported_grant_type" });
        }

        [HttpGet("external-login/google")]
        public IActionResult ExternalLoginGoogle([FromQuery] string returnUrl)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback)),
            };

            props.Items["returnUrl"] = returnUrl;

            return Challenge(props, "Google");
        }

        [HttpGet("external-login/google/callback")]
        [ProducesResponseType(302)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GoogleCallback()
        {
            var externalPrincipal = User;
            if (externalPrincipal?.Identity == null || !externalPrincipal.Identity.IsAuthenticated)
            {
                return BadRequest("Google authentication failed.");
            }

            var email = externalPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = externalPrincipal.FindFirst("given_name")?.Value;
            var lastName = externalPrincipal.FindFirst("family_name")?.Value;
            var providerSubjectId = externalPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerSubjectId) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                return BadRequest("Required information missing from Google response.");
            }

            var user = await _userService.FindByEmailAsync(email);
            if (user is null)
            {
                user = await _userService.CreateUserAsync(email, firstName, lastName, (int)IdentityProvider.Google);
            }

            var providerMappingExists = await _providerUserService.ExistsAsync((int)IdentityProvider.Google, user.Id);

            if (!providerMappingExists)
            {
                await _providerUserService.AddProviderUserAsync(new ProviderUser
                {
                    ProviderId = (int)IdentityProvider.Google,
                    UserId = user.Id,
                    SubjectId = providerSubjectId,
                    Status = CommonStatus.Enabled,
                    CreatedTime = DateTime.UtcNow,
                    ModifiedTime = DateTime.UtcNow,
                });
            }

            // Sign in with app's cookie (so OpenIddict will see the session)
            var claims = new List<Claim>
            {
                new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
                new Claim(OpenIddictConstants.Claims.Email, user.Email),
                new Claim(OpenIddictConstants.Claims.Name, user.FirstName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            //Redirect to /connect/authorize with original returnUrl (if any)
            var returnUrl = HttpContext.Request.Query["returnUrl"].FirstOrDefault();
            return Redirect(returnUrl ?? "/");
        }

        [HttpGet("external-login/microsoft")]
        public IActionResult ExternalLoginMicrosoft([FromQuery] string returnUrl)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(MicrosoftCallback))
            };
            props.Items["returnUrl"] = returnUrl;
            return Challenge(props, "Microsoft");
        }

        [HttpGet("external-login/microsoft/callback")]
        [ProducesResponseType(302)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> MicrosoftCallback()
        {
            var externalPrincipal = User;
            if (externalPrincipal?.Identity == null || !externalPrincipal.Identity.IsAuthenticated)
                return BadRequest("Microsoft authentication failed.");

            var email = externalPrincipal.FindFirst(ClaimTypes.Email)?.Value
                      ?? externalPrincipal.FindFirst("preferred_username")?.Value
                      ?? externalPrincipal.FindFirst("userPrincipalName")?.Value;

            var firstName = externalPrincipal.FindFirst(ClaimTypes.GivenName)?.Value
                         ?? externalPrincipal.FindFirst("given_name")?.Value;

            var lastName = externalPrincipal.FindFirst(ClaimTypes.Surname)?.Value
                        ?? externalPrincipal.FindFirst("surname")?.Value;

            var providerSubjectId = externalPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerSubjectId))
                return BadRequest("Required information missing from Microsoft response.");

            var user = await _userService.FindByEmailAsync(email);
            if (user is null)
            {
                user = await _userService.CreateUserAsync(email, firstName ?? "", lastName ?? "", (int)IdentityProvider.Microsoft);
            }

            var providerMappingExists = await _providerUserService.ExistsAsync((int)IdentityProvider.Microsoft, user.Id);
            if (!providerMappingExists)
            {
                await _providerUserService.AddProviderUserAsync(new ProviderUser
                {
                    ProviderId = (int)IdentityProvider.Microsoft,
                    UserId = user.Id,
                    SubjectId = providerSubjectId,
                    Status = CommonStatus.Enabled,
                    CreatedTime = DateTime.UtcNow,
                    ModifiedTime = DateTime.UtcNow,
                });
            }

            var claims = new List<Claim>
            {
                new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
                new Claim(OpenIddictConstants.Claims.Email, user.Email),
                new Claim(OpenIddictConstants.Claims.Name, user.FirstName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            var returnUrl = HttpContext.Request.Query["returnUrl"].FirstOrDefault();
            return Redirect(returnUrl ?? "/");
        }


        private async Task<IActionResult> SignInUser(ClaimsPrincipal principal, string clientId, string scope)
        {
            var userId = principal.FindFirstValue(CustomClaimTypes.UserId);
            var sessionId = principal.FindFirstValue(CustomClaimTypes.SessionId);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(sessionId))
                return Forbid("Missing user identity claims.");

            var claims = new List<Claim>
            {
                new(OpenIddictConstants.Claims.Subject, userId),
                new(OpenIddictConstants.Claims.ClientId, clientId),
                new(CustomClaimTypes.UserId, userId),
                new(CustomClaimTypes.SessionId, sessionId)
            };
            if (!int.TryParse(userId, out var id))
            {
                return Forbid("User ID not found");
            }
            var user = await _userService.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var exists = await _userClientService.ExistsAsync(id, clientId);
                if (!exists)
                {
                    await _userClientService.AddUserClientAsync(new UserClient
                    {
                        UserId = id,
                        AppId = clientId,
                        Scope = scope,
                        CreatedTime = DateTime.UtcNow,
                        ModifiedTime = DateTime.UtcNow,
                        Status = CommonStatus.Enabled,
                    });
                }
                else
                {
                    await _userClientService.UpdateScopeIfNeeded(id, clientId, scope);
                }
            }

            if (user.ProviderId.HasValue)
            {
                var alreadyMapped = await _providerUserService.ExistsAsync(user.ProviderId.Value, user.Id);
                if (!alreadyMapped)
                {
                    await _providerUserService.AddProviderUserAsync(new ProviderUser
                    {
                        ProviderId = user.ProviderId.Value,
                        UserId = user.Id,
                        Status = CommonStatus.Enabled,
                        CreatedTime = DateTime.UtcNow,
                        ModifiedTime = DateTime.UtcNow,
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(user!.Email) && scope.Contains("email"))
                claims.Add(new Claim(OpenIddictConstants.Claims.Email, user.Email));


            var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var finalPrincipal = new ClaimsPrincipal(identity);

            finalPrincipal.SetScopes(scope.Split(' '));
            finalPrincipal.SetResources(clientId);

            return SignIn(finalPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


    }
}
