using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Entities;

namespace WoWMarketWatcher.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int,
        IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DbSet<RefreshToken> RefreshTokens => this.Set<RefreshToken>();
        public DbSet<UserPreference> UserPreferences => this.Set<UserPreference>();
        public DbSet<LinkedAccount> LinkedAccounts => this.Set<LinkedAccount>();
        public DbSet<ConnectedRealm> ConnectedRealms => this.Set<ConnectedRealm>();
        public DbSet<Realm> Realms => this.Set<Realm>();
        public DbSet<WoWItem> WoWItems => this.Set<WoWItem>();
        public DbSet<AuctionTimeSeriesEntry> AuctionTimeSeries => this.Set<AuctionTimeSeriesEntry>();
        public DbSet<WatchList> WatchLists => this.Set<WatchList>();

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            builder.Entity<RefreshToken>(rToken =>
            {
                rToken.HasKey(k => new { k.UserId, k.DeviceId });
            });

            builder.Entity<User>(user =>
            {
                user.Property(u => u.MembershipLevel).HasConversion<string>();
            });

            builder.Entity<UserPreference>(preference =>
            {
                preference.Property(p => p.UITheme).HasConversion<string>();
            });

            builder.Entity<LinkedAccount>(linkedAccount =>
            {
                linkedAccount.HasKey(account => new { account.Id, account.LinkedAccountType });
                linkedAccount.Property(account => account.LinkedAccountType).HasConversion<string>();
            });

            builder.Entity<AuctionTimeSeriesEntry>().HasIndex(entry => entry.Timestamp);
        }
    }
}