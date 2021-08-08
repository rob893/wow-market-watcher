using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Data
{
    public sealed class DataContext : IdentityDbContext<User, Role, int,
        IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens => this.Set<RefreshToken>();

        public DbSet<UserPreference> UserPreferences => this.Set<UserPreference>();

        public DbSet<LinkedAccount> LinkedAccounts => this.Set<LinkedAccount>();

        public DbSet<ConnectedRealm> ConnectedRealms => this.Set<ConnectedRealm>();

        public DbSet<Realm> Realms => this.Set<Realm>();

        public DbSet<WoWItem> WoWItems => this.Set<WoWItem>();

        public DbSet<AuctionTimeSeriesEntry> AuctionTimeSeries => this.Set<AuctionTimeSeriesEntry>();

        public DbSet<WatchList> WatchLists => this.Set<WatchList>();

        public DbSet<Alert> Alerts => this.Set<Alert>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

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

            builder.Entity<AuctionTimeSeriesEntry>()
                .HasIndex(entry => new { entry.WoWItemId, entry.ConnectedRealmId, entry.Timestamp });

            builder.Entity<AuctionTimeSeriesEntry>()
                .HasIndex(entry => entry.Timestamp);

            builder.Entity<AlertCondition>(condition =>
            {
                condition.Property(c => c.AggregationType).HasConversion<string>();
                condition.Property(c => c.Metric).HasConversion<string>();
                condition.Property(c => c.Operator).HasConversion<string>();
            });

            builder.Entity<AlertAction>(condition =>
            {
                condition.Property(c => c.Type).HasConversion<string>();
            });
        }
    }
}