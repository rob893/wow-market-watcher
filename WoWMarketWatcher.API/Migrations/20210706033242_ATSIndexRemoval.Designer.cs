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
    [Migration("20210706033242_ATSIndexRemoval")]
    partial class ATSIndexRemoval
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.7");

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

            modelBuilder.Entity("WatchListWoWItem", b =>
                {
                    b.Property<int>("WatchedInId")
                        .HasColumnType("int");

                    b.Property<int>("WatchedItemsId")
                        .HasColumnType("int");

                    b.HasKey("WatchedInId", "WatchedItemsId");

                    b.HasIndex("WatchedItemsId");

                    b.ToTable("WatchListWoWItem");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.AuctionTimeSeriesEntry", b =>
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

                    b.HasIndex("WoWItemId", "ConnectedRealmId", "Timestamp");

                    b.ToTable("AuctionTimeSeries");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.ConnectedRealm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Population")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("ConnectedRealms");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.LinkedAccount", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LinkedAccountType")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id", "LinkedAccountType");

                    b.HasIndex("UserId");

                    b.ToTable("LinkedAccounts");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.Realm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("ConnectedRealmId")
                        .HasColumnType("int");

                    b.Property<bool>("IsTournament")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Locale")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Timezone")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("ConnectedRealmId");

                    b.ToTable("Realms");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.RefreshToken", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("DeviceId")
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

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.Role", b =>
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

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.User", b =>
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
                        .HasColumnType("longtext");

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

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.UserPreference", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("UITheme")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("UserPreferences");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.WatchList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("ConnectedRealmId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConnectedRealmId");

                    b.HasIndex("UserId");

                    b.ToTable("WatchLists");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.WoWItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("InventoryType")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsEquippable")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsStackable")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("ItemClass")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ItemSubclass")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<int>("MaxCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("PurchasePrice")
                        .HasColumnType("int");

                    b.Property<int>("PurchaseQuantity")
                        .HasColumnType("int");

                    b.Property<string>("Quality")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("RequiredLevel")
                        .HasColumnType("int");

                    b.Property<long>("SellPrice")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("WoWItems");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WatchListWoWItem", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.WatchList", null)
                        .WithMany()
                        .HasForeignKey("WatchedInId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Entities.WoWItem", null)
                        .WithMany()
                        .HasForeignKey("WatchedItemsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.LinkedAccount", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.User", "User")
                        .WithMany("LinkedAccounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.Realm", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.ConnectedRealm", "ConnectedRealm")
                        .WithMany("Realms")
                        .HasForeignKey("ConnectedRealmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectedRealm");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.RefreshToken", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.User", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.UserPreference", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.User", "User")
                        .WithOne("Preferences")
                        .HasForeignKey("WoWMarketWatcher.API.Entities.UserPreference", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.UserRole", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Entities.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.WatchList", b =>
                {
                    b.HasOne("WoWMarketWatcher.API.Entities.ConnectedRealm", "ConnectedRealm")
                        .WithMany()
                        .HasForeignKey("ConnectedRealmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WoWMarketWatcher.API.Entities.User", "User")
                        .WithMany("WatchLists")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConnectedRealm");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.ConnectedRealm", b =>
                {
                    b.Navigation("Realms");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("WoWMarketWatcher.API.Entities.User", b =>
                {
                    b.Navigation("LinkedAccounts");

                    b.Navigation("Preferences")
                        .IsRequired();

                    b.Navigation("RefreshTokens");

                    b.Navigation("UserRoles");

                    b.Navigation("WatchLists");
                });
#pragma warning restore 612, 618
        }
    }
}
