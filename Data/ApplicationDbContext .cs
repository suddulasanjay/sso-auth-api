using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using SSOAuthAPI.Data.Entities;

namespace SSOAuthAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        // OpenIddict Default Entities
        public DbSet<OpenIddictEntityFrameworkCoreApplication> Applications => Set<OpenIddictEntityFrameworkCoreApplication>();
        public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations => Set<OpenIddictEntityFrameworkCoreAuthorization>();
        public DbSet<OpenIddictEntityFrameworkCoreScope> Scopes => Set<OpenIddictEntityFrameworkCoreScope>();
        public DbSet<OpenIddictEntityFrameworkCoreToken> Tokens => Set<OpenIddictEntityFrameworkCoreToken>();

        // SSO Entities
        public DbSet<User> Users => Set<User>();
        public DbSet<Provider> Providers => Set<Provider>();
        public DbSet<ProviderUser> ProviderUsers => Set<ProviderUser>();
        public DbSet<UserClient> UserClients => Set<UserClient>();
        public DbSet<Session> Sessions => Set<Session>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            builder.Entity<ProviderUser>().HasIndex(pu => new { pu.ProviderId, pu.SubjectId }).IsUnique();

            builder.Entity<UserClient>().HasOne(uc => uc.App).WithMany().HasForeignKey(uc => uc.AppId).HasPrincipalKey(app => app.ClientId);
        }
    }
}
