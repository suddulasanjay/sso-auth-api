using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using SSOAuthAPI.Data;
using SSOAuthAPI.Filters;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Models.Configuration;
using SSOAuthAPI.Services;
using SSOAuthAPI.Services.Jobs;
using System.Security.Claims;

namespace SSOAuthAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var services = builder.Services;

            services.AddScoped<CustomCookieAuthenticationEvents>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                    {
                        options.LoginPath = builder.Configuration["FrontEndSettings:LoginPath"];
                        options.AccessDeniedPath = builder.Configuration["FrontEndSettings:AccessDeniedPath"];
                        options.EventsType = typeof(CustomCookieAuthenticationEvents);
                        options.ExpireTimeSpan = TimeSpan.FromDays(30);
                        options.SlidingExpiration = true;
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.Cookie.SameSite = SameSiteMode.Lax; // Changed from None to Lax
                        options.Cookie.Name = "SSOAuth.Cookie"; // Explicit cookie name
                    })
                    .AddGoogle("Google", options =>
                    {
                        var googleConfig = builder.Configuration.GetSection("Authentication:Google");
                        options.ClientId = googleConfig["ClientId"];
                        options.ClientSecret = googleConfig["ClientSecret"];
                        options.ClaimActions.MapJsonKey("given_name", "given_name");
                        options.ClaimActions.MapJsonKey("family_name", "family_name");
                        options.SaveTokens = true;
                        options.CorrelationCookie.SameSite = SameSiteMode.None;
                        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.CorrelationCookie.HttpOnly = true;
                    })
                    .AddMicrosoftAccount("Microsoft", options =>
                    {
                        var microsoftConfig = builder.Configuration.GetSection("Authentication:Microsoft");
                        options.ClientId = microsoftConfig["ClientId"];
                        options.ClientSecret = microsoftConfig["clientSecretValue"];
                        //options.CallbackPath = "/signin-microsoft"; // or your custom path
                        options.SaveTokens = true;

                        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "surname");
                        options.ClaimActions.MapJsonKey("email", "email");
                    });
            //CORS
            string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                    x => x
                        .WithOrigins(builder.Configuration.GetSection("Cors").GetSection("AllowedOrigins").Get<string[]>()!)
                        .AllowAnyMethod()
                        .WithExposedHeaders("Content-Disposition")
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddControllers();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict();
            });

            services.AddOpenIddict()
                    .AddCore(options =>
                    {
                        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
                    })
                    .AddServer(options =>
                    {
                        options.AllowAuthorizationCodeFlow()
                               .AllowRefreshTokenFlow()
                               .AllowClientCredentialsFlow();

                        options.SetTokenEndpointUris("/connect/token")
                                .SetAuthorizationEndpointUris("/connect/authorize")
                                .SetUserInfoEndpointUris("/connect/userinfo");


                        options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate().DisableAccessTokenEncryption();

                        options.RegisterScopes(OpenIddictConstants.Scopes.Email, "offline_access", "profile");

                        options.UseAspNetCore()
                               .EnableTokenEndpointPassthrough()
                               .EnableUserInfoEndpointPassthrough()
                               .EnableAuthorizationEndpointPassthrough()
                               .EnableStatusCodePagesIntegration();
                    })
                    .AddValidation(options =>
                    {
                        options.UseLocalServer();
                        options.UseAspNetCore();
                        options.AddEventHandler<OpenIddictValidationEvents.ValidateTokenContext>(openIddictBuilder =>
                            openIddictBuilder.UseInlineHandler(async context =>
                            {
                                if (context.RequestUri is not null)
                                {
                                    var audiences = builder.Configuration.GetSection("ResourceServer:ValidAudiences").Get<string[]>() ?? Array.Empty<string>();
                                    foreach (var audience in audiences)
                                    {
                                        context.Options.Audiences.Add(audience);
                                    }
                                }
                            }
                        ).SetOrder(OpenIddictValidationHandlers.Protection.ValidateAudience.Descriptor.Order - 1));

                        options.AddEventHandler<OpenIddictValidationEvents.ValidateTokenContext>(openIddictBuilder => openIddictBuilder.UseScopedHandler<CustomOpenIdDictValidationHandlers>());

                    });

            services.Configure<FrontEndSettings>(builder.Configuration.GetSection("FrontEndSettings"));
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserClientService, UserClientService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IProviderUserService, ProviderUserService>();
            services.AddTransient<ISessionService, SessionService>();
            services.AddScoped<IOpenIddictCleanupService, OpenIddictCleanupService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var app = builder.Build();
            app.UseCors(MyAllowSpecificOrigins);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
