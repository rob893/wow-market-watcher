﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WoWMarketWatcher.API.Data;

namespace WoWMarketWatcher.API.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210823045209_UpdateWatchListAndAlerts")]
    partial class UpdateWatchListAndAlerts
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(4000)
                        .HasColumnType("varchar(4000)");

                    b.Property<DateTime>("LastEvaluated")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastFired")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.AlertAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ActionOn")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<int>("AlertId")
                        .HasColumnType("int");

                    b.Property<string>("Target")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.HasKey("Id");

                    b.HasIndex("AlertId");

                    b.ToTable("AlertAction");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.AlertCondition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AggregationTimeGranularityInHours")
                        .HasColumnType("int");

                    b.Property<string>("AggregationType")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<int>("AlertId")
                        .HasColumnType("int");

                    b.Property<int>("ConnectedRealmId")
                        .HasColumnType("int");

                    b.Property<string>("Metric")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<string>("Operator")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<long>("Threshold")
                        .HasColumnType("bigint");

                    b.Property<int>("WoWItemId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlertId");

                    b.HasIndex("ConnectedRealmId");

                    b.HasIndex("WoWItemId");

                    b.ToTable("AlertConditions");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.AuctionTimeSeriesEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long>("AveragePrice")
                        .HasColumnType("bigint");

                    b.Property<int>("ConnectedRealmId")
                        .HasColumnType("int");

                    b.Property<long>("MaxPrice")
                        .HasColumnType("bigint");

                    b.Property<long>("MinPrice")
                        .HasColumnType("bigint");

                    b.Property<long>("Price25Percentile")
                        .HasColumnType("bigint");

                    b.Property<long>("Price50Percentile")
                        .HasColumnType("bigint");

                    b.Property<long>("Price75Percentile")
                        .HasColumnType("bigint");

                    b.Property<long>("Price95Percentile")
                        .HasColumnType("bigint");

                    b.Property<long>("Price99Percentile")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime(6)");

                    b.Property<long>("TotalAvailableForAuction")
                        .HasColumnType("bigint");

                    b.Property<int>("WoWItemId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Timestamp");

                    b.HasIndex("WoWItemId", "ConnectedRealmId", "Timestamp");

                    b.ToTable("AuctionTimeSeries");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.ConnectedRealm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Population")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("ConnectedRealms");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.LinkedAccount", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LinkedAccountType")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id", "LinkedAccountType");

                    b.HasIndex("UserId");

                    b.ToTable("LinkedAccounts");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.Realm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("ConnectedRealmId")
                        .HasColumnType("int");

                    b.Property<bool>("IsTournament")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Locale")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Timezone")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("ConnectedRealmId");

                    b.ToTable("Realms");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.RefreshToken", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("DeviceId")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<DateTimeOffset>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId", "DeviceId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LastName")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("MembershipLevel")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.UserPreference", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("UITheme")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("UserPreferences");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.WatchList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(4000)
                        .HasColumnType("varchar(4000)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int?>("WoWItemId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("WoWItemId");

                    b.ToTable("WatchLists");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.WatchedItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("ConnectedRealmId")
                        .HasColumnType("int");

                    b.Property<int>("WatchListId")
                        .HasColumnType("int");

                    b.Property<int>("WoWItemId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConnectedRealmId");

                    b.HasIndex("WatchListId");

                    b.HasIndex("WoWItemId");

                    b.ToTable("WatchedItems");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.WoWItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("InventoryType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("IsEquippable")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsStackable")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("ItemClass")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("ItemSubclass")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<int>("MaxCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<int>("PurchasePrice")
                        .HasColumnType("int");

                    b.Property<int>("PurchaseQuantity")
                        .HasColumnType("int");

                    b.Property<string>("Quality")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("RequiredLevel")
                        .HasColumnType("int");

                    b.Property<long>("SellPrice")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("WoWItems");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.Alert", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", "User")
                        .WithMany("Alerts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.AlertAction", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.Alert", "Alert")
                        .WithMany("Actions")
                        .HasForeignKey("AlertId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Alert");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.AlertCondition", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.Alert", "Alert")
                        .WithMany("Conditions")
                        .HasForeignKey("AlertId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Models.Entities.ConnectedRealm", "ConnectedRealm")
                        .WithMany()
                        .HasForeignKey("ConnectedRealmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Models.Entities.WoWItem", "WoWItem")
                        .WithMany()
                        .HasForeignKey("WoWItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Alert");

                    b.Navigation("ConnectedRealm");

                    b.Navigation("WoWItem");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.LinkedAccount", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", "User")
                        .WithMany("LinkedAccounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.Realm", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.ConnectedRealm", "ConnectedRealm")
                        .WithMany("Realms")
                        .HasForeignKey("ConnectedRealmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectedRealm");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.RefreshToken", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.UserPreference", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", "User")
                        .WithOne("Preferences")
                        .HasForeignKey("WoWMarketWatcher.API.Models.Entities.UserPreference", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.UserRole", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.WatchList", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.User", "User")
                        .WithMany("WatchLists")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Models.Entities.WoWItem", null)
                        .WithMany("WatchedIn")
                        .HasForeignKey("WoWItemId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.WatchedItem", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Models.Entities.ConnectedRealm", "ConnectedRealm")
                        .WithMany()
                        .HasForeignKey("ConnectedRealmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Models.Entities.WatchList", "WatchList")
                        .WithMany("WatchedItems")
                        .HasForeignKey("WatchListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Models.Entities.WoWItem", "WoWItem")
                        .WithMany()
                        .HasForeignKey("WoWItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectedRealm");

                    b.Navigation("WatchList");

                    b.Navigation("WoWItem");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.Alert", b =>
                {
                    b.Navigation("Actions");

                    b.Navigation("Conditions");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.ConnectedRealm", b =>
                {
                    b.Navigation("Realms");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.User", b =>
                {
                    b.Navigation("Alerts");

                    b.Navigation("LinkedAccounts");

                    b.Navigation("Preferences")
                        .IsRequired();

                    b.Navigation("RefreshTokens");

                    b.Navigation("UserRoles");

                    b.Navigation("WatchLists");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.WatchList", b =>
                {
                    b.Navigation("WatchedItems");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Models.Entities.WoWItem", b =>
                {
                    b.Navigation("WatchedIn");
                });
#pragma warning restore 612, 618
        }
    }
}