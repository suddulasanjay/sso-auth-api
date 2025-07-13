using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Models.Configuration;
using SSOAuthAPI.Models.Security;
using SSOAuthAPI.Utilities;
using SSOAuthAPI.Utilities.Constants;
using System.Security.Claims;

namespace SSOAuthAPI.Controllers.Security
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly FrontEndSettings _frontEndSettings;
        private readonly ISessionService _sessionService;
        private readonly IProviderUserService _providerUserService;

        public AccountController(IUserService userService, ISessionService sessionService, IOptions<FrontEndSettings> frontEndSettings, IProviderUserService providerUserService)
        {
            _frontEndSettings = frontEndSettings.Value;
            _userService = userService;
            _sessionService = sessionService;
            _providerUserService = providerUserService;
        }

        [HttpPost("[action]")]
        [ProducesResponseType<LoginResponseDto>(200)]
        [ProducesResponseType<string>(400)]
        [ProducesResponseType<string>(404)]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] CustomLoginDto login)
        {
            var user = await _userService.FindByEmailAsync(login.Email);


            return await LoginUserWithPassword(user, login.Password);

        }

        [HttpPost("Signup")]
        [ProducesResponseType<SignUpResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> Signup(SignupRequestDto model)
        {
            var existingUser = await _userService.FindByEmailAsync(model.Email);

            if (existingUser is not null)
            {
                if (existingUser.Status == CommonStatus.Disabled)
                {
                    return BadRequest("This account is disabled");
                }
                if (existingUser.Verified)
                {
                    return Conflict("User already exists");
                }
                else
                {
                    //resend verification code
                    return NoContent();
                }

            }

            var user = await _userService.CreateUserWithSignUp(model);


            return Ok(new SignUpResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName!
            });
        }


        private async Task<IActionResult> LoginUserWithSession(User user)
        {
            if (user.Status == CommonStatus.Disabled)
            {
                return BadRequest("This account is disabled");
            }

            var providerMappingExists = await _providerUserService.ExistsAsync((int)IdentityProvider.Microsoft, user.Id);
            if (!providerMappingExists)
            {
                await _providerUserService.AddProviderUserAsync(new ProviderUser
                {
                    ProviderId = (int)IdentityProvider.Microsoft,
                    UserId = user.Id,
                    SubjectId = user.Id.ToString(),
                    Status = CommonStatus.Enabled,
                    CreatedTime = DateTime.UtcNow,
                    ModifiedTime = DateTime.UtcNow,
                });
            }

            var sessionId = await _userService.LoginUserWithSession(user);
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

            return Ok(new LoginResponseDto
            {
                Success = true,
                RequiresPasswordUpdate = string.IsNullOrEmpty(user.PasswordHash)
            });
        }

        private async Task<IActionResult> LoginUserWithPassword(User? user, string password)
        {
            string? error = _userService.CheckLockAndVerifyPassword(user, password);

            if (error.HasValue())
            {
                return BadRequest(error);
            }

            return await LoginUserWithSession(user!);
        }
    }
}
