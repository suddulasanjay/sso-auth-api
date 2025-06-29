using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Models.Configuration;
using SSOAuthAPI.Models.Security;
using SSOAuthAPI.Utilities.Constants;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace SSOAuthAPI.Controllers.OpenID
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly IUserService _userService;
        private readonly IUserClientService _userClientService;
        private readonly IProviderUserService _providerUserService;
        private readonly ISessionService _sessionService;
        private readonly FrontEndSettings _frontEndSettings;
        public AuthorizationController(IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager, IUserService userService, IUserClientService userClientService, IProviderUserService providerUserService, ISessionService sessionService, IOptions<FrontEndSettings> frontEndSettings)
        {
            _applicationManager = applicationManager;
            _scopeManager = scopeManager;
            _userService = userService;
            _providerUserService = providerUserService;
            _userClientService = userClientService;
            _sessionService = sessionService;
            _frontEndSettings = frontEndSettings.Value;
        }

        [HttpGet("~/connect/authorize")]
        [ProducesResponseType(302)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Authorize(
        [FromQuery(Name = "client_id"), Required] string clientId,
        [FromQuery(Name = "redirect_uri"), Required] string redirectUri,
        [FromQuery(Name = "response_type"), Required] string responseType,
        [FromQuery(Name = "scope"), Required] string scope,
        [FromQuery(Name = "state")] string? state)
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("Invalid OpenID Connect request");

            var requestedScopes = request.GetScopes();

            if (!requestedScopes.Any())
            {
                return BadRequest("No scopes were requested.");
            }

            // Get the client application
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
            if (application is null)
            {
                return BadRequest("Invalid client ID.");
            }

            // Fetch the list of scopes assigned to the client ***important

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var authId = Request.Query["auth_id"].FirstOrDefault() ?? Guid.NewGuid().ToString();

            if (!result.Succeeded)
            {
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
            return await SignInUser(result, clientId, scope);
        }

        [HttpPost("~/connect/token")]
        [Consumes("application/x-www-form-urlencoded")]
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

                var authId = principal.FindFirstValue(OpenIddictConstants.Claims.Private.AuthorizationId);
                var sessionId = principal.FindFirstValue(CustomClaimTypes.SessionId);
                await _sessionService.AddAuthorizationToSession(authId, sessionId);

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else if (request.IsRefreshTokenGrantType())
            {
                var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))?.Principal!;
                return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new { error = "unsupported_grant_type" });
        }

        [HttpGet("external-login/google")]
        public IActionResult ExternalLoginGoogle([FromQuery] string returnUrl)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), values: new { returnUrl }),
            };


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

            //login user
            var sessionId = await _userService.LoginUserWithSession(user);
            // Sign in with app's cookie (so OpenIddict will see the session)
            var claims = new List<Claim>
            {
                new Claim(CustomClaimTypes.UserId, user.Id.ToString()),
                new Claim(CustomClaimTypes.SessionId, sessionId.ToString())
            };

            if (user.Email is { })
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

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
                RedirectUri = Url.Action(nameof(MicrosoftCallback), values: new { returnUrl })
            };
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


            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerSubjectId) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return BadRequest("Required information missing from Microsoft response.");

            var user = await _userService.FindByEmailAsync(email);
            if (user is null)
            {
                user = await _userService.CreateUserAsync(email, firstName, lastName, (int)IdentityProvider.Microsoft);
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

            //login user
            var sessionId = await _userService.LoginUserWithSession(user);
            // Sign in with app's cookie (so OpenIddict will see the session)
            var claims = new List<Claim>
            {
                new Claim(CustomClaimTypes.UserId, user.Id.ToString()),
                new Claim(CustomClaimTypes.SessionId, sessionId.ToString()) //sessionId claim
            };

            if (user.Email is { })
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            var returnUrl = HttpContext.Request.Query["returnUrl"].FirstOrDefault();
            return Redirect(returnUrl ?? "/");
        }

        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
        [ProducesResponseType<UserInfoDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Userinfo()
        {
            var scopes = HttpContext.User.FindFirst("scope")?.Value?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            bool hasProfileScope = scopes.Contains("profile");
            if (!hasProfileScope)
            {
                return Forbid("User Info Forbidden");
            }
            int.TryParse(HttpContext.User.FindFirstValue(CustomClaimTypes.UserId), out var userId);

            if (userId == 0)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = JsonSerializer.Serialize(new
                    {
                        error = "User token needed",
                    }),
                    ContentType = "application/json"
                };
            }

            var user = await _userService.FindByIdAsync(userId);

            if (user is null)
            {
                return NotFound("No user found");
            }

            var userInfo = new UserInfoDto
            {
                Id = userId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Verified = user.Verified,
                IsActive = user.Status == CommonStatus.Enabled
            };

            return Ok(userInfo);
        }

        [HttpGet("~/connect/logout")]
        public async Task<IActionResult> Logout(string redirectTo = null)
        {
            var auth = HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;

            if (auth.Succeeded)
            {
                var sessionId = auth.Principal.FindFirstValue(CustomClaimTypes.SessionId);
                if (!string.IsNullOrEmpty(sessionId))
                    await _sessionService.EndSession(sessionId);
            }

            if (redirectTo == null)
            {
                redirectTo = "/";
            }


            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect(redirectTo);
        }

        private async Task<IActionResult> SignInUser(AuthenticateResult result, string clientId, string scope)
        {
            if (result.Principal == null)
            {
                return Forbid("Missing user identity");
            }
            var authId = result.Principal.FindFirstValue(OpenIddictConstants.Claims.Private.AuthorizationId);
            var userId = result.Principal.FindFirstValue(CustomClaimTypes.UserId);
            var sessionId = result.Principal.FindFirstValue(CustomClaimTypes.SessionId);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(sessionId))
            {
                return Forbid("Missing user identity claims.");
            }

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
            }

            var claims = new List<Claim>
            {
                new Claim(OpenIddictConstants.Claims.Subject, userId),
                new Claim(OpenIddictConstants.Claims.ClientId, clientId),
                new Claim(CustomClaimTypes.UserId, userId).SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim(CustomClaimTypes.SessionId, sessionId).SetDestinations(OpenIddictConstants.Destinations.AccessToken)
            };

            if (!string.IsNullOrWhiteSpace(user!.Email) && scope.Contains("email"))
            {
                claims.Add(new Claim(OpenIddictConstants.Claims.Email, user.Email).SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            }

            var role = result.Principal.FindFirstValue(ClaimTypes.Role);

            if (!string.IsNullOrEmpty(role) && scope.Contains(OpenIddictConstants.Scopes.Roles))
            {
                claims.Add(new Claim(OpenIddictConstants.Claims.Role, role).SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            }


            var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var finalPrincipal = new ClaimsPrincipal(identity);

            finalPrincipal.SetScopes(scope.Split(' '));
            finalPrincipal.SetResources(clientId);

            return SignIn(finalPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


    }
}
