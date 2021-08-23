using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WoWMarketWatcher.API.Migrations
{
    public partial class UpdateWatchListAndAlerts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WatchLists_ConnectedRealms_ConnectedRealmId",
                table: "WatchLists");

            migrationBuilder.DropTable(
                name: "WatchListWoWItem");

            migrationBuilder.DropIndex(
                name: "IX_WatchLists_ConnectedRealmId",
                table: "WatchLists");

            migrationBuilder.DropColumn(
                name: "ConnectedRealmId",
                table: "WatchLists");

            migrationBuilder.AddColumn<int>(
                name: "WoWItemId",
                table: "WatchLists",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastEvaluated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastFired = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WatchedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WatchListId = table.Column<int>(type: "int", nullable: false),
                    ConnectedRealmId = table.Column<int>(type: "int", nullable: false),
                    WoWItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchedItems_ConnectedRealms_ConnectedRealmId",
                        column: x => x.ConnectedRealmId,
                        principalTable: "ConnectedRealms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchedItems_WatchLists_WatchListId",
                        column: x => x.WatchListId,
                        principalTable: "WatchLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchedItems_WoWItems_WoWItemId",
                        column: x => x.WoWItemId,
                        principalTable: "WoWItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AlertAction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AlertId = table.Column<int>(type: "int", nullable: false),
                    ActionOn = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertAction_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AlertConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AlertId = table.Column<int>(type: "int", nullable: false),
                    WoWItemId = table.Column<int>(type: "int", nullable: false),
                    ConnectedRealmId = table.Column<int>(type: "int", nullable: false),
                    Metric = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Operator = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AggregationType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AggregationTimeGranularityInHours = table.Column<int>(type: "int", nullable: false),
                    Threshold = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertConditions_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertConditions_ConnectedRealms_ConnectedRealmId",
                        column: x => x.ConnectedRealmId,
                        principalTable: "ConnectedRealms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertConditions_WoWItems_WoWItemId",
                        column: x => x.WoWItemId,
                        principalTable: "WoWItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLists_WoWItemId",
                table: "WatchLists",
                column: "WoWItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertAction_AlertId",
                table: "AlertAction",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertConditions_AlertId",
                table: "AlertConditions",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertConditions_ConnectedRealmId",
                table: "AlertConditions",
                column: "ConnectedRealmId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertConditions_WoWItemId",
                table: "AlertConditions",
                column: "WoWItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId",
                table: "Alerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchedItems_ConnectedRealmId",
                table: "WatchedItems",
                column: "ConnectedRealmId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchedItems_WatchListId",
                table: "WatchedItems",
                column: "WatchListId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchedItems_WoWItemId",
                table: "WatchedItems",
                column: "WoWItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_WatchLists_WoWItems_WoWItemId",
                table: "WatchLists",
                column: "WoWItemId",
                principalTable: "WoWItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WatchLists_WoWItems_WoWItemId",
                table: "WatchLists");

            migrationBuilder.DropTable(
                name: "AlertAction");

            migrationBuilder.DropTable(
                name: "AlertConditions");

            migrationBuilder.DropTable(
                name: "WatchedItems");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_WatchLists_WoWItemId",
                table: "WatchLists");

            migrationBuilder.DropColumn(
                name: "WoWItemId",
                table: "WatchLists");

            migrationBuilder.AddColumn<int>(
                name: "ConnectedRealmId",
                table: "WatchLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WatchListWoWItem",
                columns: table => new
                {
                    WatchedInId = table.Column<int>(type: "int", nullable: false),
                    WatchedItemsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchListWoWItem", x => new { x.WatchedInId, x.WatchedItemsId });
                    table.ForeignKey(
                        name: "FK_WatchListWoWItem_WatchLists_WatchedInId",
                        column: x => x.WatchedInId,
                        principalTable: "WatchLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchListWoWItem_WoWItems_WatchedItemsId",
                        column: x => x.WatchedItemsId,
                        principalTable: "WoWItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WatchLists_ConnectedRealmId",
                table: "WatchLists",
                column: "ConnectedRealmId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchListWoWItem_WatchedItemsId",
                table: "WatchListWoWItem",
                column: "WatchedItemsId");

            migrationBuilder.AddForeignKey(
                name: "FK_WatchLists_ConnectedRealms_ConnectedRealmId",
                table: "WatchLists",
                column: "ConnectedRealmId",
                principalTable: "ConnectedRealms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
